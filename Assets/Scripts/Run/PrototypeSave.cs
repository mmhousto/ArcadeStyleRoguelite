using System;
using Com.MMsGamingCo.Core;

namespace Com.MMsGamingCo.Ricomet
{

    /// <summary>
    /// Best results from the prototype. Replaced by the full player profile later.
    /// </summary>
    [Serializable]
    public class PrototypeSave : SaveDataBase
    {
        public const string Slot = "prototype";

        public int bestScore = 0;
        public int bestCombo = 0;
        public float bestTime = 0f;
        public int runsPlayed = 0;
        public int bestWave = 0;
        public int sectorsCleared = 0;
        public int bossesDefeated = 0;

        public override int CurrentSchemaVersion { get { return 3; } }

        public override void Normalize()
        {
            // Version 1 (0.1) had no wave tracking and version 2 (0.2) no boss count;
            // the new fields default to 0, so there is nothing to convert.
            base.Normalize();
        }
    }

}
