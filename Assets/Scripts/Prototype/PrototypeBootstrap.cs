using Com.MMsGamingCo.Core;
using UnityEngine;

namespace Com.MMsGamingCo.Ricomet
{

    /// <summary>
    /// Builds the whole gray-box prototype from code: camera, arena, comet, waves,
    /// upgrades and HUD. Put this on one empty GameObject in a scene and press Play.
    /// </summary>
    public class PrototypeBootstrap : MonoBehaviour
    {
        [Tooltip("Optional. Leave empty to use Resources/PrototypeTuning or the defaults.")]
        public PrototypeTuning tuning;

        [Tooltip("0 = random seed each run. Set a number to replay the same spawns.")]
        public int seed = 0;

        [Tooltip("Extra world units shown around the arena (inside the HUD bands).")]
        public float cameraMargin = 0.4f;

        [Tooltip("Let the playtest bot play. Press P during play to toggle.")]
        public bool botPlays = false;

        /// <summary>Set by automated playtests before loading the scene. Null = use the inspector values.</summary>
        public static int? seedOverride = null;
        public static bool? botOverride = null;

        /// <summary>Editor menu flag (Ricomet > Playtest > Watch Bot).</summary>
        public const string WatchBotKey = "Ricomet.WatchBot";

        private void Awake()
        {
            PrototypeTuning.Use(tuning);
            PrototypeTuning settings = PrototypeTuning.Instance;
            int chosenSeed = seedOverride ?? seed;
            int runSeed = chosenSeed != 0 ? chosenSeed : SeededRandom.NewSeed();

            bool bot = botOverride ?? botPlays;
#if UNITY_EDITOR
            if (UnityEditor.SessionState.GetBool(WatchBotKey, false)) bot = true;
#endif

            Application.targetFrameRate = PreferencesManager.GetTargetFrameRate();
            GameFlow.Configure();
            GameAudio.EnsureAudioManager();
            IOSHaptics.Register();

            CreateChild<TimeController>("Time Controller");
            SetupCamera(settings);
            CreateChild<Arena>("Arena").Build(settings.arenaSize);
            CreateChild<EffectSpawner>("Effects");
            CreateChild<ParticleBursts>("Particles");
            CreateChild<RunManager>("Run Manager").Setup(settings, runSeed);
            CreateChild<CometController>("Comet").Setup(settings);

            // Effects live on their own objects so they don't move with the comet.
            CreateChild<CometAbilities>("Comet Abilities");
            CreateChild<PlasmaTrail>("Plasma Trail");

            CreateChild<EnemyFactory>("Enemies").Setup(settings);
            CreateChild<EnemyProjectiles>("Enemy Projectiles");
            CreateChild<GameHUD>("HUD");
            CreateChild<UpgradePickerUI>("Upgrade Picker");
            CreateChild<PauseMenu>("Pause Menu");
            CreateChild<GameAudioDirector>("Audio Director");
            CreateChild<TutorialManager>("Tutorial");
            BotDirector botDirector = CreateChild<BotDirector>("Playtest Bot");

            // Separate random streams, so taking different upgrades never changes the spawns.
            CreateChild<WaveDirector>("Wave Director").Setup(settings, SectorCatalog.Sector1(),
                new SeededRandom(runSeed), new SeededRandom(runSeed + 1));

            if (bot)
            {
                botDirector.SetBotActive(true);
                // Watching from the editor menu: loop runs so you can leave it going.
                botDirector.autoRestart = botOverride == null;
            }

            Debug.Log("Ricomet prototype started. Seed " + runSeed + (bot ? " (bot playing)" : ""));
        }

        private void SetupCamera(PrototypeTuning settings)
        {
            Camera cam = Camera.main;
            if (cam == null)
            {
                cam = new GameObject("Main Camera").AddComponent<Camera>();
                cam.tag = "MainCamera";
            }

            // Leave bands above and below the arena for the HUD.
            Rect safe = Screen.safeArea;
            float topInset = Screen.height > 0 ? (Screen.height - safe.yMax) / Screen.height : 0f;
            float bottomInset = Screen.height > 0 ? safe.y / Screen.height : 0f;
            HudLayout.FitArena(settings.arenaSize, cameraMargin, cam.aspect, topInset, bottomInset, out float orthographicSize, out float cameraY);

            cam.orthographic = true;
            cam.orthographicSize = orthographicSize;
            cam.transform.position = new Vector3(0f, cameraY, -10f);
            cam.transform.rotation = Quaternion.identity;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = NeonColors.Background;

            // Added after positioning: ScreenShake shakes around the position it starts at.
            if (cam.GetComponent<ScreenShake>() == null)
                cam.gameObject.AddComponent<ScreenShake>();
        }

        private T CreateChild<T>(string childName) where T : Component
        {
            GameObject child = new GameObject(childName);
            child.transform.SetParent(transform, false);
            return child.AddComponent<T>();
        }
    }

}
