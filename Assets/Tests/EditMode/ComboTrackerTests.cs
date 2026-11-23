using NUnit.Framework;
using Com.MMsGamingCo.Ricomet;

// Checks combo counting, timeouts, multipliers and milestones.
public class ComboTrackerTests
{
    [Test]
    public void KillsInsideWindowBuildCombo()
    {
        var combo = new ComboTracker(1.5f, 5f);
        combo.RegisterKill(0f);
        combo.RegisterKill(1f);
        combo.RegisterKill(2f);
        Assert.AreEqual(3, combo.Count);
    }

    [Test]
    public void ComboEndsAfterWindow()
    {
        var combo = new ComboTracker(1.5f, 5f);
        int endedAt = 0;
        combo.comboEnded += count => endedAt = count;

        combo.RegisterKill(0f);
        combo.RegisterKill(1f);
        combo.Tick(2.6f);

        Assert.AreEqual(0, combo.Count);
        Assert.AreEqual(2, endedAt);
        Assert.AreEqual(2, combo.Best);
    }

    [Test]
    public void LateKillStartsNewCombo()
    {
        var combo = new ComboTracker(1.5f, 5f);
        combo.RegisterKill(0f);
        combo.RegisterKill(5f);
        Assert.AreEqual(1, combo.Count);
    }

    [Test]
    public void MultiplierGrowsAndCaps()
    {
        var combo = new ComboTracker(100f, 5f);
        Assert.AreEqual(1f, combo.Multiplier);

        for (int i = 0; i < 20; i++) combo.RegisterKill(i);
        Assert.AreEqual(3f, combo.Multiplier, 0.001f);

        for (int i = 20; i < 100; i++) combo.RegisterKill(i);
        Assert.AreEqual(5f, combo.Multiplier, 0.001f);
    }

    [Test]
    public void MilestonesFireOnce()
    {
        var combo = new ComboTracker(100f, 5f);
        var reached = new System.Collections.Generic.List<int>();
        combo.milestoneReached += reached.Add;

        for (int i = 0; i < 30; i++) combo.RegisterKill(i);

        CollectionAssert.AreEqual(new[] { 10, 25 }, reached);
    }
}
