using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

namespace Com.MMsGamingCo.Core
{

    /// <summary>
    /// Persistent audio manager. Owns the master mixer, applies the saved volumes,
    /// plays music with crossfades and plays sound effects from a small pool of sources.
    /// Put it on a prefab in your first scene. Settings UI talks to it through
    /// AudioPlayerSettings, so it no longer searches the scene for sliders every frame.
    /// </summary>
    public class AudioManager : Singleton<AudioManager>
    {

        #region Fields/Variables

        [Header("Mixer")]
        [Tooltip("Mixer with exposed volume parameters for Master, Music and SFX.")]
        public AudioMixer masterMixer;
        [Tooltip("Exposed parameter names on the mixer.")]
        public string masterParameter = "MasterVol";
        public string musicParameter = "MusicVol";
        public string sfxParameter = "SFXVol";

        [Header("Routing")]
        public AudioMixerGroup musicGroup;
        public AudioMixerGroup sfxGroup;

        [Header("Music")]
        [Tooltip("Music that starts playing when the manager loads. Optional.")]
        public AudioClip startingMusic;
        [Tooltip("Default crossfade time in seconds.")]
        public float musicFadeTime = 1f;

        [Header("Sound Effects")]
        [Tooltip("How many sound effects can play at the same time.")]
        [Range(4, 32)]
        public int sfxPoolSize = 12;

        private AudioSource[] musicSources;
        private int activeMusicSource = 0;
        private AudioSource[] sfxSources;
        private int nextSfxSource = 0;
        private Coroutine musicFadeCoroutine;

        public AudioClip CurrentMusic
        {
            get { return musicSources[activeMusicSource].clip; }
        }

        #endregion


        #region MonoBehaviour Methods

        protected override void Awake()
        {
            base.Awake();
            if (IsDuplicate) return;

            musicSources = new AudioSource[2];
            for (int i = 0; i < musicSources.Length; i++)
            {
                musicSources[i] = CreateSource("Music " + i, musicGroup);
                musicSources[i].loop = true;
            }

            sfxSources = new AudioSource[sfxPoolSize];
            for (int i = 0; i < sfxSources.Length; i++)
            {
                sfxSources[i] = CreateSource("SFX " + i, sfxGroup);
            }
        }

        private void Start()
        {
            // AudioMixer.SetFloat is ignored in Awake, so the saved volumes are applied here.
            ApplySavedVolumes();

            if (startingMusic != null)
                PlayMusic(startingMusic, 0f);
        }

        #endregion


        #region Volume

        /// <summary>
        /// Applies the volumes stored in PreferencesManager to the mixer.
        /// </summary>
        public void ApplySavedVolumes()
        {
            SetMixerVolume(masterParameter, PreferencesManager.GetMasterVolume());
            SetMixerVolume(musicParameter, PreferencesManager.GetMusicVolume());
            SetMixerVolume(sfxParameter, PreferencesManager.GetSFXVolume());
        }

        /// <summary>Sets master volume (0-1) and saves it.</summary>
        public void SetMasterVolume(float volume)
        {
            SetMixerVolume(masterParameter, volume);
            PreferencesManager.SetMasterVolume(volume);
        }

        /// <summary>Sets music volume (0-1) and saves it.</summary>
        public void SetMusicVolume(float volume)
        {
            SetMixerVolume(musicParameter, volume);
            PreferencesManager.SetMusicVolume(volume);
        }

        /// <summary>Sets sound effect volume (0-1) and saves it.</summary>
        public void SetSFXVolume(float volume)
        {
            SetMixerVolume(sfxParameter, volume);
            PreferencesManager.SetSFXVolume(volume);
        }

        private void SetMixerVolume(string parameter, float volume)
        {
            if (masterMixer == null || string.IsNullOrEmpty(parameter)) return;

            if (!masterMixer.SetFloat(parameter, AudioMath.LinearToDecibels(volume)))
                Debug.LogWarning("AudioManager: mixer has no exposed parameter named " + parameter);
        }

        #endregion


        #region Music

        /// <summary>
        /// Crossfades to a new music clip. Does nothing if the clip is already playing.
        /// </summary>
        /// <param name="clip">Clip to play.</param>
        /// <param name="fadeTime">Seconds to crossfade. Negative uses musicFadeTime.</param>
        public void PlayMusic(AudioClip clip, float fadeTime = -1f)
        {
            if (clip == null) return;

            AudioSource current = musicSources[activeMusicSource];
            if (current.clip == clip && current.isPlaying) return;

            if (fadeTime < 0) fadeTime = musicFadeTime;

            activeMusicSource = 1 - activeMusicSource;
            AudioSource next = musicSources[activeMusicSource];
            next.clip = clip;
            next.volume = fadeTime > 0 ? 0f : 1f;
            next.Play();

            StartMusicFade(Crossfade(current, next, fadeTime));
        }

        /// <summary>Fades out and stops the current music.</summary>
        public void StopMusic(float fadeTime = -1f)
        {
            if (fadeTime < 0) fadeTime = musicFadeTime;
            StartMusicFade(Crossfade(musicSources[activeMusicSource], null, fadeTime));
        }

        public void PauseMusic(bool paused)
        {
            if (paused)
                musicSources[activeMusicSource].Pause();
            else
                musicSources[activeMusicSource].UnPause();
        }

        private void StartMusicFade(IEnumerator fade)
        {
            if (musicFadeCoroutine != null)
                StopCoroutine(musicFadeCoroutine);
            musicFadeCoroutine = StartCoroutine(fade);
        }

        private IEnumerator Crossfade(AudioSource from, AudioSource to, float fadeTime)
        {
            float fromStart = from != null ? from.volume : 0f;
            float timer = 0f;

            // Unscaled time so music still fades while the game is paused (timeScale 0).
            while (timer < fadeTime)
            {
                timer += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(timer / fadeTime);
                if (from != null) from.volume = Mathf.Lerp(fromStart, 0f, t);
                if (to != null) to.volume = t;
                yield return null;
            }

            if (from != null && from != to)
            {
                from.Stop();
                from.clip = null;
            }
            if (to != null) to.volume = 1f;

            musicFadeCoroutine = null;
        }

        #endregion


        #region Sound Effects

        /// <summary>
        /// Plays a 2D sound effect.
        /// </summary>
        /// <param name="clip">Clip to play.</param>
        /// <param name="volume">Volume scale, 0-1.</param>
        /// <param name="pitchVariance">Random pitch offset (0.1 = +/-10%). Stops repeated sounds feeling robotic.</param>
        public void PlaySFX(AudioClip clip, float volume = 1f, float pitchVariance = 0f)
        {
            if (clip == null) return;

            AudioSource source = NextSfxSource();
            source.spatialBlend = 0f;
            source.pitch = 1f + Random.Range(-pitchVariance, pitchVariance);
            source.PlayOneShot(clip, volume);
        }

        /// <summary>
        /// Plays a 3D sound effect at a world position.
        /// </summary>
        public void PlaySFXAt(AudioClip clip, Vector3 position, float volume = 1f, float pitchVariance = 0f)
        {
            if (clip == null) return;

            AudioSource source = NextSfxSource();
            source.transform.position = position;
            source.spatialBlend = 1f;
            source.pitch = 1f + Random.Range(-pitchVariance, pitchVariance);
            source.PlayOneShot(clip, volume);
        }

        /// <summary>Plays a random clip from the array.</summary>
        public void PlayRandomSFX(AudioClip[] clips, float volume = 1f, float pitchVariance = 0f)
        {
            if (clips == null || clips.Length == 0) return;
            PlaySFX(clips[Random.Range(0, clips.Length)], volume, pitchVariance);
        }

        public void StopAllSFX()
        {
            foreach (AudioSource source in sfxSources)
                source.Stop();
        }

        /// <summary>
        /// Round-robin through the pool, preferring a source that has finished playing.
        /// </summary>
        private AudioSource NextSfxSource()
        {
            for (int i = 0; i < sfxSources.Length; i++)
            {
                int index = (nextSfxSource + i) % sfxSources.Length;
                if (!sfxSources[index].isPlaying)
                {
                    nextSfxSource = (index + 1) % sfxSources.Length;
                    return sfxSources[index];
                }
            }

            // Every source is busy, so cut off the oldest one.
            AudioSource oldest = sfxSources[nextSfxSource];
            oldest.Stop();
            nextSfxSource = (nextSfxSource + 1) % sfxSources.Length;
            return oldest;
        }

        private AudioSource CreateSource(string sourceName, AudioMixerGroup group)
        {
            GameObject child = new GameObject(sourceName);
            child.transform.SetParent(transform, false);

            AudioSource source = child.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.outputAudioMixerGroup = group;
            return source;
        }

        #endregion

    }

}
