# Balance report

Bot: greedy, 200 games each, target 20 rounds.

## Goldfish (each team alone; how fast does it reach its goal?)

| Team | Games | Reached goal | Median round | p10-p90 | Pass turns |
| --- | --- | --- | --- | --- | --- |
| Farm | 200 | 100% | 16 | 13-21 | 2% |
| Forest | 200 | 0% | - | - | 46% |
| Factory | 200 | 86% | 31 | 24-36 | 9% |
| DarkForest | 200 | 100% | 12 | 10-14 | 1% |

## 1v1 matchups

### Win rate (row team vs column team)

| | Farm | Forest | Factory | DarkForest |
| --- | --- | --- | --- | --- |
| **Farm** |  | 100% | 100% | 8% |
| **Forest** | 0% |  | 0% | 0% |
| **Factory** | 1% | 100% |  | 0% |
| **DarkForest** | 92% | 100% | 100% |  |

### Matchups

| Matchup | Games | Win A | Win B | Draw | Seat 1 wins | Median rounds | p10-p90 | Goal / full / limit | First contact | Board fill |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| Farm vs Forest | 200 | 100% | 0% | 0% | 50% | 15 | 13-21 | 100% / 0% / 0% | round 19 (3%) | 22% |
| Farm vs Factory | 200 | 100% | 1% | 0% | 51% | 15 | 13-21 | 100% / 0% / 0% | round 17.5 (11%) | 22% |
| Farm vs DarkForest | 200 | 8% | 92% | 0% | 52% | 12 | 10-14 | 100% / 0% / 0% | round 15 (1%) | 17% |
| Forest vs Factory | 200 | 0% | 100% | 0% | 50% | 31.5 | 23.9-61 | 86% / 0% / 15% | round 21 (91%) | 43% |
| Forest vs DarkForest | 200 | 0% | 100% | 0% | 50% | 12 | 10-14 | 100% / 0% / 0% | round 14 (2%) | 17% |
| Factory vs DarkForest | 200 | 0% | 100% | 0% | 50% | 12 | 10-14 | 100% / 0% / 0% | round 13.5 (3%) | 17% |

### Overall, per team

| Team | Games | Win rate | Avg goal progress | Pass turns | Squares at end |
| --- | --- | --- | --- | --- | --- |
| Farm | 600 | 69% | 83% | 2% | 14.8 |
| Forest | 600 | 0% | 39% | 9% | 19.6 |
| Factory | 600 | 34% | 44% | 5% | 20.4 |
| DarkForest | 600 | 97% | 99% | 1% | 11.8 |

## Cards (value model vs par, and how they did in the matchups above)

| Card | Team | Cost (V) | Value (V) | Par | Delta | Verdict | Played per game | Win rate when played |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| Chicken Coop | Farm | 2 | 24 | 13 | +85% | strong | 3.1 | 70% |
| Cow | Farm | 2 | 20 | 13 | +54% | strong | 3.1 | 70% |
| Wheat Field | Farm | 0 | 10 | 10 | +0% | on curve | 5.4 | 69% |
| Farm | Farm | 0 | 10 | 10 | +0% | on curve | 0 | - |
| Sleepy Barn Cat | Farm | 2 | 10 | 13 | -23% | weak | 1.9 | 72% |
| Barn | Farm | 3 | 4 | 14.5 | -72% | weak | 0.3 | 74% |
| Beehive | Forest | 2 | 20 | 13 | +54% | strong | 4 | 0% |
| Squirrel Drey | Forest | 2 | 20 | 13 | +54% | strong | 3.9 | 0% |
| Huge Tree | Forest | 0 | 13 | 10 | +30% | strong | 0 | - |
| Oak | Forest | 2 | 13 | 13 | +0% | on curve | 3.8 | 0% |
| Sapling | Forest | 0 | 8 | 10 | -20% | weak | 3.9 | 0% |
| Owl's Hollow | Forest | 3 | 9.7 | 14.5 | -33% | weak | 2.9 | 0% |
| Workshop | Factory | 2 | 20 | 13 | +54% | strong | 4 | 34% |
| Scrap Pile | Factory | 0 | 10 | 10 | +0% | on curve | 6.9 | 34% |
| Headquarters | Factory | 0 | 10 | 10 | +0% | on curve | 0 | - |
| Clock Tower | Factory | 4 | 10.2 | 16 | -36% | weak | 3.9 | 34% |
| Night Shift Assembler | Factory | 3 | 8 | 14.5 | -45% | weak | 3.5 | 38% |
| Warehouse | Factory | 1 | 2 | 11.5 | -83% | weak | 1 | 57% |
| Bat Cave | DarkForest | 1 | 20 | 11.5 | +74% | strong | 2.6 | 97% |
| Haunted Lantern | DarkForest | 5 | 30 | 17.5 | +71% | strong | 2.5 | 98% |
| Mushroom Ring | DarkForest | 0 | 10 | 10 | +0% | on curve | 3 | 97% |
| Evil Tree | DarkForest | 0 | 10 | 10 | +0% | on curve | 0 | - |
| Sleepy Web | DarkForest | 1 | 10 | 11.5 | -13% | on curve | 0.4 | 97% |
| Spooky Graveyard | DarkForest | 2 | 10 | 13 | -23% | weak | 2.3 | 97% |

## Flags

* Goldfish Forest: reached its goal in only 0% of games.
* Goldfish Factory: reached its goal in only 86% of games.
* Goldfish speeds differ by 19 rounds (fastest DarkForest, slowest Factory; target within 2).
* Farm vs Forest: Farm wins 100% (target 50% ± 10%).
* Farm vs Forest: median game is 15 rounds (target 20 ± 3).
* Farm vs Factory: Farm wins 100% (target 50% ± 10%).
* Farm vs Factory: median game is 15 rounds (target 20 ± 3).
* Farm vs DarkForest: Farm wins 8% (target 50% ± 10%).
* Farm vs DarkForest: median game is 12 rounds (target 20 ± 3).
* Forest vs Factory: Forest wins 0% (target 50% ± 10%).
* Forest vs Factory: median game is 31.5 rounds (target 20 ± 3).
* Forest vs DarkForest: Forest wins 0% (target 50% ± 10%).
* Forest vs DarkForest: median game is 12 rounds (target 20 ± 3).
* Factory vs DarkForest: Factory wins 0% (target 50% ± 10%).
* Factory vs DarkForest: median game is 12 rounds (target 20 ± 3).
* Chicken Coop (Farm) is strong: every day: 1 Eggs per touching crop (x1.2) = 24
* Cow (Farm) is strong: every day: 1 Milk = 20
* Sleepy Barn Cat (Farm) is weak: every night: 1 Wheat = 10
* Barn (Farm) is weak: on play: draw 2 = 4
* Beehive (Forest) is strong: every day: 1 Honey = 20
* Squirrel Drey (Forest) is strong: every night: 2 Acorns = 20
* Huge Tree (Forest) is strong: every day: 1 Acorns = 10; tag tree = 3
* Sapling (Forest) is weak: every 2 days: 1 Acorns = 5; tag tree = 3
* Owl's Hollow (Forest) is weak: every 3 nights: draw 1 = 6.7; tag tree = 3
* Workshop (Factory) is strong: every day: 2 Parts = 20
* Clock Tower (Factory) is weak: once after 3 days: 4 Goods = 10.2
* Night Shift Assembler (Factory) is weak: every night: 2 Parts -> 1 Goods = 8
* Warehouse (Factory) is weak: on play: draw 1 = 2
* Bat Cave (DarkForest) is strong: every night: 2 Bats = 20
* Haunted Lantern (DarkForest) is strong: every night: 1 Ghouls = 30
* Spooky Graveyard (DarkForest) is weak: every 3 nights: 1 Ghouls = 10
