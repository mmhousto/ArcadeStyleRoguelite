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
        /// <returns>What was hit, if anything.</returns>
        public static DashCastResult Cast(Vector2 start, Vector2 delta, Rect bounds, float radius, out Vector2 end, out Vector2 normal)
        {
            DashCastResult result = new DashCastResult();
            result.hitWall = Geometry2D.MoveInsideBounds(start, delta, bounds, radius, out end, out normal);

            float bestDistance = (end - start).sqrMagnitude;
            foreach (IDashObstacle obstacle in active)
            {
                if (!obstacle.TryDeflect(start, end, radius, out Vector2 hitPoint, out Vector2 hitNormal, out bool isDashHit)) continue;

                float distance = (hitPoint - start).sqrMagnitude;
                if (distance <= bestDistance)
                {
                    bestDistance = distance;
                    end = hitPoint;
                    normal = hitNormal;
                    result.obstacle = obstacle;
                    result.isDashHit = isDashHit;
                    // An obstacle closer than the wall means the wall wasn't reached.
                    result.hitWall = false;
                }
            }

            return result;
        }
    }

}
