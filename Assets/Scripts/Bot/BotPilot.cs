using System.Collections.Generic;
using UnityEngine;

namespace Com.MMsGamingCo.Ricomet
{

    /// <summary>
    /// The playtest bot's aim. Every few frames it simulates a fan of dashes (with
    /// wall bounces and shield ricochets), scores each by kills and boss hits minus
    /// how dangerous the landing spot is, and fires the best one. It keeps a charge
    /// in reserve for escapes and dashes away when standing still gets dangerous.
    /// </summary>
    public class BotPilot : IAimSource
    {

        #region Fields/Variables

        private const int DirectionCount = 72;
        private static readonly float[] Powers = { 0.4f, 0.7f, 1f };
        private const float ThinkInterval = 0.04f;
        private const float KillScore = 10f;
        private const float BossHitScore = 40f;
        private const float DangerWeight = 12f;
        private const float EscapeDanger = 1.2f;

        public bool IsAiming { get { return false; } }
        public bool InDeadZone { get { return false; } }
        public Vector2 Direction { get; private set; } = Vector2.right;
        public float Power { get; private set; } = 1f;
        public bool DashRequested { get; private set; }

        /// <summary>Last decision, for debugging: "attack", "boss", "escape" or "wait".</summary>
        public string LastDecision { get; private set; } = "wait";

        private float thinkTimer = 0f;
        private readonly List<Vector2> path = new List<Vector2>();
        private readonly HashSet<Enemy> pathKills = new HashSet<Enemy>();
        // Set by SimulatePath when the dash would ricochet off a boss core (a hit).
        private bool pathHitsBossCore = false;

        private struct Plan
        {
            public Vector2 direction;
            public float power;
            public float score;
            public float kills;
            public bool bossHit;
            public float landingDanger;
        }

        #endregion


        #region Public Methods

        public void Tick()
        {
            DashRequested = false;

            CometController comet = CometController.Instance;
            if (comet == null || comet.Health.IsDead || comet.IsDashing || comet.InputLocked) return;

            thinkTimer -= Time.deltaTime;
            if (thinkTimer > 0f) return;
            thinkTimer = ThinkInterval;

            if (!comet.Energy.CanDash)
            {
                LastDecision = "wait";
                return;
            }

            Decide(comet);
        }

        public void Cancel()
        {
            DashRequested = false;
        }

        #endregion


        #region Decision

        private void Decide(CometController comet)
        {
            float hereDanger = Danger(comet.Position, comet.Radius, null);
            bool hasReserve = comet.Energy.Current >= 2f;

            Plan bestAttack = default;
            Plan safest = default;
            bestAttack.score = float.MinValue;
            safest.landingDanger = float.MaxValue;

            for (int d = 0; d < DirectionCount; d++)
            {
                float angle = d * Mathf.PI * 2f / DirectionCount;
                Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

                foreach (float power in Powers)
                {
                    Plan plan = Evaluate(comet, direction, power);
                    if (plan.score > bestAttack.score) bestAttack = plan;
                    if (plan.landingDanger < safest.landingDanger ||
                        (Mathf.Approximately(plan.landingDanger, safest.landingDanger) && plan.score > safest.score))
                        safest = plan;
                }
            }

            bool worthIt = bestAttack.bossHit || bestAttack.kills >= 2f || (bestAttack.kills >= 1f && hasReserve);
            if (bestAttack.score >= KillScore * 0.5f && worthIt)
            {
                Fire(bestAttack, bestAttack.bossHit ? "boss" : "attack");
            }
            else if (hereDanger >= EscapeDanger && safest.landingDanger < hereDanger)
            {
                Fire(safest, "escape");
            }
            else if (bestAttack.score >= KillScore * 0.5f && hereDanger >= EscapeDanger * 0.5f)
            {
                // Getting crowded and there's a kill on offer: take it even without a reserve.
                Fire(bestAttack, "attack");
            }
            else
            {
                LastDecision = "wait";
            }
        }

        private void Fire(Plan plan, string decision)
        {
            Direction = plan.direction;
            Power = plan.power;
            DashRequested = true;
            LastDecision = decision;
        }

        private Plan Evaluate(CometController comet, Vector2 direction, float power)
        {
            CometStats stats = comet.Stats;
            SimulatePath(comet.Position, direction, stats.dashDistance * power, stats.wallBounces, comet.Radius);

            Plan plan = new Plan { direction = direction, power = power };
            pathKills.Clear();

            foreach (Enemy enemy in Enemy.Active)
            {
                if (!enemy.IsAlive || !PathHits(enemy.Position, enemy.Radius, comet.Radius)) continue;

                if (enemy is PrismBoss boss)
                {
                    if (!boss.IsInvulnerable) plan.bossHit = true;
                    continue;
                }

                // Splitters need two hits; a hit is still progress.
                bool dies = enemy.Health <= stats.dashDamage;
                plan.kills += dies ? 1f : 0.6f;
                if (dies) pathKills.Add(enemy);
            }

            // Core ricochets stop just outside the core, so PathHits can miss them.
            if (pathHitsBossCore && PrismBoss.Current != null && !PrismBoss.Current.IsInvulnerable)
                plan.bossHit = true;

            Vector2 landing = path[path.Count - 1];
            plan.landingDanger = Danger(landing, comet.Radius, pathKills);

            // Shockwave clears the landing spot, so landings near groups are worth more.
            if (stats.shockwaveRadius > 0f)
                plan.kills += 0.5f * CountNear(landing, stats.shockwaveRadius, pathKills);

            // Slight preference for open space over corners.
            float centerBonus = 1f - Mathf.Clamp01(landing.magnitude / (Arena.Bounds.width * 0.5f));

            plan.score = plan.kills * KillScore + (plan.bossHit ? BossHitScore : 0f)
                - plan.landingDanger * DangerWeight + centerBonus;
            return plan;
        }

        #endregion


        #region Simulation

        /// <summary>Same tracing as the comet's aim preview: walls use bounces, obstacles deflect for free.</summary>
        private void SimulatePath(Vector2 start, Vector2 direction, float distance, int bounces, float radius)
        {
            path.Clear();
            path.Add(start);
            pathHitsBossCore = false;

            Vector2 position = start;
            float distanceLeft = distance;

            while (distanceLeft > 0.01f && path.Count < 8)
            {
                DashCastResult hit = DashObstacles.Cast(position, direction * distanceLeft, Arena.Bounds, radius, out Vector2 end, out Vector2 normal);
                float moved = (end - position).magnitude;
                distanceLeft -= moved;
                position = end;
                path.Add(position);

                if (hit.isDashHit) pathHitsBossCore = true;
                if (!hit.HitAnything || moved < 0.001f) break;
                if (!hit.HitObstacle)
                {
                    if (bounces <= 0) break;
                    bounces--;
                }
                direction = Vector2.Reflect(direction, normal);
            }
        }

        private bool PathHits(Vector2 center, float targetRadius, float radius)
        {
            for (int i = 0; i < path.Count - 1; i++)
            {
                if (Geometry2D.SweptCircleHit(path[i], path[i + 1], radius, center, targetRadius))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// How dangerous it is to be stopped at a point over the next moment.
        /// 0 = safe; 1+ = likely to get hit. Enemies in ignore are about to die.
        /// </summary>
        private static float Danger(Vector2 point, float radius, HashSet<Enemy> ignore)
        {
            float danger = 0f;

            foreach (Enemy enemy in Enemy.Active)
            {
                if (!enemy.IsAlive || (ignore != null && ignore.Contains(enemy))) continue;

                if (enemy is PrismBoss boss)
                {
                    danger += BossDanger(boss, point, radius);
                    continue;
                }

                if (enemy is DartEnemy dart && dart.IsThreatening)
                {
                    Vector2 chargeEnd = dart.Position + dart.ChargeDirection * dart.ChargeLength;
                    if (Geometry2D.SweptCircleHit(dart.Position, chargeEnd, dart.Radius + 0.5f, point, radius))
                        danger += 4f;
                }

                // Enemies keep moving toward us, so give them a buffer.
                float reach = enemy.Radius + radius + 1.4f;
                float distance = (enemy.Position - point).magnitude;
                if (distance < reach)
                    danger += (1f - distance / reach) * (enemy.IsStunned ? 0.6f : 2f);
            }

            foreach (EnemyProjectile projectile in EnemyProjectiles.All)
            {
                if (!projectile.IsActive) continue;

                for (float t = 0f; t <= 0.9f; t += 0.3f)
                {
                    Vector2 future = projectile.Position + projectile.Velocity * t;
                    if (Geometry2D.CirclesOverlap(future, projectile.HitRadius + 0.4f, point, radius))
                    {
                        danger += 3f;
                        break;
                    }
                }
            }

            return danger;
        }

        private static float BossDanger(PrismBoss boss, Vector2 point, float radius)
        {
            float danger = 0f;
            float distance = (point - boss.Position).magnitude;

            if (distance < boss.coreRadius + radius + 0.7f) danger += 5f;

            // Inside the ring is close to the beams' origin, where they sweep fastest past you.
            if (distance < boss.shieldRadius) danger += boss.BeamsDangerous ? 1.5f : 0.3f;

            if (boss.BeamsDangerous)
            {
                int count = boss.BeamCount;
                for (int i = 0; i < count; i++)
                {
                    for (float t = 0f; t <= 0.9f; t += 0.3f)
                    {
                        Vector2 direction = boss.BeamDirection(i, count, t);
                        Vector2 end = boss.Position + direction * boss.beamLength;
                        if (Geometry2D.SweptCircleHit(boss.Position, end, boss.beamHalfWidth + 0.4f, point, radius))
                        {
                            danger += boss.BeamsActive ? 6f : 4f;
                            t = 1f;
                        }
                    }
                }
            }

            return danger;
        }

        private static int CountNear(Vector2 point, float range, HashSet<Enemy> ignore)
        {
            int count = 0;
            foreach (Enemy enemy in Enemy.Active)
            {
                if (enemy.IsAlive && !(enemy is PrismBoss) && !ignore.Contains(enemy) &&
                    Geometry2D.CirclesOverlap(point, range, enemy.Position, enemy.Radius))
                    count++;
            }
            return count;
        }

        #endregion

    }

}
