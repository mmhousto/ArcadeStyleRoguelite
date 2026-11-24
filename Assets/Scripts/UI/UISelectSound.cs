using UnityEngine;
using UnityEngine.EventSystems;

namespace Com.MMsGamingCo.Ricomet
{

    /// <summary>
    /// Plays a soft tick when a gamepad or keyboard moves the selection onto this control.
    /// </summary>
    public class UISelectSound : MonoBehaviour, ISelectHandler
    {
        public void OnSelect(BaseEventData eventData)
        {
            // Pointer clicks also select; only play for navigation.
            if (eventData is PointerEventData) return;
            GameAudio.Play(GameAudio.UiSelect, 0.4f, 0f);
        }
    }

}
