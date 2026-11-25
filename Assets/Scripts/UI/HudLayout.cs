using UnityEngine;

namespace Com.MMsGamingCo.Ricomet
{

    /// <summary>
    /// Screen bands kept clear of the arena for the HUD, in the UI's 1080-high reference
    /// space. The camera frames the arena between them, so shields, score and upgrade
    /// icons never sit on top of the action.
    /// </summary>
    public static class HudLayout
    {
        /// <summary>Top band: shields, score, combo, wave info, pause.</summary>
        public const float TopBand = 150f;

        /// <summary>Bottom band: owned upgrades and the boss bar.</summary>
        public const float BottomBand = 110f;

        public const float ReferenceHeight = 1080f;

        /// <summary>
        /// Orthographic size and vertical offset that fit the arena (plus a margin) between
        /// the bands on a screen of the given aspect ratio. The insets are the screen fractions
        /// outside the safe area (iPhone home indicator), since the HUD sits inside it.
        /// </summary>
        public static void FitArena(Vector2 arenaSize, float margin, float aspect, float topInset, float bottomInset, out float orthographicSize, out float cameraY)
        {
            float top = TopBand / ReferenceHeight + Mathf.Max(0f, topInset);
            float bottom = BottomBand / ReferenceHeight + Mathf.Max(0f, bottomInset);
            float available = 1f - top - bottom;

            float byHeight = (arenaSize.y * 0.5f + margin) / available;
            float byWidth = (arenaSize.x * 0.5f + margin) / Mathf.Max(0.1f, aspect);
            orthographicSize = Mathf.Max(byHeight, byWidth);

            // Moving the camera up moves the arena down, centering it in the space between the bands.
            cameraY = (top - bottom) * orthographicSize;
        }
    }

}
