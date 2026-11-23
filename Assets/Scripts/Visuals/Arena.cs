using UnityEngine;

namespace Com.MMsGamingCo.Ricomet
{

    /// <summary>
    /// The play area: a neon border with a faint grid. Everything that moves is kept
    /// inside Arena.Bounds.
    /// </summary>
    public class Arena : MonoBehaviour
    {
        private static Arena _instance;

        public static Arena Instance { get { return _instance; } }

        /// <summary>Play area in world units, centered on the origin.</summary>
        public static Rect Bounds { get; private set; } = new Rect(-15f, -8f, 30f, 16f);

        public float gridSpacing = 2f;

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

        private void AddGridLine(Vector2 from, Vector2 to)
        {
            LineRenderer line = ShapeFactory.CreateLine(transform, "Grid", NeonColors.Grid, 0.04f, -10);
            ShapeFactory.SetPoints(line, new[] { from, to });
        }
    }

}
