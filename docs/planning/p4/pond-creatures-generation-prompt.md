# Pond FAF creature generation prompt

**Use:** Standalone implementation prompt for the Pond's first non-fish creature pack. This prompt authors isolated assets and a proof scene; it does not claim P4 implementation or a phase-runner checkpoint. Preserve the current Ocean world, ongoing P3 work and root version.

## Prompt

Create three original, rigged **Pond Fish Asset Factory** creature bases for Fish: **one frog, one water snake and one snapping turtle**. They must be recognizable, anatomically credible low-poly animals with different body plans and different movement. Deliver editable Blender sources, one clean GLB per creature, reproducible construction code where useful, a provenance/technical manifest, focused validation and an isolated native Godot Forward+ proof scene. These are **ambient presentation assets only** in this slice. Do not add feeding, attacks, damage, predator AI, growth, saves, spawn ecology or player selection.

### Preflight and identity

1. Read `AGENTS.md`, `BOOT.md`, `docs/workflow.md`, `docs/planning/p4/pond-faf-assets.md`, and the focused FAF art, naming, modeling, rigging, animation, budget, export and visual-review contracts. Inspect the existing fish/plant asset recipes, manifest format, Godot proof fixtures, Git HEAD and unrelated working-tree changes before editing. Preserve the marine FAF library and all current P3 changes.
2. Use `/home/jfin/best-pond-faf.png` as the Pond visual reference: intentional facets, natural proportions, muted greens/browns, warm filtered light and strong silhouettes against olive water and dark wood. Select and record an exact representative temperate-pond species for the frog and water snake before modeling; use a recognizable snapping turtle anatomy. Use reputable anatomy/motion references, make original geometry and materials, and record source/provenance. Do not copy or commit unverified third-party models or textures.
3. Register three new stable FAF creature identities after checking the live ID registry. If `CREATURE` is not yet an accepted category, extend the registry and naming helpers deliberately rather than borrowing a fish or detail ID. Give each manifest entry an explicit `habitat_id: pond`, species/type, intended placement zones and ambient-only role. Keep file names and display names separate from identity and habitat rules.

### Model and rig the three bases

- **Frog:** build a compact body, recognizable head/eyes, distinct forelegs and long articulated hind legs with usable feet. Its rig must support a crouched shoreline pose, leg-driven hop, frog-like swim kick, surface float and underwater pose without detached feet or body collapse.
- **Water snake:** build a continuous tapered body and readable head, with enough deforming spine segments for a traveling lateral wave. Its rig must support swimming at the surface, submerged swimming and shoreline crawl. Avoid a rigid tube that only rotates as a whole; check tight curves for self-intersection and visible stretching.
- **Snapping turtle:** build a broad solid shell, distinct head/neck, four articulated limbs and tail. Its rig must support heavy bottom movement and slower, limb-driven swimming/float poses. Keep shell deformation rigid and test limb/shell clearance; do not animate the turtle like a fish or rely on a predatory snap to convey its identity.
- Model in real-world metres with consistent Blender/Godot orientation, useful origin pivots, applied export transforms, clean skin weights and intentional material regions. Keep broad low-poly facets and recognizable silhouettes at the actual follow-camera distance. Record intended and imported dimensions and measured triangle/material/bone counts. Use the FAF budgets as starting guidance, then judge actual scene cost.

### Required presentation Actions

Every exported creature must contain a **real deforming armature and nonempty named Actions**, not a rigid mesh under a dummy bone. Keep gameplay root position and heading in Godot; Actions deform the animal and do not move it through the world. Make looping actions seam cleanly, mark one-shots correctly and verify them after GLB import.

| Creature | Required loops | Required one-shot/transition poses |
| --- | --- | --- |
| Frog | `IdleShore`, `SurfaceFloat`, `Swim`, `TurnLeft`, `TurnRight` | `Hop`, `Dive` |
| Water snake | `IdleFloat`, `SurfaceSwim`, `UnderwaterSwim`, `ShoreCrawl`, `TurnLeft`, `TurnRight` | `Dive` |
| Snapping turtle | `IdleBottom`, `BottomWalk`, `SurfaceFloat`, `Swim`, `TurnLeft`, `TurnRight` | `Dive` |

The `Dive` and `Hop` Actions supply pose and limb/body motion; the scene controller, if later added, owns actual translation and waterline crossing. Tune duration and cadence for each anatomy. A short neutral `Idle` variation is optional only after the required set passes. Do not add combat, bite, grab, death or eating Actions as a substitute for missing movement.

### Export, proof and validation

1. Save first-party `.blend` masters and source-controlled generator/recipe changes under the FAF structure. Export one GLB per base containing only intended render meshes, materials, deform armature and species-owned Actions. Record stable FAF ID, species/type, `habitat_id`, source identity, dimensions, counts, Action names, loop types and technical/style status in a manifest. Do not create color/size variants until each base passes the checks below.
2. Create an **isolated** Godot Forward+ proof with a simple silt/wood bank and a visible waterline. Show each creature at appropriate shoreline, surface and underwater positions and let reviewers switch creature, Action and camera angle. Include close and actual gameplay-distance views under representative Pond lighting. The viewer may play ambient motion but must not alter normal play, the Ocean scene, shared movement, World Select or authoritative simulation.
3. Add focused checks for three unique pond-tagged IDs, valid source/export paths, nonempty skinning, correct skeleton/material import, all required Actions, real per-Action deformation, clean loop seams, no gameplay-root translation and correct one-shot import. Inspect shoulder/hip/foot, snake bend and turtle limb/shell extremes in Blender and imported Godot. Use simple authored collision envelopes only where a placement needs them; do not infer gameplay-ready collision from the display mesh.
4. Capture side, top and three-quarter views; strong movement poses; waterline and underwater views; and representative native Godot screenshots. Report actual Blender/Godot versions, source revision, measured costs, checks run, defects fixed and gaps. A headless import proves technical loading, not native visual acceptance. Mark any unobserved style or motion criterion honestly.

### Done condition

The pack is technically complete when the frog, water snake and snapping turtle each have an editable source, correctly imported GLB, genuine working rig, all listed Actions, explicit Pond identity and a manifest matching the exported bytes. Native review must show three distinct, readable animals moving believably in their appropriate shoreline, surface and underwater contexts. Any later color/size variant remains a separate, restrained follow-up after these three bases pass. Do not claim P4 Gate completion or wildlife gameplay from this isolated asset pack.
