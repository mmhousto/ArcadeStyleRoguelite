namespace Com.MorganHouston.Core
{

    /// <summary>
    /// Implement on pooled objects to reset state, since Awake and Start only run once.
    /// </summary>
    public interface IPoolable
    {
        /// <summary>Called after the object is taken from the pool and placed.</summary>
        void OnSpawned();

        /// <summary>Called right before the object goes back into the pool.</summary>
        void OnDespawned();
    }

}
