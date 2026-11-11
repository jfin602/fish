"""Build four original Pond insect variants; Blender -Y forward, +Z up, metres."""
from pathlib import Path
import argparse, json, math, sys
import bpy

BASE = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(BASE))
from common.export import export_selected_glb
from creatures.pond_creatures import material, ellipsoid, tube, polygon, armature, rigid, skin

RECIPES = (
    dict(family=1, variant='a', slug='green_darner_nymph', common='Common green darner nymph',
         species='Anax junius', zone='submerged shallow silt, wood, plant roots', length=.042, width=.014,
         leg_spread=.018, wing=.008, colors=('#665f3c','#877b4d','#343b29','#a89b69'), role='Crawl', phase=0),
    dict(family=1, variant='b', slug='green_darner_nymph', common='Common green darner nymph',
         species='Anax junius', zone='submerged shallow silt, wood, plant roots', length=.052, width=.0125,
         leg_spread=.023, wing=.010, colors=('#6d6943','#96905c','#333b2d','#afa272'), role='Crawl', phase=.13),
    dict(family=2, variant='a', slug='backswimmer', common='Backswimmer',
         species='Notonecta undulata', zone='inverted just below shallow water surface', length=.015, width=.006,
         leg_spread=.014, wing=.0035, colors=('#9b8d61','#5b543d','#242a22','#c1b287'), role='Row', phase=0),
    dict(family=2, variant='b', slug='backswimmer', common='Backswimmer',
         species='Notonecta undulata', zone='inverted just below shallow water surface', length=.017, width=.0042,
         leg_spread=.019, wing=.003, colors=('#b1a072','#625b42','#252b24','#d2bd89'), role='Row', phase=.11),
)

def ident(c): return f"FAF-INSECT-{c['family']:03d}{c['variant'].upper()}"
def stem(c): return f"insect_{c['family']:03d}{c['variant']}_{c['slug']}"

def build_nymph(c, mats):
    st=stem(c); L=c['length']; w=c['width']; s=c['leg_spread']; wing=c['wing']
    specs=[('root',(0,0,-.006),(0,0,.006),None),
           ('thorax',(0,-L*.13,0),(0,-L*.27,0),'root'),
           ('head',(0,-L*.28,0),(0,-L*.40,0),'thorax'),
           ('abdomen_01',(0,L*.04,0),(0,L*.25,0),'thorax'),
           ('abdomen_02',(0,L*.25,0),(0,L*.48,0),'abdomen_01')]
    for side,label in ((1,'L'),(-1,'R')):
        for i in range(3):
            y=(-.22+i*.18)*L
            specs.append((f'leg_{i+1}_{label}',(side*w*.72,y,-.003),(side*s,y+L*.025,-.014),'thorax'))
    arm=armature(st,specs); parts=[]
    # Tapered, connected abdomen with ring-to-ring skin blend.
    ys=[-L*.18,-L*.09,0,L*.10,L*.22,L*.34,L*.47]
    pts=[(0,y,-.002) for y in ys]
    r=[w*.56,w*.72,w*.78,w*.74,w*.57,w*.35,w*.10]
    abdomen=tube(st+'__segmented_abdomen',pts,r,[mats[0],mats[1]],8,lambda i,j: 1 if i in (1,3,5) else 0)
    inf=[]
    for i in range(len(ys)):
        weights=({'thorax':1.0} if i==0 else {'thorax':.75,'abdomen_01':.25} if i==1 else
                 {'abdomen_01':1.0} if i in (2,3) else {'abdomen_01':.6,'abdomen_02':.4} if i==4 else {'abdomen_02':1.0})
        inf.extend([weights]*8)
    parts.append(skin(abdomen,arm,inf))
    parts.append(rigid(ellipsoid(st+'__thorax',(0,-L*.18,.001),(w*.88,L*.17,w*.55),[mats[0]],4,8),arm,'thorax'))
    parts.append(rigid(ellipsoid(st+'__broad_head',(0,-L*.37,.004),(w*1.04,L*.13,w*.51),[mats[1]],4,8),arm,'head'))
    for side,label in ((1,'L'),(-1,'R')):
        parts.append(rigid(ellipsoid(st+f'__eye_{label}',(side*w*.87,-L*.41,.009),(w*.16,L*.032,w*.16),[mats[2]],3,6),arm,'head'))
        # Dorsal wing pads point rearward and are unequal to keep this a naiad.
        pad=polygon(st+f'__wing_pad_{label}',[(side*w*.37,-L*.18,w*.45),(side*(w*.47+wing*.22),L*.03,w*.36),
            (side*(w*.29+wing*.12),L*.23,w*.29),(side*w*.18,L*.02,w*.41)],[mats[2]],.002)
        parts.append(rigid(pad,arm,'thorax'))
        for i in range(3):
            y=(-.22+i*.18)*L
            leg=tube(st+f'__leg_{i+1}_{label}',[(side*w*.72,y,-.003),(side*s*.72,y+L*.015,-.012),
                (side*s,y+L*.043,-.021)],[w*.13,w*.11,w*.045],[mats[0]],6)
            parts.append(rigid(leg,arm,f'leg_{i+1}_{label}'))
    return arm,parts

def build_bug(c,mats):
    st=stem(c); L=c['length']; w=c['width']; s=c['leg_spread']
    specs=[('root',(0,0,-.005),(0,0,.005),None),
           ('body',(0,0,0),(0,-L*.20,0),'root'),
           ('head',(0,-L*.30,0),(0,-L*.43,0),'body'),
           ('abdomen',(0,L*.03,0),(0,L*.42,0),'body')]
    for side,label in ((1,'L'),(-1,'R')):
        specs += [(f'oar_{label}',(side*w*.7,L*.09,-.003),(side*s,L*.19,-.014),'body'),
                  (f'foreleg_{label}',(side*w*.65,-L*.29,0),(side*s*.46,-L*.30,-.012),'body'),
                  (f'midleg_{label}',(side*w*.75,-L*.05,0),(side*s*.49,-L*.01,-.012),'body')]
    arm=armature(st,specs); parts=[]
    # Boat hull remains coherent as the abdomen bends slightly around its hinge.
    pts=[(0,y,0) for y in (-L*.39,-L*.23,-L*.07,L*.09,L*.25,L*.39,L*.47)]
    r=[w*.45,w*.88,w*.96,w*.88,w*.66,w*.35,w*.06]
    hull=tube(st+'__boat_body',pts,r,[mats[0],mats[1]],8,lambda i,j: 1 if j in (3,4,5) else 0)
    weights=[]
    for i in range(len(pts)):
        weights.extend([{'body':1.0} if i<=2 else {'body':.7,'abdomen':.3} if i==3 else {'abdomen':1.0}]*8)
    parts.append(skin(hull,arm,weights))
    parts.append(rigid(ellipsoid(st+'__head',(0,-L*.34,0),(w*.63,L*.12,w*.40),[mats[1]],4,8),arm,'head'))
    for side,label in ((1,'L'),(-1,'R')):
        parts.append(rigid(ellipsoid(st+f'__eye_{label}',(side*w*.53,-L*.37,.004),(w*.21,L*.038,w*.22),[mats[2]],3,6),arm,'head'))
        # Broad leaf-shaped oar paddle on a long angled leg, no costly hair fringe.
        oar=tube(st+f'__oar_{label}',[(side*w*.70,L*.07,-.003),(side*s*.67,L*.16,-.010),
            (side*s,L*.24,-.014)],[w*.12,w*.10,w*.05],[mats[1]],6)
        parts.append(rigid(oar,arm,f'oar_{label}'))
        paddle=polygon(st+f'__paddle_{label}',[(side*s*.78,L*.17,-.011),(side*(s+c['wing']*.4),L*.29,-.015),
            (side*s,L*.36,-.015),(side*s*.88,L*.24,-.011)],[mats[3]],.0015)
        parts.append(rigid(paddle,arm,f'oar_{label}'))
        for kind,y,reach in (('foreleg',-L*.29,.46),('midleg',-L*.05,.49)):
            part=tube(st+f'__{kind}_{label}',[(side*w*.72,y,0),(side*s*reach,y+L*.025,-.008),
                (side*s*(reach+.11),y+L*.055,-.013)],[w*.10,w*.065,w*.02],[mats[1]],5)
            parts.append(rigid(part,arm,f'{kind}_{label}'))
    return arm,parts

def animate(c,arm):
    st=stem(c); role=c['role']; action=bpy.data.actions.new(st+'__'+role);action.use_fake_user=True
    arm.animation_data_create();arm.animation_data.action=action
    for i in range(17):
        t=i/16; frame=1+60*t; a=math.sin(math.tau*(t+c['phase']))
        for bone in arm.pose.bones:
            if bone.name=='root':continue
            bone.rotation_mode='XYZ'; bone.rotation_euler=(0,0,0)
            if c['family']==1:
                if bone.name=='abdomen_01':bone.rotation_euler.z=.055*a
                if bone.name=='abdomen_02':bone.rotation_euler.z=.105*math.sin(math.tau*(t-.12+c['phase']))
                if bone.name.startswith('leg_'):
                    side=1 if bone.name.endswith('_L') else -1
                    leg=int(bone.name.split('_')[1])
                    bone.rotation_euler.z=side*.25*math.sin(math.tau*(t+(leg%2)*.5+(0 if side>0 else .5)))
            else:
                if bone.name=='abdomen':bone.rotation_euler.z=.025*a
                if bone.name.startswith('oar_'):
                    side=1 if bone.name.endswith('_L') else -1
                    bone.rotation_euler.z=side*.43*math.sin(math.tau*(t+c['phase']))
                if bone.name.startswith(('foreleg_','midleg_')):
                    side=1 if bone.name.endswith('_L') else -1
                    bone.rotation_euler.z=side*.08*a
            bone.keyframe_insert(data_path='rotation_euler',frame=frame,group=bone.name)
    track=arm.animation_data.nla_tracks.new();track.name=action.name;track.strips.new(action.name,1,action)
    arm.animation_data.use_nla=False
    for b in arm.pose.bones:b.rotation_euler=(0,0,0)
    bpy.context.scene.frame_set(1)
    return action.name

def make(c,root,overwrite):
    bpy.ops.wm.read_factory_settings(use_empty=True)
    bpy.context.scene.unit_settings.system='METRIC'
    st=stem(c); mats=[material(f'mat__{st}_{i}',x) for i,x in enumerate(c['colors'])]
    arm,parts=(build_nymph if c['family']==1 else build_bug)(c,mats)
    # Merge same-rig parts into one skinned mesh to reduce group draw calls.
    bpy.ops.object.select_all(action='DESELECT')
    for part in parts:part.select_set(True)
    bpy.context.view_layer.objects.active=parts[0]
    bpy.ops.object.join()
    parts=[parts[0]]
    parts[0].name=st+'__skinned_mesh'
    act=animate(c,arm)
    for obj in [arm,*parts]:
        obj['faf_id']=ident(c);obj['faf_family_id']=f"FAF-INSECT-{c['family']:03d}"
        obj['faf_variant']=c['variant'].upper();obj['faf_habitat']='pond'
        obj['faf_species']=c['species'];obj['faf_role']='ambient_only'
        obj['faf_placement_zone']=c['zone']
    coords=[v.co for p in parts for v in p.data.vertices]
    dims=[round(max(v[i] for v in coords)-min(v[i] for v in coords),6) for i in range(3)]
    tris=sum(sum(len(poly.vertices)-2 for poly in p.data.polygons) for p in parts)
    used=sorted({slot.material.name for p in parts for slot in p.material_slots if slot.material})
    src=root/'assets/faf/pond/insects'/f'{st}.blend'
    glb=root/'game/assets/faf/pond/insects'/f'{st}.glb'
    bpy.ops.object.select_all(action='DESELECT')
    for o in [arm,*parts]:o.select_set(True)
    bpy.context.view_layer.objects.active=arm
    export_selected_glb(str(glb),include_animations=True,overwrite=overwrite,animation_mode='NLA_TRACKS',export_metadata=True)
    bpy.context.preferences.filepaths.save_version=0
    bpy.ops.wm.save_as_mainfile(filepath=str(src),compress=True)
    return dict(faf_id=ident(c),faf_family_id=f"FAF-INSECT-{c['family']:03d}",variant=c['variant'].upper(),
        category='INSECT',habitat='pond',role='ambient_only',common_name=c['common'],scientific_name=c['species'],
        placement_zone=c['zone'],stem=st,dimensions_m=dims,triangles=tris,materials=len(used),material_names=used,
        bones=len(arm.data.bones),bone_names=[b.name for b in arm.data.bones],actions=[act],loop_roles=[c['role']],
        source=src.relative_to(root).as_posix(),glb=glb.relative_to(root).as_posix(),
        generator='insects.build_pond_insects.v1',parameters={k:c[k] for k in ('length','width','leg_spread','wing','phase')},
        origin_m=[0,0,0],rotation_rad=[0,0,0],scale=[1,1,1],front_blender='-Y',up_blender='+Z',
        technical_review='pending_export_check',visual_review='pending_native_review')

def main(root,overwrite):
    owned=[root/'assets/faf/pond/insects'/f'{stem(c)}.blend' for c in RECIPES]
    owned += [root/'game/assets/faf/pond/insects'/f'{stem(c)}.glb' for c in RECIPES]
    owned += [root/'assets/faf/pond/insects/pond_insect_001_manifest.json']
    if not overwrite and any(p.exists() for p in owned):raise FileExistsError('Refusing to overwrite owned insect outputs')
    for p in owned:p.parent.mkdir(parents=True,exist_ok=True)
    records=[make(c,root,overwrite) for c in RECIPES]
    manifest={'pack':'Pond FAF insects 001','habitat':'pond','source_reference':'owner /home/jfin/best-pond-faf.png style only',
              'blender_version':bpy.app.version_string,'assets':records}
    owned[-1].write_text(json.dumps(manifest,indent=2)+'\n')
    for r in records:print('FAF_INSECT_BUILT',r['faf_id'],r['triangles'],r['materials'],r['bones'],r['dimensions_m'])

if __name__=='__main__':
    args=sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else []
    p=argparse.ArgumentParser();p.add_argument('--root',type=Path,required=True);p.add_argument('--overwrite',action='store_true')
    opts=p.parse_args(args);main(opts.root.resolve(),opts.overwrite)
