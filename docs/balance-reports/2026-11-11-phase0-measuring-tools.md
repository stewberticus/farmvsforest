# Phase 0: measuring tools (2026-11-11)

First step of the [implementation plan](https://claude.ai/artifact/HiebN1HdedieXB72aDCvpq). No rules change; this
adds the numbers the later phases are judged on, measured on main after PR #20.

New in the sim:

* **Sloppy bot** (`--bot sloppy`, and a column in `--skill`): the greedy bot with noise 6 instead of 1, so it often takes
  the second or third best play. "Good beats sloppy" is the realistic skill number: random and worst are far weaker than
  any real player.
* **Shuffle luck** (column in `--skill`): each team plays the same solo games twice, with the same deck orders but
  different bot seeds, and the report gives the correlation of the two finish rounds. 1 means the shuffle decides when
  you finish; 0 means the choices along the way do.
* **Does the opponent matter** (`--compare-board A,B`): every matchup at two board sizes, with the win rate and length
  changes and how often the teams touch by round 13.

Commands:

```
dotnet run -c Release --project Tools/BalanceSim -- --skill --games 600 --seed 3001
dotnet run -c Release --project Tools/BalanceSim -- --compare-board 10,16 --games 1000 --seed 3001
dotnet run -c Release --project Tools/BalanceSim -- --compare-board 8,16 --games 1000 --seed 3001
```

The baseline numbers in the plan (from throwaway code) reproduce: shuffle luck 0.76 to 0.91 (plan: 0.77 to 0.93),
good beats sloppy 51 to 57% (plan: 49 to 56%), and 10x10 plays the same as 16x16.

## Skill: how much do decisions matter?

Solo finish round (median, with the middle 80% of games) and how often the goal is reached by round 60. Mirror columns: how often the good bot beats the other bot with the same team on both sides. Sloppy is the good bot with noise 6 instead of 1. Shuffle luck: correlation of the solo finish round when the same deck orders are replayed with different bot choices (1 = the shuffle decides everything).

| Team | Good bot | Random bot | Worst bot | Good beats random | Good beats sloppy | Good beats worst | Shuffle luck |
| --- | --- | --- | --- | --- | --- | --- | --- |
| Farm | round 22 (19-29), 99% | round 39 (31-48), 95% | never (0%) | 98% | 54% | 99% | 0.90 |
| Forest | round 22 (19-26), 100% | round 28 (24-32), 98% | round 56 (51-60), 3% | 94% | 51% | 100% | 0.76 |
| Factory | round 22 (17-27), 100% | round 42 (35-48), 99% | round 49 (47-51), 100% | 99% | 57% | 100% | 0.79 |
| DarkForest | round 23 (17-29), 100% | round 37 (24-54), 72% | never (0%) | 94% | 51% | 100% | 0.91 |


## Does the opponent matter? Board 10x10 vs 16x16

Win rate is the first team's. If nothing changes between the two sizes, the teams never get in each other's way.

| Matchup | Win 10 | Win 16 | Change | Mean rounds 10 | Mean rounds 16 | Change | Contact by round 13 (10) | Contact by round 13 (16) | Goal endings 10 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| Farm vs Forest | 51% | 53% | -2 pts | 21.0 | 21.0 | 0.0 | 6% | 0% | 100% |
| Farm vs Factory | 46% | 46% | 0 pts | 20.4 | 20.4 | 0.0 | 1% | 0% | 100% |
| Farm vs DarkForest | 49% | 51% | -2 pts | 20.7 | 20.6 | 0.0 | 2% | 0% | 100% |
| Forest vs Factory | 51% | 49% | +2 pts | 20.4 | 20.4 | 0.0 | 14% | 0% | 100% |
| Forest vs DarkForest | 54% | 52% | +2 pts | 20.6 | 20.6 | 0.0 | 18% | 0% | 100% |
| Factory vs DarkForest | 53% | 55% | -3 pts | 20.2 | 20.1 | +0.1 | 9% | 0% | 100% |

Average change: 1.7 win-rate points, 0.0 rounds.


## Does the opponent matter? Board 8x8 vs 16x16

Win rate is the first team's. If nothing changes between the two sizes, the teams never get in each other's way.

| Matchup | Win 8 | Win 16 | Change | Mean rounds 8 | Mean rounds 16 | Change | Contact by round 13 (8) | Contact by round 13 (16) | Goal endings 8 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| Farm vs Forest | 50% | 53% | -3 pts | 21.1 | 21.0 | +0.1 | 63% | 0% | 93% |
| Farm vs Factory | 50% | 46% | +4 pts | 20.7 | 20.4 | +0.3 | 55% | 0% | 99% |
| Farm vs DarkForest | 50% | 51% | -1 pts | 20.7 | 20.6 | +0.1 | 50% | 0% | 100% |
| Forest vs Factory | 58% | 49% | +9 pts | 21.0 | 20.4 | +0.6 | 73% | 0% | 87% |
| Forest vs DarkForest | 51% | 52% | -1 pts | 20.9 | 20.6 | +0.3 | 72% | 0% | 88% |
| Factory vs DarkForest | 51% | 55% | -5 pts | 20.6 | 20.1 | +0.5 | 66% | 0% | 99% |

Average change: 3.8 win-rate points, 0.3 rounds.

