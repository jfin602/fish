# Fish Asset Factory (FAF)

FAF is the source-controlled system for making **original Fish art** in the owner's [bright stylized low-poly reef naturalism](art-style.md). It establishes repeatable decisions and review, not a completed procedural asset library. Existing P1/P2 gameplay and third-party imports remain as they are. [Art and assets](../art-and-assets.md) continues to own licensing, acquisition and provenance.

## Production path

Owner [references](references/README.md) → [art style](art-style.md) → [palette](palette.md) → [modeling](modeling-standards.md) → [rigging](rigging-standards.md) / [animation](animation-standards.md) where applicable → [ID and naming](asset-ids.md) → [working budgets](budgets.md) → [Blender / GLB / Godot handoff](blender-godot-contract.md) → [visual review](visual-review.md) → reusable [family generators](generators.md).

Each candidate carries a small [manifest/check record](blender-godot-contract.md#candidate-record) and receives separate technical and style decisions. Blender is the editable source and viewport inspection environment. The repo's [tooling skeleton](../../tools/asset-factory/README.md) holds reusable Python; Blender MCP can inspect scenes, capture viewport screenshots and run controlled Python, but transient MCP commands are not the source of truth. Godot's native view is the final production-style check. This workflow supplies no Roadmap Gate evidence by itself.

For every new fish, the [rigging standard](rigging-standards.md#breathing-anatomy-is-required-at-generation) makes breathing anatomy and its deformation rig part of the initial generator path. Ordinary bony fish receive paired operculum bones; non-operculate species require their own declared mechanism or remain explicitly pending. Godot will supply runtime breathing behavior later.

## Current boundary

The former **FAF-001 Yellow Tang** prototype is registered as [FAF-FISH-001](fish-001/README.md). Its editable Blender source, GLB, rig, three Actions, technical checks and isolated native Godot proof are recorded there. It is a technical reference candidate; reef-context style approval remains open. The older [Sea life Yellow Tang](../assets/p1-starter-manifest.md) remains a separate third-party candidate. The current player and movement are unchanged.

The first varied original reef-fish library is [FAF-FISH-001–005](reef-fish-pack-001/README.md): the preserved Yellow Tang plus four new compact, schooling, highly compressed, and shark body-family candidates. Four separate GLBs, a reusable multi-scene Blender source/recipe, deformation evidence, and individual Godot proofs are recorded there. Reef-context style approval remains open; no fish replaced the player.

The first original rock family is [FAF-ROCK-001](rock-001/README.md): four deterministic boulder variants with a shared generator, Blender review collection, separate GLBs and isolated Godot import/native proof. It has not been placed into the live P2 habitat.

The first original coral pack is [FAF-CORAL-001–004](coral-pack-001/README.md): twelve deterministic assets in branching, rounded, tube and fan families, with Blender family review, separate GLBs and isolated Godot import/native proof. It has not been placed into the live P2 habitat.

The first original vegetation pack is [FAF-PLANT-001–003](plant-pack-001/README.md): eight deterministic grass, broad seaweed and reef algae assets with sway-ready height coordinates, family/density review, separate GLBs and isolated Godot proof. It has not been placed into the live P2 habitat.

The first original modular seabed pack is [FAF-TERRAIN-001](terrain-001/README.md): six deterministic broad-polygon sand/reef chunks with overlap-friendly irregular aprons, separate GLBs, Blender composition review, and isolated Godot proof. Visible close-view seams need authored dressing before use in a style-proving scene. It has not been placed into the live P2 habitat.

The first small reef-floor detail pack is [FAF-DETAIL-001–004](detail-pack-001/README.md): eight inexpensive shell, rubble, pebble and sponge A/B assets with deterministic family generators, separate GLBs, Blender pair review and one isolated Godot cluster proof. It has not been placed into the live P2 habitat.

The first [integrated FAF style proof](style-proof-001/README.md) places the six completed base packs together in an isolated Godot Forward+ scene using Fish's current water presentation. Native gameplay-scale and overview views make the remaining arch, terrain seam, lighting and fish-scale gaps concrete for owner review. It does not replace or approve the live P2 habitat.
