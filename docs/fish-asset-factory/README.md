# Fish Asset Factory (FAF)

FAF is the source-controlled system for making **original Fish art** in the owner's [bright stylized low-poly reef naturalism](art-style.md). It establishes repeatable decisions and review, not a completed procedural asset library. Existing P1/P2 gameplay and third-party imports remain as they are. [Art and assets](../art-and-assets.md) continues to own licensing, acquisition and provenance.

## Production path

Owner [references](references/README.md) → [art style](art-style.md) → [palette](palette.md) → [modeling](modeling-standards.md) → [rigging](rigging-standards.md) / [animation](animation-standards.md) where applicable → [ID and naming](asset-ids.md) → [working budgets](budgets.md) → [Blender / GLB / Godot handoff](blender-godot-contract.md) → [visual review](visual-review.md) → reusable [family generators](generators.md).

Each candidate carries a small [manifest/check record](blender-godot-contract.md#candidate-record) and receives separate technical and style decisions. Blender is the editable source and viewport inspection environment. The repo's [tooling skeleton](../../tools/asset-factory/README.md) holds reusable Python; Blender MCP can inspect scenes, capture viewport screenshots and run controlled Python, but transient MCP commands are not the source of truth. Godot's native view is the final production-style check. This workflow supplies no Roadmap Gate evidence by itself.

## Current boundary

The former **FAF-001 Yellow Tang** prototype is registered as [FAF-FISH-001](fish-001/README.md). Its editable Blender source, GLB, rig, three Actions, technical checks and isolated native Godot proof are recorded there. It is a technical reference candidate; reef-context style approval remains open. The older [Sea life Yellow Tang](../assets/p1-starter-manifest.md) remains a separate third-party candidate. The current player and movement are unchanged.
