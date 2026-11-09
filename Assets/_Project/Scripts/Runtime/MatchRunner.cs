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
            [Tooltip("Card placed in this seat's corner at the start. Empty uses the team's placeholder starting card.")]
            public CardDefinitionAsset startingCard;
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

        [Tooltip("Cards that effects can add to a hand by id (e.g. a Barn adding a Cow) but that aren't in any deck. " +
                 "Cards already in a deck don't need listing here.")]
        public List<CardDefinitionAsset> extraCards = new List<CardDefinitionAsset>();

        [Tooltip("Turn off to start with an empty board; each player then begins on their corner square.")]
        public bool useStartingCards = true;
        public bool randomSeed = true;

        [Tooltip("Start a match as soon as the scene plays. Turn off when a menu (like DemoGame) starts it.")]
        public bool startOnPlay = true;

        public Match Match { get; private set; }

        /// <summary>Raised after a new match has been created and started.</summary>
        public event Action<Match> MatchStarted;

        private void Start()
        {
            if (startOnPlay) StartNewMatch();
        }

        /// <summary>Sets the mode and each seat's team, then starts a new match. Used by the demo's setup screen.</summary>
        public void StartNewMatch(GameMode mode, IList<Team> teams, IList<string> names = null)
        {
            rules.mode = mode;
            for (int i = 0; i < teams.Count; i++)
            {
                if (i >= seats.Count) seats.Add(new SeatSetup());
                var s = seats[i];
                if (s.team != teams[i])
                {
                    // A deck or starting card set in the inspector belongs to the old team.
                    s.deck = null;
                    s.startingCard = null;
                }
                s.team = teams[i];
                s.name = names != null && i < names.Count ? names[i] : teams[i].ToString();
            }
            StartNewMatch();
        }

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

            int count = GameModes.PlayerCount(config.mode);
            if (seats.Count < count)
                throw new InvalidOperationException(config.mode + " needs " + count + " seats set up on " + name + ".");

            config.cardCatalog = new List<CardDefinition>();
            foreach (var c in extraCards) if (c != null) config.cardCatalog.Add(c.card);
            foreach (Team team in Enum.GetValues(typeof(Team))) config.cardCatalog.AddRange(PlaceholderCards.ForTeam(team));

            config.players = new List<PlayerSetup>();
            for (int i = 0; i < count; i++)
            {
                var s = seats[i];
                var deck = s.deck != null && s.deck.Count > 0 ? s.deck.ToDefinitions() : PlaceholderCards.StarterDeck(s.team);
                var start = !useStartingCards ? null
                    : s.startingCard != null ? s.startingCard.card : PlaceholderCards.StartingCard(s.team);
                config.players.Add(new PlayerSetup(s.name, s.team, deck, start));
            }
            return config;
        }
    }
}
