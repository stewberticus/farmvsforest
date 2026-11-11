using System;

namespace FarmVsForest.Core
{
    /// <summary>The two halves of every round.</summary>
    public enum TimeOfDay
    {
        Day = 0,
        Night = 1,
    }

    /// <summary>When an effect fires.</summary>
    public enum EffectTrigger
    {
        /// <summary>Once, right after the card is placed.</summary>
        OnPlay = 0,
        /// <summary>When a day starts. Use <see cref="EffectDefinition.interval"/> for "every X days".</summary>
        Day = 1,
        /// <summary>When a night starts. Use <see cref="EffectDefinition.interval"/> for "every X nights".</summary>
        Night = 2,
        /// <summary>Never fires. A rule reads it while the card is on the board (e.g. "Ghouls cost 1 less").</summary>
        Passive = 3,
    }

    /// <summary>What the effect does. Add new built-ins here, or use Custom with a registered id.</summary>
    public enum EffectKind
    {
        /// <summary>Gain <c>amount</c> of <c>resource</c>.</summary>
        GainResource = 0,
        /// <summary>Draw <c>amount</c> cards.</summary>
        DrawCard = 1,
        /// <summary>Spend <c>fromAmount</c> of <c>fromResource</c> to gain <c>amount</c> of <c>resource</c>. Does nothing if you can't pay.</summary>
        ConvertResource = 2,
        /// <summary>Gain <c>amount</c> of <c>resource</c> for each of your cards touching this one that has <c>tag</c> (any card if empty).</summary>
        GainPerTouching = 3,
        /// <summary>
        /// Put <c>amount</c> new copies of the card with id <c>cardId</c> into your hand, e.g. "Every 2 days: add a Cow
        /// to your hand". The new card still costs its normal price to play. If your hand is full the card is lost.
        /// </summary>
        AddCardToHand = 4,
        /// <summary>Gain <c>amount</c> of <c>resource</c> for each empty square touching this card.</summary>
        GainPerEmptyTouching = 5,
        /// <summary>Looks up <see cref="EffectDefinition.customEffectId"/> in the EffectRegistry.</summary>
        Custom = 100,
    }

    /// <summary>Who the effect applies to.</summary>
    public enum EffectTarget
    {
        /// <summary>The card's owner.</summary>
        Owner = 0,
        /// <summary>The owner and their partner in 2v2.</summary>
        OwnerAlliance = 1,
    }

    /// <summary>
    /// One line of card behaviour, e.g. "Every 3 nights: gain 1 Ghoul" is
    /// trigger Night, interval 3, kind GainResource, resource Ghouls, amount 1.
    /// </summary>
    [Serializable]
    public class EffectDefinition
    {
        public EffectTrigger trigger = EffectTrigger.Day;
        public EffectKind kind = EffectKind.GainResource;
        public EffectTarget target = EffectTarget.Owner;
        public int amount = 1;
        public ResourceType resource;

        /// <summary>
        /// For Day and Night triggers: fire on every Nth day (or night) since the card was placed.
        /// 1 means every day. Ignored for OnPlay.
        /// </summary>
        public int interval = 1;
        /// <summary>True: fire every <see cref="interval"/> days or nights. False: fire once, after that many, then never again.</summary>
        public bool repeat = true;

        /// <summary>Used by ConvertResource: what gets spent.</summary>
        public ResourceType fromResource;
        /// <summary>Used by ConvertResource: how much gets spent.</summary>
        public int fromAmount;
        /// <summary>
        /// Used by ConvertResource: only convert if at least this much <c>fromResource</c> is left afterwards, e.g. Factory
        /// Assemblers only use spare Parts so there's always something left to build with. 0 converts whenever it can pay.
        /// </summary>
        public int fromKeep;

        /// <summary>Used by GainPerTouching: only touching cards with this tag count. Empty counts any of your cards.</summary>
        public string tag;
        /// <summary>Used by GainPerTouching: count at most this many touching cards. 0 means no limit.</summary>
        public int maxCount;

        /// <summary>Used by AddCardToHand: the id of the card to create, e.g. "farm.cow".</summary>
        public string cardId;
        /// <summary>
        /// Used by AddCardToHand: skip it if you already hold a copy of that card. Stops makers clogging your hand,
        /// since you can only play one card a turn and a full hand loses new cards.
        /// </summary>
        public bool skipIfInHand;

        /// <summary>Used when kind is Custom.</summary>
        public string customEffectId;

        public EffectDefinition() { }

        public EffectDefinition(EffectTrigger trigger, EffectKind kind, int amount, ResourceType resource = default,
            int interval = 1, bool repeat = true)
        {
            this.trigger = trigger;
            this.kind = kind;
            this.amount = amount;
            this.resource = resource;
            this.interval = interval;
            this.repeat = repeat;
        }

        /// <summary>Whether a Day/Night effect fires on the card's <paramref name="count"/>th day or night.</summary>
        public bool FiresOn(int count)
        {
            int every = interval < 1 ? 1 : interval;
            return repeat ? count > 0 && count % every == 0 : count == every;
        }
    }
}
