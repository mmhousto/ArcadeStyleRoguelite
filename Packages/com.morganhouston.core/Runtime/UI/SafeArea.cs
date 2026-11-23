using UnityEngine;

namespace Com.MorganHouston.Core
{

    /// <summary>
    /// Fits a RectTransform inside Screen.safeArea so UI stays clear of the iPhone
    /// notch, Dynamic Island and home indicator. Put it on a full-screen child of
    /// the Canvas (ZombCube's "SafeArea" object) and parent the UI under it.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class SafeArea : MonoBehaviour
    {
        [Tooltip("Turn off an edge to let that side bleed under the notch or home bar.")]
        public bool conformLeft = true, conformRight = true, conformTop = true, conformBottom = true;

        private RectTransform rectTransform;
        private Rect lastSafeArea = Rect.zero;
        private Vector2Int lastScreenSize = Vector2Int.zero;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            ApplySafeArea();
        }

        private void Update()
        {
            // Cheap checks; the layout is only rebuilt when something actually changes.
            if (Screen.safeArea != lastSafeArea || Screen.width != lastScreenSize.x || Screen.height != lastScreenSize.y)
                ApplySafeArea();
        }

        private void ApplySafeArea()
        {
            Rect safeArea = Screen.safeArea;
            lastSafeArea = safeArea;
            lastScreenSize = new Vector2Int(Screen.width, Screen.height);

            if (Screen.width <= 0 || Screen.height <= 0) return;

            Vector2 anchorMin = safeArea.position;
            Vector2 anchorMax = safeArea.position + safeArea.size;
            anchorMin.x /= Screen.width;
            anchorMin.y /= Screen.height;
            anchorMax.x /= Screen.width;
            anchorMax.y /= Screen.height;

            if (!conformLeft) anchorMin.x = 0;
            if (!conformBottom) anchorMin.y = 0;
            if (!conformRight) anchorMax.x = 1;
            if (!conformTop) anchorMax.y = 1;

            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }
    }

}
