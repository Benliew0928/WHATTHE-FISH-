"""Export edited Rally source without rebuilding geometry or changing the .blend.

Run: blender --background --python Tools/Blender/export_rally_arena.py
Keep collection, root, mesh, and material role names for Unity import compatibility.
"""
import bpy, json
from pathlib import Path

ROOT=Path(__file__).resolve().parents[2]
SOURCE=ROOT/'ArtSource/Basketball/Rally'
EXPORT=ROOT/'Game/Assets/_Game/Art/Basketball/Rally'
bpy.ops.wm.open_mainfile(filepath=str(SOURCE/'Arena_Rally.blend'))
manifest=json.loads((SOURCE/'arena-manifest.json').read_text())
def srgb(c):return c*12.92 if c<=.0031308 else 1.055*c**(1/2.4)-.055
for name in manifest['modules']:
    root=bpy.data.objects['Hoop_North' if name=='Hoop' else name]
    # The reusable hoop exports at rim centre / floor level, not its assembled placement.
    root.location=(0,0,0);root.rotation_euler=(0,0,0);root.scale=(1,1,1)
    bpy.ops.object.select_all(action='DESELECT');root.hide_set(False);root.select_set(True)
    children=[o for o in root.children_recursive if o.type=='MESH']
    for o in children:o.hide_set(False);o.select_set(True);o.data.calc_loop_triangles()
    manifest['modules'][name].update(meshes=len(children),
        triangles_lod0=sum(len(o.data.loop_triangles) for o in children if not o.name.endswith('LOD1')),
        triangles_lod1=sum(len(o.data.loop_triangles) for o in children if not o.name.endswith('LOD0')))
    bpy.ops.export_scene.fbx(filepath=str(EXPORT/(name+'.fbx')),use_selection=True,object_types={'EMPTY','MESH'},axis_forward='-Z',axis_up='Y',add_leaf_bones=False,bake_anim=False,path_mode='RELATIVE',use_mesh_modifiers=True)
for name,spec in manifest['materials'].items():
    # A reserved role with no assigned faces may be discarded by Blender on save.
    # Keep its catalog default; it cannot affect any exported renderer.
    m=bpy.data.materials.get(name)
    if m is None:continue
    bs=next(n for n in m.node_tree.nodes if n.type=='BSDF_PRINCIPLED')
    multiply=next((n for n in m.node_tree.nodes if n.type=='MIX_RGB' and n.blend_type=='MULTIPLY'),None)
    color=multiply.inputs[2].default_value if multiply else bs.inputs['Base Color'].default_value
    spec['color_srgb']=[srgb(c) for c in color[:3]]
    spec['roughness']=bs.inputs['Roughness'].default_value
    spec['metallic']=bs.inputs['Metallic'].default_value
    spec['emission']=bs.inputs['Emission Strength'].default_value
for im in bpy.data.images:
    if im.name.startswith('Rally_') and im.size[0]:
        im.filepath_raw=str(EXPORT/(im.name+'.png'));im.file_format='PNG';im.save()
for folder in (SOURCE,EXPORT):(folder/'arena-manifest.json').write_text(json.dumps(manifest,indent=2)+'\n')
(EXPORT/'unity-import.json').write_text(json.dumps({'materials':[dict(name=n,**s) for n,s in manifest['materials'].items()],'modules':list(manifest['modules'])},indent=2)+'\n')
print('RALLY_EDITED_SOURCE_EXPORTED',flush=True)
