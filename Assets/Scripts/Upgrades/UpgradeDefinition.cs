using System;

namespace Com.MMsGamingCo.Ricomet
{

    /// <summary>
    /// One upgrade card. apply is given the total stacks owned, so stacking rules
    /// live in one place. Defined in UpgradeCatalog for now; these move to
    /// ScriptableObjects once they need icons and artist-tuned values.
    /// </summary>
    public class UpgradeDefinition
    {
        /// <summary>Stable ID. Saved in run history and analytics; never rename after release.</summary>
        public string id;
        public string displayName;
        /// <summary>Card text. Keep it to one short line.</summary>
        public string description;
        public UpgradeRarity rarity;
        public int maxStacks = 1;
        public string[] tags = new string[0];
        public Action<CometStats, int> apply;

        public UpgradeDefinition(string id, string displayName, string description, UpgradeRarity rarity, int maxStacks, string[] tags, Action<CometStats, int> apply)
        {
            this.id = id;
            this.displayName = displayName;
            this.description = description;
            this.rarity = rarity;
            this.maxStacks = maxStacks;
            this.tags = tags ?? new string[0];
            this.apply = apply;
        }
    }

}
