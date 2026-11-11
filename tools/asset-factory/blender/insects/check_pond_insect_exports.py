"""Validate actual exported GLB bytes, not just Blender scene state."""
from pathlib import Path
import argparse,json,struct

def glb(path):
    blob=path.read_bytes()
    assert blob[:4]==b'glTF' and struct.unpack_from('<I',blob,4)[0]==2
    assert struct.unpack_from('<I',blob,8)[0]==len(blob)
    n,k=struct.unpack_from('<II',blob,12);assert k==0x4e4f534a
    return json.loads(blob[20:20+n]),blob[28+n:]
def vectors(doc,binary,accessor_id):
    a=doc['accessors'][accessor_id];v=doc['bufferViews'][a['bufferView']]
    width={'SCALAR':1,'VEC3':3,'VEC4':4}[a['type']]
    assert a['componentType']==5126
    start=v.get('byteOffset',0)+a.get('byteOffset',0);stride=v.get('byteStride',4*width)
    return [struct.unpack_from('<'+'f'*width,binary,start+i*stride) for i in range(a['count'])]
def check(root):
    records=json.loads((root/'assets/faf/pond/insects/pond_insect_001_manifest.json').read_text())['assets']
    assert len(records)==4 and len({r['faf_id'] for r in records})==4
    for r in records:
        d,b=glb(root/r['glb']);nodes=d['nodes'];joint=d['skins'][0]['joints']
        assert len(d.get('skins',[]))==1 and len(joint)==r['bones']
        assert {a['name'] for a in d['animations']}==set(r['actions'])
        assert {m['name'] for m in d['materials']}==set(r['material_names'])
        assert {nodes[i]['name'] for i in joint}==set(r['bone_names'])
        assert all(node.get('skin')==0 and node.get('extras',{}).get('faf_id')==r['faf_id']
            and node['extras'].get('faf_habitat')=='pond' and node['extras'].get('faf_role')=='ambient_only' and node['extras'].get('faf_placement_zone')==r['placement_zone']
            for node in nodes if 'mesh' in node)
        triangles=0;meshes=0
        for node in nodes:
            if 'mesh' not in node:continue
            meshes+=1
            for prim in d['meshes'][node['mesh']]['primitives']:
                assert {'POSITION','JOINTS_0','WEIGHTS_0'}.issubset(prim['attributes'])
                triangles += d['accessors'][prim['indices']]['count']//3
        assert triangles==r['triangles']
        roots=[i for i,n in enumerate(nodes) if n.get('name')=='root'];assert len(roots)==1
        moving=False
        for anim in d['animations']:
            for ch in anim['channels']:
                sampler=anim['samplers'][ch['sampler']]
                if ch['target']['path'] not in ('rotation','translation','scale'):continue
                values=vectors(d,b,sampler['output'])
                if ch['target']['node']==roots[0]:
                    assert all(max(abs(x-y) for x,y in zip(values[0],v))<1e-5 for v in values)
                elif ch['target']['path']=='rotation':
                    if max(max(abs(x-y) for x,y in zip(values[0],v)) for v in values)>1e-4:moving=True
                    assert max(abs(x-y) for x,y in zip(values[0],values[-1]))<2e-4
        assert moving
        print('FAF_INSECT_GLB_OK',r['faf_id'],'triangles',triangles,'meshes',meshes,'bones',len(joint),'loop',r['actions'][0])
    print('FAF_INSECT_EXPORTS_OK',len(records))
if __name__=='__main__':
    p=argparse.ArgumentParser();p.add_argument('--root',type=Path,required=True)
    check(p.parse_args().root.resolve())
