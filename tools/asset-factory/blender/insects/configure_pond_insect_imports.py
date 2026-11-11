"""Mark the four imported insect Actions as seamless Godot loops."""
from pathlib import Path
import argparse,json,re

def configure(root):
    entries=json.loads((root/'assets/faf/pond/insects/pond_insect_001_manifest.json').read_text())['assets']
    for entry in entries:
        path=root/(entry['glb']+'.import')
        source=path.read_text()
        clip=entry['actions'][0]
        replacement='_subresources={\n"animations": '+json.dumps({clip:{'settings/loop_mode':1}},indent=2)+'\n}'
        updated,hits=re.subn(r'_subresources=\{.*?\}\n(?=gltf/naming_version)',replacement+'\n',source,flags=re.DOTALL)
        if hits!=1:raise ValueError(path)
        path.write_text(updated)
    return len(entries)
if __name__=='__main__':
    p=argparse.ArgumentParser();p.add_argument('--root',type=Path,required=True)
    print('FAF_POND_INSECT_IMPORT_OPTIONS',configure(p.parse_args().root.resolve()))
