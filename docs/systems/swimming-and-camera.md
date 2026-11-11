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

P10 owner feedback rejected a side-offset follow camera. The accepted progression baseline is centered slightly above, looking down, with modest lag during turns so the fish's sides become visible. Right mouse hold uses the Slow/brake-then-reverse action; middle click or Escape pauses. The scroll wheel zooms the third-person camera in either input mode. In keyboard mode, Left/Right arrows pan the camera; holding Shift makes all four arrow keys control and hold the camera, and releasing Shift returns it. Without Shift, Up/Down still pitch the fish. Further comfort and polish checks remain deferred, as recorded in the closeout.

The C1 correction adds built-in Keyboard Only, Mouse Only and Hybrid Swim profiles plus saved custom bindings in the pause editor. Debug playtests can cycle them with F6 when no preset assigns F6; retail builds use the visible pause selector. The active profile appears briefly after debug switching. The [same-course comparison routine](../tasks/c1-bindings/README.md#development-comparison-on-one-course) remains available for later refinement. The owner accepted the current profile set for P1 progression; the shared motor tuning is unchanged.

**New controls addition, pending owner feel testing:** Hybrid 2 is a fourth built-in preset (`builtin:hybrid2`). Captured mouse X/Y orbit the third-person camera through full horizontal and vertical rotations without recentering or steering the fish. W/S pitch the fish up/down; A/D yaw left/right; left mouse thrusts; right mouse slows; middle mouse or Escape opens Options; the wheel zooms. Camera orbit and zoom remain presentation-only and preserve obstacle avoidance where space permits. The pause selector, Control Presets editor, custom duplication and debug F6 cycle include Hybrid 2. Existing custom files migrate to the existing camera behavior. Hybrid 2 is implemented and checked in headless scene tests, but it is not a preferred or default scheme until the owner tests its feel in native play.

**Adaptive Follow, pending owner feel testing:** `builtin:adaptive-follow` adds a fifth built-in choice. Mouse X/Y steer fish yaw/pitch as in Hybrid Swim; W/S and Up/Down pitch the fish up/down, A/D yaw left/right, left mouse thrusts, and right mouse slows. Holding Left Shift temporarily routes captured mouse motion to an independent 360° camera look without steering the fish; releasing Left Shift smoothly returns the view behind the fish. Normal follow lags turns by about 0.25 seconds, limits temporary heading separation to about 35°, and recenters while swimming straight or gliding. A 2px cumulative mouse noise gate and about 0.12-second steering filter apply only to this preset; keyboard steering takes priority on its axis. The existing motor, zoom, pause, obstacle avoidance, and steady horizon are unchanged. Left Shift is reserved for free-look in Adaptive Follow and copies of it. Existing custom profiles and saved selections retain their camera policy. Headless checks cover behavior and lifecycle; the owner still needs to judge native steering and camera feel.

## Core acceptance and work order

Implement and tune normal swimming, turning, vertical travel, and slowing first, with the selected camera and body motion. The owner-approved [core acceptance criteria](../validation.md#core-movement-acceptance) cover predictable starting/slowing, curved navigation, depth control, camera comfort, pause/resume, both desktop modes, and enjoyable free swimming.

The core is locked for progression by the owner's direct 2026-11-09 acceptance together with the bounded native observations and source checks in [P1 closeout](../tasks/p1/closeout.md). The authored exhaustive per-mode matrix remains incomplete; the closeout does not present absent observations as results.

After that core acceptance, the owner routed the next roadmap work to **[P2 worldbuilding](../planning/p2/underwater-world.md)**, followed by the [P3 FAF reef-world rebuild](../planning/p3/reef-world-rebuild.md). The previously selected [Movement abilities](../planning/p1/movement-abilities.md)—short speed burst, precision mode and quick evasive turn—remain approved future work, but their immediate scheduling is superseded. Feeding/predators/growth begin in P4.

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
- Input captured by menus must not continue swimming or trigger a bite when the menu closes.
- Pausing and resuming must clear stale input without advancing the paused simulation. Intended coasting after input release is distinct from stuck propulsion input.
- Visual scale, collision shape, and camera distance need an explicit policy as the fish grows. A visual enlargement alone must not claim qualified passage through cover.

Third-person collision avoidance should keep the fish readable near rocks and plants. First person needs an intentional eye position, visibility treatment for the fish mesh, and usable near clipping. Switching view must not move the fish through an obstacle or provide a different collision footprint.

## Later integration checks

When the relevant systems exist, extend movement and camera checks to threats above/below, close cover, and pursuing prey near boundaries. Audio, screen-edge indicators, and predator posture remain possible aids; their exact design is open.

For later Movement abilities, any burst must have an observable availability and effect. If stamina or cooldown is selected, its authoritative state persists through view changes and saves. Costs, recovery, and use in escape/pursuit remain later tuning choices.

Preserve the wider game checks: reach food without fighting the camera; use cover during a readable predator approach; preserve fish state through view switches; preserve collision boundaries at multiple sizes; and resume saved positions near cover coherently. Those checks belong to the relevant later [validation scenarios](../validation.md), separate from the initial core lock.
