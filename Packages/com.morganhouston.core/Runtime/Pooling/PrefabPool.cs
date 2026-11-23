using UnityEngine;
using UnityEngine.Pool;

namespace Com.MorganHouston.Core
{

    /// <summary>
    /// Inspector-friendly pool for one prefab, built on Unity's ObjectPool.
    /// Use it for anything spawned a lot (bullets, enemies, pickups, particles) so
    /// mobile builds don't stutter from Instantiate/Destroy and garbage collection.
    /// Objects can implement IPoolable to reset themselves.
    /// </summary>
    public class PrefabPool : MonoBehaviour
    {
        public GameObject prefab;
        [Tooltip("Objects created up front so the first spawns don't hitch.")]
        public int prewarmCount = 10;
        [Tooltip("Most inactive objects kept. Extras are destroyed when released.")]
        public int maxSize = 100;

        private ObjectPool<GameObject> pool;

        public int CountActive { get { return pool != null ? pool.CountActive : 0; } }
        public int CountInactive { get { return pool != null ? pool.CountInactive : 0; } }

        private void Awake()
        {
            pool = new ObjectPool<GameObject>(CreateObject, OnGet, OnRelease, OnDestroyObject, true, prewarmCount, maxSize);
            Prewarm(prewarmCount);
        }

        /// <summary>Takes an object from the pool and places it.</summary>
        public GameObject Get(Vector3 position, Quaternion rotation)
        {
            GameObject pooledObject = pool.Get();
            pooledObject.transform.SetPositionAndRotation(position, rotation);
            NotifySpawned(pooledObject);
            return pooledObject;
        }

        public GameObject Get()
        {
            return Get(transform.position, Quaternion.identity);
        }

        /// <summary>Takes an object and returns the component T from it.</summary>
        public T Get<T>(Vector3 position, Quaternion rotation) where T : Component
        {
            return Get(position, rotation).GetComponent<T>();
        }

        /// <summary>Returns an object to the pool. Call this instead of Destroy.</summary>
        public void Release(GameObject pooledObject)
        {
            if (pooledObject == null || !pooledObject.activeSelf) return;
            pool.Release(pooledObject);
        }

        /// <summary>Releases every active child the pool spawned. Handy between waves or runs.</summary>
        public void ReleaseAll()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                GameObject child = transform.GetChild(i).gameObject;
                if (child.activeSelf) Release(child);
            }
        }

        private void Prewarm(int count)
        {
            var created = new GameObject[count];
            for (int i = 0; i < count; i++)
                created[i] = pool.Get();
            for (int i = 0; i < count; i++)
                pool.Release(created[i]);
        }

        private GameObject CreateObject()
        {
            GameObject pooledObject = Instantiate(prefab, transform);
            PooledObject marker = pooledObject.GetComponent<PooledObject>();
            if (marker == null) marker = pooledObject.AddComponent<PooledObject>();
            marker.Pool = this;
            return pooledObject;
        }

        private void OnGet(GameObject pooledObject)
        {
            pooledObject.SetActive(true);
        }

        private void OnRelease(GameObject pooledObject)
        {
            foreach (IPoolable poolable in pooledObject.GetComponents<IPoolable>())
                poolable.OnDespawned();

            pooledObject.SetActive(false);
            pooledObject.transform.SetParent(transform, false);
        }

        private void OnDestroyObject(GameObject pooledObject)
        {
            Destroy(pooledObject);
        }

        private static void NotifySpawned(GameObject pooledObject)
        {
            foreach (IPoolable poolable in pooledObject.GetComponents<IPoolable>())
                poolable.OnSpawned();
        }
    }

}
