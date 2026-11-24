using UnityEngine;

namespace Com.MMsGamingCo.Ricomet
{

    /// <summary>
    /// A slow orange orb fired by bosses. Hurts the comet when it's stopped; a dash
    /// smashes straight through it.
    /// </summary>
    public class EnemyProjectile : MonoBehaviour
    {
        private const float Radius = 0.28f;

        private NeonShape outline;
        private Vector2 velocity;
        private float lifetime;

        public bool IsActive { get; private set; }
        public Vector2 Position { get { return transform.position; } }
        public Vector2 Velocity { get { return velocity; } }
        public float HitRadius { get { return Radius; } }

        private void Awake()
        {
            outline = NeonShape.Create(transform, "orb", Radius * 1.1f, ShapeFactory.Circle(Radius, 10), NeonColors.Danger, 6);
        }

        public void Launch(Vector2 position, Vector2 newVelocity, float seconds)
        {
            transform.position = position;
            velocity = newVelocity;
            lifetime = seconds;
            IsActive = true;
            gameObject.SetActive(true);
        }

        private void Update()
        {
            lifetime -= Time.deltaTime;
            Vector2 position = (Vector2)transform.position + velocity * Time.deltaTime;
            transform.position = position;

            // Pulse so orbs read as dangerous.
            outline.transform.localScale = Vector3.one * (1f + 0.15f * Mathf.Sin(Time.time * 18f));

            CometController comet = CometController.Instance;
            if (comet != null && !comet.Health.IsDead && Geometry2D.CirclesOverlap(position, Radius, comet.Position, comet.Radius))
            {
                if (comet.IsDashing)
                {
                    EffectSpawner.Kill(position, NeonColors.Danger);
                    Despawn();
                    return;
                }

                if (comet.TryDamage("orb"))
                {
                    Despawn();
                    return;
                }
            }

            if (lifetime <= 0f || !Arena.Bounds.Contains(position))
                Despawn();
        }

        public void Despawn()
        {
            IsActive = false;
            gameObject.SetActive(false);
        }
    }

}
