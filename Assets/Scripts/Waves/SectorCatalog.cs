using System.Collections.Generic;

namespace Com.MMsGamingCo.Ricomet
{

    /// <summary>
    /// Sector layouts, plus generated waves for endless mode. Defined in code for
    /// now; they move to ScriptableObjects when level design starts in earnest.
    /// </summary>
    public static class SectorCatalog
    {
        private const string D = EnemyIds.Drifter;
        private const string T = EnemyIds.Dart;
        private const string S = EnemyIds.Splitter;

        /// <summary>Sector 1: teaches the dash, then Darts, then Splitters.</summary>
        public static SectorDefinition Sector1()
        {
            return new SectorDefinition("sector_1", "Sector 1: Outskirts",
                // Wave 1: just Drifters, in loose groups. Learn to chain.
                new WaveDefinition(20f,
                    new SpawnGroup(D, 6, 0f, 0.4f),
                    new SpawnGroup(D, 6, 5f, 0.3f),
                    new SpawnGroup(D, 8, 11f, 0.25f)),

                // Wave 2: first Darts mixed into Drifters.
                new WaveDefinition(25f,
                    new SpawnGroup(D, 8, 0f, 0.3f),
                    new SpawnGroup(T, 2, 4f, 2f),
                    new SpawnGroup(D, 10, 9f, 0.25f),
                    new SpawnGroup(T, 3, 14f, 1.5f)),

                // Wave 3: Splitters arrive.
                new WaveDefinition(28f,
                    new SpawnGroup(S, 3, 0f, 1.5f),
                    new SpawnGroup(D, 10, 3f, 0.3f),
                    new SpawnGroup(T, 3, 8f, 1.5f),
                    new SpawnGroup(S, 3, 13f, 1f),
                    new SpawnGroup(D, 10, 16f, 0.2f)),

                // Wave 4: everything at once.
                new WaveDefinition(32f,
                    new SpawnGroup(D, 14, 0f, 0.2f),
                    new SpawnGroup(T, 4, 3f, 1f),
                    new SpawnGroup(S, 4, 6f, 1.2f),
                    new SpawnGroup(D, 16, 12f, 0.15f),
                    new SpawnGroup(T, 5, 16f, 0.8f),
                    new SpawnGroup(S, 3, 20f, 0.8f)));
        }

        /// <summary>
        /// A generated wave for endless mode. Gets denser and harder with each wave.
        /// </summary>
        public static WaveDefinition EndlessWave(int waveNumber, SeededRandom random)
        {
            float difficulty = 1f + waveNumber * 0.2f;
            var groups = new List<SpawnGroup>();
            float time = 0f;

            int groupCount = 4 + waveNumber / 2;
            for (int i = 0; i < groupCount; i++)
            {
                float roll = random.Value();
                string enemyId = roll < 0.55f ? D : roll < 0.8f ? T : S;
                int count = enemyId == D
                    ? random.Range(6, 10) + (int)(difficulty * 2f)
                    : random.Range(2, 4) + (int)(difficulty * 0.5f);
                float interval = enemyId == D ? 0.25f / difficulty + 0.08f : 1.2f / difficulty + 0.3f;

                groups.Add(new SpawnGroup(enemyId, count, time, interval));
                time += random.Range(3f, 5f) / difficulty + 1.5f;
            }

            return new WaveDefinition(25f + waveNumber, groups.ToArray());
        }
    }

}
