# C3 world generation implementation evidence

## P1 — world compiler and baked contract (2026-11-10)

**Pre-task state:** clean `main` at `4901ea36cdf31e5b1cde3564cfb7cea14a267a79`; root `package.json` was `0.3.6`. Git history contains P3 P1–P6 checkpoints `22acded`, `3263a1c`, `606767f`, `a086831`, `d6a13f6`, `d694157` in order. C3 visibility P1–P3 were already committed before this task. The runner owns the P1 implementation commit; changes remain uncommitted here.

**Implemented boundary:** `WorldGenerationSettings` pins seed, settings schema version 1 and generator algorithm `c3-fixture-1`. `WorldCompileContext.Stream(subsystem, zone)` derives independent SplitMix64 streams from SHA-256 with explicit little-endian interpretation. Catalog traversal sorts by stable FAF ID, rejects duplicate IDs and hashes canonical ID/path/family/variant rows. The current fixture samples three named zones only. It does not form terrain, caves, routes or placements.

`GeneratedWorldManifest` format 1 records the world ID, seed, settings/generator versions, FAF catalog metadata hash, relative baked resource path/hash, sample count/content hash and per-stage milliseconds. Timings are diagnostic and excluded from identity/content hashes. `BakedWorldLoader` reads saved manifest and resource bytes, validates format/catalog/world identity/schema/hash and fails on missing or corrupt content without invoking the generator. It is not connected to normal gameplay yet; `main.tscn` still loads the authored P2/P3 habitat.

`tools/dev compile-world-fixture <seed>` retains a candidate under ignored `artifacts/world-data/godot/app_userdata/Fish/world-candidates/fixture-<seed>/` using isolated XDG user data. It refuses a nonempty destination rather than silently overwriting a candidate. `tools/dev inspect-world-compiler` uses a separate temporary user profile, compiles the same settings twice with reversed catalog order and a different culture, compiles a second seed, loads a baked package, rejects a corrupted resource, then removes its temporary candidates. The fixture command is for contract inspection, not production Base World promotion.

**T2 results:**

| Check | Observed result |
| --- | --- |
| `./tools/dev build` | PASS; Godot .NET `Fish.dll`, 0 warnings, 0 errors. |
| `./tools/dev inspect-world-compiler` | PASS; four coarse stages reported. Same seed/settings produced `world-b5a4b0168eace7ee` and content SHA-256 `4c222569411fe1bbdb3fe7d87b1a1c1887a7cbfb2809755991cd020567f97f6a`; reversed catalog order and French culture matched, different seed changed world ID and content hash, baked load succeeded, corruption was rejected. |
| `./tools/dev compile-world-fixture 42` | PASS; retained ignored `fixture-42/manifest.json`. Its resource hash matches the deterministic check. Manifest stage durations were catalog 16 ms, fixture 58 ms, bake 0 ms, manifest 10 ms in that run; these are fixture timings, not the full-world 120-second target. |
| Main headless `--smoke` | PASS; `FISH_SMOKE_OK`, normal habitat and fish initialized. |
| `git diff --check`, tracked payload inspection | PASS after correcting one README whitespace issue; no tracked `package-lock.json` or `game/assets/vendor-local` payload. |

**Next:** P2 must generate the irregular central-deep basin, variable natural perimeter, continuous mesh/collision and focused geometry checks. P3–P6 must add geology, routes, placement and full bake/load; P7 remains native qualification. No world art or gameplay Gate is claimed by this fixture.

## P2 — irregular basin and natural perimeter (2026-11-10)

**Pre-task state:** clean `main` at `1766c992e170cf16ea8a7792dc23561da60f0864`, root `0.3.6`. The Godot Editor MCP server `2026.11.04` reported no connected editor peer, so scene and C# edits were made on disk and verified headlessly. The phase runner owns the implementation commit.

`GeneratedBasin` uses seed `42`, settings schema `1`, the P1 stream context `c3-fixture-1`, and basin algorithm `c3-basin-1`. Nine seeded phases drive angular radius variation, smooth X/Z domain warp, a broad bowl, folded shoulders, an offset channel and bounded contour detail. The field is sampled cheaply by `GetHeightAt`/`NormalAt`; `SeabedSurface` exposes the field, deep-core metadata, actual `ArrayMesh` and its matching `ConcavePolygonShape3D`. The 96 × 96 m, 1 m grid is one 18,432-triangle open-seabed surface. Its collision shape is derived from that mesh with backface collision enabled. The P3 FAF draped-terrain collision resource was regenerated against the changed floor.

The dense seed-42 check found a minimum of `-12.59 m` at `(4.5, 2.0)` in the central region. The analytic perimeter radius varies from `30.93` to `39.07 m`; the sampled `Y=9 m` usable-water crossing varies from `24.00` to `33.50 m`. The largest adjacent half-metre height change on the checked ±40 m grid is `2.34 m`. Every checked radial direction rises above the fish's `Y=9 m` upper bound before its ±36.7 m horizontal clamp. Four rectangular side shapes now sit at ±47 m behind the bank; they are fail-safe collision, not ordinary route geometry. The overhead water surface remains `Y=10.39 m`, and the atmosphere/shader deep reference now reads generated deep-core metadata (`Y=-10.59 m`) instead of the old `Z=-21 m` shelf coordinate.

**T2 results:**

| Check | Result |
| --- | --- |
| `./tools/dev build` | PASS, 0 warnings/errors. |
| `./tools/dev inspect-basin` | PASS; 25,921 finite same-seed grid samples, center/edge ordering, adjacent continuity, different-seed variation, 144 angular water crossings, exact 18,432-triangle visual/collision identity and remote fail-safe walls. |
| `./tools/dev inspect-habitat` | PASS; 202 physics ray samples agree with generated height, fish moves, floor/underside and natural bank block, upper limit and P1 fixture remain. |
| `./tools/dev inspect-depth`, `inspect-atmosphere` | PASS; generated core drives shader/fog depth identity, atmosphere state isolation and equal 30-step motor route. Shafts now select lit caustic anchors whose generated floor lies inside the existing projection volume. |
| `./tools/dev inspect-world-compiler` | PASS; the P1 fixture's deterministic bake/load/corruption checks remain intact. |
| `inspect-collision` with isolated `XDG_DATA_HOME` | PASS; regenerated FAF draped collision still matches its visual triangles, and the new side-wall locations are verified. |
| Main scene `--smoke` with isolated `XDG_DATA_HOME` | PASS; `FISH_SMOKE_OK` and motor check on the generated basin. |
| `git diff --check` | PASS. |

The repository's `prepare-faf-collision` wrapper invokes `godot --import`; that editor stage could not create a local socket under this sandbox. The same tracked terrain collision preparation script ran directly in headless mode and reported `TERRAIN_COLLISION_PREP_OK`. The headless checks are geometry and runtime identity evidence, not native visual qualification.

**P3 geology entry:** `inspect-reef` fails tunnel camera clearance near `(-7.9, -3.53, -17.78)`; `inspect-terrain` fails a 1.22 m visual/collision route-floor agreement near `(21, -0.64, 11)`; `inspect-geology` fails west tunnel camera clearance near `(-7.9, -3.76, -17.31)`. These are real conflicts between the new basin and the interim authored P3 piece/route layout. P3 must generate geological masses, cave/tunnel shells and collision from the basin, and P4–P5 must establish protected routes and supported placement before those checks can be qualified. Normal play still generates this P2 candidate at startup; P6 must bake/load the complete world. P7 must inspect the actual visual perimeter and full traversal. No P3 Gate or new native P2 claim is made here.

## P3 — true 3D geological regions (2026-11-10)

**Pre-task state:** clean `main` at `12cb7ccdbb00a21b52a938c665b30c097c483cc8`; root `0.3.6`. The Godot Editor MCP server `2026.11.04` was reachable but had no connected editor peer. The pinned local executable was `Godot 4.7.2.stable.mono`, and the pinned .NET SDK was `10.0.403`. `SurfaceTool`, `ArrayMesh.CreateTrimeshShape`, `ConcavePolygonShape3D.BackfaceCollision` and static-body attachment were verified by build and headless scene execution. The runner owns the commit; this task leaves its changes uncommitted.

`GeologicalRegionGenerator` uses bounded, deterministic quadratic centerlines and 16 stations × 20 cross-section sides per closed shell. Each station has an inner traversable floor/wall/ceiling ring and an outer geological ring; paired loft faces and portal annuli close the mesh. The outer ring extends 2.7–3.2 m beyond the opening and 1.7 m below the sampled basin, creating broad structural mass and buried contact instead of a suspended roof. Floors follow the actual analytic basin with a 0.24–0.62 m lift, higher in each passage and lower at its portals. The canyon is a separately closed 16 × 13-profile ribbon with an open top, two ridges, side walls, a floor and buried skirts. All five use ordinary `ArrayMesh` resources and matching `ConcavePolygonShape3D` collision made from the visual triangles; they can be serialized by P6 without runtime CSG or voxels. Independent named seed streams affect each shell, and basin height changes affect every structure. FAF rocks/terrain remain in the interim scene for edge/detail use, but no generated cave, tunnel or arch depends on a placed FAF roof/support piece.

The generated scene path is `Habitat/Composition/ReefComposition/Geology/GeneratedRegions/<id>`. Every body has `Mesh`, `Collision`, and `Centerline` children. The line contains ordered `Entry`, `Center`, `Exit` and `ProbeNN` markers (16 samples total) carrying the fish/camera probe radii (0.48/0.22 m), nominal side/headroom metadata or `open_top`. These are P4 inputs, not a claim that the old authored route graph has been reconciled. P4 must measure clearance against the complete final world before routing and reserve the resulting corridors from dressing.

| Generated ID | Kind | Visual/collision triangles | Measured minimum clear sphere radius along 16 probes, fish/camera | Live camera distance at center |
| --- | --- | ---: | ---: | ---: |
| `cave-chamber` | Widened cave shell with two portals | 1,280 / 1,280 | 1.55 / 2.32 m | 4.29 m |
| `west-swim-through` | Curved tunnel | 1,280 / 1,280 | 1.51 / 1.79 m | 4.29 m |
| `east-swim-through` | Narrower, differently curved tunnel | 1,280 / 1,280 | 1.40 / 1.56 m | 4.29 m |
| `stone-arch` | Broad short arch/overhang shell | 1,280 / 1,280 | 1.64 / 2.49 m | 4.29 m |
| `canyon-ridge` | Open ridge/canyon transition | 828 / 828 | 0.80 / 1.38 m | 4.29 m |

The minimum clear sphere radii come from 8-step physics shape-query bisection at each named route probe; the camera point is 2 m above the fish point. They include the current FAF and terrain collision, so the canyon's 0.80 m fish radius is the tightest measured point. All five routes also passed 225 sampled fish/camera overlap pairs across their centerlines, fish collision sweeps outward and back, generated-floor and side-wall rays at the center, generated-ceiling rays for the four enclosed structures, and a live camera nonpenetration check. The 4.29 m camera distance is the stationary center sample, not a native camera-feel or full input-path judgment.

**T2 results:**

| Check | Observed result |
| --- | --- |
| `./tools/dev build` | PASS; 0 warnings, 0 errors. |
| `./tools/dev inspect-generated-geology` | PASS; five distinct IDs/kinds, 5,948 finite nondegenerate visual triangles and 5,948 identical collision triangles, 8,922 oppositely paired geometric edges, buried basin overlap, floor/wall/ceiling classification, two-surface X/Z cases, same-seed identity, different-seed variation, route hooks and physical clearance/traversal. |
| `./tools/dev inspect-collision` | PASS; 46 static bodies, 51 shapes, 38 interim FAF geological pieces, five generated geological mesh/collision pairs, seabed/draped terrain and remote limits. The open floor ray moved to a clear X/Z because the generated arch now covers the origin below the spawn. |
| `./tools/dev inspect-basin` | PASS; the seed-42 18,432-triangle open basin and its collision, deep core and irregular natural perimeter remain unchanged. |
| `./tools/dev inspect-habitat` | PASS; 202 heightfield collision samples exclude the new geology, while a clear open-floor location verifies downward/underside blocking, bank, spawn, motor and P1 fixture. |
| Main scene headless `--smoke` | PASS with isolated `XDG_DATA_HOME`; normal habitat/atmosphere loaded, motor check and `FISH_SMOKE_OK`. |
| `./tools/dev inspect-geology` | FAIL on the already recorded interim west-tunnel camera route: FAF `Solid` at approximately `(-7.9, -3.76, -17.31)`. P4–P5 own route/placement reconciliation; this result remains visible. |
| `git diff --check` | PASS. |

This is headless source/physics evidence. Native visual continuity, the final route graph, protected placement, bake/load timing and owner acceptance remain P4–P7 work. The generated shells currently build during normal scene initialization; P6 must serialize and load them as baked resources without generation on the normal path.

## P4 — protected routes and environmental analysis (2026-11-10)

**Pre-task state:** `main` at `f1f4dca3fbf7bc86da5cef57f81d4643449eb50b`; root version `0.3.6`. The runner resumed an intentionally dirty working tree containing partial P4 implementation and evidence. The phase runner owns the implementation commit; these changes are left uncommitted by the task agent.

`ReefComposition.TryBuild` now constructs `GeneratedWorldRoutes` immediately after the basin, draped terrain and generated/FAF geology, and before decorative FAF dressing. The seed-42 runtime graph has **27 named nodes and 32 edges**: the actual spawn, ordered shallow/middle/deep fish positions, west and east open-water links, two separate shallow returns, landmark approaches, all five generated geological centerlines, and entrance connectors. Each geological edge reuses its actual 16-point centerline. Open connectors are repaired deterministically against the current static collision with bounded 3D A* search, exact fish/camera sphere samples along each candidate segment, and an additional triangle-distance check against all generated geology plus the basin heightfield. Nine connectors are repaired in the seed-42 candidate. The graph's node/edge IDs, ordered polylines and protection radii are repeatable for the same seed and collision inputs.

The fish radius is **0.48 m** and the camera probe radius **0.22 m**, at **2 m** above the fish route point, matching the accepted P1/P3 probe contracts. Open edges reserve **2.2 m** radial exclusion around their 3D polylines; enclosed centerlines reserve **1 m** for added placement. Every edge also reserves a **0.65 m camera tube** along the elevated probe path. `GeneratedWorldRoutes.IsProtected(position, objectRadius)` lets P5 reject a candidate whose bounds enter either volume; `DistanceToRoute(position)` supplies a continuous 3D suitability signal. Node roles and edge kinds identify spawn, depth bands, open corridor, return/alternate, landmark, entrance and each geology kind. The interim draped terrain seals the **far east tunnel mouth**, so that generated tunnel is an accessible out-and-back branch from its near entrance. The cave, west tunnel, arch and canyon have connected approach/departure links. P5 should revisit the east far mouth when it replaces interim terrain/placement.

`ReefComposition.EnvironmentAnalysis` owns a **41 × 41 grid at 2 m spacing** over X/Z `[-40, 40] m`, derived once at world construction. `SampleGrid(x,z)` and `SampleNearest(x,z)` expose a `WorldEnvironmentSample` with surface position/elevation, water depth, surface normal and slope, 4 m-scale concavity, perimeter and deep-core distance, 3D route distance, cave mesh and entrance distance, shelter/exposure approximation, and nearest generated geology mesh distance (capped at 12 m). Downward projection uses the basin's analytic one-height surface only for open seabed. `RaycastSurface(origin,direction,maxDistance)` and `ProjectDown(origin)` return actual generated collision-triangle hits for cave roof/floor/wall and overhang queries. `SampleProjected(origin,direction,maxDistance)` returns both that 3D hit and the full field sample, using neighboring surface rays for broad concavity. The shelter signal combines an upward roof ray, concavity and nearby geology; it is a deterministic placement heuristic, not a lighting or predator model.

The analysis caches **1,681 complete field samples**, **5,948 generated triangles**, and X/Z triangle buckets used for repeated vertical projection and nearby mesh-distance queries. Broad curvature is computed from preprojected neighbor heights. Arbitrary ray directions scan the triangle set on demand, while downward projection and proximity queries use buckets. The current P1 compiler fixture remains a fixture; P6 must serialize the complete routes/fields with the generated world and remove generation from normal load.

**T2 results:**

| Check | Observed result |
| --- | --- |
| `./tools/dev build` | PASS; 0 warnings, 0 errors. |
| `./tools/dev inspect-route-fields` | PASS; 27 nodes/32 edges connected, shallow/middle/deep ordered, deep return and separate alternate shallow paths, all five geological routes reachable, 804 full-world fish/camera sphere probe pairs clear at ≤0.75 m spacing, both exclusion tubes verified, 1,681 cached field samples, bounded ranges, same-seed graph/polyline/field repeatability, and actual cave floor/ceiling/wall mesh projection with full finite field samples. |
| `./tools/dev inspect-generated-geology` | PASS; five distinct generated mesh/collision regions, 5,948 triangles, same-seed geometry, 225 geological centerline fish/camera probe pairs, fish sweeps out and back, floor/wall/ceiling rays and live stationary camera clearance. |
| `./tools/dev inspect-basin`, `inspect-habitat`, `inspect-depth` | PASS; generated basin mesh/collision and perimeter, clear spawn/fish motor and containment, and existing water-column state/lighting behavior. The depth check intentionally emits recovery warnings for invalid preset fixtures. |
| `./tools/dev inspect-collision`, `inspect-course` | PASS; generated/full-world collision alignment and the preserved P1 fish gap/rock clearance fixture. The course command now uses an isolated user-data profile to avoid Godot log-file conflicts. |
| `git diff --check` | PASS. |

These are headless geometry, physics and deterministic source checks. They do not qualify native route feel or final art. The older authored `inspect-reef`, `inspect-terrain` and `inspect-geology` route assertions still describe a superseded fixed-marker layout and retain their recorded failures; P5 must replace placement and reconcile those assertions against the generated graph. P7 must inspect the actual dressed world with native player control.

## P5 — natural FAF placement and relaxation (2026-11-10)

**Pre-task state:** clean `main` at `b05de6041cb460a17259bd0a446cdf62ac8deba9`, root version `0.3.6`. The Godot Editor MCP server was available but its Fish editor bridge was disconnected; source edits and checks used the pinned local Godot/.NET CLI. The phase runner owns the implementation commit, so all P5 changes remain uncommitted here.

Normal play now skips the P3 authored FAF ground patch and rock layouts. The generated basin and five geological mesh/collision regions own the playable floor/walls. The generated route graph and cached environmental analysis are built first; `GeneratedFafPlacement` then selects up to six suitability-ranked anchors for each of eight roles: coral garden, grass meadow, sandy clearing, rocky slope, cave entrance, sheltered pocket, drop-off reef and deep rubble. Roles score measured depth, slope, shelter/exposure, cave/geology proximity and route distance. Some roles can remain empty on a particular seed when their sites fail support or route checks; this is preferable to forcing a prop into a passage.

Each of the 38 catalog entries has an independent named seed stream and a bounded quota: one localized terrain accent per A–F variant, three placements per rock variant, four per coral variant, six per plant variant and three per small-detail variant. Candidates are dart-thrown in continuous coordinates around suitable anchors. Every candidate is projected onto the generated surface, graded for its family, rejected near protected route/camera tubes and cave entrances, sampled at four footprint directions, and separated by a radius-dependent distance from accepted neighbors. Rocks use the lowest support sample as their embedded base. Terrain accents are individually draped over exact generated surface queries with three mesh substeps per source facet and collision created from the draped visual triangles. They are local features; none replaces the basin floor. Coral/plant/detail pieces use per-ID MultiMeshes; rock and terrain instances have matching individual collision.

Two bounded relaxation passes move or remove late conflicts and density spikes; a final pass rotates repeated nearby identical variants. Ambient fish schools now choose actual generated habitat sites while keeping the P3 visual-only population and animation contracts. The score reports role/depth/family and living-depth counts, variant coverage, rejected candidate support/route/spacing counts, unresolved support/route/intersection counts, repeated headings, minimum spacing, density spikes, repair/removal counts and planning estimates for instances/triangles/render nodes. The `before` counters are candidate rejections during selection, not floating props retained in an initial playable world. The estimates are not measured frame cost.

The east swim-through remains an out-and-back branch: removing P3 authored terrain revealed that its far mouth is also sealed by the generated basin. Reconnecting its far-side graph edge failed a live fish-sphere route probe against the basin, so P5 preserves the clear P4 graph. Native visual evaluation and any redesign of that far mouth remain for later world qualification.

**T2 evidence:** `./tools/dev build` passes with zero warnings/errors. `./tools/dev inspect-faf` loads all 38 environment variants and five fish. The generated main scene and route-field probe load the new dressing; the latter verifies all protected edges with fish/camera clearance samples. `./tools/dev inspect-placement` checks seed 42 against a second same-seed plan, seeds 43/44 for variation, all-variant coverage, living-density trend, exact surface support, zero unresolved critical route/support/intersection failures, non-grid X/Z coordinates, per-variant runtime meshes and all 18 major collision bodies. `./tools/dev smoke-world` runs the main generated-world motor/smoke marker with an isolated user profile. `git diff --check` verifies whitespace. These are headless source/geometry/physics checks, not owner acceptance of visual composition or frame time. P6 still owns full-world bake/load, actual mesh/triangle/material and generation/load timing measurements, compile-time optimization and Base World promotion; P7 owns native visual/traversal review.

| Seed | Plan hash prefix | Depth counts shallow/upper/middle/lower/deep | Living counts by same bands | Candidate rejects support/route/spacing | Final critical support/route/intersections | Minimum X/Z spacing | Repairs |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 42 | `e56351267092c1d9` | 13/47/17/42/19 | 13/46/14/23/0 | 679/1067/827 | 0/0/0 | 0.41 m | 5 |
| 43 | `faf7fab037707adb` | 0/62/5/47/24 | 0/59/5/31/1 | 688/1344/946 | 0/0/0 | 0.58 m | 3 |
| 44 | `93e8a5d4f02e4266` | 14/51/7/43/23 | 14/50/7/25/0 | 703/1212/948 | 0/0/0 | 0.50 m | 7 |

Every seed places 138 instances across all 38 variants with 48 selected anchors and a 46-render-node estimate. The score's conservative triangle estimate is 58,680; P6 must replace it with measured imported/baked totals. Living counts are concentrated in the shallower two bands relative to lower/deep on all three seeds; seed 43 has no eligible site in the narrowest `shallow` band and uses the adjacent `upper` band. `inspect-route-fields` passes 795 seed-42 fish/camera clearance probes on 27 nodes and 32 edges after the new static collision is installed. `inspect-ambient` passes nine visual-only schools, 31 fish, four species and animation replay; `smoke-world` passes `FISH_MOTOR_CHECK_OK` and `FISH_SMOKE_OK`.
# P6 — full bake, load and Base World workflow (2026-11-10)

**Pre-task state:** clean `main` at `4330dfa0d68bdd9c123a25e6c95315d0bd461ce0`, root `0.3.6`. The runner owns the implementation commit; this task makes none. Fish's Godot MCP server `2026.11.04` was reachable but its editor bridge was disconnected, so C# and resource changes were made on disk and validated with the pinned local CLI. The current desktop reported AMD Ryzen AI 9 465 w/ Radeon 880M, 20 logical CPUs, 30 GiB RAM, Godot `4.7.2.stable.mono`, and .NET SDK `10.0.403` under Linux/X11 (`DISPLAY=:0`). The full compile timing below is **headless on this identified desktop**, not native-play timing acceptance.

`./tools/dev generate-world 42 qualification-42-final` compiled the full seed-42 P3 world with settings `1` and generator `c3-full-1` and baked a 3.8 MiB candidate under ignored `artifacts/world-data/.../world-candidates/qualification-42-final/`. The world ID is `world-76d44405d319640d`, catalog SHA-256 `d7f9e29007443eb53b09a780028f2176f23fcc01774fbec178cce80d1581cf9a`, and baked content SHA-256 `5b62157809953d93eeb0dfb3888540e3070b12512561ab6e3cdd1042750f4b9f`.

| Stage | Measured milliseconds |
| --- | ---: |
| Palette/player preparation | 1,147 |
| Basin mesh and collision | 79 |
| Historical source preparation | 2,459 |
| Geological mesh and collision | 15 |
| Route repair | 3,311 |
| Environment analysis | 294 |
| Placement and dressing | **43,396** |
| Ambient life | 20 |
| **Generation total** | **50,726** |
| Data serialization | 28 |
| Scene/resource serialization | 198 |
| **Full compile and bake total** | **50,967** |

The measured full compile is below the 120-second development-machine target. Placement/dressing is the dominant measured hotspot and is retained in the timing and P7 handoff; no quality reductions were made. Earlier first-pass generation was 50,868 ms and the initial whole-habitat pack failed validation because Godot did not include generated children of an instanced subscene. The final bake explicitly saves the generated components. Godot also emitted random local resource and packed node IDs; the baker now assigns stable resource IDs and normalizes the pinned 4.7.2 packed node-ID array. Two independent seed-42 packages were byte-identical after normalization, and the final end-to-end compiler output matched their content hash. The failed ignored package was never promoted.

The candidate `manifest.json` records seed/settings/generator/catalog/content identity, resource hashes, stage timings and counts. `seabed-mesh.res` and `seabed-collision.res` hold the matching open floor, `geology.scn` holds five generated mesh/collision regions, `dressing.scn` holds 138 placements and their mesh/collision nodes, `ambient.scn` holds 31 visual fish, and `world-data.json` holds 27 route nodes, 32 route edges, 1,681 field samples, placement records and ambient motion data. The package references existing FAF assets; it does not copy original source payloads. Baked loading reads/verifies every hash and count, installs the resources into the existing authored Habitat shell, restores ambient animation, and asserts the generation-stage counter did not change. It does not fall back to generation on error.

`./tools/dev inspect-candidate qualification-42-final` passed and reported a cold baked load of **917 ms**, with a separate smoke run at **638 ms**; `generator_invocations=0`. This is materially faster than the measured 50.97-second compile. `./tools/dev smoke-baked-candidate qualification-42-final` passed `FISH_MOTOR_CHECK_OK` and `FISH_SMOKE_OK`. `./tools/dev promote-world qualification-42-v2 qualification-42 --dry-run` showed the exact ignored candidate and tracked destination. After the deterministic-byte correction, the earlier provisional package was removed by an exact-manifest guarded operation and `./tools/dev promote-world qualification-42-final qualification-42` created the final reviewable `game/worlds/base/qualification-42/` package. An existing destination is rejected. Normal play now loads that package by default and fails visibly if its manifest/assets are missing or corrupt. It is marked a **provisional P7 qualification Base World, not a shipping decision**.

**T2/T3 preparation:** `./tools/dev build` passed with zero warnings/errors. `inspect-world-compiler` passed deterministic fixture/reordered-catalog/different-seed/corruption checks. `inspect-basin`, `inspect-generated-geology`, `inspect-route-fields` and `inspect-placement` emitted their passing geometry, route and multi-seed markers on the final source. An ignored copy with one appended corrupt byte in `world-data.json` was rejected with a hash mismatch and no regeneration. `./tools/dev smoke-world` passed against the promoted baked default. `./tools/dev inspect-character-select` passed all five fish and `CHARACTER_ROUTE_OK` after the baked switch, preserving selected fish, pause/return and new selection routing. `git diff --check` passed. These are headless checks; P7 still needs hand-controlled visual, collision and performance judgment in native play.

**P7 handoff:** use seed `42`, settings `1`, generator `c3-full-1`, `./tools/dev run-baked-candidate qualification-42-final` or normal `./tools/dev run` for native traversal. For a fresh timed compilation use a new candidate name with `./tools/dev generate-world 42 <new-name>`, inspect it with `./tools/dev inspect-candidate <new-name>`, compare its manifest/content hashes with this candidate, and record native machine/display/method and observed load/visual behavior in `qualification/native-world-gen.md`. Seeds `43` and `44` remain deterministic placement comparison settings, not promoted Base Worlds. P7 decides whether to retain or revise the provisional seed-42 package; no P3 Gate is closed here.
