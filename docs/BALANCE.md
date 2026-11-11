# Farm vs Forest: Card Design and Balance

How we price cards, what numbers each team's design should hit, and how we check it. Every team (Farm, Forest,
Factory, Dark Forest) is designed against this framework so the four can be compared. The rules themselves are in
[DESIGN.md](DESIGN.md).

Balance focus is **1v1**. 2v2 and 1v1v1v1 are for fun and only need to "not be broken".

## 1. Pace: plays are the scarce thing

You play **one card per turn**, so a player gets about as many plays as there are rounds. Resources pile up; plays
don't. That single fact drives every other number here.

**Target game length (T).** The number of rounds a typical 1v1 game should last. Everything below is derived from T.
**T = 20** (Alexander, 2026-11-09): every team should win around round 20 on average.

| Derived number | Rule of thumb | At T = 20 |
| --- | --- | --- |
| Plays per player | about T | 20 |
| Cards seen per game | starting hand + one draw a turn, about T + 4 | 24 |
| Deck size | Cards past about T + 8 are rarely drawn. Keep decks small enough that the cards you design show up. | 30 or fewer |
| 1v1 board area | Big enough for each team's goal shape, small enough that teams meet around 60 to 75% of the way in | 10x10 (100), decided |
| Forest tree goal | Linear: one tree a play. With one play a turn that caps it near T; Forest's extra-tree plays let it go higher. | set by the Forest design, tuned by sim |
| Resource goals (Farm, Dark Forest) | Snowball: income grows by about one producer per turn, so totals grow with the square of the round. Set the goal so the goldfish bot (section 6) reaches it at about T - 1. | tune by sim |
| Factory goal (Toys in one round) | Needs a Toys income rate, so it is reached when enough Toys makers are out. Tune by sim like the others. | tune by sim |

Two consequences are worth calling out:

* **Linear goals and snowball goals don't line up by accident.** Forest's goal grows by at most one per turn;
  Farm and Dark Forest grow faster and faster. The goal numbers have to be tuned against T, not picked on their own.
* **Board size decides whether teams ever meet**, and it has to fit every goal. With goals tuned to round 20, the
  placeholder teams met in this share of games (and at this median round): 12x12 7 to 43% (round 16 to 18),
  10x10 44 to 75% (round 15), 9x9 64 to 88% (round 14), 8x8 82 to 95% (round 12). Bot win rates barely moved.
  Forest's goal of 30 connected trees needs about half of a 1v1 board to itself, which 8x8 (32 squares a side)
  can't really give. Alexander picked **10x10** (2026-11-09): room for 30 trees, and teams still meet in most games.

### What the starting position means

* **Starting cards make 1 basic resource a day** (phase 1 of the 2026-11-11 plan, `startingCardIncome`). This removes
  the lock-outs without the free-card rule below.
* **The corner has only 2 neighbours.** Your first few cards fight over a tiny frontier: turn 1 has 2 legal squares,
  turn 2 about 3. Cards that need neighbours (per-touching, "plant a sapling next to this") are weak early and should
  be priced and timed for mid-game. Never design a card whose effect needs empty squares around the start.
* **The free-card opening rule is off** (`openingHandHasFreeCard`, now false by default) because starting cards make
  income. Decks should still be about a third free cards so later hands don't stall.

## 2. The value model

Every card is priced in **value points (V)**. 1 V is one tier-1 resource (Wheat, Acorns, Parts, Bats, Spiders).

### Resource tiers

Each team has cheap resources, harder ones, and (for some teams) the goal resource.

| Tier | Worth | Farm | Forest | Factory | Dark Forest |
| --- | --- | --- | --- | --- | --- |
| 1: basic, from free cards | 1 V | Wheat | Acorns | Parts | Bats, Spiders |
| 2: made by cards that cost tier 1 | 2 V | Milk, Eggs | Honey | | |
| 3: the goal resource | 3 V | | | Toys | Ghouls |

These are starting values. Each team thread may retune its own tiers, and must say so in its design notes.

Guidelines:

* Cards cost their own team's resources. Tier 1 pays for tier-2 producers; tier 1 and 2 pay for goal-resource makers.
* Spending a resource your goal needs (Farm spending Wheat, Dark Forest spending Ghouls) is a real choice. Keep costs
  in the goal resource rare and obvious.
* There's no trading between teams.

### Pricing a card

A card's **value** is what it gives you over its time on the board:

| Card text | Value |
| --- | --- |
| When played: X | X |
| Every day: X, or every night: X | X × H |
| Every N days (or nights): X | X × H / N |
| Once, after N days (or nights): X | X × (1 − 5% per round waited), never below 50%. |
| Put a specific card in your hand ("every 2 days: a Cow card") | 3 V per card, plus however far that card is above its own par |
| Draw a random card | 2 V per card. See "Card flow" below. |
| Convert A into B | (B − A) per fire × 80%, because you can't always pay |
| X per touching card (with a tag) | X × expected touching cards (2 for any card, 1.2 for a tag) per fire |
| Counts towards your own goal (Forest "tree") | +3 V |
| Takes an extra square, blocks a square, other special rules | Price by hand; a square is worth about 3 V |

**H, the horizon**, is how many rounds a typical card works before the game ends: about T / 2. At T = 20, H = 10.

### Card flow: drawing and making cards

Card-draw effects are allowed (the "no card-draw effects" rule was lifted on 2026-11-11). Cards can also **make
specific cards**: a Barn puts a Cow card in your hand every 2 days. What the sim says about both:

* You play one card a turn, so extra cards only help if they give you a better choice. Drawing more without playing
  more fills the hand: with 2 draws a turn and nothing else changed, Factory finished only 78% of solo games and
  Dark Forest passed on 23% of turns. A full hand (8) loses new cards, including the draws you needed.
* So prefer draw effects that **choose** over ones that pile up: "look at the top 3, keep 1", "discard a card, then
  draw", or a draw on a card that also lets you play again.
* Give makers `skipIfInHand` ("if you don't already have one"), keep each team to about one card made every
  2 to 3 turns, and make the made card cheap enough to play straight away.

### Delayed one-shot payoffs

Delayed one-shots are allowed (the rule against them was lifted on 2026-11-11). The old worry still applies to weak
ones: "Once, after 3 days: gain 4 Goods" makes you wait and then leaves a dead card on the board. They work best when
the wait is visible to both players and the payoff is big enough to plan around.

**Par** is what a card *should* be worth for its cost:

> **Par = 10 + 1.5 × cost (in V)**

So the vanilla free card, "Every day: gain 1 tier-1 resource", is exactly on par (10). A card that costs 4 Wheat
should give about 16 V, for example 1 Milk every day plus a small bonus.

Cost buys more than 1 V per V spent because resources are plentiful and plays are not. If the sim shows expensive
cards winning more than cheap ones, lower the 1.5; if nobody plays them, raise it.

A card within **±15% of par** is on curve. Outside that band is fine when it's deliberate (a rare, swingy, or
build-around card), but the team's design notes must say why.

### Day and night

* **A day trigger and a night trigger are worth the same.** Both fire once a round, and both land before every
  player's next turn. Factory's "Toys in one round" counts both.
* What makes day and night matter is cards that **care** about time: "every night, gain 2", "by day, gain 1 per
  touching flower", "at night, this counts as a tree". Price each half separately and add them.
* Dark Forest plays at night, so for them a night effect comes just before their turn and a day effect half a round
  earlier. Same value.
* Each team should have both day and night cards (DESIGN.md), and its deck should have a reason to care about at
  least one of them.

### Early and late cards

Real value depends on when you play a card: an "every day" card played on round 2 works ~18 times, on round 18 only
twice. Par assumes the middle of the game. Ways to make that a decision rather than a trap:

* Cheap producers are best early. Expensive cards should pay off fast (bigger per fire, or a one-off when played).
* Late-game cards should give their value now (when played, or move you straight towards your goal).

## 3. Card templates

Start every card from one of these. Numbers are at T = 20 (H = 10). "Share" is a rough slice of a 30-card deck.

| Template | Shape | Example (par) | Share |
| --- | --- | --- | --- |
| **Sprout** (free producer) | Free. Every day or night: 1 tier 1. | Wheat Field: every day, 1 Wheat (10 V, par 10) | 25 to 35% |
| **Upgrade** (paid producer) | Costs tier 1. Makes tier 2, or more tier 1. | Cow, 4 Wheat: every day, 1 Milk (20 V, par 16; trim or raise cost) | 20 to 30% |
| **Slow burn** | Every N days: a bigger amount. Cheaper per play, lumpier. | Every 3 nights: 1 Ghoul (10 V, par 10 at cost 0) | 5 to 15% |
| **Converter** | Every day/night: spend A, get B. The bridge from tier 1 to the goal. | Night Shift: each night, 2 Parts → 1 Goods (0.8 V per fire, 8 V; make it free) | 5 to 15% |
| **Neighbour** | X per touching card (with a tag). Rewards layout and makes space matter. | Chicken Coop: 1 Egg per touching crop (24 V; price it near cost 9 or make it 1 Egg per 2 crops) | 10 to 20% |
| **Goal piece** | Counts towards the goal (tree), so its yield is lower. | Sapling: free tree, every 2 days 1 Acorn (5 + 3 = 8 V) | team specific |
| **Day/night swing** | Different by day and by night. | Owl: by night 2 Acorns, by day nothing (20 V, cost about 7) | 5 to 15% |
| **Card maker** | Every N days or nights: put a specific card in your hand. Use `skipIfInHand`. | Barn, 3 Wheat: every 2 days a Cow card (5 Cows × (3 + Cow's 0 V above par) = 15 V, about on par) | 5 to 10% |

Content rules that keep the model honest:

* No damage, no destroying, no moving cards (DESIGN.md). Combining cards is allowed (rule lifted 2026-11-11).
* Interaction comes from **space**: blocking, claiming the middle, cards that care about empty or edge squares.
  Cards that "ignore" an opponent are fine; cards that hurt them are not.
* Prefer one effect per card for readability (family game). Two effects at most.
* New effect kinds are added as `Custom` effects; give them a hand-set value in `BalanceSettings.customEffectValue`
  so the model can price them.

## 4. What each team's design must state

So the four teams can be compared, each team thread writes these down (in its own doc or a DESIGN.md section):

1. The team's resources and their tiers, if different from the table above.
2. Its goal number, tuned with the goldfish sim so the median win is about round 20 (`--sweep`).
3. Its deck list: each card's template, cost, value, and its delta from par (the sim's `cards` report prints this).
4. Every off-curve card and why.
5. How the team uses day and night.
6. How the team uses space (does it want the middle, edges, clusters, long lines?).

## 5. Balance targets (1v1)

| Metric | Target | Why |
| --- | --- | --- |
| Goldfish speed (team alone, no opponent) | Median win at T − 2 to T, and all four teams within 2 rounds of each other | Teams barely interfere with each other, so raw speed is the first thing to match. |
| Matchup win rate | Each 1v1 matchup 40 to 60% | No hopeless matchups. |
| Overall win rate per team | 45 to 55% | |
| First player | 45 to 55% | Seat order shouldn't decide games. |
| Game length | Median T ± 3 rounds | The pace we chose. |
| How games end | At least 80% by a goal | Board-full and round-limit endings mean goals are too slow. |
| Teams meet | First contact by about 0.6 × T in most games | Space is the interaction. |
| Pass turns | Under 10% of turns | A turn where you can't play anything is a bad turn. |
| Card usage | No card in a team's deck played in every game or in none | Every card should be a choice. |

Bot numbers are a filter, not proof. Greedy bots don't plan or block, so a matchup at 58% for bots may be 50% for
people, or 70%. Use the sim to catch big problems fast; use playtests to decide the close ones.

How many games? With 200 games, a measured 50% could really be anywhere from about 43% to 57%. Use 200 to explore and
800+ before calling a matchup settled.

## 6. Method: the tuning loop

1. **Design** cards from the templates, priced with the value model.
2. **Price check**: `--only cards`. Fix anything flagged strong or weak, or write down why it's deliberate.
3. **Goldfish**: `--teams <team> --only goldfish`. Tune the goal number (or the card curve) until the median win is
   at T − 2 to T and the deck can actually reach it (for example Forest needs enough trees in the deck).
4. **Matchups**: `--only matchups` against every other designed team. Check the flags.
5. **Change one thing at a time**, re-run, and save the report to `docs/balance-reports/<date>-<what>.md`.
6. **Playtest** with people. Bots can't tell us: is it fun, is the card text clear, are there real choices each
   turn, does anyone feel stuck, does day and night feel different?
7. If the sim and playtests disagree, trust the playtest and retune the value model so it predicts better next time.

### Running the simulator

Needs the .NET 8 SDK (CI already has it). From the repo root:

```sh
dotnet run -c Release --project Tools/BalanceSim -- --help
dotnet run -c Release --project Tools/BalanceSim -- --games 200                     # everything
dotnet run -c Release --project Tools/BalanceSim -- --teams Forest --only goldfish
dotnet run -c Release --project Tools/BalanceSim -- --only cards
dotnet run -c Release --project Tools/BalanceSim -- --set boardSize=10 --set forestGoalTrees=30
dotnet run -c Release --project Tools/BalanceSim -- --teams Farm --sweep farmGoalEach=8,10,12,14   # tune a goal
dotnet run -c Release --project Tools/BalanceSim -- --target 15 --out docs/balance-reports/2026-11-10-t15.md
```

`--set` overrides any number in `MatchConfig` (board size, goal numbers, hand size, draws per turn...).
`--target` changes T, which also changes H and the length targets.

The simulator plays the real rules engine with the decks in `PlaceholderCards` (swap `BalanceSimulator.DeckFor` to
point at a team's designed deck). The bots are in `Core/AI/Bots.cs`: `greedy` (picks the best-priced card for its
goal, grows towards the middle, keeps tagged cards together), `random`, and `pass` (used for goldfish).
The value model is `Core/Balance/CardValuation.cs`; its numbers live in `BalanceSettings`.

## 7. Baseline: the placeholder cards (2026-11-09)

Full report: [balance-reports/2026-11-09-placeholder-baseline.md](balance-reports/2026-11-09-placeholder-baseline.md).
The placeholder cards were never meant to be balanced, but they show what the tools catch:

| Team | Goldfish median | Overall 1v1 win rate | Main problem |
| --- | --- | --- | --- |
| Farm | round 16 | 69% | Cow and Chicken Coop are far above par. |
| Forest | never wins | 0% | Its 40-card deck only has 26 trees, but the goal is 30. |
| Factory | round 31 | 34% | Goods come too slowly (Night Shift is below par). |
| Dark Forest | round 12 | 97% | Haunted Lantern (1 Ghoul every night) is 70% above par. |

Also: on the 12x12 board the teams almost never touched before someone won.

### Tuning the placeholders to round 20 (2026-11-09, later)

As a dry run of the method, starting cards were made to do nothing, the worst placeholder cards were patched (Haunted
Lantern every 2 nights, makers instead of draw, no one-shots), and the goals swept until each team's goldfish median
was about 20. Those card patches weren't kept, because each team thread is replacing its cards. The real numbers come
from each team's own deck, so this is only an example:

| Team | Goal that gives a median of about round 20 | Notes |
| --- | --- | --- |
| Farm | 12 of each resource (was 20) | 3% of games stalled with no affordable card. |
| Forest | 12 trees (was 30) | Placeholder deck with one play a turn; the Forest design adds extra tree plays and keeps 30. |
| Factory | 8 Goods in a round (was 10) | Only after limiting Workshop makers; before that, a clogged hand meant it never won. |
| Dark Forest | 24 Ghouls (was 13) | Ghoul makers snowball, so the number moves fast; 6% of games stalled. |

With those goals, every 1v1 matchup ended at a median of round 18 and all four teams were between 35% and 65%.

## 8. Open questions for Alexander

* Opening hand always has a free card (now on by default in the engine).
* Deck size: the model suggests 30 or fewer, since only about 24 cards are seen per game.

## 9. Cross-team pass (2026-11-09)

All four team designs merged, then they were tuned together against Alexander's targets:

* **Dark Forest's goal stays at 13 Ghouls.** Dark Forest is paced by its cards instead (Dark Forest thread, PR #10).
* **Games last 20+ rounds on average.** A 1v1 ends when the faster team wins, so each team's solo (goldfish)
  median has to be about round 22 for the matchups to average 20.
* **No team wins more than 55% of any 1v1 matchup.**

Reports: [before](balance-reports/2026-11-09-cross-team-before.md), [first pass](balance-reports/2026-11-09-cross-team-v1.md),
[final](balance-reports/2026-11-09-cross-team-v2.md) (1,000 games per matchup, greedy bots, 10x10).

| Team | Goal before → final | Goldfish median before → final | 1v1 win rate before → final |
| --- | --- | --- | --- |
| Farm | 40 of each → **62** | 20 → 22 | 46% → 50% |
| Forest | 30 trees → **33** | 22 → 23 | 26% → 50% |
| Factory | 10 Toys in a round → **15** (Factory v2 cards) | 16 → 22 | 64% → 52% |
| Dark Forest | 13 Ghouls → **13** (slower Ghoul cards) | 17 → 22 | 64% → 48% |

Final: every matchup is 45 to 55% on two different seeds (1,000 games each), games average 20.1 to 20.9 rounds, and
every game ends on a goal. At 1,000 games a matchup's win rate is only known to about ±3%, so 55% versus 56% is noise.
Teams meet in about 40 to 95% of games, mostly around round 15 to 18.

Value-model fixes made on the way:

* Card makers now count only 10% of the made card's value above its par (`madeCardExcessShare`). Big Red Barn and
  Nesting Box had been priced at +229%. In play they had no win-rate edge, so they're now on curve.
* The simulator looks up made cards from each match's own catalog, so the Factory's makers get priced.
* The matchup table shows mean rounds, and flags a mean under 20 or a win rate outside 45 to 55%.

Still open:

* **Going second wins a little more often** (seat 1 wins 41 to 47% in most matchups, and 41% in Farm vs Forest).
  Watch it in playtests before changing any rule.
* Card flags that are the model's fault, not the card's: per-touching cards (Cow, Chicken Coop, Blossom Hive,
  Wrapping Station) are priced at 1.2 touching cards, but each team's layout gets more. Their goals are tuned with
  them in, so leave them unless playtests disagree.

## 10. Does skill matter? (2026-11-09)

`--skill` plays each team with three bots: **good** (the greedy bot), **random** (any legal card on any legal square),
and **worst** (the greedy bot's scoring turned upside down: it always picks what it thinks is the weakest play).
It reports each one's solo finish round, then plays mirror matches (same team on both sides) to see how often the good
player beats the others with identical cards. [Report](balance-reports/2026-11-09-skill.md), 400 games each.

Numbers after the Dark Forest (#15) and Forest growth goal (#16) changes:

| Team | Good | Random | Worst | Good beats random | Good beats worst |
| --- | --- | --- | --- | --- | --- |
| Farm | round 22 | round 40 | never | 97% | 99% |
| Forest | round 22 | round 25 | round 54 (19% finish) | 88% | 98% |
| Factory | round 21 | round 41.5 | round 49 | 99% | 100% |
| Dark Forest | round 22 | round 38 (76% finish) | never | 93% | 100% |

The random bot takes every extra play it can afford, the same as the good bot. (Until 2026-11-09 it played one card
a turn, so Forest's random bot skipped its Acorn extra plays and looked worse than the worst bot.)

Decisions now matter for every team: the good player beats random play in 88 to 99% of mirror matches and the worst
play in 98 to 100%. Forest is still the most forgiving (random play loses only 3 rounds), then Dark Forest, whose
Haunts now depend on placement. History: before #15 and #16, Forest random play was 1 round behind good (71% mirror)
and Dark Forest 6 rounds (86%); see the Forest growth goal in [teams/FOREST.md](teams/FOREST.md) and
[teams/DARK_FOREST.md](teams/DARK_FOREST.md).

1v1 after both changes (1,000 games per matchup, seed 3001): every matchup 45 to 55% (Forest vs Dark Forest is at
the 55% edge), games average 20.1 to 20.6 rounds, seat 1 wins 42 to 53% (Farm vs Forest is the outlier at 42%).

### Lock-outs

About 1% of solo games never finish, and a person would be stuck in them too:

* **Forest (1.3%)**: the opening hand's free card is a Sapling, which makes nothing. Every other card in hand costs
  Acorns, nothing makes Acorns, and once the hand is full new draws are lost.
* **Farm (0.8%)**: either the same hand lock, or Milk never starts. Cows come from the deck or from the Big Red Barn,
  and the Big Red Barn costs Milk.

## 11. Going first (2026-11-09)

Seat 1 won only 42% of Farm vs Forest. The cause was the old rule "the first turn of the match skips its draw": the
first player was a card behind the second all game. It also always hit a day team (Dark Forest never takes turn 1),
which quietly helped Dark Forest.

Options tried (1,000 games per matchup, seeds 3001 and 4001):

| Rule | Seat 1 wins | Matchups | Mean rounds |
| --- | --- | --- | --- |
| Old: first turn of the match skips its draw | 42 to 53% | 45 to 55% | 20.1 to 20.6 |
| Nobody skips | 51 to 53% | 42 to 58% (Dark Forest drops to 42 to 45%) | 19.8 to 20.4 |
| Nobody skips, Dark Forest starts with 5 cards | 50 to 53% | 39 to 61% (an extra card doesn't help Dark Forest) | 19.8 to 20.4 |
| **Everyone skips their own first draw** (chosen) | 49 to 55% | 45 to 55% | 20.1 to 21.0 |

`everyoneSkipsFirstDraw` (on by default): your opening hand is your first draw, so nobody draws on their own first
turn. Every player has the same number of cards by their Nth turn. Dark Forest also skips one draw, which costs it
little, so its matchups stay where they were.

## 12. Implementation plan (2026-11-11)

The [design review](https://claude.ai/artifact/1DT3H42vFZdnQWkQpEjbMV) found two problems: the game plays like
solitaire (a 16x16 board where teams never touch gives the same results as 10x10), and the shuffle decides more than
the player. The [implementation plan](https://claude.ai/artifact/HiebN1HdedieXB72aDCvpq) fixes them in phases. Every
new rule has a `MatchConfig` switch. Each phase has a report in `balance-reports/2026-11-11-phase*.md`.

New sim numbers (phase 0): **good beats sloppy** (a mirror against the greedy bot with noise 6, a realistic careless
player), **shuffle luck** (correlation of the solo finish round when the same deck orders are replayed with different
choices; 1 means the shuffle decides everything), and `--compare-board A,B` (does the opponent matter?).

| Phase | Change | Switch |
| --- | --- | --- |
| 0 | Sim tools above | - |
| 1 | Starting cards make 1 basic resource a day; free-card opening rule off; Forest 24 Old Trees, Farm 64 each | `startingCardIncome`, `openingHandHasFreeCard` |
| 2 | 9x9 board for 1v1 (8x8 in the plan) and 4 Sunny Glades | `boardSize`, `sunnyGladeBonus`, `sunnyGladeSquares` |
| 3 | Dark Forest summons Ghouls from Bats and Spiders; Haunts make it cheaper. Goals: Farm 62, Forest 23, Factory 16 | `darkForestGhoulBats`, `darkForestGhoulSpiders`, `darkForestGhoulsPerPriceStep`, `darkForestGhoulMinEach`, `darkForestGhoulStep`, `darkForestGhoulsPerTurn` |
| 4 | One mulligan; 2 scout cards per team (look at the top 3, keep 1). Farm 66 each, Forest 22 Old Trees | `mulligans`, `EffectKind.Scout`, `BalanceSettings.scoutValue` |
