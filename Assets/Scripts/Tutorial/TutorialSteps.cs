namespace Com.MMsGamingCo.Ricomet
{

    /// <summary>
    /// The tutorial, from the first drag to the first boss. Hints appear in order
    /// (each needs the one before) except new-enemy, picker and boss hints, which
    /// appear the moment they become relevant.
    /// </summary>
    public static class TutorialSteps
    {
        private const int Basics = 1;
        private const int Context = 2;
        private const int Picker = 3;

        public static TutorialStep[] All()
        {
            return new[]
            {
                // Basics, in order.
                new TutorialStep("tut_aim", "Touch anywhere and drag back to aim.", Basics,
                    (c, isDone) => true,
                    c => c.aimSeconds > 0.15f),

                new TutorialStep("tut_dash", "Let go to dash. You can't be hurt while dashing.", Basics,
                    (c, isDone) => isDone("tut_aim"),
                    c => c.dashes >= 1),

                new TutorialStep("tut_kill", "Dash through enemies to destroy them.", Basics,
                    (c, isDone) => isDone("tut_dash"),
                    c => c.kills >= 1),

                new TutorialStep("tut_energy", "Each dash uses a charge (the dots above you). Kills refund energy, so chain them!", Basics,
                    (c, isDone) => isDone("tut_kill"),
                    c => c.kills >= 5) { minShowSeconds = 3f },

                new TutorialStep("tut_focus", "Time slows while you aim. The yellow ring shows how much slow motion is left.", Basics,
                    (c, isDone) => isDone("tut_energy"),
                    c => c.aimSeconds >= 1.5f) { minShowSeconds = 3f },

                new TutorialStep("tut_bounce", "Dashes bounce off walls. Aim at a wall to ricochet.", Basics,
                    (c, isDone) => isDone("tut_focus"),
                    c => c.wallBounces >= 1),

                new TutorialStep("tut_stop", "Careful: you can be hit while stopped. Landing knocks nearby enemies back.", Basics,
                    (c, isDone) => isDone("tut_bounce"),
                    null) { autoCompleteSeconds = 5f },

                new TutorialStep("tut_combo", "Kill quickly to build your combo multiplier.", Basics,
                    (c, isDone) => isDone("tut_stop"),
                    c => c.bestCombo >= 5) { minShowSeconds = 2.5f },

                // New enemies: shown when they first appear.
                new TutorialStep("tut_dart", "Darts flash an orange line, then charge. Dodge, then hit them while they recover.", Context,
                    (c, isDone) => c.dartSeen,
                    c => c.dartKills >= 1) { minShowSeconds = 3f },

                new TutorialStep("tut_splitter", "Splitters take two hits and burst into small Drifters.", Context,
                    (c, isDone) => c.splitterSeen,
                    c => c.splitterKills >= 1) { minShowSeconds = 3f },

                // Upgrade picker.
                new TutorialStep("tut_upgrade", "Pick an upgrade. Taking the same one again levels it up.", Picker,
                    (c, isDone) => c.pickerOpen,
                    c => c.upgradesTaken >= 1) { duringPicker = true, minShowSeconds = 0.5f },

                // Boss.
                new TutorialStep("tut_boss", "Boss! Dash through the gaps in the shield ring to hit the core.", Context,
                    (c, isDone) => c.bossActive,
                    c => c.bossDashHits >= 1) { minShowSeconds = 2.5f },

                new TutorialStep("tut_shield", "Solid parts of the ring bounce your dash. Bank shots off it!", Context,
                    (c, isDone) => c.bossActive && isDone("tut_boss"),
                    c => c.shieldDeflects >= 1) { minShowSeconds = 2.5f, autoCompleteSeconds = 8f },

                new TutorialStep("tut_beams", "Orange lines warn of beams. Beams and orbs only hurt you while stopped, so keep dashing.", Context,
                    (c, isDone) => c.bossActive && c.beamsWarning,
                    null) { autoCompleteSeconds = 5f },

                new TutorialStep("tut_phase", "Each phase adds gaps, beams and orbs. The ticks on the health bar mark the next phase.", Context,
                    (c, isDone) => c.bossActive && c.bossPhase >= 2,
                    null) { autoCompleteSeconds = 4.5f },
            };
        }
    }

}
