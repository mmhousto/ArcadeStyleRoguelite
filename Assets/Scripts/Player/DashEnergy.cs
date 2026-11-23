using UnityEngine;

namespace Com.MMsGamingCo.Ricomet
{

    /// <summary>
    /// Dash charges. Each dash spends one charge, kills refund part of one, and
    /// charges slowly come back while the comet is stopped.
    /// </summary>
    public class DashEnergy
    {
        public float Max { get; private set; }
        public float Current { get; private set; }
        public float RegenPerSecond { get; set; }

        public bool CanDash { get { return Current >= 1f; } }

        /// <summary>Whole charges ready to use.</summary>
        public int ReadyCharges { get { return Mathf.FloorToInt(Current); } }

        public DashEnergy(float max, float regenPerSecond)
        {
            Max = max;
            Current = max;
            RegenPerSecond = regenPerSecond;
        }

        /// <summary>Spends one charge. Returns false if there wasn't a full charge.</summary>
        public bool Spend()
        {
            if (!CanDash) return false;
            Current -= 1f;
            return true;
        }

        public void Refund(float amount)
        {
            Current = Mathf.Min(Max, Current + Mathf.Max(0f, amount));
        }

        public void Regen(float deltaTime)
        {
            Refund(RegenPerSecond * deltaTime);
        }

        /// <summary>Changes the maximum. Raising it also adds the new charges.</summary>
        public void SetMax(float max)
        {
            float gained = Mathf.Max(0f, max - Max);
            Max = Mathf.Max(1f, max);
            Current = Mathf.Min(Max, Current + gained);
        }
    }

}
