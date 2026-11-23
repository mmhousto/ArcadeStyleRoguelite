using System;
using UnityEngine;

namespace Com.MMsGamingCo.Ricomet
{

    /// <summary>
    /// The comet's shields. A hit costs one shield and gives a short invulnerability
    /// window. No shields left ends the run.
    /// </summary>
    public class CometHealth : MonoBehaviour
    {
        public int MaxShields { get; private set; }
        public int Shields { get; private set; }
        public bool IsDead { get { return Shields <= 0; } }
        public bool IsInvulnerable { get { return invulnerableTimer > 0f; } }

        /// <summary>Raised with the shields left after a hit.</summary>
        public static event Action<int> cometDamaged;
        public static event Action cometDied;

        private float invulnerableTimer = 0f;
        private float invulnerableTime = 1f;

        public void Setup(int maxShields, float invulnerableSeconds)
        {
            MaxShields = maxShields;
            Shields = maxShields;
            invulnerableTime = invulnerableSeconds;
        }

        private void Update()
        {
            if (invulnerableTimer > 0f)
                invulnerableTimer -= Time.deltaTime;
        }

        /// <summary>Takes one hit unless invulnerable. Returns true if the hit landed.</summary>
        public bool TakeHit()
        {
            if (IsDead || IsInvulnerable) return false;

            Shields--;
            invulnerableTimer = invulnerableTime;
            cometDamaged?.Invoke(Shields);

            if (IsDead)
                cometDied?.Invoke();

            return true;
        }

        /// <summary>Changes max shields. Raising it also repairs the new shields.</summary>
        public void SetMaxShields(int maxShields)
        {
            int gained = Mathf.Max(0, maxShields - MaxShields);
            MaxShields = Mathf.Max(1, maxShields);
            if (!IsDead)
                Shields = Mathf.Min(MaxShields, Shields + gained);
        }

        public void RestoreShield()
        {
            if (!IsDead)
                Shields = Mathf.Min(MaxShields, Shields + 1);
        }
    }

}
