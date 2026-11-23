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
    }

}
