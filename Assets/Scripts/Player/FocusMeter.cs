using UnityEngine;

namespace Com.MMsGamingCo.Ricomet
{

    /// <summary>
    /// Limits how long the player can aim in slow motion. Drains in real seconds
    /// while aiming, refills while not aiming and on kills.
    /// </summary>
    public class FocusMeter
    {
        public float Max { get; private set; }
        public float Current { get; private set; }
        public float RefillPerSecond { get; set; }

        public bool HasFocus { get { return Current > 0f; } }
        public float Fraction { get { return Max > 0f ? Current / Max : 0f; } }

        public FocusMeter(float maxSeconds, float refillPerSecond)
        {
            Max = maxSeconds;
            Current = maxSeconds;
            RefillPerSecond = refillPerSecond;
        }

        /// <summary>Changes the maximum. Raising it also adds the difference.</summary>
        public void SetMax(float maxSeconds)
        {
            float gained = Mathf.Max(0f, maxSeconds - Max);
            Max = Mathf.Max(0.1f, maxSeconds);
            Current = Mathf.Min(Max, Current + gained);
        }

        public void Drain(float realDeltaTime)
        {
            Current = Mathf.Max(0f, Current - realDeltaTime);
        }

        public void Refill(float realDeltaTime)
        {
            Add(RefillPerSecond * realDeltaTime);
        }

        public void Add(float seconds)
        {
            Current = Mathf.Min(Max, Current + Mathf.Max(0f, seconds));
        }
    }

}
