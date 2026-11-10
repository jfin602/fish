# Technical direction

**State:** chosen development direction with P0's Godot .NET/C# 3D foundation implemented and [qualified on Linux/X11](tasks/p0/closeout.md). The owner asked to use Godot and Godot MCP as in Space Factory; Fish adapts that workflow to a 3D game.

## Stack and first platform

| Area | Direction |
| --- | --- |
| Engine/gameplay | Godot 4 .NET edition; C# first-party gameplay in namespace `Fish`. |
| Reference pins | Godot editor and `Godot.NET.Sdk` 4.7.2, .NET SDK 10.0.403, `net10.0`, from the inspected Space Factory revision. |
| Project location | `game/`, with `Fish.csproj`, `Fish.sln`, and `global.json`. |
| Editor integration | Godot Editor MCP from `hybridindie/godot-mcp`; local Python MCP server plus the matching Godot editor addon. |
| First verification target | Owner's Linux desktop, following the reference workflow. Other desktop targets require separate evidence. |
| Renderer | **Forward+** is the tracked P2 desktop default from `0.2.2`: `renderer/rendering_method="forward_plus"`, the same method used by `tools/dev compare forward-plus`. The owner selected it after a smooth real-desktop trial. Compatibility remains available only through the diagnostic comparison command. [Renderer decision](tasks/p2/qualification/renderer-decision.md) records the lock and evidence limits. |
| Development orchestration | Existing Node.js 24 phase runner; a thin repository-relative `tools/dev` created by p0. |
| P1 display contract | Fullscreen development from the first task, with windowed fullscreen for testing; [P1 design](planning/p1/core-swimming.md) and [UX](ux-and-presentation.md#fullscreen-presentation) own mappings, layout and acceptance. |

These pins began as source references; the installed Linux/X11 toolchain was subsequently verified in [P0 closeout](tasks/p0/closeout.md) and the P2 native record. Other platform support still needs its own evidence. [Development setup](development.md) owns installation details and verification.

## Architecture principles

Keep authoritative fish/run/ecology state separate from input adapters, camera transforms, rendering, UI, and story prose. Use one explicit simulation clock. Future saves record simulation state, not UI snapshots; elapsed real-world time while the application is closed produces no simulated progress.

Introduce stable run and significant event IDs when their first owning systems are added. Growth, consumption, health changes, escapes, and death must have inspectable real causes. An event journal should capture meaningful occurrences, not every rendered frame. [The module map](../MODULES.md) describes intended responsibilities without requiring empty frameworks.

Use Godot scenes/resources for composition. Keep movement in appropriate physics updates and presentation independent of frame rate. A camera toggle changes viewpoint, not the fish identity, physics, health, or time. Begin with a small bounded habitat and simple native geometry for the foundation. The owner selected realistic ocean creatures and environments; [art and assets](art-and-assets.md) owns the approved source shortlist, first import set, and license/import checks. P2 now owns the first deliberate habitat/water treatment and renderer lock; final player species, production biome, larger world generation, and exact balance remain open. Asset selection alone does not change the renderer or establish runtime suitability.

## P2 worldbuilding and rendering boundary

After P1 close, [P2](planning/p2/underwater-world.md) replaced the normal-play movement laboratory with `main.tscn` → `p2_habitat.tscn` → `p2_reef_composition.tscn`. The byte-preserved P1 main lives in `p1_course_fixture.tscn` for development/qualification; its targets and posts are absent from normal play.

P2/P1 provides same-candidate Compatibility/Forward+ diagnostic paths. The owner selected **Forward+** after a smooth real-desktop run, and P2/P2 locked the saved desktop project to that method. Normal main play and subsequent P2 world/effect work use Forward+; the Compatibility command remains a bounded diagnostic. The [decision record](tasks/p2/qualification/renderer-decision.md) separates the owner's observation, the saved setting, and post-lock checks. A renderer reversal requires a concrete blocker and explicit owner decision.

The current candidate blends the four prepared seabed material sets on one visual shader surface over the continuous collidable heightfield. `UnderwaterAtmosphere.cs` installs Forward+ distance/height/volumetric fog, top light, a moving overhead visual water mesh, drifting organic caustic projection, six caustic-aligned ribbon shafts, low-detail suspended GPU motes and a restrained `underwater_screen.gdshader` pass below the UI. The paused settings shell now offers a GFX panel with live bounded sliders and local lighting presets. `WaterReaction.cs` reads fish movement to drive capped tail-space GPU ribbons and spherical bubbles. These presentation components do not apply forces or own simulation state. [Correction 2 closeout](tasks/c2-watet-light-fx/closeout.md) records the still-open revised-look and performance decision; [P8 review](tasks/p2/closeout.md) remains the broader Gate checkpoint. Keep HUD/pause/settings readable outside world distortion.

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

## P1 implementation boundary

After verified P0 readiness, the [P1 stack](tasks/p1/README.md) builds the selected-asset course, shared swim motor, mouse/keyboard adapters, follow camera and fish animation, and settles the dynamics through actual owner play. Small local display/input preferences and a pause/settings surface are part of this scope; survival saves and menus remain later work. P2 planning does not permit unfinished P1 movement/camera behavior to be deferred.

## P2 implementation boundary

After actual P1 close and deliberate root `0.2.0` entry, the [P2 stack](tasks/p2/README.md) implemented the renderer comparison, **Forward+ lock**, natural habitat composition, underwater atmosphere and fish-reactive water presentation. P7 recorded native observations, but [P8 closeout](tasks/p2/closeout.md) remains BLOCKED, so P2 is not yet qualified. The stack does not add ecology, growth, persistence, first person or Movement abilities.

## Source references

- [Space Factory development setup at inspected revision](https://github.com/jfin602/space-factory/blob/ce163753d8973ccd018f2344c6175c52fe4b113d/docs/development.md)
- [Reference C# project](https://github.com/jfin602/space-factory/blob/ce163753d8973ccd018f2344c6175c52fe4b113d/game/SpaceFactory.csproj) and [SDK pin](https://github.com/jfin602/space-factory/blob/ce163753d8973ccd018f2344c6175c52fe4b113d/game/global.json)
- [Godot Editor MCP stable release](https://github.com/hybridindie/godot-mcp/releases/tag/2026.11.04) and [C# contract tests](https://github.com/hybridindie/godot-mcp/blob/2026.11.04/tests/contract/test_csharp_phase1.py)
