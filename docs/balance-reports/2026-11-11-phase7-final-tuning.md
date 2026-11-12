# Phase 7: final tuning pass (2026-11-11)

Phase 7 of the [implementation plan](https://claude.ai/artifact/HiebN1HdedieXB72aDCvpq), the last one.

## What changed

* **Goals:** Farm 27 to **30 of each**, Forest 22 to **24 Old Trees**, Factory 17 to **18 Toys**. Dark Forest stays at
  13 Ghouls; it was slowed through its recipe instead: summoning a Ghoul now costs **8 Bats and 8 Spiders** (was 6
  and 6), still plus 1 of each per Ghoul already at the party.
* **Docs:** DESIGN.md is rewritten to describe the game as it is now, and states plainly that day and night are flavour
  for the day teams. Each team doc opens with a "Now" section (goal, how it plays, the current deck straight from the
  card data, sim numbers), with the design history kept below it. BALANCE.md's phase table now ends at phase 7, with a
  summary of where the plan ended.
* **Playtest sheet:** [PLAYTEST.md](../PLAYTEST.md), one page for the first five human games.

## How the numbers were picked

After phase 6 every matchup was 45 to 53%, but games averaged 19.1 to 19.7 rounds. Raising every goal one step at a
time (1,000 games per matchup, both seeds):

| Try | Farm | Forest | Factory | Ghoul price | Matchups, seed 3001 | Seed 4001 | Mean rounds |
| --- | --- | --- | --- | --- | --- | --- | --- |
| A | 28 | 23 | 18 | 6+6 | 41 to 59% | 43 to 57% | 19.4 to 20.2 |
| B | 28 | 23 | 17 | 7+7 | 47 to 53% | 46 to 54% | 19.3 to 20.1 |
| X | 29 | 24 | 18 | 7+7 | 44 to 56% | 43 to 57% | 19.6 to 20.4 |
| Y | 30 | 24 | 18 | 8+7 | 44 to 56% | 44 to 56% | 19.7 to 20.5 |
| **Z (shipped)** | **30** | **24** | **18** | **8+8** | **44 to 56%** | **46 to 54%** | **19.7 to 20.5** |
| W | 30 | 25 | 18 | 8+8 | 42 to 58% | 42 to 58% | 19.7 to 20.7 |

Factory's goal is coarse: one Toy moves its matchups by 5 to 8 points. 18 only works once Dark Forest is slowed too.

## Results (shipped settings)

| Matchup | Win A, seed 3001 / 4001 | Mean rounds | Teams touch by round 13 |
| --- | --- | --- | --- |
| Farm vs Forest | 54% / 49% | 20.5 / 20.5 | 54% / 59% |
| Farm vs Factory | 52% / 47% | 20.1 / 20.0 | 20% / 18% |
| Farm vs Dark Forest | 51% / 46% | 20.0 / 19.9 | 16% / 18% |
| Forest vs Factory | 56% / 52% | 20.4 / 20.3 | 72% / 71% |
| Forest vs Dark Forest | 50% / 46% | 20.4 / 20.2 | 72% / 71% |
| Factory vs Dark Forest | 48% / 48% | 19.9 / 19.7 | 34% / 36% |

| Team | Solo median (p10 to p90) | Good beats random | Good beats sloppy | Shuffle luck |
| --- | --- | --- | --- | --- |
| Farm | 21 (18 to 26) | 97% | 55% | 0.56 |
| Forest | 22 (19 to 27) | 80% | 54% | 0.61 |
| Factory | 21 (18 to 27) | 96% | 54% | 0.63 |
| Dark Forest | 21 (18 to 25) | 98% | 56% | 0.71 |

Dark Forest Ghouls lost to hunger per solo game: greedy 0, sloppy 0, random 0.4, worst 5.

## Against the plan's targets

* **Games average 20+ rounds:** yes, 20.2 across all matchups. Factory vs Dark Forest is the shortest at 19.7 to 19.9.
* **No team above 55%:** 11 of 12 results are 46 to 54%. Forest vs Factory is 56% on seed 3001 and 52% on seed 4001,
  so about 54% on average. Closing it means a half-step for Factory or Forest, which neither goal has.
* **Solo medians 21 to 23:** yes, 21 to 22 for every team.
* **Shuffle luck under 0.75:** yes, 0.56 to 0.71 (0.77 to 0.93 before the plan).
* **Good beats sloppy 60%:** no, 54 to 56%, unchanged through every phase. The sloppy bot still plays a decent card
  most turns, and bots don't plan, so this gap is a question for playtests.
* **Teams meet by round 13 in 90% of games:** only Forest's matchups come close (54 to 72%). Farm, Factory and Dark
  Forest meet by round 14 to 16 in 74 to 86% of games. The playtest sheet asks people to note when teams touch.

Commands: `--games 1000 --seed 3001`, `--games 1000 --seed 4001`, `--skill --games 1000 --seed 3001`, and the tries
above with `--only matchups --set farmGoalEach=.. --set forestGoalOldTrees=.. --set factoryGoalToys=..
--set darkForestGhoulBats=.. --set darkForestGhoulSpiders=..`.
