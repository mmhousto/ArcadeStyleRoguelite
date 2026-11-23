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

        [Tooltip("Stable ID used by waves and saves. Never rename after release.")]
        public string enemyId = "enemy";
        public float maxHealth = 1f;
        public float radius = 0.5f;
        public int points = 100;

        public float Health { get; private set; }
        public bool IsAlive { get; private set; }
        public float Radius { get { return radius; } }
        public Vector2 Position { get { return transform.position; } }

        /// <summary>False while an enemy is harmless (e.g. a Dart recovering). Most are always dangerous.</summary>
        public virtual bool DealsContactDamage { get { return true; } }

        protected PrototypeTuning tuning;
        protected LineRenderer outline;
        private float flashTimer = 0f;
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

            Tick(Time.deltaTime);
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
            if (!IsAlive) return;

            Health -= damageTaken;
            flashTimer = 0.06f;

            if (Health <= 0f)
                Die();
        }

        public virtual void OnSpawned()
        {
            Health = maxHealth;
            IsAlive = true;
            flashTimer = 0f;
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

            if (pooledObject != null)
                pooledObject.ReturnToPool();
            else
                Destroy(this.gameObject);
        }

        #endregion

    }

}
