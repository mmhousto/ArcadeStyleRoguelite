using UnityEngine;

namespace Com.MMsGamingCo.Ricomet
{

    /// <summary>
    /// Builds glowing outline shapes from LineRenderers, so the prototype needs no
    /// art assets. These are stand-ins for the vector sprites in the full game.
    /// </summary>
    public static class ShapeFactory
    {
        private static Material lineMaterial;

        /// <summary>Shared unlit material for every line. Sprites/Default supports vertex colors.</summary>
        public static Material LineMaterial
        {
            get
            {
                if (lineMaterial == null)
                    lineMaterial = new Material(Shader.Find("Sprites/Default"));
                return lineMaterial;
            }
        }

        /// <summary>Adds a LineRenderer child that draws a closed polygon.</summary>
        public static LineRenderer CreatePolygon(Transform parent, string shapeName, Vector2[] points, Color color, float width, int sortingOrder = 0)
        {
            LineRenderer line = CreateLine(parent, shapeName, color, width, sortingOrder);
            line.loop = true;
            SetPoints(line, points);
            return line;
        }

        /// <summary>Adds an open line with no points yet.</summary>
        public static LineRenderer CreateLine(Transform parent, string lineName, Color color, float width, int sortingOrder = 0)
        {
            GameObject lineObject = new GameObject(lineName);
            lineObject.transform.SetParent(parent, false);

            LineRenderer line = lineObject.AddComponent<LineRenderer>();
            line.sharedMaterial = LineMaterial;
            line.useWorldSpace = false;
            line.startWidth = width;
            line.endWidth = width;
            line.startColor = color;
            line.endColor = color;
            line.numCornerVertices = 2;
            line.numCapVertices = 2;
            line.sortingOrder = sortingOrder;
            line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            line.receiveShadows = false;
            line.positionCount = 0;
            return line;
        }

        public static void SetPoints(LineRenderer line, Vector2[] points)
        {
            line.positionCount = points.Length;
            for (int i = 0; i < points.Length; i++)
                line.SetPosition(i, points[i]);
        }

        public static void SetColor(LineRenderer line, Color color)
        {
            line.startColor = color;
            line.endColor = color;
        }

        #region Point Sets

        public static Vector2[] Circle(float radius, int segments = 24)
        {
            return RegularPolygon(radius, segments, 0f);
        }

        /// <summary>Points of a regular polygon. rotation is in degrees.</summary>
        public static Vector2[] RegularPolygon(float radius, int sides, float rotation)
        {
            var points = new Vector2[sides];
            float start = rotation * Mathf.Deg2Rad;
            for (int i = 0; i < sides; i++)
            {
                float angle = start + i * Mathf.PI * 2f / sides;
                points[i] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
            }
            return points;
        }

        /// <summary>Triangle pointing along +X.</summary>
        public static Vector2[] Triangle(float radius)
        {
            return RegularPolygon(radius, 3, 0f);
        }

        /// <summary>Long thin arrow pointing along +X, for the Dart.</summary>
        public static Vector2[] Arrow(float length, float width)
        {
            return new[]
            {
                new Vector2(length, 0f),
                new Vector2(-length * 0.6f, width),
                new Vector2(-length * 0.3f, 0f),
                new Vector2(-length * 0.6f, -width),
            };
        }

        public static Vector2[] Rectangle(Vector2 size)
        {
            Vector2 half = size * 0.5f;
            return new[]
            {
                new Vector2(-half.x, -half.y),
                new Vector2(-half.x, half.y),
                new Vector2(half.x, half.y),
                new Vector2(half.x, -half.y),
            };
        }

        /// <summary>Points along an arc, used for ring meters. fraction 1 = full circle.</summary>
        public static Vector2[] Arc(float radius, float fraction, int maxSegments = 32)
        {
            fraction = Mathf.Clamp01(fraction);
            int segments = Mathf.Max(2, Mathf.CeilToInt(maxSegments * fraction) + 1);
            var points = new Vector2[segments];
            for (int i = 0; i < segments; i++)
            {
                // Starts at the top and runs clockwise.
                float angle = Mathf.PI * 0.5f - (i / (float)(segments - 1)) * fraction * Mathf.PI * 2f;
                points[i] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
            }
            return points;
        }

        #endregion
    }

}
