# Farm vs Forest: Game Design

A cozy, whimsical, family-friendly card-placement game. Players grow their team out from a corner of a shared board,
competing for space. Inspired by Carcassonne: there is **no damage or combat**, and cards never move once played.

This page describes the game as it is on main (2026-11-11, after the
[implementation plan](https://claude.ai/artifact/HiebN1HdedieXB72aDCvpq)). Balance numbers and how they were found are
in [BALANCE.md](BALANCE.md). Each team has its own page under [teams/](teams/).

## Rules

| Topic | Rule |
| --- | --- |
| Tone | Cozy, cute, child and family friendly. Dark Forest is cute-spooky, never scary. Every card has a little flavour. |
| Board | 9x9 for 1v1 (Alexander, 2026-11-11). 16x16 for 4 players, still to tune. |
| Start | Each player begins with their starting card in their corner. It makes 1 basic resource every day (Evil Tree: every night). |
| Sunny Glades | 4 shared squares near the middle of the 1v1 board. Nobody can build on them. A card touching one makes 1 extra of what it makes, once a round. They count as sunlight for Forest trees. No glades on the 4-player board yet. |
| Turn | Draw a card (nobody draws on their own first turn), then play one card if you can pay its cost. |
| Mulligan | Once, at the start of your first turn before doing anything, you may shuffle your opening hand away and draw a new one. |
| Placement | A new card goes on an empty square touching one of your own cards, edge to edge (no diagonals). |
| Upgrades | An upgrade card goes onto one of your own cards instead of an empty square. The card underneath keeps its square and gains the upgrade's effect. One upgrade per card. Today only the Farm has upgrades. |
| Scout | Some cards say "look at the top 3 cards of your deck, keep 1 and put the rest on the bottom". Each team has 2 or 3 of them. |
| Ownership | A square with your card on it is yours. |
| Interaction | No damage, no destroying, no moving cards. Teams compete for space and for the Sunny Glades. |
| Modes | Balanced around **1v1**. 2v2 and 1v1v1v1 are fun extra modes. |
| Win | Each team has its **own goal** (below). If the board fills first, whoever owns the most squares wins. |

### Day and night

Each round is one **day** followed by one **night**. Farm, Forest and Factory take their turns during the day, and
Dark Forest takes its turn at night. With no Dark Forest player, everyone plays during the day and the night only fires
night effects.

**For the day teams, day and night are flavour** (Alexander, 2026-11-11). A card that works "every night" is worth the
same as one that works "every day": both fire once a round, before your next turn. There is no rule that makes a
day team care which it is. For Dark Forest it's more than flavour, because it plays at night and its Ghouls eat every
morning.

Card effects can trigger every day or night, every X days or nights, or once after X days (a countdown, like
Factory's Big Order).

## Teams and goals

| Team | Starting card | Resources | Goal |
| --- | --- | --- | --- |
| Farm | Farm (every day: 1 Wheat) | Wheat, Milk, Eggs | Have **30 Wheat, 30 Milk and 30 Eggs** at once. |
| Forest | Huge Tree (every day: 1 Acorn; holds the Compost rule) | Acorns, Honey | Grow **24 Old Trees in one connected forest**. |
| Factory | Headquarters (every day: 1 Part) | Parts, Toys | Make **18 Toys in a single round** (a day and its night). |
| Dark Forest | Evil Tree (every night: 1 Bat) | Bats, Spiders, Ghouls | Throw a **Ghoul Party: 13 Ghouls** at once. |

Alexander's balance targets: games average 20 or more rounds, no team wins more than 55% of any 1v1 matchup, decisions
matter (a good player clearly beats a careless one), and Dark Forest's goal stays at 13 Ghouls. Except the 13 Ghouls,
goal numbers are tuned with the bot sim, not designed, so change them freely when cards change.

## How each team plays

* **Farm** ([teams/FARM.md](teams/FARM.md)): land, then Wheat, then Milk and Eggs. Animals make Milk or Eggs every
  2 days for each crop touching them, so the layout matters. Big Red Barn and Nesting Box are upgrades that double an
  animal, so the best-placed animal is the one to upgrade.
* **Forest** ([teams/FOREST.md](teams/FOREST.md)): growth. Trees grow from Sapling to Young Tree to Old Tree on sunny
  mornings (touching an empty square or a Sunny Glade), so spreading out competes with leaving light. Forest can pay
  Acorns to play extra trees, compost a card for an Acorn, and spend Honey to pollinate (a tree grows a step).
* **Factory** ([teams/FACTORY.md](teams/FACTORY.md)): Tinkerers gather Parts, Assemblers turn Parts into Toys, and the
  Factory wins with one big round. A Big Order counts down 3 days and then runs every Assembler twice more, so both
  players can see the finish coming.
* **Dark Forest** ([teams/DARK_FOREST.md](teams/DARK_FOREST.md)): Bats and Spiders come out at night. On your turn you
  summon Ghouls by spending them (8 Bats and 8 Spiders, plus 1 of each for every Ghoul already at the party), and
  switched-on Haunts make it cheaper. Every morning each Ghoul eats a Bat or Spider, and on a morning you can't feed
  them all, one Ghoul goes home.

## Rules lifted (2026-11-11)

Alexander removed four earlier card rules as too strict. Cards may now combine cards or resources, draw cards, pay
off later once, and starting cards may have an effect. All four are in use: Ghoul summoning and upgrades combine, the
scout cards steer draws, Big Order pays off later, and every starting card makes a resource. No damage, no destroying
and no moving cards still stand.

## Open questions

* Playtest everything above with people. The [playtest sheet](PLAYTEST.md) lists what to watch for.
* 2v2 and 1v1v1v1: board size, glades and goals haven't been tuned.
* Should the Factory be able to spend Toys? Should other teams get upgrades?
