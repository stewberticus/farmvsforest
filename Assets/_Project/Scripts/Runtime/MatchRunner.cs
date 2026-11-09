using System;
using System.Collections.Generic;
using FarmVsForest.Core;
using UnityEngine;

namespace FarmVsForest.Runtime
{
    /// <summary>
    /// Owns the rules engine for the scene: builds a MatchConfig from inspector settings and starts a Match.
    /// Presentation scripts read <see cref="Match"/> and listen to <see cref="Match.EventRaised"/>.
    /// </summary>
    public class MatchRunner : MonoBehaviour
    {
        [Serializable]
        public class SeatSetup
        {
            public string name = "Player";
            public Team team;
            [Tooltip("Leave empty to use the built-in placeholder deck for the team.")]
            public DeckAsset deck;
        }

        [Tooltip("Board size, hand size, cards per turn and other rule knobs.")]
        public MatchConfig rules = new MatchConfig();

        [Tooltip("One per seat, in turn order. 1v1 uses the first two.")]
        public List<SeatSetup> seats = new List<SeatSetup>
        {
            new SeatSetup { name = "Farm", team = Team.Farm },
            new SeatSetup { name = "Forest", team = Team.Forest },
            new SeatSetup { name = "Factory", team = Team.Factory },
            new SeatSetup { name = "Dark Forest", team = Team.DarkForest },
        };

        [Tooltip("Card placed in each player's corner. Losing it knocks you out. Empty uses the placeholder Home.")]
        public CardDefinitionAsset homeCard;
        public bool useHomeCards = true;
        public bool randomSeed = true;

        public Match Match { get; private set; }

        /// <summary>Raised after a new match has been created and started.</summary>
        public event Action<Match> MatchStarted;

        private void Start() => StartNewMatch();

        public void StartNewMatch()
        {
            var config = BuildConfig();
            Match = new Match(config);
            Match.EventRaised += e => Debug.Log("[Match] " + e);
            Match.Start();
            MatchStarted?.Invoke(Match);
        }

        public MatchConfig BuildConfig()
        {
            // Copy the rule knobs so a running match isn't affected by inspector edits.
            var config = JsonUtility.FromJson<MatchConfig>(JsonUtility.ToJson(rules));
            if (randomSeed) config.seed = Environment.TickCount;

            config.homeCard = !useHomeCards ? null : homeCard != null ? homeCard.card : PlaceholderCards.Home();

            int count = GameModes.PlayerCount(config.mode);
            if (seats.Count < count)
                throw new InvalidOperationException(config.mode + " needs " + count + " seats set up on " + name + ".");

            config.players = new List<PlayerSetup>();
            for (int i = 0; i < count; i++)
            {
                var s = seats[i];
                var deck = s.deck != null && s.deck.Count > 0 ? s.deck.ToDefinitions() : PlaceholderCards.StarterDeck(s.team);
                config.players.Add(new PlayerSetup(s.name, s.team, deck));
            }
            return config;
        }
    }
}
