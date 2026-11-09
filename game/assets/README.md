# Local starter assets

The public project deliberately omits source downloads and restricted working assets. P2 acquisition status and exact hashes are in [the starter manifest](../../docs/assets/p1-starter-manifest.md). A clone without local packages remains inspectable; missing art is an acquisition/preparation dependency, not evidence that the selected asset worked in Godot.

## Storage

- Put original source files and downloaded archives under repository-root `artifacts/assets/source/<asset>/`, outside Godot's import tree. This entire `artifacts/` tree is ignored.
- Put any locally prepared resource with restricted or unresolved source rights under `game/assets/vendor-local/`. This directory is ignored explicitly. Do not embed those payloads in tracked `.tscn`, `.tres`, generated packs or public wrappers.
- The four acquired starter sources are `artifacts/assets/source/quaternius-animated-fish/`, `artifacts/assets/source/rock_face_01-2k/`, `artifacts/assets/source/coast_sand_01-2k/`, and `artifacts/assets/source/heliopora-coerulea/`. The owner selected Quaternius `Fish3` / `ClownFish` as provisional player art; the local Godot preparation paths are below. Additional owner-provided ocean candidates are retained under `sea-life-blendswap/`, `great-white-shark-blendswap/`, and `rigged-archer-fish-blendswap/`; the rejected Barramundi research GLB is under `barramundi-fish/`.
- If machine-specific external paths are needed, record them in ignored `tools/local.env`; do not commit personal absolute paths or account/download tokens.

Before copying a private file, verify `git check-ignore -v artifacts/assets/source/<asset>/<filename>` and `git check-ignore -v game/assets/vendor-local/<filename>`. Check `git ls-files` and the staged diff before any checkpoint. Keep originals and exact included license/notice files available for later preparation and packaging review.

## Starter gap and preparation

1. The owner-provided Quaternius pack is a deliberate low-poly P1 prototype choice. Its source ZIP and included CC0 1.0 license are retained outside Godot's import tree. Godot 4.7.2 imports `Fish3` / `ClownFish` as a skinned mesh with eight bones, six skin binds, three opaque material surfaces and a rotation-only `Armature|Swim` clip. The runtime visual scale is 0.12 with a 90° Y facing correction; the imported armature contains its own -90° X/100× transform. The clip is nonlooping in the source import and is replayed at its end. P5/P9 still need to assess the seam and deformation. Other pack fish are decorative only; final realistic art remains open.
2. The first habitat uses the acquired Smithsonian *Heliopora coerulea* coral alongside Rock Face 01 and Coast Sand 01. Godot imports the coral mesh/material and the rock glTF with its external binary and maps. The Smithsonian scan is one approximately 100k-triangle specimen; P4/P5 must assess collision and instance cost. This P3 import check makes no native quality claim.
3. Sea life's `.blend` includes packed images with Poliigon product names and stored paths. Its creator's CC0 listing does not establish rights to redistribute those third-party images. Keep the original and derivatives with those images ignored; isolate individual geometry and replace its materials with independently licensed content only after provenance review. Great White Shark is a later predator candidate; Rigged Archer Fish is a freshwater/brackish estuary species, not the selected ocean fish. Do not execute its embedded `rig_ui.py.001` as gameplay code.

## Local Godot installation and current prototype

`StarterWorld` in `game/scripts/StarterWorld.cs` loads prepared files by checked paths. The local P1 prototype installs them as follows; keep the source archive and these working copies ignored:

| Source under `artifacts/assets/source/` | Working path under `game/assets/vendor-local/` |
| --- | --- |
| `quaternius-animated-fish/*.zip`, `FBX/Fish1.fbx` through `Fish3.fbx`, plus `Manta ray`, `Dolphin`, `Shark`, `Whale` from inside the ZIP | `quaternius/<model>.fbx` for all seven models |
| `rock_face_01-2k/rock_face_01_2k.gltf`, `.bin`, and `textures/*.jpg` | `rock_face_01/` with the same relative layout |
| `coast_sand_01-2k/*.jpg` | `coast_sand_01/` with original filenames |
| `heliopora-coerulea/usnm_1145291-100k-2048_std.glb` | `heliopora-coerulea.glb` |

Copy the source files unchanged, extracting only the listed FBX members from the owner ZIP. Preserve Rock Face 01's `rock_face_01.bin` and all three `textures/` files beside its glTF in the same relative layout. Do not extract the ZIP into tracked scene/resource files. Run `./tools/dev import` after installing them, then `./tools/dev check` for a headless import/build/main-scene check. `./tools/dev run --display-mode=windowed-fullscreen` launches a local visual session. With Fish3, rock and its dependencies, the three sand maps and coral present, the runtime hides the original placeholder fish/markers and installs the imported Fish3 as player visual, seven pack fish as stationary decorative models with repeating source swim poses, two rocks, two corals and a textured floor. Fish movement still comes from the P0 `FishController`; background models have no ecology or collision behavior.

If a required source is absent, the game reports `FISH_ASSET_MISSING` with its item/path/preparation action, displays a help-layer diagnostic while leaving the P1 pause shell usable, and makes `--smoke` fail with exit 1. A fresh clone needs the four original packages listed in the manifest: use the listed official source for each public CC0 item or obtain the unchanged owner ZIP for the fish pack. Keep its included `License.txt`. Godot's cached import is insufficient without the source file: the loader checks both `FileAccess.FileExists` and `ResourceLoader.Exists`, including the rock's external binary and textures. A present but invalid import reports `FISH_ASSET_INVALID` with the affected item and reimport action. An editor play preview confirmed visible placement after rotating the imported fish visuals to face the camera side-on; animation seam, native display behavior and performance have not been qualified for P5.
