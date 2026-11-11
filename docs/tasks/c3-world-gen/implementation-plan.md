# c3-world-gen implementation plan

**Execution version:** Correction 3, required unchanged `0.3.6`.  
**Ordering:** after actual P3 P1–P6 checkpoint; before c3-visibility and before P3 P7 native closeout.

## Entry and source trace

Read `AGENTS.md`, `BOOT.md`, workflow/runner docs, this stack, [procedural-world plan](../../planning/p3/procedural-world-generation.md), P3 reef plan, FAF art/asset records, current P3 implementation evidence, `MODULES.md`, current terrain/world source and `tools/dev`.

Before editing:

- inspect branch/HEAD/dirty work;
- verify root is exactly `0.3.6`;
- prove P3 P1–P6 are actually completed rather than inferring from the number;
- inspect actual current generated/placed world source because GitHub's authoring baseline may lag local P3 work;
- preserve compatible Character Select/ambient/world-art work;
- do not run c3-visibility concurrently.

## P1 — world compiler and baked contract

Create the smallest explicit generation boundary capable of compiling a finite world deterministically.

Likely responsibilities (names may adapt to source):

- generation settings/schema;
- seed + generator version;
- compile context with deterministic subsystem PRNG streams;
- generated world manifest/summary;
- stage progress/timing;
- candidate output location;
- baked-world loader that does not invoke generation.

Candidate output belongs in `user://`, ignored artifacts, or another existing non-tracked development location. Curated Base Worlds get a separate tracked destination only through explicit promotion.

Manifest includes seed, generator/settings versions, asset identities, resource paths, content summary/hashes and timing.

Add a minimal developer command/check to compile a tiny/fixture world twice and prove deterministic summaries/resources. Do not build final terrain in P1.

## P2 — irregular basin and natural perimeter

Replace the square/terrace mental model with a deterministic basin domain.

Generate:

- irregular, non-circular perimeter radius/domain;
- useful deepest area in the central region;
- broad outward rise toward shallow perimeter;
- large low-frequency folds/ridges/channels;
- continuous open seabed mesh and collision;
- a natural terrain boundary that rises/leaves usable water.

Hidden safety containment may remain, but move/shape it outside the natural boundary and verify normal routes cannot perceive/reach it.

Provide surface sampling for open seabed and record normal/depth/perimeter metrics.

Focused checks should sample a dense grid and prove:

- finite continuous mesh/collision;
- central region contains the useful minimum/deep core;
- perimeter is materially higher on average;
- perimeter radius varies by angle (not a perfect circle);
- no square four-wall geometry is the ordinary world boundary;
- same seed/settings reproduce identical basin metrics.

## P3 — true 3D geological regions

Add deterministic arbitrary-mesh geological features that can have multiple surfaces at the same X/Z.

Use a bounded generated representation suitable for baking (for example spline/lofted ridge/cave shells or generation-time CSG converted to mesh). Do not introduce a voxel sandbox.

Generate at least:

- one cave chamber;
- two tunnel/swim-through structures with distinct geometry;
- one arch/overhang;
- one canyon/ridge transition.

Geology must integrate into/overlap the basin with broad transition skirts/masses, not float above it.

Create matching simplified or generated collision and actual route-clearance samples. Cave/tunnel checks must prove floor, side walls and ceiling exist and that a third-person fish/camera route passes through without relying on placed roof props.

## P4 — route graph and environmental analysis

Generate playable structure before decorative dressing.

Build a deterministic route graph with nodes/edges for:

- spawn;
- shallow/middle/deep traversal;
- return/alternate route;
- caves/tunnels;
- open-water corridors;
- landmarks.

Attach protected clearance/exclusion volumes or fields.

Then derive environment analysis grids/queries from actual final terrain/geology:

- depth/elevation;
- slope/normal;
- curvature/concavity approximation;
- perimeter/deep-core distance;
- route distance;
- cave/entrance distance;
- exposure/shelter;
- large-geology proximity.

Arbitrary terrain queries use actual mesh projection/raycast where one-height sampling is insufficient.

Checks prove route connectivity, clearance and deterministic field values.

## P5 — natural FAF placement, relaxation and scoring

Replace hard-coded/random XYZ placement with terrain-caused composition.

1. Generate deterministic habitat zones/cluster templates.
2. Score candidate locations from environment fields.
3. Use deterministic blue-noise/Poisson-like spacing inside suitable regions.
4. Project each placement to actual terrain and orient to the relevant surface normal.
5. Preserve route/camera exclusions.
6. Run support tests for large rocks/terrain accents using multiple footprint samples.
7. Embed/partially sink geological accents where appropriate; reject unsupported/floating features.
8. Place coral/plants/details by family suitability and cluster role.
9. Run bounded relaxation/repair for intersections, repetition, support, density spikes and blocked routes.
10. Produce world score/diagnostics.

FAF terrain A–F are localized embedded features only; the generated basin/geology is the ground.

Maintain the shallow/high -> deep/lower living-density trend. Keep every required FAF variant meaningful without checklist clutter.

Focused checks compare several seeds and prove deterministic placements, route preservation, zero unresolved support failures, bounded intersection failures, non-grid spacing and depth-density ordering.

## P6 — full compile, bake/load and Base World workflow

Add the developer-facing complete workflow.

Provide repository-relative commands equivalent to:

- generate candidate from explicit seed/settings;
- inspect/validate candidate;
- load/run a baked candidate;
- promote an explicitly selected candidate to a Base World review location.

Use existing `tools/dev` conventions rather than inventing unrelated tooling.

Compile representative full P3 worlds with per-stage timings. **Target ≤120 seconds** on the identified current development machine. If the target is missed, profile and optimize actual hotspots; do not fake timings or silently reduce required world content.

Bake world resources/collision/placements/routes so subsequent load:

- does not invoke compiler/generator work;
- reproduces the generated candidate;
- is materially faster than generation;
- preserves manifest seed/generator/settings identity.

Promotion must never overwrite a Base World silently and should produce a normal reviewable diff. Do not automatically promote a world just because generation succeeds.

Prepare a deterministic qualification seed/settings set for P7 without declaring it a production Base World.

## P7 — native generated-world closeout

Mandatory owner/native review.

Generate at least one qualification candidate in the real environment while recording:

- source/package;
- seed/settings/generator version;
- stage timings and total time;
- hardware;
- errors;
- output manifest/hash summary.

Then load the baked world separately and record load behavior/time, proving the compiler did not rerun.

In normal play inspect:

- irregular basin silhouette/shape;
- central deep region and outward shallowing;
- natural perimeter in several directions;
- absence of visible rectangular/perfect-circle world walls;
- cave chamber, both tunnels, arch/overhang and canyon;
- actual terrain floor/wall/ceiling continuity;
- no floating terrain accents/major rocks;
- natural cluster placement and varied spacing;
- shallow/mid/deep density;
- open routes/camera clearance;
- FAF visual style and Forward+ water;
- Character Select/shared movement unaffected.

Demonstrate candidate-to-Base-World promotion using a disposable/reviewable target or owner-approved candidate. Do not silently declare the qualification seed a shipping world.

Create `qualification/native-world-gen.md` and one `closeout.md`, mapping V26/V27 and exact PASS/FAIL/NOT OBSERVED/BLOCKED items. Preserve failures.

## Hard boundaries

No infinite world streaming, voxel sandbox, destructible terrain, runtime erosion, runtime cave carving, procedural ecology, feeding/predator/growth, save-game system, network world sharing, renderer replacement or movement/camera authority changes.

Keep root `0.3.6` unchanged throughout the correction.
