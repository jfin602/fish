#!/usr/bin/env python3
"""Compare the P5 scene-only catalog with both owner FAF expansion manifests."""

from __future__ import annotations

import hashlib
import json
from pathlib import Path


ROOT = Path(__file__).resolve().parents[2]
PLANTS = ROOT / "assets/faf/pond/plants/pond_stealth_vegetation_001_manifest.json"
WOOD = ROOT / "assets/faf/pond/wood/pond_wood_002_manifest.json"
CATALOG = ROOT / "game/assets/faf/pond/p5_cover_catalog.json"


def sha(path: Path) -> str:
    return hashlib.sha256(path.read_bytes()).hexdigest()


def require(test: bool, message: str) -> None:
    if not test:
        raise SystemExit("PIKE_COVER_INTAKE_FAILED " + message)


def run() -> None:
    plant_manifest = json.loads(PLANTS.read_text())
    wood_manifest = json.loads(WOOD.read_text())
    snapshot = json.loads(CATALOG.read_text())["Assets"]
    entries = plant_manifest["assets"] + wood_manifest["assets"]
    require(len(plant_manifest["assets"]) == 13 and len(wood_manifest["assets"]) == 10,
            "producer pack inventory")
    require(len(snapshot) == 23 and len({a["Id"] for a in snapshot}) == 23,
            "scene catalog inventory")
    old = set()
    for path in (ROOT / "assets/faf/pond/plants/pond_foliage_001_manifest.json",
                 ROOT / "assets/faf/pond/wood/pond_wood_001_manifest.json"):
        old.update(a["faf_id"] for a in json.loads(path.read_text())["assets"])
    require(not old.intersection(a["Id"] for a in snapshot), "old pack ID substitution")
    by_id = {a["Id"]: a for a in snapshot}
    require({a["faf_id"] for a in entries} == set(by_id), "manifest/catalog ID drift")
    plant_recipe = (ROOT / "tools/asset-factory/blender/plants/build_pond_stealth_vegetation_001.py").read_text()
    wood_recipe = (ROOT / "tools/asset-factory/blender/wood/build_pond_wood_002.py").read_text()
    require((ROOT / "tools/asset-factory/blender/plants/pond_stealth_vegetation.py").is_file() and
            (ROOT / "tools/asset-factory/blender/wood/pond_cover_expansion.py").is_file(),
            "generator source missing")
    families = set()
    for entry in entries:
        item = by_id[entry["faf_id"]]
        plant = entry["faf_id"].startswith("FAF-PLANT-")
        source = entry["source"] if plant else entry["blender_source"]
        glb = entry["glb"]
        wrapper = entry.get("godot_wrapper")
        require(entry["habitat"] == "pond" and item["Family"] == entry["family"],
                "habitat/family " + item["Id"])
        require(item["Seed"] == entry["seed"] and str(item["Seed"]) in
                (plant_recipe if plant else wood_recipe), "generator seed " + item["Id"])
        if plant:
            require(entry["spec"]["seed"] == item["Seed"] and entry["placement"]["anchor"] == "mud_root" and
                    entry["collision"].startswith("none") and
                    -.05 <= entry["bounds_min_m_blender_xyz"][2] <= .01,
                    "plant root/soft collision " + item["Id"])
        else:
            require(entry["generator_config"]["seed"] == item["Seed"] and
                    entry["collision_count"] == len(entry["collision_shapes"]) and
                    entry["collision_count"] > 0 and wrapper, "wood wrapper " + item["Id"])
        require(item["Source"] == source and item["Glb"] == "res://" + glb.removeprefix("game/") and
                item["Wrapper"] == ("res://" + wrapper.removeprefix("game/") if wrapper else None),
                "source/export/wrapper path " + item["Id"])
        require(item["Size"] == entry.get("dimensions_m_blender_xyz", entry.get("dimensions_m")) and
                item["Triangles"] == entry.get("triangles", entry.get("triangle_count")) and
                item["Materials"] == entry.get("materials", entry.get("material_count")) and
                item["CollisionShapes"] == entry.get("collision_count", 0),
                "size/cost " + item["Id"])
        require(sha(ROOT / source) == item["SourceSha256"] and
                sha(ROOT / glb) == item["GlbSha256"] and
                (not wrapper or (ROOT / wrapper).is_file()), "source/export hash " + item["Id"])
        families.add(entry["family"])
    require(len(families) == 10, "structural family count")
    grass = sorted(a["dimensions_m_blender_xyz"][2] for a in plant_manifest["assets"]
                   if a["family"] == "ribbon_grass")
    require(len(grass) == 3 and .5 <= grass[0] <= 1 and 1 <= grass[1] <= 2 and grass[2] >= 3.5,
            "short/medium/surface grass classes")
    print("PIKE_COVER_INTAKE_OK plants=13 wood=10 families=10 "
          f"plant_manifest_sha={sha(PLANTS)} wood_manifest_sha={sha(WOOD)}")


if __name__ == "__main__":
    run()
