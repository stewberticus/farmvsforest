# Farm vs Forest

A digital card game played on a chess-like board. Players start in the corners, draw from their own deck,
and play one card per turn onto a square if they can pay its cost. Some cards are free and produce resources;
others cost resources and do something every turn or affect the squares around them.

Four themed teams: **Farm**, **Forest**, **Factory** and **Dark Forest**. Modes: **1v1**, **2v2** and **1v1v1v1**.

## Getting started

1. Open the folder in **Unity Hub** (Add > Add project from disk) with a **Unity 6** editor.
   No `ProjectVersion.txt` is committed yet, so Hub will ask which editor to use. Commit the
   `ProjectSettings/` folder Unity creates on first open.
2. In the menu bar: **Farm vs Forest > Create Prototype Scene**, then press **Play**.
   You get a hot-seat debug board: click a card in your hand, click a cyan square, then **End Turn**.
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
