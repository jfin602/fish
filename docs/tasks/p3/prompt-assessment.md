# P3 prompt assessment — FAF reef world rebuild

**Assessment date:** 2026-11-10  
**Authoring state:** P2 G2.1/V18 and G2.2/V19 have owner-green repaired depth evidence; G2.3/V20 remains open. Root remains `0.2.6`. P3 is authored ahead of execution and requires a later P2 GREEN baseline.

## Owner request

All core Fish Asset Factory assets are made. Rebuild the current **varying-depth** world using them, fill it out, create caves/tunnels/etc., and make higher/shallower areas carry more vegetation than lower/deeper areas.

## Existing world substrate

The accepted repaired P2 depth candidate supplies:

- five traversable bands;
- primary/alternate shallow-to-deep return routes;
- tunnel/shelter routes;
- Forward+ water/light/depth extinction;
- owner-accepted route/camera/depth presentation for V18/V19;
- movement-reactive water still requiring V20/P2 closeout.

P3 must consume the final **fully closed P2** source, not skip the remaining P2 evidence.

## Completed original asset inputs

Technically proven FAF assets now include:

- 6 terrain variants;
- 4 rock variants;
- 12 coral variants;
- 8 plant variants;
- 8 small detail variants;
- 5 fish candidates.

The integrated [style proof 001](../../fish-asset-factory/style-proof-001/README.md) places all 43 GLBs in an isolated scene and exposes the exact live-world problems P3 must solve: stacked-boulder arch language is weak, terrain overlap seams/perimeter are obvious, reef forms are too sparse against oversized stone masses, and the composition is still a diorama rather than traversable world geometry. P3 should reuse its successful clustering ideas but not copy its weaknesses.

## Required outcome

1. Keep accepted P2 depth/navigation/collision and Forward+ water/light.
2. Replace visible technical/terrace terrain with layered FAF terrain and geology.
3. Build natural caves, tunnels, arches, overhangs, canyon/corridor routes and landmarks.
4. Integrate all 38 environment variants coherently.
5. Establish a strong vertical density gradient: shallow/high lushest; upper/middle rich; lower/deep sparse/geological.
6. Use details to enrich close/midground bases and hide contacts without clutter.
7. Preserve negative/open swimming lanes and camera readability.
8. Add Character Select using **all five FAF fish**, including the Blacktip Reef Shark, as selectable player visuals. All choices share the current motor/collision/camera/tuning; normalize only presentation scale/orientation.
9. Fresh launch enters Character Select; selection starts normal play; Pause/Game Menu provides Return to Character Select that resets/unloads the current P3 session without death/save/history semantics.
10. Optionally integrate FAF-FISH-001–004 as presentation-only ambient schools; shark remains excluded from ambient predator behavior.
11. Beat style-proof 001 from the actual gameplay camera and obtain direct owner acceptance.
12. Measure representative dense/cave/deep performance.

## Decomposition

| Task | Responsibility | Evidence |
| --- | --- | --- |
| P1 | Runtime catalog/wrappers for 38 environment variants + five selectable fish descriptors | T1/T2 |
| P2 | Visible terrain rebuild over accepted five-band P2 route/depth skeleton | T2 |
| P3 | Geological caves/tunnels/arches/chambers/landmarks | T2 |
| P4 | Full coral/plant/detail dressing with depth-density gradient and optional sway | T2 |
| P5 | Character Select, selected player visual, session-local routing and Return to Character Select | T2 |
| P6 | Ambient reef life and world-wide repetition/intersection cleanup | T2 |
| P7 | Native owner world-art + character-select/return-flow + performance tuning | T3 |
| P8 | Evidence/Gate closeout | T1 review of T3 |

## Risks and mitigations

- **P3 hides an unfinished P2 V20:** hard entry on P2 GREEN and `0.3.0`.
- **Visible art lies about collision:** retain one authoritative P2 collision/query boundary or deliberately rebuild matching simplified collision; sample route-critical agreement.
- **Style-proof weak arch repeats:** use terrain shelf/drop-off mass plus rock clusters; do not use a simple three-boulder lintel as the hero structure.
- **Terrain seams remain visible:** overlap irregular aprons, vary orientation and use rock/coral/plant/detail dressing at contacts.
- **World becomes cluttered:** explicit depth zones, exclusion corridors and negative space.
- **Uniform scatter / copy-paste:** deterministic authored clusters with varied silhouette/rotation/scale.
- **Too much deep vegetation:** require a visible shallow→deep density trend.
- **Dense dressing hurts performance:** shared resources/MultiMesh where useful, no decorative collision, native measurement.
- **Caves break camera:** route checks use accepted fish radius and camera obstruction/recovery.
- **Details become confetti:** keep sparse base/foreground placement and quiet sand.
- **Character choice accidentally changes gameplay:** every candidate uses one shared authoritative FishController/collision/motor/tuning; only visual scene, animation, presentation transform and display name vary.
- **Shark raw size breaks the camera/collider contract:** give each character an explicit presentation normalization transform into the shared player envelope; never resize collision/tuning in P3.
- **Return to Character Select becomes hidden persistence/death:** make it an explicit session-routing action that clears input/unloads play and never calls death, save, archive or later run-lifecycle APIs.
- **Ambient fish become gameplay AI:** no collision, feeding, detection, health, predator or event state.
- **Target is only a screenshot:** P7 requires real swimming through depths/caves plus the actual Character Select/return flow with owner judgment.

## Success boundary

P3 remains NOT GREEN if the world still reads as terraces/patchwork, caves/tunnels are unusable, required variants look like checklist clutter, shallow/deep density is visually flat, seams/repetition dominate, routes become unreadable, the gameplay-camera view does not approach the target, representative density performs poorly, any of the five FAF fish is unavailable in Character Select, the selected visual changes authoritative gameplay, or Return to Character Select triggers death/save/history behavior.

Authoring changes no current package version or P2 Gate.
