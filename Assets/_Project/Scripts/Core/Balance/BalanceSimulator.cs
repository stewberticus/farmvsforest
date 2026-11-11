using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FarmVsForest.Core
{
    /// <summary>How a match ended.</summary>
    public enum EndKind
    {
        Goal,
        BoardFull,
        RoundLimit,
        Unfinished,
    }

    /// <summary>What happened to one player in one simulated match.</summary>
    public class SeatRecord
    {
        public Team Team;
        public string Bot;
        public bool Won;
        public int Squares;
        public int Turns;
        /// <summary>Turns where the bot had nothing it could afford to play (or nowhere to put it).</summary>
        public int PassTurns;
        /// <summary>0..1 progress towards the team goal when the match ended.</summary>
        public double GoalProgress;
        public List<string> CardsPlayed = new List<string>();
    }

    /// <summary>One simulated match.</summary>
    public class MatchRecord
    {
        public int Seed;
        public SeatRecord[] Seats;
        /// <summary>Seat index of the winner, or -1 for a draw.</summary>
        public int WinnerSeat = -1;
        public int Rounds;
        public EndKind End;
        public string EndReason;
        /// <summary>The round when two opponents' cards first touched, or 0 if they never did.</summary>
        public int FirstContactRound;
        /// <summary>Fraction of the board covered at the end.</summary>
        public double BoardFill;
    }

    /// <summary>Balance targets from docs/BALANCE.md. The report flags anything outside them.</summary>
    [Serializable]
    public class BalanceTargets
    {
        public int targetRounds = 20;
        /// <summary>Median game length may be this many rounds off target.</summary>
        public int roundsTolerance = 3;
        /// <summary>Alexander: games should last 20+ rounds on average. Flag a matchup whose mean is below this.</summary>
        public double minMeanRounds = 20;
        /// <summary>Each 1v1 matchup should be within this of 50%.</summary>
        public double matchupWinRateSpread = 0.05;
        /// <summary>Going first should be within this of 50%.</summary>
        public double firstSeatSpread = 0.05;
        /// <summary>At least this share of games should end by someone reaching their goal.</summary>
        public double minGoalEndings = 0.80;
        /// <summary>Unopposed ("goldfish") median win round for every team should be within this of each other.</summary>
        public int goldfishSpread = 2;
    }

    /// <summary>
    /// Plays lots of bot-vs-bot matches and reports win rates, game length and card stats.
    /// Plug in real decks with <see cref="DeckFor"/> and <see cref="StartingCardFor"/> as teams get designed.
    /// </summary>
    public class BalanceSimulator
    {
        public Func<Team, List<CardDefinition>> DeckFor = PlaceholderCards.StarterDeck;
        public Func<Team, CardDefinition> StartingCardFor = PlaceholderCards.StartingCard;
        /// <summary>Change rules for every simulated match (board size, goal numbers...).</summary>
        public Action<MatchConfig> Configure;
        public BalanceSettings Settings = new BalanceSettings();
        /// <summary>Safety net so a stalled match still ends.</summary>
        public int RoundLimit = 60;

        /// <param name="seed">Match seed: sets the deck shuffles.</param>
        /// <param name="botSeedOffset">
        /// Shifts the bots' own random seeds without changing the shuffle. Replaying a seed with a different offset
        /// gives the same deck order with different (noisy) choices, which is how the shuffle-luck number is measured.
        /// </param>
        public MatchRecord Play(Team first, Team second, string firstBot, string secondBot, int seed, int botSeedOffset = 0)
        {
            var config = new MatchConfig
            {
                mode = GameMode.OneVsOne, seed = seed, roundLimit = RoundLimit,
                cardCatalog = PlaceholderCards.All().ToList(),
            };
            config.players.Add(new PlayerSetup("Seat 1", first, DeckFor(first), StartingCardFor(first)));
            config.players.Add(new PlayerSetup("Seat 2", second, DeckFor(second), StartingCardFor(second)));
            Configure?.Invoke(config);

            var match = new Match(config);
            var record = new MatchRecord
            {
                Seed = seed,
                Seats = new[]
                {
                    new SeatRecord { Team = first, Bot = firstBot },
                    new SeatRecord { Team = second, Bot = secondBot },
                },
            };

            match.EventRaised += e =>
            {
                if (e.Type != GameEventType.CardPlayed || e.Card == null) return;
                record.Seats[e.Player.Id].CardsPlayed.Add(e.Card.Definition.id);
                if (record.FirstContactRound == 0 && match.Board.Neighbours(e.Card.Position)
                        .Select(match.Board.Get).Any(c => c != null && !c.Owner.IsAlliedWith(e.Player)))
                    record.FirstContactRound = match.Round;
            };

            match.Start();
            Settings.cardLookup = match.FindCard;
            var bots = new[] { Bots.Create(firstBot, seed * 7 + 1 + botSeedOffset, Settings),
                Bots.Create(secondBot, seed * 7 + 2 + botSeedOffset, Settings) };

            int safety = 0;
            while (!match.IsOver && safety++ < 10000)
            {
                var p = match.CurrentPlayer;
                if (p == null) break;
                bots[p.Id].PlayTurn(match);
                var seat = record.Seats[p.Id];
                seat.Turns++;
                if (p.CardsPlayedThisTurn == 0) seat.PassTurns++;
                if (!match.IsOver) match.EndTurn(p.Id);
            }

            record.Rounds = match.Round;
            record.EndReason = match.EndReason;
            record.End = !match.IsOver ? EndKind.Unfinished
                : (match.EndReason ?? "").Contains("goal") ? EndKind.Goal
                : (match.EndReason ?? "").Contains("full") ? EndKind.BoardFull
                : EndKind.RoundLimit;
            record.BoardFill = match.Board.OccupiedCount / (double)match.Board.SquareCount;
            foreach (var p in match.Players)
            {
                var seat = record.Seats[p.Id];
                seat.Squares = match.SquaresOwnedBy(p);
                seat.GoalProgress = GoalProgress(match, p);
                seat.Won = match.WinningAllianceId == p.AllianceId;
                if (seat.Won) record.WinnerSeat = p.Id;
            }
            return record;
        }

        /// <summary>Plays <paramref name="games"/> matches of a vs b, swapping who goes first every game.</summary>
        public List<MatchRecord> Series(Team a, Team b, int games, string bot = "greedy", int seed = 1)
        {
            var list = new List<MatchRecord>();
            for (int i = 0; i < games; i++)
                list.Add(i % 2 == 0 ? Play(a, b, bot, bot, seed + i) : Play(b, a, bot, bot, seed + i));
            return list;
        }

        /// <summary>
        /// Mirror match, same team on both sides, <paramref name="botA"/> against <paramref name="botB"/>, swapping seats every
        /// game. Measures skill: how often the better player wins with identical cards.
        /// </summary>
        public List<MatchRecord> SkillMirror(Team team, string botA, string botB, int games, int seed = 1)
        {
            var list = new List<MatchRecord>();
            for (int i = 0; i < games; i++)
                list.Add(i % 2 == 0 ? Play(team, team, botA, botB, seed + i) : Play(team, team, botB, botA, seed + i));
            return list;
        }

        /// <summary>Plays a team against an opponent that never plays, to see how fast it reaches its goal on its own.</summary>
        public List<MatchRecord> Goldfish(Team team, int games, string bot = "greedy", int seed = 1, int botSeedOffset = 0)
        {
            // The opponent sits second and passes; pick a different team so seat colours/corners still differ.
            var dummy = team == Team.Farm ? Team.Forest : Team.Farm;
            var list = new List<MatchRecord>();
            for (int i = 0; i < games; i++) list.Add(Play(team, dummy, bot, "pass", seed + i, botSeedOffset));
            return list;
        }

        /// <summary>
        /// Shuffle luck: plays <paramref name="games"/> solo games twice with the same deck orders but different bot
        /// seeds, and returns the correlation of the two finish rounds. Near 1 means the shuffle decides when you finish;
        /// near 0 means the choices made along the way do. Games that don't reach the goal count as the round limit.
        /// </summary>
        public double ShuffleCorrelation(Team team, int games, string bot = "greedy", int seed = 1)
        {
            var a = Goldfish(team, games, bot, seed).Select(FinishRound).ToList();
            var b = Goldfish(team, games, bot, seed, botSeedOffset: 1000003).Select(FinishRound).ToList();
            return BalanceReport.Correlation(a, b);
        }

        private int FinishRound(MatchRecord r) => r.WinnerSeat == 0 && r.End == EndKind.Goal ? r.Rounds : RoundLimit;

        public static double GoalProgress(Match match, PlayerState p)
        {
            switch (match.GoalOf(p))
            {
                case HaveResourcesGoal have:
                    return have.Types.Min(t => Math.Min(1.0, p.Resources.Get(t) / (double)have.Amount));
                case ConnectedTaggedCardsGoal group:
                    return Math.Min(1.0, group.LargestGroup(match, p) / (double)group.Amount);
                case GainInOneRoundGoal round:
                    return Math.Min(1.0, p.GainedThisRound.Get(round.Type) / (double)round.Amount);
                default:
                    return 0;
            }
        }
    }

    /// <summary>Turns simulation results into a Markdown report.</summary>
    public static class BalanceReport
    {
        public static readonly Team[] AllTeams = { Team.Farm, Team.Forest, Team.Factory, Team.DarkForest };

        public static double Median(IEnumerable<int> values) => Percentile(values, 0.5);

        /// <summary>Pearson correlation of two equal-length lists. NaN if either list doesn't vary.</summary>
        public static double Correlation(IReadOnlyList<int> a, IReadOnlyList<int> b)
        {
            int n = Math.Min(a.Count, b.Count);
            if (n < 2) return double.NaN;
            double ma = a.Take(n).Average(), mb = b.Take(n).Average();
            double cov = 0, va = 0, vb = 0;
            for (int i = 0; i < n; i++)
            {
                double da = a[i] - ma, db = b[i] - mb;
                cov += da * db;
                va += da * da;
                vb += db * db;
            }
            return va == 0 || vb == 0 ? double.NaN : cov / Math.Sqrt(va * vb);
        }

        public static double Percentile(IEnumerable<int> values, double p)
        {
            var sorted = values.OrderBy(v => v).ToList();
            if (sorted.Count == 0) return double.NaN;
            double idx = p * (sorted.Count - 1);
            int lo = (int)Math.Floor(idx), hi = (int)Math.Ceiling(idx);
            return sorted[lo] + (sorted[hi] - sorted[lo]) * (idx - lo);
        }

        private static string Pct(double v) => double.IsNaN(v) ? "-" : (v * 100).ToString("0") + "%";
        private static string Num(double v) => double.IsNaN(v) ? "-" : v.ToString("0.#");
        private static string Range(List<int> v) => v.Count == 0 ? "-" : Num(Percentile(v, 0.1)) + "-" + Num(Percentile(v, 0.9));

        public static string Matchups(Dictionary<(Team a, Team b), List<MatchRecord>> series, BalanceTargets t, List<string> flags)
        {
            var sb = new StringBuilder();
            var teams = AllTeams.Where(x => series.Keys.Any(k => k.a == x || k.b == x)).ToList();

            sb.AppendLine("### Win rate (row team vs column team)");
            sb.AppendLine();
            sb.AppendLine("| | " + string.Join(" | ", teams) + " |");
            sb.AppendLine("| --- |" + string.Concat(teams.Select(_ => " --- |")));
            foreach (var row in teams)
            {
                sb.Append("| **" + row + "** |");
                foreach (var col in teams)
                {
                    var games = Find(series, row, col);
                    sb.Append(" " + (games == null ? "" : Pct(WinRate(games, row, col))) + " |");
                }
                sb.AppendLine();
            }

            sb.AppendLine();
            sb.AppendLine("### Matchups");
            sb.AppendLine();
            sb.AppendLine("| Matchup | Games | Win A | Win B | Draw | Seat 1 wins | Mean rounds | Median rounds | p10-p90 | Goal / full / limit | First contact | Board fill |");
            sb.AppendLine("| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |");
            foreach (var kv in series)
            {
                var (a, b) = kv.Key;
                var g = kv.Value;
                int n = g.Count;
                double winA = WinRate(g, a, b), winB = a == b ? double.NaN : WinRate(g, b, a);
                double draws = g.Count(r => r.WinnerSeat < 0) / (double)n;
                double seat1 = g.Count(r => r.WinnerSeat == 0) / (double)Math.Max(1, g.Count(r => r.WinnerSeat >= 0));
                var rounds = g.Select(r => r.Rounds).ToList();
                double goal = g.Count(r => r.End == EndKind.Goal) / (double)n;
                double full = g.Count(r => r.End == EndKind.BoardFull) / (double)n;
                double limit = g.Count(r => r.End == EndKind.RoundLimit || r.End == EndKind.Unfinished) / (double)n;
                var contact = g.Where(r => r.FirstContactRound > 0).Select(r => r.FirstContactRound).ToList();
                sb.AppendLine("| " + a + " vs " + b + " | " + n + " | " + Pct(winA) + " | " + Pct(winB) + " | " + Pct(draws) +
                              " | " + Pct(seat1) + " | " + Num(rounds.Average()) + " | " + Num(Median(rounds)) + " | " + Range(rounds) + " | " + Pct(goal) + " / " + Pct(full) + " / " + Pct(limit) +
                              " | " + (contact.Count == 0 ? "never" : "round " + Num(Median(contact)) + " (" + Pct(contact.Count / (double)n) + ")") +
                              " | " + Pct(g.Average(r => r.BoardFill)) + " |");

                if (a != b && Math.Abs(winA - 0.5) > t.matchupWinRateSpread)
                    flags.Add(a + " vs " + b + ": " + a + " wins " + Pct(winA) + " (target 50% ± " + Pct(t.matchupWinRateSpread) + ").");
                if (Math.Abs(seat1 - 0.5) > t.firstSeatSpread)
                    flags.Add(a + " vs " + b + ": seat 1 wins " + Pct(seat1) + " (target 50% ± " + Pct(t.firstSeatSpread) + ").");
                if (Math.Abs(Median(rounds) - t.targetRounds) > t.roundsTolerance)
                    flags.Add(a + " vs " + b + ": median game is " + Num(Median(rounds)) + " rounds (target " + t.targetRounds + " ± " + t.roundsTolerance + ").");
                if (rounds.Average() < t.minMeanRounds)
                    flags.Add(a + " vs " + b + ": games average " + Num(rounds.Average()) + " rounds (target at least " + Num(t.minMeanRounds) + ").");
                if (goal < t.minGoalEndings)
                    flags.Add(a + " vs " + b + ": only " + Pct(goal) + " of games end on a goal (target at least " + Pct(t.minGoalEndings) + ").");
            }

            sb.AppendLine();
            sb.AppendLine("### Overall, per team");
            sb.AppendLine();
            sb.AppendLine("| Team | Games | Win rate | Avg goal progress | Pass turns | Squares at end |");
            sb.AppendLine("| --- | --- | --- | --- | --- | --- |");
            var seats = series.Where(kv => kv.Key.a != kv.Key.b).SelectMany(kv => kv.Value).SelectMany(r => r.Seats).ToList();
            foreach (var team in teams)
            {
                var mine = seats.Where(s => s.Team == team).ToList();
                if (mine.Count == 0) continue;
                sb.AppendLine("| " + team + " | " + mine.Count + " | " + Pct(mine.Count(s => s.Won) / (double)mine.Count) + " | " +
                              Pct(mine.Average(s => s.GoalProgress)) + " | " +
                              Pct(mine.Sum(s => s.PassTurns) / (double)Math.Max(1, mine.Sum(s => s.Turns))) + " | " +
                              Num(mine.Average(s => s.Squares)) + " |");
            }
            return sb.ToString();
        }

        public static string Goldfish(Dictionary<Team, List<MatchRecord>> runs, BalanceTargets t, List<string> flags)
        {
            var sb = new StringBuilder();
            sb.AppendLine("| Team | Games | Reached goal | Median round | p10-p90 | Pass turns |");
            sb.AppendLine("| --- | --- | --- | --- | --- | --- |");
            var medians = new Dictionary<Team, double>();
            foreach (var kv in runs)
            {
                var wins = kv.Value.Where(r => r.WinnerSeat == 0 && r.End == EndKind.Goal).Select(r => r.Rounds).ToList();
                var seat = kv.Value.Select(r => r.Seats[0]).ToList();
                double med = Median(wins);
                if (!double.IsNaN(med)) medians[kv.Key] = med;
                sb.AppendLine("| " + kv.Key + " | " + kv.Value.Count + " | " + Pct(wins.Count / (double)kv.Value.Count) + " | " +
                              Num(med) + " | " + Range(wins) + " | " +
                              Pct(seat.Sum(s => s.PassTurns) / (double)Math.Max(1, seat.Sum(s => s.Turns))) + " |");
                if (wins.Count < 0.95 * kv.Value.Count)
                    flags.Add("Goldfish " + kv.Key + ": reached its goal in only " + Pct(wins.Count / (double)kv.Value.Count) + " of games.");
            }
            if (medians.Count > 1 && medians.Values.Max() - medians.Values.Min() > t.goldfishSpread)
                flags.Add("Goldfish speeds differ by " + Num(medians.Values.Max() - medians.Values.Min()) + " rounds (fastest " +
                          medians.OrderBy(m => m.Value).First().Key + ", slowest " + medians.OrderBy(m => m.Value).Last().Key +
                          "; target within " + t.goldfishSpread + ").");
            return sb.ToString();
        }

        public static string Cards(IEnumerable<CardDefinition> cards, BalanceSettings s, IEnumerable<MatchRecord> games, List<string> flags)
        {
            var seats = (games ?? Enumerable.Empty<MatchRecord>()).SelectMany(r => r.Seats).ToList();
            var sb = new StringBuilder();
            sb.AppendLine("| Card | Team | Cost (V) | Value (V) | Par | Delta | Verdict | Played per game | Win rate when played |");
            sb.AppendLine("| --- | --- | --- | --- | --- | --- | --- | --- | --- |");
            foreach (var c in CardValuation.EvaluateAll(cards, s).OrderBy(c => c.Card.team).ThenByDescending(c => c.Delta))
            {
                var teamSeats = seats.Where(x => x.Team == c.Card.team).ToList();
                var withCard = teamSeats.Where(x => x.CardsPlayed.Contains(c.Card.id)).ToList();
                double perGame = teamSeats.Count == 0 ? double.NaN : teamSeats.Sum(x => x.CardsPlayed.Count(id => id == c.Card.id)) / (double)teamSeats.Count;
                double winWhen = withCard.Count == 0 ? double.NaN : withCard.Count(x => x.Won) / (double)withCard.Count;
                sb.AppendLine("| " + c.Card.displayName + " | " + c.Card.team + " | " + Num(c.Cost) + " | " + Num(c.Value) + " | " +
                              Num(c.Par) + " | " + (c.Delta >= 0 ? "+" : "") + Pct(c.Delta) + " | " + c.Verdict + " | " +
                              Num(perGame) + " | " + Pct(winWhen) + " |");
                if (c.Verdict != "on curve" && c.Verdict != "starting card") flags.Add(c.Card.displayName + " (" + c.Card.team + ") is " + c.Verdict + ": " + string.Join("; ", c.Breakdown));
            }
            return sb.ToString();
        }

        private static List<MatchRecord> Find(Dictionary<(Team a, Team b), List<MatchRecord>> series, Team a, Team b) =>
            series.TryGetValue((a, b), out var g) ? g : series.TryGetValue((b, a), out g) ? g : null;

        /// <summary>Share of games <paramref name="team"/> won in a matchup. In a mirror, counts seat-0-by-team wins as half.</summary>
        public static double WinRate(List<MatchRecord> games, Team team, Team opponent)
        {
            if (team == opponent) return games.Count(r => r.WinnerSeat >= 0) / (2.0 * games.Count);
            return games.Count(r => r.WinnerSeat >= 0 && r.Seats[r.WinnerSeat].Team == team) / (double)games.Count;
        }
    }
}
