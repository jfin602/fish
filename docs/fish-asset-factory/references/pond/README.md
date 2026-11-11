# Freshwater Pond visual reference pack

**Status:** five generated reference candidates for owner visual review, 2026-11-11. This pack defines an aspirational Pond look. It does not approve the Pond art direction, qualify a FAF model or Godot scene, start Pond world implementation, or change the current Ocean style contract.

## Visual goal and provenance

**Beautiful freshwater wilderness hiding something dangerous.** The [owner-supplied style target](pond-style-target.png) is the primary anchor: intentional faceted low-poly forms, a dense lily-pad ceiling, thin stems, large submerged wood and roots, muddy sediment, restrained olive/moss/brown/emerald colors, isolated warm light, and visibility that falls with distance. The images below explore different habitats within that same visual language; they are not production textures, geometry, or proof of runtime effects.

The owner supplied `/home/jfin/best-pond-faf.png` in this task. `pond-style-target.png` is a byte-for-byte copy of that 1448 × 1086 PNG. Its creator, source history, and license were not supplied. Keep it attributed as **owner-supplied visual reference**; this repository copy records the owner's requested project use and does not assert a license for independent redistribution or use as game imagery. The five numbered images were generated for Fish in this task using the built-in image generation tool, with that source image supplied as a style reference for every generation. No third-party artwork was downloaded.

## Candidate gallery

| File | Role and visual distinction |
| --- | --- |
| [01 — Overcast canopy](pond-01-overcast-canopy.png) | **Primary Pond hero target.** Dense overhead pads, a massive fallen-tree passage, dark water and selective warm pools of light. Closest in mood and material language to the owner anchor. |
| [02 — Sunlit shallows](pond-02-sunlit-shallows.png) | Brighter bank-side route with denser small aquatic plants, a branch shelter, controlled golden-green shafts and caustics. Shows how welcoming the same pond can look. |
| [03 — Deep basin](pond-03-deep-basin.png) | Sparse vegetation, open sloping mud, buried wood, boulders, stumps and weak overhead light. Foreground remains legible while distant forms vanish into silt. |
| [04 — Root labyrinth](pond-04-root-labyrinth.png) | Interlocking shoreline roots divide into several fish-sized corridors and shelters. Shows cover, branching routes and uncertainty around the next turn. |
| [05 — Waterline and shore](pond-05-waterline-shore.png) | Split-level fish-camera view: submerged lilies and roots below; eroded banks, cattails, woodland and overcast sky above. Targets a coherent waterline transition. |

All five numbered files are 1672 × 941 PNGs, approximately 16:9, without text or UI. Each is a separate image rather than a collage.

## Generation method and settings

- Built-in `image_gen` tool, one generation call per numbered scene, 2026-11-11. The tool's exact underlying model/version, seed, guidance, reference strength and sampler settings were not exposed, so an identical rerender cannot be guaranteed.
- Each call used `/home/jfin/best-pond-faf.png` through reference-image conditioning. The image's role was explicitly **style anchor**, not an edit target. The requested output was one landscape 16:9 scene of at least 1600 × 900 pixels; the tool returned 1672 × 941.
- Common prompt direction: `stylized-concept` for an official Fish Freshwater Pond 3D environment reference; preserve intentional visible low-poly faceting, matte polygonal bark/rock/mud, graphic lily foliage, restrained olive/moss/brown/emerald palette, natural scale, layered fish-camera composition, progressive underwater haze and plausible swimming space. Avoid photorealism, painterly 2D, cartoon outlines, tropical saturation, uniform green wash, overt horror, fantasy, predators, artificial geometry, repeated grids, labels, UI and watermarks.
- Scene-specific prompt direction: **01** overcast canopy, fallen-tree corridor and isolated golden pools; **02** sunlit bank, smaller branch shelter, aquatic plants and subtle caustics; **03** sparse deep basin, sloping mud, stumps and ambiguous remote silhouette; **04** branching interlocked roots and alternating light/dark passages; **05** approximately 40% above-water and 60% underwater split view with natural rippled waterline and overcast woodland shore.
- Files are stored as the tool's original PNG output without edits or upscaling. The style reference is a direct copy, not a regenerated substitute.

The [exact generation prompts](generation-prompts.md) preserve the scene instructions issued to the tool. They are a recipe for related variants, not a deterministic guarantee without the unavailable seed/model settings.

## Palette, light and composition guidance

The working Pond palette is deep emerald and blue-black for distant water, olive/moss for lilies and aquatic plants, earthy brown/ochre for wood and mud, gray-brown faceted stone, and small muted gold highlights. This is a *separate Pond target*; the accepted bright blue reef palette and Ocean reference remain authoritative for the current Ocean work. Light should be strongest near sparse surface openings, weaken across deeper or sheltered water, and reveal selected wood and sediment forms. Avoid turning all water equally green or all darkness uniformly black.

For procedural composition, place lily pads as an uneven, nearly continuous near-surface canopy with broken windows of light. Let stems carry that canopy into the water. Use large logs and root systems as landmarks and navigable cover, with distinct broad corridors, tight hiding gaps, and camera clearance; verify actual collision later in Godot. Distribute reeds and pondweed by bank/depth and keep the basin substantially sparser. Layer readable near-field obstacles, a discoverable middle distance and distant silhouettes dissolved by sediment. Vary scale, rotation, and clustering so roots, pads, rocks and light do not repeat as stamps.

Relevant future FAF families include lily pads/stems, reeds/cattails/pondweed, algae and floating muck, fallen and hollow logs, tree roots/stumps/sticks, eroded banks, faceted boulders and pebbles, silt/mud and leaf litter. The [Pond FAF asset plan](../../../planning/p4/pond-faf-assets.md) owns the proposed kit; the present images do not constitute those models.

## Visual review and implementation boundary

**Candidate review, not owner acceptance.** Together, the images share faceted geometry, matte materials, vegetation silhouettes, olive water and golden accents. The overcast hero conveys the supplied image's atmosphere. The deep basin retains a readable foreground; the root maze offers plausible corridors; the waterline preserves low-poly language on both sides. Weaknesses for owner review: 01 follows the anchor's log-passage motif closely; 02 and 04 reuse the same overhead lily canopy strongly; 05 is a single illustrative split-level frame, not evidence that a moving third-person camera can render or cross the surface this way. The apparent passages in all images still need scale and collision proof.

Future Godot work should treat dense foliage, many overlapping pads/stems, translucent or refractive water, moving caustics, volumetric shafts, particles and long visibility as **artistic targets**, not mandatory expensive techniques. Use instancing, LOD/culling, controlled materials, depth-appropriate fog and measured native performance to choose an implementation. No render-time performance, gameplay readability, camera clearance or in-engine appearance has been qualified by these images. Owner review is required before calling the Pond art direction finally accepted.
