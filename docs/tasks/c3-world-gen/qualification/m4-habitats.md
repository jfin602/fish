# C3 R1/M4 dense habitats — 2026-11-11

**State: owner-reviewable qualification candidate; visual acceptance open.** The manual M4 implementation replaces the sparse per-variant quota composition with depth-conditioned habitat patches and area-scaled fill. Three new `c3-full-12` candidates compile cleanly and reload without generation. Do not promote a Base World, mark C3 GREEN, or run c3-visibility from this evidence. The historical P7 failure remains in force until owner review and a formal rerun.

## Implementation and dimensions

Habitat roles are selected for cached 4 m terrain cells using 12 m shared patch choices. Each cell's actual water depth, slope/normal, shore distance, route distance, and cave-entrance distance can override the broad patch. This makes the habitat cover continuous areas while honoring ledges and route clearances. Local family density varies by role and depth with stable clustered rolls, gaps, and sandy lanes. Structural terrain and rocks retain support probes and collision; repeated coral, plants, and details use shared mesh resources and 28 non-colliding MultiMesh batches. The full FAF dressing has **54 estimated draw batches**, including 26 structural solids, for **7,960** seed-42 placements. All **38/38** environment variants appear. Decorative transforms are saved in the hash-checked data and restored on baked load because headless scene packing did not preserve their MultiMesh transform buffers. The generator identity is `c3-full-12`; older package compatibility is retained where supported.

The inherited M1 seed-42 shoreline spans **253.1 × 305.7 m**, with about **0.7 m** shore water and **39.46 m** deepest water. The generated placement domain covers **52,448 m²** of 4 m cells. Seeds 43/44 reach **41.49/38.44 m** deepest water. M2 geological bodies and M3 31-node/37-edge routes remain in the package. M4 adds asymmetric cave-mouth vegetation, coral, and detail accents while reserving route clearance.

### Seed-42 habitat coverage

Patch count means distinct 12 m block/role combinations, not connected components. Area is the sum of accepted 4 m cells; placement counts include the 26 structural accents.

| Habitat | Patches | Area m² | Placements |
| --- | ---: | ---: | ---: |
| Grass meadow | 83 | 6,992 | 2,276 |
| Branching reef | 60 | 5,792 | 1,269 |
| Coral garden | 99 | 8,624 | 1,844 |
| Rocky shelf | 42 | 3,712 | 634 |
| Sandy clearing | 56 | 4,944 | 327 |
| Cave entrance | 31 | 1,056 | 448 |
| Rocky slope | 84 | 7,744 | 711 |
| Drop-off reef | 67 | 4,288 | 240 |
| Deep rubble | 33 | 3,232 | 133 |
| Deep rock field | 12 | 1,072 | 22 |
| Deep sparse fan | 11 | 1,136 | 33 |
| Open-water corridor | 56 | 3,856 | 23 |

Seed 42 has **3,321 plants, 1,918 corals, 2,695 details, 20 rocks, and 6 terrain accents**. A representative ~15 m radius shallow garden near `(71,130)` has **241 decorations**: 134 plants, 50 corals, 57 details. The source estimate is **2,487,760** dressing triangles before actual frustum culling; the native renderer's visible triangle counters below are point samples.

| Actual water depth | Habitat area m² | Placements | Living plants + coral | Living / 100 m² |
| --- | ---: | ---: | ---: | ---: |
| 0.5–2 m | 4,752 | 809 | 592 | 12.46 |
| 2–6 m | 13,808 | 3,565 | 2,621 | 18.98 |
| 6–15 m | 20,576 | 2,964 | 1,829 | 8.89 |
| 15–30 m | 12,112 | 563 | 187 | 1.54 |
| 30–45 m | 2,704 | 59 | 10 | 0.37 |

These area values come from the baked environment grid; placement depth uses each actual surface hit. The difference between area-binned counts above and the generator's placement-only bands is expected where a placed instance crosses a cell's depth boundary. The shallow reef's measured living density is about **12.3×** the 15–30 m band and **51×** the trench band. Ambient life remains 31 fish in nine schools, distributed 13/8/6/2/2 from shallow to deep.

## Qualification packages and timing

All times below are native-machine headless Godot 4.7.2 .NET runs on the Linux/X11 Ryzen/Radeon development desktop. They are individual runs, not benchmark averages. Full time includes data and scene serialization and is far below the **120 s** target. Manifest timings are retained in each linked candidate.

| Seed | Candidate, world ID | Full compile/bake | Habitat plan / dense fill / dressing install | Data / scene serialization | Baked load | Placements |
| --- | --- | ---: | ---: | ---: | ---: | ---: |
| 42 | [m4-qualified-42](../../../../artifacts/world-data/godot/app_userdata/Fish/world-candidates/m4-qualified-42/manifest.json), `world-6cba9a67e6275297` | **9.695 s** | 6 / 68 / 261 ms | 130 / 265 ms | 870 ms | 7,960 |
| 43 | [m4-qualified-43](../../../../artifacts/world-data/godot/app_userdata/Fish/world-candidates/m4-qualified-43/manifest.json), `world-24b82bb2f42a4e3d` | **10.658 s** | 6 / 69 / 258 ms | 84 / 277 ms | 878 ms | 7,851 |
| 44 | [m4-qualified-44](../../../../artifacts/world-data/godot/app_userdata/Fish/world-candidates/m4-qualified-44/manifest.json), `world-f9dba59ff38f04e6` | **9.555 s** | 6 / 65 / 268 ms | 80 / 274 ms | 868 ms | 7,819 |

The other seed-42 manifest stages are palette/player 218 ms, basin mesh/collision 1,134 ms, legacy source preparation 2,431 ms, geology mesh/collision 20 ms, route repair 4,221 ms, environment analysis 383 ms, and ambient life 544 ms. Route repair is the largest variable stage. The full seed-42 content hash is `ba397da2b4ff2d2982898a3c3519b0e5cd0443222e9d31650197d8bcb10e3879`. A prior repeated seed-42 bake had this same world ID and content hash. The three seeds have different content hashes; each hash-verified baked load reported `generator_invocations=0`.

`tools/dev build` passed with zero warnings/errors. `tools/dev inspect-placement` passed seeds 42/43/44 for deterministic same-seed output, different-seed output, all variants, support/route clearance, non-grid positions and 26 supported structural solids. Full-bake placement reports zero remaining support failures, zero protected-route conflicts, and zero structural intersections, with minimum structural spacing **4.28/0.97/1.26 m** for seeds 42/43/44. The baked M3 route checker passed the hash-identical review packages for all three seeds: 31 nodes, 37 edges, five geological passages, connected graph and 3,855/4,859/3,787 fish/camera collision samples. `tools/dev smoke-baked-candidate m4-qualified-42` passed after correcting the smoke check to compare the fish's spawn to the generated Spawn marker rather than the old fixed world-height interval. `tools/dev smoke-world` also passed against the untouched provisional default Base World.

## Native Forward+ screenshot review

The [native capture set](../../../../artifacts/c3-m4-native/review-42-v2/) was taken from `m4-review-42-v2`. That candidate's world ID and full content hash match `m4-qualified-42`; the later code edits only added measured score fields, patch counts, timing separation, and corrected smoke validation. All captures are 1920 × 1200, native Forward+, with the baked candidate loaded. They are diagnostic camera placements, **not** a fish-controlled full-route playthrough.

| Required view | Evidence |
| --- | --- |
| Shore / very shallow | [shore-shallow.png](../../../../artifacts/c3-m4-native/review-42-v2/shore-shallow.png) |
| Shallow garden | [shallow-garden.png](../../../../artifacts/c3-m4-native/review-42-v2/shallow-garden.png), [shallow-reef.png](../../../../artifacts/c3-m4-native/review-42-v2/shallow-reef.png) |
| Shallow to middle | [shallow-middle-transition.png](../../../../artifacts/c3-m4-native/review-42-v2/shallow-middle-transition.png) |
| Middle reef/geology | [middle.png](../../../../artifacts/c3-m4-native/review-42-v2/middle.png) |
| Cave entrance / interior / exit | [cave-chamber-far.png](../../../../artifacts/c3-m4-native/review-42-v2/cave-chamber-far.png), [cave-chamber-medium.png](../../../../artifacts/c3-m4-native/review-42-v2/cave-chamber-medium.png), [cave-chamber-entrance.png](../../../../artifacts/c3-m4-native/review-42-v2/cave-chamber-entrance.png), [cave-chamber-inside.png](../../../../artifacts/c3-m4-native/review-42-v2/cave-chamber-inside.png), [cave-chamber-reverse.png](../../../../artifacts/c3-m4-native/review-42-v2/cave-chamber-reverse.png). Each of the other four passages has the same five-view sequence. |
| Deep / trench | [deep-basin.png](../../../../artifacts/c3-m4-native/review-42-v2/deep-basin.png), [trench.png](../../../../artifacts/c3-m4-native/review-42-v2/trench.png) |
| Natural perimeter approaches | [perimeter-east.png](../../../../artifacts/c3-m4-native/review-42-v2/perimeter-east.png), [perimeter-west.png](../../../../artifacts/c3-m4-native/review-42-v2/perimeter-west.png), [perimeter-north.png](../../../../artifacts/c3-m4-native/review-42-v2/perimeter-north.png), [perimeter-south.png](../../../../artifacts/c3-m4-native/review-42-v2/perimeter-south.png) |
| Wide foreground/midground/background | [wide-layering.png](../../../../artifacts/c3-m4-native/review-42-v2/wide-layering.png) |

At the shallow garden, transition, and wide views, native point counters reported **60 FPS**, **138/230/403 draw calls**, **290,280/297,762/303,618 visible primitives**, and **227/350/530 rendered objects** respectively. This is a short camera sample, not a sustained frame-time or traversal benchmark. The images show colorful grass/coral masses, open lanes, a near surface in shallows, and visibly sparse/deeper darker water. Some shallow perspectives remain rock dominated, and the cave cut/shell is still easy to identify. The extra asymmetric mouth accents help locally but do not fully blend the large structural form.

## Owner review checklist and remaining gap

| Criterion | Current judgment |
| --- | --- |
| Larger irregular body, near/far surface contrast, five depth bands | **PASS for this candidate's measured shape and captured views**; owner feel review open. |
| Thousands of dense decorations, all FAF variants, clear shallow/deep density gradient | **PASS** by baked metrics and native samples. |
| Routes, mouth clearance, supported solids, clean seeds 42/43/44, ≤120 s compile and generator-free load | **PASS** for automated/baked checks. Full player-controlled swim remains open. |
| Shallow banks immediately lush and bright, without a giant naked dark slab in ordinary views | **PARTIAL / FAIL owner checkpoint**: garden views improve substantially, but [shallow-reef.png](../../../../artifacts/c3-m4-native/review-42-v2/shallow-reef.png) still emphasizes an exposed dark geological face. |
| Structural tunnel geometry visually recedes into natural geology/habitat | **FAIL owner checkpoint**: entrances retain angular cuts and visibly isolated shell portions despite accent blending. |
| Performance throughout normal exploration and native camera comfort | **NOT YET OBSERVED**: point counters and collision sweeps do not prove sustained traversal or player feel. |
| Explicit owner acceptance and formal P7 rerun | **OPEN**. Do not call C3 GREEN or tune c3-visibility. |

The candidate is ready for owner inspection, but visual failures remain before formal P7 can be accepted. Keep the old provisional Base World untouched; the new candidates are separate ignored qualification artifacts.
