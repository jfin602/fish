"""Check all twelve GLB payloads, metadata and editable sources with stdlib only."""
from __future__ import annotations
import json
import struct
from pathlib import Path


def check(root: Path) -> tuple[int,int]:
    manifest = json.loads((root/"assets/faf/pond/wood/pond_wood_001_manifest.json").read_text())
    assets = manifest["assets"]
    assert len(assets) == 12
    assert {e["family"] for e in assets} == {"fallen_log","hollow_trunk","stump","cluster"}
    assert len({e["faf_id"] for e in assets}) == 12
    total = 0
    for entry in assets:
        source = root/entry["blender_source"]
        glb = root/entry["glb"]
        wrapper = root/entry["godot_wrapper"]
        assert source.is_file() and glb.is_file() and wrapper.is_file(), entry["faf_id"]
        # Blender 5.2 may save native sources as Zstandard-compressed .blend.
        assert source.stat().st_size > 1000 and source.read_bytes()[:4] in (b"BLEN",b"\x28\xb5\x2f\xfd"), source
        raw = glb.read_bytes()
        assert raw[:4] == b"glTF" and struct.unpack_from("<I",raw,8)[0] == len(raw), glb
        json_length = struct.unpack_from("<I",raw,12)[0]
        data = json.loads(raw[20:20+json_length])
        assert not data.get("animations") and not data.get("skins"), glb
        extras = [n.get("extras",{}) for n in data["nodes"]]
        assert any(x.get("faf_id") == entry["faf_id"] and x.get("faf_habitat") == "pond" for x in extras), glb
        triangles = sum(data["accessors"][p["indices"]]["count"]//3
                        for mesh in data["meshes"] for p in mesh["primitives"])
        assert triangles == entry["triangle_count"] and len(data["materials"]) == entry["material_count"], glb
        assert entry["transform"] == "identity" and entry["scale_range"] == [.9,1.1]
        if entry["family"] == "hollow_trunk":
            assert entry["nominal_clear_diameter_m"] > 1.7 and "passage" in entry["role"] or entry["role"] == "limited refuge"
        total += triangles
        print("FAF_POND_WOOD_GLB_OK",entry["faf_id"],triangles,len(data["materials"]))
    print("FAF_POND_WOOD_EXPORTS_OK",len(assets),total)
    return len(assets),total


if __name__ == "__main__":
    check(Path(__file__).resolve().parents[4])
