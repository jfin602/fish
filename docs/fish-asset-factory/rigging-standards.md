# Fish rigging standards

A fish rig serves game deformation and clean GLB export. Species may need different bone counts and motion distributions; the names below are role conventions, not a universal skeleton template.

| Role | Convention |
| --- | --- |
| Game root | `root` for the stable transform; gameplay translation/heading remains in Godot. |
| Body and spine | `spine_01`, `spine_02`, ... from front to rear, as anatomy and topology require. |
| Tail | `tail_01`, `tail_02`, ... with increasing flexibility toward `tail_fin`. |
| Optional fins/jaw | `pectoral_l`, `pectoral_r`, `dorsal`, `anal`, `jaw` when the species and animation need them. |
| Bony-fish breathing | `gill_L`, `gill_R` are required deform bones for ordinary operculate fish; the uppercase side suffix is the FAF breathing convention. |

Use predictable lowercase names and a clean export armature named by [FAF convention](asset-ids.md). Prefer deterministic skin weights, with bilateral mirroring where appropriate and deliberate cleanup at fin roots. Keep the head and eyes stable; do not let a body wave distort them accidentally. Test skinning in extreme useful swim and turn poses, including both sides. Export only deform bones and required hierarchy; keep Blender-only IK/widgets/control rigs out of the runtime armature. Document any species-specific deviations in the candidate record.

## Breathing anatomy is required at generation

Every fish blueprint must declare a breathing anatomy type before rigging. Ordinary bony fish use `operculate_bony`: add paired `gill_L` and `gill_R` to the export armature during the initial build, parent them to a stable head/front-body bone, then deterministically transfer only local surface weight behind each eye to the matching bone. Keep the eye meshes and snout rigid. Their neutral pose must reproduce the original source mesh, and the gill bones remain neutral in Glide, Swim and SwimFast. The bones provide independent left/right outward rotation without root movement. The current implementation uses `spine_01` as parent and local Z rotations with opposite signs on the two sides; use the per-species anatomical parameters and `opening_rotation_z` rather than assuming the same world-space sign for every model.

`exposed_gill_slits_pending` is the explicit current classification for the Blacktip Reef Shark. Do not assign it fake operculum bones. A future species-specific slit expansion method must be implemented and validated before this type can pass final breathing validation. An unknown or omitted classification fails. The current shark remains a documented technical candidate with breathing presentation pending; it is not falsely certified as breathing-complete.

Blender owns the deformation bones and weights. Godot will later own runtime breathing timing and intensity, which may respond to effort or other game state. This standard adds no Blender breathing Actions and no game translation. Before a bony fish is technically complete, validate both source and exported GLB: anatomy, paired deform bones, correct parent, localized bilateral weights, neutral mesh equivalence, clean skin, and exported joints. Review neutral, subtle left, subtle right, both, and strong safe poses from side, front and three-quarter views. A missing required breathing rig fails final FAF technical validation.
