using System.Collections.Generic;
using UnityEngine;

namespace Com.MMsGamingCo.Ricomet
{

    /// <summary>
    /// Plasma Trail upgrade (trail_fire). Each dash leaves a glowing strand that
    /// destroys enemies touching it until it fades.
    /// </summary>
    public class PlasmaTrail : MonoBehaviour
    {
        private const float TrailRadius = 0.25f;
        private const float MinPointSpacing = 0.3f;

        private class Strand
        {
            public LineRenderer line;
            public readonly List<Vector2> points = new List<Vector2>();
            public readonly List<float> times = new List<float>();
            public bool open;
        }

        private readonly List<Strand> strands = new List<Strand>();
        private readonly List<Enemy> enemyBuffer = new List<Enemy>();
        private Strand current;

        private void OnEnable()
        {
            CometController.dashStarted += OnDashStarted;
            CometController.dashMoved += OnDashMoved;
            CometController.dashEnded += OnDashEnded;
        }

        private void OnDisable()
        {
            CometController.dashStarted -= OnDashStarted;
            CometController.dashMoved -= OnDashMoved;
            CometController.dashEnded -= OnDashEnded;
        }

        private float Lifetime
        {
            get
            {
                CometController comet = CometController.Instance;
                return comet != null ? comet.Stats.trailSeconds : 0f;
            }
        }

        private void Update()
        {
            float now = Time.time;
            float lifetime = Lifetime;

            foreach (Strand strand in strands)
            {
                // Drop points from the tail as they expire.
                while (strand.times.Count > 0 && now - strand.times[0] > lifetime)
                {
                    strand.times.RemoveAt(0);
                    strand.points.RemoveAt(0);
                }

                Redraw(strand, now, lifetime);
            }

            DamageEnemies();
        }

        private void OnDashStarted()
        {
            if (Lifetime <= 0f) return;

            current = GetStrand();
            current.open = true;
            AddPoint(current, CometController.Instance.Position, true);
        }

        private void OnDashMoved(Vector2 from, Vector2 to)
        {
            if (current != null)
                AddPoint(current, to, false);
        }

        private void OnDashEnded(Vector2 position, int kills)
        {
            if (current == null) return;

            AddPoint(current, position, true);
            current.open = false;
            current = null;
        }

        private void AddPoint(Strand strand, Vector2 point, bool force)
        {
            int last = strand.points.Count - 1;
            if (!force && last >= 0 && (strand.points[last] - point).sqrMagnitude < MinPointSpacing * MinPointSpacing)
                return;

            strand.points.Add(point);
            strand.times.Add(Time.time);
        }

        private void DamageEnemies()
        {
            enemyBuffer.Clear();
            enemyBuffer.AddRange(Enemy.Active);

            foreach (Strand strand in strands)
            {
                for (int i = 0; i < strand.points.Count - 1; i++)
                {
                    Vector2 a = strand.points[i];
                    Vector2 b = strand.points[i + 1];

                    foreach (Enemy enemy in enemyBuffer)
                    {
                        if (enemy.IsAlive && Geometry2D.SweptCircleHit(a, b, TrailRadius, enemy.Position, enemy.Radius))
                            enemy.Damage(1f, DamageSource.Trail);
                    }
                }
            }
        }

        private void Redraw(Strand strand, float now, float lifetime)
        {
            int count = strand.points.Count;
            strand.line.enabled = count >= 2;
            if (count < 2) return;

            strand.line.positionCount = count;
            for (int i = 0; i < count; i++)
                strand.line.SetPosition(i, strand.points[i]);

            // Older end fades out.
            float oldest = Mathf.Clamp01((now - strand.times[0]) / Mathf.Max(0.01f, lifetime));
            strand.line.startColor = NeonColors.WithAlpha(NeonColors.Player, 0.8f * (1f - oldest));
            strand.line.endColor = NeonColors.WithAlpha(NeonColors.White, 0.9f);
        }

        private Strand GetStrand()
        {
            foreach (Strand strand in strands)
            {
                if (!strand.open && strand.points.Count == 0) return strand;
            }

            Strand created = new Strand { line = ShapeFactory.CreateLine(transform, "Plasma", NeonColors.Player, TrailRadius * 1.2f, 3) };
            created.line.useWorldSpace = true;
            strands.Add(created);
            return created;
        }
    }

}
