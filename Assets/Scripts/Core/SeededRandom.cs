using UnityEngine;

namespace Com.MMsGamingCo.Ricomet
{

    /// <summary>
    /// Deterministic random numbers for a run. The same seed always gives the same
    /// sequence on every device, which Daily Runs and bug replays depend on.
    /// Never use UnityEngine.Random for anything that affects a run.
    /// </summary>
    public class SeededRandom
    {
        private ulong state;

        public int Seed { get; private set; }

        public SeededRandom(int seed)
        {
            Seed = seed;

            // SplitMix64 scrambles the seed so nearby seeds give unrelated sequences.
            ulong z = (ulong)(uint)seed + 0x9E3779B97F4A7C15UL;
            z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
            z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
            state = z ^ (z >> 31);
            if (state == 0) state = 0x9E3779B97F4A7C15UL;
        }

        /// <summary>A seed from the current time, for normal runs.</summary>
        public static int NewSeed()
        {
            return (int)(System.DateTime.UtcNow.Ticks & 0x7FFFFFFF);
        }

        /// <summary>Same seed for everyone on a given UTC day, for the Daily Run.</summary>
        public static int DailySeed(System.DateTime utcDate)
        {
            return utcDate.Year * 10000 + utcDate.Month * 100 + utcDate.Day;
        }

        /// <summary>Next raw 64-bit value (xorshift64*).</summary>
        public ulong NextULong()
        {
            state ^= state >> 12;
            state ^= state << 25;
            state ^= state >> 27;
            return state * 0x2545F4914F6CDD1DUL;
        }

        /// <summary>Float in [0, 1).</summary>
        public float Value()
        {
            return (NextULong() >> 40) / (float)(1UL << 24);
        }

        /// <summary>Float in [min, max).</summary>
        public float Range(float min, float max)
        {
            return min + (max - min) * Value();
        }

        /// <summary>Int in [min, max). Returns min if max is not above min.</summary>
        public int Range(int min, int max)
        {
            if (max <= min) return min;
            return min + (int)(NextULong() % (ulong)(max - min));
        }

        public bool Chance(float probability)
        {
            return Value() < probability;
        }

        /// <summary>Unit vector in a random direction.</summary>
        public Vector2 OnUnitCircle()
        {
            float angle = Range(0f, Mathf.PI * 2f);
            return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        }
    }

}
