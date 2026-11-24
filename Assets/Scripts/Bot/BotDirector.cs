using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.MMsGamingCo.Ricomet
{

    /// <summary>
    /// Puts the playtest bot in charge: flies the comet with a BotPilot, picks
    /// upgrades with BotUpgradePicker, and records how the run went for reports.
    /// Toggle with P during play, or start it from Ricomet > Playtest in the editor.
    /// </summary>
    public class BotDirector : MonoBehaviour
    {

        #region Fields/Variables

        private static BotDirector _instance;

        public static BotDirector Instance { get { return _instance; } }

        [Tooltip("Restart a few seconds after the run ends, for watching the bot loop.")]
        public bool autoRestart = false;
        [Tooltip("Real seconds before the bot takes an upgrade, so you can see the offer.")]
        public float pickDelay = 0.6f;

        public bool IsActive { get; private set; }
        public BotPilot Pilot { get; private set; }

        // Run record
        public bool ReachedBoss { get; private set; }
        public float TimeToBoss { get; private set; }
        public int BossPhaseReached { get; private set; }
        public int ShieldsAtBoss { get; private set; }
        public int HitsTaken { get; private set; }
        public int ShieldDeflects { get; private set; }
        public List<string> Picks { get; private set; } = new List<string>();
        public List<string> HitLog { get; private set; } = new List<string>();

        private Coroutine restartCoroutine;

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
            WaveDirector.upgradeOfferReady += OnOffer;
            WaveDirector.bossIncoming += OnBossIncoming;
            PrismBoss.phaseChanged += OnPhaseChanged;
            CometHealth.cometDamaged += OnDamaged;
            CometController.obstacleDeflected += OnDeflected;
        }

        private void OnDisable()
        {
            WaveDirector.upgradeOfferReady -= OnOffer;
            WaveDirector.bossIncoming -= OnBossIncoming;
            PrismBoss.phaseChanged -= OnPhaseChanged;
            CometHealth.cometDamaged -= OnDamaged;
            CometController.obstacleDeflected -= OnDeflected;
        }

        private void Update()
        {
            if (!IsActive || !autoRestart || restartCoroutine != null) return;

            RunManager run = RunManager.Instance;
            if (run != null && run.IsOver)
                restartCoroutine = StartCoroutine(RestartSoon());
        }

        #endregion


        #region Public Methods

        /// <summary>Turns the bot on or off. Off gives control back to the player.</summary>
        public void SetBotActive(bool active)
        {
            IsActive = active;
            CometController comet = CometController.Instance;
            if (comet == null) return;

            Pilot = active ? new BotPilot() : null;
            comet.SetInputSource(Pilot);

            // Take over an offer that's already waiting.
            if (active && WaveDirector.Instance != null && WaveDirector.Instance.CurrentPhase == WaveDirector.Phase.Picking)
                OnOffer(WaveDirector.Instance.CurrentOffer);
        }

        public void Toggle()
        {
            SetBotActive(!IsActive);
        }

        #endregion


        #region Private Methods

        private void OnOffer(List<UpgradeDefinition> offer)
        {
            if (IsActive) StartCoroutine(PickSoon());
        }

        private IEnumerator PickSoon()
        {
            yield return new WaitForSecondsRealtime(pickDelay);

            WaveDirector director = WaveDirector.Instance;
            if (!IsActive || director == null || director.CurrentPhase != WaveDirector.Phase.Picking) yield break;

            UpgradeDefinition choice = BotUpgradePicker.Choose(director.CurrentOffer);
            if (choice == null) yield break;

            Picks.Add(choice.displayName + " (" + choice.rarity + ")");
            director.ChooseUpgrade(choice);
        }

        private IEnumerator RestartSoon()
        {
            yield return new WaitForSecondsRealtime(3f);
            if (RunManager.Instance != null) RunManager.Instance.Restart();
        }

        private void OnBossIncoming()
        {
            ReachedBoss = true;
            TimeToBoss = RunManager.Instance != null ? RunManager.Instance.RunTime : 0f;
            BossPhaseReached = 1;
            ShieldsAtBoss = CometController.Instance != null ? CometController.Instance.Health.Shields : 0;
        }

        private void OnPhaseChanged(int phase)
        {
            BossPhaseReached = Mathf.Max(BossPhaseReached, phase);
        }

        private void OnDamaged(int shieldsLeft)
        {
            HitsTaken++;
            string where = PrismBoss.Current != null ? "boss phase " + (PrismBoss.Current.Phase + 1) :
                WaveDirector.Instance != null ? "wave " + WaveDirector.Instance.WaveNumber : "?";
            CometController comet = CometController.Instance;
            string cause = comet != null ? comet.LastHitCause : "?";
            string energy = comet != null ? comet.Energy.Current.ToString("0.0") : "?";
            HitLog.Add(where + ": " + cause + " (energy " + energy + ", decision " + (Pilot != null ? Pilot.LastDecision : "player") + ")");
        }

        private void OnDeflected()
        {
            ShieldDeflects++;
        }

        #endregion

    }

}
