using System;
using System.Collections.Generic;
using FarmVsForest.Core;
using UnityEngine;

namespace FarmVsForest.Runtime
{
    /// <summary>A deck list. Create via Assets > Create > Farm vs Forest > Deck.</summary>
    [CreateAssetMenu(menuName = "Farm vs Forest/Deck", fileName = "NewDeck")]
    public class DeckAsset : ScriptableObject
    {
        [Serializable]
        public class Entry
        {
            public CardDefinitionAsset card;
            [Min(1)] public int copies = 1;
        }

        public Team team;
        public List<Entry> cards = new List<Entry>();

        public int Count
        {
            get
            {
                int n = 0;
                foreach (var e in cards) if (e.card != null) n += e.copies;
                return n;
            }
        }

        /// <summary>Expands the list into one CardDefinition per copy, ready for a MatchConfig.</summary>
        public List<CardDefinition> ToDefinitions()
        {
            var list = new List<CardDefinition>();
            foreach (var e in cards)
            {
                if (e.card == null) continue;
                for (int i = 0; i < e.copies; i++) list.Add(e.card.card);
            }
            return list;
        }
    }
}
