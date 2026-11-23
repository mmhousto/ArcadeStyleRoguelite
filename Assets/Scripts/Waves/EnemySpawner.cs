using System.Collections.Generic;
using Com.MMsGamingCo.Core;
using UnityEngine;

namespace Com.MMsGamingCo.Ricomet
{

    /// <summary>
    /// Prototype spawner: endless enemies that ramp up over time. Every spawn is
    /// warned with a shrinking ring first so enemies never pop in on top of the
    /// player unfairly. Replaced by the WaveDirector in the vertical slice.
    /// </summary>
    public class EnemySpawner : MonoBehaviour
    {
        private class PendingSpawn
        {
            public PrefabPool pool;
            public Vector2 position;
            public float timer;
        }

        public float Elapsed { get; private set; }

        private PrototypeTuning tuning;
        private SeededRandom random;
        private PrefabPool drifterPool;
        private PrefabPool dartPool;
        private float spawnTimer = 1f;
        private bool running = false;
        private readonly List<PendingSpawn> pending = new List<PendingSpawn>();

        private void OnEnable()
        {
            CometHealth.cometDied += Stop;
        }

        private void OnDisable()
        {
            CometHealth.cometDied -= Stop;
        }

        /// <summary>Creates the enemy pools and starts spawning.</summary>
        public void Setup(PrototypeTuning newTuning, SeededRandom runRandom)
        {
            tuning = newTuning;
            random = runRandom;
            drifterPool = CreatePool<DrifterEnemy>("Drifters", 30);
            dartPool = CreatePool<DartEnemy>("Darts", 10);
            running = true;
        }

        private void Update()
        {
            if (!running) return;

            Elapsed += Time.deltaTime;
            UpdatePending(Time.deltaTime);

            spawnTimer -= Time.deltaTime;
            if (spawnTimer > 0f) return;

            float ramp = Mathf.Clamp01(Elapsed / tuning.rampSeconds);
            spawnTimer = Mathf.Lerp(tuning.startSpawnInterval, tuning.endSpawnInterval, ramp);

            if (Enemy.Active.Count + pending.Count >= tuning.maxAliveEnemies) return;

            // Later on, spawn small groups instead of single enemies.
            int groupSize = 1 + (random.Chance(ramp * 0.5f) ? random.Range(1, 3) : 0);
            for (int i = 0; i < groupSize; i++)
                QueueSpawn(ramp);
        }

        private void QueueSpawn(float ramp)
        {
            float dartChance = Elapsed < tuning.dartUnlockTime ? 0f : tuning.maxDartChance * ramp;
            PrefabPool pool = random.Chance(dartChance) ? dartPool : drifterPool;

            Vector2 position = FindSpawnPoint();
            pending.Add(new PendingSpawn { pool = pool, position = position, timer = tuning.spawnWarningTime });
            EffectSpawner.SpawnWarning(position, NeonColors.Enemy, tuning.spawnWarningTime);
        }

        private void UpdatePending(float deltaTime)
        {
            for (int i = pending.Count - 1; i >= 0; i--)
            {
                pending[i].timer -= deltaTime;
                if (pending[i].timer > 0f) continue;

                pending[i].pool.Get(pending[i].position, Quaternion.identity);
                pending.RemoveAt(i);
            }
        }

        /// <summary>A random point near the arena edge that isn't too close to the comet.</summary>
        private Vector2 FindSpawnPoint()
        {
            Vector2 comet = CometController.Instance != null ? CometController.Instance.Position : Vector2.zero;
            Vector2 best = Vector2.zero;
            float bestDistance = -1f;

            for (int attempt = 0; attempt < 8; attempt++)
            {
                Vector2 point = Arena.RandomPoint(random, 1f);

                // Pull toward the nearest edge so enemies come in from the sides.
                if (random.Chance(0.7f))
                {
                    if (random.Chance(0.5f)) point.x = Mathf.Sign(point.x) * (Arena.Bounds.width * 0.5f - 1f);
                    else point.y = Mathf.Sign(point.y) * (Arena.Bounds.height * 0.5f - 1f);
                }

                float distance = Vector2.Distance(point, comet);
                if (distance >= tuning.minSpawnDistanceFromComet) return point;
                if (distance > bestDistance)
                {
                    bestDistance = distance;
                    best = point;
                }
            }

            return best;
        }

        private void Stop()
        {
            running = false;
            pending.Clear();
        }

        private PrefabPool CreatePool<T>(string poolName, int prewarm) where T : Enemy
        {
            // The template stays inactive so its Awake never runs; the pool clones it.
            GameObject template = new GameObject(typeof(T).Name + " Template");
            template.SetActive(false);
            template.transform.SetParent(transform, false);
            template.AddComponent<T>();

            // Inactive while configuring, because PrefabPool prewarms in Awake.
            GameObject poolObject = new GameObject(poolName);
            poolObject.SetActive(false);
            poolObject.transform.SetParent(transform, false);

            PrefabPool pool = poolObject.AddComponent<PrefabPool>();
            pool.prefab = template;
            pool.prewarmCount = prewarm;
            pool.maxSize = tuning.maxAliveEnemies;
            poolObject.SetActive(true);
            return pool;
        }
    }

}
