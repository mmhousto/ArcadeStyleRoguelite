namespace Com.MMsGamingCo.Ricomet
{

    /// <summary>
    /// Shared boss rules. Dashes are the main way to hurt a boss; upgrade effects
    /// chip in, but can't melt it on their own.
    /// </summary>
    public static class BossRules
    {
        /// <summary>Damage multiplier for each source when it hits a boss.</summary>
        public static float DamageMultiplier(DamageSource source)
        {
            switch (source)
            {
                case DamageSource.Dash: return 2f;
                case DamageSource.Shockwave: return 1f;
                default: return 0.5f;
            }
        }

        /// <summary>
        /// Phase index for a health fraction. thresholds are descending, e.g. { 0.66, 0.33 }:
        /// above 0.66 is phase 0, above 0.33 phase 1, otherwise phase 2.
        /// </summary>
        public static int PhaseFor(float healthFraction, float[] thresholds)
        {
            int phase = 0;
            foreach (float threshold in thresholds)
            {
                if (healthFraction <= threshold) phase++;
            }
            return phase;
        }
    }

}
