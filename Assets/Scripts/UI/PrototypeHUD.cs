using System.Collections.Generic;
using UnityEngine;

namespace Com.MMsGamingCo.Ricomet
{

    /// <summary>
    /// Gray-box HUD drawn with IMGUI so the prototype needs no UI prefabs.
    /// Shows shields, score, combo, wave, upgrades, banners, trick popups and the
    /// end-of-run screen. Replaced by a real uGUI HUD in a later version.
    /// </summary>
    public class PrototypeHUD : MonoBehaviour
    {

        #region Fields/Variables

        private const float ButtonDelay = 0.8f;
        private const float BannerTime = 1.6f;

        private class Popup
        {
            public string text;
            public float timer;
        }

        private readonly List<Popup> popups = new List<Popup>();
        private string bannerText = "";
        private string bannerSubText = "";
        private float bannerTimer = 0f;
        private float overTimer = 0f;
        private bool pressStartedWhileOver = false;
        private GUIStyle labelStyle, bigStyle, smallStyle, centerStyle, buttonStyle;

        #endregion


        #region MonoBehaviour Methods

        private void OnEnable()
        {
            RunManager.trickPerformed += ShowPopup;
            WaveDirector.waveStarted += OnWaveStarted;
            WaveDirector.bossIncoming += OnBossIncoming;
            PrismBoss.phaseChanged += OnBossPhaseChanged;
        }

        private void OnDisable()
        {
            RunManager.trickPerformed -= ShowPopup;
            WaveDirector.waveStarted -= OnWaveStarted;
            WaveDirector.bossIncoming -= OnBossIncoming;
            PrismBoss.phaseChanged -= OnBossPhaseChanged;
        }

        private void Update()
        {
            for (int i = popups.Count - 1; i >= 0; i--)
            {
                popups[i].timer -= Time.unscaledDeltaTime;
                if (popups[i].timer <= 0f) popups.RemoveAt(i);
            }
            if (bannerTimer > 0f) bannerTimer -= Time.unscaledDeltaTime;

            HandleDebugKeys();

            RunManager run = RunManager.Instance;
            if (run == null || !run.IsOver)
            {
                overTimer = 0f;
                pressStartedWhileOver = false;
                return;
            }

            overTimer += Time.unscaledDeltaTime;
            if (overTimer < ButtonDelay) return;

            if (GuiInput.PointerPressed) pressStartedWhileOver = true;
            bool tapped = pressStartedWhileOver && GuiInput.PointerReleased;

            if (run.IsVictory)
            {
                if ((tapped && GuiInput.Tapped(PrimaryButtonRect())) || GuiInput.ConfirmPressed())
                    WaveDirector.Instance.ContinueEndless();
                else if ((tapped && GuiInput.Tapped(SecondaryButtonRect())) || GuiInput.AltPressed())
                    run.Restart();
            }
            else if ((tapped && GuiInput.Tapped(PrimaryButtonRect())) || GuiInput.ConfirmPressed() || GuiInput.AltPressed())
            {
                run.Restart();
            }
        }

        private void OnApplicationPause(bool pause)
        {
            // iOS: pause when the app goes to the background. Don't unpause the upgrade picker.
            if (TimeController.Instance == null) return;
            bool picking = WaveDirector.Instance != null && WaveDirector.Instance.CurrentPhase == WaveDirector.Phase.Picking;
            if (pause || !picking)
                TimeController.Instance.SetPaused(pause);
        }

        private void OnGUI()
        {
            RunManager run = RunManager.Instance;
            CometController comet = CometController.Instance;
            if (run == null || comet == null) return;

            BuildStyles();
            GUI.matrix = Matrix4x4.Scale(new Vector3(GuiInput.Scale, GuiInput.Scale, 1f));
            float width = GuiInput.Width;

            Rect safe = GuiInput.SafeRect();
            float left = safe.xMin + 30f, right = safe.xMax - 30f, top = safe.yMin + 20f;

            DrawShields(comet, left, top);
            DrawScore(run, width, top);
            DrawWaveInfo(run, right, top);
            DrawUpgrades(run, right, safe.yMax - 30f);
            DrawBossBar(width, safe.yMax);
            DrawPopups(width, top);
            DrawBanner(width);

            GUI.color = new Color(1f, 1f, 1f, 0.3f);
            PrototypeTuning tuning = PrototypeTuning.Instance;
            GUI.Label(new Rect(left, safe.yMax - 50f, 1300f, 40f),
                "[B] Wall bounce: " + (tuning.bounceOffWalls ? "on" : "off") + "   [F] Limit focus: " + (tuning.limitFocus ? "on" : "off") +
                "   [P] Bot: " + (BotDirector.Instance != null && BotDirector.Instance.IsActive ? "on" : "off") + "   [T] Reset tutorial", smallStyle);

            if (run.IsOver)
                DrawEndScreen(run, width);

            GUI.color = Color.white;
            GUI.matrix = Matrix4x4.identity;
        }

        #endregion


        #region Drawing

        private void DrawShields(CometController comet, float left, float top)
        {
            // Squares, because the default font has no shape glyphs.
            for (int i = 0; i < comet.Health.MaxShields; i++)
            {
                bool full = i < comet.Health.Shields;
                GUI.color = full ? new Color(0f, 0.94f, 1f) : new Color(1f, 1f, 1f, 0.2f);
                GUI.DrawTexture(new Rect(left + i * 52f, top + 10f, 38f, 38f), Texture2D.whiteTexture);
            }
        }

        private void DrawScore(RunManager run, float width, float top)
        {
            GUI.color = Color.white;
            GUI.Label(new Rect(0f, top - 10f, width, 70f), run.Score.ToString("N0"), centerStyle);

            if (run.Combo.Count < 2) return;

            GUI.color = new Color(1f, 0.9f, 0f);
            GUI.Label(new Rect(0f, top + 60f, width, 50f), "x" + run.Combo.Multiplier.ToString("0.0") + "   " + run.Combo.Count + " combo", Centered(smallStyle));
            float bar = run.Combo.TimeLeftFraction(Time.time);
            GUI.DrawTexture(new Rect(width * 0.5f - 120f, top + 112f, 240f * bar, 6f), Texture2D.whiteTexture);
        }

        private void DrawWaveInfo(RunManager run, float right, float top)
        {
            WaveDirector director = WaveDirector.Instance;
            GUI.color = new Color(1f, 1f, 1f, 0.8f);

            if (director != null && director.IsBossFight)
            {
                GUI.color = new Color(1f, 0.18f, 0.6f);
                GUI.Label(new Rect(right - 500f, top, 500f, 50f), "BOSS", RightAligned(labelStyle));
            }
            else if (director != null)
            {
                string wave = director.IsEndless
                    ? "Wave " + director.WaveNumber + "  ENDLESS"
                    : "Wave " + director.WaveNumber + " / " + director.WaveCount;
                GUI.Label(new Rect(right - 500f, top, 500f, 50f), wave, RightAligned(labelStyle));

                GUI.color = new Color(1f, 0.18f, 0.6f, 0.9f);
                GUI.Label(new Rect(right - 500f, top + 52f, 500f, 40f), director.EnemiesRemaining + " left", RightAligned(smallStyle));
            }

            GUI.color = new Color(1f, 1f, 1f, 0.5f);
            GUI.Label(new Rect(right - 500f, top + 92f, 500f, 40f), FormatTime(run.RunTime) + "   Best " + run.Save.bestScore.ToString("N0"), RightAligned(smallStyle));
        }

        private void DrawUpgrades(RunManager run, float right, float bottom)
        {
            IReadOnlyList<string> owned = run.Inventory.OwnedIds;
            for (int i = 0; i < owned.Count; i++)
            {
                UpgradeDefinition upgrade = UpgradeCatalog.Get(owned[i]);
                if (upgrade == null) continue;

                int stacks = run.Inventory.GetStacks(upgrade.id);
                string label = stacks > 1 ? upgrade.displayName + " " + stacks : upgrade.displayName;
                GUI.color = NeonColors.WithAlpha(UpgradePickerUI.RarityColor(upgrade.rarity), 0.8f);
                GUI.Label(new Rect(right - 400f, bottom - 40f - (owned.Count - 1 - i) * 36f, 400f, 36f), label, RightAligned(smallStyle));
            }
        }

        private void DrawBossBar(float width, float bottom)
        {
            PrismBoss boss = PrismBoss.Current;
            if (boss == null || !boss.IsAlive) return;

            float barWidth = Mathf.Min(900f, width * 0.5f);
            Rect back = new Rect((width - barWidth) * 0.5f, bottom - 70f, barWidth, 22f);

            GUI.color = new Color(1f, 1f, 1f, 0.12f);
            GUI.DrawTexture(back, Texture2D.whiteTexture);

            Color fill = boss.IsInvulnerable ? new Color(1f, 1f, 1f, 0.5f) : new Color(1f, 0.18f, 0.6f);
            GUI.color = fill;
            GUI.DrawTexture(new Rect(back.x, back.y, back.width * boss.HealthFraction, back.height), Texture2D.whiteTexture);

            // Ticks where the phases change.
            GUI.color = new Color(0f, 0f, 0f, 0.8f);
            foreach (float threshold in PrismBoss.PhaseThresholds)
                GUI.DrawTexture(new Rect(back.x + back.width * threshold - 2f, back.y, 4f, back.height), Texture2D.whiteTexture);

            GUI.color = Color.white;
            GUI.Label(new Rect(back.x, back.y - 46f, back.width, 44f), boss.DisplayName + "   PHASE " + (boss.Phase + 1), Centered(smallStyle));
        }

        private void DrawPopups(float width, float top)
        {
            for (int i = 0; i < popups.Count; i++)
            {
                GUI.color = new Color(1f, 0.9f, 0f, Mathf.Clamp01(popups[i].timer));
                GUI.Label(new Rect(0f, top + 140f + i * 55f, width, 60f), popups[i].text, centerStyle);
            }
        }

        private void DrawBanner(float width)
        {
            if (bannerTimer <= 0f) return;

            float alpha = Mathf.Clamp01(bannerTimer / 0.4f);
            bool danger = bannerText == "WARNING" || bannerText.StartsWith("PHASE");
            GUI.color = danger ? new Color(1f, 0.42f, 0f, alpha) : new Color(0f, 0.94f, 1f, alpha);
            GUI.Label(new Rect(0f, 380f, width, 130f), bannerText, bigStyle);
            GUI.color = new Color(1f, 1f, 1f, alpha * 0.8f);
            GUI.Label(new Rect(0f, 510f, width, 60f), bannerSubText, Centered(labelStyle));
        }

        private void DrawEndScreen(RunManager run, float width)
        {
            GUI.color = new Color(0f, 0f, 0f, 0.65f);
            GUI.DrawTexture(new Rect(0f, 0f, width, GuiInput.ReferenceHeight), Texture2D.whiteTexture);

            GUI.color = run.IsVictory ? new Color(0f, 0.94f, 1f) : new Color(1f, 0.18f, 0.6f);
            GUI.Label(new Rect(0f, 170f, width, 130f), run.IsVictory ? "SECTOR CLEAR" : "GAME OVER", bigStyle);

            GUI.color = Color.white;
            int wave = WaveDirector.Instance != null ? WaveDirector.Instance.WaveNumber : 0;
            string stats = "Score " + run.Score.ToString("N0") + (run.IsNewBest ? "   NEW BEST!" : "") +
                "\nWave " + wave + "     Best combo " + run.Combo.Best + "     Kills " + run.Kills + "     Time " + FormatTime(run.RunTime);
            GUI.Label(new Rect(0f, 320f, width, 120f), stats, Centered(labelStyle));

            // Build summary: every upgrade taken this run.
            var names = new List<string>();
            foreach (string id in run.Inventory.OwnedIds)
            {
                UpgradeDefinition upgrade = UpgradeCatalog.Get(id);
                int stacks = run.Inventory.GetStacks(id);
                if (upgrade != null) names.Add(stacks > 1 ? upgrade.displayName + " " + stacks : upgrade.displayName);
            }
            GUI.color = new Color(1f, 1f, 1f, 0.6f);
            GUI.Label(new Rect(100f, 460f, width - 200f, 120f), names.Count > 0 ? "Build: " + string.Join(", ", names) : "No upgrades taken", Centered(smallStyle));

            if (overTimer < ButtonDelay) return;

            if (run.IsVictory)
            {
                GuiInput.DrawButton(PrimaryButtonRect(), "Keep Going (Endless)", new Color(0f, 0.94f, 1f), true, buttonStyle);
                GuiInput.DrawButton(SecondaryButtonRect(), "New Run", new Color(1f, 1f, 1f), false, buttonStyle);
            }
            else
            {
                GuiInput.DrawButton(PrimaryButtonRect(), "Play Again", new Color(0f, 0.94f, 1f), true, buttonStyle);
            }
        }

        private Rect PrimaryButtonRect()
        {
            bool twoButtons = RunManager.Instance != null && RunManager.Instance.IsVictory;
            float buttonWidth = twoButtons ? 520f : 420f;
            float x = twoButtons ? GuiInput.Width * 0.5f - buttonWidth - 20f : (GuiInput.Width - buttonWidth) * 0.5f;
            return new Rect(x, 640f, buttonWidth, 110f);
        }

        private Rect SecondaryButtonRect()
        {
            return new Rect(GuiInput.Width * 0.5f + 20f, 640f, 360f, 110f);
        }

        #endregion


        #region Private Methods

        private void OnWaveStarted(int waveNumber)
        {
            WaveDirector director = WaveDirector.Instance;
            bool endless = director != null && director.IsEndless;
            bool finalWave = director != null && !endless && waveNumber == director.WaveCount;

            bannerText = "WAVE " + waveNumber;
            bannerSubText = endless ? "Endless" : finalWave ? "Final wave" : (director != null ? director.Sector.displayName : "");
            bannerTimer = BannerTime;
        }

        private void OnBossIncoming()
        {
            bannerText = "WARNING";
            bannerSubText = "Prism approaching. Dash through the gaps in its shield.";
            bannerTimer = 2.2f;
        }

        private void OnBossPhaseChanged(int phase)
        {
            bannerText = "PHASE " + phase;
            bannerSubText = phase == 3 ? "Final phase" : "";
            bannerTimer = 1.4f;
        }

        private void ShowPopup(string text)
        {
            popups.Insert(0, new Popup { text = text, timer = 1.2f });
            if (popups.Count > 3) popups.RemoveAt(popups.Count - 1);
        }

        private static void HandleDebugKeys()
        {
            UnityEngine.InputSystem.Keyboard keyboard = UnityEngine.InputSystem.Keyboard.current;
            if (keyboard == null) return;

            PrototypeTuning tuning = PrototypeTuning.Instance;
            bool changed = false;
            if (keyboard.bKey.wasPressedThisFrame) { tuning.bounceOffWalls = !tuning.bounceOffWalls; changed = true; }
            if (keyboard.fKey.wasPressedThisFrame) tuning.limitFocus = !tuning.limitFocus;
            if (keyboard.pKey.wasPressedThisFrame && BotDirector.Instance != null) BotDirector.Instance.Toggle();
            if (keyboard.tKey.wasPressedThisFrame && TutorialManager.Instance != null) TutorialManager.Instance.ResetTutorial();

            if (changed && RunManager.Instance != null)
                RunManager.Instance.RebuildStats();
        }

        private static string FormatTime(float seconds)
        {
            int total = Mathf.FloorToInt(seconds);
            return (total / 60) + ":" + (total % 60).ToString("00");
        }

        private void BuildStyles()
        {
            if (labelStyle != null) return;

            labelStyle = new GUIStyle(GUI.skin.label) { fontSize = 44, fontStyle = FontStyle.Bold };
            labelStyle.normal.textColor = Color.white;
            smallStyle = new GUIStyle(labelStyle) { fontSize = 30, fontStyle = FontStyle.Normal, wordWrap = true };
            centerStyle = new GUIStyle(labelStyle) { alignment = TextAnchor.MiddleCenter, fontSize = 56 };
            bigStyle = new GUIStyle(centerStyle) { fontSize = 110 };
            buttonStyle = new GUIStyle(centerStyle) { fontSize = 42 };
        }

        private static GUIStyle Centered(GUIStyle style)
        {
            return new GUIStyle(style) { alignment = TextAnchor.MiddleCenter };
        }

        private static GUIStyle RightAligned(GUIStyle style)
        {
            return new GUIStyle(style) { alignment = TextAnchor.UpperRight };
        }

        #endregion

    }

}
