# Balance report

Bot: greedy, 400 games each, target 20 rounds.

## Goldfish (each team alone; how fast does it reach its goal?)

| Team | Games | Reached goal | Median round | p10-p90 | Pass turns |
| --- | --- | --- | --- | --- | --- |
| Farm | 400 | 99% | 20 | 16-25 | 3% |
| Forest | 400 | 99% | 22 | 18-26 | 7% |
| Factory | 400 | 94% | 16 | 13-25 | 18% |
| DarkForest | 400 | 100% | 18 | 15-23 | 0% |

## 1v1 matchups

### Win rate (row team vs column team)

| | Farm | Forest | Factory | DarkForest |
| --- | --- | --- | --- | --- |
| **Farm** |  | 70% | 34% | 42% |
| **Forest** | 31% |  | 28% | 27% |
| **Factory** | 66% | 72% |  | 60% |
| **DarkForest** | 58% | 73% | 40% |  |

### Matchups

| Matchup | Games | Win A | Win B | Draw | Seat 1 wins | Median rounds | p10-p90 | Goal / full / limit | First contact | Board fill |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| Farm vs Forest | 400 | 70% | 31% | 0% | 42% | 18 | 16-21 | 100% / 0% / 0% | round 15 (80%) | 52% |
| Farm vs Factory | 400 | 34% | 66% | 0% | 50% | 16 | 13-20 | 100% / 0% / 0% | round 16 (16%) | 32% |
| Farm vs DarkForest | 400 | 42% | 58% | 0% | 54% | 17 | 15-21 | 100% / 0% / 0% | round 17 (30%) | 35% |
| Forest vs Factory | 400 | 28% | 72% | 0% | 47% | 16 | 13-22 | 100% / 0% / 0% | round 14 (65%) | 45% |
| Forest vs DarkForest | 400 | 27% | 73% | 0% | 50% | 18 | 15-22 | 100% / 0% / 0% | round 14 (77%) | 50% |
| Factory vs DarkForest | 400 | 60% | 40% | 0% | 58% | 16 | 13-20 | 100% / 0% / 0% | round 15 (30%) | 32% |

### Overall, per team

| Team | Games | Win rate | Avg goal progress | Pass turns | Squares at end |
| --- | --- | --- | --- | --- | --- |
| Farm | 1200 | 49% | 73% | 3% | 17.6 |
| Forest | 1200 | 29% | 69% | 5% | 31.1 |
| Factory | 1200 | 66% | 81% | 6% | 16.3 |
| DarkForest | 1200 | 57% | 80% | 1% | 17.2 |

## Cards (value model vs par, and how they did in the matchups above)

| Card | Team | Cost (V) | Value (V) | Par | Delta | Verdict | Played per game | Win rate when played |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| Big Red Barn | Farm | 6 | 62.5 | 19 | +229% | strong | 1.2 | 49% |
| Nesting Box | Farm | 6 | 62.5 | 19 | +229% | strong | 1.3 | 50% |
| Cow | Farm | 3 | 24 | 14.5 | +66% | strong | 2.9 | 49% |
| Chicken Coop | Farm | 3 | 24 | 14.5 | +66% | strong | 3.1 | 49% |
| Scarecrow | Farm | 1 | 12 | 11.5 | +4% | on curve | 0.9 | 45% |
| Wheat Field | Farm | 0 | 10 | 10 | +0% | on curve | 6.9 | 49% |
| Farmhand's Cottage | Farm | 4 | 15 | 16 | -6% | on curve | 0.2 | 50% |
| Barn Cat | Farm | 2 | 12 | 13 | -8% | on curve | 0.1 | 33% |
| Farm | Farm | 0 | 0 | 10 | -100% | starting card | 0 | - |
| Blossom Hive | Forest | 3 | 24 | 14.5 | +66% | strong | 1.2 | 28% |
| Weeping Willow | Forest | 8 | 33 | 22 | +50% | strong | 0.6 | 39% |
| Pine | Forest | 2 | 18 | 13 | +38% | strong | 1.9 | 30% |
| Dormouse Den | Forest | 6 | 20 | 19 | +5% | on curve | 1.1 | 33% |
| Squirrel Drey | Forest | 0 | 10 | 10 | +0% | on curve | 2.4 | 30% |
| Wild Bees' Nest | Forest | 0 | 10 | 10 | +0% | on curve | 1.8 | 30% |
| Oak | Forest | 2 | 13 | 13 | +0% | on curve | 1.8 | 31% |
| Blue Jay's Nest | Forest | 4 | 15 | 16 | -6% | on curve | 1.2 | 33% |
| Sapling | Forest | 0 | 3 | 10 | -70% | weak | 18.2 | 29% |
| Huge Tree | Forest | 0 | 3 | 10 | -70% | starting card | 0 | - |
| Wrapping Station | Factory | 3 | 36 | 14.5 | +148% | strong | 2.2 | 70% |
| Tinker Shed | Factory | 2 | 22 | 13 | +69% | strong | 1.5 | 70% |
| Moonlight Robot | Factory | 2 | 20 | 13 | +54% | strong | 2.1 | 63% |
| Blueprint Desk | Factory | 2 | 15 | 13 | +15% | price by hand | 0.5 | 65% |
| Robot Charging Dock | Factory | 3 | 15 | 14.5 | +3% | price by hand | 0.4 | 47% |
| Scrap Pile | Factory | 0 | 10 | 10 | +0% | on curve | 4.3 | 67% |
| Toy Assembler | Factory | 3 | 8 | 14.5 | -45% | weak | 2.4 | 68% |
| Night Shift Assembler | Factory | 3 | 8 | 14.5 | -45% | weak | 1.4 | 71% |
| Spare Parts Bin | Factory | 0 | 2 | 10 | -80% | weak | 0.5 | 60% |
| Big Whistle | Factory | 5 | 0 | 17.5 | -100% | price by hand | 0 | - |
| Headquarters | Factory | 0 | 0 | 10 | -100% | starting card | 0 | - |
| Sleepy Web | DarkForest | 3 | 20 | 14.5 | +38% | strong | 2 | 56% |
| Glow Mushrooms | DarkForest | 0 | 12 | 10 | +20% | strong | 0.8 | 59% |
| Haunted Lantern | DarkForest | 10 | 30 | 25 | +20% | strong | 0.8 | 68% |
| Bat Belfry | DarkForest | 5 | 20 | 17.5 | +14% | on curve | 1.3 | 57% |
| Bat Roost | DarkForest | 0 | 10 | 10 | +0% | on curve | 3.7 | 57% |
| Cobweb Corner | DarkForest | 0 | 10 | 10 | +0% | on curve | 3.8 | 57% |
| Picnic Blanket | DarkForest | 2 | 12 | 13 | -8% | on curve | 1.3 | 56% |
| Full Moon Shrine | DarkForest | 4 | 12 | 16 | -25% | weak | 1.2 | 61% |
| Giggling Graveyard | DarkForest | 4 | 10 | 16 | -38% | weak | 1.3 | 60% |
| Evil Tree | DarkForest | 0 | 0 | 10 | -100% | starting card | 0 | - |

## Flags

* Goldfish Farm: reached its goal in only 99% of games.
* Goldfish Forest: reached its goal in only 99% of games.
* Goldfish Factory: reached its goal in only 94% of games.
* Goldfish speeds differ by 6 rounds (fastest Factory, slowest Forest; target within 2).
* Farm vs Forest: Farm wins 70% (target 50% ± 10%).
* Farm vs Forest: seat 1 wins 42% (target 50% ± 5%).
* Farm vs Factory: Farm wins 34% (target 50% ± 10%).
* Farm vs Factory: median game is 16 rounds (target 20 ± 3).
* Forest vs Factory: Forest wins 28% (target 50% ± 10%).
* Forest vs Factory: median game is 16 rounds (target 20 ± 3).
* Forest vs DarkForest: Forest wins 27% (target 50% ± 10%).
* Factory vs DarkForest: seat 1 wins 58% (target 50% ± 5%).
* Factory vs DarkForest: median game is 16 rounds (target 20 ± 3).
* Big Red Barn (Farm) is strong: every 2 days: add 1 Cow card to hand = 62.5
* Nesting Box (Farm) is strong: every 2 days: add 1 Chicken Coop card to hand = 62.5
* Cow (Farm) is strong: every day: 1 Milk per touching crop (x1.2) = 24
* Chicken Coop (Farm) is strong: every night: 1 Eggs per touching crop (x1.2) = 24
* Blossom Hive (Forest) is strong: every day: 1 Honey per touching tree (x1.2) = 24
* Weeping Willow (Forest) is strong: every day: add 1 Sapling card to hand = 30; tag tree = 3
* Pine (Forest) is strong: every 2 nights: add 1 Sapling card to hand = 15; tag tree = 3
* Sapling (Forest) is weak: tag tree = 3
* Wrapping Station (Factory) is strong: every night: 1 Toys per touching assembler (x1.2) = 36
* Tinker Shed (Factory) is strong: every day: 1 Parts = 10; every day: 1 Parts per touching scrap (x1.2) = 12
* Moonlight Robot (Factory) is strong: every night: 2 Parts = 20
* Blueprint Desk (Factory) is price by hand: every 2 days: add 1 factory.toy_assembler card to hand = 15
* Robot Charging Dock (Factory) is price by hand: every 2 nights: add 1 factory.moonlight_robot card to hand = 15
* Toy Assembler (Factory) is weak: every day: 2 Parts -> 1 Toys = 8
* Night Shift Assembler (Factory) is weak: every night: 2 Parts -> 1 Toys = 8
* Spare Parts Bin (Factory) is weak: on play: 2 Parts = 2
* Big Whistle (Factory) is price by hand: on play: run_tagged = 0
* Sleepy Web (DarkForest) is strong: every day: 2 Spiders = 20
* Glow Mushrooms (DarkForest) is strong: every night: 1 Bats per touching mushroom (x1.2) = 12
* Haunted Lantern (DarkForest) is strong: every night: 1 Ghouls = 30
* Full Moon Shrine (DarkForest) is weak: every 3 nights: 1 Ghouls per touching haunt (x1.2) = 12
* Giggling Graveyard (DarkForest) is weak: every 3 nights: 1 Ghouls = 10
