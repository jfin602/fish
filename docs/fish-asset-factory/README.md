# Fish Asset Factory (FAF)

FAF is the source-controlled system for making **original Fish art** in the owner's [bright stylized low-poly reef naturalism](art-style.md). It establishes repeatable decisions and review, not a completed procedural asset library. Existing P1/P2 gameplay and third-party imports remain as they are. [Art and assets](../art-and-assets.md) continues to own licensing, acquisition and provenance.

## Production path

Owner [references](references/README.md) → [art style](art-style.md) → [palette](palette.md) → [modeling](modeling-standards.md) → [rigging](rigging-standards.md) / [animation](animation-standards.md) where applicable → [ID and naming](asset-ids.md) → [working budgets](budgets.md) → [Blender / GLB / Godot handoff](blender-godot-contract.md) → [visual review](visual-review.md) → reusable [family generators](generators.md).

Each candidate carries a small [manifest/check record](blender-godot-contract.md#candidate-record) and receives separate technical and style decisions. Blender is the editable source and viewport inspection environment. The repo's [tooling skeleton](../../tools/asset-factory/README.md) holds reusable Python; Blender MCP can inspect scenes, capture viewport screenshots and run controlled Python, but transient MCP commands are not the source of truth. Godot's native view is the final production-style check. This workflow supplies no Roadmap Gate evidence by itself.

## Current boundary

**FAF-001 Yellow Tang** is a local modeling experiment, not accepted production art. Local ignored `.blend` files were observed under `artifacts/assets/source/`; none is tracked in this checkout. Rig, actions, export, triangle count and material count have not been qualified here. The older [Sea life Yellow Tang](../assets/p1-starter-manifest.md) is a separate third-party candidate and must not be conflated with this original experiment. See [asset IDs](asset-ids.md#current-experiment) before assigning a permanent production ID.
