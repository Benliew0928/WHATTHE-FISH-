"""Export hand-edited sources without regenerating or overwriting the .blend files."""
import bpy
from pathlib import Path
root=Path(__file__).resolve().parents[2]
output=root/'Game/Assets/_Game/Art'
output.mkdir(parents=True,exist_ok=True)
for name,source in [('Stadium','Football/Stadium.blend'),('Athlete','Shared/Characters/Athlete.blend')]:
 bpy.ops.wm.open_mainfile(filepath=str(root/'ArtSource'/source))
 bpy.ops.export_scene.fbx(filepath=str(output/(name+'.fbx')),use_selection=False,object_types={'MESH','ARMATURE'},axis_forward='-Z',axis_up='Y',add_leaf_bones=False,bake_anim=name=='Athlete',bake_anim_use_all_actions=True,bake_anim_use_nla_strips=False,path_mode='AUTO',use_mesh_modifiers=True)
 for image in bpy.data.images:
  if image.type=='IMAGE' and image.size[0]>0 and image.source!='VIEWER':
   textures=output/'Textures';textures.mkdir(exist_ok=True)
   image.filepath_raw=str(textures/(Path(image.name).stem+'.png'));image.file_format='PNG';image.save()
 print('EXPORTED',name)
