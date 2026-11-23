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
        private GUIStyle textStyle, labelStyle;

        #endregion


        #region MonoBehaviour Methods

        private void Awake()
        {
            _instance = this;
            save = Com.MMsGamingCo.Core.SaveSystem.Load<TutorialSave>(TutorialSave.Slot);
            sequence = new TutorialSequence(TutorialSteps.All(), save.completedSteps);
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

        private void OnGUI()
        {
            if (!IsRunning()) return;

            bool showHint = sequence.IsVisible(context);
            if (!showHint && doneTimer <= 0f) return;

            BuildStyles();
            GUI.matrix = Matrix4x4.Scale(new Vector3(GuiInput.Scale, GuiInput.Scale, 1f));

            float width = Mathf.Min(1100f, GuiInput.Width - 120f);
            Rect safe = GuiInput.SafeRect();
            // Below the reroll button while picking; above the bottom HUD otherwise.
            float y = context.pickerOpen ? 895f : safe.yMax - 250f;
            Rect panel = new Rect((GuiInput.Width - width) * 0.5f, y, width, 135f);

            if (showHint)
            {
                GUI.color = new Color(0.02f, 0.03f, 0.08f, 0.82f);
                GUI.DrawTexture(panel, Texture2D.whiteTexture);
                GUI.color = new Color(0f, 0.94f, 1f);
                GUI.DrawTexture(new Rect(panel.x, panel.y, 8f, panel.height), Texture2D.whiteTexture);

                GUI.color = new Color(0f, 0.94f, 1f, 0.8f);
                GUI.Label(new Rect(panel.x + 30f, panel.y + 8f, 300f, 30f), "HOW TO PLAY", labelStyle);
                GUI.color = Color.white;
                GUI.Label(new Rect(panel.x + 30f, panel.y + 36f, panel.width - 60f, 95f), sequence.Current.text, textStyle);
            }
            else
            {
                GUI.color = new Color(1f, 0.9f, 0f, Mathf.Clamp01(doneTimer / 0.3f));
                GUI.Label(panel, doneText, Centered(textStyle));
            }

            GUI.color = Color.white;
            GUI.matrix = Matrix4x4.identity;
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

        private void BuildStyles()
        {
            if (textStyle != null) return;

            textStyle = new GUIStyle(GUI.skin.label) { fontSize = 38, fontStyle = FontStyle.Bold, wordWrap = true };
            textStyle.normal.textColor = Color.white;
            labelStyle = new GUIStyle(GUI.skin.label) { fontSize = 22, fontStyle = FontStyle.Bold };
            labelStyle.normal.textColor = Color.white;
        }

        private static GUIStyle Centered(GUIStyle style)
        {
            return new GUIStyle(style) { alignment = TextAnchor.MiddleCenter };
        }

        #endregion

    }

}
