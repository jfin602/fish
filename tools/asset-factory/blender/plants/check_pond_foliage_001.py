"""Focused binary GLB checks for the Pond foliage manifest and export metadata."""

from __future__ import annotations

import json
from pathlib import Path
import struct
import sys


def glb_json(path: Path):
    raw = path.read_bytes()
    if raw[:4] != b"glTF" or struct.unpack_from("<I", raw, 4)[0] != 2:
        raise AssertionError(f"invalid GLB header: {path}")
    if struct.unpack_from("<I", raw, 8)[0] != len(raw):
        raise AssertionError(f"invalid GLB length: {path}")
    length, chunk_type = struct.unpack_from("<II", raw, 12)
    if chunk_type != 0x4E4F534A:
        raise AssertionError(f"missing GLB JSON chunk: {path}")
    return json.loads(raw[20:20 + length])


def check(root: Path):
    manifest = json.loads((root / "assets/faf/pond/plants/pond_foliage_001_manifest.json").read_text())
    entries = manifest["assets"]
    assert len(entries) == 13
    assert len({a["faf_id"] for a in entries}) == 13
    total = 0
    for entry in entries:
        faf_id = entry["faf_id"]
        assert entry["habitat"] == "pond" and entry["source"] == manifest["source"]
        assert (root / entry["source"]).is_file()
        assert entry["scale"] == [1, 1, 1] and entry["origin_m"] == [0, 0, 0]
        data = glb_json(root / entry["glb"])
        nodes = [n for n in data["nodes"] if "mesh" in n]
        assert len(nodes) == len(entry["components"]), faf_id
        labels = {n["name"].rsplit("__", 1)[-1] for n in nodes}
        assert labels == set(entry["components"]), faf_id
        for node in nodes:
            extras = node.get("extras", {})
            assert extras.get("faf_id") == faf_id and extras.get("faf_habitat") == "pond", faf_id
            assert extras.get("faf_family") == entry["family"] and extras.get("faf_seed") == entry["seed"], faf_id
            assert not any(k in node for k in ("translation", "rotation", "scale")), faf_id
        count = sum(data["accessors"][primitive["indices"]]["count"] // 3
                    for mesh in data["meshes"] for primitive in mesh["primitives"])
        assert count == entry["triangles"], (faf_id, count, entry["triangles"])
        assert len(data["materials"]) == entry["materials"], faf_id
        assert all(m.get("alphaMode", "OPAQUE") == "OPAQUE" for m in data["materials"]), faf_id
        assert not data.get("textures"), faf_id
        if entry["family"] == "lily_pad":
            assert labels == {"pad", "stem"}
            assert abs(entry["bounds_min_m_blender_xyz"][2] + entry["placement"]["mud_depth_m"]) < 1e-4
        total += count
        print(f"FAF_POND_FOLIAGE_GLB {faf_id} triangles={count} components={len(nodes)}")
    print(f"FAF_POND_FOLIAGE_GLB_OK assets=13 triangles={total}")


if __name__ == "__main__":
    check(Path(sys.argv[1] if len(sys.argv) > 1 else ".").resolve())
