# P0 prompt assessment

## Requested outcome and starting evidence

The owner requested a Fish starter repository using Space Factory's workflow/documentation style, then a Codex prompt to establish Godot and Godot MCP. The current starter supplies docs and the phase runner. Source reference: Space Factory `ce163753d8973ccd018f2344c6175c52fe4b113d`.

Confirmed product scope is recorded in [the decision register](../../decisions-and-questions.md). P0 establishes the C12 development stack and a foundation for C01/C04; it does not fulfill the later gameplay, save or history requirements.

## Decomposition

| Prompt | Coherent responsibility | Evidence |
| --- | --- | --- |
| P1 | Runnable Godot .NET 3D project and repository-relative developer CLI. | T1 actual C# build/main-scene smoke, command/path failures and disposable saved-scene round trip. |
| P2 | Matching MCP server/addon and isolated Fish-local configuration. | T1 package/addon/configuration checks, ignored local data, compatible server invocation, exact reload/handoff state. |
| P3 | Real native editor/MCP/game qualification and single closeout. | T3 project identity, scene/runtime operations, visible input/capture and P0 Gate mapping. |

P1/P2 are initial environment/bootstrap tasks: installation, import and client setup can exceed the ordinary planning-time target. Reuse existing compatible installations and preserve partial work. P3 is a dedicated manual integration task. Time targets do not justify missing evidence or unbounded reinstallation.

## Risks and controls

- **Wrong editor edition/SDK:** verify the .NET editor and SDK selected from `game/`; make reference-pin deviations deliberate and documented.
- **Copied 2D/game content:** port conventions, not Space Factory scenes, art, ship logic or qualification claims.
- **Wrong MCP peer or port collision:** isolate the local bridge and verify live project identity before mutation; preserve the other project and user-level config.
- **MCP C# overclaim:** retain the actual .NET build; file-write success is not compilation.
- **New tools unavailable until reload:** finish configuration and leave an exact handoff; qualify the live connection in P3.
- **False native confidence:** require real game input/state/capture and separate editor screenshots from gameplay output.
- **Scope creep:** use provisional geometry and movement only; defer all survival and story systems.

## Done boundary

P0 is complete only when G0.1–G0.3 have evidence. A successful P1/P2 checkpoint can permit the next task while native Gates remain unqualified. If closeout is incomplete, preserve the useful setup and report the specific remaining observation/repair.
