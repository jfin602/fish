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
| `docs/tasks/p3/`, `docs/tasks/c3-world-gen/` | FAF reef-world rebuild and procedural correction implemented through C3 M4 at `0.3.6`; [P3 direct closeout](docs/tasks/p3/closeout.md) records owner-closed progression with failed/unobserved Gates, while clean M4 multi-seed bakes replace the earlier alternate-seed failure. |
| `docs/tasks/p4/` | Phase 4 P1–P11 implemented through independent Pond bake, FAF integration, waterline/camera and surface scenery, with [source/check evidence](docs/tasks/p4/qualification/implementation.md). P12 native waterline review and P14 closeout remain pending. |
| `docs/fish-asset-factory/` | Original Fish art style, palette, construction/export standards and visual review; no gameplay or Gate authority. |
| `tools/asset-factory/` | Tracked Blender naming, palette, basic validation and GLB export helpers; family generator packages are reserved, not implemented. |

## Implemented Godot/P1 and P2 foundation source boundaries

| Path | Responsibility |
| --- | --- |
| `game/project.godot` | Godot project, owner-selected Forward+ renderer, World Select startup scene, display/input defaults, enabled MCP addon/runtime probe. |
| `game/Fish.csproj`, `game/Fish.sln`, `game/global.json` | Godot .NET target and .NET SDK pins. |
| `game/scenes/main.tscn` | Normal-play root: instances `p2_habitat.tscn`, retains accepted Fish3 controller/camera/input and world light/environment hook. |
| `game/scenes/p2_habitat.tscn`, `p2_reef_composition.tscn` | Normal spawn and authored habitat shell; accepted progression Ocean Base World `ocean-m4-42` supplies baked seabed, geology, routes/analysis and dense habitat dressing. Remote fail-safe limits remain behind the terrain. Historical authored markers and P3 proofs remain, without claiming C3 P7 visual PASS. |
| `game/scenes/p1_course_fixture.tscn`, `movement_course.tscn` | Byte-preserved P1 main scene and its movement/obstruction course for regression and renderer comparison. |
| `game/scenes/renderer_comparison.tscn` | P1 diagnostic comparison now instances the dedicated P1 fixture, keeping normal play free of the course. |
| `game/scripts/Main.cs` | Main-scene initialization, shell setup and smoke integration. |
| `game/scripts/StarterWorld.cs` | P1 fixture-only local asset readiness and sand/rock/coral/ambient-fish dressing. |
| `game/scripts/PlayerArt.cs`, `CharacterCatalog.cs`, `CatalogFishVisualMotion.cs` | Shared Fish3 fixture path and ten FAF fish visuals (five marine, five Pond Adult demo choices), presentation normalization and rig-local clip playback. `SpeciesCompatibility` in `CharacterCatalog.cs` owns exact ID, native habitat, role, asset and baked-world eligibility for selection and spawning. `DemoSelection` holds only the selected FAF ID in process memory. |
| `game/scenes/world_select.tscn`, `game/scripts/WorldSelect.cs`, `game/dev/WorldSelectChecks.cs` | P4 demo entry, Ocean/Pond cards, keyboard/pointer selection, unavailable-world errors and focused route/lifecycle checks. The installed seed-71 Pond bake and owner FAF creature pack now support five headless-checked native fish choices; native integrated review is pending. |
| `game/scenes/p5_pike_encounter.tscn`, `game/scripts/PikeEncounter.cs`, `PikeCoverFixture.cs`, `WorldSightSensor.cs`, `MovementDisturbanceField.cs`, `PikeAwareness.cs`, `PikeActor.cs` | Isolated seed-5202 Bluegill/Pike encounter, verified FAF cover and physical Pike. Sight/cues supply sampled positions and times; awareness owns finite memory and behavior, actor owns collision and steering, encounter owns temporary outcomes/reset. No normal-world spawn or permanent run state. `game/dev/PikeAiChecks.cs` and `tools/dev inspect-pike-ai` check the headless behavior. |
| `game/scenes/character_select.tscn`, `game/scripts/CharacterSelect.cs` | Habitat-filtered second selection page with live 3D animated FAF previews, five Ocean and five Pond choices, session-local start and Back to World Select. |
| `game/scripts/PondCreatureCatalog.cs`, `game/assets/faf/pond/pond_creature_catalog.json`, `game/scripts/FafAmbientPondLife.cs`, `game/dev/PondCreatureChecks.cs` | Snapshot of the 25 owner fish stage exports and seven wildlife exports, Pond-only runtime lookup, Adult demo choice adapter, sparse 12-actor visual ambient placement and P8 headless integration checks. Growth and ecology are not active. |
| `game/scripts/P2HabitatWorld.cs`, `ReefComposition.cs`, `FafTerrainWorld.cs`, `FafGeologyWorld.cs`, `GeologicalRegionGenerator.cs` | Normal habitat readiness and local model installation. The accepted baked Ocean installs one generated basin, five geological mesh/collision forms and 7,960 seed-42 FAF placements across 12 depth-conditioned habitat roles. Older authored P3 source remains for historical proofs; no movement authority. |
| `game/scripts/HabitatPalette.cs`, `SeabedSurface.cs`, `GeneratedBasin.cs`, `game/shaders/seabed_blend.gdshader` | Checked palette and seed-42 `c3-full-12` shore-first basin: a 253.1 × 305.7 m water outline, inward depth field and 368 × 368 m open-seabed mesh with matching two-sided collision, cheap height/normal queries and world-height shading. |
| `game/scripts/FishController.cs`, `SwimMotor.cs`, `SwimTuning.cs` | Authoritative swimming root, physics integration and tuning. The corrected generated world expands the playable bounds while the historical P1 fixture and old Base World retain their earlier limits; presentation does not own fish movement. |
| `game/scripts/ControlPreset.cs`, `ControlPresetStore.cs`, `KeyboardSwimInput.cs`, `MouseSwimInput.cs`, `ControlPresetsPanel.cs` | Built-in/custom profiles, versioned persistence, intent resolution and pause editor. Keyboard Only, Mouse Only and Hybrid Swim form the accepted baseline; Hybrid 2 is newly implemented and awaits owner feel testing. |
| `game/scripts/FollowCamera.cs` | Third-person follow/obstruction behavior and the Hybrid 2 presentation-only 360 mouse orbit. P2 geometry must support it without making camera state authoritative. |
| `game/scripts/FishVisualMotion.cs` | Player visual swim/glide/turn deformation driven from movement state. |
| `game/scripts/UnderwaterAtmosphere.cs`, `PondTurbidityField.cs`, `PondSurfaceGeometry.cs`, `WaterVisualProfile.cs`, `LightingSettings.cs`, `LightingPresetStore.cs`, `game/shaders/water_surface.gdshader`, `underwater_sky.gdshader`, `sun_shafts.gdshader`, `underwater_screen.gdshader` | Forward+ camera-depth extinction, separate Ocean/Pond profiles, a seeded Pond-only world-space turbidity texture in one FogVolume, a generated Pond shoreline-clipped water mesh, air-side Pond screen-space/sky reflection over shallow transmission, overcast air fog/sky, one batched shaft mesh, one caustic projection, GPU motes and restrained screen treatment. No fish authority. Surface reflection and Pond spatial clarity await owner native review. |
| `game/scripts/WaterReaction.cs` | Forward+ tail-space GPU ribbons and spherical bubbles driven by read-only motor presentation values; Pond adds an eight-ring world-space pool and six-particle contact splash from read-only position/velocity/effort. No forces or collision. |
| `game/scripts/DisplayPreferences.cs`, `PlayShell.cs` | Local display/input preferences and pause/options/focus lifecycle, including nonterminal Return to World Select. Water post effects must not distort or capture this UI unintentionally. |
| `game/resources/swim_tuning.tres` | Tracked owner-accepted P1 movement/camera/animation baseline; [exact values](docs/tasks/p1/qualification/accepted-tuning.md). |
| `game/assets/vendor-local/` | Ignored working copies of acquired third-party assets. Public source must not embed restricted payloads. |
| `game/dev/`, `tools/dev` | Focused scene/runtime checks and repository-relative developer commands. `inspect-generated-geology` verifies manifold-enough meshes, collision identity, route probes, actual fish/camera clearance and traversal. `inspect-geology` retains the old authored route check, which remains red against the generated basin pending C3 P4–P5. Other P2/P1 checks remain available. |
| `game/scripts/WorldGeneration.cs`, `BakedWorldContract.cs`, `BakedWorldLoader.cs`, `FullWorldPackage.cs`, `game/dev/WorldCompilerChecks.cs`, `BasinChecks.cs`, `RouteAnchorChecks.cs`, `c3_world_native_probe.gd` | Deterministic compiler/settings/manifest, full candidate bake, verified generator-free loader and focused/native diagnostics. Normal play loads accepted progression `game/worlds/base/ocean-m4-42/` (`c3-full-12`, seed 42) without compiler stages; the older provisional package remains intact. M4 clean bakes for seeds 42/43/44 and native diagnostic views are recorded in [M4 evidence](docs/tasks/c3-world-gen/qualification/m4-habitats.md), with visual and full hand-play gaps preserved. `tools/dev promote-world` copies a validated candidate only to a new Base World destination. |
| `game/scripts/WorldCatalog.cs`, `P2HabitatWorld.cs`, `game/dev/WorldCatalogChecks.cs` | Stable Ocean/Pond identity, preview/spawn/environment/asset routes and session-local selected world. Normal play verifies pinned Ocean and Pond manifests before loading; mismatches fail closed without cross-world fallback. Active habitat/manifest references clear on unload. `tools/dev inspect-world-catalog` checks both identities, failure paths and generator-free load. |
| `game/scenes/pond_habitat.tscn`, `game/worlds/base/pond-p4-71/`, `game/dev/PondWorldChecks.cs`, `PondAtmosphereChecks.cs` | Independent Pond terrain scene, hashed bake and headless Forward+ atmosphere fixture. Shore-derived depth, seed-specific route loops, simplified physical terrain and inspectable route markers load without generation. `tools/dev inspect-pond-atmosphere` checks depth, materials, UI/focus, presets and Pond→Ocean→Pond disposal. Native naturalness and presentation remain P7 work. |
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

P1 implements the fixed 38-entry environment boundary in `game/scripts/FafRuntimeCatalog.cs` and the five-fish descriptor boundary in `game/scripts/CharacterCatalog.cs`. `game/dev/FafRuntimeChecks.cs` checks imported identity and readiness through that path; `tools/dev inspect-faf` runs it. The environment wrapper instantiates visual-only GLBs and rejects embedded physics. Plant UV.y is recorded as a future visual height mask. Fish presentation transforms normalize model scale/facing without changing the shared collider, motor or camera tuning. The existing dirty-tree Character Select/session work remains outside P1 qualification.

| Boundary | Owns | Does not own |
| --- | --- | --- |
| FAF world catalog / wrappers | Canonical tracked FAF environment IDs, runtime resource paths and reusable placement roots for terrain, rocks, coral, plants and details. | Re-exporting Blender sources or inventing gameplay state. |
| P3 world compiler / bake loader | Seed/settings/generator-version context, deterministic stages, candidate manifest/resources, baked-world loader, timings and Base World promotion. | Gameplay simulation, save-game persistence or every-load regeneration. |
| P3 basin / geology | Irregular center-deep basin, natural perimeter, arbitrary 3D cave/tunnel/arch/overhang mesh and generated collision. | Visible rectangular world walls, voxel sandbox or movement/camera authority. |
| P3 routes / analysis | Protected 3D route graph plus terrain-derived depth/slope/normal/curvature/perimeter/cave/exposure fields. | Decorative placement or fish simulation. |
| P3 reef dressing | Habitat zones, terrain-aware cluster placement, surface projection, blue-noise spacing, support/embedding checks, relaxation/scoring and depth-density rules. | Random XYZ/grid stamping, floating terrain tiles or procedural ecology. |
| P3 character select / player visual | Five-fish selectable visual catalog, animated preview, session-local selected FAF ID, shared-player visual attachment and Return to Character Select routing. | Species stats, alternate colliders/motors, persistence, death/history or predator behavior. |
| P3 ambient life | Bounded presentation-only FAF-FISH-001–004 schools/loops when used. | Feeding, prey/predator state, damage, persistence or ambient shark predator gameplay. |

Exact source paths should follow the smallest implementation that fits the compiled-world boundary; c3-world-gen may introduce only the concrete compiler/bake/query owners its prompts require. Do not create empty frameworks.

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
