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
