# P4 — Freshwater Pond and World Select

**State: P12 native V32 review attempted at `0.4.11`, FAIL / NOT GREEN; no `0.4.12` checkpoint or P4 Gate.** The [P12 record](qualification/native-surface.md) captures real air/water pixels, an obscured fish, a scoped transition repair, and missing owner comfort judgment and matrix cells. The [native P7 record](qualification/native-pond.md) retains near-field darkness and incomplete deep/return traversal. Ocean Base World `ocean-m4-42` and Pond Base World `pond-p4-71` retain their tracked identities. Earlier P3/C3 gaps remain explicit in the [P3 direct closeout](../p3/closeout.md). [P11 implementation evidence](qualification/implementation.md) records headless checks and native limits.

[P8 intake evidence](qualification/integration.md) records the 25 fish stages, three creature bases, four insect variants, runtime wiring, facing correction, bounded native observations and remaining visual limits. P11 source evidence covers generated shore geometry and bounded surface effects. P12 still needs a passing native waterline/camera result and explicit owner review before the runner advances.

[Assessment](prompt-assessment.md) · [Implementation plan](implementation-plan.md) · [Owner Pond FAF integration inventory](pond-faf-integration.md) · [P4 design](../../planning/p4/freshwater-world.md) · [Roadmap](../../roadmap.md) · [Runner contract](../../phase-runner.md) · [Validation V28–V32](../../validation.md)

## Goal and preservation

Offer two independently playable worlds, **Ocean** (preserve the existing reef, five marine FAF fish, art, generated/baked world, water effects) and **Freshwater Pond** (original olive-green/muddy, overgrown pond). **New Life → World Select → habitat-filtered Character Select → Play.** Species and ambient population are restricted to their native habitat by one fail-closed policy. Both share the deterministic compiler/baked loader, Forward+ renderer, FishController/SwimMotor, camera/control presets and FAF infrastructure.

The Pond includes **surface swimming and camera air–water crossing**: fish can swim immediately under the surface and partially breach with fins/mouth/back without ordinary airborne movement, while the follow/orbit/zoom camera passes above/below without snaps or visibility bugs. It transitions fog/distortion/exposure, shows genuine nearby generated banks/reeds/sky and supports cheap surface ripples/wake. The Ocean is not restyled or deleted.

## S21 Pond clarity review routing (documentation-only)

The owner selected world-space, nonuniform Pond clarity on 2026-11-11 after a uniform green underwater view. [The water-visibility contract](../../systems/underwater-visibility.md) owns spatial turbidity versus illumination and initial near-field visual targets; the [P4 design](../../planning/p4/freshwater-world.md) owns Pond habitat appearance and [V29/V32](../../validation.md) own native checks. Introduce the smallest bounded rendering correction **without rewriting already executed P4 prompts or P12-R1/R2 evidence**, preserving the owner-rejected pale/shiny trials, accepted dark muddy-green direction and Ocean. Compare a fixed-view prototype before wider deterministic world-field wiring; P5 later owns gameplay sensing and dynamic sediment. No prompt-stack execution, checkpoint, version change, or Gate PASS follows merely from this documentation update. P12 remains NOT GREEN pending owner-controlled native qualification.

## Day/night routing (documentation-only)

The owner selected a staged environment pass on 2026-11-12 after reflective Pond water exposed the static flat sky as the next visual bottleneck. [c4-day-night](../c4-day-night/README.md) now owns the shared continuous solar clock, procedural sky and above/below-water daylight integration at unchanged `0.4.11`. Weather is explicitly deferred until that baseline is stable. This does not rewrite executed P11/P12 evidence or make P12/P4 GREEN.

## S23 surface reference experiment (P12-R3)

The owner requested a checkpoint and reference-driven surface improvement on 2026-11-12. [R3 evidence](qualification/r3-surface-reflections.md) records rollback branch `checkpoint/water-surface-2026-11-12` at `cc8cbb9`, the isolated Pond surface/sky candidate and diagnostic limits. [The native handoff](manual/R3-surface-reflections-native.txt) uses `./tools/dev review-pond-reflections` to capture matching actual-Pond views, then runs the affected checks and owner-play matrix. Full pinned .NET integration and native cost are unobserved; `0.4.11`, P12/V32 and runner progression remain unchanged. S21 water clarity remains separate.

## Non-negotiable entry dependency

1. C3 manual M1–M4 completed; the owner approved progression with remaining visual/native gaps in the [P3 closeout](../p3/closeout.md). Preserve that disposition rather than converting previous failures to PASS or silently waiving V25/V26/V27.
2. The corrected Ocean Base World was promoted under new `ocean-m4-42` without overwriting `qualification-42`; normal baked loading and five-fish Character Select passed focused checks. Complete fish-controlled native exploration remains an accepted progression gap.
3. The deliberate root baseline is `0.4.0` after reconciled C3 source, owner-closed P3 handoff and clean phase-entry commit. Recheck branch/HEAD, package manifest, scene/world identity and working tree before the runner starts.
4. Preserve the Ocean identity and package during P4. Do not create an artificial P3 P7/P8 or P4 checkpoint.
5. Before running inspect the phase runner's exact command composition, Node/Codex model availability and selected Fish MCP project identity. Avoid concurrent P3/c3/P4 runners.

## Prompt order

| Task | File | Outcome | Version | Tier / handoff |
| --- | --- | --- | --- | --- |
| P1 | [World definitions + baked loader](P1-world-definitions-and-baked-loader.txt) | Independent Ocean/Pond manifests, world identity and safe baked loading | `0.4.1` | T2 |
| P2 | [World Select navigation](P2-world-select-and-navigation.txt) | World Select first, back/return routing and Ocean startup preserved | `0.4.2` | T2 |
| P3 | [Habitat species compatibility](P3-habitat-species-enforcement.txt) | One fail-closed character/ambient eligibility policy | `0.4.3` | T2 |
| P4 | [Pond generation and bake](P4-pond-generation-and-bake.txt) | Freshwater compiler settings, irregular banked depth and baked Pond | `0.4.4` | T2 |
| P5 | [Pond lighting and atmosphere](P5-pond-atmosphere-and-water.txt) | Independently tunable olive-green depth/fog/overcast water | `0.4.5` | T2 |
| P6 | [Pond environment FAF integration](P6-pond-environment-faf-kit.txt) | Audit/import **existing** wood, hollow trunks, foliage and ground FAF pack; place deterministically with honest clearance; **no model generation** | `0.4.6` | T2 |
| P7 | [Native pond visual review](P7-native-pond-visual-review.txt) | **Manual** playtest of terrain, atmosphere, route, style and Ocean regression | `0.4.7` | T3, pause |
| P8 | [Freshwater FAF creature integration](P8-freshwater-fish-and-wildlife.txt) | Import **existing** 5 Pond species × 5 life stages (25 exports), frog/snake/turtle and two insect families; **no model generation** | `0.4.8` | T2 |
| P9 | [Surface swim constraints](P9-surface-swimming-boundary.txt) | Fish swims at/barely through waterline without invisible ceiling or launch | `0.4.9` | T2 |
| P10 | [Camera and immersion crossing](P10-camera-air-water-immersion.txt) | Smooth air–water camera/orbit/zoom and independently blended presentation | `0.4.10` | T2 |
| P11 | [Surface view and water reaction](P11-surface-shore-and-reactions.txt) | Generated nearby banks/sky/cattails and bounded ripples/wake; historical static overcast only, with real day/night/weather superseded by the staged environment corrections | `0.4.11` | T2 |
| P12 | [Native surface qualification](P12-native-surface-camera-review.txt) | **Manual** V32 waterline, controls, camera, visual and performance test | `0.4.12` | T3, pause |
| P13 | [Cross-world integration](P13-two-world-regression-and-performance.txt) | V28–V31 focused matrix, asset/perf checks, save-boundary invariants | `0.4.13` | T2 |
| P14 | [Phase 4 closeout](P14-freshwater-world-closeout.txt) | **Manual final** G4.1–G4.4 / V28–V32 evidence disposition | `0.4.14` | T3 review, pause |

**Runner edit safety:** This prompt revision is prepared on a separate Git branch while P4/P3 may be executing. Do not apply/merge it into the active checkout or edit the runner's snapshotted prompts until the owner has stopped the runner after its P3 checkpoint. Then reconcile the exact P1–P3 completed prefix and dirty tree, merge/review these P4 prompt changes, re-run `npm run fish:phase:validate -- p4`, and only then resume. Do not rewrite finished P1–P3 checkpoint history or fabricate later versions.

P7 and P12 are **human/native visual checkpoints** (the runner's `Browser required: yes.` means stop **before** executing). P14 is the sole final closeout and also requires owner review. A stopped prompt is not executed or checkpointed. Keep the stack frozen while the runner is active.

## Deliverable and test scope

- G4.1: two selection cards; correct baked world identity, back/return/focus paths, missing-resource errors and repeated world loading.
- G4.2: convincing shallow/middle/deep Pond, terrain/rock/tree support, visible shore/sky, no bogus collisions, murky-yet-readable Forward+ atmosphere, controllable fish/camera at the waterline and actual native performance.
- G4.3: integrate the owner's full [Pond FAF inventory](pond-faf-integration.md): five Bluegill/Bass/Catfish/Carp/Pike species, 25 separately authored stages, frog/water-snake/snapping-turtle bases, submerged larva and water-bug variants, wood/hollow trunks, foliage and ground dressing. Verify actual existing Blender/GLB/rig/provenance and native placement without producing duplicate assets. Missing required inputs block the relevant checkpoint and must be named.
- G4.4: no cross-biome selectable fish/ambient spawns, including invalid identifiers and reloads. Existing Ocean fish and art remain intact; shared controls/camera are unchanged.

Use P4 [planning contract](../../planning/p4/freshwater-world.md) for biome design and the [owner FAF inventory](pond-faf-integration.md) for required asset identities/counts, which supersedes the earlier illustrative species shortlist. P6 and P8 are **integration-only**; independent FAF production owns all model/GLB creation. Do not re-export, rename, regenerate or replace owner-produced models to satisfy a missing input. Reconcile input availability at execution time. The pending owner-supplied dark-day Pond image/reference pack is a candidate aesthetic target; check actual file availability and never substitute an unavailable image with an invented file.

## Evidence and execution

Only write `qualification/implementation.md`, `qualification/native-pond.md`, `qualification/native-surface.md`, `qualification/integration.md` and `closeout.md` **after actual work/observation**. Record source/dirty identity, seed and bake hash, Godot/OS/settings, actions, screen evidence, timing/performance, errors, PASS/FAIL/NOT OBSERVED/BLOCKED and owner judgment separately. The V32 test is an actual camera crossing in a native game, not a shader screenshot or headless simulation.

From verified P4 entry at root `0.4.0`:

```bash
npm run fish:phase:validate -- p4
npm run fish:phase -- p4
```

Validation checks prompt grammar/versions, not entry readiness or gameplay. The runner owns implementation checkpoint commits; only actual executed work earns a `0.4.x` checkpoint. P5 is feeding/predators/growth, P6 persistence, P7 history and P8 integration; none is hidden P4 work.
