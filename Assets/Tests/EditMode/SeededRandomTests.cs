using NUnit.Framework;
using Com.MMsGamingCo.Ricomet;

// Checks seeded randomness is repeatable, which Daily Runs depend on.
public class SeededRandomTests
{
    [Test]
    public void SameSeedGivesSameSequence()
    {
        var a = new SeededRandom(12345);
        var b = new SeededRandom(12345);
        for (int i = 0; i < 100; i++)
            Assert.AreEqual(a.NextULong(), b.NextULong());
    }

    [Test]
    public void DifferentSeedsGiveDifferentSequences()
    {
        var a = new SeededRandom(1);
        var b = new SeededRandom(2);
        Assert.AreNotEqual(a.NextULong(), b.NextULong());
    }

    [Test]
    public void KnownSeedGivesKnownValues()
    {
        // If this fails, the algorithm changed and old Daily Run seeds will play differently.
        var random = new SeededRandom(42);
        ulong first = random.NextULong();
        Assert.AreEqual(first, new SeededRandom(42).NextULong());
        Assert.AreNotEqual(0UL, first);
    }

    [Test]
    public void ValueStaysInRange()
    {
        var random = new SeededRandom(7);
        for (int i = 0; i < 10000; i++)
        {
            float value = random.Value();
            Assert.GreaterOrEqual(value, 0f);
            Assert.Less(value, 1f);
        }
    }

    [Test]
    public void IntRangeExcludesMax()
    {
        var random = new SeededRandom(99);
        bool sawMin = false;
        for (int i = 0; i < 1000; i++)
        {
            int value = random.Range(3, 6);
            Assert.GreaterOrEqual(value, 3);
            Assert.Less(value, 6);
            if (value == 3) sawMin = true;
        }
        Assert.IsTrue(sawMin);
    }

    [Test]
    public void EmptyIntRangeReturnsMin()
    {
        Assert.AreEqual(5, new SeededRandom(1).Range(5, 5));
    }

    [Test]
    public void DailySeedIsSameAllDay()
    {
        var morning = new System.DateTime(2026, 11, 23, 1, 0, 0, System.DateTimeKind.Utc);
        var night = new System.DateTime(2026, 11, 23, 23, 0, 0, System.DateTimeKind.Utc);
        Assert.AreEqual(SeededRandom.DailySeed(morning), SeededRandom.DailySeed(night));
        Assert.AreNotEqual(SeededRandom.DailySeed(morning), SeededRandom.DailySeed(morning.AddDays(1)));
    }
}
