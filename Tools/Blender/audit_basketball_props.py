"""Format-aware dependency and geometry audit; optional portable-root comparison."""
import argparse,hashlib,json,re,sys
from pathlib import Path
import bpy
from io_scene_fbx import parse_fbx
p=argparse.ArgumentParser();p.add_argument('--root',type=Path);p.add_argument('--compare-root',type=Path)
a=p.parse_args(sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else [])
root=(a.root or Path(__file__).resolve().parents[2]).resolve()
source=root/'ArtSource/Basketball/Props';out=root/'Builds/BasketballPropsQA';out.mkdir(parents=True,exist_ok=True)
record=json.loads((source/'integrity.json').read_text(encoding='utf-8'))
for relative,info in record['files'].items():
 path=root/relative;assert path.stat().st_size==info['bytes'];assert hashlib.sha256(path.read_bytes()).hexdigest()==info['sha256'],relative
bpy.ops.wm.open_mainfile(filepath=str(source/'BasketballProps.blend'))
assert not list(bpy.data.libraries)
assert all(not i.filepath or i.filepath.startswith('//') for i in bpy.data.images if i.name not in ('Render Result','Viewer Node'))
assert bpy.context.scene.render.filepath.startswith('//')
historical=[]
def inspect(node,trail=()):
 trail=trail+(node.id.decode(),);assert node.id not in (b'Texture',b'Video'),trail
 for v in node.props:
  if isinstance(v,bytes) and re.search(rb'(?<![a-zA-Z0-9:])[A-Za-z]:[/\\]|^/(Users|home|root|mnt)/',v):
   assert trail[0]=='FBXHeaderExtension' and 'SceneInfo' in trail,trail
   historical.append('/'.join(trail))
 for child in node.elems:inspect(child,trail)
def geometry(path):
 tree,_=parse_fbx.parse(str(path));result=[]
 for node in tree.elems:inspect(node)
 objects=next(n for n in tree.elems if n.id==b'Objects')
 for node in objects.elems:
  if node.id==b'Geometry':
   raw=next(c.props[0] for c in node.elems if c.id==b'Vertices')
   vertices=[tuple(round(x,6) for x in raw[i:i+3]) for i in range(0,len(raw),3)]
   indices=next(c.props[0] for c in node.elems if c.id==b'PolygonVertexIndex')
   faces=[];face=[]
   for index in indices:
    face.append(vertices[-index-1 if index<0 else index])
    if index<0:faces.append(tuple(sorted(face)));face=[]
   # Blender join/export may reorder vertices. Compare actual polygon surfaces,
   # not incidental array indices or generated mesh datablock names.
   digest=hashlib.sha256(repr(sorted(faces)).encode()).hexdigest()
   result.append({'vertices':len(vertices),'polygons':len(faces),'surfaceSha256':digest})
 return sorted(result,key=lambda v:v['surfaceSha256'])
meshes={}
for name in ('LockOn','Turbo','Portal'):
 relative=Path('Game/Assets/_Game/Resources/BasketballProps')/(name+'.fbx');meshes[name]=geometry(root/relative)
 if a.compare_root:assert meshes[name]==geometry(a.compare_root/relative),name
(out/'asset-audit.json').write_text(json.dumps({'success':True,'activeDependencies':[],'renderOutput':'//renders/','geometry':meshes,'permittedExporterProvenance':sorted(set(historical)),'portableGeometryCompared':bool(a.compare_root)},indent=2)+'\n',encoding='utf-8')
print('BASKETBALL_PROPS_ASSET_AUDIT_OK')
