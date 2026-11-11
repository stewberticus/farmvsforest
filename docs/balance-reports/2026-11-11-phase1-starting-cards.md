# Phase 1: starting cards work (2026-11-11)

Each starting card now makes 1 basic resource: Farm 1 Wheat a day, Huge Tree 1 Acorn a day, Headquarters 1 Part a day,
Evil Tree 1 Bat a night (`startingCardIncome`, 0 turns it off). The free-card opening rule is off by default
(`openingHandHasFreeCard`). The Huge Tree's text now carries the Compost rule.

Retune: Forest got about 2 rounds faster solo (median 19 at the old goal of 20), so its goal went up. A sweep put the
solo median at 21 for 22 to 24 Old Trees, but in matchups Forest still won 57 to 58% at 23. At 25 Farm beat Forest
57%, so 24 is the middle. Farm went from 62 to 64 each to keep Farm vs Forest even. Chosen: **Forest 24 Old Trees,
Farm 64 each**.

Factory's "always leaves you 3 Parts" and the Spare Parts Bin were left alone: Headquarters makes it hard to get stuck
now, but they're part of how Factory paces, and phase 5 changes Factory anyway.

## Done-when check

| Check | Target | Result |
| --- | --- | --- |
| Stuck solo games, free-card rule off | 0 in 2,000 per team | 0 for Forest, Factory and Dark Forest; Farm misses the goal by round 60 in 1% (slow games, not a lock-out: it still plays most turns, as before phase 1) |
| Solo medians | 21 to 23 | Farm 21, Forest 21, Factory 21, Dark Forest 22 |
| Matchups | 45 to 55% | 46 to 54% on seed 3001; seed 4001 has Forest at 56% vs Factory and 57% vs Dark Forest |
| Mean length | 20+ rounds | 19.7 to 20.4 (Factory vs Dark Forest 19.7) |

Forest's two matchups and the 19.7-round mean are just outside the targets. Phase 2 moves to an 8x8 board, which
changes every matchup, so they're retuned there rather than chased here.

Commands: `--games 1000 --seed 3001`, `--only matchups --games 1000 --seed 4001`, `--skill --games 600 --seed 3001`,
`--only goldfish --games 2000 --seed 3001`.

## Skill

| Team | Good bot | Random bot | Worst bot | Good beats random | Good beats sloppy | Good beats worst | Shuffle luck |
| --- | --- | --- | --- | --- | --- | --- | --- |
| Farm | round 22 (18-29), 98% | round 40 (31-49), 95% | never (0%) | 97% | 53% | 99% | 0.88 |
| Forest | round 22 (19-26), 100% | round 28 (25-32), 99% | never (0%) | 88% | 53% | 98% | 0.74 |
| Factory | round 21 (17-27), 99% | round 42 (33-48), 100% | round 48 (45-50), 100% | 98% | 58% | 99% | 0.80 |
| DarkForest | round 22 (17-28), 100% | round 37 (24-52), 73% | round 53 (53-53), 0% | 93% | 52% | 100% | 0.92 |

## Solo, 2,000 games

| Team | Games | Reached goal | Median round | p10-p90 | Pass turns |
| --- | --- | --- | --- | --- | --- |
| Farm | 2000 | 99% | 21 | 18-29 | 2% |
| Forest | 2000 | 100% | 21 | 19-25.1 | 1% |
| Factory | 2000 | 100% | 21 | 17-28 | 1% |
| DarkForest | 2000 | 100% | 22 | 17-28 | 0% |


## Seed 3001

## 1v1 matchups

### Win rate (row team vs column team)

| | Farm | Forest | Factory | DarkForest |
| --- | --- | --- | --- | --- |
| **Farm** |  | 51% | 50% | 51% |
| **Forest** | 49% |  | 54% | 54% |
| **Factory** | 50% | 46% |  | 50% |
| **DarkForest** | 49% | 46% | 50% |  |

### Matchups

| Matchup | Games | Win A | Win B | Draw | Seat 1 wins | Mean rounds | Median rounds | p10-p90 | Goal / full / limit | First contact | Board fill |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| Farm vs Forest | 1000 | 51% | 49% | 0% | 47% | 20.4 | 20 | 18-23 | 100% / 0% / 0% | round 16 (87%) | 56% |
| Farm vs Factory | 1000 | 50% | 50% | 0% | 51% | 20 | 20 | 17-24 | 100% / 0% / 0% | round 19 (34%) | 41% |
| Farm vs DarkForest | 1000 | 51% | 49% | 0% | 50% | 19.9 | 20 | 16-24 | 100% / 0% / 0% | round 17 (46%) | 40% |
| Forest vs Factory | 1000 | 54% | 46% | 0% | 51% | 20.2 | 20 | 17-23 | 100% / 0% / 0% | round 15 (91%) | 57% |
| Forest vs DarkForest | 1000 | 54% | 46% | 0% | 51% | 20.1 | 20 | 17-24 | 100% / 0% / 0% | round 14 (90%) | 56% |
| Factory vs DarkForest | 1000 | 50% | 50% | 0% | 53% | 19.7 | 19 | 16-24 | 100% / 0% / 0% | round 16 (58%) | 40% |

### Overall, per team

| Team | Games | Win rate | Avg goal progress | Pass turns | Squares at end |
| --- | --- | --- | --- | --- | --- |
| Farm | 3000 | 50% | 78% | 2% | 20.2 |
| Forest | 3000 | 53% | 86% | 1% | 36.1 |
| Factory | 3000 | 49% | 71% | 1% | 20.6 |
| DarkForest | 3000 | 49% | 71% | 0% | 19.8 |

## Seed 4001

## 1v1 matchups

### Win rate (row team vs column team)

| | Farm | Forest | Factory | DarkForest |
| --- | --- | --- | --- | --- |
| **Farm** |  | 52% | 51% | 53% |
| **Forest** | 48% |  | 56% | 57% |
| **Factory** | 49% | 44% |  | 53% |
| **DarkForest** | 47% | 43% | 47% |  |

### Matchups

| Matchup | Games | Win A | Win B | Draw | Seat 1 wins | Mean rounds | Median rounds | p10-p90 | Goal / full / limit | First contact | Board fill |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| Farm vs Forest | 1000 | 52% | 48% | 0% | 53% | 20.3 | 20 | 18-23 | 100% / 0% / 0% | round 16 (89%) | 56% |
| Farm vs Factory | 1000 | 51% | 49% | 0% | 51% | 19.9 | 20 | 17-23 | 100% / 0% / 0% | round 18 (36%) | 40% |
| Farm vs DarkForest | 1000 | 53% | 47% | 0% | 53% | 20 | 20 | 17-23.1 | 100% / 0% / 0% | round 17 (46%) | 40% |
| Forest vs Factory | 1000 | 56% | 44% | 0% | 49% | 20 | 20 | 17-23 | 100% / 0% / 0% | round 15 (91%) | 57% |
| Forest vs DarkForest | 1000 | 57% | 43% | 0% | 50% | 20 | 20 | 17-23 | 100% / 0% / 0% | round 14 (91%) | 56% |
| Factory vs DarkForest | 1000 | 53% | 47% | 0% | 54% | 19.7 | 19 | 16-24 | 100% / 0% / 0% | round 16 (59%) | 40% |

### Overall, per team

| Team | Games | Win rate | Avg goal progress | Pass turns | Squares at end |
| --- | --- | --- | --- | --- | --- |
| Farm | 3000 | 52% | 80% | 2% | 20.2 |
| Forest | 3000 | 53% | 87% | 1% | 36.1 |
| Factory | 3000 | 49% | 70% | 1% | 20.5 |
| DarkForest | 3000 | 46% | 70% | 0% | 19.8 |

