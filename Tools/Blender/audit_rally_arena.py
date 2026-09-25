"""Audit the actual saved Rally source and its modular FBX package."""
import bpy, bmesh, json, math
from pathlib import Path
from mathutils import Vector

ROOT=Path(__file__).resolve().parents[2]
SOURCE=ROOT/'ArtSource/Basketball/Rally'
EXPORT=ROOT/'Game/Assets/_Game/Art/Basketball/Rally'
bpy.ops.wm.open_mainfile(filepath=str(SOURCE/'Arena_Rally.blend'))
manifest=json.loads((SOURCE/'arena-manifest.json').read_text())
report={'invalid_coordinates':0,'invalid_uvs':0,'degenerate_triangles':0,'missing_materials':0,'mesh_objects':0,
        'visible_triangles':0,'hidden_lod_triangles':0,'module_count':len(manifest['modules'])}
for obj in bpy.context.scene.objects:
    if obj.type!='MESH':continue
    mesh=obj.data;mesh.calc_loop_triangles();report['mesh_objects']+=1
    assert mesh.uv_layers.active, obj.name
    assert len(mesh.vertices) and len(mesh.polygons),obj.name
    report['invalid_coordinates']+=sum(not all(math.isfinite(c) for c in v.co) for v in mesh.vertices)
    report['invalid_uvs']+=sum(not all(math.isfinite(c) and -.00001<=c<=1.00001 for c in u.uv) for u in mesh.uv_layers.active.data)
    report['missing_materials']+=sum(m is None for m in mesh.materials)
    for t in mesh.loop_triangles:
        a,b,c=(mesh.vertices[i].co for i in t.vertices)
        report['degenerate_triangles']+=int((b-a).cross(c-a).length<1e-10)
    report['hidden_lod_triangles' if obj.hide_render else 'visible_triangles']+=len(mesh.loop_triangles)
    assert obj.parent and obj.parent.type=='EMPTY',obj.name
    assert all(abs(s-1)<1e-6 for s in obj.scale),obj.name

wood=bpy.data.objects['Court__Maple'];coords=[v.co for v in wood.data.vertices]
report['court_dimensions']=[max(p[i] for p in coords)-min(p[i] for p in coords) for i in (0,1)]
assert all(abs(a-b)<1e-5 for a,b in zip(report['court_dimensions'],[15.24,28.6512]))
report['upward_court_faces']=all(p.normal.z>.99 for p in wood.data.polygons)
assert report['upward_court_faces']
foundation=bpy.data.objects['Court__Foundation'];apron=bpy.data.objects['Court__Apron']
foundation_top=max((foundation.matrix_world@v.co).z for v in foundation.data.vertices)
apron_top=max((apron.matrix_world@v.co).z for v in apron.data.vertices)
apron_bottom=min((apron.matrix_world@v.co).z for v in apron.data.vertices)
report['apron_foundation_clearance_metres']=apron_top-foundation_top
assert foundation_top<=apron_bottom+1e-5,'Court foundation overlaps the apron surface'
assert report['apron_foundation_clearance_metres']>=.10,'Insufficient floor separation'
for obj in bpy.data.objects:
    if obj.type=='MESH' and (obj.name=='Court__Markings' or obj.name.startswith('CenterEmblem__')):
        assert all(p.normal.z>.99 for p in obj.data.polygons),obj.name
rims=[o for o in bpy.context.scene.objects if o.type=='MESH' and o.name.split('.')[0]=='Hoop__Rim']
assert len(rims)==2
report['rim_centers']=[]
for obj in rims:
    coords=[obj.matrix_world@v.co for v in obj.data.vertices]
    center=[(max(p[i] for p in coords)+min(p[i] for p in coords))/2 for i in range(3)]
    report['rim_centers'].append(center)
    assert abs(center[2]-3.048)<1e-5 and abs(abs(center[1])-12.7254)<1e-5
    assert abs(center[0])<1e-5
report['lod0_bays']=sum(o.name.endswith('Seats_LOD0') for o in bpy.context.scene.objects)
report['lod1_bays']=sum(o.name.endswith('Seats_LOD1') for o in bpy.context.scene.objects)
assert report['lod0_bays']==report['lod1_bays']==48
assert all(o.hide_render for o in bpy.context.scene.objects if o.name.endswith('LOD1'))
report['packed_maps']=[im.name for im in bpy.data.images if im.packed_file]
assert len(report['packed_maps'])==5,report['packed_maps']
report['material_count']=len(manifest['materials'])
report['fbx_exports']=[]
for name in manifest['modules']:
    path=EXPORT/(name+'.fbx');assert path.exists() and path.stat().st_size>1000
    report['fbx_exports'].append({'module':name,'bytes':path.stat().st_size})
    assert name in bpy.data.collections
# Materials must not couple unrelated modules. Two hoop instances intentionally share them.
owners={}
for obj in bpy.context.scene.objects:
    if obj.type!='MESH':continue
    module=obj.name.split('__')[0]
    for m in obj.data.materials:owners.setdefault(m.name,set()).add(module)
assert all(len(o)==1 for o in owners.values()),owners
report['independent_module_materials']=True
report['inward_banner_prints']=True
for obj in bpy.context.scene.objects:
    if obj.type=='MESH' and obj.name.startswith('Banners__') and obj.name.endswith('Graphics'):
        for p in obj.data.polygons:
            inward=Vector((-p.center.x,-p.center.y,0))
            report['inward_banner_prints'] &= p.normal.dot(inward)>0
assert report['inward_banner_prints'],'Banner graphics face away from the court'
assert report['module_count']==13
assert report['invalid_coordinates']==report['invalid_uvs']==report['degenerate_triangles']==report['missing_materials']==0,report
(SOURCE/'blender-source-audit.json').write_text(json.dumps(report,indent=2)+'\n')
print('RALLY_SOURCE_AUDIT_PASS',json.dumps(report),flush=True)
