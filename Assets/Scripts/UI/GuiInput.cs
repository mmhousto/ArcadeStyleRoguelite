using UnityEngine;
using UnityEngine.InputSystem;

namespace Com.MMsGamingCo.Ricomet
{

    /// <summary>
    /// Helpers for the gray-box IMGUI screens: a 1080p layout scale, and taps
    /// tested against GUI rects. Input is read in Update, not OnGUI, because
    /// OnGUI runs several times a frame.
    /// </summary>
    public static class GuiInput
    {
        public const float ReferenceHeight = 1080f;

        public static float Scale { get { return Screen.height / ReferenceHeight; } }

        /// <summary>Screen width in GUI units.</summary>
        public static float Width { get { return Screen.width / Scale; } }

        /// <summary>Pointer position in GUI units (y down), or a far-off point if there's no pointer.</summary>
        public static Vector2 PointerPosition
        {
            get
            {
                if (Pointer.current == null) return new Vector2(-9999f, -9999f);
                Vector2 screen = Pointer.current.position.ReadValue();
                return new Vector2(screen.x / Scale, (Screen.height - screen.y) / Scale);
            }
        }

        public static bool PointerPressed
        {
            get { return Pointer.current != null && Pointer.current.press.wasPressedThisFrame; }
        }

        public static bool PointerReleased
        {
            get { return Pointer.current != null && Pointer.current.press.wasReleasedThisFrame; }
        }

        /// <summary>True on the frame a press is released inside the rect.</summary>
        public static bool Tapped(Rect rect)
        {
            return PointerReleased && rect.Contains(PointerPosition);
        }

        /// <summary>Screen.safeArea in GUI units (IMGUI's y axis points down).</summary>
        public static Rect SafeRect()
        {
            float scale = Scale;
            Rect area = Screen.safeArea;
            return new Rect(area.x / scale, (Screen.height - area.yMax) / scale, area.width / scale, area.height / scale);
        }

        public static bool ConfirmPressed()
        {
            if (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame) return true;
            if (Keyboard.current != null && (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame)) return true;
            return false;
        }

        public static bool AltPressed()
        {
            if (Gamepad.current != null && (Gamepad.current.buttonWest.wasPressedThisFrame || Gamepad.current.buttonNorth.wasPressedThisFrame)) return true;
            if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame) return true;
            return false;
        }

        /// <summary>-1 / +1 when left or right is pressed this frame (gamepad d-pad or keyboard arrows).</summary>
        public static int HorizontalPressed()
        {
            if (Gamepad.current != null)
            {
                if (Gamepad.current.dpad.left.wasPressedThisFrame) return -1;
                if (Gamepad.current.dpad.right.wasPressedThisFrame) return 1;
            }
            if (Keyboard.current != null)
            {
                if (Keyboard.current.leftArrowKey.wasPressedThisFrame) return -1;
                if (Keyboard.current.rightArrowKey.wasPressedThisFrame) return 1;
            }
            return 0;
        }

        /// <summary>Draws a simple outlined button. Tap handling happens separately in Update.</summary>
        public static void DrawButton(Rect rect, string label, Color color, bool highlighted, GUIStyle style)
        {
            GUI.color = NeonColors.WithAlpha(color, highlighted ? 0.35f : 0.15f);
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            DrawOutline(rect, NeonColors.WithAlpha(color, 1f), highlighted ? 4f : 2f);
            GUI.color = Color.white;
            GUI.Label(rect, label, style);
        }

        public static void DrawOutline(Rect rect, Color color, float thickness)
        {
            GUI.color = color;
            GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width, thickness), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(rect.x, rect.yMax - thickness, rect.width, thickness), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(rect.x, rect.y, thickness, rect.height), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(rect.xMax - thickness, rect.y, thickness, rect.height), Texture2D.whiteTexture);
        }
    }

}
