using System.Collections.Generic;
using UnityEngine;

namespace Com.MMsGamingCo.Ricomet
{

    /// <summary>
    /// The play area: a neon border with a faint grid. Everything that moves is kept
    /// inside Arena.Bounds. Impacts send ripples through the grid.
    /// </summary>
    public class Arena : MonoBehaviour
    {
        private static Arena _instance;

        public static Arena Instance { get { return _instance; } }

        /// <summary>Play area in world units, centered on the origin.</summary>
        public static Rect Bounds { get; private set; } = new Rect(-15f, -8f, 30f, 16f);

        public float gridSpacing = 2f;
        [Tooltip("Distance between grid line points. Smaller = smoother ripples, more work.")]
        public float gridResolution = 0.5f;

        private const float RippleSpeed = 20f;
        private const float RippleLifetime = 0.7f;
        private const float RippleBand = 1.6f;
        private const int MaxRipples = 6;

        private class GridLine
        {
            public LineRenderer line;
            public Vector3[] basePoints;
            public Vector3[] points;
        }

        private struct RippleWave
        {
            public Vector2 center;
            public float age;
            public float strength;
        }

        private readonly List<GridLine> gridLines = new List<GridLine>();
        private readonly List<RippleWave> ripples = new List<RippleWave>();
        private bool gridDisplaced = false;

        private void Awake()
        {
            _instance = this;
        }

        private void OnDestroy()
        {
            if (_instance == this) _instance = null;
        }

        /// <summary>Sets the arena size and draws it.</summary>
        public void Build(Vector2 size)
        {
            Bounds = new Rect(-size * 0.5f, size);

            ShapeFactory.CreatePolygon(transform, "Border", ShapeFactory.Rectangle(size), NeonColors.ArenaLine, 0.12f, 1);

            // Faint grid lines, the background for the neon look.
            for (float x = Bounds.xMin + gridSpacing; x < Bounds.xMax - 0.01f; x += gridSpacing)
                AddGridLine(new Vector2(x, Bounds.yMin), new Vector2(x, Bounds.yMax));
            for (float y = Bounds.yMin + gridSpacing; y < Bounds.yMax - 0.01f; y += gridSpacing)
                AddGridLine(new Vector2(Bounds.xMin, y), new Vector2(Bounds.xMax, y));
        }

        /// <summary>A random point inside the arena, kept away from the walls.</summary>
        public static Vector2 RandomPoint(SeededRandom random, float margin)
        {
            return new Vector2(
                random.Range(Bounds.xMin + margin, Bounds.xMax - margin),
                random.Range(Bounds.yMin + margin, Bounds.yMax - margin));
        }

        /// <summary>Clamps a position so a circle of the given radius stays inside.</summary>
        public static Vector2 Clamp(Vector2 position, float radius)
        {
            position.x = Mathf.Clamp(position.x, Bounds.xMin + radius, Bounds.xMax - radius);
            position.y = Mathf.Clamp(position.y, Bounds.yMin + radius, Bounds.yMax - radius);
            return position;
        }

        /// <summary>Sends a ripple through the grid from a point. strength is in world units.</summary>
        public static void Ripple(Vector2 position, float strength)
        {
            if (_instance == null || strength <= 0f) return;

            if (_instance.ripples.Count >= MaxRipples)
                _instance.ripples.RemoveAt(0);
            _instance.ripples.Add(new RippleWave { center = position, age = 0f, strength = strength });
        }

        private void Update()
        {
            if (ripples.Count == 0)
            {
                if (gridDisplaced) ResetGrid();
                return;
            }

            for (int i = ripples.Count - 1; i >= 0; i--)
            {
                RippleWave wave = ripples[i];
                wave.age += Time.deltaTime;
                if (wave.age >= RippleLifetime) ripples.RemoveAt(i);
                else ripples[i] = wave;
            }

            foreach (GridLine grid in gridLines)
            {
                for (int p = 0; p < grid.basePoints.Length; p++)
                {
                    Vector2 point = grid.basePoints[p];
                    Vector2 offset = Vector2.zero;

                    foreach (RippleWave wave in ripples)
                    {
                        Vector2 away = point - wave.center;
                        float distance = away.magnitude;
                        float front = wave.age * RippleSpeed;
                        float band = distance - front;
                        if (distance < 0.01f || Mathf.Abs(band) > RippleBand) continue;

                        // A single bump that travels outward and fades.
                        float shape = Mathf.Cos(band / RippleBand * Mathf.PI * 0.5f);
                        float fade = 1f - wave.age / RippleLifetime;
                        offset += away / distance * wave.strength * shape * fade;
                    }

                    grid.points[p] = point + offset;
                }
                grid.line.SetPositions(grid.points);
            }
            gridDisplaced = true;
        }

        private void ResetGrid()
        {
            foreach (GridLine grid in gridLines)
                grid.line.SetPositions(grid.basePoints);
            gridDisplaced = false;
        }

        private void AddGridLine(Vector2 from, Vector2 to)
        {
            LineRenderer line = ShapeFactory.CreateLine(transform, "Grid", NeonColors.Grid, 0.04f, -10);
            line.useWorldSpace = true;

            int count = Mathf.Max(2, Mathf.CeilToInt(Vector2.Distance(from, to) / gridResolution) + 1);
            var grid = new GridLine { line = line, basePoints = new Vector3[count], points = new Vector3[count] };
            for (int i = 0; i < count; i++)
                grid.basePoints[i] = Vector2.Lerp(from, to, i / (float)(count - 1));

            line.positionCount = count;
            line.SetPositions(grid.basePoints);
            gridLines.Add(grid);
        }
    }

}
