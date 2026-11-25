using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Com.MMsGamingCo.Ricomet
{

    /// <summary>
    /// Fonts and colors for the UI. Orbitron (SIL Open Font License, see
    /// Resources/Fonts/Orbitron-OFL.txt) as TextMeshPro font assets gives the neon arcade look.
    /// </summary>
    public static class UITheme
    {
        public const string BoldFontPath = "Fonts/Orbitron-Bold SDF";
        public const string MediumFontPath = "Fonts/Orbitron-Medium SDF";

        public static readonly Color Cyan = new Color(0f, 0.94f, 1f);
        public static readonly Color Magenta = new Color(1f, 0.18f, 0.6f);
        public static readonly Color Orange = new Color(1f, 0.42f, 0f);
        public static readonly Color Yellow = new Color(1f, 0.9f, 0f);
        public static readonly Color White = Color.white;
        public static readonly Color Dim = new Color(1f, 1f, 1f, 0.55f);
        public static readonly Color PanelFill = new Color(0.03f, 0.03f, 0.09f, 0.88f);
        public static readonly Color Backdrop = new Color(0.01f, 0.01f, 0.04f, 0.72f);

        private static TMP_FontAsset boldFont;
        private static TMP_FontAsset mediumFont;
        private static readonly Dictionary<string, Material> glowMaterials = new Dictionary<string, Material>();

        /// <summary>Orbitron Bold, or TextMeshPro's default font if the asset is missing.</summary>
        public static TMP_FontAsset BoldFont
        {
            get
            {
                if (boldFont == null) boldFont = LoadFont(BoldFontPath);
                return boldFont;
            }
        }

        public static TMP_FontAsset MediumFont
        {
            get
            {
                if (mediumFont == null) mediumFont = LoadFont(MediumFontPath);
                return mediumFont;
            }
        }

        /// <summary>
        /// A copy of the font's material with a soft glow in the text's color. Shared by every
        /// text with the same font and color, so it's one material per color, not per text.
        /// </summary>
        public static Material GlowMaterial(TMP_FontAsset font, Color color)
        {
            if (font == null || font.material == null) return null;

            string key = font.name + ColorUtility.ToHtmlStringRGB(color);
            if (!glowMaterials.TryGetValue(key, out Material material))
            {
                material = new Material(font.material) { name = font.name + " Glow " + key };
                if (material.HasProperty("_GlowColor"))
                {
                    material.EnableKeyword("GLOW_ON");
                    material.SetColor("_GlowColor", new Color(color.r, color.g, color.b, 0.55f));
                    material.SetFloat("_GlowOuter", 0.3f);
                    material.SetFloat("_GlowInner", 0.05f);
                    material.SetFloat("_GlowPower", 0.75f);
                }
                glowMaterials[key] = material;
            }
            return material;
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

        private static TMP_FontAsset LoadFont(string path)
        {
            TMP_FontAsset font = Resources.Load<TMP_FontAsset>(path);
            // Fall back to TextMeshPro's default font so text still shows if the asset is missing.
            return font != null ? font : TMP_Settings.defaultFontAsset;
        }
    }

}
