using System;
using System.Collections.Generic;
using UnityEngine;

namespace Com.MMsGamingCo.Ricomet
{

    /// <summary>
    /// The player's comet: slingshot aiming, dashing, dash energy, focus and contact
    /// damage. While dashing the comet is invulnerable and destroys what it passes
    /// through. While stopped it is vulnerable.
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
        public AimInput Input { get; private set; }

        public bool IsDashing { get { return State == CometState.Dashing; } }
        public Vector2 Position { get { return transform.position; } }

        /// <summary>Raised when a dash starts.</summary>
        public static event Action dashStarted;

        /// <summary>Raised with the number of kills when a dash ends.</summary>
        public static event Action<int> dashEnded;

        private PrototypeTuning tuning;

        // Dash state
        private Vector2 dashDirection;
        private float dashDistanceLeft;
        private int bouncesLeft;
        private int dashKills;
        private float graceTimer;
        private readonly HashSet<Enemy> hitThisDash = new HashSet<Enemy>();
        private readonly List<Enemy> enemyBuffer = new List<Enemy>();

        // Visuals
        private LineRenderer body;
        private LineRenderer core;
        private LineRenderer aimLine;
        private LineRenderer focusRing;
        private LineRenderer[] chargePips;
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
            CometHealth.cometDamaged += OnDamaged;
        }

        private void OnDisable()
        {
            Enemy.enemyKilled -= OnEnemyKilled;
            CometHealth.cometDamaged -= OnDamaged;
        }

        private void Update()
        {
            if (Health.IsDead)
            {
                SetAimVisuals(false);
                return;
            }

            Input.Tick();

            if (State == CometState.Dashing)
            {
                UpdateDash(Time.deltaTime);
            }
            else
            {
                UpdateAim();

                if (Input.DashRequested)
                    TryStartDash();

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
            Energy = new DashEnergy(tuning.maxCharges, 1f / Mathf.Max(0.1f, tuning.secondsPerCharge));
            Focus = new FocusMeter(tuning.focusSeconds, tuning.focusRefillPerSecond);
            Input = new AimInput(tuning);

            Health = gameObject.AddComponent<CometHealth>();
            Health.Setup(tuning.maxShields, tuning.invulnerableAfterHit);

            BuildVisuals();
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

            if (TimeController.Instance != null)
                TimeController.Instance.SetAimScale(canSlowDown ? tuning.aimTimeScale : 1f);
        }

        private void TryStartDash()
        {
            if (!Energy.Spend()) return;

            dashDirection = Input.Direction;
            dashDistanceLeft = tuning.dashDistance * Input.Power;
            bouncesLeft = tuning.bounceOffWalls ? tuning.maxWallBounces : 0;
            dashKills = 0;
            hitThisDash.Clear();
            State = CometState.Dashing;

            if (TimeController.Instance != null)
                TimeController.Instance.SetAimScale(1f);

            trail.emitting = true;
            dashStarted?.Invoke();
        }

        private void UpdateDash(float deltaTime)
        {
            float step = Mathf.Min(tuning.dashSpeed * deltaTime, dashDistanceLeft);
            Vector2 start = Position;

            bool hitWall = Geometry2D.MoveInsideBounds(start, dashDirection * step, Arena.Bounds, tuning.cometRadius, out Vector2 end, out Vector2 wallNormal);
            float moved = (end - start).magnitude;

            HitEnemiesAlong(start, end);
            transform.position = end;
            dashDistanceLeft -= moved;

            if (hitWall)
            {
                if (bouncesLeft > 0)
                {
                    bouncesLeft--;
                    dashDirection = Vector2.Reflect(dashDirection, wallNormal);
                    ScreenShake.Add(0.1f);
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
                    enemy.Damage(tuning.dashDamage);
                }
            }
        }

        private void EndDash()
        {
            State = CometState.Idle;
            graceTimer = tuning.postDashGrace;
            trail.emitting = false;

            EffectSpawner.Landing(Position);
            if (dashKills >= 3 && TimeController.Instance != null)
                TimeController.Instance.HitStop(tuning.multiKillHitStopSeconds);

            dashEnded?.Invoke(dashKills);
        }

        private void CheckContactDamage()
        {
            if (Health.IsInvulnerable) return;

            foreach (Enemy enemy in Enemy.Active)
            {
                if (enemy.IsAlive && enemy.DealsContactDamage &&
                    Geometry2D.CirclesOverlap(Position, tuning.cometRadius, enemy.Position, enemy.Radius))
                {
                    Health.TakeHit();
                    return;
                }
            }
        }

        private void OnEnemyKilled(Enemy enemy)
        {
            Energy.Refund(tuning.killRefund);
            Focus.Add(tuning.focusPerKill);
            if (IsDashing) dashKills++;
        }

        private void OnDamaged(int shieldsLeft)
        {
            EffectSpawner.Damage(Position);
            ScreenShake.Add(tuning.damageShake);
            if (TimeController.Instance != null)
                TimeController.Instance.HitStop(0.12f);

            if (shieldsLeft <= 0)
            {
                Input.Cancel();
                State = CometState.Idle;
                trail.emitting = false;
                if (TimeController.Instance != null)
                    TimeController.Instance.SetAimScale(1f);
            }
        }

        #endregion


        #region Visuals

        private void BuildVisuals()
        {
            body = ShapeFactory.CreatePolygon(transform, "Body", ShapeFactory.Circle(tuning.cometRadius, 20), NeonColors.Player, 0.1f, 10);
            core = ShapeFactory.CreatePolygon(transform, "Core", ShapeFactory.Circle(tuning.cometRadius * 0.35f, 10), NeonColors.White, 0.08f, 11);

            aimLine = ShapeFactory.CreateLine(transform, "Aim", NeonColors.Player, 0.06f, 9);
            aimLine.useWorldSpace = true;
            aimLine.textureMode = LineTextureMode.Tile;

            focusRing = ShapeFactory.CreateLine(transform, "FocusRing", NeonColors.Pickup, 0.06f, 9);

            chargePips = new LineRenderer[Mathf.CeilToInt(tuning.maxCharges)];
            for (int i = 0; i < chargePips.Length; i++)
                chargePips[i] = ShapeFactory.CreatePolygon(transform, "Pip", ShapeFactory.Circle(0.09f, 8), NeonColors.Player, 0.06f, 10);

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

        private void UpdateVisuals()
        {
            bool showAim = State == CometState.Aiming && !Input.InDeadZone;
            SetAimVisuals(showAim);
            if (showAim) DrawAimPreview();

            // Charge pips orbit above the comet; empty ones are dim.
            for (int i = 0; i < chargePips.Length; i++)
            {
                float angle = Mathf.PI * 0.5f + (i - (chargePips.Length - 1) * 0.5f) * 0.55f;
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
            bool canDash = Energy.CanDash;
            Color color = canDash ? NeonColors.Player : NeonColors.Disabled;
            aimLine.startColor = color;
            aimLine.endColor = NeonColors.WithAlpha(color, 0.1f);

            // Trace the path, including wall bounces when they are enabled.
            var points = new List<Vector3> { Position };
            Vector2 position = Position;
            Vector2 direction = Input.Direction;
            float distanceLeft = tuning.dashDistance * Input.Power;
            int bounces = tuning.bounceOffWalls ? tuning.maxWallBounces : 0;

            while (distanceLeft > 0.01f)
            {
                bool hitWall = Geometry2D.MoveInsideBounds(position, direction * distanceLeft, Arena.Bounds, tuning.cometRadius, out Vector2 end, out Vector2 normal);
                distanceLeft -= (end - position).magnitude;
                position = end;
                points.Add(position);

                if (!hitWall || bounces <= 0) break;
                bounces--;
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
