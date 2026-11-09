# P1 — Core swimming, ocean scene, and fullscreen play

**State: owner-directed scope, assessed and planned; NOT IMPLEMENTED or QUALIFIED by these documents.** Source inspected for authoring: `5dc1c7c73b4f1f0b61579a76ea7e8cce86303613`. The published repository contains the starter and runner; the owner's local tech demo must be inspected before execution. Preserve compatible existing work and reconcile actual P0 evidence.

## Outcome

By the end of P1, ordinary swimming dynamics are settled through implementation, tuning, native observation, and the owner's acceptance in both desktop control modes. The running scene uses the four selected starter assets and supports fullscreen presentation from the first P1 implementation task, including windowed fullscreen for testing. A controllable prototype with remaining core-feel work does not satisfy this phase.

The [swimming contract](../../systems/swimming-and-camera.md) owns the approved behavior, the [completed worksheet](swim-controls-worksheet.md) preserves its source, [art and assets](../../art-and-assets.md) owns selected sources and licenses, and [validation](../../validation.md#core-movement-acceptance) owns the seven playtest criteria. [P1 Gates](../../roadmap.md#p1--swim-through-a-habitat) alone own progress. The [execution stack](../../tasks/p1/README.md) turns this design into bounded tasks.

## Phase finish line

| Required result | Evidence required before P1 passes |
| --- | --- |
| Natural core movement | Quick acceleration, gradual release slowdown, controlled keyboard braking, curved turns with mild anticipation, pitch-based depth travel, reliable collision and input handling. |
| Both desktop modes | The same course is comfortable in mouse-only mode and keyboard fallback, including pause, settings, and resumption with each device method alone. |
| Camera and body feel | Whole-fish framing from behind/slightly above, moderate distance, smooth follow, obstacle adjustment, steady horizon, visible swimming/gliding motion and bend/lean in turns. |
| Selected ocean scene | Actual Emperor Angelfish, Rock Face 01, Coast Sand 01, and Soft Coral Set integrated and observed; exact asset preparation and distribution boundaries recorded. |
| Fullscreen presentation | Fullscreen startup and a borderless windowed-fullscreen testing option, readable layout and camera at actual display sizes, safe focus/mode transitions in both input modes. |
| Settled dynamics | A saved, reproducible tuning profile and controls description, identified source/configuration/asset candidate, all seven checks, and the owner's explicit acceptance of several minutes of actual swimming. |

G1.1 through G1.4 must all pass. A missing selected asset, a control mode that remains uncomfortable, unfinished camera/body behavior, or absent owner acceptance keeps P1 incomplete. Checkpoints can record useful partial implementation without passing the phase.

## Scope and sequence

Establish the fullscreen play shell first. Acquire and prepare the four starter assets, assemble a compact course, and observe the real scene and display behavior before the main movement-tuning work. Build one shared motor, the keyboard and mouse adapters, then camera and visual motion. Tune the integrated result with the owner until it meets the accepted criteria; finish with one evidence review.

Movement abilities starts after the accepted core and the P1 review: short speed burst, precision mode, and quick evasive turn remain the next movement follow-up. Controller, free look, optional first person, food/prey/predator behavior, growth, saves, death, stories, production water, and broader habitat expansion remain outside this stack. A small pause/settings panel and local display/input preferences are in scope; they do not implement the later survival menu or living-save system.

## Concrete implementation starting points

The following are reversible implementation defaults for P1, not additional quotations from the owner. Adjust them when actual source or playtests justify a change, preserving the approved qualitative model. Record final choices before P1 closes.

### Shared movement and input

Use a single C# motor on the authoritative collision body. It owns position, orientation, velocity, and collision response in physics time. Input adapters request propulsion, slowing, yaw and pitch; visual animation and the camera read that state. Do not make separate mouse and keyboard physics implementations.

Begin with a speed-demand motor: held propulsion accelerates promptly toward a bounded cruise speed, release produces a slower decay, and keyboard backward input increases slowing without requesting reverse travel. Turning changes heading at a bounded rate while velocity aligns with a slight lag. Tune alignment, turn rate, passive slowdown and braking independently enough to remove excessive sliding or steering delay. Preserve usable steering near rest and a way to turn away from a wall. No automatic leveling is the initial choice; pitch limits and near-vertical response are tunable and must be settled in play.

| Action | Initial mouse-only mapping | Initial keyboard fallback mapping |
| --- | --- | --- |
| Steer | Relative mouse motion changes yaw/pitch intent; consume each motion delta once. | A/D steer left/right; Up/Down pitch. |
| Propel | Hold primary mouse button. | Hold W. |
| Slow | Release propulsion to coast; no added active mouse brake in the initial mapping. | Release W to coast; S actively slows. |
| Pause/options | Secondary-button press releases capture and opens the panel. | Escape opens the panel. |
| Resume and choose settings | Pointer/click controls. | Visible keyboard focus, navigation and activation. |

Use explicit mode selection; incidental mouse movement must not switch modes. Clear pending deltas and held intents on pause, focus loss, mode change and display transition. Resume deliberately, and require release/rearm so the click used to resume cannot immediately propel the fish. Pause the test simulation on focus loss. Keep this behavior consistent across both display modes.

### Camera and animation

Follow the unbanked gameplay root, with separate visual body lean. Use time-based smoothing and an obstacle query that excludes the player. Keep readable space ahead and recover camera distance smoothly after obstructions clear. Handle climbs/dives and near-vertical orientation without horizon roll or singular jumps. Avoid stacking enough motor, camera and animation lag to make steering feel delayed.

Inspect the imported Angelfish skeleton and actual clip names. Drive effort/glide blending from propulsion and measured motion, and turn deformation from steering. Keep imported root motion from translating or rotating the collision root. Use suitable real clips or a bounded bone/visual adaptation of this selected asset; an advertised turn animation is not proof of usable turning. Record the actual animation mapping and values.

## Selected asset integration

| Selected asset | P1 use |
| --- | --- |
| Emperor Angelfish | Controlled visual fish with prepared Godot materials, usable swim/glide motion, and turn bend/lean. It remains an art-test candidate, not the final species decision. |
| Rock Face 01 | Recognizable obstacles, circling route, and a passage; author collision that agrees with visible clearance. |
| Coast Sand 01 | Material on Fish-authored seabed geometry, with usable spatial scale. |
| Soft Coral Set | Selected coral pieces providing reef context and readable spatial cues; deliberately specify any collidable pieces. |

Keep the course small and reproducible: open travel, a slowing landmark, upper/lower landmarks, a rock loop, turnaround space, a gap, and nearby geometry that tests camera visibility. Record scene scale, starting position and route. Do not import the full later shortlist. Inspect Sea life only if a concrete follow-up need warrants it; it does not silently replace a required starter asset.

The public repository must not contain restricted vendor model/texture/animation data, including converted or embedded copies. Before acquisition/preparation, establish and verify an ignored in-project location for restricted assets and keep source archives outside tracked public content. Public code can load a documented local resource path and give a clear missing-asset diagnostic. Do not use static scene dependencies that make the project unreadable on a clone without the private resources, or silently qualify a placeholder as the selected fish. A diagnostic placeholder can help isolated development but cannot pass G1.3 or final acceptance.

During execution, create a public-safe per-asset manifest with creator/source, exact package/version, acquisition date, hashes, license/version and credit/modification requirements, preparation steps, and distinct acquired/prepared/imported/observed states. Retain exact source terms with the acquired package. Record local installation and import mapping. Preserve the existing Compatibility renderer unless a measured incompatibility warrants a separately documented decision.

## Fullscreen from the first task

Design camera framing and interface layout for the actual screen from P1's first runnable change. A fresh settings profile starts in Fullscreen; provide Windowed fullscreen for easy switching between the game and tools. Both choices use the full display. Menus must be usable with the mouse alone or keyboard alone. Retain only local display/input preferences; a developer launch override is temporary and must not overwrite the saved choice.

Proposed engine mapping: Fullscreen requests Godot's exclusive-fullscreen mode where supported; Windowed fullscreen uses its ordinary fullscreen mode. The latter fills the display without decorations. Some backends, including Wayland, treat these modes equivalently; record the effective behavior rather than claim exclusive presentation. Verify against the installed engine and [official Window mode documentation](https://docs.godotengine.org/en/stable/classes/class_window.html#enum-window-mode).

Provide an explicit developer launch option such as `./tools/dev run --display-mode=windowed-fullscreen`, implemented and documented in P1. Validate the option and forward it as a game-owned argument; it is not an existing command or an assumed Godot flag. Godot supports user arguments after `--`; see its [command-line reference](https://docs.godotengine.org/en/stable/tutorials/editor/command_line_tutorial.html#command-line-reference). Keep the shell's ordinary run behavior and existing build composition intact.

Observe the actual native game window. An editor-embedded preview can help iteration but cannot qualify desktop fullscreen/focus behavior. Check current monitor size, a second practical resolution/aspect when available, anchored menu/help layout, mouse capture and Alt-Tab/refocus. Record the tested display/backend scope. Fullscreen selection and viewport size must not change the motor's simulation behavior.

## Tuning and acceptance loop

Tune in a repeatable order: acceleration/coasting and slowing; curved steering and residual momentum; pitch/near-rest travel; camera; body motion; integrated course. Change a small related set of values at a time and retain the observation that motivated each change. Keep a simple saved resource/configuration for the profile; a large tuning framework is unnecessary.

Use the seven approved checks in both desktop modes. Also smoke the two-input-by-two-display-mode matrix for capture, pause/resume, framing and mode transitions. The owner spends several minutes swimming with each input mode and gives an actual comfort/feel judgment. Shared motor/camera/animation changes require checking the affected experience in both modes again.

Before acceptance, record the chosen bindings, sensitivity/inversion, speed-demand model, cruise speed, acceleration, coast/braking, steering/alignment, pitch/near-rest policy, camera framing/smoothing/obstruction, animation mapping, and supported display behavior. Bind that record to the final source, tuning configuration and imported-asset identity. Do not invent numerical tolerances or owner feedback after the fact. Unfinished core tuning cannot be relabeled as later polish.

## Execution and evidence

The [P1 assessment](../../tasks/p1/prompt-assessment.md) defines risks and decomposition; the [implementation plan](../../tasks/p1/implementation-plan.md) routes files, task dependencies and checks. The stack has early native display/asset inspection, a dedicated owner tuning task, and a single final review. Follow the [entry and resume procedure](../../tasks/p1/manual/entry-and-resume.md) before running it: the authoring snapshot is still version `0.0.0`, and actual P0 evidence plus a deliberate `0.1.0` baseline are prerequisites.
