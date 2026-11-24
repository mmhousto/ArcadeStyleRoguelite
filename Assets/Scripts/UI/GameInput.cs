using UnityEngine.InputSystem;

namespace Com.MMsGamingCo.Ricomet
{

    /// <summary>
    /// Menu shortcuts that aren't covered by uGUI navigation: pause/back and number keys.
    /// </summary>
    public static class GameInput
    {
        /// <summary>Escape, gamepad Start, or Android back.</summary>
        public static bool PausePressed()
        {
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame) return true;
            if (Gamepad.current != null && Gamepad.current.startButton.wasPressedThisFrame) return true;
            return false;
        }

        /// <summary>Escape or gamepad B.</summary>
        public static bool BackPressed()
        {
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame) return true;
            if (Gamepad.current != null && Gamepad.current.buttonEast.wasPressedThisFrame) return true;
            return false;
        }

        /// <summary>1, 2 or 3 pressed this frame (0-based), or -1.</summary>
        public static int NumberPressed()
        {
            if (Keyboard.current == null) return -1;
            if (Keyboard.current.digit1Key.wasPressedThisFrame) return 0;
            if (Keyboard.current.digit2Key.wasPressedThisFrame) return 1;
            if (Keyboard.current.digit3Key.wasPressedThisFrame) return 2;
            return -1;
        }

        /// <summary>R or gamepad Y.</summary>
        public static bool RerollPressed()
        {
            if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame) return true;
            if (Gamepad.current != null && Gamepad.current.buttonNorth.wasPressedThisFrame) return true;
            return false;
        }
    }

}
