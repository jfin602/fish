# c1-bindings prompt assessment — 2026-11-09

## Request / current source

Owner requested a separate `/prompt-ass + /prompt-plan + /prompt-write c1-bindings` correction stack for a hybrid keyboard/mouse control scheme, customizable key bindings and saved presets that can be changed while playing to compare combinations. The current `game/project.godot` binds W/S/A/D/Up/Down, and the P7 mouse adapter steers with captured relative X/Y and holds left click for thrust; right click pauses in mouse-only mode. `FishController` selects exactly one adapter, so the present architecture does **not** combine keyboard and mouse input. `PlayShell` offers a compact pause/options menu; `DisplayPreferences` persists mode, sensitivity and inversion in `user://preferences.cfg`. `SwimMotor.Step` accepts a single `SwimIntent(Thrust, Slow, Yaw, Pitch)` and must remain the authority.

The published `package.json` version at authoring is `0.1.9`. P1 P1–P9 implementation checks are recorded; P10 final native tuning/acceptance is unqualified. Source references were inspected through the connected GitHub repository, not through the owner's local working tree. The prompt executor must inspect HEAD/dirty state and reconcile any divergence.

## Outcome / non-goals

1. Authoritative semantic actions independent of device and physical binding. A profile can mix devices on the same action and use mouse motion as configured yaw/pitch. Zero engine/controller duplicate authority.
2. Three usable built-in presets (Keyboard Only, Mouse Only, Hybrid Swim), and custom presets with create/duplicate/rename/delete/reset, conflict detection, save/reload and immediate apply/test/revert while preserving previous users' selection.
3. A compact, accessible pause/options control editor usable from both devices. Capturing a key or button does not also activate gameplay or close the menu. Escape and an always-accessible menu path cannot be stranded by rebinding.
4. A development-friendly quick preset cycle during swimming, with current preset visible; measured profile comparison in the same saved course with unchanged swim tuning, input-neutral rearm and no camera/fish reset.
5. Real native qualification only at the handoff. No claim of chosen final input mapping until P1 owner acceptance.

Do not implement controller support, free look, Movement abilities, first-person, feeding, survival persistence, new motor physics, a framework for arbitrary macros/chords or general settings overhaul. Optional import/export should be safe, local and scoped to control profiles only.

## Decomposition and verification

| Task | Responsibility | Evidence |
| --- | --- | --- |
| P1 | Define action/schema/template/persistence contract and legacy migration | T1 serialized round trips, invalid config rejection, no loss of display preferences |
| P2 | Runtime compose keyboard+mouse into one bounded intent; stable pause/focus/rearm | T2 physical input and scene trace, existing motor tests, legacy mode parity |
| P3 | In-game preset editor and binding-capture transaction; input conflict UX | T2 UI binding/mouse/keyboard navigation and persistence integration |
| P4 | Debug quick-cycle and comparison affordance; import/export if small/safe; regressions | T2 check plus repeated preset switching with no movement-state reset or stuck inputs |
| P5 | Mandatory native Godot/owner observation and correction closeout | T3 native input matrix across both fullscreen choices; T1 documentation/evidence reconciliation |

## Risks and mitigations

- **Swim physics drift:** retain existing `SwimMotor`, `SwimTuning`, collision root, camera and animation response. Only the intent adapter changes; prove same intent produces same motor outcome.
- **Double movement:** combine action strengths and relative mouse delta once per physics tick, clamp and handle opposing inputs deterministically. Never let two adapters each step the motor.
- **Stuck input/capture spikes:** clear queued mouse motion, held states, modifier capture and synthetic warp events on focus, pause, preset switch and restore. Require release before rearm.
- **Lost menu access:** reserve a reliable Escape/Pause recovery gesture plus visible click target independent of custom gameplay bindings; validate conflicts rather than silently stealing a binding.
- **Broken existing preferences:** preserve fullscreen/windowed fullscreen saved behavior, prior mode/sensitivity/inversion, and existing active selection. Save controls outside living-run data; handle versioned migration/invalid profile files.
- **Misleading acceptance:** a headless trace, screenshot or grammatical runner PASS is not evidence of comfortable hybrid swimming. Distinguish implementation, native observation, and final P1 feel approval.
- **Concurrent P1 changes:** correction requires the authoring version `0.1.9`. Reconcile P10 and Git history before any run; do not overwrite newly tuned profiles or downgrade root version.
- **Documentation conflicts:** inherited BOOT/AGENTS/MODULES contain unresolved conflict-marker text in the inspected snapshot; preserve and escalate separately if relevant. This stack is not a cleanup authorization.

## Success boundary

Prompts are authored only. Successful execution means safe live profile selection and persistence with meaningful focused/integration checks plus a native evidence handoff, not P1 GREEN. P1 P10 must explicitly test and select the final control setup and swimming feel. The root version remains `0.1.9` for the entire correction, as required by Fish correction grammar.
