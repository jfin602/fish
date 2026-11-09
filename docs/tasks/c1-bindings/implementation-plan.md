# c1-bindings implementation plan

## Entry / source routing

Read AGENTS.md, BOOT.md, [workflow](../../workflow.md), [runner](../../phase-runner.md), [swimming contract](../../systems/swimming-and-camera.md), [P1 state](../p1/README.md) and affected source only. Inspect actual Git branch, HEAD, dirty changes, `package.json`, and `tools/dev` command composition first. Keep corrections to active Fish code and scoped docs; do not resolve unrelated documentation conflict markers as an unreviewed side effect. Confirm Godot MCP's project identity before editor mutations.

Existing authoritative path:
`KeyboardSwimInput.Read()` and `MouseSwimInput.Consume()` -> selected by `FishController` -> one `SwimMotor.Step()` -> one `CharacterBody3D`. `PlayShell` owns pause/settings/focus; `DisplayPreferences` owns saved display and legacy control preferences. `game/project.godot` contains baseline keyboard actions.

## P1 — Actions, templates and persistence

Introduce a finite set of semantic swim actions: thrust, slow/brake, yaw left/right, pitch up/down, pause/menu. Keep future abilities out. Represent keyboard keys (physical layout-aware preference where supported), mouse buttons and two mouse-motion axis intents separately. A binding may map multiple devices to one action; each built-in profile has complete usable fallback bindings. Define conflict policy (duplicate input on competing actions rejected or explicitly resolved), constrained names, limited user-defined preset count, deterministic and validated profile identity, and sensible defaults. Prevent a profile from removing every reliable menu/pause route.

Use Godot C# and existing storage facilities: a versioned controls-only store under `user://`, not fish-run persistence or Godot editor `ProjectSettings.Save`. Migrate old `preferences.cfg` input mode, mouse sensitivity and inversion without clobbering display choices or existing saved preferences. Existing installations keep their active old-mode equivalent until selected otherwise; fresh users may select Hybrid, but do not silently relabel the accepted default. Built-ins are immutable with Duplicate/Save As for edits. Invalid or partial files fall back to a safe built-in and preserve a recoverable copy/log rather than killing the game.

Likely owners: new narrowly scoped `ControlPreset` / `ControlPresetStore` (actual names flexible), `DisplayPreferences.cs`, existing input identifiers in `project.godot`. Test serialized round trips, migration, corruption, missing file, conflicting bindings, and safe fallback before changing runtime.

## P2 — Hybrid intent resolver and lifecycle

Replace exclusive keyboard-or-mouse selection in `FishController` with one active-profile intent resolver. Feed *one* `SwimIntent` into the unchanged `SwimMotor.Step` per physics tick. Resolve digital thrust/brake and yaw/pitch keyboard strengths, mouse-button thrust/brake and mouse relative X/Y steering based on active profile. Explicitly define combination rules (e.g., keyboard and mouse steering contributions summed and clamped once, opposite digital directions cancel); no doubled thrust, additive speed advantage or alternate camera/physics authority. Preserve existing sensitivity bounds, Y inversion, first-motion ignore and warp rejection. Mouse-only stays usable by mouse; keyboard-only cannot be steered accidentally by pointer motion.

Profile activation is atomic at a safe input boundary: clear held/queued state, release/capture mouse as appropriate, keep fish world transform, velocity, and tuning untouched, and require fresh release/rearm. This differs from resetting the swim velocity; a control change should not teleport or halt the fish unnaturally, although while paused physics remains frozen. On focus loss, pause and release capture; on resume discard menu activation events and synthetic deltas. Keep menu and game input ownership separate.

Likely affected: `FishController.cs`, `KeyboardSwimInput.cs`, `MouseSwimInput.cs`, `PlayShell.cs` plus a small input resolver. Existing physics tests must remain valid; add integrated event/focus/menu tests.

## P3 — Preset creator UI

Extend `PlayShell`'s small pause/options overlay with a dedicated Control Presets panel rather than crowding the existing tiny options. Accessible by pointer and keyboard focus. Provide visible selected profile, new/duplicate/rename/delete/reset, list of semantic actions with current bindings, a "press any key / mouse button" capture dialog, a dedicated mouse motion steering assignment selector, sensitivity/inversion, and Apply/Save/Revert. Offer conflict warnings and explicit replacement/removal; support more than one binding for an action. Built-in presets never edited in place. Prevent arbitrary captured Escape from breaking the safety exit. Ensure keyboard-only and mouse-only users can reach and leave menus without needing the device being configured. Current control help reflects the active profile, not a hard-coded mode name.

Presets save as local configuration immediately on confirmed changes; selecting/previewing a profile does not destroy the prior active profile if cancelled. Use stable UI focus and input-event suppression: a capture or Resume event must not simultaneously become thrust/yaw/pause. Keep fullscreen and windowed fullscreen layouts usable.

## P4 — Quick comparison and regression

Provide a clearly labeled *development-only* quick-cycle (e.g., F6 when the build is a debug playtest; never steal a user remap unknowingly) to move among available profiles with a brief unobtrusive active-profile indicator. Also permit switching via pause screen. The quick-cycle must not silently apply to retail player builds or accept untrusted import data. Optional scoped JSON preset import/export belongs here only if it can be done safely and without diverting from binding/playtest correctness; otherwise document explicit deferral. If included, validate schema, paths, size, identities and collisions; never overwrite built-ins or arbitrary disk files.

Run identical short-course tests per profile with consistent `SwimTuning`, same route/seed and position starting point where manually reset between trials; do **not** reset fish state merely because profile changed. Verify repeat switching (keyboard <-> mouse <-> hybrid), no input backlog, pause focus, no stuck thrust, sensitivity/inversion, display transitions, restart persistence, and no game/menu double dispatch. Report test configuration so P1 P10 can compare candidates. Do not select a final favorite based on synthetic tests.

## P5 — Native handoff and closeout

At a native-capable Fish desktop, run keyboard-only, mouse-only and hybrid in normal fullscreen and windowed fullscreen. Rebind a key and button, customize a hybrid, test same course, switch live with input held, pause/resume/focus loss, return from pointer capture, restart and recover from invalid bindings. Judge UI access and observed motion, not final subjective swim feel. Record exact source revision, asset/profile identity, key/button assignment, display choice, device actions, PASS/FAIL/NOT OBSERVED and stable evidence paths. Owner can compare but final `P1 P10` owns acceptance of actual chosen binding/tuning and the 7 core swimming tests. Keep correction version unchanged and reconcile scoped docs once; leave final closeout uncommitted for review.

## Check strategy / exclusions

T1 focused C# and storage tests; T2 scene and UI/input integration using actual scene and existing `./tools/dev check` (inspect composition before calling; do not redundantly rebuild); T3 native manual qualification in P5. Run `git diff --check` for changed code/docs, check license/asset boundaries, and confirm root version unchanged. Prefer bounded checks and real failure reporting. No controller/free-look, ability actions, survival save integration, cosmetic UI overhaul, renderer upgrade, extra local tools or speculative physics rework.

## Ownership and evidence

Detailed tested outcomes: `docs/tasks/c1-bindings/qualification/implementation.md` and `qualification/native-controls.md` when executed. One correction `closeout.md` only at P5. P1 owner docs `docs/systems/swimming-and-camera.md` / `docs/planning/p1/core-swimming.md` and relevant acceptance artifacts update after implementation evidence, while `docs/roadmap.md` remains the single Gate authority. Keep unqualified P1 Gates open until actual P1 native/owner evidence exists.
