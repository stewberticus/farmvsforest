using System.Collections.Generic;

namespace FarmVsForest.Core
{
    /// <summary>
    /// A scout effect waiting for its owner to pick: "look at the top N cards, keep 1, the rest go on the bottom".
    /// Pick with <see cref="Match.ChooseScouted"/>. Ending the turn keeps the first card.
    /// </summary>
    public class ScoutChoice
    {
        public PlayerState Player { get; }
        public IReadOnlyList<CardInstance> Cards { get; }
        public CardInstance Source { get; }

        public ScoutChoice(PlayerState player, List<CardInstance> cards, CardInstance source)
        {
            Player = player;
            Cards = cards;
            Source = source;
        }
    }
}
