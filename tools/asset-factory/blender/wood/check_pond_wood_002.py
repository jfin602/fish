"""Check ten source/GLB/wrapper identities and costs without Blender."""
import json
import struct
from pathlib import Path

ROOT=Path(__file__).resolve().parents[4]
data=json.loads((ROOT/"assets/faf/pond/wood/pond_wood_002_manifest.json").read_text())
items=data["assets"]
assert data["asset_count"]==10 and len(items)==10
assert len({x["faf_id"] for x in items})==10
assert len({x["family"] for x in items})==5
total=0
for item in items:
    assert item["habitat"]=="pond" and item["collision_count"]==len(item["collision_shapes"])>0
    for key in ("blender_source","glb","godot_wrapper"):
        assert (ROOT/item[key]).is_file(),(item["faf_id"],key)
    source=(ROOT/item["blender_source"]).read_bytes()
    assert source[:4] in (b"BLEN",b"\x28\xb5\x2f\xfd")
    raw=(ROOT/item["glb"]).read_bytes()
    assert raw[:4]==b"glTF" and struct.unpack_from("<I",raw,8)[0]==len(raw)
    length=struct.unpack_from("<I",raw,12)[0]
    gltf=json.loads(raw[20:20+length])
    assert any(node.get("extras",{}).get("faf_id")==item["faf_id"] and
               node.get("extras",{}).get("faf_habitat")=="pond" for node in gltf["nodes"])
    assert not gltf.get("animations") and not gltf.get("skins")
    assert all(mat.get("alphaMode","OPAQUE")=="OPAQUE" for mat in gltf["materials"])
    tris=sum(gltf["accessors"][p["indices"]]["count"]//3
             for mesh in gltf["meshes"] for p in mesh["primitives"])
    assert tris==item["triangle_count"] and len(gltf["materials"])==item["material_count"]
    total+=tris
    print("POND_WOOD_002_GLB",item["faf_id"],tris,item["material_count"],item["collision_count"])
print("POND_WOOD_002_GLB_OK assets=10 triangles=",total,sep="")
