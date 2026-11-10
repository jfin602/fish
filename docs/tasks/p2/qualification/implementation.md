# P2 implementation evidence

## P2 P1 — Renderer comparison vertical slice

Date: 2026-11-09. Source: clean `main` at `fcd9428b92760c0e55baed5be09eece6f0915f80`, root `0.2.0` before this task. [P1 closeout](../../p1/closeout.md) and [accepted tuning](../../p1/qualification/accepted-tuning.md) established the inherited baseline. No P1 motor, camera, input, tuning, scene or asset source was modified. The Godot Editor MCP server was version `2026.11.04` but its editor bridge was disconnected; no editor mutation was attempted. Scene and shader files were authored on disk and checked with the installed Godot 4.7.2 .NET CLI.

### Implementation

- Godot 4.7.2 `--help` exposes `--rendering-method <renderer>` and `--scene <path>`. `tools/dev compare compatibility` maps to `gl_compatibility`; `tools/dev compare forward-plus` maps to `forward_plus`. Both launch `res://scenes/renderer_comparison.tscn`, which instances the exact P1 `main.tscn`. The override is process-local; tracked `game/project.godot` remains `gl_compatibility`. `--effects=off` is an optional diagnostic argument.
- `RendererComparison.cs` adds presentation after P1 main and `StarterWorld` finish initialization. The P1 sand floor, Fish3 player, camera, course, rock, coral, display and UI stay in place. The bounded extension adds one Rock Face 02 and one Acropora palmata, fog/distance color, existing top directional light, animated caustic overlay and 24 CPU motes. Forward+ alone loads a screen-copy tint shader on CanvasLayer 0, below P1 help and pause UI. Compatibility does not load this resource. The slice is diagnostic; normal play still uses `main.tscn`.
- The check mode loads and instantiates all seven added P2 mesh packages, loads all 15 Ground060/Gravel041/Rock063 maps, checks three Poly Haven binary dependencies and loads their nine texture maps. P1 main separately checks Fish3, Coast Sand 01, Rock Face 01 and Heliopora. These source files reside in ignored `vendor-local` storage and match the local inventory; the comparison scene does not place the full prepared palette.
- Effect toggling changes only fog and presentation visibility. A focused runtime assertion checks fish position, velocity and tuning identity, camera identity, one fish and one camera, finite/bounded shader and fog values, the mote count, and UI layer order. It does not replace a moving-frame native test.

### Checks and outcomes

| Check | Actual command or source | Result |
| --- | --- | --- |
| Toolchain and CLI | `./tools/dev doctor`; installed Godot 4.7.2 .NET `--help` | Godot 4.7.2 mono and .NET 10.0.403; CLI flags confirmed. |
| Import and build | `./tools/dev import`; `./tools/dev build` | Both passed; final build had zero warnings and zero errors. |
| Saved scene | `./tools/dev inspect-scene res://scenes/renderer_comparison.tscn` | `SCENE_OK`, one instanced main child. |
| Compatibility path | Godot CLI with `--path game --rendering-method gl_compatibility --scene res://scenes/renderer_comparison.tscn -- --comparison-check`, under `xvfb-run -a` | `RENDER_COMPARISON_OK`; method `gl_compatibility`, driver `opengl3`, Mesa llvmpipe. |
| Forward+ path | Same CLI with `--rendering-method forward_plus`, under `xvfb-run -a` | `RENDER_COMPARISON_OK`; method `forward_plus`, driver `vulkan`, Mesa llvmpipe. |
| Effects off/restoration | Compatibility CLI with `--comparison-check --comparison-effects=off`; source inspection of `tools/dev` and unchanged `game/project.godot` | Off path passed; launch override has no tracked renderer edit. |
| Resource and state checks | `RendererComparison.CheckPreparedAssets` and `CheckIsolation`, invoked by both comparison checks | Prepared paths loaded; shader resources loaded on applicable path; one fish/camera; state unchanged at effect toggle; UI above screen treatment. |

The Xvfb runs used software renderers and reported X input-method / V-Sync warnings (Compatibility) and missing DRI3 presentation support (Forward+). They establish startup, resource, shader and source-level isolation on this host, not native visual parity, actual display behavior, gameplay feel or representative hardware performance. P2 must run both `./tools/dev compare` paths in the real target desktop window, compare identical viewpoints/materials and movement, capture errors and frame timing, and record the renderer decision in its own evidence. No renderer decision or G2 Gate pass is recorded here.

## P2 P3 — Natural habitat foundation

Date: 2026-11-09. Starting checkout: clean `main` at `d1151788ecc598d16e541fc4a09a7eb8e84bffe3`, root `0.2.2`, saved `forward_plus` method and `Forward Plus` feature. The [P2 renderer decision](renderer-decision.md) remains in force. Godot Editor MCP server version `2026.11.04` had no connected Fish editor peer; the project path and Godot .NET CLI were checked before on-disk scene edits. No editor mutation was attempted. The ignored P2 inventory was SHA-256 `391099f0febe9ce04daa9be8d7614ae32b90f89d6c25f12f57aeda9cdee681d0`; all 34 runtime files across 10 prepared entries were present. `git ls-files` returned no `game/assets/vendor-local/` or `artifacts/assets/source/p2-environment/` payload.

### Implemented boundaries

- `main.tscn` now instances `p2_habitat.tscn` and retains the exact P1 fish/collision/camera/control node tail. The original P1 main bytes are preserved as `p1_course_fixture.tscn` (SHA-256 `8461741933c12ced142ac26fc43cff6a391df7328d8c6e4ccd9872cc4434f403`). The renderer comparison and course/camera/motor/animation regression scripts use this dedicated fixture, including its unchanged `movement_course.tscn` instance. Normal play no longer instances P1 target spheres, camera-check post, prototype shelf or retaining walls.
- `p2_habitat.tscn` owns the spawn at `(0, 0, 0)`, a `Seabed` static body with visual mesh and matching concave collision, five collision-only `HiddenLimits` shapes and an empty `Composition` node. `SeabedSurface.cs` builds one continuous 84 × 84 m authored heightfield at 1.5 m cells, with gentle central variation and a rising exposed-rock perimeter. Four material surfaces select Coast Sand 01, Ground060 shell sand, Gravel041 gravel and Rock063 rock using regional placement hooks. `HabitatPalette.cs` validates the prepared color/normal/roughness maps and additional supplied ambient-occlusion/displacement files; missing or invalid resources produce explicit diagnostics. P4 can reuse the palette and `Composition` root for placed forms without replacing the foundation.
- `PlayerArt.cs` now owns shared Fish3 rig/clip/material verification and visual attachment. `StarterWorld.cs` retains P1 fixture dressing and diagnostics; `P2HabitatWorld.cs` owns normal habitat readiness/spawn and simple environment color. Neither changes `FishController`, `SwimMotor`, `FollowCamera`, input or tuning. The accepted `swim_tuning.tres`, `FishController.cs` and `FollowCamera.cs` SHA-256 values remain `6d97d70d7b280d2491d08378d1c3e54cdeca1ca14aa245055bec967943e8e2d7`, `8bffbcdd85547b48a571445a0983b0324e63f9f926893502495c12ab0b0ce9da` and `a77e8162f98b4b4a37aa1e60c962887c472fe1f7a369c364eac95a247000cc81`.

### Checks and limitations

| Check | Result |
| --- | --- |
| `./tools/dev check` | Passed import, build (zero warnings/errors), normal main smoke/motor, scene automation, missing-scene negative case, input lifecycle, P1 fixture camera and animation, and new habitat physics checks. The animation fixture sample was extended from 20 to 24 physics frames to let the unchanged swim activity rise above its existing threshold. |
| `./tools/dev inspect-scene` on `main.tscn`, `p2_habitat.tscn`, `p1_course_fixture.tscn`, `renderer_comparison.tscn`; `./tools/dev inspect-course` | All loaded; preserved course marker/rock clearance and fixture resource check passed. |
| `./tools/dev inspect-habitat` | `HABITAT_CHECK_OK`: normal scene/fixture split, Fish3 and current camera, four material surfaces, safe fish/camera spawn, five representative seabed ray hits with height variation, actual W movement, downward and underside mesh collision, horizontal rim/edge collision and upper-limit containment. Fish/seabed/limits use layer 1; fish mask is 1. |
| Godot .NET headless comparison with `--rendering-method forward_plus --scene res://scenes/renderer_comparison.tscn -- --comparison-check` | `RENDER_COMPARISON_OK`, Vulkan `forward_plus`, prepared assets and effect isolation; P1 comparison remains inspectable. |
| Native `./tools/dev run` on Linux/X11 1920×1200, AMD Radeon 880M | Godot 4.7.2 .NET reported Vulkan Forward+ and rendered Fish3 above the new seabed without P1 markers. An initial desktop view revealed the mesh face winding was reversed; it was corrected, default face culling restored, and the final native view showed the continuous sand/gravel/rock basin. The final window had no stdout engine/script/resource errors. This spot check does not establish V18 traversal or final appearance. |
| `git diff --check`; tracked-asset listing; baseline hashes | Passed; no vendor payload tracked and P1 movement/camera/tuning files unchanged. |

The structural perimeter is a high exposed-rock slope with collision-only outer/upper limits. In the current clear P3 lighting it reads as a stark basin and material zone boundaries are visible. P4 must compose natural rock/coral forms, tunnels and landmarks across it; P5 adds underwater attenuation; P7 must judge the completed native look, camera clearance and routes. No G2.1, G2.2 or V18 pass is claimed here. Root version advances to `0.2.3` for this reproducible foundation.
