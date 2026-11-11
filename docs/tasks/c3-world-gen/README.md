# c3-world-gen — deterministic procedural reef world correction

**State:** P1 IMPLEMENTED / P2–P7 NOT EXECUTED / NOT QUALIFIED.
**Type:** Correction 3.  
**Authoring baseline:** GitHub main at `4c9b601df7c8aa97f983b02bea216aee13af2748`, root `0.3.0`.  
**Execution version:** every prompt requires **unchanged `0.3.6`**, the owner-selected P3 P6 checkpoint. This is a strict preflight for the intended local execution point, not an instruction to bump/downgrade GitHub main or fabricate P3 checkpoints.

## Purpose

Replace the artificial P3 world-construction architecture with the owner-approved [procedural world compiler](../../planning/p3/procedural-world-generation.md).

The correction fixes the observed structural problems:

- ground/terrain pieces floating or reading as patches laid on another floor;
- uniformly authored/random-looking placement;
- world walls/perimeter that do not resemble a natural pond/reef basin;
- square/circular/terraced world language;
- caves/tunnels assembled from placed props instead of actual geological terrain;
- no deterministic compile/bake/load workflow for preserving good worlds.

The correction keeps Fish's bright stylized low-poly art, FAF asset library, Forward+ water/light/depth systems, accepted movement/camera/controls and Character Select.

## Conflict-safe entry

The owner's active local P3 state is not visible from GitHub. Before execution:

1. confirm the P3 runner's Git-proven completed prefix through P6;
2. confirm root `package.json` is exactly `0.3.6`;
3. inspect the real current terrain/world source and preserve compatible P3 work;
4. do not run concurrently with the P3 runner or c3-visibility;
5. never downgrade/reset local work to satisfy this stack.

If root is not exactly `0.3.6`, **stop and reconcile actual P3 progress**.

The planned order was **c3-world-gen before c3-visibility**. Actual local history already contains c3-visibility P1–P3; preserve those commits and revisit visibility against the generated/baked world rather than obsolete placed geometry.

## Prompt order

| Step | Prompt | Outcome | Tier / manual |
| --- | --- | --- | --- |
| P1 | [World compiler and baked contract](P1-world-compiler-and-bake-contract.txt) | Deterministic seed/settings pipeline, manifest, candidate/baked load boundary | T2 |
| P2 | [Irregular basin and natural perimeter](P2-irregular-basin-and-perimeter.txt) | Continuous non-circular basin, central deep region, natural terrain boundary | T2 |
| P3 | [True 3D geological regions](P3-geological-caves-and-tunnels.txt) | Generated ridge/cave/tunnel/arch/overhang mesh + collision | T2 |
| P4 | [Routes and environmental analysis](P4-routes-and-analysis-fields.txt) | Protected 3D route graph and terrain-derived placement fields | T2 |
| P5 | [Natural FAF placement and repair](P5-natural-placement-and-relaxation.txt) | Cluster/suitability/blue-noise placement, support checks, repair/scoring | T2 |
| P6 | [Bake/load and Base World workflow](P6-bake-load-base-worlds.txt) | Full compile command, ≤120s target evidence, baked reload and promotion workflow | T2/T3 preparation |
| P7 | [Native generated-world closeout](P7-native-world-gen-closeout.txt) | Owner play/visual/timing qualification and correction closeout | T3, mandatory manual final closeout |

P7 is the sole final closeout.

## Acceptance boundary

The correction is GREEN only with applicable evidence that:

- same seed/version/settings reproduces the same compiled world;
- the world is an irregular basin with its useful deepest region near the center and terrain generally rising outward;
- normal play does not perceive a rectangular wall or perfect circular rim;
- open terrain is continuous and cave/tunnel/overhang geometry has actual floor/walls/ceilings;
- terrain/rock features are supported/embedded rather than visibly floating;
- routes are generated/protected before decoration;
- placement is terrain-aware and cluster-driven rather than random XYZ/grid stamping;
- shallow/high regions are denser/lusher than deep regions by generated metrics and native inspection;
- invalid support/intersection/route-blocking candidates are repaired/rejected;
- generation is deterministic and targets **≤120 seconds** on the identified current development machine;
- a baked world reloads without running the generator and is materially faster than generation;
- a candidate can be explicitly promoted to a reviewable curated Base World package;
- existing fish movement/camera/controls, Character Select and Forward+ presentation remain authoritative.

The 120-second target is not pre-approved evidence. Measure it.

## Evidence

Execution records:

- [implementation evidence](qualification/implementation.md) — P1 source, commands, deterministic checks and fixture phase timings; later P2–P6 results belong here when executed;
- `qualification/native-world-gen.md` — P7 native world inspection/play, generation/load measurements and owner judgment;
- `closeout.md` — V26/V27 and correction disposition.

## Commands

After confirming actual local P3 P6 / `0.3.6` state:

```bash
npm run fish:phase:validate -- c3-world-gen
npm run fish:phase -- c3-world-gen
```

Writing this correction does not execute it. The runner owns implementation checkpoint commits; P7 remains reviewable.
