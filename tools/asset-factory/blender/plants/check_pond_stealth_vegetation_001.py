"""Inspect delivered GLB bytes, identities, costs and basic soft-cover contract."""

from __future__ import annotations

import json
from pathlib import Path
import struct
import sys


def glb_json(path: Path):
    raw = path.read_bytes()
    assert raw[:4] == b"glTF" and struct.unpack_from("<I", raw, 4)[0] == 2, path
    assert struct.unpack_from("<I", raw, 8)[0] == len(raw), path
    size, kind = struct.unpack_from("<II", raw, 12)
    assert kind == 0x4E4F534A, path
    return json.loads(raw[20:20+size])


def check(root: Path):
    manifest = json.loads((root / "assets/faf/pond/plants/pond_stealth_vegetation_001_manifest.json").read_text())
    assets = manifest["assets"]
    assert len(assets) == 13 and manifest["asset_count"] == 13
    assert len({a["faf_id"] for a in assets}) == 13
    assert {a["family"] for a in assets} == {
        "ribbon_grass", "branching_waterweed", "coontail", "broad_pondweed", "rooted_thicket"}
    total = 0
    for entry in assets:
        faf_id = entry["faf_id"]
        assert entry["habitat"] == "pond" and entry["source"] == manifest["source"]
        assert entry["collision"].startswith("none") and entry["placement"]["anchor"] == "mud_root"
        assert entry["origin_m"] == [0,0,0] and entry["scale"] == [1,1,1]
        assert (root / entry["source"]).is_file()
        assert abs(entry["bounds_min_m_blender_xyz"][2]) <= .035
        data = glb_json(root / entry["glb"])
        nodes = [n for n in data["nodes"] if "mesh" in n]
        assert {n["name"].rsplit("__",1)[-1] for n in nodes} == set(entry["components"]), faf_id
        for node in nodes:
            extras = node.get("extras",{})
            assert extras.get("faf_id") == faf_id and extras.get("faf_habitat") == "pond", faf_id
            assert extras.get("faf_family") == entry["family"] and extras.get("faf_seed") == entry["seed"], faf_id
            assert "translation" not in node and "rotation" not in node and "scale" not in node, faf_id
        tris = sum(data["accessors"][primitive["indices"]]["count"] // 3
                   for mesh in data["meshes"] for primitive in mesh["primitives"])
        assert tris == entry["triangles"], (faf_id,tris,entry["triangles"])
        assert len(data["materials"]) == entry["materials"], faf_id
        assert all(m.get("alphaMode","OPAQUE") == "OPAQUE" for m in data["materials"]), faf_id
        assert not data.get("textures") and not data.get("images"), faf_id
        total += tris
        print(f"POND_COVER_GLB {faf_id} triangles={tris} materials={len(data['materials'])}")
    grass = [a for a in assets if a["family"] == "ribbon_grass"]
    assert len(grass) == 3
    heights = sorted(a["dimensions_m_blender_xyz"][2] for a in grass)
    assert .5 < heights[0] < 1 and 1 < heights[1] < 2 and heights[2] > 3.5, heights
    assert len({a["spec"]["blade_count"] for a in grass}) == 3
    print(f"POND_COVER_GLB_OK assets={len(assets)} triangles={total} grass_heights={heights}")


if __name__ == "__main__":
    check(Path(sys.argv[1] if len(sys.argv)>1 else ".").resolve())
