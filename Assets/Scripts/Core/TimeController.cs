using UnityEngine;

namespace Com.MMsGamingCo.Ricomet
{

    /// <summary>
    /// The only script that changes Time.timeScale. Combines aim slow motion, hit
    /// stop and pausing so they never fight over the value.
    /// </summary>
    public class TimeController : MonoBehaviour
    {
        private static TimeController _instance;

        public static TimeController Instance { get { return _instance; } }

        private const float BaseFixedDeltaTime = 0.02f;
        private const float HitStopScale = 0.02f;

        private float aimScale = 1f;
        private float hitStopTimer = 0f;
        private bool paused = false;

        public bool IsPaused { get { return paused; } }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(this.gameObject);
                return;
            }
            _instance = this;
        }

        private void OnDestroy()
        {
            if (_instance != this) return;

            _instance = null;
            Time.timeScale = 1f;
            Time.fixedDeltaTime = BaseFixedDeltaTime;
        }

        private void Update()
        {
            // Hit stop counts down in real time, since game time is nearly frozen.
            if (hitStopTimer > 0f)
                hitStopTimer -= Time.unscaledDeltaTime;

            Apply();
        }

        /// <summary>Sets the slow-motion scale used while aiming. 1 = normal speed.</summary>
        public void SetAimScale(float scale)
        {
            aimScale = Mathf.Clamp(scale, 0.01f, 1f);
            Apply();
        }

        /// <summary>Briefly freezes the game to make a hit land harder.</summary>
        public void HitStop(float seconds)
        {
            hitStopTimer = Mathf.Max(hitStopTimer, seconds);
            Apply();
        }

        public void SetPaused(bool isPaused)
        {
            paused = isPaused;
            Apply();
        }

        private void Apply()
        {
            float scale = aimScale;
            if (hitStopTimer > 0f) scale = Mathf.Min(scale, HitStopScale);
            if (paused) scale = 0f;

            Time.timeScale = scale;
            // Keep physics steps smooth in slow motion.
            Time.fixedDeltaTime = BaseFixedDeltaTime * Mathf.Max(scale, 0.05f);
        }
    }

}
