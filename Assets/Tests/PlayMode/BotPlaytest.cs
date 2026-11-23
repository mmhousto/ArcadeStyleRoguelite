using System.Collections;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Com.MMsGamingCo.Core;
using Com.MMsGamingCo.Ricomet;

// Lets the bot play Sector 1 on several seeds and writes a report for each run to Logs/Playtests.
// A failure means the bot died: check the report to see where and why.
public class BotPlaytest
{
    private const float GameSpeed = 3f;
    private const float TimeoutGameSeconds = 900f;

    private string saveFolder;

    [SetUp]
    public void SetUp()
    {
        // Never touch the player's real saves or tutorial progress.
        saveFolder = Path.Combine(Path.GetTempPath(), "RicometPlaytest_" + System.Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(saveFolder);
        SaveSystem.saveFolder = saveFolder;
    }

    [TearDown]
    public void TearDown()
    {
        PrototypeBootstrap.seedOverride = null;
        PrototypeBootstrap.botOverride = null;
        TimeController.speedMultiplier = 1f;
        SaveSystem.saveFolder = null;
        if (Directory.Exists(saveFolder))
            Directory.Delete(saveFolder, true);
    }

    [UnityTest, Timeout(900000)]
    public IEnumerator BotClearsSector1([Values(101, 202, 303)] int seed)
    {
        PrototypeBootstrap.seedOverride = seed;
        PrototypeBootstrap.botOverride = true;
        TimeController.speedMultiplier = GameSpeed;

        SceneManager.LoadScene("Prototype");
        yield return null;
        yield return null;

        RunManager run = RunManager.Instance;
        Assert.IsNotNull(run, "The Prototype scene didn't start a run.");
        Assert.IsTrue(BotDirector.Instance != null && BotDirector.Instance.IsActive, "The bot isn't flying the comet.");

        while (!run.IsOver && run.RunTime < TimeoutGameSeconds)
            yield return null;

        PlaytestReport report = PlaytestReport.Build(seed);
        string path = report.Write();
        Debug.Log(report.ToMarkdown() + "\nReport: " + path);

        Assert.IsTrue(report.reachedBoss, "Bot never reached the boss. " + report.Outcome + ". Report: " + path);
        Assert.IsTrue(report.victory, "Bot didn't clear Sector 1: " + report.Outcome + ". Report: " + path);
    }
}
