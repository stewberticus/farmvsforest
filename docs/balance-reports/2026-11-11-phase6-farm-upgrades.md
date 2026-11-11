# Phase 6: Farm upgrades (2026-11-11)

Phase 6 of the [implementation plan](https://claude.ai/artifact/HiebN1HdedieXB72aDCvpq).

## What changed

* **Upgrades** (`MatchConfig.allowUpgrades`, on by default): a card with `CardDefinition.upgradeFor` set is played onto
  one of your own cards whose id or tag matches, instead of onto an empty square. The card underneath keeps its square,
  owner, tags and timers. Its definition becomes `CardDefinition.Upgraded(base, upgrade)`: the base card's effects plus
  the upgrade's, named "Cow + Big Red Barn". One upgrade per card (`CardInstance.UpgradedWith`). An upgrade takes no
  new square, so squares owned and board fill don't change. A Forest tree that grows keeps its upgrade, though no
  Forest card uses upgrades yet.
* **Farm cards:** Big Red Barn (3 Milk) and Nesting Box (3 Eggs) used to add a Cow or Coop to your hand. They are now
  upgrades for a Cow and a Chicken Coop: 1 more Milk or Egg per touching crop.
* **Slower animals:** Cow, Chicken Coop and both upgrades make Milk and Eggs every 2 days or nights
  (`FarmCards.AnimalDays`). Without this, a goal of 30 finished on round 14.
* **Goal:** Farm 66 to **27 of each**.
* **Bot:** an upgrade's square score is just what its effects make there (a Barn on the Cow with the most crops), with
  no "room to grow" or middle-of-the-board bonus.
* **Demo:** a legal target card gets a glowing ring, upgraded cards show a gold star, and the hints explain upgrades.
  The demo is Unity-only code and wasn't compiled here.

## Goal sweep (Farm solo, 400 games, seed 3001)

| Animals | Goal 25 | 30 | 35 | 40 |
| --- | --- | --- | --- | --- |
| Every day (as before) | 14 | 15 | 16 | 17 |
| Every day, at most 2 crops | 14 | 15 | 16 | 17 |
| **Every 2 days** | 20 | 21 | 22 | 23 |
| Every 2 days, at most 2 crops | 22 | 24 | 25 | 26 |
| Every 3 days | 28 | 30 | 32 | 33 |

Matchups set the final number. At 34 Farm won only 30 to 38%, because the other teams finish around round 20 to 21.
At 26 it was 53 to 55% (seed 3001), at 28 44 to 49%, and at **27** 45 to 52% on both seeds.

## Done-when check

| Check | Target | Result |
| --- | --- | --- |
| Farm's Milk and Egg cards | played 2+ times a game in total | Yes: 2.8 (Barn Cat 0.8 to 0.9, Big Red Barn 1.0, Nesting Box 1.0) |
| Farm goal | 40 or lower | Yes: 27 of each |
| Farm solo median | 21 to 23 | Yes: 21 (p10 to p90 18 to 25) |
| Farm matchups | 45 to 55% | Yes: 45 to 52% on both seeds |
| Upgrades matter | | Yes: with `allowUpgrades=false` Farm beats Forest 39% (52% with them) |
| Shuffle luck | under 0.75 | Yes: Farm 0.58 (phase 5: 0.75) |
| Good beats sloppy | 60% | No: Farm 56% (was 56%) |

## All matchups (seed 3001 / 4001)

| Matchup | Win A | Mean rounds |
| --- | --- | --- |
| Farm vs Forest | 52% / 48% | 19.7 / 19.7 |
| Farm vs Factory | 51% / 48% | 19.3 / 19.4 |
| Farm vs Dark Forest | 51% / 45% | 19.4 / 19.3 |
| Forest vs Factory | 53% / 53% | 19.6 / 19.6 |
| Forest vs Dark Forest | 51% / 49% | 19.7 / 19.6 |
| Factory vs Dark Forest | 48% / 50% | 19.3 / 19.1 |

Every matchup is now within 45 to 55% on both seeds, and Farm vs Forest (55 to 56% since phase 4) is fixed. Still open
for phase 7: games average 19.1 to 19.7 rounds (target 20+), and good beats sloppy is 53 to 57% (target 60%).

Commands: `--games 1000 --seed 3001`, `--games 1000 --seed 4001`, `--skill --games 1000 --seed 3001`,
`--only matchups --set farmGoalEach=26|27|28|30|34`, `--only matchups --teams Farm,Forest --set allowUpgrades=false`,
`--teams Farm --sweep farmGoalEach=25,30,35,40` (with `AnimalDays` and the crop cap edited by hand).
