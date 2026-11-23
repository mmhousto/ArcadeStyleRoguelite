using NUnit.Framework;
using Com.MorganHouston.Core;

// Checks slider values convert to mixer decibels and back.
public class AudioMathTests
{
    [Test]
    public void FullVolumeIsZeroDecibels()
    {
        Assert.AreEqual(0f, AudioMath.LinearToDecibels(1f), 0.001f);
    }

    [Test]
    public void ZeroVolumeIsSilent()
    {
        Assert.AreEqual(AudioMath.MinDecibels, AudioMath.LinearToDecibels(0f));
        Assert.AreEqual(0f, AudioMath.DecibelsToLinear(AudioMath.MinDecibels));
    }

    [Test]
    public void HalfVolumeIsAboutMinusSixDecibels()
    {
        Assert.AreEqual(-6.02f, AudioMath.LinearToDecibels(0.5f), 0.01f);
    }

    [Test]
    public void OutOfRangeValuesAreClamped()
    {
        Assert.AreEqual(0f, AudioMath.LinearToDecibels(2f), 0.001f);
        Assert.AreEqual(AudioMath.MinDecibels, AudioMath.LinearToDecibels(-1f));
    }

    [Test]
    public void ConversionRoundTrips()
    {
        foreach (float volume in new[] { 0.1f, 0.25f, 0.5f, 0.75f, 1f })
            Assert.AreEqual(volume, AudioMath.DecibelsToLinear(AudioMath.LinearToDecibels(volume)), 0.001f);
    }
}
