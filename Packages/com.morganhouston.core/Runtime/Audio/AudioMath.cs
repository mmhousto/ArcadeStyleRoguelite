using UnityEngine;

namespace Com.MMsGamingCo.Core
{

    /// <summary>
    /// Converts between slider values (0-1) and AudioMixer decibels.
    /// </summary>
    public static class AudioMath
    {
        /// <summary>The quietest value an AudioMixer group accepts. Treated as silent.</summary>
        public const float MinDecibels = -80f;

        /// <summary>Linear 0-1 volume to decibels. 1 = 0 dB, 0 = -80 dB.</summary>
        public static float LinearToDecibels(float linear)
        {
            linear = Mathf.Clamp01(linear);
            if (linear <= 0.0001f) return MinDecibels;
            return Mathf.Max(MinDecibels, Mathf.Log10(linear) * 20f);
        }

        /// <summary>Decibels back to a linear 0-1 volume.</summary>
        public static float DecibelsToLinear(float decibels)
        {
            if (decibels <= MinDecibels) return 0f;
            return Mathf.Clamp01(Mathf.Pow(10f, decibels / 20f));
        }
    }

}
