using System;
using System.Collections.Generic;

namespace Com.MMsGamingCo.Ricomet
{

    /// <summary>
    /// A sector: a run of waves in one arena, ending with a boss.
    /// </summary>
    [Serializable]
    public class SectorDefinition
    {
        /// <summary>Stable ID. Never rename after release.</summary>
        public string id;
        public string displayName;
        public List<WaveDefinition> waves = new List<WaveDefinition>();
        [UnityEngine.Tooltip("Boss fought after the last wave. Empty = no boss.")]
        public string bossId;

        public bool HasBoss { get { return !string.IsNullOrEmpty(bossId); } }

        public SectorDefinition(string id, string displayName, string bossId, params WaveDefinition[] sectorWaves)
        {
            this.id = id;
            this.displayName = displayName;
            this.bossId = bossId;
            waves.AddRange(sectorWaves);
        }
    }

}
