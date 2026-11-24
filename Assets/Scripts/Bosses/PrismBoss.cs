using System;
using System.Collections.Generic;
using UnityEngine;

namespace Com.MMsGamingCo.Ricomet
{

    /// <summary>
    /// Sector 1 boss. A rotating crystal core inside a spinning shield ring.
    /// Dashes ricochet off the solid shield segments, so the player has to slip
    /// through a gap (or bank off the ring) to hit the core. The core sweeps laser
    /// beams, fires orb rings and calls in minions. Three phases at 66% and 33% health.
    /// </summary>
    public class PrismBoss : Enemy, IDashObstacle
    {

        #region Fields/Variables

        private enum BeamState { Resting, Telegraph, Active }

        [Header("Prism")]
        public float bossHealth = 24f;
        public float coreRadius = 1.3f;
        public float shieldRadius = 3.6f;
        [Tooltip("Seconds the boss is untouchable while it appears.")]
        public float introTime = 2f;
        [Tooltip("Seconds of invulnerability when it changes phase.")]
        public float phaseChangeTime = 1.5f;
        [Tooltip("Seconds between hits, so effects like Plasma Trail can't melt it.")]
        public float hitCooldown = 0.2f;
        public int defeatPoints = 5000;

        [Header("Beams")]
        public float beamTelegraphTime = 1.1f;
        public float beamActiveTime = 1.8f;
        public float beamRestTime = 2.2f;
        public float beamHalfWidth = 0.22f;
        public float beamLength = 40f;

        public static readonly float[] PhaseThresholds = { 0.66f, 0.33f };

        /// <summary>Raised when the Prism appears.</summary>
        public static event Action<PrismBoss> bossAppeared;

        /// <summary>Raised with the new phase number (1-based) when the phase changes.</summary>
        public static event Action<int> phaseChanged;

        public static event Action bossDefeated;

        public static PrismBoss Current { get; private set; }

        public int Phase { get; private set; }
        public float HealthFraction { get { return Mathf.Clamp01(Health / maxHealth); } }
        public bool IsInvulnerable { get { return introTimer > 0f || phaseChangeTimer > 0f; } }
        public string DisplayName { get { return "PRISM"; } }

        /// <summary>True while beams are warning or firing.</summary>
        public bool BeamsDangerous { get { return beamState != BeamState.Resting && phaseChangeTimer <= 0f && introTimer <= 0f; } }
        public bool BeamsActive { get { return beamState == BeamState.Active && phaseChangeTimer <= 0f; } }
        public int BeamCount { get { return CurrentPhaseSettings.beamCount; } }
        public float BeamSweepSpeed { get { return CurrentPhaseSettings.beamSweepSpeed; } }

        /// <summary>Raised when beams start firing (after their warning).</summary>
        public static event Action beamsFired;

        private readonly PrismPhase[] phases =
        {
            new PrismPhase("XXX-XXX-", 35f, 2, 22f, 0f, 0, 7f, EnemyIds.Drifter, EnemyIds.Drifter, EnemyIds.Drifter),
            new PrismPhase("XX-XX-X-", -55f, 3, 30f, 4.5f, 8, 9f, EnemyIds.Drifter, EnemyIds.Drifter, EnemyIds.Dart),
            new PrismPhase("X-X-X-X-", 80f, 4, 38f, 3.5f, 12, 10f, EnemyIds.Splitter, EnemyIds.Drifter, EnemyIds.Drifter),
        };

        private RotatingShield shield;
        private float introTimer;
        private float phaseChangeTimer;
        private float hitCooldownTimer;
        private BeamState beamState = BeamState.Resting;
        private float beamTimer;
        private float beamAngle;
        private float volleyTimer;
        private float minionTimer;
        private float volleyOffset;

        private LineRenderer innerCrystal;
        private readonly List<LineRenderer> shieldSegments = new List<LineRenderer>();
        private readonly List<LineRenderer> beams = new List<LineRenderer>();

        private PrismPhase CurrentPhaseSettings { get { return phases[Mathf.Clamp(Phase, 0, phases.Length - 1)]; } }

        #endregion


        #region MonoBehaviour Methods

        protected override void OnEnable()
        {
            base.OnEnable();
            DashObstacles.Register(this);
            Current = this;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            DashObstacles.Unregister(this);
            if (Current == this) Current = null;
        }

        #endregion


        #region Public Methods

        /// <summary>Starts the fight. Call once after creating the boss.</summary>
        public void Begin()
        {
            OnSpawned();
            Phase = 0;
            introTimer = introTime;
            beamState = BeamState.Resting;
            beamTimer = beamRestTime;
            volleyTimer = 2f;
            minionTimer = 3f;
            ApplyPhase();
            bossAppeared?.Invoke(this);
        }

        public bool TryDeflect(Vector2 start, Vector2 end, float cometRadius, out Vector2 hitPoint, out Vector2 normal, out bool isDashHit)
        {
            hitPoint = end;
            normal = Vector2.zero;
            isDashHit = false;
            if (!IsAlive) return false;

            // The core is a bumper: dashes ricochet off it, so the comet never ends up inside.
            bool hitCore = Geometry2D.BounceOffCircle(Position, coreRadius, start, end, cometRadius, out Vector2 corePoint, out Vector2 coreNormal);

            // No shield while it's still appearing (the ring is drawn smaller then).
            bool hitShield = false;
            Vector2 shieldPoint = end, shieldNormal = Vector2.zero;
            if (introTimer <= 0f)
                hitShield = shield.TryDeflect(Position, start, end, cometRadius, out shieldPoint, out shieldNormal);

            if (hitShield && (!hitCore || (shieldPoint - start).sqrMagnitude < (corePoint - start).sqrMagnitude))
            {
                hitPoint = shieldPoint;
                normal = shieldNormal;
                return true;
            }

            if (hitCore)
            {
                hitPoint = corePoint;
                normal = coreNormal;
                isDashHit = true;
                return true;
            }

            return false;
        }

        /// <summary>Bosses don't get pushed around.</summary>
        public override void ApplyKnockback(Vector2 velocity, float stunSeconds)
        {
        }

        #endregion


        #region Enemy Overrides

        protected override void BuildVisuals()
        {
            enemyId = BossIds.Prism;
            maxHealth = bossHealth;
            radius = coreRadius;
            points = defeatPoints;

            // Two counter-rotating triangles read as a crystal.
            outline = ShapeFactory.CreatePolygon(transform, "Crystal", ShapeFactory.Triangle(coreRadius * 1.15f), NeonColors.Enemy, 0.14f, 6);
            innerCrystal = ShapeFactory.CreatePolygon(transform, "Inner", ShapeFactory.RegularPolygon(coreRadius * 0.75f, 3, 180f), NeonColors.White, 0.1f, 7);

            shield = new RotatingShield(shieldRadius, RotatingShield.Pattern(phases[0].shieldPattern));
        }

        protected override float AdjustDamage(float amount, DamageSource source)
        {
            if (IsInvulnerable || hitCooldownTimer > 0f) return 0f;

            hitCooldownTimer = hitCooldown;
            float damage = amount * BossRules.DamageMultiplier(source);

            // Never skip a phase in one hit: stop at the threshold.
            int nextPhase = Phase + 1;
            if (nextPhase < phases.Length)
            {
                float floor = PhaseThresholds[Phase] * maxHealth;
                if (Health > floor && Health - damage < floor)
                    damage = Health - floor;
            }
            return damage;
        }

        protected override void Tick(float deltaTime)
        {
            if (hitCooldownTimer > 0f) hitCooldownTimer -= deltaTime;
            PushCometOutOfCore();

            if (introTimer > 0f)
            {
                introTimer -= deltaTime;
                float grow = 1f - Mathf.Clamp01(introTimer / introTime);
                transform.localScale = Vector3.one * Mathf.SmoothStep(0.2f, 1f, grow);
                UpdateVisuals(deltaTime);
                return;
            }
            transform.localScale = Vector3.one;

            CheckPhase();

            if (phaseChangeTimer > 0f)
            {
                phaseChangeTimer -= deltaTime;
                SetBeamsVisible(false);
                UpdateVisuals(deltaTime);
                return;
            }

            UpdateBeams(deltaTime);
            UpdateVolleys(deltaTime);
            UpdateMinions(deltaTime);
            UpdateVisuals(deltaTime);
        }

        protected override void OnDied()
        {
            SetBeamsVisible(false);
            EnemyProjectiles.ClearAll();

            // Big finish: several bursts, heavy shake and a long hit stop.
            for (int i = 0; i < 6; i++)
            {
                Vector2 offset = UnityEngine.Random.insideUnitCircle * shieldRadius * 0.8f;
                EffectSpawner.Kill(Position + offset, i % 2 == 0 ? NeonColors.Enemy : NeonColors.White);
            }
            EffectSpawner.Shockwave(Position, shieldRadius * 2f);
            ScreenShake.Add(1f);
            if (TimeController.Instance != null)
                TimeController.Instance.HitStop(0.5f);

            bossDefeated?.Invoke();
        }

        #endregion


        #region Private Methods

        /// <summary>
        /// Safety net: a stopped comet should never sit inside the core (the bumper bounce
        /// prevents it), but if it does, push it out instead of hitting it every second.
        /// </summary>
        private void PushCometOutOfCore()
        {
            CometController comet = CometController.Instance;
            if (comet == null || comet.IsDashing || comet.Health.IsDead) return;

            Vector2 away = comet.Position - Position;
            float contact = coreRadius + comet.Radius;
            if (away.sqrMagnitude >= contact * contact) return;

            Vector2 direction = away.sqrMagnitude > 0.0001f ? away.normalized : Vector2.left;
            comet.PushTo(Position + direction * (contact + 0.6f));
        }

        private void CheckPhase()
        {
            // Small margin so landing exactly on a threshold always counts.
            int target = BossRules.PhaseFor(HealthFraction - 0.0001f, PhaseThresholds);
            if (target <= Phase) return;

            Phase = target;
            phaseChangeTimer = phaseChangeTime;
            beamState = BeamState.Resting;
            beamTimer = beamRestTime * 0.5f;
            ApplyPhase();

            EffectSpawner.Shockwave(Position, shieldRadius * 1.5f);
            ScreenShake.Add(0.6f);
            EnemyProjectiles.ClearAll();
            phaseChanged?.Invoke(Phase + 1);
        }

        private void ApplyPhase()
        {
            shield.SetSegments(RotatingShield.Pattern(CurrentPhaseSettings.shieldPattern));
            BuildShieldVisuals();
            BuildBeams(CurrentPhaseSettings.beamCount);
        }

        private void UpdateBeams(float deltaTime)
        {
            PrismPhase settings = CurrentPhaseSettings;
            beamTimer -= deltaTime;

            switch (beamState)
            {
                case BeamState.Resting:
                    SetBeamsVisible(false);
                    if (beamTimer <= 0f)
                    {
                        beamState = BeamState.Telegraph;
                        beamTimer = beamTelegraphTime;
                        beamAngle = UnityEngine.Random.Range(0f, 360f);
                    }
                    break;

                case BeamState.Telegraph:
                    beamAngle += settings.beamSweepSpeed * 0.5f * deltaTime;
                    DrawBeams(false);
                    if (beamTimer <= 0f)
                    {
                        beamState = BeamState.Active;
                        beamTimer = beamActiveTime;
                        ScreenShake.Add(0.15f);
                        beamsFired?.Invoke();
                    }
                    break;

                case BeamState.Active:
                    beamAngle += settings.beamSweepSpeed * deltaTime;
                    DrawBeams(true);
                    CheckBeamHits(settings.beamCount);
                    if (beamTimer <= 0f)
                    {
                        beamState = BeamState.Resting;
                        beamTimer = beamRestTime;
                    }
                    break;
            }
        }

        private void CheckBeamHits(int count)
        {
            CometController comet = CometController.Instance;
            if (comet == null) return;

            for (int i = 0; i < count; i++)
            {
                Vector2 direction = BeamDirection(i, count);
                if (Geometry2D.SweptCircleHit(Position, Position + direction * beamLength, beamHalfWidth, comet.Position, comet.Radius))
                {
                    comet.TryDamage();
                    return;
                }
            }
        }

        private void UpdateVolleys(float deltaTime)
        {
            PrismPhase settings = CurrentPhaseSettings;
            if (settings.volleyInterval <= 0f) return;

            volleyTimer -= deltaTime;
            if (volleyTimer > 0f) return;

            volleyTimer = settings.volleyInterval;
            volleyOffset += 360f / Mathf.Max(1, settings.volleyCount) * 0.5f;
            EnemyProjectiles.FireRing(Position, settings.volleyCount, settings.volleySpeed, volleyOffset);
        }

        private void UpdateMinions(float deltaTime)
        {
            PrismPhase settings = CurrentPhaseSettings;
            if (settings.minionInterval <= 0f || WaveDirector.Instance == null) return;

            minionTimer -= deltaTime;
            if (minionTimer > 0f) return;

            minionTimer = settings.minionInterval;
            // Don't let minions pile up if the player is ignoring them.
            if (Enemy.Active.Count > 12) return;

            foreach (string minion in settings.minions)
                WaveDirector.Instance.SpawnWithWarning(minion);
        }

        /// <summary>Direction of a beam, optionally predicted extraSeconds ahead.</summary>
        public Vector2 BeamDirection(int index, int count, float extraSeconds = 0f)
        {
            float angle = (beamAngle + BeamSweepSpeed * extraSeconds + index * 360f / count) * Mathf.Deg2Rad;
            return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        }

        #endregion


        #region Visuals

        private void UpdateVisuals(float deltaTime)
        {
            shield.Rotation += CurrentPhaseSettings.shieldSpeed * deltaTime;
            outline.transform.Rotate(0f, 0f, 40f * deltaTime);
            innerCrystal.transform.Rotate(0f, 0f, -70f * deltaTime);

            // The shield objects are children, so rotating them matches the shield math.
            foreach (LineRenderer segment in shieldSegments)
                segment.transform.localRotation = Quaternion.Euler(0f, 0f, shield.Rotation);

            // Dim while invulnerable so the player knows hits won't count.
            float alpha = IsInvulnerable ? 0.35f + 0.25f * Mathf.Sin(Time.time * 20f) : 1f;
            ShapeFactory.SetColor(innerCrystal, NeonColors.WithAlpha(NeonColors.White, alpha));
            foreach (LineRenderer segment in shieldSegments)
                ShapeFactory.SetColor(segment, NeonColors.WithAlpha(NeonColors.Enemy, 0.5f + 0.5f * alpha));
        }

        private void BuildShieldVisuals()
        {
            foreach (LineRenderer segment in shieldSegments)
                Destroy(segment.gameObject);
            shieldSegments.Clear();

            float segmentAngle = shield.SegmentAngle;
            for (int i = 0; i < shield.Segments.Length; i++)
            {
                if (!shield.Segments[i]) continue;

                // Arc for this segment, drawn in the shield's local (unrotated) space.
                var points = new Vector2[9];
                for (int p = 0; p < points.Length; p++)
                {
                    float angle = (i * segmentAngle + segmentAngle * p / (points.Length - 1)) * Mathf.Deg2Rad;
                    points[p] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * shieldRadius;
                }

                LineRenderer segment = ShapeFactory.CreateLine(transform, "Shield", NeonColors.Enemy, 0.18f, 6);
                ShapeFactory.SetPoints(segment, points);
                shieldSegments.Add(segment);
            }
        }

        private void BuildBeams(int count)
        {
            while (beams.Count < count)
            {
                LineRenderer beam = ShapeFactory.CreateLine(transform, "Beam", NeonColors.Danger, 0.1f, 5);
                beam.useWorldSpace = true;
                beams.Add(beam);
            }
        }

        private void DrawBeams(bool active)
        {
            int count = CurrentPhaseSettings.beamCount;
            for (int i = 0; i < beams.Count; i++)
            {
                LineRenderer beam = beams[i];
                beam.enabled = i < count;
                if (!beam.enabled) continue;

                Vector2 direction = BeamDirection(i, count);
                beam.positionCount = 2;
                beam.SetPosition(0, Position + direction * coreRadius);
                beam.SetPosition(1, Position + direction * beamLength);

                // Telegraph: thin and flickering. Active: thick and solid.
                float width = active ? beamHalfWidth * 2f : 0.05f;
                beam.startWidth = width;
                beam.endWidth = width;
                float alpha = active ? 1f : 0.35f + 0.25f * Mathf.Sin(Time.time * 25f);
                ShapeFactory.SetColor(beam, NeonColors.WithAlpha(NeonColors.Danger, alpha));
            }
        }

        private void SetBeamsVisible(bool visible)
        {
            foreach (LineRenderer beam in beams)
                beam.enabled = visible;
        }

        #endregion

    }

}
