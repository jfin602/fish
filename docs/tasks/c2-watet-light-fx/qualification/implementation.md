# Correction 2 P1 — overhead water surface implementation

Date: 2026-11-10. Pre-task checkout: clean `main` at `c234b4125aefbf952bf69d10ddfdc6508c5b464a`; root `package.json` version `0.2.6`. All implementation changes remain uncommitted for the phase runner.

## Implemented boundary

`UnderwaterAtmosphere` now adds one 77 × 77 m `PlaneMesh` with 32 subdivisions per axis (about 1,089 vertices and 2,048 triangles) and one underside-only translucent shader material. Two slow wave components displace vertices by at most 0.165 m. The plane center is derived at runtime from `Habitat/HiddenLimits/Upper`: its saved box top is Y 10.05 and the visual plane center is Y 10.27, keeping its lowest wave at least 0.055 m above the collision top. Its custom visibility box includes the full wave range. No collision node, body or movement force was added.

The same atmosphere owns a simple shader sky with a bright sun cue. Both shaders receive the direction of the existing `Sun` light; the sky leaves horizontal views blue and brightens upward views. The surface uses turquoise transmission, ripple variation and restrained Fresnel response. The surface shader uses atmosphere-supplied presentation time, so tree pause freezes waves. `SetEnabled` hides the effects subtree and switches the sky back to the prior flat color for diagnostic comparison. Fish, camera, tuning, input, habitat collision and renderer files were not edited.

## Executed checks and limits

| Command/check | Result |
| --- | --- |
| `./tools/dev doctor` | Godot 4.7.2 .NET, .NET SDK 10.0.403 and Fish project files confirmed. Godot Editor MCP 2026.11.04 reported no connected editor peer, so edits were made on disk. |
| `./tools/dev import` | **Blocked by the workspace sandbox**: Godot editor/.NET and MCP addon could not open local sockets, and editor settings outside writable roots could not be saved. The command reported engine errors. This does not qualify shader rendering or native appearance. |
| `./tools/dev build` | Final build passed with zero warnings and zero errors. An initial build failed on an incorrect `Environment.BackgroundSky` property name; corrected to the Godot 4.7 `Environment.Sky` API before the passing build. |
| `env XDG_DATA_HOME=/tmp/fish-c2-data XDG_CONFIG_HOME=/tmp/fish-c2-config ./tools/dev inspect-scene res://scenes/main.tscn` and `res://scenes/p2_habitat.tscn` | Both returned `SCENE_OK`. |
| `env XDG_DATA_HOME=/tmp/fish-c2-data XDG_CONFIG_HOME=/tmp/fish-c2-config ./tools/dev inspect-atmosphere` | `ATMOSPHERE_CHECK_OK` and `ATMOSPHERE_MOVEMENT_OK`. The focused check verifies the sky and surface shaders load, their sun directions match the existing light, surface footprint/height/subdivision bounds agree with the saved upper limit, no additional physics body exists, the toggle controls surface/sky, pause freezes surface time, UI remains above the screen effect, and immediate fish/camera/tuning state is unchanged. The movement check compares equal 30-step routes with atmosphere enabled and disabled. The intentional optional-screen-shader warning in its negative case was observed. An initial GDScript static type error in the new assertion was corrected before the passing check. |
| `git diff --check`, `git status --short`, version/renderer and payload search | Whitespace check passed; only the listed source, shaders with Godot UID sidecars, module map and this evidence file are changed. Tracked renderer remains `forward_plus`, root version remains `0.2.6`, and no package lock or vendor payload appeared. |

Native visual quality, actual shader appearance, motion feel and performance are **NOT OBSERVED** here. P4 retains the real-window and owner checkpoint. P2 Gate status is unchanged.

## Correction 2 P2 — organic caustics

Date: 2026-11-10. Pre-task checkout: clean `main` at `f75a1d8c3597fb7e520e961473e6f33b959fc09c`; root version `0.2.6` and saved Forward+ renderer. Changes remain uncommitted for the phase runner.

The production `MakeCausticTexture()` no longer combines periodic sine lattices. It generates a deterministic 256² texture once per atmosphere initialization from jittered cellular sites, a rotated and warped sample field, the nearest/second-nearest distance ridge, low-frequency gap modulation, and per-cell brightness. One decal still owns the projection. Its 100 × 24 × 100 m volume covers the 84 m habitat after a small off-axis rotation and bounded sub-metre drift; the focused check transforms reef anchors into decal-local coordinates. Emission energy is reduced from 0.45 to 0.32. Drift remains a presentation-only `_Process` update, frozen by tree pause and the atmosphere toggle. No seabed material, collision, fish, camera, tuning, or project setting changed.

| Command/check | Result |
| --- | --- |
| `./tools/dev doctor` | Fish project, Godot 4.7.2 .NET, .NET 10.0.403 and MCP addon 2026.11.04 confirmed. MCP health reported no connected editor peer and a local bridge bind error, so no editor mutation was attempted. |
| `env XDG_DATA_HOME=/tmp/fish-c2-data XDG_CONFIG_HOME=/tmp/fish-c2-config ./tools/dev import` | **Blocked** by sandbox denial of local sockets used by the Godot .NET editor and MCP addon. The command exited 1 after engine socket errors; import success is not claimed. |
| `./tools/dev build` | Final build passed with zero warnings/errors. The initial focused check exposed that rotation was set only by the first process tick; the decal now has the intended initial rotation before play. |
| `env XDG_DATA_HOME=/tmp/fish-c2-data XDG_CONFIG_HOME=/tmp/fish-c2-config ./tools/dev inspect-atmosphere` | Final `ATMOSPHERE_CHECK_OK` and `ATMOSPHERE_MOVEMENT_OK`. Checks verify one bounded rotated projection, finite energy/drift, generated texture with lit/dark samples, identical texture bytes across two scene instances, transformed rock/coral bounds, pause/toggle freezing and resumption, and equal 30-step fish routes with effects on/off. The missing optional screen shader warning is the expected negative case. |
| `git diff --check`, status, version/renderer and source review | Whitespace check passed. `package.json` is `0.2.6`, `project.godot` names Fish and selects Forward+, and fish/camera/tuning/project diffs are empty. During final status review, unrelated `tools/asset-factory` changes and untracked files appeared after the initially clean checkout; they were not edited or validated by this P2 task and must be kept out of its runner-owned commit. |

Organic appearance, moving pattern quality, coral/fish readability, and native cost are **NOT OBSERVED**. Structural checks do not certify those judgments; P4 retains them. P3 remains the next implementation step, and P2 Gate status remains open.
