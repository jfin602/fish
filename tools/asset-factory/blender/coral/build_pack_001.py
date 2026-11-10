"""Build the first twelve FAF coral variants in Blender.

Blender command: blender -b --python tools/asset-factory/blender/coral/build_pack_001.py -- --root REPO
The source scene contains origin-pivot meshes plus linked review copies. Exports
are one selected source mesh per GLB. Existing outputs require --overwrite.
"""

from __future__ import annotations

import argparse
import json
from pathlib import Path
import sys

import bpy

TOOLING = Path(__file__).resolve().parents[1]
if str(TOOLING) not in sys.path:
    sys.path.insert(0, str(TOOLING))

from common.export import export_selected_glb  # noqa: E402
from common.palette import hex_rgb  # noqa: E402
from common.validation import inspect_objects  # noqa: E402
from coral.families import (BranchingSpec, RoundedSpec, TubeSpec, FanSpec,
                            create_coral)  # noqa: E402


RECIPES = {
    "branching": {
        "A": (BranchingSpec(0.85, 5, 0.70, 0.95, 0.48, 0.42, 1101), "reef_warm", "red"),
        "B": (BranchingSpec(1.04, 6, 0.92, 0.88, 0.56, 0.62, 1102), "reef_warm", "pink"),
        "C": (BranchingSpec(0.76, 4, 0.60, 1.18, 0.42, 0.55, 1103), "reef_warm", "orange"),
    },
    "rounded": {
        "A": (RoundedSpec(0.89, 6, 0.72, 0.52, 0.36, 2201), "reef_warm", "orange"),
        "B": (RoundedSpec(1.10, 7, 0.92, 0.45, 0.54, 2202), "reef_warm", "yellow"),
        "C": (RoundedSpec(0.78, 5, 0.61, 0.67, 0.42, 2203), "reef_cool", "muted_purple"),
    },
    "tube": {
        "A": (TubeSpec(0.80, 7, 0.71, 0.87, 0.30, 0.43, 0.16, 3301), "reef_warm", "pink"),
        "B": (TubeSpec(0.97, 9, 0.84, 0.78, 0.40, 0.60, 0.23, 3302), "reef_cool", "cyan"),
        "C": (TubeSpec(0.72, 6, 0.62, 1.15, 0.25, 0.50, 0.19, 3303), "reef_warm", "yellow"),
    },
    "fan": {
        "A": (FanSpec(0.96, 6, 0.98, 0.82, 0.18, 0.38, 4401), "reef_cool", "muted_purple"),
        "B": (FanSpec(1.12, 7, 1.16, 0.71, 0.30, 0.58, 4402), "reef_warm", "red"),
        "C": (FanSpec(0.82, 5, 0.84, 1.05, 0.10, 0.45, 4403), "reef_cool", "blue"),
    },
}

FAMILY_INDEX = {"branching": "001", "rounded": "002", "tube": "003", "fan": "004"}


def coral_material(family, swatch):
    name = f"mat__coral_{swatch}"
    material = bpy.data.materials.get(name)
    if material is None:
        material = bpy.data.materials.new(name)
    material.use_nodes = True
    srgb = hex_rgb(family, swatch)
    linear = tuple(((v + 0.055) / 1.055) ** 2.4 if v > 0.04045 else v / 12.92 for v in srgb)
    material.diffuse_color = (*linear, 1)
    shader = material.node_tree.nodes.get("Principled BSDF")
    shader.inputs["Base Color"].default_value = (*linear, 1)
    shader.inputs["Roughness"].default_value = 0.82
    shader.inputs["Metallic"].default_value = 0
    return material


def create_sources(scene):
    """Create all source meshes in dedicated collections; do not move source pivots."""
    scene.unit_settings.system = "METRIC"
    scene.unit_settings.scale_length = 1
    result = {}
    for family, variants in RECIPES.items():
        group = bpy.data.collections.new(f"FAF-CORAL-{FAMILY_INDEX[family]} Source")
        scene.collection.children.link(group)
        for letter, (spec, palette_family, swatch) in variants.items():
            stem = f"coral_{FAMILY_INDEX[family]}{letter.lower()}_{family}"
            obj = create_coral(stem + "__mesh", family, spec,
                               coral_material(palette_family, swatch), group)
            result[(family, letter)] = obj
    return result


def add_review_instances(scene, sources):
    """Four family rows of linked copies, excluded from exports."""
    review = bpy.data.collections.new("FAF-CORAL Pack Review")
    scene.collection.children.link(review)
    for row, family in enumerate(RECIPES):
        for col, letter in enumerate("ABC"):
            source = sources[(family, letter)]
            copy = source.copy()
            copy.data = source.data
            copy.name = f"review__coral_{FAMILY_INDEX[family]}{letter.lower()}_{family}"
            copy.location = ((col - 1) * 1.8, row * 1.9, 0)
            review.objects.link(copy)
            source.hide_set(True)
    return review


def export_sources(scene, sources, root, overwrite=False):
    output = root / "game/assets/faf/coral"
    output.mkdir(parents=True, exist_ok=True)
    reports = []
    for (family, letter), obj in sources.items():
        spec, palette_family, swatch = RECIPES[family][letter]
        stem = f"coral_{FAMILY_INDEX[family]}{letter.lower()}_{family}"
        destination = output / (stem + ".glb")
        bpy.ops.object.select_all(action="DESELECT")
        was_hidden = obj.hide_get()
        obj.hide_set(False)
        obj.select_set(True)
        bpy.context.view_layer.objects.active = obj
        check = inspect_objects([obj])
        if check["problems"] or not 100 <= check["triangle_count_source"] <= 800:
            raise RuntimeError(f"{stem} failed FAF source checks: {check}")
        scene.name = f"FAF-CORAL-{FAMILY_INDEX[family]}{letter} {family.title()}"
        export_selected_glb(str(destination), include_animations=False, overwrite=overwrite)
        obj.hide_set(was_hidden)
        reports.append({"faf_id": f"FAF-CORAL-{FAMILY_INDEX[family]}{letter}",
                        "category": "CORAL", "species_or_type": family,
                        "stem": stem, "family": family,
                        "generator": f"coral.{family}.v1",
                        "dimensions_m": [round(v, 5) for v in obj.dimensions],
                        "triangles": check["triangle_count_source"],
                        "materials": check["material_count"],
                        "armature_bone_count": 0, "animation_actions": [],
                        "origin_m": [round(v, 5) for v in obj.location],
                        "rotation_rad": [round(v, 5) for v in obj.rotation_euler],
                        "scale": [round(v, 5) for v in obj.scale],
                        "palette": f"{palette_family}.{swatch}", "spec": spec.__dict__,
                        "glb": destination.relative_to(root).as_posix()})
    scene.name = "FAF-CORAL-001-to-004 Production Pack"
    return reports


def build(root: Path, overwrite=False):
    source = root / "assets/faf/coral/coral_001_to_004_family.blend"
    if not overwrite and source.exists():
        raise FileExistsError(source)
    bpy.ops.wm.read_factory_settings(use_empty=True)
    scene = bpy.context.scene
    scene.name = "FAF-CORAL-001-to-004 Production Pack"
    sources = create_sources(scene)
    reports = export_sources(scene, sources, root, overwrite)
    manifest = source.with_name("coral_001_to_004_manifest.json")
    manifest.write_text(json.dumps({"source": source.relative_to(root).as_posix(),
                                    "assets": reports}, indent=2) + "\n")
    add_review_instances(scene, sources)
    source.parent.mkdir(parents=True, exist_ok=True)
    bpy.ops.wm.save_as_mainfile(filepath=str(source))
    return reports


if __name__ == "__main__":
    args = sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else []
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path, required=True)
    parser.add_argument("--overwrite", action="store_true")
    options = parser.parse_args(args)
    print("FAF_CORAL_PACK_REPORT=" + json.dumps(build(options.root, options.overwrite)))
