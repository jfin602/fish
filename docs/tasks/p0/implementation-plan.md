# P0 implementation plan

## Inspect before changing

Read root AGENTS/BOOT and the prompt's routed documents, inspect current revision and local work, then discover actual Godot .NET, SDK, Codex and MCP availability. Use existing suitable user-local installs. The reference pins and exact MCP source are in [development setup](../../development.md); do not assume the cloud environment or another checkout represents the owner's desktop.

## P1 — Godot project and CLI

Create `game/project.godot`, `Fish.csproj`, `Fish.sln`, `global.json`, `scenes/main.tscn`, and a small set of C# scripts in namespace `Fish`. Establish a real 3D scene with camera/light/landmarks, a placeholder fish and simple horizontal/vertical controls within static bounds. Use temporary art and documented defaults; no save, food or history stubs masquerading as features.

Implement `tools/dev` with doctor/import/build/check/editor/run/inspect-scene/automation-smoke. Resolve paths from the wrapper, select the .NET SDK from the game directory, support safe argument arrays and ignored local settings, and compose checks once. Keep small scene-automation helpers under `game/dev/`, separate from first-party C# gameplay.

Checks must import/compile/execute the actual main scene, observe a bounded successful C# marker after assertions, detect errors/timeouts, and serialize/reload/clean a disposable scene. Invoke at least one command outside the repository CWD. Record actual configuration/build evidence and defer rendered claims.

## P2 — MCP configuration

Inspect existing client entries and active bridges without exposing secrets. Use the selected stable Python package and matching release addon, retaining provenance and notices. Register the editor plugin and supported runtime probe when needed. Keep the probe's upstream supported export behavior intact.

Write ignored Fish-local client/environment configuration with actual absolute game path, compatible .NET executable, and a distinct loopback bridge URL. Forward the matching URL when launching the editor. Preserve existing unrelated settings and normal trust/approval controls. The helper's `--project` flag does not choose a project-local Codex destination; avoid forced global replacement.

Validate parseability, matching package/addon versions, enabled plugin/probe paths, local ignore coverage and the resolved server command. Run available configuration diagnostics and record exactly what they prove. If live tools were not loaded in this session, document the needed client restart and leave live verification for P3.

## P3 — Native closeout

Use a refreshed interactive session with actual native tools. Verify server health and the connected Fish project identity before scene mutations. Inspect the main scene and perform a bounded disposable scene save/reload operation. Build as needed, start the main scene, inspect live state, show actual horizontal/vertical movement and release, capture the game's rendered frame, read errors and close only the task's session.

Map B01–B04 and all P0 criteria to actual source-bound evidence. Reconcile roadmap, BOOT, MODULES and development instructions. Stop at this foundation; hand off P1 gameplay planning as the next distinct scope.

## Evidence and versions

P1/P2 append to `qualification/implementation.md`; P3 writes `qualification/native-playtest.md` and the single `closeout.md`. Capture exact tool versions, local configuration location (without secrets), source identity, observed commands, screenshots where available, limitations, and next action.

Baseline package version is `0.0.0`; assigned checkpoints are `0.0.1`, `0.0.2`, `0.0.3`. Update only after each prompt's required checks pass. Failed or unobserved closeout keeps the previous successful checkpoint. Version metadata never changes a Gate result by itself. Follow [runner semantics](../../phase-runner.md) for checkpoint commits and direct-execution reconciliation.
