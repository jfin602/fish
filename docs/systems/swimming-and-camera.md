# Swimming and camera

## Status and ownership

**Design approved — 2026-11-09.** The completed [ten-question worksheet](../planning/p1/swim-controls-worksheet.md) preserves the owner's answers and final review. This document owns the accepted core movement and camera contract. [Validation](../validation.md#core-movement-acceptance) owns the accepted playtest scenarios, and [the roadmap](../roadmap.md) owns actual Phase/Gate results.

The [P1 closeout](../tasks/p1/closeout.md) records the actual implemented baseline and the owner's 2026-11-09 acceptance for progression. Its native and source-check limits remain explicit. Further movement/control refinement is deferred until later world/gameplay work provides context.

Third person is the primary view. Optional first person remains part of the wider game for the same fish. [Ecology and growth](ecology-and-growth.md) owns size and feeding; [UX](../ux-and-presentation.md) owns presentation beyond swimming.

## Accepted core feel

The fish should feel natural, nimble, agile, and easy to control. A slight sense of water resistance and momentum rewards starting a turn a little early. Acceleration is quick; slowing after propulsion input is released is more gradual. Tune these qualities together so the player can approach a target and navigate deliberately.

Left/right steering makes curved turns. Depth changes come from pitching the fish's nose upward or downward and swimming in that direction. These are desired movement qualities; the precise movement algorithm and numerical response remain tuning choices.

## Initial control modes

Keyboard/mouse development and qualification come first. Both desktop modes must be usable on the same playtest course.

| Mode | Accepted behavior |
| --- | --- |
| Mouse-only | Mouse movement steers the fish; vertical mouse movement pitches it up/down. Holding a mouse button controls swimming speed or thrust. Releasing propulsion produces the selected gradual slowdown. The mode is playable using the mouse alone. |
| Keyboard fallback | Provides control without an external mouse. Left/right input steers through curved paths; backward input slows the fish. Up/down steering inputs pitch the fish for vertical travel. Exact propulsion and steering keys remain to be assigned. |

The Slow input starts swimming backward immediately from a complete standstill, up to 1.5 m/s. From forward motion it first brakes to a full stop, waits 250 ms while Slow remains held, then reverses. Releasing it coasts, and forward thrust first brakes reverse motion before accelerating forward. The accepted built-ins use W/left mouse for thrust and S/right mouse for Slow; [accepted P1 tuning](../tasks/p1/qualification/accepted-tuning.md) records the earlier no-reverse baseline, which this later owner-directed change supersedes. Turning remains available at rest and while reversing.

Controller support follows later, with eventual support for controller as well as desktop controls. The owner's expectation that a controller may feel especially natural does not replace qualification of either desktop mode. During P10, the owner revised the camera scope: in mouse mode, W/S move the camera up/down and A/D orbit it around the fish, returning to center after release; holding Shift retains that offset until Shift is released. The fish remains steerable and pausable using only the mouse. This describes the accepted P1 Mouse Only behavior; Hybrid 2 adds a separate camera orbit below.

## Camera and body presentation

The third-person camera sits behind and slightly above the fish at a moderate distance. Keep its whole body visible and leave enough space ahead to read the route. Follow direction and depth changes smoothly while keeping up with the fish. Near rocks and other obstacles, adjust the camera to keep the fish visible; the exact obstruction technique remains open.

Show visible tail and body motion during swimming, easing during a glide. The body bends and leans into turns. The camera follows smoothly with a steady horizon, letting the fish's movement supply the sense of motion. A steady horizon must remain compatible with following climbs and dives.

The current Fish3 presentation also idles after speed and propulsion settle: a slow, small wave through the existing Root, Spine3 and Tail bones, with a very slight hover and sway on `PlayerVisual` only. Speed hysteresis and a smoothed idle weight carry motion through swim, glide and idle without changing the motor, collider or camera. `FishVisualMotion` exposes idle speed thresholds, frequency, amplitudes and transition response for tuning. Fish3 has no separate fin or gill bones, so those parts retain their imported pose; no fin or gill motion is claimed. The presentation clock stops with the paused scene tree.

Camera offsets, smoothing, animation strength, and obstacle transitions need actual playtesting. A still image or headless position assertion cannot establish comfortable control or camera feel.

### Follow-camera smoothing repair (2026-11-14; owner review open)

The follow rig and fish now interpolate physics poses for rendered frames at 30/60/120 FPS. Camera collision and follow position remain physics-tick decisions; the camera's look orientation is reconstructed each rendered frame from the interpolated safe eye and look target. Vertical follow and pitch presentation are filtered independently, with a faster response during deliberate vertical swimming. Collision compression now uses the fast response only when an obstacle actually blocks the desired eye; recovery is tracked until the obstruction clears. Spawn, hatch handoff, teleports and viewport resize snap both the view and interpolation history. The motor and Pond lighting are unchanged.

The repeatable [camera motion probe](../../game/dev/camera_motion_probe.gd) and [camera scene check](../../game/dev/camera_scene_check.gd) pass. On the same scripted 120 FPS ascent route, stationary rendered camera frames fell from 120/240 to 0/240; the largest coast vertical step fell from 0.0637 m to 0.0093 m. The scene check covers obstruction retraction/recovery, steady horizon, pause and resize; the Pond immersion check covers all five presets, orbit/zoom and surface reentry. A native Forward+ Pike Encounter launch and Pond screenshots confirm the current scene renders, but the owner still needs to judge swimming, boost, obstacles, surface crossing and all camera presets in play. No visual Gate is GREEN from this evidence alone.

P10 owner feedback rejected a side-offset follow camera. The accepted progression baseline is centered slightly above, looking down, with modest lag during turns so the fish's sides become visible. Right mouse hold uses the Slow/brake-then-reverse action; middle click or Escape pauses. The scroll wheel zooms the third-person camera in either input mode. In keyboard mode, Left/Right arrows pan the camera; holding Shift makes all four arrow keys control and hold the camera, and releasing Shift returns it. Without Shift, Up/Down still pitch the fish. Further comfort and polish checks remain deferred, as recorded in the closeout.

The C1 correction adds built-in Keyboard Only, Mouse Only and Hybrid Swim profiles plus saved custom bindings in the pause editor. Debug playtests can cycle them with F6 when no preset assigns F6; retail builds use the visible pause selector. The active profile appears briefly after debug switching. The [same-course comparison routine](../tasks/c1-bindings/README.md#development-comparison-on-one-course) remains available for later refinement. The owner accepted the current profile set for P1 progression; the shared motor tuning is unchanged.

**New controls addition, pending owner feel testing:** Hybrid 2 is a fourth built-in preset (`builtin:hybrid2`). Captured mouse X/Y orbit the third-person camera through full horizontal and vertical rotations without recentering or steering the fish. W/S pitch the fish up/down; A/D yaw left/right; left mouse thrusts; right mouse slows; middle mouse or Escape opens Options; the wheel zooms. Camera orbit and zoom remain presentation-only and preserve obstacle avoidance where space permits. The pause selector, Control Presets editor, custom duplication and debug F6 cycle include Hybrid 2. Existing custom files migrate to the existing camera behavior. Hybrid 2 is implemented and checked in headless scene tests, but it is not a preferred or default scheme until the owner tests its feel in native play.

**Adaptive Follow, pending owner feel testing:** `builtin:adaptive-follow` adds a fifth built-in choice. Mouse X/Y steer fish yaw/pitch as in Hybrid Swim; W/S and Up/Down pitch the fish up/down, A/D yaw left/right, left mouse thrusts, and right mouse slows. Holding Left Shift temporarily routes captured mouse motion to an independent 360° camera look without steering the fish; releasing Left Shift smoothly returns the view behind the fish. Normal follow lags turns by about 0.25 seconds, limits temporary heading separation to about 35°, and recenters while swimming straight or gliding. A 2px cumulative mouse noise gate and about 0.12-second steering filter apply only to this preset; keyboard steering takes priority on its axis. The existing motor, zoom, pause, obstacle avoidance, and steady horizon are unchanged. Left Shift is reserved for free-look in Adaptive Follow and copies of it. Existing custom profiles and saved selections retain their camera policy. Headless checks cover behavior and lifecycle; the owner still needs to judge native steering and camera feel.

## S27 — P6 feeding reticle and truthful mouth heading (approved design; not implemented)

A tiny, unobtrusive, viewport-centered reticle gives the player a stable visual reference for directing the Bass toward food. It must not be interpreted as the contact volume or an automatic aiming command. The fish's forward mouth is stage-dependent and authoritative, whereas the third-person camera may lag, orbit independently in Hybrid 2, or temporarily look away in Adaptive Follow. When the actual mouth-forward direction projects noticeably away from the camera center, show a **separate subtle heading marker** projected from the current fish mouth/forward ray, not a fabricated center-only bite promise. Handle offscreen/behind-camera cases without misleading arrows, clamp or suppress noisy cues and avoid camera jitter. All indicators remain presentation only.

Show the gameplay reticle only when the fish is controllable: hide during hatch cutscene, menus, paused/unfocused/disarmed controls and session transitions. At a genuinely committed mouth-contact feeding event, a brief restrained pulse may confirm success. Never steer the motor, change camera look, alter collision or prey reach, award nutrition, reveal hidden prey, or inform NPC detection from the reticle. Retain proper visibility at larva through adult scale, camera presets, zoom and above/below Pond surface. Owner must judge actual food-aiming comfort in native play under [V44](../validation.md); headless projection checks are not sufficient.

[UX](../ux-and-presentation.md) owns visuals and [ecology](ecology-and-growth.md) owns actual feed eligibility. [P6 design](../planning/p6/living-pond.md) owns the implementation sequence and the source branch must be checked for unpushed camera changes before editing.

## S25 — limited Spacebar boost (M7 headless implementation; native review open)

**Hold Spacebar** in the Pike Encounter for a limited forward swimming boost through the existing motor. FishController owns a normalized finite energy reserve, currently with a `1.8×` speed/acceleration multiplier, `2.5 s` full drain, `5 s` full recharge and `0.25` re-arm fraction. These are exposed tuning defaults, not owner-locked balance. Release or exhaustion ends boosted acceleration, preserving normal coasting, real collision and waterline limits. The player does not press a bite key or auto-target prey. [M7 preflight](../tasks/c5-polish/qualification/preflight.md) records measured headless speed and remaining native review.

The encounter reserves Space for boost across current presets, including older custom profiles; ordinary mouse-only swimming still uses its normal control. Hatch/cutscene, pause, menu, focus loss, retry and disarm clear held boost, requiring a fresh valid press. Camera/UI only read energy. Faster movement increases existing world-space disturbance rather than sending a keypress to Pike or Bluegill. Future saving must preserve authoritative energy. [Ecology](ecology-and-growth.md) owns automatic mouth feeding. Native steering/camera feel remains unobserved.

## P4 — Pond surface swimming boundary and planned camera transition

The owner-approved [P4 Pond plan](../planning/p4/freshwater-world.md#surface-swimming-and-airwater-camera-transition-p4-requirement) extends the already accepted P1 underwater baseline. P9 implements the Pond physical boundary at `0.4.9` with headless checks. Camera crossing and presentation remain planned for P10–P12.

- The Pond motor keeps one authoritative waterborne fish position/collision body. Its center approaches the generated water plane minus 65% of the current collider radius, with upward velocity redirected along the surface through a 0.35 m band. Selectable fish in Pond and Ocean use a capsule fitted to the actual attached player mesh; Pike Encounter Bass retains its registered stage capsule. The old Pond upper collision is disabled. Headless Bluegill checks show partial anatomical breach and sustained bounded skimming. Jumping and land movement are deferred.
- Third-person follow/orbit/zoom, including existing bindings and Adaptive Follow, may place the **camera** above the surface without lifting the fish. Crossing back and forth must avoid snapping, clipping, unstable horizon, jitter and obstacle-avoidance errors at banks/cattails.
- The camera's immersion state drives air–water fog/filter/lighting transitions, not movement or simulation. Repeated crossings, pause/focus and scene reload must return to correct underwater treatment and preserve fish/camera control.
- One shared WaterReaction maps actual movement to bounded wake particles in Ocean, Pond and Pike Encounter. It measures the current player mesh for wake origins, size and surface contact, scattering small particles along the rear body with varied lifetimes. Pond uses its darker water palette; Ocean uses its lighter palette. Pond surface rings respond to actual fish depth and velocity as visual-only effects. Shoreline geometry and vegetation are drawn from the generated Pond banks. [Correction evidence](../tasks/c5-polish/qualification/water-reactions.md) records focused checks and the remaining owner visual review.
- Bird silhouettes/audio/shadows are possible future scene consumers, not P4 predator AI.

Qualify the above in native Forward+ play via [V32](../validation.md#gameplay-scenarios) and G4.2. Do not retroactively mark earlier P1/P2 underwater tests as having qualified above-water camera behavior.

### Dynamic Pond waterline correction — 2026-11-15, owner visual review open

The P9 description above is historical. The current Pond and Pike Encounter surface boundary samples the same three geometric waves as `water_surface.gdshader` at the fish's world X/Z. `PondWaterline` evaluates the shader's vertex displacement with the selected wave height, speed and detail; the generated basin's signed shoreline distance supplies the same 1.6 m attenuation used to color the water vertices. Fragment-stage ripple and reflection distortion do not move geometry. Mesh vertex colors are quantized to 8 bits, and the analytic shoreline sample does not interpolate triangle vertex colors, so sub-centimeter differences can remain within the narrow shoreline band. No water mesh or physics shape is rebuilt per frame.

`UnderwaterAtmosphere` advances the scene-local wave clock on physics ticks before the fish, then sends the physics-interpolated phase to the shader on rendered frames. Pause freezes both; settings update the shader and CPU sample together. A surface-contacting fish follows the local limit's change with a bounded vertical correction of at most 1 m/s. The correction is kept out of the swim motor's stored velocity; deep fish receive none. The current Pond center limit is sampled surface height minus 35% of the registered collider radius, allowing the visible dorsal body to meet the water while retaining a partial breach. Growth immediately uses the next registered radius. `WaterReaction` places and updates world-anchored rings on the same local sample, and gates ring/splash eligibility by the measured visible body top. Boost still strengthens eligible effects and keeps underwater turbulence below the surface. Camera immersion samples the local rendered phase with its existing smooth transition; the camera itself does not bob with the wave. Ocean has no Pond swim-boundary change.

Focused import/build, dynamic waterline, Pond ascent, camera immersion, scenery, Bass growth, auto-feed/boost and Ocean equal-input checks passed. The dynamic check found zero wave-formula error at sampled mesh vertices (allowing 0.0041 for packed shoreline color), a maximum 0.0049 m skimming gap, no deep-fish vertical drift over 120 physics frames and 27–43 ms for 100,000 surface samples on this desktop. A native Forward+ Pike probe measured 12.23 ms for 10,000 GDScript-to-C# surface reads, including call overhead. Its side view captured a juvenile Bass at three one-second intervals while the wave rose from 10.396 m to 10.452 m. The fish followed from 10.368 m to 10.429 m, leaving a 0.0027–0.0064 m gap to its moving center limit. Temporary red surface/yellow limit markers showed the visible dorsal body meeting the sampled interface; the markers exist only in the development probe, not normal play. Surface-effect eligibility measures the rest-pose body top at each stage change, not each animated fin pose. These fixed-camera captures do not qualify player-controlled skimming, diving, boost splashes, camera feel or all growth-stage visuals. Owner playthrough remains open; no P4/P5 Gate changes.

## S26 — Body-generated natural Pond surface wakes (approved design; implementation pending)

The existing `WaterReaction` stage-scaled underwater tail/bubble presentation is retained. The current pool of solid-colored Pond `SurfaceRing` circles is to be replaced by subtle surface disturbance starting at actual visible fish body/water contact: low-amplitude bow pressure, two diverging wave fronts whose world-space history follows real travel/turns, and faint tail turbulence. Waves spread and fade independently after the fish passes rather than staying rigidly attached. Normal/reflection/highlight breakup should be natural and translucent, not opaque painted geometry.

Use the **current shared dynamic waterline** for fish contact and wake elevation, verify local (potentially unpushed) wave-height implementation first, and scale from Bass larva/fry through adult. Real Spacebar boost intensifies the effect only where the fish moves and contacts the surface. Deep, resting and blocked fish create no new surface wake. All wake code remains read-only presentation: no new motor/collider/camera, game-clock, AI or disturbance-field authority. Both normal Pond and Pike Encounter use it; Ocean underwater behavior remains unchanged. [C5 Wake](../planning/c5-wake.md) owns design, [manual W1–W3](../tasks/c5-wake/README.md) own planned work, and [V43](../validation.md) requires real native owner judgment. No Wake implementation or Gate pass is claimed by this paragraph.

## Core acceptance and work order

Implement and tune normal swimming, turning, vertical travel, and slowing first, with the selected camera and body motion. The owner-approved [core acceptance criteria](../validation.md#core-movement-acceptance) cover predictable starting/slowing, curved navigation, depth control, camera comfort, pause/resume, both desktop modes, and enjoyable free swimming.

The core is locked for progression by the owner's direct 2026-11-09 acceptance together with the bounded native observations and source checks in [P1 closeout](../tasks/p1/closeout.md). The authored exhaustive per-mode matrix remains incomplete; the closeout does not present absent observations as results.

After that core acceptance, the owner routed the next roadmap work to **[P2 worldbuilding](../planning/p2/underwater-world.md)**, followed by the [P3 FAF reef-world rebuild](../planning/p3/reef-world-rebuild.md). The original [Movement abilities](../planning/p1/movement-abilities.md) were deferred after P1. S25 schedules limited Spacebar boost for C5 while precision/evasive turn stay deferred. Feeding/predators/growth belong to P5 after P4.

## P1 delivery requirement

The owner accepted the current dynamics as the P1 baseline and deferred additional tuning. The [P1 plan](../planning/p1/core-swimming.md) and [execution stack](../tasks/p1/README.md) retain the original owner tuning brief; [accepted tuning](../tasks/p1/qualification/accepted-tuning.md) records actual motor, bindings, camera and body-motion settings against the final source.

The table below is the original tuning agenda, now retained for later gameplay-informed refinement. Future controller and Movement abilities details stay separate; P2 consumes the accepted P1 movement/camera behavior. Fullscreen/layout/focus behavior is owned by [UX](../ux-and-presentation.md#fullscreen-presentation); runtime evidence is owned by validation V16/V17 and the roadmap's G1.3/G1.4.

## Deferred tuning and bindings

| Area | Still to determine |
| --- | --- |
| Input details | Exact keys and mouse buttons, keyboard propulsion input, sensitivity, comfort settings, and the input needed for any active mouse-mode slowing. |
| Movement response | Speed/thrust implementation, cruising speed, acceleration and slowing rates, turn radius/rate at different speeds, near-stationary handling, pitch limits, and any leveling assistance. |
| Presentation | Exact camera offsets and framing, follow smoothing, obstacle adjustment, optional zoom behavior, animation cadence/amplitude, and transitions between effort and gliding. |
| Later work | Controller layout/milestone, free-look design if revisited, and the detailed Movement abilities design. |

These details should be chosen and documented during implementation and playtesting without changing the accepted qualitative control model. Neither a realistic fluid solver nor a particular animation technique has been selected.

## State boundaries

- The fish has one authoritative position, orientation, velocity, and collision body.
- Input requests movement or actions; the camera does not own the fish's survival state.
- Camera changes preserve identity, location, growth, health, resources, and active encounters.
- Input captured by menus must not continue swimming, trigger feeding, or restart a stale held boost when the menu closes.
- Pausing and resuming must clear stale input without advancing the paused simulation. Intended coasting after input release is distinct from stuck propulsion input.
- Visual scale, collision shape, and camera distance need an explicit policy as the fish grows. A visual enlargement alone must not claim qualified passage through cover.

Third-person collision avoidance should keep the fish readable near rocks and plants. First person needs an intentional eye position, visibility treatment for the fish mesh, and usable near clipping. Switching view must not move the fish through an obstacle or provide a different collision footprint.

## Later integration checks

When the relevant systems exist, extend movement and camera checks to threats above/below, close cover, and pursuing prey near boundaries. Audio, screen-edge indicators, and predator posture remain possible aids; their exact design is open.

S25's finite boost needs observable authoritative energy/effect through view changes and future saves; costs and recovery remain native tuning choices. Precision mode and evasive turning are later work.

Preserve the wider game checks: reach food without fighting the camera; use cover during a readable predator approach; preserve fish state through view switches; preserve collision boundaries at multiple sizes; and resume saved positions near cover coherently. Those checks belong to the relevant later [validation scenarios](../validation.md), separate from the initial core lock.
