# Farm vs Forest: Game Design

A cozy, whimsical, family-friendly card-placement game. Players grow their team out from a corner of a shared board,
competing for space and resources. Inspired by Carcassonne: there is **no damage or combat**.

## Decided

| Topic | Decision |
| --- | --- |
| Tone | Cozy, cute, child/family friendly. Dark Forest is cute-spooky, never scary. |
| Combat | None. Teams mostly ignore each other; the main interaction is competing for space. |
| Board | Starting point: 12x12 for 2 players, 16x16 for 4. Will be tuned during design. |
| Start | Each player begins with one starting card in their corner. |
| Movement | Cards never move once played. |
| Placement | A new card must touch one of your own cards, edge to edge (no diagonals). |
| Ownership | A square with your card on it is yours. |
| Turn | Draw from your deck; play one card per turn if you can pay its cost. |
| Modes | Balanced around **1v1**. 2v2 and 1v1v1v1 are fun extra modes. |
| Win | Each team has its **own goal** (below). If the board fills first, whoever owns the most squares wins. |

## Day and night

Each round is one **day** followed by one **night**.

* Farm, Forest and Factory take their turns during the **day**.
* Dark Forest takes its turn at **night**.
* With no Dark Forest player (e.g. Farm vs Forest), everyone plays during the day and the night only fires night effects.
* Day and night matters to **every team**: each team has cards that work in the day, at night, or differently in each. Effects trigger the same way whoever is playing.
* Card effects can trigger:
  * every day, or every night,
  * every X days or every X nights (repeating),
  * once, after X days or X nights (a delayed effect).

## Teams

| Team | Starting card | Resources |
| --- | --- | --- |
| Farm | Farm | Wheat, Milk, Eggs |
| Forest | Huge Tree | Acorns, Honey |
| Factory | Headquarters | Parts, Goods |
| Dark Forest | Evil Tree | Bats, Spiders, Ghouls |

## Team goals

| Team | Goal |
| --- | --- |
| Farm | Have 20 Wheat, 20 Milk and 20 Eggs. |
| Forest | Have 30 Tree cards all touching (one connected forest). |
| Factory | Make 10 Goods in a single turn (starting number). |
| Dark Forest | **Ghoul Party**: have 13 Ghouls at once. How Ghouls are made will be decided when Dark Forest is designed (no combining). |

## Team designs

* **Forest**: [teams/FOREST.md](teams/FOREST.md). Spreads by playing extra trees: after its normal card, Forest can pay
  2 Acorns to play another tree card. Generator cards add Saplings to its hand.

## Open questions

* How each team plays differently, including its day/night rhythm (designed team by team).
* The card list for each team.
* Tuning every goal number in playtests.
