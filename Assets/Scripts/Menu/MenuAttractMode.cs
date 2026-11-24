using System.Collections.Generic;
using UnityEngine;

namespace Com.MMsGamingCo.Ricomet
{

    /// <summary>
    /// Background for the main menu: enemies drift around the neon grid and a comet
    /// ricochets through them, popping them in bursts. Purely visual.
    /// </summary>
    public class MenuAttractMode : MonoBehaviour
    {
        private const int EnemyCount = 9;
        private const float CometSpeed = 30f;
        private const float CometRadius = 0.45f;

        private class Floater
        {
            public NeonShape shape;
            public Vector2 velocity;
            public float spin;
            public float radius;
            public float respawnTimer;
        }

        private readonly List<Floater> floaters = new List<Floater>();
        private NeonShape comet;
        private TrailRenderer trail;
        private Vector2 cometDirection;
        private float dashLeft;
        private float restTimer = 1f;

        private static readonly string[] Shapes = { "drifter", "dart", "splitter" };

        private void Start()
        {
            for (int i = 0; i < EnemyCount; i++)
            {
                var floater = new Floater { radius = Random.Range(0.45f, 0.75f) };
                string art = Shapes[i % Shapes.Length];
                floater.shape = NeonShape.Create(transform, art, floater.radius, ShapeFactory.Triangle(floater.radius), NeonColors.WithAlpha(NeonColors.Enemy, 0.85f), 6);
                Respawn(floater);
                floaters.Add(floater);
            }

            comet = NeonShape.Create(transform, "comet", CometRadius, ShapeFactory.Circle(CometRadius, 20), NeonColors.Player, 10, 0.1f);
            trail = comet.gameObject.AddComponent<TrailRenderer>();
            trail.sharedMaterial = ShapeFactory.LineMaterial;
            trail.time = 0.3f;
            trail.startWidth = CometRadius * 1.6f;
            trail.endWidth = 0f;
            trail.startColor = NeonColors.Player;
            trail.endColor = NeonColors.WithAlpha(NeonColors.Player, 0f);
            trail.sortingOrder = 8;
        }

        private void Update()
        {
            float dt = Time.deltaTime;

            foreach (Floater floater in floaters)
            {
                if (floater.respawnTimer > 0f)
                {
                    floater.respawnTimer -= dt;
                    if (floater.respawnTimer <= 0f) Respawn(floater);
                    continue;
                }

                Vector2 position = (Vector2)floater.shape.transform.position + floater.velocity * dt;
                Rect bounds = Arena.Bounds;
                if (position.x < bounds.xMin + 1f || position.x > bounds.xMax - 1f) floater.velocity.x = -floater.velocity.x;
                if (position.y < bounds.yMin + 1f || position.y > bounds.yMax - 1f) floater.velocity.y = -floater.velocity.y;
                floater.shape.transform.position = Arena.Clamp(position, 1f);
                floater.shape.transform.Rotate(0f, 0f, floater.spin * dt);
            }

            UpdateComet(dt);
        }

        private void UpdateComet(float dt)
        {
            if (dashLeft <= 0f)
            {
                restTimer -= dt;
                if (restTimer > 0f) return;

                // Aim at a random live floater, like a player lining up a shot.
                Floater target = floaters[Random.Range(0, floaters.Count)];
                Vector2 to = (Vector2)target.shape.transform.position - (Vector2)comet.transform.position;
                cometDirection = to.sqrMagnitude > 0.01f ? to.normalized : Random.insideUnitCircle.normalized;
                dashLeft = Mathf.Clamp(to.magnitude + 3f, 6f, 14f);
                restTimer = Random.Range(0.6f, 1.3f);
                GameAudio.Play(GameAudio.Dash, 0.25f, 0.1f);
            }

            Vector2 start = comet.transform.position;
            float step = Mathf.Min(CometSpeed * dt, dashLeft);
            bool hitWall = Geometry2D.MoveInsideBounds(start, cometDirection * step, Arena.Bounds, CometRadius, out Vector2 end, out Vector2 normal);
            comet.transform.position = end;
            dashLeft -= Mathf.Max(0.01f, (end - start).magnitude);
            if (hitWall) cometDirection = Vector2.Reflect(cometDirection, normal);

            foreach (Floater floater in floaters)
            {
                if (floater.respawnTimer > 0f) continue;
                if (!Geometry2D.SweptCircleHit(start, end, CometRadius, floater.shape.transform.position, floater.radius)) continue;

                EffectSpawner.Kill(floater.shape.transform.position, NeonColors.Enemy);
                floater.shape.SetVisible(false);
                floater.respawnTimer = Random.Range(1.5f, 3f);
            }

            if (dashLeft <= 0f) EffectSpawner.Landing(end, 2f);
        }

        private void Respawn(Floater floater)
        {
            floater.shape.transform.position = Arena.RandomPoint(new SeededRandom(Random.Range(1, 99999)), 2f);
            floater.velocity = Random.insideUnitCircle.normalized * Random.Range(0.6f, 1.6f);
            floater.spin = Random.Range(-60f, 60f);
            floater.shape.SetVisible(true);
        }
    }

}
