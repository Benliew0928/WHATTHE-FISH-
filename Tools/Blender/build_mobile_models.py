"""Create bounded-error delivery meshes. Original FBXs/Blender masters are untouched.

Keep all objects/material slots/UVs. Simplify fine bevel tessellation; retain a
source mesh when reduction would move its sampled surface more than 2.5 cm.
Cabin, towers, cables, characters and authored collision models are excluded.
"""
import bpy, math, json, hashlib
from pathlib import Path
from mathutils import Vector
from mathutils.bvhtree import BVHTree

root=Path(__file__).resolve().parents[2];art=root/'Game/Assets/_Game/Art';out=art/'MobileModels';out.mkdir(exist_ok=True)
sources=['CoastalStadiums/Football_Architecture.fbx','CoastalStadiums/Basketball_Architecture.fbx',
         'Football/Sunvale/Stadium.fbx','Basketball/Rally/Stadium.fbx','Basketball/Rally/LowerSeating.fbx','Basketball/Rally/UpperSeating.fbx',
         'CoastalIslands/Football_Coast.fbx','CoastalIslands/Basketball_Coast.fbx','RefinedIslands/Golf/Structures.fbx','RefinedIslands/Fishing/Structures.fbx']
results=[]
for index,relative in enumerate(sources):
    bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
    for mesh in list(bpy.data.meshes):
        if not mesh.users:bpy.data.meshes.remove(mesh)
    bpy.ops.import_scene.fbx(filepath=str(art/relative),use_anim=False)
    rows=[]
    for ob in list(bpy.context.scene.objects):
        if ob.type!='MESH':continue
        mesh=ob.data;mesh.calc_loop_triangles();before=len(mesh.loop_triangles)
        skip=any(p in ob.name for p in ('PalmGarden_','Meadow_','Terrain_','DockPlanks','ArrivalStairs','GardenBridge','DockPiles','Sea','Cloud','Collision'))
        if skip or before<200:continue
        original=mesh.copy();matrix=ob.matrix_world.copy()
        points=[matrix@v.co for v in original.vertices]
        # Every source vertex is checked, not just a visual estimate.
        budget=.025
        ratio=.35;accepted=False;error=0
        for attempt in range(4):
            if attempt:
                retired=ob.data;ob.data=original.copy();bpy.data.meshes.remove(retired)
            bpy.context.view_layer.objects.active=ob
            modifier=ob.modifiers.new('Bounded delivery tessellation','DECIMATE');modifier.ratio=ratio
            modifier.use_collapse_triangulate=True
            bpy.ops.object.modifier_apply(modifier=modifier.name)
            ob.data.calc_loop_triangles()
            tree=BVHTree.FromPolygons([matrix@v.co for v in ob.data.vertices],[tuple(t.vertices) for t in ob.data.loop_triangles],all_triangles=True)
            error=max((tree.find_nearest(p)[3] for p in points),default=0)
            if error<=budget:accepted=True;break
            ratio=(ratio+1)/2
        if not accepted:
            retired=ob.data;ob.data=original;bpy.data.meshes.remove(retired)
        else:bpy.data.meshes.remove(original)
        ob.data.calc_loop_triangles()
        rows.append(dict(mesh=ob.name,before=before,after=len(ob.data.loop_triangles),maxSourceVertexDistance=error if accepted else 0,accepted=accepted))
    output=f'{index:02d}_{Path(relative).stem}.fbx'
    bpy.ops.object.select_all(action='DESELECT')
    changed={r['mesh'] for r in rows if r['accepted'] and r['after']<r['before']}
    for ob in bpy.context.scene.objects:
        if ob.type=='EMPTY' or ob.name in changed:ob.select_set(True)
    if changed:bpy.ops.export_scene.fbx(filepath=str(out/output),use_selection=True,object_types={'EMPTY','MESH'},axis_forward='-Z',axis_up='Y',add_leaf_bones=False,bake_anim=False,path_mode='STRIP')
    results.append(dict(source='Assets/_Game/Art/'+relative,sourceHash=hashlib.sha256((art/relative).read_bytes()).hexdigest(),delivery='Assets/_Game/Art/MobileModels/'+output,meshes=rows))
    print('MOBILE_MODEL',relative,sum(r['before'] for r in rows),'->',sum(r['after'] for r in rows),'triangles; max',max((r['maxSourceVertexDistance'] for r in rows),default=0),flush=True)
(out/'model-map.json').write_text(json.dumps(dict(models=results),indent=2))
