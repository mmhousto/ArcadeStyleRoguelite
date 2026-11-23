using System;
using System.Collections.Generic;

namespace Com.MMsGamingCo.Ricomet
{

    /// <summary>
    /// Upgrades taken this run, with stack counts. Cleared when the run ends.
    /// </summary>
    public class UpgradeInventory
    {
        private readonly Dictionary<string, int> stacks = new Dictionary<string, int>();
        private readonly List<string> ownedIds = new List<string>();

        /// <summary>Owned upgrade IDs in the order they were first taken.</summary>
        public IReadOnlyList<string> OwnedIds { get { return ownedIds; } }

        /// <summary>Raised with the upgrade after it is added.</summary>
        public event Action<UpgradeDefinition> upgradeAdded;

        public int GetStacks(string id)
        {
            return stacks.TryGetValue(id, out int count) ? count : 0;
        }

        public bool CanTake(UpgradeDefinition upgrade)
        {
            return upgrade != null && GetStacks(upgrade.id) < upgrade.maxStacks;
        }

        /// <summary>Adds one stack. Returns false if the upgrade is unknown or maxed.</summary>
        public bool Add(string id)
        {
            UpgradeDefinition upgrade = UpgradeCatalog.Get(id);
            if (!CanTake(upgrade)) return false;

            if (!stacks.ContainsKey(id))
            {
                stacks[id] = 0;
                ownedIds.Add(id);
            }
            stacks[id]++;

            upgradeAdded?.Invoke(upgrade);
            return true;
        }

        public int TotalStacks()
        {
            int total = 0;
            foreach (int count in stacks.Values)
                total += count;
            return total;
        }
    }

}
