# Farm vs Forest: Game Design

A cozy, whimsical, family-friendly card-placement game. Players grow their team out from a corner of a shared board,
competing for space and resources. Inspired by Carcassonne: there is **no damage or combat**.

## Decided

| Topic | Decision |
| --- | --- |
| Tone | Cozy, cute, child/family friendly. Dark Forest is cute-spooky, never scary. |
| Combat | None. Teams mostly ignore each other; the main interaction is competing for space. |
| Board | 10x10 for 1v1 (decided 2026-11-09: teams still meet). 16x16 for 4 players, to tune. |
| Start | Each player begins with one starting card in their corner. It makes 1 basic resource every day (Evil Tree: every night), decided 2026-11-11. |
| Movement | Cards never move once played. |
| Placement | A new card must touch one of your own cards, edge to edge (no diagonals). |
| Ownership | A square with your card on it is yours. |
| Turn | Draw from your deck; play one card per turn if you can pay its cost. |
| Modes | Balanced around **1v1**. 2v2 and 1v1v1v1 are fun extra modes. |
| Win | Each team has its **own goal** (below). If the board fills first, whoever owns the most squares wins. |

## Rules lifted (2026-11-11)

Alexander removed four earlier card rules as too strict. New cards may now do any of these:

* **Combine** cards or resources (for example Bats and Spiders into a Ghoul, or one card played onto another).
* **Draw cards** as a card effect.
* **Pay off later, once** (delayed one-shot effects).
* **Starting cards can have an effect.**

No damage, no destroying and no moving cards still stand. The lifted rules are being put to use in the phases of the
[implementation plan](https://claude.ai/artifact/HiebN1HdedieXB72aDCvpq) (see [BALANCE.md](BALANCE.md#12-implementation-plan-2026-11-11)).
So far: starting cards make a resource (phase 1).

## Day and night

Each round is one **day** followed by one **night**.

* Farm, Forest and Factory take their turns during the **day**.
* Dark Forest takes its turn at **night**.
* With no Dark Forest player (e.g. Farm vs Forest), everyone plays during the day and the night only fires night effects.
* Day and night matters to **every team**: each team has cards that work in the day, at night, or differently in each. Effects trigger the same way whoever is playing.
* Card effects can trigger:
  * every day, or every night,
  * every X days or every X nights (repeating),
  * once, after X days or X nights (a delayed effect).

## Teams

| Team | Starting card | Resources |
| --- | --- | --- |
| Farm | Farm (every day: 1 Wheat) | Wheat, Milk, Eggs |
| Forest | Huge Tree (every day: 1 Acorn; holds the Compost rule) | Acorns, Honey |
| Factory | Headquarters (every day: 1 Part) | Parts, Toys |
| Dark Forest | Evil Tree (every night: 1 Bat) | Bats, Spiders, Ghouls |

## Team goals

| Team | Goal |
| --- | --- |
| Farm | Have 64 Wheat, 64 Milk and 64 Eggs. |
| Forest | **Grow 24 Old Trees in one connected forest.** Trees grow Sapling > Young Tree > Old Tree on sunny mornings (when they touch an empty square). |
| Factory | Make 15 Toys in a single round (a day and its night). |
| Dark Forest | **Ghoul Party**: have 13 Ghouls at once (decided; Dark Forest is paced by its cards). |

Goal numbers come from the cross-team balance pass (2026-11-09). Alexander's targets: games last 20+ rounds on
average and no team wins more than 55% of any 1v1 matchup in the bot sim. Except the 13 Ghouls, the goal numbers
are tuned, not designed, so change them freely as cards change. See
[BALANCE.md](BALANCE.md#9-cross-team-pass-2026-11-09).

## Team designs

* **Farm**: [teams/FARM.md](teams/FARM.md). Land → Wheat → Milk and Eggs. Animals make more for every crop touching
  them, and Milk and Egg buildings add Cows, Chicken Coops and Wheat Fields to your hand.
* **Forest**: [teams/FOREST.md](teams/FOREST.md). Growth: trees grow from Sapling to Old Tree on sunny mornings (touching an
  empty square), so spreading competes with leaving light. Forest can pay Acorns to play extra trees, compost cards
  for Acorns, and spend Honey to pollinate (a tree grows a step).
* **Dark Forest**: [teams/DARK_FOREST.md](teams/DARK_FOREST.md). Haunts invite Ghouls; every morning each Ghoul eats a
  Bat or Spider, and a morning you can't feed them all, one Ghoul goes home.

## Open questions

* How each team plays differently, including its day/night rhythm (designed team by team).
* The card list for each team.
* Tuning every goal number in playtests.
