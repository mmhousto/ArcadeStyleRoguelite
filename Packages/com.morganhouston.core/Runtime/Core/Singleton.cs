using UnityEngine;

namespace Com.MMsGamingCo.Core
{

    /// <summary>
    /// Base class for scene singletons. Keeps the first instance and destroys any
    /// duplicates, the same way every manager in ZombCube does it by hand.
    /// Override Awake? Call base.Awake() first and return if IsDuplicate is true.
    /// </summary>
    public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T>
    {
        private static T _instance;

        public static T Instance { get { return _instance; } }

        /// <summary>True once an instance exists in the scene.</summary>
        public static bool HasInstance { get { return _instance != null; } }

        [Tooltip("Keep this object alive between scene loads.")]
        [SerializeField] protected bool persistAcrossScenes = true;

        /// <summary>True on a copy that is being destroyed because an instance already exists.</summary>
        protected bool IsDuplicate { get; private set; }

        protected virtual void Awake()
        {
            if (_instance != null && _instance != this)
            {
                IsDuplicate = true;
                Destroy(this.gameObject);
                return;
            }

            _instance = (T)this;

            if (persistAcrossScenes)
            {
                // DontDestroyOnLoad only works on root objects.
                transform.SetParent(null);
                DontDestroyOnLoad(this.gameObject);
            }
        }

        protected virtual void OnDestroy()
        {
            if (_instance == this)
                _instance = null;
        }

    }

}
