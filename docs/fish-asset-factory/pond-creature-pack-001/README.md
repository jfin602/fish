# Pond FAF creature pack 001

**Status:** Three original, editable Pond creature bases and their GLBs passed focused Blender, GLB and Godot 4.7.2 Forward+ import checks on 2026-11-11. Native visual review is a candidate review; Pond art style owner lock and live-world placement remain open. This isolated pack adds no feeding, attacks, damage, predator AI, player selection or normal-play wiring.

## Inventory and measured cost

All dimensions are Blender X/Y/Z metres; Blender front is -Y and up +Z, imported in Godot as -Z/+Y. Object transforms are identity at the centred body pivot. Each export has `faf_habitat=pond`, its stable `faf_id` and scientific species in node extras. See [the manifest](../../../assets/faf/pond/creatures/pond_creature_001_manifest.json) for per-object paths, bones, clips, measured geometry and review evidence.

| ID | Representative species | Dimensions m | Triangles | Materials | Bones | Source / GLB |
| --- | --- | ---: | ---: | ---: | ---: | --- |
| FAF-CREATURE-001 | Northern leopard frog, *Lithobates pipiens* | .1915 × .2500 × .0795 | 700 | 5 | 15 | `assets/faf/pond/creatures/creature_001_northern_leopard_frog.blend` / `game/assets/faf/pond/creatures/creature_001_northern_leopard_frog.glb` |
| FAF-CREATURE-002 | Northern water snake, *Nerodia sipedon* | .0940 × .9733 × .0894 | 1,040 | 4 | 12 | `assets/faf/pond/creatures/creature_002_northern_water_snake.blend` / `game/assets/faf/pond/creatures/creature_002_northern_water_snake.glb` |
| FAF-CREATURE-003 | Common snapping turtle, *Chelydra serpentina* | .4285 × .7502 × .2208 | 838 | 5 | 14 | `assets/faf/pond/creatures/creature_003_common_snapping_turtle.blend` / `game/assets/faf/pond/creatures/creature_003_common_snapping_turtle.glb` |

The frog has a pear-shaped spotted body, pale dorsolateral folds, webbed feet and deliberately longer folded hind limbs. The snake has a broad head, alternating brown blotches, a pale ventral region and a continuously tapered body. The turtle has a rigid broad carapace, reduced plastron, pointed beak, stout clawed limbs, serrated shell rear and long tapered tail. The models share small mesh, skin and export helpers; their silhouettes and bone layouts are separate.

## Rig and Actions

- **Frog:** `root → body → head`, paired `fore_upper/lower/foot_L/R` and `hind_upper/lower/foot_L/R`. Looping `IdleShore`, `SurfaceFloat`, `Swim`, `TurnLeft`, `TurnRight`; one-shot `Hop`, `Dive`. Paired hind kicks and restrained forelimbs move the skinned legs and feet. `Hop` returns to the crouch; `Dive` finishes in a lowered pose. Neither moves the gameplay root.
- **Snake:** `root → spine_01…10 → head`. The tube has adjacent-bone skin blending and a caudally increasing traveling lateral wave. Looping `IdleFloat`, `SurfaceSwim`, `UnderwaterSwim`, `ShoreCrawl`, `TurnLeft`, `TurnRight`; one-shot `Dive`. Turns add a temporary directional curve without translating `root`.
- **Turtle:** `root → body`, then `neck → head`, four paired upper/lower limb chains and `tail_01 → tail_02`. The carapace is rigidly skinned to `body`; neck, legs and tail deform. Looping `IdleBottom`, `BottomWalk`, `SurfaceFloat`, `Swim`, `TurnLeft`, `TurnRight`; one-shot `Dive`. Bottom walk alternates diagonal limbs; swim paddles them with lower amplitude. The shell does not flex.

The imported Actions are named `<stem>__<role>`. Godot `.glb.import` sidecars set loop modes from the manifest. Blender's NLA exporter samples constant `root` channels even though no root keys are authored; the GLB checker verifies those sampled values never change, and the Godot checker verifies the root pose remains fixed.

## Native proof and observations

The isolated [Forward+ proof scene](../../../game/scenes/faf_pond_creature_proof.tscn) has a sloped mud bank, a transparent reference waterline, a submerged bottom, olive fog and filtered warm top light. It can switch animal, action, side/top/three-quarter view and close/3.8 m camera distance. The creature is placed on the bank, at the surface or underwater as appropriate to its selected action. Screenshots in [evidence](evidence/) were captured from the actual Godot renderer at 1920 × 1200, not a neutral Blender render.

| Animal | Native views and observed motion | Limits visible in evidence |
| --- | --- | --- |
| Frog | [side](evidence/frog-side.png), [top](evidence/frog-top.png), [three-quarter](evidence/frog-three-quarter.png), [swim](evidence/frog-swim.png), [kick A](evidence/frog-motion-a.png)/[B](evidence/frog-motion-b.png), [Hop](evidence/frog-hop.png), [Dive](evidence/frog-dive.png), [3.8 m](evidence/frog-game.png). Spots, folds, long hind limbs and kicking remain identifiable close. | At 3.8 m, the frog is small and its feet/markings disappear. Bank and surface context is sparse. |
| Snake | [side](evidence/snake-side.png), [top](evidence/snake-top.png), [shore](evidence/snake-three-quarter.png), [underwater](evidence/snake-underwater.png), [wave A](evidence/snake-motion-a.png)/[B](evidence/snake-motion-b.png), [turn](evidence/snake-turn.png), [Dive](evidence/snake-dive.png), [3.8 m](evidence/snake-game.png). Banded taper stays connected through turns. | Head detail is subdued at game distance; the body needs nearby contrast against dark mud. |
| Turtle | [side](evidence/turtle-side.png), [top](evidence/turtle-top.png), [swim](evidence/turtle-three-quarter.png), [paddle A](evidence/turtle-motion-a.png)/[B](evidence/turtle-motion-b.png), [walk](evidence/turtle-walk.png), [Dive](evidence/turtle-dive.png), [3.8 m](evidence/turtle-game.png). Rigid shell, beak, four limbs and tail read in native views; limbs clear shell. | Shell scutes and claws are deliberately simplified; body appearance still needs review in a richer Pond composition. |

The initial snake generator had a radius discontinuity near the tail; native side review exposed it, and the profile was corrected before final export. A first set of raised turtle shell ridges appeared detached in native review; they were removed. The final records and screenshots use those corrections. No size or color variants were made before the three bases passed.

## Focused checks

- `blender -b -t 2 --python tools/asset-factory/blender/creatures/build_pond_creatures.py -- --root "$PWD"` generates the three masters, GLBs and manifest. `--overwrite` is guarded to these owned Pond outputs only.
- `blender -b -t 2 --python tools/asset-factory/blender/creatures/check_pond_creatures.py -- --root "$PWD"` evaluated every source Action's skinned mesh. All 21 clips changed visible mesh points. The sampled snake swim and turn bends kept non-adjacent body-ring centres at least .142 m apart and adjacent centreline segment length at .982–1.079 times rest. Turtle foot centres remained at least .165 m lateral from the shell centre in sampled locomotion and dive poses.
- `python3 tools/asset-factory/blender/creatures/check_pond_creature_exports.py --root "$PWD"` checked GLB headers, Pond and species metadata, skin attributes, bones, materials, exact Action names and constant root channels.
- `tools/dev import` followed by `Godot --headless --path game --script res://dev/faf_pond_creature_check.gd` verified all three actual imports: dimensions, triangles, materials, skinned meshes, bone names, Action lists, loop modes, fixed root, animated deform controls, clean loop endpoints and one-shot stop after playback. Godot 4.7.2 .NET, Forward+ renderer. The proof scene was run natively with Vulkan on AMD Radeon 880M and captured side/top/three-quarter/movement and 3.8 m views.

The validator samples selected frames and body-centre distances; it does not prove every possible animation blend, collision state or future gameplay camera path. The isolated fixture's bank and waterline are presentation reference geometry, not authored Pond terrain or collision. No normal-play integration has been attempted.

## Anatomy and motion references

Original geometry was built from species descriptions, without third-party mesh or texture assets. The [Animal Diversity Web northern leopard frog account](https://animaldiversity.org/accounts/Lithobates_pipiens/) supports the spotted green/brown dorsum and dorsolateral folds; [published frog-swimming research](https://www.nature.com/articles/420621a) supports paired hindlimb propulsion. The [Animal Diversity Web northern water snake account](https://animaldiversity.org/accounts/Nerodia_sipedon/) supports its banding, size and aquatic/shore habitat; [research on swimming *N. sipedon*](https://pubmed.ncbi.nlm.nih.gov/17993267/) documents body-form changes, and [a related *Nerodia* study](https://pubmed.ncbi.nlm.nih.gov/3184194/) supports the modeled posteriorly propagating lateral wave. The [Illinois Natural History Survey snapping turtle account](https://herpetology.inhs.illinois.edu/resources/species-lists/ilspecies/snapping-turtle/) supports the rough broad shell, large head, exposed limbs and long tail. The owner-supplied `best-pond-faf.png` was used for faceting, earthy color and lighting direction only; it was not used as a texture or redistributable geometry source.
