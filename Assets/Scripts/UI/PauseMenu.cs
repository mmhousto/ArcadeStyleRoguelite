using Com.MMsGamingCo.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Com.MMsGamingCo.Ricomet
{

    /// <summary>
    /// Pause menu: Resume, Settings, Restart, Main Menu. Opens from the HUD pause
    /// button, Escape / gamepad Start, or when the app goes to the background on iOS.
    /// Not available while picking an upgrade or after the run has ended.
    /// </summary>
    public class PauseMenu : MonoBehaviour
    {
        private static PauseMenu _instance;

        public static PauseMenu Instance { get { return _instance; } }

        public bool IsOpen { get; private set; }

        private RectTransform root;
        private Button resumeButton;
        private SettingsPanel settings;

        private void Awake()
        {
            _instance = this;
            Build();
        }

        private void OnDestroy()
        {
            if (_instance == this) _instance = null;
        }

        private void Update()
        {
            if (settings.gameObject.activeSelf || SettingsPanel.ClosedFrame == Time.frameCount) return;

            if (GameInput.PausePressed())
            {
                if (IsOpen) Close();
                else Open();
            }
            else if (IsOpen && GameInput.BackPressed())
            {
                Close();
            }
        }

        private void OnApplicationPause(bool paused)
        {
            // iOS: going to the background pauses the run.
            if (paused) Open();
        }

        /// <summary>Pauses the run and shows the menu, if pausing is allowed right now.</summary>
        public void Open()
        {
            if (IsOpen || !CanPause()) return;

            IsOpen = true;
            root.gameObject.SetActive(true);
            SetGamePaused(true);
            UIFactory.Select(resumeButton);
        }

        public void Close()
        {
            if (!IsOpen) return;

            IsOpen = false;
            settings.Hide();
            root.gameObject.SetActive(false);
            SetGamePaused(false);
        }

        private bool CanPause()
        {
            RunManager run = RunManager.Instance;
            WaveDirector director = WaveDirector.Instance;
            CometController comet = CometController.Instance;
            if (run == null || run.IsOver) return false;
            if (director != null && director.CurrentPhase == WaveDirector.Phase.Picking) return false;
            if (comet != null && comet.IsBotControlled) return false;
            return true;
        }

        private static void SetGamePaused(bool paused)
        {
            if (TimeController.Instance != null) TimeController.Instance.SetPaused(paused);
            if (CometController.Instance != null) CometController.Instance.InputLocked = paused;
            if (AudioManager.HasInstance) AudioManager.Instance.PauseMusic(paused);
        }

        private void Build()
        {
            Canvas canvas = UIFactory.CreateCanvas("Pause Canvas", 30, transform);
            root = UIFactory.CreateRect("Pause Menu", canvas.transform);
            UIFactory.Stretch(root);
            UIFactory.CreateBackdrop(root);

            Image panel = UIFactory.CreatePanel(root, UITheme.Cyan, UITheme.PanelFill, true);
            UIFactory.Place(panel.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(640f, 760f));

            Text title = UIFactory.CreateText(panel.transform, "PAUSED", 72, UITheme.Cyan, TextAnchor.MiddleCenter);
            UIFactory.Place(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -40f), new Vector2(600f, 100f));

            RectTransform buttons = UIFactory.CreateRect("Buttons", panel.transform);
            UIFactory.Place(buttons, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -170f), new Vector2(480f, 540f));
            UIFactory.AddVerticalLayout(buttons, 24f);

            resumeButton = UIFactory.CreateButton(buttons, "Resume", UITheme.Cyan, Close);
            Button settingsButton = UIFactory.CreateButton(buttons, "Settings", UITheme.White, OpenSettings);
            Button restartButton = UIFactory.CreateButton(buttons, "Restart", UITheme.White, Restart);
            Button menuButton = UIFactory.CreateButton(buttons, "Main Menu", UITheme.Magenta, ToMenu);
            foreach (Button button in new[] { resumeButton, settingsButton, restartButton, menuButton })
                UIFactory.SetLayoutHeight(button, 110f);

            settings = SettingsPanel.Create(canvas.transform, () => UIFactory.Select(resumeButton));
            root.gameObject.SetActive(false);
        }

        private void OpenSettings()
        {
            settings.Show();
        }

        private void Restart()
        {
            Close();
            if (RunManager.Instance != null) RunManager.Instance.Restart();
        }

        private void ToMenu()
        {
            Close();
            GameFlow.ToMainMenu();
        }
    }

}
