# Underwater Visibility & Discovery

**Decision date:** 2026-11-10  
**Status:** Owner-selected experience contract; C3/P1–P3 implemented, P4 native qualification incomplete with a failed clearance check. See [C3 closeout](../tasks/c3-visibility/closeout.md).
**Applies to:** Fish's normal-play Forward+ water presentation, including P3's FAF reef and the independently profiled freshwater Pond. **S21 (2026-11-11)** adds owner-approved spatial water clarity as an unimplemented Pond requirement; C3's historical implementation/qualification status is unchanged.

## Player experience

The ocean should be explored, not exposed all at once. From an ordinary underwater gameplay viewpoint, the player must not be able to recognize the opposite playable map boundary or survey all the reef's terrain, caves, and landmarks at once. Nearby fish, reef details, plants, and navigational obstacles stay readable. Swimming forward should gradually reveal the next stretch of reef as it emerges naturally from the water. No abrupt clipping plane, sudden pop-in, artificial fog wall, or blanket screen darkening should substitute for continuous distance extinction.

Distance and depth are separate influences: objects farther from the **camera** lose contrast/detail into water; greater camera depth reduces useful visibility and shifts the water toward cooler, dimmer colors. World-space depth still affects geometry independently of camera position. Keep the bright, colorful, stylized low-poly FAF foreground and the accepted c2 surface, caustic, shaft, water-reaction, and depth effects.

## Initial visibility targets (tunable, not qualified)

| Camera-to-object distance | Desired appearance |
| --- | --- |
| 0–8 m | Clear, colorful, legible fish, terrain, coral, and obstacles |
| 8–15 m | Increasing haze; reduced color and contrast |
| 15–25 m | Faint silhouettes and geological suggestions; details mostly lost |
| 25–35 m | Terrain merges almost entirely with the surrounding water |

| Camera water-depth zone | Initial maximum useful horizontal visibility target |
| --- | --- |
| Shallow | ~30–40 m |
| Middle | ~25–30 m |
| Deep | ~18–22 m |

These are starting **art-direction targets**, not physical optical measurements, absolute clipping distances, validated thresholds, or fixed numbers independent of the final world dimensions. Tune the extent of near clarity and far extinction by native observation. The playability requirement—opposite boundaries invisible, nearby routes readable—takes priority over exact meters.

## Freshwater Pond — spatially variable clarity (S21; design, not implemented)

The Pond must not resemble a uniform green screen filter. Keep a restrained shared water-column extinction foundation, but vary **suspended-particle density in world space**, independently of depth and camera position. An observer should be able to move from a clearer sunlit bank into a silty cove or near-bottom sediment pocket with a visibly different useful range at comparable depth. Broad transitions must be smoothly blended, stable as the camera turns, seed-reproducible where generated, and free of obvious grid tiling, boundary seams, flicker, or scrolling camera-attached noise. No particular noise texture or FogVolume architecture is mandated.

Distinguish three contributions: **absorption/scattering** reduces contrast with optical path length; **local turbidity** changes the amount of suspended sediment along that path; **illumination** changes available light and apparent detail. Shadows and overhangs make a place darker, not automatically muddier. Brighter sun can reveal more detail through contrast/lighting without physically clearing particles. Near-bottom water may be siltier where substrate, shoreline and shelter plausibly support it, but murk must not be an automatic depth-only scalar or a dark overlay over all geometry.

| Pond region (provisional) | Useful near detail, not a hard sight cutoff |
| --- | --- |
| Clear, sunlit shallows | About 4–6 m |
| Ordinary Pond water | About 2–4 m; outlines may persist farther |
| Silty pocket or muddy near-bed water | About 1–2 m where safe navigation remains legible |
| Shaded trunk or shelter | Locally darker, with a recognizable nearby entrance; turbidity need not increase |
| Deeper water | Shorter useful visibility and gradual silhouette loss, with locally variable clarity |

These Pond targets are **art-direction starting values**, not physical optical measurements, hard clips, validated renderer values, or replacements for the earlier Ocean visibility table. A nearby fish should remain visibly fish-shaped and materially recognizable when in ordinary water; silhouettes become subtler through longer or murkier paths rather than disappearing behind a constant green veil. Keep P12-R1 corrected air-facing surface geometry and P12-R2 muddy-green optical transmission coherent with underwater water properties; no false clear-water view from air and no return to the owner-rejected pale/shiny surface treatment.

## Visual behavior

- **Continuous reveal:** As a fish approaches a formation, a soft silhouette gradually resolves into its shape, then color and details. Retreating reverses the effect. Avoid sharply fading an entire object at one threshold.
- **Water color:** Tropical turquoise dominates near the surface; mid/deep water trends to cooler, darker blue-green. Match extinction colors to the background and current water column so far objects do not leave obvious cutout seams.
- **Light preservation:** Existing above-water surface brightness, moving caustics, and sun shafts remain coherent. Deeper water naturally attenuates them; increased distance fog must not simply destroy the accepted lighting identity.
- **Gameplay clarity:** Preserve near-field visibility of the player fish, routes, cave entrances and obstacles. Later threats need enough warning distance for a meaningful player reaction; this contract does not introduce AI or alter authoritative perception rules.
- **Local murkiness:** S21 makes smoothly variable clarity a Pond presentation requirement, not an optional later cosmetic effect. It should be realized through bounded shared/world-space variation rather than a costly population of FogVolume nodes. Existing Ocean treatment remains independent; localized Ocean tuning may be considered separately.

## Implementation boundaries and planned GFX control

Reuse `game/scripts/UnderwaterAtmosphere.cs` and the Forward+ Environment for view-distance extinction and coordinated depth response. Treat basic distance fog separately from volumetric fog used for light scattering; `VolumetricFogLength` is a rendering extent, **not** a measured maximum useful sight distance. The C3 implementation uses an artistic 35 m default depth-fog end distance at the surface, with a 5 m clear start and depth-derived shorter end distance. These values need native calibration.

C3/P2 exposes **Visibility Distance (m)** in the paused GFX panel from 18–60 m; lower means murkier and higher means clearer. Presets save the base artistic value; camera depth changes only the effective runtime fog end. Schema-v1 custom presets migrate deterministically to schema v2 using `clamp(35 * sqrt(0.0035 / max(legacyDensity, 0.0001)), 18, 60)` meters. The old density remains stored as compatibility metadata. Natural/Soft and custom preset behavior are retained; [implementation evidence](../tasks/c3-visibility/qualification/implementation.md) records the isolated fixture checks.

The candidate low-cost rendering path is a measured test of one bounded world-anchored volumetric/noise-density approach in Pond, after softening the global baseline. Godot FogVolume/noise is an option, **not a requirement**; adding positive fog cannot remove a too-opaque baseline, so tune the foundation first. Avoid a high-cost per-pixel multi-step raymarch, numerous independent volumes, extra real lights, or extensive transparent overlays. A future low-resolution deterministic clarity field may be baked alongside world terrain if the native prototype demonstrates visual and performance value. It should be queryable without using renderer output when P5 adds sensing. Static visual variation is P4; temporary fish-created silt clouds and simulated predator response are later work.

Fog is presentation-only. It must not change `FishController`, `SwimMotor`, collisions, fish identity, input/camera control, clock, or simulation. Fog does not inherently reduce draw calls: later LOD/culling work is separate and requires its own measurements. Favor bounded scalar environment updates over extra full-screen passes, costly transparent layers, added real lights, or widespread fog volumes.

## Native acceptance and evidence

Use the existing five-depth normal-play world (or the completed P3 FAF rebuild) on the tracked Forward+ renderer:

1. At a reachable point near one side of the map, look toward the opposite playable boundary: it is not recognizable. Confirm this in more than one direction rather than relying on terrain blocking the view.
2. From the regular follow camera, approach a distant landmark and then retreat: detail emerges and disappears gradually without a sharp clipping plane.
3. From shallow, middle, and deep routes, compare horizontal visibility at similar geometry/angles; useful range decreases with depth, and transitions remain continuous during a dive/ascent.
4. Swim a cave/tunnel and an open corridor without losing near obstacles, entrance silhouettes, or player-fish readability.
5. Check bright overhead surface, caustics, shafts, UI, and GFX preset load/save/reload. Test older custom preset data for compatibility.
6. Record native same-scene performance comparisons with the effect changed, noting frame times, settings, renderer, errors, source revision, and hardware. Do not claim a speed gain just because fog hides distant objects.
7. Capture representative before/after views and owner gameplay judgment. Automated or headless checks can verify parameter bounds and isolation, but not the actual feeling of progressive discovery.

For S21's future Pond correction, compare fixed native fish/log/mud positions at unchanged light/camera settings with spatial variation on/off; compare same-depth clear/silty/shaded regions, a near-bed versus raised view, a continuous swim between regions, and air-to-water views with the same optical intent. Check near-fish silhouette, hollow-log opening, routes, far-bank concealment, plausible light contrast, Ocean return, stable world seed/bake identity, GFX preset isolation, frame-time median/p95 and errors. A static image or shader parameter check is not a native owner-play PASS. See [V29/V32](../validation.md) and [P4 plan](../planning/p4/freshwater-world.md); no Gate or version changes until actually qualified.

The C3 visibility correction has implemented P1–P3. The [P4 native matrix](../tasks/c3-visibility/qualification/native-visibility.md#v25-and-visibility-contract-matrix) records a collision-clearance failure, incomplete route/view and preset observations, and measured same-scene cost; owner fog acceptance remains open. Existing P2 V18/V19 accepted evidence and unresolved V20/G2.3 are historical facts, not changed by this contract. Reference [P3 plan](../planning/p3/reef-world-rebuild.md), [UX](../ux-and-presentation.md), and [validation V25](../validation.md).
