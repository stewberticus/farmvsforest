# Farm vs Forest

A cozy, family-friendly digital card game about growing your team across a shared board, like Carcassonne with
cards. There's no combat. Each player starts with one card in their corner, draws from their own deck, and plays one
card per turn onto an empty square touching their own cards, if they can pay its cost. Cards make resources by day
or by night.

Four themed teams, each with its own resources and way to win: **Farm**, **Forest**, **Factory** and **Dark Forest**.
Balanced for **1v1**, with **2v2** and **1v1v1v1** for fun. See [docs/DESIGN.md](docs/DESIGN.md).

## Getting started

1. Open the folder in **Unity Hub** (Add > Add project from disk) with a **Unity 6** editor.
   No `ProjectVersion.txt` is committed yet, so Hub will ask which editor to use. Commit the
   `ProjectSettings/` folder Unity creates on first open.
2. In the menu bar: **Farm vs Forest > Create Prototype Scene**, then press **Play**.
   You get a hot-seat debug board: click a card in your hand, click a cyan square, then **End Turn**.
   Each player's goal progress is shown in the side panel.
3. Optional: **Farm vs Forest > Generate Placeholder Card Assets** turns the placeholder cards into
   editable assets under `Assets/_Project/Data/`. Drag a deck asset onto a seat on the `Match` object to use it.

Tests: **Window > General > Test Runner > EditMode**, or without Unity: `dotnet test Tools/CoreTests`.

## Project layout

| Path | What's in it |
| --- | --- |
| `Assets/_Project/Scripts/Core` | The rules engine. Plain C#, no Unity references (`noEngineReferences`). |
| `Assets/_Project/Scripts/Runtime` | Unity glue: card and deck ScriptableObjects, `MatchRunner`, the debug IMGUI board. |
| `Assets/_Project/Scripts/Editor` | Editor menus for generating placeholder assets and the prototype scene. |
| `Assets/_Project/Tests/EditMode` | NUnit tests for the rules. Run in Unity or with `dotnet test`. |
| `Tools/CoreTests` | A .NET project that compiles Core + tests outside Unity (also used by CI). |
| `docs/` | Design and architecture notes. |

See [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md) for how the systems fit together and how to add content.
