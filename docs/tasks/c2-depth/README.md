# c2-depth — depth-structured basin and water-column correction

**State:** P1–P2 IMPLEMENTED / P3 NEXT; correction not closed. **Type:** Correction 2 follow-up. **Required unchanged root version:** `0.2.6`.

[P1–P2 implementation evidence](qualification/implementation.md) records the depth basin, camera-depth extinction, focused T2 checks and the remaining native visual limit. P3 world-depth lighting is the next runner step.

## Purpose

Build the missing sense of underwater depth on top of the owner-green c2 water/light baseline.

Normal play should stop reading as one broad bowl beneath a lit ceiling. The default P2 basin must expose **at least five natural traversable reef ledges/shelves at distinct elevations**, and the existing Forward+ atmosphere must behave like a water column:

- bright/colorful shallow water with long useful visibility;
- progressively cooler/dimmer middle and lower depths;
- deeper water with shorter horizontal visibility and far reef masses fading from detail to silhouette to water;
- caustics and shafts strongest near the surface and progressively weaker with depth;
- lower ledges visibly receiving different water-column treatment even when several depths are visible in the same frame;
- no new simulation authority, fluid solver, renderer switch or expensive pile of extra lights/fog volumes.

This stack preserves the completed [c2 water/light correction](../c2-watet-light-fx/README.md), accepted P1 movement/camera, Forward+, prepared environment assets, movement-reactive water and GFX preset system.

## Authoring baseline

The owner pushed the accepted lighting candidate at `d292aebd` on 2026-11-10. Documentation/prompt authoring may advance HEAD afterward, but the gameplay source boundary is:

- `SeabedSurface.GetHeightAt()` is still one broad analytic bowl around Y -5.6 with gentle rolling variation and a raised rim.
- Normal play is `main.tscn -> p2_habitat.tscn -> p2_reef_composition.tscn`.
- The hidden upper collision is centered around Y 9.75; c2 derives its visual water surface just above that real limit.
- `UnderwaterAtmosphere` already owns distance/height/volumetric fog, sun/ambient energy, surface, one organic caustic projection, one batched ribbon-shaft mesh, motes and screen treatment.
- `LightingSettings` / `LightingPresetStore` own bounded **base** artistic settings and saved presets.
- `seabed_blend.gdshader` already carries world position, giving c2-depth a cheap terrain-depth integration point.
- `ReefComposition` repositions prepared rock/coral anchors from `SeabedSurface.GetHeightAt()`; route markers and hard-coded checks still need deliberate reconciliation when the terrain changes.

## Prompt order

| Step | Prompt | Outcome | Tier / handoff |
| --- | --- | --- | --- |
| P1 | [Depth-structured basin](P1-depth-structured-basin.txt) | Implemented: five ordered bands, continuous collision, updated routes/checks | T2 headless pass; import blocked by sandbox sockets |
| P2 | [Water-column extinction](P2-water-column-extinction.txt) | Implemented: real-surface camera-depth extinction and preset-safe base values | T2 headless pass; native visual review remains P4 |
| P3 | [Depth-integrated lighting](P3-depth-integrated-lighting.txt) | World-depth color/light treatment, depth-faded c2 caustics/shafts, integrated performance tuning | T2, no native pause |
| P4 | [Native depth closeout](P4-native-depth-closeout.txt) | Real-window five-level traversal, shallow/mid/deep comparison and correction closeout | T3 manual closeout |

Only P4 is this correction's final closeout. Its `Browser required: yes.` metadata means an actual native Godot/owner handoff.

## Acceptance boundary

The correction is successful when a player can swim continuously through at least five natural elevation bands and **feel the water getting deeper without reading a HUD**. Comparable horizontal views must lose useful range as the player descends, and multiple shelves visible together should not all look identically lit.

Performance remains comparative and measured. Reuse c2's existing environment/material/effect paths; avoid multiple real lights, dense fog-volume stacks, many new decals or large transparent coverage. If depth presentation introduces a repeatable material regression, tune the actual cause before closeout and retain measured evidence.

This correction does not implement feeding, prey/predators, growth, persistence/death/history, first person, controller, Movement abilities, procedural world generation or a new biome.

## Execute

From a reviewed checkout:

```bash
npm run fish:phase:validate -- c2-depth
npm run fish:phase -- c2-depth
```

Writing this stack does not execute it. Runner-owned implementation prompts must not self-commit or push. P4 leaves its final closeout reviewable for human approval.
