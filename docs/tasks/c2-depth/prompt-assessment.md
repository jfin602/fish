# c2-depth prompt assessment

## Outcome

Turn the current P2 bowl into a vertically legible reef basin and make the existing owner-green c2 presentation respond to real water depth. The result should provide a reliable visual testbed for future ecology while staying strictly inside P2 world/presentation scope.

## Preserved behavior

- Forward+ remains the tracked renderer.
- Preserve the accepted c2 water surface, procedural sky/sun cue, organic moving caustics, caustic-aligned shafts, motes, restrained screen treatment, GFX panel and saved lighting presets.
- Preserve accepted P1 fish identity, motor, tuning, input, camera authority and pause/focus lifecycle.
- Preserve one continuous seabed visual/collision surface, hidden world limits, prepared rock/coral/material identities and the separate P1 course fixture.
- Preserve movement-reactive water as read-only presentation.
- Root `package.json` remains `0.2.6` throughout the correction.

## Source findings at authoring

1. **Terrain has insufficient depth structure.** `SeabedSurface.GetHeightAt()` returns `-5.6 + rolling + rim`. It creates useful variation but still reads as one bowl; there are no five meaningful traversable depth bands.
2. **Existing assets can follow a changed floor.** `ReefComposition.TryBuild()` repositions rock/coral anchors using `seabed.GetHeightAt(x,z)` plus each authored height offset. Route markers in `p2_reef_composition.tscn` and several habitat checks are still fixed and must be reconciled.
3. **The water surface already has a truthful origin.** c2 derives `_surfaceHeight` from `Habitat/HiddenLimits/Upper`; do not introduce a second arbitrary surface Y.
4. **The atmosphere already owns the right knobs.** `UnderwaterAtmosphere` owns distance/height/volumetric fog, background/ambient/sun color/energy and all c2 effect materials. The active camera may be read for presentation-only depth; it must never become movement/simulation authority.
5. **World-space terrain depth is cheap.** `seabed_blend.gdshader` already carries `world_position`. Existing Environment height fog can affect all world geometry, including prepared third-party rock/coral, without replacing their materials.
6. **Presets are base values.** `LightingSettings` and `LightingPresetStore` persist artistic base settings. Runtime depth modulation must derive effective values from those bases and must not write camera-depth-adjusted values back to the preset.
7. **Current c2 effect cost matters.** The prior native record already measured meaningful uncapped cost. c2-depth should mainly change scalar environment/material parameters and existing shader math rather than multiply lights, decals, particles or transparent geometry.

## Main risks

### Artificial shelf geometry

Five numeric height levels can easily become a staircase. Build unequal, irregular shelf footprints with smooth/angled transitions, cliffs, channels, slopes and occasional drop-offs. The requirement is five **readable traversal bands**, not five flat rectangles.

### Broken spawn/routes/assets

Changing the analytic floor moves prepared anchors automatically but can bury/block route markers, spawn clearance, tunnel collision and camera paths. P1 must update structural checks around actual sampled floor heights rather than preserve old `-5.6` assumptions.

### Camera-depth-only fakery

A purely global camera-depth tint would make all visible shelves share one lighting state. c2-depth needs both:
- world-space depth cues (height fog and/or world-position shader attenuation), and
- camera/view-depth extinction (dynamic fog/light values as the camera descends).

### Preset drift

If `ApplyLighting()` writes an already depth-modulated value into the base fields, saving a preset at depth could permanently darken/fog it. Keep base settings and effective runtime values conceptually separate.

### Surface-light contradiction

Caustics or shafts that remain equally strong on the deepest shelf will defeat the depth model. Prefer fading the existing single decal/shaft paths through vertical projection/fog/shader attenuation rather than adding many replacement effects.

### Performance

Do not solve depth with many FogVolumes, extra shadow-casting lights, a second fullscreen raymarch or a stack of transparent shells. The desired effect should come primarily from the existing Environment, the existing c2 effect shaders and low-cost world-position math.

## Work order

1. Reshape and qualify the continuous basin first so the later rendering work has stable depth bands to target.
2. Add one shared water-column normalizer tied to the real c2 surface and a deep reference derived from the authored habitat.
3. Make camera depth modulate effective visibility/ambient/sun/fog while preserving base GFX preset values.
4. Add world-space depth cues to the seabed/height fog and attenuate c2 surface-light effects with depth.
5. Perform one integrated native shallow -> deep -> shallow qualification and comparative performance review.

## Deferrals

No ecology, feeding, predators, growth, save state, first-person camera, control redesign, Movement abilities, procedural terrain system, production biome selection, SDFGI, fluid simulation, broad graphics-menu redesign or unrelated asset replacement.

## Evidence / validation tier

P1–P3 are T2 integration work with focused structural/runtime checks. P4 is T3 because convincing depth, natural shelf composition, fog-wall avoidance, motion comfort and comparative performance require a real Forward+ game window and owner judgment.
