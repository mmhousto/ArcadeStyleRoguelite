using System;
using UnityEngine;

namespace Com.MMsGamingCo.Ricomet
{

    /// <summary>
    /// Counts kills made close together. The combo drops if no kill happens within
    /// the window. Times are passed in so this can be tested without Unity's clock.
    /// </summary>
    public class ComboTracker
    {
        public static readonly int[] Milestones = { 10, 25, 50, 100 };

        public float Window { get; set; }
        public float MaxMultiplier { get; set; }
        public int Count { get; private set; }
        public int Best { get; private set; }

        /// <summary>Raised with the milestone value when the combo reaches one.</summary>
        public event Action<int> milestoneReached;

        /// <summary>Raised with the final count when a combo of 2 or more ends.</summary>
        public event Action<int> comboEnded;

        private float lastKillTime = float.NegativeInfinity;

        /// <summary>Score multiplier: 1 + combo / 10, capped.</summary>
        public float Multiplier
        {
            get { return Mathf.Min(MaxMultiplier, 1f + Count / 10f); }
        }

        /// <summary>0-1, how much of the combo window is left. For the HUD timer bar.</summary>
        public float TimeLeftFraction(float now)
        {
            if (Count == 0 || Window <= 0f) return 0f;
            return Mathf.Clamp01(1f - (now - lastKillTime) / Window);
        }

        public ComboTracker(float window, float maxMultiplier)
        {
            Window = window;
            MaxMultiplier = maxMultiplier;
        }

        public void RegisterKill(float now)
        {
            if (Count > 0 && now - lastKillTime > Window)
                EndCombo();

            Count++;
            lastKillTime = now;
            if (Count > Best) Best = Count;

            if (Array.IndexOf(Milestones, Count) >= 0)
                milestoneReached?.Invoke(Count);
        }

        /// <summary>Call every frame so the combo ends on time even with no new kills.</summary>
        public void Tick(float now)
        {
            if (Count > 0 && now - lastKillTime > Window)
                EndCombo();
        }

        public void Reset()
        {
            Count = 0;
            lastKillTime = float.NegativeInfinity;
        }

        private void EndCombo()
        {
            if (Count >= 2) comboEnded?.Invoke(Count);
            Count = 0;
        }
    }

}
