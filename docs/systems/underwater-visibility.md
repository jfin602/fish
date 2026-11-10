# Underwater Visibility & Discovery

**Decision date:** 2026-11-10  
**Status:** Owner-selected experience contract; implementation and native qualification pending.  
**Applies to:** Fish's normal-play Forward+ water presentation, including P3's FAF reef.

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

## Visual behavior

- **Continuous reveal:** As a fish approaches a formation, a soft silhouette gradually resolves into its shape, then color and details. Retreating reverses the effect. Avoid sharply fading an entire object at one threshold.
- **Water color:** Tropical turquoise dominates near the surface; mid/deep water trends to cooler, darker blue-green. Match extinction colors to the background and current water column so far objects do not leave obvious cutout seams.
- **Light preservation:** Existing above-water surface brightness, moving caustics, and sun shafts remain coherent. Deeper water naturally attenuates them; increased distance fog must not simply destroy the accepted lighting identity.
- **Gameplay clarity:** Preserve near-field visibility of the player fish, routes, cave entrances and obstacles. Later threats need enough warning distance for a meaningful player reaction; this contract does not introduce AI or alter authoritative perception rules.
- **Local murkiness:** Optional cave, tunnel, or deep pocket fog is an *additional, later tuning tool* only if needed. Start with coordinated shared fog; avoid a costly large population of localized FogVolume nodes.

## Implementation boundaries and planned GFX control

Reuse `game/scripts/UnderwaterAtmosphere.cs` and the Forward+ Environment for view-distance extinction and coordinated depth response. Treat basic distance fog separately from volumetric fog used for light scattering; `VolumetricFogLength` is a rendering extent, **not** a measured maximum useful sight distance. The current base `DistanceFogDensity = 0.0035`, runtime `FogDensity` clamp `0.02`, and existing depth behavior are implementation facts to revisit and calibrate, not approved new defaults.

A future correction should expose a meaningful **Visibility Distance (m)** control through the existing paused GFX panel, with intuitive near/far tuning and depth-dependent effective values. Base artistic values are what presets save; derived depth-dependent effective values are never written back into stored presets. Preserve or explicitly migrate existing `user://lighting-presets.json` files and built-in Natural/Soft preset behavior. Keep setting ranges validated and avoid silently discarding an older custom preset.

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

The P3 visibility refinement is pending implementation and native judgment. Existing P2 V18/V19 accepted evidence and unresolved V20/G2.3 are historical facts, not changed by this contract. Reference [P3 plan](../planning/p3/reef-world-rebuild.md), [UX](../ux-and-presentation.md), and [validation V25](../validation.md).
