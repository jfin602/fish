"""Three original Pond animals in metres, Blender -Y forward and +Z up.

Faceted primitives are deliberately species-specific compositions. All visible
parts carry skin weights; shells alone are rigid, as a snapping turtle requires.
"""

from __future__ import annotations

import math
from dataclasses import dataclass

import bpy
from mathutils import Vector


@dataclass(frozen=True)
class Creature:
    number: int
    slug: str
    common: str
    scientific: str
    placement: str
    colors: tuple[str, ...]
    loops: tuple[str, ...]
    one_shots: tuple[str, ...]

    @property
    def faf_id(self):
        return f"FAF-CREATURE-{self.number:03d}"

    @property
    def stem(self):
        return f"creature_{self.number:03d}_{self.slug}"


CREATURES = (
    Creature(1, "northern_leopard_frog", "Northern leopard frog", "Lithobates pipiens",
             "shallow bank and surface", ("#65784A", "#A6A678", "#464C32", "#D3BE78", "#1D2520"),
             ("IdleShore", "SurfaceFloat", "Swim", "TurnLeft", "TurnRight"), ("Hop", "Dive")),
    Creature(2, "northern_water_snake", "Northern water snake", "Nerodia sipedon",
             "shoreline, surface and shallow water", ("#746F55", "#A49A75", "#423E31", "#827954", "#20251F"),
             ("IdleFloat", "SurfaceSwim", "UnderwaterSwim", "ShoreCrawl", "TurnLeft", "TurnRight"), ("Dive",)),
    Creature(3, "common_snapping_turtle", "Common snapping turtle", "Chelydra serpentina",
             "pond bottom and surface", ("#596149", "#8C855D", "#343C2F", "#777A57", "#1C241F"),
             ("IdleBottom", "BottomWalk", "SurfaceFloat", "Swim", "TurnLeft", "TurnRight"), ("Dive",)),
)


def material(name, color):
    m = bpy.data.materials.new(name)
    h = color.lstrip("#")
    srgb = [int(h[i:i+2], 16)/255 for i in (0, 2, 4)]
    rgb = [v/12.92 if v <= .04045 else ((v+.055)/1.055)**2.4 for v in srgb]
    m.diffuse_color = (*rgb, 1)
    m.use_nodes = True
    bsdf = m.node_tree.nodes.get("Principled BSDF")
    bsdf.inputs["Base Color"].default_value = (*rgb, 1)
    bsdf.inputs["Roughness"].default_value = .82
    return m


def mesh(name, vertices, faces, mats, indices=None):
    data = bpy.data.meshes.new(name + "__data")
    data.from_pydata(vertices, [], faces)
    data.update()
    obj = bpy.data.objects.new(name, data)
    bpy.context.collection.objects.link(obj)
    for m in mats:
        data.materials.append(m)
    if indices:
        for poly, mid in zip(data.polygons, indices):
            poly.material_index = mid
    return obj


def ellipsoid(name, center, radii, mats, bands=5, sides=10, material_for_face=None):
    c = Vector(center)
    vertices = [tuple(c + Vector((0, 0, radii[2])))]
    for band in range(1, bands):
        phi = math.pi * band / bands
        for side in range(sides):
            th = math.tau * side / sides
            vertices.append((c.x + radii[0]*math.sin(phi)*math.cos(th),
                             c.y + radii[1]*math.sin(phi)*math.sin(th),
                             c.z + radii[2]*math.cos(phi)))
    vertices.append(tuple(c + Vector((0, 0, -radii[2]))))
    faces = []
    mids = []
    for j in range(sides):
        faces.append((0, 1+j, 1+(j+1)%sides)); mids.append(0)
    for band in range(bands-2):
        for j in range(sides):
            a = 1+band*sides+j
            b = 1+band*sides+(j+1)%sides
            d = 1+(band+1)*sides+j
            faces.append((a,d,1+(band+1)*sides+(j+1)%sides,b))
            mids.append(material_for_face(band, j) if material_for_face else 0)
    bottom = len(vertices)-1
    for j in range(sides):
        faces.append((bottom, 1+(bands-2)*sides+(j+1)%sides,
                      1+(bands-2)*sides+j)); mids.append(1 if len(mats)>1 else 0)
    return mesh(name, vertices, faces, mats, mids)


def tube(name, points, radii, mats, sides=8, pattern=None):
    """Loft a tapered articulated part; ring spans are blended to adjacent bones."""
    pts = [Vector(p) for p in points]
    vertices=[]; faces=[]; mids=[]
    for i, p in enumerate(pts):
        tangent = (pts[min(i+1,len(pts)-1)]-pts[max(i-1,0)]).normalized()
        ref = Vector((0,0,1)) if abs(tangent.z)<.82 else Vector((1,0,0))
        u = tangent.cross(ref).normalized(); v = tangent.cross(u).normalized()
        for j in range(sides):
            th=math.tau*j/sides
            vertices.append(tuple(p + radii[i]*(math.cos(th)*u+math.sin(th)*v)))
    faces.append(tuple(reversed(range(sides)))); mids.append(0)
    for i in range(len(pts)-1):
        for j in range(sides):
            faces.append((i*sides+j, i*sides+(j+1)%sides,
                          (i+1)*sides+(j+1)%sides, (i+1)*sides+j))
            mids.append(pattern(i,j) if pattern else 0)
    faces.append(tuple((len(pts)-1)*sides+j for j in range(sides))); mids.append(0)
    return mesh(name,vertices,faces,mats,mids)


def polygon(name, points, mats, thickness=.0012):
    vertices=[(x-thickness/2,y,z) for x,y,z in points]+[(x+thickness/2,y,z) for x,y,z in points]
    n=len(points)
    faces=[tuple(range(n-1,-1,-1)),tuple(range(n,2*n))]
    for i in range(n):
        faces.append((i,(i+1)%n,(i+1)%n+n,i+n))
    return mesh(name,vertices,faces,mats)


def armature(stem, specs):
    data=bpy.data.armatures.new(stem+"__rig_data")
    arm=bpy.data.objects.new(stem+"__armature",data)
    bpy.context.collection.objects.link(arm)
    bpy.ops.object.select_all(action="DESELECT")
    arm.select_set(True); bpy.context.view_layer.objects.active=arm
    bpy.ops.object.mode_set(mode="EDIT")
    for name, head, tail, parent in specs:
        bone=data.edit_bones.new(name)
        bone.head=head; bone.tail=tail
        if parent:
            bone.parent=data.edit_bones[parent]
        bone.use_deform=name!="root"
    bpy.ops.object.mode_set(mode="OBJECT")
    return arm


def skin(obj, arm, weighted):
    obj.parent=arm
    mod=obj.modifiers.new("FAF Skin","ARMATURE")
    mod.object=arm
    groups={name:obj.vertex_groups.new(name=name)
            for name in sorted({name for influences in weighted for name in influences})}
    for vertex, influences in enumerate(weighted):
        total=sum(influences.values())
        if not total or abs(total-1)>1e-4:
            raise ValueError(f"invalid skin weights on {obj.name} vertex {vertex}")
        for name, value in influences.items():
            if value>0:
                groups[name].add([vertex],value,"REPLACE")
    return obj


def rigid(obj,arm,bone):
    return skin(obj,arm,[{bone:1.} for _ in obj.data.vertices])


def blended_tube(obj,arm,bones,sides,ring_count):
    weighted=[]
    for i in range(ring_count):
        pos=i*(len(bones)-1)/(ring_count-1)
        lo=min(int(pos),len(bones)-1); hi=min(lo+1,len(bones)-1)
        t=pos-lo
        inf={bones[lo]:1-t}
        if hi!=lo and t>0: inf[bones[hi]]=t
        weighted.extend([inf]*sides)
    return skin(obj,arm,weighted)


def _root_specs():
    return [("root",(0,0,-.012),(0,0,.012),None)]


def frog(stem,m):
    specs=_root_specs()+[("body",(0,.02,0),(0,-.045,0),"root"),
                         ("head",(0,-.045,0),(0,-.080,0),"body")]
    for sign,label in ((1,"L"),(-1,"R")):
        for kind,pts in (
            ("fore",[(sign*.027,-.036,-.01),(sign*.043,-.044,-.035),(sign*.046,-.075,-.045),(sign*.047,-.091,-.047)]),
            ("hind",[(sign*.026,.033,-.011),(sign*.073,.045,-.022),(sign*.066,.089,-.041),(sign*.078,.116,-.047)])):
            parent="body"
            for part,a,b in zip(("upper","lower","foot"),pts,pts[1:]):
                name=f"{kind}_{part}_{label}"
                specs.append((name,a,b,parent));parent=name
    arm=armature(stem,specs)
    parts=[]
    body=ellipsoid(stem+"__body",(0,.005,0),(.034,.057,.027),m[:2],6,12,
                   lambda b,j: 1 if b>=3 else 0)
    parts.append(rigid(body,arm,"body"))
    head=ellipsoid(stem+"__head",(0,-.059,.009),(.026,.032,.021),m[:2],5,10)
    parts.append(rigid(head,arm,"head"))
    # Flat dorsal spots and a pale dorsolateral fold are geometry, not textures.
    for side in (-1,1):
        label="L" if side>0 else "R"
        eye=ellipsoid(stem+f"__eye_{label}",(side*.020,-.069,.022),(.006,.006,.006),[m[4]],3,8)
        parts.append(rigid(eye,arm,"head"))
        fold=tube(stem+f"__fold_{label}",[(side*.025,-.045,.022),(side*.030,-.009,.023),(side*.025,.029,.019)],
                  [.0018,.0018,.0013],[m[3]],6)
        parts.append(rigid(fold,arm,"body"))
        for k,(y,x) in enumerate(((-.020,.011),(.006,.014),(.028,.011))):
            spot=ellipsoid(stem+f"__spot_{label}_{k}",(side*x,y,.026),(.006,.008,.0017),[m[2]],2,7)
            parts.append(rigid(spot,arm,"body"))
        for kind,pts,rads in (
            ("fore",[(side*.027,-.036,-.01),(side*.043,-.044,-.035),(side*.046,-.075,-.045),(side*.047,-.091,-.047)],
             [.0065,.005,.0037,.002]),
            ("hind",[(side*.026,.033,-.011),(side*.073,.045,-.022),(side*.066,.089,-.041),(side*.078,.116,-.047)],
             [.011,.009,.006,.0025])):
            limb=tube(stem+f"__{kind}_{label}",pts,rads,[m[0]],8)
            bones=[f"{kind}_{p}_{label}" for p in ("upper","lower","foot")]
            # The first and last rings anchor to their nearby bones; intermediate
            # joint rings use adjacent influences, so limbs visibly bend as skin.
            weights=[]
            for inf in ({bones[0]:1},{bones[0]:.5,bones[1]:.5},
                        {bones[1]:.5,bones[2]:.5},{bones[2]:1}):
                weights.extend([inf]*8)
            parts.append(skin(limb,arm,weights))
            foot=polygon(stem+f"__{kind}_web_{label}",
                         [(side*.078,.114,-.047),(side*.095,.132,-.047),
                          (side*.082,.141,-.047),(side*.065,.132,-.047)],
                         [m[3]],.0015) if kind=="hind" else polygon(
                         stem+f"__{kind}_web_{label}",
                         [(side*.047,-.090,-.047),(side*.059,-.104,-.047),
                          (side*.048,-.109,-.047),(side*.035,-.102,-.047)],
                         [m[3]],.001)
            parts.append(rigid(foot,arm,bones[-1]))
    return arm,parts


def snake(stem,m):
    length=.91
    ys=[-.445+i*length/44 for i in range(45)]
    radii=[];points=[]
    for i,y in enumerate(ys):
        u=i/44
        radius=.029+.018*math.sin(math.pi*.5*u/.67) if u<.67 else .047*(1-(u-.67)/.33)**1.12
        radii.append(max(.002,radius))
        points.append((0,y,0))
    names=[f"spine_{i:02d}" for i in range(1,11)]
    bounds=[-.445+i*length/10 for i in range(11)]
    specs=_root_specs()
    for i,name in enumerate(names):
        specs.append((name,(0,bounds[i],0),(0,bounds[i+1],0),"root" if i==0 else names[i-1]))
    specs.append(("head",(0,-.445,0),(0,-.49,0),names[0]))
    arm=armature(stem,specs)
    # Alternating broad dorsal blotches and pale underside identify N. sipedon.
    def mark(i,j):
        if j in (4,5,6): return 1
        band=i//4
        return 2 if band%2==0 and j in (0,1,2,8,9) else 0
    body=tube(stem+"__body",points,radii,m[:3],10,mark)
    weights=[]
    for y in ys:
        pos=max(0,min(9.999,(y+.445)/length*10))
        lo=min(9,int(pos));hi=min(9,lo+1);t=pos-lo
        inf={names[lo]:1. if hi==lo else 1-t}
        if hi!=lo and t>0:inf[names[hi]]=t
        weights.extend([inf]*10)
    parts=[skin(body,arm,weights)]
    head=ellipsoid(stem+"__head",(0,-.464,.002),(.041,.049,.026),m[:3],5,10,
                   lambda b,j:1 if b>=2 else 0)
    parts.append(rigid(head,arm,"head"))
    for sign,label in ((1,"L"),(-1,"R")):
        eye=ellipsoid(stem+f"__eye_{label}",(sign*.036,-.474,.015),(.0045,.0045,.0045),[m[4]],3,8)
        parts.append(rigid(eye,arm,"head"))
    return arm,parts


def turtle(stem,m):
    specs=_root_specs()+[("body",(0,.08,0),(0,-.13,0),"root"),
                         ("neck",(0,-.15,.005),(0,-.23,.008),"body"),
                         ("head",(0,-.23,.008),(0,-.300,.015),"neck")]
    for sign,label in ((1,"L"),(-1,"R")):
        for kind,y in (("fore",-.11),("hind",.12)):
            p0=(sign*.105,y,-.012);p1=(sign*.165,y+(.02 if kind=="hind" else -.025),-.048)
            p2=(sign*.19,y+(.045 if kind=="hind" else -.06),-.070)
            specs.extend([(f"{kind}_upper_{label}",p0,p1,"body"),
                          (f"{kind}_lower_{label}",p1,p2,f"{kind}_upper_{label}")])
    specs.extend([("tail_01",(0,.18,-.019),(0,.27,-.030),"body"),
                  ("tail_02",(0,.27,-.030),(0,.40,-.048),"tail_01")])
    arm=armature(stem,specs)
    parts=[]
    # Broad faceted carapace is rigid; two-ring shell geometry has large scutes.
    sides=12;verts=[];faces=[];mids=[]
    for z,rx,ry in ((-.016,.132,.202),(.035,.148,.216),(.090,.107,.158),(.126,.042,.072)):
        for j in range(sides):
            a=math.tau*j/sides
            verts.append((rx*math.cos(a),ry*math.sin(a),z))
    verts.append((0,0,.133))
    faces.append(tuple(reversed(range(sides))));mids.append(2)
    for ring in range(3):
        for j in range(sides):
            faces.append((ring*sides+j,ring*sides+(j+1)%sides,(ring+1)*sides+(j+1)%sides,
                          (ring+1)*sides+j))
            mids.append(2 if ring==0 else (0 if j%3 else 1))
    for j in range(sides):
        faces.append((48,36+j,36+(j+1)%sides));mids.append(0)
    shell=mesh(stem+"__carapace",verts,faces,m[:3],mids)
    parts.append(rigid(shell,arm,"body"))
    # A shallow serrated rear edge distinguishes a snapper without floating
    # decorative ridge pieces over the broad faceted shell.
    for side,label in ((1,"L"),(-1,"R")):
        for k in range(3):
            rear=polygon(stem+f"__rear_serration_{label}_{k}",
                         [(side*(.024+k*.034),.184,.024),
                          (side*(.025+k*.034),.228,.012),
                          (side*(.047+k*.034),.189,.023)],[m[2]],.004)
            parts.append(rigid(rear,arm,"body"))
    plastron=ellipsoid(stem+"__plastron",(0,.005,-.027),(.096,.145,.020),[m[1]],3,10)
    parts.append(rigid(plastron,arm,"body"))
    neck=tube(stem+"__neck",[(0,-.14,.002),(0,-.19,.004),(0,-.24,.008)],
              [.034,.028,.020],[m[3]],8)
    parts.append(skin(neck,arm,[{"body":1}]*8+[{"neck":1}]*16))
    head=ellipsoid(stem+"__head",(0,-.265,.012),(.052,.067,.037),[m[3]],5,10)
    parts.append(rigid(head,arm,"head"))
    beak=polygon(stem+"__beak",[(-.018,-.316,-.002),(0,-.350,-.013),(.018,-.316,-.002)],
                 [m[2]],.020)
    parts.append(rigid(beak,arm,"head"))
    for sign,label in ((1,"L"),(-1,"R")):
        eye=ellipsoid(stem+f"__eye_{label}",(sign*.046,-.278,.030),(.006,.006,.006),[m[4]],3,8)
        parts.append(rigid(eye,arm,"head"))
        for kind,y in (("fore",-.11),("hind",.12)):
            pts=[(sign*.105,y,-.012),(sign*.165,y+(.02 if kind=="hind" else -.025),-.048),
                 (sign*.19,y+(.045 if kind=="hind" else -.06),-.070)]
            limb=tube(stem+f"__{kind}_{label}",pts,[.034,.028,.020],[m[3]],8)
            parts.append(skin(limb,arm,[{f"{kind}_upper_{label}":1}]*8+
                              [{f"{kind}_upper_{label}":.5,f"{kind}_lower_{label}":.5}]*8+
                              [{f"{kind}_lower_{label}":1}]*8))
            foot=ellipsoid(stem+f"__{kind}_foot_{label}",pts[-1],(.028,.038,.010),[m[3]],3,8)
            parts.append(rigid(foot,arm,f"{kind}_lower_{label}"))
            for claw in range(3):
                x=pts[-1][0]+sign*(claw-1)*.009
                tip=polygon(stem+f"__{kind}_claw_{label}_{claw}",
                            [(x,pts[-1][1]-.020,-.07),(x+sign*.006,pts[-1][1]-.050,-.073),
                             (x+sign*.007,pts[-1][1]-.016,-.070)],[m[1]],.003)
                parts.append(rigid(tip,arm,f"{kind}_lower_{label}"))
    tail=tube(stem+"__tail",[(0,.18,-.019),(0,.24,-.026),(0,.30,-.036),(0,.40,-.048)],
              [.036,.027,.017,.002],[m[3]],8)
    parts.append(skin(tail,arm,[{"tail_01":1}]*8+
                      [{"tail_01":.7,"tail_02":.3}]*8+
                      [{"tail_01":.3,"tail_02":.7}]*8+[{"tail_02":1}]*8))
    return arm,parts


def _motions(spec, role, t):
    """Return per-bone XYZ Euler rotations. t=0 and 1 agree for loops."""
    cyc=math.sin(math.tau*t)
    q=math.sin(math.pi*t*.5)**2 if role=="Dive" else math.sin(math.pi*t)**2
    out={}
    if spec.number==1:
        strength={"IdleShore":.04,"SurfaceFloat":.10,"Swim":1.,"TurnLeft":.75,
                  "TurnRight":.75,"Hop":1.,"Dive":1.}[role]
        turn=1 if role=="TurnLeft" else -1 if role=="TurnRight" else 0
        for side,label in ((1,"L"),(-1,"R")):
            bias=1+turn*side*.42
            kick=cyc if role not in ("Hop","Dive") else q
            out[f"hind_thigh_{label}"]=(side*.13*strength*kick,0,side*.34*strength*kick*bias)
            out[f"hind_lower_{label}"]=(side*.20*strength*kick,0,-side*.65*strength*kick*bias)
            out[f"hind_foot_{label}"]=(0,0,side*.23*strength*kick)
            out[f"fore_upper_{label}"]=(side*.10*strength*kick,0,side*.08*strength*kick)
            out[f"fore_lower_{label}"]=(0,0,-side*.12*strength*kick)
            out[f"fore_foot_{label}"]=(0,0,side*.07*strength*kick)
        if role=="Hop":out["body"]=(.22*q,0,0)
        if role=="Dive":out["body"]=(-.19*q,0,0)
        if turn:out["body"]=(0,0,turn*.08*q)
    elif spec.number==2:
        amp={"IdleFloat":.05,"SurfaceSwim":.19,"UnderwaterSwim":.25,
             "ShoreCrawl":.22,"TurnLeft":.29,"TurnRight":.29,"Dive":.18}[role]
        turn=1 if role=="TurnLeft" else -1 if role=="TurnRight" else 0
        for i in range(1,11):
            phase=t-(i-1)*.11
            value=amp*(.16+.84*i/10)*math.sin(math.tau*phase)
            if turn:value+=turn*.15*q*(i/10)
            out[f"spine_{i:02d}"]=(0,0,value)
        if role=="Dive":out["head"]=(-.18*q,0,0)
    else:
        amp={"IdleBottom":.035,"BottomWalk":.21,"SurfaceFloat":.07,
             "Swim":.27,"TurnLeft":.21,"TurnRight":.21,"Dive":.28}[role]
        turn=1 if role=="TurnLeft" else -1 if role=="TurnRight" else 0
        for side,label in ((1,"L"),(-1,"R")):
            for kind,phase in (("fore",0 if side>0 else .5),("hind",.5 if side>0 else 0)):
                wave=math.sin(math.tau*(t+phase)) if role!="Dive" else q
                asym=1+turn*side*.3
                out[f"{kind}_upper_{label}"]=(side*amp*.65*wave,0,side*amp*wave*asym)
                out[f"{kind}_lower_{label}"]=(side*amp*.35*wave,0,-side*amp*.5*wave)
        out["tail_01"]=(0,0,amp*.30*cyc)
        out["tail_02"]=(0,0,amp*.47*math.sin(math.tau*(t-.18)))
        out["neck"]=(0,0,turn*.08*q if turn else .02*cyc)
        if role=="Dive":out["head"]=(-.20*q,0,0)
    return out


def actions(spec,arm):
    arm.animation_data_create()
    records=[]
    for role in (*spec.loops,*spec.one_shots):
        action=bpy.data.actions.new(spec.stem+"__"+role)
        action.use_fake_user=True
        arm.animation_data.action=action
        frames=61 if role in spec.loops else 31
        for i in range(17):
            t=i/16;frame=1+(frames-1)*t
            values=_motions(spec,role,t)
            for bone in arm.pose.bones:
                if bone.name=="root":continue
                bone.rotation_mode="XYZ"
                bone.rotation_euler=values.get(bone.name,(0,0,0))
                bone.keyframe_insert(data_path="rotation_euler",frame=frame,group=bone.name)
        track=arm.animation_data.nla_tracks.new()
        track.name=action.name
        track.strips.new(action.name,1,action)
        arm.animation_data.use_nla=False
        records.append(action.name)
    arm.animation_data.action=bpy.data.actions[spec.stem+"__"+spec.loops[0]]
    for bone in arm.pose.bones:
        bone.rotation_mode="XYZ";bone.rotation_euler=(0,0,0)
    bpy.context.scene.frame_set(1)
    return records


def build(spec,source):
    bpy.ops.wm.read_factory_settings(use_empty=True)
    scene=bpy.context.scene
    scene.unit_settings.system="METRIC"
    scene.unit_settings.scale_length=1
    m=[material(f"mat__{spec.stem}_{i}",color) for i,color in enumerate(spec.colors)]
    arm,parts={1:frog,2:snake,3:turtle}[spec.number](spec.stem,m)
    actions_list=actions(spec,arm)
    for obj in [arm,*parts]:
        obj["faf_id"]=spec.faf_id
        obj["faf_habitat"]="pond"
        obj["faf_species"]=spec.scientific
    coords=[v.co for part in parts for v in part.data.vertices]
    dimensions=[round(max(v[i] for v in coords)-min(v[i] for v in coords),6) for i in range(3)]
    triangles=sum(sum(len(p.vertices)-2 for p in obj.data.polygons) for obj in parts)
    used_materials=sorted({slot.material.name for obj in parts for slot in obj.material_slots
                           if slot.material is not None})
    return arm,parts,{"faf_id":spec.faf_id,"category":"CREATURE","habitat":"pond",
        "common_name":spec.common,"scientific_name":spec.scientific,"stem":spec.stem,
        "placement":spec.placement,"dimensions_m":dimensions,"triangles":triangles,
        "materials":len(used_materials),"material_names":used_materials,
        "bones":len(arm.data.bones),"bone_names":[b.name for b in arm.data.bones],
        "actions":actions_list,"loop_roles":list(spec.loops),"one_shot_roles":list(spec.one_shots),
        "source":source,"generator":"creatures.pond_creatures.v1","front_blender":"-Y",
        "up_blender":"+Z","origin_m":[0,0,0],"rotation_rad":[0,0,0],"scale":[1,1,1],
        "technical_review":"pending_export_check","visual_review":"pending_native_review"}
