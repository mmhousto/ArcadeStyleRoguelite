using UnityEngine;

namespace Com.MorganHouston.Core
{

    /// <summary>
    /// Vibration that respects the player's Haptics setting.
    /// Handheld.Vibrate is one strong buzz on iOS; for light taps and impacts use a
    /// native haptics plugin and route it through here so the setting still applies.
    /// </summary>
    public static class Haptics
    {
        public static bool Enabled
        {
            get { return PreferencesManager.GetHaptics(); }
            set { PreferencesManager.SetHaptics(value); }
        }

        public static void Vibrate()
        {
            if (!Enabled) return;

#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
            Handheld.Vibrate();
#endif
        }
    }

}
