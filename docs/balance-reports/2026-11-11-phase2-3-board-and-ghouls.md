# Phases 2 and 3: contested board and Dark Forest summoning (2026-11-11)

Phases 2 and 3 of the [implementation plan](https://claude.ai/artifact/HiebN1HdedieXB72aDCvpq), tuned together because
the smaller board sped Dark Forest up so much that its tuning only made sense once phase 3 changed how it makes Ghouls.

## What changed

* **Board 9x9 for 1v1** (`boardSize`). The plan said 8x8; Alexander suggested 9x9, and it tunes much better (below).
  This is waiting on his pick.
* **4 Sunny Glades** (`sunnyGladeBonus`, `sunnyGladeSquares`): (3,3), (5,5), (2,6) and (6,2) on 9x9, symmetric for both
  seats. Nobody can build on them; a card touching one makes 1 extra of what it makes, once a round. They count as
  sunlight for Forest trees but not as empty squares for Wildflower Meadow. Four-player boards have none for now.
* **Dark Forest summons Ghouls**: on your turn, 6 Bats and 6 Spiders make a Ghoul, plus 1 of each for every Ghoul you
  already have. Each switched-on Haunt takes 1 of each off (never below 2 of each). Graveyard and Lantern no longer
  make Ghouls; the Full Moon Shrine gives 1 free Ghoul every 3 nights while it touches a Haunt. Deck: 6 Bat Roosts, 6
  Cobweb Corners, 3 Graveyards, 2 Lanterns (was 7, 7, 2, 1). The careful bot summons only Ghouls it can feed
  tomorrow; random and worst summon whatever they can.
* Goals: Farm 62 each, Forest 23 Old Trees, Factory 16 Toys, Dark Forest 13 Ghouls.

## 8x8 or 9x9

On 8x8 Forest spreads fastest and wins the games that fill the board on "most squares". With phase 3 in, no goal set
tried (Forest 18 to 24, Factory 15 to 18, Farm 56 to 64; 600 games each) kept Forest under 60% against both Factory and
Dark Forest: lower Forest goals mean Forest finishes sooner, higher ones mean more board-full wins for Forest.

| Board | Matchups | Board-full endings | Contact by round 13 |
| --- | --- | --- | --- |
| 8x8 (best set: Forest 20, Factory 16, Farm 56) | 45 to 65% | up to 23% | 66 to 96% |
| 9x9 (chosen set) | 44 to 56% | 0 to 3% | 10 to 67% |

On 9x9 the teams still meet (first contact median round 12 to 16, in 59 to 100% of games), but Farm and Factory, and
Farm and Dark Forest, often only meet late.

## Done-when check

| Check | Target | Result |
| --- | --- | --- |
| Contact by round 13 | 90% of games | No: 10 to 67% on 9x9 (66 to 96% on 8x8) |
| Goal endings | 80% or more | 97 to 100% |
| Matchups | 45 to 55% | 44 to 56% across two seeds (Factory 54 to 56% vs Farm, 54 to 55% vs Dark Forest) |
| Mean length | 20+ rounds | 18.9 to 20.1. Factory's matchups are the short ones |
| Glades matter (greedy beats noglades) | 55% | Dark Forest 68%; Farm 50%, Forest 49%, Factory 52%. One extra Wheat or Part a round is too small to fight for |
| Dark Forest solo median | 21 to 23 | 21 |
| Ghouls lost to hunger | random 2+, good under 1 | good 0, random 0.1, worst 0.8. Feeding still rarely bites |
| Dark Forest shuffle luck | under 0.8 | 0.76 (was 0.93) |
| Dark Forest matchups | 45 to 55% | 45 to 54% |

Shuffle luck dropped for every team on the new board (Farm 0.84, Forest 0.62, Factory 0.71, Dark Forest 0.76), and
good beats sloppy rose to 52 to 58%. Forest's good-beats-random fell from 88% to 75%: with glades and more contact, a
random Forest stumbles into light more often. Mean length and the glade value for day teams are left for phase 7.

Commands: `--games 1000 --seed 3001`, `--only matchups --games 1000 --seed 4001`, `--skill --games 600 --seed 3001`,
`--versus greedy,noglades --games 1000 --seed 3001`.

## Skill

| --- | --- | --- | --- | --- | --- | --- | --- |
| Farm | round 21 (18-28), 98% | round 37 (29-47), 97% | never (0%) | 96% | 52% | 99% | 0.84 |
| Forest | round 22 (19-27), 100% | round 28 (24-32), 98% | round 52 (48-55), 0% | 75% | 55% | 88% | 0.62 |
| Factory | round 20 (16-27), 100% | round 38 (30-44), 100% | round 51 (48-52), 100% | 98% | 55% | 100% | 0.71 |
| DarkForest | round 21 (18-24), 100% | round 29 (25-32), 100% | round 41 (38-45), 98% | 96% | 58% | 100% | 0.76 |

Dark Forest Ghouls per solo game (summoned / went home hungry):

| Bot | Summoned | Went home hungry |
| --- | --- | --- |
| greedy | 7.2 | 0.00 |
| sloppy | 7.5 | 0.00 |
| random | 10.2 | 0.13 |
| worst | 13.7 | 0.76 |

## Glades

| Team | greedy beats noglades (mirror) | Mean rounds |
| --- | --- | --- |
| Farm | 50% | 19.8 |
| Forest | 49% | 20.5 |
| Factory | 52% | 18.7 |
| DarkForest | 68% | 20.5 |
## Seed 3001

## Goldfish (each team alone; how fast does it reach its goal?)

| Team | Games | Reached goal | Median round | p10-p90 | Pass turns |
| --- | --- | --- | --- | --- | --- |
| Farm | 1000 | 99% | 21 | 18-28 | 2% |
| Forest | 1000 | 100% | 22 | 19-28 | 1% |
| Factory | 1000 | 100% | 20 | 16-27 | 1% |
| DarkForest | 1000 | 100% | 21 | 18-24 | 1% |

## 1v1 matchups

### Win rate (row team vs column team)

| | Farm | Forest | Factory | DarkForest |
| --- | --- | --- | --- | --- |
| **Farm** |  | 52% | 44% | 50% |
| **Forest** | 48% |  | 51% | 47% |
| **Factory** | 56% | 49% |  | 54% |
| **DarkForest** | 50% | 53% | 46% |  |

### Matchups

| Matchup | Games | Win A | Win B | Draw | Seat 1 wins | Mean rounds | Median rounds | p10-p90 | Goal / full / limit | First contact | Contact by round 13 | Board fill |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| Farm vs Forest | 1000 | 52% | 48% | 0% | 48% | 20 | 20 | 18-23 | 100% / 1% / 0% | round 14 (98%) | 47% | 72% |
| Farm vs Factory | 1000 | 44% | 56% | 0% | 52% | 19.1 | 19 | 16-23 | 100% / 0% / 0% | round 16 (62%) | 11% | 51% |
| Farm vs DarkForest | 1000 | 50% | 50% | 0% | 51% | 19.6 | 19 | 17-23 | 100% / 0% / 0% | round 16 (71%) | 13% | 51% |
| Forest vs Factory | 1000 | 51% | 49% | 0% | 51% | 19.7 | 20 | 16-24 | 97% / 3% / 0% | round 12 (97%) | 64% | 73% |
| Forest vs DarkForest | 1000 | 47% | 53% | 0% | 52% | 20.1 | 20 | 18-23 | 99% / 1% / 0% | round 12 (99%) | 67% | 74% |
| Factory vs DarkForest | 1000 | 54% | 46% | 0% | 51% | 19 | 19 | 16-23 | 100% / 0% / 0% | round 14 (77%) | 34% | 51% |

### Overall, per team

| Team | Games | Win rate | Avg goal progress | Pass turns | Squares at end |
| --- | --- | --- | --- | --- | --- |
| Farm | 3000 | 49% | 77% | 2% | 19.7 |
| Forest | 3000 | 49% | 83% | 1% | 35.9 |
| Factory | 3000 | 53% | 75% | 1% | 19.9 |
| DarkForest | 3000 | 50% | 85% | 1% | 19.6 |

## Seed 4001

## 1v1 matchups

### Win rate (row team vs column team)

| | Farm | Forest | Factory | DarkForest |
| --- | --- | --- | --- | --- |
| **Farm** |  | 52% | 46% | 52% |
| **Forest** | 48% |  | 53% | 53% |
| **Factory** | 54% | 47% |  | 55% |
| **DarkForest** | 48% | 47% | 45% |  |

### Matchups

| Matchup | Games | Win A | Win B | Draw | Seat 1 wins | Mean rounds | Median rounds | p10-p90 | Goal / full / limit | First contact | Contact by round 13 | Board fill |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| Farm vs Forest | 1000 | 52% | 48% | 0% | 52% | 19.9 | 20 | 18-23 | 99% / 1% / 0% | round 14 (98%) | 47% | 72% |
| Farm vs Factory | 1000 | 46% | 54% | 0% | 52% | 18.9 | 19 | 16-23 | 100% / 0% / 0% | round 16 (59%) | 10% | 50% |
| Farm vs DarkForest | 1000 | 52% | 48% | 0% | 52% | 19.6 | 19 | 17-23 | 100% / 0% / 0% | round 16 (72%) | 14% | 51% |
| Forest vs Factory | 1000 | 53% | 47% | 0% | 48% | 19.6 | 19 | 16-24 | 97% / 3% / 0% | round 12 (97%) | 65% | 72% |
| Forest vs DarkForest | 1000 | 53% | 47% | 0% | 50% | 20.1 | 20 | 18-23 | 99% / 1% / 0% | round 12 (100%) | 66% | 74% |
| Factory vs DarkForest | 1000 | 55% | 45% | 0% | 53% | 19.2 | 19 | 16-23 | 100% / 0% / 0% | round 14 (80%) | 32% | 51% |

### Overall, per team

| Team | Games | Win rate | Avg goal progress | Pass turns | Squares at end |
| --- | --- | --- | --- | --- | --- |
| Farm | 3000 | 50% | 78% | 2% | 19.6 |
| Forest | 3000 | 51% | 83% | 1% | 36 |
| Factory | 3000 | 52% | 74% | 1% | 19.9 |
| DarkForest | 3000 | 47% | 84% | 1% | 19.7 |

