using System.Collections.Generic;
using UnityEngine;

namespace Com.MMsGamingCo.Ricomet
{

    /// <summary>
    /// Upgrade effects that react to dashes and kills: Shockwave, Arc and Shatter.
    /// Plasma Trail lives in PlasmaTrail; Seeker lives in CometController.
    /// </summary>
    public class CometAbilities : MonoBehaviour
    {
        // Separate lists: a Shockwave kill can trigger an Arc while the shockwave loop is running.
        private readonly List<Enemy> shockwaveBuffer = new List<Enemy>();
        private readonly List<Enemy> arcBuffer = new List<Enemy>();
        private readonly List<Fragment> fragments = new List<Fragment>();

        private CometStats Stats
        {
            get { return CometController.Instance != null ? CometController.Instance.Stats : null; }
        }

        private void OnEnable()
        {
            CometController.dashEnded += OnDashEnded;
            Enemy.enemyKilled += OnEnemyKilled;
        }

        private void OnDisable()
        {
            CometController.dashEnded -= OnDashEnded;
            Enemy.enemyKilled -= OnEnemyKilled;
        }

        private void OnDashEnded(Vector2 position, int kills)
        {
            CometStats stats = Stats;
            if (stats == null || stats.shockwaveRadius <= 0f) return;

            EffectSpawner.Shockwave(position, stats.shockwaveRadius);
            ScreenShake.Add(0.2f);

            shockwaveBuffer.Clear();
            shockwaveBuffer.AddRange(Enemy.Active);
            foreach (Enemy enemy in shockwaveBuffer)
            {
                if (enemy.IsAlive && Geometry2D.CirclesOverlap(position, stats.shockwaveRadius, enemy.Position, enemy.Radius))
                    enemy.Damage(1f, DamageSource.Shockwave);
            }
        }

        private void OnEnemyKilled(Enemy enemy)
        {
            CometStats stats = Stats;
            if (stats == null) return;

            // Only direct hits chain, so one kill can't cascade forever.
            bool direct = enemy.LastDamageSource == DamageSource.Dash || enemy.LastDamageSource == DamageSource.Shockwave;
            if (!direct) return;

            Vector2 origin = enemy.Position;
            if (stats.chainTargets > 0) Arc(origin, stats.chainTargets, stats.chainRange, enemy);
            if (stats.shatterFragments > 0) Shatter(origin, stats.shatterFragments);
        }

        private void Arc(Vector2 origin, int targets, float range, Enemy source)
        {
            arcBuffer.Clear();
            foreach (Enemy other in Enemy.Active)
            {
                if (other != source && other.IsAlive && (other.Position - origin).sqrMagnitude <= range * range)
                    arcBuffer.Add(other);
            }
            arcBuffer.Sort((a, b) => (a.Position - origin).sqrMagnitude.CompareTo((b.Position - origin).sqrMagnitude));

            for (int i = 0; i < targets && i < arcBuffer.Count; i++)
            {
                EffectSpawner.Lightning(origin, arcBuffer[i].Position, NeonColors.Player);
                arcBuffer[i].Damage(1f, DamageSource.Chain);
            }
        }

        private void Shatter(Vector2 origin, int count)
        {
            float start = Random.value * Mathf.PI * 2f;
            for (int i = 0; i < count; i++)
            {
                float angle = start + i * Mathf.PI * 2f / count;
                GetFragment().Launch(origin, new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)));
            }
        }

        private Fragment GetFragment()
        {
            foreach (Fragment fragment in fragments)
            {
                if (!fragment.IsActive) return fragment;
            }

            GameObject fragmentObject = new GameObject("Fragment");
            fragmentObject.transform.SetParent(transform, false);
            Fragment created = fragmentObject.AddComponent<Fragment>();
            fragmentObject.SetActive(false);
            fragments.Add(created);
            return created;
        }
    }

}
