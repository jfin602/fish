"""Inspect the produced GLB bytes, including Pond metadata and clean skin clips."""

from pathlib import Path
import argparse
import json
import struct


def read_glb(path):
    blob=path.read_bytes()
    if blob[:4]!=b"glTF" or struct.unpack_from("<I",blob,4)[0]!=2:
        raise ValueError(f"invalid GLB header: {path}")
    if struct.unpack_from("<I",blob,8)[0]!=len(blob):
        raise ValueError(f"invalid GLB length: {path}")
    length,kind=struct.unpack_from("<II",blob,12)
    if kind!=0x4E4F534A:
        raise ValueError(f"missing JSON chunk: {path}")
    return json.loads(blob[20:20+length]),blob[28+length:]


def accessor_vectors(data,binary,index):
    accessor=data["accessors"][index]
    view=data["bufferViews"][accessor["bufferView"]]
    width={"SCALAR":1,"VEC3":3,"VEC4":4}[accessor["type"]]
    if accessor["componentType"]!=5126:
        raise ValueError("unexpected non-float animation channel")
    start=view.get("byteOffset",0)+accessor.get("byteOffset",0)
    stride=view.get("byteStride",4*width)
    return [struct.unpack_from("<"+"f"*width,binary,start+i*stride)
            for i in range(accessor["count"])]


def check(root):
    records=json.loads((root/"assets/faf/pond/creatures/pond_creature_001_manifest.json").read_text())["assets"]
    for record in records:
        data,binary=read_glb(root/record["glb"])
        nodes=data["nodes"]
        if len(data.get("skins",[]))!=1:
            raise ValueError(record["stem"]+" expected one deform skin")
        skin=data["skins"][0]
        if len(skin["joints"])!=record["bones"]:
            raise ValueError(record["stem"]+" joint count differs")
        names={node["name"] for node in nodes}
        if not set(record["bone_names"]).issubset(names):
            raise ValueError(record["stem"]+" missing named joints")
        if {a["name"] for a in data.get("animations",[])}!=set(record["actions"]):
            raise ValueError(record["stem"]+" Action inventory differs")
        mats={material["name"] for material in data.get("materials",[])}
        if mats!=set(record["material_names"]):
            raise ValueError(record["stem"]+" material inventory differs")
        skinned=0
        for node in nodes:
            if "mesh" in node:
                skinned+=1
                if node.get("skin")!=0:
                    raise ValueError(record["stem"]+" has unskinned mesh")
                if node.get("extras",{}).get("faf_habitat")!="pond" or node["extras"].get("faf_id")!=record["faf_id"]:
                    raise ValueError(record["stem"]+" missing Pond identity extras")
                for primitive in data["meshes"][node["mesh"]]["primitives"]:
                    if not {"POSITION","JOINTS_0","WEIGHTS_0"}.issubset(primitive["attributes"]):
                        raise ValueError(record["stem"]+" mesh lacks skin attributes")
        root_index=next(i for i,node in enumerate(nodes) if node.get("name")=="root")
        for action in data["animations"]:
            for channel in action["channels"]:
                if channel["target"]["node"]==root_index:
                    # Blender's NLA export writes sampled constant root channels
                    # even without root keyframes. Reject any actual root change.
                    sampler=action["samplers"][channel["sampler"]]
                    values=accessor_vectors(data,binary,sampler["output"])
                    if any(max(abs(a-b) for a,b in zip(values[0],value))>1e-5
                           for value in values[1:]):
                        raise ValueError(action["name"]+" moves gameplay root")
        print("FAF_POND_CREATURE_GLB_OK",record["stem"],"skins=1",
              "skinned_meshes="+str(skinned),"actions="+str(len(data["animations"])))
    return len(records)


if __name__=="__main__":
    p=argparse.ArgumentParser();p.add_argument("--root",type=Path,required=True)
    a=p.parse_args();print("FAF_POND_CREATURE_EXPORTS_OK",check(a.root.resolve()))
