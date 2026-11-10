# Module map

This map distinguishes implemented source from planned boundaries. Inspect the checkout before editing; a planned P2 path is not an implemented module until execution creates it.

## Existing development tooling

| Path | Owner/responsibility |
| --- | --- |
| `scripts/fish-phase.mjs` | Codex process orchestration, capacity retries, dirty-tree decisions, Git checkpoints/resume, and manual handoffs. |
| `scripts/fish-phase-core.mjs` | Prompt grammar, task ordering/configuration, version rules, and runner planning helpers. |
| `scripts/validate-fish-phase.mjs` | Read-only task-stack validation entry point. |
| `test/unit/` | Runner and prompt-grammar regression coverage; these are not gameplay tests. |
| `package.json` | Private Node tooling commands and Phase/checkpoint version metadata. |
| `docs/tasks/p0/` | Executed foundation briefs and evidence. |
| `docs/tasks/p1/` | Completed core-swimming implementation and [closeout](docs/tasks/p1/closeout.md), with accepted source tuning and evidence limits. |
| `docs/tasks/c1-bindings/` | Implemented hybrid presets/rebinding, reconciled into P1 with advanced native gaps retained. |
| `docs/tasks/p2/` | Authored, unexecuted underwater-world Phase stack; ready from root `0.2.0`. |

## Implemented Godot/P1 source boundaries

| Path | Responsibility |
| --- | --- |
| `game/project.godot` | Godot project, current Compatibility renderer, main scene, display/input defaults, enabled MCP addon/runtime probe. |
| `game/Fish.csproj`, `game/Fish.sln`, `game/global.json` | Godot .NET target and .NET SDK pins. |
| `game/scenes/main.tscn` | Current normal-play root: boxed P1 habitat, world light/environment hook, fish/camera and prototype presentation. This is the source P2 replaces as the visible world, not evidence of a finished habitat. |
| `game/scenes/movement_course.tscn` | Reproducible P1 movement/obstruction qualification fixture with markers and simple test geometry. Preserve it or an equivalent dedicated fixture when normal play moves to the P2 habitat. |
| `game/scripts/Main.cs` | Main-scene initialization, shell setup and smoke integration. |
| `game/scripts/StarterWorld.cs` | P1 local asset readiness, Fish3 visual, sand/rock/coral placement and stationary ambient fish. P2 should split/retire its world-dressing responsibility deliberately rather than growing it into a monolith. |
| `game/scripts/FishController.cs`, `SwimMotor.cs`, `SwimTuning.cs` | Authoritative swimming root, physics integration and tuning. P2 presentation must not take ownership of these. |
| `game/scripts/ControlPreset.cs`, `ControlPresetStore.cs`, `KeyboardSwimInput.cs`, `MouseSwimInput.cs`, `ControlPresetsPanel.cs` | Implemented built-in/custom control profiles, persistence, intent resolution and pause editor. The accepted baseline has Keyboard Only, Mouse Only and Hybrid Swim. |
| `game/scripts/FollowCamera.cs` | Third-person follow/obstruction behavior. P2 geometry must support it without making camera state authoritative. |
| `game/scripts/FishVisualMotion.cs` | Player visual swim/glide/turn deformation driven from movement state. |
| `game/scripts/DisplayPreferences.cs`, `PlayShell.cs` | Local display/input preferences and pause/options/focus lifecycle. Water post effects must not distort or capture this UI unintentionally. |
| `game/resources/swim_tuning.tres` | Tracked owner-accepted P1 movement/camera/animation baseline; [exact values](docs/tasks/p1/qualification/accepted-tuning.md). |
| `game/assets/vendor-local/` | Ignored working copies of acquired third-party assets. Public source must not embed restricted payloads. |
| `game/dev/`, `tools/dev` | Focused scene/runtime checks and repository-relative developer commands. |
| `game/addons/godot_mcp/` | Matched upstream editor bridge/runtime probe, separate from gameplay. |

## P2 boundaries to introduce only when needed

| Boundary | Owns | Does not own |
| --- | --- | --- |
| Habitat composition | Natural seabed, rock formations, tunnels/arches/caves, coral/reef placement, hidden world limits, spawn and traversal landmarks. | Fish movement rules, feeding, predator AI or save state. |
| Underwater environment | Selected renderer configuration, light/fog/color/depth treatment, caustic-style surface lighting, suspended particles and bounded post-processing. | HUD simulation state or camera-driven gameplay advantages. |
| Water reaction presentation | Read-only sampling of fish velocity/turn/acceleration to drive wakes, turbulence, bubbles/particles or trails. | Applying forces, changing collision, modifying speed or simulating authoritative fluid. |
| Environment assets | Prepared reusable/optimized visual pieces, provenance/license records, local installation mapping and performance-minded instancing. | Treating an asset listing as runtime qualification. |
| P1 qualification fixture | The preserved movement course and any checks still needed to reproduce P1 evidence. | Becoming the normal P2 world presentation. |

Names and exact paths are implementation choices. Prefer small reusable scenes/resources over one giant scripted world builder, and keep renderer-specific code behind the selected P2 presentation boundary.

## Later gameplay boundaries

| Boundary | Owns | Does not own |
| --- | --- | --- |
| Fish/run simulation | Stable life identity, health, growth, body size, life state, simulation time. | Camera position, UI labels, narrative wording. |
| Ecology | Food/prey/predator entities, consumption, pursuit, escape and encounter outcomes. | Saving UI or final story prose. |
| Persistence | Versioned active-life data, durable terminal state, archive writes and explicit recovery. | Treating storage failure as fish death. |
| Life event journal | Bounded meaningful events with IDs, time, location/entity references and actual outcomes. | Invented achievements or frame-by-frame replay capture. |
| Story/archive | Selection of supported life events, final saved account and history entries. | Reviving a dead life or requiring a remote AI service. |
| Presentation | Third/first-person cameras, fish appearance, HUD, menus, recap/archive views. | Advancing simulation from rendering or history browsing. |

These are responsibility boundaries, not a requirement to create frameworks, autoloads, or empty classes now.
