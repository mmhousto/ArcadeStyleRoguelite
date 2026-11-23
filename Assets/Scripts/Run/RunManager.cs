using System;
using Com.MMsGamingCo.Core;
using UnityEngine;

namespace Com.MMsGamingCo.Ricomet
{

    /// <summary>
    /// Tracks the current run: score, combo, kills, time, upgrades and how it ended.
    /// </summary>
    public class RunManager : MonoBehaviour
    {

        #region Fields/Variables

        private static RunManager _instance;

        public static RunManager Instance { get { return _instance; } }

        public int Seed { get; private set; }
        public int Score { get; private set; }
        public int Kills { get; private set; }
        public float RunTime { get; private set; }
        public ComboTracker Combo { get; private set; }
        public UpgradeInventory Inventory { get; private set; }
        public PrototypeSave Save { get; private set; }

        /// <summary>True once the run has ended, in victory or defeat.</summary>
        public bool IsOver { get; private set; }
        public bool IsVictory { get; private set; }
        public bool IsNewBest { get; private set; }

        /// <summary>Raised with the trick name when the player pulls one off, for HUD popups.</summary>
        public static event Action<string> trickPerformed;

        private PrototypeTuning tuning;
        private bool runCounted = false;

        #endregion


        #region MonoBehaviour Methods

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(this.gameObject);
                return;
            }
            _instance = this;
        }

        private void OnDestroy()
        {
            if (_instance == this) _instance = null;
        }

        private void OnEnable()
        {
            Enemy.enemyKilled += OnEnemyKilled;
            CometController.dashEnded += OnDashEnded;
            CometHealth.cometDied += OnCometDied;
            WaveDirector.waveCleared += OnWaveCleared;
        }

        private void OnDisable()
        {
            Enemy.enemyKilled -= OnEnemyKilled;
            CometController.dashEnded -= OnDashEnded;
            CometHealth.cometDied -= OnCometDied;
            WaveDirector.waveCleared -= OnWaveCleared;
        }

        private void Update()
        {
            if (IsOver) return;

            RunTime += Time.deltaTime;
            Combo.Tick(Time.time);
        }

        #endregion


        #region Public Methods

        public void Setup(PrototypeTuning newTuning, int seed)
        {
            tuning = newTuning;
            Seed = seed;
            Combo = new ComboTracker(tuning.comboWindow, tuning.maxComboMultiplier);
            Combo.milestoneReached += OnComboMilestone;
            Inventory = new UpgradeInventory();
            Save = SaveSystem.Load<PrototypeSave>(PrototypeSave.Slot);
        }

        public void AddScore(int points)
        {
            if (!IsOver && points > 0)
                Score += points;
        }

        /// <summary>Adds an upgrade and pushes the new stats to the comet.</summary>
        public void AddUpgrade(string upgradeId)
        {
            if (Inventory.Add(upgradeId))
                RebuildStats();
        }

        /// <summary>Recalculates comet stats from tuning and upgrades.</summary>
        public void RebuildStats()
        {
            if (CometController.Instance != null)
                CometController.Instance.ApplyStats(CometStats.Build(tuning, Inventory));
        }

        /// <summary>Ends the run and saves results. Victory = sector cleared.</summary>
        public void EndRun(bool victory)
        {
            if (IsOver) return;

            IsOver = true;
            IsVictory = victory;

            if (!runCounted)
            {
                Save.runsPlayed++;
                runCounted = true;
            }
            if (victory) Save.sectorsCleared++;

            IsNewBest = Score > Save.bestScore;
            Save.bestScore = Mathf.Max(Save.bestScore, Score);
            Save.bestCombo = Mathf.Max(Save.bestCombo, Combo.Best);
            Save.bestTime = Mathf.Max(Save.bestTime, RunTime);
            if (WaveDirector.Instance != null)
                Save.bestWave = Mathf.Max(Save.bestWave, WaveDirector.Instance.WaveNumber);
            SaveSystem.Save(Save, PrototypeSave.Slot);
        }

        /// <summary>Keeps playing after a sector clear (endless).</summary>
        public void ContinueRun()
        {
            IsOver = false;
            IsVictory = false;
        }

        /// <summary>Starts a fresh run by reloading the scene.</summary>
        public void Restart()
        {
            SceneLoader.ReloadCurrentScene();
        }

        #endregion


        #region Private Methods

        private void OnEnemyKilled(Enemy enemy)
        {
            if (IsOver) return;

            Kills++;
            Combo.RegisterKill(Time.time);
            Score += Mathf.RoundToInt(enemy.points * Combo.Multiplier);
        }

        private void OnDashEnded(Vector2 position, int kills)
        {
            if (kills < 3) return;

            // Multi-kill bonus grows with each extra enemy.
            int bonus = (kills - 2) * 150;
            AddScore(Mathf.RoundToInt(bonus * Combo.Multiplier));
            trickPerformed?.Invoke(kills >= 5 ? "MEGA KILL x" + kills : "MULTI KILL x" + kills);
        }

        private void OnComboMilestone(int milestone)
        {
            ScreenShake.Add(0.3f);
            trickPerformed?.Invoke(milestone + " COMBO!");
        }

        private void OnWaveCleared(int waveNumber, int timeBonus)
        {
            if (timeBonus > 0)
                trickPerformed?.Invoke("FAST CLEAR +" + timeBonus);
        }

        private void OnCometDied()
        {
            EndRun(false);
        }

        #endregion

    }

}
