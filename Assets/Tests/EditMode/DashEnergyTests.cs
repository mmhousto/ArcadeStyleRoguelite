using NUnit.Framework;
using Com.MMsGamingCo.Ricomet;

// Checks dash charges, kill refunds, regen and the focus meter.
public class DashEnergyTests
{
    [Test]
    public void StartsFull()
    {
        var energy = new DashEnergy(3f, 0.4f);
        Assert.AreEqual(3, energy.ReadyCharges);
        Assert.IsTrue(energy.CanDash);
    }

    [Test]
    public void CannotDashWithoutAFullCharge()
    {
        var energy = new DashEnergy(3f, 0f);
        Assert.IsTrue(energy.Spend());
        Assert.IsTrue(energy.Spend());
        Assert.IsTrue(energy.Spend());
        Assert.IsFalse(energy.CanDash);
        Assert.IsFalse(energy.Spend());

        energy.Refund(0.9f);
        Assert.IsFalse(energy.CanDash);
        energy.Refund(0.1f);
        Assert.IsTrue(energy.CanDash);
    }

    [Test]
    public void ThreeKillsRefundAboutOneDash()
    {
        var energy = new DashEnergy(3f, 0f);
        energy.Spend();
        for (int i = 0; i < 3; i++)
            energy.Refund(0.35f);
        Assert.AreEqual(3, energy.ReadyCharges);
    }

    [Test]
    public void RefundNeverGoesOverMax()
    {
        var energy = new DashEnergy(3f, 0f);
        energy.Refund(10f);
        Assert.AreEqual(3f, energy.Current);
    }

    [Test]
    public void RegenRestoresOverTime()
    {
        var energy = new DashEnergy(3f, 0.4f);
        energy.Spend();
        energy.Regen(2.5f);
        Assert.AreEqual(3f, energy.Current, 0.001f);
    }

    [Test]
    public void StrandedRechargeIsFaster()
    {
        var energy = new DashEnergy(3f, 0.5f) { EmptyRegenMultiplier = 2f };
        energy.Spend(); energy.Spend(); energy.Spend();

        // Below one charge: double speed, so one charge is back in 1 second instead of 2.
        energy.Regen(1f);
        Assert.AreEqual(1f, energy.Current, 0.001f);

        // Above one charge: normal speed again.
        energy.Regen(1f);
        Assert.AreEqual(1.5f, energy.Current, 0.001f);
    }

    [Test]
    public void FocusDrainsAndRefills()
    {
        var focus = new FocusMeter(2.5f, 1f);
        focus.Drain(3f);
        Assert.IsFalse(focus.HasFocus);
        Assert.AreEqual(0f, focus.Fraction);

        focus.Refill(1.25f);
        Assert.AreEqual(0.5f, focus.Fraction, 0.001f);

        focus.Add(10f);
        Assert.AreEqual(1f, focus.Fraction, 0.001f);
    }
}
