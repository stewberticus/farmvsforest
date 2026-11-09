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

* **Starting cards do nothing** (decided). They only mark your corner. All income comes from cards you play.
* **The corner has only 2 neighbours.** Your first few cards fight over a tiny frontier: turn 1 has 2 legal squares,
  turn 2 about 3. Cards that need neighbours (per-touching, "plant a sapling next to this") are weak early and should
  be priced and timed for mid-game. Never design a card whose effect needs empty squares around the start.
* **Every opening hand needs a free card**, or a team with nothing on the board can't play at all. The engine now
  guarantees one (`MatchConfig.openingHandHasFreeCard`, proposed rule). Decks should still be about a third free
  cards so later hands don't stall.

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
| Once, after N days (or nights): X | X × (1 − 5% per round waited), never below 50%. Avoid: see below. |
| Put a specific card in your hand ("every 2 days: a Cow card") | 3 V per card, plus however far that card is above its own par |
| Draw a random card | 2 V per card. Avoid: see below. |
| Convert A into B | (B − A) per fire × 80%, because you can't always pay |
| X per touching card (with a tag) | X × expected touching cards (2 for any card, 1.2 for a tag) per fire |
| Counts towards your own goal (Forest "tree") | +3 V |
| Takes an extra square, blocks a square, other special rules | Price by hand; a square is worth about 3 V |

**H, the horizon**, is how many rounds a typical card works before the game ends: about T / 2. At T = 20, H = 10.

### Card flow: make cards, don't draw them

You only play one card a turn, so an extra random card rarely matters. Instead, cards **make specific cards**:
a Barn puts a Cow card in your hand every 2 days (decided). Rules of thumb:

* Your hand gets one card a turn from your deck and you play one a turn. Every made card is extra, so hands grow.
  A full hand (8) loses new cards, including the draws you needed. In the sim, two "make a Workshop every 2 days"
  cards filled the Factory's hand with Workshops and it never drew the cards it needed to win.
* So: give makers `skipIfInHand` ("if you don't already have one"), keep each team to about one card made every
  2 to 3 turns, and make the made card cheap enough to play straight away.

### Avoid delayed one-shot payoffs

"Once, after 3 days: gain 4 Goods" feels bad: you wait, nothing happens, then it's a dead card on the board. Prefer
repeating effects ("every 3 days: gain 2 Goods"). The engine still supports one-shots; save them for rare, special
cards.

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

* No damage, no destroying, no moving cards, no combining cards (DESIGN.md).
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

All four team designs merged, then their goal numbers were tuned together. Reports:
[before](balance-reports/2026-11-09-cross-team-before.md), [after](balance-reports/2026-11-09-cross-team-v1.md)
(800 games per matchup, greedy bots, 10x10).

| Team | Goal before → after | Goldfish median before → after | 1v1 win rate before → after |
| --- | --- | --- | --- |
| Farm | 40 of each → **48** | 20 → 20 | 46% → 52% |
| Forest | 30 trees → **27** | 22 → 21 | 26% → 47% |
| Factory | 10 Toys in a round → **12** | 16 → 19 | 64% → 53% |
| Dark Forest | 13 Ghouls → **19** | 17 → 20 | 64% → 48% |

After: every matchup is between 43% and 57%, games end at a median of round 18 to 19, and every game ends on a goal.
Teams meet in about 30 to 90% of games, mostly around round 15. Forest meets everyone, Farm and Factory meet each
other least.

Value-model fixes made on the way:

* Card makers now count only 10% of the made card's value above its par (`madeCardExcessShare`). Big Red Barn and
  Nesting Box had been priced at +229%. In play they had no win-rate edge, so they're now on curve.
* The simulator looks up made cards from each match's own catalog, so the Factory's makers get priced.

Still open:

* ~~Factory stalls in about 6% of goldfish games~~ Fixed in Factory v2: Assemblers always leave 3 Parts and Spare
  Parts Bin trickles Parts, so the Factory can always build. Its goal went from 12 to 14 Toys to stay at round 20. See
  [teams/FACTORY.md](teams/FACTORY.md).
* **Farm vs Forest: the second seat wins 58%.** Watch it in playtests before changing anything.
* Card flags that are the model's fault, not the card's: per-touching cards (Cow, Chicken Coop, Blossom Hive,
  Wrapping Station) are priced at 1.2 touching cards, but each team's layout gets more. Their goals are tuned with
  them in, so leave them unless playtests disagree.

