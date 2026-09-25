"""Export the current hand-edited Tidebloom source without regenerating geometry.

For Rally use export_rally_arena.py; Sunvale has its own modular builder.
Legacy single-mesh venue exporters are archived under Legacy/Tools/Blender.
"""
import bpy
from pathlib import Path
root=Path(__file__).resolve().parents[2]
output=root/'Game/Assets/_Game/Art'
bpy.ops.wm.open_mainfile(filepath=str(root/'ArtSource/Golf/Tidebloom/Island_Tidebloom.blend'))
bpy.ops.export_scene.fbx(filepath=str(output/'GolfIsland.fbx'),use_selection=False,object_types={'MESH'},axis_forward='-Z',axis_up='Y',add_leaf_bones=False,bake_anim=False,path_mode='STRIP',use_mesh_modifiers=True)
for image in bpy.data.images:
 if image.name.startswith('Tidebloom_') and image.size[0]>0:
  image.filepath_raw=str(output/'Golf/Tidebloom'/(Path(image.name).stem+'.png'));image.file_format='PNG';image.save()
print('EXPORTED_TIDEBLOOM_ONLY')
