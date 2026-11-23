using System.IO;
using NUnit.Framework;
using Com.MMsGamingCo.Core;

// Checks local saves round-trip, survive a corrupt file, and upgrade old formats.
public class SaveSystemTests
{
    [System.Serializable]
    private class TestSave : SaveDataBase
    {
        public int coins = 0;
        public string[] unlockedIds;
        public int legacyUnlockCount = 0;

        public override int CurrentSchemaVersion { get { return 2; } }

        public override void Normalize()
        {
            // Version 1 stored a count; version 2 stores IDs.
            // JsonUtility loads a missing array as empty, not null, so check both.
            if (schemaVersion < 2 && (unlockedIds == null || unlockedIds.Length == 0))
            {
                unlockedIds = new string[legacyUnlockCount];
                for (int i = 0; i < legacyUnlockCount; i++)
                    unlockedIds[i] = "item_" + i;
            }
            if (unlockedIds == null)
                unlockedIds = new string[0];

            base.Normalize();
        }
    }

    private string folder;

    [SetUp]
    public void SetUp()
    {
        folder = Path.Combine(Path.GetTempPath(), "MHCoreSaveTests_" + System.Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        SaveSystem.saveFolder = folder;
    }

    [TearDown]
    public void TearDown()
    {
        SaveSystem.saveFolder = null;
        if (Directory.Exists(folder))
            Directory.Delete(folder, true);
    }

    [Test]
    public void SaveThenLoadKeepsValues()
    {
        Assert.IsTrue(SaveSystem.Save(new TestSave { coins = 42, unlockedIds = new[] { "a" } }));

        TestSave loaded = SaveSystem.Load<TestSave>();
        Assert.AreEqual(42, loaded.coins);
        CollectionAssert.AreEqual(new[] { "a" }, loaded.unlockedIds);
        Assert.AreEqual(2, loaded.schemaVersion);
        Assert.Greater(loaded.lastSavedTicks, 0);
    }

    [Test]
    public void MissingSaveReturnsFreshNormalizedData()
    {
        Assert.IsFalse(SaveSystem.Exists());
        Assert.IsNull(SaveSystem.TryLoad<TestSave>());

        TestSave fresh = SaveSystem.Load<TestSave>();
        Assert.AreEqual(0, fresh.coins);
        Assert.IsNotNull(fresh.unlockedIds);
    }

    [Test]
    public void CorruptSaveFallsBackToBackup()
    {
        SaveSystem.Save(new TestSave { coins = 1 });
        SaveSystem.Save(new TestSave { coins = 2 });

        File.WriteAllText(SaveSystem.GetPath(), "{ this is not json");

        // The backup holds the save before the latest one.
        Assert.AreEqual(1, SaveSystem.Load<TestSave>().coins);
    }

    [Test]
    public void OldSchemaIsUpgradedOnLoad()
    {
        File.WriteAllText(SaveSystem.GetPath(), "{\"schemaVersion\":1,\"coins\":5,\"legacyUnlockCount\":2}");

        TestSave loaded = SaveSystem.Load<TestSave>();
        CollectionAssert.AreEqual(new[] { "item_0", "item_1" }, loaded.unlockedIds);
        Assert.AreEqual(2, loaded.schemaVersion);
    }

    [Test]
    public void DeleteRemovesSaveAndBackup()
    {
        SaveSystem.Save(new TestSave());
        SaveSystem.Save(new TestSave());
        SaveSystem.Delete();

        Assert.IsFalse(SaveSystem.Exists());
    }

    [Test]
    public void SlotsAreSeparate()
    {
        SaveSystem.Save(new TestSave { coins = 1 }, "slotA");
        SaveSystem.Save(new TestSave { coins = 2 }, "slotB");

        Assert.AreEqual(1, SaveSystem.Load<TestSave>("slotA").coins);
        Assert.AreEqual(2, SaveSystem.Load<TestSave>("slotB").coins);
    }

    [Test]
    public void NewestPicksLatestSave()
    {
        var older = new TestSave { lastSavedTicks = 100 };
        var newer = new TestSave { lastSavedTicks = 200 };

        Assert.AreSame(newer, SaveDataBase.Newest(older, newer));
        Assert.AreSame(newer, SaveDataBase.Newest(newer, older));
        Assert.AreSame(older, SaveDataBase.Newest(older, null));
        Assert.AreSame(newer, SaveDataBase.Newest<TestSave>(null, newer));
    }
}
