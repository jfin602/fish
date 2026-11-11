# Fish Asset Factory (FAF)

FAF is the source-controlled system for making **original Fish art** under the shared [stylized low-poly aquatic naturalism contract](art-style.md), retaining bright reef naturalism as the **Ocean-specific** style and adding a distinct freshwater Pond style in P4. It establishes repeatable decisions and review, not a completed procedural asset library. Existing P1/P2 gameplay and third-party imports remain as they are. [Art and assets](../art-and-assets.md) continues to own licensing, acquisition and provenance.

## Production path

Owner [references](references/README.md) → [art style](art-style.md) → [palette](palette.md) → [modeling](modeling-standards.md) → [rigging](rigging-standards.md) / [animation](animation-standards.md) where applicable → [ID and naming](asset-ids.md) → [working budgets](budgets.md) → [Blender / GLB / Godot handoff](blender-godot-contract.md) → [visual review](visual-review.md) → reusable [family generators](generators.md).

Each candidate carries a small [manifest/check record](blender-godot-contract.md#candidate-record) and receives separate technical and style decisions. Blender is the editable source and viewport inspection environment. The repo's [tooling skeleton](../../tools/asset-factory/README.md) holds reusable Python; Blender MCP can inspect scenes, capture viewport screenshots and run controlled Python, but transient MCP commands are not the source of truth. Godot's native view is the final production-style check. This workflow supplies no Roadmap Gate evidence by itself.

For every new fish, the [rigging standard](rigging-standards.md#breathing-anatomy-is-required-at-generation) makes breathing anatomy and its deformation rig part of the initial generator path. Ordinary bony fish receive paired operculum bones; non-operculate species require their own declared mechanism or remain explicitly pending. Godot will supply runtime breathing behavior later.

## Habitat-aware FAF expansion (P4 planned)

After the P3 Ocean generator corrections/qualification, [P4](../planning/p4/freshwater-world.md) extends FAF without replacing any existing marine assets or disrupting fish life-stage/gill-rigging conventions. New freshwater GLBs/Blender masters, manifests, asset IDs and variant generators must be independent of ocean identities. Planned families: freshwater fish, muddy terrain and sediment, logs/stumps/roots, pond vegetation (reeds/cattails/lilies/pondweed/algae), and representative frog/turtle/snake/crayfish/aquatic insects.

A stable species ID and explicit native habitat compatibility (Ocean or Pond) accompany each creature. Ordinary character selection, ambient spawning and future ecology use the same compatibility definition. Existing five FAF marine fish remain Ocean-only. Shared generic geometry may be marked compatible with both habitats explicitly, but habitat-specific coral and fish must never leak across worlds through fallback loading. New species must follow appropriate breathing/animation/rigging standards, with technical and native style review for each selected habitat. These are prospective P4 deliverables, not currently completed packs.

## Current boundary

The former **FAF-001 Yellow Tang** prototype is registered as [FAF-FISH-001](fish-001/README.md). Its editable Blender source, GLB, rig, three Actions, technical checks and isolated native Godot proof are recorded there. It is a technical reference candidate; reef-context style approval remains open. The older [Sea life Yellow Tang](../assets/p1-starter-manifest.md) remains a separate third-party candidate. The current player and movement are unchanged.

The first varied original reef-fish library is [FAF-FISH-001–005](reef-fish-pack-001/README.md): the preserved Yellow Tang plus four new compact, schooling, highly compressed, and shark body-family candidates. Four separate GLBs, a reusable multi-scene Blender source/recipe, deformation evidence, and individual Godot proofs are recorded there. Reef-context style approval remains open; no fish replaced the player.

The [life-stage family](life-stages-001/README.md) adds 20 developmental representations to those five identities, alongside their preserved canonical adults. Four bony-fish species have Egg, Larva, Juvenile, Subadult and Adult; the live-bearing shark has NewbornPup, EarlyJuvenile, Juvenile, Subadult and Adult. Stage-specific geometry, proportions, markings, color and swim motion are recorded in a separate source, manifest, GLBs and isolated Godot viewer. These assets do not change gameplay growth or the current player.

The first original freshwater [Pond fish pack](pond-fish-pack-001/README.md) registers **FAF-FISH-006–010**: bluegill, largemouth bass, channel catfish, common carp and northern pike. Each has five separately rigged developmental stages, including a deforming Egg with Idle/Hatch. The 25 Pond-tagged GLBs, five editable Blender masters, manifest, focused checks and isolated Forward+ viewer do not enter Ocean or normal play. Pond-context owner style approval remains open.

The first original rock family is [FAF-ROCK-001](rock-001/README.md): four deterministic boulder variants with a shared generator, Blender review collection, separate GLBs and isolated Godot import/native proof. It has not been placed into the live P2 habitat.

The first original coral pack is [FAF-CORAL-001–004](coral-pack-001/README.md): twelve deterministic assets in branching, rounded, tube and fan families, with Blender family review, separate GLBs and isolated Godot import/native proof. It has not been placed into the live P2 habitat.

The first original vegetation pack is [FAF-PLANT-001–003](plant-pack-001/README.md): eight deterministic grass, broad seaweed and reef algae assets with sway-ready height coordinates, family/density review, separate GLBs and isolated Godot proof. It has not been placed into the live P2 habitat.

The first Pond-only [foliage pack](pond-foliage-pack-001/README.md) adds **FAF-PLANT-004–009**: 13 lily, grass, cattail, reed, submerged algae and floating muck variants. It has a separate editable Blender master, deterministic family generators, Pond-tagged GLBs and an isolated Forward+ shallow-bank proof. It does not enter Ocean or normal play.

The first Pond-only [insect pack](pond-insect-pack-001/README.md) registers **FAF-INSECT-001–002** with two common green darner nymphs and two backswimmers. Four individually skinned Blender masters and GLBs have looping ambient Actions, Pond metadata, focused imported-clip checks and an isolated Forward+ shallow-bank proof. Actual 3.8 m fish-camera readability is limited; normal play remains unchanged.

The first Pond-only [creature pack](pond-creature-pack-001/README.md) registers **FAF-CREATURE-001–003**: a northern leopard frog, northern water snake and common snapping turtle. Three original deforming rigs, editable Blender masters, Pond-tagged GLBs, species-specific Actions and an isolated Forward+ bank/waterline proof passed focused source, export and imported-clip checks. No gameplay or Ocean scene is wired to them; Pond style owner lock remains open.

The first Pond [wood and cover pack](pond-wood-pack-001/README.md) registers **FAF-WOOD-001–004**: twelve distinct fallen-log, hollow-trunk, stump and branch/root variants. Source-controlled Blender recipes, editable masters, Pond-tagged GLBs, simple scene collision wrappers and an isolated Forward+ traversal fixture preserve the current Ocean and P3 worlds. Its hero opening is qualified with the existing fish controller and follow camera in both directions; style refinement remains separately recorded.

The first Pond [ground-dressing pack](pond-ground-pack-001/README.md) reserves **FAF-TERRAIN-002A/B** and **FAF-DETAIL-005–007 A/B**: two bank shoulders, two algae-coated stones, two pebble clusters and two leaf-litter clusters. Four shared earthy materials and one broad-frequency basin shader support a single-floor isolated Forward+ placement proof. It does not change the generated Pond or Ocean runtime worlds.

The first original modular seabed pack is [FAF-TERRAIN-001](terrain-001/README.md): six deterministic broad-polygon sand/reef chunks with overlap-friendly irregular aprons, separate GLBs, Blender composition review, and isolated Godot proof. Visible close-view seams need authored dressing before use in a style-proving scene. It has not been placed into the live P2 habitat.

The first small reef-floor detail pack is [FAF-DETAIL-001–004](detail-pack-001/README.md): eight inexpensive shell, rubble, pebble and sponge A/B assets with deterministic family generators, separate GLBs, Blender pair review and one isolated Godot cluster proof. It has not been placed into the live P2 habitat.

The first [integrated FAF style proof](style-proof-001/README.md) places the six completed base packs together in an isolated Godot Forward+ scene using Fish's current water presentation. Native gameplay-scale and overview views make the remaining arch, terrain seam, lighting and fish-scale gaps concrete for owner review. It does not replace or approve the live P2 habitat.
