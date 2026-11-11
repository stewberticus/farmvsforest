# Forest

The Forest is about **growth and life**. You plant Saplings, give them sunlight, and grow them into Old Trees.
**Goal: 23 Old Trees in one connected forest** (2026-11-11, on the 9x9 board; was 20 on 10x10). A Sunny Glade counts as sunlight.

Status: second pass (2026-11-09). Alexander chose the growth goal so that good decisions matter more. The numbers come
from the balance sim and still need playtesting.

## Growth

Every tree grows in stages, in place (cards never move):

| Stage | Becomes | After |
| --- | --- | --- |
| Sapling | Young Tree | 2 sunny mornings |
| Young Tree, Oak, Pine, Weeping Willow | Old Tree, Great Oak, Tall Pine, Grand Willow | 4 sunny mornings |

* **A sunny morning** is a day when the tree touches at least one **empty square**. A tree with every side
  filled (by your cards or an opponent's) stops growing until a side opens up, which in practice is never.
* The Huge Tree starts as an Old Tree.
* The goal counts Old Trees (Huge Tree, Old Tree, Great Oak, Tall Pine, Grand Willow) in your biggest connected group of
  trees. Saplings and Young Trees connect the forest but don't count yet.
* Code: the `forest.grow` effect in `ForestCards.cs`, `Match.Transform`, and `ConnectedTaggedCardsGoal.CountTag`. Config:
  `MatchConfig.forestGoalOldTrees` (0 switches back to the older "33 connected trees" goal).

**Why:** spreading fills the squares your trees need for light. Every play is a choice between growing the forest
outward and leaving room for trees to grow up. Random play smothers its own trees; good play leaves light gaps.

## Forest rules

* **Extra trees.** After your normal card, you can play another tree card by paying Acorns on top of its cost: 2 for
  the first extra, 3 for the next, and so on (`forestExtraTreeAcorns`, `forestExtraTreeStep`).
* **Compost.** Once a turn, discard a card from your hand to gain 1 Acorn (`forestCompostAcorns`). This also stops
  the rare game where Forest had no way to make Acorns.
* **Pollinate.** On your turn, spend **2 Honey** to make one of your growing trees grow one step right away, even if
  it's in shade (`forestPollinateHoney`). As often as you can pay. Bees carry pollen from flower to tree. In the demo,
  click a growing tree.
  *Why:* Honey had almost nothing to buy, so it piled up. Now it speeds up growth directly. You choose which
  tree to help: one that's nearly grown, one that joins your forest, or one your trees have shaded in.

## Resources

| Resource | Made by | Spent on |
| --- | --- | --- |
| Acorns | Squirrel Drey, Oak, Dormouse Den, Compost | Extra trees, Oak, Pine, Blossom Hive, Weeping Willow |
| Honey | Wildflower Meadow, Blossom Hive | **Pollinate** (2 Honey: a tree grows a step), Dormouse Den, Blue Jay's Nest, Weeping Willow |

## Cards

| Card | Cost | Tree | Text | Flavor |
| --- | --- | --- | --- | --- |
| Huge Tree (start) | - | old | Every day: gain 1 Acorn. Once a turn: discard a card to gain 1 Acorn (Compost). | Your forest's first Old Tree. |
| Sapling | free | yes | Grows into a Young Tree (2 sunny mornings). | A little tree reaching for the sun. |
| Young Tree | (grown) | yes | Grows into an Old Tree (4 sunny mornings). | |
| Oak | 2 Acorns | yes | Every day: gain 1 Acorn. Grows into a Great Oak. | Oaks drop acorns. |
| Pine | 2 Acorns | yes | Every 2 nights: add a Sapling to your hand. Grows into a Tall Pine. | Pinecones drop and sprout. |
| Weeping Willow | 4 Acorns + 2 Honey | yes | Every day: add a Sapling to your hand. Grows into a Grand Willow. | Willow twigs take root wherever they fall. |
| Squirrel Drey | free | | Every day: gain 1 Acorn, plus 1 for each Oak touching this. | Squirrels live by the oaks. |
| Wildflower Meadow | free | | Every day: gain 1 Honey for each empty square touching this. | Flowers need open, sunny ground. |
| Blossom Hive | 3 Acorns | | Every day: gain 1 Honey for each tree touching this. | Bees visit the blossoms. |
| Dormouse Den | 3 Honey | | Every night: gain 2 Acorns. | Dormice gather nuts at night. |
| Blue Jay's Nest | 2 Honey | | Every 2 days: add a Sapling to your hand. | Jays bury acorns and forget them. |

The grown forms (Great Oak, Tall Pine, Grand Willow) keep the young card's effect.

Starter deck (30): Sapling ×10, Squirrel Drey ×4, Wildflower Meadow ×3, Oak ×3, Pine ×3, Blossom Hive ×2,
Dormouse Den ×2, Blue Jay's Nest ×2, Weeping Willow ×1.

## Space: the puzzle

* **Light vs spread:** every card you place can take light from a growing tree next to it. Leave gaps.
* **Meadows want open ground, and so do your trees.** Blossom Hives want to be surrounded by trees, and Squirrels want
  Oaks. Each card pulls its square a different way.
* **Opponents shade you.** Their cards next to your trees block light too, so the edge where you meet them matters.

## Day and night

* **Day:** trees grow, Squirrels, Oaks and flowers make Acorns and Honey, and Jays and Willows bring Saplings.
* **Night:** Dormice bring Acorns and Pines drop Saplings, so you wake up with more to plant.

## Balance (see [BALANCE.md](../BALANCE.md))

Report: [balance-reports/2026-11-09-forest-growth.md](../balance-reports/2026-11-09-forest-growth.md) (800 games each).

| | Good | Random | Worst | Good beats random | Good beats worst |
| --- | --- | --- | --- | --- | --- |
| Old goal (33 trees) | round 23 | round 24 | round 28 | 71% | 82% |
| **Growth goal (16 Old Trees, before Pollinate)** | **round 22** | **round 25** | **round 54** | **86%** | **99%** |

* 1v1: Forest wins 51% overall (Farm 52%, Factory 50%, Dark Forest 51%). Games average 20+ rounds.
* Slower growth (more sunny mornings needed) widens the skill gap. 2 + 4 mornings was the best mix of skill and pace.
* The value model flags Squirrel Drey, Wildflower Meadow, Pine, Oak, Willow and Blossom Hive as strong. Its
  "per touching" and growth guesses don't capture the light trade-off; the sim's win rates are the better check.

### Pollinate update (2026-11-09)

Report: [balance-reports/2026-11-09-forest-pollinate.md](../balance-reports/2026-11-09-forest-pollinate.md).
With Pollinate the goal is **20 Old Trees**: good round 22, random 28, worst 57.5; good beats random **93%** in mirror
matches. 1v1: Forest 49% overall, and every Forest matchup is 49 to 51%.

## Open

* Playtest whether "sunny morning" is easy for kids to read on the board (maybe show a little sun icon on trees that
  will grow tonight).
* Tune with people, not just bots.
