#!/usr/bin/env python3
"""Reject runtime audio literals outside the owner-approved audio folder."""

from pathlib import Path
import os
import re
import sys


ROOT = Path(__file__).resolve().parent.parent
GAME = ROOT / "game"
APPROVED = (ROOT / "assets" / "sfx").resolve()
PATH = re.compile(r"res://[^\s\"'<>]*?\.(?:wav|ogg|mp3|flac)\b", re.IGNORECASE)
SOURCE_SUFFIXES = {".cs", ".gd", ".tscn", ".tres", ".godot"}
SKIP_DIRS = {".godot", "bin", "obj", ".git"}


def main() -> int:
    errors = []
    checked = set()
    for directory, dirs, files in os.walk(GAME):
        dirs[:] = [name for name in dirs if name not in SKIP_DIRS]
        for name in files:
            source = Path(directory) / name
            if source.suffix not in SOURCE_SUFFIXES:
                continue
            for number, line in enumerate(source.read_text(encoding="utf-8").splitlines(), 1):
                for match in PATH.finditer(line):
                    resource = match.group()
                    target = (GAME / resource.removeprefix("res://")).resolve()
                    checked.add(resource)
                    if not target.is_relative_to(APPROVED):
                        errors.append(f"{source.relative_to(ROOT)}:{number}: audio outside assets/sfx: {resource}")
                    elif not target.is_file():
                        errors.append(f"{source.relative_to(ROOT)}:{number}: missing approved audio: {resource}")
    if errors:
        print("\n".join(errors), file=sys.stderr)
        return 1
    print(f"AUDIO_PATHS_OK {len(checked)} runtime audio files under assets/sfx")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
