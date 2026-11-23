using System.Collections.Generic;
using UnityEngine;

namespace Com.MMsGamingCo.Ricomet
{

    /// <summary>
    /// Every active IDashObstacle, checked by the comet each dash step and by the aim preview.
    /// </summary>
    public static class DashObstacles
    {
        private static readonly List<IDashObstacle> active = new List<IDashObstacle>();

        public static IReadOnlyList<IDashObstacle> Active { get { return active; } }

        public static void Register(IDashObstacle obstacle)
        {
            if (obstacle != null && !active.Contains(obstacle)) active.Add(obstacle);
        }

        public static void Unregister(IDashObstacle obstacle)
        {
            active.Remove(obstacle);
        }

        /// <summary>
        /// Moves from start toward end and stops at the first obstacle or wall.
        /// </summary>
        /// <returns>True if anything was hit. isObstacle tells which.</returns>
        public static bool Cast(Vector2 start, Vector2 delta, Rect bounds, float radius, out Vector2 end, out Vector2 normal, out bool isObstacle)
        {
            bool hitWall = Geometry2D.MoveInsideBounds(start, delta, bounds, radius, out end, out normal);
            isObstacle = false;

            float bestDistance = (end - start).sqrMagnitude;
            foreach (IDashObstacle obstacle in active)
            {
                if (!obstacle.TryDeflect(start, end, radius, out Vector2 hitPoint, out Vector2 hitNormal)) continue;

                float distance = (hitPoint - start).sqrMagnitude;
                if (distance <= bestDistance)
                {
                    bestDistance = distance;
                    end = hitPoint;
                    normal = hitNormal;
                    isObstacle = true;
                }
            }

            return hitWall || isObstacle;
        }
    }

}
