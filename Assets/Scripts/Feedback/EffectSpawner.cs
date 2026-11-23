using System.Collections.Generic;
using UnityEngine;

namespace Com.MMsGamingCo.Ricomet
{

    /// <summary>
    /// Reuses BurstEffects instead of creating new ones for every kill.
    /// </summary>
    public class EffectSpawner : MonoBehaviour
    {
        private static EffectSpawner _instance;

        public static EffectSpawner Instance { get { return _instance; } }

        public int prewarmCount = 20;

        private readonly List<BurstEffect> bursts = new List<BurstEffect>();
        private readonly List<LightningEffect> bolts = new List<LightningEffect>();

        private void Awake()
        {
            _instance = this;
            for (int i = 0; i < prewarmCount; i++)
                CreateBurst();
        }

        private void OnDestroy()
        {
            if (_instance == this) _instance = null;
        }

        /// <summary>Kill burst: ring grows and fades, sparks fly out.</summary>
        public static void Kill(Vector2 position, Color color)
        {
            if (_instance != null)
                _instance.GetBurst().Play(position, color, 0.3f, 1.6f, 0.35f, true);
        }

        /// <summary>Ring where the comet stops, out to the knockback radius.</summary>
        public static void Landing(Vector2 position, float radius)
        {
            if (_instance != null)
                _instance.GetBurst().Play(position, NeonColors.WithAlpha(NeonColors.Player, 0.7f), 0.4f, radius, 0.22f, false);
        }

        /// <summary>Shrinking ring that warns where an enemy is about to appear.</summary>
        public static void SpawnWarning(Vector2 position, Color color, float seconds)
        {
            if (_instance != null)
                _instance.GetBurst().Play(position, color, 1.4f, 0.3f, seconds, false, true);
        }

        /// <summary>Shockwave upgrade: a fast ring out to the blast radius.</summary>
        public static void Shockwave(Vector2 position, float radius)
        {
            if (_instance != null)
                _instance.GetBurst().Play(position, NeonColors.Player, 0.3f, radius, 0.3f, true);
        }

        /// <summary>Jagged bolt between two points.</summary>
        public static void Lightning(Vector2 from, Vector2 to, Color color)
        {
            if (_instance != null)
                _instance.GetBolt().Play(from, to, color);
        }

        /// <summary>Big burst when the comet takes damage.</summary>
        public static void Damage(Vector2 position)
        {
            if (_instance != null)
                _instance.GetBurst().Play(position, NeonColors.Danger, 0.5f, 3.5f, 0.5f, true);
        }

        private BurstEffect GetBurst()
        {
            foreach (BurstEffect burst in bursts)
            {
                if (!burst.IsPlaying) return burst;
            }
            return CreateBurst();
        }

        private LightningEffect GetBolt()
        {
            foreach (LightningEffect bolt in bolts)
            {
                if (!bolt.IsPlaying) return bolt;
            }

            GameObject boltObject = new GameObject("Bolt");
            boltObject.transform.SetParent(transform, false);
            LightningEffect created = boltObject.AddComponent<LightningEffect>();
            boltObject.SetActive(false);
            bolts.Add(created);
            return created;
        }

        private BurstEffect CreateBurst()
        {
            GameObject burstObject = new GameObject("Burst");
            burstObject.transform.SetParent(transform, false);
            BurstEffect burst = burstObject.AddComponent<BurstEffect>();
            burstObject.SetActive(false);
            bursts.Add(burst);
            return burst;
        }
    }

}
