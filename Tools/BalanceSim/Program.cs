using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using FarmVsForest.Core;

namespace FarmVsForest.BalanceSim
{
    /// <summary>Command-line front end for <see cref="BalanceSimulator"/>. See docs/BALANCE.md.</summary>
    public static class Program
    {
        private const string Help = @"Farm vs Forest balance simulator

Usage: dotnet run -c Release --project Tools/BalanceSim -- [options]

  --games N          Games per matchup and per goldfish run (default 200)
  --bot NAME         greedy (default) or random
  --teams LIST       Comma-separated teams to include (default Farm,Forest,Factory,DarkForest)
  --mirrors          Also play mirror matches (Farm vs Farm...)
  --only PART        matchups, goldfish or cards (default: all three)
  --target N         Target game length in rounds (default 20)
  --set key=value    Override any MatchConfig number, e.g. --set boardSize=8 --set forestGoalTrees=19
                     (repeatable)
  --sweep key=a,b,c  Goldfish each team once per value of a MatchConfig number and print a table, e.g.
                     --teams Forest --sweep forestGoalTrees=12,14,16,18
  --skill            Does skill matter? Each team alone with the good, random and worst bot, mirror
                     matches (same team both sides) good vs random, good vs sloppy and good vs worst, and
                     the shuffle-luck number (same deck orders replayed with different bot choices).
  --compare-board A,B
                     Does the opponent matter? Plays every matchup at board size A and at size B and
                     prints the win rate and length differences. Same numbers on both means the teams
                     never get in each other's way.
  --seed N           First seed (default 1)
  --out FILE         Also write the report to FILE
";

        public static int Main(string[] args)
        {
            int games = 200, seed = 1, target = 20;
            string bot = "greedy", only = null, outFile = null;
            bool mirrors = false;
            string sweep = null;
            bool skill = false;
            string compareBoard = null;
            var teams = BalanceReport.AllTeams.ToList();
            var overrides = new List<(string key, string value)>();

            for (int i = 0; i < args.Length; i++)
            {
                string Next() => i + 1 < args.Length ? args[++i] : throw new ArgumentException(args[i] + " needs a value");
                switch (args[i])
                {
                    case "--games": games = int.Parse(Next()); break;
                    case "--bot": bot = Next(); break;
                    case "--teams": teams = Next().Split(',').Select(t => (Team)Enum.Parse(typeof(Team), t.Trim(), true)).ToList(); break;
                    case "--mirrors": mirrors = true; break;
                    case "--only": only = Next().ToLowerInvariant(); break;
                    case "--target": target = int.Parse(Next()); break;
                    case "--sweep": sweep = Next(); break;
                    case "--skill": skill = true; break;
                    case "--compare-board": compareBoard = Next(); break;
                    case "--seed": seed = int.Parse(Next()); break;
                    case "--out": outFile = Next(); break;
                    case "--set":
                        var kv = Next().Split('=');
                        overrides.Add((kv[0].Trim(), kv[1].Trim()));
                        break;
                    case "-h":
                    case "--help":
                        Console.WriteLine(Help);
                        return 0;
                    default:
                        Console.Error.WriteLine("Unknown option " + args[i] + "\n\n" + Help);
                        return 1;
                }
            }

            var settings = new BalanceSettings { targetRounds = target, horizon = target / 2.0 };
            var targets = new BalanceTargets { targetRounds = target };
            var sim = new BalanceSimulator { Settings = settings, RoundLimit = Math.Max(60, target * 3) };
            sim.Configure = config =>
            {
                foreach (var (key, value) in overrides) Apply(config, key, value);
            };

            if (skill)
            {
                var table = new StringBuilder();
                table.AppendLine("## Skill: how much do decisions matter?");
                table.AppendLine();
                table.AppendLine("Solo finish round (median, with the middle 80% of games) and how often the goal is reached by round " + sim.RoundLimit + ". " +
                                 "Mirror columns: how often the good bot beats the other bot with the same team on both sides. " +
                                 "Sloppy is the good bot with noise " + Bots.SloppyNoise + " instead of 1. " +
                                 "Shuffle luck: correlation of the solo finish round when the same deck orders are replayed with different bot choices " +
                                 "(1 = the shuffle decides everything).");
                table.AppendLine();
                table.AppendLine("| Team | Good bot | Random bot | Worst bot | Good beats random | Good beats sloppy | Good beats worst | Shuffle luck |");
                table.AppendLine("| --- | --- | --- | --- | --- | --- | --- | --- |");
                string Solo(Team t, string b)
                {
                    var runs = sim.Goldfish(t, games, b, seed);
                    var wins = runs.Where(r => r.WinnerSeat == 0 && r.End == EndKind.Goal).Select(r => r.Rounds).ToList();
                    if (wins.Count == 0) return "never (0%)";
                    return "round " + BalanceReport.Median(wins).ToString("0.#") + " (" + BalanceReport.Percentile(wins, 0.1).ToString("0") + "-" +
                           BalanceReport.Percentile(wins, 0.9).ToString("0") + "), " + (wins.Count * 100 / runs.Count) + "%";
                }
                string Beats(Team t, string good, string bad)
                {
                    var runs = sim.SkillMirror(t, good, bad, games, seed);
                    int won = runs.Count(r => r.WinnerSeat >= 0 && r.Seats[r.WinnerSeat].Bot == good);
                    return (won * 100 / runs.Count) + "%";
                }
                foreach (var t in teams)
                    table.AppendLine("| " + t + " | " + Solo(t, bot) + " | " + Solo(t, "random") + " | " + Solo(t, "worst") + " | " +
                                     Beats(t, bot, "random") + " | " + Beats(t, bot, "sloppy") + " | " + Beats(t, bot, "worst") + " | " +
                                     sim.ShuffleCorrelation(t, games, bot, seed).ToString("0.00") + " |");
                Console.WriteLine(table);
                if (outFile != null) File.WriteAllText(outFile, table.ToString());
                return 0;
            }

            if (compareBoard != null)
            {
                var sizes = compareBoard.Split(',').Select(v => int.Parse(v.Trim())).ToArray();
                if (sizes.Length != 2) throw new ArgumentException("--compare-board needs two sizes, e.g. 8,16");
                var results = new Dictionary<(Team, Team), List<MatchRecord>>[2];
                for (int k = 0; k < 2; k++)
                {
                    int size = sizes[k];
                    sim.Configure = config =>
                    {
                        foreach (var (key, value) in overrides) Apply(config, key, value);
                        config.boardSize = size;
                    };
                    results[k] = new Dictionary<(Team, Team), List<MatchRecord>>();
                    for (int a = 0; a < teams.Count; a++)
                    for (int b = a + 1; b < teams.Count; b++)
                        results[k][(teams[a], teams[b])] = sim.Series(teams[a], teams[b], games, bot, seed);
                }
                var table = new StringBuilder();
                table.AppendLine("## Does the opponent matter? Board " + sizes[0] + "x" + sizes[0] + " vs " + sizes[1] + "x" + sizes[1]);
                table.AppendLine();
                table.AppendLine("Win rate is the first team's. If nothing changes between the two sizes, the teams never get in each other's way.");
                table.AppendLine();
                table.AppendLine("| Matchup | Win " + sizes[0] + " | Win " + sizes[1] + " | Change | Mean rounds " + sizes[0] + " | Mean rounds " + sizes[1] +
                                 " | Change | Contact by round 13 (" + sizes[0] + ") | Contact by round 13 (" + sizes[1] + ") | Goal endings " + sizes[0] + " |");
                table.AppendLine("| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |");
                double totalWinShift = 0, totalLenShift = 0;
                foreach (var key in results[0].Keys)
                {
                    var (a, b) = key;
                    var g0 = results[0][key];
                    var g1 = results[1][key];
                    double w0 = BalanceReport.WinRate(g0, a, b), w1 = BalanceReport.WinRate(g1, a, b);
                    double l0 = g0.Average(r => r.Rounds), l1 = g1.Average(r => r.Rounds);
                    totalWinShift += Math.Abs(w0 - w1);
                    totalLenShift += Math.Abs(l0 - l1);
                    table.AppendLine("| " + a + " vs " + b + " | " + (w0 * 100).ToString("0") + "% | " + (w1 * 100).ToString("0") + "% | " +
                                     ((w0 - w1) * 100).ToString("+0;-0;0") + " pts | " + l0.ToString("0.0") + " | " + l1.ToString("0.0") + " | " +
                                     (l0 - l1).ToString("+0.0;-0.0;0.0") + " | " + Contact13(g0) + " | " + Contact13(g1) + " | " +
                                     (g0.Count(r => r.End == EndKind.Goal) * 100 / g0.Count) + "% |");
                }
                int n = Math.Max(1, results[0].Count);
                table.AppendLine();
                table.AppendLine("Average change: " + (totalWinShift / n * 100).ToString("0.0") + " win-rate points, " +
                                 (totalLenShift / n).ToString("0.0") + " rounds.");
                Console.WriteLine(table);
                if (outFile != null) File.WriteAllText(outFile, table.ToString());
                return 0;
            }

            if (sweep != null)
            {
                var parts = sweep.Split('=');
                string key = parts[0].Trim();
                var table = new StringBuilder();
                table.AppendLine("| Team | " + key + " | Reached goal | Median round | p10-p90 |");
                table.AppendLine("| --- | --- | --- | --- | --- |");
                foreach (var team in teams)
                foreach (var value in parts[1].Split(',').Select(v => v.Trim()))
                {
                    sim.Configure = config =>
                    {
                        foreach (var (k, v) in overrides) Apply(config, k, v);
                        Apply(config, key, value);
                    };
                    var runs = sim.Goldfish(team, games, bot, seed);
                    var wins = runs.Where(r => r.WinnerSeat == 0 && r.End == EndKind.Goal).Select(r => r.Rounds).ToList();
                    table.AppendLine("| " + team + " | " + value + " | " + (wins.Count * 100 / runs.Count) + "% | " +
                                     (wins.Count == 0 ? "-" : BalanceReport.Median(wins).ToString("0.#")) + " | " +
                                     (wins.Count == 0 ? "-" : BalanceReport.Percentile(wins, 0.1).ToString("0.#") + "-" +
                                                              BalanceReport.Percentile(wins, 0.9).ToString("0.#")) + " |");
                }
                Console.WriteLine(table);
                if (outFile != null) File.WriteAllText(outFile, table.ToString());
                return 0;
            }

            var flags = new List<string>();
            var sb = new StringBuilder();
            sb.AppendLine("# Balance report");
            sb.AppendLine();
            sb.AppendLine("Bot: " + bot + ", " + games + " games each, target " + target + " rounds" +
                          (overrides.Count > 0 ? ", overrides: " + string.Join(", ", overrides.Select(o => o.key + "=" + o.value)) : "") + ".");
            sb.AppendLine();

            var allGames = new List<MatchRecord>();
            bool Want(string part) => only == null || only == part;

            if (Want("goldfish"))
            {
                var runs = teams.ToDictionary(t => t, t => sim.Goldfish(t, games, bot, seed));
                sb.AppendLine("## Goldfish (each team alone; how fast does it reach its goal?)");
                sb.AppendLine();
                sb.AppendLine(BalanceReport.Goldfish(runs, targets, flags));
            }

            if (Want("matchups"))
            {
                var series = new Dictionary<(Team, Team), List<MatchRecord>>();
                for (int a = 0; a < teams.Count; a++)
                for (int b = mirrors ? a : a + 1; b < teams.Count; b++)
                {
                    var g = sim.Series(teams[a], teams[b], games, bot, seed);
                    series[(teams[a], teams[b])] = g;
                    allGames.AddRange(g);
                }
                sb.AppendLine("## 1v1 matchups");
                sb.AppendLine();
                sb.AppendLine(BalanceReport.Matchups(series, targets, flags));
            }

            if (Want("cards"))
            {
                var cards = teams.SelectMany(t => sim.DeckFor(t).Concat(PlaceholderCards.ForTeam(t)).Concat(new[] { sim.StartingCardFor(t) }))
                    .Where(c => c != null);
                sb.AppendLine("## Cards (value model vs par, and how they did in the matchups above)");
                sb.AppendLine();
                sb.AppendLine(BalanceReport.Cards(cards, settings, allGames, flags));
            }

            sb.AppendLine("## Flags");
            sb.AppendLine();
            if (flags.Count == 0) sb.AppendLine("Everything is inside the targets.");
            foreach (var f in flags) sb.AppendLine("* " + f);

            var report = sb.ToString();
            Console.WriteLine(report);
            if (outFile != null) File.WriteAllText(outFile, report);
            return 0;
        }

        private static string Contact13(List<MatchRecord> games) =>
            (games.Count(r => r.FirstContactRound > 0 && r.FirstContactRound <= 13) * 100 / games.Count) + "%";

        private static void Apply(MatchConfig config, string key, string value)
        {
            var field = typeof(MatchConfig).GetField(key, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (field == null) throw new ArgumentException("MatchConfig has no field '" + key + "'");
            object parsed = field.FieldType.IsEnum ? Enum.Parse(field.FieldType, value, true) : Convert.ChangeType(value, field.FieldType);
            field.SetValue(config, parsed);
        }
    }
}
