# Farm vs Forest: Game Design

A cozy, whimsical, family-friendly card-placement game. Players grow their team out from a corner of a shared board,
competing for space and resources. Inspired by Carcassonne: there is **no damage or combat**.

## Decided

| Topic | Decision |
| --- | --- |
| Tone | Cozy, cute, child/family friendly. Dark Forest is cute-spooky, never scary. |
| Combat | None. Teams mostly ignore each other; the main interaction is competing for space. |
| Board | 10x10 for 1v1 (decided 2026-11-09: teams still meet). 16x16 for 4 players, to tune. |
| Start | Each player begins with one starting card in their corner. |
| Movement | Cards never move once played. |
| Placement | A new card must touch one of your own cards, edge to edge (no diagonals). |
| Ownership | A square with your card on it is yours. |
| Turn | Draw from your deck; play one card per turn if you can pay its cost. |
| Modes | Balanced around **1v1**. 2v2 and 1v1v1v1 are fun extra modes. |
| Win | Each team has its **own goal** (below). If the board fills first, whoever owns the most squares wins. |

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
| Farm | Farm | Wheat, Milk, Eggs |
| Forest | Huge Tree | Acorns, Honey |
| Factory | Headquarters | Parts, Toys |
| Dark Forest | Evil Tree | Bats, Spiders, Ghouls |

## Team goals

| Team | Goal |
| --- | --- |
| Farm | Have 62 Wheat, 62 Milk and 62 Eggs. |
| Forest | **Grow 20 Old Trees in one connected forest.** Trees grow Sapling > Young Tree > Old Tree on sunny mornings (when they touch an empty square). |
| Factory | Make 15 Toys in a single round (a day and its night). |
| Dark Forest | **Ghoul Party**: have 13 Ghouls at once (decided; Dark Forest is paced by its cards). |

Goal numbers come from the cross-team balance pass (2026-11-09). Alexander's targets: games last 20+ rounds on
average and no team wins more than 55% of any 1v1 matchup in the bot sim. Except the 13 Ghouls, the goal numbers
are tuned, not designed, so change them freely as cards change. They're tuned numbers, not design: change them freely as cards change. See
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
