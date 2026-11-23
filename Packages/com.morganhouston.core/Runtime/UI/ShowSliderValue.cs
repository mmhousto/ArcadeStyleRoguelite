using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Com.MorganHouston.Core
{

    /// <summary>
    /// Shows a slider's value in a label, e.g. "75%" for a 0-1 volume slider.
    /// </summary>
    [RequireComponent(typeof(Slider))]
    public class ShowSliderValue : MonoBehaviour
    {
        public TextMeshProUGUI sliderValueLabel;
        [Tooltip("Shows 0-1 sliders as 0-100%.")]
        public bool showAsPercent = true;
        [Tooltip("Format string for non-percent values, e.g. \"0.0\".")]
        public string numberFormat = "0";

        private Slider slider;

        private void Awake()
        {
            slider = GetComponent<Slider>();
        }

        private void OnEnable()
        {
            slider.onValueChanged.AddListener(UpdateLabel);
            UpdateLabel(slider.value);
        }

        private void OnDisable()
        {
            slider.onValueChanged.RemoveListener(UpdateLabel);
        }

        private void UpdateLabel(float value)
        {
            if (sliderValueLabel == null) return;

            if (showAsPercent)
                sliderValueLabel.text = Mathf.RoundToInt(value * 100f) + "%";
            else
                sliderValueLabel.text = value.ToString(numberFormat);
        }
    }

}
