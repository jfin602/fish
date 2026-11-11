# Development setup

**Current state:** P0's Godot .NET 3D foundation and developer CLI are implemented and qualified on the owner's Linux/X11 desktop. [P0 implementation](tasks/p0/qualification/implementation.md), [native playtest](tasks/p0/qualification/native-playtest.md), and [closeout](tasks/p0/closeout.md) record the actual evidence. Survival gameplay and exports remain later work.

## Reference pins

The inspected Space Factory [development setup](https://github.com/jfin602/space-factory/blob/ce163753d8973ccd018f2344c6175c52fe4b113d/docs/development.md), [C# project](https://github.com/jfin602/space-factory/blob/ce163753d8973ccd018f2344c6175c52fe4b113d/game/SpaceFactory.csproj), and [global.json](https://github.com/jfin602/space-factory/blob/ce163753d8973ccd018f2344c6175c52fe4b113d/game/global.json) establish this starting point:

| Component | Reference / intended bootstrap value |
| --- | --- |
| Godot editor | 4.7.2 .NET, not the standard non-.NET edition. |
| `Godot.NET.Sdk` | 4.7.2. |
| .NET SDK | 10.0.403, explicit `global.json` with `rollForward: disable`. |
| C# target | `net10.0`. |
| Renderer | Compatibility for the initial simple 3D prototype; final rendering remains open. |
| Godot Editor MCP | Stable release tag `2026.11.04`, Python package version `2026.11.4`, matching release addon. |
| Runner | Node.js 24 (`>=24 <25`); no npm dependencies. |

The local P0 run used Godot `4.7.2.stable.mono.official.ed1daf0bf`, `Godot.NET.Sdk/4.7.2`, .NET SDK `10.0.403` and `net10.0` without a pin deviation. The editor is the existing user-local .NET build; no system default or Space Factory setup changed. Future machines must inspect their installations rather than treat these local paths as portable.

## Commands available now

Run these from the repository root:

```bash
npm run fish:phase:validate -- p0
npm run fish:phase:validate -- p1
npm run test:runner
```

Execution also requires a locally available/authenticated Codex CLI:

```bash
npm run fish:phase -- p0
```

The root `package.json` is private development metadata; P0 began at `0.0.0` and the passing direct closeout reaches `0.0.3`. It is independent of engine pins and public release versions. Do not add a root package lock or npm dependencies to run the inherited tools. [The runner guide](phase-runner.md) owns its exact contract.

The completed direct P0 stack assigned checkpoint `0.0.3`. To use the Godot project from any working directory:

```bash
/home/jfin/dev/fish/tools/dev editor
/home/jfin/dev/fish/tools/dev run
```

From this checkout, `./tools/dev editor` and `./tools/dev run` are equivalent. `run` builds C# before opening the game; `editor` opens the .NET editor. The ignored `tools/local.env` on this desktop supplies `GODOT_CMD` as an argument array, `DOTNET_BIN`, and `GODOT_MCP_BRIDGE_URL`. On another machine, create that file with local executable paths; it is deliberately not committed. `tools/dev doctor` reports the selected tools and display. `tools/dev check` imports, builds once, runs a bounded C# main-scene smoke, and tests saved-scene automation/missing-scene failure. `inspect-scene <path>` accepts `res://`, `game/`-relative, or an absolute path under this checkout's `game/`.

## Developer CLI implemented in P0

`tools/dev` resolves the repository/project from its own location, uses quoted arguments, and fails on missing prerequisites. It reads ignored `tools/local.env` for local paths; `GODOT_CMD` is an argument array and `DOTNET_BIN` is invoked from `game/` so `global.json` selects the SDK.

| Command | Current behavior |
| --- | --- |
| `./tools/dev doctor` | Report actual Godot version/.NET capability, SDK, project/config readiness and display availability. |
| `./tools/dev import` | Import the Fish project and wait for completion; fail on engine errors/timeouts. |
| `./tools/dev build` | Compile `game/Fish.csproj` with the pinned SDK. |
| `./tools/dev check` | Compose import, build, generated-world C# smoke, scene automation, missing-scene failure, P1 camera/animation fixtures, FAF asset load, generated route clearance and multi-seed placement checks. The old authored P3 terrain/geology/dressing probes remain historical commands and are not part of this generated-world gate. |
| `./tools/dev smoke-world` | Run the generated main scene and motor smoke with an isolated user-data profile. |
| `./tools/dev inspect-placement` | Verify same/different seed placement, all 38 variants, support, route clearance, spacing, depth density and major meshes/collision. |
| `./tools/dev editor` | Open the Fish editor, forwarding the Fish bridge environment after P2. |
| `./tools/dev run [--display-mode=fullscreen|--display-mode=windowed-fullscreen]` | Build and launch the real Fish main scene. The optional, validated game-owned argument follows Godot's `--` separator and does not save a preference. |
| `./tools/dev compare compatibility` or `./tools/dev compare forward-plus` | Build and launch the preserved P1 fixture inside the bounded P2 rendering slice. Godot 4.7.2 uses `--rendering-method gl_compatibility` / `forward_plus` for one process; the saved production method remains Forward+. Add `--effects=off` to either command to hide comparison effects and fog. |
| `./tools/dev inspect-scene <path>` | Inspect a real saved scene; missing paths fail nonzero. |
| `./tools/dev inspect-geology` | Check live FAF geology IDs/placement, two covered swim-throughs, cave turn space, deep corridor/return, visual bounds and camera obstruction recovery. |
| `./tools/dev inspect-generated-geology` | Check C3 generated geological mesh topology, deterministic collision identity, named probes, physics clearance, fish traversal and camera obstruction. |
| `./tools/dev inspect-atmosphere` | Check tracked Forward+ settings, normal atmosphere resources and bounds, UI ordering, resize/pause safety, optional screen resource behavior and equal fish movement with effects on/off. |
| `./tools/dev automation-smoke` | Create/edit/save/reload/assert/remove a disposable scene using Godot APIs. |

Generated command logs live under ignored `artifacts/logs/`; relevant UID sidecars are tracked. The C# smoke prints `FISH_SMOKE_OK` only after its main-scene assertions. Timeouts, missing markers, and engine/script errors fail the wrapper. `check` runs saved-scene camera/Fish3 animation probes and generated route/placement checks in fresh ignored local profiles. The older input lifecycle script remains in `game/dev/` as historical evidence until its authored-world expectations are reconciled. Keyboard controls are `W` thrust, `S` slowing, `A`/`D` yaw, Up/Down fish pitch and Esc pause. Left/Right arrows pan the camera; holding Shift turns all four arrow keys into camera controls and holds the offset. Releasing Shift returns the camera to its centered follow view. Mouse mode uses relative movement for fish yaw/pitch, left hold for thrust, right hold for slowing, and middle press or Esc for pause. In mouse mode, `W`/`S` raise/lower the camera and `A`/`D` pan it around the fish; releasing a camera key returns that axis to the centered follow view. Holding Shift keeps the offset after those keys are released; releasing Shift returns it. These keys do not propel the fish in mouse mode. The wheel zooms the camera in either input mode (up closer, down farther) for the current session. The enlarged scene's x/y/z swim limits are ±24.9/±9/±24.9, with static enclosing collision at X/Z ±28.5 and Y ±12.6. The original 19 m movement course stays on a sand-covered local shelf above the expanded seabed. Camera and visual-motion defaults are in `game/resources/swim_tuning.tres`. These tracked values are the owner-accepted P1 progression baseline; [accepted tuning](tasks/p1/qualification/accepted-tuning.md) records exact values and [closeout](tasks/p1/closeout.md) records native/source evidence limits. Further feel refinement is deferred.

Exports are later qualification scope. When introduced, use named platform presets and matching .NET export templates, retain the associated .NET runtime output, and test each claimed target separately. A configured preset is not a successful export or target-OS run.

## Godot Editor MCP

Fish uses [hybridindie/godot-mcp](https://github.com/hybridindie/godot-mcp), with installed Python server package `2026.11.04` and the matching [2026.11.04 release addon](https://github.com/hybridindie/godot-mcp/releases/tag/2026.11.04) under `game/addons/godot_mcp/`. The plugin and `MCPRuntimeProbe` autoload are enabled in `project.godot`; the addon license and UID files are retained. MCP reports C# read/write support but no C# build capability; `tools/dev build` remains the compiler check.

An isolated pinned invocation avoids replacing another project's installed tool:

```bash
uvx --from 'godot-editor-mcp==2026.11.4' godot-editor-mcp
```

Use the [matching addon asset](https://github.com/hybridindie/godot-mcp/releases/download/2026.11.04/godot_mcp_addon.zip), place its actual addon under `game/addons/godot_mcp/`, retain upstream license/notices, and enable its plugin. Record the installed versions and source. Verify the selected release's runtime probe/export behavior; preserve its supported no-op behavior for exported games rather than removing an autoload's file while leaving a dangling reference.

### Isolate Fish from Space Factory

Inspect existing client configuration and active editor connections. Use Fish's ignored project-local `.codex/config.toml` under Codex's normal trusted-project rules. Reuse/override the existing Godot MCP server key locally when appropriate. If using a different key, prevent an inherited duplicate listener only in Fish's local configuration. Preserve unrelated entries and the Space Factory/global setup.

Configure the actual local server command, Fish `game/` working directory and the following environment consistently:

| Setting | Fish value |
| --- | --- |
| `GODOT_MCP_PROJECT_DIR` | Absolute path to this checkout's `game/`. |
| `GODOT_MCP_BRIDGE_URL` | `ws://127.0.0.1:<verified-unused-port>`, distinct from any active Space Factory bridge. |
| `GODOT_MCP_GODOT_BIN` | Actual compatible Godot .NET executable. |
| `GODOT_MCP_DEFAULT_TOOLSETS` | Only the sets needed for inspection, scene/3D editing, runtime/input/testing and editor capture; inspect the installed tool catalog. |

The ignored `.codex/config.toml` supplies the existing pinned executable, Fish `cwd`/project directory, actual .NET editor path, and Fish bridge `ws://127.0.0.1:19080`, distinct from the preexisting 9080 user-level connection. `tools/local.env` passes that same bridge URL to `tools/dev editor`. In P0, `godot_get_server_info` identified the live peer as `/home/jfin/dev/fish/game/` before scene mutation. A fresh Codex app session or reload from this trusted Fish project is needed for its built-in tool inventory to use the local override; P0's live checks used a direct MCP client with the same configuration. Do not change the user's global server or Space Factory to route Fish.

The upstream `config --client codex --project ... --write` helper writes the user-level Codex file; `--project` supplies a project value rather than selecting a project-local destination. Do not blindly use `--force` over an existing server. A Codex restart/reload may be required to load new MCP tools; record that exact handoff without claiming a connection that has not been observed.

### Verify configuration and live behavior separately

With the resolved Fish environment, use the pinned command's `doctor --project <absolute-fish-game-path>`. Its listener check is only part of readiness. P2 may finish configuring the integration while live verification awaits a refreshed interactive session; G0.2 does not pass until the live checks below are observed in P3.

1. Query server info and health; verify exact Fish project path, server/addon versions and C# capability.
2. Inspect the actual main scene. Use a disposable development scene for any save/reload mutation check, and clean it up.
3. Compile C# with `tools/dev build` as needed. MCP C# read/write capability does not compile the project.
4. Enable the matching `MCPRuntimeProbe` if required by the installed live-runtime tools, using the documented path.
5. Start the real main scene, inspect live state, perform a bounded movement/input check, capture actual running-game pixels, and inspect output/errors.
6. Stop only the Fish session launched for the check. Keep the working connection ready for subsequent tasks when appropriate.

An editor viewport screenshot and a running-game screenshot establish different things. A headless run cannot establish rendered appearance or input feel. Preserve those distinctions in evidence.

### Primary references

- [Upstream stable CLI behavior](https://github.com/hybridindie/godot-mcp/blob/2026.11.04/mcp_server/cli.py)
- [Environment variables](https://hybridindie.github.io/godot-mcp/reference-env-vars.html)
- [Addon installation](https://hybridindie.github.io/godot-mcp/getting-started-addon.html)
- [Live runtime and screenshots](https://hybridindie.github.io/godot-mcp/guides-playtest-debug.html)
- [Stable C# contract](https://github.com/hybridindie/godot-mcp/blob/2026.11.04/tests/contract/test_csharp_phase1.py)
- [Official Codex MCP configuration](https://developers.openai.com/codex/mcp/) and [configuration precedence](https://developers.openai.com/codex/config-basic/)

## P1 display shell and planned asset workflow

The first P1 task was executed directly on the existing P0 working tree. Godot starts a fresh profile in `ExclusiveFullscreen` (`display/window/size/mode=4`). The pause/options panel offers **Fullscreen** (`Window.ModeEnum.ExclusiveFullscreen`) and **Windowed fullscreen** (`Window.ModeEnum.Fullscreen`), using the installed Godot 4.7.2 .NET API. The latter requests a full-display, undecorated window. Wayland treats the two modes equivalently; [P5 native evidence](tasks/p1/qualification/native-display-and-assets.md) records the observed X11 behavior and its limits.

`./tools/dev run` still builds and launches the main scene. `./tools/dev run --display-mode=fullscreen` and `./tools/dev run --display-mode=windowed-fullscreen` pass a validated user argument to Fish after Godot's `--` separator. Unsupported values fail before build. The current session uses the override without rewriting `user://preferences.cfg`; a deliberate panel change saves the new choice. That local file stores `display/mode`, `input/show_control_help`, `input/mode`, `input/mouse_sensitivity` and `input/invert_mouse_y` separately from future living-save data. The c1 preset store separately uses `user://control-presets.json`, with fresh-profile Keyboard Only default. A missing file uses fullscreen, keyboard mode, 0.12°/pixel sensitivity, no inversion and visible help. The small Options button opens the panel by pointer from default keyboard mode; Escape also opens it. Mouse mode captures the pointer for play; middle press or Escape opens the panel, right hold slows the fish, and the wheel zooms the view in either mode. Resume, display/input selection, sensitivity, inversion, help and Quit have pointer and keyboard focus controls. Focus loss pauses and releases the pointer; resumption requires release then fresh movement input. Native capture/focus observations and owner acceptance are recorded in [P1 closeout](tasks/p1/closeout.md), with advanced matrix gaps explicit.

The recorded P0 foundation is present. P1 entered direct execution at root version `0.0.3` rather than the originally planned `0.1.0` baseline; its implementation advanced through `0.1.9`. The direct owner closeout set root `0.2.0` for P2 without inventing `0.1.10` or `0.1.11` checkpoints. [P1 implementation evidence](tasks/p1/qualification/implementation.md) records that entry/reconciliation boundary. Do not infer a runner-compatible checkpoint chain from the package version alone.

P1 also establishes verified local installation/preparation instructions for the four selected starter assets. Restricted vendor originals and converted resources remain in an ignored local location, with public-safe code, provenance and explicit missing-asset diagnostics. Actual paths, commands, input/display settings and accepted tuning are recorded during implementation. Observe the native game window for display/focus claims; editor preview and headless checks establish different results.

## Evidence status

The [repository-bootstrap record](tasks/repository-bootstrap/qualification.md) covers docs/runner checks only. P0's [implementation](tasks/p0/qualification/implementation.md), [native playtest](tasks/p0/qualification/native-playtest.md), and [closeout](tasks/p0/closeout.md) qualify only the development foundation on this Linux desktop.

Windows/macOS behavior, exports, final water rendering, detailed gameplay-informed swimming refinement, save persistence and survival gameplay remain unqualified until their own Gates pass.
