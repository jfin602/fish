# P2 environment working set and P4 use

Prepared on 2026-11-09. The authoritative local byte/file inventory is `artifacts/assets/source/p2-environment/inventory.json` (SHA-256 `391099f0febe9ce04daa9be8d7614ae32b90f89d6c25f12f57aeda9cdee681d0`). Its ten entries provide 34 runtime files; P4 checked every file against that inventory. Source archives and the Godot working copies under `game/assets/vendor-local/p2-environment/` are ignored local payload, not repository content. The existing P1 Rock Face 01, Coast Sand 01 and *Heliopora coerulea* remain described in the [P1 starter manifest](p1-starter-manifest.md). No additional source was fetched for P4.

| Prepared source | Provider / source page | Runtime preparation and P4 use |
| --- | --- | --- |
| Rock Face 02 | [Poly Haven](https://polyhaven.com/a/rock_face_02) | 2K glTF with color, normal and ARM maps; 29,566 triangles. Tunnel wall, shelter pillar and upper landmark. |
| Coastal Cliff 01 | [Poly Haven](https://polyhaven.com/a/coastal_cliff_01) | 2K glTF with color, normal and ARM maps; 461,824 triangles. One scaled tunnel roof/hero overhang. |
| Boulder 01 | [Poly Haven](https://polyhaven.com/a/boulder_01) | 2K glTF with color, normal and ARM maps; 66,122 triangles. Two seam/landmark pieces. |
| Ground060 | [ambientCG](https://ambientcg.com/view?id=Ground060) | 2K shell-rich sand maps; one visible region of the continuous seabed. |
| Gravel041 | [ambientCG](https://ambientcg.com/view?id=Gravel041) | 2K gravel maps; one visible transition region of the continuous seabed. |
| Rock063 | [ambientCG](https://ambientcg.com/view?id=Rock063) | 2K layered rock maps; visible exposed perimeter of the continuous seabed. |
| *Acropora cervicornis* | [Smithsonian coral collection](https://3d.si.edu/corals) | Unmodified 150,000-triangle GLB, embedded 4K images; two sparse branching placements. The saved filename does not establish an individual specimen catalog number. |
| *Acropora palmata* | [Smithsonian USNM 3968](https://3d.si.edu/es/object/3d/acropora-palmata%3A6fd30c79-3345-4d06-b27c-99d5b320ce5f) | Unmodified 100,000-triangle GLB, embedded 2K images; two broad shelf placements. |
| *Meandrina meandrites* | [Smithsonian USNM 1174917](https://3d.si.edu/es/object/3d/meandrina-meandrites%3A74dc5709-1808-4de3-bce5-bb9be8573d42) | Unmodified 99,996-triangle GLB, embedded 2K images; one rounded coral placement. |
| *Montipora danae* | [Smithsonian USNM 307](https://3d.si.edu/object/3d/montipora-danae%3A34b57d25-06d4-42b3-ac95-4c35cb361f6f) | Unmodified 99,998-triangle GLB, embedded 2K images; two contrasting shelf placements. |

[Poly Haven](https://polyhaven.com/license) and [ambientCG](https://docs.ambientcg.com/license/) publish these assets under CC0. The Smithsonian [Terms of Use](https://www.si.edu/termsofuse) and [Open Access FAQ](https://www.si.edu/openaccess/faq) explain the CC0/public-domain marks used for the selected downloadable media. The local inventory retains each exact archive/GLB hash, source URL, dimensions and Godot import result. The private working-set boundary is retained even where a source is CC0, so the repository does not accidentally stage raw vendor payload.

The P4 scene instantiates the imported source models at authored anchors. It scales and rotates them, offsets each imported model to its measured bottom center, and seats coral/rock bases into the heightfield or supporting rock. No mesh decimation or separate LOD was created. Simplified static box collision belongs to the rock anchors; coral is decorative. The current layout uses ten rock instances and eight coral instances, including all four prepared rock models and five coral species, for 1,663,394 imported instance triangles. `./tools/dev inspect-reef` checks source IDs, material use, visible mesh/material loading, count bounds and traversal geometry. P7 still owns final native appearance and camera comfort.
