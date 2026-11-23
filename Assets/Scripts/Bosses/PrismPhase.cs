using System;

namespace Com.MMsGamingCo.Ricomet
{

    /// <summary>
    /// Settings for one phase of the Prism fight.
    /// </summary>
    [Serializable]
    public class PrismPhase
    {
        [UnityEngine.Tooltip("Shield pattern: X = solid, - = gap.")]
        public string shieldPattern = "XXX-XXX-";
        [UnityEngine.Tooltip("Shield spin in degrees per second. Negative spins the other way.")]
        public float shieldSpeed = 35f;
        public int beamCount = 2;
        [UnityEngine.Tooltip("Beam sweep in degrees per second.")]
        public float beamSweepSpeed = 22f;
        [UnityEngine.Tooltip("Seconds between orb rings. 0 = no orbs.")]
        public float volleyInterval = 0f;
        public int volleyCount = 8;
        public float volleySpeed = 4.5f;
        [UnityEngine.Tooltip("Seconds between minion spawns. 0 = none.")]
        public float minionInterval = 7f;
        public string[] minions = { EnemyIds.Drifter, EnemyIds.Drifter, EnemyIds.Drifter };

        public PrismPhase(string shieldPattern, float shieldSpeed, int beamCount, float beamSweepSpeed,
            float volleyInterval, int volleyCount, float minionInterval, params string[] minions)
        {
            this.shieldPattern = shieldPattern;
            this.shieldSpeed = shieldSpeed;
            this.beamCount = beamCount;
            this.beamSweepSpeed = beamSweepSpeed;
            this.volleyInterval = volleyInterval;
            this.volleyCount = volleyCount;
            this.minionInterval = minionInterval;
            this.minions = minions;
        }
    }

}
