using Com.MMsGamingCo.Core;
using UnityEngine;

namespace Com.MMsGamingCo.Ricomet
{

    /// <summary>
    /// Listens to gameplay events and plays the matching sounds, music and haptics, so
    /// gameplay code never has to know about audio. Kill sounds climb in pitch with the combo.
    /// </summary>
    public class GameAudioDirector : MonoBehaviour
    {
        private void OnEnable()
        {
            CometController.dashStarted += OnDash;
            CometController.dashEnded += OnDashEnded;
            CometController.obstacleDeflected += OnDeflected;
            CometHealth.cometDamaged += OnCometDamaged;
            CometHealth.cometDied += OnCometDied;
            Enemy.enemyKilled += OnEnemyKilled;
            Enemy.enemyDamaged += OnEnemyDamaged;
            WaveDirector.waveStarted += OnWaveStarted;
            WaveDirector.waveCleared += OnWaveCleared;
            WaveDirector.upgradeChosen += OnUpgradeChosen;
            WaveDirector.bossIncoming += OnBossIncoming;
            WaveDirector.sectorCleared += OnSectorCleared;
            WaveDirector.shieldRepaired += OnShieldRepaired;
            PrismBoss.beamsFired += OnBeamsFired;
            PrismBoss.phaseChanged += OnPhaseChanged;
            PrismBoss.bossDefeated += OnBossDefeated;
            RunManager.trickPerformed += OnTrick;
        }

        private void OnDisable()
        {
            CometController.dashStarted -= OnDash;
            CometController.dashEnded -= OnDashEnded;
            CometController.obstacleDeflected -= OnDeflected;
            CometHealth.cometDamaged -= OnCometDamaged;
            CometHealth.cometDied -= OnCometDied;
            Enemy.enemyKilled -= OnEnemyKilled;
            Enemy.enemyDamaged -= OnEnemyDamaged;
            WaveDirector.waveStarted -= OnWaveStarted;
            WaveDirector.waveCleared -= OnWaveCleared;
            WaveDirector.upgradeChosen -= OnUpgradeChosen;
            WaveDirector.bossIncoming -= OnBossIncoming;
            WaveDirector.sectorCleared -= OnSectorCleared;
            WaveDirector.shieldRepaired -= OnShieldRepaired;
            PrismBoss.beamsFired -= OnBeamsFired;
            PrismBoss.phaseChanged -= OnPhaseChanged;
            PrismBoss.bossDefeated -= OnBossDefeated;
            RunManager.trickPerformed -= OnTrick;
        }

        private void Start()
        {
            GameAudio.PlayMusic(GameAudio.GameMusic, 0.5f);
        }

        private void OnDash()
        {
            GameAudio.Play(GameAudio.Dash, 0.6f, 0.08f);
        }

        private void OnDashEnded(Vector2 position, int kills)
        {
            GameAudio.Play(GameAudio.Land, 0.5f);
            if (kills >= 3)
            {
                GameAudio.Play(GameAudio.MultiKill, 0.8f, 0f);
                Haptics.Impact(HapticStrength.Medium);
            }
        }

        private void OnDeflected()
        {
            GameAudio.Play(GameAudio.Deflect, 0.7f, 0.1f);
        }

        private void OnCometDamaged(int shieldsLeft)
        {
            GameAudio.Play(GameAudio.Hit, 1f, 0f);
            Haptics.Impact(HapticStrength.Heavy);
        }

        private void OnCometDied()
        {
            GameAudio.Play(GameAudio.GameOver, 1f, 0f);
            if (AudioManager.HasInstance) AudioManager.Instance.StopMusic(1.5f);
        }

        private void OnEnemyKilled(Enemy enemy)
        {
            // Each kill in a combo is a little higher, capped at an octave up.
            int combo = RunManager.Instance != null ? RunManager.Instance.Combo.Count : 0;
            float pitch = Mathf.Min(2f, 1f + combo * 0.04f);
            GameAudio.PlayPitched(GameAudio.Kill, 0.55f, pitch);
            Haptics.Impact(HapticStrength.Light);
        }

        private void OnEnemyDamaged(Enemy enemy, DamageSource source)
        {
            if (enemy is PrismBoss && enemy.IsAlive)
            {
                GameAudio.Play(GameAudio.BossHit, 0.9f, 0.05f);
                Haptics.Impact(HapticStrength.Medium);
            }
        }

        private void OnWaveStarted(int wave)
        {
            GameAudio.Play(GameAudio.WaveStart, 0.7f, 0f);
        }

        private void OnWaveCleared(int wave, int bonus)
        {
            GameAudio.Play(GameAudio.WaveClear, 0.7f, 0f);
        }

        private void OnUpgradeChosen()
        {
            GameAudio.Play(GameAudio.UpgradePick, 0.8f, 0f);
        }

        private void OnBossIncoming()
        {
            GameAudio.Play(GameAudio.BossWarning, 0.8f, 0f);
            GameAudio.PlayMusic(GameAudio.BossMusic, 1.5f);
        }

        private void OnBeamsFired()
        {
            GameAudio.Play(GameAudio.BeamFire, 0.6f, 0.05f);
        }

        private void OnPhaseChanged(int phase)
        {
            GameAudio.Play(GameAudio.BeamCharge, 0.7f, 0f);
            Haptics.Impact(HapticStrength.Medium);
        }

        private void OnBossDefeated()
        {
            GameAudio.Play(GameAudio.BossDefeat, 1f, 0f);
            Haptics.Impact(HapticStrength.Heavy);
            GameAudio.PlayMusic(GameAudio.GameMusic, 2f);
        }

        private void OnSectorCleared()
        {
            GameAudio.Play(GameAudio.Victory, 0.9f, 0f);
        }

        private void OnShieldRepaired()
        {
            GameAudio.Play(GameAudio.ShieldRepair, 0.7f, 0f);
        }

        private void OnTrick(string trick)
        {
            if (trick.Contains("COMBO")) GameAudio.Play(GameAudio.Combo, 0.7f, 0f);
        }
    }

}
