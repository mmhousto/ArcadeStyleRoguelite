using Com.MMsGamingCo.Core;

namespace Com.MMsGamingCo.Ricomet
{

    /// <summary>
    /// Scene names and flow: Unity splash, then MainMenu, then the run scene.
    /// Scenes are light, so loads skip the loading screen.
    /// </summary>
    public static class GameFlow
    {
        public const string MainMenuScene = "MainMenu";
        public const string RunScene = "Prototype";

        /// <summary>Points the Core SceneLoader at Ricomet's scenes. Safe to call more than once.</summary>
        public static void Configure()
        {
            SceneLoader.mainMenuSceneName = MainMenuScene;
            SceneLoader.loadingSceneName = null;
        }

        public static void StartRun()
        {
            Configure();
            SceneLoader.LoadScene(RunScene, false);
        }

        public static void ToMainMenu()
        {
            Configure();
            SceneLoader.LoadScene(MainMenuScene, false);
        }
    }

}
