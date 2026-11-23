using System.Collections.Generic;

namespace Com.MMsGamingCo.Ricomet
{

    /// <summary>
    /// Every upgrade in the game, and the rules for rolling upgrade offers.
    /// Never rename or remove a shipped ID: add new upgrades at the end.
    /// </summary>
    public static class UpgradeCatalog
    {
        /// <summary>Relative chance of each rarity appearing in an offer.</summary>
        public static readonly Dictionary<UpgradeRarity, float> RarityWeights = new Dictionary<UpgradeRarity, float>
        {
            { UpgradeRarity.Common, 60f },
            { UpgradeRarity.Rare, 28f },
            { UpgradeRarity.Epic, 10f },
            { UpgradeRarity.Legendary, 2f },
        };

        public static readonly UpgradeDefinition[] All =
        {
            // Bounce
            new UpgradeDefinition("bounce_wall", "Rebound", "Bounce off walls. +1 bounce per stack.",
                UpgradeRarity.Common, 3, new[] { "bounce" },
                (stats, stacks) => stats.wallBounces += stacks),

            new UpgradeDefinition("bounce_enemy", "Seeker", "Kills redirect your dash at the nearest enemy.",
                UpgradeRarity.Rare, 3, new[] { "bounce" },
                (stats, stacks) => stats.killRedirects += stacks),

            // Impact
            new UpgradeDefinition("impact_shockwave", "Shockwave", "Landing blasts nearby enemies.",
                UpgradeRarity.Common, 3, new[] { "impact" },
                (stats, stacks) => stats.shockwaveRadius = 1.6f + 0.6f * stacks),

            // Trail
            new UpgradeDefinition("trail_fire", "Plasma Trail", "Your dash leaves a trail that destroys enemies.",
                UpgradeRarity.Rare, 3, new[] { "trail" },
                (stats, stacks) => stats.trailSeconds = 0.6f + 0.5f * stacks),

            // Chain
            new UpgradeDefinition("chain_lightning", "Arc", "Kills zap a nearby enemy. +1 target per stack.",
                UpgradeRarity.Rare, 3, new[] { "chain" },
                (stats, stacks) => stats.chainTargets += stacks),

            // Split
            new UpgradeDefinition("split_shatter", "Shatter", "Kills burst into fragments that hit other enemies.",
                UpgradeRarity.Epic, 2, new[] { "split" },
                (stats, stacks) => stats.shatterFragments += 1 + 2 * stacks),

            // Energy
            new UpgradeDefinition("energy_capacity", "Reserve", "+1 dash charge.",
                UpgradeRarity.Common, 2, new[] { "energy" },
                (stats, stacks) => stats.maxCharges += stacks),

            new UpgradeDefinition("energy_refund", "Siphon", "Kills refund more dash energy.",
                UpgradeRarity.Common, 3, new[] { "energy" },
                (stats, stacks) => stats.killRefund += 0.1f * stacks),

            new UpgradeDefinition("energy_focus", "Clarity", "+50% slow-motion focus. Recharge faster.",
                UpgradeRarity.Common, 2, new[] { "energy" },
                (stats, stacks) =>
                {
                    stats.focusSeconds *= 1f + 0.5f * stacks;
                    stats.secondsPerCharge *= 1f - 0.12f * stacks;
                }),

            new UpgradeDefinition("energy_rush", "Overdrive", "Dash 20% faster and 15% farther.",
                UpgradeRarity.Common, 3, new[] { "energy" },
                (stats, stacks) =>
                {
                    stats.dashSpeed *= 1f + 0.2f * stacks;
                    stats.dashDistance *= 1f + 0.15f * stacks;
                }),

            // Defense
            new UpgradeDefinition("def_shield", "Bulwark", "+1 max shield, and repair one.",
                UpgradeRarity.Rare, 2, new[] { "defense" },
                (stats, stacks) => stats.maxShields += stacks),

            new UpgradeDefinition("def_phase", "Phase", "Stay invulnerable for a moment after each dash.",
                UpgradeRarity.Rare, 2, new[] { "defense" },
                (stats, stacks) => stats.phaseSeconds = 0.3f + 0.3f * stacks),
        };

        private static Dictionary<string, UpgradeDefinition> byId;

        public static UpgradeDefinition Get(string id)
        {
            if (byId == null)
            {
                byId = new Dictionary<string, UpgradeDefinition>();
                foreach (UpgradeDefinition upgrade in All)
                    byId[upgrade.id] = upgrade;
            }

            if (string.IsNullOrEmpty(id)) return null;
            byId.TryGetValue(id, out UpgradeDefinition found);
            return found;
        }

        /// <summary>
        /// Picks up to count different upgrades the player can still take, weighted by rarity.
        /// Uses the run's SeededRandom so the same seed offers the same cards.
        /// </summary>
        public static List<UpgradeDefinition> RollOffer(SeededRandom random, UpgradeInventory inventory, int count)
        {
            var candidates = new List<UpgradeDefinition>();
            foreach (UpgradeDefinition upgrade in All)
            {
                if (inventory == null || inventory.CanTake(upgrade))
                    candidates.Add(upgrade);
            }

            var offer = new List<UpgradeDefinition>();
            while (offer.Count < count && candidates.Count > 0)
            {
                float total = 0f;
                foreach (UpgradeDefinition candidate in candidates)
                    total += RarityWeights[candidate.rarity];

                float roll = random.Range(0f, total);
                int picked = candidates.Count - 1;
                for (int i = 0; i < candidates.Count; i++)
                {
                    roll -= RarityWeights[candidates[i].rarity];
                    if (roll < 0f)
                    {
                        picked = i;
                        break;
                    }
                }

                offer.Add(candidates[picked]);
                candidates.RemoveAt(picked);
            }

            return offer;
        }
    }

}
