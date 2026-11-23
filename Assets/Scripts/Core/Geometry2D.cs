using UnityEngine;

namespace Com.MMsGamingCo.Ricomet
{

    /// <summary>
    /// 2D math used for dash hit detection. Dashes are checked as a swept circle
    /// (a capsule) from the start to the end of each frame's movement, so a fast
    /// dash can never skip over an enemy between frames.
    /// </summary>
    public static class Geometry2D
    {

        /// <summary>Closest point to p on the segment a-b.</summary>
        public static Vector2 ClosestPointOnSegment(Vector2 a, Vector2 b, Vector2 p)
        {
            Vector2 ab = b - a;
            float lengthSq = ab.sqrMagnitude;
            if (lengthSq < 0.000001f) return a;

            float t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / lengthSq);
            return a + ab * t;
        }

        /// <summary>
        /// True if a circle of sweepRadius moving from a to b touches a circle of
        /// targetRadius at center.
        /// </summary>
        public static bool SweptCircleHit(Vector2 a, Vector2 b, float sweepRadius, Vector2 center, float targetRadius)
        {
            float reach = sweepRadius + targetRadius;
            return (ClosestPointOnSegment(a, b, center) - center).sqrMagnitude <= reach * reach;
        }

        public static bool CirclesOverlap(Vector2 a, float radiusA, Vector2 b, float radiusB)
        {
            float reach = radiusA + radiusB;
            return (a - b).sqrMagnitude <= reach * reach;
        }

        /// <summary>
        /// Moves from start toward start + delta inside bounds (shrunk by radius).
        /// Stops at the first wall hit and reports the wall's normal.
        /// </summary>
        /// <returns>True if a wall was hit.</returns>
        public static bool MoveInsideBounds(Vector2 start, Vector2 delta, Rect bounds, float radius, out Vector2 end, out Vector2 wallNormal)
        {
            float minX = bounds.xMin + radius, maxX = bounds.xMax - radius;
            float minY = bounds.yMin + radius, maxY = bounds.yMax - radius;

            float t = 1f;
            wallNormal = Vector2.zero;

            if (delta.x > 0 && start.x + delta.x > maxX) TryWall((maxX - start.x) / delta.x, Vector2.left, ref t, ref wallNormal);
            if (delta.x < 0 && start.x + delta.x < minX) TryWall((minX - start.x) / delta.x, Vector2.right, ref t, ref wallNormal);
            if (delta.y > 0 && start.y + delta.y > maxY) TryWall((maxY - start.y) / delta.y, Vector2.down, ref t, ref wallNormal);
            if (delta.y < 0 && start.y + delta.y < minY) TryWall((minY - start.y) / delta.y, Vector2.up, ref t, ref wallNormal);

            end = start + delta * t;
            end.x = Mathf.Clamp(end.x, minX, maxX);
            end.y = Mathf.Clamp(end.y, minY, maxY);
            return wallNormal != Vector2.zero;
        }

        private static void TryWall(float wallT, Vector2 normal, ref float t, ref Vector2 wallNormal)
        {
            wallT = Mathf.Clamp01(wallT);
            if (wallT < t)
            {
                t = wallT;
                wallNormal = normal;
            }
        }
    }

}
