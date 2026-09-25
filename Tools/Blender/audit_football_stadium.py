"""Validate the saved Sunvale source, including game-facing roof and lettering normals."""
import bpy, bmesh, json, math
from pathlib import Path

root=Path(__file__).resolve().parents[2]
source=root/'ArtSource/Football/Sunvale'
bpy.ops.wm.open_mainfile(filepath=str(source/'Stadium_Sunvale.blend'))
report={'roof_shells':0,'mesh_objects':0,'invalid_coordinates':0,'invalid_uvs':0,'roof_boundary_edges':0}
for obj in bpy.data.objects:
    if obj.type!='MESH' or obj.name=='Review ground':continue
    mesh=obj.data;report['mesh_objects']+=1
    assert len(mesh.vertices)>0 and len(mesh.polygons)>0, obj.name
    report['invalid_coordinates']+=sum(not all(math.isfinite(x) for x in v.co) for v in mesh.vertices)
    assert len(mesh.materials)==1 and mesh.materials[0].name=='Sunvale_Painted', obj.name
    assert mesh.uv_layers.active, obj.name
    report['invalid_uvs']+=sum(not all(math.isfinite(x) and 0<=x<=1 for x in uv.uv) for uv in mesh.uv_layers.active.data)
    if obj.name.endswith('_Canopy'):
        bm=bmesh.new();bm.from_mesh(mesh)
        report['roof_boundary_edges']+=sum(not e.is_manifold for e in bm.edges)
        bm.free();report['roof_shells']+=1
letters=bpy.data.objects['Scoreboard__Lettering'].data
report['letter_front_facing']=all(p.normal.y<-.99 for p in letters.polygons)
report['packed_texture']=bool(bpy.data.images['Sunvale_PaintedPalette'].packed_file)
base=bpy.data.objects['Lawn__TurfBase'];pitch=bpy.data.objects['Lawn__Pitch']
report['pitch_base_separation_metres']=max(v.co.z for v in pitch.data.vertices)-max(v.co.z for v in base.data.vertices)
report['lod0_bays']=sum(o.name.endswith('_Seats_LOD0') for o in bpy.data.objects)
report['lod1_bays']=sum(o.name.endswith('_Seats_LOD1') for o in bpy.data.objects)
assert report['roof_shells']==32
assert report['invalid_coordinates']==report['invalid_uvs']==report['roof_boundary_edges']==0,report
assert report['letter_front_facing'] and report['packed_texture'],report
assert report['lod0_bays']==report['lod1_bays']==32,report
assert report['pitch_base_separation_metres']>.015,report
(source/'blender-source-audit.json').write_text(json.dumps(report,indent=2)+'\n')
print('SUNVALE_SOURCE_AUDIT_PASS',json.dumps(report))
