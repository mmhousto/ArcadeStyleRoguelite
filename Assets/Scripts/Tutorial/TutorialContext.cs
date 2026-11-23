namespace Com.MMsGamingCo.Ricomet
{

    /// <summary>
    /// What the player has done so far this run, for deciding which tutorial hint to
    /// show and when it's done. Filled in by TutorialManager; plain data so the
    /// tutorial rules can be tested.
    /// </summary>
    public class TutorialContext
    {
        public float aimSeconds;
        public int dashes;
        public int kills;
        public int wallBounces;
        public int bestCombo;
        public bool pickerOpen;
        public int upgradesTaken;
        public bool dartSeen;
        public int dartKills;
        public bool splitterSeen;
        public int splitterKills;
        public bool bossActive;
        public int bossDashHits;
        public int shieldDeflects;
        public bool beamsWarning;
        public int bossPhase;
    }

}
