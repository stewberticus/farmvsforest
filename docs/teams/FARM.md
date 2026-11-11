# Farm

> **Upgrades (2026-11-11, plan phase 6):** the goal is now **27 of each**. Animals make Milk and Eggs every 2 days, and
> Big Red Barn and Nesting Box are upgrades played onto your own Cows and Chicken Coops. See [Upgrades](#upgrades-2026-11-11-plan-phase-6).

The Farm is a tidy homestead. It wins by having **27 Wheat, 27 Milk and 27 Eggs** at the same time, and it gets
there by laying out fields and animals well, then upgrading the best-placed animals.

Status: the cards and direction are agreed with Alexander. The numbers come from the balance sim and still need
playtesting.

## Identity

* **Land → Wheat → Milk and Eggs.** Crops on your squares make Wheat. Wheat raises animals. Animals make Milk and
  Eggs. Milk and Eggs pay for buildings that bring you more cards.
* **Animals love touching crops.** Cows, hens and the barn cat make more for every crop touching them, so the farm
  grows into a neat patchwork. Where you put a card matters more than which card you play.
* **No draws, no converters, no delayed payoffs.** Farm never turns one resource into another and never "pays now
  for a big payoff later". Instead, buildings put a **specific card** into your hand every few days.
* **The Farm card makes 1 Wheat every day** (2026-11-11, phase 1). It's the corner your farm grows from.
* The rules against draws, delayed payoffs and working starting cards were lifted on 2026-11-11 (see DESIGN.md).
  These cards were designed under them; new Farm cards don't have to follow them. "No converters" is still Farm's
  own identity.

## Resources

| Resource | Tier | Made by | Spent on |
| --- | --- | --- | --- |
| Wheat | 1 | Wheat Field, Scarecrow, Barn Cat | Scarecrow, Cow, Chicken Coop |
| Milk | 2 | Cow | Barn Cat, Big Red Barn, Farmhand's Cottage |
| Eggs | 2 | Chicken Coop | Nesting Box, Farmhand's Cottage |

All three count towards the goal, so every purchase is a small "spend now or save" choice. Wheat comes easily, so
the real race is Milk and Eggs.

## Cards

| Card | Cost | Tags | Text | Flavor | Template |
| --- | --- | --- | --- | --- | --- |
| Farm (start) | - | | Every day: gain 1 Wheat. | Home sweet home, with a little vegetable patch out back. | Starting card |
| Wheat Field | free | crop | Every day: gain 1 Wheat. | The heart of the farm. | Sprout |
| Scarecrow | 1 Wheat | | Every day: gain 1 Wheat for each touching crop. | Shoos the crows away. | Neighbour |
| Cow | 3 Wheat | animal | Every 2 days: gain 1 Milk for each touching crop. | She's happiest grazing by the fields. | Neighbour |
| Chicken Coop | 3 Wheat | animal | Every 2 nights: gain 1 Egg for each touching crop. | Hens peck spilled grain and leave eggs by morning. | Neighbour |
| Barn Cat | 1 Milk | animal | Every night: gain 1 Wheat for each touching crop. | A saucer of milk, and she keeps the mice out of the fields. | Neighbour |
| Big Red Barn | 3 Milk | upgrade | Upgrade: play onto one of your Cows. Every 2 days: gain 1 more Milk for each crop touching her. | A warm barn and fresh straw make for happy cows. | Upgrade |
| Nesting Box | 3 Eggs | upgrade | Upgrade: play onto one of your Chicken Coops. Every 2 nights: gain 1 more Egg for each crop touching it. | Soft hay in every box, and the hens lay twice as well. | Upgrade |
| Farmhand's Cottage | 1 Milk + 1 Egg | building | Every 2 days: add a Wheat Field to your hand. | Feed a farmhand a good breakfast and they'll plant a new field. | Generator |

The generators skip their turn if you already hold a copy of the card they make (`skipIfInHand`), so they never
clog your hand.

Starter deck (30): Wheat Field ×10, Cow ×4, Chicken Coop ×4, Scarecrow ×3, Barn Cat ×3, Big Red Barn ×2,
Nesting Box ×2, Weathervane ×2.

Data: `Assets/_Project/Scripts/Core/Content/FarmCards.cs`. In Unity, **Farm vs Forest > Generate Placeholder Card
Assets** turns these into editable card assets and a Farm Starter deck.

## Day and night

* **Day:** fields, scarecrows and cows work. You spend Wheat on animals during your turn.
* **Night:** hens lay and the barn cat guards the fields, so you wake up with Eggs and Wheat to spend.

## Space

Farm wants **clusters**. A Wheat Field with animals on all sides feeds all of them, so the best farms are tight
checkerboards of crops and animals. Farm doesn't need the middle of the board, but it does need room: a squeezed farm
runs out of spots that touch two or three crops.

## Balance (sim, 2026-11-09)

Full report: [balance-reports/2026-11-09-farm-v1.md](../balance-reports/2026-11-09-farm-v1.md). Target T = 20.

* **Goldfish:** median win on round **20** (p10 to p90: 16 to 25), reaches the goal in 99% of games, 3% pass turns.
* **Goal number:** the design started at 20 of each. With these cards the sim reaches that on round 16, which is
  too fast. Sweep: 30 → round 18, 35 → 19, **40 → 20**, 45 → 20. The other way to hit round 20 is to keep 20 of each
  and have animals produce every 2 days instead of every day (the sim gives round 21). We picked 40 so the card text
  stays "every day".
* **Matchups:** Farm beats Forest 72% of the time, mostly because Forest's goldfish is round 22. Factory and Dark
  Forest are still placeholders, so those numbers don't mean much yet.

### Off-curve cards and why

| Card | Delta from par | Why it stays |
| --- | --- | --- |
| Cow, Chicken Coop | -17% | Every 2 days now, so the model's 1.2 touching crops prices them low. A good farm gets 2 or 3 crops around each animal. |
| Big Red Barn, Nesting Box | -37% | Priced like the animal they go on, at twice the cost. Their real value is the square they don't need: a well-placed animal gets doubled. Played about once a game each. |

### Known issues

* The bot rarely plays **Barn Cat** (0.1 a game) or **Farmhand's Cottage** (0.2 a game), because it hates spending
  Milk and Eggs it needs for the goal. People may value them differently, so check in playtests before changing them.
* About 1% of goldfish games stall. Worth a look once the bots are smarter.

## Open questions

* Playtest whether people find the upgrades worth 3 Milk or 3 Eggs, which also count towards the goal.

## Scout card (2026-11-11, plan phase 4)

| Card | Copies | Cost | Text |
| --- | --- | --- | --- |
| Weathervane | 2 | 1 Wheat | When played: look at the top 3 cards of your deck, keep 1 and put the rest on the bottom. Every 2 days: gain 1 Wheat. |

It replaced Farmhand's Cottage in the deck (played 0.3 times a game). The Cottage is still defined, with 0 copies.

## Upgrades (2026-11-11, plan phase 6)

An **upgrade** is played onto one of your own cards instead of an empty square. The card underneath keeps its square,
tags and timers, and gains the upgrade's effect. Each card takes one upgrade, and an upgrade takes no new square. The
rule has a switch (`MatchConfig.allowUpgrades`) and the card data says what it goes onto (`CardDefinition.upgradeFor`).

* **Big Red Barn** (3 Milk) goes onto a Cow and **Nesting Box** (3 Eggs) onto a Chicken Coop. Each doubles what that
  animal makes, so the best spot is the animal with the most crops around it.
* Animals now make Milk and Eggs every 2 days (`FarmCards.AnimalDays`). That's what lets the goal drop from 66 to 27
  of each without the Farm finishing sooner.
* With upgrades switched off the same goal gives Farm 39% against Forest (52% with them), so they matter.
