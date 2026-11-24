using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using Com.MMsGamingCo.Ricomet;

// Checks every upgrade has an icon, every sound name has a clip, and the UI font is present.
public class AssetTests
{
    [Test]
    public void EveryUpgradeHasAnIcon()
    {
        foreach (UpgradeDefinition upgrade in UpgradeCatalog.All)
            Assert.IsNotNull(Resources.Load<Texture2D>(SpriteLibrary.ArtPath + "icon_" + upgrade.id), "Missing icon for " + upgrade.id);
    }

    [Test]
    public void GameplayArtExists()
    {
        string[] art = { "comet", "drifter", "dart", "splitter", "splitter_cracked", "prism_outer", "prism_inner", "orb", "pip", "shard", "spark", "ui_frame", "ui_fill", "shield", "shield_full", "icon_pause" };
        foreach (string name in art)
            Assert.IsNotNull(Resources.Load<Texture2D>(SpriteLibrary.ArtPath + name), "Missing art: " + name);
    }

    [Test]
    public void EverySoundNameHasAClip()
    {
        foreach (FieldInfo field in typeof(GameAudio).GetFields(BindingFlags.Public | BindingFlags.Static))
        {
            if (!field.IsLiteral || field.Name.EndsWith("Path")) continue;

            string name = (string)field.GetValue(null);
            string folder = name.StartsWith("music_") ? GameAudio.MusicPath : GameAudio.SfxPath;
            Assert.IsNotNull(Resources.Load<AudioClip>(folder + name), "Missing sound: " + folder + name);
        }
    }

    [Test]
    public void UiFontExists()
    {
        Assert.IsNotNull(Resources.Load<Font>(UITheme.BoldFontPath));
        Assert.IsNotNull(Resources.Load<Font>(UITheme.MediumFontPath));
    }
}
