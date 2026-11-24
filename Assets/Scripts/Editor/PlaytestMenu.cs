using Com.MMsGamingCo.Core;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace Com.MMsGamingCo.Ricomet.Editor
{

    /// <summary>
    /// Ricomet menu: watch the bot play, play yourself, run the bot playtests, and
    /// reset the tutorial or saves.
    /// </summary>
    [InitializeOnLoad]
    public static class PlaytestMenu
    {
        private const string PrototypeScene = "Assets/Scenes/Prototype.unity";
        private const string MainMenuScene = "Assets/Scenes/MainMenu.unity";

        static PlaytestMenu()
        {
            // Watching the bot only lasts for one play session.
            EditorApplication.playModeStateChanged += state =>
            {
                if (state == PlayModeStateChange.EnteredEditMode)
                    SessionState.SetBool(PrototypeBootstrap.WatchBotKey, false);
            };
        }

        [MenuItem("Ricomet/Playtest/Watch Bot Play", false, 1)]
        public static void WatchBot()
        {
            Play(true);
        }

        [MenuItem("Ricomet/Playtest/Play Run Scene", false, 2)]
        public static void PlayMyself()
        {
            Play(false);
        }

        [MenuItem("Ricomet/Playtest/Play From Main Menu", false, 3)]
        public static void PlayFromMenu()
        {
            if (EditorApplication.isPlaying) return;
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            SessionState.SetBool(PrototypeBootstrap.WatchBotKey, false);
            EditorSceneManager.OpenScene(MainMenuScene);
            EditorApplication.EnterPlaymode();
        }

        [MenuItem("Ricomet/Playtest/Run Bot Playtests (Test Runner)", false, 20)]
        public static void OpenTestRunner()
        {
            EditorApplication.ExecuteMenuItem("Window/General/Test Runner");
        }

        [MenuItem("Ricomet/Playtest/Open Playtest Reports", false, 21)]
        public static void OpenReports()
        {
            string folder = System.IO.Path.GetFullPath("Logs/Playtests");
            System.IO.Directory.CreateDirectory(folder);
            EditorUtility.RevealInFinder(folder);
        }

        [MenuItem("Ricomet/Saves/Reset Tutorial", false, 40)]
        public static void ResetTutorial()
        {
            SaveSystem.Delete(TutorialSave.Slot);
            EditorUtility.DisplayDialog("Ricomet", "Tutorial reset. It will play again from the start.", "OK");
        }

        [MenuItem("Ricomet/Saves/Delete Prototype Save", false, 41)]
        public static void DeletePrototypeSave()
        {
            if (EditorUtility.DisplayDialog("Ricomet", "Delete best scores and stats from the prototype save?", "Delete", "Cancel"))
                SaveSystem.Delete(PrototypeSave.Slot);
        }

        private static void Play(bool bot)
        {
            if (EditorApplication.isPlaying) return;
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            SessionState.SetBool(PrototypeBootstrap.WatchBotKey, bot);
            EditorSceneManager.OpenScene(PrototypeScene);
            EditorApplication.EnterPlaymode();
        }
    }

}
