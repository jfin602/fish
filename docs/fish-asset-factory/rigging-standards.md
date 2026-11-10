# Fish rigging standards

A fish rig serves game deformation and clean GLB export. Species may need different bone counts and motion distributions; the names below are role conventions, not a universal skeleton template.

| Role | Convention |
| --- | --- |
| Game root | `root` for the stable transform; gameplay translation/heading remains in Godot. |
| Body and spine | `spine_01`, `spine_02`, ... from front to rear, as anatomy and topology require. |
| Tail | `tail_01`, `tail_02`, ... with increasing flexibility toward `tail_fin`. |
| Optional fins/jaw | `pectoral_l`, `pectoral_r`, `dorsal`, `anal`, `jaw` when the species and animation need them. |

Use predictable lowercase names and a clean export armature named by [FAF convention](asset-ids.md). Prefer deterministic skin weights, with bilateral mirroring where appropriate and deliberate cleanup at fin roots. Keep the head and eyes stable; do not let a body wave distort them accidentally. Test skinning in extreme useful swim and turn poses, including both sides. Export only deform bones and required hierarchy; keep Blender-only IK/widgets/control rigs out of the runtime armature. Document any species-specific deviations in the candidate record.
