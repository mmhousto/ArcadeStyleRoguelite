using UnityEngine;

namespace Com.MMsGamingCo.Ricomet
{

    /// <summary>
    /// A shard from the Shatter upgrade. Flies outward and destroys the first enemy it touches.
    /// </summary>
    public class Fragment : MonoBehaviour
    {
        private const float Speed = 14f;
        private const float Lifetime = 0.4f;
        private const float Radius = 0.2f;

        private NeonShape line;
        private Vector2 velocity;
        private float timer;

        public bool IsActive { get; private set; }

        private void Awake()
        {
            line = NeonShape.Create(transform, "shard", 0.3f, new[] { new Vector2(-0.25f, 0f), new Vector2(0.25f, 0f) }, NeonColors.Player, 7, 0.08f);
        }

        public void Launch(Vector2 position, Vector2 direction)
        {
            transform.position = position;
            velocity = direction.normalized * Speed;
            transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
            timer = 0f;
            IsActive = true;
            gameObject.SetActive(true);
        }

        private void Update()
        {
            timer += Time.deltaTime;
            Vector2 start = transform.position;
            Vector2 end = start + velocity * Time.deltaTime;
            transform.position = end;

            foreach (Enemy enemy in Enemy.Active)
            {
                if (enemy.IsAlive && Geometry2D.SweptCircleHit(start, end, Radius, enemy.Position, enemy.Radius))
                {
                    // Stop before damaging: a kill can change Enemy.Active.
                    Stop();
                    enemy.Damage(1f, DamageSource.Fragment);
                    return;
                }
            }

            if (timer >= Lifetime || !Arena.Bounds.Contains(end))
                Stop();
        }

        private void Stop()
        {
            IsActive = false;
            gameObject.SetActive(false);
        }
    }

}
