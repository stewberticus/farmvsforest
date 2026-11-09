# Architecture

The rules live in a pure C# engine (`FarmVsForest.Core`) that knows nothing about Unity. Unity scripts build a
`MatchConfig`, create a `Match`, call `PlayCard` / `EndTurn`, and draw whatever the `Match` says. This keeps the
rules testable in milliseconds and leaves the door open for AI opponents, replays and online play later.

The game's design (what the rules are and why) lives in [DESIGN.md](DESIGN.md). This file is about the code.

## Core concepts

| Type | Role |
| --- | --- |
| `Match` | The engine. Owns all state, runs days and nights, enforces the rules, raises a `GameEvent` for everything that happens. |
| `MatchConfig` | Every tunable rule: mode, board sizes, hand size, draws and plays per turn, goal numbers, round limit, seed. |
| `Board`, `Coord` | Grid of squares, one card per square. `(0,0)` is bottom-left. Touching is edge to edge (`AdjacencyRule.Orthogonal`). |
| `PlayerState` | Team, corner, alliance, deck, hand, resources, starting card, resources gained this round. |
| `CardDefinition` | Static card data: id, name, team, cost, tags, list of effects. |
| `CardInstance` | One copy of a card in a match, with its position and how many days and nights it has seen. |
| `EffectDefinition` | One line of behaviour: *trigger* (OnPlay, Day, Night) + *interval/repeat* + *kind* + amount/resource. |
| `EffectRegistry` / `IEffect` | Maps effect kinds to code. Register new `IEffect`s to add mechanics. |
| `IPlacementRule` | Where cards may go. Default `TouchOwnCardPlacement`: an empty square touching one of your cards. |
| `ITeamGoal`, `TeamRules` | How each team wins, and which teams play by day or by night. |
| `IBot` (`GreedyBot`, `RandomBot`, `PassBot`) | Plays a turn for the current player. Used by the simulator; a start for AI opponents. |
| `CardValuation`, `BalanceSettings` | Prices a card in value points against the cost curve ([BALANCE.md](BALANCE.md)). |
| `BalanceSimulator`, `BalanceReport` | Plays many bot matches and writes the Markdown balance report (`Tools/BalanceSim`). |

## Round flow

Each round is a **day** then a **night**.

1. **Day starts.** Every card on the board counts the day, and its `Day` effects fire if their interval says so.
   Day players (Farm, Forest, Factory) then take turns in seat order.
2. **Night falls.** Same again for `Night` effects. Night players (Dark Forest) then take turns.
   With no night players, the night just fires its effects and the next day starts.
3. **A turn** is: draw (the very first turn of the match skips it), play up to `cardsPerTurn` cards whose cost you can
   pay onto a legal square (their `OnPlay` effects fire straight away), then `EndTurn`.

Timed effects use `interval` and `repeat`, counted from when the card was placed:

| Card text | trigger | interval | repeat |
| --- | --- | --- | --- |
| Every day | Day | 1 | true |
| Every 3 nights | Night | 3 | true |
| Once, after 2 days | Day | 2 | false |

## Winning

Goals are checked after every card play and after each day's or night's effects. The first player found to have met
their goal wins for their side (in 2v2, either partner's goal counts).

| Team | Goal class | Config |
| --- | --- | --- |
| Farm | `HaveResourcesGoal` (Wheat, Milk, Eggs) | `farmGoalEach` = 20 |
| Forest | `ConnectedTaggedCardsGoal` (cards tagged `tree`, one touching group) | `forestGoalTrees` = 30 |
| Factory | `GainInOneRoundGoal` (Goods made between one dawn and the next) | `factoryGoalGoods` = 10 |
| Dark Forest | `HaveResourcesGoal` (Ghouls) | `darkForestGoalGhouls` = 13 |

If the board fills up, the side owning the most squares wins (a tie is a draw). `roundLimit` is an optional safety net
that ends the match the same way. Pass `goalFor` to the `Match` constructor to try different goals.

## Modes and seating

| Mode | Seats | Sides | Board |
| --- | --- | --- | --- |
| 1v1 | Bottom-left, Top-right | 2 | `boardSize` (10) |
| 2v2 | BL (A), TR (B), TL (A), BR (B) | Partners share the left or right edge | `fourPlayerBoardSize` (16) |
| 1v1v1v1 | BL, TL, TR, BR (clockwise) | 4 | `fourPlayerBoardSize` (16) |

Within a day or night, players go in seat order. Change `GameModes.Seats` to try different seatings.

## Adding content

* **New card (no code)**: Assets > Create > Farm vs Forest > Card. Fill in cost, tags and effects using the built-in
  kinds: `GainResource`, `DrawCard`, `ConvertResource` (spend `fromAmount` of `fromResource` for `amount` of
  `resource`), `GainPerTouching` (per touching card of yours, optionally only those with `tag`) and `AddCardToHand`
  (put `amount` new copies of the card with id `cardId` into your hand; it still costs its normal price to play, and
  is lost if your hand is full). A card made by `AddCardToHand` must be in a deck, a starting card, or listed in
  `MatchConfig.cardCatalog` (the `MatchRunner`'s **Extra Cards** in Unity).
* **Starting cards**: set a seat's `startingCard` on the `MatchRunner`, or edit `PlaceholderCards.StartingCard`.
* **New mechanic**: implement `IEffect`, register it with `EffectRegistry.Register("my_id", ...)`, and set an effect's
  kind to `Custom` with `customEffectId = "my_id"`. Or add a value to `EffectKind` and register it in `CreateDefault`.
* **New resource**: add to `ResourceType` (give it an explicit number so saved assets keep their meaning) and to
  `TeamRules.Resources`.

## Placeholders and open questions

Everything in `PlaceholderCards` except the starting cards' names is stand-in content. Still guesses in code:

* Starting hand 4, draw 1 per turn, max hand 8 (extra draws are discarded), first player skips their first draw.
* Factory's "single turn" is measured as one round (dawn to dawn), so night-shift cards count.
* Running out of deck does nothing.
* 2v2 partners sit on the same side; `canPlaceNextToAllies` is off.
* If two goals are met at the same moment, the current player (then seat order) wins.
