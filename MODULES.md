# Module map

This map distinguishes implemented P0 source from later gameplay boundaries. Inspect the checkout before editing; a planned gameplay directory is not an implemented module.

## Existing development tooling

| Path | Owner/responsibility |
| --- | --- |
| `scripts/fish-phase.mjs` | Codex process orchestration, capacity retries, dirty-tree decisions, Git checkpoints/resume, and manual handoffs. |
| `scripts/fish-phase-core.mjs` | Prompt grammar, task ordering/configuration, version rules, and runner planning helpers. |
| `scripts/validate-fish-phase.mjs` | Read-only task-stack validation entry point. |
| `test/unit/` | Ported runner and prompt-grammar regression coverage; these are not gameplay tests. |
| `package.json` | Private Node tooling commands and Phase/checkpoint version metadata. |
<<<<<<< Updated upstream
| `docs/tasks/p0/` | Authored, unexecuted Godot/MCP bootstrap instructions. |
| `docs/tasks/p1/` | Authored, unexecuted core swimming, selected-asset, fullscreen and owner-acceptance instructions. |
=======
| `docs/tasks/p0/` | Executed briefs, implementation/native evidence and foundation closeout. |
>>>>>>> Stashed changes

## Implemented P0 source boundaries

| Path | Responsibility |
| --- | --- |
| `game/project.godot` | Compatibility renderer, prototype input definitions, main scene, enabled MCP addon and runtime probe. |
| `game/Fish.csproj`, `game/Fish.sln`, `game/global.json` | Godot .NET SDK, target framework, solution, and .NET SDK pins. |
| `game/scenes/main.tscn` | Saved 3D habitat, primitive fish, static bounds/landmarks, sun, third-person camera and prototype help. |
| `game/scripts/Main.cs`, `FishController.cs` | Main-scene C# initialization/smoke and bounded prototype movement in the `Fish` namespace. |
| `game/dev/scene_tool.gd` | Isolated headless saved-scene inspection and disposable serialization check; development-only helper. |
| `game/addons/godot_mcp/` | Matched upstream 2026.11.04 editor bridge, runtime probe, license and UID sidecars. |
| `tools/dev` | Repository-relative local CLI; owns doctor/import/build/check/editor/run/inspection composition. |
| `tools/local.env`, `.codex/config.toml` | Ignored, machine-local executable/bridge/client configuration. |

The upstream editor bridge remains separate from simulation; its probe no-ops outside a debug session. [P0 implementation](docs/tasks/p0/qualification/implementation.md) and [native playtest](docs/tasks/p0/qualification/native-playtest.md) record the qualified scope.

## P1 planned source mapping

The [P1 implementation plan](docs/tasks/p1/implementation-plan.md#planned-source-ownership) maps proposed display/settings, input/motor, camera/animation, asset and course responsibilities onto the actual checkout at execution. These are planned boundaries, not existing gameplay modules. Restricted vendor resources need an ignored local installation path; public code and source-safe configuration remain tracked.

## Gameplay boundaries to introduce when needed

| Boundary | Owns | Does not own |
| --- | --- | --- |
| Fish/run simulation | Stable life identity, health, growth, body size, life state, simulation time. | Camera position, UI labels, narrative wording. |
| Swimming/input adapter | Player intent, movement integration, collision responses. | Independent copies of health or growth. |
| Ecology | Food/prey/predator entities, consumption, pursuit, escape and encounter outcomes. | Saving UI or final story prose. |
| Persistence | Versioned active-life data, durable terminal state, archive writes and explicit recovery. | Treating storage failure as fish death. |
| Life event journal | Bounded meaningful events with IDs, time, location/entity references and actual outcomes. | Invented achievements or frame-by-frame replay capture. |
| Story/archive | Selection of supported life events, final saved account and history entries. | Reviving a dead life or requiring a remote AI service. |
| Presentation | Third/first-person cameras, fish appearance, HUD, menus, recap/archive views. | Advancing simulation from rendering or history browsing. |

These are responsibility boundaries, not a requirement to create frameworks, autoloads, or empty classes now. Start with the smallest implementation that keeps authoritative state clear. [Technical direction](docs/technical-direction.md) and the owning system documents define invariants.
