# c1-bindings — Hybrid control presets and live rebinding

**Status: implemented and reconciled into the accepted P1 baseline; advanced P5 native matrix remains partial.** [Implementation evidence](qualification/implementation.md) records focused checks; [native evidence](qualification/native-controls.md) and the [single closeout](closeout.md) preserve the observed Fish-window paths and gaps. The owner accepted the current right mouse slow / middle mouse pause candidate for progression on 2026-11-09. This correction is not a separate Phase; [P1 closeout](../p1/closeout.md) owns Gate results. Read [assessment](prompt-assessment.md), [implementation plan](implementation-plan.md), and the [runner contract](../../phase-runner.md).

## Goal and boundary

Give Fish a reusable in-game **Control Presets** editor. Players can play with a keyboard and mouse together, rebind keyboard keys and mouse buttons, choose mouse steering axes/sensitivity/inversion, save named custom configurations, and switch among presets without restarting or modifying the authoritative swim motor. Start with selectable **Keyboard Only**, **Mouse Only**, and **Hybrid Swim** presets. Built-in templates are immutable but duplicable; experimental and user-created presets are editable.

The initial Hybrid candidate is W thrust, S slow, mouse X yaw, mouse Y pitch, with optional A/D yaw and keyboard pitch fallbacks; retain the proven keyboard-only and mouse-only mappings. These are *candidates* for native comparison, not accepted final P1 bindings. Preserve the existing active preference on upgrade. Do not silently change existing users to Hybrid, or claim that controls feel right without an owner playtest.

Existing source at authoring: `game/scripts/KeyboardSwimInput.cs`, `MouseSwimInput.cs`, `FishController.cs`, `SwimMotor.cs`, `DisplayPreferences.cs`, `PlayShell.cs`, and `game/project.godot`. Godot 4.7.2 .NET / C# and the shared `SwimIntent` are already implemented. Do not implement a second motor or alter camera/body motion to make a binding pass.

## Historical entry and version

At authoring on 2026-11-09, root `package.json` was **`0.1.9`**, with P1 P1–P9 direct-execution evidence and the original P10 tuning plan open. Every authored correction prompt targeted unchanged `0.1.9`; c1 implementation kept that version. The later owner-accepted P1 closeout deliberately moved root to `0.2.0` for P2. Do not rerun c1 or manufacture old P1 runner checkpoints from this historical entry plan.

Inspect branch/HEAD, current local changes, and history. There are pre-existing unresolved conflict markers in root documentation (including BOOT/AGENTS/MODULES in the inspected published snapshot); this stack does **not** authorize sweeping conflict resolution. Surface any documentation conflict that blocks unambiguous work, and preserve unrelated changes. Check the current `tools/dev` command composition before launching checks. Godot Editor MCP must target Fish, not Space Factory.

## Prompt order

| Step | Prompt | Outcome | Tier / handoff |
| --- | --- | --- | --- |
| P1 | [Action and preset model](P1-control-actions-and-preset-store.txt) | Stable actions, immutable templates, user profiles, safe persistence/migration | T1, no native pause |
| P2 | [Hybrid runtime input](P2-hybrid-runtime-input.txt) | Implemented; one shared motor intent from keyboard/mouse, safe switching and input ownership | T2 headless evidence, native observation open |
| P3 | [Preset creator UI](P3-preset-creator-ui.txt) | Implemented pause editor with draft, capture, conflict and persistence checks | T2 headless evidence; native observation remains P5 |
| P4 | [Rapid comparison and hardening](P4-rapid-comparison-and-hardening.txt) | Quick-switch testing, optional portable presets, regression coverage | T2, no native pause |
| P5 | [Native closeout](P5-native-bindings-closeout.txt) | Partial native Fish window evidence, then direct owner acceptance of the current controls for P1 progression; advanced gaps retained in [qualification](qualification/native-controls.md#follow-up-qualification-task) | T3 historical manual handoff; no full matrix claimed |

Only P5 is classified as this stack's final closeout. The runner pauses on its exact `- Browser required: yes.` field: this means **native Godot observation** or owner playtest, not ordinary web browsing.

## Acceptance

A user can create a preset, bind physical keys/mouse buttons, use mouse yaw/pitch and keyboard thrust/braking concurrently, save/reload the preset, switch profiles in-game without a scene restart, and safely get back to Pause/Options even with an invalid binding. No key-press/mouse-motion leaks on capture, menu focus, resume, focus loss or switching. Existing Keyboard Only and Mouse Only remain playable; quick switching allows testing the same course without modifying tuning or fish identity. Persist settings locally, never inside a living-fish save.

The correction's implementation checks alone do not pass P1's G1.1–G1.4 or V01/V02. The later direct owner decision accepts the current candidate for P1 progression; [P1 closeout](../p1/closeout.md) maps the combined evidence and remaining limits. Controller, broader free look, Movement abilities, survival, networking, broad menu redesign and engine upgrades are out of scope.

## Development comparison on one course

In a debug playtest, **F6** cycles Keyboard Only → Mouse Only → Hybrid Swim → saved custom presets, showing the active name briefly. It is disabled if any available preset binds F6, so that binding remains gameplay input; choose profiles from Pause → Swim controls in that case. Release held keys and mouse buttons after each switch before pressing again. The visible pause selector works in every build; the F6 shortcut and its help text are debug-only. Switching does not reset the fish, collision, velocity, position, camera, or tuning.

For measured trials, start at the saved `(0, 0, 0)` spawn, approach the yellow target at `(0, -1.5, -2.1)`, pass the rock gap via the markers at Z `-2.8`, `-4.45`, `-6.1`, then turn toward the upper and lower depth landmarks. Record route/time and observations for each preset. **Manually reset between trials** by quitting from Pause and relaunching `./tools/dev run --display-mode=windowed-fullscreen`; select the next preset before moving. F6 is for live comparison while swimming and never teleports to the start. Keep `game/resources/swim_tuning.tres` and course scene unchanged across comparisons. P5 observes the native game window; P1 P10 decides feel and final bindings.

## Historical execute and record

These commands were the original correction-stack execution route. c1 is already implemented and reconciled into P1; [P2](../p2/README.md) is now the next runnable stack.

From a reconciled, reviewed checkout:

```bash
npm run fish:phase:validate -- c1-bindings
npm run fish:phase -- c1-bindings
```

The runner owns implementation checkpoints; it does not automatically prove Godot or native feel. Keep real checks under `qualification/implementation.md` and P5's native evidence under `qualification/native-controls.md` **when performed**, not prefilled as PASS. Finish with the correction's `closeout.md` (if native review completed). Manual closeout stays reviewable/uncommitted. Never commit vendor art or local secrets.
