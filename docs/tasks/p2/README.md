# P2 — Build the underwater world

**Status: authored, NOT EXECUTED.** This Phase follows actual P1 close. It replaces the visible movement-laboratory presentation with a natural reef-like habitat, locks the renderer from measured native comparison, adds convincing underwater atmosphere, and adds movement-reactive water presentation. Read the [phase design](../../planning/p2/underwater-world.md), [assessment](prompt-assessment.md), [implementation plan](implementation-plan.md), and [runner contract](../../phase-runner.md).

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

At authoring on 2026-11-09, main is at documentation commit `7c428c403cb9bafdcad1491a9cc404d0f6da171e`; gameplay source is still the P1 direct-P9 candidate and root `package.json` is **`0.1.9`**. P1 P10/P11 and the separate c1-bindings correction remain open. This stack must **not** be run merely because it exists.

Before P2 execution:

1. P1 must be actually closed with G1.1–G1.4 evidence and accepted final movement/camera/input tuning.
2. Reconcile any c1-bindings work and current main/working tree; do not downgrade or overwrite later P1 changes.
3. Deliberately prepare root version **`0.2.0`** as the Phase 2 entry baseline before invoking the runner. The runner will not create that baseline for you.
4. Confirm the real Godot project/toolchain and Fish MCP identity. Inspect current `tools/dev` composition and actual renderer.
5. Preserve the P1 movement course as a regression/qualification fixture even though it should no longer be normal-play scenery.

The inspected repository still contains unrelated pre-existing conflict-marker text in root `AGENTS.md` and `README.md`. This P2 stack does not authorize a sweeping cleanup. Follow unambiguous current source/evidence and surface any conflict that materially blocks execution.

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

After the reviewed P1 close and deliberate `0.2.0` baseline:

```bash
npm run fish:phase:validate -- p2
npm run fish:phase -- p2
```

The runner owns implementation checkpoint commits. Manual tasks require reviewed completion/checkpoint handling before resume. Preserve partial work on failure; do not manufacture a version or Gate pass to move forward.
