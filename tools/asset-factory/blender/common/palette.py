"""Small editable sRGB palette seeds; Blender material conversion remains explicit."""

FAMILIES = {
    "reef_warm": {"red": "#DB6354", "orange": "#F29449", "pink": "#E85D86", "yellow": "#E6B85C"},
    "reef_cool": {"blue": "#248FC0", "cyan": "#38B8C2", "teal": "#268D91", "violet": "#9169C9", "muted_purple": "#9B75A9"},
    "vegetation": {"green": "#46A967", "deep_green": "#287B66",
                   "yellow_green": "#91B85A", "muted_teal": "#4B948A"},
    "sand": {"warm_light": "#D8D4AD"},
    "rock": {"cool_slate": "#7798A9", "warm_stone": "#A29B89"},
    "fish": {
        "yellow": "#F2C94C", "orange": "#ED8B45", "blue": "#377BB2",
        "white": "#ECECE2", "black": "#1E2833",
    },
}


def hex_rgb(family: str, name: str) -> tuple[float, float, float]:
    """Return normalized sRGB components for explicit downstream conversion."""
    value = FAMILIES[family][name].lstrip("#")
    return tuple(int(value[index:index + 2], 16) / 255 for index in (0, 2, 4))
