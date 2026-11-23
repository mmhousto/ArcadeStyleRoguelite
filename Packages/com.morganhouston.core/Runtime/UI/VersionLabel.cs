using TMPro;
using UnityEngine;

namespace Com.MMsGamingCo.Core
{

    /// <summary>
    /// Writes the app version into a TextMeshPro label.
    /// </summary>
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class VersionLabel : MonoBehaviour
    {
        public string prefix = "v";

        private void Start()
        {
            GetComponent<TextMeshProUGUI>().text = prefix + Application.version;
        }
    }

}
