# Modeling standards

These are working construction rules for original FAF assets. The [visual contract](art-style.md) decides whether the result belongs in Fish.

- Model in Blender real-world meters. Document the intended physical dimensions and verify them in Godot; never silently use scale to hide a mismatched source size.
- Author a consistent forward/up orientation: fish face Blender **-Y**, dorsal side **+Z**, left/right across **X**. Godot scene integration must verify the imported forward axis and placement rather than assuming Blender and Godot axes coincide. Static assets use +Z up and a documented front when relevant.
- Place a fish's origin at its main body/root pivot near the center of mass; static props use a useful ground/contact pivot, and terrain chunks use a repeatable tile anchor. Keep object transforms applied before export (location at the intended pivot, rotation zero, scale one); preserve rig bind/rest relationships when applying transforms.
- Name objects per [asset IDs](asset-ids.md). Remove hidden construction debris and unused datablocks from the export selection.
- Spend geometry on silhouette, joints and meaningful broad form. Avoid modeled scales, photographic micro-detail and needless subdivisions. Fish need enough longitudinal loops and clean bilateral topology to deform without collapsing; intentional fin roots and tail transitions matter more than polygon count alone.
- Use bilateral symmetry where anatomy calls for it. Allow controlled asymmetry in natural coral and rocks. Coral is chunky/faceted, plants are graphic blades/fronds, rocks strongly faceted, and terrain broad polygonal forms; fish are organic low-poly and somewhat smoother around deforming joints.
- Prefer reusable modules where repetition helps: fin construction, coral branches/plates, boulder shapes, vegetation patches and terrain chunks. Vary proportions deliberately so instances do not look stamped.

Keep collision design in Godot separate from detailed display meshes. Check [working budgets](budgets.md) and actual density at the intended camera distance.
