"""Write reproducible Godot wrappers from the wood recipe, without editing gameplay."""
from pathlib import Path
import json

ROOT = Path(__file__).resolve().parents[4]
RECIPES = [entry["generator_config"] for entry in json.loads(
    (ROOT / "assets/faf/pond/wood/pond_wood_001_manifest.json").read_text())["assets"]]
OUT = ROOT / "game/scenes/faf_pond_wood"
OUT.mkdir(parents=True, exist_ok=True)
for spec in RECIPES:
    stem = spec["stem"]
    prop = f'faf_id = "{spec["id"]}"\nfamily = "{spec["family"]}"\n'
    for key, attr in (("length", "length_m"), ("radius", "radius_m"),
                      ("inner", "inner_radius_m"), ("height", "height_m"),
                      ("bend", "bend_m"), ("forks", "fork_count")):
        if key in spec:
            value = int(spec[key]) if key == "forks" else float(spec[key])
            prop += f"{attr} = {value}\n"
    content = f'''[gd_scene load_steps=3 format=3]

[ext_resource type="Script" path="res://dev/faf_pond_wood_piece.gd" id="1_script"]
[ext_resource type="PackedScene" path="res://assets/faf/pond/wood/{stem}.glb" id="2_visual"]

[node name="{stem}" type="Node3D"]
script = ExtResource("1_script")
{prop}
[node name="Visual" parent="." instance=ExtResource("2_visual")]
'''
    (OUT/(stem+".tscn")).write_text(content)
print(f"FAF_POND_WOOD_WRAPPERS={len(RECIPES)}")
