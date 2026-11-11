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
