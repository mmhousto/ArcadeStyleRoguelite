using Com.MMsGamingCo.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Com.MMsGamingCo.Ricomet
{

    /// <summary>
    /// Builds the main menu from code: the neon arena with an attract-mode comet in the
    /// background, the title, Play / Settings / Credits / Quit, best score and version.
    /// Put this on one empty GameObject in the MainMenu scene.
    /// </summary>
    public class MainMenuBootstrap : MonoBehaviour
    {
        private RectTransform menuRoot;
        private RectTransform creditsRoot;
        private SettingsPanel settings;
        private Button playButton;

        private void Awake()
        {
            Application.targetFrameRate = PreferencesManager.GetTargetFrameRate();
            Time.timeScale = 1f;
            GameFlow.Configure();
            GameAudio.EnsureAudioManager();
            IOSHaptics.Register();

            Vector2 arenaSize = PrototypeTuning.Instance.arenaSize;
            SetupCamera(arenaSize);
            CreateChild<Arena>("Arena").Build(arenaSize);
            CreateChild<EffectSpawner>("Effects");
            CreateChild<ParticleBursts>("Particles");
            CreateChild<MenuAttractMode>("Attract Mode");

            BuildUI();
        }

        private void Start()
        {
            GameAudio.PlayMusic(GameAudio.MenuMusic, 1f);
            UIFactory.Select(playButton);
        }

        private void Update()
        {
            if (creditsRoot.gameObject.activeSelf && GameInput.BackPressed())
                CloseCredits();
        }

        #region UI

        private void BuildUI()
        {
            Canvas canvas = UIFactory.CreateCanvas("Menu Canvas", 10, transform);
            RectTransform safe = UIFactory.CreateSafeArea(canvas.transform);

            menuRoot = UIFactory.CreateRect("Menu", safe);
            UIFactory.Stretch(menuRoot);

            TextMeshProUGUI title = UIFactory.CreateText(menuRoot, "RICOMET", 190, UITheme.Cyan, TextAnchor.MiddleCenter);
            UIFactory.Place(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -90f), new Vector2(1600f, 230f));

            TextMeshProUGUI tagline = UIFactory.CreateText(menuRoot, "RICOCHET  /  COMET  /  NEON", 34, UITheme.Magenta, TextAnchor.MiddleCenter, false);
            UIFactory.Place(tagline.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -310f), new Vector2(1600f, 50f));

            RectTransform buttons = UIFactory.CreateRect("Buttons", menuRoot);
            UIFactory.Place(buttons, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 1f), new Vector2(0f, 120f), new Vector2(520f, 520f));
            UIFactory.AddVerticalLayout(buttons, 22f);

            playButton = UIFactory.CreateButton(buttons, "PLAY", UITheme.Cyan, Play, 60);
            UIFactory.SetLayoutHeight(playButton, 130f);
            Button settingsButton = UIFactory.CreateButton(buttons, "SETTINGS", UITheme.White, OpenSettings, 40);
            UIFactory.SetLayoutHeight(settingsButton, 100f);
            Button creditsButton = UIFactory.CreateButton(buttons, "CREDITS", UITheme.White, OpenCredits, 40);
            UIFactory.SetLayoutHeight(creditsButton, 100f);

#if !(UNITY_IOS || UNITY_ANDROID || UNITY_WEBGL)
            // Desktop only: mobile apps don't quit themselves.
            Button quitButton = UIFactory.CreateButton(buttons, "QUIT", UITheme.Magenta, Application.Quit, 40);
            UIFactory.SetLayoutHeight(quitButton, 100f);
#endif

            PrototypeSave save = SaveSystem.Load<PrototypeSave>(PrototypeSave.Slot);
            string best = save.bestScore > 0
                ? "BEST " + save.bestScore.ToString("N0") + "    BEST COMBO " + save.bestCombo + (save.bossesDefeated > 0 ? "    PRISM DEFEATED x" + save.bossesDefeated : "")
                : "DRAG BACK, LET GO, RICOCHET";
            TextMeshProUGUI bestText = UIFactory.CreateText(menuRoot, best, 32, UITheme.Yellow, TextAnchor.MiddleCenter, false);
            UIFactory.Place(bestText.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 120f), new Vector2(1600f, 50f));

            TextMeshProUGUI company = UIFactory.CreateText(menuRoot, "MM'S GAMING CO", 26, UITheme.Dim, TextAnchor.LowerLeft, false, false);
            UIFactory.Place(company.rectTransform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(40f, 30f), new Vector2(600f, 40f));
            TextMeshProUGUI version = UIFactory.CreateText(menuRoot, "V" + Application.version, 26, UITheme.Dim, TextAnchor.LowerRight, false, false);
            UIFactory.Place(version.rectTransform, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-40f, 30f), new Vector2(400f, 40f));

            BuildCredits(canvas.transform);
            settings = SettingsPanel.Create(canvas.transform, ShowMenu);
        }

        private void BuildCredits(Transform canvas)
        {
            creditsRoot = UIFactory.CreateRect("Credits", canvas);
            UIFactory.Stretch(creditsRoot);
            UIFactory.CreateBackdrop(creditsRoot);

            Image panel = UIFactory.CreatePanel(creditsRoot, UITheme.Magenta, UITheme.PanelFill, true);
            UIFactory.Place(panel.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1100f, 760f));

            TextMeshProUGUI title = UIFactory.CreateText(panel.transform, "CREDITS", 64, UITheme.Magenta, TextAnchor.MiddleCenter);
            UIFactory.Place(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -40f), new Vector2(1000f, 90f));

            TextMeshProUGUI body = UIFactory.CreateText(panel.transform,
                "A game by MM's Gaming Co\n\n" +
                "Made with Unity\n\n" +
                "Orbitron font by The Orbitron Project Authors\nSIL Open Font License 1.1\n\n" +
                "Placeholder art and audio generated for development",
                32, UITheme.White, TextAnchor.UpperCenter, false, false);
            UIFactory.Place(body.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -160f), new Vector2(980f, 420f));

            Button back = UIFactory.CreateButton(panel.transform, "BACK", UITheme.White, CloseCredits, 40);
            UIFactory.Place((RectTransform)back.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 40f), new Vector2(360f, 100f));
            creditsRoot.gameObject.SetActive(false);
        }

        #endregion

        #region Actions

        private void Play()
        {
            GameAudio.Play(GameAudio.WaveStart, 0.8f, 0f);
            GameFlow.StartRun();
        }

        private void OpenSettings()
        {
            menuRoot.gameObject.SetActive(false);
            settings.Show();
        }

        private void ShowMenu()
        {
            menuRoot.gameObject.SetActive(true);
            UIFactory.Select(playButton);
        }

        private void OpenCredits()
        {
            menuRoot.gameObject.SetActive(false);
            creditsRoot.gameObject.SetActive(true);
            UIFactory.Select(creditsRoot.GetComponentInChildren<Button>());
        }

        private void CloseCredits()
        {
            creditsRoot.gameObject.SetActive(false);
            ShowMenu();
        }

        #endregion

        private void SetupCamera(Vector2 arenaSize)
        {
            Camera cam = Camera.main;
            if (cam == null)
            {
                cam = new GameObject("Main Camera").AddComponent<Camera>();
                cam.tag = "MainCamera";
            }

            cam.orthographic = true;
            cam.transform.position = new Vector3(0f, 0f, -10f);
            cam.transform.rotation = Quaternion.identity;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = NeonColors.Background;
            cam.orthographicSize = Mathf.Max(arenaSize.y * 0.5f + 1f, (arenaSize.x * 0.5f + 1f) / Mathf.Max(0.1f, cam.aspect));
            if (cam.GetComponent<ScreenShake>() == null) cam.gameObject.AddComponent<ScreenShake>();
        }

        private T CreateChild<T>(string childName) where T : Component
        {
            GameObject child = new GameObject(childName);
            child.transform.SetParent(transform, false);
            return child.AddComponent<T>();
        }
    }

}
