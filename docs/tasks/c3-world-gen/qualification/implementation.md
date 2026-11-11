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
