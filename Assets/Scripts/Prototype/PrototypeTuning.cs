using UnityEngine;

namespace Com.MMsGamingCo.Ricomet
{

    /// <summary>
    /// Every number the prototype uses, in one asset so the feel can be tuned in
    /// play mode. Create one with Assets > Create > Ricomet > Prototype Tuning and
    /// put it in a Resources folder. Without an asset the defaults below are used.
    /// </summary>
    [CreateAssetMenu(fileName = "PrototypeTuning", menuName = "Ricomet/Prototype Tuning")]
    public class PrototypeTuning : ScriptableObject
    {
        public const string ResourcePath = "PrototypeTuning";

        private static PrototypeTuning instance;

        public static PrototypeTuning Instance
        {
            get
            {
                if (instance == null)
                    instance = Resources.Load<PrototypeTuning>(ResourcePath);
                if (instance == null)
                    instance = CreateInstance<PrototypeTuning>();
                return instance;
            }
        }

        /// <summary>Makes every script use this asset, e.g. one assigned on the bootstrap.</summary>
        public static void Use(PrototypeTuning tuning)
        {
            if (tuning != null) instance = tuning;
        }

        [Header("Arena")]
        [Tooltip("Arena size in world units. 16:9-ish so it fills landscape screens.")]
        public Vector2 arenaSize = new Vector2(30f, 16f);

        [Header("Aim")]
        [Tooltip("Drag distance for a full-power dash, as a fraction of screen height.")]
        public float fullDragScreenFraction = 0.3f;
        [Tooltip("Drags shorter than this fraction of a full drag cancel the dash.")]
        public float cancelDeadZone = 0.12f;
        [Tooltip("Weakest dash, as a fraction of full distance.")]
        public float minDashPower = 0.3f;
        [Tooltip("Time scale while aiming.")]
        [Range(0.05f, 1f)] public float aimTimeScale = 0.25f;

        [Header("Dash")]
        public float dashDistance = 11f;
        public float dashSpeed = 48f;
        public float dashDamage = 1f;
        [Tooltip("Collision radius of the comet.")]
        public float cometRadius = 0.45f;
        [Tooltip("Bounce off walls instead of stopping. An upgrade in the full game; on here to test feel.")]
        public bool bounceOffWalls = false;
        public int maxWallBounces = 1;

        [Header("Energy")]
        public float maxCharges = 3f;
        public float killRefund = 0.35f;
        [Tooltip("Seconds to regain one charge while stopped.")]
        public float secondsPerCharge = 2.5f;

        [Header("Focus")]
        [Tooltip("Real seconds of slow-motion aiming when full.")]
        public float focusSeconds = 2.5f;
        public float focusRefillPerSecond = 0.6f;
        public float focusPerKill = 0.25f;
        [Tooltip("Turn off to test unlimited slow motion.")]
        public bool limitFocus = true;

        [Header("Health")]
        public int maxShields = 3;
        public float invulnerableAfterHit = 1f;
        [Tooltip("Grace period after a dash ends before contact damage applies.")]
        public float postDashGrace = 0.12f;

        [Header("Combo")]
        public float comboWindow = 1.5f;
        public float maxComboMultiplier = 5f;
        public int pointsPerKill = 100;

        [Header("Feedback")]
        public float hitStopSeconds = 0.035f;
        public float multiKillHitStopSeconds = 0.08f;
        public float killShake = 0.15f;
        public float damageShake = 0.6f;

        [Header("Spawning")]
        [Tooltip("Spawns wait while this many enemies are alive.")]
        public int maxAliveEnemies = 45;
        public float spawnWarningTime = 0.7f;
        public float minSpawnDistanceFromComet = 6f;

        [Header("Drifter")]
        public float drifterSpeed = 2.4f;
        public float drifterRadius = 0.5f;

        [Header("Splitter")]
        public float splitterSpeed = 1.5f;
        public float splitterRadius = 0.7f;
        [Tooltip("Speed multiplier for the small Drifters a Splitter bursts into.")]
        public float splitterShardSpeed = 1.6f;

        [Header("Waves")]
        [Tooltip("Seconds the wave banner shows before enemies spawn.")]
        public float waveIntroTime = 1.4f;
        [Tooltip("Pause after the last enemy dies before the upgrade picker opens.")]
        public float waveClearDelay = 0.8f;
        [Tooltip("Points per second under par time when a wave is cleared.")]
        public int timeBonusPerSecond = 50;
        public int upgradeChoices = 3;
        [Tooltip("Free rerolls of the upgrade offer per sector.")]
        public int rerollsPerSector = 1;

        [Header("Dart")]
        public float dartApproachSpeed = 1.6f;
        public float dartChargeRange = 8f;
        public float dartWindupTime = 0.7f;
        public float dartChargeSpeed = 17f;
        public float dartChargeTime = 0.55f;
        public float dartRecoverTime = 1.1f;
        public float dartRadius = 0.45f;
    }

}
