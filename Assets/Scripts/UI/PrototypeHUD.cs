using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Com.MMsGamingCo.Ricomet
{

    /// <summary>
    /// Gray-box HUD drawn with IMGUI so the prototype needs no UI prefabs.
    /// Shows shields, score, combo, time, trick popups and the game over screen.
    /// Replaced by a real uGUI HUD in the vertical slice.
    /// </summary>
    public class PrototypeHUD : MonoBehaviour
    {
        private const float ReferenceHeight = 1080f;
        private const float RestartDelay = 0.8f;

        private class Popup
        {
            public string text;
            public float timer;
        }

        private readonly List<Popup> popups = new List<Popup>();
        private float gameOverTimer = 0f;
        private GUIStyle labelStyle, bigStyle, smallStyle, centerStyle;
        private Texture2D pixel;

        private void OnEnable()
        {
            RunManager.trickPerformed += ShowPopup;
        }

        private void OnDisable()
        {
            RunManager.trickPerformed -= ShowPopup;
        }

        private void Update()
        {
            for (int i = popups.Count - 1; i >= 0; i--)
            {
                popups[i].timer -= Time.unscaledDeltaTime;
                if (popups[i].timer <= 0f) popups.RemoveAt(i);
            }

            RunManager run = RunManager.Instance;
            if (run == null || !run.IsGameOver) return;

            gameOverTimer += Time.unscaledDeltaTime;
            if (gameOverTimer >= RestartDelay && RestartPressed())
                run.Restart();
        }

        private void OnApplicationPause(bool pause)
        {
            // iOS: pause when the app goes to the background.
            if (TimeController.Instance != null)
                TimeController.Instance.SetPaused(pause);
        }

        private void OnGUI()
        {
            RunManager run = RunManager.Instance;
            CometController comet = CometController.Instance;
            if (run == null || comet == null) return;

            BuildStyles();

            // Scale everything from a 1080p layout so it reads the same on any screen.
            float scale = Screen.height / ReferenceHeight;
            GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1f));
            float width = Screen.width / scale;

            Rect safe = SafeRect(scale);
            float left = safe.xMin + 30f, right = safe.xMax - 30f, top = safe.yMin + 20f;

            // Shields, top left.
            // Drawn as squares: the default font has no shape glyphs.
            for (int i = 0; i < comet.Health.MaxShields; i++)
            {
                bool full = i < comet.Health.Shields;
                GUI.color = full ? new Color(0f, 0.94f, 1f) : new Color(1f, 1f, 1f, 0.2f);
                GUI.DrawTexture(new Rect(left + i * 52f, top + 10f, 38f, 38f), pixel);
            }

            // Score and combo, top center.
            GUI.color = Color.white;
            GUI.Label(new Rect(0f, top - 10f, width, 70f), run.Score.ToString("N0"), centerStyle);
            if (run.Combo.Count >= 2)
            {
                GUI.color = new Color(1f, 0.9f, 0f);
                GUI.Label(new Rect(0f, top + 60f, width, 50f), "x" + run.Combo.Multiplier.ToString("0.0") + "   " + run.Combo.Count + " combo", smallCentered());
                float bar = run.Combo.TimeLeftFraction(Time.time);
                GUI.DrawTexture(new Rect(width * 0.5f - 120f, top + 112f, 240f * bar, 6f), pixel);
            }

            // Time and best, top right.
            GUI.color = new Color(1f, 1f, 1f, 0.7f);
            GUI.Label(new Rect(right - 400f, top, 400f, 50f), FormatTime(run.RunTime), rightAligned(labelStyle));
            GUI.Label(new Rect(right - 400f, top + 50f, 400f, 40f), "Best " + run.Save.bestScore.ToString("N0"), rightAligned(smallStyle));

            // Trick popups, under the score.
            for (int i = 0; i < popups.Count; i++)
            {
                GUI.color = new Color(1f, 0.9f, 0f, Mathf.Clamp01(popups[i].timer));
                GUI.Label(new Rect(0f, top + 140f + i * 55f, width, 60f), popups[i].text, centerStyle);
            }

            // Debug toggles, bottom left (keyboard, for feel testing).
            GUI.color = new Color(1f, 1f, 1f, 0.35f);
            PrototypeTuning tuning = PrototypeTuning.Instance;
            GUI.Label(new Rect(left, safe.yMax - 50f, 900f, 40f),
                "[B] Wall bounce: " + (tuning.bounceOffWalls ? "on" : "off") + "    [F] Limit focus: " + (tuning.limitFocus ? "on" : "off"), smallStyle);

            if (run.IsGameOver)
                DrawGameOver(run, width);

            GUI.color = Color.white;
            GUI.matrix = Matrix4x4.identity;

            HandleDebugKeys(tuning);
        }

        private void DrawGameOver(RunManager run, float width)
        {
            GUI.color = new Color(0f, 0f, 0f, 0.6f);
            GUI.DrawTexture(new Rect(0f, 0f, width, ReferenceHeight), pixel);

            GUI.color = new Color(1f, 0.18f, 0.6f);
            GUI.Label(new Rect(0f, 300f, width, 120f), "GAME OVER", bigStyle);

            GUI.color = Color.white;
            string stats = "Score " + run.Score.ToString("N0") + (run.IsNewBest ? "   NEW BEST!" : "") +
                "\nBest combo " + run.Combo.Best + "     Kills " + run.Kills + "     Time " + FormatTime(run.RunTime);
            GUI.Label(new Rect(0f, 450f, width, 120f), stats, smallCentered());

            if (gameOverTimer >= RestartDelay)
            {
                GUI.color = new Color(0f, 0.94f, 1f, 0.6f + 0.4f * Mathf.Sin(Time.unscaledTime * 4f));
                GUI.Label(new Rect(0f, 640f, width, 60f), "Tap to play again", centerStyle);
            }
        }

        private void ShowPopup(string text)
        {
            popups.Insert(0, new Popup { text = text, timer = 1.2f });
            if (popups.Count > 3) popups.RemoveAt(popups.Count - 1);
        }

        private static bool RestartPressed()
        {
            if (Pointer.current != null && Pointer.current.press.wasPressedThisFrame) return true;
            if (Gamepad.current != null && (Gamepad.current.buttonSouth.wasPressedThisFrame || Gamepad.current.startButton.wasPressedThisFrame)) return true;
            if (Keyboard.current != null && (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.enterKey.wasPressedThisFrame)) return true;
            return false;
        }

        private static void HandleDebugKeys(PrototypeTuning tuning)
        {
            if (Event.current.type != EventType.KeyDown) return;

            if (Event.current.keyCode == KeyCode.B) tuning.bounceOffWalls = !tuning.bounceOffWalls;
            if (Event.current.keyCode == KeyCode.F) tuning.limitFocus = !tuning.limitFocus;
        }

        /// <summary>Screen.safeArea in the scaled GUI space (IMGUI's y axis points down).</summary>
        private static Rect SafeRect(float scale)
        {
            Rect area = Screen.safeArea;
            return new Rect(area.x / scale, (Screen.height - area.yMax) / scale, area.width / scale, area.height / scale);
        }

        private static string FormatTime(float seconds)
        {
            int total = Mathf.FloorToInt(seconds);
            return (total / 60) + ":" + (total % 60).ToString("00");
        }

        private void BuildStyles()
        {
            if (labelStyle != null) return;

            pixel = Texture2D.whiteTexture;
            labelStyle = new GUIStyle(GUI.skin.label) { fontSize = 44, fontStyle = FontStyle.Bold };
            labelStyle.normal.textColor = Color.white;
            smallStyle = new GUIStyle(labelStyle) { fontSize = 30, fontStyle = FontStyle.Normal };
            centerStyle = new GUIStyle(labelStyle) { alignment = TextAnchor.MiddleCenter, fontSize = 56 };
            bigStyle = new GUIStyle(centerStyle) { fontSize = 110 };
        }

        private GUIStyle smallCentered()
        {
            return new GUIStyle(smallStyle) { alignment = TextAnchor.MiddleCenter };
        }

        private static GUIStyle rightAligned(GUIStyle style)
        {
            return new GUIStyle(style) { alignment = TextAnchor.UpperRight };
        }
    }

}
