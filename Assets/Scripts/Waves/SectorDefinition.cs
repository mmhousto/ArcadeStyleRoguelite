using System;
using System.Collections.Generic;

namespace Com.MMsGamingCo.Ricomet
{

    /// <summary>
    /// A sector: a run of waves in one arena, ending with a boss (from 0.3).
    /// </summary>
    [Serializable]
    public class SectorDefinition
    {
        /// <summary>Stable ID. Never rename after release.</summary>
        public string id;
        public string displayName;
        public List<WaveDefinition> waves = new List<WaveDefinition>();

        public SectorDefinition(string id, string displayName, params WaveDefinition[] sectorWaves)
        {
            this.id = id;
            this.displayName = displayName;
            waves.AddRange(sectorWaves);
        }
    }

}
