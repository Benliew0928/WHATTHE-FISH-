import bpy,json
from pathlib import Path
root=Path('C:/UMPSA')
bpy.ops.wm.open_mainfile(filepath=str(root/'ArtSource/Football/Stadium.blend'))
report=[]
for o in bpy.context.scene.objects:
 if o.type=='MESH':
  report.append({'name':o.name,'loc':list(o.location),'dimensions':list(o.dimensions),'verts':len(o.data.vertices),'polys':len(o.data.polygons),'materials':[(m.name,list(m.diffuse_color)) for m in o.data.materials],'topNormals':[list(p.normal) for p in o.data.polygons[:6]]})
(root/'Builds/asset-audit.json').write_text(json.dumps(report,indent=2))
print('AUDIT_COMPLETE')
