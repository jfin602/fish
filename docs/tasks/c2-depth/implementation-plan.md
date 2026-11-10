# c2-depth implementation plan

## Entry and routing

Read `AGENTS.md`, `BOOT.md`, `docs/workflow.md`, `docs/phase-runner.md`, this task README/assessment, `docs/planning/p2/underwater-world.md`, `docs/fish-asset-factory/art-style.md`, `MODULES.md`, the owner-green c2 closeout/evidence, current P2 habitat evidence, and affected source only.

Before edits inspect branch/HEAD, dirty changes, root `0.2.6`, actual `tools/dev` composition and Fish Godot project identity. The gameplay/lighting baseline at authoring is pushed commit `d292aebd`; later documentation-only commits do not replace source inspection.

## P1 — depth-structured basin

Keep `SeabedSurface` as one continuous mesh/collision owner. Replace the one-bowl height function with a deterministic authored shape exposing at least five distinct traversable elevation bands across the current 84 × 84 m basin.

Working composition bands may be approximately:

- shallow sun shelf around Y +4 to +5;
- upper reef around Y +1 to +2;
- middle shelf around Y -2 to -3;
- lower reef around Y -5.5 to -6.5;
- deep shelf around Y -9 to -10;
- optional bounded trenches/drop-offs approaching about Y -11 where collision/limits and camera remain safe.

These are **working bands, not hard product constants**. Adjust them to the real upper surface, side-limit vertical extent, prepared geometry and camera clearance. What must survive is five visually/traversably distinct bands spanning most of the water column.

Use irregular/elliptical regions, low-frequency contour variation, smooth blends, cliffs and connecting slopes/channels so the shape does not read as concentric rings or stairs. Keep the rim/hidden limits natural-looking and prevent underside/edge escape.

`ReefComposition` already follows `GetHeightAt()`; inspect every prepared rock/coral anchor after the reshape. Reposition X/Z anchors or metadata offsets only when needed to preserve tunnel/shelter/landmark readability and all prepared source identities. Update route markers to represent the new shallow/upper/middle/lower/deep traversal. Choose a safe spawn with enough fish/camera clearance.

Extend the focused habitat check to sample/identify at least five ordered elevation bands, prove one continuous seabed mesh/collision with no sampled holes, check spawn/camera clearance and adjacent-band transitions, retain tunnel/shelter collision and every prepared asset, replace old hard-coded `-5.x` collision assumptions with actual `GetHeightAt()` expectations, and retain the P1 course fixture plus unchanged fish/tuning identity.

Do not build procedural world generation. This remains one authored analytic basin.

## P2 — one water-column extinction model

Use the **actual c2 surface height** as depth zero. Add one small, centrally owned depth calculation in the existing atmosphere boundary or a narrowly scoped helper if that makes testing clearer:

`depth01 = clamp((surface_y - camera_y) / (surface_y - deep_reference_y), 0, 1)`

Use an eased curve. Derive `deep_reference_y` from the authored habitat contract/source, not from an unrelated magic number duplicated across systems.

Read the active camera for presentation only. Treat `LightingSettings` / saved presets as **base** values. Runtime effective parameters derive from base settings plus depth: distance/volumetric fog strengthen, useful range shortens, sun/ambient energy fall, and fog/background/ambient color shifts cooler while silhouettes remain readable. Never persist camera-depth-modulated values.

Re-anchor height fog to the real water column where practical so lower third-party geometry receives depth treatment independently of camera depth. Add focused shallow/middle/deep checks proving monotonic effective behavior, stable preset bases and unchanged fish/camera/tuning authority.

## P3 — world-depth lighting and c2 effect integration

Make depth differences visible **within one frame**:

1. Extend `seabed_blend.gdshader` with bounded surface/deep uniforms and restrained world-depth color/brightness attenuation.
2. Use Environment height fog for world-space depth treatment across imported rocks/coral without replacing all materials.
3. Extend the batched shaft shader with world-position depth attenuation.
4. Keep the **single** c2 caustic projection if practical; tune vertical projection/fade plus water-column fog so deep shelves do not receive surface-strength caustics.
5. Preserve overhead surface, sky, caustic drift/alignment, motes, screen treatment and GFX presets.
6. Keep the bright surface legible from deep water.
7. No new shadow-casting lights, fog-volume stack, full-screen raymarch or per-frame resource rebuilds.
8. Extend the existing A/B probe for representative shallow/deep views, recording available frame time/FPS and draw/primitives counters.

Run the smallest meaningful build/habitat/atmosphere/movement checks plus `git diff --check`; root stays `0.2.6`.

## P4 — native depth closeout

In a real Forward+ normal-play window, traverse shallow -> upper -> middle -> lower -> deep -> shallow plus an alternate transition and existing tunnel/shelter route. At shallow/middle/deep positions, look horizontally across comparable distances and look upward from shallow/deep. Include one view showing several depth bands at once.

Judge natural five-level composition, traversal/camera clearance, world-depth lighting differences, progressively shorter visibility, detail -> silhouette -> water fade, absence of hard fog bands, weaker deep caustics/shafts, bright-surface coherence, unchanged UI/GFX/movement, and same-revision shallow/deep performance.

Create `qualification/implementation.md`, `qualification/native-depth.md` and one `closeout.md` only from actual evidence. Reconcile owning docs once. Do not mark P2 GREEN unless all G2.1–G2.3 evidence supports it.

## Hard boundaries

Preserve Forward+, `FishController`, `SwimMotor`, `FollowCamera`, `swim_tuning.tres`, P1 fixture, c2 GFX presets and owner-green water/light identity. No feeding/predators/growth, saves, first person, control rewrite, procedural world framework, new renderer, SDFGI or authoritative fluid simulation.
