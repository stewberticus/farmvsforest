# Farm

> **Cross-team pass (2026-11-09):** the goal is now **64 of each** (62 before phase 1 of the 2026-11-11 plan) so that games average 20+ rounds and no team wins more than 55% of any 1v1 matchup. See [BALANCE.md](../BALANCE.md#9-cross-team-pass-2026-11-09).

The Farm is a tidy homestead. It wins by having **40 Wheat, 40 Milk and 40 Eggs** at the same time, and it gets
there by laying out fields and animals well.

Status: first pass (2026-11-09). The cards and direction are agreed with Alexander. The numbers are starting values
from the balance sim and still need playtesting. The goal number (40) is the sim's suggestion and is waiting on
Alexander's OK.

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
| Cow | 3 Wheat | animal | Every day: gain 1 Milk for each touching crop. | She's happiest grazing by the fields. | Neighbour |
| Chicken Coop | 3 Wheat | animal | Every night: gain 1 Egg for each touching crop. | Hens peck spilled grain and leave eggs by morning. | Neighbour |
| Barn Cat | 1 Milk | animal | Every night: gain 1 Wheat for each touching crop. | A saucer of milk, and she keeps the mice out of the fields. | Neighbour |
| Big Red Barn | 3 Milk | building | Every 2 days: add a Cow to your hand. | Calves are born in the barn. | Generator |
| Nesting Box | 3 Eggs | building | Every 2 days: add a Chicken Coop to your hand. | Eggs hatch into chicks, and chicks need a coop. | Generator |
| Farmhand's Cottage | 1 Milk + 1 Egg | building | Every 2 days: add a Wheat Field to your hand. | Feed a farmhand a good breakfast and they'll plant a new field. | Generator |

The generators skip their turn if you already hold a copy of the card they make (`skipIfInHand`), so they never
clog your hand.

Starter deck (30): Wheat Field ×10, Cow ×4, Chicken Coop ×4, Scarecrow ×3, Barn Cat ×3, Big Red Barn ×2,
Nesting Box ×2, Farmhand's Cottage ×2.

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
| Cow, Chicken Coop | +66% | The model assumes 1.2 touching crops; a good farm averages more. That's the team's hook, and the goal number is tuned around it. |
| Big Red Barn, Nesting Box | +229% | Priced as "a Cow (or Coop) every 2 days", and those cards are above par themselves. In play they're used about once a game, and the goal is tuned with them in. |
| Farm | -100% | Starting cards do nothing (decided). |

### Known issues

* The bot rarely plays **Barn Cat** (0.1 a game) or **Farmhand's Cottage** (0.2 a game), because it hates spending
  Milk and Eggs it needs for the goal. People may value them differently, so check in playtests before changing them.
* About 1% of goldfish games stall. Worth a look once the bots are smarter.

## Open questions

* Goal: 40 of each (recommended), or keep 20 and slow the animals down?
* Playtest whether people find the Milk and Egg buildings worth their cost.
