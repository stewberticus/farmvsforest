using System.Collections.Generic;
using System.Linq;
using FarmVsForest.Core;
using UnityEngine;

namespace FarmVsForest.Runtime
{
    /// <summary>
    /// A throwaway hot-seat UI drawn with IMGUI so the rules can be played before real art and UI exist.
    /// Pick a card from the hand, click a highlighted square, then End Turn. Delete this once a proper UI exists.
    /// </summary>
    [RequireComponent(typeof(MatchRunner))]
    public class DebugMatchGUI : MonoBehaviour
    {
        public int logLines = 14;

        private MatchRunner _runner;
        private CardInstance _selected;
        private HashSet<Coord> _legal = new HashSet<Coord>();
        private CardInstance _inspected;
        private string _message = "";
        private GUIStyle _cell, _wrap;

        private static readonly Dictionary<Team, Color> TeamColors = new Dictionary<Team, Color>
        {
            { Team.Farm, new Color(0.95f, 0.8f, 0.3f) },
            { Team.Forest, new Color(0.35f, 0.75f, 0.35f) },
            { Team.Factory, new Color(0.65f, 0.65f, 0.7f) },
            { Team.DarkForest, new Color(0.6f, 0.4f, 0.8f) },
        };

        private void Awake() => _runner = GetComponent<MatchRunner>();

        private Match M => _runner != null ? _runner.Match : null;

        private void OnGUI()
        {
            if (M == null) return;
            if (_cell == null)
            {
                _cell = new GUIStyle(GUI.skin.button) { wordWrap = true, fontSize = 11 };
                _wrap = new GUIStyle(GUI.skin.label) { wordWrap = true };
            }

            float pad = 10;
            float boardSize = Mathf.Min(Screen.height - pad * 2, Screen.width * 0.6f);
            DrawBoard(new Rect(pad, pad, boardSize, boardSize));
            DrawSidePanel(new Rect(boardSize + pad * 2, pad, Screen.width - boardSize - pad * 3, Screen.height - pad * 2));
        }

        private void DrawBoard(Rect area)
        {
            var board = M.Board;
            float size = Mathf.Min(area.width / board.Width, area.height / board.Height);
            var player = M.CurrentPlayer;
            var oldColor = GUI.backgroundColor;

            for (int x = 0; x < board.Width; x++)
            for (int y = 0; y < board.Height; y++)
            {
                var c = new Coord(x, y);
                // y = 0 at the bottom of the screen.
                var r = new Rect(area.x + x * size, area.y + (board.Height - 1 - y) * size, size - 2, size - 2);
                var card = board.Get(c);

                Color bg;
                if (card != null) bg = TeamColors[card.Owner.Team];
                else if (_legal.Contains(c)) bg = Color.cyan;
                else if (M.Players.Any(p => !p.IsEliminated && M.IsInHomeZone(p, c))) bg = new Color(0.45f, 0.45f, 0.45f);
                else bg = new Color(0.25f, 0.25f, 0.25f);
                GUI.backgroundColor = bg;

                string label = card == null ? "" : card.Definition.displayName + "\n" + card.Health + "/" + card.MaxHealth;
                if (GUI.Button(r, label, _cell)) OnSquareClicked(c, card, player);
            }
            GUI.backgroundColor = oldColor;
        }

        private void OnSquareClicked(Coord c, CardInstance occupant, PlayerState player)
        {
            if (_selected != null)
            {
                var result = M.PlayCard(player.Id, _selected, c);
                _message = result == PlayResult.Ok ? "" : "Can't play: " + result;
                ClearSelection();
            }
            else
            {
                _inspected = occupant;
            }
        }

        private void DrawSidePanel(Rect area)
        {
            GUILayout.BeginArea(area);
            var p = M.CurrentPlayer;

            if (M.IsOver)
            {
                GUILayout.Label("<b>Match over.</b> " + M.Log.Last(), _wrap);
                if (GUILayout.Button("New match")) { ClearSelection(); _runner.StartNewMatch(); }
            }
            else
            {
                GUILayout.Label("Round " + M.Round + " | Turn " + M.TurnNumber + " | " + p +
                                " | Plays left: " + (M.Config.cardsPerTurn - p.CardsPlayedThisTurn));
            }

            foreach (var pl in M.Players)
            {
                string res = pl.Resources.ToString();
                GUILayout.Label((pl == p ? "> " : "   ") + pl + (pl.IsEliminated ? " [OUT]" : "") +
                                " | Side " + pl.AllianceId + " | Hand " + pl.Hand.Count + " Deck " + pl.Deck.Count +
                                " | " + (res.Length > 0 ? res : "no resources"));
            }

            GUILayout.Space(8);
            GUILayout.Label("Hand (click a card, then a cyan square):");
            foreach (var card in p.Hand.ToList())
            {
                var d = card.Definition;
                bool affordable = p.Resources.CanAfford(d.cost);
                string cost = d.IsFree ? "Free" : string.Join(", ", d.cost);
                var prev = GUI.color;
                if (!affordable) GUI.color = new Color(1, 1, 1, 0.5f);
                string prefix = card == _selected ? "▶ " : "";
                if (GUILayout.Button(prefix + d.displayName + " [" + cost + "] HP " + d.health + " - " + d.rulesText, _cell))
                    Select(card);
                GUI.color = prev;
            }

            GUILayout.BeginHorizontal();
            GUI.enabled = !M.IsOver;
            if (GUILayout.Button("End Turn")) { ClearSelection(); M.EndTurn(p.Id); }
            GUI.enabled = true;
            if (GUILayout.Button("Restart")) { ClearSelection(); _runner.StartNewMatch(); }
            GUILayout.EndHorizontal();

            if (!string.IsNullOrEmpty(_message)) GUILayout.Label(_message);

            if (_inspected != null)
                GUILayout.Label("Selected on board: " + _inspected.Definition.displayName + " (" + _inspected.Owner + ") HP " +
                                _inspected.Health + "/" + _inspected.MaxHealth + " - " + _inspected.Definition.rulesText, _wrap);

            GUILayout.Space(8);
            GUILayout.Label("Log:");
            foreach (var e in M.Log.Skip(Mathf.Max(0, M.Log.Count - logLines)))
                GUILayout.Label(e.ToString(), _wrap);

            GUILayout.EndArea();
        }

        private void Select(CardInstance card)
        {
            if (_selected == card) { ClearSelection(); return; }
            _selected = card;
            _legal = new HashSet<Coord>(M.LegalSquares(M.CurrentPlayer, card.Definition));
            var check = M.Players.Contains(card.Owner) && !card.Owner.Resources.CanAfford(card.Definition.cost);
            _message = check ? "Not enough resources for " + card.Definition.displayName : "";
        }

        private void ClearSelection()
        {
            _selected = null;
            _legal.Clear();
        }
    }
}
