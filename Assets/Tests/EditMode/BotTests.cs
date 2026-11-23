using System.Collections.Generic;
using NUnit.Framework;
using Com.MMsGamingCo.Ricomet;

// Checks the playtest bot's upgrade choices: always Rare or better when offered.
public class BotTests
{
    private static List<UpgradeDefinition> Offer(params string[] ids)
    {
        var offer = new List<UpgradeDefinition>();
        foreach (string id in ids) offer.Add(UpgradeCatalog.Get(id));
        return offer;
    }

    [Test]
    public void PicksRareOverCommon()
    {
        UpgradeDefinition choice = BotUpgradePicker.Choose(Offer("energy_capacity", "chain_lightning", "energy_refund"));
        Assert.AreEqual("chain_lightning", choice.id);
    }

    [Test]
    public void PicksRarestWhenSeveralAreRare()
    {
        UpgradeDefinition choice = BotUpgradePicker.Choose(Offer("def_shield", "split_shatter", "trail_fire"));
        Assert.AreEqual("split_shatter", choice.id);
        Assert.AreEqual(UpgradeRarity.Epic, choice.rarity);
    }

    [Test]
    public void UsesPreferenceWhenAllAreCommon()
    {
        UpgradeDefinition choice = BotUpgradePicker.Choose(Offer("energy_focus", "energy_capacity", "bounce_wall"));
        Assert.AreEqual("energy_capacity", choice.id);
    }

    [Test]
    public void AlwaysPicksRareIfThereIsOne()
    {
        for (int seed = 0; seed < 200; seed++)
        {
            List<UpgradeDefinition> offer = UpgradeCatalog.RollOffer(new SeededRandom(seed), new UpgradeInventory(), 3);
            bool hasRare = offer.Exists(upgrade => upgrade.rarity >= UpgradeRarity.Rare);
            UpgradeDefinition choice = BotUpgradePicker.Choose(offer);

            CollectionAssert.Contains(offer, choice);
            if (hasRare) Assert.GreaterOrEqual(choice.rarity, UpgradeRarity.Rare, "Seed " + seed);
        }
    }

    [Test]
    public void EmptyOfferPicksNothing()
    {
        Assert.IsNull(BotUpgradePicker.Choose(new List<UpgradeDefinition>()));
    }
}
