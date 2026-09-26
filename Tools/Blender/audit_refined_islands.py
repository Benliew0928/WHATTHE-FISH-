"""Audit the exported source kit and selected manifests before accepting a build."""
import bpy,bmesh,json,math
from pathlib import Path
root=Path(__file__).resolve().parents[2];art=root/'Game/Assets/_Game/Art/RefinedIslands';errors=[];report={}
bpy.ops.wm.open_mainfile(filepath=str(root/'ArtSource/RefinedIslands/IslandAssetKit.blend'))
for module in json.loads((art/'Shared/kit.json').read_text())['modules']:
    col=bpy.data.collections[module['name']];stats=[]
    for o in col.objects:
        if o.type!='MESH':continue
        mesh=o.data;mesh.calc_loop_triangles()
        if any(not math.isfinite(v) for vert in mesh.vertices for v in vert.co):errors.append(o.name+': nonfinite vertex')
        if not mesh.uv_layers:errors.append(o.name+': no UVs')
        if any(not math.isfinite(v) for loop in mesh.uv_layers.active.data for v in loop.uv):errors.append(o.name+': nonfinite UV')
        if any(not slot.material for slot in o.material_slots):errors.append(o.name+': missing material')
        bm=bmesh.new();bm.from_mesh(mesh);boundary=sum(e.is_boundary for e in bm.edges)
        if module['name'] in ('PlayerStand','InletBridge') and o.name.startswith('LOD0') and boundary:errors.append(o.name+': furniture has '+str(boundary)+' open edges')
        stats.append(dict(mesh=o.name,triangles=len(mesh.loop_triangles),boundary_edges=boundary));bm.free()
    report[module['name']]=stats
for sport,capacity in [('Golf',10),('Fishing',5)]:
    data=json.loads((art/sport/'layout.json').read_text());assert len(data['spawns'])==capacity
    assert len(data['views'])>=12 and len(data['routes'])>=8
    if sport=='Fishing':assert len([p for p in data['instances'] if p['module']=='PlayerStand'])==5
    else:assert len(data['bunkers'])==5
    for file in ['Terrain.fbx','Structures.fbx','Grass.fbx',sport+'_TerrainColor.png',sport+'_TerrainControl.png',sport+'_WaterDepth.png']:
        if not (art/sport/file).is_file():errors.append(sport+': missing '+file)
report['errors']=errors;report['success']=not errors
out=root/'Builds/IslandRefinementAudit/source-audit.json';out.write_text(json.dumps(report,indent=2))
print('REFINED_SOURCE_AUDIT',report['success'],'errors',len(errors),flush=True)
if errors:raise RuntimeError('\n'.join(errors))
