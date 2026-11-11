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
