# C5 Wake — natural fish-generated surface wakes

**Decision:** S26 (2026-11-15), owner-approved design, with a subsequent owner choice of the hybrid W2-R1 candidate and a 0.2 m near-surface wake band. **Status:** W1 and W2 implemented; [W2-R1](../tasks/c5-wake/qualification/w2-r1.md) is the historical trial, while [W2-R2](../tasks/c5-wake/qualification/w2-r2.md) promotes hybrid and repairs rear-body anchors and shallow-dip continuity. Native owner playfeel remains open; W3 remains open. **Execution:** [manual W1–W3](../tasks/c5-wake/README.md). No package-version or Roadmap Gate change.

## Player-visible objective

Replace Pond's flat solid-color expanding ring meshes with a restrained, believable **body-generated wake**. As a fish advances near the surface, a shallow disturbance begins around the mid-body and two delicate wave fronts peel away from the rear flanks. Fine tail turbulence follows. These disturbances bend along the historical path, spread outward, fragment, lose strength and dissolve. A wake must look like water's changing normals, reflection and highlights—not white/green painted circles, straight permanent V-shaped decals or solid translucent ribbons.

The shared Pond effect is required in ordinary Pond and Pike Encounter. The existing underwater trailing particles and bubbles are separate and preserved. Ocean underwater presentation remains intact; Ocean surface wake implementation is not part of this correction.

## Expected response

| Motion / body state | Intended surface presentation |
| --- | --- |
| Idle, negligible displacement | No new wake; existing waves gradually expire in world coordinates |
| Slow glide | Very weak, narrow and short-lived pressure front |
| Normal cruise | Soft paired diverging wave fronts with understated tail texture |
| Acceleration, sharp turn | Stronger curvature/breakup on the real traveled path; no turn-in-place dragging of old waves |
| Genuine active Spacebar boost near surface | Clearly more pronounced and somewhat longer disturbance with occasional small plausible splash; no giant high-contrast foam trail |
| More than 0.2 m below the local surface or blocked propulsion | No new surface wake even if boost button is held; underwater wake remains separate |
| Shallow dip / deeper transition | Keep the surface path continuous while the fish origin is within 0.2 m below the moving local surface, with a 0.05 m release margin to prevent boundary flicker; then let previous history decay |
| Egg / hatch lock | No wake |
| Bass larva/fry, juvenile, subadult, adult | Dimensions, anchor locations and effect amplitude scale with the registered visible body |
| Pause, focus loss, retry, world exit | No ghost bursts, stale position bridges or leaking nodes/resources |

Exact width, sample interval, lifetime, lateral travel, boost gain and effect count are tunable rather than owner-locked numerical values.

## Implementation contract

1. **Body/contact truth:** use the actual stage's model geometry, transform and orientation to establish stable mid-body, rear-flank and tail contact anchors. `PlayerBodyGeometry` reports rest-pose mesh extents, not an exact animated skin intersection; verify practical anchors in native footage. Never assume a single adult tail offset.
2. **Shared moving water:** inspect the latest **local** dynamic-waterline implementation. GitHub may lag local work. Use the same sampled surface height/phase as FishController boundary and the visible water; do not create another wave clock or sample only an old fixed `_pondWaterY`. Only geometric displacement influences physical contact; normal-map ripples are visual-only.
3. **World-space path:** keep bounded, pooled samples of world X/Z, historical direction, birth time, body width, contact intensity and stage. Sample from actual displacement at controlled distance/time, not from camera motion, GPU frame count or merely boost input. Older wake positions remain attached to the world path, while their wave fronts propagate sideways based on stored headings.
4. **Rendering:** prefer a fixed-capacity pair of curved, feathered wake ribbons or similarly bounded method, emphasizing normals/specular highlights and soft translucency. Bow pressure and tail micro-turbulence must be subtle and integrated, with correct moving-wave Y and no Z-fighting, shore spill, jagged segment joins or bright solid-color rings.
5. **Authority:** `WaterReaction` reads movement and water contact; it never writes velocity, forces, collision, camera, authoritative time, predator/prey sensing or game state. More visible boost wake does not create an extra AI detection channel.
6. **Performance:** avoid CPU rebuilding the entire pond mesh, expensive fluid simulation, per-frame spawned nodes or unbounded buffers. Qualify normal and boosted frame pacing on native Godot Forward+ and low-effects behavior.

## Manual execution

- **W1 — contact and wake history:** inspect local water-height contract, build testable body-origin/side-history data plus development-only debug paths; T2 source/scene checks, then review.
- **W2 — natural surface rendering:** replace ring appearance with paired, curved, outward-propagating and fading disturbances; capture native side/overhead/underwater A/B, measure cost, then review.
- **W3 — growth/boost/qualification:** tune through playable Bass stages, actual velocity/boost/turn/depth and pause/retry; verify both Pond modes/Ocean isolation; launch the game for owner T3 visual qualification.

[Task instructions](../tasks/c5-wake/README.md) · [V43](../validation.md) · [Swimming and camera](../systems/swimming-and-camera.md). W1–W3 are manually executed Codex briefs, not runner P-series prompts. No task automatically authorizes the next; do not mark V20/V32/V43 or any Gate GREEN without their evidence.

## Scope/dependency

The recent dynamic-waterline and wake/boost changes may be present only in the owner's working tree. Preserve and build on that actual code. Do not rework fish swimming physics, the follow camera, surface collision, environmental lighting/fog, day/night, sounds, stealth/ecology, save/death or the Ocean water style. Weather, deformable water collision, full hydrodynamics and new fish art are deferred.
