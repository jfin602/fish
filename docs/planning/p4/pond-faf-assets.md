# Pond FAF — freshwater base asset plan

**Status:** proposed asset plan, 2026-11-11. No asset or P4 Gate is claimed complete. The owner's 2026-11-11 roster below is the requested Pond scope; the earlier P4 design's shorter fish list was illustrative. The owner has selected all five fish and all their life stages as the first asset milestone. Isolated FAF asset authoring can proceed without changing the Ocean scene or its version; Pond world integration follows the current P3 Ocean world review and correction work.

## Target scene

Use the owner-supplied `best-pond-faf.png` as the visual reference for this plan. It shows a submerged, faceted temperate pond: lily pads overhead, layered reeds and grasses, warm shafts through olive-green water, a broad fallen hollow trunk as the main shelter and route, dark silt, roots, sticks, leaf litter, and a short, hazy view into deeper water. The image is a composition reference, not a sheet of production models or permission to redistribute its source. Record its provenance before committing a copy to the public repository.

The finished kit should make a detailed scene through **reusable families and varied placement**, with recognizable near, middle and distant silhouettes. Preserve open swim lanes and camera clearance around dense cover. Extend the shared FAF low-poly construction, Blender/GLB handoff and native Forward+ review rules with a pond palette; keep the reef palette and Ocean assets intact.

## Asset inventory

The counts below are the initial **distinct base models/variants**, not a target for simultaneous on-screen instances. Assign new stable FAF IDs after checking the live registry. Add `WOOD`, `CREATURE` and `INSECT` categories to the ID convention if the production manifest needs them; do not reuse marine species IDs or encode habitat in a display name.

| Pack | Required subjects | Initial reusable kit | Primary checks |
| --- | --- | --- | --- |
| Fish | Bluegill, bass, catfish, carp, pike | Five species identities × Egg, Larva, Juvenile, Subadult and Adult = 25 rigged stage exports; working anatomy choices: largemouth bass, channel catfish, common carp and northern pike | Distinct profiles and markings at follow-camera distance; species-appropriate fins/tails/barbels, stage-specific rigs and Actions, breathing classification for swimming stages, pond-only manifest tags |
| Other creatures | Frogs, water snakes, snapping turtles | One frog, one snake and one snapping turtle base, with restrained color/size variants only after the base passes | Anatomical silhouette and movement appropriate to each body; surface/shore/underwater placement; ambient presentation only in the first slice |
| Shallow-water insects | Bug larvae and water bugs | At least one submerged larva and one surface or swimming water-bug family, each with two readable variants | Small-scale silhouette, simple movement loop, density and visibility in shallow water; no feeding mechanics implied |
| Wood and cover | Fallen logs, sticks, tree trunks, hollow trunks | Three fallen-log shapes, three hollow-trunk openings, two trunk/stump forms, four stick/branch/root clusters | Hero hollow log permits a real swim-through; simpler collision matches openings and outer bounds; varied rotations/scales avoid obvious repetition |
| Foliage | Lily pads, tall grasses, cattails, reeds, slime, floating muck/algae | Three lily-pad silhouettes plus stems, two tall-grass clumps, two cattail clumps, two reed clumps, two submerged slime/algae shapes and two floating-muck/duckweed patches | Surface and submerged placement, gentle sway where appropriate, restrained transparency, readable waterline and clear routes |
| Ground dressing | Silt/mud, pebbles, leaf litter, algae-coated stone, eroded bank | Shared terrain palette/materials and a small bank/stone/leaf detail family | Ground contact, no floating objects, broad color variation without noisy photographic texture, inexpensive dense scattering |

The five fish are separate identities. Fish shape generators may share construction code, but bluegill should not read as a recolored bass, and catfish, carp and pike need their own recognizable body structures. Working species names are modeling choices to settle in the asset manifest, not new ecology rules.

## Build order and review points

1. **Reference and technical contract.** Register the Pond reference/provenance for project use, create a freshwater palette and scale sheet, inspect current FAF IDs and fish rig rules, then define shared origin, forward axis, material, animation and habitat metadata. Make a small neutral-light and pond-light comparison fixture.
2. **Five fish and all life stages — first asset milestone.** Create one species at a time from silhouette sheet through editable Blender master, stage-specific geometry, rig/deformation, named Actions, GLBs, manifest and native Godot proof. Start with bluegill as the scale/palette control, then bass, catfish, carp and pike. For each species make Egg, Larva, Juvenile, Subadult and Adult. The owner's requirement for every stage to be rigged makes the Egg a deliberate exception to the prior mesh-only egg pattern: use a minimal egg-specific rig and egg-appropriate motion, never a fake swimming cycle. Review side, top and three-quarter views plus actual follow-camera reading.
3. **Composition proof with cover.** Model the hero hollow trunk, one fallen log, two branch/root clusters, a silt bank piece, lily pads and reeds. Place these with the reviewed fish in an isolated Godot proof scene. Review the log opening, fish/camera route, waterline, layered depth and lighting against the reference before multiplying variants.
4. **Full terrain, wood and foliage kit.** Build the remaining reusable variants. Make log, stick and plant clusters with controlled scale/rotation/color variation and placement rules tied to bank depth, shoreline and water surface. Confirm collision only where useful; small plants and leaf litter should remain inexpensive decoration.
5. **Other creatures and insects.** Add frog, snake and snapping turtle bases, then larva and water-bug families. Give each only the motion needed to make its planned placement believable. Keep any predator, prey, damage, feeding or growth behavior for later gameplay phases.
6. **Integrated density pass.** Assemble shallow bank, log-filled middle water and sparse deep basin views using the kit. Review from the player camera and while swimming, measure representative instance/draw/triangle cost, fix repeated stamps, clipping, floating cover, blocked routes and unreadable silhouettes. Supply the assets to P4's deterministic catalog and bake/load workflow after the P3 dependency is satisfied.

## Deliverables per asset family

- Editable first-party `.blend` master and reproducible generator/source recipe where generation is used; separate reviewed `.glb` exports in the runtime asset tree.
- Manifest entry with stable FAF ID, category, species/type, pond habitat tag (or explicitly neutral), dimensions, exported triangles/materials, source/output paths, rig/Action names where applicable, provenance and technical/style review states.
- Side/top/three-quarter evidence for animals and cover; family overview for foliage/details; Godot Forward+ proof at intended scale and lighting.
- Simple authored collision for large logs, trunks, banks and routes. Record actual opening/camera clearance. Do not infer playable clearance from a beauty render.

## Acceptance for the asset plan

- Every requested subject has a source model, exported asset and manifest identity; fish have verified rig/animation clips and species-specific breathing classification.
- The representative scene reads as the supplied pond reference: overhead lily-pad canopy, layered grasses/reeds, believable silt and woody shelter, warm shallow light and darker hazy depth.
- The hero hollow trunk is an actual usable route with visual and collision agreement. Shallow cover remains navigable at the selected fish/camera envelope.
- Native Godot inspection shows correct scale, orientation, materials, animation and habitat placement. Density is measured in a representative scene; the numerical budget is set from that observation rather than invented now.
- Pond-only species/assets are tagged and cannot silently populate Ocean. Shared generic assets are marked neutral explicitly. No claim of P4 Gate completion follows from an isolated proof scene.

## Boundaries and dependencies

This plan covers **base art and visual integration proof**. It does not implement World Select, final pond generation, feeding, predator AI, growth, saves, death or Life History. P4 integration should use the accepted shared deterministic compiler and baked loader after P3's pending M1–M4 Ocean review. Preserve the existing Ocean art, scene and Forward+ settings while creating a separately tunable Pond profile.
