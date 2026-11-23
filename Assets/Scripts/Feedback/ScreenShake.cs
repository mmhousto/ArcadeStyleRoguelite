using UnityEngine;

namespace Com.MMsGamingCo.Ricomet
{

    /// <summary>
    /// Trauma-based camera shake. Add trauma on hits; it decays on its own.
    /// Shake strength is trauma squared, so small hits stay subtle and big ones punch.
    /// </summary>
    public class ScreenShake : MonoBehaviour
    {
        private static ScreenShake _instance;

        public static ScreenShake Instance { get { return _instance; } }

        [Tooltip("Largest offset in world units at full trauma.")]
        public float maxOffset = 0.6f;
        [Tooltip("Largest roll in degrees at full trauma.")]
        public float maxAngle = 2f;
        [Tooltip("Trauma lost per second.")]
        public float decay = 1.8f;
        public float frequency = 25f;

        [Tooltip("Scales every shake. Hook up to a Reduce Screen Shake setting.")]
        [Range(0f, 1f)] public float intensity = 1f;

        private float trauma = 0f;
        private Vector3 basePosition;
        private float seed;

        private void Awake()
        {
            _instance = this;
            basePosition = transform.localPosition;
            seed = Random.value * 100f;
        }

        private void OnDestroy()
        {
            if (_instance == this) _instance = null;
        }

        /// <summary>Adds trauma (0-1). Safe to call when no shake exists.</summary>
        public static void Add(float amount)
        {
            if (_instance != null)
                _instance.trauma = Mathf.Clamp01(_instance.trauma + amount);
        }

        private void LateUpdate()
        {
            // Unscaled, so shake still plays during hit stop and slow motion.
            float time = Time.unscaledTime * frequency;
            float shake = trauma * trauma * intensity;

            Vector3 offset = new Vector3(
                (Mathf.PerlinNoise(seed, time) * 2f - 1f) * maxOffset * shake,
                (Mathf.PerlinNoise(seed + 1f, time) * 2f - 1f) * maxOffset * shake,
                0f);
            float angle = (Mathf.PerlinNoise(seed + 2f, time) * 2f - 1f) * maxAngle * shake;

            transform.localPosition = basePosition + offset;
            transform.localRotation = Quaternion.Euler(0f, 0f, angle);

            trauma = Mathf.Max(0f, trauma - decay * Time.unscaledDeltaTime);
        }
    }

}
