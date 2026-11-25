using NUnit.Framework;
using UnityEngine;
using Com.MMsGamingCo.Ricomet;

// Checks the camera framing keeps the whole arena between the HUD bands on common screens.
public class HudLayoutTests
{
    private static readonly Vector2 Arena = new Vector2(30f, 16f);

    private static void AssertArenaClearOfBands(float aspect, float topInset, float bottomInset)
    {
        HudLayout.FitArena(Arena, 0.4f, aspect, topInset, bottomInset, out float size, out float cameraY);

        // Screen edges in world units, then the band edges.
        float screenTop = cameraY + size;
        float screenBottom = cameraY - size;
        float worldPerReference = size * 2f / HudLayout.ReferenceHeight;
        float bandTop = screenTop - (HudLayout.TopBand + topInset * HudLayout.ReferenceHeight) * worldPerReference;
        float bandBottom = screenBottom + (HudLayout.BottomBand + bottomInset * HudLayout.ReferenceHeight) * worldPerReference;

        Assert.LessOrEqual(Arena.y * 0.5f, bandTop + 0.001f, "Arena top runs under the top HUD band.");
        Assert.GreaterOrEqual(-Arena.y * 0.5f, bandBottom - 0.001f, "Arena bottom runs under the bottom HUD band.");
        Assert.LessOrEqual(Arena.x * 0.5f, size * aspect + 0.001f, "Arena is wider than the screen.");
    }

    [Test]
    public void FitsOnSixteenByNine()
    {
        AssertArenaClearOfBands(16f / 9f, 0f, 0f);
    }

    [Test]
    public void FitsOnModernIPhoneWithHomeIndicator()
    {
        // iPhone landscape: 19.5:9, ~21pt home indicator inset of 390pt height.
        AssertArenaClearOfBands(19.5f / 9f, 0f, 21f / 390f);
    }

    [Test]
    public void FitsOnIPad()
    {
        AssertArenaClearOfBands(4f / 3f, 0f, 0f);
    }
}
