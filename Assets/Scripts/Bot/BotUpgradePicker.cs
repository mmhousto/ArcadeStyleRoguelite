using System.Collections.Generic;

namespace Com.MMsGamingCo.Ricomet
{

    /// <summary>
    /// How the playtest bot picks upgrades: always a Rare-or-better card if one is
    /// offered (the rarest first), otherwise by a fixed preference list.
    /// </summary>
    public static class BotUpgradePicker
    {
        /// <summary>Preference among same-rarity cards, best first.</summary>
        public static readonly string[] Preference =
        {
            "split_shatter", "chain_lightning", "trail_fire", "bounce_enemy", "def_shield", "def_phase",
            "energy_capacity", "energy_refund", "impact_shockwave", "bounce_wall", "energy_rush", "energy_focus",
        };

        public static UpgradeDefinition Choose(IReadOnlyList<UpgradeDefinition> offer)
        {
            if (offer == null || offer.Count == 0) return null;

            UpgradeDefinition best = null;
            foreach (UpgradeDefinition upgrade in offer)
            {
                if (best == null || IsBetter(upgrade, best)) best = upgrade;
            }
            return best;
        }

        private static bool IsBetter(UpgradeDefinition candidate, UpgradeDefinition current)
        {
            bool candidateRare = candidate.rarity >= UpgradeRarity.Rare;
            bool currentRare = current.rarity >= UpgradeRarity.Rare;

            if (candidateRare != currentRare) return candidateRare;
            if (candidateRare && candidate.rarity != current.rarity) return candidate.rarity > current.rarity;
            return Rank(candidate.id) < Rank(current.id);
        }

        private static int Rank(string id)
        {
            int index = System.Array.IndexOf(Preference, id);
            return index < 0 ? Preference.Length : index;
        }
    }

}
