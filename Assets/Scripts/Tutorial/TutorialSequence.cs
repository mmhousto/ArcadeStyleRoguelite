using System.Collections.Generic;

namespace Com.MMsGamingCo.Ricomet
{

    /// <summary>
    /// Decides which tutorial hint is on screen and when it's completed.
    /// Completed hints never come back. Pure logic, driven by TutorialManager.
    /// </summary>
    public class TutorialSequence
    {
        private readonly List<TutorialStep> steps;
        private readonly HashSet<string> completed;

        /// <summary>The hint on screen, or null.</summary>
        public TutorialStep Current { get; private set; }

        /// <summary>Seconds the current hint has been on screen.</summary>
        public float ShownTime { get; private set; }

        public bool IsFinished { get { return completed.Count >= steps.Count; } }

        public TutorialSequence(IEnumerable<TutorialStep> allSteps, IEnumerable<string> alreadyCompleted)
        {
            steps = new List<TutorialStep>(allSteps);
            completed = new HashSet<string>(alreadyCompleted ?? new string[0]);
        }

        public bool IsDone(string id)
        {
            return completed.Contains(id);
        }

        /// <summary>
        /// Advances the tutorial. Returns the ID of a hint completed this update, or null.
        /// </summary>
        public string Update(TutorialContext context, float deltaTime)
        {
            TutorialStep eligible = FindEligible(context);

            // A more important hint (new enemy, picker, boss) takes over the screen.
            if (eligible != null && (Current == null || (eligible != Current && eligible.priority > Current.priority)))
            {
                Current = eligible;
                ShownTime = 0f;
            }

            if (Current == null) return null;

            // Time only counts while the hint is visible: normal hints wait while the
            // picker is open, and picker hints only show then.
            if (Current.duringPicker == context.pickerOpen)
                ShownTime += deltaTime;

            // Completion is checked even while hidden: the picker hint finishes as the picker closes.
            if (ShownTime < Current.minShowSeconds) return null;

            bool finished = Current.done(context) ||
                (Current.autoCompleteSeconds > 0f && ShownTime >= Current.autoCompleteSeconds);
            if (!finished) return null;

            string id = Current.id;
            completed.Add(id);
            Current = null;
            ShownTime = 0f;
            return id;
        }

        /// <summary>Whether the current hint should be drawn right now.</summary>
        public bool IsVisible(TutorialContext context)
        {
            return Current != null && Current.duringPicker == context.pickerOpen;
        }

        private TutorialStep FindEligible(TutorialContext context)
        {
            TutorialStep best = null;
            foreach (TutorialStep step in steps)
            {
                if (completed.Contains(step.id)) continue;
                if (step.duringPicker != context.pickerOpen) continue;
                if (!step.trigger(context, IsDone)) continue;

                if (best == null || step.priority > best.priority) best = step;
            }
            return best;
        }
    }

}
