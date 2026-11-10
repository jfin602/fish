# c2-depth P1 — structural basin evidence

**Status:** P1 implemented at pre-task HEAD `538dc51ba0d9b121bb6c5b41e11b6a549db8d6a0`, with runner-owned implementation changes left uncommitted. This is T2 structural evidence, not native visual acceptance. Root version remains `0.2.6`.

## Geometry and routes

`SeabedSurface` still builds one 84 × 84 m mesh and its matching two-sided trimesh collision. The authored height function now uses five broad, unequal south-to-north elevation bands, wandering transition contours, local shoulders and a channel, plus an outer rise into the hidden side limits. It retains no separate shelf meshes or collision seams.

| Band | Sample X/Z (m) | `GetHeightAt` Y (m) |
| --- | --- | ---: |
| Shallow | (0, 27) | +3.57 |
| Upper | (0, 14) | +0.04 |
| Middle | (0, 0) | -2.90 |
| Lower | (0, -14) | -5.98 |
| Deep | (0, -28) | -9.60 |

The primary route marks these five bands near X 3; an alternate east route near X 24 traverses them as well. Markers now derive their runtime Y from `GetHeightAt`, with per-marker clearance where needed. The ten prepared rock and eight coral anchors also follow the changed floor; all nine prepared source IDs remain. The existing open, tunnel, shelter and vertical routes stay present. The alternate lane uses 3 m floor clearance because a 2 m line segment intersected the rising terrain between its markers. The hidden limits remain five collision-only shapes. Spawn remains at `(0, 0, 0)` with clear fish and camera probes.

## T2 checks

- `tools/dev doctor`: Fish Godot 4.7.2 .NET project, .NET SDK 10.0.403 and matched MCP addon identified. The editor bridge was offline; no editor mutation occurred.
- `tools/dev build`: passed, zero warnings and errors.
- `XDG_DATA_HOME=/tmp/fish-xdg-data XDG_CONFIG_HOME=/tmp/fish-xdg-config tools/dev inspect-habitat`: passed. It checked 202 grid and off-grid downward collision samples against `GetHeightAt`, five ordered bands, sampled transition slopes on both lanes, clear spawn/camera, two-sided underside, outer edge and upper containment, accepted fish/tuning identity, and the P1 fixture.
- The same environment with `tools/dev inspect-reef`: passed. It checked all prepared source identities, 10 rock and 8 coral anchors, tunnel/shelter/vertical/open paths, both five-band routes and fish/camera clearance along sampled segments. The prepared models total 1,663,394 imported triangles, below the existing 1,750,000 bound.
- Direct headless main-scene `--smoke`: passed (`FISH_SMOKE_OK`, `FISH_MOTOR_CHECK_OK`).
- `git diff --check`: passed. `swim_tuning.tres` retained SHA-256 `6d97d70d7b280d2491d08378d1c3e54cdeca1ca14aa245055bec967943e8e2d7`. No motor, camera, tuning, renderer or project-version source changed.

`tools/dev import` was attempted but its editor process could not create local sockets inside this sandbox (Godot IDE messaging and the MCP bridge reported permission denied). Direct scene loading, build and the focused checks above succeeded using writable XDG paths. Import remains a validation gap for an unrestricted environment.

Headless collision and route checks do not establish that the ledges look natural from a real camera. P2/P3 add water-column response; P4 owns real-window traversal, visual composition, and comfort review before correction closeout.

## P2 — water-column extinction

**Status:** Implemented against pre-task HEAD `131f8c372f4a5446b5d3dfaad80be880a1452e3e`; runner owns the implementation commit. Root version remains `0.2.6`. The Editor MCP bridge was offline, so evidence below is headless T2, not a native visual judgment.

`UnderwaterAtmosphere` uses its existing visual surface, derived from the upper collision, as depth zero (Y `10.39` in the current habitat). It samples the authored deep traversal shelf at `(0, -28)` through `SeabedSurface.GetHeightAt()` and adds 2 m camera clearance, giving a deep reference near Y `-7.60`. The active viewport camera is read only; normalized depth is clamped to `[0, 1]` and eased with smoothstep. Height fog now anchors to this same column, near Y `-1.40` with the current base offset.

Base `LightingSettings` values and preset JSON remain unchanged. Effective distance and volumetric density, volumetric range, sun/ambient energy, and fog/background/ambient colors are recomputed from base plus depth. The existing procedural sky also receives one depth scalar so its visible horizon cools with the environment; its upward sun cue remains. Shallow depth reproduces the owner-green c2 base values. The calculation runs in the existing atmosphere process, uses scalar/struct operations, and skips depth changes below `0.001`. Applying or switching a preset calls the same calculation immediately, including while the game is paused.

The focused `tools/dev inspect-depth` check creates shallow/middle/deep camera samples and checks monotonic extinction, bounded and cooler colors, clamping beyond both anchors, base-value preservation, paused deep preset application/switching, JSON reload, deep toggle restoration, and unchanged fish/tuning/camera identity and physical state. Effective samples below list distance fog, volumetric density, volumetric range, sun energy and ambient energy:

| Camera Y | Depth | Distance fog | Volumetric density | Range | Sun | Ambient |
| ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| `10.39` | 0 | `0.0035` | `0.024` | `64.0` | `1.25` | `0.42` |
| `1.39` | 0.5 | `0.0081` | `0.039` | `53.8` | `0.84` | `0.31` |
| `-7.60` | 1 | `0.0126` | `0.054` | `43.5` | `0.44` | `0.20` |

T2: `tools/dev build` passed with zero warnings/errors; headless main `--smoke`, `tools/dev inspect-habitat`, `tools/dev inspect-atmosphere` (including movement parity, pause, toggle), and `tools/dev inspect-depth` passed. Each Godot run used isolated writable XDG data/config paths. `git diff --check` passed. There is no real-window evidence yet for fog comfort, view range or light balance. P3 must add within-frame world-depth cues and attenuate c2 surface-light effects before P4 native review.

## P3 — world-depth lighting and c2 effect integration

**Status:** Implemented against pre-task HEAD `bc73d7f39474a12ebad9dee98b45a3f9b9080be9`, uncommitted for the phase runner. Root remains `0.2.6`. This is T2 structural evidence; P4 owns native visual and performance judgment.

The existing surface Y and authored deep camera reference now feed both the continuous seabed material and the one batched shaft material. The seabed shader uses each fragment's world Y for a smooth, bounded cool/dim multiplier; its four texture weights, normals and roughness selection stay intact, and the surface end is exactly the accepted shallow color. The existing continuous Environment height fog remains the common world-space treatment for imported rock/coral materials. It is anchored to the same water column and falls continuously below the midwater start; no imported material replacement or horizontal shelf bands were added. Camera-depth fog, sun and ambient behavior from P2 remains separately active.

The shaft shader interpolates world Y per fragment and takes deep transmission down to `0.12` of its shallow contribution before its existing edge, top and ground fades. The ribbon geometry, sun alignment, six active shafts in one shadowless mesh and GFX brightness/count controls remain. The single organic caustic decal is centered at Y 3 with 22 m vertical size and a 0.30 lower fade, so its lower edge is Y -8: the deep shelf near Y -9.6 receives no direct projection, while lower shelves fade into the water-column fog. Its texture, drift, upper coverage and paired shaft translation remain unchanged. The surface, motes, screen shader, UI layers and movement-reactive particles were not modified.

The shallow/deep A/B probe now uses the authored `DepthShallow` and `DepthDeep` route markers. It keeps one scene and revision, 120 warmup and 180 sample frames per fixed view/mode, reporting median FPS, frame time, draw calls and primitives plus on/off deltas. “Off” is the existing full atmosphere diagnostic toggle; the seabed material remains loaded in both modes. The native probe was attempted but **could not run**: X11 was unavailable and Wayland could not connect, so there are no honest frame or rendering-counter values for this revision. The Godot Editor MCP bridge also reported disconnected. P4 must run the probe on a real Forward+ display, compare the shallow/deep pairs and tune any repeatable regression before closeout.

Focused validation: `tools/dev build` passed with zero warnings/errors; `inspect-habitat`, `inspect-reef`, `inspect-atmosphere` (including equal-route movement), `inspect-depth` (including saved preset isolation), and `inspect-water` passed under writable XDG paths. The intentional missing optional screen-shader warning in the atmosphere negative case was expected. New checks confirm finite common depth anchors on seabed and shaft materials, preserved four material channels, bounded deep shaft transmission, one caustic decal, one shaft mesh/material, one real light, caustic exclusion at the deep shelf, existing GFX/preset UI and unchanged fish/camera/tuning behavior. `git diff --check` passed. No package-lock or vendor payload was introduced by this P3 work; unrelated concurrent coral and FAF work in the working tree was preserved.

## P4 native correction to earlier structural claims

The [native P4 observation](native-depth.md) found a real reachability defect in the P1 route evidence: `FishController` clamps horizontal coordinates to ±24.9 m, but the shallow and deep markers on both lanes are at Z +27 and -28 m. The earlier `inspect-reef` route segments and `inspect-habitat` height samples validated collision and numeric shelf ordering without moving the authoritative fish through those endpoints. Therefore “five traversable bands” in the P1–P3 sections above means **structural candidate**, not qualified full normal-play traversal. A scripted fish can cross five reachable proxy elevations at Z ±24 and return, but it cannot reach the four authored endpoint markers. Preserve this failed native result until the route and its focused checks are repaired.

The P3 probe's original shallow/deep camera positions also sit outside or aim beyond the authoritative play span. P4 retained its measured output as a diagnostic and added a same-revision reachable-view A/B; neither toggles depth math alone. The P4 visual record shows cool/dim depth direction but unresolved terrace/perimeter appearance, unproven full distance fade and a weak deep overhead-surface cue. The correction and G2.1/G2.2 remain open; P3's headless PASS is not native acceptance.
