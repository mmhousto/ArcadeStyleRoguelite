using System;

namespace Com.MMsGamingCo.Ricomet
{

    /// <summary>
    /// One tutorial hint. It appears when trigger is true and is completed when the
    /// player does what it says (done), or after autoCompleteSeconds for hints that
    /// only explain something.
    /// </summary>
    public class TutorialStep
    {
        /// <summary>Stable ID, saved once completed. Never rename a shipped ID.</summary>
        public string id;
        public string text;
        /// <summary>Higher priority hints interrupt lower ones (e.g. a new enemy appears).</summary>
        public int priority;
        /// <summary>Shown only while the upgrade picker is open (and only those are).</summary>
        public bool duringPicker;
        public float minShowSeconds = 1.5f;
        /// <summary>0 = wait for done. Otherwise completes after this many seconds on screen.</summary>
        public float autoCompleteSeconds = 0f;
        public Func<TutorialContext, Func<string, bool>, bool> trigger;
        public Func<TutorialContext, bool> done;

        public TutorialStep(string id, string text, int priority,
            Func<TutorialContext, Func<string, bool>, bool> trigger, Func<TutorialContext, bool> done)
        {
            this.id = id;
            this.text = text;
            this.priority = priority;
            this.trigger = trigger;
            this.done = done ?? (context => false);
        }
    }

}
