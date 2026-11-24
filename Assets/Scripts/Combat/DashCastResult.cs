namespace Com.MMsGamingCo.Ricomet
{

    /// <summary>
    /// What a dash step ran into: nothing, a wall, or an obstacle.
    /// </summary>
    public struct DashCastResult
    {
        public bool hitWall;
        /// <summary>The obstacle that deflected the dash, or null.</summary>
        public IDashObstacle obstacle;
        /// <summary>True when the obstacle part hit takes damage (e.g. a boss core).</summary>
        public bool isDashHit;

        public bool HitObstacle { get { return obstacle != null; } }
        public bool HitAnything { get { return hitWall || obstacle != null; } }
    }

}
