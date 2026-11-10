# P2 — Build the underwater world

**Status: authored, NOT STARTED, READY TO RUN from root version `0.2.0`.** [P1 closeout](../p1/closeout.md) records the accepted baseline and Gate results. This Phase replaces the visible movement-laboratory presentation with a natural reef-like habitat, locks the renderer from measured native comparison, adds convincing underwater atmosphere, and adds movement-reactive water presentation. Read the [phase design](../../planning/p2/underwater-world.md), [assessment](prompt-assessment.md), [implementation plan](implementation-plan.md), and [runner contract](../../phase-runner.md).

## Scope and finish line

P2 is successful when normal play is enjoyable to explore with no survival mechanics yet:

- the flat/test-course presentation is gone from normal play;
- the fish swims over a continuous natural seabed with varied elevation, coral/reef areas, open water, rock formations, a tunnel/tight passage, cover/overhang and meaningful vertical routes;
- the world looks unmistakably underwater through a layered atmosphere, not just a blue tint;
- the fish visibly disturbs the water differently while gliding, cruising, accelerating and turning hard;
- the chosen renderer and effects are based on native evidence from the actual target desktop;
- P1 movement/camera/input behavior remains authoritative and comfortable in the new space.

P2 does **not** implement feeding, prey/predator AI, growth, persistence/death/history, first person, controller, free look, Movement abilities, production procedural generation, final biome commitment, audio overhaul or a full fluid solver.

## Entry and version

At authoring on 2026-11-09, main was at documentation commit `7c428c403cb9bafdcad1491a9cc404d0f6da171e`; root was `0.1.9`. The current accepted gameplay source is clean `main` `27c4debf0ac04f9cb2eff927c1e31c6b604c0b60` before this documentation-only closeout. P1 is COMPLETE, c1 is reconciled, and root `package.json` is deliberately **`0.2.0`**. [Accepted P1 tuning](../p1/qualification/accepted-tuning.md) is the P2 inheritance record. No P2 implementation has begun.

P2 entry is satisfied as follows:

1. [P1 closeout](../p1/closeout.md) records G1.1–G1.4 PASS under the owner's accepted progression criteria and names evidence limits.
2. [c1 closeout](../c1-bindings/closeout.md) reconciles controls into that candidate while retaining advanced native gaps.
3. Root version **`0.2.0`** is the Phase 2 entry baseline; P2 prompts retain their authored `0.2.1`–`0.2.8` assignments.
4. At execution, confirm the real Godot project/toolchain and Fish MCP identity and inspect `tools/dev` composition and actual Compatibility renderer.
5. Preserve the P1 movement course as a regression/qualification fixture even though it should no longer be normal-play scenery.
6. Verify the locally prepared P2 environment inventory at `artifacts/assets/source/p2-environment/inventory.json` and its `game/assets/vendor-local/p2-environment/` working assets before world-building. The intended palette is Coast Sand 01 + Ground060 + Gravel041 + Rock063; Rock Face 01 + Rock Face 02 + Coastal Cliff 01 + Boulder 01; and Heliopora coerulea + Acropora cervicornis + Acropora palmata + Meandrina meandrites + Montipora danae. **Owner requirement: the final normal-play habitat must visibly use every prepared model at least once and visibly use every prepared terrain material set.** Do not silently replace a missing prepared asset with a new download; if one cannot be used, block the affected P2 task and report the exact issue.

The P1 handoff resolved stale conflict-marker text in root `AGENTS.md` and `README.md` where the current checkout made the correct state unambiguous; no broader cleanup was performed.

## Prompt order

| Step | Prompt | Outcome | Assigned version | Tier / handoff |
| --- | --- | --- | --- | --- |
| P1 | [Renderer comparison vertical slice](P1-renderer-comparison-vertical-slice.txt) | Reversible Compatibility/Forward+ comparison harness and underwater feature slice | `0.2.1` | T1/T2, no native pause |
| P2 | [Native renderer lock](P2-native-renderer-lock.txt) | Same-candidate native comparison; one renderer selected/recorded | `0.2.2` | T3, mandatory manual handoff |
| P3 | [Natural habitat foundation](P3-natural-habitat-foundation.txt) | Continuous seabed, natural limits, new normal-play habitat, P1 course preserved | `0.2.3` | T2 |
| P4 | [Reef tunnels and world composition](P4-reef-tunnels-and-world-composition.txt) | Coral floor, rock tunnels/arches/overhangs, open/vertical routes, prototype cleanup | `0.2.4` | T2 |
| P5 | [Underwater atmosphere](P5-underwater-atmosphere.txt) | Fog/depth/color/light/caustics/motes/screen treatment on selected renderer | `0.2.5` | T2 |
| P6 | [Fish-reactive water](P6-fish-reactive-water.txt) | Wake/turbulence/particle response driven read-only by actual movement | `0.2.6` | T2 |
| P7 | [Native world tuning](P7-native-world-tuning.txt) | V18–V20 owner/native tuning and performance/readability qualification | `0.2.7` | T3, mandatory manual handoff |
| P8 | [Underwater world closeout](P8-underwater-world-closeout.txt) | G2.1–G2.3 evidence review, docs reconciliation and Phase disposition | `0.2.8` | T3/T1, final manual closeout |

P8 is the one final closeout. P2 and P7 are intermediate native checkpoints. The historical runner field `- Browser required: yes.` means native/manual Godot observation; ordinary web browsing does not qualify the game.

## Acceptance and evidence

Use roadmap G2.1–G2.3 and validation V18–V20.

Execution should create evidence only when checks occur:

| Record | Purpose |
| --- | --- |
| `qualification/implementation.md` | P1/P3–P6 source, commands, focused/integration results, failures/repairs. |
| `qualification/renderer-decision.md` | P2 same-candidate Compatibility/Forward+ native comparison, selected renderer and rationale. |
| `qualification/native-world.md` | P7 V18–V20 real-window traversal/visual/effect/performance observations and owner feedback. |
| `qualification/accepted-environment.md` | Final renderer, scene/resource/effect/tuning identity that P7 accepted. |
| `closeout.md` | P8 evidence mapping and final G2.1–G2.3 / Phase 2 disposition. |

Do not prefill these as PASS. A shader compiling, a headless scene tree, or a static screenshot does not establish a convincing underwater world.

## Execute

From the reviewed P1 close and deliberate `0.2.0` baseline:

```bash
npm run fish:phase:validate -- p2
npm run fish:phase -- p2
```

The runner owns implementation checkpoint commits. Manual tasks require reviewed completion/checkpoint handling before resume. Preserve partial work on failure; do not manufacture a version or Gate pass to move forward.
