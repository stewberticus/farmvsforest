# Balance report

Bot: greedy, 1000 games each, target 20 rounds.

## Goldfish (each team alone; how fast does it reach its goal?)

| Team | Games | Reached goal | Median round | p10-p90 | Pass turns |
| --- | --- | --- | --- | --- | --- |
| Farm | 1000 | 100% | 22 | 19-29 | 3% |
| Forest | 1000 | 99% | 23 | 19-27 | 6% |
| Factory | 1000 | 100% | 22 | 17-27 | 1% |
| DarkForest | 1000 | 100% | 22 | 18-28 | 0% |

## 1v1 matchups

### Win rate (row team vs column team)

| | Farm | Forest | Factory | DarkForest |
| --- | --- | --- | --- | --- |
| **Farm** |  | 55% | 47% | 50% |
| **Forest** | 45% |  | 50% | 54% |
| **Factory** | 53% | 51% |  | 55% |
| **DarkForest** | 50% | 46% | 45% |  |

### Matchups

| Matchup | Games | Win A | Win B | Draw | Seat 1 wins | Mean rounds | Median rounds | p10-p90 | Goal / full / limit | First contact | Board fill |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| Farm vs Forest | 1000 | 55% | 45% | 0% | 41% | 20.8 | 20 | 18-24 | 100% / 0% / 0% | round 16 (93%) | 60% |
| Farm vs Factory | 1000 | 47% | 53% | 0% | 47% | 20.1 | 20 | 17-23 | 100% / 0% / 0% | round 18 (38%) | 41% |
| Farm vs DarkForest | 1000 | 50% | 50% | 0% | 52% | 20.7 | 20 | 18-24 | 100% / 0% / 0% | round 18 (53%) | 41% |
| Forest vs Factory | 1000 | 50% | 51% | 0% | 47% | 20.3 | 20 | 17-24 | 100% / 0% / 0% | round 15 (93%) | 60% |
| Forest vs DarkForest | 1000 | 54% | 46% | 0% | 49% | 20.9 | 21 | 18-24 | 100% / 0% / 0% | round 15 (94%) | 61% |
| Factory vs DarkForest | 1000 | 55% | 45% | 0% | 52% | 20.2 | 20 | 17-24 | 100% / 0% / 0% | round 16 (69%) | 41% |

### Overall, per team

| Team | Games | Win rate | Avg goal progress | Pass turns | Squares at end |
| --- | --- | --- | --- | --- | --- |
| Farm | 3000 | 51% | 80% | 3% | 20.6 |
| Forest | 3000 | 50% | 83% | 4% | 39.4 |
| Factory | 3000 | 53% | 73% | 1% | 20.8 |
| DarkForest | 3000 | 47% | 72% | 0% | 20.5 |

## Cards (value model vs par, and how they did in the matchups above)

| Card | Team | Cost (V) | Value (V) | Par | Delta | Verdict | Played per game | Win rate when played |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| Cow | Farm | 3 | 24 | 14.5 | +66% | strong | 3.9 | 51% |
| Chicken Coop | Farm | 3 | 24 | 14.5 | +66% | strong | 4 | 51% |
| Scarecrow | Farm | 1 | 12 | 11.5 | +4% | on curve | 1.2 | 50% |
| Big Red Barn | Farm | 6 | 19.8 | 19 | +4% | on curve | 0.9 | 51% |
| Nesting Box | Farm | 6 | 19.8 | 19 | +4% | on curve | 0.9 | 48% |
| Wheat Field | Farm | 0 | 10 | 10 | +0% | on curve | 8 | 51% |
| Farmhand's Cottage | Farm | 4 | 15 | 16 | -6% | on curve | 0.3 | 47% |
| Barn Cat | Farm | 2 | 12 | 13 | -8% | on curve | 0.4 | 48% |
| Farm | Farm | 0 | 0 | 10 | -100% | starting card | 0 | - |
| Blossom Hive | Forest | 3 | 24 | 14.5 | +66% | strong | 1.4 | 50% |
| Weeping Willow | Forest | 8 | 33 | 22 | +50% | strong | 0.7 | 57% |
| Pine | Forest | 2 | 18 | 13 | +38% | strong | 2.2 | 51% |
| Dormouse Den | Forest | 6 | 20 | 19 | +5% | on curve | 1.3 | 52% |
| Squirrel Drey | Forest | 0 | 10 | 10 | +0% | on curve | 2.7 | 51% |
| Wild Bees' Nest | Forest | 0 | 10 | 10 | +0% | on curve | 2.1 | 51% |
| Oak | Forest | 2 | 13 | 13 | +0% | on curve | 2.1 | 51% |
| Blue Jay's Nest | Forest | 4 | 15 | 16 | -6% | on curve | 1.4 | 52% |
| Sapling | Forest | 0 | 3 | 10 | -70% | weak | 24.4 | 50% |
| Huge Tree | Forest | 0 | 3 | 10 | -70% | starting card | 0 | - |
| Wrapping Station | Factory | 3 | 36 | 14.5 | +148% | strong | 3 | 53% |
| Tinker Shed | Factory | 2 | 22 | 13 | +69% | strong | 2.1 | 53% |
| Moonlight Robot | Factory | 2 | 20 | 13 | +54% | strong | 2.6 | 49% |
| Robot Charging Dock | Factory | 3 | 18.5 | 14.5 | +28% | strong | 0.6 | 40% |
| Blueprint Desk | Factory | 2 | 15 | 13 | +15% | strong | 0.6 | 60% |
| Scrap Pile | Factory | 0 | 10 | 10 | +0% | on curve | 5.1 | 53% |
| Big Whistle | Factory | 5 | 12 | 17.5 | -31% | weak | 0 | 53% |
| Toy Assembler | Factory | 3 | 8 | 14.5 | -45% | weak | 3.3 | 53% |
| Night Shift Assembler | Factory | 3 | 8 | 14.5 | -45% | weak | 2.1 | 53% |
| Spare Parts Bin | Factory | 0 | 5.3 | 10 | -47% | weak | 0.3 | 54% |
| Headquarters | Factory | 0 | 0 | 10 | -100% | starting card | 0 | - |
| Sleepy Web | DarkForest | 3 | 20 | 14.5 | +38% | strong | 2.3 | 47% |
| Glow Mushrooms | DarkForest | 0 | 12 | 10 | +20% | strong | 0.9 | 46% |
| Bat Belfry | DarkForest | 5 | 20 | 17.5 | +14% | on curve | 1.5 | 46% |
| Bat Roost | DarkForest | 0 | 10 | 10 | +0% | on curve | 4.8 | 47% |
| Cobweb Corner | DarkForest | 0 | 10 | 10 | +0% | on curve | 4.8 | 47% |
| Picnic Blanket | DarkForest | 2 | 12 | 13 | -8% | on curve | 1.5 | 46% |
| Full Moon Shrine | DarkForest | 4 | 12 | 16 | -25% | weak | 1.5 | 52% |
| Haunted Lantern | DarkForest | 7 | 15 | 20.5 | -27% | weak | 0.8 | 55% |
| Giggling Graveyard | DarkForest | 4 | 7.5 | 16 | -53% | weak | 1.4 | 52% |
| Evil Tree | DarkForest | 0 | 0 | 10 | -100% | starting card | 0 | - |

## Flags

* Farm vs Forest: seat 1 wins 41% (target 50% ± 5%).
* Cow (Farm) is strong: every day: 1 Milk per touching crop (x1.2) = 24
* Chicken Coop (Farm) is strong: every night: 1 Eggs per touching crop (x1.2) = 24
* Blossom Hive (Forest) is strong: every day: 1 Honey per touching tree (x1.2) = 24
* Weeping Willow (Forest) is strong: every day: add 1 Sapling card to hand = 30; tag tree = 3
* Pine (Forest) is strong: every 2 nights: add 1 Sapling card to hand = 15; tag tree = 3
* Sapling (Forest) is weak: tag tree = 3
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
