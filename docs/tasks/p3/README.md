# P3 — FAF reef world rebuild

**Status: AUTHORED / BLOCKED ON P2.** P3 rebuilds and dresses the accepted varying-depth P2 world with the completed original FAF library. It must not execute until P2 is actually GREEN and root is deliberately moved to `0.3.0`.

Read the [phase design](../../planning/p3/reef-world-rebuild.md), [assessment](prompt-assessment.md), [implementation plan](implementation-plan.md), [FAF art style](../../fish-asset-factory/art-style.md), [style proof 001](../../fish-asset-factory/style-proof-001/README.md) and [runner contract](../../phase-runner.md).

## Goal

Turn the accepted technical depth world into the first fully dressed Fish reef:

- preserve P2 routes/depth/water;
- rebuild visible terrain with FAF terrain chunks;
- create caves, tunnels, arches, overhangs and geological landmarks;
- integrate the complete coral/plant/detail library;
- make higher/shallower bands the lushest and lower/deep bands increasingly sparse/geological;
- optionally add bounded ambient FAF reef fish;
- improve on style-proof 001's weak stacked arch, visible terrain seams and sparse layering;
- reach the owner-approved reef target from the actual gameplay camera.

## Entry and version

At authoring, root remains **`0.2.6`** and P2 is not yet closed because G2.3/V20 remains open. This stack changes no current version.

Before execution:

1. P2 G2.1–G2.3 must be GREEN with final accepted source.
2. Prepare deliberate root baseline **`0.3.0`**.
3. Inspect current HEAD/dirty state, normal-play scene, depth/collision authority, FAF manifests/GLBs and `tools/dev` composition.
4. Verify Fish MCP identity before editor mutation.
5. Preserve Forward+, P2 water/light/depth and water-reaction behavior, accepted movement/camera/controls and the P1 fixture.

## Prompt order

| Step | Prompt | Outcome | Assigned version | Tier / handoff |
| --- | --- | --- | --- | --- |
| P1 | [FAF runtime integration](P1-faf-runtime-integration.txt) | All 38 core FAF environment variants load through reusable normal-play wrappers/catalog | `0.3.1` | T1/T2 |
| P2 | [Depth-world visual rebuild](P2-depth-world-visual-rebuild.txt) | Five-band world visibly rebuilt with FAF terrain while P2 traversal/collision authority is preserved | `0.3.2` | T2 |
| P3 | [Caves tunnels and geology](P3-caves-tunnels-and-geology.txt) | Natural large-scale geology, multiple swim-throughs, cave/shelter and deep corridor | `0.3.3` | T2 |
| P4 | [Depth-based reef dressing](P4-depth-based-reef-dressing.txt) | Complete coral/plant/detail coverage with lush-shallow → sparse-deep gradient and seam dressing | `0.3.4` | T2 |
| P5 | [Ambient reef life and finish](P5-ambient-reef-life-and-finish.txt) | Presentation-only reef fish plus repetition/color/intersection cleanup | `0.3.5` | T2 |
| P6 | [Native style and performance tuning](P6-native-style-and-performance-tuning.txt) | Owner/native target-reference review across depths/routes plus performance tuning | `0.3.6` | T3, mandatory manual handoff |
| P7 | [FAF reef world closeout](P7-faf-reef-world-closeout.txt) | G3.1–G3.3 evidence review, owning-doc reconciliation and next-phase routing | `0.3.7` | T3/T1, final manual closeout |

P7 is the sole final closeout. P6 is the main native owner checkpoint.

## Required environment coverage

Accepted normal play must coherently use:

- 6 terrain variants;
- 4 rock variants;
- 12 coral variants;
- 8 plant variants;
- 8 detail variants.

**38 environment variants total.**

FAF-FISH-001–004 are available for bounded non-gameplay ambience. FAF-FISH-005 shark stays deferred to later ecology by default.

## Evidence

Execution creates evidence only when checks occur:

| Record | Purpose |
| --- | --- |
| `qualification/implementation.md` | P1–P5 asset, placement, terrain/collision, geology, density and ambient-life implementation results. |
| `qualification/native-world-art.md` | P6 native shallow/mid/deep, cave/tunnel, target-style, owner judgment and performance observations. |
| `qualification/accepted-world-art.md` | Accepted scene/resources, density/placement rules, ambient configuration and performance scope. |
| `closeout.md` | P7 G3.1–G3.3 evidence mapping and Phase disposition. |

Isolated FAF proofs and style-proof 001 are design/candidate evidence, not P3 Gate evidence.

## Execute

Only after P2 closeout and deliberate `0.3.0` entry:

```bash
npm run fish:phase:validate -- p3
npm run fish:phase -- p3
```

The runner owns implementation checkpoints. Preserve partial work; never manufacture P2/P3 versions or Gate evidence.
