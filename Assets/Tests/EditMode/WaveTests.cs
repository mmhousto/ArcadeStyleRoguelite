using System.Collections.Generic;
using NUnit.Framework;
using Com.MMsGamingCo.Ricomet;

// Checks sector layouts and generated endless waves.
public class WaveTests
{
    private static readonly HashSet<string> KnownEnemies = new HashSet<string>
    {
        EnemyIds.Drifter, EnemyIds.DrifterMini, EnemyIds.Dart, EnemyIds.Splitter
    };

    [Test]
    public void Sector1HasFourWaves()
    {
        Assert.AreEqual(4, SectorCatalog.Sector1().waves.Count);
    }

    [Test]
    public void SectorWavesOnlyUseKnownEnemies()
    {
        foreach (WaveDefinition wave in SectorCatalog.Sector1().waves)
        {
            Assert.Greater(wave.TotalEnemies(), 0);
            foreach (SpawnGroup group in wave.groups)
            {
                Assert.IsTrue(KnownEnemies.Contains(group.enemyId), "Unknown enemy: " + group.enemyId);
                Assert.Greater(group.count, 0);
            }
        }
    }

    [Test]
    public void WavesGetBigger()
    {
        List<WaveDefinition> waves = SectorCatalog.Sector1().waves;
        for (int i = 1; i < waves.Count; i++)
            Assert.GreaterOrEqual(waves[i].TotalEnemies(), waves[i - 1].TotalEnemies(), "Wave " + (i + 1));
    }

    [Test]
    public void SpawnDurationIsLastSpawn()
    {
        var wave = new WaveDefinition(20f, new SpawnGroup("a", 3, 2f, 1f), new SpawnGroup("b", 1, 1f, 5f));
        Assert.AreEqual(4f, wave.SpawnDuration(), 0.001f);
        Assert.AreEqual(4, wave.TotalEnemies());
    }

    [Test]
    public void EndlessWavesAreRepeatableAndGrow()
    {
        WaveDefinition a = SectorCatalog.EndlessWave(1, new SeededRandom(5));
        WaveDefinition b = SectorCatalog.EndlessWave(1, new SeededRandom(5));
        Assert.AreEqual(a.TotalEnemies(), b.TotalEnemies());
        Assert.AreEqual(a.groups.Count, b.groups.Count);

        WaveDefinition later = SectorCatalog.EndlessWave(10, new SeededRandom(5));
        Assert.Greater(later.TotalEnemies(), a.TotalEnemies());

        foreach (SpawnGroup group in later.groups)
            Assert.IsTrue(KnownEnemies.Contains(group.enemyId));
    }
}
