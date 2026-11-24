using System.Collections.Generic;
using Com.MMsGamingCo.Core;
using UnityEngine;

namespace Com.MMsGamingCo.Ricomet
{

    /// <summary>
    /// Ricomet's sounds by name, loaded from Resources/Audio and played through the
    /// Core AudioManager. Replace a WAV in Resources/Audio/Sfx or /Music with a real
    /// sound of the same name and it's used automatically.
    /// </summary>
    public static class GameAudio
    {
        public const string SfxPath = "Audio/Sfx/";
        public const string MusicPath = "Audio/Music/";

        // Sound names. Keep in sync with Tools/generate_audio.py.
        public const string Dash = "dash";
        public const string Kill = "kill";
        public const string Land = "land";
        public const string Hit = "hit";
        public const string ShieldRepair = "shield_repair";
        public const string WaveStart = "wave_start";
        public const string WaveClear = "wave_clear";
        public const string BossWarning = "boss_warning";
        public const string BeamCharge = "beam_charge";
        public const string BeamFire = "beam_fire";
        public const string OrbFire = "orb_fire";
        public const string Deflect = "deflect";
        public const string BossHit = "boss_hit";
        public const string BossDefeat = "boss_defeat";
        public const string UpgradePick = "upgrade_pick";
        public const string UiClick = "ui_click";
        public const string UiSelect = "ui_select";
        public const string GameOver = "game_over";
        public const string Victory = "victory";
        public const string MultiKill = "multikill";
        public const string Combo = "combo";

        public const string MenuMusic = "music_menu";
        public const string GameMusic = "music_game";
        public const string BossMusic = "music_boss";

        private static readonly Dictionary<string, AudioClip> clips = new Dictionary<string, AudioClip>();

        /// <summary>Plays a sound effect by name. Does nothing if the clip or AudioManager is missing.</summary>
        public static void Play(string soundName, float volume = 1f, float pitchVariance = 0.04f)
        {
            if (!AudioManager.HasInstance) return;
            AudioManager.Instance.PlaySFX(Load(SfxPath, soundName), volume, pitchVariance);
        }

        /// <summary>Plays a sound effect at an exact pitch (1 = normal).</summary>
        public static void PlayPitched(string soundName, float volume, float pitch)
        {
            if (!AudioManager.HasInstance) return;
            AudioManager.Instance.PlaySFXPitched(Load(SfxPath, soundName), volume, pitch);
        }

        /// <summary>Crossfades to a music loop by name.</summary>
        public static void PlayMusic(string musicName, float fadeTime = 1f)
        {
            if (!AudioManager.HasInstance) return;
            AudioManager.Instance.PlayMusic(Load(MusicPath, musicName), fadeTime);
        }

        /// <summary>Makes sure an AudioManager exists (created on first use, kept across scenes).</summary>
        public static void EnsureAudioManager()
        {
            if (AudioManager.HasInstance) return;
            new GameObject("Audio Manager").AddComponent<AudioManager>();
        }

        private static AudioClip Load(string folder, string clipName)
        {
            string key = folder + clipName;
            if (!clips.TryGetValue(key, out AudioClip clip))
            {
                clip = Resources.Load<AudioClip>(key);
                if (clip == null) Debug.LogWarning("GameAudio: missing sound " + key);
                clips[key] = clip;
            }
            return clip;
        }
    }

}
