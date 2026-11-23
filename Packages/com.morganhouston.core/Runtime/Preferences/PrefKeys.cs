namespace Com.MorganHouston.Core
{

    /// <summary>
    /// PlayerPrefs keys used by PreferencesManager.
    /// Never rename a key once it has shipped: players would lose that setting.
    /// </summary>
    public static class PrefKeys
    {
        public const string MasterVolume = "MasterVolume";
        public const string MusicVolume = "MusicVolume";
        public const string SFXVolume = "SFXVolume";
        public const string Haptics = "Haptics";
        public const string HorizontalSens = "HorizontalSens";
        public const string VerticalSens = "VerticalSens";
        public const string Shadows = "Shadows";
        public const string TargetFrameRate = "TargetFrameRate";
        public const string QualityLevel = "QualityLevel";
        public const string FirstLaunch = "FirstLaunch";
    }

}
