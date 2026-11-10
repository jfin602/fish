# Module map

This map distinguishes implemented source from planned boundaries. Inspect the checkout before editing; [P2's P8 review](docs/tasks/p2/closeout.md) and [c2-depth P4](docs/tasks/c2-depth/closeout.md) record historical failures, while the later R1 native closeout is owner-green for depth.

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
| `docs/tasks/p2/` | Underwater-world Phase stack; historical P1–P8 implementation/evidence retained while later corrections refine the still-open P2 candidate. |
| `docs/tasks/c2-watet-light-fx/` | Owner-green P2 water/light correction: overhead surface, organic caustics, ribbon shafts, GFX presets and native evidence. |
| `docs/tasks/c2-depth/` | Five-band basin/depth-light correction with preserved historical failure and later R1 route/visual repair qualified by normal-input traversal and owner hand-play. |
| `docs/tasks/p3/` | Authored, unexecuted FAF reef-world rebuild stack; blocked until P2 GREEN and deliberate `0.3.0` entry. |
| `docs/fish-asset-factory/` | Original Fish art style, palette, construction/export standards and visual review; no gameplay or Gate authority. |
| `tools/asset-factory/` | Tracked Blender naming, palette, basic validation and GLB export helpers; family generator packages are reserved, not implemented. |

## Implemented Godot/P1 and P2 foundation source boundaries

| Path | Responsibility |
| --- | --- |
| `game/project.godot` | Godot project, owner-selected Forward+ renderer, main scene, display/input defaults, enabled MCP addon/runtime probe. |
| `game/Fish.csproj`, `game/Fish.sln`, `game/global.json` | Godot .NET target and .NET SDK pins. |
| `game/scenes/main.tscn` | Normal-play root: instances `p2_habitat.tscn`, retains accepted Fish3 controller/camera/input and world light/environment hook. |
| `game/scenes/p2_habitat.tscn`, `p2_reef_composition.tscn` | Reusable P2 spawn, continuous seabed/hidden limits and authored rock/coral anchors with open, tunnel, shelter, vertical and five-level depth route markers. R1's authored endpoints fit the ±24.9 m fish bound and passed normal-input forward/return traversal. |
| `game/scenes/p1_course_fixture.tscn`, `movement_course.tscn` | Byte-preserved P1 main scene and its movement/obstruction course for regression and renderer comparison. |
| `game/scenes/renderer_comparison.tscn` | P1 diagnostic comparison now instances the dedicated P1 fixture, keeping normal play free of the course. |
| `game/scripts/Main.cs` | Main-scene initialization, shell setup and smoke integration. |
| `game/scripts/StarterWorld.cs` | P1 fixture-only local asset readiness and sand/rock/coral/ambient-fish dressing. |
| `game/scripts/PlayerArt.cs` | Shared checked Fish3 import, rig/clip verification and visual attachment. |
| `game/scripts/P2HabitatWorld.cs`, `ReefComposition.cs` | Normal P2 readiness, safe spawn, simple environment setup and checked local model installation at authored anchors; no movement authority. |
| `game/scripts/HabitatPalette.cs`, `SeabedSurface.cs`, `game/shaders/seabed_blend.gdshader` | Checked local Coast Sand 01/Ground060/Gravel041/Rock063 palette, one visually blended five-band basin surface and matching two-sided collision, with world-height cool/dim shading. R1 reshaped the transitions/outer rise; the owner accepted native naturalness and perimeter presentation. |
| `game/scripts/FishController.cs`, `SwimMotor.cs`, `SwimTuning.cs` | Authoritative swimming root, physics integration, ±24.9 m horizontal bound and tuning. P2 route markers must fit reachable player space; presentation must not take ownership of these. |
| `game/scripts/ControlPreset.cs`, `ControlPresetStore.cs`, `KeyboardSwimInput.cs`, `MouseSwimInput.cs`, `ControlPresetsPanel.cs` | Built-in/custom profiles, versioned persistence, intent resolution and pause editor. Keyboard Only, Mouse Only and Hybrid Swim form the accepted baseline; Hybrid 2 is newly implemented and awaits owner feel testing. |
| `game/scripts/FollowCamera.cs` | Third-person follow/obstruction behavior and the Hybrid 2 presentation-only 360 mouse orbit. P2 geometry must support it without making camera state authoritative. |
| `game/scripts/FishVisualMotion.cs` | Player visual swim/glide/turn deformation driven from movement state. |
| `game/scripts/UnderwaterAtmosphere.cs`, `LightingSettings.cs`, `LightingPresetStore.cs`, `game/shaders/water_surface.gdshader`, `underwater_sky.gdshader`, `sun_shafts.gdshader`, `underwater_screen.gdshader` | Forward+ camera-depth extinction and world-height fog/shaft treatment, top light, bounded overhead surface derived from the habitat upper limit, procedural sky, one batched caustic-aligned shaft mesh, one moving organic caustic projection, GPU motes, restrained screen treatment and bounded saved GFX presets; no fish authority. R1 preserves deep upward brightness; the owner accepted depth visibility, effects and GFX behavior in native play. |
| `game/scripts/WaterReaction.cs` | Forward+ tail-space GPU ribbons and spherical bubbles driven by read-only motor presentation values; 44 live slots maximum, no forces or collision. |
| `game/scripts/DisplayPreferences.cs`, `PlayShell.cs` | Local display/input preferences and pause/options/focus lifecycle. Water post effects must not distort or capture this UI unintentionally. |
| `game/resources/swim_tuning.tres` | Tracked owner-accepted P1 movement/camera/animation baseline; [exact values](docs/tasks/p1/qualification/accepted-tuning.md). |
| `game/assets/vendor-local/` | Ignored working copies of acquired third-party assets. Public source must not embed restricted payloads. |
| `game/dev/`, `tools/dev` | Focused scene/runtime checks and repository-relative developer commands, including `inspect-habitat`, `inspect-reef`, `inspect-atmosphere`, `inspect-depth`, `measure-atmosphere`, `inspect-water` and preserved P1 fixture checks. R1's reef check measures the actual fish clamp and authored-marker margin; native physical-key evidence verifies traversal. |
| `game/addons/godot_mcp/` | Matched upstream editor bridge/runtime probe, separate from gameplay. |

## P2 composition/effects boundaries

| Boundary | Owns | Does not own |
| --- | --- | --- |
| Habitat composition | Natural seabed, rock formations, tunnels/arches/caves, coral/reef placement, hidden world limits, spawn and traversal landmarks. | Fish movement rules, feeding, predator AI or save state. |
| Underwater environment | Selected renderer configuration, light/fog/color/depth treatment, caustic-style surface lighting, suspended particles and bounded post-processing. | HUD simulation state or camera-driven gameplay advantages. |
| Water reaction presentation | Read-only sampling of fish velocity/turn/acceleration to drive wakes, turbulence, bubbles/particles or trails. | Applying forces, changing collision, modifying speed or simulating authoritative fluid. |
| Environment assets | Prepared reusable/optimized visual pieces, provenance/license records, local installation mapping and performance-minded instancing. | Treating an asset listing as runtime qualification. |
| P1 qualification fixture | The preserved movement course and any checks still needed to reproduce P1 evidence. | Becoming the normal P2 world presentation. |

The paths above are the current source owners. Keep renderer-specific code behind the selected P2 presentation boundary; [P8 review](docs/tasks/p2/closeout.md) and [c2-depth R1 native closeout](docs/tasks/c2-depth/qualification/native-depth.md#r1-addendum--repaired-candidate-native-observation) distinguish historical implementation evidence from the later owner-qualified depth result. G2.3 fish-reactive water still needs separate P2 qualification.

## Planned P3 world-art boundaries

| Boundary | Owns | Does not own |
| --- | --- | --- |
| FAF world catalog / wrappers | Canonical tracked FAF environment IDs, runtime resource paths and reusable placement roots for terrain, rocks, coral, plants and details. | Re-exporting Blender sources or inventing gameplay state. |
| P3 terrain/geology composition | Visible FAF terrain skin, rock clusters, caves/tunnels/overhangs and authored deterministic placement over the accepted P2 navigation/depth skeleton. | Replacing fish movement/camera/depth authority to make art fit. |
| P3 reef dressing | Coral/plant/detail clusters, depth-density rules, optional visual sway, seam dressing and open-route exclusions. | Ecology simulation or collision-heavy decorative dressing. |
| P3 ambient life | Bounded presentation-only FAF-FISH-001–004 schools/loops when used. | Feeding, prey/predator state, damage, persistence or shark gameplay. |

Exact source paths should follow the smallest implementation that fits the accepted P2 world; do not create empty frameworks before P3 execution.

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
