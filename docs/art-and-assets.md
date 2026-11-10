# Art and assets

## Decision and current state

**Owner-approved direction — 2026-11-09:** use realistic ocean creatures and environments, starting from free assets suitable for eventual commercial game use. The owner chose the ocean shortlist after reviewing the research and requested that these decisions be recorded in the repository.

This document owns the approved sources, starter order, licensing notes, and asset preparation checks. [The decision register](decisions-and-questions.md) records the approval as S7/C16/C17. [The roadmap](roadmap.md) continues to own implementation progress.

**P1 starter decision revised 2026-11-09:** The owner provided Quaternius's Animated Fish Pack and selected its small `Fish3` / `ClownFish` as the provisional player, with other pack fish for world population. Rock Face 01, Coast Sand 01, and Smithsonian *Heliopora coerulea* are the other three acquired starter sources. This is a deliberate low-poly P1 prototype exception to the longer-term realistic-ocean art direction, recorded in [decision C31](decisions-and-questions.md). [The P2 manifest](assets/p1-starter-manifest.md) records actual pack inspection and prior candidate evidence. The four sources import and instantiate in the local Godot prototype; [P5 native evidence](tasks/p1/qualification/native-display-and-assets.md) confirms their visible integration and representative timing on Linux/X11, with final art and motion quality still open.

## Approved starter order

1. **First art/import set:** Quaternius `Fish3` / `ClownFish` from the acquired Animated Fish Pack, Rock Face 01, Coast Sand 01, and Smithsonian *Heliopora coerulea*. Use these for a compact ocean/reef test scene with one animated player fish, rocky obstacles, a seabed surface, and coral. Other pack fish may provide non-gameplay population after preparation.
2. **Additional downloaded ocean content:** Sea life (coral, plankton, fish meshes) and a Great White Shark from BlendSwap were inventoried. Sea life carries packed images with Poliigon product names; their redistribution rights remain unresolved. The shark is a later predator candidate. No entire Blender scene is approved for wholesale import.
3. **Later expansion shortlist:** Tuna Fish Animated, Tropical Fish Pack, Great White Shark, and Kelp with animation, as the selected habitat and gameplay need them.

The controllable art-test fish is `Fish3` / `ClownFish`; it does not decide the final player species or final visual treatment. Emperor Angelfish was removed from the starter set because official acquisition requires Unity Asset Store/Package Manager and account-backed tooling. The shortlist does not require every species and plant to coexist in one biome. Keep reef and kelp composition deliberate.

## Ocean creatures

The linked creator/store listing supplies claims for unacquired candidates. For acquired files, the [P2 manifest](assets/p1-starter-manifest.md) records inspected contents and actual license evidence. Keep those distinctions when selecting new assets.

| Asset and creator | Intended role / priority | Reported contents and preparation | Listed license |
| --- | --- | --- | --- |
| [Animated Fish Pack — Quaternius](https://quaternius.itch.io/lowpoly-animated-fish) | First controllable art-test fish: `Fish3` / `ClownFish`; other fish for background population. Low-poly appearance is explicitly accepted for this P1 prototype. | Owner-provided ZIP has seven Blender/FBX/OBJ models. Godot imported the FBX scenes; Fish3 has a weighted mesh, eight bones and a rotation-only `Armature|Swim` clip. The loop seam and native appearance still need P5/P9 review. | Included `License.txt`: CC0 1.0 Universal; no required attribution. Retain optional creator credit and original archive. |
| [Tuna Fish Animated — GoldenZtuff](https://www.cgtrader.com/free-3d-models/animal/fish/tuna-fish-9f41924a-83d4-478e-a8e9-370946f141b3) | Later larger ocean fish candidate. | Blender control and export rigs, FBX, one swim animation, and 4K body/eye textures. Additional gameplay actions are not established by the listing. | CGTrader Royalty Free License (no AI); listing permits personal/commercial use. Source redistribution and packaged-use conditions apply. |
| [Rigged Archer Fish — nerk987](https://blendswap.com/blend/20576) | Acquired for evaluation; **excluded from the ocean starter** because archerfish are freshwater/brackish estuary fish. | Blender source has an armature, packed texture images, and one action with many keyframes; a swim loop was not verified. Includes an embedded rig UI script that must not become gameplay code. | Included BlendSwap license: CC BY 3.0; author credit and change notice required for redistribution. |
| [Tropical Fish Pack — ZivixLLC](https://www.turbosquid.com/3d-models/tropical-fish-pack-652729) | Later small fish, prey variety, and background schools. | 15 textured fish, approximately 474–728 triangles each, FBX/OBJ, 512-pixel-class diffuse/normal textures. Explicitly unrigged and unanimated; add swimming motion and evaluate the appearance at camera distance. | TurboSquid Standard license. Free at research date; source redistribution and packaged-use conditions apply. |
| [Great White Shark — CanYuTsai](https://sketchfab.com/3d-models/great-white-shark-carcharodon-carcharias-6a08bdc55884492ca66cfa9f589ff82f) | Later marine predator candidate. | Approximately 3,800 triangles; animated with a mouth-opening animation. Full action list, source formats, and texture detail require inspection. | CC Attribution; preserve the exact license version and attribution supplied with the download. |
| [Great White Shark — xSNOWM4Nx](https://blendswap.com/blend/20882) | **Acquired** separate later predator candidate. | Blender 2.79 source with shark meshes parented to an armature; one PoseLib action, no verified swim cycle. Cycles procedural materials/micro-displacement need rebuilding or baking. | Included BlendSwap license: CC BY 3.0; author credit and change notice required for redistribution. |

These roles are content candidates. An included attack or death animation does not implement those mechanics or move them ahead of the current swimming work.

### Starter source changes and access checks

- [Emperor Angelfish](https://assetstore.unity.com/packages/3d/characters/animals/fish/emperor-angelfish-263329) was researched for the original first set but rejected for this Godot starter workflow: its official package requires Unity Asset Store/Package Manager and an account. Its advertised rig and 17 animations were never inspected locally.
- The [Bluefin Tuna candidate](https://www.cgtrader.com/free-3d-models/animal/fish/tuna-3d-model-5cb4bec7-4090-450e-9c39-00f25a2bf841) advertises Blender/FBX, a rig and a swim animation. Its official anonymous Download button opened a login dialog on 2026-11-09, so it does not meet the account-free requirement. No package or license file was acquired.
- The [Khronos Barramundi Fish](https://github.com/KhronosGroup/glTF-Sample-Assets/tree/main/Models/BarramundiFish) is an official, CC0, account-free GLB. The downloaded file has no skin or animation, so it is not the starter fish.
- A [Sea Bream page on blen3d](https://blen3d.com/en/models/seabream-animated-91e25be820d73cd2) was not accepted: a matching [Blendkit listing](https://www.blendkit.com/?query=category_subtree%3Afish+order%3A-created) marks the model Full Plan, while the blen3d page labels it CC0. Its provenance/rights could not be established from the original creator and its download was not completed.

## Ocean environment

| Asset and creator | Intended role / priority | Reported contents and preparation | Listed license |
| --- | --- | --- | --- |
| [Rock Face 01 — Poly Haven / Dario Barresi](https://polyhaven.com/a/rock_face_01) | First set: rocky boundaries, obstacles, and shelter formations. | Approximately 20,000 triangles, 7.1 m source width, glTF/Blend/FBX options and 1K–8K maps. Inspect scale, collision needs, and the selected texture resolution. | CC0. |
| [Coast Sand 01 — Poly Haven / Rob Tuytel](https://polyhaven.com/a/coast_sand_01) | First set: seabed surface. | Sand and small-stone material with associated texture maps. Apply to terrain geometry authored for Fish; this is not a complete playable seabed. | CC0. |
| [Heliopora coerulea — Smithsonian Institution](https://3d.si.edu/object/3d/heliopora-coerulea%3Aebea5b5f-5613-4d38-97a2-32e1b8e03ac4) | First set: realistic coral specimen for reef context; potential shelter requires authored collision. | Official low-resolution GLB acquired and inspected: one mesh, approximately 99,940 triangles, three embedded 2K JPEG images. P3 checked imported size/material; appearance, optimization and collision need P4/P5 checks. This is one specimen, not a coral set. | CC0 1.0, public domain; no required credit. |
| [Sea life — mark06gt](https://blendswap.com/blend/28958) | Acquired for possible individual coral/plankton/environment pieces and unrigged background fish. | Original Blender 2.93 file contains 47 objects, including coral balls/stalks/fans, sea ferns/grass, plankton, Blue Fish and Yellow Tang; no armature or action. Sixteen packed image datablocks include eight maps with Poliigon product names/paths. Whole-scene import and use of those images are not cleared. | Creator listing CC0, but embedded third-party texture rights are unresolved; keep raw file and derivatives with those maps private. |
| [Kelp with animation — cswindley](https://sketchfab.com/3d-models/kelp-with-animation-22f71d83535c41c1bfa12b2d3e64af0e) | Later vegetation for an appropriate kelp habitat. | Animated textured kelp with opacity maps; approximately 49,300 triangles. Check animation export, transparency, and cost before dense duplication. | CC Attribution; preserve the download's exact license and attribution. |

[Soft Coral Set — Kanna-Nakajima](https://sketchfab.com/3d-models/soft-coral-set-256355f15fcb4095af17b75ae572bff0) was the original coral choice. Its public metadata lists CC BY 4.0 and says it is downloadable, but the official anonymous download API returned HTTP 401 requiring credentials on 2026-11-09. It was therefore replaced for the starter workflow with the account-free Smithsonian coral scan above. The two assets differ: the replacement is one specimen and may require optimization or duplication for useful reef composition. Soft Coral Set remains a researched option only if access is later authorized.

## P2 worldbuilding use

The P1 starter set is deliberately small and does not by itself constitute the finished P2 habitat. P2 should reuse Rock Face 01, Coast Sand 01 and Heliopora where they work, but composition must be driven by the habitat target rather than by forcing two rocks/two coral props to fill the world.

The [P2 environment manifest](assets/p2-environment-manifest.md) records the prepared local palette and P4 placements: three additional Poly Haven rocks, three ambientCG terrain materials and four additional Smithsonian coral species. Normal play now instances all nine prepared model identities and binds all four seabed material sets into one blended shader surface, with sparse high-detail coral instances and one Coastal Cliff hero piece. P7 real-window captures show the repaired blended terrain and mixed reef routes; [P8 review](tasks/p2/closeout.md) leaves each asset's visible use and the final natural/underwater judgment open for the owner.

The Smithsonian coral is a high-detail specimen; do not duplicate it densely without measuring the cost. Prepare efficient derivatives, sparse hero placement, instancing/LOD or lower-cost authored/provenance-safe pieces as needed. Additional P2 environment assets are allowed only when they are free, legally suitable for the project's intended direction, account-free or otherwise explicitly available to the owner, and recorded with source/creator/date/license/dependency information. No paid/account-gated asset becomes a hidden P2 requirement.

Keep the normal public-repository boundary: permission to ship an asset in a game does not automatically permit raw source redistribution. The P2 renderer/habitat task may change materials, meshes, LODs or scene wrappers; record those transformations without replacing the original provenance.

## License and provenance handling

Fish's repository is public. Permission to include an asset in a finished game is distinct from permission to distribute its source model and textures through GitHub.

- **CC0:** modification, commercial use, and redistribution are permitted. Keep creator/source records even where attribution is optional. See [CC0](https://creativecommons.org/publicdomain/zero/1.0/) and [Poly Haven's asset license](https://polyhaven.com/license).
- **CC Attribution:** commercial use and adaptation are permitted under the applicable attribution terms. Retain creator credit, source URL, exact license/version, and modification notices. See [CC BY 4.0](https://creativecommons.org/licenses/by/4.0/) as a reference; use the version attached to the actual download.
- **Later shortlisted Unity, CGTrader, and TurboSquid sources:** do not commit raw or converted model/texture files under their researched terms. Check game-packaging requirements before distributing an export. See [Unity's other-engine guidance](https://support.unity.com/hc/en-us/articles/34387186019988-Can-I-use-assets-from-the-Asset-Store-with-other-engines), [CGTrader's royalty-free terms](https://help.cgtrader.com/hc/en-us/articles/360015124437-Royalty-Free-License), and [TurboSquid's license](https://www.turbosquid.com/licensing).

At acquisition, retain the exact source URL, creator, download date, package/version identity, license text and notices, required credit, and a record of conversions or edits. Verify all included dependencies against the package terms. A later storefront or license change must be recorded rather than silently assumed.

The owner-provided BlendSwap Great White Shark and Archer Fish ZIPs contain CC BY 3.0 license HTML; original archives remain ignored. Sea life's standalone `.blend` has no separate notice. Its packed `RockMossyFull005` and `RockSpottyMoss009` images point to Poliigon paths. [Poliigon's licensing FAQ](https://help.poliigon.com/en/articles/8749749-asset-use-licensing) forbids redistribution of its assets and embedding them in shared scene files. Treat those maps as restricted until independently cleared; the BlendSwap CC0 label alone does not resolve their rights. The [manifest](assets/p1-starter-manifest.md) records exact local bytes, hashes and Blender datablock findings.

## Godot preparation and first checks

Use the existing Godot .NET/C# direction. Prefer glTF/GLB for prepared scene assets; FBX is also supported. Direct Blender import invokes Blender's glTF exporter. OBJ does not carry skeletal animation. These format capabilities are documented by [Godot's 3D import guide](https://docs.godotengine.org/en/stable/tutorials/assets_pipeline/importing_3d_scenes/available_formats.html); they do not qualify any shortlisted download.

P1's authored [acquisition, preparation and course tasks](tasks/p1/implementation-plan.md#p2--starter-asset-acquisition) must use the first set in the actual movement scene. Keep exact provenance and a reproducible local installation path; selected-asset acceptance requires all four in native play. A placeholder can isolate development but cannot pass the P1 asset requirement. Source access or a defective asset needs a precise dependency/repair record; do not silently substitute another shortlist entry.

For the first authorized art/import task:

1. Acquire only the selected set, inspect archives and terms, and preserve source/license records.
2. Inspect geometry, textures, scale, orientation, deforming bones, and available animation clips. Bake or adapt authoring-specific rigs/materials when required; do not assume Unity prefabs/shaders or Blender procedural effects transfer.
3. Assemble one small scene with the four starter assets. Check swimming loops, speed transitions, fin transparency, body deformation, material consistency, and visibility from the actual third-person camera.
4. Check representative fish/vegetation duplication on the target machine before expanding the habitat. Record actual performance and preparation changes, rather than inferring suitability from polygon counts alone.
5. Keep imported visual motion aligned with the choices in [swimming and camera](systems/swimming-and-camera.md) and the [completed worksheet](planning/p1/swim-controls-worksheet.md). P1 still must lock its core movement. After that, P2 worldbuilding is the next roadmap Phase; Movement abilities remain accepted future work.

The **P1 task P2/P3** source and import results are recorded in the [manifest](assets/p1-starter-manifest.md) and [implementation evidence](tasks/p1/qualification/implementation.md). The P0/P1 movement implementation remains the base for P2 environment work.

## Remaining choices and limits

- **Final player species and exact ocean biome:** open. The first test fish and reef arrangement do not settle production ecology or the complete species roster.
- **Growth visuals:** no matching hatchling/juvenile/adult asset sequence has been verified. Scaling a mesh is a possible early prototype approach, not an approved final growth-art solution. Growth remains within the same fish identity.
- **Water, lighting, and detailed visual treatment:** P2 implements a Forward+ atmosphere and movement-reactive water candidate. Its final appearance and comfort remain open in [P8 review](tasks/p2/closeout.md); realistic source assets do not set a photorealism target by themselves.
- **Renderer and performance budget:** [the P2 decision](tasks/p2/qualification/renderer-decision.md) locks normal play to Forward+; Compatibility remains a diagnostic comparison. Production budgets and wider-platform claims remain later measured decisions.
- **Implementation evidence:** P2 source downloads and inventories are recorded in the [manifest](assets/p1-starter-manifest.md). P3 verifies headless Godot imports and resource structure. Animation quality, collision/shelter function, native display and performance remain unqualified for later tasks. Asset acquisition alone passes no Roadmap Gate.
