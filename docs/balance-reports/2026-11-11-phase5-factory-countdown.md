# Phase 5: Factory's Big Order (2026-11-11)

Phase 5 of the [implementation plan](https://claude.ai/artifact/HiebN1HdedieXB72aDCvpq).

## What changed

* **Big Order** (Factory, 4 Parts, 2 copies): "In 3 days: every Assembler you own runs twice that day." It's a
  one-time day effect with a 3-day timer (`interval = 3`, `repeat = false`) using the `run_tagged` custom effect, which
  now runs the tagged cards `amount` times. On the payoff day each Assembler runs three times in all.
* It replaces **Big Whistle** (now 0 copies; the bot played it 0 times a game) and one **Wrapping Station** (4 to 3).
* **Forecast:** `FactoryCards.OrderDaysLeft` (the countdown on a card), `ProjectedToysThisRound` (Toys made this round
  plus what the night will still make) and `ForecastToys` (a rough forecast for a round N days ahead, from Parts
  income, the Parts pile, Assemblers, Wrapping Stations and Orders due that day).
* **Demo:** a Big Order on the board says "due in N days" / "due tomorrow!", and Factory's goal line adds
  "on track for N" and, while an Order is counting down, "Big Order in N days: about M". The demo is Unity-only code
  and wasn't compiled here; it uses only the core helpers above.
* **Bot:** the good bot holds a Big Order until the forecast for its payoff day reaches the goal
  (`Bots.OrderIsReady`), then scores it high. The worst and random bots play it whenever.
* **Retune:** Factory 16 to 17 Toys in a round. At 16 Factory won 54 to 57% of every matchup; at 18, 38 to 41%.

## Done-when check

| Check | Target | Result |
| --- | --- | --- |
| Good bot plays Big Order | at least half its games | Yes: 60 to 67% of games (vs Farm, Forest, Dark Forest). About two thirds of Factory wins land on an Order's payoff day |
| Factory solo median | 21 to 23 | Close: 20 to 21 (p10-p90 17 to 26) |
| Factory matchups | 45 to 55% | Yes: 46 to 54% on both seeds (phase 4: 54 to 57%) |
| Shuffle luck | under 0.75 | Yes: Factory 0.67 (phase 4: 0.63) |
| Good beats sloppy | 60% | No: Factory 53% (was 53%). Same gap as every other team |

## Matchups (seed 3001 / 4001)

| Matchup | Win A | Mean rounds |
| --- | --- | --- |
| Farm vs Forest | 55% / 56% | 19.6 / 19.5 |
| Farm vs Factory | 54% / 53% | 19.2 / 19.1 |
| Farm vs Dark Forest | 55% / 54% | 19.2 / 19.2 |
| Forest vs Factory | 53% / 53% | 19.6 / 19.6 |
| Forest vs Dark Forest | 51% / 49% | 19.7 / 19.6 |
| Factory vs Dark Forest | 48% / 50% | 19.3 / 19.1 |

Still open for phase 7: Farm vs Forest 55 to 56%, mean game length 19.1 to 19.7 rounds (target 20+), good beats
sloppy 53 to 57% (target 60%).

Commands: `--games 1000 --seed 3001`, `--games 1000 --seed 4001`, `--skill --games 1000 --seed 3001`,
`--set factoryGoalToys=16`, `--set factoryGoalToys=18`. The Big Order play rate came from a short script that plays
500 games per matchup and counts games where the Factory played one.
