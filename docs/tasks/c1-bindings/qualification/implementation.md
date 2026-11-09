# C1 P1 — Control actions and preset store

**Implementation state: partial pending binding decision.** Pre-task `main` HEAD was `20ce6fd3516e6cb0d04fccc164c0fa4ecfe99417`, with a clean working tree. Root `package.json` remains `0.1.9`. The pre-existing conflict markers in root guides were left untouched. No commit was made.

## Source and behavior

- `game/scripts/ControlPreset.cs` defines seven finite semantic requests, physical key and mouse-button bindings, independent mouse X/Y steering assignments, sensitivity and inversion. Built-in IDs are fixed (`builtin:keyboard`, `builtin:mouse`, `builtin:hybrid`); editable IDs are canonical `custom:` UUIDs. Names, binding counts, button/key codes, axis conflicts, duplicate inputs, sensitivity, reserved Escape and reserved middle click are validated. Built-ins are snapshots and cannot be edited through the store.
- `game/scripts/ControlPresetStore.cs` stores schema 1 under `user://control-presets.json`, separate from fish saves and `preferences.cfg`. It supports list, create/duplicate, update, rename, reset, delete, choose, load and atomic save, with a 16-custom-profile limit. Missing data use Keyboard Only. Invalid, oversized or corrupt files are copied to a timestamped `.corrupt-*.json` file and fall back to legacy preferences or safe defaults. Loading an existing `preferences.cfg` preserves its selected keyboard/mouse mode and mouse sensitivity/inversion in the new profile store. The original preferences file is only read.
- `game/scripts/PlayShell.cs` keeps Escape as an unconditional keyboard menu path and middle click as the existing mouse menu path. The visible Options button now shows “Options (Middle)” in both modes; in captured mouse mode, middle click is the usable mouse recovery gesture. Gameplay binding capture is a later P3 UI operation, independent of these recovery paths.
- `game/dev/ControlPresetChecks.cs` exercises the real C# model and store via `Main`'s `--control-preset-check` hook. `game/project.godot`, `FishController`, `SwimMotor`, scene assets, and living-fish saves were not changed. The new C# UID sidecars are retained.

## Configuration examples and P2 entry contract

Built-in Keyboard Only requests W thrust, S slow, A/D yaw, Up/Down pitch and Escape pause. Current Mouse Only requests mouse X yaw, mouse Y pitch, left-button thrust, right-button slow, middle-button pause and Escape pause. Hybrid Swim combines W/left thrust, S/right slow, A/D yaw, Up/Down pitch, mouse X/Y steering, and the same menu recovery. Sensitivity defaults to 0.12 degrees per captured pixel and is bounded to 0.04–0.40. A saved store has `schemaVersion: 1`, `currentId`, and custom `profiles` with ID, name, built-in origin, action bindings, mouse axis assignments and sensitivity. The three built-ins are code-defined and are never written as mutable records.

**P2 handoff:** read `ControlPresetStore.Current`, resolve each `ControlAction` from its `ControlBinding` list plus `MouseX`/`MouseY` once per physics tick, and feed one `SwimIntent` to the existing `SwimMotor.Step`. Keep `PlayShell` recovery events outside capture and consume them before gameplay input. Preset activation must clear held events and queued mouse deltas; do not change fish or camera state here. Current runtime selection still uses the two legacy modes and is intentionally unchanged in P1.

## Material source conflict

The P1 prompt says Mouse Only must use **right-button pause**. Live P10 source and its saved-scene probe instead use **right-button slow and middle-button pause** (`MouseSwimInput`, `PlayShell`, `input_lifecycle_check.gd`). The prompt also says to preserve implemented desktop controls and not change `FishController` mode selection. The templates currently reflect the live P10 behavior, preserving its working input path. The requested right-button-pause mapping cannot also coexist with right-button slow under the conflict guard. The final Mouse Only template mapping needs an explicit decision before P2 wires it into runtime. P10 native feel and all P1 Gates remain open.

## Validation

| Command or observation | Result | Evidence / limit |
| --- | --- | --- |
| `./tools/dev build` | PASS after an initial test-code compile error was repaired | Final build: 0 warnings, 0 errors. Initial `params` target-typed `new` calls failed to compile and were replaced with explicit `ControlBinding` construction. |
| `source tools/local.env; env XDG_DATA_HOME=<fresh /tmp dir> "${GODOT_CMD[@]}" --headless --path game -- --control-preset-check` | PASS | Real C# schema/template checks, custom round trip, immutable built-ins, rename/reset/delete, conflict/reserved key rejection, legacy mouse and keyboard migration, unchanged legacy file bytes, invalid binding, duplicate identity, corrupt JSON and unknown-schema backup/fallback. The expected invalid-file cases emit Godot warnings. |
| `source tools/local.env; env XDG_DATA_HOME=<fresh /tmp dir> "${GODOT_CMD[@]}" --headless --path game --script res://dev/input_lifecycle_check.gd` | PASS | `INPUT_LIFECYCLE_OK` against the saved scene, including keyboard Escape, mouse middle pause, visible Options, capture/rearm and unchanged legacy mode behavior. |
| `./tools/dev check` | FAIL — environment | Its composition is import → build → smoke → scene automation/missing-scene/input/camera/animation checks. It stopped at import because this sandbox denies Godot's local socket and editor-settings write; no later `check` stages ran. The direct build and focused scene check above cover the modified C# and shell paths. |
| Native Fish window, editor bridge, P10 comfort | NOT OBSERVED | Godot MCP reports the Fish bridge offline. Headless checks do not establish native control feel or final preset choice. |
| `git diff --check`; `test ! -e package-lock.json`; root version check | PASS | No whitespace errors, no lockfile, `package.json` still `0.1.9`. |

No P1 Gate status or historical P1 execution evidence was changed.

## P2 — Hybrid runtime input (2026-11-09)

Pre-task `main` HEAD: `213f4f688b3894f2a0d187c302c73025421ce5bb` (the C1 P1 store checkpoint). Root version stayed `0.1.9`. The working tree already contained partial P2 edits to the resolver, shell and scene checks; this turn continued them without discarding that work. P1 P9's saved Fish3 scene, collision root, shared `SwimMotor`, tuning and camera remained in place. The Godot MCP server reported version `2026.11.04` but no editor connection, so the installed Godot 4.7.2 .NET headless runner supplied scene evidence.

`ProfileSwimInput` resolves the active preset's physical keys, mouse buttons and captured relative motion to one bounded `SwimIntent` per physics tick. Duplicate thrust/slow requests remain 1, opposing digital directions cancel, and digital plus mouse yaw/pitch sum once before clamping. `MouseSwimInput` retains first-motion suppression, per-event warp rejection and queued-pixel caps; sensitivity is finite and clamped. `FishController` calls the unchanged `SwimMotor.Step` once per active physics tick, including a neutral coasting tick while disarmed after a profile switch. Profile selection clears held/queued controls and changes pointer capture without changing fish transform, collision, speed, tuning or camera state. Intentional pause/focus/display stops retain their existing stop semantics. The shell exposes the three built-ins in its existing selector and preserves legacy preferences; the current P10 candidate remains right mouse slow and middle mouse pause with Escape recovery.

| Check | Outcome | Scope |
| --- | --- | --- |
| Inspected `./tools/dev check`, then ran it once | BLOCKED at import | Composition: import → one .NET build → C# smoke → scene automation/negative scene check → input, camera and animation scene probes. Sandbox denied Godot's local socket and editor-settings write; later stages did not run. |
| `./tools/dev build` on final C# | PASS | Zero warnings and errors. |
| Isolated saved-scene `hybrid_input_check.gd` | PASS | W with mouse yaw/pitch in one motor tick; one-time motion consumption; overlapping thrust/slow; opposing steer; final clamps; capture warp; live switch with held W and continuing coast; mouse-only fallback; Escape, focus and display menu ownership. |
| Isolated saved-scene `input_lifecycle_check.gd`, `motor_scene_check.gd`, `camera_scene_check.gd`, `animation_scene_check.gd` | PASS | Existing keyboard/mouse paths, pause/rearm, collision/bounds, camera and Fish3 pose. Motor and animation fixtures now send physical key events. Camera fixture checks recentering against measured turn lag and course obstruction. |
| Isolated C# `--control-preset-check`; `git diff --check`; root version | PASS | Three templates, migration/store regressions, clean diff, unchanged `0.1.9`. Expected invalid-store fixture warnings appeared. |
| Native Fish input comfort, actual OS capture/focus and fullscreen switching | NOT OBSERVED | Headless Godot reports effective display `Minimized`; P5/P1 P10 require real desktop observation. |

Initial focused scene runs failed because the old animation fixture used action presses that no longer reach physical bindings, and the camera fixture expected exact recentering despite its measured course offset. Those fixtures were corrected and passed on rerun. P3 should build the preset creator/rebinding UI, show active-profile sensitivity and inversion accurately, and retain the menu-event suppression and existing right-slow/middle-pause candidate for P5 native comparison. No P1 Gate or final binding comfort is claimed; no self-commit or push was made.
