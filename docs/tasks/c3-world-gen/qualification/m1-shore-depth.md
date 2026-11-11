# C3 R1/M1 shore-first world repair — 2026-11-11

**State:** implemented and structurally checked at root `0.3.6`; owner visual review pending. This is a manual M1 handoff. It does not change the historical P7 failure or promote a Base World. M2–M4 and full P7 qualification remain open.

## Implemented field

The generator constructs a 256-point water-domain polygon before evaluating elevations. Three unequal broad lobes and three coves use seeded bearings and widths, plus a mild axis stretch and offset. A 2 m lattice caches signed distance to the nearest shoreline segment, positive inside. Bilinear distance queries drive a broad water-depth profile: 0.7 m at the shore; successive smooth bands add 1.3, 4, 9, and 12 m as distance inside reaches about 6, 24, 52, and 92 m. Long folds, a ridge, shelf, channel, and a localized deep pocket are applied afterward and fade out toward the shore. Dry perimeter terrain rises gradually over about 21 m beyond the water edge.

The open seabed is a continuous 368 × 368 m mesh at 2 m spacing with matching two-sided collision (67,712 triangles). The cached environmental field spans -176..176 m in X/Z at 4 m spacing (7,921 samples). Fail-safe collision and the water surface cover the expanded terrain, while historical `c3-full-1` Base Worlds retain their original analytical basin and limits. New candidates are identified by `c3-full-3` and `c3-shore-depth-3`; the manifest records shoreline bounds and deepest water depth. Route search and placement no longer reject positions at the old 35–36 m radial/axis clamp. Tunnel geometry, route anchor design, vegetation quotas, and visibility tuning were not redesigned in M1.

## Structural evidence

`tools/dev inspect-shore-field` verifies exact same-seed shoreline and sampled depth equality, seed variation, broad nonperiodic angular variation, inward distance and shallowing on eight boundary approaches, all depth bands, an interior deep core, and ≤1.45 m vertical change between adjacent 2 m wet samples. Areas below are 2 m lattice estimates of horizontal water area in m², not exact polygon integrations.

| Seed | Shore bounds X × Z | Radius range | Deepest water / core X,Z | Core shore distance | 0.5–2 | 2–6 | 6–15 | 15–30 | 30–45 |
| --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| 42 | 253.1 × 305.7 m | 92.0–165.0 m | 39.5 m / (10, 8) | 82.3 m | 4,648 | 13,408 | 19,316 | 13,860 | 2,744 |
| 43 | 263.0 × 292.3 m | 96.4–160.9 m | 41.5 m / (-12, -28) | 89.0 m | 4,608 | 14,780 | 16,496 | 14,984 | 3,032 |
| 44 | 285.0 × 265.6 m | 95.3–168.5 m | 38.4 m / (2, -14) | 86.7 m | 4,656 | 14,268 | 16,296 | 15,512 | 3,060 |

The lowest mean angular radius difference under two- through six-fold rotation was 16.4, 13.8, and 15.1 m for seeds 42–44. `tools/dev inspect-basin` also verified identical visual/collision triangles for seed 42. The measured seed-42 full compile/bake took **43.102 s**, including **1.338 s** for basin mesh/collision and **36.557 s** for dressing. The baked candidate loaded in **0.836–1.524 s** with zero generator invocations. Its seven package files total about 11 MiB; world data is about 2 MiB. [Candidate manifest](../../../../artifacts/world-data/godot/app_userdata/Fish/world-candidates/m1-seed-42-v3/manifest.json), [structural log](../../../../artifacts/logs/shore-field.log), and [basin/collision log](../../../../artifacts/logs/basin-check.log) are local ignored evidence.

The seed-42 baked candidate is `m1-seed-42-v3`, world ID `world-25dbe5f95ddd8666`. `tools/dev inspect-candidate` verified resource hashes and generator-free load; `tools/dev smoke-baked-candidate` passed motor/main smoke. `tools/dev inspect-character-select` passed all five fish and select/play/return/reselect against the preserved historical default package. The native capture reported **Vulkan Forward+ on AMD Radeon 880M**.

`tools/dev inspect-route-fields` passed 933 collision clearance probes and repeated analysis samples over the expanded grid. `tools/dev inspect-placement` passed same-seed and alternate-seed placement determinism for seeds 42/43/44 with 38 catalog variants and zero unresolved support/route failures. These checks do not establish completed alternate-seed baked worlds or native swim-through comfort. [Route check log](../../../../artifacts/logs/route-fields.log) and [placement check log](../../../../artifacts/logs/placement-check.log) are local ignored evidence.

## Native review images

These are diagnostic teleports with a settling follow camera; they establish rendered views at sampled floor depths, not swimming reachability or owner feel. The overview uses a temporary top-down camera with the surface, caustic decal, and fog hidden to expose the terrain outline. It changes no saved scene or baked resource.

| View | Sampled depth | Image |
| --- | ---: | --- |
| Shoreline shallow | 1.28 m | [shore-shallow.png](../../../../artifacts/c3-m1-native-v3/shore-shallow.png) |
| Shallow reef looking inward | 4.01 m | [shallow-reef.png](../../../../artifacts/c3-m1-native-v3/shallow-reef.png) |
| Middle water | 10.00 m | [middle.png](../../../../artifacts/c3-m1-native-v3/middle.png) |
| Deep basin | 23.01 m | [deep-basin.png](../../../../artifacts/c3-m1-native-v3/deep-basin.png) |
| Deep pocket | 37.00 m | [trench.png](../../../../artifacts/c3-m1-native-v3/trench.png) |
| Perimeter east/west/north/south | 1.67 / 1.75 / 1.40 / 1.35 m | [east](../../../../artifacts/c3-m1-native-v3/perimeter-east.png), [west](../../../../artifacts/c3-m1-native-v3/perimeter-west.png), [north](../../../../artifacts/c3-m1-native-v3/perimeter-north.png), [south](../../../../artifacts/c3-m1-native-v3/perimeter-south.png) |
| Debug overview | — | [debug-overview.png](../../../../artifacts/c3-m1-native-v3/debug-overview.png) |

The overview shows a larger asymmetric water body with a pronounced west/southwest cove and varying bank width. In ordinary camera views, the large region is still sparse, and the existing dark geology sits in a central cluster. Those composition problems are visible in the evidence and belong to M2/M4. Full alternate-seed compilation and route reliability belong to M3; seed 43/44 were structurally checked here, not claimed as baked successes. Complete fish-controlled perimeter and tunnel passage and the owner's visual judgment remain unobserved.
