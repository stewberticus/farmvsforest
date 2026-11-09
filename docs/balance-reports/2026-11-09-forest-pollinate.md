# Forest Pollinate (2026-11-09)

Honey had few uses (5 cards in the deck), so it piled up. New Forest rule **Pollinate**: on your turn, spend 2 Honey to make one of your growing trees grow one step now, even in shade. Goal raised from 16 to **20 Old Trees** to keep 1v1 at 50%. 800 games each.

Before (16 Old Trees, no Pollinate): Forest good round 22, random 25, worst 54; good beat random 86%.

Solo finish round (median, with the middle 80% of games) and how often the goal is reached by round 60.

| Team | Good bot | Random bot | Worst bot | Good beats random (mirror) | Good beats worst (mirror) |
| --- | --- | --- | --- | --- | --- |
| Farm | round 22 (19-29), 99% | round 40 (32-49), 96% | never (0%) | 98% | 99% |
| Forest | round 22 (20-26), 100% | round 28 (25-32), 98% | round 57.5 (47-60), 2% | 93% | 99% |
| Factory | round 22 (17-27), 100% | round 42 (34-48), 100% | round 49 (47-52), 100% | 99% | 100% |
| DarkForest | round 21 (17-28), 100% | round 36 (23-53), 75% | never (0%) | 91% | 100% |

Bot: greedy, 800 games each, target 20 rounds.

## Goldfish (each team alone; how fast does it reach its goal?)

| Team | Games | Reached goal | Median round | p10-p90 | Pass turns |
| --- | --- | --- | --- | --- | --- |
| Farm | 800 | 99% | 22 | 19-29 | 3% |
| Forest | 800 | 100% | 22 | 20-26.1 | 4% |
| Factory | 800 | 100% | 22 | 17-27 | 1% |
| DarkForest | 800 | 100% | 21 | 17-28 | 1% |

## 1v1 matchups

### Win rate (row team vs column team)

| | Farm | Forest | Factory | DarkForest |
| --- | --- | --- | --- | --- |
| **Farm** |  | 51% | 45% | 49% |
| **Forest** | 49% |  | 51% | 49% |
| **Factory** | 55% | 49% |  | 48% |
| **DarkForest** | 51% | 51% | 52% |  |

### Matchups

| Matchup | Games | Win A | Win B | Draw | Seat 1 wins | Mean rounds | Median rounds | p10-p90 | Goal / full / limit | First contact | Board fill |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| Farm vs Forest | 800 | 51% | 49% | 0% | 50% | 21.1 | 21 | 18-24 | 100% / 0% / 0% | round 17 (82%) | 52% |
| Farm vs Factory | 800 | 45% | 55% | 0% | 54% | 20.4 | 20 | 17-24 | 100% / 0% / 0% | round 18 (44%) | 41% |
| Farm vs DarkForest | 800 | 49% | 51% | 0% | 57% | 20.3 | 20 | 17-24 | 100% / 0% / 0% | round 18 (49%) | 41% |
| Forest vs Factory | 800 | 51% | 49% | 0% | 51% | 20.4 | 20 | 17-24 | 100% / 0% / 0% | round 16 (87%) | 52% |
| Forest vs DarkForest | 800 | 49% | 51% | 0% | 54% | 20.3 | 20 | 17-24 | 100% / 0% / 0% | round 16 (85%) | 51% |
| Factory vs DarkForest | 800 | 48% | 52% | 0% | 59% | 20 | 20 | 16-24 | 100% / 0% / 0% | round 16 (60%) | 41% |

### Overall, per team

| Team | Games | Win rate | Avg goal progress | Pass turns | Squares at end |
| --- | --- | --- | --- | --- | --- |
| Farm | 2400 | 49% | 77% | 3% | 20.6 |
| Forest | 2400 | 49% | 85% | 4% | 31.2 |
| Factory | 2400 | 51% | 72% | 1% | 20.8 |
| DarkForest | 2400 | 51% | 74% | 1% | 20.1 |

## Cards (value model vs par, and how they did in the matchups above)

| Card | Team | Cost (V) | Value (V) | Par | Delta | Verdict | Played per game | Win rate when played |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| Cow | Farm | 3 | 24 | 14.5 | +66% | strong | 3.9 | 49% |
| Chicken Coop | Farm | 3 | 24 | 14.5 | +66% | strong | 4.2 | 49% |
| Scarecrow | Farm | 1 | 12 | 11.5 | +4% | on curve | 1.1 | 50% |
| Big Red Barn | Farm | 6 | 19.8 | 19 | +4% | on curve | 0.9 | 48% |
| Nesting Box | Farm | 6 | 19.8 | 19 | +4% | on curve | 0.9 | 47% |
| Wheat Field | Farm | 0 | 10 | 10 | +0% | on curve | 7.9 | 49% |
| Farmhand's Cottage | Farm | 4 | 15 | 16 | -6% | on curve | 0.3 | 49% |
| Barn Cat | Farm | 2 | 12 | 13 | -8% | on curve | 0.4 | 50% |
| Farm | Farm | 0 | 0 | 10 | -100% | starting card | 0 | - |
| Squirrel Drey | Forest | 0 | 22 | 10 | +120% | strong | 1.3 | 54% |
| Wildflower Meadow | Forest | 0 | 20 | 10 | +100% | strong | 2.2 | 50% |
| Pine | Forest | 2 | 24 | 13 | +85% | strong | 2.2 | 50% |
| Weeping Willow | Forest | 8 | 39 | 22 | +77% | strong | 0.7 | 59% |
| Blossom Hive | Forest | 3 | 24 | 14.5 | +66% | strong | 0.6 | 58% |
| Oak | Forest | 2 | 19 | 13 | +46% | strong | 2 | 53% |
| Dormouse Den | Forest | 6 | 20 | 19 | +5% | on curve | 0.4 | 52% |
| Blue Jay's Nest | Forest | 4 | 15 | 16 | -6% | on curve | 0.2 | 43% |
| Sapling | Forest | 0 | 9 | 10 | -10% | on curve | 20.6 | 49% |
| Huge Tree | Forest | 0 | 3 | 10 | -70% | starting card | 0 | - |
| Wrapping Station | Factory | 3 | 36 | 14.5 | +148% | strong | 2.9 | 51% |
| Tinker Shed | Factory | 2 | 22 | 13 | +69% | strong | 2 | 51% |
| Moonlight Robot | Factory | 2 | 20 | 13 | +54% | strong | 2.7 | 47% |
| Robot Charging Dock | Factory | 3 | 18.5 | 14.5 | +28% | strong | 0.6 | 38% |
| Blueprint Desk | Factory | 2 | 15 | 13 | +15% | strong | 0.6 | 56% |
| Scrap Pile | Factory | 0 | 10 | 10 | +0% | on curve | 5 | 51% |
| Big Whistle | Factory | 5 | 12 | 17.5 | -31% | weak | 0 | 53% |
| Toy Assembler | Factory | 3 | 8 | 14.5 | -45% | weak | 3.5 | 51% |
| Night Shift Assembler | Factory | 3 | 8 | 14.5 | -45% | weak | 2 | 51% |
| Spare Parts Bin | Factory | 0 | 5.3 | 10 | -47% | weak | 0.4 | 51% |
| Headquarters | Factory | 0 | 0 | 10 | -100% | starting card | 0 | - |
| Sleepy Web | DarkForest | 3 | 20 | 14.5 | +38% | strong | 2.1 | 50% |
| Glow Mushrooms | DarkForest | 0 | 12 | 10 | +20% | strong | 3.3 | 53% |
| Bat Belfry | DarkForest | 5 | 20 | 17.5 | +14% | on curve | 1.4 | 49% |
| Giggling Graveyard | DarkForest | 4 | 18 | 16 | +13% | on curve | 1.4 | 57% |
| Bat Roost | DarkForest | 0 | 10 | 10 | +0% | on curve | 4.1 | 51% |
| Cobweb Corner | DarkForest | 0 | 10 | 10 | +0% | on curve | 4.1 | 51% |
| Picnic Blanket | DarkForest | 2 | 12 | 13 | -8% | on curve | 0.7 | 49% |
| Haunted Lantern | DarkForest | 7 | 18 | 20.5 | -12% | on curve | 0.8 | 58% |
| Full Moon Shrine | DarkForest | 4 | 12 | 16 | -25% | weak | 1.1 | 55% |
| Evil Tree | DarkForest | 0 | 0 | 10 | -100% | starting card | 0 | - |

## Flags

* Farm vs DarkForest: seat 1 wins 57% (target 50% ± 5%).
* Factory vs DarkForest: seat 1 wins 59% (target 50% ± 5%).
* Factory vs DarkForest: games average 20 rounds (target at least 20).
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
