# P5 P2 — New FAF cover intake and Pike fixture

**2026-11-12 source result: PASS; native appearance and owner play: NOT OBSERVED.** Pre-task HEAD was `eaad62563f16fa3bf32be7d08ff2121b1b815397` on `main`, clean, root `0.5.1`. P1's `0.5.1` runner checkpoint and [P1 focused evidence](p1-routing.md) were present. [P4 closeout](../../p4/closeout.md) authorized progression while retaining NOT GREEN Gates. P1 was not a manual checkpoint, so no separate post-manual authorization was required. This record describes the uncommitted P2 implementation for the runner's checkpoint; it does not claim a commit.

The Godot Editor MCP `2026.11.04` bridge was disconnected, so no editor mutation or editor play claim was made. `tools/dev` targets this repository's `game/` project; the CLI scene and import checks ran against Fish, with no Space Factory connection.

## Actual producer intake

The two separate owner FAF runs have delivered packs since the earlier tree check in [the intake brief](../stealth-cover-asset-intake.md). The new [vegetation pack](../../../fish-asset-factory/pond-stealth-vegetation-pack-001/README.md) manifest is `assets/faf/pond/plants/pond_stealth_vegetation_001_manifest.json`, SHA-256 `57392fb9d518e4d88c948b478ea9836349a63e8299670863d03b98bc2dde0b17`. The [wood pack](../../../fish-asset-factory/pond-wood-pack-002/README.md) manifest is `assets/faf/pond/wood/pond_wood_002_manifest.json`, SHA-256 `cb8eaac5e50d4102f962608abeaae075bae9886634858b36cb78914832dea1c3`. `game/assets/faf/pond/p5_cover_catalog.json` records each delivered Pond ID, seed, actual `.blend` and GLB path/hash, dimensions, triangle/material cost, and wood wrapper/collision count. `inspect-pike-cover-assets` compares that snapshot with both manifests and original pack 001 IDs, verifies generator recipe seeds and source/export hashes, runs both binary GLB checkers and both Godot import checkers.

| New family | Delivered Pond variants | P5 role |
| --- | --- | --- |
| Ribbon grass | FAF-PLANT-010A/B/C | Nursery short `.830 m`, medium `1.738 m`, and near-surface tall `3.577 m` beds. |
| Branching waterweed | FAF-PLANT-011A/B/C | Porous intermediate screens. |
| Coontail | FAF-PLANT-012A/B/C | Compact, tall and horizontal intermediate cover. |
| Broad pondweed | FAF-PLANT-013A/B | Nursery and transition leaf fringe. |
| Rooted thicket | FAF-PLANT-014A/B | Intermediate soft brush beside wood. |
| Long fallen tree | FAF-WOOD-005A/B | Nursery bank-side hard cover. |
| Forked log | FAF-WOOD-006A/B | Intermediate raised lateral routes. |
| Root-ball tree | FAF-WOOD-007A/B | Intermediate undercut refuge; no through passage claimed. |
| Split hollow | FAF-WOOD-008A/B | Deep end and side-entry passage. |
| Sunken branch crown | FAF-WOOD-009A/B | Deep lateral and vertical openings. |

All five wood families use the delivered segmented/open collision wrappers; all five plant families remain traversable without hard collision. The current asset candidates have producer Blender/Forward+ technical records, but no owner style lock.

## Scene result and checks

Seed `5202` builds one continuous shallow/intermediate/deep mud bed and waterline in the isolated Pike Encounter. Bed vertex colors shift from lighter bank sediment to darker deep silt beneath the authored overhead light and bounded underwater fog. Forty-seven varied, rooted cover instances use all 23 delivered structural variants. A long tree screens the safe Bluegill start; central open water is an exposed crossing, with side alternatives. The stationary registered Pike is now a correctly sized physical `CharacterBody3D`, ready for later AI without hunting, feeding or death. The normal baked Pond/Ocean and their catalogs remain unchanged.

| Check actually run | Result and scope |
| --- | --- |
| `python3 tools/asset-factory/blender/plants/check_pond_stealth_vegetation_001.py` | **PASS** — 13 GLBs, 18,098 source triangles, grass heights `.830/1.738/3.577 m`. |
| `python3 tools/asset-factory/blender/wood/check_pond_wood_002.py` | **PASS** — 10 GLBs, 5,420 source triangles and wrappers. |
| `./tools/dev inspect-pike-cover-assets` | **PASS** — 23 manifest/catalog/source/hash/seed variants, two Godot GLB import/cost/root checkers, 10 families. |
| `./tools/dev build` | **PASS** — Godot .NET project compiled with zero warnings/errors. |
| `./tools/dev inspect-pike-cover` | **PASS** — 47 deterministic placements, three height bands, rooted contact (maximum sampled wood-axis bed variation `0.38 m`), 54,126 instanced source triangles, 82 material slots, 193 wood collision shapes; sampled fork, root, split end/side and crown lateral/vertical fish/camera clearances. A small physical sightline probe intersects the long tree and leaves the open crossing clear. Bluegill + FollowCamera crossed nursery to `z=-9.34` and the real controller crossed split A to `z=-22.42`; zero sampled camera overlaps. The real Pike actor crossed the central search lane and split A. The root is classified limited refuge, narrow apex/roof and lower branch gaps visual-only pending native review. |
| `./tools/dev inspect-pike-encounter` | **PASS** — three destinations, two habitats, retry/return and Pond-to-fixture isolation. |
| `./tools/dev inspect-pond-atmosphere` | **PASS** — normal Pond/Ocean atmosphere isolation and unchanged fish movement. |
| `XDG_DATA_HOME=/tmp/fish-p5-scene ./tools/dev inspect-scene res://scenes/p5_pike_encounter.tscn` | **PASS** — fixture scene parses. The first invocation without a writable XDG data profile printed log-path errors; the profiled rerun passed. |
| `./tools/dev import` | **BLOCKED by execution environment** — editor import scanned the assets, but the sandbox refused the .NET IDE and MCP socket binds and local editor-settings write; the wrapper rejected the error markers. The separate headless Godot import checkers above passed for every delivered GLB/wrapper. |
| `./tools/dev measure-pike-cover` and `xvfb-run -a ./tools/dev measure-pike-cover` | **NOT OBSERVED** — X11/Wayland display connection unavailable here. Draw calls, overdraw, native style/spacing and owner controlled swim judgment are not claimed. |

The sightline diagnostic uses small physical overlap samples, not the P3 world-space perception system. Fixed-pose gap samples and scripted motor routes do not establish native camera comfort, all approach angles, realistic Pike search decisions, or an owner art PASS. P3 can build sensing on the actual solid/soft cover, while P6 must revisit multi-angle native appearance, split/crown/fork traversal and renderer cost before M1 approval. No P5 Gate changes status here.
