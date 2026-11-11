# Phase 4: steering draws (2026-11-11)

Phase 4 of the [implementation plan](https://claude.ai/artifact/HiebN1HdedieXB72aDCvpq).

## What changed

* **Mulligan** (`mulligans`, 1 by default): at the start of your first turn, before doing anything, you may shuffle
  your opening hand into your deck and draw the same number. The bot mulligans when nothing in hand is playable within
  two turns.
* **Scout** (`EffectKind.Scout`): look at the top N cards of your deck, keep 1, put the rest on the bottom. The match
  holds a pending choice (`Match.PendingScout`, `Match.ChooseScouted`); plays wait until it's made, and ending the turn
  keeps the first card. Bots keep the card that scores best (random picks at random, worst picks the worst).
* **Scout cards**, 2 or 3 per team, each also doing something small, replacing the least-played card:
  Weathervane (Farm, for Farmhand's Cottage), Owl's Hollow (Forest, for Blue Jay's Nest), Mail-Order Catalogue
  (Factory, 3 copies, for Spare Parts Bin), Fortune-Telling Toad (Dark Forest, for Picnic Blanket and one Glow
  Mushrooms). No plain extra draw.
* Bot value of a scout (`BalanceSettings.scoutValue`) is 14 V. At 6 the bot hardly played scout cards (0.2 to 1.5 a
  game) and shuffle luck barely moved; at 14 they're played 1.2 to 1.5 times a game.
* Retune: Farm 62 to 66 each, Forest 23 to 22 Old Trees.

## Done-when check

| Check | Target | Result |
| --- | --- | --- |
| Shuffle luck | under 0.75 every team | Yes: Farm 0.74, Forest 0.59, Factory 0.63, Dark Forest 0.70 (phase 3: 0.84, 0.62, 0.71, 0.76) |
| Good beats sloppy | 60% every mirror | No: 53 to 57%. The gap between the greedy bot and a noisy greedy bot is small whatever the cards do |
| Pass turns | under 5% | 1% for every team |
| Matchups | 45 to 55% | 43 to 57%: Factory beats Farm 54 to 57%, Farm beats Forest 55 to 56% |

The mulligan changed nothing in the sim: the same matchups with `mulligans=0` gave identical results, because with
starting-card income and a third of each deck free, the bot almost never holds a hand worth throwing back. It's
still in for people, who will want it after a bad opening.

Farm vs Factory and Farm vs Forest pull in opposite directions (raising Farm's goal helps the first and hurts the
second), and Factory's goal is coarse (16 to 17 Toys moves Forest vs Factory from 51% to 61%). Phase 5 rewrites
Factory's finisher, so the last few points are left for it and phase 7.

Commands: `--games 1000 --seed 3001`, `--only matchups --games 1000 --seed 4001`, `--skill --games 600 --seed 3001`,
`--only matchups --games 1000 --seed 3001 --set mulligans=0`.

## Skill

| Team | Good bot | Random bot | Worst bot | Good beats random | Good beats sloppy | Good beats worst | Shuffle luck |
| --- | --- | --- | --- | --- | --- | --- | --- |
| Farm | round 20 (18-25), 99% | round 38 (30-49), 94% | never (0%) | 99% | 54% | 100% | 0.74 |
| Forest | round 21 (18-27), 100% | round 28 (24-34), 99% | round 48.5 (46-51), 1% | 84% | 55% | 92% | 0.59 |
| Factory | round 20 (15-25), 99% | round 38 (31-44), 98% | round 55 (52-58), 99% | 98% | 53% | 99% | 0.63 |
| DarkForest | round 20 (18-23), 100% | round 29 (25-33), 100% | round 42 (38-46), 64% | 98% | 57% | 100% | 0.70 |

Dark Forest Ghouls per solo game (summoned / went home hungry):

| Bot | Summoned | Went home hungry |
| --- | --- | --- |
| greedy | 8.0 | 0.00 |
| sloppy | 8.2 | 0.00 |
| random | 10.5 | 0.52 |
| worst | 17.8 | 5.82 |

## Seed 3001

## Goldfish (each team alone; how fast does it reach its goal?)

| Team | Games | Reached goal | Median round | p10-p90 | Pass turns |
| --- | --- | --- | --- | --- | --- |
| Farm | 1000 | 100% | 20 | 18-25 | 1% |
| Forest | 1000 | 100% | 21 | 18-26.1 | 1% |
| Factory | 1000 | 100% | 20 | 15-25 | 1% |
| DarkForest | 1000 | 100% | 20 | 18-23 | 0% |

## 1v1 matchups

### Win rate (row team vs column team)

| | Farm | Forest | Factory | DarkForest |
| --- | --- | --- | --- | --- |
| **Farm** |  | 55% | 43% | 55% |
| **Forest** | 45% |  | 50% | 51% |
| **Factory** | 57% | 50% |  | 55% |
| **DarkForest** | 45% | 49% | 45% |  |

### Matchups

| Matchup | Games | Win A | Win B | Draw | Seat 1 wins | Mean rounds | Median rounds | p10-p90 | Goal / full / limit | First contact | Contact by round 13 | Board fill |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| Farm vs Forest | 1000 | 55% | 45% | 0% | 47% | 19.6 | 19 | 17-22 | 99% / 1% / 0% | round 13 (98%) | 56% | 72% |
| Farm vs Factory | 1000 | 43% | 57% | 0% | 52% | 18.5 | 18 | 15-22 | 100% / 0% / 0% | round 15 (66%) | 17% | 49% |
| Farm vs DarkForest | 1000 | 55% | 45% | 0% | 49% | 19.2 | 19 | 17-22 | 100% / 0% / 0% | round 15 (72%) | 14% | 51% |
| Forest vs Factory | 1000 | 50% | 50% | 0% | 52% | 19.1 | 19 | 16-23 | 98% / 2% / 0% | round 12 (98%) | 74% | 72% |
| Forest vs DarkForest | 1000 | 51% | 49% | 0% | 49% | 19.7 | 20 | 18-22 | 100% / 1% / 0% | round 12 (99%) | 71% | 75% |
| Factory vs DarkForest | 1000 | 55% | 45% | 0% | 51% | 18.7 | 19 | 16-22 | 100% / 0% / 0% | round 14 (78%) | 34% | 50% |

### Overall, per team

| Team | Games | Win rate | Avg goal progress | Pass turns | Squares at end |
| --- | --- | --- | --- | --- | --- |
| Farm | 3000 | 51% | 83% | 1% | 19.5 |
| Forest | 3000 | 48% | 84% | 1% | 36.5 |
| Factory | 3000 | 54% | 75% | 1% | 19.4 |
| DarkForest | 3000 | 47% | 85% | 1% | 19.3 |

## Cards (value model vs par, and how they did in the matchups above)

| Card | Team | Cost (V) | Value (V) | Par | Delta | Verdict | Played per game | Win rate when played |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| Cow | Farm | 3 | 24 | 14.5 | +66% | strong | 3.5 | 51% |
| Chicken Coop | Farm | 3 | 24 | 14.5 | +66% | strong | 3.6 | 51% |
| Weathervane | Farm | 1 | 19 | 11.5 | +65% | strong | 1.5 | 52% |
| Scarecrow | Farm | 1 | 12 | 11.5 | +4% | on curve | 1.1 | 48% |
| Big Red Barn | Farm | 6 | 19.8 | 19 | +4% | on curve | 0.6 | 46% |
| Nesting Box | Farm | 6 | 19.8 | 19 | +4% | on curve | 0.6 | 48% |
| Wheat Field | Farm | 0 | 10 | 10 | +0% | on curve | 7.4 | 51% |
| Farmhand's Cottage | Farm | 4 | 15 | 16 | -6% | on curve | 0 | - |
| Barn Cat | Farm | 2 | 12 | 13 | -8% | on curve | 0.3 | 45% |
| Farm | Farm | 0 | 0 | 10 | -100% | starting card | 0 | - |
| Squirrel Drey | Forest | 0 | 22 | 10 | +120% | strong | 1.2 | 49% |
| Owl's Hollow | Forest | 1 | 24 | 11.5 | +109% | strong | 1.2 | 49% |
| Wildflower Meadow | Forest | 0 | 20 | 10 | +100% | strong | 2.1 | 49% |
| Pine | Forest | 2 | 24 | 13 | +85% | strong | 2.5 | 49% |
| Weeping Willow | Forest | 8 | 39 | 22 | +77% | strong | 0.9 | 53% |
| Blossom Hive | Forest | 3 | 24 | 14.5 | +66% | strong | 0.9 | 49% |
| Oak | Forest | 2 | 19 | 13 | +46% | strong | 2.2 | 49% |
| Dormouse Den | Forest | 6 | 20 | 19 | +5% | on curve | 0.5 | 48% |
| Sapling | Forest | 0 | 9 | 10 | -10% | on curve | 23.9 | 48% |
| Huge Tree | Forest | 0 | 3 | 10 | -70% | starting card | 0 | - |
| Wrapping Station | Factory | 3 | 36 | 14.5 | +148% | strong | 3.2 | 54% |
| Tinker Shed | Factory | 2 | 22 | 13 | +69% | strong | 1.9 | 52% |
| Moonlight Robot | Factory | 2 | 20 | 13 | +54% | strong | 2.4 | 50% |
| Mail-Order Catalogue | Factory | 0 | 15 | 10 | +50% | strong | 1.3 | 51% |
| Robot Charging Dock | Factory | 3 | 18.5 | 14.5 | +28% | strong | 0.6 | 42% |
| Blueprint Desk | Factory | 2 | 15 | 13 | +15% | strong | 0.3 | 70% |
| Scrap Pile | Factory | 0 | 10 | 10 | +0% | on curve | 3.7 | 53% |
| Big Whistle | Factory | 5 | 12 | 17.5 | -31% | weak | 0 | 67% |
| Toy Assembler | Factory | 3 | 8 | 14.5 | -45% | weak | 2.9 | 54% |
| Night Shift Assembler | Factory | 3 | 8 | 14.5 | -45% | weak | 2.1 | 54% |
| Spare Parts Bin | Factory | 0 | 5.3 | 10 | -47% | weak | 0 | - |
| Headquarters | Factory | 0 | 0 | 10 | -100% | starting card | 0 | - |
| Fortune-Telling Toad | DarkForest | 1 | 24 | 11.5 | +109% | strong | 1.5 | 48% |
| Sleepy Web | DarkForest | 3 | 20 | 14.5 | +38% | strong | 2.5 | 47% |
| Glow Mushrooms | DarkForest | 0 | 12 | 10 | +20% | strong | 1.1 | 40% |
| Bat Belfry | DarkForest | 5 | 20 | 17.5 | +14% | on curve | 1.6 | 47% |
| Bat Roost | DarkForest | 0 | 10 | 10 | +0% | on curve | 3.6 | 47% |
| Cobweb Corner | DarkForest | 0 | 10 | 10 | +0% | on curve | 3.7 | 47% |
| Picnic Blanket | DarkForest | 2 | 12 | 13 | -8% | on curve | 0 | - |
| Giggling Graveyard | DarkForest | 4 | 12 | 16 | -25% | weak | 2 | 49% |
| Haunted Lantern | DarkForest | 5 | 12 | 17.5 | -31% | weak | 1 | 50% |
| Full Moon Shrine | DarkForest | 4 | 7.5 | 16 | -53% | weak | 1.2 | 53% |
| Evil Tree | DarkForest | 0 | 0 | 10 | -100% | starting card | 0 | - |

## Seed 4001

## 1v1 matchups

### Win rate (row team vs column team)

| | Farm | Forest | Factory | DarkForest |
| --- | --- | --- | --- | --- |
| **Farm** |  | 56% | 46% | 54% |
| **Forest** | 44% |  | 51% | 49% |
| **Factory** | 54% | 49% |  | 55% |
| **DarkForest** | 46% | 51% | 45% |  |

### Matchups

| Matchup | Games | Win A | Win B | Draw | Seat 1 wins | Mean rounds | Median rounds | p10-p90 | Goal / full / limit | First contact | Contact by round 13 | Board fill |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| Farm vs Forest | 1000 | 56% | 44% | 0% | 53% | 19.5 | 19 | 17-22 | 99% / 1% / 0% | round 13 (98%) | 59% | 72% |
| Farm vs Factory | 1000 | 46% | 54% | 0% | 51% | 18.4 | 18 | 15-22 | 100% / 0% / 0% | round 15 (66%) | 16% | 49% |
| Farm vs DarkForest | 1000 | 54% | 46% | 0% | 53% | 19.2 | 19 | 17-22 | 100% / 0% / 0% | round 15 (73%) | 15% | 51% |
| Forest vs Factory | 1000 | 51% | 49% | 0% | 51% | 19 | 19 | 15-22.1 | 98% / 2% / 0% | round 12 (98%) | 74% | 72% |
| Forest vs DarkForest | 1000 | 49% | 51% | 0% | 49% | 19.6 | 19 | 17-22 | 99% / 1% / 0% | round 12 (99%) | 72% | 74% |
| Factory vs DarkForest | 1000 | 55% | 45% | 0% | 54% | 18.6 | 18 | 16-22 | 100% / 0% / 0% | round 14 (81%) | 38% | 50% |

### Overall, per team

| Team | Games | Win rate | Avg goal progress | Pass turns | Squares at end |
| --- | --- | --- | --- | --- | --- |
| Farm | 3000 | 52% | 82% | 1% | 19.4 |
| Forest | 3000 | 48% | 83% | 1% | 36.4 |
| Factory | 3000 | 53% | 74% | 1% | 19.3 |
| DarkForest | 3000 | 47% | 85% | 1% | 19.2 |

