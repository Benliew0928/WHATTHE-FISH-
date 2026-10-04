"""Verify source integrity, spherical fit and portable rounded-ball dependencies."""
import argparse,hashlib,json,re
from pathlib import Path
import bpy,numpy as np
from io_scene_fbx import parse_fbx
from build_golf_equipment import audit

parser=argparse.ArgumentParser();parser.add_argument('--root',type=Path);parser.add_argument('--compare-root',type=Path);args=parser.parse_args()
root=(args.root or Path(__file__).resolve().parents[2]).resolve();source=root/'ArtSource/Golf/Ball';output=root/'Builds/GolfBallRoundedQA/PortableAudit';output.mkdir(parents=True,exist_ok=True)
record=json.loads((source/'delivery-audit.json').read_text(encoding='utf8'))
for item in record['files']:
    path=root/item['path'];assert path.is_file() and path.stat().st_size==item['bytes'];assert hashlib.sha256(path.read_bytes()).hexdigest()==item['sha256'],item['path']
bpy.ops.wm.open_mainfile(filepath=str(source/'RoundedBall.blend'));assert not bpy.data.images and not bpy.data.libraries
master=bpy.data.objects['RoundedBall_Master'];assert audit(master)['triangles']==87048
for level,count in ((0,15998),(1,720)):
    obj=bpy.data.objects['Ball_LOD'+str(level)];data=audit(obj);assert data['triangles']==count and data['nonmanifold_edges']==0 and len(data['components'])==1
    assert max(v.co.length for v in obj.data.vertices)<=.021501
    points=np.array([tuple(v.co) for v in obj.data.vertices]);assert np.max(np.abs((points.min(0)+points.max(0))/2))<.0001
fbx=root/'Game/Assets/_Game/Art/Golf/Equipment/Ball.fbx'
tree,_=parse_fbx.parse(str(fbx));historical=[]
def dependencies(node,path=()):
    path=path+(node.id.decode(),);assert node.id not in (b'Texture',b'Video')
    for value in node.props:
        if isinstance(value,bytes) and re.search(rb'(?<![a-zA-Z0-9:])[A-Za-z]:[/\\]|^/(Users|home|root|mnt)/',value):
            assert path[0]=='FBXHeaderExtension' and 'SceneInfo' in path,path;historical.append('/'.join(path))
    for child in node.elems:dependencies(child,path)
for node in tree.elems:dependencies(node)
def surfaces(path):
    tree,_=parse_fbx.parse(str(path));objects=next(n for n in tree.elems if n.id==b'Objects').elems;result={}
    for node in objects:
        if node.id!=b'Geometry':continue
        name=node.props[1].split(b'\x00')[0].decode()
        def child(parent,key):return next(n for n in parent.elems if n.id==key)
        vertices=np.array(child(node,b'Vertices').props[0]).reshape(-1,3)
        indices=child(node,b'PolygonVertexIndex').props[0]
        normals=child(node,b'LayerElementNormal');uvs=child(node,b'LayerElementUV')
        assert child(normals,b'MappingInformationType').props[0]==b'ByPolygonVertex'
        normal_values=np.array(child(normals,b'Normals').props[0]).reshape(-1,3)
        normal_indices=next((n.props[0] for n in normals.elems if n.id==b'NormalsIndex'),range(len(indices)))
        uv_values=np.array(child(uvs,b'UV').props[0]).reshape(-1,2);uv_indices=child(uvs,b'UVIndex').props[0]
        faces=[];face=[];corner_normals=[]
        for corner,index in enumerate(indices):
            point=vertices[index if index>=0 else -index-1]
            uv=uv_values[uv_indices[corner]];normal=normal_values[normal_indices[corner]]
            face.append(tuple(np.rint(point*1e8).astype(np.int64))+tuple(np.rint(uv*1e6).astype(np.int64)))
            corner_normals.append(tuple(point)+tuple(uv)+tuple(normal))
            if index<0:
                # Preserve winding while ignoring face/vertex/normal index order.
                faces.append(min(tuple(face[i:]+face[:i]) for i in range(len(face))));face=[]
        assert not face
        result[name]=(hashlib.sha256(repr(sorted(faces)).encode()).hexdigest(),np.array(sorted(corner_normals)))
    return result
surfaces_here=surfaces(fbx);hashes={name:value[0] for name,value in surfaces_here.items()};normal_delta=0
if args.compare_root:
    alternate=surfaces(args.compare_root.resolve()/'Game/Assets/_Game/Art/Golf/Equipment/Ball.fbx');assert hashes=={name:value[0] for name,value in alternate.items()}
    for name,(_,corners) in surfaces_here.items():
        other=alternate[name][1];assert np.array_equal(corners[:,:5],other[:,:5])
        # Blender's packed custom sphere normals vary slightly with loop order.
        # Winding/UV/positions are strict; custom-normal rounding has a small bound.
        normal_delta=max(normal_delta,float(np.max(np.abs(corners[:,5:]-other[:,5:]))))
    assert normal_delta<.0002
bpy.ops.wm.read_factory_settings(use_empty=True);bpy.ops.import_scene.fbx(filepath=str(fbx))
meshes=[o for o in bpy.context.scene.objects if o.type=='MESH'];assert len(meshes)==2
for obj in meshes:
    data=audit(obj);assert data['nonmanifold_edges']==0 and len(data['components'])==1
    assert max((obj.matrix_world@v.co).length for v in obj.data.vertices)<=.021501
summary=dict(status='PASS',master_triangles=87048,lod_triangles=[15998,720],textures=0,external_libraries=0,geometry_hashes=hashes,alternate_checkout_equal=bool(args.compare_root),maximum_custom_normal_delta=normal_delta,inactive_exporter_provenance_fields=sorted(set(historical)))
(output/'audit.json').write_text(json.dumps(summary,indent=2)+'\n',encoding='utf8');print('GOLF_ROUNDED_BALL_ART_AUDIT_PASS '+json.dumps(summary))
