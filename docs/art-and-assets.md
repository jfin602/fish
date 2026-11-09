# Art and assets

## Decision and current state

**Owner-approved direction — 2026-11-09:** use realistic ocean creatures and environments, starting from free assets suitable for eventual commercial game use. The owner chose the ocean shortlist after reviewing the research and requested that these decisions be recorded in the repository.

This document owns the approved sources, starter order, licensing notes, and asset preparation checks. [The decision register](decisions-and-questions.md) records the approval as S7/C16/C17. [The roadmap](roadmap.md) continues to own implementation progress.

**The four starter assets are selected for P1 acquisition, preparation and runtime integration; the other entries retain their later sourcing/evaluation order.** The owner's P1 request is recorded as S9/C27 in [the decision register](decisions-and-questions.md) and planned in [the P1 stack](tasks/p1/README.md). Listing and license research was checked on 2026-11-09; no asset download, archive inspection, Godot import, runtime performance, or final production acceptance has been verified. A free listing is not proof that its materials, rig, or animations work in Fish.

## Approved starter order

1. **First art/import set:** Emperor Angelfish, Rock Face 01, Coast Sand 01, and Soft Coral Set. Use these to prepare a compact ocean/reef test scene with one animated fish, rocky obstacles, a seabed surface, and coral.
2. **Next collection to inspect:** Sea life. Select useful pieces after inspecting its Blender contents; importing the entire collection is not a requirement.
3. **Later expansion shortlist:** Tuna Fish Animated, Tropical Fish Pack, Great White Shark, and Kelp with animation, as the selected habitat and gameplay need them.

Emperor Angelfish is the first controllable art-test candidate, not a final player-species decision. The shortlist does not require every species and plant to coexist in one biome. In particular, keep reef and kelp composition deliberate.

## Ocean creatures

The linked creator/store listing is the source for each row's reported contents. Exact archive contents and license text must be captured at acquisition.

| Asset and creator | Intended role / priority | Reported contents and preparation | Listed license |
| --- | --- | --- | --- |
| [Emperor Angelfish — Mikhail Nesterov](https://assetstore.unity.com/packages/3d/characters/animals/fish/emperor-angelfish-263329) | First controllable fish for the art/import test. | Rigged, 17 animations covering swimming, turns, feeding, attack, and death; 1,246 vertices and three 2K textures. Distributed as a Unity package: inspect the supplied mesh/animation files and prepare Godot materials. | Standard Unity Asset Store EULA. Free at research date; raw source redistribution restricted. |
| [Tuna Fish Animated — GoldenZtuff](https://www.cgtrader.com/free-3d-models/animal/fish/tuna-fish-9f41924a-83d4-478e-a8e9-370946f141b3) | Later larger ocean fish candidate. | Blender control and export rigs, FBX, one swim animation, and 4K body/eye textures. Additional gameplay actions are not established by the listing. | CGTrader Royalty Free License (no AI); listing permits personal/commercial use. Source redistribution and packaged-use conditions apply. |
| [Tropical Fish Pack — ZivixLLC](https://www.turbosquid.com/3d-models/tropical-fish-pack-652729) | Later small fish, prey variety, and background schools. | 15 textured fish, approximately 474–728 triangles each, FBX/OBJ, 512-pixel-class diffuse/normal textures. Explicitly unrigged and unanimated; add swimming motion and evaluate the appearance at camera distance. | TurboSquid Standard license. Free at research date; source redistribution and packaged-use conditions apply. |
| [Great White Shark — CanYuTsai](https://sketchfab.com/3d-models/great-white-shark-carcharodon-carcharias-6a08bdc55884492ca66cfa9f589ff82f) | Later marine predator candidate. | Approximately 3,800 triangles; animated with a mouth-opening animation. Full action list, source formats, and texture detail require inspection. | CC Attribution; preserve the exact license version and attribution supplied with the download. |

These roles are content candidates. An included attack or death animation does not implement those mechanics or move them ahead of the current swimming work.

## Ocean environment

| Asset and creator | Intended role / priority | Reported contents and preparation | Listed license |
| --- | --- | --- | --- |
| [Rock Face 01 — Poly Haven / Dario Barresi](https://polyhaven.com/a/rock_face_01) | First set: rocky boundaries, obstacles, and shelter formations. | Approximately 20,000 triangles, 7.1 m source width, glTF/Blend/FBX options and 1K–8K maps. Inspect scale, collision needs, and the selected texture resolution. | CC0. |
| [Coast Sand 01 — Poly Haven / Rob Tuytel](https://polyhaven.com/a/coast_sand_01) | First set: seabed surface. | Sand and small-stone material with associated texture maps. Apply to terrain geometry authored for Fish; this is not a complete playable seabed. | CC0. |
| [Soft Coral Set — Kanna-Nakajima](https://sketchfab.com/3d-models/soft-coral-set-256355f15fcb4095af17b75ae572bff0) | First set: reef decoration and potential shelter. | Textured coral made for an underwater game; approximately 28,100 triangles for the displayed set. Inspect individual meshes, materials, and download formats. Gameplay shelter/collisions require implementation. | CC Attribution; creator requests credit. |
| [Sea life — mark06gt](https://blendswap.com/blend/28958) | Inspect next for additional useful underwater pieces. | A 55.5 MB Blender 2.9/Cycles collection of fish, coral, and plankton. Polygon counts, rigs, animations, and material portability are not specified. | CC0. |
| [Kelp with animation — cswindley](https://sketchfab.com/3d-models/kelp-with-animation-22f71d83535c41c1bfa12b2d3e64af0e) | Later vegetation for an appropriate kelp habitat. | Animated textured kelp with opacity maps; approximately 49,300 triangles. Check animation export, transparency, and cost before dense duplication. | CC Attribution; preserve the download's exact license and attribution. |

## License and provenance handling

Fish's repository is public. Permission to include an asset in a finished game is distinct from permission to distribute its source model and textures through GitHub.

- **CC0:** modification, commercial use, and redistribution are permitted. Keep creator/source records even where attribution is optional. See [CC0](https://creativecommons.org/publicdomain/zero/1.0/) and [Poly Haven's asset license](https://polyhaven.com/license).
- **CC Attribution:** commercial use and adaptation are permitted under the applicable attribution terms. Retain creator credit, source URL, exact license/version, and modification notices. See [CC BY 4.0](https://creativecommons.org/licenses/by/4.0/) as a reference; use the version attached to the actual download.
- **The selected Unity, CGTrader, and TurboSquid licenses:** do not commit their raw or converted model/texture files to the public repository under the researched terms. Keep acquisition/source files outside tracked public content, and check the applicable game-packaging requirements before distributing an export. Record any separately obtained permission before relying on it. See [Unity's other-engine guidance](https://support.unity.com/hc/en-us/articles/34387186019988-Can-I-use-assets-from-the-Asset-Store-with-other-engines), [CGTrader's royalty-free terms](https://help.cgtrader.com/hc/en-us/articles/360015124437-Royalty-Free-License), and [TurboSquid's license](https://www.turbosquid.com/licensing).

At acquisition, retain the exact source URL, creator, download date, package/version identity, license text and notices, required credit, and a record of conversions or edits. Verify all included dependencies against the package terms. A later storefront or license change must be recorded rather than silently assumed.

## Godot preparation and first checks

Use the existing Godot .NET/C# direction. Prefer glTF/GLB for prepared scene assets; FBX is also supported. Direct Blender import invokes Blender's glTF exporter. OBJ does not carry skeletal animation. These format capabilities are documented by [Godot's 3D import guide](https://docs.godotengine.org/en/stable/tutorials/assets_pipeline/importing_3d_scenes/available_formats.html); they do not qualify any shortlisted download.

P1's authored [acquisition, preparation and course tasks](tasks/p1/implementation-plan.md#p2--starter-asset-acquisition) must use the first set in the actual movement scene. Keep exact provenance and a reproducible local installation path; selected-asset acceptance requires all four in native play. A placeholder can isolate development but cannot pass the P1 asset requirement. Source access or a defective asset needs a precise dependency/repair record; do not silently substitute another shortlist entry.

For the first authorized art/import task:

1. Acquire only the selected set, inspect archives and terms, and preserve source/license records.
2. Inspect geometry, textures, scale, orientation, deforming bones, and available animation clips. Bake or adapt authoring-specific rigs/materials when required; do not assume Unity prefabs/shaders or Blender procedural effects transfer.
3. Assemble one small scene with the four starter assets. Check swimming loops, speed transitions, fin transparency, body deformation, material consistency, and visibility from the actual third-person camera.
4. Check representative fish/vegetation duplication on the target machine before expanding the habitat. Record actual performance and preparation changes, rather than inferring suitability from polygon counts alone.
5. Keep imported visual motion aligned with the choices in [swimming and camera](systems/swimming-and-camera.md) and the [completed worksheet](planning/p1/swim-controls-worksheet.md). Core swimming, turning, and slowing remain the current priority; [Movement abilities](planning/p1/movement-abilities.md) follows core lock.

This document does not execute an import or expand the [p0 foundation scope](tasks/p0/README.md). P0 may still use simple native geometry and a placeholder fish to establish the development loop.

## Remaining choices and limits

- **Final player species and exact ocean biome:** open. The first test fish and reef arrangement do not settle production ecology or the complete species roster.
- **Growth visuals:** no matching hatchling/juvenile/adult asset sequence has been verified. Scaling a mesh is a possible early prototype approach, not an approved final growth-art solution. Growth remains within the same fish identity.
- **Water, lighting, and detailed visual treatment:** still to be designed and measured in Godot. Realistic source assets do not set a photorealism target or supply the finished underwater presentation.
- **Renderer and performance budget:** follow [technical direction](technical-direction.md). Compatibility remains the reversible bootstrap default; this asset decision does not switch renderers.
- **Implementation evidence:** downloads, imports, animation behavior, collision/shelter function, and performance remain unverified until observed and recorded in the relevant task evidence. Asset selection alone passes no Roadmap Gate.
