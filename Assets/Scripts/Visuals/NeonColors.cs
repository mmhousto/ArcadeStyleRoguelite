using UnityEngine;

namespace Com.MMsGamingCo.Ricomet
{

    /// <summary>
    /// Gameplay color rules from the design doc. Player is cyan, enemies magenta,
    /// danger orange, pickups yellow. Arenas may tint the background but never use these.
    /// Values above 1 are HDR so they glow once Bloom is on.
    /// </summary>
    public static class NeonColors
    {
        public static readonly Color Background = new Color(0.027f, 0.027f, 0.059f);
        public static readonly Color Player = new Color(0f, 0.94f, 1f) * 1.6f;
        public static readonly Color Enemy = new Color(1f, 0.18f, 0.6f) * 1.5f;
        public static readonly Color Danger = new Color(1f, 0.42f, 0f) * 1.6f;
        public static readonly Color Pickup = new Color(1f, 0.9f, 0f) * 1.5f;
        public static readonly Color ArenaLine = new Color(0.35f, 0.25f, 0.9f) * 1.2f;
        public static readonly Color Grid = new Color(0.25f, 0.2f, 0.6f, 0.18f);
        public static readonly Color Disabled = new Color(0.5f, 0.5f, 0.6f, 0.6f);
        public static readonly Color White = Color.white * 1.5f;

        /// <summary>Same color with a new alpha.</summary>
        public static Color WithAlpha(Color color, float alpha)
        {
            color.a = alpha;
            return color;
        }
    }

}
