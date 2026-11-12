# Dark Forest

## Now (2026-11-11, after the implementation plan)

**Goal:** Throw a **Ghoul Party: 13 Ghouls** at once (decided; Dark Forest is paced by its cards).

Bats and Spiders come out at night. On your turn, summon Ghouls by spending 8 Bats and 8 Spiders, plus 1 of each for
every Ghoul already at the party. Each switched-on Haunt (a Graveyard touching another Haunt, a Lantern touching Glow
Mushrooms) takes 1 of each off, never below 2. The Full Moon Shrine brings a free Ghoul every 3 nights. Every morning
each Ghoul eats a Bat or a Spider, and on a morning you can't feed them all, one goes home, so don't summon more than
you can feed.

| Card | Copies | Cost | Text |
| --- | --- | --- | --- |
| Evil Tree (start) | - | - | Every night: gain 1 Bat. Your home in the dark woods. It looks spooky, but the bats think it's cosy. |
| Bat Roost | 6 | free | Every night: gain 1 Bat. |
| Cobweb Corner | 6 | free | Every night: gain 1 Spider. |
| Glow Mushrooms | 4 | free | Every night: gain 1 Bat for each touching Glow Mushrooms. Bats love a night light. |
| Sleepy Web | 3 | 3 Bats | Every day: gain 2 Spiders. They spin while you sleep, just in time for breakfast. |
| Bat Belfry | 2 | 5 Spiders | Every night: gain 1 Bat for each of your cards touching it. |
| Giggling Graveyard | 3 | 2 Bats + 2 Spiders | While this touches another Haunt: Ghouls cost 1 less to summon. Ghouls love a crowd. |
| Haunted Lantern | 2 | 3 Bats + 2 Spiders | While this touches Glow Mushrooms: Ghouls cost 1 less to summon. Ghouls follow the glow. |
| Full Moon Shrine | 2 | 4 Spiders | Every 3 nights: if this touches a Graveyard or Lantern, a Ghoul drops by for free. |
| Fortune-Telling Toad | 2 | 1 Bat | When played: look at the top 3 cards of your deck, keep 1 and put the rest on the bottom. Every night: gain 1 Spider. It croaks your fortune for a fly. |

30 cards in the deck. Card data: `Assets/_Project/Scripts/Core/Content/DarkForestCards.cs`.

**Sim (1,000 games, seeds 3001 and 4001):** Solo median round 21. Matchups 49 to 54%. The careful bot never loses a Ghoul to hunger; a bot that summons
whenever it can loses 0.4 a game, and the worst bot 5.

Everything below this section is how the team got here. Numbers in it are from when each part was written; where they
disagree with this section, this section is current.

---

Cute-spooky. Bats and Spiders come out at night, and you combine them into Ghouls for a party. Dark Forest plays at
night and wins by throwing a **Ghoul Party: 13 Ghouls at once**.

> **Summoning (2026-11-11, plan phase 3):** Ghouls are no longer made by Haunts. On your turn you **summon** a Ghoul by
> spending Bats and Spiders, as often as you can pay: 6 Bats and 6 Spiders, plus 1 of each for every Ghoul already at
> the party. Each Haunt that is switched on (a Graveyard touching another Haunt, a Lantern touching Glow Mushrooms)
> takes 1 Bat and 1 Spider off the price, never below 2 of each. The Full Moon Shrine still brings a free Ghoul every
> 3 nights. Food and Ghouls now come out of the same pile. Numbers: `darkForestGhoulBats`, `darkForestGhoulSpiders`,
> `darkForestGhoulsPerPriceStep`, `darkForestGhoulMinEach` in `MatchConfig`.

Agreed with Alexander on 2026-11-09. Card numbers are tuned with the balance sim ([BALANCE.md](../BALANCE.md)) and
will keep moving in playtests. Code: `Core/Content/DarkForestCards.cs` and `Core/Rules/GhoulFeeding.cs`.

## Core rule: hungry Ghouls

* Every morning, after day effects fire, **each Ghoul eats 1 Bat or 1 Spider**. They eat whatever you have
  (the game takes from your bigger pile first).
* If you can't feed them all, they eat what there is and **exactly one Ghoul goes home** that morning.
* Ghouls are summoned from Bats and Spiders (above) and are never spent on cards.

So the Ghoul Party needs two things: enough Bats and Spiders to summon Ghouls, and enough left over to keep them fed.
Haunts make the first part cheaper.

## Resources

| Resource | Tier | Role |
| --- | --- | --- |
| Bats | 1 | Night food and the main cost of Haunts. Quick, come in bursts. |
| Spiders | 1 | Steady food; some webs spin during the day. |
| Ghouls | 3 (goal) | The goal. Summoned from Bats and Spiders; eat every morning. |

## Cards

The Evil Tree starts in your corner. The starter deck is 30 cards.

| Card | Copies | Cost | Text | Template | Value / par |
| --- | --- | --- | --- | --- | --- |
| Evil Tree | start | - | Every night: gain 1 Bat. Your home in the dark woods; the bats think it's cosy. | Start | - |
| Bat Roost | 6 | free | Every night: gain 1 Bat. | Sprout | 10 / 10 |
| Cobweb Corner | 6 | free | Every night: gain 1 Spider. | Sprout | 10 / 10 |
| Glow Mushrooms | 5 | free | Every night: gain 1 Bat for each touching Glow Mushrooms. | Neighbour | 12 / 10 |
| Sleepy Web | 3 | 3 Bats | Every day: gain 2 Spiders (spun just before breakfast). | Day/night swing | 20 / 14.5 |
| Bat Belfry | 2 | 5 Spiders | Every night: gain 1 Bat for each of your cards touching it. | Neighbour | 20 / 17.5 |
| Picnic Blanket | 1 | 2 Bats | Every day: 2 of your Ghouls eat for free. | Helper | 12 / 13 |
| Giggling Graveyard (Haunt) | 3 | 2 Bats, 2 Spiders | While this touches another Haunt: Ghouls cost 1 less to summon. Ghouls love a crowd. | Booster | 12 / 16 |
| Haunted Lantern (Haunt) | 2 | 3 Bats, 2 Spiders | While this touches Glow Mushrooms: Ghouls cost 1 less to summon. Ghouls follow the glow. | Booster | 12 / 17.5 |
| Full Moon Shrine | 2 | 4 Spiders | Every 3 nights: if this touches a Graveyard or Lantern, a Ghoul drops by for free. | Neighbour | 9 / 16 |

### Off-curve cards and why

(The notes below were written when Haunts made Ghouls; since phase 3 they make Ghouls cheaper instead. The deck went
from 2 Graveyards and 1 Lantern to 3 and 2, with one fewer Bat Roost and Cobweb Corner, so the boosters show up.)

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

## Sim results after summoning (2026-11-11, 9x9 board)

[Report](../balance-reports/2026-11-11-phase2-3-board-and-ghouls.md). Solo median round 21; 1v1 45 to 55%; shuffle
luck 0.76 (was 0.93); good beats sloppy 58%; a bot that goes for the Sunny Glades beats one that ignores them 68%.
The careful bot only summons Ghouls it can feed tomorrow and loses none to hunger; the random bot loses 0.1 a game and
the worst bot (summons whatever it can) 0.8. The plan hoped the random bot would lose 2 or more: the feeding rule
still rarely bites, because the rising price, not food, is what stops you summoning.

The plan started the recipe at 3 Bats and 3 Spiders. At that price (with or without a per-turn rise) Dark Forest
finished around round 15 to 17, because its Bats and Spiders pile up fast. The price that rises with the party size is
what brings it to round 21.

## Open

* Should the feeding rule bite harder (more food per Ghoul, or Bats and Spiders that don't pile up)?
* Goal number 13 is decided (Alexander); the pace is tuned with cards instead.

## Scout card (2026-11-11, plan phase 4)

| Card | Copies | Cost | Text |
| --- | --- | --- | --- |
| Fortune-Telling Toad | 2 | 1 Bat | When played: look at the top 3 cards of your deck, keep 1 and put the rest on the bottom. Every night: gain 1 Spider. |

It replaced the Picnic Blanket (0 copies now; feeding rarely bites) and one Glow Mushrooms (5 to 4).
