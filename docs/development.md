# Development setup

**Current state:** documentation and Node tooling only. P0 will create `game/` and `tools/dev`. No Fish engine install, build, editor connection, gameplay session, or export has been qualified yet.

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

Reference values are not proof of local availability. Discover the actual editor/SDK and reuse suitable existing installations. Prefer Fish-local configuration to changing system defaults or Space Factory. Document a necessary deliberate deviation with its cause and measured result. Install missing components from official upstream sources using ordinary user-local mechanisms; a missing capability or access approval is an explicit blocker, not permission to bypass controls.

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

The root `package.json` is private development metadata, starting at `0.0.0`. It is independent of engine pins and public release versions. Do not add a root package lock or npm dependencies to run the inherited tools. [The runner guide](phase-runner.md) owns its exact contract.

## Developer CLI to implement in P0

`tools/dev` must resolve the repository/project from its own location, use properly quoted arguments, and fail clearly on missing prerequisites. It should read ignored `tools/local.env` for actual local executable paths; preserve Space Factory's safe array-based `GODOT_CMD` convention and `DOTNET_BIN` override without evaluating arbitrary command strings.

| Planned command | Required behavior |
| --- | --- |
| `./tools/dev doctor` | Report actual Godot version/.NET capability, SDK, project/config readiness and display availability. |
| `./tools/dev import` | Import the Fish project and wait for completion; fail on engine errors/timeouts. |
| `./tools/dev build` | Compile `game/Fish.csproj` with the pinned SDK. |
| `./tools/dev check` | Compose import, build, a bounded real-main-scene C# smoke, and isolated scene-automation smoke once. |
| `./tools/dev editor` | Open the Fish editor, forwarding the Fish bridge environment after P2. |
| `./tools/dev run` | Build and launch the real Fish main scene, with documented composition. |
| `./tools/dev inspect-scene <path>` | Inspect a real saved scene; missing paths fail nonzero. |
| `./tools/dev automation-smoke` | Create/edit/save/reload/assert/remove a disposable scene using Godot APIs. |

Keep generated outputs under ignored `artifacts/`; preserve relevant UID sidecars. The C# smoke must prove real scene/C# execution and produce `FISH_SMOKE_OK` only after its assertions. Timeouts, missing markers, and engine/script errors are failures. Avoid creating a general testing framework for this foundation.

Exports are later qualification scope. When introduced, use named platform presets and matching .NET export templates, retain the associated .NET runtime output, and test each claimed target separately. A configured preset is not a successful export or target-OS run.

## Godot Editor MCP

Use [hybridindie/godot-mcp](https://github.com/hybridindie/godot-mcp), the `godot-editor-mcp` Python server, and its `addons/godot_mcp` editor addon. The selected stable [2026.11.04 release](https://github.com/hybridindie/godot-mcp/releases/tag/2026.11.04) is the reference pair for a new install; use its matching addon, not moving beta `main`. Inspect an existing suitable installation before replacing anything.

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

The Godot editor process must receive the same bridge URL as its server. `GODOT_MCP_PROJECT_DIR` alone does not prove the live peer is Fish; check returned project identity before mutations. Reopen the editor after externally changing the enabled plugin list when needed.

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

## Planned P1 display and asset workflow

The [P1 stack](tasks/p1/README.md) is authored, not executed. After actual P0 readiness, its [entry procedure](tasks/p1/manual/entry-and-resume.md) prepares the root `0.1.0` baseline. The first task adds fullscreen startup, windowed fullscreen and a documented temporary display-mode override to the actual `tools/dev run`; these additions are not present in the published starter.

P1 also establishes verified local installation/preparation instructions for the four selected starter assets. Restricted vendor originals and converted resources remain in an ignored local location, with public-safe code, provenance and explicit missing-asset diagnostics. Actual paths, commands, input/display settings and accepted tuning are recorded during implementation. Observe the native game window for display/focus claims; editor preview and headless checks establish different results.

## Evidence status

The [repository-bootstrap record](tasks/repository-bootstrap/qualification.md) covers docs/runner checks only. P0 creates its actual configuration/build evidence under `docs/tasks/p0/qualification/` and its final `closeout.md` after execution. Neither exists as a passing runtime record in this starter.

Record Linux results only when observed on the selected revision. Windows/macOS behavior, exports, final water rendering, full swimming feel, save persistence and gameplay remain unqualified until their own Gates pass.
