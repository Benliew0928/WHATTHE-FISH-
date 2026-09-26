"""Audit both saved stadium sources and their independently exported coastal meshes."""
import bpy, json, math
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2]
reports=[]
for sport,source in [('Football','Football/Sunvale/Stadium_Sunvale.blend'),('Basketball','Basketball/Rally/Arena_Rally.blend')]:
    bpy.ops.wm.open_mainfile(filepath=str(ROOT/'ArtSource'/source))
    report={'sport':sport,'meshes':0,'triangles':0,'collision_triangles':0,'invalid_coordinates':0,'invalid_uvs':0,'degenerate_triangles':0}
    for suffix in ('Architecture','Collision'):
        root=bpy.data.objects[sport+'_'+suffix]
        assert all(abs(v)<1e-6 for v in root.location)
        for obj in root.children:
            mesh=obj.data;mesh.calc_loop_triangles();report['meshes']+=1
            report['collision_triangles' if suffix=='Collision' else 'triangles']+=len(mesh.loop_triangles)
            assert mesh.materials and all(m.name.startswith('Coastal_') for m in mesh.materials)
            assert mesh.uv_layers.active
            report['invalid_coordinates']+=sum(not all(math.isfinite(c) for c in v.co) for v in mesh.vertices)
            report['invalid_uvs']+=sum(not all(math.isfinite(c) for c in uv.uv) for uv in mesh.uv_layers.active.data)
            for triangle in mesh.loop_triangles:
                a,b,c=(mesh.vertices[i].co for i in triangle.vertices)
                report['degenerate_triangles']+=int((b-a).cross(c-a).length<1e-9)
        export=ROOT/'Game/Assets/_Game/Art/CoastalStadiums'/(sport+'_'+suffix+'.fbx')
        assert export.exists() and export.stat().st_size>10000
    layout=json.loads((ROOT/'ArtSource/CoastalStadiums'/(sport+'-layout.json')).read_text())
    assert len(layout['routes'])==4
    assert all(len(r['stair_path'])>=75 for r in layout['routes'])
    assert report['invalid_coordinates']==report['invalid_uvs']==report['degenerate_triangles']==0,report
    reports.append(report)
(ROOT/'ArtSource/CoastalStadiums/source-audit.json').write_text(json.dumps(reports,indent=2))
print('COASTAL_STADIUM_SOURCE_AUDIT_PASS',json.dumps(reports),flush=True)
