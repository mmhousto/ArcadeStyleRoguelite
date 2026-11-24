using System.Runtime.InteropServices;
using Com.MMsGamingCo.Core;

namespace Com.MMsGamingCo.Ricomet
{

    /// <summary>
    /// Light, medium and heavy taps on iPhone through UIImpactFeedbackGenerator
    /// (Assets/Plugins/iOS/RicometHaptics.mm). Registered with the Core Haptics hook,
    /// so the player's Haptics setting still applies. Does nothing on other platforms.
    /// </summary>
    public static class IOSHaptics
    {
#if UNITY_IOS && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void _RicometHapticImpact(int style);
#endif

        /// <summary>Hooks iOS haptics into Haptics.Impact. Safe to call more than once.</summary>
        public static void Register()
        {
#if UNITY_IOS && !UNITY_EDITOR
            Haptics.impactHandler = strength => _RicometHapticImpact((int)strength);
#endif
        }
    }

}
