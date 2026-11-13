"""Build the isolated Pond stealth-cover pack (FAF-PLANT-010–014).

blender -b --python tools/asset-factory/blender/plants/build_pond_stealth_vegetation_001.py -- --root "$PWD"
"""

from __future__ import annotations

import argparse
from dataclasses import asdict
import json
from pathlib import Path
import sys

import bpy

TOOLING = Path(__file__).resolve().parents[1]
if str(TOOLING) not in sys.path:
    sys.path.insert(0, str(TOOLING))

from common.export import export_selected_glb
from common.validation import inspect_objects
from plants.build_pond_foliage_001 import bounds, create_object
from plants.pond_stealth_vegetation import (
    RibbonGrassSpec, BranchingWeedSpec, CoontailSpec, PondweedSpec,
    ThicketSpec, GENERATORS,
)


# The three grass records are actual short/medium/near-surface constructions.
RECIPES = [
    ("010", "A", "ribbon_grass", RibbonGrassSpec(.84, 18, .27, .052, .20, 7101), "short grass; 0.5–1 m shallows", "olive"),
    ("010", "B", "ribbon_grass", RibbonGrassSpec(1.82, 27, .40, .064, .38, 7102), "medium grass; 1–2 m water", "moss"),
    ("010", "C", "ribbon_grass", RibbonGrassSpec(3.58, 37, .53, .072, .67, 7103), "tall near-surface grass; mud depth 3.2–3.7 m", "olive"),
    ("011", "A", "branching_waterweed", BranchingWeedSpec(1.58, .48, 5, 3, 2, 7201), "narrow upright mid-water", "deep_moss"),
    ("011", "B", "branching_waterweed", BranchingWeedSpec(1.25, .82, 6, 4, 2, 7202), "medium spreading mid-water", "moss"),
    ("011", "C", "branching_waterweed", BranchingWeedSpec(1.55, 1.12, 8, 4, 3, 7203), "broad porous mass; intermediate depth", "olive"),
    ("012", "A", "coontail", CoontailSpec(.82, .62, 5, 5, .30, 7301), "compact small-fish cover", "deep_moss"),
    ("012", "B", "coontail", CoontailSpec(1.58, .83, 6, 7, .36, 7302), "tall irregular small-fish cover", "moss"),
    ("012", "C", "coontail", CoontailSpec(.76, 1.36, 7, 5, .88, 7303), "horizontal bottom growth", "deep_moss"),
    ("013", "A", "broad_pondweed", PondweedSpec(1.12, .65, 5, 3, .18, 7401), "shallow transition and log edge", "yellow_olive"),
    ("013", "B", "broad_pondweed", PondweedSpec(1.66, .90, 7, 4, .23, 7402), "open intermediate-depth leaves", "olive"),
    ("014", "A", "rooted_thicket", ThicketSpec(1.32, 1.05, 6, 6, 3, 7501), "rooted hide-behind brush near wood", "moss"),
    ("014", "B", "rooted_thicket", ThicketSpec(1.88, 1.48, 8, 8, 4, 7502), "large porous hide-inside brush", "deep_moss"),
]

COLORS = {"olive": "626F35", "moss": "4A6034", "deep_moss": "314C32",
          "yellow_olive": "8A943F", "brown": "57472F"}


def material(swatch):
    name = "mat__pond_cover_" + swatch
    mat = bpy.data.materials.get(name)
    if mat:
        return mat
    mat = bpy.data.materials.new(name)
    mat.use_nodes = True
    h = COLORS[swatch]
    srgb = tuple(int(h[i:i + 2], 16) / 255 for i in (0, 2, 4))
    linear = tuple(((v + .055) / 1.055)**2.4 if v > .04045 else v / 12.92 for v in srgb)
    mat.diffuse_color = (*linear, 1)
    shader = mat.node_tree.nodes.get("Principled BSDF")
    shader.inputs["Base Color"].default_value = (*linear, 1)
    shader.inputs["Roughness"].default_value = .91
    shader.inputs["Metallic"].default_value = 0
    return mat


def build(root: Path, overwrite=False):
    source = root / "assets/faf/pond/plants/pond_stealth_vegetation_001.blend"
    manifest = source.with_name("pond_stealth_vegetation_001_manifest.json")
    output = root / "game/assets/faf/pond/plants"
    stems = [f"plant_{num}{letter.lower()}_{family}" for num, letter, family, *_ in RECIPES]
    paths = [output / (stem + ".glb") for stem in stems]
    if not overwrite and any(p.exists() for p in [source, manifest, *paths]):
        raise FileExistsError("Vegetation outputs exist; use --overwrite deliberately")
    source.parent.mkdir(parents=True, exist_ok=True)
    output.mkdir(parents=True, exist_ok=True)
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.object.delete(use_global=False)
    for obj in list(bpy.data.objects):
        owned = obj.name.removeprefix("review__")
        if any(owned.startswith(f"plant_{number}") for number in ("010", "011", "012", "013", "014")):
            bpy.data.objects.remove(obj, do_unlink=True)
    for mesh in list(bpy.data.meshes):
        if any(mesh.name.startswith(f"plant_{number}") for number in ("010", "011", "012", "013", "014")):
            bpy.data.meshes.remove(mesh)
    for collection in list(bpy.data.collections):
        if collection.name.startswith("FAF-PLANT-01") or collection.name.startswith("Family Review"):
            bpy.data.collections.remove(collection)
    bpy.context.scene.name = "FAF Pond Stealth Vegetation 001"
    review = bpy.data.collections.new("Family Review — copies only")
    bpy.context.scene.collection.children.link(review)
    rows = {"ribbon_grass": 0, "branching_waterweed": 3.0,
            "coontail": 6.0, "broad_pondweed": 9.0, "rooted_thicket": 12.0}
    reports = []
    for num, letter, family, spec, zone, swatch in RECIPES:
        faf_id = f"FAF-PLANT-{num}{letter}"
        stem = f"plant_{num}{letter.lower()}_{family}"
        collection = bpy.data.collections.new(f"{faf_id} {family} source")
        bpy.context.scene.collection.children.link(collection)
        parts = GENERATORS[family](spec)
        objects = []
        for label, geometry in parts.items():
            role_swatch = "brown" if label == "roots_and_shoots" else swatch
            obj = create_object(stem + "__" + label, geometry, material(role_swatch),
                                collection, faf_id, family, spec.seed, "mud_root")
            objects.append(obj)
        check = inspect_objects(objects)
        if check["problems"]:
            raise RuntimeError(f"{faf_id}: {check['problems']}")
        dimensions, low, high = bounds(objects)
        if abs(low[2]) > .035:
            raise RuntimeError(f"{faf_id}: bad root datum {low[2]}")
        bpy.ops.object.select_all(action="DESELECT")
        for obj in objects:
            obj.select_set(True)
        bpy.context.view_layer.objects.active = objects[0]
        export_selected_glb(str(output / (stem + ".glb")), include_animations=False,
                            overwrite=overwrite, export_metadata=True)
        # Source collection keeps every editable part at the local mud pivot.
        # Review copies are for side-by-side inspection and never enter an export.
        offset = (3.0 * (ord(letter)-ord("A")), rows[family], 0)
        for obj in objects:
            obj.select_set(False)
            copy = obj.copy()
            copy.data = obj.data
            copy.name = "review__" + obj.name
            copy.location = offset
            review.objects.link(copy)
            obj.hide_set(True)
        reports.append({
            "faf_id":faf_id, "habitat":"pond", "family":family, "variant":letter,
            "generator":f"plants.pond_stealth_vegetation.{family}.v1",
            "seed":spec.seed, "spec":asdict(spec), "intended_zone":zone,
            "cover_role":"soft visual concealment; no automatic gameplay hiding",
            "collision":"none (swimmable soft cover)",
            "dimensions_m_blender_xyz":dimensions, "bounds_min_m_blender_xyz":low,
            "bounds_max_m_blender_xyz":high,
            "triangles":check["triangle_count_source"], "materials":check["material_count"],
            "components":list(parts), "palette":(["brown",swatch] if family == "rooted_thicket" else [swatch]),
            "origin_m":[0,0,0], "rotation_rad":[0,0,0], "scale":[1,1,1],
            "placement":{"anchor":"mud_root", "root_height_m":0,
                         "clearance":"maintain open fish/camera lanes between clusters"},
            "sway_coordinate":"UV.y root=0 tip=1; roots and thick stems remain fixed",
            "source":source.relative_to(root).as_posix(),
            "glb":(output/(stem+".glb")).relative_to(root).as_posix(),
            "technical_validation_state":"generated; observed checks in pack candidate record",
            "style_review_state":"candidate; observed views in pack candidate record; owner lock open",
        })
    manifest.write_text(json.dumps({"pack":"pond_stealth_vegetation_001",
                                    "habitat":"pond","asset_count":len(reports),
                                    "source":source.relative_to(root).as_posix(),
                                    "assets":reports},indent=2)+"\n")
    bpy.ops.wm.save_as_mainfile(filepath=str(source))
    return {"assets":len(reports),"triangles":sum(x["triangles"] for x in reports),
            "source":str(source),"manifest":str(manifest)}


if __name__ == "__main__":
    args = sys.argv[sys.argv.index("--")+1:] if "--" in sys.argv else []
    parser = argparse.ArgumentParser()
    parser.add_argument("--root",type=Path,required=True)
    parser.add_argument("--overwrite",action="store_true")
    options = parser.parse_args(args)
    print("POND_STEALTH_VEGETATION_REPORT=" + json.dumps(build(options.root.resolve(),options.overwrite)))
