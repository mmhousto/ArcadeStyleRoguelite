using System;
using System.Collections.Generic;
using Com.MMsGamingCo.Core;
using UnityEngine;

namespace Com.MMsGamingCo.Ricomet
{

    /// <summary>
    /// Base for every enemy. Tracks itself in Enemy.Active so the comet can check
    /// hits without physics, takes damage, and returns to its pool on death.
    /// Subclasses only write movement in Tick().
    /// </summary>
    public abstract class Enemy : MonoBehaviour, IDamageable<float>, IPoolable
    {

        #region Fields/Variables

        private static readonly List<Enemy> active = new List<Enemy>();

        /// <summary>Every live enemy in the scene.</summary>
        public static IReadOnlyList<Enemy> Active { get { return active; } }

        /// <summary>Raised when any enemy dies.</summary>
        public static event Action<Enemy> enemyKilled;

        /// <summary>Raised when any enemy takes damage (including the killing blow).</summary>
        public static event Action<Enemy, DamageSource> enemyDamaged;

        [Tooltip("Stable ID used by waves and saves. Never rename after release.")]
        public string enemyId = "enemy";
        public float maxHealth = 1f;
        public float radius = 0.5f;
        public int points = 100;

        public float Health { get; private set; }
        public bool IsAlive { get; private set; }

        /// <summary>What dealt the last hit. Read it in enemyKilled to see what killed the enemy.</summary>
        public DamageSource LastDamageSource { get; private set; }
        public float Radius { get { return radius; } }
        public Vector2 Position { get { return transform.position; } }

        /// <summary>False while an enemy is harmless (e.g. a Dart recovering). Most are always dangerous.</summary>
        public virtual bool DealsContactDamage { get { return true; } }

        /// <summary>True while knocked back. Stunned enemies can't move or hurt the comet.</summary>
        public bool IsStunned { get { return stunTimer > 0f; } }

        /// <summary>Whether touching this enemy right now hurts the comet.</summary>
        public bool CanHurtComet { get { return IsAlive && !IsStunned && DealsContactDamage; } }

        protected PrototypeTuning tuning;
        protected LineRenderer outline;
        private float flashTimer = 0f;
        private float stunTimer = 0f;
        private Vector2 knockbackVelocity = Vector2.zero;
        private PooledObject pooledObject;

        #endregion


        #region MonoBehaviour Methods

        protected virtual void Awake()
        {
            tuning = PrototypeTuning.Instance;
            pooledObject = GetComponent<PooledObject>();
            BuildVisuals();
        }

        protected virtual void OnEnable()
        {
            if (!active.Contains(this)) active.Add(this);
        }

        protected virtual void OnDisable()
        {
            active.Remove(this);
        }

        private void Update()
        {
            if (!IsAlive) return;

            if (stunTimer > 0f)
            {
                // Knocked back: slide instead of acting.
                stunTimer -= Time.deltaTime;
                transform.position = Position + knockbackVelocity * Time.deltaTime;
                knockbackVelocity = Knockback.Decay(knockbackVelocity, tuning.knockbackDrag, Time.deltaTime);
            }
            else
            {
                Tick(Time.deltaTime);
            }
            transform.position = Arena.Clamp(Position, radius);

            if (flashTimer > 0f)
            {
                flashTimer -= Time.unscaledDeltaTime;
                ShapeFactory.SetColor(outline, flashTimer > 0f ? NeonColors.White : BaseColor());
            }
        }

        #endregion


        #region Public Methods

        public void Damage(float damageTaken)
        {
            Damage(damageTaken, DamageSource.Dash);
        }

        public void Damage(float damageTaken, DamageSource source)
        {
            if (!IsAlive) return;

            damageTaken = AdjustDamage(damageTaken, source);
            if (damageTaken <= 0f) return;

            LastDamageSource = source;
            Health -= damageTaken;
            flashTimer = 0.06f;
            enemyDamaged?.Invoke(this, source);

            if (Health <= 0f)
                Die();
        }

        /// <summary>Pushes the enemy and stuns it for a moment.</summary>
        public virtual void ApplyKnockback(Vector2 velocity, float stunSeconds)
        {
            if (!IsAlive || velocity == Vector2.zero) return;

            knockbackVelocity = velocity;
            stunTimer = Mathf.Max(stunTimer, stunSeconds);
        }

        public virtual void OnSpawned()
        {
            Health = maxHealth;
            IsAlive = true;
            flashTimer = 0f;
            stunTimer = 0f;
            knockbackVelocity = Vector2.zero;
            ShapeFactory.SetColor(outline, BaseColor());
        }

        public virtual void OnDespawned()
        {
            IsAlive = false;
        }

        #endregion


        #region Protected Methods

        /// <summary>Movement and behavior. Called each frame while alive.</summary>
        protected abstract void Tick(float deltaTime);

        /// <summary>Creates the enemy's outline. Assign it to outline.</summary>
        protected abstract void BuildVisuals();

        /// <summary>Lets an enemy change or ignore incoming damage. Return 0 to ignore the hit.</summary>
        protected virtual float AdjustDamage(float amount, DamageSource source)
        {
            return amount;
        }

        /// <summary>Called after the kill is reported, before returning to the pool.</summary>
        protected virtual void OnDied()
        {
        }

        protected virtual Color BaseColor()
        {
            return NeonColors.Enemy;
        }

        /// <summary>Position of the comet, or this enemy's own position if there isn't one.</summary>
        protected Vector2 TargetPosition()
        {
            return CometController.Instance != null ? CometController.Instance.Position : Position;
        }

        /// <summary>Push away from nearby enemies so groups spread out instead of stacking.</summary>
        protected Vector2 Separation(float range)
        {
            Vector2 push = Vector2.zero;
            foreach (Enemy other in active)
            {
                if (other == this) continue;

                Vector2 away = Position - other.Position;
                float distance = away.magnitude;
                if (distance < range && distance > 0.0001f)
                    push += away / distance * (1f - distance / range);
            }
            return push;
        }

        /// <summary>Turns the outline to face a direction.</summary>
        protected void Face(Vector2 direction)
        {
            if (direction.sqrMagnitude < 0.0001f) return;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            outline.transform.localRotation = Quaternion.Euler(0f, 0f, angle);
        }

        #endregion


        #region Private Methods

        private void Die()
        {
            IsAlive = false;
            EffectSpawner.Kill(Position, BaseColor());
            ScreenShake.Add(tuning.killShake);
            if (TimeController.Instance != null)
                TimeController.Instance.HitStop(tuning.hitStopSeconds);

            enemyKilled?.Invoke(this);
            OnDied();

            if (pooledObject != null)
                pooledObject.ReturnToPool();
            else
                Destroy(this.gameObject);
        }

        #endregion

    }

}
