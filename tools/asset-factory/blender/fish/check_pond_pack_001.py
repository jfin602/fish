"""Check all 25 Pond exports, rigs, metadata, actions and stage progression."""

import argparse
import hashlib
import json
from pathlib import Path
import struct

SPECIES=(6,7,8,9,10)
STAGES=("egg","larva","juvenile","subadult","adult")
SWIM_ROLES=("Idle","Glide","SwimSlow","Swim","SwimFast","TurnLeft","TurnRight")
LOOP_ROLES=("Idle","Glide","SwimSlow","Swim","SwimFast")


def glb(path):
    blob=path.read_bytes()
    if blob[:4]!=b"glTF" or struct.unpack_from("<I",blob,4)[0]!=2:
        raise ValueError(f"invalid GLB: {path}")
    size,kind=struct.unpack_from("<II",blob,12)
    if kind!=0x4E4F534A:
        raise ValueError(f"missing JSON chunk: {path}")
    return json.loads(blob[20:20+size]),blob[28+size:]


def _values(data,binary,accessor_index):
    accessor=data["accessors"][accessor_index]
    count=accessor["count"]
    kind=accessor["type"]
    components={"SCALAR":1,"VEC3":3,"VEC4":4}.get(kind)
    if accessor["componentType"]!=5126 or components is None:
        return []
    view=data["bufferViews"][accessor["bufferView"]]
    offset=view.get("byteOffset",0)+accessor.get("byteOffset",0)
    stride=view.get("byteStride",components*4)
    return [struct.unpack_from("<"+"f"*components,binary,offset+i*stride)
            for i in range(count)]


def _joint_values(data,binary,accessor_index):
    accessor=data["accessors"][accessor_index]
    if accessor["type"]!="VEC4" or accessor["componentType"] not in (5121,5123):
        raise ValueError("unsupported exported joint format")
    view=data["bufferViews"][accessor["bufferView"]]
    component="B" if accessor["componentType"]==5121 else "H"
    width=4 if component=="B" else 8
    offset=view.get("byteOffset",0)+accessor.get("byteOffset",0)
    stride=view.get("byteStride",width)
    return [struct.unpack_from("<"+component*4,binary,offset+i*stride)
            for i in range(accessor["count"])]


def check(root):
    manifest_path=root/"assets/faf/pond/fish/pond_fish_001_manifest.json"
    manifest=json.loads(manifest_path.read_text())
    assets=manifest["assets"]
    if manifest["habitat"]!="pond" or len(assets)!=25:
        raise ValueError("Pond manifest needs exactly 25 pond assets")
    pairs={(a["faf_id"],a["life_stage"]) for a in assets}
    if len(pairs)!=25:
        raise ValueError("duplicate Pond identity/stage")
    for number in SPECIES:
        faf_id=f"FAF-FISH-{number:03d}"
        if {(faf_id,stage) for stage in STAGES} - pairs:
            raise ValueError(f"missing stages for {faf_id}")
    digests=set()
    stats=[]
    for asset in assets:
        stem=asset["stem"]
        path=root/asset["glb"]
        data,binary=glb(path)
        if asset["habitat"]!="pond" or "/pond/" not in asset["glb"]:
            raise ValueError(f"{stem} habitat/path mismatch")
        meshes=data.get("meshes",[])
        triangles=sum(data["accessors"][part["indices"]]["count"]//3
                      for mesh in meshes for part in mesh["primitives"])
        if triangles!=asset["triangles"] or triangles<100:
            raise ValueError(f"{stem} triangle mismatch")
        if len(data.get("materials",[]))!=asset["materials"]:
            raise ValueError(f"{stem} material mismatch")
        nodes=data.get("nodes",[])
        extras=[node.get("extras",{}) for node in nodes]
        if not any(e.get("faf_id")==asset["faf_id"] and
                   e.get("faf_life_stage")==asset["life_stage"] and
                   e.get("faf_habitat")=="pond" for e in extras):
            raise ValueError(f"{stem} missing exported Pond metadata")
        skins=data.get("skins",[])
        if len(skins)!=1 or len(skins[0]["joints"])!=asset["bones"]:
            raise ValueError(f"{stem} missing real deform skeleton")
        bones={nodes[index].get("name") for index in skins[0]["joints"]}
        if asset["life_stage"]=="egg":
            required={"root","shell_l","shell_r","embryo"}
            expected={stem+"__Idle",stem+"__Hatch"}
            if asset["bones"]!=4 or asset["breathing_anatomy"]!="not_applicable_egg":
                raise ValueError(f"{stem} egg rig/classification")
        else:
            required={"root","spine_01","spine_02","spine_03","tail_01",
                      "tail_02","tail_fin","pectoral_l","pectoral_r","dorsal",
                      "anal","gill_L","gill_R"}
            expected={stem+"__"+role for role in SWIM_ROLES}
            if asset["bones"]!=13 or asset["breathing_anatomy"]!="operculate_bony" or not asset["breathing_validation"]["complete"]:
                raise ValueError(f"{stem} incomplete swimmer rig")
        if not required <= bones:
            raise ValueError(f"{stem} missing deform roles {required-bones}")
        joint_names=[nodes[index].get("name") for index in skins[0]["joints"]]
        weighted={name:0 for name in (("shell_l","shell_r","embryo") if asset["life_stage"]=="egg"
                                     else ("gill_L","gill_R"))}
        for mesh in meshes:
            for primitive in mesh["primitives"]:
                attributes=primitive["attributes"]
                if "JOINTS_0" not in attributes or "WEIGHTS_0" not in attributes:
                    raise ValueError(f"{stem} unskinned exported primitive")
                joints=_joint_values(data,binary,attributes["JOINTS_0"])
                weights=_values(data,binary,attributes["WEIGHTS_0"])
                if len(joints)!=len(weights):
                    raise ValueError(f"{stem} malformed exported weights")
                for vertex_joints,vertex_weights in zip(joints,weights):
                    for index,weight in zip(vertex_joints,vertex_weights):
                        if weight>.01 and joint_names[index] in weighted:
                            weighted[joint_names[index]]+=1
        if any(count==0 for count in weighted.values()):
            raise ValueError(f"{stem} missing exported deform weights {weighted}")
        clips=data.get("animations",[])
        if {clip["name"] for clip in clips}!=expected:
            raise ValueError(f"{stem} action names mismatch")
        for clip in clips:
            role=clip["name"].removeprefix(stem+"__")
            if role in LOOP_ROLES or role=="Idle":
                loop=True
            else:
                loop=False
            if role=="Hatch" and any("tail" in nodes[channel["target"]["node"]].get("name","")
                                      for channel in clip["channels"]):
                raise ValueError(f"{stem} egg has fake swim track")
            if asset["life_stage"]!="egg" and role in ("Swim","SwimFast","TurnLeft","TurnRight"):
                names={nodes[channel["target"]["node"]].get("name","") for channel in clip["channels"]}
                if not any(name.startswith("tail_") for name in names):
                    raise ValueError(f"{stem} {role} has no tail movement")
            inputs=[data["accessors"][sampler["input"]] for sampler in clip["samplers"]]
            if not inputs or max(a.get("max",[0])[0] for a in inputs)<=0:
                raise ValueError(f"{stem} empty Action {role}")
            for channel in clip["channels"]:
                target=nodes[channel["target"]["node"]].get("name")
                kind=channel["target"]["path"]
                values=_values(data,binary,clip["samplers"][channel["sampler"]]["output"])
                if target=="root" and values and any(max(abs(a-b) for a,b in zip(values[0],v))>1e-6 for v in values[1:]):
                    raise ValueError(f"{stem} {role} animates gameplay root")
                if target=="spine_01" and kind=="rotation" and values and any(max(abs(a-b) for a,b in zip(values[0],v))>1e-6 for v in values[1:]):
                    raise ValueError(f"{stem} {role} moves stable head")
                if loop and kind=="rotation" and values and max(abs(a-b) for a,b in zip(values[0],values[-1]))>2e-4:
                    raise ValueError(f"{stem} {role} loop seam")
        geometry_hash=hashlib.sha256(json.dumps(meshes,sort_keys=True).encode()+binary).hexdigest()
        if geometry_hash in digests:
            raise ValueError(f"{stem} duplicates another export")
        digests.add(geometry_hash)
        stats.append((asset["faf_id"],asset["life_stage"],triangles,asset["materials"],asset["bones"]))
    for number in SPECIES:
        faf_id=f"FAF-FISH-{number:03d}"
        group=[next(a for a in assets if a["faf_id"]==faf_id and a["life_stage"]==stage) for stage in STAGES]
        lengths=[a["dimensions_m"][1] for a in group]
        if lengths!=sorted(lengths) or len(set(lengths))!=5:
            raise ValueError(f"{faf_id} length progression")
        ratios=[round(a["dimensions_m"][2]/a["dimensions_m"][1],3) for a in group[1:]]
        if len(set(ratios))<3:
            raise ValueError(f"{faf_id} scaled adult duplicates")
    return {"assets":len(stats),"species":5,"egg_rigs":5,"swimming_rigs":20,
            "triangles_range":[min(s[2] for s in stats),max(s[2] for s in stats)],
            "materials_range":[min(s[3] for s in stats),max(s[3] for s in stats)]}


if __name__=="__main__":
    parser=argparse.ArgumentParser();parser.add_argument("--root",type=Path,required=True)
    options=parser.parse_args()
    print("FAF_POND_CHECK="+json.dumps(check(options.root.resolve())))
