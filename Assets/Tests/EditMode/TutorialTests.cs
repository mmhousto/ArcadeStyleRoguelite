using System.Collections.Generic;
using NUnit.Framework;
using Com.MMsGamingCo.Ricomet;

// Checks tutorial hints appear in order, finish when the action is done, and never come back.
public class TutorialTests
{
    private static string Run(TutorialSequence sequence, TutorialContext context, float seconds)
    {
        string completed = null;
        for (float t = 0f; t < seconds; t += 0.1f)
            completed = sequence.Update(context, 0.1f) ?? completed;
        return completed;
    }

    [Test]
    public void StartsWithAiming()
    {
        var sequence = new TutorialSequence(TutorialSteps.All(), null);
        sequence.Update(new TutorialContext(), 0.1f);
        Assert.AreEqual("tut_aim", sequence.Current.id);
    }

    [Test]
    public void StepCompletesWhenPlayerDoesIt()
    {
        var sequence = new TutorialSequence(TutorialSteps.All(), null);
        var context = new TutorialContext();
        Run(sequence, context, 3f);
        Assert.AreEqual("tut_aim", sequence.Current.id, "Shouldn't finish before the player aims.");

        context.aimSeconds = 0.5f;
        Assert.AreEqual("tut_aim", Run(sequence, context, 0.5f));
        Assert.IsTrue(sequence.IsDone("tut_aim"));

        sequence.Update(context, 0.1f);
        Assert.AreEqual("tut_dash", sequence.Current.id);
    }

    [Test]
    public void CompletedStepsNeverShowAgain()
    {
        var sequence = new TutorialSequence(TutorialSteps.All(), new[] { "tut_aim", "tut_dash" });
        sequence.Update(new TutorialContext(), 0.1f);
        Assert.AreEqual("tut_kill", sequence.Current.id);
    }

    [Test]
    public void NewEnemyHintInterruptsBasics()
    {
        var sequence = new TutorialSequence(TutorialSteps.All(), null);
        var context = new TutorialContext();
        sequence.Update(context, 0.1f);

        context.dartSeen = true;
        sequence.Update(context, 0.1f);
        Assert.AreEqual("tut_dart", sequence.Current.id);
    }

    [Test]
    public void UpgradeHintOnlyShowsInPickerAndFinishesWhenItCloses()
    {
        var sequence = new TutorialSequence(TutorialSteps.All(), null);
        var context = new TutorialContext { pickerOpen = true };
        Run(sequence, context, 1f);
        Assert.AreEqual("tut_upgrade", sequence.Current.id);
        Assert.IsTrue(sequence.IsVisible(context));

        // Choosing closes the picker in the same moment the upgrade is taken.
        context.pickerOpen = false;
        context.upgradesTaken = 1;
        Assert.AreEqual("tut_upgrade", Run(sequence, context, 0.3f));
    }

    [Test]
    public void BasicHintsHideWhilePicking()
    {
        var sequence = new TutorialSequence(TutorialSteps.All(), new[] { "tut_upgrade" });
        var context = new TutorialContext();
        sequence.Update(context, 0.1f);

        context.pickerOpen = true;
        sequence.Update(context, 0.1f);
        Assert.IsFalse(sequence.IsVisible(context));
    }

    [Test]
    public void BossHintsCoverTheFight()
    {
        var all = new List<string>();
        foreach (TutorialStep step in TutorialSteps.All()) all.Add(step.id);
        CollectionAssert.IsSubsetOf(new[] { "tut_boss", "tut_shield", "tut_beams", "tut_phase" }, all);

        var done = new List<string>(all);
        done.Remove("tut_boss");
        var sequence = new TutorialSequence(TutorialSteps.All(), done);
        var context = new TutorialContext { bossActive = true };
        sequence.Update(context, 0.1f);
        Assert.AreEqual("tut_boss", sequence.Current.id);

        context.bossDashHits = 1;
        Assert.AreEqual("tut_boss", Run(sequence, context, 3f));
    }

    [Test]
    public void InfoHintsCompleteOnTheirOwn()
    {
        var all = new List<string>();
        foreach (TutorialStep step in TutorialSteps.All()) all.Add(step.id);
        all.Remove("tut_stop");

        var sequence = new TutorialSequence(TutorialSteps.All(), all);
        Assert.AreEqual("tut_stop", Run(sequence, new TutorialContext(), 6f));
        Assert.IsTrue(sequence.IsFinished);
    }

    [Test]
    public void StepIdsAreUnique()
    {
        var seen = new HashSet<string>();
        foreach (TutorialStep step in TutorialSteps.All())
            Assert.IsTrue(seen.Add(step.id), "Duplicate tutorial id: " + step.id);
    }

    [Test]
    public void SaveRemembersCompletedSteps()
    {
        var save = new TutorialSave();
        save.MarkCompleted("tut_aim");
        save.MarkCompleted("tut_aim");
        CollectionAssert.AreEqual(new[] { "tut_aim" }, save.completedSteps);

        save.completedSteps = null;
        save.Normalize();
        Assert.IsNotNull(save.completedSteps);
    }
}
