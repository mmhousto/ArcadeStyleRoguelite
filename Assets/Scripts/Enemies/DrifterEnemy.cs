using UnityEngine;

namespace Com.MMsGamingCo.Ricomet
{

    /// <summary>
    /// Basic fodder. Drifts toward the comet with a slight wobble.
    /// </summary>
    public class DrifterEnemy : Enemy
    {
        [Tooltip("Size multiplier. Splitter shards are small, fast Drifters.")]
        public float sizeScale = 1f;
        [Tooltip("Speed multiplier.")]
        public float speedScale = 1f;

        private float wobbleOffset;
        private float spin;

        protected override void BuildVisuals()
        {
            enemyId = sizeScale < 1f ? "enemy_drifter_mini" : "enemy_drifter";
            radius = tuning.drifterRadius * sizeScale;
            points = sizeScale < 1f ? 50 : 100;
            outline = ShapeFactory.CreatePolygon(transform, "Outline", ShapeFactory.Triangle(radius * 1.2f), NeonColors.Enemy, 0.09f, 6);
        }

        public override void OnSpawned()
        {
            base.OnSpawned();
            wobbleOffset = Random.value * 10f;
            spin = Random.Range(-90f, 90f);
        }

        protected override void Tick(float deltaTime)
        {
            Vector2 toTarget = TargetPosition() - Position;
            Vector2 direction = toTarget.sqrMagnitude > 0.01f ? toTarget.normalized : Vector2.zero;

            // Sideways wobble so groups don't move in perfect lines.
            Vector2 side = new Vector2(-direction.y, direction.x);
            float wobble = Mathf.Sin(Time.time * 2.5f + wobbleOffset) * 0.35f;

            Vector2 velocity = (direction + side * wobble + Separation(1.2f) * 1.5f) * tuning.drifterSpeed * speedScale;
            transform.position = Position + velocity * deltaTime;

            outline.transform.Rotate(0f, 0f, spin * deltaTime);
        }
    }

}
