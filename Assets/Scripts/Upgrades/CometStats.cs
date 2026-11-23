namespace Com.MMsGamingCo.Ricomet
{

    /// <summary>
    /// The comet's numbers for the current run: tuning values plus every upgrade
    /// taken. Rebuilt whenever an upgrade is added.
    /// </summary>
    public class CometStats
    {
        // Dash
        public float dashDistance;
        public float dashSpeed;
        public float dashDamage;
        public int wallBounces;
        /// <summary>Times per dash a kill can redirect the comet at the next enemy.</summary>
        public int killRedirects;

        // Energy and focus
        public float maxCharges;
        public float killRefund;
        public float secondsPerCharge;
        public float focusSeconds;

        // Defense
        public int maxShields;
        /// <summary>Invulnerable time after a dash ends. 0 = only the normal grace period.</summary>
        public float phaseSeconds;

        // Effects. 0 means the effect is off.
        public float shockwaveRadius;
        public float trailSeconds;
        public int chainTargets;
        public float chainRange;
        public int shatterFragments;

        /// <summary>Base stats from tuning, before upgrades.</summary>
        public static CometStats FromTuning(PrototypeTuning tuning)
        {
            return new CometStats
            {
                dashDistance = tuning.dashDistance,
                dashSpeed = tuning.dashSpeed,
                dashDamage = tuning.dashDamage,
                wallBounces = tuning.bounceOffWalls ? tuning.maxWallBounces : 0,
                killRedirects = 0,
                maxCharges = tuning.maxCharges,
                killRefund = tuning.killRefund,
                secondsPerCharge = tuning.secondsPerCharge,
                focusSeconds = tuning.focusSeconds,
                maxShields = tuning.maxShields,
                phaseSeconds = 0f,
                shockwaveRadius = 0f,
                trailSeconds = 0f,
                chainTargets = 0,
                chainRange = 4.5f,
                shatterFragments = 0,
            };
        }

        /// <summary>Base stats with every owned upgrade applied.</summary>
        public static CometStats Build(PrototypeTuning tuning, UpgradeInventory inventory)
        {
            CometStats stats = FromTuning(tuning);
            if (inventory == null) return stats;

            foreach (string id in inventory.OwnedIds)
            {
                UpgradeDefinition upgrade = UpgradeCatalog.Get(id);
                if (upgrade != null)
                    upgrade.apply(stats, inventory.GetStacks(id));
            }
            return stats;
        }
    }

}
