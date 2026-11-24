using UnityEngine;

namespace Com.MMsGamingCo.Ricomet
{

    /// <summary>
    /// Fonts and colors for the UI. Orbitron (SIL Open Font License, see
    /// Resources/Fonts/Orbitron-OFL.txt) gives the neon arcade look.
    /// </summary>
    public static class UITheme
    {
        public const string BoldFontPath = "Fonts/Orbitron-Bold";
        public const string MediumFontPath = "Fonts/Orbitron-Medium";

        public static readonly Color Cyan = new Color(0f, 0.94f, 1f);
        public static readonly Color Magenta = new Color(1f, 0.18f, 0.6f);
        public static readonly Color Orange = new Color(1f, 0.42f, 0f);
        public static readonly Color Yellow = new Color(1f, 0.9f, 0f);
        public static readonly Color White = Color.white;
        public static readonly Color Dim = new Color(1f, 1f, 1f, 0.55f);
        public static readonly Color PanelFill = new Color(0.03f, 0.03f, 0.09f, 0.88f);
        public static readonly Color Backdrop = new Color(0.01f, 0.01f, 0.04f, 0.72f);

        private static Font boldFont;
        private static Font mediumFont;

        public static Font Bold
        {
            get
            {
                if (boldFont == null) boldFont = LoadFont(BoldFontPath);
                return boldFont;
            }
        }

        public static Font Medium
        {
            get
            {
                if (mediumFont == null) mediumFont = LoadFont(MediumFontPath);
                return mediumFont;
            }
        }

        public static Color RarityColor(UpgradeRarity rarity)
        {
            switch (rarity)
            {
                case UpgradeRarity.Rare: return new Color(0.2f, 0.7f, 1f);
                case UpgradeRarity.Epic: return new Color(0.75f, 0.35f, 1f);
                case UpgradeRarity.Legendary: return new Color(1f, 0.8f, 0.1f);
                default: return new Color(0.85f, 0.85f, 0.9f);
            }
        }

        private static Font LoadFont(string path)
        {
            Font font = Resources.Load<Font>(path);
            // Fall back to Unity's built-in font so text still shows if the file is missing.
            return font != null ? font : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }
    }

}
