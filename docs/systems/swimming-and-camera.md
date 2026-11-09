# Swimming and camera

## Status and ownership

**Planned; no implementation or native qualification.** The confirmed perspective is 3D with third person as the primary view and an optional first-person view for the same fish. This document owns player movement and camera behavior; [ecology and growth](ecology-and-growth.md) owns size and feeding, and [UX](../ux-and-presentation.md) owns visible feedback.

## Working prototype

Use a compact 3D habitat with clear depth cues, a visible fish body, and a camera that leaves enough space to read nearby threats. Direct swimming, controlled turning, vertical movement, and a short burst of speed are the proposed initial actions. Exact keyboard, mouse, and controller mappings remain task-level choices.

Swimming should be responsive while still conveying a moving body. The first prototype should establish how the player aims, changes depth, turns around cover, and stops or slows. Detailed hydrodynamics, currents, advanced animation, and multiple locomotion styles are later possibilities.

## State boundaries

- The fish has one authoritative position, orientation, velocity, and collision body.
- Input requests movement or actions; the camera does not own the fish's survival state.
- Camera changes preserve identity, location, growth, health, resources, and active encounters.
- Input captured by menus must not continue swimming or trigger a bite when the menu closes.
- Pausing and resuming must clear stale input without advancing the paused simulation.
- Visual scale, collision shape, and camera distance need an explicit policy as the fish grows. A visual enlargement alone must not claim qualified passage through cover.

Third-person collision avoidance should keep the fish readable near rocks and plants. First person needs an intentional eye position, visibility treatment for the fish mesh, and usable near clipping. Switching view must not move the fish through an obstacle or provide a different collision footprint.

## Threat readability

Movement and camera tests must include threats above and below the player, close cover, a tight turn, and pursuing prey near a boundary. Audio, screen-edge indicators, and predator posture are possible aids. Their exact design is open; the target is an understandable opportunity to react.

Any burst must have a clearly observable availability and effect. If stamina or cooldown is selected, its authoritative state persists through view changes and saves. Numeric costs, recovery speed, and whether burst helps both escape and pursuit remain tuning choices.

## Acceptance targets

These are future checks, not completed results:

- Navigate in all intended directions and reach visible food without fighting the camera.
- Use cover to escape a readable predator approach.
- Keep the same fish state through repeated third-/first-person switches.
- Preserve collision boundaries at multiple fish sizes.
- Pause, open a menu, resume, and regain control without stuck movement.
- Resume a saved position near cover with coherent camera and collision state.

Use [validation](../validation.md) for evidence standards. Headless position assertions cannot establish camera comfort or readable native presentation.
