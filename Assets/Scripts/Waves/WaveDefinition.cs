using System;
using System.Collections.Generic;

namespace Com.MMsGamingCo.Ricomet
{

    /// <summary>
    /// One wave: groups of enemies spawned over time. The wave ends once every
    /// enemy has spawned and been destroyed.
    /// </summary>
    [Serializable]
    public class WaveDefinition
    {
        public List<SpawnGroup> groups = new List<SpawnGroup>();
        [UnityEngine.Tooltip("Clear the wave faster than this for a time bonus.")]
        public float parTime = 25f;

        public WaveDefinition(float parTime, params SpawnGroup[] spawnGroups)
        {
            this.parTime = parTime;
            groups.AddRange(spawnGroups);
        }

        public int TotalEnemies()
        {
            int total = 0;
            foreach (SpawnGroup group in groups)
                total += group.count;
            return total;
        }

        /// <summary>Time the last enemy spawns.</summary>
        public float SpawnDuration()
        {
            float end = 0f;
            foreach (SpawnGroup group in groups)
                end = Math.Max(end, group.EndTime);
            return end;
        }
    }

}
