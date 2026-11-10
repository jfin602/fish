# c2-watet-light-fx implementation plan

## Entry and routing

Read `AGENTS.md`, `BOOT.md`, `docs/workflow.md`, `docs/phase-runner.md`, this task README/assessment, `docs/art-style.md`, the relevant P2 atmosphere/native evidence, `MODULES.md`, and affected source only. Inspect branch/HEAD, dirty changes, root `0.2.6`, actual `tools/dev` command composition and Fish Godot project identity before edits.

Current presentation path is:

`main.tscn` -> `UnderwaterAtmosphere.Initialize()` -> one Forward+ Environment + one DirectionalLight3D -> `WaterEffects` (caustic decal + GPU motes) + below-UI `WaterScreen`.

Keep that ownership. A helper script/shader or small reusable scene is allowed when it makes the implementation clearer, but do not create a second atmosphere controller or move visual state into fish/controller code.

## P1 — overhead water surface

Add a bounded visual surface at the actual top of the playable habitat. Derive height/extent from the saved habitat/upper limit or one centralized atmosphere value that is cross-checked against it; do not invent an unrelated ceiling. The fish/camera should remain below it under normal collision.

Use a single efficient mesh/material path. Prefer one modestly subdivided plane (well below a pathological vertex count; thousands, not hundreds of thousands) with GPU vertex displacement from two low-frequency wave components. The underside should read through simple transmission/Fresnel/normal variation rather than a heavy screen-space water simulation. Render the needed underside cleanly; avoid duplicate two-sided depth artifacts.

Replace the flat-background-only upward read with a lightweight sky/sun-above treatment that the surface can reveal or imply. A procedural/simple sky is preferred over a high-resolution HDRI dependency. The sun cue must agree with the existing `DirectionalLight3D` direction and color. Surface movement is presentation time, pauses with the game, and obeys the atmosphere enable/disable diagnostic.

New shader/resource names should be explicit, e.g. `water_surface.gdshader`, but names may differ if the source architecture suggests a better owner.

Extend `atmosphere_check.gd` (or the smallest existing focused check) to prove: one bounded surface exists, it is above the normal swim volume and consistent with the upper habitat boundary, material/shader loads, no collision/physics body is added, the effect toggle hides/disables it, pause does not advance its controller-side presentation state, and fish/camera/tuning are unchanged.

## P2 — organic caustics

Replace `MakeCausticTexture()`'s regular sine-lattice result. Use deterministic irregular/cellular/Worley-like ridges or another cheap non-periodic construction generated once at initialization, or an equivalently cheap shader/decal resource that fits Godot's decal path. The visual language should be broad broken loops/veins with empty areas and unequal brightness, not evenly spaced crossing lines.

Keep the projection world-anchored and bounded over the actual habitat. One projection is acceptable; at most two are allowed if a weaker second layer with different scale/rotation/phase materially removes repetition. Do not solve repetition by adding many decals. Keep combined energy restrained so coral/fish silhouettes remain readable.

Animate slowly using offset/rotation/phase without moving the environment or camera. If two layers exist, give them different drift/rotation so their repetition does not synchronize. Avoid obvious axis alignment with the 84 m basin.

Update focused checks for deterministic creation, resource/texture presence, projection count and bounds, finite energies/drift, pause/toggle behavior and unchanged fish/camera state. Structural tests cannot prove the pattern looks organic; retain that visual judgment for P4.

## P3 — light shafts, screen cleanup and integrated cost

Add a **small** set of broad sun-aligned shaft visuals descending from the surface into the existing Forward+ fog. Use cheap transparent/unshaded/additive geometry or an equivalent bounded local technique. Prefer a single batched mesh/material where practical, otherwise keep the number of surfaces/draws small. Use soft edge/end fades, modest low-frequency noise breakup and slightly varied widths/offsets. Do not create multiple real spotlights and do not enable shadows on shaft helpers.

Tie shaft direction and placement to the same sun/surface configuration so they do not contradict the directional light. Keep the center of the play area readable; shafts should be sparse accents, not an opaque curtain.

Reduce whole-screen wobble. The target keeps world geometry stable and puts visible motion in the surface/caustics/rays. Lower `ScreenWarpPixels` to a nearly imperceptible value (roughly 0–0.2 px is an initial range, not a hardcoded acceptance value) or remove UV displacement if the new stack already reads underwater. Retain only restrained color variation if useful. UI must remain on its existing higher CanvasLayer.

Expose/retain a simple atmosphere on/off path so native A/B is reproducible. Extend focused runtime instrumentation or a small ignored probe to record representative FPS/frame-time, draw calls and primitives for the same open/upper route with effects enabled and disabled. Do not treat old P7 numbers as the same-run baseline. If the new stack shows a repeatable median cost around 10% or more, inspect transparent coverage, surface subdivision, shaft count, volumetric range and decal layering and tune before P4.

Run the smallest meaningful build/scene/atmosphere checks, `git diff --check`, confirm no package-lock/vendor payload, and keep root version 0.2.6.

## P4 — native visual/performance closeout

Use a real Fish Forward+ window and the normal P2 habitat. Observe at least:

1. mid-depth open-water view looking horizontally through the reef;
2. upward view near the upper route showing the water surface and bright above-water source;
3. seabed/rock/coral view showing caustic shape and motion;
4. tunnel/shelter movement confirming rays/caustics do not destroy readability;
5. normal swimming/yaw/pitch confirming the world stays stable and the screen does not wobble distractingly.

Run the same route/view sequence with atmosphere/effects enabled and disabled on the same source revision. Record renderer/device/resolution, FPS or frame time plus draw/primitives counters, effect counts and any stutter. Historical P7 medians are context only. Investigate/tune a repeatable ~10%+ median regression or obvious upper-view drop before requesting owner approval; record an explicit owner decision if a larger cost is knowingly accepted.

The owner should judge: surface convincing, sun/rays convincing, caustics organic/not grid-like, underwater depth/color readable, motion comfortable, and cost acceptable. Record PASS/FAIL/NOT OBSERVED separately. A single still image cannot qualify motion.

Create `qualification/implementation.md`, `qualification/native-water-light.md` and the one `closeout.md` only when evidence actually exists. Reconcile owning presentation/technical docs after observed results; preserve P2 Gate truth. Leave final closeout uncommitted for review.

## Validation and hard boundaries

P1–P3 are T2 implementation/integration prompts with no mandatory native pause. P4 is T3 and requires the actual native/owner handoff. Preserve `FishController`, `SwimMotor`, `FollowCamera`, `SwimTuning`, habitat collision, renderer choice, prepared asset identities and UI ownership. No fluid simulation, new gameplay, camera/control rewrite, world rebuild, SDFGI, renderer switch or speculative graphics settings menu.
