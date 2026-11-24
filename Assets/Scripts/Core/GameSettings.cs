using Com.MMsGamingCo.Core;

namespace Com.MMsGamingCo.Ricomet
{

    /// <summary>
    /// Ricomet's own player settings, stored with PreferencesManager. Volumes and
    /// haptics use the Core package's settings directly.
    /// </summary>
    public static class GameSettings
    {
        public const string ReduceShakeKey = "ReduceScreenShake";
        public const string ReduceFlashingKey = "ReduceFlashing";

        // Cached: these are read every frame, and PlayerPrefs reads aren't free.
        private static bool? reduceShake;
        private static bool? reduceFlashing;

        /// <summary>Screen shake at a quarter of its strength.</summary>
        public static bool ReduceShake
        {
            get
            {
                if (reduceShake == null) reduceShake = PreferencesManager.GetBool(ReduceShakeKey, false);
                return reduceShake.Value;
            }
            set
            {
                reduceShake = value;
                PreferencesManager.SetBool(ReduceShakeKey, value);
            }
        }

        /// <summary>No blinking or white hit flashes; steady dimming instead.</summary>
        public static bool ReduceFlashing
        {
            get
            {
                if (reduceFlashing == null) reduceFlashing = PreferencesManager.GetBool(ReduceFlashingKey, false);
                return reduceFlashing.Value;
            }
            set
            {
                reduceFlashing = value;
                PreferencesManager.SetBool(ReduceFlashingKey, value);
            }
        }

        public static float ShakeIntensity { get { return ReduceShake ? 0.25f : 1f; } }
    }

}
