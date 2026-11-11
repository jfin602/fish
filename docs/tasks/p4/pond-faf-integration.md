# Pond FAF — P4 integration contract

**Owner scope supplied 2026-11-11.** This is an **asset integration inventory**, not a request to author new assets during the P4 runner. The owner is already building the pond families independently in FAF. The current reported state is: five fish species modeled, foliage package made, creature package nearly complete. **Those progress statements are owner reports, not verified GLB/rig/import proof**; inspect the local checkout when each P4 task executes. Main-branch GitHub search at authoring did not expose the finished freshwater packs. Do not claim P4 or FAF asset acceptance from this plan.

## Authority and separation of work

- The existing [P4 world plan](../../planning/p4/freshwater-world.md) owns world design, World Select and surface camera. This inventory supersedes its earlier **illustrative** freshwater creature shortlist for P4 asset integration.
- Independent FAF production owns modeling, rigs, Blender masters, exports, variants, previews and asset technical reviews. **P4 P6/P8 must not create, regenerate, re-export wholesale, relicense, rename, or overwrite FAF originals**, even if an asset is missing. If an import needs a technical adapter, implement a non-destructive Godot-side wrapper or report the exact source issue to FAF.
- P4 owns intake validation, registration, selected Godot imports, scene/ambient placement, procedural habitat suitability, actual collision/camera clearance, compatibility and native game-view review. A verified asset source is not evidence of in-world placement.
- Inspect the real current local FAF manifests, source paths and revision before choosing asset IDs; never guess paths or allocate duplicate IDs. Preserve Ocean fish 001–005 and Ocean environment assets. Shared generic assets need explicit neutral habitat tags; otherwise freshwater is Pond-only.
- If files are not yet present, preserve earlier completed P4 steps, report **BLOCKED/MISSING** per exact item, and do not advance the affected P6 or P8 checkpoint. No unapproved substitute or fictional file. **Do not rerun the independent FAF generator from P4.**

## Required intake matrix

| Family | Owner-selected scope | P4 integration responsibility |
| --- | --- | --- |
| Freshwater fish | **Bluegill, bass, catfish, carp, pike**: five identities, each with **Egg, Larva, Juvenile, Subadult, Adult** = **25 stage exports** | Verify unique species ID and stage identity; actual .blend/.glb presence, skeleton, skin weights, named Actions, anatomical silhouettes, gill/breathing classification, import, previews/animation and Pond-only tags. The Egg requires a minimal egg-specific rig/motion, **not** a fake swim cycle. P4 demo can select one tested presentation stage per species, but **all 25 exports must be accounted for**; connecting stage transitions/growth is P5. |
| Other creatures | **Frog, water snake, snapping turtle** base models, owner-prepared variations where available | Validate actual model/rig/animation, scale and habitat; use suitable passive/ambient movement and placement at waterline/shore/underwater as appropriate. No predation, attacks or health. |
| Shallow-water insects | **Submerged larva** and **water/surface bug** families with **two variants each** | Import/validate all four variants and inexpensive near-shore/waterline ambient motion. No feeding logic or expensive full simulation. |
| Wood and cover | **Three fallen-log shapes, three hollow-trunk openings, two trunk/stump forms, four stick/branch/root clusters** | Import variants, project them onto terrain, place by shoreline/depth/slope; protect routes. Hero hollow trunk must be a true swim-through with simplified collider matching inner passage and fish/camera clearance. |
| Foliage | **Three lily-pad silhouettes plus stems, two tall-grass clumps, two cattail clumps, two reed clumps, two slime/algae forms, two floating muck/duckweed patches** | Keep lily pad canopy/floating muck at water surface; root grasses/reeds/cattails/pondweed in suitable sediment; animate only when appropriate, use lightweight instancing/culling and protect navigation and visible waterline. |
| Ground/detail | **Silt/mud, pebbles, leaf litter, algae-coated stone, eroded bank**, with shared pond terrain palette/materials and small reusable dressing family | Preserve real generated bank/terrain ownership; register appropriate material and detail variants, surface-project, avoid floating or repeated-grid scatter, and keep structural collision honest. |

Modeling species refinements (e.g. largemouth bass, channel catfish, common carp and northern pike) are **working FAF manifest choices**; do not introduce unapproved alternate species, such as minnow or perch, in their place. Eggs and all larval/juvenile stages remain separate representations, not 25 independently playable species.

## Two distinct P4 integration passes

### P6 — environment intake / world placement

Read existing FAF wood/foliage/ground packs and their manifests; record every specified subject/variant as FOUND, PARTIAL, MISSING or TECHNICALLY INVALID, with actual paths and evidence. Import existing GLBs through stable resource IDs; build deterministic pond-world placement and per-habitat selection. Validate model/collision support and genuine hollow-trunk fish+camera swim-through. Never open Blender to generate a replacement. P7 native Pond review covers composition, readability and source-bound geometry; missing essentials block the relevant visual acceptance.

### P8 — creature intake / character and ambient integration

Read current freshwater fish/creature/insect packs. Verify all five species × five stages, including the minimal egg rig exception, and relevant swim/glide/breathing Actions. Register five Pond-only fish identities into habitat-filtered Character Select and attach compatible existing GLBs/animations without changing shared P4 demo physics. Import frog/snake/turtle + insect variants for anatomically appropriate ambient life. Lifecycle progression, combat, predation, damage, survival saves and stages changing through gameplay are deferred; technical stage exports must still be validated. Report exact blockers rather than generating any missing assets.

## Evidence and acceptance

Use FAF technical manifest/source proof plus Godot import and native Forward+ verification. Record real file paths/IDs, source revision, per-item counts, size/orientation, rig/skin/action checks, triangle/material budget, art review status, and explicit habitat compatibility. Verify shallow bank, middle hollow-log route and sparse deep views with near/mid/far reading. Perform sampled camera/collision and waterline checks, representative render/memory counters, and Ocean regression. Store real evidence under P4 qualification; do not relabel isolated FAF proofs as G4.3 PASS. Refer to [P4 validation V30/V31](../../validation.md) and the final native closeout for actual Gate disposition.

**Status at authoring:** owner reports the fish five-species models and foliage complete, other creatures almost complete; **25 stage exports, insects, wood, ground, technical rigs, Godot imports and actual scene integration remain unverified in the published repository**. Re-assess at execution against the user's working checkout, not this static note.
