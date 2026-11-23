using System;
using System.Collections.Generic;
using UnityEngine;

namespace Com.MMsGamingCo.Ricomet
{

    /// <summary>
    /// Runs a sector: wave intro, spawning, wave clear, upgrade pick, next wave.
    /// After the last wave the sector is cleared; the player can then continue
    /// into endless waves. Every spawn is warned with a shrinking ring first.
    /// </summary>
    public class WaveDirector : MonoBehaviour
    {

        #region Fields/Variables

        private static WaveDirector _instance;

        public static WaveDirector Instance { get { return _instance; } }

        public enum Phase { Intro, Fighting, Cleared, Picking, SectorClear, Stopped }

        private class PendingSpawn
        {
            public string enemyId;
            public Vector2 position;
            public float timer;
        }

        public Phase CurrentPhase { get; private set; } = Phase.Stopped;
        public SectorDefinition Sector { get; private set; }
        public bool IsEndless { get; private set; }

        /// <summary>1-based wave number, counting on through endless waves.</summary>
        public int WaveNumber { get { return waveIndex + 1; } }
        public int WaveCount { get { return Sector != null ? Sector.waves.Count : 0; } }
        public int RerollsLeft { get; private set; }
        public List<UpgradeDefinition> CurrentOffer { get; private set; } = new List<UpgradeDefinition>();

        /// <summary>Enemies still to spawn plus enemies alive.</summary>
        public int EnemiesRemaining
        {
            get { return Mathf.Max(0, waveTotal - spawnedThisWave) + pending.Count + Enemy.Active.Count; }
        }

        /// <summary>Raised with the wave number when a wave's intro starts.</summary>
        public static event Action<int> waveStarted;

        /// <summary>Raised with the wave number and time bonus when a wave is cleared.</summary>
        public static event Action<int, int> waveCleared;

        /// <summary>Raised when an upgrade offer is ready to show.</summary>
        public static event Action<List<UpgradeDefinition>> upgradeOfferReady;

        /// <summary>Raised when the picker should close.</summary>
        public static event Action upgradeChosen;

        public static event Action sectorCleared;

        private PrototypeTuning tuning;
        private SeededRandom spawnRandom;
        private SeededRandom upgradeRandom;
        private WaveDefinition wave;
        private int waveIndex = -1;
        private float phaseTimer;
        private float waveTime;
        private int waveTotal;
        private int spawnedThisWave;
        private int[] spawnedPerGroup;
        private readonly List<PendingSpawn> pending = new List<PendingSpawn>();

        #endregion


        #region MonoBehaviour Methods

        private void Awake()
        {
            _instance = this;
        }

        private void OnDestroy()
        {
            if (_instance == this) _instance = null;
        }

        private void OnEnable()
        {
            CometHealth.cometDied += Stop;
        }

        private void OnDisable()
        {
            CometHealth.cometDied -= Stop;
        }

        private void Update()
        {
            switch (CurrentPhase)
            {
                case Phase.Intro:
                    phaseTimer -= Time.deltaTime;
                    if (phaseTimer <= 0f)
                    {
                        CurrentPhase = Phase.Fighting;
                        waveTime = 0f;
                    }
                    break;

                case Phase.Fighting:
                    UpdateFighting(Time.deltaTime);
                    break;

                case Phase.Cleared:
                    phaseTimer -= Time.deltaTime;
                    if (phaseTimer <= 0f)
                        AfterWaveCleared();
                    break;
            }
        }

        #endregion


        #region Public Methods

        public void Setup(PrototypeTuning newTuning, SectorDefinition sector, SeededRandom spawnSeed, SeededRandom upgradeSeed)
        {
            tuning = newTuning;
            Sector = sector;
            spawnRandom = spawnSeed;
            upgradeRandom = upgradeSeed;
            RerollsLeft = tuning.rerollsPerSector;
            StartWave(0);
        }

        /// <summary>Takes an upgrade from the current offer and starts the next wave.</summary>
        public void ChooseUpgrade(UpgradeDefinition upgrade)
        {
            if (CurrentPhase != Phase.Picking || upgrade == null || !CurrentOffer.Contains(upgrade)) return;

            if (RunManager.Instance != null)
                RunManager.Instance.AddUpgrade(upgrade.id);

            SetPickingPaused(false);
            upgradeChosen?.Invoke();
            StartWave(waveIndex + 1);
        }

        /// <summary>Rolls a new offer if a reroll is left.</summary>
        public bool Reroll()
        {
            if (CurrentPhase != Phase.Picking || RerollsLeft <= 0) return false;

            RerollsLeft--;
            OfferUpgrades();
            return true;
        }

        /// <summary>After clearing the sector, keep going with generated waves.</summary>
        public void ContinueEndless()
        {
            if (CurrentPhase != Phase.SectorClear) return;

            IsEndless = true;
            if (RunManager.Instance != null)
                RunManager.Instance.ContinueRun();
            OfferUpgrades();
        }

        #endregion


        #region Private Methods

        private void StartWave(int index)
        {
            waveIndex = index;
            wave = index < WaveCount ? Sector.waves[index] : SectorCatalog.EndlessWave(index - WaveCount + 1, spawnRandom);
            waveTotal = wave.TotalEnemies();
            spawnedThisWave = 0;
            spawnedPerGroup = new int[wave.groups.Count];
            pending.Clear();

            CurrentPhase = Phase.Intro;
            phaseTimer = tuning.waveIntroTime;
            waveStarted?.Invoke(WaveNumber);
        }

        private void UpdateFighting(float deltaTime)
        {
            waveTime += deltaTime;
            UpdatePending(deltaTime);

            for (int g = 0; g < wave.groups.Count; g++)
            {
                SpawnGroup group = wave.groups[g];
                while (spawnedPerGroup[g] < group.count && waveTime >= group.startTime + spawnedPerGroup[g] * group.interval)
                {
                    // Hold spawns while the arena is full; they come once there's room.
                    if (Enemy.Active.Count + pending.Count >= tuning.maxAliveEnemies) return;

                    QueueSpawn(group.enemyId);
                    spawnedPerGroup[g]++;
                    spawnedThisWave++;
                }
            }

            if (spawnedThisWave >= waveTotal && pending.Count == 0 && Enemy.Active.Count == 0)
                OnWaveCleared();
        }

        private void OnWaveCleared()
        {
            int timeBonus = Mathf.Max(0, Mathf.RoundToInt((wave.parTime - waveTime) * tuning.timeBonusPerSecond));
            if (RunManager.Instance != null)
                RunManager.Instance.AddScore(timeBonus);

            CurrentPhase = Phase.Cleared;
            phaseTimer = tuning.waveClearDelay;
            waveCleared?.Invoke(WaveNumber, timeBonus);
        }

        private void AfterWaveCleared()
        {
            bool lastSectorWave = !IsEndless && waveIndex >= WaveCount - 1;
            if (lastSectorWave)
            {
                CurrentPhase = Phase.SectorClear;
                if (CometController.Instance != null)
                    CometController.Instance.InputLocked = true;
                if (RunManager.Instance != null)
                    RunManager.Instance.EndRun(true);
                sectorCleared?.Invoke();
                return;
            }

            OfferUpgrades();
        }

        private void OfferUpgrades()
        {
            CurrentPhase = Phase.Picking;
            UpgradeInventory inventory = RunManager.Instance != null ? RunManager.Instance.Inventory : null;
            CurrentOffer = UpgradeCatalog.RollOffer(upgradeRandom, inventory, tuning.upgradeChoices);

            // Everything is maxed: skip straight to the next wave.
            if (CurrentOffer.Count == 0)
            {
                StartWave(waveIndex + 1);
                return;
            }

            SetPickingPaused(true);
            upgradeOfferReady?.Invoke(CurrentOffer);
        }

        private void SetPickingPaused(bool picking)
        {
            if (TimeController.Instance != null)
                TimeController.Instance.SetPaused(picking);
            if (CometController.Instance != null)
                CometController.Instance.InputLocked = picking;
        }

        private void QueueSpawn(string enemyId)
        {
            Vector2 position = FindSpawnPoint();
            pending.Add(new PendingSpawn { enemyId = enemyId, position = position, timer = tuning.spawnWarningTime });
            EffectSpawner.SpawnWarning(position, NeonColors.Enemy, tuning.spawnWarningTime);
        }

        private void UpdatePending(float deltaTime)
        {
            for (int i = pending.Count - 1; i >= 0; i--)
            {
                pending[i].timer -= deltaTime;
                if (pending[i].timer > 0f) continue;

                if (EnemyFactory.Instance != null)
                    EnemyFactory.Instance.Spawn(pending[i].enemyId, pending[i].position);
                pending.RemoveAt(i);
            }
        }

        /// <summary>A random point, mostly near the edges, that isn't too close to the comet.</summary>
        private Vector2 FindSpawnPoint()
        {
            Vector2 comet = CometController.Instance != null ? CometController.Instance.Position : Vector2.zero;
            Vector2 best = Vector2.zero;
            float bestDistance = -1f;

            for (int attempt = 0; attempt < 8; attempt++)
            {
                Vector2 point = Arena.RandomPoint(spawnRandom, 1f);

                if (spawnRandom.Chance(0.7f))
                {
                    if (spawnRandom.Chance(0.5f)) point.x = Mathf.Sign(point.x) * (Arena.Bounds.width * 0.5f - 1f);
                    else point.y = Mathf.Sign(point.y) * (Arena.Bounds.height * 0.5f - 1f);
                }

                float distance = Vector2.Distance(point, comet);
                if (distance >= tuning.minSpawnDistanceFromComet) return point;
                if (distance > bestDistance)
                {
                    bestDistance = distance;
                    best = point;
                }
            }

            return best;
        }

        private void Stop()
        {
            CurrentPhase = Phase.Stopped;
            pending.Clear();
        }

        #endregion

    }

}
