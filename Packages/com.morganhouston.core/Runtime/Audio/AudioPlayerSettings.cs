using UnityEngine;
using UnityEngine.UI;

namespace Com.MMsGamingCo.Core
{

    /// <summary>
    /// Hooks volume sliders on a settings screen up to the AudioManager.
    /// Sliders should go from 0 to 1. Any slider left empty is ignored.
    /// </summary>
    public class AudioPlayerSettings : MonoBehaviour
    {
        public Slider masterSlider, musicSlider, sfxSlider;

        private void OnEnable()
        {
            // Set values before adding listeners so loading them doesn't re-save them.
            if (masterSlider) masterSlider.SetValueWithoutNotify(PreferencesManager.GetMasterVolume());
            if (musicSlider) musicSlider.SetValueWithoutNotify(PreferencesManager.GetMusicVolume());
            if (sfxSlider) sfxSlider.SetValueWithoutNotify(PreferencesManager.GetSFXVolume());

            if (masterSlider) masterSlider.onValueChanged.AddListener(ChangeMasterVolume);
            if (musicSlider) musicSlider.onValueChanged.AddListener(ChangeMusicVolume);
            if (sfxSlider) sfxSlider.onValueChanged.AddListener(ChangeSFXVolume);
        }

        private void OnDisable()
        {
            if (masterSlider) masterSlider.onValueChanged.RemoveListener(ChangeMasterVolume);
            if (musicSlider) musicSlider.onValueChanged.RemoveListener(ChangeMusicVolume);
            if (sfxSlider) sfxSlider.onValueChanged.RemoveListener(ChangeSFXVolume);

            PreferencesManager.Save();
        }

        public void ChangeMasterVolume(float volume)
        {
            if (AudioManager.HasInstance)
                AudioManager.Instance.SetMasterVolume(volume);
            else
                PreferencesManager.SetMasterVolume(volume);
        }

        public void ChangeMusicVolume(float volume)
        {
            if (AudioManager.HasInstance)
                AudioManager.Instance.SetMusicVolume(volume);
            else
                PreferencesManager.SetMusicVolume(volume);
        }

        public void ChangeSFXVolume(float volume)
        {
            if (AudioManager.HasInstance)
                AudioManager.Instance.SetSFXVolume(volume);
            else
                PreferencesManager.SetSFXVolume(volume);
        }

    }

}
