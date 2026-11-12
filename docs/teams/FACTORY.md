# Factory: the Toy Workshop

## Now (2026-11-11, after the implementation plan)

**Goal:** Make **18 Toys in a single round** (a day and its night).

Gather Parts with Scrap Piles, Tinker Sheds and Moonlight Robots, then build Assemblers and tuck Wrapping Stations
between them. When the Parts pile and the machines are ready, play a Big Order: three days later every Assembler runs
three times, which is what tips the round past 18 Toys. The demo shows the countdown on the card and the forecast for
that day next to the goal.

| Card | Copies | Cost | Text |
| --- | --- | --- | --- |
| Headquarters (start) | - | - | Every day: gain 1 Part. Where the workshop begins, and the junk drawer is never empty. |
| Scrap Pile | 9 | free | Every day: gain 1 Part. |
| Tinker Shed | 3 | 2 Parts | Every day: gain 1 Part, plus 1 if it touches a Scrap Pile. |
| Moonlight Robot | 2 | 2 Parts | Every night: gain 2 Parts. |
| Toy Assembler | 3 | 3 Parts | Every day: turn 2 spare Parts into 1 Toy (always leaves you 3 Parts). |
| Night Shift Assembler | 3 | 3 Parts | Every night: turn 2 spare Parts into 1 Toy (always leaves you 3 Parts). |
| Wrapping Station | 3 | 3 Parts | Every night: gain 1 Toy for each Assembler it touches. |
| Mail-Order Catalogue | 3 | free | When played: gain 1 Part, then look at the top 3 cards of your deck, keep 1 and put the rest on the bottom. Circle the good bits! |
| Blueprint Desk | 1 | 2 Parts | Every 2 days: add a Toy Assembler to your hand. |
| Robot Charging Dock | 1 | 3 Parts | Every 2 nights: add a Moonlight Robot to your hand. |
| Big Order | 2 | 4 Parts | In 3 days: every Assembler you own runs twice that day. A whole town's worth of birthday presents, due Friday! |

30 cards in the deck. Card data: `Assets/_Project/Scripts/Core/Content/FactoryCards.cs`.

**Sim (1,000 games, seeds 3001 and 4001):** Solo median round 21. Matchups 44 to 53%. The good bot plays a Big Order in about two thirds of games, and most
Factory wins land on an Order's payoff day.

Everything below this section is how the team got here. Numbers in it are from when each part was written; where they
disagree with this section, this section is current.

---

A cozy little toy workshop. Tinkerers gather Parts (mostly by day), Assemblers turn Parts into Toys, and the
Factory wins with one big round of toymaking. Code: `Assets/_Project/Scripts/Core/Content/FactoryCards.cs`.

Agreed with Alexander on 2026-11-09:

* Theme: toy workshop. The goal resource is **Toys** (renamed from Goods); Parts stay.
* Goal: make Toys in one round (a day and its night). Started at 10; 15 after the cross-team pass; 16 on the 9x9 board with Sunny Glades, 17 with the Big Order, and **18** after the phase 7 retune (2026-11-11) (see [BALANCE.md](../BALANCE.md#9-cross-team-pass-2026-11-09)). Toys from day cards, night cards and cards played that
  round all count; Toys saved up from earlier rounds don't.
* Headquarters, the starting corner card, makes 1 Part every day (2026-11-11, phase 1).
* No "draw a card" cards. Cards that make more cards add a specific card to your hand instead.
* Both of those were rules when these cards were designed. They were lifted on 2026-11-11 (see DESIGN.md), so new
  Factory cards may draw, and Headquarters may get an effect.

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
| Spare Parts Bin | free | When played: gain 2 Parts. Every 3 days: gain 1 Part. | Helper | 3 |
| Tinker Shed | 2 Parts | Every day: gain 1 Part, plus 1 if it touches a Scrap Pile. | Neighbour | 3 |
| Moonlight Robot | 2 Parts | Every night: gain 2 Parts. | Day/night swing | 2 |
| Toy Assembler | 3 Parts | Every day: turn 2 spare Parts into 1 Toy (always leaves you 3 Parts). | Converter | 3 |
| Night Shift Assembler | 3 Parts | Every night: turn 2 spare Parts into 1 Toy (always leaves you 3 Parts). | Converter | 3 |
| Wrapping Station | 3 Parts | Every night: gain 1 Toy for each Assembler it touches. | Neighbour | 3 |
| Blueprint Desk | 2 Parts | Every 2 days: add a Toy Assembler to your hand (not if you already hold one). | Helper | 1 |
| Robot Charging Dock | 3 Parts | Every 2 nights: add a Moonlight Robot to your hand (not if you already hold one). | Helper | 1 |
| Big Order | 4 Parts | In 3 days: every Assembler you own runs twice that day. | Countdown finisher | 2 |

30 cards in the deck.

## How it plays

* **Early (rounds 1 to 6):** free Scrap Piles and Spare Parts Bins, then Tinker Sheds next to Scrap Piles.
* **Middle:** Moonlight Robots for night Parts, Assemblers, and Wrapping Stations tucked between Assemblers.
* **Finish:** play a Big Order when the machines and the Parts pile will be ready in 3 days. On that day every
  Assembler runs three times, which is what tips the round past 17 Toys. The demo shows the countdown on the card and
  the forecast for that day next to the goal, so you can plan for it (and so can the other player).

**Day and night:** Parts come mostly by day (Scrap Pile, Tinker Shed) and Toys mostly at night (Night Shift
Assembler, Wrapping Station). Moonlight Robot only works at night.

**Space:** the Factory wants a compact cluster. No card needs more than 1 or 2 touching neighbours, since the corner
only has 2 open squares and you place one card a turn. Tinker Shed wants a Scrap Pile; Wrapping Station wants 1 or 2
Assemblers (an L shape works).

## Balance (goldfish sim, T = 20)

Goldfish runs use the simulator from docs/BALANCE.md (Factory alone, greedy bot):

| Version | Board | Goal | Reached goal | Median round | Pass turns |
| --- | --- | --- | --- | --- | --- |
| First draft (7 Scrap, 2 Whistles, Bin costs 1) | 12x12 | 10 | 86% | 19 | 36% |
| v1 (merged in #6) | 12x12 | 10 | 98% | 20 | 8% |
| v1 after the balance update and cross-team pass | 10x10 | 12 | 94% | 18 | 14% |
| **v2: Assemblers keep 3 Parts, Bin trickles Parts** | 10x10 | **14** | **100%** | **20** | **1%** |

**Why v2:** in about 6% of games the Factory got stuck forever. Assemblers ate every Part at dawn and dusk, so the
Factory could never afford its next card, its hand filled with cards it couldn't pay for, and new draws were thrown
away. Assemblers now only use spare Parts (they always leave 3), and Spare Parts Bin keeps giving a Part every 3 days so
a Factory with no Scrap Pile still has income. Stuck games went from 11 in 300 to 0 in 400. The Factory got faster, so
the goal went up from 12 to 14 (`--sweep factoryGoalToys=10..14`: medians 15, 17, 18, 19, 20).

1v1 at goal 14 (400 games each, all four designed decks): Factory beats Farm 52%, Forest 52% and Dark Forest 48%, 50%
overall. Full report: [2026-11-09-factory-v2.md](../balance-reports/2026-11-09-factory-v2.md).

### Off-curve cards (deliberate)

The value model prices cards one at a time, but the Factory's cards work in pairs, so several cards sit far from par
on purpose:

* **Assemblers (−45%)** look weak alone because they spend Parts. Their real value is enabling Wrapping Stations and
  the Big Order, and Toys only matter as a rate within one round.
* **Wrapping Station (+148%)** looks strong because the model assumes 1.2 touching Assemblers every night. It does
  nothing without Assemblers.
* **Tinker Shed and Moonlight Robot (+54 to +69%)** feed the Parts that the Assemblers need.
* **Spare Parts Bin (−47%)** is a free play and a small trickle of Parts so the Factory never gets stuck.
* **Big Order (−36%)** replaced the Big Whistle (2026-11-11, phase 5). The Whistle ran each Assembler once, the bot
  almost never played it, and nobody could see it coming. The Order is a visible 3-day countdown: the good bot holds it
  until the forecast for the payoff day reaches the goal, then plays it. It's played in 60 to 67% of the good bot's
  games, and about two thirds of Factory wins land on an Order's payoff day.

### Still open

* Should the Factory be able to spend Toys on cards? The goal counts Toys made, not held, so spending wouldn't slow it.
* Playtest whether "always leaves you 3 Parts" reads clearly on the Assembler cards.

## Scout card (2026-11-11, plan phase 4)

| Card | Copies | Cost | Text |
| --- | --- | --- | --- |
| Mail-Order Catalogue | 3 | free | When played: gain 1 Part, then look at the top 3 cards of your deck, keep 1 and put the rest on the bottom. |

It replaced the Spare Parts Bin in the deck (played 0.4 times a game). The Bin is still defined, with 0 copies.
