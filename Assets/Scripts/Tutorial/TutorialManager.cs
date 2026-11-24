using UnityEngine;

namespace Com.MMsGamingCo.Ricomet
{

    /// <summary>
    /// Shows tutorial hints over gameplay. Watches what the player does, feeds it to a
    /// TutorialSequence, and saves each completed hint so it never shows again.
    /// Turned off while the playtest bot is flying.
    /// </summary>
    public class TutorialManager : MonoBehaviour
    {

        #region Fields/Variables

        private static TutorialManager _instance;

        public static TutorialManager Instance { get { return _instance; } }

        private readonly TutorialContext context = new TutorialContext();
        private TutorialSequence sequence;
        private TutorialSave save;
        private string doneText = "";
        private float doneTimer = 0f;
        private UnityEngine.UI.Image panel;
        private UnityEngine.UI.Image panelFrame;
        private UnityEngine.UI.Text headerText, hintText;

        #endregion


        #region MonoBehaviour Methods

        private void Awake()
        {
            _instance = this;
            save = Com.MMsGamingCo.Core.SaveSystem.Load<TutorialSave>(TutorialSave.Slot);
            sequence = new TutorialSequence(TutorialSteps.All(), save.completedSteps);
            BuildPanel();
        }

        private void OnDestroy()
        {
            if (_instance == this) _instance = null;
        }

        private void OnEnable()
        {
            CometController.dashStarted += OnDashStarted;
            CometController.wallBounced += OnWallBounced;
            CometController.obstacleDeflected += OnDeflected;
            Enemy.enemyKilled += OnEnemyKilled;
            Enemy.enemyDamaged += OnEnemyDamaged;
            WaveDirector.upgradeChosen += OnUpgradeChosen;
        }

        private void OnDisable()
        {
            CometController.dashStarted -= OnDashStarted;
            CometController.wallBounced -= OnWallBounced;
            CometController.obstacleDeflected -= OnDeflected;
            Enemy.enemyKilled -= OnEnemyKilled;
            Enemy.enemyDamaged -= OnEnemyDamaged;
            WaveDirector.upgradeChosen -= OnUpgradeChosen;
        }

        private void Update()
        {
            if (doneTimer > 0f) doneTimer -= Time.unscaledDeltaTime;
            if (!IsRunning()) return;

            PollContext();

            string completedId = sequence.Update(context, Time.unscaledDeltaTime);
            if (completedId != null)
            {
                save.MarkCompleted(completedId);
                Com.MMsGamingCo.Core.SaveSystem.Save(save, TutorialSave.Slot);
                doneText = "Nice!";
                doneTimer = 0.8f;
            }
        }

        private void LateUpdate()
        {
            bool running = IsRunning();
            bool showHint = running && sequence.IsVisible(context);
            bool showDone = running && !showHint && doneTimer > 0f;

            panel.gameObject.SetActive(showHint || showDone);
            if (!showHint && !showDone) return;

            // While picking, sit bottom-left beside the reroll button; otherwise bottom center.
            RectTransform rect = panel.rectTransform;
            if (context.pickerOpen)
                UIFactory.Place(rect, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(40f, 40f), new Vector2(620f, 170f));
            else
                UIFactory.Place(rect, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 110f), new Vector2(1150f, 140f));

            headerText.gameObject.SetActive(showHint);
            hintText.text = showHint ? sequence.Current.text : doneText;
            hintText.color = showHint ? UITheme.White : UITheme.Yellow;
            hintText.alignment = showHint ? TextAnchor.MiddleLeft : TextAnchor.MiddleCenter;
            // "Nice!" floats on its own, without the panel behind it.
            panel.enabled = showHint;
            if (panelFrame != null) panelFrame.enabled = showHint;
            hintText.canvasRenderer.SetAlpha(showHint ? 1f : Mathf.Clamp01(doneTimer / 0.3f));
        }

        private void BuildPanel()
        {
            Canvas canvas = UIFactory.CreateCanvas("Tutorial Canvas", 25, transform);
            RectTransform safeArea = UIFactory.CreateSafeArea(canvas.transform);

            panel = UIFactory.CreatePanel(safeArea, UITheme.Cyan, UITheme.PanelFill);
            Transform frame = panel.transform.Find("Frame");
            panelFrame = frame != null ? frame.GetComponent<UnityEngine.UI.Image>() : null;
            headerText = UIFactory.CreateText(panel.transform, "HOW TO PLAY", 22, UITheme.Cyan, TextAnchor.UpperLeft);
            headerText.rectTransform.anchorMin = new Vector2(0f, 1f);
            headerText.rectTransform.anchorMax = new Vector2(1f, 1f);
            headerText.rectTransform.pivot = new Vector2(0f, 1f);
            headerText.rectTransform.offsetMin = new Vector2(30f, -44f);
            headerText.rectTransform.offsetMax = new Vector2(-30f, -14f);

            hintText = UIFactory.CreateText(panel.transform, "", 34, UITheme.White, TextAnchor.MiddleLeft, true, false);
            hintText.rectTransform.anchorMin = Vector2.zero;
            hintText.rectTransform.anchorMax = Vector2.one;
            hintText.rectTransform.offsetMin = new Vector2(30f, 10f);
            hintText.rectTransform.offsetMax = new Vector2(-30f, -40f);

            panel.gameObject.SetActive(false);
        }

        #endregion


        #region Public Methods

        /// <summary>Forgets every completed hint so the tutorial plays again from the start.</summary>
        public void ResetTutorial()
        {
            save = new TutorialSave();
            Com.MMsGamingCo.Core.SaveSystem.Save(save, TutorialSave.Slot);
            sequence = new TutorialSequence(TutorialSteps.All(), save.completedSteps);
            doneTimer = 0f;
        }

        #endregion


        #region Private Methods

        private bool IsRunning()
        {
            CometController comet = CometController.Instance;
            RunManager run = RunManager.Instance;
            if (comet == null || run == null || run.IsOver) return false;
            return !comet.IsBotControlled && !sequence.IsFinished;
        }

        private void PollContext()
        {
            CometController comet = CometController.Instance;
            if (comet.State == CometController.CometState.Aiming && !comet.Input.InDeadZone)
                context.aimSeconds += Time.unscaledDeltaTime;

            context.bestCombo = RunManager.Instance.Combo.Best;
            context.pickerOpen = WaveDirector.Instance != null && WaveDirector.Instance.CurrentPhase == WaveDirector.Phase.Picking;

            foreach (Enemy enemy in Enemy.Active)
            {
                if (enemy.enemyId == EnemyIds.Dart) context.dartSeen = true;
                else if (enemy.enemyId == EnemyIds.Splitter) context.splitterSeen = true;
            }

            PrismBoss boss = PrismBoss.Current;
            context.bossActive = boss != null && boss.IsAlive;
            if (boss != null && boss.IsAlive)
            {
                context.bossPhase = boss.Phase + 1;
                if (boss.BeamsDangerous) context.beamsWarning = true;
            }
        }

        private void OnDashStarted()
        {
            context.dashes++;
        }

        private void OnWallBounced()
        {
            context.wallBounces++;
        }

        private void OnDeflected()
        {
            context.shieldDeflects++;
        }

        private void OnEnemyKilled(Enemy enemy)
        {
            context.kills++;
            if (enemy.enemyId == EnemyIds.Dart) context.dartKills++;
            else if (enemy.enemyId == EnemyIds.Splitter) context.splitterKills++;
        }

        private void OnEnemyDamaged(Enemy enemy, DamageSource source)
        {
            if (enemy is PrismBoss && source == DamageSource.Dash)
                context.bossDashHits++;
        }

        private void OnUpgradeChosen()
        {
            context.upgradesTaken++;
        }

        #endregion

    }

}
