"""Build the isolated, authored FAF visual checkpoint from reviewed GLBs.

The placements below are hand-composed, not a random scatter. Rebuilding writes
only the dedicated proof scene and its placement manifest.
"""

from __future__ import annotations

import argparse
from collections import Counter
from dataclasses import dataclass
import json
import math
from pathlib import Path
import struct


@dataclass(frozen=True)
class Place:
    stem: str
    x: float
    z: float
    scale: float = 1.0
    yaw: float = 0.0
    lift: float = 0.0
    y: float | None = None
    pitch: float = 0.0
    roll: float = 0.0


# The central sand lane runs north toward a deliberately built rock arch.
# Shelves and reef masses frame it; color clusters occupy their bases and rims.
TERRAIN = (
    Place("terrain_001a_open_sand", 0, 0, yaw=0.08, lift=-0.08),
    Place("terrain_001a_open_sand", -8.2, -2.4, yaw=-0.18, lift=-0.11),
    Place("terrain_001a_open_sand", 8.0, -2.9, yaw=0.22, lift=-0.10),
    Place("terrain_001a_open_sand", 0.2, -7.3, yaw=-0.28, lift=-0.12),
    Place("terrain_001b_rolling", -0.5, 3.8, yaw=0.43, lift=-0.05),
    Place("terrain_001b_rolling", -6.55, 2.66, yaw=-0.37, lift=-0.08),
    Place("terrain_001b_rolling", 6.38, 2.74, yaw=0.51, lift=-0.09),
    Place("terrain_001b_rolling", -7.79, -7.14, yaw=0.87, lift=-0.10),
    Place("terrain_001c_reef_shelf", -6.5, -4.2, yaw=-0.36),
    Place("terrain_001d_slope", -3.8, -2.6, yaw=0.48),
    Place("terrain_001e_drop_off", 6.2, -4.6, yaw=-0.30),
    Place("terrain_001f_rocky_floor", 4.7, -1.6, yaw=0.41),
    Place("terrain_001c_reef_shelf", 9.3, -7.9, yaw=0.83, lift=-0.07),
)

ROCKS = (
    # Swim-through arch, with unequal supports and a broad low lintel.
    Place("rock_001c_reef_boulder", -1.56, -2.83, 1.53, -0.40),
    Place("rock_001a_reef_boulder", 1.54, -2.72, 1.63, 0.29),
    Place("rock_001b_reef_boulder", -0.02, -2.78, 2.10, 0.23, 1.50, pitch=-0.11, roll=0.08),
    Place("rock_001d_reef_boulder", -2.46, -3.10, 1.03, 0.88, 0.10),
    Place("rock_001d_reef_boulder", 2.48, -2.55, 0.88, -0.65),
    # West shelf and ledge; a low boulder projects over its front shoulder.
    Place("rock_001a_reef_boulder", -6.45, -4.65, 1.34, -0.74, 0.26),
    Place("rock_001b_reef_boulder", -5.22, -3.32, 1.40, 0.31, 0.25),
    Place("rock_001d_reef_boulder", -8.40, -5.39, 1.36, 1.12, 0.36),
    Place("rock_001c_reef_boulder", -9.65, -6.55, 1.22, -0.49, 0.46),
    # East drop-off, broken cliff face, and an offset overhang.
    Place("rock_001c_reef_boulder", 6.82, -4.20, 1.85, -0.27, 0.08),
    Place("rock_001a_reef_boulder", 8.78, -5.32, 1.95, 0.76, 0.20),
    Place("rock_001d_reef_boulder", 9.66, -6.74, 1.79, -0.96, 0.57),
    Place("rock_001b_reef_boulder", 5.74, -3.12, 1.62, -0.18, 0.72, roll=-0.18),
    Place("rock_001c_reef_boulder", 10.15, -8.54, 1.57, 0.51, 1.14),
    # Near framing stones; deliberately keep the central floor open.
    Place("rock_001a_reef_boulder", -4.68, 1.60, 1.25, 0.46),
    Place("rock_001d_reef_boulder", 4.82, 2.04, 1.20, -0.52),
    Place("rock_001b_reef_boulder", -7.38, 3.15, 1.13, 1.01),
    Place("rock_001a_reef_boulder", 7.84, 2.21, 1.08, -0.91),
    Place("rock_001c_reef_boulder", -10.17, -2.31, 1.46, 0.67),
)

CORAL = (
    # Near accents frame the follow view while leaving a three-metre swim lane.
    Place("coral_002a_rounded", -2.78, 1.44, 0.83, 0.33),
    Place("coral_003a_tube", 2.72, 1.35, 0.79, -0.49),
    Place("coral_001a_branching", -2.95, 0.22, 0.77, -0.72),
    Place("coral_003c_tube", 3.04, 0.12, 0.91, 0.37),
    Place("coral_001b_branching", -4.94, 1.96, 0.93, 0.26),
    Place("coral_003a_tube", -5.50, 1.13, 0.76, 0.66),
    Place("coral_002a_rounded", -4.22, 1.46, 0.81, -0.55),
    Place("coral_003c_tube", -5.76, 2.28, 0.88, -0.37),
    Place("coral_002c_rounded", -3.98, 2.49, 0.99, 0.84),
    Place("coral_001a_branching", -6.34, 2.83, 0.76, -0.80),
    Place("coral_004b_fan", -5.37, 0.68, 0.91, -0.38),
    Place("coral_001a_branching", -3.15, 1.18, 0.85, -0.19),
    Place("coral_002a_rounded", -6.10, 0.62, 1.04, 0.63),
    Place("coral_003b_tube", -7.25, 0.86, 0.86, -0.72),
    Place("coral_004a_fan", -7.58, 1.48, 0.80, 0.52),
    Place("coral_004a_fan", -4.48, -0.55, 1.08, 1.25),
    Place("coral_001c_branching", -2.70, -1.74, 0.82, 0.38),
    Place("coral_004b_fan", -2.48, -2.68, 0.73, 0.88),
    Place("coral_003c_tube", -2.24, -3.65, 0.74, -0.49),
    Place("coral_002b_rounded", -3.52, -3.40, 0.90, -0.65),
    Place("coral_003a_tube", -5.22, -3.67, 0.98, 0.72),
    Place("coral_004c_fan", -7.17, -3.38, 1.07, -0.26),
    Place("coral_001b_branching", -7.91, -5.45, 0.77, 0.90),
    Place("coral_002c_rounded", -9.09, -4.12, 1.03, 0.33),
    Place("coral_003c_tube", -5.93, -6.14, 0.95, -0.14),
    Place("coral_004b_fan", -7.65, -6.92, 0.86, 0.74),
    Place("coral_001a_branching", 2.92, -1.44, 0.98, -0.50),
    Place("coral_004c_fan", 2.42, -2.80, 0.80, -0.57),
    Place("coral_004c_fan", 3.11, 0.87, 0.76, 0.42),
    Place("coral_002b_rounded", 3.73, 0.20, 0.89, 0.28),
    Place("coral_001b_branching", 4.64, 2.05, 0.79, -0.22),
    Place("coral_003b_tube", 5.05, -0.45, 0.91, -0.68),
    Place("coral_004a_fan", 4.02, 1.36, 0.95, 1.15),
    Place("coral_001c_branching", 5.88, 1.34, 0.93, 0.42),
    Place("coral_003a_tube", 6.68, 1.83, 0.74, -0.82),
    Place("coral_002a_rounded", 6.47, -0.55, 1.01, -0.41),
    Place("coral_003a_tube", 6.97, -1.53, 0.86, 0.51),
    Place("coral_004c_fan", 8.15, -1.20, 0.99, -0.79),
    Place("coral_002c_rounded", 7.61, 0.37, 0.76, 0.38),
    Place("coral_001b_branching", 7.83, -3.23, 0.85, 0.21),
    Place("coral_002c_rounded", 9.21, -2.84, 1.05, -0.92),
    Place("coral_003c_tube", 9.49, -4.32, 0.91, 0.87),
    Place("coral_004a_fan", 8.59, -4.61, 0.72, -0.50),
    Place("coral_004b_fan", 6.02, -5.27, 0.88, -0.46),
    Place("coral_001a_branching", 2.35, -3.95, 0.78, 0.71),
    Place("coral_003b_tube", 3.34, -5.83, 0.84, -0.22),
    Place("coral_002b_rounded", -2.92, -6.62, 0.88, 0.58),
    Place("coral_004a_fan", -4.29, -7.36, 0.91, -0.94),
    Place("coral_001c_branching", 1.48, -7.58, 0.77, 0.37),
    Place("coral_002a_rounded", 9.66, -7.77, 0.93, -0.26),
)

PLANTS = (
    Place("plant_002b_broad_seaweed", -6.81, 1.91, 1.00, 0.44),
    Place("plant_001a_sea_grass", -3.55, 2.95, 0.91, -0.51),
    Place("plant_001c_sea_grass", -4.08, 0.64, 1.04, 0.71),
    Place("plant_003a_reef_algae", -5.18, 3.02, 0.84, -0.87),
    Place("plant_002a_broad_seaweed", -7.56, 2.25, 0.92, -0.13),
    Place("plant_001b_sea_grass", -6.47, 0.12, 0.78, 0.63),
    Place("plant_003b_reef_algae", -2.87, 0.72, 1.02, -0.24),
    Place("plant_002c_broad_seaweed", -3.20, -3.04, 0.86, 0.38),
    Place("plant_001a_sea_grass", -4.52, -2.70, 0.87, -0.92),
    Place("plant_003a_reef_algae", -6.88, -2.17, 0.91, 0.24),
    Place("plant_002b_broad_seaweed", -8.34, -4.34, 0.82, -0.48),
    Place("plant_001c_sea_grass", -5.01, -5.82, 0.96, 0.75),
    Place("plant_003b_reef_algae", -9.53, -5.28, 0.88, -0.70),
    Place("plant_001b_sea_grass", -7.36, -7.48, 0.84, 0.19),
    Place("plant_002a_broad_seaweed", 3.23, 2.39, 0.87, -0.74),
    Place("plant_001b_sea_grass", 5.77, 2.44, 0.92, 0.35),
    Place("plant_003a_reef_algae", 4.84, 0.79, 0.83, -0.23),
    Place("plant_002c_broad_seaweed", 6.49, 0.73, 1.01, 0.91),
    Place("plant_001a_sea_grass", 7.56, 1.54, 0.89, -0.56),
    Place("plant_003b_reef_algae", 4.18, -0.99, 0.96, 0.43),
    Place("plant_002b_broad_seaweed", 5.54, -2.20, 0.88, -0.82),
    Place("plant_001c_sea_grass", 7.44, -2.65, 0.94, 0.18),
    Place("plant_003a_reef_algae", 8.65, -0.15, 0.92, -0.49),
    Place("plant_001b_sea_grass", 8.95, -3.42, 0.80, 0.67),
    Place("plant_002a_broad_seaweed", 9.83, -5.23, 0.95, -0.31),
    Place("plant_003b_reef_algae", 6.45, -5.67, 0.85, 0.81),
    Place("plant_001a_sea_grass", 3.14, -4.84, 0.77, -0.08),
    Place("plant_002c_broad_seaweed", -2.57, -6.10, 0.90, 0.46),
    Place("plant_003a_reef_algae", 1.56, -6.12, 0.83, -0.43),
    Place("plant_001c_sea_grass", 0.47, -8.41, 0.84, 0.27),
)

DETAILS = (
    Place("detail_001a_fan_shell", -3.42, 1.86, 1.00, 0.42),
    Place("detail_003a_slate_pebbles", -4.24, 1.95, 1.08, -0.72),
    Place("detail_002a_coral_rubble", -5.77, 1.54, 0.87, 0.23),
    Place("detail_004a_encrusting_sponge", -4.91, 0.31, 1.04, -0.39),
    Place("detail_001b_ridge_shell", -2.89, 0.40, 0.91, -0.23),
    Place("detail_003b_warm_pebbles", -6.02, -1.30, 0.93, 0.73),
    Place("detail_002b_reef_rubble", -5.04, -3.05, 0.87, -0.67),
    Place("detail_004b_cup_sponge", -6.61, -4.90, 0.92, 0.86),
    Place("detail_001a_fan_shell", 3.13, 1.07, 0.87, -0.58),
    Place("detail_003b_warm_pebbles", 4.04, 1.62, 1.06, 0.32),
    Place("detail_002a_coral_rubble", 5.04, 0.33, 0.95, -0.91),
    Place("detail_004a_encrusting_sponge", 6.87, 1.14, 0.93, 0.58),
    Place("detail_001b_ridge_shell", 7.59, -0.36, 1.06, -0.31),
    Place("detail_003a_slate_pebbles", 4.35, -2.67, 0.89, 0.79),
    Place("detail_002b_reef_rubble", 8.48, -3.13, 1.03, -0.73),
    Place("detail_004b_cup_sponge", 9.66, -5.77, 0.91, 0.25),
    Place("detail_003a_slate_pebbles", -1.88, -6.14, 0.96, 0.34),
    Place("detail_001a_fan_shell", 1.75, -5.36, 1.03, -0.47),
)

FISH = (
    # Fish are in water, never terrain-snapped. Godot import faces +Z.
    Place("fish_001_yellow_tang", 0.00, 0.68, 1.0, math.pi, y=1.10),
    Place("fish_002_ocellaris_clownfish", -2.32, -1.16, 1.0, 2.25, y=1.23),
    Place("fish_002_ocellaris_clownfish", -2.82, -1.82, 0.88, 2.60, y=1.38),
    Place("fish_003_blue_green_chromis", -0.70, -3.57, 0.94, 3.45, y=2.00),
    Place("fish_003_blue_green_chromis", -0.31, -3.86, 1.05, 3.58, y=1.82),
    Place("fish_003_blue_green_chromis", 0.15, -4.13, 0.89, 3.29, y=2.12),
    Place("fish_003_blue_green_chromis", 0.56, -3.72, 0.98, 3.88, y=1.94),
    Place("fish_003_blue_green_chromis", 0.87, -4.35, 0.91, 3.12, y=2.26),
    Place("fish_004_reef_butterflyfish", 3.46, -1.06, 1.0, 3.86, y=1.18),
    Place("fish_004_reef_butterflyfish", -4.24, -3.29, 0.93, 2.42, y=1.35),
    Place("fish_005_blacktip_reef_shark", 6.10, -7.80, 0.83, 2.83, y=2.50),
)

GROUPS = {"Terrain": TERRAIN, "Rocks": ROCKS, "Coral": CORAL,
          "Plants": PLANTS, "Details": DETAILS, "Fish": FISH}


def _glb(path: Path):
    data = path.read_bytes()
    if data[:4] != b"glTF":
        raise ValueError(f"not a GLB: {path}")
    json_size, json_type = struct.unpack_from("<II", data, 12)
    if json_type != 0x4E4F534A:
        raise ValueError(f"missing JSON chunk: {path}")
    gltf = json.loads(data[20:20 + json_size])
    offset = 20 + json_size
    bin_size, bin_type = struct.unpack_from("<II", data, offset)
    if bin_type != 0x004E4942:
        raise ValueError(f"missing BIN chunk: {path}")
    binary = memoryview(data)[offset + 8:offset + 8 + bin_size]
    return gltf, binary


def _accessor(gltf, binary, number):
    accessor = gltf["accessors"][number]
    view = gltf["bufferViews"][accessor["bufferView"]]
    component = {5123: "H", 5125: "I", 5126: "f"}[accessor["componentType"]]
    count = {"SCALAR": 1, "VEC3": 3}[accessor["type"]]
    fmt = "<" + component * count
    size = struct.calcsize(fmt)
    stride = view.get("byteStride", size)
    start = view.get("byteOffset", 0) + accessor.get("byteOffset", 0)
    values = [struct.unpack_from(fmt, binary, start + i * stride)
              for i in range(accessor["count"])]
    return [v[0] if count == 1 else v for v in values]


def _terrain_triangles(path: Path):
    gltf, binary = _glb(path)
    triangles = []
    for primitive in gltf["meshes"][0]["primitives"]:
        vertices = _accessor(gltf, binary, primitive["attributes"]["POSITION"])
        indices = _accessor(gltf, binary, primitive["indices"])
        triangles.extend(tuple(vertices[indices[i + j]] for j in range(3))
                         for i in range(0, len(indices), 3))
    return triangles


def _interpolate_height(triangle, x, z):
    a, b, c = triangle
    denominator = (b[2] - c[2]) * (a[0] - c[0]) + (c[0] - b[0]) * (a[2] - c[2])
    if abs(denominator) < 1e-8:
        return None
    u = ((b[2] - c[2]) * (x - c[0]) + (c[0] - b[0]) * (z - c[2])) / denominator
    v = ((c[2] - a[2]) * (x - c[0]) + (a[0] - c[0]) * (z - c[2])) / denominator
    w = 1 - u - v
    if min(u, v, w) < -1e-5:
        return None
    return u * a[1] + v * b[1] + w * c[1]


def _ground_height(x, z, terrain_meshes):
    heights = []
    for placement, triangles in terrain_meshes:
        dx, dz = x - placement.x, z - placement.z
        c, s = math.cos(placement.yaw), math.sin(placement.yaw)
        local_x, local_z = c * dx - s * dz, s * dx + c * dz
        for triangle in triangles:
            height = _interpolate_height(triangle, local_x, local_z)
            if height is not None:
                heights.append(placement.lift + height * placement.scale)
    if not heights:
        raise ValueError(f"placement ({x:.2f}, {z:.2f}) lies outside FAF terrain")
    return max(heights)


def _number(value):
    return format(value, ".5f").rstrip("0").rstrip(".") if value else "0"


def _vector(values):
    return "Vector3(" + ", ".join(_number(value) for value in values) + ")"


def _triangles(path: Path):
    gltf, _binary = _glb(path)
    return sum(gltf["accessors"][primitive["indices"]]["count"] // 3
               for mesh in gltf["meshes"] for primitive in mesh["primitives"])


def build(root: Path, overwrite: bool):
    scene = root / "game/scenes/faf_style_proof_001.tscn"
    manifest = root / "docs/fish-asset-factory/style-proof-001/placement-manifest.json"
    if not overwrite and (scene.exists() or manifest.exists()):
        raise FileExistsError("style proof already exists; use --overwrite intentionally")
    used = {place.stem for group in GROUPS.values() for place in group}
    paths = {path.stem: path for path in (root / "game/assets/faf").glob("*/*.glb")
             if path.stem in used}
    if paths.keys() != used:
        raise FileNotFoundError(f"missing FAF GLBs: {sorted(used - paths.keys())}")
    terrain_meshes = [(place, _terrain_triangles(paths[place.stem])) for place in TERRAIN]
    script_paths = {
        "proof": "res://dev/faf_style_proof_001.gd",
        "atmosphere": "res://scripts/UnderwaterAtmosphere.cs",
        "seabed": "res://scripts/SeabedSurface.cs",
    }
    resources = {key: str(index) for index, key in enumerate(sorted(used), 4)}
    lines = [f'[gd_scene load_steps={4 + len(used) + 1} format=3]', ""]
    for key, path in script_paths.items():
        lines.append(f'[ext_resource type="Script" path="{path}" id="{key}"]')
    for stem in sorted(used):
        path = paths[stem].relative_to(root / "game").as_posix()
        lines.append(f'[ext_resource type="PackedScene" path="res://{path}" id="{resources[stem]}"]')
    lines += ["", '[sub_resource type="BoxShape3D" id="proof_upper"]',
              'size = Vector3(84, 0.6, 84)', "",
              '[node name="FAFStyleProof001" type="Node3D"]',
              'script = ExtResource("proof")', "",
              '[node name="WorldEnvironment" type="WorldEnvironment" parent="."]', "",
              '[node name="Sun" type="DirectionalLight3D" parent="."]', "",
              '[node name="UnderwaterAtmosphere" type="Node3D" parent="."]',
              'script = ExtResource("atmosphere")', "",
              '[node name="Habitat" type="Node3D" parent="."]', "",
              '[node name="Seabed" type="StaticBody3D" parent="Habitat"]',
              'position = Vector3(0, -11, 0)',
              'collision_layer = 0',
              'collision_mask = 0',
              'script = ExtResource("seabed")', "",
              '[node name="HiddenLimits" type="StaticBody3D" parent="Habitat"]',
              'collision_layer = 0', 'collision_mask = 0', "",
              '[node name="Upper" type="CollisionShape3D" parent="Habitat/HiddenLimits"]',
              'position = Vector3(0, 6, 0)', 'disabled = true',
              'shape = SubResource("proof_upper")', "",
              '[node name="ReviewCamera" type="Camera3D" parent="."]',
              'position = Vector3(0, 2.8, 4.6)', 'current = true', 'fov = 75.0', "",
              '[node name="FAFAssets" type="Node3D" parent="."]', ""]
    records = []
    for group_name, placements in GROUPS.items():
        lines += [f'[node name="{group_name}" type="Node3D" parent="FAFAssets"]', ""]
        for index, place in enumerate(placements, 1):
            if group_name == "Terrain":
                y = place.lift
            elif place.y is not None:
                y = place.y
            else:
                y = _ground_height(place.x, place.z, terrain_meshes) + place.lift
            name = f"{index:02d}_{place.stem}"
            lines.append(f'[node name="{name}" parent="FAFAssets/{group_name}" instance=ExtResource("{resources[place.stem]}")]')
            lines.append(f'position = {_vector((place.x, y, place.z))}')
            lines.append(f'rotation = {_vector((place.pitch, place.yaw, place.roll))}')
            lines.append(f'scale = {_vector((place.scale,) * 3)}')
            lines.append("")
            records.append({"name": name, "group": group_name, "stem": place.stem,
                            "position_m": [round(place.x, 5), round(y, 5), round(place.z, 5)],
                            "rotation_rad": [place.pitch, place.yaw, place.roll],
                            "uniform_scale": place.scale})
    scene.parent.mkdir(parents=True, exist_ok=True)
    manifest.parent.mkdir(parents=True, exist_ok=True)
    scene.write_text("\n".join(lines))
    counts = Counter(record["group"] for record in records)
    source_triangles = {stem: _triangles(path) for stem, path in paths.items()}
    authored_triangles = sum(source_triangles[record["stem"]] for record in records)
    payload = {"scene": scene.relative_to(root).as_posix(),
               "composition": "authored open central lane, unequal arch, west shelf, east drop-off/cliff, foreground color clusters",
               "atmosphere": "existing UnderwaterAtmosphere; non-rendering depth/upper anchors only",
               "counts": dict(counts), "unique_glbs": len(used),
               "authored_source_triangles": authored_triangles,
               "source_triangles": source_triangles, "placements": records}
    manifest.write_text(json.dumps(payload, indent=2) + "\n")
    return payload


if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", required=True, type=Path)
    parser.add_argument("--overwrite", action="store_true")
    args = parser.parse_args()
    report = build(args.root.resolve(), args.overwrite)
    print("FAF_STYLE_PROOF_BUILD", report["counts"], report["unique_glbs"],
          report["authored_source_triangles"])
