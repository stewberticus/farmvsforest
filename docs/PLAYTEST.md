# Playtest sheet: the first five games

One page for running the first human games of Farm vs Forest. The bot sim says the teams are even and games last
about 20 rounds. Bots don't plan, block or get confused, so these games are to find out what bots can't tell us.

## Setup

* 1v1 on the 9x9 board, with the 4 Sunny Glades near the middle. Each player starts with their starting card in their
  corner and a hand of cards. Nobody draws on their own first turn.
* Remind both players of the mulligan before the first turn.
* Play these pairings first: **Farm vs Forest**, **Factory vs Dark Forest**, **Forest vs Dark Forest**,
  **Farm vs Factory**, then a rematch of whichever felt worst.
* Swap seats between games, so each person plays at least two teams.
* Before each game, read out that team's goal from the table in [DESIGN.md](DESIGN.md#teams-and-goals).

## Write down every game

| | Game 1 | Game 2 | Game 3 | Game 4 | Game 5 |
| --- | --- | --- | --- | --- | --- |
| Teams (who went first) | | | | | |
| Winner and how (goal / full board) | | | | | |
| Rounds played | | | | | |
| Round the two teams first touched | | | | | |
| Who held Sunny Glades at the end | | | | | |
| Turns where someone couldn't play anything | | | | | |
| Minutes played | | | | | |

## What to watch for

1. **Blocking.** Does anyone place a card to take a square the other player wanted? Teams meet around round 12 to 16
   in the sim. If people never touch, the board is too big. If one team gets walled into its corner, it's too small.
2. **Forest's light rule.** Can the Forest player tell, without asking, which trees will grow tomorrow (a tree grows
   only while it touches an empty square or a Sunny Glade)? Note every time someone asks.
3. **Sunny Glades.** Do people fight over them, or ignore them? The sim says only Dark Forest gains much from them.
4. **Feeling stuck.** Note any turn where a player says "I can't do anything" or passes. Also note whether anyone
   used the mulligan, and why.
5. **Seeing the finish coming.** Could the other player tell when Factory's Big Order was about to pay off, when
   Farm was close to 30 of each, or when Dark Forest was nearly at 13 Ghouls? Did they try to do anything about it?
6. **Choices.** Ask after each game: "Was there a turn where you had to think hard?" and "Was there a turn where it
   didn't matter what you did?" Write down both answers.
7. **Hungry Ghouls and upgrades.** Did the Dark Forest player lose Ghouls to hunger, and did it feel fair? Did the
   Farm player upgrade an animal, and did they know which one to pick?
8. **Cozy.** Did any card name, picture or bit of text feel scary, mean or confusing for a child?

## Afterwards

* Any team that won every game it played, or lost every one, is the first thing to look at.
* If games run well past 25 rounds, or end before 15, note which goal was close and which wasn't.
* Bring the sheet back to the project. Goal numbers can be tried in the sim straight away
  (`dotnet run -c Release --project Tools/BalanceSim -- --set farmGoalEach=32`).
