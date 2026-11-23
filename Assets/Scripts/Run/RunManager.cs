using System;
using Com.MMsGamingCo.Core;
using UnityEngine;

namespace Com.MMsGamingCo.Ricomet
{

    /// <summary>
    /// Tracks the current run: score, combo, kills, time and game over.
    /// </summary>
    public class RunManager : MonoBehaviour
    {
        private static RunManager _instance;

        public static RunManager Instance { get { return _instance; } }

        public int Score { get; private set; }
        public int Kills { get; private set; }
        public float RunTime { get; private set; }
        public bool IsGameOver { get; private set; }
        public bool IsNewBest { get; private set; }
        public ComboTracker Combo { get; private set; }
        public PrototypeSave Save { get; private set; }

        /// <summary>Raised with the trick name when the player pulls one off, for HUD popups.</summary>
        public static event Action<string> trickPerformed;

        private PrototypeTuning tuning;

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
        }

        private void OnDisable()
        {
            Enemy.enemyKilled -= OnEnemyKilled;
            CometController.dashEnded -= OnDashEnded;
            CometHealth.cometDied -= OnCometDied;
        }

        public void Setup(PrototypeTuning newTuning)
        {
            tuning = newTuning;
            Combo = new ComboTracker(tuning.comboWindow, tuning.maxComboMultiplier);
            Combo.milestoneReached += OnComboMilestone;
            Save = SaveSystem.Load<PrototypeSave>(PrototypeSave.Slot);
        }

        private void Update()
        {
            if (IsGameOver) return;

            RunTime += Time.deltaTime;
            Combo.Tick(Time.time);
        }

        /// <summary>Starts a fresh run by reloading the scene.</summary>
        public void Restart()
        {
            SceneLoader.ReloadCurrentScene();
        }

        private void OnEnemyKilled(Enemy enemy)
        {
            if (IsGameOver) return;

            Kills++;
            Combo.RegisterKill(Time.time);
            Score += Mathf.RoundToInt(enemy.points * Combo.Multiplier);
        }

        private void OnDashEnded(int kills)
        {
            if (kills < 3) return;

            // Multi-kill bonus grows with each extra enemy.
            int bonus = (kills - 2) * 150;
            Score += Mathf.RoundToInt(bonus * Combo.Multiplier);
            trickPerformed?.Invoke(kills >= 5 ? "MEGA KILL x" + kills : "MULTI KILL x" + kills);
        }

        private void OnComboMilestone(int milestone)
        {
            ScreenShake.Add(0.3f);
            trickPerformed?.Invoke(milestone + " COMBO!");
        }

        private void OnCometDied()
        {
            IsGameOver = true;

            Save.runsPlayed++;
            IsNewBest = Score > Save.bestScore;
            Save.bestScore = Mathf.Max(Save.bestScore, Score);
            Save.bestCombo = Mathf.Max(Save.bestCombo, Combo.Best);
            Save.bestTime = Mathf.Max(Save.bestTime, RunTime);
            SaveSystem.Save(Save, PrototypeSave.Slot);
        }
    }

}
