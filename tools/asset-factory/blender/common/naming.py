"""Pure-Python canonical FAF ID and Blender name construction."""

import re


CATEGORIES = frozenset({"FISH", "CORAL", "ROCK", "PLANT", "TERRAIN", "DETAIL", "CREATURE", "WOOD", "INSECT"})
ACTION_ROLES = frozenset({
    "Idle", "Glide", "SwimSlow", "Swim", "SwimFast", "TurnLeft", "TurnRight",
    "IdleShore", "SurfaceFloat", "Hop", "Dive", "IdleFloat", "SurfaceSwim",
    "UnderwaterSwim", "ShoreCrawl", "IdleBottom", "BottomWalk", "Crawl", "Row",
})
_ID = re.compile(r"^FAF-(FISH|CORAL|ROCK|PLANT|TERRAIN|DETAIL|CREATURE|WOOD|INSECT)-([0-9]{3,})$")
_SLUG = re.compile(r"^[a-z][a-z0-9]*(?:_[a-z0-9]+)*$")
_PART = re.compile(r"^[a-z][a-z0-9]*(?:_[a-z0-9]+)*$")


def parse_id(faf_id: str) -> tuple[str, str]:
    """Return category and number, rejecting experiment aliases and malformed IDs."""
    match = _ID.fullmatch(faf_id)
    if not match:
        raise ValueError(f"invalid canonical FAF ID: {faf_id!r}")
    return match.group(1), match.group(2)


def asset_stem(faf_id: str, slug: str) -> str:
    category, number = parse_id(faf_id)
    if not _SLUG.fullmatch(slug):
        raise ValueError(f"invalid asset slug: {slug!r}")
    return f"{category.lower()}_{number}_{slug}"


def object_name(faf_id: str, slug: str, part: str) -> str:
    if not _PART.fullmatch(part):
        raise ValueError(f"invalid object part: {part!r}")
    return f"{asset_stem(faf_id, slug)}__{part}"


def action_name(faf_id: str, slug: str, role: str) -> str:
    if role not in ACTION_ROLES:
        raise ValueError(f"unknown standard action role: {role!r}")
    return f"{asset_stem(faf_id, slug)}__{role}"
