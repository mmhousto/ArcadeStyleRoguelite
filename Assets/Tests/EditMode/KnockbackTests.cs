using NUnit.Framework;
using UnityEngine;
using Com.MMsGamingCo.Ricomet;

// Checks knockback direction, falloff and decay.
public class KnockbackTests
{
    [Test]
    public void PushesAwayFromSource()
    {
        Vector2 velocity = Knockback.Velocity(Vector2.zero, new Vector2(1f, 0f), 3f, 10f);
        Assert.Greater(velocity.x, 0f);
        Assert.AreEqual(0f, velocity.y, 0.0001f);
    }

    [Test]
    public void CloserTargetsArePushedHarder()
    {
        float near = Knockback.Velocity(Vector2.zero, new Vector2(0.5f, 0f), 3f, 10f).magnitude;
        float far = Knockback.Velocity(Vector2.zero, new Vector2(2.5f, 0f), 3f, 10f).magnitude;
        Assert.Greater(near, far);
    }

    [Test]
    public void EdgeOfRadiusStillGetsHalfPush()
    {
        Assert.AreEqual(5f, Knockback.Velocity(Vector2.zero, new Vector2(3f, 0f), 3f, 10f).magnitude, 0.001f);
    }

    [Test]
    public void OutsideRadiusIsNotPushed()
    {
        Assert.AreEqual(Vector2.zero, Knockback.Velocity(Vector2.zero, new Vector2(3.5f, 0f), 3f, 10f));
    }

    [Test]
    public void TargetOnTopOfSourceStillMoves()
    {
        Assert.Greater(Knockback.Velocity(Vector2.zero, Vector2.zero, 3f, 10f).magnitude, 0f);
    }

    [Test]
    public void DecaySlowsDown()
    {
        Vector2 start = new Vector2(10f, 0f);
        Vector2 later = Knockback.Decay(start, 7f, 0.1f);
        Assert.Less(later.magnitude, start.magnitude);
        Assert.Greater(later.magnitude, 0f);
    }
}
