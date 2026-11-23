using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Com.MMsGamingCo.Core
{

    /// <summary>
    /// Goes in the loading scene. Loads SceneLoader.SceneToLoad asynchronously and
    /// shows progress. Every UI field is optional.
    /// </summary>
    public class LoadingScreen : MonoBehaviour
    {
        public Slider progressSlider;
        public TextMeshProUGUI progressText;
        public TextMeshProUGUI loadingText;

        [Tooltip("Keeps the loading screen up at least this long so it doesn't just flash.")]
        public float minimumDisplayTime = 0.5f;

        private int dots = 0;

        private void Start()
        {
            if (loadingText)
                InvokeRepeating(nameof(UpdateLoadingText), 0f, 0.5f);

            StartCoroutine(LoadAsynchronously());
        }

        private IEnumerator LoadAsynchronously()
        {
            AsyncOperation operation = SceneLoader.BeginAsyncLoad();
            operation.allowSceneActivation = false;

            float timer = 0f;

            // Progress stops at 0.9 until allowSceneActivation is true.
            while (operation.progress < 0.9f || timer < minimumDisplayTime)
            {
                timer += Time.unscaledDeltaTime;
                SetProgress(Mathf.Clamp01(operation.progress / 0.9f));
                yield return null;
            }

            SetProgress(1f);
            // This scene unloads once the target activates; SceneLoader raises sceneLoadFinished.
            operation.allowSceneActivation = true;
        }

        private void SetProgress(float progress)
        {
            if (progressSlider) progressSlider.value = progress;
            if (progressText) progressText.text = Mathf.RoundToInt(progress * 100f) + "%";
        }

        private void UpdateLoadingText()
        {
            dots = (dots % 3) + 1;
            loadingText.text = "Loading" + new string('.', dots);
        }
    }

}
