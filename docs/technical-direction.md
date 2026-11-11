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

Use Godot scenes/resources for composition. Keep movement in appropriate physics updates and presentation independent of frame rate. A camera toggle changes viewpoint, not the fish identity, physics, health, or time. Begin with a small bounded habitat and simple native geometry for the foundation. The owner selected realistic ocean creatures and environments; [art and assets](art-and-assets.md) owns the approved source shortlist, first import set, and license/import checks. P2 owns the first deliberate habitat/water treatment and renderer lock. **S18 now selects P3's larger-world direction: a finite deterministic world compiler with baked outputs and curated Base Worlds.** Ocean and Pond are both selected habitats under P4; future world scale, player-facing generation UX and exact balance remain open. Asset selection alone does not change the renderer or establish runtime suitability.

## P2 worldbuilding and rendering boundary

After P1 close, [P2](planning/p2/underwater-world.md) replaced the normal-play movement laboratory with `main.tscn` → `p2_habitat.tscn` → `p2_reef_composition.tscn`. The byte-preserved P1 main lives in `p1_course_fixture.tscn` for development/qualification; its targets and posts are absent from normal play.

P2/P1 provides same-candidate Compatibility/Forward+ diagnostic paths. The owner selected **Forward+** after a smooth real-desktop run, and P2/P2 locked the saved desktop project to that method. Normal main play and subsequent P2 world/effect work use Forward+; the Compatibility command remains a bounded diagnostic. The [decision record](tasks/p2/qualification/renderer-decision.md) separates the owner's observation, the saved setting, and post-lock checks. A renderer reversal requires a concrete blocker and explicit owner decision.

The current candidate blends the four prepared seabed material sets on one visual shader surface over the continuous collidable heightfield. `UnderwaterAtmosphere.cs` installs Forward+ camera-depth distance/height/volumetric fog, top light, a moving overhead visual water mesh, drifting organic caustic projection, six world-depth-attenuated ribbon shafts, low-detail suspended GPU motes and a restrained `underwater_screen.gdshader` pass below the UI. The seabed shader attenuates color by world height. The paused settings shell offers a GFX panel with live bounded sliders and local lighting presets. `WaterReaction.cs` reads fish movement to drive capped tail-space GPU ribbons and spherical bubbles. These presentation components do not apply forces or own simulation state. The owner-green [c2 water/light closeout](tasks/c2-watet-light-fx/closeout.md) remains the base; [c2-depth P4](tasks/c2-depth/closeout.md) retains the historical failed route/visual observation, and the later [R1 native closeout](tasks/c2-depth/qualification/native-depth.md#r1-addendum--repaired-candidate-native-observation) qualifies repaired authored traversal, natural terrain, depth visibility and deep overhead cue with owner acceptance. Keep HUD/pause/settings readable outside world distortion.

## P3 deterministic world compiler boundary

P3 world formation is no longer an authored heightfield plus fixed prop-anchor problem. [Procedural world generation](planning/p3/procedural-world-generation.md) owns the selected architecture and [c3-world-gen](tasks/c3-world-gen/README.md) owns implementation.

Separate **generation-time** and **runtime** responsibilities:

- generation consumes an explicit seed/settings/generator version and the FAF catalog;
- generation creates an irregular basin, true 3D geological regions, protected route graph, environment analysis and deterministic placements;
- generation performs support/intersection/route repair and records world metrics/timings;
- generation bakes ordinary Godot mesh/collision/data resources plus a manifest;
- normal play loads baked resources and must not run the compiler;
- generated candidates stay ignored/user-side until explicitly promoted to curated Base Worlds.

The open basin may retain a cheap height/sample API, but `GetHeightAt(x,z)` is not a universal terrain contract. Cave ceilings/walls/overhangs require actual mesh projection/raycast queries and arbitrary 3D collision.

The perceived world boundary comes from generated rising terrain/geology. Fail-safe collision can exist outside/behind it but should not define ordinary navigation.

The owner target for a P3-sized compile is **≤120 seconds** on the identified current development machine. This must be measured. Baked load must be generator-independent and materially faster; record actual timing.

This is finite content compilation, not infinite streaming, voxel/destructible terrain, runtime erosion or procedural ecology.

## Persistence and narrative boundaries

The active living run and completed-life archive have separate lifecycles. New Life creates a new identity without clearing prior stories. A durable terminal outcome prevents normal Continue from reviving a dead fish. Recovery of a pending death/archive operation must be idempotent by run/event identity.

Use a small versioned save format with explicit validation and recovery once P5 persistence is implemented. Avoid silently treating corrupt files as death or discarding records. Resolve atomic-write and recovery mechanics with the real storage implementation rather than creating a save framework in p0. [Survival and runs](systems/survival-and-runs.md) owns this contract.

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

After actual P1 close and deliberate root `0.2.0` entry, the [P2 stack](tasks/p2/README.md) implemented the renderer comparison, **Forward+ lock**, natural habitat composition, underwater atmosphere and fish-reactive water presentation. The later [c2 water/light correction](tasks/c2-watet-light-fx/README.md) is owner-green. [c2-depth](tasks/c2-depth/README.md) implemented five depth bands and water-column extinction/light; P4's failed native observation is preserved, while R1 repaired the authored routes and visual concerns and passed direct owner review. G2.1/G2.2 qualify on that source; G2.3 fish-reactive water remains for P2 owner qualification. None of these P2 corrections add ecology, growth, persistence, first person or Movement abilities.

## P3 world-art boundary

After P2 is fully closed and root `0.3.0` is deliberately prepared, [P3](planning/p3/reef-world-rebuild.md) rebuilds the accepted varying-depth world's **visible** terrain and dressing with the completed FAF terrain/rock/coral/plant/detail library. It adds authored caves, tunnels, arches, geological landmarks, a shallow-lush to deep-sparse density gradient, and optional presentation-only ambient FAF reef fish while preserving P2 collision/depth/water and P1 movement/camera authority. P3 is art/world composition, not ecology; P4 adds Freshwater Pond and World Select, and feeding/predators/growth begin in P5.

## P4 multi-habitat architecture (planned, not implemented)

P4 preserves the P3 deterministic compiler, baked loader, Forward+ renderer and five marine fish while registering two independent, selectable **Ocean** and **Freshwater Pond** worlds. [P4 design](planning/p4/freshwater-world.md) owns the full acceptance contract.

- Represent each world by a stable habitat/world ID, its compiled/baked resource identity, generator/settings version, spawn environment and per-habitat water/light/fog/art profile. Never duplicate an entire code path merely to make freshwater green.
- Represent each creature with stable species ID and explicit native habitat eligibility. A single fail-closed compatibility policy must serve Character Select, normal ambient spawns and future ecology; unknown tags do not mean compatible.
- New Life selects a world before a species; P4 selection is an ephemeral tech-demo session until persistent lives are added.
- Load/unload ocean and pond without leaking cached visuals, selected fish, camera input, ambient populations or scene state. Missing resources never fall back into an incompatible habitat.
- At P6 persistence, a run must bind species ID, habitat/world ID, baked-world identity and encounter/resource state; Continue must restore that run directly, with no in-world transfer exploit.

The P4 decision changes future phase numbering but does not claim that any source implementation or qualification already exists.

## Source references

- [Space Factory development setup at inspected revision](https://github.com/jfin602/space-factory/blob/ce163753d8973ccd018f2344c6175c52fe4b113d/docs/development.md)
- [Reference C# project](https://github.com/jfin602/space-factory/blob/ce163753d8973ccd018f2344c6175c52fe4b113d/game/SpaceFactory.csproj) and [SDK pin](https://github.com/jfin602/space-factory/blob/ce163753d8973ccd018f2344c6175c52fe4b113d/game/global.json)
- [Godot Editor MCP stable release](https://github.com/hybridindie/godot-mcp/releases/tag/2026.11.04) and [C# contract tests](https://github.com/hybridindie/godot-mcp/blob/2026.11.04/tests/contract/test_csharp_phase1.py)
