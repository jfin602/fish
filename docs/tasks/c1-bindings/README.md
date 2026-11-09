# c1-bindings — Hybrid control presets and live rebinding

**Status: P1 model/store, P2 runtime resolver, and P3 preset editor implemented; P4–P5 remain.** [Implementation evidence](qualification/implementation.md) records focused checks and the current P10 candidate of right mouse slow / middle mouse pause. Native comparison and final binding acceptance remain open. This is a separate correction to P1 swimming input, not a new Phase or a declaration that P1 is GREEN. Read [assessment](prompt-assessment.md), [implementation plan](implementation-plan.md), and the [runner contract](../../phase-runner.md).

## Goal and boundary

Give Fish a reusable in-game **Control Presets** editor. Players can play with a keyboard and mouse together, rebind keyboard keys and mouse buttons, choose mouse steering axes/sensitivity/inversion, save named custom configurations, and switch among presets without restarting or modifying the authoritative swim motor. Start with selectable **Keyboard Only**, **Mouse Only**, and **Hybrid Swim** presets. Built-in templates are immutable but duplicable; experimental and user-created presets are editable.

The initial Hybrid candidate is W thrust, S slow, mouse X yaw, mouse Y pitch, with optional A/D yaw and keyboard pitch fallbacks; retain the proven keyboard-only and mouse-only mappings. These are *candidates* for native comparison, not accepted final P1 bindings. Preserve the existing active preference on upgrade. Do not silently change existing users to Hybrid, or claim that controls feel right without an owner playtest.

Existing source at authoring: `game/scripts/KeyboardSwimInput.cs`, `MouseSwimInput.cs`, `FishController.cs`, `SwimMotor.cs`, `DisplayPreferences.cs`, `PlayShell.cs`, and `game/project.godot`. Godot 4.7.2 .NET / C# and the shared `SwimIntent` are already implemented. Do not implement a second motor or alter camera/body motion to make a binding pass.

## Entry and version

At authoring on 2026-11-09, the published root `package.json` is **`0.1.9`**, with P1 P1–P9 direct-execution evidence and P10 owner tuning still open. **Every correction prompt requires unchanged `0.1.9`.** Run this correction before P1's P10 native tuning if the root version still matches; if the tree or version has advanced, reconcile with live source and deliberately update this correction's version metadata in *every* prompt before execution. Do not downgrade the package or manufacture P1 runner checkpoints.

Inspect branch/HEAD, current local changes, and history. There are pre-existing unresolved conflict markers in root documentation (including BOOT/AGENTS/MODULES in the inspected published snapshot); this stack does **not** authorize sweeping conflict resolution. Surface any documentation conflict that blocks unambiguous work, and preserve unrelated changes. Check the current `tools/dev` command composition before launching checks. Godot Editor MCP must target Fish, not Space Factory.

## Prompt order

| Step | Prompt | Outcome | Tier / handoff |
| --- | --- | --- | --- |
| P1 | [Action and preset model](P1-control-actions-and-preset-store.txt) | Stable actions, immutable templates, user profiles, safe persistence/migration | T1, no native pause |
| P2 | [Hybrid runtime input](P2-hybrid-runtime-input.txt) | Implemented; one shared motor intent from keyboard/mouse, safe switching and input ownership | T2 headless evidence, native observation open |
| P3 | [Preset creator UI](P3-preset-creator-ui.txt) | Implemented pause editor with draft, capture, conflict and persistence checks | T2 headless evidence; native observation remains P5 |
| P4 | [Rapid comparison and hardening](P4-rapid-comparison-and-hardening.txt) | Quick-switch testing, optional portable presets, regression coverage | T2, no native pause |
| P5 | [Native closeout](P5-native-bindings-closeout.txt) | Real Fish window/device observations, evidence, handoff to P1 P10 | T3, mandatory manual handoff |

Only P5 is classified as this stack's final closeout. The runner pauses on its exact `- Browser required: yes.` field: this means **native Godot observation** or owner playtest, not ordinary web browsing.

## Acceptance

A user can create a preset, bind physical keys/mouse buttons, use mouse yaw/pitch and keyboard thrust/braking concurrently, save/reload the preset, switch profiles in-game without a scene restart, and safely get back to Pause/Options even with an invalid binding. No key-press/mouse-motion leaks on capture, menu focus, resume, focus loss or switching. Existing Keyboard Only and Mouse Only remain playable; quick switching allows testing the same course without modifying tuning or fish identity. Persist settings locally, never inside a living-fish save.

The correction does not pass P1's G1.1–G1.4, V01/V02, or owner swimming-feel acceptance. P1 P10 still owns final movement/camera feel, the chosen accepted bindings, and its test matrix. Controller, free look, Movement abilities, survival, networking, broad menu redesign and engine upgrades are out of scope.

## Execute and record

From a reconciled, reviewed checkout:

```bash
npm run fish:phase:validate -- c1-bindings
npm run fish:phase -- c1-bindings
```

The runner owns implementation checkpoints; it does not automatically prove Godot or native feel. Keep real checks under `qualification/implementation.md` and P5's native evidence under `qualification/native-controls.md` **when performed**, not prefilled as PASS. Finish with the correction's `closeout.md` (if native review completed). Manual closeout stays reviewable/uncommitted. Never commit vendor art or local secrets.
