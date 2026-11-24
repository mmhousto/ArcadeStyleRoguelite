using UnityEngine;

namespace Com.MMsGamingCo.Ricomet
{

    /// <summary>
    /// Slow hexagon that takes two hits and bursts into two small, fast Drifters
    /// when destroyed. Rewards area damage (Shockwave, Shatter, Arc).
    /// </summary>
    public class SplitterEnemy : Enemy
    {
        private const int ShardCount = 2;

        protected override void BuildVisuals()
        {
            enemyId = "enemy_splitter";
            radius = tuning.splitterRadius;
            maxHealth = 2f;
            points = 200;
            outline = NeonShape.Create(transform, "splitter", radius, ShapeFactory.RegularPolygon(radius, 6, 30f), NeonColors.Enemy, 6);
        }

        protected override void Tick(float deltaTime)
        {
            Vector2 toTarget = TargetPosition() - Position;
            Vector2 velocity = (toTarget.normalized + Separation(1.6f) * 1.5f) * tuning.splitterSpeed;
            transform.position = Position + velocity * deltaTime;
            outline.transform.Rotate(0f, 0f, 30f * deltaTime);

            // Cracks and shrinks a little once damaged, so players can see it's nearly done.
            bool damaged = Health < maxHealth;
            outline.transform.localScale = Vector3.one * (damaged ? 0.85f : 1f);
            outline.SetArt(damaged ? "splitter_cracked" : "splitter");
        }

        protected override void OnDied()
        {
            if (EnemyFactory.Instance == null) return;

            Vector2 toTarget = TargetPosition() - Position;
            Vector2 side = toTarget.sqrMagnitude > 0.01f ? new Vector2(-toTarget.y, toTarget.x).normalized : Vector2.up;

            for (int i = 0; i < ShardCount; i++)
            {
                Vector2 offset = side * (i == 0 ? 0.5f : -0.5f);
                EnemyFactory.Instance.Spawn(EnemyIds.DrifterMini, Position + offset);
            }
        }

        public override void OnSpawned()
        {
            base.OnSpawned();
            outline.transform.localScale = Vector3.one;
        }
    }

}
