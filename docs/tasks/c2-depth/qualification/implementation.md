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
