"""Focused source-manifest/GLB identity checks for the FAF life-stage library."""

import argparse
import json
from pathlib import Path
import struct

STAGES={1:("egg","larva","juvenile","subadult"),
        2:("egg","larva","juvenile","subadult"),
        3:("egg","larva","juvenile","subadult"),
        4:("egg","larva","juvenile","subadult"),
        5:("newborn_pup","early_juvenile","juvenile","subadult")}
ROLES=("Glide","Swim","SwimFast")


def glb_json(path):
    blob=path.read_bytes()
    if blob[:4]!=b"glTF" or struct.unpack_from("<I",blob,4)[0]!=2:
        raise ValueError(f"invalid GLB: {path}")
    length,kind=struct.unpack_from("<II",blob,12)
    if kind!=0x4e4f534a:
        raise ValueError(f"missing JSON chunk: {path}")
    data=json.loads(blob[20:20+length])
    binary_start=20+length+8
    return data,blob[binary_start:]


def translation_varies(data,binary,accessor_index):
    accessor=data["accessors"][accessor_index]
    if accessor["componentType"]!=5126 or accessor["type"]!="VEC3":
        raise ValueError("unexpected translation accessor format")
    view=data["bufferViews"][accessor["bufferView"]]
    start=view.get("byteOffset",0)+accessor.get("byteOffset",0)
    stride=view.get("byteStride",12)
    values=[struct.unpack_from("<fff",binary,start+i*stride)
            for i in range(accessor["count"])]
    return any(max(abs(a-b) for a,b in zip(values[0],value))>1e-6
               for value in values[1:])


def check(root):
    manifest=json.loads((root/"assets/faf/fish/life_stages_001_to_005_manifest.json").read_text())
    assets=manifest["assets"]
    if len(assets)!=20:
        raise ValueError("expected exactly twenty new developmental assets")
    keys={(a["faf_id"],a["life_stage"]) for a in assets}
    if len(keys)!=20:
        raise ValueError("duplicate FAF identity/stage")
    for number,stages in STAGES.items():
        if {stage for faf_id,stage in keys if faf_id==f"FAF-FISH-{number:03d}"}!=set(stages):
            raise ValueError(f"missing stage for FAF-FISH-{number:03d}")
    for asset in assets:
        path=root/asset["glb"]
        data,binary=glb_json(path)
        meshes=data.get("meshes",[])
        triangles=sum(data["accessors"][primitive["indices"]]["count"]//3
                      for mesh in meshes for primitive in mesh["primitives"])
        if triangles!=asset["triangles"]:
            raise ValueError(f"{asset['stem']} GLB triangles {triangles} != {asset['triangles']}")
        if len(data.get("materials",[]))!=asset["materials"]:
            raise ValueError(f"{asset['stem']} GLB material count")
        if min(asset["dimensions_m"])<=0 or max(asset["dimensions_m"])>3:
            raise ValueError(f"{asset['stem']} invalid source dimensions")
        extras=[node.get("extras",{}) for node in data.get("nodes",[])]
        if not any(extra.get("faf_id")==asset["faf_id"] and
                   extra.get("faf_life_stage")==asset["life_stage"] for extra in extras):
            raise ValueError(f"{asset['stem']} GLB lost stage metadata")
        swimming=asset["life_stage"]!="egg"
        if swimming:
            skins=data.get("skins",[])
            if len(skins)!=1 or len(skins[0]["joints"])!=asset["bones"]:
                raise ValueError(f"{asset['stem']} wrong skin/bones")
            names={data["nodes"][index].get("name") for index in skins[0]["joints"]}
            required={"root","spine_01","tail_01","tail_fin","pectoral_l",
                      "pectoral_r","dorsal","anal","gill_L","gill_R"}
            if not required <= names:
                raise ValueError(f"{asset['stem']} missing rig roles: {required-names}")
            anatomy=("exposed_gill_slits_rigged" if asset["faf_id"]=="FAF-FISH-005"
                     else "operculate_bony")
            if asset["breathing_anatomy"]!=anatomy or not asset["breathing_validation"]["complete"]:
                raise ValueError(f"{asset['stem']} breathing classification incomplete")
            if not any(extra.get("faf_breathing_anatomy")==anatomy for extra in extras):
                raise ValueError(f"{asset['stem']} GLB lost breathing metadata")
            expected={asset["stem"]+"__"+role for role in ROLES}
            animations=data.get("animations",[])
            if {clip["name"] for clip in animations}!=expected:
                raise ValueError(f"{asset['stem']} missing or cross-species Actions")
            for clip in animations:
                for channel in clip["channels"]:
                    if channel["target"]["path"]=="translation":
                        target=data["nodes"][channel["target"]["node"]].get("name")
                        if target=="root" and translation_varies(
                            data,binary,clip["samplers"][channel["sampler"]]["output"]):
                            raise ValueError(f"{asset['stem']} Action moves gameplay root")
                inputs=[data["accessors"][sampler["input"]] for sampler in clip["samplers"]]
                if not inputs or max(accessor.get("max",[0])[0] for accessor in inputs)<=0:
                    raise ValueError(f"{asset['stem']} zero-length Action")
                animated={data["nodes"][channel["target"]["node"]].get("name")
                          for channel in clip["channels"]}
                if not any(name.startswith("tail_") for name in animated if name):
                    raise ValueError(f"{asset['stem']} Action has no tail track")
        elif data.get("skins") or data.get("animations") or asset["bones"]!=0:
            raise ValueError(f"{asset['stem']} egg has swimming skeleton/Actions")
    for number,stages in STAGES.items():
        group=[next(a for a in assets if a["faf_id"]==f"FAF-FISH-{number:03d}"
                    and a["life_stage"]==stage) for stage in stages]
        lengths=[a["dimensions_m"][1] for a in group]
        if lengths!=sorted(lengths) or len(set(lengths))!=len(lengths):
            raise ValueError(f"FAF-FISH-{number:03d} lengths do not progress")
        ratios=[round(a["dimensions_m"][2]/a["dimensions_m"][1],2) for a in group[1:]]
        if len(set(ratios))<2:
            raise ValueError(f"FAF-FISH-{number:03d} forms differ only in scale")
    for adult in manifest["adult_assets"]:
        if not (root/adult["glb"]).is_file():
            raise ValueError(f"canonical adult missing: {adult['faf_id']}")
    return {"new_assets":len(assets),"canonical_adults":5,
            "total_stage_representations":len(assets)+5}


if __name__=="__main__":
    parser=argparse.ArgumentParser();parser.add_argument("--root",type=Path,required=True)
    options=parser.parse_args()
    print("FAF_LIFE_STAGES_CHECK="+json.dumps(check(options.root.resolve())))
