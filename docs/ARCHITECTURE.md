# Architecture

The rules live in a pure C# engine (`FarmVsForest.Core`) that knows nothing about Unity. Unity scripts build a
`MatchConfig`, create a `Match`, call `PlayCard` / `EndTurn`, and draw whatever the `Match` says. This keeps the
rules testable in milliseconds and leaves the door open for AI opponents, replays and online play later.

## Core concepts

| Type | Role |
| --- | --- |
| `Match` | The engine. Owns all state, enforces the rules, raises a `GameEvent` for everything that happens. |
| `MatchConfig` | Every tunable rule: mode, board size, hand size, draws and plays per turn, home zone size, round limit, seed. |
| `Board`, `Coord` | Grid of squares, one card per square. `(0,0)` is bottom-left. Adjacency is 8-way (king moves) or orthogonal. |
| `PlayerState` | Team, corner, alliance, deck, hand, discard, resources. |
| `CardDefinition` | Static card data: id, name, team, cost, health, tags, list of effects. |
| `CardInstance` | One copy of a card in a match, with its current health and position. |
| `EffectDefinition` | One line of behaviour: *trigger* + *kind* + *target* + amount/range/resource. |
| `EffectRegistry` / `IEffect` | Maps effect kinds to code. Register new `IEffect`s to add mechanics. |
| `IPlacementRule` | Where cards may go. Default: inside your corner's home zone, or next to one of your cards. |
| `IWinCondition` | Who is knocked out. Default: lose your Home card (corner "king") and you're out; last side standing wins. |

## Turn flow

1. **Start of turn**: the current player's `OnOwnerTurnStart` effects fire (producers make resources here), then they draw.
2. **Main**: they may play up to `cardsPerTurn` cards (default 1) whose cost they can pay, onto a legal square.
   The card's `OnPlay` effects fire immediately.
3. **End of turn**: their `OnOwnerTurnEnd` effects fire, then play passes to the next player still in the game.

Effects that reduce a card to 0 health destroy it, fire its `OnDestroyed` effects, and may eliminate a player.

## Modes and seating

| Mode | Seats (turn order) | Sides |
| --- | --- | --- |
| 1v1 | Bottom-left, Top-right | 2 |
| 2v2 | BL (A), TR (B), TL (A), BR (B) | Partners share the left or right edge; turns alternate sides |
| 1v1v1v1 | BL, TL, TR, BR (clockwise) | 4 |

Change `GameModes.Seats` to try different seatings.

## Adding content

* **New card (no code)**: Assets > Create > Farm vs Forest > Card. Fill in cost, health and effects using the
  built-in kinds (`GainResource`, `Damage`, `Heal`, `DrawCard`) and targets (`Owner`, `OwnerAlliance`, `Opponents`,
  `Self`, `AdjacentCards`, `AdjacentAllies`, `AdjacentEnemies`, `AllEnemyCards`, `AllAlliedCards`).
* **New mechanic**: implement `IEffect`, register it with `EffectRegistry.Register("my_id", ...)`, and set an effect's
  kind to `Custom` with `customEffectId = "my_id"`. Or add a value to `EffectKind` and register it in `CreateDefault`.
* **New trigger**: add to `EffectTrigger` and call `FireTrigger` / `FireForPlayerCards` from the right spot in `Match`.
* **New resource**: add to `ResourceType`. Nothing else needs to change.

## Placeholders and open questions

Everything in `PlaceholderCards` is stand-in content. Design questions the code currently answers with a guess,
each one a single setting or class to change:

* Board size (8x8), home zone size (3x3), adjacency (8-way).
* Starting hand 4, draw 1 per turn, max hand 8 (extra draws are discarded), first player skips their first draw.
* Win condition: a 10-health Home card in each corner. No home card means the match only ends at the round limit.
* Running out of deck currently does nothing (no fatigue).
* Eliminated players' cards stay on the board as inert obstacles.
* 2v2 partners sit on the same side; `canPlaceNextToAllies` is off.
