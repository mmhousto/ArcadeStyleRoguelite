using System.Collections.Generic;
using Com.MMsGamingCo.Core;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace Com.MMsGamingCo.Ricomet
{

    /// <summary>
    /// Builds neon uGUI from code: canvases, panels, text, buttons, sliders and toggles.
    /// Layout is in a 1920x1080 reference space that scales to the screen.
    /// Decorative graphics never block touches, so the player can aim through the HUD.
    /// </summary>
    public static class UIFactory
    {
        public static readonly Vector2 ReferenceResolution = new Vector2(1920f, 1080f);

        private static readonly List<RaycastResult> raycastResults = new List<RaycastResult>();

        #region Canvas

        /// <summary>A screen-space canvas that scales with screen height.</summary>
        public static Canvas CreateCanvas(string canvasName, int sortingOrder, Transform parent = null)
        {
            EnsureEventSystem();

            GameObject canvasObject = new GameObject(canvasName);
            if (parent != null) canvasObject.transform.SetParent(parent, false);

            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortingOrder;

            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = ReferenceResolution;
            scaler.matchWidthOrHeight = 1f;

            canvasObject.AddComponent<GraphicRaycaster>();
            return canvas;
        }

        /// <summary>A full-screen child kept inside the iPhone safe area (notch, home bar).</summary>
        public static RectTransform CreateSafeArea(Transform canvas)
        {
            RectTransform area = CreateRect("Safe Area", canvas);
            Stretch(area);
            area.gameObject.AddComponent<SafeArea>();
            return area;
        }

        /// <summary>Creates an EventSystem using the Input System if the scene has none.</summary>
        public static void EnsureEventSystem()
        {
            if (EventSystem.current != null || Object.FindAnyObjectByType<EventSystem>() != null) return;

            GameObject eventSystemObject = new GameObject("Event System");
            eventSystemObject.AddComponent<EventSystem>();
            InputSystemUIInputModule module = eventSystemObject.AddComponent<InputSystemUIInputModule>();
            module.AssignDefaultActions();
        }

        /// <summary>True if a screen position is over something interactive on any canvas.</summary>
        public static bool IsPointerOverUI(Vector2 screenPosition)
        {
            EventSystem eventSystem = EventSystem.current;
            if (eventSystem == null) return false;

            var pointer = new PointerEventData(eventSystem) { position = screenPosition };
            raycastResults.Clear();
            eventSystem.RaycastAll(pointer, raycastResults);
            return raycastResults.Count > 0;
        }

        #endregion

        #region Layout

        public static RectTransform CreateRect(string rectName, Transform parent)
        {
            GameObject rectObject = new GameObject(rectName, typeof(RectTransform));
            rectObject.transform.SetParent(parent, false);
            return (RectTransform)rectObject.transform;
        }

        /// <summary>Fills the parent.</summary>
        public static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        /// <summary>Anchors at a point of the parent (0-1), with a pivot, position and size.</summary>
        public static void Place(RectTransform rect, Vector2 anchor, Vector2 pivot, Vector2 position, Vector2 size)
        {
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = pivot;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }

        public static VerticalLayoutGroup AddVerticalLayout(RectTransform rect, float spacing, TextAnchor alignment = TextAnchor.UpperCenter)
        {
            VerticalLayoutGroup layout = rect.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = spacing;
            layout.childAlignment = alignment;
            layout.childControlWidth = true;
            // Rows take their height from LayoutElement (SetLayoutHeight). Without this every row
            // falls back to 100 units and the list overlaps whatever sits below it.
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            return layout;
        }

        public static HorizontalLayoutGroup AddHorizontalLayout(RectTransform rect, float spacing, TextAnchor alignment = TextAnchor.MiddleCenter)
        {
            HorizontalLayoutGroup layout = rect.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = spacing;
            layout.childAlignment = alignment;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            return layout;
        }

        /// <summary>Gives a layout child a fixed height.</summary>
        public static void SetLayoutHeight(Component component, float height)
        {
            LayoutElement element = component.GetComponent<LayoutElement>() ?? component.gameObject.AddComponent<LayoutElement>();
            element.preferredHeight = height;
            element.minHeight = height;
        }

        #endregion

        #region Graphics

        /// <summary>Text in the theme's TextMeshPro font, with an optional neon glow. Never blocks touches.</summary>
        public static TextMeshProUGUI CreateText(Transform parent, string content, int fontSize, Color color, TextAnchor alignment, bool bold = true, bool glow = true)
        {
            RectTransform rect = CreateRect("Text", parent);
            TextMeshProUGUI text = rect.gameObject.AddComponent<TextMeshProUGUI>();
            TMP_FontAsset font = bold ? UITheme.BoldFont : UITheme.MediumFont;
            if (font != null) text.font = font;
            text.text = content;
            text.fontSize = fontSize;
            text.color = color;
            text.alignment = ToTextAlignment(alignment);
            text.textWrappingMode = TextWrappingModes.Normal;
            text.overflowMode = TextOverflowModes.Overflow;
            text.raycastTarget = false;

            if (glow)
            {
                Material glowMaterial = UITheme.GlowMaterial(font, color);
                if (glowMaterial != null) text.fontSharedMaterial = glowMaterial;
            }
            return text;
        }

        /// <summary>uGUI-style anchor to the TextMeshPro equivalent.</summary>
        public static TextAlignmentOptions ToTextAlignment(TextAnchor anchor)
        {
            switch (anchor)
            {
                case TextAnchor.UpperLeft: return TextAlignmentOptions.TopLeft;
                case TextAnchor.UpperCenter: return TextAlignmentOptions.Top;
                case TextAnchor.UpperRight: return TextAlignmentOptions.TopRight;
                case TextAnchor.MiddleLeft: return TextAlignmentOptions.Left;
                case TextAnchor.MiddleRight: return TextAlignmentOptions.Right;
                case TextAnchor.LowerLeft: return TextAlignmentOptions.BottomLeft;
                case TextAnchor.LowerCenter: return TextAlignmentOptions.Bottom;
                case TextAnchor.LowerRight: return TextAlignmentOptions.BottomRight;
                default: return TextAlignmentOptions.Center;
            }
        }

        /// <summary>An image. Pass raycastTarget true only for things that should block touches.</summary>
        public static Image CreateImage(Transform parent, Sprite sprite, Color color, bool raycastTarget = false)
        {
            RectTransform rect = CreateRect("Image", parent);
            Image image = rect.gameObject.AddComponent<Image>();
            image.sprite = sprite;
            image.color = color;
            image.raycastTarget = raycastTarget;
            if (sprite != null && sprite.border != Vector4.zero) image.type = Image.Type.Sliced;
            return image;
        }

        /// <summary>A rounded neon panel: dark fill with a glowing frame. Returns the fill image.</summary>
        public static Image CreatePanel(Transform parent, Color frameColor, Color fillColor, bool blocksTouches = false)
        {
            Image fill = CreateImage(parent, SpriteLibrary.UI("ui_fill"), fillColor, blocksTouches);
            fill.name = "Panel";
            if (fill.sprite == null) fill.color = fillColor;

            Image frame = CreateImage(fill.transform, SpriteLibrary.UI("ui_frame"), frameColor);
            frame.name = "Frame";
            Stretch(frame.rectTransform);
            if (frame.sprite == null) frame.enabled = false;
            return fill;
        }

        /// <summary>A dark full-screen backdrop that blocks touches to the game behind a menu.</summary>
        public static Image CreateBackdrop(Transform parent)
        {
            Image backdrop = CreateImage(parent, null, UITheme.Backdrop, true);
            backdrop.name = "Backdrop";
            Stretch(backdrop.rectTransform);
            return backdrop;
        }

        #endregion

        #region Controls

        /// <summary>A neon button. Clicks play the UI click sound.</summary>
        public static Button CreateButton(Transform parent, string label, Color color, UnityAction onClick, int fontSize = 44)
        {
            Image fill = CreatePanel(parent, color, UITheme.PanelFill, true);
            fill.name = "Button " + label;

            Button button = fill.gameObject.AddComponent<Button>();
            button.targetGraphic = fill;
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.4f, 1.4f, 1.6f);
            colors.selectedColor = new Color(1.6f, 1.6f, 1.9f);
            colors.pressedColor = new Color(0.7f, 0.7f, 0.8f);
            colors.colorMultiplier = 2f;
            button.colors = colors;

            TextMeshProUGUI text = CreateText(fill.transform, label, fontSize, color, TextAnchor.MiddleCenter);
            Stretch(text.rectTransform);

            button.onClick.AddListener(() => GameAudio.Play(GameAudio.UiClick, 0.6f, 0f));
            if (onClick != null) button.onClick.AddListener(onClick);

            // Sound when a gamepad/keyboard moves the selection here.
            fill.gameObject.AddComponent<UISelectSound>();
            return button;
        }

        /// <summary>A labeled 0-1 slider.</summary>
        public static Slider CreateSlider(Transform parent, string label, float value, UnityAction<float> onChanged)
        {
            RectTransform row = CreateRect("Slider " + label, parent);
            SetLayoutHeight(row, 76f);

            TextMeshProUGUI text = CreateText(row, label, 34, UITheme.White, TextAnchor.MiddleLeft, false, false);
            text.rectTransform.anchorMin = new Vector2(0f, 0f);
            text.rectTransform.anchorMax = new Vector2(0.38f, 1f);
            text.rectTransform.offsetMin = Vector2.zero;
            text.rectTransform.offsetMax = Vector2.zero;

            RectTransform track = CreateRect("Track", row);
            track.anchorMin = new Vector2(0.42f, 0.35f);
            track.anchorMax = new Vector2(1f, 0.65f);
            track.offsetMin = Vector2.zero;
            track.offsetMax = Vector2.zero;

            Image background = CreateImage(track, SpriteLibrary.UI("ui_fill"), new Color(1f, 1f, 1f, 0.15f), true);
            Stretch(background.rectTransform);

            RectTransform fillArea = CreateRect("Fill Area", track);
            Stretch(fillArea);
            Image fill = CreateImage(fillArea, SpriteLibrary.UI("ui_fill"), UITheme.Cyan);
            Stretch(fill.rectTransform);

            RectTransform handleArea = CreateRect("Handle Area", track);
            Stretch(handleArea);
            Image handle = CreateImage(handleArea, SpriteLibrary.UI("ui_fill"), UITheme.White, true);
            handle.rectTransform.sizeDelta = new Vector2(44f, 0f);
            handle.rectTransform.anchorMin = new Vector2(0f, -0.6f);
            handle.rectTransform.anchorMax = new Vector2(0f, 1.6f);

            Slider slider = row.gameObject.AddComponent<Slider>();
            slider.fillRect = fill.rectTransform;
            slider.handleRect = handle.rectTransform;
            slider.targetGraphic = handle;
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.SetValueWithoutNotify(value);
            if (onChanged != null) slider.onValueChanged.AddListener(onChanged);
            row.gameObject.AddComponent<UISelectSound>();
            return slider;
        }

        /// <summary>A labeled on/off toggle.</summary>
        public static Toggle CreateToggle(Transform parent, string label, bool value, UnityAction<bool> onChanged)
        {
            RectTransform row = CreateRect("Toggle " + label, parent);
            SetLayoutHeight(row, 76f);

            Image box = CreatePanel(row, UITheme.Cyan, UITheme.PanelFill, true);
            box.rectTransform.anchorMin = new Vector2(0f, 0.5f);
            box.rectTransform.anchorMax = new Vector2(0f, 0.5f);
            box.rectTransform.pivot = new Vector2(0f, 0.5f);
            box.rectTransform.sizeDelta = new Vector2(64f, 64f);
            box.rectTransform.anchoredPosition = Vector2.zero;

            Image check = CreateImage(box.transform, SpriteLibrary.UI("ui_fill"), UITheme.Cyan);
            check.rectTransform.anchorMin = new Vector2(0.22f, 0.22f);
            check.rectTransform.anchorMax = new Vector2(0.78f, 0.78f);
            check.rectTransform.offsetMin = Vector2.zero;
            check.rectTransform.offsetMax = Vector2.zero;

            TextMeshProUGUI text = CreateText(row, label, 34, UITheme.White, TextAnchor.MiddleLeft, false, false);
            text.rectTransform.anchorMin = Vector2.zero;
            text.rectTransform.anchorMax = Vector2.one;
            text.rectTransform.offsetMin = new Vector2(90f, 0f);
            text.rectTransform.offsetMax = Vector2.zero;

            Toggle toggle = row.gameObject.AddComponent<Toggle>();
            toggle.targetGraphic = box;
            toggle.graphic = check;
            toggle.SetIsOnWithoutNotify(value);
            toggle.onValueChanged.AddListener(isOn => GameAudio.Play(GameAudio.UiClick, 0.6f, 0f));
            if (onChanged != null) toggle.onValueChanged.AddListener(onChanged);
            row.gameObject.AddComponent<UISelectSound>();
            return toggle;
        }

        /// <summary>Selects a control for gamepad/keyboard navigation.</summary>
        public static void Select(Selectable selectable)
        {
            if (selectable == null || EventSystem.current == null) return;
            EventSystem.current.SetSelectedGameObject(selectable.gameObject);
        }

        #endregion
    }

}
