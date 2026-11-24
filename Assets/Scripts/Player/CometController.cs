using System;
using System.Collections.Generic;
using UnityEngine;

namespace Com.MMsGamingCo.Ricomet
{

    /// <summary>
    /// The player's comet: slingshot aiming, dashing, dash energy, focus and contact
    /// damage. While dashing the comet is invulnerable and destroys what it passes
    /// through. While stopped it is vulnerable. Numbers come from CometStats, so
    /// upgrades change them during a run.
    /// </summary>
    public class CometController : MonoBehaviour
    {

        #region Fields/Variables

        private static CometController _instance;

        public static CometController Instance { get { return _instance; } }

        public enum CometState { Idle, Aiming, Dashing }

        public CometState State { get; private set; } = CometState.Idle;
        public DashEnergy Energy { get; private set; }
        public FocusMeter Focus { get; private set; }
        public CometHealth Health { get; private set; }
        public IAimSource Input { get; private set; }

        /// <summary>What caused the last hit ("beam", "orb", "touched enemy_dart"...), for playtest reports.</summary>
        public string LastHitCause { get; private set; } = "";

        /// <summary>True while the playtest bot is flying the comet.</summary>
        public bool IsBotControlled { get { return !(Input is AimInput); } }
        public CometStats Stats { get; private set; }

        public bool IsDashing { get { return State == CometState.Dashing; } }
        public Vector2 Position { get { return transform.position; } }
        public float Radius { get { return tuning.cometRadius; } }

        /// <summary>
        /// Blocks aiming and dashing, e.g. while the upgrade picker is open.
        /// A touch that starts while locked never turns into a dash.
        /// </summary>
        public bool InputLocked
        {
            get { return inputLocked; }
            set
            {
                // The button that closed a menu must not also fire a dash.
                if (inputLocked && !value) unlockFrame = Time.frameCount;
                inputLocked = value;
            }
        }

        /// <summary>Raised when a dash starts.</summary>
        public static event Action dashStarted;

        /// <summary>Raised every frame of a dash with the segment moved, for trails.</summary>
        public static event Action<Vector2, Vector2> dashMoved;

        /// <summary>Raised with the landing position and number of kills when a dash ends.</summary>
        public static event Action<Vector2, int> dashEnded;

        /// <summary>Raised when a dash bounces off an arena wall.</summary>
        public static event Action wallBounced;

        /// <summary>Raised when a dash ricochets off an obstacle such as a boss shield.</summary>
        public static event Action obstacleDeflected;

        private PrototypeTuning tuning;
        private bool inputLocked = false;
        private int unlockFrame = -10;

        // Dash state
        private Vector2 dashDirection;
        private float dashDistanceLeft;
        private int bouncesLeft;
        private int redirectsLeft;
        private int dashKills;
        private float graceTimer;
        private readonly HashSet<Enemy> hitThisDash = new HashSet<Enemy>();
        private readonly List<Enemy> enemyBuffer = new List<Enemy>();

        // Visuals
        private LineRenderer body;
        private LineRenderer core;
        private LineRenderer aimLine;
        private LineRenderer focusRing;
        private readonly List<LineRenderer> chargePips = new List<LineRenderer>();
        private TrailRenderer trail;

        #endregion


        #region MonoBehaviour Methods

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(this.gameObject);
                return;
            }
            _instance = this;
        }

        private void OnDestroy()
        {
            if (_instance == this) _instance = null;
        }

        private void OnEnable()
        {
            Enemy.enemyKilled += OnEnemyKilled;
            Enemy.enemyDamaged += OnEnemyDamaged;
            CometHealth.cometDamaged += OnDamaged;
        }

        private void OnDisable()
        {
            Enemy.enemyKilled -= OnEnemyKilled;
            Enemy.enemyDamaged -= OnEnemyDamaged;
            CometHealth.cometDamaged -= OnDamaged;
        }

        private void Update()
        {
            if (Health.IsDead)
            {
                SetAimVisuals(false);
                return;
            }

            // Covers the unlock frame and the next, since Update order between scripts isn't fixed.
            bool justUnlocked = Time.frameCount <= unlockFrame + 1;

            if (InputLocked || justUnlocked)
            {
                Input.Cancel();
                if (State == CometState.Aiming) State = CometState.Idle;
                SetAimScale(1f);
            }
            else
            {
                Input.Tick();
            }

            if (State == CometState.Dashing)
            {
                UpdateDash(Time.deltaTime);
            }
            else
            {
                if (!InputLocked && !justUnlocked)
                {
                    UpdateAim();
                    if (Input.DashRequested)
                        TryStartDash();
                }

                if (State != CometState.Dashing)
                {
                    Energy.Regen(Time.deltaTime);
                    if (graceTimer > 0f) graceTimer -= Time.deltaTime;
                    else CheckContactDamage();
                }
            }

            UpdateVisuals();
        }

        #endregion


        #region Public Methods

        /// <summary>Called once by the bootstrap after the component is added.</summary>
        public void Setup(PrototypeTuning newTuning)
        {
            tuning = newTuning;
            Stats = CometStats.FromTuning(tuning);
            Energy = new DashEnergy(Stats.maxCharges, 1f / Mathf.Max(0.1f, Stats.secondsPerCharge));
            Focus = new FocusMeter(Stats.focusSeconds, tuning.focusRefillPerSecond);
            Input = new AimInput(tuning);

            Health = gameObject.AddComponent<CometHealth>();
            Health.Setup(Stats.maxShields, tuning.invulnerableAfterHit);

            BuildVisuals();
        }

        /// <summary>
        /// Swaps who controls the comet. Pass null to give control back to the player.
        /// </summary>
        public void SetInputSource(IAimSource source)
        {
            Input.Cancel();
            Input = source ?? new AimInput(tuning);
            SetAimScale(1f);
            if (State == CometState.Aiming) State = CometState.Idle;
        }

        /// <summary>Applies new stats after an upgrade. Extra charges and shields are filled.</summary>
        public void ApplyStats(CometStats newStats)
        {
            Stats = newStats;
            Energy.SetMax(Stats.maxCharges);
            Energy.RegenPerSecond = 1f / Mathf.Max(0.1f, Stats.secondsPerCharge);
            Focus.SetMax(Stats.focusSeconds);
            Health.SetMaxShields(Stats.maxShields);
            RebuildPips();
        }

        #endregion


        #region Private Methods

        private void UpdateAim()
        {
            bool canSlowDown = Input.IsAiming && !Input.InDeadZone && Energy.CanDash && (Focus.HasFocus || !tuning.limitFocus);

            if (Input.IsAiming)
            {
                State = CometState.Aiming;
                if (canSlowDown && tuning.limitFocus)
                    Focus.Drain(Time.unscaledDeltaTime);
            }
            else
            {
                State = CometState.Idle;
                Focus.Refill(Time.unscaledDeltaTime);
            }

            SetAimScale(canSlowDown ? tuning.aimTimeScale : 1f);
        }

        private void TryStartDash()
        {
            if (!Energy.Spend()) return;

            dashDirection = Input.Direction;
            dashDistanceLeft = Stats.dashDistance * Input.Power;
            bouncesLeft = Stats.wallBounces;
            redirectsLeft = Stats.killRedirects;
            dashKills = 0;
            hitThisDash.Clear();
            State = CometState.Dashing;

            SetAimScale(1f);
            trail.emitting = true;
            dashStarted?.Invoke();
        }

        private void UpdateDash(float deltaTime)
        {
            float step = Mathf.Min(Stats.dashSpeed * deltaTime, dashDistanceLeft);
            Vector2 start = Position;

            DashCastResult hit = DashObstacles.Cast(start, dashDirection * step, Arena.Bounds, tuning.cometRadius, out Vector2 end, out Vector2 wallNormal);
            float moved = (end - start).magnitude;

            transform.position = end;
            dashDistanceLeft -= moved;
            dashMoved?.Invoke(start, end);

            // After moving, so a Seeker redirect from a kill here applies to the next step.
            HitEnemiesAlong(start, end);

            if (State != CometState.Dashing) return;

            if (hit.HitObstacle)
            {
                // Obstacles (boss shields and cores) always deflect and don't use up wall bounces.
                dashDirection = Vector2.Reflect(dashDirection, wallNormal);
                EffectSpawner.Landing(end, 0.9f);
                ScreenShake.Add(0.12f);

                if (hit.isDashHit && hit.obstacle is Enemy target && target.IsAlive && !hitThisDash.Contains(target))
                {
                    hitThisDash.Add(target);
                    target.Damage(Stats.dashDamage, DamageSource.Dash);
                    ScreenShake.Add(0.2f);
                }
                else if (!hit.isDashHit)
                {
                    obstacleDeflected?.Invoke();
                }
            }
            else if (hit.hitWall)
            {
                if (bouncesLeft > 0)
                {
                    bouncesLeft--;
                    dashDirection = Vector2.Reflect(dashDirection, wallNormal);
                    ScreenShake.Add(0.1f);
                    wallBounced?.Invoke();
                }
                else
                {
                    EndDash();
                    return;
                }
            }

            if (dashDistanceLeft <= 0.001f)
                EndDash();
        }

        private void HitEnemiesAlong(Vector2 start, Vector2 end)
        {
            // Copy first: killing an enemy removes it from the registry.
            enemyBuffer.Clear();
            enemyBuffer.AddRange(Enemy.Active);

            foreach (Enemy enemy in enemyBuffer)
            {
                if (!enemy.IsAlive || hitThisDash.Contains(enemy)) continue;

                if (Geometry2D.SweptCircleHit(start, end, tuning.cometRadius, enemy.Position, enemy.Radius))
                {
                    hitThisDash.Add(enemy);
                    enemy.Damage(Stats.dashDamage, DamageSource.Dash);
                }
            }
        }

        private void EndDash()
        {
            State = CometState.Idle;
            graceTimer = Mathf.Max(tuning.postDashGrace, Stats.phaseSeconds);
            trail.emitting = false;

            EffectSpawner.Landing(Position, tuning.landingPushRadius);
            PushEnemies(tuning.landingPushRadius, tuning.landingPushSpeed, tuning.landingStunTime);
            if (dashKills >= 3 && TimeController.Instance != null)
                TimeController.Instance.HitStop(tuning.multiKillHitStopSeconds);

            dashEnded?.Invoke(Position, dashKills);
        }

        /// <summary>
        /// Seeker upgrade: turn the dash toward the nearest enemy not already hit.
        /// </summary>
        private void TryRedirect()
        {
            Enemy nearest = null;
            float nearestDistance = float.MaxValue;

            foreach (Enemy enemy in Enemy.Active)
            {
                if (!enemy.IsAlive || hitThisDash.Contains(enemy)) continue;

                float distance = (enemy.Position - Position).sqrMagnitude;
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearest = enemy;
                }
            }

            float range = Stats.dashDistance;
            if (nearest == null || nearestDistance > range * range) return;

            redirectsLeft--;
            dashDirection = (nearest.Position - Position).normalized;
            dashDistanceLeft = Mathf.Max(dashDistanceLeft, Mathf.Sqrt(nearestDistance) + 1f);
            EffectSpawner.Lightning(Position, nearest.Position, NeonColors.Player);
        }

        private void CheckContactDamage()
        {
            if (Health.IsInvulnerable) return;

            foreach (Enemy enemy in Enemy.Active)
            {
                if (enemy.CanHurtComet &&
                    Geometry2D.CirclesOverlap(Position, tuning.cometRadius, enemy.Position, enemy.Radius))
                {
                    LastHitCause = "touched " + enemy.enemyId;
                    Health.TakeHit();
                    return;
                }
            }
        }

        /// <summary>
        /// Boss hits refund energy like kills do. Without this, attacking a boss drains
        /// your charges and leaves you stranded right when its attacks peak.
        /// </summary>
        private void OnEnemyDamaged(Enemy enemy, DamageSource source)
        {
            if (!(enemy is PrismBoss) || source != DamageSource.Dash || !enemy.IsAlive) return;

            Energy.Refund(tuning.bossHitRefund);
            Focus.Add(tuning.focusPerKill * 2f);
        }

        private void OnEnemyKilled(Enemy enemy)
        {
            Energy.Refund(Stats.killRefund);
            Focus.Add(tuning.focusPerKill);

            if (!IsDashing) return;

            dashKills++;
            if (enemy.LastDamageSource == DamageSource.Dash && redirectsLeft > 0)
                TryRedirect();
        }

        /// <summary>
        /// Damage from hazards (beams, projectiles). Ignored while dashing, during the
        /// post-dash grace period and while invulnerable. Returns true if the hit landed.
        /// </summary>
        public bool TryDamage(string cause)
        {
            if (IsDashing || graceTimer > 0f || Health.IsDead || Health.IsInvulnerable) return false;
            LastHitCause = cause;
            return Health.TakeHit();
        }

        /// <summary>Moves a stopped comet, e.g. pushed out of a boss core. Ignored while dashing.</summary>
        public void PushTo(Vector2 position)
        {
            if (IsDashing) return;
            transform.position = Arena.Clamp(position, tuning.cometRadius);
            EffectSpawner.Landing(Position, 1.2f);
        }

        /// <summary>Knocks back and stuns every enemy within radius of the comet.</summary>
        private void PushEnemies(float radius, float speed, float stunSeconds)
        {
            foreach (Enemy enemy in Enemy.Active)
            {
                Vector2 velocity = Knockback.Velocity(Position, enemy.Position, radius + enemy.Radius, speed);
                if (velocity != Vector2.zero)
                    enemy.ApplyKnockback(velocity, stunSeconds);
            }
        }

        private void OnDamaged(int shieldsLeft)
        {
            EffectSpawner.Damage(Position);
            PushEnemies(tuning.hitPushRadius, tuning.hitPushSpeed, tuning.hitStunTime);
            ScreenShake.Add(tuning.damageShake);
            if (TimeController.Instance != null)
                TimeController.Instance.HitStop(0.12f);

            if (shieldsLeft <= 0)
            {
                Input.Cancel();
                State = CometState.Idle;
                trail.emitting = false;
                SetAimScale(1f);
            }
        }

        private static void SetAimScale(float scale)
        {
            if (TimeController.Instance != null)
                TimeController.Instance.SetAimScale(scale);
        }

        #endregion


        #region Visuals

        private void BuildVisuals()
        {
            body = ShapeFactory.CreatePolygon(transform, "Body", ShapeFactory.Circle(tuning.cometRadius, 20), NeonColors.Player, 0.1f, 10);
            core = ShapeFactory.CreatePolygon(transform, "Core", ShapeFactory.Circle(tuning.cometRadius * 0.35f, 10), NeonColors.White, 0.08f, 11);

            aimLine = ShapeFactory.CreateLine(transform, "Aim", NeonColors.Player, 0.06f, 9);
            aimLine.useWorldSpace = true;

            focusRing = ShapeFactory.CreateLine(transform, "FocusRing", NeonColors.Pickup, 0.06f, 9);

            RebuildPips();

            trail = gameObject.AddComponent<TrailRenderer>();
            trail.sharedMaterial = ShapeFactory.LineMaterial;
            trail.time = 0.25f;
            trail.startWidth = tuning.cometRadius * 1.6f;
            trail.endWidth = 0f;
            trail.startColor = NeonColors.Player;
            trail.endColor = NeonColors.WithAlpha(NeonColors.Player, 0f);
            trail.sortingOrder = 8;
            trail.emitting = false;
        }

        /// <summary>One pip per charge; called again when Reserve adds a charge.</summary>
        private void RebuildPips()
        {
            int count = Mathf.CeilToInt(Energy != null ? Energy.Max : tuning.maxCharges);
            while (chargePips.Count < count)
                chargePips.Add(ShapeFactory.CreatePolygon(transform, "Pip", ShapeFactory.Circle(0.09f, 8), NeonColors.Player, 0.06f, 10));
            for (int i = 0; i < chargePips.Count; i++)
                chargePips[i].gameObject.SetActive(i < count);
        }

        private void UpdateVisuals()
        {
            bool showAim = State == CometState.Aiming && !Input.InDeadZone;
            SetAimVisuals(showAim);
            if (showAim) DrawAimPreview();

            // Charge pips sit in an arc above the comet; empty ones are dim.
            int count = Mathf.CeilToInt(Energy.Max);
            for (int i = 0; i < count && i < chargePips.Count; i++)
            {
                float angle = Mathf.PI * 0.5f + (i - (count - 1) * 0.5f) * 0.5f;
                chargePips[i].transform.localPosition = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * (tuning.cometRadius + 0.35f);

                float fill = Mathf.Clamp01(Energy.Current - i);
                Color pipColor = fill >= 1f ? NeonColors.Player : NeonColors.WithAlpha(NeonColors.Disabled, 0.25f + 0.5f * fill);
                ShapeFactory.SetColor(chargePips[i], pipColor);
            }

            // Blink while invulnerable after a hit.
            bool visible = !Health.IsInvulnerable || Mathf.Repeat(Time.unscaledTime * 12f, 1f) > 0.5f;
            body.enabled = visible && !Health.IsDead;
            core.enabled = visible && !Health.IsDead;
        }

        private void SetAimVisuals(bool show)
        {
            aimLine.enabled = show;
            focusRing.enabled = show && tuning.limitFocus;
        }

        private void DrawAimPreview()
        {
            Color color = Energy.CanDash ? NeonColors.Player : NeonColors.Disabled;
            aimLine.startColor = color;
            aimLine.endColor = NeonColors.WithAlpha(color, 0.1f);

            // Trace the path, including wall bounces.
            var points = new List<Vector3> { Position };
            Vector2 position = Position;
            Vector2 direction = Input.Direction;
            float distanceLeft = Stats.dashDistance * Input.Power;
            int bounces = Stats.wallBounces;

            while (distanceLeft > 0.01f)
            {
                DashCastResult hit = DashObstacles.Cast(position, direction * distanceLeft, Arena.Bounds, tuning.cometRadius, out Vector2 end, out Vector2 normal);
                float moved = (end - position).magnitude;
                distanceLeft -= moved;
                position = end;
                points.Add(position);

                if (!hit.HitAnything) break;
                if (!hit.HitObstacle)
                {
                    if (bounces <= 0) break;
                    bounces--;
                }
                // Stop the preview getting stuck on tiny steps.
                if (moved < 0.001f || points.Count > 12) break;
                direction = Vector2.Reflect(direction, normal);
            }

            aimLine.positionCount = points.Count;
            aimLine.SetPositions(points.ToArray());

            if (focusRing.enabled)
                ShapeFactory.SetPoints(focusRing, ShapeFactory.Arc(tuning.cometRadius + 0.2f, Focus.Fraction));
        }

        #endregion

    }

}
