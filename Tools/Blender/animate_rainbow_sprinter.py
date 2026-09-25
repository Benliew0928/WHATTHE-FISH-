"""Editable Rainbow Sprinter animation: setup, preview, audit, or export.

blender --background --factory-startup --python <this file> -- setup
See Docs/VisualDirection/IDLE-AUTHORING.md. Existing authored files are never
replaced by setup. Export/preview operate on saved actions, not generated keys.
"""
import argparse
import json
import math
import sys
from pathlib import Path

import bpy
from mathutils import Matrix, Vector

ROOT = Path(__file__).resolve().parents[2]
SOURCE = ROOT / "ArtSource/Shared/Characters/RainbowSprinter.blend"
AUTHOR = SOURCE.with_name("RainbowSprinterAnimation.blend")
OUTPUT = ROOT / "Game/Assets/_Game/Art/RainbowSprinterIdle.fbx"
EVIDENCE = ROOT / "Builds/IdleQA"
PREFIX = "mixamorig:"
CONTROL_NAMES = {
    "Hips": "CTRL_Pelvis", "Spine": "CTRL_Spine", "Spine1": "CTRL_Ribs",
    "Spine2": "CTRL_Chest", "Neck": "CTRL_Neck", "Head": "CTRL_Head",
}
for side, tag in [("Left", "L"), ("Right", "R")]:
    for part, label in [("Shoulder", "Shoulder"), ("Arm", "Arm"),
                        ("ForeArm", "Elbow"), ("Hand", "Wrist")]:
        CONTROL_NAMES[side + part] = f"CTRL_{label}_{tag}"


def curves(action):
    for layer in action.layers:
        for strip in layer.strips:
            for bag in strip.channelbags:
                yield from bag.fcurves


def key(bone, frame):
    for prop in ("location", "rotation_euler", "scale"):
        bone.keyframe_insert(prop, frame=frame, group=bone.name)


def pose_rotation(bone, xyz):
    """World-aligned small rotations converted into the bone's rest axes."""
    from mathutils import Euler
    rest = bone.bone.matrix_local.to_3x3()
    delta = Euler(tuple(math.radians(v) for v in xyz), "XYZ").to_matrix()
    bone.rotation_euler = (rest.inverted() @ delta @ rest).to_euler("XYZ")


def setup():
    if AUTHOR.exists():
        print("AUTHORING_SOURCE_PRESERVED", AUTHOR)
        return
    bpy.ops.wm.open_mainfile(filepath=str(SOURCE))
    scene = bpy.context.scene
    rig = bpy.data.objects["RainbowSprinterRig"]
    rig.animation_data.action = None
    for bone in rig.pose.bones:
        bone.matrix_basis.identity()
    controls = rig.copy()
    controls.data = rig.data.copy()
    controls.animation_data_clear()
    controls.name = "RainbowAnimationControls"
    bpy.context.collection.objects.link(controls)
    controls.show_in_front = True
    controls.data.display_type = "BBONE"
    mapping = {}
    for bone in controls.data.bones:
        old = bone.name
        bone.name = CONTROL_NAMES.get(old.removeprefix(PREFIX), old)
        mapping[old] = bone.name
    rig["control_mapping"] = json.dumps(mapping)
    rig["source_skeleton"] = json.dumps([
        [b.name, b.parent.name if b.parent else None,
         [list(row) for row in b.matrix_local]] for b in rig.data.bones])

    bpy.ops.object.select_all(action="DESELECT")
    controls.select_set(True)
    bpy.context.view_layer.objects.active = controls
    bpy.ops.object.mode_set(mode="EDIT")
    for side, tag in [("Left", "L"), ("Right", "R")]:
        foot = controls.data.edit_bones[PREFIX + side + "Foot"]
        toe = controls.data.edit_bones[PREFIX + side + "ToeBase"]
        heel = controls.data.edit_bones.new("CTRL_Heel_" + tag)
        heel.head = (foot.head.x, foot.head.y + .06, 0)
        heel.tail = heel.head + Vector((0, 0, .09))
        pivot = controls.data.edit_bones.new("CTRL_ToePivot_" + tag)
        pivot.head = (toe.head.x, toe.head.y - .035, 0)
        pivot.tail = pivot.head + Vector((0, 0, .09))
        pivot.parent = heel
        target = controls.data.edit_bones.new("CTRL_Foot_" + tag)
        target.head, target.tail, target.roll = foot.head.copy(), foot.tail.copy(), foot.roll
        target.length = .11
        target.parent = pivot
        pole = controls.data.edit_bones.new("CTRL_Knee_" + tag)
        pole.head = (foot.head.x, -.6, .3)
        pole.tail = pole.head + Vector((0, 0, .09))
    bpy.ops.object.mode_set(mode="OBJECT")
    for bone in controls.pose.bones:
        bone.rotation_mode = "XYZ"
        bone.bone.use_deform = False
    for side, tag in [("Left", "L"), ("Right", "R")]:
        leg = controls.pose.bones[PREFIX + side + "Leg"]
        ik = leg.constraints.new("IK")
        ik.name = "Planted foot / two bone leg"
        ik.target = controls
        ik.subtarget = "CTRL_Foot_" + tag
        ik.pole_target = controls
        ik.pole_subtarget = "CTRL_Knee_" + tag
        ik.chain_count = 2
        ik.use_stretch = False
        # Choose the pole angle that points the knee forward, in this imported rig.
        best = (float("inf"), 0)
        for i in range(72):
            ik.pole_angle = -math.pi + i * math.tau / 72
            bpy.context.view_layer.update()
            score = leg.head.y
            if score < best[0]:
                best = (score, ik.pole_angle)
        ik.pole_angle = best[1]
        foot = controls.pose.bones[PREFIX + side + "Foot"]
        rot = foot.constraints.new("COPY_ROTATION")
        rot.name = "Foot orientation from planted control"
        rot.target = controls
        rot.subtarget = "CTRL_Foot_" + tag
        rot.target_space = "WORLD"
        rot.owner_space = "WORLD"
    for bone in rig.pose.bones:
        follow = bone.constraints.new("COPY_TRANSFORMS")
        follow.name = "Authoring controls (baked for export)"
        follow.target = controls
        follow.subtarget = mapping[bone.name]
        follow.target_space = "WORLD"
        follow.owner_space = "WORLD"

    body_collection = controls.data.collections.new("Pose controls")
    leg_collection = controls.data.collections.new("Foot placement and knee direction")
    mechanism = controls.data.collections.new("Mechanism (do not key)")
    for bone in controls.data.bones:
        for collection in list(bone.collections):
            collection.unassign(bone)
        collection = (leg_collection if any(t in bone.name for t in ["Heel_", "ToePivot_", "Foot_", "Knee_"])
                      else body_collection if bone.name.startswith("CTRL_") else mechanism)
        collection.assign(bone)
        bone.color.palette = "THEME04" if collection == body_collection else "THEME03"
    mechanism.is_visible = False
    action = bpy.data.actions.new("Idle_Playful")
    action.use_fake_user = True
    controls.animation_data_create()
    controls.animation_data.action = action
    scene.render.fps = 60
    scene.frame_start, scene.frame_end = 1, 481
    keyed = [b for b in controls.pose.bones if b.name.startswith("CTRL_")]
    # Sparse editable keys, with matching periodic tangents; no per-frame sine driver.
    for frame in range(1, 482, 15):
        t = (frame - 1) / 60
        phase = math.tau * t / 8
        sway = math.sin(phase)
        breath = math.sin(phase * 2)
        glance = max(0, math.sin(math.pi * (t - 2) / 3)) ** 3 if 2 < t < 5 else 0
        heel_lift = math.sin(math.pi * (t - 4.3) / 1.7) ** 4 if 4.3 < t < 6 else 0
        for bone in keyed:
            bone.matrix_basis.identity()
        pelvis = controls.pose.bones["CTRL_Pelvis"]
        pelvis.location = pelvis.bone.matrix_local.to_3x3().inverted() @ Vector((.012*sway, -.014, -.021 + .0025*breath))
        pose_rotation(pelvis, (0, 1.2*sway, .8*sway))
        pose_rotation(controls.pose.bones["CTRL_Spine"], (1 + .6*breath, -.6*sway, 0))
        pose_rotation(controls.pose.bones["CTRL_Chest"], (.8*breath, -1.1*sway, -1.1*sway))
        pose_rotation(controls.pose.bones["CTRL_Head"], (.6*breath, 2.2*glance, -6*glance))
        for side, tag, sign in [("Left", "L", 1), ("Right", "R", -1)]:
            lag = math.sin(phase - .22 - (0 if sign == 1 else .15))
            pose_rotation(controls.pose.bones["CTRL_Shoulder_" + tag], (0, sign*2, 0))
            pose_rotation(controls.pose.bones["CTRL_Arm_" + tag], (-5 + 1.5*lag, sign*(27 + 1.2*lag), sign*2))
            pose_rotation(controls.pose.bones["CTRL_Elbow_" + tag], (-14 + 2*lag, 0, 0))
            pose_rotation(controls.pose.bones["CTRL_Wrist_" + tag], (2*lag, sign*3, sign*(2*lag + 3*glance)))
            heel = controls.pose.bones["CTRL_Heel_" + tag]
            heel.location = heel.bone.matrix_local.to_3x3().inverted() @ Vector((-sign*.018, -.015 if sign == 1 else .008, 0))
        pose_rotation(controls.pose.bones["CTRL_ToePivot_L"], (2.5*heel_lift, 0, 0))
        for bone in keyed:
            key(bone, frame)
    for curve in curves(action):
        for point in curve.keyframe_points:
            point.interpolation = "BEZIER"
            point.handle_left_type = point.handle_right_type = "AUTO_CLAMPED"
        curve.modifiers.new("CYCLES")
    block = action.copy()
    block.name = "Idle_Playful_Blocking"
    block.use_fake_user = True
    for curve in curves(block):
        for modifier in list(curve.modifiers):
            curve.modifiers.remove(modifier)
        for point in list(curve.keyframe_points):
            if int(point.co.x) not in (1, 121, 241, 301, 361, 481):
                curve.keyframe_points.remove(point)
        for point in curve.keyframe_points:
            point.interpolation = "CONSTANT"
    rig.hide_set(True)
    controls["workflow"] = "Edit Idle_Playful controls. Export bakes a temporary copy. Running is untouched."
    scene.frame_set(1)
    bpy.ops.wm.save_as_mainfile(filepath=str(AUTHOR))
    print("IDLE_AUTHORING_CREATED", AUTHOR)


def load_author(action_name):
    bpy.ops.wm.open_mainfile(filepath=str(AUTHOR))
    controls = bpy.data.objects["RainbowAnimationControls"]
    controls.animation_data.action = bpy.data.actions[action_name]
    bpy.context.scene.frame_set(1)
    return bpy.data.objects["RainbowSprinterRig"], controls


def audit(rig):
    scene = bpy.context.scene
    samples = []
    for frame in range(1, 482):
        scene.frame_set(frame)
        samples.append({b.name: (rig.matrix_world @ b.matrix).copy() for b in rig.pose.bones})
    seam_position = max((samples[0][name].translation - samples[-1][name].translation).length for name in samples[0])
    seam_rotation = max(samples[0][name].to_quaternion().rotation_difference(samples[-1][name].to_quaternion()).angle for name in samples[0])
    # A toe contact is a shoe-space anchor; rotating around the ball should not slide it.
    contacts = {}
    for side in ("Left", "Right"):
        foot = rig.data.bones[PREFIX + side + "Foot"]
        toe = rig.data.bones[PREFIX + side + "ToeBase"]
        anchor = Vector((toe.head_local.x, toe.head_local.y - .035, 0))
        local = foot.matrix_local.inverted() @ anchor
        points = [s[foot.name] @ local for s in samples]
        contacts[side] = max((p - points[0]).length for p in points)
    report = {"frames": len(samples), "duration_seconds": 8, "bone_count": len(rig.data.bones),
              "seam_position_m": seam_position, "seam_rotation_rad": seam_rotation,
              "toe_contact_drift_m": contacts,
              "hip_travel_m": max(s[PREFIX+'Hips'].translation.x for s in samples) - min(s[PREFIX+'Hips'].translation.x for s in samples)}
    report["seam_velocity_difference_m_s"] = max(
        ((samples[1][name].translation-samples[0][name].translation) * 60 -
         (samples[-1][name].translation-samples[-2][name].translation) * 60).length
        for name in samples[0])
    original = json.loads(rig["source_skeleton"])
    current = [[b.name, b.parent.name if b.parent else None,
                [list(row) for row in b.matrix_local]] for b in rig.data.bones]
    if original != current:
        raise RuntimeError("The export skeleton rest transforms or hierarchy changed")
    report["skeleton_unchanged"] = True
    EVIDENCE.mkdir(parents=True, exist_ok=True)
    (EVIDENCE / "blender-audit.json").write_text(json.dumps(report, indent=2))
    reference = {"samples": [{"time": index/60, "bones": [
        # FBX -Z forward / Y up plus Unity's right-to-left handed conversion.
        {"name": name, "position": {"x": -matrix.translation.x, "y": matrix.translation.z, "z": -matrix.translation.y}}
        for name, matrix in samples[index].items()]} for index in [0, 1, 60, 120, 180, 240, 300, 360, 420, 479, 480]]}
    (EVIDENCE / "blender-reference.json").write_text(json.dumps(reference))
    if seam_position > .0005 or seam_rotation > .002 or max(contacts.values()) > .005 or report["seam_velocity_difference_m_s"] > .01:
        raise RuntimeError("Idle continuity/contact audit failed: " + json.dumps(report))
    print("IDLE_BLENDER_AUDIT", json.dumps(report))
    return samples


def export(rig):
    samples = audit(rig)
    bake_samples(rig, samples, OUTPUT, "Idle_Playful_Baked")


def bake_samples(rig, samples, output, action_name):
    """Bake an explicitly sampled action without saving over the authoring file."""
    scene = bpy.context.scene
    scene.frame_start, scene.frame_end = 1, len(samples)
    rig.hide_set(False)
    for bone in rig.pose.bones:
        for constraint in list(bone.constraints):
            bone.constraints.remove(constraint)
        bone.rotation_mode = "QUATERNION"
    rig.animation_data_clear()
    rig.animation_data_create()
    action = bpy.data.actions.new(action_name)
    rig.animation_data.action = action
    previous = {}
    for index, sample in enumerate(samples):
        scene.frame_set(index + 1)
        for bone in rig.pose.bones:
            matrix = rig.matrix_world.inverted() @ sample[bone.name]
            parent_pose = rig.matrix_world.inverted() @ sample[bone.parent.name] if bone.parent else Matrix.Identity(4)
            basis = bone.bone.convert_local_to_pose(matrix, bone.bone.matrix_local,
                parent_matrix=parent_pose,
                parent_matrix_local=bone.parent.bone.matrix_local if bone.parent else Matrix.Identity(4), invert=True)
            loc, rot, scale = basis.decompose()
            if bone.name in previous and rot.dot(previous[bone.name]) < 0:
                rot.negate()
            previous[bone.name] = rot.copy()
            bone.location, bone.rotation_quaternion, bone.scale = loc, rot, scale
            for prop in ("location", "rotation_quaternion", "scale"):
                bone.keyframe_insert(prop, frame=index+1, group=bone.name)
    bpy.ops.object.select_all(action="DESELECT")
    rig.select_set(True)
    bpy.context.view_layer.objects.active = rig
    # Active action only; do not export other preserved actions or control objects.
    bpy.ops.export_scene.fbx(filepath=str(output), use_selection=True, object_types={"ARMATURE"},
        axis_forward="-Z", axis_up="Y", add_leaf_bones=False, bake_anim=True,
        bake_anim_use_all_actions=False, bake_anim_use_nla_strips=False,
        bake_anim_step=1, bake_anim_simplify_factor=0, path_mode="AUTO")
    print("ANIMATION_EXPORT_READY", output)


def preview(rig, args):
    scene = bpy.context.scene
    for obj in bpy.data.objects:
        if obj.type == "MESH":
            visible = obj.name == "RainbowSprinter_" + args.lod
            obj.hide_render = not visible
            obj.hide_set(not visible)
    scene.render.engine = "CYCLES" if args.engine == "cycles" else "BLENDER_EEVEE"
    scene.cycles.samples = 8 if args.video else 16
    scene.cycles.use_denoising = True
    scene.render.resolution_x = scene.render.resolution_y = args.size
    scene.render.resolution_percentage = 100
    scene.render.image_settings.file_format = "PNG"
    if scene.world is None:
        scene.world = bpy.data.worlds.new("QA world")
    scene.world.color = (.18, .18, .18)
    scene.view_settings.view_transform = "AgX"
    bpy.ops.mesh.primitive_plane_add(size=200, location=(0, 0, -.005))
    floor = bpy.context.object
    floor.name = "QA floor"
    material = bpy.data.materials.new("QA warm grey")
    material.diffuse_color = (.19, .23, .25, 1)
    floor.data.materials.append(material)
    for name, position, power, size in [("Key", (3,-4,5), 450, 4), ("Fill", (-3,-2,3), 250, 3), ("Rim", (1,2,4), 500, 3)]:
        light = bpy.data.lights.new(name, "AREA")
        light.energy, light.shape, light.size = power, "DISK", size
        obj = bpy.data.objects.new(name, light)
        scene.collection.objects.link(obj)
        obj.location = position
        obj.rotation_euler = (Vector((0,0,.8))-obj.location).to_track_quat("-Z","Y").to_euler()
    camera_data = bpy.data.cameras.new("QA camera")
    camera = bpy.data.objects.new("QA camera", camera_data)
    scene.collection.objects.link(camera)
    scene.camera = camera
    camera_data.type, camera_data.ortho_scale = "ORTHO", 2.05
    views = {"front": (0,-4,1.25), "side": (4,0,1.25), "back": (0,4,1.25), "three-quarter": (2.5,-4,1.5)}
    out = EVIDENCE / args.label
    out.mkdir(parents=True, exist_ok=True)
    frames = range(scene.frame_start,scene.frame_end,2) if args.video else [int(f) for f in args.frames.split(",")]
    for view in (["three-quarter"] if args.video else views):
        camera.location = views[view]
        camera.rotation_euler = (Vector((0,0,.82))-camera.location).to_track_quat("-Z","Y").to_euler()
        for frame in frames:
            scene.frame_set(frame)
            scene.render.filepath = str(out / f"{view}-{frame:04d}.png")
            bpy.ops.render.render(write_still=True)
    print("IDLE_PREVIEW_READY", out)


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("command", choices=["setup", "export", "preview", "audit"])
    parser.add_argument("--action", default="Idle_Playful")
    parser.add_argument("--frames", default="1,121,241,301,361")
    parser.add_argument("--size", type=int, default=512)
    parser.add_argument("--video", action="store_true")
    parser.add_argument("--engine", choices=["cycles", "eevee"], default="cycles")
    parser.add_argument("--label", default="review")
    parser.add_argument("--lod", choices=["LOD0", "LOD1"], default="LOD0")
    args = parser.parse_args(sys.argv[sys.argv.index("--")+1:])
    if args.command == "setup":
        setup()
        return
    rig, _ = load_author(args.action)
    if args.command == "export":
        if args.action != "Idle_Playful":
            raise ValueError("Delivery export requires Idle_Playful")
        export(rig)
    elif args.command == "audit":
        audit(rig)
    else:
        preview(rig, args)


if __name__ == "__main__":
    main()
