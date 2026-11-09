# Swimming and camera

## Status and ownership

**Design approved — 2026-11-09.** The completed [ten-question worksheet](../planning/p1/swim-controls-worksheet.md) preserves the owner's answers and final review. This document owns the accepted core movement and camera contract. [Validation](../validation.md#core-movement-acceptance) owns the accepted playtest scenarios, and [the roadmap](../roadmap.md) owns actual Phase/Gate results.

The owner reported playing a tech demo. The reviewed GitHub snapshot contains the starter and documentation; this review does not establish the local demo's implementation or qualify its feel. Design approval is complete. The implemented core still needs native playtesting and owner acceptance.

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

Backward keyboard input is a slowing command. A turnaround is performed through a curved steering turn. The specific mouse button, speed-demand versus thrust implementation, and any active slowing input for mouse mode remain open.

Controller support follows later, with eventual support for controller as well as desktop controls. The owner's expectation that a controller may feel especially natural does not replace qualification of either desktop mode. **Free look is deferred** for the current controls scope.

## Camera and body presentation

The third-person camera sits behind and slightly above the fish at a moderate distance. Keep its whole body visible and leave enough space ahead to read the route. Follow direction and depth changes smoothly while keeping up with the fish. Near rocks and other obstacles, adjust the camera to keep the fish visible; the exact obstruction technique remains open.

Show visible tail and body motion during swimming, easing during a glide. The body bends and leans into turns. The camera follows smoothly with a steady horizon, letting the fish's movement supply the sense of motion. A steady horizon must remain compatible with following climbs and dives.

Camera offsets, smoothing, animation strength, and obstacle transitions need actual playtesting. A still image or headless position assertion cannot establish comfortable control or camera feel.

## Core acceptance and work order

Implement and tune normal swimming, turning, vertical travel, and slowing first, with the selected camera and body motion. The owner-approved [core acceptance criteria](../validation.md#core-movement-acceptance) cover predictable starting/slowing, curved navigation, depth control, camera comfort, pause/resume, both desktop modes, and enjoyable free swimming.

The core is locked for progression when the required native observations and the owner's acceptance of the implemented feel are recorded. Completing the worksheet or approving this design does not establish that result.

After that core acceptance, the owner now routes the next roadmap work to **[P2 worldbuilding](../planning/p2/underwater-world.md)**. The previously selected [Movement abilities](../planning/p1/movement-abilities.md)—short speed burst, precision mode and quick evasive turn—remain approved future work, but their immediate scheduling is superseded. Feeding/predators/growth begin in P3.

## P1 delivery requirement

The owner requires swimming dynamics to be set by the end of P1. The [P1 plan](../planning/p1/core-swimming.md) and [execution stack](../tasks/p1/README.md) include the four selected ocean starter assets, fullscreen development from the first task, windowed fullscreen for testing, and a dedicated owner tuning loop. Finish with the implemented motor, bindings, camera and body-motion settings recorded against the accepted candidate. The current review has not run that work.

Exact choices below remain open at authoring and must be resolved as needed for the core before P1 passes. Future controller and Movement abilities details stay separate; P2 must consume the accepted P1 movement/camera behavior rather than silently retuning it to make the environment work. Fullscreen/layout/focus behavior is owned by [UX](../ux-and-presentation.md#fullscreen-presentation); runtime evidence is owned by validation V16/V17 and the roadmap's G1.3/G1.4.

## Remaining tuning and bindings

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
