"""Validate saved coastal geometry, UVs, texture maps and venue preservation."""
import bpy, math, json, hashlib
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2];SRC=ROOT/'ArtSource/CoastalIslands';OUT=ROOT/'Game/Assets/_Game/Art/CoastalIslands'
bpy.ops.wm.open_mainfile(filepath=str(SRC/'PalmShore_Islands.blend'))
report={};errors=[]
for sport in ['Football','Basketball']:
    objects=list(bpy.data.collections[sport+'_Exterior'].objects)
    finite=all(all(math.isfinite(v) for v in p.co) for ob in objects for p in ob.data.vertices)
    uv=all(len(ob.data.uv_layers)>0 and all(math.isfinite(c) for item in ob.data.uv_layers.active.data for c in item.uv) for ob in objects)
    terrain=[ob for ob in objects if '__Terrain_' in ob.name]
    upward=all(p.normal.z>.1 for ob in terrain for p in ob.data.polygons if p.area>.00001)
    report[sport]=dict(meshes=len(objects),triangles=sum(sum(len(p.vertices)-2 for p in ob.data.polygons) for ob in objects),finite_vertices=finite,valid_uvs=uv,terrain_normals_up=upward,terrain_sectors=len(terrain),export_present=(OUT/(sport+'_Coast.fbx')).exists())
    if not finite or not uv or not upward or len(terrain)!=16:errors.append(sport+' geometry failed')
report['packed_textures']=sum(im.packed_file is not None for im in bpy.data.images)
report['venue_sources_unchanged']=True
for entry in json.loads((SRC/'venue-baseline.json').read_text(encoding='utf-8-sig')):
    if hashlib.sha256((ROOT/entry['Path']).read_bytes()).hexdigest().upper()!=entry['Hash']:
        report['venue_sources_unchanged']=False;errors.append('Changed venue '+entry['Path'])
report['errors']=errors;(SRC/'blender-audit.json').write_text(json.dumps(report,indent=2))
print(json.dumps(report,indent=2),flush=True)
if errors:raise RuntimeError('; '.join(errors))
