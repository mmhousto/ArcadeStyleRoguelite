using System.Collections.Generic;
using Com.MMsGamingCo.Core;
using UnityEngine;

namespace Com.MMsGamingCo.Ricomet
{

    /// <summary>
    /// Owns a PrefabPool per enemy type and spawns enemies by ID.
    /// Enemies are built from code for now, so each pool clones an inactive template.
    /// </summary>
    public class EnemyFactory : MonoBehaviour
    {
        private static EnemyFactory _instance;

        public static EnemyFactory Instance { get { return _instance; } }

        private readonly Dictionary<string, PrefabPool> pools = new Dictionary<string, PrefabPool>();
        private PrototypeTuning tuning;

        private void Awake()
        {
            _instance = this;
        }

        private void OnDestroy()
        {
            if (_instance == this) _instance = null;
        }

        public void Setup(PrototypeTuning newTuning)
        {
            tuning = newTuning;

            CreatePool<DrifterEnemy>(EnemyIds.Drifter, 30, null);
            CreatePool<DrifterEnemy>(EnemyIds.DrifterMini, 16, template =>
            {
                DrifterEnemy mini = template.GetComponent<DrifterEnemy>();
                mini.sizeScale = 0.6f;
                mini.speedScale = tuning.splitterShardSpeed;
            });
            CreatePool<DartEnemy>(EnemyIds.Dart, 10, null);
            CreatePool<SplitterEnemy>(EnemyIds.Splitter, 8, null);
        }

        public bool HasEnemy(string enemyId)
        {
            return pools.ContainsKey(enemyId);
        }

        /// <summary>Spawns an enemy right away. Returns null for an unknown ID.</summary>
        public Enemy Spawn(string enemyId, Vector2 position)
        {
            if (!pools.TryGetValue(enemyId, out PrefabPool pool))
            {
                Debug.LogWarning("EnemyFactory: no enemy with ID " + enemyId);
                return null;
            }

            return pool.Get<Enemy>(Arena.Clamp(position, 0.6f), Quaternion.identity);
        }

        /// <summary>Returns every live enemy to its pool.</summary>
        public void ClearAll()
        {
            foreach (PrefabPool pool in pools.Values)
                pool.ReleaseAll();
        }

        private void CreatePool<T>(string enemyId, int prewarm, System.Action<GameObject> configure) where T : Enemy
        {
            // The template stays inactive so its Awake never runs; the pool clones it.
            GameObject template = new GameObject(enemyId + " Template");
            template.SetActive(false);
            template.transform.SetParent(transform, false);
            template.AddComponent<T>();
            configure?.Invoke(template);

            // Inactive while configuring, because PrefabPool prewarms in Awake.
            GameObject poolObject = new GameObject(enemyId + " Pool");
            poolObject.SetActive(false);
            poolObject.transform.SetParent(transform, false);

            PrefabPool pool = poolObject.AddComponent<PrefabPool>();
            pool.prefab = template;
            pool.prewarmCount = prewarm;
            pool.maxSize = 80;
            poolObject.SetActive(true);

            pools[enemyId] = pool;
        }
    }

}
