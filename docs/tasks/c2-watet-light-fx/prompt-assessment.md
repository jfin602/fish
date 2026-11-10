# c2-watet-light-fx prompt assessment — 2026-11-10

## Request and observed gap

The owner selected the generated low-poly reef target as the desired in-game water/light finish and requested a performance-conscious correction stack. The target has four defining lighting cues: a readable rippling water surface overhead, bright sunlight/sky visible through that surface, soft downward light shafts, and irregular moving caustics that break across the seabed instead of forming an obvious grid.

Current Fish source already has the right renderer and several supporting pieces, but not the full effect. `UnderwaterAtmosphere.cs` owns Forward+ distance/height/volumetric fog, one directional sun, 128 GPU motes, one large decal caustic projection and a below-UI screen pass. `MakeCausticTexture()` creates its pattern from two high-frequency periodic sine fields, which naturally produces a lattice-like visual. The screen shader applies periodic whole-view UV displacement. There is no actual overhead water mesh/sky-transmission presentation and no localized shaft geometry.

The normal P2 habitat has an upper collision around Y 9.75 and an 84 × 84 m basin. The correction should make the visual surface agree with that existing world boundary without making water physics authoritative or moving the accepted limit.

## Outcome

1. **Surface read:** looking upward from normal swimming depth clearly shows a moving water ceiling, bright sun/sky above it and believable underside shimmer. It is a visual plane only; no fluid simulation or fish force.
2. **Light-ray read:** a small art-directed set of broad, soft shafts reinforces sun direction through the existing volumetric atmosphere without adding multiple real/shadowed lights.
3. **Caustic read:** projected light on sand/rock/coral uses irregular cellular/broken shapes, unequal spacing, low repetition and slow drift. It should not look like a tiled square, checker, regular diamonds or a sinusoidal net.
4. **Camera stability:** the world stays stable while swimming. Whole-screen warp becomes subtle enough to be nearly imperceptible or is removed if the surface/atmosphere now carries the underwater cue.
5. **Performance discipline:** preserve Forward+, one real directional sun, bounded volumetric fog, sparse motes, a modest water mesh/material, at most a small number of transparent shaft surfaces, and no expensive fluid/SDFGI/ray-march solution. Measure effects-on/off native cost on representative routes.

## Preserved behavior / exclusions

Preserve `FishController`, `SwimMotor`, `SwimTuning`, `FollowCamera`, input presets, fish visual motion, habitat geometry/collision, prepared asset identities, UI layering and pause/focus behavior. The atmosphere remains presentation-only and its pause/toggle path must not alter fish position, velocity, camera identity, tuning or collision.

Do not switch renderer, add water-body physics, change the upper collision merely to accommodate art, introduce SDFGI, SSR-dependent water, multiple shadow-casting spotlights, dense CPU particles, a high-resolution simulation texture, or P3 gameplay. Do not restyle the reef assets in this correction.

## Decomposition and evidence

| Prompt | Responsibility | Required evidence |
| --- | --- | --- |
| P1 | Overhead water surface and above-water sky/sun read | T2 resource/scene checks, visual-surface bounds, toggle/pause isolation, one-render-pass/draw-cost accounting |
| P2 | Organic caustic generation/projection | T2 deterministic/bounded resource checks, max projection count, no movement authority; native pattern quality deferred to P4 |
| P3 | Cheap light shafts, screen-treatment cleanup and integrated performance instrumentation/tuning | T2 source/runtime checks, on/off counter collection, no extra real light authority or UI distortion |
| P4 | Native look/performance closeout | T3 real game window, near/mid/surface/seabed/moving views, effects A/B, owner PASS/FAIL/NOT OBSERVED |

## Main risks and mitigations

- **Transparent water/beam overdraw:** keep the surface to one bounded mesh/material; keep shaft count small and coverage sparse; avoid full-screen multi-pass volumetric ray marching. Measure actual draw/frame cost.
- **Surface aliasing or ugly clipping:** derive its height/extent from the habitat and camera play volume, keep it outside normal fish/camera intersection, use modest displacement and a conservative subdivision count.
- **Surface looks like glass ceiling:** combine gentle geometry motion, Fresnel/transmission and bright sky/sun cue; avoid mirror-like roughness or heavy distortion.
- **Caustics still repeat:** replace the current periodic formula with deterministic irregular/cellular structure and, if needed, a second low-energy layer with different scale/rotation/drift. Keep projection count tightly bounded.
- **Rays look like solid cones/cards:** use soft edge/end fades, low alpha, noise breakup and sun-aligned placement. They are presentation meshes, not physical spotlights.
- **Performance falls off at upper views:** use same-route effects-on/off measurements. Investigate a repeatable roughly 10%+ median regression, excessive transparent overdraw or materially increased draw calls before native approval.
- **UI or swimming gets distorted:** water screen remains below UI and no effect may write movement/camera state. Prefer putting motion in the surface/caustics/rays rather than the whole screen.
- **Qualification inflation:** compilation and screenshots prove implementation only. P4 owner/native review determines whether the target effect actually reads correctly.

## Authoring state

Root `package.json` is `0.2.6`; this is a Correction 2 stack and every prompt must leave that version unchanged. Source was inspected from the connected `jfin602/fish` repository. The executing agent must still inspect current branch/HEAD, dirty state, `tools/dev` composition and any divergence before changing source.
