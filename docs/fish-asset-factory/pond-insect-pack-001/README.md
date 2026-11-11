# Pond FAF insect pack 001

## Group selection before modeling

The submerged family is the **common green darner dragonfly nymph, *Anax junius***. Both variants share a broad head, six bent walking legs, paired dorsal wing pads, and a tapered segmented abdomen. A is a compact resting nymph; B is longer with wider-set legs and larger wing pads. The larva's animation flexes the abdomen and legs while the head remains stable. It lives just above shallow silt, submerged wood, and plant roots.

The surface/swimming family is the **backswimmer, *Notonecta undulata***. Both variants share a slim boat-shaped body, large lateral eyes, six legs, and a long pair of rowing hind legs. A has a fuller oval abdomen and compact oars; B is slimmer with wider paddle reach. They rest inverted just under the water film, with the abdominal tip near it, and the oars row in a presentation-only loop.

These are original ambient geometry, with no prey, feeding, attack, damage, or collision role. The owner's `/home/jfin/best-pond-faf.png` controls the faceted earthy style, not topology or texture. The chosen morphology and posture are supported by [University of Michigan's green darner account](https://www.biokids.umich.edu/critters/Anax_junius/), [Florida Atlantic University's green darner life-stage guide](https://www.ces.fau.edu/education/resources/pdfs/green-darner-and-pondhawk-dragonfly.pdf), [University of Minnesota's aquatic Hemiptera guide](https://midge.cfans.umn.edu/sites/midge.cfans.umn.edu/files/files/media/07hemiptera.pdf), and [Texas A&M's *N. undulata* record](https://elp.tamu.edu/ipm/bugs/true-bugs/hemiptera-heteroptera-notonectidae-notonecta-undulata-backswimmers-a-3/). Feeding observations in those references do not imply gameplay behavior here.

The new family identities are reserved as **FAF-INSECT-001** (A/B larva) and **FAF-INSECT-002** (A/B backswimmer). Every export is Pond-only. Technical measurements and native review follow after generation.

## Delivered inventory and cost

Dimensions are bounding-box X/Y/Z metres in Blender. The backswimmer's X span includes its long oars; body length runs along Y. All four meshes are joined by variant for import efficiency while preserving distinct weighted limbs and abdomen bones. Material regions are opaque, rough, and texture-free.

| ID | Variant shape | Dimensions m | Triangles | Materials | Bones | Action |
| --- | --- | ---: | ---: | ---: | ---: | --- |
| FAF-INSECT-001A | Compact darner nymph | .0370 × .0407 × .0325 | 468 | 3 | 11 | `Crawl` |
| FAF-INSECT-001B | Longer, wider-legged darner nymph | .0469 × .0504 × .0323 | 468 | 3 | 11 | `Crawl` |
| FAF-INSECT-002A | Fuller backswimmer, shorter oars | .0323 × .0140 × .0208 | 396 | 4 | 10 | `Row` |
| FAF-INSECT-002B | Slimmer backswimmer, longer oars | .0419 × .0158 × .0199 | 396 | 4 | 10 | `Row` |

The editable sources are `assets/faf/pond/insects/insect_*.blend`; individual GLBs are `game/assets/faf/pond/insects/insect_*.glb`. Exact paths, numeric generator inputs, zone, identity, orientation, Actions and review fields are in the [manifest](../../../assets/faf/pond/insects/pond_insect_001_manifest.json). The reproducible builder is [build_pond_insects.py](../../../tools/asset-factory/blender/insects/build_pond_insects.py); it has a guarded `--overwrite` path limited to its owned files. Each GLB's node extras include the full variant ID, family ID, species, Pond habitat, ambient-only role and placement zone. All variants have one skinned mesh and no gameplay collision.

The nymph rig has `root → thorax → head / abdomen_01 → abdomen_02` with six paired leg controls under thorax. The abdomen mesh blends across its two deform bones. The backswimmer has `root → body → head / abdomen` and paired foreleg, midleg and oar controls. `Crawl` alternates the nymph legs and flexes its rear abdomen; `Row` sweeps the backswimmer's oars. The head stays comparatively stable; neither Action moves the gameplay root. Both are seamless 60-frame presentation loops. Godot owns any future position/drift.

## Technical proof

Blender 5.2.3 LTS evaluated the saved masters at frames 1, 22 and 61. Maximum skinned point displacement was .0040–.0059 m, and first/last evaluated seam displacement was 0 m in all four. The GLB byte validator confirmed one skin, all expected joints, skin attributes, exact actions/materials, Pond/ambient metadata, fixed root channels and matching triangle totals. Godot 4.7.2 imported four skinned scenes, measured the same dimensions/triangles/materials and verified named bone movement, loop mode and the imported loop seam.

Run the focused checks from the repository root:

```sh
blender -b -t 2 --python tools/asset-factory/blender/insects/check_pond_insect_sources.py -- --root "$PWD"
python3 tools/asset-factory/blender/insects/check_pond_insect_exports.py --root "$PWD"
tools/dev import
godot --headless --path game --script res://dev/faf_pond_insect_check.gd
```

The [isolated Forward+ proof](../../../game/scenes/faf_pond_insect_proof.tscn) uses a single shallow bank, a waterline reference, existing Pond lily/reed/algae/wood context and an open lane. The four original GLBs stay ambient and are not registered in P4 normal play. Backswimmers are rolled inverted and pitched so the rowing legs face the surface film. The proof's 3.8 m/68° setting matches `SwimTuning`'s default camera distance and field of view; the close view is for anatomy review. The full 24-image matrix is saved as `evidence/matrix_<index>_<above|waterline|underwater>_<close|game>.png` for indices 0–3 in manifest order. Screenshots are actual Godot Forward+ Vulkan output on AMD Radeon 880M.

## Native visual review

- [Nymph A from above](evidence/nymph_a_above.png) and [nymph B underwater](evidence/nymph_b_underwater.png) show the broad heads, tapered segmented abdomens, wing pads and different leg spreads. The [two Crawl phases](evidence/nymph_crawl_a.png) / [second phase](evidence/nymph_crawl_b.png) show visible leg and abdomen movement. The nymphs are coherent with the Pond reference's broad faceting and ochre/olive palette.
- [Backswimmer A at the waterline](evidence/bug_a_waterline.png) and [B from above](evidence/bug_b_above.png) show inverted hulls and different oar reach. [Row phase A](evidence/bug_row_a.png) and [phase B](evidence/bug_row_b.png) show clear paddle sweeps. The geometry is readable close without transparency or photographic detail.
- At the [nymph](evidence/nymph_game.png) and [bug](evidence/bug_game.png) game-distance views, these physically small animals read as slight movement/marks rather than species silhouettes. This is the pack's main visual limitation, not an import failure. The [20-instance above](evidence/dense_above.png) and [underwater](evidence/dense_underwater.png) stress views make the group visible but begin to look busy and do not justify dense live placement.
- The sparse proof has four animated insects, 1,728 total source triangles and a sampled 27 scene draw calls; the 20-insect stress proof has 8,640 insect triangles and 83 sampled scene draw calls. Counts include existing proof context in Godot render monitors, so the draw-call figures are scene observations rather than insect-only draw counts. A practical starting point is two to four insects per shallow-bank local cluster, separated by quiet water and culled at distance.

The geometry and import are technically ready for later P4 intake. Individual species readability at the default fish camera and integration with the final Pond water treatment remain open for owner review. The fixture is a presentation proof, not an ecology, gameplay or P4 Gate pass. Blender MCP's open user session was left on its unrelated Yellow Tang scene; source deformation was checked in a separate Blender process and native appearance in Godot rather than changing that session.
