# Dark Forest

> **Cross-team pass (2026-11-09):** the goal is now **13 Ghouls (kept; the cards set the pace)** so that games average 20+ rounds and no team wins more than 55% of any 1v1 matchup. See [BALANCE.md](../BALANCE.md#9-cross-team-pass-2026-11-09).

Cute-spooky. Bats and Spiders come out at night, and Ghouls drop by for a party. Dark Forest plays at night and wins
by throwing a **Ghoul Party: 13 Ghouls at once**.

Agreed with Alexander on 2026-11-09. Card numbers are tuned with the balance sim ([BALANCE.md](../BALANCE.md)) and
will keep moving in playtests. Code: `Core/Content/DarkForestCards.cs` and `Core/Rules/GhoulFeeding.cs`.

## Core rule: hungry Ghouls

* Every morning, after day effects fire, **each Ghoul eats 1 Bat or 1 Spider**. They eat whatever you have
  (the game takes from your bigger pile first).
* If you can't feed them all, they eat what there is and **exactly one Ghoul goes home** that morning.
* Ghouls are never combined from other resources and are never spent on cards. They come only from Haunt cards.
  (The "no combining" rule was lifted on 2026-11-11, so this can change; see DESIGN.md.)

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
| Glow Mushrooms | 5 | free | Every night: gain 1 Bat for each touching Glow Mushrooms. | Neighbour | 12 / 10 |
| Sleepy Web | 3 | 3 Bats | Every day: gain 2 Spiders (spun just before breakfast). | Day/night swing | 20 / 14.5 |
| Bat Belfry | 2 | 5 Spiders | Every night: gain 1 Bat for each of your cards touching it. | Neighbour | 20 / 17.5 |
| Picnic Blanket | 1 | 2 Bats | Every day: 2 of your Ghouls eat for free. | Helper | 12 / 13 |
| Giggling Graveyard (Haunt) | 2 | 2 Bats, 2 Spiders | Every 2 nights: gain 1 Ghoul for each touching Haunt. Ghouls love a crowd. | Neighbour | 6 / 16 |
| Haunted Lantern (Haunt) | 1 | 4 Bats, 3 Spiders | Every 2 nights: gain 1 Ghoul for each touching Glow Mushrooms. Ghouls follow the glow. | Neighbour | 18 / 20.5 |
| Full Moon Shrine | 2 | 4 Spiders | Every 3 nights: gain 1 Ghoul for each touching Graveyard or Lantern. | Neighbour | 12 / 16 |

### Off-curve cards and why

* **All the Ghoul makers price as weak.** That is on purpose. Alexander wants the goal kept at 13 Ghouls and games
  lasting 20 rounds or more, so Dark Forest is slowed through its Haunts, not its goal. Plays limit Dark Forest more
  than Bats and Spiders do, since those pile up, so the pace is set by how many Ghouls each play makes.
* **Every Ghoul maker depends on where you put it** (Alexander: make Dark Forest less forgiving). A Graveyard alone
  makes nothing; it needs other Haunts touching it. A Lantern needs Glow Mushrooms touching it, and the Shrine needs
  Haunts. When the Haunts made Ghouls on their own, a random player still won by round 28, and a good player beat a
  random one only 86% of the time. Now careless placement leaves you short of Ghouls (see Sim results).
* **Sleepy Web** is strong on paper, but taking it out of the deck doesn't change the pace. It is Dark Forest's
  day card.
* **Picnic Blanket** is only useful once you have Ghouls. Kept because it supports the feeding rule (see below).

## Day and night

Dark Forest plays at night, and most of its cards fire at night. Ghouls eat in the morning. Sleepy Web and
Picnic Blanket are the day cards, and both feed the breakfast.

## Space

Dark Forest is all about clusters. Build a haunted patch: Graveyards side by side, a Lantern in a ring of Glow
Mushrooms, and the Full Moon Shrine wedged between Haunts. A Bat Belfry wants to sit in the middle of your cards. Dark
Forest doesn't need the middle of the board, but it does need room to build its patch together, so getting boxed in
hurts.

## Sim results (2026-11-09)

Target (Alexander, 2026-11-09): goal stays at 13 Ghouls, games last 20 or more rounds, and no team wins more than 55%.
The balance thread aims every team at a solo (goldfish) median of round 22, so 1v1 games end around round 20.

Skill test (`--skill`, 300 games each, two seeds), on the 10x10 board with a free card in the opening hand:

| Bot | Before (Haunts made Ghouls on their own) | Now (Haunts depend on placement) |
| --- | --- | --- |
| Good | round 22 | round 22 |
| Random | round 28, always reaches the goal | round 37 to 38, reaches it in about 75% of games |
| Worst | round 34 | never |
| Good beats random | 86% | 93 to 96% |

* **1v1 against the other teams** (600 games each, two seeds): Dark Forest wins 48 to 54% against each team, and games
  last 20.1 to 20.7 rounds on average.
* **The feeding rule rarely bites in the sim**: about 0.03 Ghouls go home per game, because Bats and Spiders pile up
  (one play a turn leaves little to spend them on). Eating 2 or 3 each makes it bite more (1.6 or 3 Ghouls lost per
  game) without changing the pace much. To decide with Alexander.

## Open

* Should the feeding rule bite harder (more food per Ghoul, or Bats and Spiders that don't pile up)?
* Goal number 13 is decided (Alexander); the pace is tuned with cards instead.
