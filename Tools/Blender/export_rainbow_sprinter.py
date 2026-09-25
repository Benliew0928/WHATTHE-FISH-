"""Build an editable Blender master and two skinned Unity LODs from Meshy FBX.

Run after prepare_rainbow_sprinter.py with Blender 5.1 in background mode.
Only the 41-frame running action is exported; the short Meshy take is ignored.
"""
from pathlib import Path

import bpy


ROOT = Path(__file__).resolve().parents[2]
SOURCE = ROOT / "ArtSource/Shared/Characters"
MESHY = SOURCE / "Meshy"
OUTPUT = ROOT / "Game/Assets/_Game/Art/RainbowSprinter.fbx"
RUN_OUTPUT = ROOT / "Game/Assets/_Game/Art/RainbowSprinterRun.fbx"


def decimated_copy(original, name, triangle_target):
    copy = original.copy()
    copy.data = original.data.copy()
    bpy.context.collection.objects.link(copy)
    copy.name = name
    ratio = min(1.0, triangle_target / len(original.data.polygons))
    modifier = copy.modifiers.new("Game triangle budget", "DECIMATE")
    modifier.ratio = ratio
    modifier.delimit = {"UV"}
    bpy.ops.object.select_all(action="DESELECT")
    copy.select_set(True)
    bpy.context.view_layer.objects.active = copy
    while copy.modifiers[0] != modifier:
        bpy.ops.object.modifier_move_up(modifier=modifier.name)
    bpy.ops.object.modifier_apply(modifier=modifier.name)
    if not copy.vertex_groups or len(copy.data.polygons) > triangle_target * 1.08:
        raise RuntimeError(f"LOD export failed for {name}")
    return copy


def main():
    bpy.ops.wm.read_factory_settings(use_empty=True)
    bpy.ops.import_scene.fbx(filepath=str(MESHY / "RainbowSprinter-running-original.fbx"))
    rigs = [obj for obj in bpy.data.objects if obj.type == "ARMATURE"]
    meshes = sorted((obj for obj in bpy.data.objects if obj.type == "MESH"),
                    key=lambda obj: len(obj.data.polygons), reverse=True)
    if len(rigs) != 1 or not meshes or len(meshes[0].data.polygons) < 100000:
        raise RuntimeError("The Meshy source rig or high-detail mesh was not found")
    rig, source = rigs[0], meshes[0]
    for obj in list(bpy.data.objects):
        if obj not in (rig, source):
            bpy.data.objects.remove(obj, do_unlink=True)
    rig.name = "RainbowSprinterRig"
    source.name = "RainbowSprinter_SOURCE"

    actions = list(bpy.data.actions)
    run = max(actions, key=lambda action: action.frame_range[1] - action.frame_range[0])
    if run.frame_range[1] - run.frame_range[0] < 35:
        raise RuntimeError("The expected running action was not found")
    for action in actions:
        if action != run:
            bpy.data.actions.remove(action, do_unlink=True)
    run.name = "Running"
    run.use_fake_user = True
    rig.animation_data_create()
    rig.animation_data.action = None
    bpy.context.scene.render.fps = 60
    bpy.context.scene.frame_start = int(run.frame_range[0])
    bpy.context.scene.frame_end = int(run.frame_range[1])

    material = bpy.data.materials.new("RainbowSprinter_PBR")
    material.use_nodes = True
    nodes = material.node_tree.nodes
    links = material.node_tree.links
    principled = nodes.get("Principled BSDF")
    color = bpy.data.images.load(str(MESHY / "albedo.png"), check_existing=True)
    normal = bpy.data.images.load(str(MESHY / "normal.png"), check_existing=True)
    roughness = bpy.data.images.load(str(MESHY / "roughness.png"), check_existing=True)
    normal.colorspace_settings.name = "Non-Color"
    roughness.colorspace_settings.name = "Non-Color"
    color_node = nodes.new("ShaderNodeTexImage")
    color_node.image = color
    links.new(color_node.outputs["Color"], principled.inputs["Base Color"])
    normal_node = nodes.new("ShaderNodeTexImage")
    normal_node.image = normal
    normal_map = nodes.new("ShaderNodeNormalMap")
    normal_map.inputs["Strength"].default_value = 0.6
    links.new(normal_node.outputs["Color"], normal_map.inputs["Color"])
    links.new(normal_map.outputs["Normal"], principled.inputs["Normal"])
    roughness_node = nodes.new("ShaderNodeTexImage")
    roughness_node.image = roughness
    links.new(roughness_node.outputs["Color"], principled.inputs["Roughness"])
    source.data.materials.clear()
    source.data.materials.append(material)

    lod0 = decimated_copy(source, "RainbowSprinter_LOD0", 100000)
    lod1 = decimated_copy(source, "RainbowSprinter_LOD1", 12000)
    bpy.context.scene.frame_set(bpy.context.scene.frame_start)
    for bone in rig.pose.bones:
        bone.matrix_basis.identity()
    source.hide_set(True)
    source.hide_render = True
    lod1.hide_set(True)
    lod1.hide_render = True
    bpy.ops.wm.save_as_mainfile(filepath=str(SOURCE / "RainbowSprinter.blend"))

    bpy.ops.object.select_all(action="DESELECT")
    for obj in (rig, lod0, lod1):
        obj.hide_set(False)
        obj.hide_render = False
        obj.select_set(True)
    bpy.context.view_layer.objects.active = rig
    bpy.ops.export_scene.fbx(
        filepath=str(OUTPUT), use_selection=True, object_types={"MESH", "ARMATURE"},
        axis_forward="-Z", axis_up="Y", add_leaf_bones=False,
        bake_anim=False,
        path_mode="AUTO", use_mesh_modifiers=True,
    )
    bpy.ops.object.select_all(action="DESELECT")
    for obj in (rig, lod0, lod1):
        obj.select_set(True)
    bpy.context.view_layer.objects.active = rig
    bpy.ops.export_scene.fbx(
        filepath=str(RUN_OUTPUT), use_selection=True, object_types={"MESH", "ARMATURE"},
        axis_forward="-Z", axis_up="Y", add_leaf_bones=False,
        bake_anim=True, bake_anim_use_all_actions=True,
        bake_anim_use_nla_strips=False, bake_anim_simplify_factor=0,
        path_mode="AUTO", use_mesh_modifiers=True,
    )
    print("RAINBOW_EXPORT_READY", len(source.data.polygons),
          len(lod0.data.polygons), len(lod1.data.polygons),
          len(rig.data.bones), tuple(run.frame_range), OUTPUT, RUN_OUTPUT)


if __name__ == "__main__":
    main()
