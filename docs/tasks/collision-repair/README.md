# World collision repair — 2026-11-10

**Disposition: repaired collision geometry PASS in focused checks; full hand-controlled clearance NOT OBSERVED.** This direct correction keeps root version `0.3.6`. It starts from `main` at `1f406a387ae2a2f7f5c33d9b2ea2a77dc8ec1bdd`, with existing uncommitted C3/control work preserved. The repair was not committed or published.

## Causes and scope

The normal scene had ten P2 `StaticBody3D` rock boxes whose imported visuals were hidden by P3. They blocked empty water. Three additional runtime `BoxShape3D` bodies in `FafGeologyWorld` stood apart from the 38 visible FAF geology placements. `FishController` also clamped horizontal position to ±24.9 m, while the intentional physical walls sit at ±37.6 m. The camera used the same collision layer as the fish, so those phantom solids could also push the follow view forward. The fish's 0.48 m sphere was appropriately sized and was retained.

Audit of the active main scene after repair finds 41 static bodies: 38 FAF geology solids, one prepared draped-terrain solid, one continuous seabed, and one five-shape boundary body. It finds 46 enabled `CollisionShape3D` nodes including the fish and no `CollisionPolygon3D` nodes. The imported FAF GLBs have no built-in physics. Coral, plants, details, and ambient fish remain decorative and non-blocking. The P1 course fixture is separate from normal play.

## Repair and reusable workflow

- Removed the ten invisible P2 rock bodies and their ten box resources from `p2_reef_composition.tscn`; their historical visual/source anchors remain for provenance. Removed the three coarse FAF geology boxes.
- Added one solid to each of 38 visible geology placements: 26 boulders use convex hulls and 12 irregular terrain/shelf pieces use static concave triangle surfaces. Concave surfaces preserve recesses and overhangs without filling openings. Each solid is a child of the same visual wrapper, so position, rotation, nested model transforms, and nonuniform placement scale stay aligned.
- `game/dev/prepare_faf_collision.gd` prepares four rock and six terrain variant resources from imported mesh vertices. The 38 active placements share seven relevant prepared resources; the remaining three terrain resources are ready for future solid FAF pieces. Generation is explicit and offline, never per frame or per scene load. Run it after an asset import change, then run `tools/dev inspect-collision`.
- A second offline preparation step saves one 58,752-triangle concave shape from the **rendered, draped** geometry of all 51 FAF terrain chunks. Its triangles match the visible surfaces after the terrain's nested transforms and drape. It is one static body; the unchanged P2 seabed remains a continuous collidable underlay. The three open-route markers were raised 0.35 m to clear the newly solid visible relief while keeping their X/Z path.
- Moved the fish's horizontal safety clamp to ±36.7 m, just inside the existing wall's inner face and fish radius. The physical world limits and upper ceiling are unchanged.
- Added `tools/dev prepare-faf-collision` to regenerate both prepared sets, `tools/dev collision-debug` for a normal Forward+ game with Godot's visible collision shapes, and `tools/dev inspect-collision` to check runtime inventory, ownership, layer/mask, enabled shapes, every prepared source triangle/vertex, world-space bounds, seabed, fish envelope, and wall/ceiling rays. Existing reef/geology checks now test the FAF collision contract and route sweeps rather than assuming P2 boxes.

Changed collision/runtime owners: `game/scenes/p2_reef_composition.tscn`, `game/scripts/ReefComposition.cs`, `game/scripts/FafGeologyWorld.cs`, `game/scripts/FafTerrainWorld.cs`, `game/scripts/FishController.cs`, ten new `game/assets/faf/collision/*.tres` resources plus `faf_draped_terrain.res`, both `game/dev/prepare_faf*_collision.gd` preparation scripts, `game/dev/world_collision_check.gd`, `game/dev/collision_native_probe.gd`, and `tools/dev`. Updated focused checks: `game/dev/reef_check.gd`, `game/dev/faf_geology_check.gd`, `game/dev/faf_terrain_world_check.gd`, `game/dev/habitat_check.gd`, and `game/dev/motor_scene_check.gd`. This report and concise BOOT/roadmap routing were updated; existing C3/control edits were retained.

## Evidence

| Check | Result |
| --- | --- |
| .NET build | **PASS**, 0 warnings/errors. |
| Full `tools/dev check` (import, build, main smoke, course/input/camera/animation, habitat, reef, terrain, geology, collision, dressing, ambient, atmosphere, water) | **PASS** after updating the habitat probe to target FAF solids and isolating its input profile. The earlier run failed on its obsolete P2 `get_rid()` assumption; that historical failure remains in ignored `artifacts/collision-native/full-check.log`. |
| Prepared resource rerun | **PASS**, all ten variant and the draped terrain SHA-256 hashes identical on rerun. |
| World collision inventory/source geometry | **PASS**: 41 static bodies, 46 enabled shapes, zero polygons, 38 geology solids and one draped-terrain solid, eight active prepared resources, no hidden P2 physics. Every prepared geology source vertex and every draped terrain triangle matches the rendered model within 1 mm after nested transforms; geology world bounds are within 5 cm. Bounds are diagnostic; source-geometry comparison provides the stronger correspondence evidence. |
| Authored route sweeps | **PASS** in headless checks: west tunnel, east cave/turn, deep canyon/return, both five-band depth lanes, open/shelter/vertical routes. Fish radius 0.48 m; camera radius 0.22 m sampled on geology routes. |
| Native Forward+ play probe | **PASS for sampled scenarios** on Godot 4.7.2 .NET, Linux/X11, Vulkan AMD Radeon 880M: real W input advanced 15.30 m in 240 physics frames, passing the historical z = −10.47 m stop; native physical sweeps cleared west tunnel, east cave, deep corridor, and depth descent. With Godot debug outlines, a 90-frame deep-zone sample measured 45 minimum / 45 median FPS. The same current scene without outlines measured 48/53 FPS; historical P3 deep corridor was 53/53 FPS. The short normal-view sample shows no clear gameplay cost, while the dense debug overlay has a visible cost. |
| Full hand-controlled close-contact swim around every object, gaps, ceiling, and all boundary sides | **NOT OBSERVED**. The native probe covered representative routes and views, not every one of 38 solids or sustained human steering. |

The historical [blocked forward lane](../c3-visibility/qualification/evidence/native/blocked-forward-lane.png) shows a pre-repair stop. The current [open-water input](evidence/open-water-after-input-debug.png), [west tunnel](evidence/west-tunnel-debug.png), and [east cave](evidence/east-cave-debug.png) show native debug collision outlines. These are different viewpoints, not pixel-matched before/after captures.

## Remaining discrepancy and qualification

The original seabed mesh and collision still share one generated surface. P3's draped FAF terrain now has its own single prepared triangle collider matching all 51 rendered chunks. The focused terrain check's **0.139 m** maximum is the difference between the visible top and the preserved **underlay**, not the exposed top collision. Where the draped terrain and continuous underlay overlap, both are intentional solid surfaces; the native route and floor checks passed with that arrangement. No collision was added to thin coral or vegetation.

The native probe verified representative openings and camera clearance numerically, but sustained hand-controlled close passes, all boundary approaches, and owner feel remain **NOT OBSERVED**. The overall request cannot receive a full PASS on the present native evidence. P3/C3 native route and visual review should be repeated before their Gate disposition changes.
