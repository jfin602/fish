"""Regenerate only Pond wood-002 Godot wrappers from its collision manifest."""
from pathlib import Path
import json

ROOT=Path(__file__).resolve().parents[4]
entries=json.loads((ROOT/"assets/faf/pond/wood/pond_wood_002_manifest.json").read_text())["assets"]
out=ROOT/"game/scenes/faf_pond_wood_002"
out.mkdir(parents=True,exist_ok=True)
for entry in entries:
    stem=entry["stem"]
    collision=json.dumps(entry["collision_shapes"],separators=(",",":"))
    content=f'''[gd_scene load_steps=3 format=3]

[ext_resource type="Script" path="res://dev/faf_pond_wood_002_piece.gd" id="1_script"]
[ext_resource type="PackedScene" path="res://assets/faf/pond/wood/{stem}.glb" id="2_visual"]

[node name="{stem}" type="Node3D"]
script = ExtResource("1_script")
faf_id = "{entry['faf_id']}"
collision_json = {json.dumps(collision)}

[node name="Visual" parent="." instance=ExtResource("2_visual")]
'''
    (out/(stem+".tscn")).write_text(content)
print(f"POND_WOOD_002_WRAPPERS={len(entries)}")
