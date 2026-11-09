# Farm vs Forest

A cozy, family-friendly digital card game about growing your team across a shared board, like Carcassonne with
cards. There's no combat. Each player starts with one card in their corner, draws from their own deck, and plays one
card per turn onto an empty square touching their own cards, if they can pay its cost. Cards make resources by day
or by night.

Four themed teams, each with its own resources and way to win: **Farm**, **Forest**, **Factory** and **Dark Forest**.
Balanced for **1v1**, with **2v2** and **1v1v1v1** for fun. See [docs/DESIGN.md](docs/DESIGN.md).

## Getting started

1. Open the folder in **Unity Hub** (Add > Add project from disk) with **Unity 6000.6.7f1** (the version in
   `ProjectSettings/ProjectVersion.txt`). A different Unity 6 version works too, but Hub will ask to switch and
   Unity may bump package versions; commit those changes only if the whole team moves to that version.
2. Press **Play** in any scene. If the scene has no `MatchRunner` (the empty starting scene is fine), the
   **v1 demo** starts by itself. To keep a proper scene, use **Farm vs Forest > Create Demo Scene**.
   Pick a mode, a team for each seat and Human or Bot, then **Start**. Click a card in your hand, click a glowing
   square, and press **End Turn** (or Space). The side panel shows every team's resources and goal progress, the
   banner shows day or night, and a winner screen appears at the end. With two or more humans it's hot-seat.
   **Farm vs Forest > Create Prototype Scene (debug board)** gives the older bare debug board for testing rules.
3. Optional: **Farm vs Forest > Generate Placeholder Card Assets** turns the placeholder cards into
   editable assets under `Assets/_Project/Data/`. Drag a deck asset onto a seat on the `Match` object to use it.

Tests: **Window > General > Test Runner > EditMode**, or without Unity: `dotnet test Tools/CoreTests`.

Balance: `dotnet run -c Release --project Tools/BalanceSim -- --games 200` plays bot matches and reports win rates,
game length and how every card prices against the curve. How to design and tune cards is in [docs/BALANCE.md](docs/BALANCE.md).

## Project layout

| Path | What's in it |
| --- | --- |
| `Assets/_Project/Scripts/Core` | The rules engine. Plain C#, no Unity references (`noEngineReferences`). |
| `Assets/_Project/Scripts/Runtime` | Unity glue: card and deck ScriptableObjects, `MatchRunner`, the v1 demo UI (`DemoGame`) and the debug board. |
| `Assets/_Project/Scripts/Editor` | Editor menus for generating placeholder assets and the prototype scene. |
| `Assets/_Project/Tests/EditMode` | NUnit tests for the rules. Run in Unity or with `dotnet test`. |
| `Tools/CoreTests` | A .NET project that compiles Core + tests outside Unity (also used by CI). |
| `Tools/BalanceSim` | Bot-vs-bot balance simulator that prints a win-rate and card report. See [docs/BALANCE.md](docs/BALANCE.md). |
| `docs/` | Design and architecture notes. |

See [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md) for how the systems fit together and how to add content.
