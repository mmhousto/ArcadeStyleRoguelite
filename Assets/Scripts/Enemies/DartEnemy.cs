using UnityEngine;

namespace Com.MMsGamingCo.Ricomet
{

    /// <summary>
    /// Approaches slowly, stops and telegraphs, then charges in a straight line.
    /// Harmless while recovering after a charge, which is the moment to hit it.
    /// </summary>
    public class DartEnemy : Enemy
    {
        private enum DartState { Approach, Windup, Charge, Recover }

        private DartState state;
        private float stateTimer;
        private Vector2 chargeDirection;
        private LineRenderer telegraph;

        public override bool DealsContactDamage { get { return state != DartState.Recover; } }

        /// <summary>True while winding up or charging: the line ahead of it is dangerous.</summary>
        public bool IsThreatening { get { return IsAlive && !IsStunned && (state == DartState.Windup || state == DartState.Charge); } }

        /// <summary>Direction of the current or upcoming charge.</summary>
        public Vector2 ChargeDirection { get { return chargeDirection; } }

        /// <summary>How far a charge travels.</summary>
        public float ChargeLength { get { return tuning.dartChargeSpeed * tuning.dartChargeTime; } }

        protected override void BuildVisuals()
        {
            enemyId = "enemy_dart";
            radius = tuning.dartRadius;
            outline = ShapeFactory.CreatePolygon(transform, "Outline", ShapeFactory.Arrow(radius * 1.5f, radius * 0.75f), NeonColors.Enemy, 0.09f, 6);

            telegraph = ShapeFactory.CreateLine(transform, "Telegraph", NeonColors.Danger, 0.05f, 4);
            telegraph.useWorldSpace = true;
        }

        public override void OnSpawned()
        {
            base.OnSpawned();
            SetState(DartState.Approach, 0f);
        }

        protected override Color BaseColor()
        {
            if (state == DartState.Recover) return NeonColors.WithAlpha(NeonColors.Enemy, 0.45f);
            if (state == DartState.Windup || state == DartState.Charge) return NeonColors.Danger;
            return NeonColors.Enemy;
        }

        protected override void Tick(float deltaTime)
        {
            stateTimer -= deltaTime;
            Vector2 toTarget = TargetPosition() - Position;

            switch (state)
            {
                case DartState.Approach:
                    Face(toTarget);
                    transform.position = Position + (toTarget.normalized + Separation(1.2f)) * tuning.dartApproachSpeed * deltaTime;
                    if (toTarget.magnitude < tuning.dartChargeRange)
                        SetState(DartState.Windup, tuning.dartWindupTime);
                    break;

                case DartState.Windup:
                    // Keeps tracking until the last moment, then locks in.
                    if (stateTimer > tuning.dartWindupTime * 0.3f && toTarget.sqrMagnitude > 0.01f)
                        chargeDirection = toTarget.normalized;
                    Face(chargeDirection);
                    DrawTelegraph();
                    if (stateTimer <= 0f)
                        SetState(DartState.Charge, tuning.dartChargeTime);
                    break;

                case DartState.Charge:
                    Vector2 next = Position + chargeDirection * tuning.dartChargeSpeed * deltaTime;
                    Vector2 clamped = Arena.Clamp(next, radius);
                    transform.position = clamped;
                    if (stateTimer <= 0f || clamped != next)
                        SetState(DartState.Recover, tuning.dartRecoverTime);
                    break;

                case DartState.Recover:
                    if (stateTimer <= 0f)
                        SetState(DartState.Approach, 0f);
                    break;
            }
        }

        public override void ApplyKnockback(Vector2 velocity, float stunSeconds)
        {
            // A push interrupts a windup or charge: the Dart has to recover first.
            if (IsAlive && velocity != Vector2.zero && (state == DartState.Windup || state == DartState.Charge))
                SetState(DartState.Recover, tuning.dartRecoverTime);

            base.ApplyKnockback(velocity, stunSeconds);
        }

        private void SetState(DartState newState, float duration)
        {
            state = newState;
            stateTimer = duration;
            telegraph.enabled = newState == DartState.Windup;
            ShapeFactory.SetColor(outline, BaseColor());
        }

        private void DrawTelegraph()
        {
            float length = tuning.dartChargeSpeed * tuning.dartChargeTime;
            Vector2 end = Arena.Clamp(Position + chargeDirection * length, radius);

            // Brightens as the charge gets closer.
            float t = 1f - Mathf.Clamp01(stateTimer / tuning.dartWindupTime);
            Color color = NeonColors.WithAlpha(NeonColors.Danger, Mathf.Lerp(0.15f, 0.8f, t));
            telegraph.startColor = color;
            telegraph.endColor = NeonColors.WithAlpha(color, 0f);
            telegraph.positionCount = 2;
            telegraph.SetPosition(0, Position);
            telegraph.SetPosition(1, end);
        }
    }

}
