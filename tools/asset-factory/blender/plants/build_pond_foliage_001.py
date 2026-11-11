"""Build 13 Pond-only FAF foliage GLBs, one editable master, and a measured manifest.

blender -b --python tools/asset-factory/blender/plants/build_pond_foliage_001.py -- --root "$PWD"
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

from common.export import export_selected_glb  # noqa: E402
from common.validation import inspect_objects  # noqa: E402
from plants.pond_foliage import (  # noqa: E402
    LilySpec, GrassSpec, CattailSpec, ReedSpec, AlgaeSpec, MuckSpec, GENERATORS,
)


# Family IDs are reserved only for Pond; the existing Ocean PLANT-001–003 remain intact.
RECIPES = [
    ("004", "A", "lily_pad", LilySpec(0.46, 0.72, 0.27, 0.10, 6401), "olive", "stem_brown"),
    ("004", "B", "lily_pad", LilySpec(0.62, 1.08, 0.38, 0.17, 6402), "yellow_olive", "stem_brown"),
    ("004", "C", "lily_pad", LilySpec(0.77, 1.48, 0.33, 0.24, 6403), "moss", "stem_brown"),
    ("005", "A", "tall_grass", GrassSpec(0.96, 8, 0.40, 0.068, 0.24, 6501), "olive", None),
    ("005", "B", "tall_grass", GrassSpec(1.24, 11, 0.52, 0.057, 0.32, 6502), "yellow_olive", None),
    ("006", "A", "cattail", CattailSpec(1.42, 3, 0.43, 0.24, 6601), "moss", "seed_brown"),
    ("006", "B", "cattail", CattailSpec(1.76, 4, 0.55, 0.30, 6602), "olive", "seed_brown"),
    ("007", "A", "reed", ReedSpec(1.38, 6, 0.47, 0.24, 6701), "yellow_olive", None),
    ("007", "B", "reed", ReedSpec(1.85, 8, 0.58, 0.32, 6702), "moss", None),
    ("008", "A", "submerged_algae", AlgaeSpec(0.43, 0.20, 5, 6801), "deep_moss", None),
    ("008", "B", "submerged_algae", AlgaeSpec(0.64, 0.29, 7, 6802), "moss", None),
    ("009", "A", "floating_muck", MuckSpec(0.78, 5, 0.43, 6901), "olive", None),
    ("009", "B", "floating_muck", MuckSpec(1.12, 7, 0.50, 6902), "deep_moss", None),
]

COLORS = {
    "olive": "676F2E", "yellow_olive": "90903B", "moss": "4F602C",
    "deep_moss": "344A29", "stem_brown": "5D4A30", "seed_brown": "65472C",
}


def material(swatch):
    name = "mat__pond_" + swatch
    existing = bpy.data.materials.get(name)
    if existing:
        return existing
    mat = bpy.data.materials.new(name)
    mat.use_nodes = True
    h = COLORS[swatch]
    srgb = tuple(int(h[i:i + 2], 16) / 255 for i in (0, 2, 4))
    linear = tuple(((v + 0.055) / 1.055) ** 2.4 if v > 0.04045 else v / 12.92 for v in srgb)
    mat.diffuse_color = (*linear, 1)
    bsdf = mat.node_tree.nodes.get("Principled BSDF")
    bsdf.inputs["Base Color"].default_value = (*linear, 1)
    bsdf.inputs["Roughness"].default_value = 0.92
    bsdf.inputs["Metallic"].default_value = 0
    return mat


def create_object(name, part, mat, collection, faf_id, family, seed, anchor):
    mesh = bpy.data.meshes.new(name)
    mesh.from_pydata(part.vertices, [], part.faces)
    mesh.update(calc_edges=True)
    mesh.materials.append(mat)
    uv = mesh.uv_layers.new(name="rooted_sway_uv")
    for poly in mesh.polygons:
        poly.use_smooth = False
        for loop in poly.loop_indices:
            uv.data[loop].uv = part.uv[mesh.loops[loop].vertex_index]
    obj = bpy.data.objects.new(name, mesh)
    collection.objects.link(obj)
    obj["faf_id"] = faf_id
    obj["faf_habitat"] = "pond"
    obj["faf_family"] = family
    obj["faf_seed"] = seed
    obj["faf_anchor"] = anchor
    obj["sway_mask"] = "UV.y: root=0, tip=1; visual-only"
    return obj


def bounds(objects):
    verts = [tuple(v.co) for obj in objects for v in obj.data.vertices]
    low = [min(v[i] for v in verts) for i in range(3)]
    high = [max(v[i] for v in verts) for i in range(3)]
    return [round(high[i] - low[i], 5) for i in range(3)], [round(v, 5) for v in low], [round(v, 5) for v in high]


def placement(family, spec):
    if family == "lily_pad":
        return {"anchor": "water_surface", "origin_height_m": 0,
                "mud_depth_m": spec.stem_depth, "clearance": "stem only below pad; keep route center open"}
    if family == "tall_grass":
        return {"anchor": "dry_or_damp_bank", "origin_height_m": 0,
                "clearance": "bank edge; do not place in main swim lane"}
    if family in ("cattail", "reed"):
        return {"anchor": "shallow_mud", "origin_height_m": 0,
                "clearance": "cluster along banks, leave underwater route open"}
    if family == "submerged_algae":
        return {"anchor": "substrate_contact", "origin_height_m": 0,
                "clearance": "attach local +Z to ground, log, or rock face"}
    return {"anchor": "water_surface", "origin_height_m": 0,
            "clearance": "small separated patch with open water around it"}


def build(root, overwrite=False):
    source = root / "assets/faf/pond/plants/pond_foliage_001.blend"
    manifest = source.with_name("pond_foliage_001_manifest.json")
    output = root / "game/assets/faf/pond/plants"
    if not overwrite and (source.exists() or manifest.exists() or any(output.glob("*.glb"))):
        raise FileExistsError("Pond foliage outputs exist; pass --overwrite deliberately")
    output.mkdir(parents=True, exist_ok=True)
    source.parent.mkdir(parents=True, exist_ok=True)
    bpy.ops.wm.read_factory_settings(use_empty=True)
    scene = bpy.context.scene
    scene.name = "FAF Pond Foliage 001 - Source and Family Review"
    scene.unit_settings.system = "METRIC"
    scene.unit_settings.scale_length = 1
    reports = []
    review = bpy.data.collections.new("Family Review - above waterline")
    scene.collection.children.link(review)
    row = {"lily_pad": 0, "tall_grass": 2.2, "cattail": 4.2,
           "reed": 6.2, "submerged_algae": 8.2, "floating_muck": 10.2}
    for index, (number, letter, family, spec, swatch, accent) in enumerate(RECIPES):
        faf_id = f"FAF-PLANT-{number}{letter}"
        stem = f"plant_{number}{letter.lower()}_{family}"
        collection = bpy.data.collections.new(f"{faf_id} Source")
        scene.collection.children.link(collection)
        parts = GENERATORS[family](spec)
        objects = []
        for label, geometry in parts.items():
            part_swatch = (accent if label in ("stem", "seed_heads") else swatch)
            obj = create_object(stem + "__" + label, geometry, material(part_swatch),
                                collection, faf_id, family, spec.seed, placement(family, spec)["anchor"])
            objects.append(obj)
        report = inspect_objects(objects)
        if report["problems"] or report["triangle_count_source"] > 500:
            raise RuntimeError(f"{faf_id} failed source validation: {report}")
        dims, low, high = bounds(objects)
        if family == "lily_pad":
            if abs(low[2] + spec.stem_depth) > 0.001 or abs(high[2]) > 0.05:
                raise RuntimeError(f"{faf_id} waterline/stem mismatch: {low}, {high}")
        destination = output / f"{stem}.glb"
        bpy.ops.object.select_all(action="DESELECT")
        for obj in objects:
            obj.select_set(True)
        bpy.context.view_layer.objects.active = objects[0]
        export_selected_glb(str(destination), include_animations=False,
                            overwrite=overwrite, export_metadata=True)
        for obj in objects:
            obj.select_set(False)
            clone = obj.copy()
            clone.data = obj.data
            clone.name = "review__" + obj.name
            clone.location = ((ord(letter) - ord("B")) * 1.8, row[family],
                              spec.stem_depth if family == "lily_pad" else 0)
            review.objects.link(clone)
            obj.hide_set(True)
        reports.append({
            "faf_id": faf_id, "habitat": "pond", "family": family,
            "variant": letter, "generator": f"plants.pond_foliage.{family}.v1",
            "seed": spec.seed, "spec": asdict(spec),
            "dimensions_m_blender_xyz": dims, "bounds_min_m_blender_xyz": low,
            "bounds_max_m_blender_xyz": high,
            "triangles": report["triangle_count_source"],
            "materials": report["material_count"],
            "components": list(parts.keys()),
            "palette": [swatch] + ([accent] if accent else []),
            "origin_m": [0, 0, 0], "rotation_rad": [0, 0, 0], "scale": [1, 1, 1],
            "placement": placement(family, spec),
            "sway_coordinate": "UV.y, root=0, tip=1; visual-only shader-ready",
            "source": source.relative_to(root).as_posix(),
            "glb": destination.relative_to(root).as_posix(),
            "review_status": "generated; native review pending",
        })
    manifest.write_text(json.dumps({"pack": "pond_foliage_001", "habitat": "pond",
                                    "asset_count": len(reports), "source": source.relative_to(root).as_posix(),
                                    "assets": reports}, indent=2) + "\n")
    bpy.ops.wm.save_as_mainfile(filepath=str(source))
    return reports


if __name__ == "__main__":
    args = sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else []
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path, required=True)
    parser.add_argument("--overwrite", action="store_true")
    options = parser.parse_args(args)
    print("POND_FOLIAGE_REPORT=" + json.dumps(build(options.root.resolve(), options.overwrite)))
