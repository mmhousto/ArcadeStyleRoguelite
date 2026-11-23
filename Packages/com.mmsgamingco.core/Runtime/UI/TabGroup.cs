using System;
using UnityEngine;

namespace Com.MMsGamingCo.Core
{

    /// <summary>
    /// Shows one group of objects at a time, like the Audio / Game / Visual tabs on
    /// ZombCube's settings screen. Hook each tab button up to ShowTab(index).
    /// </summary>
    public class TabGroup : MonoBehaviour
    {
        [Serializable]
        public class Tab
        {
            public string name;
            public GameObject[] objects;
        }

        public Tab[] tabs;
        [Tooltip("Tab shown when this object is enabled.")]
        public int defaultTab = 0;

        public int CurrentTab { get; private set; } = -1;

        private void OnEnable()
        {
            ShowTab(defaultTab);
        }

        public void ShowTab(int index)
        {
            if (tabs == null || index < 0 || index >= tabs.Length) return;

            for (int i = 0; i < tabs.Length; i++)
            {
                foreach (GameObject tabObject in tabs[i].objects)
                {
                    if (tabObject) tabObject.SetActive(i == index);
                }
            }

            CurrentTab = index;
        }
    }

}
