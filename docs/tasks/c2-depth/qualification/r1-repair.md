# c2-depth R1 — route and composition repair

**State: IMPLEMENTED / NOT GREEN; awaiting native owner review.** This is an R1 addendum to the unchanged [P4 FAIL](native-depth.md) and [P4 closeout](../closeout.md). It does not replace either result. Root `package.json` remains `0.2.6`.

## Starting identity and preservation

Work began 2026-11-10 on `main` at `fd29eacc89788b616f3c2fe85aebbf812662d893` (`origin/main` at inspection). Initial dirty state included `tools/asset-factory/blender/common/palette.py` and an untracked `tools/asset-factory/blender/details/` directory. Concurrent unrelated FAF detail-pack work later modified and staged further asset, documentation, proof-scene and tooling files. This correction did not reset, stage, commit, push or edit that work. Its gameplay files remained separate unstaged changes. `fd29eac` is two commits after the earlier P4 starting source `fe5a851` (`46efbd5` FAF reef fish, then `fd29eac` P4 failure record); the P4 run also used the uncommitted P3 candidate. R1 starts from the actual current tree, not from a reconstructed `fe5a851` checkout.

During validation, the unrelated detail pack was committed as `1ca8c4f123646dbe472fd18198f9a13c5d7b0019` on `main`. At the final R1 checkpoint this is HEAD, and all R1 source/evidence changes remain unstaged on top of it. The commit adds FAF detail assets and their isolated proof scene; it does not change the normal P2 habitat source used by these checks.

The Godot Editor MCP bridge reported disconnected, so source/scene edits were made on disk. The selected project was verified by `tools/dev doctor` as `/home/jfin/dev/fish/game`, Godot 4.7.2 .NET, Forward+ with the matched addon. No editor mutation was attempted.

## Route repair and measured reach

P4's mandatory primary endpoints were `(3, *, +27)` and `(3, *, -28)`; the alternate endpoints were `(24, *, +27)` and `(24, *, -28)`. `*` was a floor-derived Y. The accepted fish stopped at Z `+24.9` during native normal input. The four endpoints were outside its controller clamp even though the terrain mesh and hidden limits extended farther.

R1 authored X/Z positions (runtime Y remains `GetHeightAt(X,Z)` plus 2 m on primary and 3 m on alternate):

| Band | Primary X/Z; measured floor Y | Alternate X/Z; measured floor Y |
| --- | --- | --- |
| Shallow | `(3, 21)`; `+2.96` | `(21, 21)`; `+2.66` |
| Upper | `(3, 11)`; `+0.63` | `(21, 11)`; `-1.84` |
| Middle | `(3, 0)`; `-2.64` | `(21, -1.5)`; `-3.82` |
| Lower | `(3, -11)`; `-5.64` | `(21, -11)`; `-6.24` |
| Deep | `(3, -21)`; `-10.01` | `(21, -21)`; `-8.94` |

`inspect-reef` now places the real fish beyond each X/Z side, advances its normal physics process and reads back `-24.90 / +24.90 m` on both axes. Every mandatory marker must retain at least **3.7 m** center margin inside those measured limits. The tightest authored margin is **3.9 m** (alternate X and both Z endpoints); the primary X margin is 21.9 m. The check rejects synthetic points beyond the measured clamp, validates fish-radius and camera probes along each lane, and moves the accepted collision body through both authored lanes **and back**. Tunnel, shelter, open and vertical paths remain checked. This prevents another marker-inside-mesh/outside-fish-reach error; it does not replace owner hand swimming or prove camera comfort.

## Composition and water presentation

`SeabedSurface.GetHeightAt()` remains one continuous meshed/collidable analytic surface. Its four transitions now use different oblique directions, widths and phases. Broader folds, a west shoulder, an east channel, a local cut and staggered ridges interrupt the old parallel shelf read while retaining five distinct heights. The old uniform 19 m rise at the outer edge is replaced with a lower, irregular 4 m nominal rise with local variation; hidden side limits still contain the player. The existing boulder anchor moved from X 19.7 to 17.5 to clear the alternate route, and shelter roof clearance increased from 4.4 to 5.2 m above its new floor. All nine prepared source identities, ten rock and eight coral instances remain.

The existing overhead surface shader now bypasses scene fog only on that surface, receives the camera-depth scalar already computed by `UnderwaterAtmosphere`, and adds bounded upward transmission and sun halo at depth. Its finite edge fades toward the existing sky instead of drawing a hard surface cutoff. Distance/height/volumetric fog for horizontal geometry, world-height seabed lighting, shaft/caustic depth attenuation, GFX base values and preset isolation are unchanged. No light, fog volume, extra surface, or per-frame mesh regeneration was added. A local native diagnostic [several-band frame](../../../../artifacts/c2-depth-r1/multiband.png) shows the high continuous terrain rim removed; [deep upward frame](../../../../artifacts/c2-depth-r1/deep-upward.png) shows a bright overhead source. These fixed diagnostic cameras do **not** establish naturalness or owner acceptance; the surface/sky transition remains for owner judgment.

## Focused checks

- `tools/dev doctor`, `tools/dev import`, `tools/dev build`: passed; build 0 warnings/errors.
- `tools/dev inspect-habitat`: passed; 202 collision/height samples, one two-sided seabed, five ordered center heights `+2.95 / +0.24 / -2.90 / -5.41 / -9.61`, edge/underside/upper containment, accepted fish/tuning and P1 fixture.
- `tools/dev inspect-reef`: passed; measured fish clamp, mandatory marker margin, authored forward/return movement of the accepted collision body, alternate route, tunnel/shelter/camera clearance, all prepared source identities. Imported prepared meshes remain 1,663,394 triangles.
- `tools/dev inspect-atmosphere`, `tools/dev inspect-depth`, `tools/dev inspect-water`: passed; one real light, one caustic, one shaft mesh, surface/sky linkage, water column anchors, caustic deep exclusion, paused/toggled presentation, equal-intent movement, GFX/preset base isolation and unchanged fish state. Deep fog/range remain `0.0126 / 43.5 m`, versus shallow `0.0035 / 64.0 m`.
- `FishController.cs`, `SwimMotor.cs`, `FollowCamera.cs`, `swim_tuning.tres` and tracked Forward+ setting were not edited. The accepted tuning SHA-256 remains `6d97d70d7b280d2491d08378d1c3e54cdeca1ca14aa245055bec967943e8e2d7`.
- `git diff --check`: passed for current tracked changes at this checkpoint. Structural checks cannot prove final depth appearance.

## Bounded native performance diagnostic

Two Forward+ `tools/dev measure-atmosphere` runs used the current R1 route markers, AMD Radeon 880M and 1920 × 1200. With 120 warmup and 180 sampled frames per view/mode, shallow atmosphere on/off medians were `18.10 / 16.82 ms` then `17.73 / 16.81 ms`; deep were `20.15 / 16.85 ms` then `18.88 / 16.75 ms`. The deep full-atmosphere on/off cost was `19.6%` then `12.7%`, with three more draws in that view. This compares the whole atmosphere against its diagnostic bypass, **not R1 against P4 on the same camera**. The current views and geometry differ from P4's capped reachable +X comparison, so these samples cannot establish an R1-specific regression or dismiss the slower deep result. No broad graphics tuning was opened; repeatable performance in the owner route remains part of native review.

## Required native owner checkpoint

Run the actual authored primary shallow → upper → middle → lower → deep route, return deep → shallow, complete the authored alternate transition, and reach tunnel/shelter under normal controls in Forward+. Inspect a several-band view, comparable shallow/middle/deep horizontal views and the deep upward surface. The owner must judge route usability, natural five-zone geology, perimeter read, cooler/dimmer lower terrain, progressively shorter horizontal visibility, detail → silhouette → water, warm-color loss, caustic/shaft fade, distinct bright deep surface, movement/camera comfort, UI/GFX clarity, overall effect and representative performance. Until that direct judgment, **R1 NOT GREEN**, c2-depth remains open, and P2 G2.1–G2.3 remain open.
