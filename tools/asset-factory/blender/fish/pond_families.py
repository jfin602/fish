"""Original faceted Pond fish, with species-specific silhouettes and ontogeny.

All coordinates are metres, -Y forward, +Z dorsal. The Ocean generators and
their adult files are deliberately independent of this module.
"""

from __future__ import annotations

from dataclasses import dataclass, asdict
import math

import bpy
from mathutils import Vector

from fish.animation import reset_pose
from fish.breathing import (OPERCULATE, OperculumSpec, add_operculum_rig,
                            weight_operculum_region, validate_breathing_rig)
from fish.reef_families import _material, _mesh_object, _prism_parts, _eye
from fish.rig import skin_body_at_stations, skin_report


STAGES = ("egg", "larva", "juvenile", "subadult", "adult")
SWIM_ROLES = ("Idle", "Glide", "SwimSlow", "Swim", "SwimFast", "TurnLeft", "TurnRight")
LOOP_ROLES = ("Idle", "Glide", "SwimSlow", "Swim", "SwimFast")


@dataclass(frozen=True)
class Development:
    stage: str
    size: float
    depth: float
    width: float
    head: float
    peduncle: float
    eye: float
    fin: float
    tail: float
    wave: float
    cadence: float
    marking: float


PROFILES = {
    "larva": Development("larva", .085, .57, .70, .42, -.34, 1.65, .38, .62, 1.28, 1.20, .0),
    "juvenile": Development("juvenile", .33, .80, .84, .23, -.17, 1.22, .74, .84, 1.15, 1.12, .45),
    "subadult": Development("subadult", .68, .94, .95, .08, -.05, 1.06, .93, .96, 1.04, 1.04, .82),
    "adult": Development("adult", 1., 1., 1., 0., 0., 1., 1., 1., 1., 1., 1.),
}


@dataclass(frozen=True)
class Species:
    number: int
    slug: str
    common_name: str
    family: str
    length: float
    half_width: float
    half_height: float
    stations: tuple[tuple[float, float, float, float], ...]  # u, width, height, z centre
    eye_u: float
    eye_z: float
    eye_radius: float
    dorsal: tuple[tuple[tuple[float, float], ...], ...]
    anal: tuple[tuple[tuple[float, float], ...], ...]
    tail: tuple[tuple[float, float], ...]
    wave: tuple[float, float, float, float, float]
    cadence: float
    palettes: tuple[tuple[str, str, str, str, str], ...]  # body, belly, mark, fin, eye

    @property
    def faf_id(self):
        return f"FAF-FISH-{self.number:03d}"

    def stem(self, stage):
        return f"fish_{self.number:03d}_{self.slug}__{stage}"


# Each station, dorsal profile, anal profile, caudal profile and head placement
# is authored for its species. Shared loft/prism construction is not a body mold.
SPECIES = (
    Species(6, "bluegill", "Bluegill", "deep_sunfish", .255, .032, .085,
        ((0,.23,.20,0),(.10,.61,.57,0),(.25,.92,.91,0),(.43,1,1,0),
         (.62,.89,.94,0),(.78,.59,.64,0),(.91,.23,.25,0),(1,.13,.14,0)),
        .17,.16,.008,
        (((.24,.80),(.32,1.34),(.48,1.50),(.62,1.40),(.78,1.16),(.84,.46)),),
        (((.47,-.84),(.57,-1.19),(.75,-1.27),(.83,-.47)),),
        ((.94,.10),(1.12,.75),(1.19,.60),(1.14,0),(1.19,-.60),(1.12,-.75),(.94,-.10)),
        (.0,.022,.070,.15,.23),1.05,
        (("#90967B","#B7AF91","#6D766C","#939675","#2B302B"),
         ("#83976C","#C2B98E","#66785E","#879469","#252B28"),
         ("#5F825E","#D2BD78","#426457","#708865","#20292C"),
         ("#466F5A","#D5AF67","#314F61","#5D805F","#1B252B"))),
    Species(7, "largemouth_bass", "Largemouth Bass", "large_mouth_predator", .58, .070, .083,
        ((0,.27,.30,-.05),(.12,.68,.58,-.06),(.28,.96,.82,0),(.45,1,1,0),
         (.61,.86,.89,0),(.77,.56,.58,0),(.92,.22,.25,0),(1,.13,.15,0)),
        .18,.20,.010,
        (((.32,.83),(.40,1.27),(.50,1.24),(.55,.75)),
         ((.54,.74),(.62,1.32),(.78,1.23),(.84,.48))),
        (((.66,-.65),(.73,-1.02),(.84,-.93),(.88,-.32)),),
        ((.94,.11),(1.16,.88),(1.23,.85),(1.12,0),(1.23,-.85),(1.16,-.88),(.94,-.11)),
        (.0,.013,.050,.13,.25),.88,
        (("#A09C79","#C8BE9D","#76745D","#979476","#252B27"),
         ("#8B966D","#D1C89F","#596B4E","#86906A","#202722"),
         ("#6A8057","#D2C7A0","#344B38","#697D56","#1B2522"),
         ("#58704B","#D6CBAA","#2E493A","#5C714B","#192320"))),
    Species(8, "channel_catfish", "Channel Catfish", "broad_head_bottom_fish", .66, .074, .070,
        ((0,.57,.26,-.20),(.12,.93,.54,-.09),(.28,1,.82,0),(.46,.88,1,0),
         (.63,.69,.83,0),(.79,.44,.53,0),(.93,.19,.24,0),(1,.12,.15,0)),
        .16,.08,.007,
        (((.32,.80),(.37,1.33),(.46,1.13),(.52,.55)),
         ((.72,.55),(.78,.79),(.84,.53))),
        (((.56,-.76),(.66,-1.12),(.86,-.95),(.91,-.28)),),
        ((.95,.10),(1.16,.96),(1.22,.91),(1.09,0),(1.22,-.91),(1.16,-.96),(.95,-.10)),
        (.0,.010,.045,.13,.27),.80,
        (("#8A8A80","#B8B3A3","#5E615B","#85877E","#202629"),
         ("#798989","#C3C1B4","#596765","#758480","#1E2528"),
         ("#657B80","#CECDC2","#475D63","#60777B","#1B2228"),
         ("#546D75","#D5D1C4","#394F59","#526B70","#192027"))),
    Species(9, "common_carp", "Common Carp", "deep_scaled_cyprinid", .60, .083, .106,
        ((0,.33,.31,-.10),(.12,.72,.62,-.03),(.28,.93,.88,0),(.46,1,1,0),
         (.65,.87,.91,0),(.80,.53,.63,0),(.93,.21,.25,0),(1,.13,.15,0)),
        .16,.15,.008,
        (((.30,.87),(.37,1.35),(.52,1.48),(.72,1.33),(.84,.47)),),
        (((.70,-.63),(.76,-1.00),(.85,-.82),(.90,-.29)),),
        ((.94,.10),(1.15,.78),(1.22,.72),(1.10,0),(1.22,-.72),(1.15,-.78),(.94,-.10)),
        (.0,.011,.046,.12,.22),.73,
        (("#9A9782","#C6BCA0","#777565","#99927D","#292B28"),
         ("#A99C6A","#CFBE88","#89784E","#A99B70","#252925"),
         ("#B59959","#D9C68F","#826847","#B49960","#222721"),
         ("#B58B50","#E0CC94","#79593B","#AA8150","#1F2520"))),
    Species(10, "northern_pike", "Northern Pike", "long_snouted_ambush", .84, .058, .065,
        ((0,.39,.31,-.11),(.14,.53,.36,-.08),(.26,.72,.60,0),(.42,1,1,0),
         (.59,.86,.88,0),(.76,.52,.52,0),(.92,.19,.22,0),(1,.12,.14,0)),
        .20,.22,.008,
        (((.73,.55),(.79,1.15),(.88,1.18),(.94,.29)),),
        (((.72,-.50),(.80,-1.15),(.90,-1.03),(.94,-.28)),),
        ((.94,.09),(1.16,.70),(1.22,.65),(1.11,0),(1.22,-.65),(1.16,-.70),(.94,-.09)),
        (.0,.006,.030,.095,.23),.92,
        (("#8B9077","#B8B59C","#68715D","#86876B","#222923"),
         ("#71825D","#C1BE99","#B0B58A","#71805D","#202720"),
         ("#62774E","#C9C7A3","#B4BC8B","#64744D","#1C241F"),
         ("#526D48","#D3CBA9","#B9C58D","#5C734A","#19231D"))),
)


def species_by_number(number):
    return next(spec for spec in SPECIES if spec.number == number)


def _u_y(length, u):
    return (u - .5) * length


def _interpolate(stations, u):
    for a, b in zip(stations, stations[1:]):
        if a[0] <= u <= b[0]:
            t = (u-a[0])/(b[0]-a[0])
            return tuple(a[i]*(1-t)+b[i]*t for i in (1,2,3))
    return tuple(stations[0 if u < stations[0][0] else -1][1:])


def _body_position(spec, profile, u):
    width, height, center = _interpolate(spec.stations, u)
    head = 1 + profile.head * max(0, 1-u/.55)**2
    tail = 1 + profile.peduncle * max(0, (u-.5)/.5)**2
    return (spec.half_width*profile.size*profile.width*width*head*tail,
            spec.half_height*profile.size*profile.depth*height*head*tail,
            spec.half_height*profile.size*profile.depth*center)


def _mark_material(spec, profile, u, angle):
    v = math.sin(angle)
    if v < -.37:
        return 1
    if profile.stage == "larva":
        return 0
    if spec.number == 6:
        bars = 3 if profile.stage == "juvenile" else 6
        return 2 if .25 < u < .82 and abs((u*.95*bars) % 1 - .5) < .12*profile.marking else 0
    if spec.number == 7:
        return 2 if .22 < u < .88 and abs(v) < (.16 + .12*profile.marking) else 0
    if spec.number == 8:
        return 2 if profile.stage != "juvenile" and .38 < u < .78 and abs(v) < .4 and int(u*24+angle*3)%11==0 else 0
    if spec.number == 9:
        return 2 if .28 < u < .79 and v > -.08 and int(u*31+angle*2)%7==0 else 0
    if profile.stage == "juvenile":
        return 2 if .27 < u < .87 and int(u*17)%3==0 and abs(v)<.8 else 0
    return 2 if .26 < u < .89 and int(u*37+angle*3)%9==0 and abs(v)<.86 else 0


def _body(spec, profile, stem, materials, collection):
    rings = 17 if profile.stage == "larva" else 26
    sides = 10 if profile.stage == "larva" else 12
    vertices=[]; faces=[]; material_ids=[]
    for row in range(rings+1):
        u=row/rings
        width,height,center=_body_position(spec,profile,u)
        for side in range(sides):
            angle=math.tau*side/sides
            vertices.append((width*math.cos(angle),_u_y(spec.length*profile.size,u),
                             center+height*math.sin(angle)))
    # The front is at -Y, so its cap faces outward with the ring's winding.
    faces.append(tuple(range(sides)));material_ids.append(0)
    for row in range(rings):
        u=(row+.5)/rings
        for side in range(sides):
            nxt=(side+1)%sides
            faces.append((row*sides+side,(row+1)*sides+side,
                          (row+1)*sides+nxt,row*sides+nxt))
            material_ids.append(_mark_material(spec,profile,u,math.tau*(side+.5)/sides))
    faces.append(tuple(rings*sides+i for i in reversed(range(sides))));material_ids.append(0)
    return _mesh_object(stem+"__body",vertices,faces,material_ids,materials,collection)


def _fin_polygon(spec, profile, points):
    length=spec.length*profile.size
    height=spec.half_height*profile.size*profile.depth
    return [(0,_u_y(length,u),z*height*(profile.fin if abs(z)>1 else 1))
            for u,z in points]


def _fins(spec, profile, stem, materials, collection):
    parts={}
    thickness=max(.00035, spec.half_width*profile.size*.08)
    parts["dorsal_fin"]=_prism_parts(stem+"__dorsal_fin",
        [_fin_polygon(spec,profile,p) for p in spec.dorsal],0,thickness,materials[3],collection)
    parts["anal_fin"]=_prism_parts(stem+"__anal_fin",
        [_fin_polygon(spec,profile,p) for p in spec.anal],0,thickness,materials[3],collection)
    tail_points=[(u if u<=1 else 1+(u-1)*profile.tail,z*profile.tail)
                 for u,z in spec.tail]
    parts["tail_fin"]=_prism_parts(stem+"__tail_fin",
        [_fin_polygon(spec,profile,tail_points)],0,thickness*1.25,materials[3],collection)
    root_u=.30 if spec.number in (6,9) else .34 if spec.number==7 else .25 if spec.number==8 else .37
    width,_,_=_body_position(spec,profile,root_u)
    length=spec.length*profile.size
    height=spec.half_height*profile.size*profile.depth
    spread={6:.065,7:.078,8:.092,9:.083,10:.071}[spec.number]*profile.size*profile.fin
    for sign,label in ((1,"l"),(-1,"r")):
        wing=[(sign*width*.85,_u_y(length,root_u),-.12*height),
              (sign*(width+spread),_u_y(length,root_u+.08),-.42*height),
              (sign*(width+spread*.82),_u_y(length,root_u+.16),-.72*height),
              (sign*width*.85,_u_y(length,root_u+.06),-.37*height)]
        parts["pectoral_"+label]=_prism_parts(stem+"__pectoral_"+label,
            [wing],2,thickness*1.5,materials[3],collection)
    return parts


def _feature_polygon(spec,profile,stem,name,polygons,material,collection,axis=0):
    thickness=max(.0003,spec.half_width*profile.size*.07)
    return _prism_parts(stem+"__"+name,polygons,axis,thickness,material,collection)


def _features(spec,profile,stem,materials,collection):
    parts={}
    length=spec.length*profile.size
    height=spec.half_height*profile.size*profile.depth
    if spec.number==6:
        # A compact forward mouth and broad blue opercular ear patch.
        mouth=[(0,_u_y(length,-.025),-.08*height),
               (0,_u_y(length,.075),.03*height),(0,_u_y(length,.075),-.25*height)]
        parts["mouth"]=_feature_polygon(spec,profile,stem,"mouth",[mouth],materials[0],collection)
        if profile.stage!="larva":
            for sign,label in ((1,"l"),(-1,"r")):
                width,_,_=_body_position(spec,profile,.23)
                x=sign*(width+length*.002)
                patch=[(x,_u_y(length,.18),.10*height),
                       (x,_u_y(length,.27),.33*height),
                       (x,_u_y(length,.32),-.12*height),
                       (x,_u_y(length,.23),-.32*height)]
                parts["ear_"+label]=_feature_polygon(spec,profile,stem,"ear_"+label,
                    [patch],materials[2],collection)
    elif spec.number==7:
        # The gape extends well behind the snout, unlike the bluegill mouth.
        upper=[(0,_u_y(length,-.035),.10*height),
               (0,_u_y(length,.18),.18*height),(0,_u_y(length,.15),-.04*height)]
        lower=[(0,_u_y(length,-.02),-.19*height),
               (0,_u_y(length,.16),-.01*height),(0,_u_y(length,.19),-.18*height)]
        parts["upper_jaw"]=_feature_polygon(spec,profile,stem,"upper_jaw",[upper],materials[0],collection)
        parts["lower_jaw"]=_feature_polygon(spec,profile,stem,"lower_jaw",[lower],materials[1],collection)
        # Paired surface wedges expose the wide gape in side view instead of
        # burying a center-plane polygon inside the head loft.
        for sign,label in ((1,"l"),(-1,"r")):
            gap=[]
            for u,z in ((.012,-.08),(.20,-.015),(.19,-.105),(.018,-.17)):
                width,_,_=_body_position(spec,profile,u)
                gap.append((sign*(width+length*.003),_u_y(length,u),z*height))
            parts["gape_"+label]=_feature_polygon(spec,profile,stem,"gape_"+label,
                [gap],materials[2],collection)
    elif spec.number in (8,9):
        # Catfish have a wide flat mouth with four long visible barbels per
        # side; carp have a smaller protruding mouth and one short pair.
        lip_u=.03 if spec.number==8 else -.015
        lip=[(0,_u_y(length,lip_u),-.15*height),(0,_u_y(length,.12),-.04*height),
             (0,_u_y(length,.11),-.24*height)]
        parts["lip"]=_feature_polygon(spec,profile,stem,"lip",[lip],materials[1],collection)
        for sign,label in ((1,"l"),(-1,"r")):
            count=(2 if profile.stage=="larva" else 4) if spec.number==8 else (1 if profile.stage=="larva" else 2)
            for index in range(count):
                u=.045+index*.028
                width,_,_=_body_position(spec,profile,u)
                reach=(.15+index*.026 if spec.number==8 else .065+index*.02)*length*profile.fin
                x=sign*width*.88
                whisker=[(x,_u_y(length,u),-.18*height),
                         (x+sign*reach,_u_y(length,u+.06),(-.32-index*.08)*height),
                         (x+sign*reach*.88,_u_y(length,u+.08),(-.36-index*.08)*height),
                         (x,_u_y(length,u+.022),-.22*height)]
                parts[f"barbel_{label}_{index+1}"]=_feature_polygon(
                    spec,profile,stem,f"barbel_{label}_{index+1}",[whisker],materials[2],collection,axis=2)
    else:
        # Long flat duckbill and lower jaw are broad in top view.
        width,_,_=_body_position(spec,profile,.08)
        bill=[(-width*.9,_u_y(length,-.06),-.10*height),
              (width*.9,_u_y(length,-.06),-.10*height),
              (width,_u_y(length,.18),-.08*height),
              (-width,_u_y(length,.18),-.08*height)]
        parts["duckbill"]=_feature_polygon(spec,profile,stem,"duckbill",[bill],materials[0],collection,axis=2)
        lower=[(0,_u_y(length,-.05),-.27*height),
               (0,_u_y(length,.20),-.10*height),
               (0,_u_y(length,.18),-.30*height)]
        parts["lower_jaw"]=_feature_polygon(spec,profile,stem,"lower_jaw",[lower],materials[1],collection)
    return parts


def _armature(spec,profile,stem,collection):
    length=spec.length*profile.size
    data=bpy.data.armatures.new(stem+"__rig_data")
    arm=bpy.data.objects.new(stem+"__armature",data)
    collection.objects.link(arm)
    bpy.ops.object.select_all(action="DESELECT")
    arm.select_set(True)
    bpy.context.view_layer.objects.active=arm
    bpy.ops.object.mode_set(mode="EDIT")
    root=data.edit_bones.new("root")
    root.head=(0,-length*.04,0);root.tail=(0,length*.04,0)
    intervals=(("spine_01",0,.28),("spine_02",.28,.47),
               ("spine_03",.47,.65),("tail_01",.65,.82),("tail_02",.82,1.))
    parent=root
    for name,start,end in intervals:
        bone=data.edit_bones.new(name)
        bone.head=(0,_u_y(length,start),0)
        bone.tail=(0,_u_y(length,end),0)
        bone.parent=parent
        parent=bone
    tail=data.edit_bones.new("tail_fin")
    tail.head=(0,_u_y(length,1.),0)
    tail.tail=(0,_u_y(length,1.18),0)
    tail.parent=parent
    pectoral_u=.30 if spec.number in (6,9) else .34 if spec.number==7 else .25 if spec.number==8 else .37
    width,_,_=_body_position(spec,profile,pectoral_u)
    for sign,label in ((1,"l"),(-1,"r")):
        bone=data.edit_bones.new("pectoral_"+label)
        bone.head=(sign*width*.8,_u_y(length,pectoral_u),0)
        bone.tail=(sign*(width+length*.08),_u_y(length,pectoral_u+.07),-length*.015)
        bone.parent=data.edit_bones["spine_02"]
    for name,u,zsign in (("dorsal",.47,1),("anal",.69,-1)):
        bone=data.edit_bones.new(name)
        bone.head=(0,_u_y(length,u),zsign*spec.half_height*profile.size*.7)
        bone.tail=(0,_u_y(length,u+.06),zsign*spec.half_height*profile.size*1.2)
        bone.parent=data.edit_bones["spine_03"]
    bpy.ops.object.mode_set(mode="OBJECT")
    return arm


def _bind(obj,arm,bone=None,stations=None):
    obj.parent=arm
    modifier=obj.modifiers.new("FAF Skin","ARMATURE")
    modifier.object=arm
    if stations is not None:
        skin_body_at_stations(obj,stations)
    else:
        group=obj.vertex_groups.new(name=bone)
        group.add(list(range(len(obj.data.vertices))),1.0,"REPLACE")


def _breathing_spec(spec,profile):
    length=spec.length*profile.size
    u=.30
    width,height,center=_body_position(spec,profile,u)
    return OperculumSpec(center_y=_u_y(length,u),center_z=center,
                         half_width=width,region_y=length*(.18 if profile.stage=="larva" else .14),
                         region_z=height*1.1,parent_bone="spine_01",max_weight=.82,
                         safe_angle_rad=.36)


def _action(arm,stem,role,frames,amps,stage_wave):
    if bpy.data.actions.get(stem+"__"+role):
        raise ValueError("duplicate Pond Action")
    action=bpy.data.actions.new(stem+"__"+role)
    action.use_fake_user=True
    arm.animation_data_create()
    arm.animation_data.action=action
    reset_pose(arm)
    turning=role.startswith("Turn")
    direction=1 if role=="TurnLeft" else -1
    power={"Idle":.08,"Glide":.22,"SwimSlow":.55,"Swim":1.,"SwimFast":1.36,
           "TurnLeft":1.,"TurnRight":1.}[role]
    names=("spine_02","spine_03","tail_01","tail_02","tail_fin")
    for sample in range(17):
        t=sample/16
        frame=1+frames*t
        for index,name in enumerate(names):
            bone=arm.pose.bones[name]
            bone.rotation_mode="XYZ"
            if turning:
                envelope=math.sin(math.pi*t)**2
                amount=direction*amps[index]*stage_wave*(1.25+index*.15)*envelope
            else:
                phase=(0,.17,.30,.42,.54)[index]
                amount=amps[index]*stage_wave*power*math.sin(math.tau*(t-phase))
            bone.rotation_euler.z=amount
            bone.keyframe_insert(data_path="rotation_euler",frame=frame,group=name)
        for name,sign in (("pectoral_l",1),("pectoral_r",-1)):
            bone=arm.pose.bones[name];bone.rotation_mode="XYZ"
            bone.rotation_euler.z=(direction*sign*.09*math.sin(math.pi*t)**2 if turning
                                   else sign*.025*power*math.sin(math.tau*(t-.25)))
            bone.keyframe_insert(data_path="rotation_euler",frame=frame,group=name)
        for name,sign in (("dorsal",1),("anal",-1)):
            bone=arm.pose.bones[name];bone.rotation_mode="XYZ"
            bone.rotation_euler.z=(sign*.02*power*math.sin(math.tau*(t-.15)) if not turning else 0)
            bone.keyframe_insert(data_path="rotation_euler",frame=frame,group=name)
    track=arm.animation_data.nla_tracks.new()
    track.name=action.name
    track.strips.new(action.name,1,action)
    arm.animation_data.use_nla=False
    return action


def _measure(spec,profile,stem,arm,meshes,source,breathing=None):
    coords=[tuple(v.co) for obj in meshes for v in obj.data.vertices]
    dims=[round(max(v[i] for v in coords)-min(v[i] for v in coords),6) for i in range(3)]
    triangles=sum(sum(len(p.vertices)-2 for p in obj.data.polygons) for obj in meshes)
    mats=sorted({obj.data.materials[p.material_index].name for obj in meshes
                 for p in obj.data.polygons if obj.data.materials[p.material_index]})
    return {"faf_id":spec.faf_id,"species":spec.common_name,"scientific_family":spec.family,
            "life_stage":profile.stage,"habitat":"pond","stem":stem,
            "dimensions_m":dims,"triangles":triangles,"materials":len(mats),
            "material_names":mats,"bones":len(arm.data.bones),
            "bone_names":[bone.name for bone in arm.data.bones],
            "actions":[stem+"__"+role for role in (("Idle","Hatch") if profile.stage=="egg" else SWIM_ROLES)],
            "loop_roles":["Idle"] if profile.stage=="egg" else list(LOOP_ROLES),
            "breathing_anatomy":"not_applicable_egg" if profile.stage=="egg" else OPERCULATE,
            "breathing_validation":breathing,
            "origin_m":[0,0,0],"rotation_rad":[0,0,0],"scale":[1,1,1],
            "front_blender":"-Y","up_blender":"+Z",
            "generator":"fish.pond_families.v1","source":source,
            "generator_config":{"species":asdict(spec),"development":asdict(profile),
                                "deterministic":True},
            "technical_review":"pending_export_check","visual_review":"pending_native_review"}


def create_swimmer(spec,profile,scene,source):
    stem=spec.stem(profile.stage)
    collection=bpy.data.collections.new(spec.faf_id+" "+profile.stage+" Pond Source")
    scene.collection.children.link(collection)
    materials=[_material("mat__"+stem+"_"+role,color) for role,color in
               zip(("body","belly","mark","fin","eye"),spec.palettes[STAGES.index(profile.stage)-1])]
    body=_body(spec,profile,stem,materials,collection)
    fins=_fins(spec,profile,stem,materials,collection)
    features=_features(spec,profile,stem,materials,collection)
    eyes={}
    width,height,center=_body_position(spec,profile,spec.eye_u)
    eye_radius=spec.eye_radius*profile.size*profile.eye
    for sign,label in ((1,"l"),(-1,"r")):
        eyes[label]=_eye(stem+"__eye_"+label,
                         (sign*(width+eye_radius*.15),_u_y(spec.length*profile.size,spec.eye_u),
                          center+spec.eye_z*height),eye_radius,materials[4],collection,
                          subdivisions=0 if profile.stage=="larva" else 1)
    arm=_armature(spec,profile,stem,collection)
    gill=_breathing_spec(spec,profile)
    add_operculum_rig(arm,gill)
    length=spec.length*profile.size
    stations=[(-(_u_y(length,a)+_u_y(length,b))*.5,{name:1.}) for name,a,b in
              (("spine_01",0,.28),("spine_02",.28,.47),("spine_03",.47,.65),
               ("tail_01",.65,.82),("tail_02",.82,1.))]
    _bind(body,arm,stations=stations)
    weight_operculum_region(body,gill)
    for name,obj in fins.items():
        bone=("pectoral_l" if name.startswith("pectoral_l") else
              "pectoral_r" if name.startswith("pectoral_r") else
              "dorsal" if name.startswith("dorsal") else
              "anal" if name.startswith("anal") else "tail_fin")
        _bind(obj,arm,bone=bone)
    for obj in features.values():
        _bind(obj,arm,bone="spine_01")
    for obj in eyes.values():
        _bind(obj,arm,bone="spine_01")
    meshes=[body,*fins.values(),*features.values(),*eyes.values()]
    skin=skin_report(meshes,arm)
    if skin["problems"]:
        raise ValueError(f"{stem} skin: {skin['problems'][:4]}")
    breathing=validate_breathing_rig(arm,meshes,gill)
    for role in SWIM_ROLES:
        base=spec.cadence*profile.cadence
        frames=round({"Idle":72,"Glide":58,"SwimSlow":43,"Swim":30,
                      "SwimFast":20,"TurnLeft":24,"TurnRight":24}[role]*base)
        _action(arm,stem,role,max(14,frames),spec.wave,profile.wave)
    arm.animation_data.action=bpy.data.actions[stem+"__Idle"]
    scene.frame_set(1)
    reset_pose(arm)
    for obj in [arm,*meshes]:
        obj["faf_id"]=spec.faf_id
        obj["faf_life_stage"]=profile.stage
        obj["faf_habitat"]="pond"
    return arm,meshes,_measure(spec,profile,stem,arm,meshes,source,breathing)


def _egg_half(name,sign,radius,material,collection):
    sides=10
    rings=4
    vertices=[(sign*radius,0,0)]
    for ring in range(1,rings+1):
        phi=(math.pi*.5)*ring/rings
        for side in range(sides):
            angle=math.tau*side/sides
            vertices.append((sign*radius*math.cos(phi),
                             radius*.82*math.sin(phi)*math.cos(angle),
                             radius*1.08*math.sin(phi)*math.sin(angle)))
    faces=[]
    for side in range(sides):
        faces.append((0,1+side,1+(side+1)%sides))
    for ring in range(rings-1):
        for side in range(sides):
            a=1+ring*sides+side
            b=1+ring*sides+(side+1)%sides
            c=1+(ring+1)*sides+(side+1)%sides
            d=1+(ring+1)*sides+side
            faces.append((a,b,c,d) if sign>0 else (d,c,b,a))
    return _mesh_object(name,vertices,faces,[0]*len(faces),[material],collection)


def _egg_armature(stem,radius,collection):
    data=bpy.data.armatures.new(stem+"__rig_data")
    arm=bpy.data.objects.new(stem+"__armature",data)
    collection.objects.link(arm)
    bpy.ops.object.select_all(action="DESELECT")
    arm.select_set(True)
    bpy.context.view_layer.objects.active=arm
    bpy.ops.object.mode_set(mode="EDIT")
    root=data.edit_bones.new("root")
    root.head=(0,-radius*.3,0);root.tail=(0,radius*.3,0)
    for sign,label in ((1,"l"),(-1,"r")):
        bone=data.edit_bones.new("shell_"+label)
        bone.head=(sign*radius*.1,0,0)
        bone.tail=(sign*radius*.8,radius*.12,0)
        bone.parent=root
    embryo=data.edit_bones.new("embryo")
    embryo.head=(radius*.7,0,0)
    embryo.tail=(radius*.7,radius*.4,radius*.25)
    embryo.parent=root
    bpy.ops.object.mode_set(mode="OBJECT")
    return arm


def _egg_action(arm,stem,role,radius):
    action=bpy.data.actions.new(stem+"__"+role)
    action.use_fake_user=True
    arm.animation_data_create()
    arm.animation_data.action=action
    reset_pose(arm)
    for frame,t in ((1,0),(7,.25),(13,.5),(19,.75),(25,1)):
        for name,sign in (("shell_l",1),("shell_r",-1)):
            bone=arm.pose.bones[name]
            bone.rotation_mode="XYZ"
            if role=="Idle":
                bone.scale=(1+.018*math.sin(math.tau*t),)*3
                bone.location=(0,0,0)
                bone.rotation_euler.z=0
            else:
                opening=math.sin(math.pi*t*.5)**2
                bone.scale=(1,1,1)
                bone.location=(sign*radius*1.45*opening,0,0)
                bone.rotation_euler.z=sign*.35*opening
            for property_name in ("scale","location","rotation_euler"):
                bone.keyframe_insert(data_path=property_name,frame=frame,group=name)
        embryo=arm.pose.bones["embryo"]
        embryo.rotation_mode="XYZ"
        embryo.rotation_euler.z=(.09*math.sin(math.tau*t) if role=="Idle" else .27*math.sin(math.pi*t))
        embryo.location=((0,0,0) if role=="Idle" else
                         (radius*.7*math.sin(math.pi*t*.5)**2,
                          -radius*.3*math.sin(math.pi*t*.5)**2,
                          radius*.55*math.sin(math.pi*t*.5)**2))
        embryo.keyframe_insert(data_path="rotation_euler",frame=frame,group="embryo")
        embryo.keyframe_insert(data_path="location",frame=frame,group="embryo")
    track=arm.animation_data.nla_tracks.new()
    track.name=action.name
    track.strips.new(action.name,1,action)
    arm.animation_data.use_nla=False
    return action


def create_egg(spec,scene,source):
    stem=spec.stem("egg")
    collection=bpy.data.collections.new(spec.faf_id+" egg Pond Source")
    scene.collection.children.link(collection)
    diameter={6:.0035,7:.0041,8:.0040,9:.0048,10:.0038}[spec.number]
    radius=diameter*.5
    shell_color={6:"#A5A38B",7:"#9E9C7C",8:"#9BA28E",9:"#B5AA7B",10:"#9AA185"}[spec.number]
    embryo_color={6:"#5D6354",7:"#59634E",8:"#576565",9:"#766345",10:"#56664E"}[spec.number]
    shell=_material("mat__"+stem+"_shell",shell_color)
    embryo_mat=_material("mat__"+stem+"_embryo",embryo_color)
    left=_egg_half(stem+"__shell_l",1,radius,shell,collection)
    right=_egg_half(stem+"__shell_r",-1,radius,shell,collection)
    # Embryo is partly visible through a broad surface mark in the closed pose.
    embryo=_eye(stem+"__embryo",(radius*.86,-radius*.12,radius*.15),
                radius*.26,embryo_mat,collection,subdivisions=0)
    arm=_egg_armature(stem,radius,collection)
    for obj,bone in ((left,"shell_l"),(right,"shell_r"),(embryo,"embryo")):
        _bind(obj,arm,bone=bone)
    meshes=[left,right,embryo]
    skin=skin_report(meshes,arm)
    if skin["problems"]:
        raise ValueError(f"{stem} egg skin: {skin['problems'][:4]}")
    _egg_action(arm,stem,"Idle",radius)
    _egg_action(arm,stem,"Hatch",radius)
    arm.animation_data.action=bpy.data.actions[stem+"__Idle"]
    scene.frame_set(1)
    reset_pose(arm)
    for obj in [arm,*meshes]:
        obj["faf_id"]=spec.faf_id
        obj["faf_life_stage"]="egg"
        obj["faf_habitat"]="pond"
    # The egg has its own rig profile, separate from larval swim anatomy.
    profile=Development("egg",diameter/spec.length,1,1,0,0,1,0,0,0,1,0)
    report=_measure(spec,profile,stem,arm,meshes,source)
    report["generator_config"]={"diameter_m":diameter,"shell_color":shell_color,
                                "embryo_color":embryo_color,"deterministic":True}
    return arm,meshes,report
