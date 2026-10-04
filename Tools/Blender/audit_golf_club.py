"""Check retained master, active FBX dependencies and handheld club geometry."""
import argparse, hashlib, json, math, re, zipfile
from pathlib import Path
import bpy
from io_scene_fbx import parse_fbx
from build_golf_equipment import audit

parser=argparse.ArgumentParser();parser.add_argument('--root',type=Path)
root=(parser.parse_args().root or Path(__file__).resolve().parents[2]).resolve()
source=root/'ArtSource/Golf/Club';out=root/'Builds/GolfClubQA/PortableAudit';out.mkdir(parents=True,exist_ok=True)
delivery=json.loads((source/'delivery-audit.json').read_text(encoding='utf8'))
for item in delivery['files']:
    p=root/item['path'];assert p.is_file() and p.stat().st_size==item['bytes']
    assert hashlib.sha256(p.read_bytes()).hexdigest()==item['sha256'],item['path']
bpy.ops.wm.open_mainfile(filepath=str(source/'MidnightIron.blend'))
assert not bpy.data.libraries
images=[]
for im in bpy.data.images:
    if im.packed_file:continue
    assert im.filepath.startswith('//'),im.name
    p=Path(bpy.path.abspath(im.filepath)).resolve();assert p.is_file() and p.is_relative_to(root),im.name
    images.append(p.relative_to(root).as_posix())
assert len(images)==7
master=bpy.data.objects['MidnightIron_Master'];assert audit(master)['triangles']==7702
lods=[]
for level,target in enumerate((3600,1400,500)):
    obj=bpy.data.objects['MidnightIron_LOD'+str(level)];data=audit(obj)
    assert data['triangles']==target and data['finite_uvs'] and data['degenerate_triangles']==0 and data['nonmanifold_edges']==0
    assert math.isclose(data['dimensions_metres'][2],1.02,abs_tol=.002)
    lods.append(data)
assert bpy.data.objects['ClubGrip'].location.length<1e-6
assert math.isclose(bpy.data.objects['ClubTop'].location.z,.0918,abs_tol=1e-6)
def objects(tree):return next(n for n in tree.elems if n.id==b'Objects').elems
fbx=root/'Game/Assets/_Game/Art/Golf/Club/MidnightIron.fbx';tree,_=parse_fbx.parse(str(fbx));refs=set();historical=[]
def inspect(node,ancestry=()):
    path=ancestry+(node.id.decode('utf8'),)
    for v in node.props:
        if not isinstance(v,bytes) or node.id==b'Content':continue
        if not re.search(rb'(?<![a-zA-Z0-9:])[A-Za-z]:[/\\]|^/(Users|home|root|mnt)/',v):continue
        assert path[0]=='FBXHeaderExtension' and 'SceneInfo' in path,'Active absolute FBX path: '+ '/'.join(path)
        historical.append('/'.join(path))
    for c in node.elems:inspect(c,path)
for node in tree.elems:inspect(node)
for node in objects(tree):
    if node.id not in (b'Texture',b'Video'):continue
    for c in node.elems:
        if c.id not in (b'FileName',b'Filename',b'RelativeFilename'):continue
        for value in c.props:
            name=value.decode('utf8');assert name==Path(name.replace('\\','/')).name
            assert (fbx.parent/name).is_file();refs.add(name)
assert len(refs)==3
provenance=json.loads((source/'provenance.json').read_text(encoding='utf8'));archive=source/provenance['archive']
assert hashlib.sha256(archive.read_bytes()).hexdigest()==provenance['sha256']
with zipfile.ZipFile(archive) as z:
    original=out/'original.fbx';original.write_bytes(z.read(next(n for n in z.namelist() if n.endswith('.fbx'))))
tree,_=parse_fbx.parse(str(original));videos=[n for n in objects(tree) if n.id==b'Video']
assert len(videos)==4 and all(any(c.id==b'Content' and any(len(v)>0 for v in c.props) for c in n.elems) for n in videos)
report=dict(status='PASS',master_triangles=7702,lods=lods,master_images=images,external_libraries=0,texture_refs=sorted(refs),original_embedded_images=4,inactive_exporter_provenance_fields=sorted(set(historical)))
(out/'audit.json').write_text(json.dumps(report,indent=2)+'\n',encoding='utf8');print('GOLF_CLUB_ART_AUDIT_PASS')
