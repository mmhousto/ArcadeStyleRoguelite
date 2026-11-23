using UnityEngine;

namespace Com.MorganHouston.Core
{

    /// <summary>
    /// Small helpers for platform and network checks used all over a project.
    /// </summary>
    public static class AppInfo
    {

        public static bool IsMobile
        {
            get
            {
#if (UNITY_IOS || UNITY_ANDROID)
                return true;
#else
                return Application.isMobilePlatform;
#endif
            }
        }

        public static bool IsIOS
        {
            get
            {
#if UNITY_IOS
                return true;
#else
                return false;
#endif
            }
        }

        /// <summary>
        /// Quick check for a network connection. It only reports whether the device
        /// has a network, not whether a server can be reached.
        /// </summary>
        public static bool HasInternet
        {
            get { return Application.internetReachability != NetworkReachability.NotReachable; }
        }

        public static string Version { get { return Application.version; } }

    }

}
