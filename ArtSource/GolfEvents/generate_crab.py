"""Editable, texture-free cartoon crab. Run with Blender --background --python.

Paths derive from this file; no installation path or external texture is saved.
Codex-assisted procedural Blender authoring, not a Lux3D/cloud generation.
"""
from pathlib import Path
import bpy, math, json, hashlib
from mathutils import Vector

HERE = Path(__file__).resolve().parent
ROOT = HERE.parents[1]
OUT = ROOT / "Game/Assets/_Game/Art/Golf/Events"
OUT.mkdir(parents=True, exist_ok=True)
bpy.ops.object.select_all(action="SELECT")
bpy.ops.object.delete(use_global=False)
scene = bpy.context.scene
scene.unit_settings.system = "METRIC"
scene.unit_settings.scale_length = 1

def material(name, color, rough=.6):
    m = bpy.data.materials.new(name)
    m.diffuse_color = (*color, 1)
    m.use_nodes = True
    p = m.node_tree.nodes.get("Principled BSDF")
    p.inputs["Base Color"].default_value = (*color, 1)
    p.inputs["Roughness"].default_value = rough
    return m

shell = material("Crab coral", (.94, .20, .055))
limb = material("Crab warm legs", (.73, .095, .025))
cream = material("Crab eye white", (1, .93, .73))
ink = material("Crab pupils", (.018, .045, .065), .35)
bones = {"Body": ((0, 0, .12), (0, 0, .5))}
for side, sign in (("L", -1), ("R", 1)):
    for i in range(4):
        y = -.28 + i * .17
        bones[f"Leg_{side}_{i}"] = ((sign*.25, y, .23), (sign*.72, y-.12, .05))
    bones[f"Claw_{side}"] = ((sign*.24, .27, .3), (sign*.53, .65, .42))

rig_data = bpy.data.armatures.new("Crab articulated skeleton")
rig = bpy.data.objects.new("CrabRig", rig_data)
scene.collection.objects.link(rig)
bpy.context.view_layer.objects.active = rig
rig.select_set(True)
bpy.ops.object.mode_set(mode="EDIT")
for name, (head, tail) in bones.items():
    bone = rig_data.edit_bones.new(name)
    bone.head, bone.tail = head, tail
    if name != "Body": bone.parent = rig_data.edit_bones["Body"]
bpy.ops.object.mode_set(mode="OBJECT")
rig.select_set(False)

meshes = []
def build_lod(lod, segments, rings, sides):
    parts = []
    def finish(obj, name, mat, bone):
        obj.name = name
        obj.data.materials.append(mat)
        bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
        obj.vertex_groups.new(name=bone).add(range(len(obj.data.vertices)), 1, "REPLACE")
        parts.append(obj)
        return obj
    def oval(name, at, scale, mat=shell, bone="Body"):
        bpy.ops.mesh.primitive_uv_sphere_add(segments=segments, ring_count=rings, location=at)
        obj = bpy.context.object
        obj.scale = scale
        return finish(obj, name, mat, bone)
    def rod(name, a, b, radius, mat=limb, bone="Body"):
        a, b = Vector(a), Vector(b)
        bpy.ops.mesh.primitive_cone_add(vertices=sides, radius1=radius, radius2=radius*.72, depth=(b-a).length, location=(a+b)*.5)
        obj = bpy.context.object
        obj.rotation_euler = (b-a).to_track_quat("Z", "Y").to_euler()
        return finish(obj, name, mat, bone)
    oval("Rounded crab shell", (0, 0, .29), (.41, .34, .21))
    oval("Soft belly", (0, .02, .18), (.31, .25, .095), cream)
    for side, sign in (("L", -1), ("R", 1)):
        for i in range(4):
            name=f"Leg_{side}_{i}"
            a=Vector(bones[name][0]); knee=Vector((sign*(.61+(.03 if i%2 else 0)), a.y-.08, .2)); tip=Vector((sign*.77,a.y-.2,.055))
            rod(name+" upper", a, knee, .052, bone=name)
            rod(name+" lower", knee, tip, .041, bone=name)
        name=f"Claw_{side}"
        rod(name+" arm", bones[name][0], (sign*.49,.53,.34), .075, bone=name)
        oval(name+" palm", (sign*.53,.61,.38), (.145,.17,.11), shell, name)
        # Two visibly separate tapered fingers leave the pincer opening clear.
        rod(name+" outer finger", (sign*.62,.65,.4), (sign*.63,.87,.43), .075, shell, name)
        rod(name+" inner finger", (sign*.48,.67,.4), (sign*.51,.83,.44), .06, shell, name)
        rod("Eye stalk "+side, (sign*.17,.21,.42), (sign*.18,.24,.6), .033, shell)
        oval("Eye white "+side, (sign*.18,.25,.62), (.09,.082,.105), cream)
        oval("Eye pupil "+side, (sign*.18,.315,.63), (.042,.021,.055), ink)
        oval("Eye glint "+side, (sign*.16,.331,.652), (.015,.008,.018), cream)
    rod("Friendly curved smile left", (-.10,.324,.305), (0,.35,.28), .012, ink)
    rod("Friendly curved smile right", (0,.35,.28), (.10,.324,.305), .012, ink)
    bpy.ops.object.select_all(action="DESELECT")
    for part in parts: part.select_set(True)
    bpy.context.view_layer.objects.active=parts[0]
    bpy.ops.object.join()
    mesh=bpy.context.object
    mesh.name=f"Crab_LOD{lod}"
    bpy.context.scene.cursor.location=(0,0,0)
    bpy.ops.object.origin_set(type="ORIGIN_CURSOR")
    tri=mesh.modifiers.new("Portable triangulation", "TRIANGULATE")
    bpy.ops.object.modifier_apply(modifier=tri.name)
    for polygon in mesh.data.polygons: polygon.use_smooth=True
    mod=mesh.modifiers.new("Crab skeleton", "ARMATURE");mod.object=rig
    mesh.parent=rig
    meshes.append(mesh)
    return len(mesh.data.polygons)

counts=[build_lod(0,12,7,8),build_lod(1,8,5,5)]
assert counts[0]<4500 and counts[1]<2200, counts
meshes[1].hide_set(True)
assert not [i for i in bpy.data.images if i.source not in {"RENDER_RESULT", "VIEWER"}] and not bpy.data.libraries
master=HERE/"CrossingCrab.blend"
bpy.ops.wm.save_as_mainfile(filepath=str(master), relative_remap=True)
for m in meshes: m.hide_set(False)
bpy.ops.object.select_all(action="DESELECT")
rig.select_set(True)
for mesh in meshes: mesh.select_set(True)
bpy.context.view_layer.objects.active=rig
bpy.ops.export_scene.fbx(filepath=str(OUT/"CrossingCrab.fbx"),use_selection=True,object_types={"MESH","ARMATURE"},add_leaf_bones=False,bake_anim=False,use_mesh_modifiers=True,axis_forward="-Z",axis_up="Y",apply_scale_options="FBX_SCALE_UNITS",path_mode="RELATIVE",embed_textures=False)
record={"authoring":"Codex-assisted local Blender generator; no Lux3D task or external download", "master":"CrossingCrab.blend", "delivery":"../../Game/Assets/_Game/Art/Golf/Events/CrossingCrab.fbx", "triangles":counts,"bones":len(bones),"externalImages":0,"externalLibraries":0,"fbxSha256":hashlib.sha256((OUT/"CrossingCrab.fbx").read_bytes()).hexdigest()}
(HERE/"crab-provenance.json").write_text(json.dumps(record,indent=2)+"\n",encoding="utf-8")
print("CRAB_EXPORT_OK",json.dumps(record))
