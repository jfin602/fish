# Module map

This map distinguishes current source from intended boundaries. Inspect the checkout before editing; a planned directory is not an implemented module.

## Existing development tooling

| Path | Owner/responsibility |
| --- | --- |
| `scripts/fish-phase.mjs` | Codex process orchestration, capacity retries, dirty-tree decisions, Git checkpoints/resume, and manual handoffs. |
| `scripts/fish-phase-core.mjs` | Prompt grammar, task ordering/configuration, version rules, and runner planning helpers. |
| `scripts/validate-fish-phase.mjs` | Read-only task-stack validation entry point. |
| `test/unit/` | Ported runner and prompt-grammar regression coverage; these are not gameplay tests. |
| `package.json` | Private Node tooling commands and Phase/checkpoint version metadata. |
| `docs/tasks/p0/` | Authored, unexecuted Godot/MCP bootstrap instructions. |
| `docs/tasks/p1/` | Authored, unexecuted core swimming, selected-asset, fullscreen and owner-acceptance instructions. |

## P0 intended source boundaries

| Planned path | Responsibility |
| --- | --- |
| `game/project.godot` | Engine settings, input definitions, main scene, and enabled editor addon. |
| `game/Fish.csproj`, `game/Fish.sln`, `game/global.json` | Godot .NET SDK, target framework, solution, and .NET SDK pins. |
| `game/scenes/` | Saved 3D scene composition. P0 contains a minimal foundation scene. |
| `game/scripts/` | First-party C# code in the `Fish` namespace. P0 owns only foundation behavior. |
| `game/dev/` | Isolated headless/scene-development helpers; exclude them from player exports. |
| `game/addons/godot_mcp/` | Versioned upstream editor bridge, with required notices and pinned source. |
| `tools/dev` | Repository-relative local CLI; owns import/build/check/editor/run/inspection composition. |
| `tools/local.env`, `.codex/config.toml` | Ignored, machine-local executable/bridge/client configuration. |

The bootstrap decides concrete files using the installed tools and the narrow p0 requirements. Keep the upstream editor bridge separate from simulation and do not open its network service in a player build.

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
