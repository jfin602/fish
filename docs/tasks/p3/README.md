# P3 — FAF reef world rebuild

**Status: IN PROGRESS at root `0.3.5`; P1–P5 implemented, P6 ambient reef life/finish next.** The owner explicitly authorized the P3 transition on 2026-11-10 while accepting P2 G2.3/V20 as a documented open gap. P3 rebuilds and dresses the accepted varying-depth P2 world with the completed original FAF library.

Read the [phase design](../../planning/p3/reef-world-rebuild.md), [assessment](prompt-assessment.md), [implementation plan](implementation-plan.md), [FAF art style](../../fish-asset-factory/art-style.md), [style proof 001](../../fish-asset-factory/style-proof-001/README.md) and [runner contract](../../phase-runner.md).

## Goal

Turn the accepted technical depth world into the first fully dressed Fish reef:

- preserve P2 routes/depth/water;
- rebuild visible terrain with FAF terrain chunks;
- create caves, tunnels, arches, overhangs and geological landmarks;
- integrate the complete coral/plant/detail library;
- make higher/shallower bands the lushest and lower/deep bands increasingly sparse/geological;
- add a Character Select page with animated previews for all five FAF fish, including the Blacktip Reef Shark, and start play with the selected visual;
- add Pause/Game Menu → Return to Character Select without death/save semantics;
- optionally add bounded ambient FAF reef fish;
- improve on style-proof 001's weak stacked arch, visible terrain seams and sparse layering;
- reach the owner-approved reef target from the actual gameplay camera.

## World-generation architecture correction

The original P3 P2–P4 prompt text was authored before S18 and may already exist in local runner history. **Do not rewrite/fabricate those checkpoints.** At the owner-selected P3 P6 / `0.3.6` pause, [c3-world-gen](../c3-world-gen/README.md) supersedes the old placed/modular world-formation assumptions with the deterministic compiler/bake architecture. Run c3-world-gen before c3-visibility and before P3 P7 native closeout. Character Select and later P3 art goals remain in force.

## Entry and version

The deliberate P3 entry was **`0.3.0`** under owner transition S16; P1–P5 implemented `0.3.1`–`0.3.5` in order. P2 G2.3/V20 remains an accepted evidence gap; this P3 entry does not relabel it PASS.

Before execution:

1. P2 G2.1/G2.2 are accepted and owner transition S16 explicitly authorizes progression with G2.3/V20 retained as an open gap. **Satisfied.**
2. Deliberate root baseline **`0.3.0`** is prepared. **Satisfied.**
3. Inspect current HEAD/dirty state, normal-play scene, depth/collision authority, FAF manifests/GLBs and `tools/dev` composition.
4. Verify Fish MCP identity before editor mutation.
5. Preserve Forward+, P2 water/light/depth and water-reaction behavior, accepted movement/camera/controls and the P1 fixture.

## Prompt order

| Step | Prompt | Outcome | Assigned version | Tier / handoff |
| --- | --- | --- | --- | --- |
| P1 | [FAF runtime integration](P1-faf-runtime-integration.txt) | All 38 environment variants plus all five player-selectable FAF fish load through reusable runtime catalogs/wrappers | `0.3.1` | T1/T2 |
| P2 | [Depth-world visual rebuild](P2-depth-world-visual-rebuild.txt) | Five-band world visibly rebuilt with FAF terrain while P2 traversal/collision authority is preserved | `0.3.2` | T2 |
| P3 | [Caves tunnels and geology](P3-caves-tunnels-and-geology.txt) | Natural large-scale geology, multiple swim-throughs, cave/shelter and deep corridor | `0.3.3` | T2 |
| P4 | [Depth-based reef dressing](P4-depth-based-reef-dressing.txt) | Complete coral/plant/detail coverage with lush-shallow → sparse-deep gradient and seam dressing | `0.3.4` | T2 |
| P5 | [Character select and session routing](P5-character-select-and-session-routing.txt) | Animated five-fish Character Select, selected player visual, and Pause → Return to Character Select | `0.3.5` | T2 |
| P6 | [Ambient reef life and finish](P6-ambient-reef-life-and-finish.txt) | Presentation-only ambient reef fish plus repetition/color/intersection cleanup | `0.3.6` | T2 |
| P7 | [Native style, character select and performance tuning](P7-native-style-character-select-and-performance-tuning.txt) | Owner/native world-art + five-character selection/return-flow + performance qualification | `0.3.7` | T3, mandatory manual handoff |
| P8 | [FAF reef world closeout](P8-faf-reef-world-closeout.txt) | G3.1–G3.4 evidence review, owning-doc reconciliation and next-phase routing | `0.3.8` | T3/T1, final manual closeout |

P8 is the sole final closeout. P7 is the main native owner checkpoint.

P1–P5 checks are in [implementation evidence](qualification/implementation.md); [Character Select evidence](qualification/character-select.md) records the P5 route and native-observation limits. The runner owns checkpoint commits.

## Required environment coverage

Accepted normal play must coherently use:

- 6 terrain variants;
- 4 rock variants;
- 12 coral variants;
- 8 plant variants;
- 8 detail variants.

**38 environment variants total.**

**All five FAF fish are required Character Select choices:** FAF-FISH-001 Yellow Tang, 002 Ocellaris Clownfish, 003 Blue-Green Chromis, 004 Reef Butterflyfish and 005 Blacktip Reef Shark. They share the same P3 gameplay motor/collision/camera/tuning; selection changes the visual/animation only. FAF-FISH-001–004 may also be used for bounded non-gameplay ambience. The shark stays out of ambient predator behavior until later ecology.

## Evidence

Execution creates evidence only when checks occur:

| Record | Purpose |
| --- | --- |
| `qualification/implementation.md` | P1–P6 asset, placement, terrain/collision, geology, density, character-select/session-routing and ambient-life implementation results. |
| `qualification/native-world-art.md` | P7 native shallow/mid/deep, cave/tunnel, target-style, owner judgment and performance observations. |
| `qualification/character-select.md` | P5/P7 selection catalog, animated preview, selected-player visual and Return to Character Select flow evidence. |
| `qualification/accepted-world-art.md` | Accepted scene/resources, density/placement rules, character selection identity, ambient configuration and performance scope. |
| `closeout.md` | P8 G3.1–G3.4 evidence mapping and Phase disposition. |

Isolated FAF proofs and style-proof 001 are design/candidate evidence, not P3 Gate evidence.

## Execute

From the owner-approved deliberate `0.3.0` entry:

```bash
npm run fish:phase:validate -- p3
npm run fish:phase -- p3
```

The runner owns implementation checkpoints. Preserve partial work; never manufacture P2/P3 versions or Gate evidence.
