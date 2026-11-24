using Com.MMsGamingCo.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Com.MMsGamingCo.Ricomet
{

    /// <summary>
    /// Settings: volumes (through the Core AudioPlayerSettings), haptics, reduce screen
    /// shake, reduce flashing, and replaying the tutorial. Used by the pause and main menus.
    /// </summary>
    public class SettingsPanel : MonoBehaviour
    {
        /// <summary>Frame the panel last closed, so the same key press doesn't also close the menu behind it.</summary>
        public static int ClosedFrame { get; private set; } = -1;

        private System.Action onBack;
        private Selectable firstControl;

        /// <summary>Builds the panel under a canvas. Starts hidden; call Show().</summary>
        public static SettingsPanel Create(Transform canvas, System.Action onBack)
        {
            RectTransform root = UIFactory.CreateRect("Settings", canvas);
            UIFactory.Stretch(root);
            UIFactory.CreateBackdrop(root);

            SettingsPanel panel = root.gameObject.AddComponent<SettingsPanel>();
            panel.onBack = onBack;
            panel.Build(root);
            root.gameObject.SetActive(false);
            return panel;
        }

        public void Show()
        {
            gameObject.SetActive(true);
            UIFactory.Select(firstControl);
        }

        public void Hide()
        {
            if (gameObject.activeSelf) ClosedFrame = Time.frameCount;
            PreferencesManager.Save();
            gameObject.SetActive(false);
        }

        private void Build(RectTransform root)
        {
            Image frame = UIFactory.CreatePanel(root, UITheme.Cyan, UITheme.PanelFill, true);
            UIFactory.Place(frame.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1000f, 900f));

            Text title = UIFactory.CreateText(frame.transform, "SETTINGS", 64, UITheme.Cyan, TextAnchor.MiddleCenter);
            UIFactory.Place(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -30f), new Vector2(900f, 90f));

            RectTransform list = UIFactory.CreateRect("Controls", frame.transform);
            UIFactory.Place(list, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -140f), new Vector2(820f, 620f));
            UIFactory.AddVerticalLayout(list, 10f);

            // The Core AudioPlayerSettings wires sliders to the AudioManager and saves them.
            Slider master = UIFactory.CreateSlider(list, "Master", PreferencesManager.GetMasterVolume(), null);
            Slider music = UIFactory.CreateSlider(list, "Music", PreferencesManager.GetMusicVolume(), null);
            Slider sfx = UIFactory.CreateSlider(list, "Effects", PreferencesManager.GetSFXVolume(), null);
            gameObject.SetActive(false);
            AudioPlayerSettings audioSettings = gameObject.AddComponent<AudioPlayerSettings>();
            audioSettings.masterSlider = master;
            audioSettings.musicSlider = music;
            audioSettings.sfxSlider = sfx;
            firstControl = master;

            UIFactory.CreateToggle(list, "Haptics", PreferencesManager.GetHaptics(), on =>
            {
                PreferencesManager.SetHaptics(on);
                if (on) Haptics.Impact(HapticStrength.Medium);
            });
            UIFactory.CreateToggle(list, "Reduce screen shake", GameSettings.ReduceShake, on => GameSettings.ReduceShake = on);
            UIFactory.CreateToggle(list, "Reduce flashing", GameSettings.ReduceFlashing, on => GameSettings.ReduceFlashing = on);

            Button tutorial = UIFactory.CreateButton(list, "Replay tutorial", UITheme.Yellow, ReplayTutorial, 32);
            UIFactory.SetLayoutHeight(tutorial, 80f);

            Button back = UIFactory.CreateButton(frame.transform, "Back", UITheme.White, Back, 40);
            UIFactory.Place((RectTransform)back.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 30f), new Vector2(360f, 100f));
        }

        private void ReplayTutorial()
        {
            if (TutorialManager.Instance != null)
                TutorialManager.Instance.ResetTutorial();
            else
                SaveSystem.Delete(TutorialSave.Slot);
        }

        private void Back()
        {
            Hide();
            onBack?.Invoke();
        }

        private void Update()
        {
            if (GameInput.BackPressed()) Back();
        }
    }

}
