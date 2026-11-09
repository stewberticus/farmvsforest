using System;
using System.Collections.Generic;
using System.Linq;
using FarmVsForest.Core;
using UnityEngine;

namespace FarmVsForest.Runtime
{
    /// <summary>
    /// The v1 no-art demo: a setup screen (mode, team and Human/Bot per seat), the board, your hand, every team's
    /// resources and goal progress, the day/night phase, and a winner screen. Drawn with IMGUI and flat colours so it
    /// needs no assets; swap it for a real UI once art exists. All rules come from <see cref="Match"/>.
    /// </summary>
    [RequireComponent(typeof(MatchRunner))]
    public class DemoGame : MonoBehaviour
    {
        [Tooltip("Seconds a bot waits before playing and before ending its turn, so you can follow what it does.")]
        public float botDelay = 0.7f;
        [Tooltip("With two or more human players, hide the hand between turns until the next player clicks.")]
        public bool hotSeatHandoff = true;

        private enum Screen { Setup, Playing }

        private const float RefWidth = 1920f, RefHeight = 1080f;

        private MatchRunner _runner;
        private Screen _screen = Screen.Setup;

        // Setup choices
        private GameMode _mode = GameMode.OneVsOne;
        private readonly Team[] _teams = { Team.Farm, Team.Forest, Team.Factory, Team.DarkForest };
        private readonly bool[] _isBot = { false, true, true, true };

        // Play state
        private readonly Dictionary<int, IBot> _bots = new Dictionary<int, IBot>();
        private CardInstance _selected;
        private readonly HashSet<Coord> _legal = new HashSet<Coord>();
        private CardInstance _lastPlayed;
        private string _message = "";
        private float _botTimer;
        private bool _botPlayed;
        private PlayerState _handoffFor;
        private PlayerState _lastTurnPlayer;
        private readonly List<string> _log = new List<string>();
        private readonly List<Floater> _floaters = new List<Floater>();
        private Vector2 _logScroll;

        private struct Floater
        {
            public Coord Square;
            public string Text;
            public Color Color;
            public float Born;
        }

        // Drawing
        private Texture2D _white;
        private GUIStyle _label, _labelCenter, _title, _small, _smallCenter, _button, _cellText, _bold;
        private Rect _boardRect;
        private float _cellSize;

        // ------------------------------------------------------------------ palette

        private static readonly Color Ink = new Color(0.17f, 0.14f, 0.12f);
        private static readonly Color Paper = new Color(0.98f, 0.95f, 0.88f);
        private static readonly Color DayBg = new Color(0.62f, 0.82f, 0.93f);
        private static readonly Color NightBg = new Color(0.16f, 0.17f, 0.32f);
        private static readonly Color Grass = new Color(0.62f, 0.78f, 0.47f);
        private static readonly Color GrassAlt = new Color(0.58f, 0.74f, 0.44f);
        private static readonly Color Legal = new Color(1f, 0.93f, 0.45f);
        private static readonly Color Accent = new Color(0.93f, 0.48f, 0.33f);

        public static Color TeamColor(Team team)
        {
            switch (team)
            {
                case Team.Farm: return new Color(0.97f, 0.79f, 0.36f);
                case Team.Forest: return new Color(0.30f, 0.62f, 0.34f);
                case Team.Factory: return new Color(0.55f, 0.66f, 0.80f);
                default: return new Color(0.58f, 0.42f, 0.74f);
            }
        }

        public static string TeamName(Team team) => team == Team.DarkForest ? "Dark Forest" : team.ToString();

        private static string TeamBlurb(Team team)
        {
            switch (team)
            {
                case Team.Farm: return "Grow crops, raise animals.";
                case Team.Forest: return "Spread one big forest.";
                case Team.Factory: return "Build a toy workshop.";
                default: return "Plays at night. Feed the Ghouls.";
            }
        }

        // ------------------------------------------------------------------ lifecycle

        /// <summary>
        /// Press Play in any scene that has no MatchRunner (even an empty one) and the demo starts by itself.
        /// Scenes with their own MatchRunner are left alone.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (FindAnyObjectByType<MatchRunner>() != null) return;
            var go = new GameObject("Farm vs Forest Demo");
            go.AddComponent<MatchRunner>().startOnPlay = false;
            go.AddComponent<DemoGame>();
        }

        private void Awake()
        {
            _runner = GetComponent<MatchRunner>();
            _runner.startOnPlay = false;
            _runner.MatchStarted += OnMatchStarted;
        }

        private void OnDestroy()
        {
            if (_runner != null) _runner.MatchStarted -= OnMatchStarted;
        }

        private Match M => _runner != null ? _runner.Match : null;

        private void StartMatch()
        {
            int count = GameModes.PlayerCount(_mode);
            var teams = _teams.Take(count).ToList();
            var names = new List<string>();
            for (int i = 0; i < count; i++)
            {
                // "Farm", "Forest Bot"; add the seat number when two seats share a team.
                bool shared = teams.Count(t => t == teams[i]) > 1;
                names.Add(TeamName(teams[i]) + (_isBot[i] ? " Bot" : "") + (shared ? " " + (i + 1) : ""));
            }

            _bots.Clear();
            for (int i = 0; i < count; i++)
                if (_isBot[i]) _bots[i] = new GreedyBot(Environment.TickCount + i * 7919);

            _runner.StartNewMatch(_mode, teams, names);
            _screen = Screen.Playing;
        }

        private void OnMatchStarted(Match match)
        {
            ClearSelection();
            _lastPlayed = null;
            _message = "";
            _log.Clear();
            _floaters.Clear();
            _handoffFor = null;
            _lastTurnPlayer = null;
            _botTimer = 0;
            _botPlayed = false;
            match.EventRaised += OnEvent;
            foreach (var e in match.Log) OnEvent(e);
        }

        private void OnEvent(GameEvent e)
        {
            switch (e.Type)
            {
                case GameEventType.CardPlayed:
                    _lastPlayed = e.Card;
                    break;
                case GameEventType.ResourcesGained:
                    if (e.Source != null && e.Source.IsOnBoard && e.Amount > 0)
                        _floaters.Add(new Floater
                        {
                            Square = e.Source.Position, Text = "+" + e.Amount + " " + e.Resource,
                            Color = TeamColor(e.Player.Team), Born = Time.time,
                        });
                    break;
                case GameEventType.CardDrawn:
                case GameEventType.CardBurned:
                case GameEventType.TurnEnded:
                    return; // too noisy for the on-screen log (and draws would show the opponent's hand)
            }
            if (!string.IsNullOrEmpty(e.Message)) _log.Add(e.Message);
            if (_log.Count > 200) _log.RemoveRange(0, _log.Count - 200);
            _logScroll.y = float.MaxValue;
        }

        private bool IsBot(PlayerState p) => p != null && _bots.ContainsKey(p.Id);
        private int HumanCount => M == null ? 0 : M.Players.Count(p => !IsBot(p));

        private void Update()
        {
            if (_screen != Screen.Playing || M == null || M.IsOver) return;
            var p = M.CurrentPlayer;
            if (p == null) return;

            if (p != _lastTurnPlayer)
            {
                _lastTurnPlayer = p;
                _botTimer = 0;
                _botPlayed = false;
                ClearSelection();
                if (!IsBot(p) && hotSeatHandoff && HumanCount > 1) _handoffFor = p;
            }

            if (!IsBot(p)) return;

            // Bots: wait, play, wait, end turn.
            _botTimer += Time.deltaTime;
            if (!_botPlayed && _botTimer >= botDelay)
            {
                _bots[p.Id].PlayTurn(M);
                _botPlayed = true;
                _botTimer = 0;
            }
            else if (_botPlayed && _botTimer >= botDelay && !M.IsOver && M.CurrentPlayer == p)
            {
                M.EndTurn(p.Id);
            }
        }

        // ------------------------------------------------------------------ GUI

        private void OnGUI()
        {
            EnsureStyles();
            float scale = Mathf.Min(UnityEngine.Screen.width / RefWidth, UnityEngine.Screen.height / RefHeight);
            float offsetX = (UnityEngine.Screen.width - RefWidth * scale) / 2f;
            float offsetY = (UnityEngine.Screen.height - RefHeight * scale) / 2f;
            Fill(new Rect(0, 0, UnityEngine.Screen.width, UnityEngine.Screen.height),
                _screen == Screen.Playing && M != null && M.TimeOfDay == TimeOfDay.Night ? NightBg : DayBg);
            GUI.matrix = Matrix4x4.TRS(new Vector3(offsetX, offsetY, 0), Quaternion.identity, new Vector3(scale, scale, 1));

            if (_screen == Screen.Setup || M == null) DrawSetup();
            else DrawMatch();
        }

        // ------------------------------------------------------------------ setup screen

        private void DrawSetup()
        {
            var panel = new Rect(360, 90, 1200, 900);
            Panel(panel, Paper);
            GUI.Label(new Rect(panel.x, panel.y + 30, panel.width, 70), "Farm vs Forest", _title);
            GUI.Label(new Rect(panel.x, panel.y + 100, panel.width, 30),
                "Play one card a turn next to your own cards. Reach your team's goal first, or own the most squares when the board fills up.",
                _smallCenter);

            GUI.Label(new Rect(panel.x + 60, panel.y + 160, 300, 36), "Mode", _bold);
            var modes = new[] { GameMode.OneVsOne, GameMode.TwoVsTwo, GameMode.FreeForAll };
            var modeNames = new[] { "1 v 1", "2 v 2", "1 v 1 v 1 v 1" };
            for (int i = 0; i < modes.Length; i++)
            {
                var r = new Rect(panel.x + 60 + i * 230, panel.y + 200, 210, 56);
                if (Toggle(r, modeNames[i], _mode == modes[i], Accent)) _mode = modes[i];
            }
            if (_mode != GameMode.OneVsOne)
                GUI.Label(new Rect(panel.x + 760, panel.y + 200, 400, 56), "Balanced for 1v1. Bigger modes are for fun.", _small);

            int count = GameModes.PlayerCount(_mode);
            var seats = GameModes.Seats(_mode);
            for (int i = 0; i < count; i++)
            {
                float y = panel.y + 290 + i * 115;
                string side = _mode == GameMode.TwoVsTwo ? "  (Side " + (seats[i].AllianceId == 0 ? "A" : "B") + ")" : "";
                GUI.Label(new Rect(panel.x + 60, y, 300, 30), "Seat " + (i + 1) + ": " + CornerName(seats[i].Corner) + side, _bold);
                foreach (Team team in new[] { Team.Farm, Team.Forest, Team.Factory, Team.DarkForest })
                {
                    var r = new Rect(panel.x + 60 + (int)team * 200, y + 36, 185, 64);
                    if (Toggle(r, TeamName(team), _teams[i] == team, TeamColor(team))) _teams[i] = team;
                    if (_teams[i] == team)
                        GUI.Label(new Rect(r.x, r.y + 64, 400, 22), TeamBlurb(team), _small);
                }
                var who = new Rect(panel.x + 900, y + 36, 240, 64);
                if (Toggle(who, _isBot[i] ? "Bot" : "Human", true, _isBot[i] ? new Color(0.8f, 0.8f, 0.8f) : Accent))
                    _isBot[i] = !_isBot[i];
            }

            if (BigButton(new Rect(panel.x + panel.width / 2 - 160, panel.yMax - 110, 320, 76), "Start", Accent))
                StartMatch();
        }

        private static string CornerName(Corner c)
        {
            switch (c)
            {
                case Corner.BottomLeft: return "bottom-left";
                case Corner.TopLeft: return "top-left";
                case Corner.TopRight: return "top-right";
                default: return "bottom-right";
            }
        }

        // ------------------------------------------------------------------ match screen

        private void DrawMatch()
        {
            var m = M;
            var p = m.CurrentPlayer;
            bool night = m.TimeOfDay == TimeOfDay.Night;

            // Phase banner
            var banner = new Rect(20, 16, 1000, 64);
            Panel(banner, night ? new Color(0.27f, 0.28f, 0.5f) : new Color(1f, 0.86f, 0.45f));
            string phase = (night ? "Night " : "Day ") + m.Round;
            string turn = m.IsOver ? "Match over"
                : p == null ? ""
                : p.Name + "'s turn" + (IsBot(p) ? " (thinking...)" : "") + "   Plays left: " + PlaysLeftText(p);
            var bannerStyle = new GUIStyle(_bold) { fontSize = 30, normal = { textColor = night ? Paper : Ink } };
            GUI.Label(new Rect(banner.x + 24, banner.y, 260, banner.height), phase, bannerStyle);
            var turnStyle = new GUIStyle(_label) { fontSize = 24, normal = { textColor = night ? Paper : Ink } };
            GUI.Label(new Rect(banner.x + 230, banner.y, 760, banner.height), turn, turnStyle);

            // Board
            float boardPx = 1000 - 0f;
            float boardTop = 96;
            float avail = Mathf.Min(boardPx, RefHeight - boardTop - 250);
            _boardRect = new Rect(20 + (1000 - avail) / 2, boardTop, avail, avail);
            DrawBoard(m, p);

            // Side panel: players, goals, log
            DrawPlayers(m, p, new Rect(1040, 16, 860, 560));
            DrawLog(new Rect(1040, 590, 860, 210));

            // Hand + buttons
            DrawHandArea(m, p, new Rect(20, RefHeight - 270, RefWidth - 40, 254));

            if (_handoffFor != null && !m.IsOver) DrawHandoff();
            if (m.IsOver) DrawWinner(m);
        }

        private string PlaysLeftText(PlayerState p)
        {
            int left = Mathf.Max(0, M.Config.cardsPerTurn - p.CardsPlayedThisTurn);
            if (left == 0 && p.Team == Team.Forest && M.Config.forestExtraTreeAcorns > 0)
                return "0 (extra Trees cost +" + M.Config.forestExtraTreeAcorns + " Acorns)";
            return left.ToString();
        }

        private void DrawBoard(Match m, PlayerState current)
        {
            var board = m.Board;
            _cellSize = Mathf.Floor(Mathf.Min(_boardRect.width / board.Width, _boardRect.height / board.Height));
            float w = _cellSize * board.Width, h = _cellSize * board.Height;
            _boardRect = new Rect(_boardRect.x + (_boardRect.width - w) / 2, _boardRect.y, w, h);
            Panel(new Rect(_boardRect.x - 8, _boardRect.y - 8, w + 16, h + 16), new Color(0.45f, 0.33f, 0.22f));

            bool night = m.TimeOfDay == TimeOfDay.Night;
            var mouse = Event.current.mousePosition;
            CardInstance hovered = null;
            Coord? hoveredSquare = null;

            for (int x = 0; x < board.Width; x++)
            for (int y = 0; y < board.Height; y++)
            {
                var c = new Coord(x, y);
                var r = CellRect(c);
                var inner = new Rect(r.x + 1, r.y + 1, r.width - 2, r.height - 2);
                var card = board.Get(c);
                bool legal = _legal.Contains(c);

                Fill(inner, (x + y) % 2 == 0 ? Grass : GrassAlt);
                if (legal)
                {
                    float pulse = 0.55f + 0.25f * Mathf.Sin(Time.time * 5f);
                    Fill(inner, new Color(Legal.r, Legal.g, Legal.b, pulse));
                }

                if (card != null) DrawBoardCard(card, inner, m);

                if (inner.Contains(mouse))
                {
                    hovered = card;
                    hoveredSquare = c;
                    Outline(inner, legal ? Accent : new Color(1, 1, 1, 0.8f), 3);
                    if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
                    {
                        OnSquareClicked(c, current);
                        Event.current.Use();
                    }
                }
            }

            if (night) Fill(_boardRect, new Color(0.05f, 0.05f, 0.25f, 0.28f));

            DrawFloaters();

            if (hovered != null) DrawCardTooltip(hovered.Definition, hovered.Owner, mouse);
            else if (hoveredSquare.HasValue && _selected != null && !_legal.Contains(hoveredSquare.Value))
                DrawTooltipText("Must be an empty square touching one of your cards.", mouse);
        }

        private Rect CellRect(Coord c) =>
            // y = 0 is the bottom row.
            new Rect(_boardRect.x + c.x * _cellSize, _boardRect.y + (M.Board.Height - 1 - c.y) * _cellSize, _cellSize, _cellSize);

        private void DrawBoardCard(CardInstance card, Rect r, Match m)
        {
            var col = TeamColor(card.Owner.Team);
            var inset = new Rect(r.x + 3, r.y + 3, r.width - 6, r.height - 6);
            Fill(inset, col);
            // A darker band shows the owner when two players share a team.
            Fill(new Rect(inset.x, inset.yMax - 8, inset.width, 8), Color.Lerp(col, Ink, 0.35f + 0.15f * card.Owner.Id));
            if (card.Definition.HasTag(Tags.Start)) Outline(inset, Ink, 3);
            var grow = ForestCards.GrowthOf(card.Definition);
            if (grow != null)
            {
                bool sunny = m.Board.Neighbours(card.Position).Any(m.Board.IsEmpty);
                var growStyle = new GUIStyle(_cellText) { alignment = TextAnchor.LowerCenter, fontSize = Mathf.Max(9, _cellText.fontSize - 2) };
                growStyle.normal.textColor = sunny ? new Color(1f, 0.92f, 0.4f) : new Color(0.75f, 0.75f, 0.8f);
                GUI.Label(new Rect(inset.x, inset.y, inset.width, inset.height - 9),
                    (sunny ? "sun " : "shade ") + card.GrowthProgress + "/" + grow.amount, growStyle);
            }
            if (card == _lastPlayed) Outline(inset, Accent, 3);
            var style = new GUIStyle(_cellText);
            style.normal.textColor = card.Owner.Team == Team.Forest || card.Owner.Team == Team.DarkForest ? Paper : Ink;
            GUI.Label(new Rect(inset.x + 2, inset.y + 2, inset.width - 4, inset.height - 10), card.Definition.displayName, style);
        }

        private void DrawFloaters()
        {
            const float life = 1.6f;
            _floaters.RemoveAll(f => Time.time - f.Born > life);
            var style = new GUIStyle(_labelCenter) { fontSize = 18, fontStyle = FontStyle.Bold };
            foreach (var f in _floaters)
            {
                float t = (Time.time - f.Born) / life;
                var r = CellRect(f.Square);
                var pos = new Rect(r.x - 30, r.y - 10 - t * 40, r.width + 60, 28);
                var shadow = new Rect(pos.x + 2, pos.y + 2, pos.width, pos.height);
                style.normal.textColor = new Color(0, 0, 0, 0.6f * (1 - t));
                GUI.Label(shadow, f.Text, style);
                style.normal.textColor = new Color(1, 1, 1, 1 - t);
                GUI.Label(pos, f.Text, style);
            }
        }

        private void OnSquareClicked(Coord c, PlayerState current)
        {
            if (current == null || IsBot(current)) return;
            if (_selected == null)
            {
                // Forest: click one of your growing trees to pollinate it with Honey.
                var tree = M.Board.Get(c);
                if (tree != null && tree.Owner == current && ForestCards.GrowthOf(tree.Definition) != null)
                    _message = M.Pollinate(current.Id, tree)
                        ? "The bees pollinated your " + tree.Definition.displayName + "."
                        : "Pollinating costs " + M.Config.forestPollinateHoney + " Honey.";
                return;
            }
            var result = M.PlayCard(current.Id, _selected, c);
            _message = result == PlayResult.Ok ? "" : Explain(result, _selected.Definition);
            ClearSelection();
        }

        private string Explain(PlayResult r, CardDefinition def)
        {
            switch (r)
            {
                case PlayResult.CannotAfford: return "Not enough resources for " + def.displayName + ".";
                case PlayResult.InvalidSquare: return "Cards go on an empty square touching one of your cards.";
                case PlayResult.NoPlaysLeft: return "No plays left this turn. Press End Turn.";
                case PlayResult.NotYourTurn: return "It isn't your turn.";
                default: return r.ToString();
            }
        }

        private void DrawPlayers(Match m, PlayerState current, Rect area)
        {
            int n = m.Players.Count;
            float gap = 10;
            float h = (area.height - gap * (n - 1)) / n;
            for (int i = 0; i < n; i++)
            {
                var pl = m.Players[i];
                var r = new Rect(area.x, area.y + i * (h + gap), area.width, h);
                Panel(r, Paper);
                Fill(new Rect(r.x, r.y, 14, r.height), TeamColor(pl.Team));
                if (pl == current) Outline(r, Accent, 4);

                string header = pl.Name + (IsBot(pl) ? "" : HumanCount > 1 ? "" : "  (you)");
                if (m.Config.mode == GameMode.TwoVsTwo) header += "   Side " + (pl.AllianceId == 0 ? "A" : "B");
                GUI.Label(new Rect(r.x + 28, r.y + 6, 520, 32), header, _bold);
                GUI.Label(new Rect(r.x + 520, r.y + 8, 320, 28),
                    "Squares " + m.SquaresOwnedBy(pl) + "   Hand " + pl.Hand.Count + "   Deck " + pl.Deck.Count, _small);

                // Resources
                float rx = r.x + 28;
                float ry = r.y + 40;
                foreach (var type in TeamRules.Resources(pl.Team))
                {
                    string text = type + " " + pl.Resources.Get(type);
                    var size = _label.CalcSize(new GUIContent(text));
                    var chip = new Rect(rx, ry, size.x + 24, 32);
                    Fill(chip, Color.Lerp(TeamColor(pl.Team), Color.white, 0.55f));
                    GUI.Label(new Rect(chip.x + 12, chip.y, chip.width, chip.height), text, _label);
                    rx += chip.width + 8;
                }

                if (pl.Team == Team.DarkForest && pl.Resources.Get(ResourceType.Ghouls) > 0)
                {
                    int food = pl.Resources.Get(ResourceType.Bats) + pl.Resources.Get(ResourceType.Spiders);
                    int need = Mathf.Max(0, pl.Resources.Get(ResourceType.Ghouls) - pl.FreeMeals) * m.Config.ghoulFoodEach;
                    GUI.Label(new Rect(rx + 8, ry, r.xMax - rx - 16, 32), "Ghouls eat " + need + " at dawn" +
                        (food < need ? " - not enough food!" : ""), new GUIStyle(_small)
                        { alignment = TextAnchor.MiddleLeft, normal = { textColor = food < need ? new Color(0.7f, 0.2f, 0.15f) : Ink } });
                }

                // Goal
                var goal = m.GoalOf(pl);
                if (goal == null) continue;
                float frac = GoalFraction(goal, m, pl);
                float gy = ry + 40;
                if (gy + 30 > r.yMax) gy = r.yMax - 34; // squeeze in 4-player layouts
                var bar = new Rect(r.x + 28, gy, 300, 26);
                Fill(bar, new Color(0.85f, 0.82f, 0.76f));
                if (frac >= 0) Fill(new Rect(bar.x, bar.y, bar.width * Mathf.Clamp01(frac), bar.height), TeamColor(pl.Team));
                Outline(bar, Ink, 1);
                GUI.Label(new Rect(bar.xMax + 12, gy - 2, r.width - 360, 30), "Goal: " + goal.Progress(m, pl), _label);
            }
        }

        /// <summary>0..1 progress for the built-in goal types, or -1 if unknown (the text still shows).</summary>
        public static float GoalFraction(ITeamGoal goal, Match m, PlayerState p)
        {
            switch (goal)
            {
                case HaveResourcesGoal g when g.Amount > 0:
                    return g.Types.Count == 0 ? 0 : g.Types.Average(t => Mathf.Min(1f, p.Resources.Get(t) / (float)g.Amount));
                case ConnectedTaggedCardsGoal g when g.Amount > 0:
                    return g.LargestGroup(m, p) / (float)g.Amount;
                case GainInOneRoundGoal g when g.Amount > 0:
                    return p.GainedThisRound.Get(g.Type) / (float)g.Amount;
                default:
                    return goal.IsMet(m, p) ? 1 : -1;
            }
        }

        private void DrawLog(Rect area)
        {
            Panel(area, new Color(Paper.r, Paper.g, Paper.b, 0.92f));
            GUI.Label(new Rect(area.x + 16, area.y + 6, 300, 28), "What happened", _bold);
            var view = new Rect(area.x + 12, area.y + 38, area.width - 24, area.height - 46);
            float lineH = 22;
            var content = new Rect(0, 0, view.width - 20, Mathf.Max(view.height, _log.Count * lineH));
            _logScroll = GUI.BeginScrollView(view, _logScroll, content);
            for (int i = 0; i < _log.Count; i++)
                GUI.Label(new Rect(4, i * lineH, content.width, lineH), _log[i], _small);
            GUI.EndScrollView();
        }

        private void DrawHandArea(Match m, PlayerState p, Rect area)
        {
            Panel(area, new Color(0.45f, 0.33f, 0.22f));

            var buttons = new Rect(area.xMax - 250, area.y + 16, 230, area.height - 32);
            bool myTurn = p != null && !IsBot(p) && !m.IsOver && _handoffFor == null;
            GUI.enabled = myTurn;
            if (BigButton(new Rect(buttons.x, buttons.y, buttons.width, 90), "End Turn", Accent) ||
                (myTurn && Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Space))
            {
                ClearSelection();
                _message = "";
                M.EndTurn(p.Id);
                if (Event.current.type == EventType.KeyDown) Event.current.Use();
            }
            GUI.enabled = true;
            if (BigButton(new Rect(buttons.x, buttons.y + 104, buttons.width, 56), "Restart", new Color(0.9f, 0.85f, 0.75f)))
                _runner.StartNewMatch();
            if (BigButton(new Rect(buttons.x, buttons.y + 168, buttons.width, 56), "Change teams", new Color(0.9f, 0.85f, 0.75f)))
            {
                ClearSelection();
                _screen = Screen.Setup;
            }

            var handArea = new Rect(area.x + 16, area.y + 16, buttons.x - area.x - 32, area.height - 32);
            if (p == null || IsBot(p) || _handoffFor != null)
            {
                string waiting = m.IsOver ? "" : p == null ? "The " + m.TimeOfDay.ToString().ToLower() + " passes..."
                    : IsBot(p) ? p.Name + " is playing..." : "";
                GUI.Label(handArea, waiting, new GUIStyle(_labelCenter) { fontSize = 26, normal = { textColor = Paper } });
                return;
            }

            if (p.Hand.Count == 0)
                GUI.Label(handArea, "Your hand is empty. Press End Turn.", new GUIStyle(_labelCenter) { fontSize = 24, normal = { textColor = Paper } });

            float cardW = Mathf.Min(200, (handArea.width - 10 * (p.Hand.Count - 1)) / Mathf.Max(1, p.Hand.Count));
            var mouse = Event.current.mousePosition;
            for (int i = 0; i < p.Hand.Count; i++)
            {
                var card = p.Hand[i];
                var r = new Rect(handArea.x + i * (cardW + 10), handArea.y + (card == _selected ? 0 : 14), cardW, handArea.height - 40);
                var cost = m.TotalCost(p, card.Definition);
                bool playable = cost != null && p.Resources.CanAfford(cost);
                DrawHandCard(card.Definition, cost, r, playable, card == _selected);
                if (r.Contains(mouse) && Event.current.type == EventType.MouseDown && Event.current.button == 0)
                {
                    Select(card, playable, cost);
                    Event.current.Use();
                }
            }

            if (_selected != null && M.CanCompost(p.Id, _selected) &&
                GUI.Button(new Rect(handArea.xMax - 230, handArea.yMax - 28, 230, 28),
                    "Compost it (+" + M.Config.forestCompostAcorns + " Acorn)", _button))
            {
                var composted = _selected;
                ClearSelection();
                M.Compost(p.Id, composted);
                _message = "Composted " + composted.Definition.displayName + ".";
            }

            if (!string.IsNullOrEmpty(_message))
                GUI.Label(new Rect(handArea.x, handArea.yMax - 24, handArea.width, 26), _message,
                    new GUIStyle(_label) { normal = { textColor = new Color(1f, 0.85f, 0.6f) } });
            else if (_selected != null)
                GUI.Label(new Rect(handArea.x, handArea.yMax - 24, handArea.width, 26),
                    "Click a glowing square to play " + _selected.Definition.displayName + ". Click the card again to put it back.",
                    new GUIStyle(_label) { normal = { textColor = Paper } });
            else
                GUI.Label(new Rect(handArea.x, handArea.yMax - 24, handArea.width, 26),
                    p.Team == Team.Forest
                        ? "Pick a card, then a glowing square. Click a growing tree to pollinate it (" + m.Config.forestPollinateHoney + " Honey)."
                        : "Pick a card, then a glowing square. Space ends your turn.", new GUIStyle(_label) { normal = { textColor = Paper } });
        }

        private void DrawHandCard(CardDefinition def, List<ResourceAmount> cost, Rect r, bool playable, bool selected)
        {
            var col = TeamColor(def.team);
            Panel(r, Paper);
            Fill(new Rect(r.x, r.y, r.width, 40), col);
            var nameStyle = new GUIStyle(_bold) { fontSize = 18, alignment = TextAnchor.MiddleCenter, wordWrap = true };
            nameStyle.normal.textColor = def.team == Team.Forest || def.team == Team.DarkForest ? Paper : Ink;
            GUI.Label(new Rect(r.x + 4, r.y, r.width - 8, 40), def.displayName, nameStyle);

            string costText = cost == null ? "No plays left"
                : cost.Count == 0 || cost.All(c => c.amount <= 0) ? "Free"
                : string.Join(", ", cost.Where(c => c.amount > 0).Select(c => c.amount + " " + c.type));
            GUI.Label(new Rect(r.x + 8, r.y + 44, r.width - 16, 26), costText,
                new GUIStyle(_small) { fontStyle = FontStyle.Bold, normal = { textColor = playable ? Ink : new Color(0.7f, 0.2f, 0.15f) } });
            GUI.Label(new Rect(r.x + 8, r.y + 70, r.width - 16, r.height - 96), def.rulesText ?? "", _small);
            if (def.tags != null && def.tags.Count > 0)
                GUI.Label(new Rect(r.x + 8, r.yMax - 26, r.width - 16, 24), string.Join(" · ", def.tags),
                    new GUIStyle(_small) { fontStyle = FontStyle.Italic, normal = { textColor = new Color(0.4f, 0.36f, 0.3f) } });

            if (!playable) Fill(r, new Color(0.2f, 0.15f, 0.1f, 0.35f));
            Outline(r, selected ? Accent : Ink, selected ? 4 : 1);
        }

        private void Select(CardInstance card, bool playable, List<ResourceAmount> cost)
        {
            if (_selected == card) { ClearSelection(); return; }
            ClearSelection();
            if (!playable)
            {
                _message = cost == null ? "No plays left this turn. Press End Turn." : "Not enough resources for " + card.Definition.displayName + ".";
                return;
            }
            _message = "";
            _selected = card;
            foreach (var c in M.LegalSquares(M.CurrentPlayer, card.Definition)) _legal.Add(c);
            if (_legal.Count == 0) _message = "There's no empty square touching your cards for this one.";
        }

        private void ClearSelection()
        {
            _selected = null;
            _legal.Clear();
        }

        private void DrawCardTooltip(CardDefinition def, PlayerState owner, Vector2 mouse)
        {
            string cost = def.IsFree ? "Free" : string.Join(", ", def.cost.Where(c => c.amount > 0).Select(c => c.amount + " " + c.type));
            string text = def.displayName + "  (" + owner.Name + ")\n" + cost + "\n" + def.rulesText;
            DrawTooltipText(text, mouse);
        }

        private void DrawTooltipText(string text, Vector2 mouse)
        {
            var style = new GUIStyle(_label) { wordWrap = true };
            float w = 340;
            float h = style.CalcHeight(new GUIContent(text), w - 20) + 16;
            var r = new Rect(mouse.x + 18, mouse.y + 18, w, h);
            if (r.xMax > RefWidth) r.x = mouse.x - w - 10;
            if (r.yMax > RefHeight) r.y = mouse.y - h - 10;
            Panel(r, new Color(1f, 0.98f, 0.92f, 0.97f));
            Outline(r, Ink, 1);
            GUI.Label(new Rect(r.x + 10, r.y + 8, w - 20, h - 16), text, style);
        }

        private void DrawHandoff()
        {
            Fill(new Rect(0, 0, RefWidth, RefHeight), new Color(0, 0, 0, 0.55f));
            var r = new Rect(RefWidth / 2 - 330, RefHeight / 2 - 120, 660, 240);
            Panel(r, Paper);
            Fill(new Rect(r.x, r.y, r.width, 16), TeamColor(_handoffFor.Team));
            GUI.Label(new Rect(r.x, r.y + 40, r.width, 60), _handoffFor.Name + "'s turn", _title);
            if (BigButton(new Rect(r.x + r.width / 2 - 140, r.y + 130, 280, 70), "I'm ready", Accent)) _handoffFor = null;
        }

        private void DrawWinner(Match m)
        {
            Fill(new Rect(0, 0, RefWidth, RefHeight), new Color(0, 0, 0, 0.5f));
            var r = new Rect(RefWidth / 2 - 420, RefHeight / 2 - 230, 840, 460);
            Panel(r, Paper);

            var winners = m.WinningAllianceId.HasValue
                ? m.Players.Where(p => p.AllianceId == m.WinningAllianceId.Value).ToList()
                : new List<PlayerState>();
            if (winners.Count > 0) Fill(new Rect(r.x, r.y, r.width, 20), TeamColor(winners[0].Team));

            string headline = winners.Count == 0 ? "It's a draw!" : string.Join(" & ", winners.Select(w => w.Name)) + " win" + (winners.Count == 1 ? "s" : "") + "!";
            GUI.Label(new Rect(r.x, r.y + 40, r.width, 70), headline, _title);
            GUI.Label(new Rect(r.x + 40, r.y + 115, r.width - 80, 30), (m.EndReason ?? "") + " on " + (m.TimeOfDay == TimeOfDay.Night ? "night " : "day ") + m.Round + ".", _labelCenter);

            float y = r.y + 170;
            foreach (var p in m.Players)
            {
                var goal = m.GoalOf(p);
                GUI.Label(new Rect(r.x + 60, y, r.width - 120, 30),
                    p.Name + ":  " + (goal != null ? goal.Progress(m, p) : "") + "   Squares " + m.SquaresOwnedBy(p), _label);
                y += 34;
            }

            if (BigButton(new Rect(r.x + r.width / 2 - 290, r.yMax - 100, 270, 70), "Play again", Accent)) _runner.StartNewMatch();
            if (BigButton(new Rect(r.x + r.width / 2 + 20, r.yMax - 100, 270, 70), "Change teams", new Color(0.9f, 0.85f, 0.75f)))
                _screen = Screen.Setup;
        }

        // ------------------------------------------------------------------ drawing helpers

        private void EnsureStyles()
        {
            if (_white != null && _label != null) return;
            _white = Texture2D.whiteTexture;
            _label = new GUIStyle(GUI.skin.label) { fontSize = 20, normal = { textColor = Ink }, alignment = TextAnchor.MiddleLeft };
            _labelCenter = new GUIStyle(_label) { alignment = TextAnchor.MiddleCenter, wordWrap = true };
            _bold = new GUIStyle(_label) { fontStyle = FontStyle.Bold, fontSize = 22 };
            _title = new GUIStyle(_label) { fontSize = 48, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
            _small = new GUIStyle(_label) { fontSize = 16, wordWrap = true, alignment = TextAnchor.UpperLeft };
            _smallCenter = new GUIStyle(_small) { alignment = TextAnchor.MiddleCenter };
            _cellText = new GUIStyle(_label) { fontSize = 14, wordWrap = true, alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
            _button = new GUIStyle(_label) { fontSize = 24, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
        }

        private void Fill(Rect r, Color c)
        {
            var old = GUI.color;
            GUI.color = c;
            GUI.DrawTexture(r, _white);
            GUI.color = old;
        }

        private void Panel(Rect r, Color c)
        {
            Fill(new Rect(r.x + 4, r.y + 5, r.width, r.height), new Color(0, 0, 0, 0.18f));
            Fill(r, c);
        }

        private void Outline(Rect r, Color c, float t)
        {
            Fill(new Rect(r.x, r.y, r.width, t), c);
            Fill(new Rect(r.x, r.yMax - t, r.width, t), c);
            Fill(new Rect(r.x, r.y, t, r.height), c);
            Fill(new Rect(r.xMax - t, r.y, t, r.height), c);
        }

        private bool BigButton(Rect r, string text, Color c)
        {
            bool hover = GUI.enabled && r.Contains(Event.current.mousePosition);
            Panel(r, GUI.enabled ? (hover ? Color.Lerp(c, Color.white, 0.25f) : c) : new Color(0.7f, 0.68f, 0.64f));
            Outline(r, Ink, 2);
            GUI.Label(r, text, _button);
            return GUI.Button(r, GUIContent.none, GUIStyle.none);
        }

        private bool Toggle(Rect r, string text, bool on, Color c)
        {
            bool hover = r.Contains(Event.current.mousePosition);
            Fill(r, on ? c : hover ? new Color(0.92f, 0.89f, 0.82f) : new Color(0.88f, 0.85f, 0.78f));
            Outline(r, on ? Ink : new Color(0.6f, 0.56f, 0.5f), on ? 3 : 1);
            var style = new GUIStyle(_button) { fontSize = 22 };
            style.normal.textColor = on && (c == TeamColor(Team.Forest) || c == TeamColor(Team.DarkForest)) ? Paper : Ink;
            GUI.Label(r, text, style);
            return GUI.Button(r, GUIContent.none, GUIStyle.none);
        }
    }
}
