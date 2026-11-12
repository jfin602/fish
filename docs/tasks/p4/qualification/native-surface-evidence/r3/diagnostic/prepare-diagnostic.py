#!/usr/bin/env python3
"""Copy this diagnostic and exact shader sources to an isolated scratch project.

Does not install tools, mutate Fish, or run its .NET project.
"""
from __future__ import annotations

import argparse
import hashlib
import json
from pathlib import Path
import shlex
import shutil
import subprocess
import tempfile

BASE_REVISION = "cc8cbb99de16f3b6138a046272721d986274a058"
CURRENT_SHADERS = (
    "pond_water_surface.gdshader",
    "pond_sky.gdshader",
    "pond_air_sky.gdshaderinc",
)
BASELINE_SHADERS = ("water_surface.gdshader", "underwater_sky.gdshader")


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--repo", type=Path, required=True, help="Fish repository checkout")
    parser.add_argument("--out", type=Path, help="Empty scratch directory outside Fish; defaults to a new temporary directory")
    args = parser.parse_args()
    repo = args.repo.expanduser().resolve()
    evidence = Path(__file__).resolve().parent
    game = repo / "game"
    if not (game / "project.godot").is_file():
        parser.error("--repo must point to the Fish repository, containing game/project.godot")
    metadata = json.loads((evidence / "metadata.json").read_text())
    sources: dict[str, bytes] = {}
    for name in CURRENT_SHADERS:
        data = (game / "shaders" / name).read_bytes()
        expected = metadata["shader_sha256"]["shaders/" + name]
        if hashlib.sha256(data).hexdigest() != expected:
            parser.error(f"{name} differs from the recorded candidate; use the source revision associated with this evidence")
        sources["shaders/" + name] = data
    for name in BASELINE_SHADERS:
        result = subprocess.run(
            ["git", "-C", str(repo), "show", f"{BASE_REVISION}:game/shaders/{name}"],
            check=True, capture_output=True,
        )
        expected = metadata["shader_sha256"]["baseline/" + name]
        if hashlib.sha256(result.stdout).hexdigest() != expected:
            parser.error(f"Historical shader hash mismatch: {name}")
        sources["baseline/" + name] = result.stdout

    output = args.out.expanduser().resolve() if args.out else Path(tempfile.mkdtemp(prefix="fish-water-diagnostic-"))
    if output == repo or output.is_relative_to(repo):
        parser.error("--out must be outside the Fish checkout; this fixture writes only to scratch")
    if output.exists() and any(output.iterdir()):
        parser.error("--out must be empty")
    output.mkdir(parents=True, exist_ok=True)
    for name in ("project.godot", "fixture.tscn", "fixture.gd"):
        shutil.copyfile(evidence / name, output / name)
    for relative, data in sources.items():
        target = output / relative
        target.parent.mkdir(parents=True, exist_ok=True)
        target.write_bytes(data)
    (output / "captures").mkdir()
    (output / "captures" / ".gdignore").touch()
    print(f"Prepared scratch fixture: {output}")
    print(f"export FISH_DIAGNOSTIC_GAME_ROOT={shlex.quote(str(game))}")
    command = f"godot --path {shlex.quote(str(output))} --rendering-method forward_plus --audio-driver Dummy --"
    for mode, shader in (
        ("baseline", "baseline/water_surface.gdshader"),
        ("candidate", "shaders/pond_water_surface.gdshader"),
        ("sky-only", "shaders/pond_water_surface.gdshader"),
        ("reflection-off", "shaders/pond_water_surface.gdshader"),
        ("surface-baseline", "baseline/water_surface.gdshader"),
    ):
        print(f"{command} {mode} res://{shader}")


if __name__ == "__main__":
    main()
