# Dark Forest

Cute-spooky. Bats and Spiders come out at night, and Ghouls drop by for a party. Dark Forest plays at night and wins
by throwing a **Ghoul Party: 13 Ghouls at once**.

Agreed with Alexander on 2026-11-09. Card numbers are tuned with the balance sim ([BALANCE.md](../BALANCE.md)) and
will keep moving in playtests. Code: `Core/Content/DarkForestCards.cs` and `Core/Rules/GhoulFeeding.cs`.

## Core rule: hungry Ghouls

* Every morning, after day effects fire, **each Ghoul eats 1 Bat or 1 Spider**. They eat whatever you have
  (the game takes from your bigger pile first).
* If you can't feed them all, they eat what there is and **exactly one Ghoul goes home** that morning.
* Ghouls are never combined from other resources and are never spent on cards. They come only from Haunt cards.

So the Ghoul Party needs two things: Haunts to invite Ghouls, and enough Bats and Spiders to keep them fed.

## Resources

| Resource | Tier | Role |
| --- | --- | --- |
| Bats | 1 | Night food and the main cost of Haunts. Quick, come in bursts. |
| Spiders | 1 | Steady food; some webs spin during the day. |
| Ghouls | 3 (goal) | The goal. Made only by Haunts; eat every morning. |

## Cards

The Evil Tree starts in your corner. The starter deck is 30 cards.

| Card | Copies | Cost | Text | Template | Value / par |
| --- | --- | --- | --- | --- | --- |
| Evil Tree | start | - | Does nothing. Your home in the dark woods. | Start | - |
| Bat Roost | 7 | free | Every night: gain 1 Bat. | Sprout | 10 / 10 |
| Cobweb Corner | 6 | free | Every night: gain 1 Spider. | Sprout | 10 / 10 |
| Glow Mushrooms | 4 | free | Every night: gain 1 Bat for each touching Glow Mushrooms. | Neighbour | 12 / 10 |
| Sleepy Web | 3 | 3 Bats | Every day: gain 2 Spiders (spun just before breakfast). | Day/night swing | 20 / 14.5 |
| Bat Belfry | 2 | 5 Spiders | Every night: gain 1 Bat for each of your cards touching it. | Neighbour | 20 / 17.5 |
| Picnic Blanket | 2 | 2 Bats | Every day: 2 of your Ghouls eat for free. | Helper | 12 / 13 |
| Giggling Graveyard (Haunt) | 3 | 2 Bats, 2 Spiders | Every 3 nights: gain 1 Ghoul. | Slow burn | 10 / 16 |
| Haunted Lantern (Haunt) | 1 | 6 Bats, 4 Spiders | Every night: gain 1 Ghoul. | Upgrade | 30 / 25 |
| Full Moon Shrine | 2 | 4 Spiders | Every 3 nights: gain 1 Ghoul for each touching Graveyard or Lantern. | Neighbour | 12 / 16 |

### Off-curve cards and why

* **Ghoul makers (Graveyard, Shrine) price as weak, Lantern as strong.** Plays are what limit Dark Forest, not
  Bats and Spiders, which pile up. Pace is set by how many Ghouls each play makes, so the sim decides these numbers:
  three slow Graveyards and a single Lantern land the goldfish median on round 18. More Lanterns, or Graveyards every
  2 nights, bring it down to 14 to 16.
* **Sleepy Web** is strong on paper, but taking it out of the deck doesn't change the pace. It is Dark Forest's
  day card.
* **Picnic Blanket** is only useful once you have Ghouls. Kept because it supports the feeding rule (see below).

## Day and night

Dark Forest plays at night, and most of its cards fire at night. Ghouls eat in the morning. Sleepy Web and
Picnic Blanket are the day cards, and both feed the breakfast.

## Space

Dark Forest likes clusters: Glow Mushrooms next to each other, a Bat Belfry in the middle of your cards, and the
Full Moon Shrine wedged between Haunts. It doesn't need the middle of the board.

## Sim results (2026-11-09)

Report: [balance-reports/2026-11-09-dark-forest-v1.md](../balance-reports/2026-11-09-dark-forest-v1.md).

* **Goldfish: median round 18** (p10 to p90: 15 to 23), on target for T = 20 (18 to 20). Stable across seeds.
* Matchups are against the old placeholder Farm, Forest and Factory decks, so they don't mean much until those teams
  are designed.
* **The feeding rule rarely bites in the sim**: about 0.03 Ghouls go home per game, because Bats and Spiders pile up
  (one play a turn leaves little to spend them on). Eating 2 or 3 each makes it bite more (1.6 or 3 Ghouls lost per
  game) without changing the pace much. To decide with Alexander.

## Open

* Should the feeding rule bite harder (more food per Ghoul, or Bats and Spiders that don't pile up)?
* Goal number 13 is kept; the pace is tuned with cards instead.
