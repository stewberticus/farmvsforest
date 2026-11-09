# Forest growth goal (2026-11-09)

Forest's goal changed to **16 Old Trees in one connected forest**: trees grow Sapling (2 sunny mornings) > Young Tree (4 sunny mornings) > Old Tree, where a sunny morning means the tree touches an empty square. Also: Compost, rising extra-tree cost, Wildflower Meadow, Squirrels like Oaks. 10x10 board, 800 games each.

Before the change (33 connected trees): good bot round 23, random round 24, worst round 28; good beat random in 71% of mirror matches and worst in 82%.

Solo finish round (median, with the middle 80% of games) and how often the goal is reached by round 60.

| Team | Good bot | Random bot | Worst bot | Good beats random (mirror) | Good beats worst (mirror) |
| --- | --- | --- | --- | --- | --- |
| Forest | round 22 (19-26), 100% | round 25 (22-29), 98% | round 54 (42-59), 20% | 86% | 99% |

Bot: greedy, 800 games each, target 20 rounds.

## Goldfish (each team alone; how fast does it reach its goal?)

| Team | Games | Reached goal | Median round | p10-p90 | Pass turns |
| --- | --- | --- | --- | --- | --- |
| Farm | 800 | 99% | 22 | 19-29 | 3% |
| Forest | 800 | 100% | 22 | 19-26 | 4% |
| Factory | 800 | 100% | 22 | 17-27 | 1% |
| DarkForest | 800 | 100% | 22 | 17-27 | 1% |

## 1v1 matchups

### Win rate (row team vs column team)

| | Farm | Forest | Factory | DarkForest |
| --- | --- | --- | --- | --- |
| **Farm** |  | 48% | 44% | 49% |
| **Forest** | 52% |  | 50% | 51% |
| **Factory** | 56% | 50% |  | 49% |
| **DarkForest** | 51% | 49% | 51% |  |

### Matchups

| Matchup | Games | Win A | Win B | Draw | Seat 1 wins | Mean rounds | Median rounds | p10-p90 | Goal / full / limit | First contact | Board fill |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| Farm vs Forest | 800 | 48% | 52% | 0% | 45% | 20.6 | 20 | 18-24 | 100% / 0% / 0% | round 16 (82%) | 51% |
| Farm vs Factory | 800 | 44% | 56% | 0% | 48% | 20 | 20 | 17-24 | 100% / 0% / 0% | round 18 (42%) | 41% |
| Farm vs DarkForest | 800 | 49% | 51% | 0% | 55% | 20.4 | 20 | 17-24 | 100% / 0% / 0% | round 17 (52%) | 41% |
| Forest vs Factory | 800 | 50% | 50% | 0% | 45% | 20 | 20 | 17-23 | 100% / 0% / 0% | round 15 (85%) | 50% |
| Forest vs DarkForest | 800 | 51% | 49% | 0% | 53% | 20.3 | 20 | 17-23.1 | 100% / 0% / 0% | round 15 (86%) | 51% |
| Factory vs DarkForest | 800 | 49% | 51% | 0% | 59% | 20.1 | 20 | 17-24 | 100% / 0% / 0% | round 16 (64%) | 41% |

### Overall, per team

| Team | Games | Win rate | Avg goal progress | Pass turns | Squares at end |
| --- | --- | --- | --- | --- | --- |
| Farm | 2400 | 47% | 77% | 3% | 20.4 |
| Forest | 2400 | 51% | 85% | 3% | 30.1 |
| Factory | 2400 | 52% | 73% | 1% | 20.5 |
| DarkForest | 2400 | 50% | 74% | 0% | 20.2 |

## Cards (value model vs par, and how they did in the matchups above)

| Card | Team | Cost (V) | Value (V) | Par | Delta | Verdict | Played per game | Win rate when played |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| Cow | Farm | 3 | 24 | 14.5 | +66% | strong | 3.9 | 47% |
| Chicken Coop | Farm | 3 | 24 | 14.5 | +66% | strong | 4.1 | 47% |
| Scarecrow | Farm | 1 | 12 | 11.5 | +4% | on curve | 1.1 | 48% |
| Big Red Barn | Farm | 6 | 19.8 | 19 | +4% | on curve | 0.9 | 46% |
| Nesting Box | Farm | 6 | 19.8 | 19 | +4% | on curve | 0.9 | 45% |
| Wheat Field | Farm | 0 | 10 | 10 | +0% | on curve | 7.9 | 47% |
| Farmhand's Cottage | Farm | 4 | 15 | 16 | -6% | on curve | 0.3 | 44% |
| Barn Cat | Farm | 2 | 12 | 13 | -8% | on curve | 0.4 | 48% |
| Farm | Farm | 0 | 0 | 10 | -100% | starting card | 0 | - |
| Squirrel Drey | Forest | 0 | 22 | 10 | +120% | strong | 1.2 | 55% |
| Wildflower Meadow | Forest | 0 | 20 | 10 | +100% | strong | 2.2 | 51% |
| Pine | Forest | 2 | 24 | 13 | +85% | strong | 2.2 | 51% |
| Weeping Willow | Forest | 8 | 39 | 22 | +77% | strong | 0.6 | 58% |
| Blossom Hive | Forest | 3 | 24 | 14.5 | +66% | strong | 0.6 | 51% |
| Oak | Forest | 2 | 19 | 13 | +46% | strong | 1.9 | 54% |
| Dormouse Den | Forest | 6 | 20 | 19 | +5% | on curve | 0.4 | 51% |
| Blue Jay's Nest | Forest | 4 | 15 | 16 | -6% | on curve | 0.2 | 42% |
| Sapling | Forest | 0 | 9 | 10 | -10% | on curve | 19.8 | 51% |
| Huge Tree | Forest | 0 | 3 | 10 | -70% | starting card | 0 | - |
| Wrapping Station | Factory | 3 | 36 | 14.5 | +148% | strong | 3 | 53% |
| Tinker Shed | Factory | 2 | 22 | 13 | +69% | strong | 2 | 52% |
| Moonlight Robot | Factory | 2 | 20 | 13 | +54% | strong | 2.6 | 49% |
| Robot Charging Dock | Factory | 3 | 18.5 | 14.5 | +28% | strong | 0.6 | 41% |
| Blueprint Desk | Factory | 2 | 15 | 13 | +15% | strong | 0.6 | 57% |
| Scrap Pile | Factory | 0 | 10 | 10 | +0% | on curve | 4.9 | 52% |
| Big Whistle | Factory | 5 | 12 | 17.5 | -31% | weak | 0 | 52% |
| Toy Assembler | Factory | 3 | 8 | 14.5 | -45% | weak | 3.4 | 53% |
| Night Shift Assembler | Factory | 3 | 8 | 14.5 | -45% | weak | 2 | 52% |
| Spare Parts Bin | Factory | 0 | 5.3 | 10 | -47% | weak | 0.4 | 51% |
| Headquarters | Factory | 0 | 0 | 10 | -100% | starting card | 0 | - |
| Sleepy Web | DarkForest | 3 | 20 | 14.5 | +38% | strong | 2.3 | 50% |
| Glow Mushrooms | DarkForest | 0 | 12 | 10 | +20% | strong | 0.9 | 50% |
| Bat Belfry | DarkForest | 5 | 20 | 17.5 | +14% | on curve | 1.5 | 49% |
| Bat Roost | DarkForest | 0 | 10 | 10 | +0% | on curve | 4.7 | 50% |
| Cobweb Corner | DarkForest | 0 | 10 | 10 | +0% | on curve | 4.7 | 50% |
| Picnic Blanket | DarkForest | 2 | 12 | 13 | -8% | on curve | 1.5 | 49% |
| Full Moon Shrine | DarkForest | 4 | 12 | 16 | -25% | weak | 1.5 | 55% |
| Haunted Lantern | DarkForest | 7 | 15 | 20.5 | -27% | weak | 0.9 | 56% |
| Giggling Graveyard | DarkForest | 4 | 7.5 | 16 | -53% | weak | 1.4 | 55% |
| Evil Tree | DarkForest | 0 | 0 | 10 | -100% | starting card | 0 | - |

## Flags

* Farm vs Factory: Farm wins 44% (target 50% ± 5%).
* Farm vs Factory: games average 20 rounds (target at least 20).
* Farm vs DarkForest: seat 1 wins 55% (target 50% ± 5%).
* Factory vs DarkForest: seat 1 wins 59% (target 50% ± 5%).
* Cow (Farm) is strong: every day: 1 Milk per touching crop (x1.2) = 24
* Chicken Coop (Farm) is strong: every night: 1 Eggs per touching crop (x1.2) = 24
* Squirrel Drey (Forest) is strong: every day: 1 Acorns = 10; every day: 1 Acorns per touching oak (x1.2) = 12
* Wildflower Meadow (Forest) is strong: every day: 1 Honey per touching empty square (x1) = 20
* Pine (Forest) is strong: every 2 nights: add 1 Sapling card to hand = 15; every day: forest.grow (hand-set) = 6; tag tree = 3
* Weeping Willow (Forest) is strong: every day: add 1 Sapling card to hand = 30; every day: forest.grow (hand-set) = 6; tag tree = 3
* Blossom Hive (Forest) is strong: every day: 1 Honey per touching tree (x1.2) = 24
* Oak (Forest) is strong: every day: 1 Acorns = 10; every day: forest.grow (hand-set) = 6; tag tree = 3
* Wrapping Station (Factory) is strong: every night: 1 Toys per touching assembler (x1.2) = 36
* Tinker Shed (Factory) is strong: every day: 1 Parts = 10; every day: 1 Parts per touching scrap (x1.2) = 12
* Moonlight Robot (Factory) is strong: every night: 2 Parts = 20
* Robot Charging Dock (Factory) is strong: every 2 nights: add 1 Moonlight Robot card to hand = 18.5
* Blueprint Desk (Factory) is strong: every 2 days: add 1 Toy Assembler card to hand = 15
* Big Whistle (Factory) is weak: on play: run_tagged (hand-set) = 12
* Toy Assembler (Factory) is weak: every day: 2 Parts -> 1 Toys = 8
* Night Shift Assembler (Factory) is weak: every night: 2 Parts -> 1 Toys = 8
* Spare Parts Bin (Factory) is weak: on play: 2 Parts = 2; every 3 days: 1 Parts = 3.3
* Sleepy Web (DarkForest) is strong: every day: 2 Spiders = 20
* Glow Mushrooms (DarkForest) is strong: every night: 1 Bats per touching mushroom (x1.2) = 12
* Full Moon Shrine (DarkForest) is weak: every 3 nights: 1 Ghouls per touching haunt (x1.2) = 12
* Haunted Lantern (DarkForest) is weak: every 2 nights: 1 Ghouls = 15
* Giggling Graveyard (DarkForest) is weak: every 4 nights: 1 Ghouls = 7.5
