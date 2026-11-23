using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Com.MorganHouston.Core
{

    /// <summary>
    /// Loads scenes by name, optionally through a loading scene that shows progress.
    /// Same idea as ZombCube's SceneLoader, but by name instead of build index so
    /// reordering Build Settings can't break it. Set the scene names once at startup
    /// if your project doesn't use the defaults.
    /// </summary>
    public static class SceneLoader
    {

        /// <summary>Scene that contains a LoadingScreen component.</summary>
        public static string loadingSceneName = "LoadingScene";

        /// <summary>Scene ToMainMenu loads.</summary>
        public static string mainMenuSceneName = "MainMenu";

        /// <summary>Scene the loading screen should load next. Read by LoadingScreen.</summary>
        public static string SceneToLoad { get; private set; }

        /// <summary>Raised with the scene name before a load starts.</summary>
        public static event Action<string> sceneLoadStarted;

        /// <summary>Raised with the scene name once LoadingScreen finishes loading it.</summary>
        public static event Action<string> sceneLoadFinished;

        public static bool IsLoading { get; private set; }

        /// <summary>
        /// Loads a scene. With useLoadingScreen, the loading scene opens first and
        /// loads the target scene asynchronously.
        /// </summary>
        public static void LoadScene(string sceneName, bool useLoadingScreen = true)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                Debug.LogError("SceneLoader: no scene name given.");
                return;
            }

            PrepareForLoad();
            SceneToLoad = sceneName;
            sceneLoadStarted?.Invoke(sceneName);

            if (useLoadingScreen && !string.IsNullOrEmpty(loadingSceneName))
            {
                IsLoading = true;
                SceneManager.LoadScene(loadingSceneName);
            }
            else
            {
                SceneManager.LoadScene(sceneName);
                sceneLoadFinished?.Invoke(sceneName);
            }
        }

        public static void ToMainMenu()
        {
            LoadScene(mainMenuSceneName);
        }

        /// <summary>Reloads the active scene without the loading screen. Handy for "Retry".</summary>
        public static void ReloadCurrentScene()
        {
            LoadScene(GetCurrentScene().name, false);
        }

        public static Scene GetCurrentScene()
        {
            return SceneManager.GetActiveScene();
        }

        /// <summary>
        /// Starts the async load of SceneToLoad. Called by LoadingScreen; you shouldn't
        /// need to call it yourself.
        /// </summary>
        public static AsyncOperation BeginAsyncLoad()
        {
            if (string.IsNullOrEmpty(SceneToLoad))
            {
                Debug.LogWarning("SceneLoader: nothing queued, going to the main menu.");
                SceneToLoad = mainMenuSceneName;
            }

            // The loading scene is unloaded when the target activates, so LoadingScreen
            // can't report completion itself. Listen for the scene instead.
            SceneManager.sceneLoaded -= OnTargetSceneLoaded;
            SceneManager.sceneLoaded += OnTargetSceneLoaded;

            return SceneManager.LoadSceneAsync(SceneToLoad);
        }

        private static void OnTargetSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name != SceneToLoad) return;

            SceneManager.sceneLoaded -= OnTargetSceneLoaded;
            IsLoading = false;
            sceneLoadFinished?.Invoke(SceneToLoad);
        }

        private static void PrepareForLoad()
        {
            // A paused game sets timeScale to 0; never carry that into the next scene.
            Time.timeScale = 1;

#if UNITY_WSA
            ResetLightingData();
#endif
        }

        private static void ResetLightingData()
        {
            LightmapSettings.lightmaps = new LightmapData[0];
            Resources.UnloadUnusedAssets();
        }

    }

}
