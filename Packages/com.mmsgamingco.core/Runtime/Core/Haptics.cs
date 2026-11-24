using UnityEngine;

namespace Com.MMsGamingCo.Core
{

    public enum HapticStrength { Light, Medium, Heavy }

    /// <summary>
    /// Vibration that respects the player's Haptics setting.
    /// Handheld.Vibrate is one strong buzz on iOS. For light taps and impacts, a game
    /// registers a native plugin as impactHandler and calls Impact(); the setting still applies.
    /// </summary>
    public static class Haptics
    {
        /// <summary>Native impact feedback (e.g. iOS UIImpactFeedbackGenerator). Null = fall back to Vibrate for Heavy.</summary>
        public static System.Action<HapticStrength> impactHandler;

        /// <summary>A tap of the given strength, if haptics are on.</summary>
        public static void Impact(HapticStrength strength)
        {
            if (!Enabled) return;

            if (impactHandler != null)
                impactHandler(strength);
            else if (strength == HapticStrength.Heavy)
                Vibrate();
        }

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
