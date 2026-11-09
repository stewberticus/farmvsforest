# Factory: the Toy Workshop

> **Cross-team pass (2026-11-09):** the goal is now **12 Toys in one round** so that every 1v1 matchup is 40 to 60%. See [BALANCE.md](../BALANCE.md#9-cross-team-pass-2026-11-09).

A cozy little toy workshop. Tinkerers gather Parts (mostly by day), Assemblers turn Parts into Toys, and the
Factory wins with one big round of toymaking. Code: `Assets/_Project/Scripts/Core/Content/FactoryCards.cs`.

Agreed with Alexander on 2026-11-09:

* Theme: toy workshop. The goal resource is **Toys** (renamed from Goods); Parts stay.
* Goal: make **10 Toys in one round** (a day and its night). Toys from day cards, night cards and cards played that
  round all count; Toys saved up from earlier rounds don't.
* Headquarters, the starting corner card, does nothing.
* No "draw a card" cards. Cards that make more cards add a specific card to your hand instead.

## Resources

| Resource | Tier | Notes |
| --- | --- | --- |
| Parts | 1 (1 V) | Made by free and cheap cards. Pays for everything. |
| Toys | 3 (3 V) | Only made by Assemblers and Wrapping Stations. Only counts toward the goal; no card costs Toys yet. |

## Cards

| Card | Cost | Text | Template | Copies |
| --- | --- | --- | --- | --- |
| Headquarters | start | No effect. Where the workshop begins. | Start | 1 (corner) |
| Scrap Pile | free | Every day: gain 1 Part. | Sprout | 9 |
| Spare Parts Bin | free | When played: gain 2 Parts. | Helper | 3 |
| Tinker Shed | 2 Parts | Every day: gain 1 Part, plus 1 if it touches a Scrap Pile. | Neighbour | 3 |
| Moonlight Robot | 2 Parts | Every night: gain 2 Parts. | Day/night swing | 2 |
| Toy Assembler | 3 Parts | Every day: turn 2 Parts into 1 Toy. | Converter | 3 |
| Night Shift Assembler | 3 Parts | Every night: turn 2 Parts into 1 Toy. | Converter | 3 |
| Wrapping Station | 3 Parts | Every night: gain 1 Toy for each Assembler it touches. | Neighbour | 4 |
| Blueprint Desk | 2 Parts | Every 2 days: add a Toy Assembler to your hand. | Helper | 1 |
| Robot Charging Dock | 3 Parts | Every 2 nights: add a Moonlight Robot to your hand. | Helper | 1 |
| Big Whistle | 5 Parts | When played: every Assembler you own runs once right now. | Finisher | 1 |

30 cards in the deck.

## How it plays

* **Early (rounds 1 to 6):** free Scrap Piles and Spare Parts Bins, then Tinker Sheds next to Scrap Piles.
* **Middle:** Moonlight Robots for night Parts, Assemblers, and Wrapping Stations tucked between Assemblers.
* **Finish:** save Parts for a round, then the Big Whistle runs every Assembler an extra time to tip past 10 Toys.

**Day and night:** Parts come mostly by day (Scrap Pile, Tinker Shed) and Toys mostly at night (Night Shift
Assembler, Wrapping Station). Moonlight Robot only works at night.

**Space:** the Factory wants a compact cluster. No card needs more than 1 or 2 touching neighbours, since the corner
only has 2 open squares and you place one card a turn. Tinker Shed wants a Scrap Pile; Wrapping Station wants 1 or 2
Assemblers (an L shape works).

## Balance (goldfish sim, T = 20)

Run with the simulator from docs/BALANCE.md, Factory alone on 12x12, greedy bot, 600 games:

| Version | Reached goal | Median round | p10 to p90 | Pass turns |
| --- | --- | --- | --- | --- |
| First draft (7 Scrap, 2 Whistles, Bin costs 1) | 86% | 19 | 15 to 23 | 36% |
| **Current deck** | **98%** | **20** | **15 to 24** | **8%** |

The first draft passed too often because only 7 of 30 cards were free. Making Spare Parts Bin free and adding Scrap
Piles fixed that. Extra Whistles didn't help the bot, while a fourth Wrapping Station did.

### Off-curve cards (deliberate)

The value model prices cards one at a time, but the Factory's cards work in pairs, so several cards sit far from par
on purpose:

* **Assemblers (−45%)** look weak alone because they spend Parts. Their real value is enabling Wrapping Stations and
  the Big Whistle, and Toys only matter as a rate within one round.
* **Wrapping Station (+148%)** looks strong because the model assumes 1.2 touching Assemblers every night. It does
  nothing without Assemblers.
* **Tinker Shed and Moonlight Robot (+54 to +69%)** feed the Parts that the Assemblers need.
* **Spare Parts Bin (−80%)** is a free play so the Factory isn't stuck with nothing to do.
* **Blueprint Desk, Charging Dock and Big Whistle** are priced by hand (the model can't price them yet).

### Still open

* Should the Factory be able to spend Toys on cards? The goal counts Toys made, not held, so spending wouldn't slow it.
* Matchup tuning once the other teams' designed decks exist.
