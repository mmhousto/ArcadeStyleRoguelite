using NUnit.Framework;
using UnityEngine;
using Com.MMsGamingCo.Ricomet;

// Checks the boss shield ricochet math, phase thresholds and boss damage rules.
public class BossTests
{
    [Test]
    public void SegmentCrossingCircleIsFound()
    {
        Assert.IsTrue(Geometry2D.SegmentCrossesCircle(new Vector2(-10f, 0f), new Vector2(0f, 0f), Vector2.zero, 2f, out float t));
        Assert.AreEqual(0.8f, t, 0.001f);
    }

    [Test]
    public void SegmentInsideCircleDoesNotCross()
    {
        Assert.IsFalse(Geometry2D.SegmentCrossesCircle(new Vector2(-1f, 0f), new Vector2(1f, 0f), Vector2.zero, 2f, out _));
    }

    [Test]
    public void SegmentLeavingCircleCrossesOnTheWayOut()
    {
        Assert.IsTrue(Geometry2D.SegmentCrossesCircle(Vector2.zero, new Vector2(4f, 0f), Vector2.zero, 2f, out float t));
        Assert.AreEqual(0.5f, t, 0.001f);
    }

    [Test]
    public void PatternReadsSolidsAndGaps()
    {
        CollectionAssert.AreEqual(new[] { true, true, false, true }, RotatingShield.Pattern("XX-X"));
    }

    [Test]
    public void SolidAndGapFollowRotation()
    {
        // Four 90-degree segments: solid, gap, solid, gap.
        var shield = new RotatingShield(3f, RotatingShield.Pattern("X-X-"));
        Assert.IsTrue(shield.IsSolidAt(45f));
        Assert.IsFalse(shield.IsSolidAt(135f));

        shield.Rotation = 90f;
        Assert.IsFalse(shield.IsSolidAt(45f));
        Assert.IsTrue(shield.IsSolidAt(135f));
        Assert.IsFalse(shield.IsSolidAt(225f));
        Assert.IsTrue(shield.IsSolidAt(315f));
    }

    [Test]
    public void DashIntoSolidSegmentBouncesOutward()
    {
        var shield = new RotatingShield(3f, RotatingShield.Pattern("XXXX"));
        bool hit = shield.TryDeflect(Vector2.zero, new Vector2(10f, 0f), new Vector2(0f, 0f), 0.5f, out Vector2 point, out Vector2 normal);

        Assert.IsTrue(hit);
        Assert.Greater(point.x, 3f);
        Assert.Greater(normal.x, 0.99f);
    }

    [Test]
    public void DashThroughGapIsNotDeflected()
    {
        // Gap from 0 to 90 degrees; dash in along 45 degrees.
        var shield = new RotatingShield(3f, RotatingShield.Pattern("-XXX"));
        Vector2 start = new Vector2(10f, 10f);
        Assert.IsFalse(shield.TryDeflect(Vector2.zero, start, Vector2.zero, 0.3f, out _, out _));
    }

    [Test]
    public void DashFromInsideAlwaysGetsOut()
    {
        // One-way shield: it never traps the comet inside with the boss.
        var shield = new RotatingShield(3f, RotatingShield.Pattern("XXXX"));
        Assert.IsFalse(shield.TryDeflect(Vector2.zero, Vector2.zero, new Vector2(10f, 0f), 0.5f, out _, out _));
        Assert.IsFalse(shield.TryDeflect(Vector2.zero, new Vector2(2.9f, 0f), new Vector2(10f, 0f), 0.5f, out _, out _));
    }

    [Test]
    public void DashIntoCoreRicochetsOff()
    {
        bool hit = Geometry2D.BounceOffCircle(Vector2.zero, 1.3f, new Vector2(-10f, 0f), new Vector2(0f, 0f), 0.45f, out Vector2 point, out Vector2 normal);

        Assert.IsTrue(hit);
        Assert.Less(point.x, -1.75f, "Stops outside the core, never inside it.");
        Assert.Less(normal.x, -0.99f);
    }

    [Test]
    public void GlancingDashMissesCore()
    {
        Assert.IsFalse(Geometry2D.BounceOffCircle(Vector2.zero, 1.3f, new Vector2(-10f, 3f), new Vector2(10f, 3f), 0.45f, out _, out _));
    }

    [Test]
    public void CometOverlappingCoreCanLeave()
    {
        Assert.IsFalse(Geometry2D.BounceOffCircle(Vector2.zero, 1.3f, new Vector2(0.5f, 0f), new Vector2(10f, 0f), 0.45f, out _, out _));
    }

    [Test]
    public void PhasesChangeAtThresholds()
    {
        float[] thresholds = { 0.66f, 0.33f };
        Assert.AreEqual(0, BossRules.PhaseFor(1f, thresholds));
        Assert.AreEqual(0, BossRules.PhaseFor(0.7f, thresholds));
        Assert.AreEqual(1, BossRules.PhaseFor(0.66f, thresholds));
        Assert.AreEqual(1, BossRules.PhaseFor(0.5f, thresholds));
        Assert.AreEqual(2, BossRules.PhaseFor(0.2f, thresholds));
    }

    [Test]
    public void DashIsTheBestWayToHurtABoss()
    {
        Assert.Greater(BossRules.DamageMultiplier(DamageSource.Dash), BossRules.DamageMultiplier(DamageSource.Shockwave));
        Assert.Greater(BossRules.DamageMultiplier(DamageSource.Shockwave), BossRules.DamageMultiplier(DamageSource.Trail));
        Assert.Greater(BossRules.DamageMultiplier(DamageSource.Trail), 0f);
    }
}
