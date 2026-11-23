using System.Collections.Generic;
using UnityEngine;

namespace Com.MMsGamingCo.Ricomet
{

    /// <summary>
    /// Reuses enemy projectiles instead of creating new ones for every shot.
    /// </summary>
    public class EnemyProjectiles : MonoBehaviour
    {
        private static EnemyProjectiles _instance;

        public static EnemyProjectiles Instance { get { return _instance; } }

        private readonly List<EnemyProjectile> projectiles = new List<EnemyProjectile>();

        private void Awake()
        {
            _instance = this;
            for (int i = 0; i < 24; i++)
                Create();
        }

        private void OnDestroy()
        {
            if (_instance == this) _instance = null;
        }

        /// <summary>Fires one projectile. Safe to call when no manager exists.</summary>
        public static void Fire(Vector2 position, Vector2 velocity, float lifetime = 6f)
        {
            if (_instance != null)
                _instance.Get().Launch(position, velocity, lifetime);
        }

        /// <summary>Fires count projectiles evenly around a circle.</summary>
        public static void FireRing(Vector2 position, int count, float speed, float angleOffset)
        {
            for (int i = 0; i < count; i++)
            {
                float angle = (angleOffset + i * 360f / count) * Mathf.Deg2Rad;
                Fire(position, new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * speed);
            }
        }

        /// <summary>Removes every projectile, e.g. when a boss dies.</summary>
        public static void ClearAll()
        {
            if (_instance == null) return;

            foreach (EnemyProjectile projectile in _instance.projectiles)
            {
                if (projectile.IsActive) projectile.Despawn();
            }
        }

        private EnemyProjectile Get()
        {
            foreach (EnemyProjectile projectile in projectiles)
            {
                if (!projectile.IsActive) return projectile;
            }
            return Create();
        }

        private EnemyProjectile Create()
        {
            GameObject projectileObject = new GameObject("Enemy Projectile");
            projectileObject.transform.SetParent(transform, false);
            EnemyProjectile projectile = projectileObject.AddComponent<EnemyProjectile>();
            projectileObject.SetActive(false);
            projectiles.Add(projectile);
            return projectile;
        }
    }

}
