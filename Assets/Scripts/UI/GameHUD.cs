using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Com.MMsGamingCo.Ricomet
{

    /// <summary>
    /// The in-run HUD, built from code with uGUI: shields, score, combo, wave info,
    /// boss health, owned upgrades, banners, trick popups, a pause button and the
    /// end-of-run screen. Only buttons and menus block touches; everything else lets
    /// the player aim through it.
    /// </summary>
    public class GameHUD : MonoBehaviour
    {

        #region Fields/Variables

        private const float BannerTime = 1.8f;
        private const float PopupTime = 1.2f;
        private const float EndButtonDelay = 0.8f;

        private RectTransform safe;
        private readonly List<Image> shieldIcons = new List<Image>();
        private RectTransform shieldRow;
        private Text scoreText, comboText, waveText, enemiesText, timeText, debugText;
        private Image comboBar;
        private RectTransform bossBar;
        private Image bossFill;
        private Text bossLabel;
        private RectTransform upgradeRow;
        private readonly Dictionary<string, Text> upgradeStacks = new Dictionary<string, Text>();
        private Text bannerText, bannerSubText;
        private float bannerTimer;
        private Text[] popupTexts;
        private float[] popupTimers;
        private Button pauseButton;

        // End screen
        private RectTransform endScreen;
        private Text endTitle, endStats, endBuild;
        private Button endPrimary, endSecondary, endMenu;
        private float endTimer;
        private bool endShown;

        #endregion


        #region MonoBehaviour Methods

        private void Awake()
        {
            Build();
        }

        private void OnEnable()
        {
            RunManager.trickPerformed += ShowPopup;
            WaveDirector.waveStarted += OnWaveStarted;
            WaveDirector.bossIncoming += OnBossIncoming;
            PrismBoss.phaseChanged += OnBossPhaseChanged;
            WaveDirector.upgradeChosen += RefreshUpgrades;
        }

        private void OnDisable()
        {
            RunManager.trickPerformed -= ShowPopup;
            WaveDirector.waveStarted -= OnWaveStarted;
            WaveDirector.bossIncoming -= OnBossIncoming;
            PrismBoss.phaseChanged -= OnBossPhaseChanged;
            WaveDirector.upgradeChosen -= RefreshUpgrades;
        }

        private void Update()
        {
            RunManager run = RunManager.Instance;
            CometController comet = CometController.Instance;
            if (run == null || comet == null) return;

            UpdateShields(comet);
            UpdateScore(run);
            UpdateWaveInfo(run);
            UpdateBossBar();
            UpdateBanner();
            UpdatePopups();
            UpdateEndScreen(run);
            HandleDebugKeys();

            pauseButton.gameObject.SetActive(!run.IsOver && !comet.IsBotControlled);
        }

        #endregion


        #region Build

        private void Build()
        {
            Canvas canvas = UIFactory.CreateCanvas("HUD Canvas", 10, transform);
            safe = UIFactory.CreateSafeArea(canvas.transform);

            // Shields, top left.
            shieldRow = UIFactory.CreateRect("Shields", safe);
            UIFactory.Place(shieldRow, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(40f, -30f), new Vector2(500f, 70f));
            UIFactory.AddHorizontalLayout(shieldRow, 10f, TextAnchor.MiddleLeft);

            // Score and combo, top center.
            scoreText = UIFactory.CreateText(safe, "0", 72, UITheme.White, TextAnchor.MiddleCenter);
            UIFactory.Place(scoreText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -20f), new Vector2(800f, 90f));
            comboText = UIFactory.CreateText(safe, "", 38, UITheme.Yellow, TextAnchor.MiddleCenter);
            UIFactory.Place(comboText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -110f), new Vector2(800f, 50f));
            comboBar = UIFactory.CreateImage(safe, SpriteLibrary.UI("ui_fill"), UITheme.Yellow);
            UIFactory.Place(comboBar.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -165f), new Vector2(300f, 10f));
            comboBar.type = Image.Type.Simple;

            // Wave info, top right (left of the pause button).
            waveText = UIFactory.CreateText(safe, "", 40, UITheme.White, TextAnchor.UpperRight);
            UIFactory.Place(waveText.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-150f, -28f), new Vector2(600f, 50f));
            enemiesText = UIFactory.CreateText(safe, "", 30, UITheme.Magenta, TextAnchor.UpperRight, false);
            UIFactory.Place(enemiesText.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-150f, -80f), new Vector2(600f, 40f));
            timeText = UIFactory.CreateText(safe, "", 28, UITheme.Dim, TextAnchor.UpperRight, false, false);
            UIFactory.Place(timeText.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-150f, -122f), new Vector2(600f, 40f));

            // Pause button, top right corner.
            pauseButton = UIFactory.CreateButton(safe, "", UITheme.Cyan, () => { if (PauseMenu.Instance != null) PauseMenu.Instance.Open(); });
            UIFactory.Place((RectTransform)pauseButton.transform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-30f, -24f), new Vector2(96f, 96f));
            Image pauseIcon = UIFactory.CreateImage(pauseButton.transform, SpriteLibrary.UI("icon_pause"), UITheme.Cyan);
            UIFactory.Place(pauseIcon.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(80f, 80f));
            pauseButton.navigation = new Navigation { mode = Navigation.Mode.None };

            // Boss bar, bottom center.
            bossBar = UIFactory.CreateRect("Boss Bar", safe);
            UIFactory.Place(bossBar, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 40f), new Vector2(900f, 34f));
            Image bossBack = UIFactory.CreatePanel(bossBar, UITheme.Magenta, new Color(1f, 1f, 1f, 0.08f));
            UIFactory.Stretch(bossBack.rectTransform);
            bossFill = UIFactory.CreateImage(bossBar, null, UITheme.Magenta);
            bossFill.rectTransform.anchorMin = new Vector2(0f, 0f);
            bossFill.rectTransform.anchorMax = new Vector2(1f, 1f);
            bossFill.rectTransform.offsetMin = new Vector2(8f, 8f);
            bossFill.rectTransform.offsetMax = new Vector2(-8f, -8f);
            foreach (float threshold in PrismBoss.PhaseThresholds)
            {
                Image tick = UIFactory.CreateImage(bossBar, null, new Color(0f, 0f, 0f, 0.85f));
                UIFactory.Place(tick.rectTransform, new Vector2(threshold, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(6f, 34f));
            }
            bossLabel = UIFactory.CreateText(bossBar, "PRISM", 32, UITheme.White, TextAnchor.MiddleCenter);
            UIFactory.Place(bossLabel.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 0f), new Vector2(0f, 6f), new Vector2(900f, 44f));
            bossBar.gameObject.SetActive(false);

            // Owned upgrades, bottom right.
            upgradeRow = UIFactory.CreateRect("Upgrades", safe);
            UIFactory.Place(upgradeRow, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-30f, 30f), new Vector2(700f, 80f));
            UIFactory.AddHorizontalLayout(upgradeRow, 8f, TextAnchor.LowerRight);

            // Banner and popups.
            bannerText = UIFactory.CreateText(safe, "", 120, UITheme.Cyan, TextAnchor.MiddleCenter);
            UIFactory.Place(bannerText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 90f), new Vector2(1600f, 150f));
            bannerSubText = UIFactory.CreateText(safe, "", 40, UITheme.White, TextAnchor.MiddleCenter, false);
            UIFactory.Place(bannerSubText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -10f), new Vector2(1600f, 60f));

            popupTexts = new Text[3];
            popupTimers = new float[3];
            for (int i = 0; i < popupTexts.Length; i++)
            {
                popupTexts[i] = UIFactory.CreateText(safe, "", 48, UITheme.Yellow, TextAnchor.MiddleCenter);
                UIFactory.Place(popupTexts[i].rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -200f - i * 58f), new Vector2(1200f, 60f));
            }

            // Debug keys (editor and development builds only).
            debugText = UIFactory.CreateText(safe, "", 24, UITheme.Dim, TextAnchor.LowerLeft, false, false);
            UIFactory.Place(debugText.rectTransform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(40f, 20f), new Vector2(1100f, 40f));
            debugText.gameObject.SetActive(Debug.isDebugBuild);

            BuildEndScreen(canvas.transform);
        }

        private void BuildEndScreen(Transform canvas)
        {
            endScreen = UIFactory.CreateRect("End Screen", canvas);
            UIFactory.Stretch(endScreen);
            UIFactory.CreateBackdrop(endScreen);

            endTitle = UIFactory.CreateText(endScreen, "GAME OVER", 130, UITheme.Magenta, TextAnchor.MiddleCenter);
            UIFactory.Place(endTitle.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -120f), new Vector2(1600f, 160f));

            endStats = UIFactory.CreateText(endScreen, "", 44, UITheme.White, TextAnchor.MiddleCenter);
            UIFactory.Place(endStats.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -300f), new Vector2(1600f, 140f));

            endBuild = UIFactory.CreateText(endScreen, "", 32, UITheme.Dim, TextAnchor.MiddleCenter, false, false);
            UIFactory.Place(endBuild.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -460f), new Vector2(1500f, 110f));

            endPrimary = UIFactory.CreateButton(endScreen, "Play Again", UITheme.Cyan, OnPrimary, 46);
            endSecondary = UIFactory.CreateButton(endScreen, "New Run", UITheme.White, OnSecondary, 40);
            endMenu = UIFactory.CreateButton(endScreen, "Main Menu", UITheme.White, OnMenu, 40);
            endScreen.gameObject.SetActive(false);
        }

        #endregion


        #region Updates

        private void UpdateShields(CometController comet)
        {
            int max = comet.Health.MaxShields;
            while (shieldIcons.Count < max)
            {
                Image icon = UIFactory.CreateImage(shieldRow, SpriteLibrary.UI("shield_full"), UITheme.Cyan);
                icon.rectTransform.sizeDelta = new Vector2(64f, 64f);
                shieldIcons.Add(icon);
            }

            for (int i = 0; i < shieldIcons.Count; i++)
            {
                bool exists = i < max;
                bool full = i < comet.Health.Shields;
                shieldIcons[i].gameObject.SetActive(exists);
                shieldIcons[i].sprite = SpriteLibrary.UI(full ? "shield_full" : "shield");
                shieldIcons[i].color = full ? UITheme.Cyan : new Color(1f, 1f, 1f, 0.25f);
            }
        }

        private void UpdateScore(RunManager run)
        {
            scoreText.text = run.Score.ToString("N0");

            bool combo = run.Combo.Count >= 2;
            comboText.text = combo ? "x" + run.Combo.Multiplier.ToString("0.0") + "   " + run.Combo.Count + " COMBO" : "";
            comboBar.enabled = combo;
            if (combo)
                comboBar.rectTransform.sizeDelta = new Vector2(300f * run.Combo.TimeLeftFraction(Time.time), 10f);
        }

        private void UpdateWaveInfo(RunManager run)
        {
            WaveDirector director = WaveDirector.Instance;
            if (director != null && director.IsBossFight)
            {
                waveText.text = "BOSS";
                waveText.color = UITheme.Magenta;
                enemiesText.text = "";
            }
            else if (director != null)
            {
                waveText.text = director.IsEndless ? "WAVE " + director.WaveNumber + "  ENDLESS" : "WAVE " + director.WaveNumber + " / " + director.WaveCount;
                waveText.color = UITheme.White;
                enemiesText.text = director.EnemiesRemaining + " LEFT";
            }

            timeText.text = FormatTime(run.RunTime) + "   BEST " + run.Save.bestScore.ToString("N0");
        }

        private void UpdateBossBar()
        {
            PrismBoss boss = PrismBoss.Current;
            bool show = boss != null && boss.IsAlive;
            bossBar.gameObject.SetActive(show);
            if (!show) return;

            // Width tracks health (a filled Image would need a sprite).
            bossFill.rectTransform.anchorMax = new Vector2(Mathf.Max(0.001f, boss.HealthFraction), 1f);
            bossFill.color = boss.IsInvulnerable ? new Color(1f, 1f, 1f, 0.5f) : UITheme.Magenta;
            bossLabel.text = boss.DisplayName + "   PHASE " + (boss.Phase + 1);
        }

        private void RefreshUpgrades()
        {
            RunManager run = RunManager.Instance;
            if (run == null) return;

            foreach (string id in run.Inventory.OwnedIds)
            {
                UpgradeDefinition upgrade = UpgradeCatalog.Get(id);
                if (upgrade == null) continue;

                if (!upgradeStacks.TryGetValue(id, out Text stackText))
                {
                    Image frame = UIFactory.CreatePanel(upgradeRow, UITheme.RarityColor(upgrade.rarity), UITheme.PanelFill);
                    frame.rectTransform.sizeDelta = new Vector2(76f, 76f);
                    Image icon = UIFactory.CreateImage(frame.transform, SpriteLibrary.UpgradeIcon(id), UITheme.RarityColor(upgrade.rarity));
                    UIFactory.Place(icon.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(64f, 64f));
                    stackText = UIFactory.CreateText(frame.transform, "", 24, UITheme.White, TextAnchor.LowerRight);
                    UIFactory.Place(stackText.rectTransform, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-4f, 2f), new Vector2(60f, 30f));
                    upgradeStacks[id] = stackText;
                }

                int stacks = run.Inventory.GetStacks(id);
                stackText.text = stacks > 1 ? stacks.ToString() : "";
            }
        }

        private void UpdateBanner()
        {
            if (bannerTimer > 0f) bannerTimer -= Time.unscaledDeltaTime;
            float alpha = Mathf.Clamp01(bannerTimer / 0.4f);
            bannerText.canvasRenderer.SetAlpha(alpha);
            bannerSubText.canvasRenderer.SetAlpha(alpha);
        }

        private void UpdatePopups()
        {
            for (int i = 0; i < popupTexts.Length; i++)
            {
                if (popupTimers[i] > 0f) popupTimers[i] -= Time.unscaledDeltaTime;
                popupTexts[i].canvasRenderer.SetAlpha(Mathf.Clamp01(popupTimers[i] / 0.3f));
            }
        }

        private void UpdateEndScreen(RunManager run)
        {
            if (!run.IsOver)
            {
                if (endShown) endScreen.gameObject.SetActive(false);
                endShown = false;
                endTimer = 0f;
                return;
            }

            if (!endShown)
            {
                endShown = true;
                ShowEndScreen(run);
            }

            endTimer += Time.unscaledDeltaTime;
            bool ready = endTimer >= EndButtonDelay;
            if (endPrimary.interactable != ready)
            {
                endPrimary.interactable = ready;
                endSecondary.interactable = ready;
                endMenu.interactable = ready;
                if (ready) UIFactory.Select(endPrimary);
            }
        }

        private void ShowEndScreen(RunManager run)
        {
            endScreen.gameObject.SetActive(true);
            endTitle.text = run.IsVictory ? "SECTOR CLEAR" : "GAME OVER";
            endTitle.color = run.IsVictory ? UITheme.Cyan : UITheme.Magenta;

            int wave = WaveDirector.Instance != null ? WaveDirector.Instance.WaveNumber : 0;
            endStats.text = "SCORE " + run.Score.ToString("N0") + (run.IsNewBest ? "   NEW BEST!" : "") +
                "\nWAVE " + wave + "    BEST COMBO " + run.Combo.Best + "    KILLS " + run.Kills + "    TIME " + FormatTime(run.RunTime);

            var names = new List<string>();
            foreach (string id in run.Inventory.OwnedIds)
            {
                UpgradeDefinition upgrade = UpgradeCatalog.Get(id);
                int stacks = run.Inventory.GetStacks(id);
                if (upgrade != null) names.Add(stacks > 1 ? upgrade.displayName + " " + stacks : upgrade.displayName);
            }
            endBuild.text = names.Count > 0 ? "BUILD: " + string.Join(", ", names) : "NO UPGRADES TAKEN";

            // Victory: Keep Going / New Run / Menu. Defeat: Play Again / Menu.
            endPrimary.GetComponentInChildren<Text>().text = run.IsVictory ? "Keep Going" : "Play Again";
            endSecondary.gameObject.SetActive(run.IsVictory);

            float y = 140f;
            if (run.IsVictory)
            {
                UIFactory.Place((RectTransform)endPrimary.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-420f, y), new Vector2(460f, 120f));
                UIFactory.Place((RectTransform)endSecondary.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, y), new Vector2(340f, 120f));
                UIFactory.Place((RectTransform)endMenu.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(400f, y), new Vector2(380f, 120f));
            }
            else
            {
                UIFactory.Place((RectTransform)endPrimary.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-220f, y), new Vector2(420f, 120f));
                UIFactory.Place((RectTransform)endMenu.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(220f, y), new Vector2(380f, 120f));
            }

            endPrimary.interactable = false;
            endSecondary.interactable = false;
            endMenu.interactable = false;
        }

        #endregion


        #region Events

        private void OnPrimary()
        {
            RunManager run = RunManager.Instance;
            if (run == null) return;

            if (run.IsVictory && WaveDirector.Instance != null)
                WaveDirector.Instance.ContinueEndless();
            else
                run.Restart();
        }

        private void OnSecondary()
        {
            if (RunManager.Instance != null) RunManager.Instance.Restart();
        }

        private void OnMenu()
        {
            GameFlow.ToMainMenu();
        }

        private void OnWaveStarted(int waveNumber)
        {
            WaveDirector director = WaveDirector.Instance;
            bool endless = director != null && director.IsEndless;
            bool finalWave = director != null && !endless && waveNumber == director.WaveCount;
            ShowBanner("WAVE " + waveNumber, endless ? "ENDLESS" : finalWave ? "FINAL WAVE" : director != null ? director.Sector.displayName.ToUpper() : "", UITheme.Cyan);
        }

        private void OnBossIncoming()
        {
            ShowBanner("WARNING", "PRISM APPROACHING", UITheme.Orange);
        }

        private void OnBossPhaseChanged(int phase)
        {
            ShowBanner("PHASE " + phase, phase == 3 ? "FINAL PHASE" : "", UITheme.Orange);
        }

        private void ShowBanner(string title, string subtitle, Color color)
        {
            bannerText.text = title;
            bannerText.color = color;
            bannerSubText.text = subtitle;
            bannerTimer = BannerTime;
        }

        private void ShowPopup(string text)
        {
            // Shift older popups down; newest at the top.
            for (int i = popupTexts.Length - 1; i > 0; i--)
            {
                popupTexts[i].text = popupTexts[i - 1].text;
                popupTimers[i] = popupTimers[i - 1];
            }
            popupTexts[0].text = text;
            popupTimers[0] = PopupTime;
        }

        private void HandleDebugKeys()
        {
            UnityEngine.InputSystem.Keyboard keyboard = UnityEngine.InputSystem.Keyboard.current;
            PrototypeTuning tuning = PrototypeTuning.Instance;

            if (debugText.gameObject.activeSelf)
            {
                debugText.text = "[B] WALL BOUNCE " + (tuning.bounceOffWalls ? "ON" : "OFF") + "   [F] LIMIT FOCUS " + (tuning.limitFocus ? "ON" : "OFF") +
                    "   [P] BOT " + (BotDirector.Instance != null && BotDirector.Instance.IsActive ? "ON" : "OFF") + "   [T] RESET TUTORIAL";
            }

            if (keyboard == null) return;

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

        #endregion

    }

}
