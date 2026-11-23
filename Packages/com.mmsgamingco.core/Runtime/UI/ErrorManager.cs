using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Com.MMsGamingCo.Core
{

    /// <summary>
    /// Shows short messages ("Error: Failed to save cloud data.") in a popup that
    /// hides itself. Messages queue up instead of overwriting each other.
    /// </summary>
    public class ErrorManager : Singleton<ErrorManager>
    {
        [Tooltip("Root object of the popup. Shown and hidden for each message.")]
        public GameObject popup;
        public TextMeshProUGUI messageText;
        [Tooltip("Seconds each message stays on screen.")]
        public float displayTime = 3f;

        private readonly Queue<string> messages = new Queue<string>();
        private Coroutine showCoroutine;

        protected override void Awake()
        {
            base.Awake();
            if (IsDuplicate) return;

            if (messageText == null) messageText = GetComponentInChildren<TextMeshProUGUI>(true);
            if (popup) popup.SetActive(false);
        }

        /// <summary>Queues a message to show. Safe to call from anywhere, any time.</summary>
        public void StartErrorMessage(string message)
        {
            if (string.IsNullOrEmpty(message)) return;

            messages.Enqueue(message);
            if (showCoroutine == null)
                showCoroutine = StartCoroutine(ShowMessages());
        }

        /// <summary>Shows a message if an ErrorManager exists, otherwise logs it.</summary>
        public static void Show(string message)
        {
            if (HasInstance)
                Instance.StartErrorMessage(message);
            else
                Debug.LogWarning(message);
        }

        private IEnumerator ShowMessages()
        {
            while (messages.Count > 0)
            {
                if (messageText) messageText.text = messages.Dequeue();
                else messages.Dequeue();
                if (popup) popup.SetActive(true);

                // Realtime so popups still close while the game is paused.
                yield return new WaitForSecondsRealtime(displayTime);
            }

            if (popup) popup.SetActive(false);
            showCoroutine = null;
        }
    }

}
