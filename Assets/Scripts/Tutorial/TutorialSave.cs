using System;
using System.Collections.Generic;
using Com.MMsGamingCo.Core;

namespace Com.MMsGamingCo.Ricomet
{

    /// <summary>
    /// Tutorial hints the player has completed. A completed hint never shows again.
    /// </summary>
    [Serializable]
    public class TutorialSave : SaveDataBase
    {
        public const string Slot = "tutorial";

        public string[] completedSteps = new string[0];

        public override void Normalize()
        {
            if (completedSteps == null) completedSteps = new string[0];
            base.Normalize();
        }

        public void MarkCompleted(string id)
        {
            var list = new List<string>(completedSteps);
            if (!list.Contains(id)) list.Add(id);
            completedSteps = list.ToArray();
        }
    }

}
