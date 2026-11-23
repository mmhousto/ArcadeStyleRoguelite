using System;
using UnityEngine;

namespace Com.MorganHouston.Core
{

    /// <summary>
    /// Typed wrapper around PlayerPrefs for device settings (volume, haptics, etc).
    /// Progress and unlocks belong in SaveSystem, not here.
    /// Volumes are stored as 0-1 linear values; AudioManager converts them to decibels.
    /// </summary>
    public static class PreferencesManager
    {

        /// <summary>Raised with the key whenever a setting is changed through this class.</summary>
        public static event Action<string> preferenceChanged;

        #region Audio

        public static void SetMasterVolume(float volume)
        {
            SetFloat(PrefKeys.MasterVolume, Mathf.Clamp01(volume));
        }

        public static void SetMusicVolume(float volume)
        {
            SetFloat(PrefKeys.MusicVolume, Mathf.Clamp01(volume));
        }

        public static void SetSFXVolume(float volume)
        {
            SetFloat(PrefKeys.SFXVolume, Mathf.Clamp01(volume));
        }

        public static float GetMasterVolume()
        {
            return PlayerPrefs.GetFloat(PrefKeys.MasterVolume, 1);
        }

        public static float GetMusicVolume()
        {
            return PlayerPrefs.GetFloat(PrefKeys.MusicVolume, 1);
        }

        public static float GetSFXVolume()
        {
            return PlayerPrefs.GetFloat(PrefKeys.SFXVolume, 1);
        }

        #endregion

        #region Controls

        public static void SetHaptics(bool input)
        {
            SetBool(PrefKeys.Haptics, input);
        }

        public static bool GetHaptics()
        {
            return GetBool(PrefKeys.Haptics, true);
        }

        public static void SetHorizontalSens(float sensitivity)
        {
            SetFloat(PrefKeys.HorizontalSens, sensitivity);
        }

        public static void SetVerticalSens(float sensitivity)
        {
            SetFloat(PrefKeys.VerticalSens, sensitivity);
        }

        public static float GetHorizontalSens()
        {
            return PlayerPrefs.GetFloat(PrefKeys.HorizontalSens, 20);
        }

        public static float GetVerticalSens()
        {
            return PlayerPrefs.GetFloat(PrefKeys.VerticalSens, 20);
        }

        #endregion

        #region Visual

        public static void SetShadows(bool input)
        {
            SetBool(PrefKeys.Shadows, input);
        }

        public static bool GetShadows()
        {
            return GetBool(PrefKeys.Shadows, true);
        }

        /// <summary>Target frame rate. 60 is a good default for iOS arcade games.</summary>
        public static void SetTargetFrameRate(int frameRate)
        {
            SetInt(PrefKeys.TargetFrameRate, frameRate);
        }

        public static int GetTargetFrameRate()
        {
            return PlayerPrefs.GetInt(PrefKeys.TargetFrameRate, 60);
        }

        /// <summary>Quality level index. -1 means use the project default.</summary>
        public static void SetQualityLevel(int level)
        {
            SetInt(PrefKeys.QualityLevel, level);
        }

        public static int GetQualityLevel()
        {
            return PlayerPrefs.GetInt(PrefKeys.QualityLevel, -1);
        }

        #endregion

        #region First Launch

        /// <summary>True until MarkFirstLaunchComplete is called.</summary>
        public static bool IsFirstLaunch()
        {
            return GetBool(PrefKeys.FirstLaunch, true);
        }

        public static void MarkFirstLaunchComplete()
        {
            SetBool(PrefKeys.FirstLaunch, false);
        }

        #endregion

        #region Generic Helpers

        public static void SetBool(string key, bool value)
        {
            PlayerPrefs.SetInt(key, value ? 1 : 0);
            preferenceChanged?.Invoke(key);
        }

        public static bool GetBool(string key, bool defaultValue = false)
        {
            return PlayerPrefs.GetInt(key, defaultValue ? 1 : 0) == 1;
        }

        public static void SetInt(string key, int value)
        {
            PlayerPrefs.SetInt(key, value);
            preferenceChanged?.Invoke(key);
        }

        public static void SetFloat(string key, float value)
        {
            PlayerPrefs.SetFloat(key, value);
            preferenceChanged?.Invoke(key);
        }

        public static void SetString(string key, string value)
        {
            PlayerPrefs.SetString(key, value);
            preferenceChanged?.Invoke(key);
        }

        /// <summary>
        /// Writes PlayerPrefs to disk. Unity also saves on quit, but iOS can kill the
        /// app without a clean quit, so call this after a settings screen closes.
        /// </summary>
        public static void Save()
        {
            PlayerPrefs.Save();
        }

        /// <summary>Clears every PlayerPrefs key for this app.</summary>
        public static void DeleteAll()
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
        }

        #endregion

    }

}
