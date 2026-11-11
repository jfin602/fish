"""Evaluate source-file skin deformation and loop seams for all four variants."""
from pathlib import Path
import argparse,json,sys
import bpy

def evaluated_points(obj):
    deps=bpy.context.evaluated_depsgraph_get()
    evaluated=obj.evaluated_get(deps)
    mesh=evaluated.to_mesh()
    points=[tuple(v.co) for v in mesh.vertices]
    evaluated.to_mesh_clear()
    return points

def check(root):
    records=json.loads((root/'assets/faf/pond/insects/pond_insect_001_manifest.json').read_text())['assets']
    for r in records:
        bpy.ops.wm.open_mainfile(filepath=str(root/r['source']))
        arm=bpy.data.objects.get(r['stem']+'__armature')
        mesh=bpy.data.objects.get(r['stem']+'__skinned_mesh')
        assert arm and mesh and len(arm.data.bones)==r['bones']
        assert len(mesh.data.materials)==r['materials']
        assert len(arm.animation_data.nla_tracks)==1
        assert arm.animation_data.nla_tracks[0].name==r['actions'][0]
        assert all(abs(x)<1e-7 for x in arm.location) and all(abs(x-1)<1e-7 for x in arm.scale)
        assert all(abs(x)<1e-7 for x in mesh.location) and all(abs(x-1)<1e-7 for x in mesh.scale)
        for f in (1,22,61):
            bpy.context.scene.frame_set(f)
            if f==1:start=evaluated_points(mesh)
            elif f==22:middle=evaluated_points(mesh)
            else:end=evaluated_points(mesh)
        assert len(start)==len(middle)==len(end)
        moving=max(sum((a-b)**2 for a,b in zip(x,y))**.5 for x,y in zip(start,middle))
        seam=max(sum((a-b)**2 for a,b in zip(x,y))**.5 for x,y in zip(start,end))
        assert moving>.0005 and seam<.00002,(r['faf_id'],moving,seam)
        print('FAF_INSECT_SOURCE_OK',r['faf_id'],'vertices',len(start),'maximum_deformation_m',round(moving,6),'seam_m',round(seam,8))
    print('FAF_INSECT_SOURCES_OK',len(records))
if __name__=='__main__':
    args=sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else []
    p=argparse.ArgumentParser();p.add_argument('--root',type=Path,required=True)
    check(p.parse_args(args).root.resolve())
