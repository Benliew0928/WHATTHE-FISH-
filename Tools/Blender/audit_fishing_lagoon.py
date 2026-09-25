"""Read-only audit of the saved fishing master and modular exports."""
import bpy,json,math
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2]
SRC=ROOT/'ArtSource/Fishing/Lagoon';OUT=ROOT/'Game/Assets/_Game/Art/Fishing/Lagoon'
bpy.ops.wm.open_mainfile(filepath=str(SRC/'Island_Lagoon_5P.blend'))
data=json.loads((SRC/'lagoon-manifest.json').read_text());results=[]
def check(ok,label):
    results.append(('PASS ' if ok else 'FAIL ')+label)
    if not ok:raise AssertionError(label)
check(len(data['modules'])==16,'sixteen module assets')
stands=[p for p in data['placements'] if p['module']=='PlayerStand']
check(len(stands)==5,'five player stand instances')
check(len({s['accent'] for s in stands})==5,'five independent accent colors')
check(all(abs(math.hypot(s['position']['x'],s['position']['z'])-28)<1e-5 for s in stands),'equal attachment radius')
check(len(data['spawns'])==5 and len(data['stands'])==5,'five spawn and standing anchors')
for module in data['modules']:
    name=module['name'];check((OUT/(name+'.fbx')).stat().st_size>1000,'FBX '+name)
    check((SRC/'Modules'/(name+'.blend')).stat().st_size>1000,'editable library '+name)
    meshes=list(bpy.data.collections[name].objects)
    check(all(o.type=='MESH' and len(o.data.uv_layers)>0 for o in meshes),'meshes and UV0 '+name)
    check(all(math.isfinite(c) for o in meshes for v in o.data.vertices for c in v.co),'finite vertices '+name)
    check(all(o.data.materials and all(m for m in o.data.materials) for o in meshes),'named materials '+name)
check(all(im.packed_file for im in bpy.data.images if im.name.startswith('Lagoon_')),'packed portable textures')
check(bpy.data.collections['AssetKit_EDITABLE'].hide_render,'origin kit excluded from beauty render')
check(sum(1 for p in data['placements'] if p['module']=='InletBridge')==1,'single separate bridge')
(ROOT/'Builds/fishing-blender-audit.txt').write_text('\n'.join(results)+'\nFISHING_SOURCE_AUDIT_COMPLETE\n')
print('\n'.join(results));print('FISHING_SOURCE_AUDIT_COMPLETE')
