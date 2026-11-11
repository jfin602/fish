"""Deterministic developmental profiles for the five registered FAF fish.

Profiles change anatomy independently of physical length. Adult meshes are never
generated here: the five canonical adult sources and GLBs remain authoritative.
"""

from __future__ import annotations

from dataclasses import dataclass, replace

from fish.breathing import OPERCULATE, SLIT_RIGGED
from fish.reef_families import Blueprint, BLUEPRINTS


@dataclass(frozen=True)
class Development:
    stage: str
    size: float
    head_length: float
    head_fullness: float
    peduncle_fullness: float
    depth: float
    width: float
    eye_ratio: float
    fin: float
    caudal: float
    cadence: float
    wave: float

    def validate(self):
        if not .05 <= self.size < 1 or not .55 <= self.depth <= 1.5:
            raise ValueError(f"invalid developmental size/depth: {self}")
        if min(self.width, self.eye_ratio, self.fin, self.caudal, self.cadence, self.wave) <= 0:
            raise ValueError(f"invalid developmental profile: {self}")


# These are authored developmental forms, not a single stage-scale ladder.
# The head, body, peduncle, eyes, fins, tail and wave may all vary independently.
PROFILES: dict[int, tuple[Development, ...]] = {
    1: (
        Development("larva", .15, .20, .30, -.35, .62, .75, 1.55, .38, .62, .82, 1.30),
        Development("juvenile", .44, .13, .17, -.15, .88, .88, 1.20, .72, .84, .88, 1.16),
        Development("subadult", .78, .04, .06, -.04, .96, .96, 1.06, .92, .95, .96, 1.04),
    ),
    2: (
        Development("larva", .16, .20, .33, -.33, .66, .78, 1.55, .35, .56, .78, 1.35),
        Development("juvenile", .43, .14, .19, -.14, .87, .90, 1.22, .74, .85, .85, 1.20),
        Development("subadult", .77, .045, .065, -.04, .96, .97, 1.06, .93, .96, .95, 1.05),
    ),
    3: (
        Development("larva", .17, .18, .30, -.32, .65, .75, 1.43, .35, .60, .77, 1.30),
        Development("juvenile", .43, .11, .17, -.14, .86, .88, 1.18, .77, .88, .84, 1.18),
        Development("subadult", .76, .035, .05, -.04, .96, .96, 1.05, .93, .97, .94, 1.05),
    ),
    4: (
        Development("larva", .16, -.10, .30, -.28, .55, .86, 1.48, .30, .60, .82, 1.25),
        Development("juvenile", .44, -.06, .19, -.12, .75, .92, 1.19, .66, .82, .90, 1.14),
        Development("subadult", .78, -.015, .06, -.03, .93, .98, 1.05, .90, .95, .98, 1.03),
    ),
    5: (
        Development("newborn_pup", .32, .14, .24, -.17, .86, 1.08, 1.20, .79, .78, .90, 1.18),
        Development("early_juvenile", .48, .10, .16, -.11, .89, 1.04, 1.13, .85, .85, .92, 1.12),
        Development("juvenile", .64, .065, .10, -.07, .93, 1.01, 1.08, .91, .91, .95, 1.07),
        Development("subadult", .82, .025, .04, -.025, .97, 1.00, 1.03, .96, .97, .98, 1.03),
    ),
}

# Explicit ontogenetic palette, with muted early-life protection and a gradual
# approach to the canonical adult palette. Role order matches each species.
STAGE_COLORS = {
    1: {
        "larva": ("#9DA487", "#BDB993", "#273039"),
        "juvenile": ("#B9B45E", "#D2CA79", "#253039"),
        "subadult": ("#DEC14D", "#EBD26A", "#222C35"),
    },
    2: {
        "larva": ("#A98B76", "#C4B4A4", "#303038"),
        "juvenile": ("#BF704C", "#D8D2C5", "#252B32"),
        "subadult": ("#D67F49", "#E7E4D7", "#222832"),
    },
    3: {
        "larva": ("#71958D", "#A3BCAD", "#24373B"),
        "juvenile": ("#4B9A96", "#78B5AC", "#21353B"),
        "subadult": ("#4BADAA", "#85C8C2", "#1F333A"),
    },
    4: {
        "larva": ("#AFA98B", "#B8AD80", "#33363A"),
        "juvenile": ("#CEC3A0", "#C5B16C", "#2E3338"),
        "subadult": ("#E2DCC0", "#DAC15C", "#282E35"),
    },
    5: {
        "newborn_pup": ("#8C928B", "#C0BCAE", "#30373B"),
        "early_juvenile": ("#87969A", "#C9C6B8", "#29333A"),
        "juvenile": ("#81959D", "#CFCCC0", "#253038"),
        "subadult": ("#7E959F", "#D3D0C3", "#222E35"),
    },
}


# The Tang's non-adult forms use their own loft. The canonical adult is the
# hand-authored prototype, not this blueprint or a regenerated adult copy.
TANG_BASE = Blueprint(
    1, "yellow_tang", "laterally_compressed",
    ((-.092,.003,.008,0),(-.070,.010,.035,0),(-.045,.017,.050,0),
     (-.015,.020,.056,0),(.020,.019,.055,0),(.048,.015,.043,0),
     (.069,.008,.024,0),(.083,.004,.010,0)),
    12, 22, (-.092,-.062,-.034,-.008,.023,.054,.083),
    ("spine_01","spine_02","spine_03","spine_04","tail_01","tail_02"),
    -.067,.010,.0032,
    (("yellow","#F2C94C"),("sunlit","#F5D967"),("eye","#1E2833")),
    (48,30,18),
    (("spine_02",.016,.34),("spine_03",.035,.25),("spine_04",.075,.17),
     ("tail_01",.14,.10),("tail_02",.22,.03),("tail_fin",.11,-.02),
     ("pectoral_l",.028,.25),("pectoral_r",-.028,.25),
     ("dorsal",.012,.17),("anal",-.012,.17)), OPERCULATE,
)


def base_blueprint(number: int) -> Blueprint:
    if number == 1:
        return TANG_BASE
    return next(bp for bp in BLUEPRINTS if bp.number == number)


def stage_y(profile: Development, y: float) -> float:
    return y * profile.size * (1 + profile.head_length if y < 0 else 1 - profile.head_length)


def inverse_stage_y(profile: Development, y: float) -> float:
    return y / (profile.size * (1 + profile.head_length if y < 0 else 1 - profile.head_length))


def _shape_factor(profile: Development, u: float) -> float:
    return 1 + profile.head_fullness * (1-u)**2 + profile.peduncle_fullness * u**2


def warp_fin_point(profile: Development, base: Blueprint, part: str, point):
    """Warp authored species fin geometry around the developmental body axis."""
    x, y, z = point
    u = max(0., min(1., (y-base.shape[0][0]) / (base.shape[-1][0]-base.shape[0][0])))
    shape = _shape_factor(profile,u)
    is_tail = part.startswith("tail")
    is_pectoral = part.startswith("pectoral")
    fin_factor = profile.caudal if is_tail else profile.fin
    if is_tail:
        rear = base.shape[-1][0]
        y = rear + (y-rear)*fin_factor if y > rear else y
    return (x*profile.size*profile.width*shape*(fin_factor if is_pectoral else 1),
            stage_y(profile,y),
            z*profile.size*profile.depth*shape*(fin_factor if not is_pectoral else 1))


def stage_blueprint(number: int, profile: Development) -> Blueprint:
    profile.validate()
    base = base_blueprint(number)
    y0,y1 = base.shape[0][0],base.shape[-1][0]
    shape=[]
    for y,width,height,center in base.shape:
        u=(y-y0)/(y1-y0)
        fullness=_shape_factor(profile,u)
        shape.append((stage_y(profile,y),
                      width*profile.size*profile.width*fullness,
                      height*profile.size*profile.depth*fullness,
                      center*profile.size*profile.depth))
    frames=tuple(max(14,round(frame*profile.cadence)) for frame in base.frames)
    wave=tuple((name,amplitude*profile.wave,phase) for name,amplitude,phase in base.wave)
    colors=tuple((role,value) for (role,_),value in
                 zip(base.colors,STAGE_COLORS[number][profile.stage]))
    return replace(base,shape=tuple(shape),chain=tuple(stage_y(profile,y) for y in base.chain),
                   eye_y=stage_y(profile,base.eye_y),
                   eye_z=base.eye_z*profile.size*profile.depth,
                   eye_radius=base.eye_radius*profile.size*profile.eye_ratio,
                   radial_sides=8 if profile.stage=="larva" else base.radial_sides,
                   body_steps=14 if profile.stage=="larva" else base.body_steps,
                   frames=frames,wave=wave,colors=colors,
                   breathing_anatomy=SLIT_RIGGED if number==5 else OPERCULATE,
                   life_stage=profile.stage,development=profile)


def all_swimming_blueprints():
    return [stage_blueprint(number,profile) for number in range(1,6)
            for profile in PROFILES[number]]
