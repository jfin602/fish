# C3 world generation correction closeout — 2026-11-11

**Disposition: FAIL / not accepted at root `0.3.6`.** P1–P6 built a deterministic seed-42 compiler and a fast, generator-free baked loader. [P7 native evidence](qualification/native-world-gen.md) shows that the present world still fails the required natural visual reading, and seeds 43/44 cannot complete full generation because route goals are blocked. Human owner exploration/acceptance and complete swim-through qualification remain open. No P3 Gate is marked GREEN. This closeout is left uncommitted for human review.

Subsequent manual [R1/M1 shore and depth repair](qualification/m1-shore-depth.md) and [R1/M2 geology/blending repair](qualification/m2-geology-blending.md) are separate progression candidates. M2's final seed-42 package compiled in 58.273 s, loaded without generation, and passed direct passage clearance. Its Forward+ review shows improved connected geology, with sparse accent coverage and no measured exposed-shell percentage. The old route graph now blocks the `stone-arch:link-out` camera connector; M3 is clear to repair that graph and establish multi-seed full-world reliability. This P7 disposition remains the historical result until repeat qualification and owner review.

The later [R1/M3 generated-route evidence](qualification/m3-routes.md) records terrain-derived routes and valid baked collision paths for seeds 42/43/44. Seed 44's final compile logs a Godot/.NET finalizer error even though the package is hash-valid and loads. Native route review remains open. The M2 route blocker above is historical, not the present route state; this P7 disposition remains failed until repeat qualification.

The subsequent [R1/M4 dense-habitat evidence](qualification/m4-habitats.md) records 7,960 seed-42 placements across 12 habitat roles, clean full seed-42/43/44 bakes in 9.695/10.658/9.555 s, generator-free baked reloads, and native Forward+ depth/habitat screenshots. The prior seed-44 finalizer error did not recur in these M4 bakes. M4 improves broad shallow vegetation and depth contrast but leaves exposed dark geology in one ordinary shallow view and conspicuous angular cave/shell transitions. The M4 candidates are separate from the provisional Base World. Human owner acceptance, full fish-controlled native route/perimeter review, and formal P7 rerun are still open. This historical P7 disposition is not silently converted to GREEN.

## 2026-11-11 progression addendum

The owner approved moving to P4 with the M4 visual and native-review gaps retained. This is an **owner-closed for progression / not GREEN** C3 disposition; the failed P7 matrix below remains historical evidence and V26/V27 are not relabeled PASS. The M4 seed-42 candidate was promoted under the new, no-overwrite `ocean-m4-42` Base World name: seed `42`, settings `1`, generator `c3-full-12`, world ID `world-6cba9a67e6275297`, content SHA-256 `ba397da2b4ff2d2982898a3c3519b0e5cd0443222e9d31650197d8bcb10e3879`. The prior `qualification-42` package remains intact. A clean source build, normal-play smoke and five-fish Character Select check loaded the new package with `generator_invocations=0`; those checks do not fill the missing hand-controlled passage/perimeter, sustained performance or V25 observations. [P3 direct closeout](../p3/closeout.md) maps the Gate consequences.

## V26 and V27

| Scenario | Result | Evidence and unresolved part |
| --- | --- | --- |
| **V26 — deterministic compilation, generated topology/routes, baked reload** | **FAIL overall** | Fresh seed 42 matched the earlier world ID, content summary and six resource hashes; basin same-seed and different-seed geometry checks pass. The seed-42 baked loader reported `generator_invocations=0`. Headless P1–P6 checks cover generated mesh/collision and protected seed-42 routes. Full seeds 43 and 44 fail blocked route goals, so meaningful variation of a complete baked world was **not established**. Native natural reading and complete route traversal are also unqualified. |
| **V27 — real-machine time, native world quality and promotion** | **FAIL overall** | Seed-42 compile/bake took 51.687 s against the ≤120 s target; baked loader took 149 ms, materially faster. Explicit disposable promotion succeeded and overwrite was refused. Four bank directions, five geology centers and multiple depth zones were rendered in Forward+; the visual world remains artificial/dark/sparse. Complete fish-controlled exploration, comfort and explicit human owner acceptance were not obtained. |

## Correction criterion matrix

| Required criterion | Status | Basis |
| --- | --- | --- |
| Same seed/settings/version reproduces world identity and content | **PASS** | Fresh seed-42 world ID/content/resource hashes match P6 candidate and provisional Base World. |
| Different seed produces meaningful full-world variation | **FAIL** | Basin variation passes; seed 43 `middle-deep` and seed 44 `middle-shallow` route repair fail before bake. |
| Irregular basin, central useful deepest region and general outward rise | **PASS** | Dense seed-42 basin check: core `(4.5, 2.0)`, `-12.59 m`; varying radial crossings. This does not qualify the visual perimeter. |
| Perimeter appears natural, neither square nor perfect circle | **FAIL** | Native directional views still read as steep wall/slab forms despite non-circular numerical field. |
| No reachable/visible rectangular fail-safe wall defines normal play | **NOT OBSERVED** | Four marker views did not show the hidden box, but full fish-controlled boundary approaches were not completed. |
| Continuous open terrain with true cave/two tunnel/arch/canyon floor/wall/ceiling mesh and collision | **PASS** | Five generated baked mesh/collision regions and prior focused clearance checks; P7 images confirm enclosed forms and open canyon. |
| Geological forms read naturally and connect to basin | **FAIL** | Native cave/tunnel/arch views show hard-edged dark tubes, abrupt seams and weak surrounding terrain transition. |
| Comfortable fish/camera clearance through every cave and route | **NOT OBSERVED** | Prior headless probes pass seed-42 points; full native swim-throughs and camera recovery were not completed. East far mouth is already an out-and-back branch. |
| Routes generated/protected before decoration | **PASS** | P4/P5 source checks: 27 nodes, 32 edges, route exclusion for seed 42; seed-43/44 goal defect remains separate. |
| Terrain-aware cluster/spacing instead of random XYZ/grid stamping | **FAIL** | P5 analysis/placement/support checks pass in source, but broad native views show sparse accents on bare/dark forms rather than convincing caused reef clusters. |
| Shallow/high lusher than lower/deep while retaining open water | **FAIL** visually | Structural living-depth counts favor shallow/upper, but native shallow and middle screenshots do not read lush; open water exists. |
| Unsupported/intersecting/route-blocking candidates repaired or rejected | **NOT OBSERVED** visually | P5 metrics report zero unresolved failures and five repairs for seed 42; specific large-rock/terrain contact and complete route clearance were not verified by normal play. |
| Cave mouths, routes and landmarks remain readable | **FAIL** | Entrances are visually overwhelmed by dark forms; complete route use is unobserved. |
| ≤120 s complete generation on identified machine | **PASS** | 51.687 s for the qualification seed on the recorded Ryzen/Radeon desktop; other seeds fail before timing completion. |
| Baked load avoids generator and is materially faster | **PASS** | 149 ms loader time, zero generation invocations/stages versus 51.687 s compile. |
| Explicit, reviewable Base World promotion without overwrite or source duplication | **PASS** | `p7-disposable` package byte-matches candidate; second promotion refused; seven baked/data files, no copied FAF source payload. |
| FAF art, Forward+ water/light remain coherent | **FAIL** | Runtime is Forward+ with FAF resources and effects active, but the visual composition fails target style. |
| Selected player, Character Select and movement/camera authority unchanged | **PASS** | Native Yellow Tang selection and short swim; all five headless select/return checks pass; P7 changed no gameplay authority. Full camera-feel matrix is **NOT OBSERVED**. |
| Human owner exploration and explicit acceptance | **BLOCKED** | Agent visual rejection is recorded; no human owner decision is attributed. |

## Gate and sequencing consequence

G3.1–G3.3 stay **FAIL / not GREEN** under the [roadmap](../../roadmap.md). V26/V27 do not supersede the historical P2 V18/V19 passes or erase the owner-accepted G2.3/V20 gap. The seed-42 provisional Base World remains a qualification artifact, not an accepted shipping world. Do not run c3-visibility against this candidate as if world generation were accepted; repair the blocked route goals and natural visual composition, then repeat native passage, perimeter and owner review.
