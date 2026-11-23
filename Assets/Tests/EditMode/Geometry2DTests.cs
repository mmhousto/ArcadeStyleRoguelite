using NUnit.Framework;
using UnityEngine;
using Com.MMsGamingCo.Ricomet;

// Checks dash hit detection and wall movement math.
public class Geometry2DTests
{
    [Test]
    public void FastDashStillHitsEnemyBetweenFrames()
    {
        // The enemy sits in the middle of one long frame step.
        Assert.IsTrue(Geometry2D.SweptCircleHit(new Vector2(0, 0), new Vector2(10, 0), 0.4f, new Vector2(5, 0.5f), 0.5f));
    }

    [Test]
    public void DashMissesEnemyOffToTheSide()
    {
        Assert.IsFalse(Geometry2D.SweptCircleHit(new Vector2(0, 0), new Vector2(10, 0), 0.4f, new Vector2(5, 2f), 0.5f));
    }

    [Test]
    public void DashDoesNotHitEnemyBehindStart()
    {
        Assert.IsFalse(Geometry2D.SweptCircleHit(new Vector2(0, 0), new Vector2(10, 0), 0.4f, new Vector2(-2f, 0f), 0.5f));
    }

    [Test]
    public void ZeroLengthSweepActsLikeOverlap()
    {
        Assert.IsTrue(Geometry2D.SweptCircleHit(Vector2.zero, Vector2.zero, 0.5f, new Vector2(0.8f, 0f), 0.5f));
    }

    [Test]
    public void MoveStopsAtWallAndReportsNormal()
    {
        var bounds = new Rect(-5, -5, 10, 10);
        bool hit = Geometry2D.MoveInsideBounds(Vector2.zero, new Vector2(20, 0), bounds, 0.5f, out Vector2 end, out Vector2 normal);

        Assert.IsTrue(hit);
        Assert.AreEqual(4.5f, end.x, 0.001f);
        Assert.AreEqual(Vector2.left, normal);
    }

    [Test]
    public void MoveInsideBoundsWithoutWall()
    {
        var bounds = new Rect(-5, -5, 10, 10);
        bool hit = Geometry2D.MoveInsideBounds(Vector2.zero, new Vector2(2, 1), bounds, 0.5f, out Vector2 end, out Vector2 normal);

        Assert.IsFalse(hit);
        Assert.AreEqual(new Vector2(2, 1), end);
    }

    [Test]
    public void DiagonalMoveStopsAtFirstWall()
    {
        var bounds = new Rect(-5, -5, 10, 10);
        Geometry2D.MoveInsideBounds(Vector2.zero, new Vector2(10, 5), bounds, 0f, out Vector2 end, out Vector2 normal);

        Assert.AreEqual(Vector2.left, normal);
        Assert.AreEqual(5f, end.x, 0.001f);
        Assert.AreEqual(2.5f, end.y, 0.001f);
    }
}
