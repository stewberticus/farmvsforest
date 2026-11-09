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
  --seed N           First seed (default 1)
  --out FILE         Also write the report to FILE
";

        public static int Main(string[] args)
        {
            int games = 200, seed = 1, target = 20;
            string bot = "greedy", only = null, outFile = null;
            bool mirrors = false;
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
                var cards = teams.SelectMany(t => sim.DeckFor(t).Concat(new[] { sim.StartingCardFor(t) })).Where(c => c != null);
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

        private static void Apply(MatchConfig config, string key, string value)
        {
            var field = typeof(MatchConfig).GetField(key, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (field == null) throw new ArgumentException("MatchConfig has no field '" + key + "'");
            object parsed = field.FieldType.IsEnum ? Enum.Parse(field.FieldType, value, true) : Convert.ChangeType(value, field.FieldType);
            field.SetValue(config, parsed);
        }
    }
}
