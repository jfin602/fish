# Fish visual art-style contract — shared language and Ocean

**Status:** Owner-approved visual direction, 2026-11-10. **Authority:** This document owns the *look* of Fish and is the visual acceptance contract for newly authored or substantially restyled art. [Art and assets](../art-and-assets.md) owns sourcing, licenses, manifests, and imports; [P2 world](../planning/p2/underwater-world.md) owns the current habitat's technical scope; [UX](../ux-and-presentation.md) owns readability. A style decision is not evidence that an asset or Roadmap Gate has passed.

## One-line identity

**Stylized low-poly aquatic naturalism** is the shared Fish language: legible faceted geometry, recognizable animals, cohesive palettes, natural proportions and rich layered but navigable spaces. The existing **bright stylized low-poly reef naturalism** palette and references remain the **Ocean habitat's** owner-approved contract: recognizable marine life, lush colorful reefs and blue-water sunlight. The freshwater Pond planned for P4 uses the same geometry and quality principles with an intentionally different water/material palette.

This **supersedes the earlier realistic/photoreal-looking rendering direction** in the 2026-11-09 ocean-art shortlist while preserving credible ocean species, natural anatomy/behavior, and marine ecology. Quaternius was accepted as P1 prototype art; low-poly itself is now the intended production language. Future *custom* assets should follow this contract. The exact playable fish species, production biome, and larger-world strategy remain open. The current P2 assets can remain as transitional content until purpose-built replacements are ready.

## Owner visual references (2026-11-10)

The owner-supplied [reef style target](references/reef-style-target.png) in the repository is the **primary visual reference**. It combines faceted blue-gray rocks and a broad swim-through arch, colorful clustered coral and plants, recognizable fish, warm sand, bright caustics and sunlit blue water. Use it to judge whether a new asset belongs in the same world; [the reference record](references/README.md) preserves its source and use boundary.

Two earlier owner-supplied screenshots from the Fish art-planning conversation remain supplemental reference descriptions. Their files are not separately registered in this checkout, so the descriptions below preserve their useful direction without asserting a match to the repository target.

- **Reference A — colorful reef and fish (style anchor):** A compact, vivid reef crowded with angular turquoise/gray/pale-sand rocks and pink, red, purple, orange and blue corals, tall green sea plants, and small bright striped tropical fish at several depths. Readable triangular/polygon faces, intentionally simplified forms, clean colors, many varied silhouettes, and no photoreal surface detail.
- **Reference B — underwater rock arch (environment/finish anchor):** Wide, navigable reef landscape with an imposing blue-gray faceted stone arch and surrounding cliffs, sandy floor, small colorful coral clusters, scattered green sea grass, visible blue water above, sunlight/caustic highlights, and atmospheric depth. Strong foreground/midground/background composition, natural swimming routes, and cinematic water lighting.

**Use the repository target as the primary world comparison.** A adds creature, geometry, coral shape and color detail; B adds habitat scale, spatial composition, density, lighting and visibility. A clean Blender asset render alone does not prove the in-game result.

### Future Freshwater Pond reference candidates

The owner-supplied [Pond style target and five generated views](references/pond/README.md) establish a **candidate** direction for the later Freshwater Pond: visible faceted low-poly geometry, a lily-pad canopy and stems, massive submerged wood and roots, muddy sediment, olive/emerald water, isolated golden light and short, depth-dependent visibility. Its theme is beautiful freshwater wilderness with natural survival tension. Owner visual acceptance remains open. This Pond pack is separate from the accepted reef/Ocean target above; it does not change the existing Ocean palette, current scene, FAF model acceptance rules or Roadmap Gates.

## Non-negotiable visual principles

1. **Low-poly shapes are visible and intentional.** Favor broad planar facets, readable topology and strong silhouettes. Avoid dense sculpted surface noise, photogrammetry, realistic scans left visually unadapted, and generic smooth primitives as finished hero art. Selective smooth shading is allowed where fish deformation, eyes, or motion need it.
2. **Stylized, not anatomically arbitrary.** Fish remain identifiable as plausible marine species. Fins, tails, body proportions and swim motion should read naturally even with simplified surfaces. Habitat shapes can be exaggerated for attractive composition and useful cover without looking like artificial test geometry.
3. **Colorful but controlled.** A restrained field of sandy neutrals, cool blue/gray stone, and blue/teal water supports vivid coral, vegetation and fish accents. Saturation is concentrated on meaningful living details; do not make every surface equally bright. A coherent palette matters more than exact hex values.
4. **Richness through reusable clustering.** Make a small kit of distinctive coral, plant, rock and fish assets that supports rotations, scales, curated color variants and multi-object clusters. Vary silhouette, height, spacing and grouping. Avoid obvious repeating stamp patterns or one huge monolithic imported reef.
5. **Natural composition and useful 3D space.** Every reef area needs negative/open water, visual landmarks, broad navigation, tighter shelter, depth/elevation changes and believable terrain transitions. Favor a layered foreground, midground and distance. Avoid box-room walls, floating props, featureless flat floor, or decorative clutter that masks traversable routes.
6. **Water and light provide the finish.** Bright top-down sunlight, restrained moving caustics, water-blue distance fade, soft volumetric feeling and subtle motes make the scene immediately read as submerged. The finish is depth-structured: shallow water remains bright and colorful; middle depths become cooler and hazier; deep water is darker blue/green with substantially shorter visibility. Warm/red-orange contribution falls before blue/green, and caustic/shaft presence weakens as depth increases. Preserve legible near-field fish and obstacles while distant reef masses dissolve through detail -> silhouette -> water. Water effects must not obscure the UI, become constant screen wobble, or modify movement physics.
7. **Gameplay readability is a style constraint.** A player fish must separate from its background at the actual follow-camera distance. Prey, predator, edible food, shelter and collision passage must read from silhouette, size, motion, placement and contrast—not color alone. Keep third-person camera clearance through arches and overhangs.

## Habitat-specific visual extension — Freshwater Pond (P4 planned)

**Owner-selected 2026-11-11; design, not asset acceptance.** Preserve every existing ocean reference, finished marine fish, coral palette and Forward+ water treatment. Do not restyle Ocean into a pond. [P4 plan](../planning/p4/freshwater-world.md) owns the new world/content boundaries.

- **Pond visual identity:** muddy green/olive water and golden-green bank shallows; silty brown/tan sediment, eroded banks, algae-covered rocks, fallen wood, submerged tangled roots, lilies, reeds/cattails, pondweed and duckweed. Deeper areas become darker, less vegetated and less visually transparent.
- **Shared shape language:** intentional low-poly facets and clean readable animal silhouettes, natural anatomy/rigging, reusable clustered variants, economical materials, gameplay-readable foreground/camera and cohesive depth lighting.
- **Pond palette:** muted olive/emerald water, algae green, dark peat/mud browns, ochre-sand shallows and restrained sunlight. Avoid painting the entire world one flat green or obscuring near-field fish/solid collisions.
- **Freshwater species:** each fish/insect/frog/snake/turtle should retain anatomy-specific silhouettes and rigging; the same fish-breathing standards apply where anatomically appropriate. Do not recolor a marine reef model and call it a freshwater species.
- **Visual boundaries:** the reef target image, vivid coral accents, blue water and ocean-specific tables below remain authoritative for Ocean, not literal requirements to add reef coral or blue water to Pond. Review a Pond candidate against pond-specific gameplay frames and the shared standards.

## Shape and material language

FAF uses five related geometry dialects: **fish** are organic low-poly with smoother deforming joints; **coral** is chunky and faceted; **plants** are graphic blades and fronds; **rocks** show strong broad facets; **terrain** uses broad polygonal contours. These dialects share silhouette clarity and simple materials without forcing every object to use identical topology or shading.

| Family | Geometry / silhouette | Surface and color |
| --- | --- | --- |
| Fish | Clean tapered bodies, clearly separated fin/tail shapes, recognizable profiles; sufficient deforming segments for smooth swim/turns | Simple color regions, stripes/spots as needed for species recognition; restrained highlights; eyes readable from gameplay camera |
| Rock / cliffs | Chunky asymmetrical wedges, broad flat planes, purposeful angular edges; interlocking modular pieces | Cool blue-gray/slate/stone neutrals with mild facet variation; hero rocks may have warmer sunlit faces |
| Sand / seabed | Gently rolling continuous playable shapes with visible slope and contour changes | Warm pale sand, low-frequency color/roughness variation; no busy repeating photographic texture |
| Coral | Distinct branching, fan, tube, rounded/mound and clustered silhouettes; deliberate unequal branch/segment thickness | Saturated rose, red, orange, violet, turquoise/blue accents; species/cluster color groups remain coherent |
| Seagrass / seaweed | Slim readable blades/fronds, uneven heights and grouped patches | Fresh and deep greens with a few teal tones; modest current motion, no chaotic jitter |
| Food / particles | Compact, instantly recognizable forms with restrained motion | Visible but subordinate to fish and habitat; avoid confetti-like overload |

Materials should be efficient and mostly matte or softly satin. Prefer flat/vertex color or compact reusable palettes and subtle roughness variation; do not rely on high-resolution photographic texture packs, heavy normal-map noise or metallic/glossy effects to achieve the look. Use emissive/glow only where diegetically justified, not as the default coral treatment. [Palette families](palette.md) provide the initial shared color vocabulary.

**Illustrative palette, not a literal color-lock:** ocean dark blue `#16456C`, brighter water blue `#248FC0`, turquoise `#38B8C2`, warm sand `#D8D4AD`, stone blue-gray `#7798A9`, coral pink `#E85D86`, red-orange `#DB6354`, orange `#F29449`, violet `#9169C9`, seagrass green `#46A967`. Judge hues under the actual Forward+ underwater treatment, not just in Blender's neutral viewport.

## Animation and environmental response

- Swimming includes believable tail-driven movement, readable fin motion, natural body curvature during turns, and a smooth reduction of effort during gliding. Rigs and deformation matter more than high polygon count.
- Scale/animation variants must keep the same fish identity when the player grows; do not make growth appear to switch to a different species or unrelated mesh.
- Seagrass and select soft coral should sway gently and coherently, with independent phase variation. Rigid rocks and hard coral need not sway.
- Fish-triggered bubbles/wake, ambient motes and caustics remain *presentation*. They follow the actual motor/world state without exerting force or changing the simulation.

## Asset construction / Godot handoff

- Keep editable first-party `.blend` masters; export clean `.glb` assets with verified scale/orientation, intentional pivots, named meshes/materials and documented variations. Follow the [Blender–Godot contract](blender-godot-contract.md) and test the actual Godot scene in Forward+.
- Fish exports must retain appropriate skeleton, skin weights and meaningful named animation clips. Never claim rigged/animated based solely on visible mesh appearance or a Blender armature.
- Separate visual geometry from simpler collision volumes. Detailed coral and vegetation usually should not generate expensive per-triangle collision; navigable rock arches/shelters require accurate *playable* collision and camera clearance.
- Prefer scene reuse, instancing, small material sets and measured LOD/mesh simplification when many instances are visible. Model complexity should follow actual rendered size and density; the [FAF starting budgets](budgets.md) are tunable guides, not measured production ceilings.
- Do not overwrite or relicense downloaded source assets. Keep the [asset provenance/redistribution rules](../art-and-assets.md#license-and-provenance-handling). Third-party placeholder models are allowed only as explicitly labeled transitional content.
- Keep atmosphere tunable centrally in the existing Forward+ pipeline. New art must not change motor authority, camera/controls or physics to manufacture a visual result.

## Native visual acceptance checklist

A newly authored asset or scene is **style-ready** only after inspecting an in-engine view, not solely a Blender screenshot:

- [ ] **Silhouette:** Recognizable from the expected third-person camera distance; faceting looks deliberate, not jagged or under-modeled.
- [ ] **Palette/material:** Fits the stone/sand/water base and vibrant living accents under current water lighting; no clashing realistic texture treatment.
- [ ] **Movement:** Animated elements deform and loop naturally, with correct rig/clip playback and no visible popping or scale/orientation errors.
- [ ] **Integration:** Fits adjacent assets and varied instances; scale/pivots/materials/occlusion are coherent, with no obvious repeats.
- [ ] **Readability:** Player and threats remain visible against coral and haze; paths, shelter and collision feel spatially credible.
- [ ] **World finish:** Inspect at least a near, mid, and distant view, and a swim-through with caustics/atmosphere; the scene belongs beside the [repository target](references/reef-style-target.png).
- [ ] **Technical:** No obvious runtime stalls, excessive draw calls or pathological collisions in representative density; record actual observed conditions rather than asserting a production performance target.

A style pass is **not** P2 qualification or full game QA. Record the asset ID, Blender source/export, variations, rig/animation names if applicable, native snapshots/observations, issue/exception, and tested Godot revision.

## Reuse in future Fish asset prompts

Put this line in the prerequisites of Blender/MCP/Codex art prompts:

> Read `docs/fish-asset-factory/art-style.md` (shared rules and selected habitat's visual contract); for Ocean inspect `docs/fish-asset-factory/references/reef-style-target.png`, and for Pond use its own freshwater references; read `docs/art-and-assets.md` for provenance/import policy. Produce testable Blender masters, glTF exports and observed Godot Forward+ integration. Call out any exception; do not silently replace the style.

**Change policy:** New styles or major departures require explicit owner approval and a revised dated contract. Small species-appropriate material/shape variations that still pass this contract are allowed without a new design decision. Use the [FAF visual review](visual-review.md) for individual assets.
