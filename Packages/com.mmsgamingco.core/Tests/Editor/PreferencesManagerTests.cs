using NUnit.Framework;
using UnityEngine;
using Com.MMsGamingCo.Core;

// Checks settings defaults and that values are clamped and stored.
// Runs against real PlayerPrefs, so every key touched here is restored afterwards.
public class PreferencesManagerTests
{
    private static readonly string[] Keys =
    {
        PrefKeys.MasterVolume, PrefKeys.MusicVolume, PrefKeys.Haptics, "MMsCoreTestBool"
    };

    private readonly System.Collections.Generic.Dictionary<string, string> saved =
        new System.Collections.Generic.Dictionary<string, string>();

    [SetUp]
    public void SetUp()
    {
        saved.Clear();
        foreach (string key in Keys)
        {
            if (PlayerPrefs.HasKey(key))
                saved[key] = PlayerPrefs.GetFloat(key, float.NaN) + "|" + PlayerPrefs.GetInt(key, int.MinValue);
            PlayerPrefs.DeleteKey(key);
        }
    }

    [TearDown]
    public void TearDown()
    {
        foreach (string key in Keys)
            PlayerPrefs.DeleteKey(key);

        foreach (var pair in saved)
        {
            string[] parts = pair.Value.Split('|');
            float floatValue = float.Parse(parts[0]);
            if (!float.IsNaN(floatValue)) PlayerPrefs.SetFloat(pair.Key, floatValue);
            else PlayerPrefs.SetInt(pair.Key, int.Parse(parts[1]));
        }
    }

    [Test]
    public void VolumesDefaultToFull()
    {
        Assert.AreEqual(1f, PreferencesManager.GetMasterVolume());
        Assert.AreEqual(1f, PreferencesManager.GetMusicVolume());
    }

    [Test]
    public void VolumeIsClamped()
    {
        PreferencesManager.SetMasterVolume(3f);
        Assert.AreEqual(1f, PreferencesManager.GetMasterVolume());

        PreferencesManager.SetMusicVolume(-1f);
        Assert.AreEqual(0f, PreferencesManager.GetMusicVolume());
    }

    [Test]
    public void HapticsDefaultsOnAndCanBeTurnedOff()
    {
        Assert.IsTrue(PreferencesManager.GetHaptics());
        PreferencesManager.SetHaptics(false);
        Assert.IsFalse(PreferencesManager.GetHaptics());
    }

    [Test]
    public void SettingAValueRaisesPreferenceChanged()
    {
        string changedKey = null;
        System.Action<string> handler = key => changedKey = key;

        PreferencesManager.preferenceChanged += handler;
        PreferencesManager.SetBool("MMsCoreTestBool", true);
        PreferencesManager.preferenceChanged -= handler;

        Assert.AreEqual("MMsCoreTestBool", changedKey);
        Assert.IsTrue(PreferencesManager.GetBool("MMsCoreTestBool"));
    }
}
