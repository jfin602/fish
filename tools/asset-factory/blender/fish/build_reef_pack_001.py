"""Build FAF-FISH-002–005 without altering FAF-FISH-001.

Background entry: blender -b --python tools/asset-factory/blender/fish/build_reef_pack_001.py -- --root REPO
Existing outputs require --overwrite. Blender MCP may call create_scenes and
export_scenes in a live session, then restore the pre-existing scene. Save the
editable master only from a clean background process, not a live shared file.
"""

from __future__ import annotations

import argparse
import json
from pathlib import Path
import sys

import bpy

TOOLING=Path(__file__).resolve().parents[1]
if str(TOOLING) not in sys.path:sys.path.insert(0,str(TOOLING))

from common.export import export_selected_glb  # noqa: E402
from common.validation import inspect_objects  # noqa: E402
from fish.reef_families import BLUEPRINTS,create_fish  # noqa: E402


def inspect_loops(bp,scene,armature):
    """Evaluate each authored seam and the presentation-only root in Blender."""
    result={}
    bpy.context.window.scene=scene
    for role,frames in zip(('Glide','Swim','SwimFast'),bp.frames):
        armature.animation_data.action=bpy.data.actions[bp.stem+'__'+role]
        scene.frame_set(1)
        first={bone.name:tuple(bone.rotation_euler) for bone in armature.pose.bones}
        root_start=tuple(armature.pose.bones['root'].location)
        object_start=tuple(armature.location)
        scene.frame_set(1+frames//4)
        root_quarter=tuple(armature.pose.bones['root'].location)
        object_quarter=tuple(armature.location)
        scene.frame_set(1+frames)
        last={bone.name:tuple(bone.rotation_euler) for bone in armature.pose.bones}
        seam=max(abs(a-b) for name in first for a,b in zip(first[name],last[name]))
        root_fixed=(root_start==root_quarter==(0.0,0.0,0.0)
                    and object_start==object_quarter==(0.0,0.0,0.0))
        if seam>1e-6 or not root_fixed:
            raise RuntimeError(f'{bp.faf_id} {role} failed loop/root check: {seam}, {root_fixed}')
        result[role]={'seam_max_rad':round(seam,8),'root_translation':False,'loop':True}
    return result


def create_scenes():
    built=[]
    for bp in BLUEPRINTS:
        scene=bpy.data.scenes.new(bp.faf_id+' '+bp.slug.replace('_',' ').title())
        bpy.context.window.scene=scene
        armature,meshes,report=create_fish(bp,scene)
        report['loop_checks']=inspect_loops(bp,scene,armature)
        built.append((bp,scene,armature,meshes,report))
    return built


def export_scenes(built,root:Path,overwrite=False):
    folder=root/'game/assets/faf/fish'
    folder.mkdir(parents=True,exist_ok=True)
    reports=[]
    for bp,scene,armature,meshes,report in built:
        bpy.context.window.scene=scene
        bpy.ops.object.select_all(action='DESELECT')
        for obj in [armature,*meshes]:obj.select_set(True)
        bpy.context.view_layer.objects.active=armature
        check=inspect_objects([armature,*meshes])
        if check['problems'] or check['triangle_count_source']!=report['triangles']:
            raise RuntimeError(f'{bp.faf_id} export failed source checks: {check}')
        output=folder/(bp.stem+'.glb')
        export_selected_glb(str(output),include_animations=True,overwrite=overwrite,
                            animation_mode='NLA_TRACKS')
        report['glb']=output.relative_to(root).as_posix()
        report['blender_source']='assets/faf/fish/reef_fish_002_to_005.blend'
        reports.append(report)
    return reports


def save_source_and_manifest(built,reports,root:Path,overwrite=False):
    source=root/'assets/faf/fish/reef_fish_002_to_005.blend'
    manifest=source.with_name('reef_fish_002_to_005_manifest.json')
    if not overwrite and (source.exists() or manifest.exists()):
        raise FileExistsError('existing FAF reef-fish output; pass --overwrite deliberately')
    source.parent.mkdir(parents=True,exist_ok=True)
    # This is a normal editable project, not a Blender append-only library.
    bpy.ops.wm.save_as_mainfile(filepath=str(source),compress=True)
    manifest.write_text(json.dumps({
        'reference_fish':'FAF-FISH-001 Yellow Tang (preserved)',
        'source':source.relative_to(root).as_posix(),
        'assets':reports,
    },indent=2)+'\n')
    return source


def build(root:Path,overwrite=False):
    source=root/'assets/faf/fish/reef_fish_002_to_005.blend'
    if source.exists() and not overwrite:raise FileExistsError(source)
    bpy.ops.wm.read_factory_settings(use_empty=True)
    factory_scenes=list(bpy.data.scenes)
    built=create_scenes()
    for scene in factory_scenes:
        bpy.data.scenes.remove(scene)
    reports=export_scenes(built,root,overwrite)
    save_source_and_manifest(built,reports,root,overwrite)
    return reports


if __name__=='__main__':
    args=sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else []
    parser=argparse.ArgumentParser()
    parser.add_argument('--root',type=Path,required=True)
    parser.add_argument('--overwrite',action='store_true')
    options=parser.parse_args(args)
    print('FAF_REEF_FISH_REPORT='+json.dumps(build(options.root,options.overwrite)))
