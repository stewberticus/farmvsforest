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
| Cobweb Corner | 7 | free | Every night: gain 1 Spider. | Sprout | 10 / 10 |
| Glow Mushrooms | 4 | free | Every night: gain 1 Bat for each touching Glow Mushrooms. | Neighbour | 12 / 10 |
| Sleepy Web | 3 | 3 Bats | Every day: gain 2 Spiders (spun just before breakfast). | Day/night swing | 20 / 14.5 |
| Bat Belfry | 2 | 5 Spiders | Every night: gain 1 Bat for each of your cards touching it. | Neighbour | 20 / 17.5 |
| Picnic Blanket | 2 | 2 Bats | Every day: 2 of your Ghouls eat for free. | Helper | 12 / 13 |
| Giggling Graveyard (Haunt) | 2 | 2 Bats, 2 Spiders | Every 4 nights: gain 1 Ghoul. | Slow burn | 7.5 / 16 |
| Haunted Lantern (Haunt) | 1 | 4 Bats, 3 Spiders | Every 2 nights: gain 1 Ghoul. | Upgrade | 15 / 20.5 |
| Full Moon Shrine | 2 | 4 Spiders | Every 3 nights: gain 1 Ghoul for each touching Graveyard or Lantern. | Neighbour | 12 / 16 |

### Off-curve cards and why

* **All the Ghoul makers price as weak.** That is on purpose. Alexander wants the goal kept at 13 Ghouls and games
  lasting 20 rounds or more, so Dark Forest is slowed through its Haunts, not its goal. Plays limit Dark Forest more
  than Bats and Spiders do, since those pile up, so the pace is set by how many Ghouls each play makes. Two Graveyards
  (every 4 nights) and one Lantern (every 2 nights) give a goldfish median of round 22. A Lantern every night brings it
  down to 19 or 20, and no Lantern at all pushes it to 23 or 24.
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

Target (Alexander, 2026-11-09): goal stays at 13 Ghouls, games last 20 or more rounds, and no team wins more than 55%.
The balance thread aims every team at a solo (goldfish) median of round 22, so 1v1 games end around round 20.

* **Goldfish: median round 22** (p10 to p90: 17 to 28), on the 10x10 board with a free card in the opening hand.
  Stable across seeds (800 games each).
* Matchups are settled in the cross-team balance pass, once Farm, Forest and Factory are tuned to the same
  round-22 target.
* **The feeding rule rarely bites in the sim**: about 0.03 Ghouls go home per game, because Bats and Spiders pile up
  (one play a turn leaves little to spend them on). Eating 2 or 3 each makes it bite more (1.6 or 3 Ghouls lost per
  game) without changing the pace much. To decide with Alexander.

## Open

* Should the feeding rule bite harder (more food per Ghoul, or Bats and Spiders that don't pile up)?
* Goal number 13 is decided (Alexander); the pace is tuned with cards instead.
