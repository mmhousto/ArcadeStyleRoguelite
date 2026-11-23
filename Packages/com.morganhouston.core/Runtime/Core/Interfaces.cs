namespace Com.MorganHouston.Core
{

    /// <summary>Anything that can take damage, e.g. IDamageable&lt;float&gt;.</summary>
    public interface IDamageable<T>
    {
        void Damage(T damageTaken);
    }

    /// <summary>Anything the player can use or interact with.</summary>
    public interface IUseable
    {
        void Use(int index);
    }

}
