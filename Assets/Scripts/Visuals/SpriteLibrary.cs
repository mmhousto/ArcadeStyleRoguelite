using System.Collections.Generic;
using UnityEngine;

namespace Com.MMsGamingCo.Ricomet
{

    /// <summary>
    /// Loads the generated neon art from Resources/Art and turns it into sprites.
    /// Sprites are built at runtime from the textures, so the PNGs work with Unity's
    /// default import settings. Regenerate the art with Tools/generate_art.py.
    /// </summary>
    public static class SpriteLibrary
    {
        public const string ArtPath = "Art/";

        /// <summary>Radius (in pixels) that gameplay shapes are drawn to in the 256px art. Matches generate_art.py.</summary>
        public const float ShapeRadiusPixels = 88f;

        /// <summary>Corner size (in pixels) of the 96px UI frame and fill sprites, for nine-slicing.</summary>
        public const float UiBorderPixels = 34f;

        private static readonly Dictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>();
        private static readonly Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();

        /// <summary>Texture for an art name (e.g. "drifter"), or null if it isn't there.</summary>
        public static Texture2D Texture(string artName)
        {
            if (string.IsNullOrEmpty(artName)) return null;
            if (!textures.TryGetValue(artName, out Texture2D texture))
            {
                texture = Resources.Load<Texture2D>(ArtPath + artName);
                if (texture != null)
                {
                    texture.wrapMode = TextureWrapMode.Clamp;
                    texture.filterMode = FilterMode.Bilinear;
                }
                textures[artName] = texture;
            }
            return texture;
        }

        /// <summary>
        /// A gameplay sprite sized so its shape spans radius world units from the center.
        /// Returns null if the art is missing.
        /// </summary>
        public static Sprite Shape(string artName, float radius)
        {
            Texture2D texture = Texture(artName);
            if (texture == null || radius <= 0f) return null;

            float pixelsPerUnit = ShapeRadiusPixels * (texture.width / 256f) / radius;
            string key = artName + "@" + pixelsPerUnit.ToString("0.###");
            if (!sprites.TryGetValue(key, out Sprite sprite))
            {
                sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), pixelsPerUnit, 0, SpriteMeshType.FullRect);
                sprite.name = key;
                sprites[key] = sprite;
            }
            return sprite;
        }

        /// <summary>A UI sprite. Frames and fills are nine-sliced so they stretch cleanly.</summary>
        public static Sprite UI(string artName)
        {
            Texture2D texture = Texture(artName);
            if (texture == null) return null;

            string key = "ui:" + artName;
            if (!sprites.TryGetValue(key, out Sprite sprite))
            {
                bool sliced = artName.StartsWith("ui_");
                Vector4 border = sliced ? Vector4.one * UiBorderPixels * (texture.width / 96f) : Vector4.zero;
                sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, border);
                sprite.name = key;
                sprites[key] = sprite;
            }
            return sprite;
        }

        /// <summary>Icon for an upgrade ID ("icon_" + id), or null.</summary>
        public static Sprite UpgradeIcon(string upgradeId)
        {
            return UI("icon_" + upgradeId);
        }
    }

}
