# c3-visibility prompt assessment — 2026-11-10

**Status:** Authored planning, not executed or qualified.
**Authoring repository:** jfin602/fish main at 9e858110a6ec571e964d72e26ef1de57bfdad36b before this task stack; root version 0.3.0. A connected GitHub read cannot establish the owner's local uncommitted P3 state.

## /prompt-ass — owner outcome and observed gap

The owner observed normal play showing essentially the whole reef, including distant map boundaries, at once. This removes discovery from swimming. Requirement S17 / C50 and the approved [underwater visibility contract](../../systems/underwater-visibility.md) call for colorful clear near-field details, softened intermediate features, faint silhouettes farther away, and distant terrain disappearing into coherent water. Depth should shorten useful visual range without making close navigation impossible.

The inspected normal-play Forward+ source already has an atmosphere owner and a GFX preset system. In game/scripts/UnderwaterAtmosphere.cs, the default DistanceFogDensity is 0.0035, RefreshDepth() clamps effective environment fog at 0.02, and EffectiveUsefulRange currently mirrors VolumetricFogLength (64 m shallow), which is **not** a physically or visually qualified sight distance. WaterColumnChecks.cs expects the existing monotonic depth curve. GraphicsPanel.cs presents a density control; LightingSettings.cs persists density; LightingPresetStore.cs serializes schema v1 to user://lighting-presets.json, including custom presets. This work should adapt those owners rather than create another atmosphere subsystem.

## Required outcome

1. An ordinary underwater view cannot recognize the opposite playable boundary in clear open sightlines.
2. Fish, terrain, nearby coral, cave mouths and obstacles remain readable in the immediate play area.
3. Forward movement brings new reefs and caves from water -> silhouette -> identifiable detail; retreat is equally smooth.
4. Shallow water is brightest/clearest, middle depth hazier, deep water more limited, cooler and darker. The initial contract's shallow ~30–40 m, middle ~25–30 m and deep ~18–22 m are tuning targets, not a promise of a hard cutoff.
5. The accepted bright overhead water surface, organic moving caustics, sun shafts, water reaction, stylized palette and crisp UI survive.
6. GFX shows an understandable Visibility Distance (m) control; existing valid custom presets are retained or migrated deterministically, not discarded on load.
7. Runtime visibility modulation never rewrites saved base preset values, gameplay state, motor, collision, selected fish, or authoritative camera controls.
8. Native owner review establishes the actual discovery feel, not only headless shader validity; representative same-scene performance deltas are reported.

## Preserved boundaries and coordination

- Do not modify the accepted P1 fish motor, SwimTuning, camera-control semantics, collision, input profiles, P1 fixture or Forward+ selection.
- Do not modify P3's separate terrain/FAF integration, caves, Character Select, five-fish catalog, return-to-select flow, or assigned 0.3.x checkpoints.
- Do not add predators, feeding, growth, saves, death/history, species stats, new renderer, fluid simulation, large fog-volume collections, multiple lights or new graphics frameworks.
- Correction 3 keeps an unchanged root version, **0.3.0 at GitHub authoring**. If local P3 has already advanced package.json, do not downgrade it, run an incompatible runner, or edit a currently active P3 prompt snapshot. Reconcile/re-author c3's unchanged-version metadata to the reviewed, actually completed P3 checkpoint before execution.
- Use a clean, paused runner boundary; never run c3 and p3 simultaneously in the same worktree. Preserve unrelated local work and existing source/evidence.
- P2 V18/V19 remain historical PASS, G2.3/V20 remains OPEN; c3 contributes pending P3 V25 evidence, not retroactive P2 or automatic P3 GREEN.

## Prompt decomposition and evidence

| Prompt | Scope | Tier / qualification |
| --- | --- | --- |
| P1 | Depth-aware distance extinction and coherent water color with clear foreground | T2 focused atmosphere/depth checks; native look deferred |
| P2 | GFX visibility-distance control, preset compatibility/migration, state isolation | T2 settings/store/UI lifecycle tests |
| P3 | Whole-scene integration, diagnostic probes, reproducible A/B method and stability | T2 focused/regression checks; native probe execution may remain P4 |
| P4 | Actual native route/view, owner visual judgment, performance comparison and correction closeout | T3 mandatory native owner handoff; final manual closeout |

## Risks and mitigations

- **Fog makes the fish/world unreadable:** tune near clarity and broad transition; test near fish, coral and cave approaches, not just distant terrain.
- **Uniform whitening or hard cutoff:** align fog/background/deep color, favor continuous extinction, avoid abrupt geometry clipping or overexposed scattering.
- **Distance and depth confused:** base distance responds to camera-to-object length, depth responds to camera Y, world-depth shading remains independent. Do not use VolumetricFogLength as an advertised optical range.
- **Preset data loss:** recognize valid schema v1, define conversion or compatibility rules, test old custom records and active ID, backup/atomic writes, keep malformed-file handling distinct.
- **P3 concurrent edits:** stop on changed source/version and merge deliberately at a clean checkpoint rather than resetting or force-pushing.
- **Performance regression:** reuse one environment/scalar updates; compare same camera, scene, preset and renderer before/after, not only effects-on/off (which tests the whole stack). Investigate repeatable >=10% median frame-time regression before acceptance.
- **False qualification:** headless checks establish finite bounds, stable state and resource validity, not true visual range or owner feeling. Explicitly report NOT OBSERVED or BLOCKED for missing native results.

## Authoring / execution distinction

Creating this stack documents work only; it does not implement fog, run Codex, capture native evidence, advance a version or mark a Gate complete. Check actual local branch/HEAD/dirty tree and renderer/mode before running.
