"""Build FAF-ROCK-001 Blender master and separate GLB variants.

Run with Blender background Python. --reference-only creates A for the first
visual pass; the default creates reviewed A/B/C/D and exports each separately.
"""

from __future__ import annotations

import argparse
import json
import sys
from pathlib import Path

import bpy

TOOLING = Path(__file__).resolve().parents[1]
if str(TOOLING) not in sys.path:
    sys.path.insert(0, str(TOOLING))

from common.palette import hex_rgb  # noqa: E402
from common.export import export_selected_glb  # noqa: E402
from common.validation import inspect_objects  # noqa: E402
from rocks.boulder import BoulderSpec, create_reef_boulder  # noqa: E402


SPECS = {
    "A": BoulderSpec(size=1.55, depth=0.84, verticality=0.66, asymmetry=0.68,
                     facet_scale=0.55, base_width=0.82, lean=(0.14, -0.06), seed=1049),
    "B": BoulderSpec(size=1.85, depth=1.10, verticality=0.38, asymmetry=0.42,
                     facet_scale=0.35, base_width=0.96, lean=(-0.015, 0.015), seed=2083),
    "C": BoulderSpec(size=1.25, depth=0.78, verticality=1.27, asymmetry=0.66,
                     facet_scale=0.72, base_width=0.72, lean=(0.10, 0.015), seed=3109),
    "D": BoulderSpec(size=1.45, depth=0.73, verticality=0.83, asymmetry=0.64,
                     facet_scale=0.60, base_width=0.78, lean=(0.26, -0.09), seed=4093),
}


def _rock_material() -> bpy.types.Material:
    material = bpy.data.materials.new("mat__rock_cool_slate")
    material.use_nodes = True
    rgb = hex_rgb("rock", "cool_slate")
    linear = tuple(((value + 0.055) / 1.055) ** 2.4 if value > 0.04045 else value / 12.92 for value in rgb)
    material.diffuse_color = (*linear, 1.0)
    shader = material.node_tree.nodes.get("Principled BSDF")
    shader.inputs["Base Color"].default_value = (*linear, 1.0)
    shader.inputs["Roughness"].default_value = 0.88
    shader.inputs["Metallic"].default_value = 0.0
    return material


def create_sources(scene: bpy.types.Scene, reference_only: bool = False) -> dict[str, bpy.types.Object]:
    """Add origin-pivot source meshes to a dedicated collection."""
    scene.unit_settings.system = "METRIC"
    scene.unit_settings.scale_length = 1.0
    source = bpy.data.collections.new("FAF-ROCK-001 Source")
    scene.collection.children.link(source)
    material = _rock_material()
    objects = {}
    for letter, spec in list(SPECS.items())[:1 if reference_only else 4]:
        stem = f"rock_001{letter.lower()}_reef_boulder"
        objects[letter] = create_reef_boulder(stem + "__mesh", spec, material, source)
    return objects


def add_review_instances(scene: bpy.types.Scene, sources: dict[str, bpy.types.Object]) -> None:
    """Lay out linked mesh copies for visual family inspection, outside export."""
    review = bpy.data.collections.new("FAF-ROCK-001 Review")
    scene.collection.children.link(review)
    spacing = {"A": (-3.2, 0.0), "B": (-1.1, 0.0),
               "C": (1.2, 0.0), "D": (3.2, 0.0)}
    for letter, source in sources.items():
        instance = source.copy()
        instance.data = source.data
        instance.name = f"review__rock_001{letter.lower()}"
        instance.location = (*spacing[letter], 0.0)
        review.objects.link(instance)
        source.hide_set(True)


def build(reference_only: bool, root: Path) -> list[dict]:
    bpy.ops.wm.read_factory_settings(use_empty=True)
    scene = bpy.context.scene
    scene.name = "FAF-ROCK-001 Reef Boulder Family"
    sources = create_sources(scene, reference_only)
    source = root / "assets/faf/rocks/rock_001_reef_boulder_family.blend"
    output = root / "game/assets/faf/rocks"
    source.parent.mkdir(parents=True, exist_ok=True)
    output.mkdir(parents=True, exist_ok=True)
    reports = []
    for letter, obj in sources.items():
        spec = SPECS[letter]
        stem = f"rock_001{letter.lower()}_reef_boulder"
        bpy.ops.object.select_all(action="DESELECT")
        obj.select_set(True)
        bpy.context.view_layer.objects.active = obj
        check = inspect_objects([obj])
        if check["problems"] or not 150 <= check["triangle_count_source"] <= 500:
            raise RuntimeError(f"{letter} failed checks: {check}")
        destination = output / (stem + ".glb")
        export = export_selected_glb(str(destination), include_animations=False, overwrite=True)
        dimensions = tuple(round(v, 5) for v in obj.dimensions)
        reports.append({"variant": letter, "faf_id": f"FAF-ROCK-001{letter}",
                        "stem": stem, "dimensions_m": dimensions,
                        "triangles": check["triangle_count_source"],
                        "materials": check["material_count"],
                        "spec": spec.__dict__, "export": export["glb_output"]})
    add_review_instances(scene, sources)
    bpy.ops.object.select_all(action="DESELECT")
    for obj in bpy.data.collections["FAF-ROCK-001 Review"].objects:
        obj.select_set(True)
    bpy.context.view_layer.objects.active = next(iter(bpy.data.collections["FAF-ROCK-001 Review"].objects))
    bpy.ops.wm.save_as_mainfile(filepath=str(source))
    return reports


if __name__ == "__main__":
    args = sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else []
    parser = argparse.ArgumentParser()
    parser.add_argument("--reference-only", action="store_true")
    parser.add_argument("--root", type=Path, required=True)
    options = parser.parse_args(args)
    print("FAF_ROCK_001_REPORT=" + json.dumps(build(options.reference_only, options.root)))
