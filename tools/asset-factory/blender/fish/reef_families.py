"""Four original FAF reef fish. Shape profiles and fins are species-specific.

The loft and fin-prism helpers are shared construction primitives; neither the
Tang station map nor a universal fish silhouette is reused here.
"""

from __future__ import annotations

from dataclasses import dataclass
import math

import bpy
import bmesh
from mathutils import Vector

from fish.rig import skin_body_at_stations, skin_report
from fish.animation import make_wave_action, reset_pose
from fish.breathing import (OPERCULATE, EXPOSED_SLITS, OperculumSpec,
                            add_operculum_rig, classify_breathing,
                            weight_operculum_region, validate_breathing_rig)


@dataclass(frozen=True)
class Blueprint:
    number: int
    slug: str
    family: str
    shape: tuple[tuple[float, float, float, float], ...]  # y, half-width, half-height, z-centre
    radial_sides: int
    body_steps: int
    chain: tuple[float, ...]
    chain_names: tuple[str, ...]
    eye_y: float
    eye_z: float
    eye_radius: float
    colors: tuple[tuple[str, str], ...]
    frames: tuple[int, int, int]
    wave: tuple[tuple[str, float, float], ...]
    breathing_anatomy: str

    @property
    def stem(self):
        return f"fish_{self.number:03d}_{self.slug}"

    @property
    def faf_id(self):
        return f"FAF-FISH-{self.number:03d}"


BLUEPRINTS = (
    Blueprint(2, "ocellaris_clownfish", "compact_oval",
        ((-.054,.003,.004,0),(-.042,.012,.024,0),(-.022,.019,.033,0),
         (.006,.021,.034,0),(.030,.017,.028,0),(.052,.007,.012,0),(.062,.004,.006,0)),
        12, 21, (-.054,-.030,-.008,.015,.037,.054,.066),
        ("spine_01","spine_02","spine_03","spine_04","tail_01","tail_02"),
        -.038,.007,.0032,
        (("orange","#EE8445"),("white","#ECECE2"),("black","#222832")),
        (48,24,16),
        (("spine_02",.023,.32),("spine_03",.055,.25),("spine_04",.09,.17),
         ("tail_01",.18,.10),("tail_02",.27,.04),("tail_fin",.15,-.02),
         ("pectoral_l",.035,.22),("pectoral_r",-.035,.22),("dorsal",.015,.20),("anal",-.015,.20)), OPERCULATE),
    Blueprint(3, "blue_green_chromis", "schooling",
        ((-.049,.002,.003,0),(-.039,.010,.014,0),(-.020,.016,.022,0),
         (.006,.017,.023,0),(.029,.012,.017,0),(.047,.004,.006,0),(.055,.003,.004,0)),
        10, 15, (-.049,-.029,-.005,.022,.044,.057),
        ("spine_01","spine_02","spine_03","tail_01","tail_02"),
        -.035,.004,.0025,
        (("teal","#4CB8AC"),("pale","#8BD5CD"),("black","#1D3139")),
        (42,20,14),
        (("spine_02",.032,.30),("spine_03",.08,.20),("tail_01",.19,.11),
         ("tail_02",.29,.03),("tail_fin",.17,-.02),("pectoral_l",.025,.2),
         ("pectoral_r",-.025,.2),("dorsal",.012,.2),("anal",-.012,.2)), OPERCULATE),
    Blueprint(4, "reef_butterflyfish", "highly_compressed",
        ((-.090,.002,.004,0),(-.071,.006,.011,-.002),(-.051,.011,.034,0),
         (-.027,.016,.053,0),(.004,.019,.062,0),(.031,.017,.052,0),
         (.054,.009,.027,0),(.069,.004,.010,0)),
        14, 22, (-.090,-.052,-.025,.004,.034,.057,.072),
        ("spine_01","spine_02","spine_03","spine_04","tail_01","tail_02"),
        -.058,.004,.0030,
        (("cream","#EEE9C8"),("yellow","#E5C65A"),("black","#242A34")),
        (54,30,20),
        (("spine_02",.012,.31),("spine_03",.032,.23),("spine_04",.064,.15),
         ("tail_01",.13,.09),("tail_02",.20,.02),("tail_fin",.11,-.02),
         ("pectoral_l",.045,.23),("pectoral_r",-.045,.23),
         ("dorsal",.014,.18),("anal",-.014,.18)), OPERCULATE),
    Blueprint(5, "blacktip_reef_shark", "fusiform_shark",
        ((-.66,.014,.020,0),(-.58,.055,.054,0),(-.40,.105,.092,0),
         (-.20,.122,.105,0),(.02,.111,.099,0),(.25,.077,.076,0),
         (.43,.045,.047,0),(.55,.025,.026,0),(.66,.014,.017,0)),
        14, 25, (-.66,-.43,-.20,.04,.27,.45,.56,.65,.73),
        ("spine_01","spine_02","spine_03","spine_04","spine_05",
         "tail_01","tail_02","tail_03"),
        -.48,.018,.013,
        (("slate","#8297A0"),("belly","#D6D4C4"),("black","#202B32")),
        (72,48,30),
        (("spine_02",.003,.40),("spine_03",.009,.32),("spine_04",.024,.24),
         ("spine_05",.048,.17),("tail_01",.090,.11),("tail_02",.145,.06),
         ("tail_03",.205,.02),("tail_fin",.16,-.02),
         ("pectoral_l",.012,.2),("pectoral_r",-.012,.2)), EXPOSED_SLITS),
)


def breathing_spec(bp: Blueprint) -> OperculumSpec | None:
    """Derive local cover dimensions from a species' own head and loft profile."""
    if bp.breathing_anatomy == EXPOSED_SLITS:
        return None
    if bp.breathing_anatomy != OPERCULATE:
        raise ValueError(f"{bp.faf_id} missing valid breathing anatomy")
    length = bp.shape[-1][0] - bp.shape[0][0]
    center_y = bp.eye_y + length * .12
    width, height, z = _shape_at(bp.shape, center_y)
    return OperculumSpec(center_y=center_y, center_z=z,
                         half_width=width, region_y=length * .16,
                         region_z=height * .90, parent_bone="spine_02")


def _material(name, hex_color):
    material = bpy.data.materials.get(name) or bpy.data.materials.new(name)
    rgb = tuple(int(hex_color[i:i+2],16)/255 for i in (1,3,5))
    linear = tuple(((v+.055)/1.055)**2.4 if v>.04045 else v/12.92 for v in rgb)
    material.use_nodes = True
    material.diffuse_color = (*linear,1)
    shader = material.node_tree.nodes.get("Principled BSDF")
    shader.inputs["Base Color"].default_value = (*linear,1)
    shader.inputs["Roughness"].default_value = .82
    shader.inputs["Metallic"].default_value = 0
    return material


def _shape_at(shape, y):
    for a,b in zip(shape,shape[1:]):
        if a[0] <= y <= b[0]:
            t=(y-a[0])/(b[0]-a[0])
            return tuple(a[i]*(1-t)+b[i]*t for i in (1,2,3))
    return tuple(shape[0 if y<shape[0][0] else -1][1:])


def _stripe_material(bp, y, vertical):
    if bp.number==2:
        for center,width in ((-.034,.010),(.002,.012),(.043,.008)):
            if abs(y-center) < width*.5: return 1
            if abs(y-center) < width*.5+.004: return 2
        return 0
    if bp.number==3:
        return 1 if vertical < -.28 else 0
    if bp.number==4:
        if abs(y+.051)<.008 or abs(y-.049)<.007: return 2
        return 1 if y>.030 else 0
    return 1 if vertical<-.26 else 0


def _mesh_object(name, vertices, faces, material_ids, materials, collection):
    mesh=bpy.data.meshes.new(name+'__geometry')
    mesh.from_pydata(vertices,[],faces)
    mesh.update()
    for material in materials: mesh.materials.append(material)
    for polygon,index in zip(mesh.polygons,material_ids):
        polygon.material_index=index
        polygon.use_smooth=False
    obj=bpy.data.objects.new(name,mesh)
    collection.objects.link(obj)
    return obj


def _body(bp, materials, collection):
    y0,y1=bp.shape[0][0],bp.shape[-1][0]
    samples={y0+(y1-y0)*i/bp.body_steps for i in range(bp.body_steps+1)}
    if bp.number==2:
        for center,width in ((-.034,.010),(.002,.012),(.043,.008)):
            samples.update((center-width*.5-.004,center-width*.5,center+width*.5,center+width*.5+.004))
    if bp.number==4:
        samples.update((-.059,-.043,.042,.056,.030))
    samples=sorted(y for y in samples if y0<=y<=y1)
    vertices=[];faces=[];regions=[]
    for y in samples:
        width,height,zcenter=_shape_at(bp.shape,y)
        for side in range(bp.radial_sides):
            angle=math.tau*side/bp.radial_sides
            vertices.append((width*math.cos(angle),y,zcenter+height*math.sin(angle)))
    sides=bp.radial_sides
    faces.append(tuple(reversed(range(sides))));regions.append(0)
    for ring in range(len(samples)-1):
        ymid=(samples[ring]+samples[ring+1])*.5
        for side in range(sides):
            next_side=(side+1)%sides
            faces.append((ring*sides+side,(ring+1)*sides+side,
                          (ring+1)*sides+next_side,ring*sides+next_side))
            regions.append(_stripe_material(bp,ymid,math.sin(math.tau*(side+.5)/sides)))
    faces.append(tuple((len(samples)-1)*sides+i for i in range(sides)));regions.append(0)
    return _mesh_object(bp.stem+'__body',vertices,faces,regions,materials,collection)


def _prism_parts(name, polygons, axis, thickness, material, collection):
    vertices=[];faces=[]
    for polygon in polygons:
        start=len(vertices)
        for sign in (-1,1):
            for point in polygon:
                p=list(point);p[axis]+=sign*thickness*.5;vertices.append(tuple(p))
        n=len(polygon)
        faces.append(tuple(start+i for i in reversed(range(n))))
        faces.append(tuple(start+n+i for i in range(n)))
        for i in range(n):
            j=(i+1)%n
            faces.append((start+i,start+j,start+n+j,start+n+i))
    return _mesh_object(name,vertices,faces,[0]*len(faces),[material],collection)


def _eye(name, center, radius, material, collection):
    mesh=bpy.data.meshes.new(name+'__geometry')
    bm=bmesh.new();bmesh.ops.create_icosphere(bm,subdivisions=1,radius=radius)
    for vert in bm.verts: vert.co+=Vector(center)
    bm.to_mesh(mesh);bm.free();mesh.update()
    mesh.materials.append(material)
    obj=bpy.data.objects.new(name,mesh);collection.objects.link(obj)
    return obj


def _fin_parts(bp, mats, collection):
    yrear=bp.shape[-1][0]
    if bp.number==5:
        return _shark_fins(bp,mats,collection)
    if bp.number==2:
        dorsal=[(0,-.033,.025),(0,-.023,.047),(0,.006,.053),(0,.043,.028)]
        anal=[(0,-.015,-.029),(0,.014,-.049),(0,.043,-.022)]
        tail_height=.034;tail_length=.035
        fin_mat=mats[0]
    elif bp.number==3:
        dorsal=[(0,-.035,.012),(0,-.017,.033),(0,.025,.035),(0,.045,.014)]
        anal=[(0,-.005,-.021),(0,.020,-.036),(0,.047,-.009)]
        tail_height=.029;tail_length=.041
        fin_mat=mats[0]
    else:
        dorsal=[(0,-.048,.030),(0,-.032,.074),(0,.012,.084),(0,.040,.068),(0,.061,.025)]
        anal=[(0,-.030,-.040),(0,.005,-.075),(0,.044,-.063),(0,.061,-.022)]
        tail_height=.047;tail_length=.042
        fin_mat=mats[1]
    parts={}
    parts['dorsal_fin']=_prism_parts(bp.stem+'__dorsal_fin',[dorsal],0,.003,fin_mat,collection)
    parts['anal_fin']=_prism_parts(bp.stem+'__anal_fin',[anal],0,.003,fin_mat,collection)
    ybase=yrear-.009; ytip=yrear+tail_length
    if bp.number==2:
        tail=[[(0,ybase,.004),(0,ytip,tail_height*.75),
               (0,ytip+.003,0),(0,ytip,-tail_height*.75),(0,ybase,-.004)]]
    else:
        tail=[[(0,ybase,.003),(0,ytip,tail_height),(0,yrear+tail_length*.46,.010),(0,yrear+.005,0)],
              [(0,ybase,-.003),(0,yrear+.005,0),(0,yrear+tail_length*.46,-.010),(0,ytip,-tail_height)]]
    parts['tail_fin']=_prism_parts(bp.stem+'__tail_fin',tail,0,.003,fin_mat,collection)
    attach_y=bp.eye_y+.013
    width=_shape_at(bp.shape,attach_y)[0]*.88
    for sign,label in ((1,'l'),(-1,'r')):
        x=sign*width
        fin=[(x,attach_y,-.004),(x+sign*width*.75,attach_y+.013,-.013),
             (x+sign*width*.38,attach_y+.024,-.009),(x,attach_y+.012,-.005)]
        parts['pectoral_'+label]=_prism_parts(bp.stem+'__pectoral_'+label,[fin],2,.0025,fin_mat,collection)
    return parts


def _shark_fins(bp,mats,collection):
    parts={}
    grey=mats[0];black=mats[2]
    dorsal=[(0,-.14,.086),(0,-.09,.28),(0,-.015,.21),(0,.20,.078)]
    parts['dorsal_fin']=_prism_parts(bp.stem+'__dorsal_fin',[dorsal],0,.010,grey,collection)
    tip=[(0,-.105,.215),(0,-.09,.283),(0,-.015,.21),(0,-.035,.178)]
    parts['dorsal_tip']=_prism_parts(bp.stem+'__dorsal_tip',[tip],0,.012,black,collection)
    parts['anal_fin']=_prism_parts(bp.stem+'__anal_fin',
        [[(0,.24,-.065),(0,.39,-.125),(0,.48,-.050)]],0,.007,grey,collection)
    for sign,label in ((1,'l'),(-1,'r')):
        base=sign*.098;tipx=sign*.39
        wing=[(base,-.28,-.035),(tipx,.025,-.09),(sign*.27,.115,-.065),(base,-.05,-.035)]
        parts['pectoral_'+label]=_prism_parts(bp.stem+'__pectoral_'+label,[wing],2,.013,grey,collection)
        dark=[(sign*.30,-.030,-.080),(tipx,.025,-.09),(sign*.27,.115,-.065)]
        parts['pectoral_'+label+'_tip']=_prism_parts(bp.stem+'__pectoral_'+label+'_tip',[dark],2,.016,black,collection)
    upper=[(0,.62,0),(0,.74,.11),(0,.91,.29),(0,.87,.14),(0,.76,.005)]
    lower=[(0,.62,0),(0,.76,.005),(0,.87,-.17),(0,.73,-.085)]
    parts['tail_fin']=_prism_parts(bp.stem+'__tail_fin',[upper,lower],0,.012,grey,collection)
    tip=[(0,.845,.21),(0,.91,.29),(0,.87,.14),(0,.83,.105)]
    parts['tail_tip']=_prism_parts(bp.stem+'__tail_tip',[tip],0,.014,black,collection)
    return parts


def _armature(bp, collection):
    data=bpy.data.armatures.new(bp.stem+'__rig_data')
    armature=bpy.data.objects.new(bp.stem+'__armature',data)
    collection.objects.link(armature)
    bpy.ops.object.select_all(action='DESELECT')
    armature.select_set(True)
    bpy.context.view_layer.objects.active=armature
    bpy.ops.object.mode_set(mode='EDIT')
    root=data.edit_bones.new('root')
    root.head=(0,-.012,0);root.tail=(0,.012,0)
    previous=root
    for name,start,end in zip(bp.chain_names,bp.chain,bp.chain[1:]):
        bone=data.edit_bones.new(name)
        bone.head=(0,start,0);bone.tail=(0,end,0)
        bone.parent=previous
        previous=bone
    tail=data.edit_bones.new('tail_fin')
    tail.head=(0,bp.chain[-1],0)
    tail.tail=(0,bp.chain[-1]+(.19 if bp.number==5 else .025),0)
    tail.parent=previous
    # Fins are optional control/deform branches. The shark's pectorals sit much
    # farther forward and remain relatively steady during the body wave.
    pectoral_parent=data.edit_bones[bp.chain_names[1]]
    dorsal_parent=data.edit_bones[bp.chain_names[2]]
    anchor_y=bp.eye_y+(.20 if bp.number==5 else .015)
    anchor_width=_shape_at(bp.shape,anchor_y)[0]*.78
    for sign,label in ((1,'l'),(-1,'r')):
        bone=data.edit_bones.new('pectoral_'+label)
        bone.head=(sign*anchor_width,anchor_y,0)
        bone.tail=(sign*(anchor_width+(.13 if bp.number==5 else .013)),
                   anchor_y+(.055 if bp.number==5 else .009),-.008)
        bone.parent=pectoral_parent
    for name,base,tip in (('dorsal',(.00,.00,.02),(.00,.01,.055)),
                          ('anal',(.00,.01,-.02),(.00,.02,-.055))):
        bone=data.edit_bones.new(name)
        factor=4.0 if bp.number==5 else 1.0
        bone.head=(base[0],base[1]*factor,base[2]*factor)
        bone.tail=(tip[0],tip[1]*factor,tip[2]*factor)
        bone.parent=dorsal_parent
    bpy.ops.object.mode_set(mode='OBJECT')
    return armature


def _bind(obj, armature, bone=None, stations=None):
    obj.parent=armature
    modifier=obj.modifiers.new('FAF Skin','ARMATURE')
    modifier.object=armature
    if stations is not None:
        skin_body_at_stations(obj,stations)
    else:
        group=obj.vertex_groups.new(name=bone)
        group.add(list(range(len(obj.data.vertices))),1.0,'REPLACE')


def create_fish(bp: Blueprint, scene):
    """Build one species in an empty scene; return runtime objects and report."""
    if bp.breathing_anatomy not in (OPERCULATE, EXPOSED_SLITS):
        raise ValueError(f"{bp.faf_id} missing valid breathing anatomy")
    collection=bpy.data.collections.new(bp.faf_id+' Source')
    scene.collection.children.link(collection)
    scene.unit_settings.system='METRIC'
    scene.unit_settings.scale_length=1
    materials=[_material(f'mat__{bp.stem}_{role}',color) for role,color in bp.colors]
    body=_body(bp,materials,collection)
    fins=_fin_parts(bp,materials,collection)
    width=_shape_at(bp.shape,bp.eye_y)[0]
    eyes={}
    for sign,label in ((1,'l'),(-1,'r')):
        center=(sign*(width+bp.eye_radius*.12),bp.eye_y,bp.eye_z)
        eyes[label]=_eye(bp.stem+'__eye_'+label,center,bp.eye_radius,materials[-1],collection)
    armature=_armature(bp,collection)
    breathing=breathing_spec(bp)
    if breathing is not None:
        add_operculum_rig(armature,breathing)
    else:
        classify_breathing(armature,EXPOSED_SLITS)
    # Mid-bone stations preserve a rigid head and blend each adjacent body
    # segment. The shark has more rear segments and a longer stable forebody.
    stations=[(-(a+b)*.5,{name:1.0}) for name,a,b in
              zip(bp.chain_names,bp.chain,bp.chain[1:])]
    _bind(body,armature,stations=stations)
    if breathing is not None:
        weight_operculum_region(body,breathing)
    for name,obj in fins.items():
        if name.startswith('pectoral_l'): bone='pectoral_l'
        elif name.startswith('pectoral_r'): bone='pectoral_r'
        elif name.startswith('dorsal'): bone='dorsal'
        elif name.startswith('anal'): bone='anal'
        else: bone='tail_fin'
        _bind(obj,armature,bone=bone)
    for obj in eyes.values():_bind(obj,armature,bone='spine_01')
    meshes=[body,*fins.values(),*eyes.values()]
    skin=skin_report(meshes,armature)
    if skin['problems']:
        raise RuntimeError(f'{bp.faf_id} invalid weights: {skin["problems"][:8]}')
    for role,frames,scale in (('Glide',bp.frames[0],.30),('Swim',bp.frames[1],1.0),
                              ('SwimFast',bp.frames[2],1.35)):
        profile={name:(amplitude*scale,phase) for name,amplitude,phase in bp.wave}
        make_wave_action(armature,bp.stem+'__'+role,frames,profile)
    armature.animation_data.action=bpy.data.actions[bp.stem+'__Glide']
    armature.animation_data.use_nla=False
    bpy.context.scene.frame_set(1)
    reset_pose(armature)
    bpy.context.view_layer.update()
    breathing_check=validate_breathing_rig(armature,meshes,breathing,
        require_complete=bp.breathing_anatomy==OPERCULATE)
    return armature,meshes,inspect_fish(bp,armature,meshes,breathing_check)


def inspect_fish(bp: Blueprint, armature, meshes, breathing_check=None):
    """Recompute source statistics without rebuilding or changing a fish."""
    coordinates=[tuple(vertex.co) for obj in meshes for vertex in obj.data.vertices]
    bounds=[(min(v[i] for v in coordinates),max(v[i] for v in coordinates)) for i in range(3)]
    triangles=sum(sum(max(0,len(p.vertices)-2) for p in obj.data.polygons) for obj in meshes)
    report={
        'faf_id':bp.faf_id,'species':bp.slug,'body_family':bp.family,
        'dimensions_m':[round(b-a,5) for a,b in bounds],
        'triangles':triangles,'materials':len(bp.colors),
        'material_names':[f'mat__{bp.stem}_{role}' for role,_ in bp.colors],
        'meshes':len(meshes),'bones':len(armature.data.bones),
        'bone_names':[b.name for b in armature.data.bones],
        'actions':[bp.stem+'__'+role for role in ('Glide','Swim','SwimFast')],
        'frames':dict(zip(('Glide','Swim','SwimFast'),bp.frames)),
        'origin_m':[0,0,0],'rotation_rad':[0,0,0],'scale':[1,1,1],
        'front_blender':'-Y','up_blender':'+Z',
        'root_translation_animated':False,'source_generator':'fish.reef_families.v1',
        'breathing_anatomy':bp.breathing_anatomy,
        'breathing_validation':breathing_check,
        'generator_config':{
            'shape_stations':bp.shape,'radial_sides':bp.radial_sides,
            'body_steps':bp.body_steps,'chain_stations':bp.chain,
            'wave_profile':bp.wave,'frames':bp.frames,
            'colors':bp.colors,'deterministic':True,
            'breathing_anatomy':bp.breathing_anatomy,
        },
    }
    return report
