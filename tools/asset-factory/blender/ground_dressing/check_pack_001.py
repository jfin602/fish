"""Focused static validation for the eight Pond ground GLBs and manifest."""
from __future__ import annotations

import json
from pathlib import Path
import struct
import sys


def gltf_json(path: Path) -> dict:
    data = path.read_bytes()
    assert data[:4] == b"glTF" and struct.unpack_from("<I", data, 8)[0] == len(data), path
    size, kind = struct.unpack_from("<I4s", data, 12)
    assert kind == b"JSON", path
    return json.loads(data[20:20 + size])


def check(root: Path) -> None:
    manifest = json.loads((root / "assets/faf/pond/ground/pond_ground_001_manifest.json").read_text())
    assets = manifest["assets"]
    expected = {"FAF-TERRAIN-002A", "FAF-TERRAIN-002B",
                *(f"FAF-DETAIL-{n}{v}" for n in ("005", "006", "007") for v in "AB")}
    assert len(assets) == 8 and {x["faf_id"] for x in assets} == expected
    assert set(manifest["shared_materials"]) == {"silt", "mud", "wet_bank", "algae_stone"}
    total = 0
    for record in assets:
        source = root / record["blender_source"]
        path = root / record["glb"]
        assert source.is_file() and path.is_file() and source.stat().st_size > 1000
        data = gltf_json(path)
        assert len(data["nodes"]) == 1 and len(data["meshes"]) == 1
        extras = data["nodes"][0]["extras"]
        assert extras["faf_id"] == record["faf_id"]
        assert extras["faf_habitat"] == "pond"
        assert extras["seed"] == record["seed"]
        assert len(data["materials"]) == record["material_count"]
        assert {m["name"] for m in data["materials"]} == set(record["materials"])
        triangles = sum(data["accessors"][p["indices"]]["count"] // 3
                        for p in data["meshes"][0]["primitives"])
        assert triangles == record["triangle_count"]
        position_accessors = [a for a in data["accessors"] if a["type"] == "VEC3" and "min" in a]
        assert position_accessors
        floor = min(a["min"][1] for a in position_accessors)
        assert abs(floor) < 1e-5, (record["faf_id"], floor)
        assert all(v > 0 for v in record["dimensions_m"])
        total += triangles
        print(f"FAF_POND_GROUND_STATIC {record['faf_id']} tris={triangles} mats={len(data['materials'])} origin_y={floor:.5f}")
    print(f"FAF_POND_GROUND_STATIC_OK assets=8 triangles={total} materials=4")


if __name__ == "__main__":
    check(Path(sys.argv[1] if len(sys.argv) > 1 else ".").resolve())
