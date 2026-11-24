using UnityEngine;

namespace Com.MMsGamingCo.Ricomet
{

    /// <summary>
    /// Something the comet ricochets off mid-dash, like the Prism's shield and core.
    /// Deflections are free: they don't use up wall bounces.
    /// </summary>
    public interface IDashObstacle
    {
        /// <summary>
        /// Checks the comet's movement from start to end. On a hit, gives the point to
        /// stop at and the surface normal to reflect off. isDashHit is true when the
        /// part that was hit takes damage from the dash (a boss core, not its shield).
        /// </summary>
        bool TryDeflect(Vector2 start, Vector2 end, float cometRadius, out Vector2 hitPoint, out Vector2 normal, out bool isDashHit);
    }

}
