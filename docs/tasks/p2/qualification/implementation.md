# P2 P1 — Renderer comparison implementation evidence

Date: 2026-11-09. Source: clean `main` at `fcd9428b92760c0e55baed5be09eece6f0915f80`, root `0.2.0` before this task. [P1 closeout](../../p1/closeout.md) and [accepted tuning](../../p1/qualification/accepted-tuning.md) established the inherited baseline. No P1 motor, camera, input, tuning, scene or asset source was modified. The Godot Editor MCP server was version `2026.11.04` but its editor bridge was disconnected; no editor mutation was attempted. Scene and shader files were authored on disk and checked with the installed Godot 4.7.2 .NET CLI.

## Implementation

- Godot 4.7.2 `--help` exposes `--rendering-method <renderer>` and `--scene <path>`. `tools/dev compare compatibility` maps to `gl_compatibility`; `tools/dev compare forward-plus` maps to `forward_plus`. Both launch `res://scenes/renderer_comparison.tscn`, which instances the exact P1 `main.tscn`. The override is process-local; tracked `game/project.godot` remains `gl_compatibility`. `--effects=off` is an optional diagnostic argument.
- `RendererComparison.cs` adds presentation after P1 main and `StarterWorld` finish initialization. The P1 sand floor, Fish3 player, camera, course, rock, coral, display and UI stay in place. The bounded extension adds one Rock Face 02 and one Acropora palmata, fog/distance color, existing top directional light, animated caustic overlay and 24 CPU motes. Forward+ alone loads a screen-copy tint shader on CanvasLayer 0, below P1 help and pause UI. Compatibility does not load this resource. The slice is diagnostic; normal play still uses `main.tscn`.
- The check mode loads and instantiates all seven added P2 mesh packages, loads all 15 Ground060/Gravel041/Rock063 maps, checks three Poly Haven binary dependencies and loads their nine texture maps. P1 main separately checks Fish3, Coast Sand 01, Rock Face 01 and Heliopora. These source files reside in ignored `vendor-local` storage and match the local inventory; the comparison scene does not place the full prepared palette.
- Effect toggling changes only fog and presentation visibility. A focused runtime assertion checks fish position, velocity and tuning identity, camera identity, one fish and one camera, finite/bounded shader and fog values, the mote count, and UI layer order. It does not replace a moving-frame native test.

## Checks and outcomes

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
