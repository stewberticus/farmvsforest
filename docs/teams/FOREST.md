# Forest

The Forest grows. It wins by having **30 tree cards in one connected group**, and its whole kit is about getting
more trees down than one card a turn allows.

Status: first pass (2026-11-09). The cards, the Forest rule and the direction are agreed with Alexander. The numbers are
starting values from the balance sim and still need playtesting.

## Identity

* **Spreading.** The forest spreads faster than other teams build. Forest gets extra plays, not extra effects.
* **More cards.** Forest doesn't draw extra cards (with one play a turn, drawing only adds choice). Instead, generator
  cards add **Saplings** to your hand.
* **Acorns and Honey speed things up.** They are never the goal. Acorns pay for extra trees; Honey pays for the
  animals and big trees that make Saplings.

## The Forest rule: extra trees

After your normal card for the turn, you can play **another tree card** from your hand by paying **2 Acorns** on top
of its cost. You can do this as many times as you can pay for.

* Only cards with the `tree` tag. Animals and hives still use your one normal play.
* Config: `MatchConfig.forestExtraTreeAcorns` (0 turns the rule off). Code: `TeamRules.ExtraPlayCost`.
* Why it matters: without it Forest needs 29 turns at the very least. In the sim, turning it off moves Forest's median win
  from round 21 to round 37.

## Resources

| Resource | Tier | Made by | Spent on |
| --- | --- | --- | --- |
| Acorns | 1 | Squirrel Drey, Oak, Dormouse Den | Extra trees, Oak, Pine, Blossom Hive, Weeping Willow |
| Honey | 2 | Wild Bees' Nest, Blossom Hive | Dormouse Den, Blue Jay's Nest, Weeping Willow |

## Cards

| Card | Cost | Tree | Text | Flavor | Template |
| --- | --- | --- | --- | --- | --- |
| Huge Tree (start) | - | yes | Does nothing. | Your forest's first tree. | Goal piece |
| Sapling | free | yes | Does nothing. | A little tree. | Goal piece |
| Squirrel Drey | free | | Every day: gain 1 Acorn. | Squirrels gather acorns. | Sprout |
| Wild Bees' Nest | free | | Every 2 days: gain 1 Honey. | Wild bees make a little honey. | Slow burn |
| Oak | 2 Acorns | yes | Every day: gain 1 Acorn. | Oaks drop acorns. | Goal piece + producer |
| Blossom Hive | 3 Acorns | | Every day: gain 1 Honey for each tree touching this. | Bees visit the blossoms. | Neighbour |
| Dormouse Den | 3 Honey | | Every night: gain 2 Acorns. | Dormice gather nuts at night. | Upgrade |
| Pine | 2 Acorns | yes | Every 2 nights: add a Sapling to your hand. | Pinecones drop and sprout. | Goal piece + generator |
| Blue Jay's Nest | 2 Honey | | Every 2 days: add a Sapling to your hand. | Jays bury acorns and forget them. | Generator |
| Weeping Willow | 4 Acorns + 2 Honey | yes | Every day: add a Sapling to your hand. | Willow twigs take root wherever they fall. | Build-around |

Starter deck (30): Sapling ×10, Squirrel Drey ×4, Wild Bees' Nest ×3, Oak ×3, Pine ×3, Blossom Hive ×2,
Dormouse Den ×2, Blue Jay's Nest ×2, Weeping Willow ×1. That's 17 trees in the deck, and the rest come from the generators.

Data: `Assets/_Project/Scripts/Core/Content/ForestCards.cs`. In Unity, **Farm vs Forest > Generate Placeholder Card
Assets** turns these into editable card assets and a Forest Starter deck.

## Day and night

* **Day:** Squirrels and Oaks make Acorns, bees make Honey, Blue Jays and Willows bring Saplings. You spend it all on
  your turn.
* **Night:** Dormice bring Acorns and Pines drop Saplings, so you wake up with more to play.

## Space

* Forest wants **one big connected blob**, so it grows out from the corner as a solid shape.
* The tension is that **non-tree cards don't count** (Squirrel Drey, hives, nests, dens). Each takes a square, and a
  badly placed one can cut the forest in two. Blossom Hive wants to be surrounded by trees, which makes it a hole in
  the middle of your forest rather than a wall.
* Extra plays let Forest grab space quickly. That's its main way to pressure an opponent.

## Balance (see [BALANCE.md](../BALANCE.md))

Report: [balance-reports/2026-11-09-forest-v1.md](../balance-reports/2026-11-09-forest-v1.md).

* **Goldfish:** median win at **round 21** (p10 to p90: 18 to 24), with a target of about 20. With a 28-tree goal the
  median is 20; with extra trees at 1 Acorn it's also 20.
* **Off-curve cards, deliberate:**
  * *Sapling, Huge Tree* (weak). They're pure goal pieces, and their value is the tree itself.
  * *Pine, Blue Jay's Nest, Weeping Willow* (Pine and Willow show as strong). The model prices "add a card to hand" at 3 V,
    but a Sapling added that way still needs a play or 2 Acorns to get on the board. So they are weaker than they look.
    The sim's goldfish speed is the better check.
  * *Blossom Hive* (strong). The model expects 1.2 touching trees, but in a forest it's more. It's watched, not
    changed: its Honey only buys Saplings indirectly.
* Matchups aren't meaningful yet because the other teams still use placeholder cards.

## Open

* Saplings growing into bigger trees (not for now).
* Whether 30 trees still fits once the 1v1 board size is settled. The balance doc suggests about 8x8 at T = 20, and
  30 trees can't fit in half of an 8x8 board.
* Night-only cards: Forest has two night cards (Dormouse Den, Pine). It may want one that's different by day and by night.
