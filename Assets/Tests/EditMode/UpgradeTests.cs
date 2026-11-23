using System.Collections.Generic;
using NUnit.Framework;
using Com.MMsGamingCo.Ricomet;

// Checks the upgrade catalog, offers, stacking and the stats upgrades produce.
public class UpgradeTests
{
    private static PrototypeTuning DefaultTuning()
    {
        return UnityEngine.ScriptableObject.CreateInstance<PrototypeTuning>();
    }

    [Test]
    public void UpgradeIdsAreUnique()
    {
        var seen = new HashSet<string>();
        foreach (UpgradeDefinition upgrade in UpgradeCatalog.All)
            Assert.IsTrue(seen.Add(upgrade.id), "Duplicate upgrade id: " + upgrade.id);
    }

    [Test]
    public void EveryUpgradeIsComplete()
    {
        foreach (UpgradeDefinition upgrade in UpgradeCatalog.All)
        {
            Assert.IsFalse(string.IsNullOrEmpty(upgrade.displayName), upgrade.id);
            Assert.IsFalse(string.IsNullOrEmpty(upgrade.description), upgrade.id);
            Assert.GreaterOrEqual(upgrade.maxStacks, 1, upgrade.id);
            Assert.IsNotNull(upgrade.apply, upgrade.id);
        }
    }

    [Test]
    public void OfferHasNoDuplicates()
    {
        for (int seed = 0; seed < 50; seed++)
        {
            List<UpgradeDefinition> offer = UpgradeCatalog.RollOffer(new SeededRandom(seed), new UpgradeInventory(), 3);
            Assert.AreEqual(3, offer.Count);
            Assert.AreEqual(3, new HashSet<UpgradeDefinition>(offer).Count);
        }
    }

    [Test]
    public void SameSeedGivesSameOffer()
    {
        List<UpgradeDefinition> a = UpgradeCatalog.RollOffer(new SeededRandom(77), new UpgradeInventory(), 3);
        List<UpgradeDefinition> b = UpgradeCatalog.RollOffer(new SeededRandom(77), new UpgradeInventory(), 3);
        CollectionAssert.AreEqual(a, b);
    }

    [Test]
    public void MaxedUpgradesAreNotOffered()
    {
        var inventory = new UpgradeInventory();
        UpgradeDefinition reserve = UpgradeCatalog.Get("energy_capacity");
        for (int i = 0; i < reserve.maxStacks; i++)
            Assert.IsTrue(inventory.Add(reserve.id));
        Assert.IsFalse(inventory.Add(reserve.id));

        for (int seed = 0; seed < 100; seed++)
            CollectionAssert.DoesNotContain(UpgradeCatalog.RollOffer(new SeededRandom(seed), inventory, 3), reserve);
    }

    [Test]
    public void OfferIsEmptyWhenEverythingIsMaxed()
    {
        var inventory = new UpgradeInventory();
        foreach (UpgradeDefinition upgrade in UpgradeCatalog.All)
            for (int i = 0; i < upgrade.maxStacks; i++)
                inventory.Add(upgrade.id);

        Assert.AreEqual(0, UpgradeCatalog.RollOffer(new SeededRandom(1), inventory, 3).Count);
    }

    [Test]
    public void UnknownUpgradeIsRejected()
    {
        Assert.IsFalse(new UpgradeInventory().Add("not_a_real_upgrade"));
    }

    [Test]
    public void InventoryKeepsTakeOrder()
    {
        var inventory = new UpgradeInventory();
        inventory.Add("trail_fire");
        inventory.Add("bounce_wall");
        inventory.Add("trail_fire");

        CollectionAssert.AreEqual(new[] { "trail_fire", "bounce_wall" }, inventory.OwnedIds);
        Assert.AreEqual(2, inventory.GetStacks("trail_fire"));
        Assert.AreEqual(3, inventory.TotalStacks());
    }

    [Test]
    public void StatsWithoutUpgradesMatchTuning()
    {
        PrototypeTuning tuning = DefaultTuning();
        CometStats stats = CometStats.Build(tuning, new UpgradeInventory());

        Assert.AreEqual(tuning.maxCharges, stats.maxCharges);
        Assert.AreEqual(tuning.maxShields, stats.maxShields);
        Assert.AreEqual(1, stats.wallBounces);
        Assert.AreEqual(0f, stats.shockwaveRadius);
        Assert.AreEqual(0, stats.chainTargets);
    }

    [Test]
    public void StackingUpgradesAddUp()
    {
        PrototypeTuning tuning = DefaultTuning();
        var inventory = new UpgradeInventory();
        inventory.Add("bounce_wall");
        inventory.Add("bounce_wall");
        inventory.Add("energy_capacity");
        inventory.Add("chain_lightning");
        inventory.Add("def_shield");

        CometStats stats = CometStats.Build(tuning, inventory);
        // Wall bounce is standard, so two Rebounds stack on top of the base bounce.
        Assert.AreEqual(tuning.maxWallBounces + 2, stats.wallBounces);
        Assert.AreEqual(tuning.maxCharges + 1f, stats.maxCharges);
        Assert.AreEqual(1, stats.chainTargets);
        Assert.AreEqual(tuning.maxShields + 1, stats.maxShields);
    }

    [Test]
    public void ShockwaveGrowsWithStacks()
    {
        PrototypeTuning tuning = DefaultTuning();
        var inventory = new UpgradeInventory();
        inventory.Add("impact_shockwave");
        float one = CometStats.Build(tuning, inventory).shockwaveRadius;
        inventory.Add("impact_shockwave");
        float two = CometStats.Build(tuning, inventory).shockwaveRadius;

        Assert.Greater(one, 0f);
        Assert.Greater(two, one);
    }
}
