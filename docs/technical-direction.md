# Technical direction

**State:** chosen development direction; no Fish Godot runtime exists in the starter. The owner asked to use Godot and Godot MCP as in Space Factory. Follow its .NET/C# development approach, adapted to this 3D game.

## Stack and first platform

| Area | Direction |
| --- | --- |
| Engine/gameplay | Godot 4 .NET edition; C# first-party gameplay in namespace `Fish`. |
| Reference pins | Godot editor and `Godot.NET.Sdk` 4.7.2, .NET SDK 10.0.403, `net10.0`, from the inspected Space Factory revision. |
| Project location | `game/`, with `Fish.csproj`, `Fish.sln`, and `global.json`. |
| Editor integration | Godot Editor MCP from `hybridindie/godot-mcp`; local Python MCP server plus the matching Godot editor addon. |
| First verification target | Owner's Linux desktop, following the reference workflow. Other desktop targets require separate evidence. |
| Initial renderer | Compatibility as a reversible bootstrap default inherited from Space Factory. It can render the simple 3D prototype; final water/lighting ambitions and renderer choice remain a later measured decision. |
| Development orchestration | Existing Node.js 24 phase runner; a thin repository-relative `tools/dev` created by p0. |

These pins are source references, not a claim that Fish has installed or run them. P0 must inspect the actual machine and preserve suitable existing installations. If the reference pins cannot be used, record the exact incompatibility and deliberate replacement coherently; never silently change the language, editor edition, SDK, or unrelated Space Factory setup. [Development setup](development.md) owns installation details and verification.

## Architecture principles

Keep authoritative fish/run/ecology state separate from input adapters, camera transforms, rendering, UI, and story prose. Use one explicit simulation clock. Future saves record simulation state, not UI snapshots; elapsed real-world time while the application is closed produces no simulated progress.

Introduce stable run and significant event IDs when their first owning systems are added. Growth, consumption, health changes, escapes, and death must have inspectable real causes. An event journal should capture meaningful occurrences, not every rendered frame. [The module map](../MODULES.md) describes intended responsibilities without requiring empty frameworks.

Use Godot scenes/resources for composition. Keep movement in appropriate physics updates and presentation independent of frame rate. A camera toggle changes viewpoint, not the fish identity, physics, health, or time. Begin with a small bounded habitat and simple native geometry for the foundation. The owner selected realistic ocean creatures and environments; [art and assets](art-and-assets.md) owns the approved source shortlist, first import set, and license/import checks. Final player species, specific ocean biome, detailed material/water treatment, world generation, and exact balance remain open. Asset selection does not change the bootstrap renderer or expand p0's implementation boundary.

## Persistence and narrative boundaries

The active living run and completed-life archive have separate lifecycles. New Life creates a new identity without clearing prior stories. A durable terminal outcome prevents normal Continue from reviving a dead fish. Recovery of a pending death/archive operation must be idempotent by run/event identity.

Use a small versioned save format with explicit validation and recovery once P3 is implemented. Avoid silently treating corrupt files as death or discarding records. Resolve atomic-write and recovery mechanics with the real storage implementation rather than creating a save framework in p0. [Survival and runs](systems/survival-and-runs.md) owns this contract.

Generate brief recountings from actual saved events. A deterministic local template/selection system is the initial working design; no remote LLM dependency, API key, voice service, or network connection is required by the confirmed feature. Preserve the saved final narrative for repeat visits. [Life stories and history](systems/life-stories-and-history.md) owns the details.

## CLI and MCP responsibilities

Use ordinary file editing and the Godot/.NET CLI for implementation, compilation, import, and repeatable headless checks. Use MCP to inspect the real editor, edit scenes when suitable, inspect runtime state, inject bounded input, and capture actual rendered output. MCP success is not a substitute for C# compilation or a player-visible observation.

The selected MCP stable line can read/write/list C# files but reports no C# build capability. Continue to use `dotnet build` through the developer CLI. Any addon/runtime probe GDScript belongs to development tooling; it does not change the C# gameplay decision.

Fish and Space Factory must have distinct project/bridge routing. Keep absolute paths, local ports, and client configuration out of Git. Verify the live peer's absolute project path before any MCP scene mutation. Preserve existing trust/approval controls and the other project's configuration.

## First implementation boundary

P0 creates a small 3D test habitat with a placeholder fish, simple input-driven movement and a third-person camera, sufficient to prove the development loop. This is a labeled foundation, not qualified swimming, feeding, or survival. Defer first-person polish, ecosystem AI, growth, saves, menus, and narratives to their roadmap Phases.

Do not copy Space Factory's 2D scenes, assets, ship/factory rules, input assumptions, or qualification results. Port the useful workflow and tooling conventions, then implement Fish's own 3D state and presentation.

## Source references

- [Space Factory development setup at inspected revision](https://github.com/jfin602/space-factory/blob/ce163753d8973ccd018f2344c6175c52fe4b113d/docs/development.md)
- [Reference C# project](https://github.com/jfin602/space-factory/blob/ce163753d8973ccd018f2344c6175c52fe4b113d/game/SpaceFactory.csproj) and [SDK pin](https://github.com/jfin602/space-factory/blob/ce163753d8973ccd018f2344c6175c52fe4b113d/game/global.json)
- [Godot Editor MCP stable release](https://github.com/hybridindie/godot-mcp/releases/tag/2026.11.04) and [C# contract tests](https://github.com/hybridindie/godot-mcp/blob/2026.11.04/tests/contract/test_csharp_phase1.py)
