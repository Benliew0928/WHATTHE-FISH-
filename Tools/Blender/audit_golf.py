"""Validate the current saved golf source without regenerating it. Run in Blender."""
import bpy,json,math
from pathlib import Path
root=Path(__file__).resolve().parents[2]
bpy.ops.wm.open_mainfile(filepath=str(root/'ArtSource/Golf/Tidebloom/Island_Tidebloom.blend'))
objects=[o for o in bpy.context.scene.objects if o.type=='MESH'];errors=[]
for o in objects:
    m=o.data
    if not m.uv_layers:errors.append(o.name+': no UVs')
    if any(not all(math.isfinite(c) for c in v.co) for v in m.vertices):errors.append(o.name+': nonfinite geometry')
    if any(not slot.material for slot in o.material_slots):errors.append(o.name+': empty material')
    if o.name.startswith('Terrain__') and any(p.normal.z<.5 or p.area<1e-9 for p in m.polygons):errors.append(o.name+': invalid floor face')
    if len(m.vertices)>65535:errors.append(o.name+': exceeds 16-bit vertex budget')
textures=[im for im in bpy.data.images if im.name.startswith('Tidebloom_')]
for im in textures:
    if not im.packed_file:errors.append(im.name+': texture not packed')
    if not (root/'Game/Assets/_Game/Art/Golf/Tidebloom'/(im.name+'.png')).exists():errors.append(im.name+': PNG missing')
manifest=json.loads((root/'ArtSource/Golf/Tidebloom/island-manifest.json').read_text())
assert len(manifest['spawns'])==10
report=dict(meshes=len(objects),triangles=sum(sum(len(p.vertices)-2 for p in o.data.polygons) for o in objects),textures=len(textures),terrain_sectors=sum(o.name.startswith('Terrain__') for o in objects),errors=errors)
(root/'Builds/golf-source-validation.json').write_text(json.dumps(report,indent=2)+'\n')
print('GOLF_SOURCE_AUDIT',report)
if errors:raise RuntimeError('; '.join(errors))
