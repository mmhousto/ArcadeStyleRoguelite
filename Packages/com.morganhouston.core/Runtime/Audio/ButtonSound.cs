using UnityEngine;
using UnityEngine.UI;

namespace Com.MorganHouston.Core
{

    /// <summary>
    /// Plays a click sound through the AudioManager when the button is pressed.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class ButtonSound : MonoBehaviour
    {
        public AudioClip clickSound;
        [Range(0f, 1f)] public float volume = 1f;
        [Range(0f, 0.5f)] public float pitchVariance = 0.05f;

        private Button button;

        private void Awake()
        {
            button = GetComponent<Button>();
        }

        private void OnEnable()
        {
            button.onClick.AddListener(PlayClick);
        }

        private void OnDisable()
        {
            button.onClick.RemoveListener(PlayClick);
        }

        private void PlayClick()
        {
            if (AudioManager.HasInstance)
                AudioManager.Instance.PlaySFX(clickSound, volume, pitchVariance);
        }
    }

}
