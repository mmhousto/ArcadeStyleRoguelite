using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace Com.MMsGamingCo.Ricomet
{

    /// <summary>
    /// Summary of one bot run, written to Logs/Playtests in the project folder.
    /// </summary>
    public class PlaytestReport
    {
        public int seed;
        public bool victory;
        public bool reachedBoss;
        public int bossPhaseReached;
        public float timeToBoss;
        public int shieldsAtBoss;
        public float runTime;
        public int wave;
        public int score;
        public int kills;
        public int bestCombo;
        public int hitsTaken;
        public int shieldDeflects;
        public string[] picks = new string[0];
        public string[] hits = new string[0];

        public static PlaytestReport Build(int seed)
        {
            var report = new PlaytestReport { seed = seed };

            RunManager run = RunManager.Instance;
            if (run != null)
            {
                report.victory = run.IsVictory;
                report.runTime = run.RunTime;
                report.score = run.Score;
                report.kills = run.Kills;
                report.bestCombo = run.Combo.Best;
            }

            if (WaveDirector.Instance != null)
                report.wave = WaveDirector.Instance.WaveNumber;

            BotDirector bot = BotDirector.Instance;
            if (bot != null)
            {
                report.reachedBoss = bot.ReachedBoss;
                report.bossPhaseReached = bot.BossPhaseReached;
                report.timeToBoss = bot.TimeToBoss;
                report.shieldsAtBoss = bot.ShieldsAtBoss;
                report.hitsTaken = bot.HitsTaken;
                report.shieldDeflects = bot.ShieldDeflects;
                report.picks = bot.Picks.ToArray();
                report.hits = bot.HitLog.ToArray();
            }

            return report;
        }

        public string Outcome
        {
            get
            {
                if (victory) return "CLEARED SECTOR 1";
                if (reachedBoss) return "DIED AT BOSS (phase " + bossPhaseReached + ")";
                return "DIED ON WAVE " + wave;
            }
        }

        public string ToMarkdown()
        {
            var text = new StringBuilder();
            text.AppendLine("# Ricomet bot playtest, seed " + seed);
            text.AppendLine();
            text.AppendLine("**" + Outcome + "**");
            text.AppendLine();
            text.AppendLine("| | |");
            text.AppendLine("|---|---|");
            text.AppendLine("| Run time | " + FormatTime(runTime) + " |");
            text.AppendLine("| Reached boss | " + (reachedBoss ? "yes, at " + FormatTime(timeToBoss) + " with " + shieldsAtBoss + " shields" : "no") + " |");
            text.AppendLine("| Score | " + score.ToString("N0") + " |");
            text.AppendLine("| Kills | " + kills + " |");
            text.AppendLine("| Best combo | " + bestCombo + " |");
            text.AppendLine("| Hits taken | " + hitsTaken + " |");
            text.AppendLine("| Shield ricochets | " + shieldDeflects + " |");
            text.AppendLine();
            text.AppendLine("Upgrades: " + (picks.Length > 0 ? string.Join(", ", picks) : "none"));
            text.AppendLine();
            text.AppendLine("Hits: " + (hits.Length > 0 ? string.Join("; ", hits) : "none"));
            return text.ToString();
        }

        /// <summary>CSV row for summary.csv, one line per run.</summary>
        public string ToCsvRow()
        {
            return string.Join(",", DateTime.Now.ToString("s"), seed, Outcome.Replace(",", ""), runTime.ToString("0"),
                timeToBoss.ToString("0"), bossPhaseReached, score, kills, bestCombo, hitsTaken, "\"" + string.Join("; ", picks) + "\"");
        }

        /// <summary>Writes the report and appends to summary.csv. Returns the report's path.</summary>
        public string Write()
        {
            string folder = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Logs", "Playtests"));
            Directory.CreateDirectory(folder);

            string path = Path.Combine(folder, "playtest-" + DateTime.Now.ToString("yyyyMMdd-HHmmss") + "-seed" + seed + ".md");
            File.WriteAllText(path, ToMarkdown());

            string summary = Path.Combine(folder, "summary.csv");
            if (!File.Exists(summary))
                File.WriteAllText(summary, "time,seed,outcome,run_seconds,boss_seconds,boss_phase,score,kills,best_combo,hits,upgrades\n");
            File.AppendAllText(summary, ToCsvRow() + "\n");

            return path;
        }

        private static string FormatTime(float seconds)
        {
            int total = Mathf.FloorToInt(seconds);
            return (total / 60) + ":" + (total % 60).ToString("00");
        }
    }

}
