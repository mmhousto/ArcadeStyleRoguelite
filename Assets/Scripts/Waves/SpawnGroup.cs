using System;

namespace Com.MMsGamingCo.Ricomet
{

    /// <summary>
    /// Part of a wave: count enemies of one type, starting at startTime seconds
    /// into the wave, one every interval seconds.
    /// </summary>
    [Serializable]
    public class SpawnGroup
    {
        public string enemyId;
        public int count = 1;
        public float startTime = 0f;
        public float interval = 0.5f;

        public SpawnGroup(string enemyId, int count, float startTime, float interval)
        {
            this.enemyId = enemyId;
            this.count = count;
            this.startTime = startTime;
            this.interval = interval;
        }

        /// <summary>Time the last enemy of this group spawns.</summary>
        public float EndTime { get { return startTime + Math.Max(0, count - 1) * interval; } }
    }

}
