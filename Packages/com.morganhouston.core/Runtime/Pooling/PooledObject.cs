using UnityEngine;

namespace Com.MorganHouston.Core
{

    /// <summary>
    /// Added automatically to objects made by a PrefabPool so they can return
    /// themselves with ReturnToPool() without a reference to the pool.
    /// </summary>
    public class PooledObject : MonoBehaviour
    {
        public PrefabPool Pool { get; set; }

        /// <summary>Returns to the pool, or destroys the object if it has no pool.</summary>
        public void ReturnToPool()
        {
            if (Pool != null)
                Pool.Release(this.gameObject);
            else
                Destroy(this.gameObject);
        }

        /// <summary>Returns to the pool after a delay. Uses scaled time.</summary>
        public void ReturnToPool(float delay)
        {
            CancelInvoke(nameof(ReturnToPool));
            Invoke(nameof(ReturnToPool), delay);
        }

        private void OnDisable()
        {
            CancelInvoke();
        }
    }

}
