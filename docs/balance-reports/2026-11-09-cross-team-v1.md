# Balance report

Bot: greedy, 800 games each, target 20 rounds.

## Goldfish (each team alone; how fast does it reach its goal?)

| Team | Games | Reached goal | Median round | p10-p90 | Pass turns |
| --- | --- | --- | --- | --- | --- |
| Farm | 800 | 100% | 20 | 17-26 | 3% |
| Forest | 800 | 99% | 21 | 18-25 | 6% |
| Factory | 800 | 94% | 19 | 15-29 | 14% |
| DarkForest | 800 | 100% | 20 | 17-25 | 1% |

## 1v1 matchups

### Win rate (row team vs column team)

| | Farm | Forest | Factory | DarkForest |
| --- | --- | --- | --- | --- |
| **Farm** |  | 57% | 47% | 53% |
| **Forest** | 43% |  | 49% | 49% |
| **Factory** | 53% | 51% |  | 55% |
| **DarkForest** | 47% | 51% | 45% |  |

### Matchups

| Matchup | Games | Win A | Win B | Draw | Seat 1 wins | Median rounds | p10-p90 | Goal / full / limit | First contact | Board fill |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| Farm vs Forest | 800 | 57% | 43% | 0% | 42% | 19 | 16-22 | 100% / 0% / 0% | round 15 (81%) | 52% |
| Farm vs Factory | 800 | 47% | 53% | 0% | 46% | 18 | 15-22 | 100% / 0% / 0% | round 17 (28%) | 36% |
| Farm vs DarkForest | 800 | 53% | 47% | 0% | 50% | 19 | 16-22 | 100% / 0% / 0% | round 17 (35%) | 38% |
| Forest vs Factory | 800 | 49% | 51% | 0% | 45% | 18 | 15-22 | 100% / 0% / 0% | round 14 (82%) | 50% |
| Forest vs DarkForest | 800 | 49% | 51% | 0% | 47% | 19 | 17-22 | 100% / 0% / 0% | round 15 (92%) | 53% |
| Factory vs DarkForest | 800 | 55% | 45% | 0% | 50% | 18 | 15-22 | 100% / 0% / 0% | round 16 (48%) | 36% |

### Overall, per team

| Team | Games | Win rate | Avg goal progress | Pass turns | Squares at end |
| --- | --- | --- | --- | --- | --- |
| Farm | 2400 | 52% | 78% | 3% | 18.7 |
| Forest | 2400 | 47% | 81% | 4% | 33.3 |
| Factory | 2400 | 53% | 73% | 5% | 18.1 |
| DarkForest | 2400 | 48% | 79% | 1% | 18.6 |

## Cards (value model vs par, and how they did in the matchups above)

| Card | Team | Cost (V) | Value (V) | Par | Delta | Verdict | Played per game | Win rate when played |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| Cow | Farm | 3 | 24 | 14.5 | +66% | strong | 3.4 | 53% |
| Chicken Coop | Farm | 3 | 24 | 14.5 | +66% | strong | 3.5 | 52% |
| Scarecrow | Farm | 1 | 12 | 11.5 | +4% | on curve | 1.2 | 50% |
| Big Red Barn | Farm | 6 | 19.8 | 19 | +4% | on curve | 0.7 | 51% |
| Nesting Box | Farm | 6 | 19.8 | 19 | +4% | on curve | 0.7 | 51% |
| Wheat Field | Farm | 0 | 10 | 10 | +0% | on curve | 7.5 | 52% |
| Farmhand's Cottage | Farm | 4 | 15 | 16 | -6% | on curve | 0.2 | 50% |
| Barn Cat | Farm | 2 | 12 | 13 | -8% | on curve | 0.4 | 50% |
| Farm | Farm | 0 | 0 | 10 | -100% | starting card | 0 | - |
| Blossom Hive | Forest | 3 | 24 | 14.5 | +66% | strong | 1.3 | 47% |
| Weeping Willow | Forest | 8 | 33 | 22 | +50% | strong | 0.7 | 56% |
| Pine | Forest | 2 | 18 | 13 | +38% | strong | 2 | 48% |
| Dormouse Den | Forest | 6 | 20 | 19 | +5% | on curve | 1.2 | 50% |
| Squirrel Drey | Forest | 0 | 10 | 10 | +0% | on curve | 2.5 | 49% |
| Wild Bees' Nest | Forest | 0 | 10 | 10 | +0% | on curve | 1.9 | 48% |
| Oak | Forest | 2 | 13 | 13 | +0% | on curve | 1.9 | 49% |
| Blue Jay's Nest | Forest | 4 | 15 | 16 | -6% | on curve | 1.2 | 50% |
| Sapling | Forest | 0 | 3 | 10 | -70% | weak | 19.6 | 47% |
| Huge Tree | Forest | 0 | 3 | 10 | -70% | starting card | 0 | - |
| Wrapping Station | Factory | 3 | 36 | 14.5 | +148% | strong | 2.4 | 55% |
| Tinker Shed | Factory | 2 | 22 | 13 | +69% | strong | 1.7 | 55% |
| Moonlight Robot | Factory | 2 | 20 | 13 | +54% | strong | 2.5 | 49% |
| Robot Charging Dock | Factory | 3 | 18.5 | 14.5 | +28% | strong | 0.5 | 35% |
| Blueprint Desk | Factory | 2 | 15 | 13 | +15% | strong | 0.5 | 52% |
| Scrap Pile | Factory | 0 | 10 | 10 | +0% | on curve | 4.7 | 53% |
| Toy Assembler | Factory | 3 | 8 | 14.5 | -45% | weak | 2.7 | 54% |
| Night Shift Assembler | Factory | 3 | 8 | 14.5 | -45% | weak | 1.6 | 56% |
| Spare Parts Bin | Factory | 0 | 2 | 10 | -80% | weak | 0.6 | 50% |
| Big Whistle | Factory | 5 | 0 | 17.5 | -100% | price by hand | 0 | - |
| Headquarters | Factory | 0 | 0 | 10 | -100% | starting card | 0 | - |
| Sleepy Web | DarkForest | 3 | 20 | 14.5 | +38% | strong | 2.1 | 48% |
| Glow Mushrooms | DarkForest | 0 | 12 | 10 | +20% | strong | 1 | 48% |
| Haunted Lantern | DarkForest | 10 | 30 | 25 | +20% | strong | 0.8 | 56% |
| Bat Belfry | DarkForest | 5 | 20 | 17.5 | +14% | on curve | 1.4 | 47% |
| Bat Roost | DarkForest | 0 | 10 | 10 | +0% | on curve | 4 | 48% |
| Cobweb Corner | DarkForest | 0 | 10 | 10 | +0% | on curve | 3.4 | 48% |
| Picnic Blanket | DarkForest | 2 | 12 | 13 | -8% | on curve | 1.4 | 49% |
| Full Moon Shrine | DarkForest | 4 | 12 | 16 | -25% | weak | 1.4 | 50% |
| Giggling Graveyard | DarkForest | 4 | 10 | 16 | -38% | weak | 2.1 | 48% |
| Evil Tree | DarkForest | 0 | 0 | 10 | -100% | starting card | 0 | - |

## Flags

* Goldfish Factory: reached its goal in only 94% of games.
* Farm vs Forest: seat 1 wins 42% (target 50% ± 5%).
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
* Toy Assembler (Factory) is weak: every day: 2 Parts -> 1 Toys = 8
* Night Shift Assembler (Factory) is weak: every night: 2 Parts -> 1 Toys = 8
* Spare Parts Bin (Factory) is weak: on play: 2 Parts = 2
* Big Whistle (Factory) is price by hand: on play: run_tagged = 0
* Sleepy Web (DarkForest) is strong: every day: 2 Spiders = 20
* Glow Mushrooms (DarkForest) is strong: every night: 1 Bats per touching mushroom (x1.2) = 12
* Haunted Lantern (DarkForest) is strong: every night: 1 Ghouls = 30
* Full Moon Shrine (DarkForest) is weak: every 3 nights: 1 Ghouls per touching haunt (x1.2) = 12
* Giggling Graveyard (DarkForest) is weak: every 3 nights: 1 Ghouls = 10
