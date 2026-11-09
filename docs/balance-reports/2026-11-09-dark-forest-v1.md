# Balance report

Bot: greedy, 400 games each, target 20 rounds.

## Goldfish (each team alone; how fast does it reach its goal?)

| Team | Games | Reached goal | Median round | p10-p90 | Pass turns |
| --- | --- | --- | --- | --- | --- |
| Farm | 400 | 100% | 16 | 13-21 | 2% |
| Forest | 400 | 0% | - | - | 46% |
| Factory | 400 | 85% | 31 | 24-36 | 9% |
| DarkForest | 400 | 100% | 18 | 15-23 | 1% |

## 1v1 matchups

### Win rate (row team vs column team)

| | Farm | Forest | Factory | DarkForest |
| --- | --- | --- | --- | --- |
| **Farm** |  | 100% | 100% | 74% |
| **Forest** | 0% |  | 0% | 0% |
| **Factory** | 1% | 100% |  | 2% |
| **DarkForest** | 26% | 100% | 99% |  |

### Matchups

| Matchup | Games | Win A | Win B | Draw | Seat 1 wins | Median rounds | p10-p90 | Goal / full / limit | First contact | Board fill |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| Farm vs Forest | 400 | 100% | 0% | 0% | 50% | 15 | 13-20 | 100% / 0% / 0% | round 19 (3%) | 22% |
| Farm vs Factory | 400 | 100% | 1% | 0% | 51% | 15 | 13-20 | 100% / 0% / 0% | round 18 (10%) | 22% |
| Farm vs DarkForest | 400 | 74% | 26% | 0% | 50% | 15 | 13-18 | 100% / 0% / 0% | round 16.5 (3%) | 21% |
| Forest vs Factory | 400 | 0% | 100% | 0% | 50% | 32 | 24-61 | 87% / 0% / 13% | round 21 (93%) | 44% |
| Forest vs DarkForest | 400 | 0% | 100% | 0% | 50% | 18 | 15-23 | 100% / 0% / 0% | round 20 (11%) | 26% |
| Factory vs DarkForest | 400 | 2% | 99% | 0% | 50% | 18 | 15-23 | 100% / 0% / 0% | round 17 (31%) | 26% |

### Overall, per team

| Team | Games | Win rate | Avg goal progress | Pass turns | Squares at end |
| --- | --- | --- | --- | --- | --- |
| Farm | 1200 | 91% | 97% | 2% | 15.7 |
| Forest | 1200 | 0% | 43% | 8% | 21.8 |
| Factory | 1200 | 34% | 49% | 4% | 22.6 |
| DarkForest | 1200 | 75% | 86% | 1% | 17.3 |

## Cards (value model vs par, and how they did in the matchups above)

| Card | Team | Cost (V) | Value (V) | Par | Delta | Verdict | Played per game | Win rate when played |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| Chicken Coop | Farm | 2 | 24 | 13 | +85% | strong | 3.2 | 91% |
| Cow | Farm | 2 | 20 | 13 | +54% | strong | 3.4 | 91% |
| Wheat Field | Farm | 0 | 10 | 10 | +0% | on curve | 5.6 | 91% |
| Farm | Farm | 0 | 10 | 10 | +0% | on curve | 0 | - |
| Sleepy Barn Cat | Farm | 2 | 10 | 13 | -23% | weak | 2.1 | 90% |
| Barn | Farm | 3 | 4 | 14.5 | -72% | weak | 0.3 | 85% |
| Beehive | Forest | 2 | 20 | 13 | +54% | strong | 4.5 | 0% |
| Squirrel Drey | Forest | 2 | 20 | 13 | +54% | strong | 4.4 | 0% |
| Huge Tree | Forest | 0 | 13 | 10 | +30% | strong | 0 | - |
| Oak | Forest | 2 | 13 | 13 | +0% | on curve | 4.3 | 0% |
| Sapling | Forest | 0 | 8 | 10 | -20% | weak | 4.3 | 0% |
| Owl's Hollow | Forest | 3 | 9.7 | 14.5 | -33% | weak | 3.3 | 0% |
| Workshop | Factory | 2 | 20 | 13 | +54% | strong | 4.4 | 34% |
| Scrap Pile | Factory | 0 | 10 | 10 | +0% | on curve | 7.6 | 34% |
| Headquarters | Factory | 0 | 10 | 10 | +0% | on curve | 0 | - |
| Clock Tower | Factory | 4 | 10.2 | 16 | -36% | weak | 4.4 | 34% |
| Night Shift Assembler | Factory | 3 | 8 | 14.5 | -45% | weak | 4 | 36% |
| Warehouse | Factory | 1 | 2 | 11.5 | -83% | weak | 1.2 | 49% |
| Sleepy Web | DarkForest | 3 | 20 | 14.5 | +38% | strong | 2.1 | 75% |
| Glow Mushrooms | DarkForest | 0 | 12 | 10 | +20% | strong | 2.7 | 75% |
| Haunted Lantern | DarkForest | 10 | 30 | 25 | +20% | strong | 0.8 | 82% |
| Bat Belfry | DarkForest | 5 | 20 | 17.5 | +14% | on curve | 1.3 | 76% |
| Bat Roost | DarkForest | 0 | 10 | 10 | +0% | on curve | 2.8 | 75% |
| Cobweb Corner | DarkForest | 0 | 10 | 10 | +0% | on curve | 2.5 | 75% |
| Picnic Blanket | DarkForest | 2 | 12 | 13 | -8% | on curve | 1.3 | 76% |
| Full Moon Shrine | DarkForest | 4 | 12 | 16 | -25% | weak | 1.4 | 77% |
| Giggling Graveyard | DarkForest | 4 | 10 | 16 | -38% | weak | 1.5 | 77% |
| Evil Tree | DarkForest | 0 | 0 | 10 | -100% | weak | 0 | - |

## Flags

* Goldfish Forest: reached its goal in only 0% of games.
* Goldfish Factory: reached its goal in only 85% of games.
* Goldfish speeds differ by 15 rounds (fastest Farm, slowest Factory; target within 2).
* Farm vs Forest: Farm wins 100% (target 50% ± 10%).
* Farm vs Forest: median game is 15 rounds (target 20 ± 3).
* Farm vs Factory: Farm wins 100% (target 50% ± 10%).
* Farm vs Factory: median game is 15 rounds (target 20 ± 3).
* Farm vs DarkForest: Farm wins 74% (target 50% ± 10%).
* Farm vs DarkForest: median game is 15 rounds (target 20 ± 3).
* Forest vs Factory: Forest wins 0% (target 50% ± 10%).
* Forest vs Factory: median game is 32 rounds (target 20 ± 3).
* Forest vs DarkForest: Forest wins 0% (target 50% ± 10%).
* Factory vs DarkForest: Factory wins 2% (target 50% ± 10%).
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
* Sleepy Web (DarkForest) is strong: every day: 2 Spiders = 20
* Glow Mushrooms (DarkForest) is strong: every night: 1 Bats per touching mushroom (x1.2) = 12
* Haunted Lantern (DarkForest) is strong: every night: 1 Ghouls = 30
* Full Moon Shrine (DarkForest) is weak: every 3 nights: 1 Ghouls per touching haunt (x1.2) = 12
* Giggling Graveyard (DarkForest) is weak: every 3 nights: 1 Ghouls = 10
* Evil Tree (DarkForest) is weak: 
