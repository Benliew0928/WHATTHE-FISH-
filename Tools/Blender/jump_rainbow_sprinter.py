"""Editable, in-place jump; physical height and facing are owned by the game.

Run with Blender --background --python <this file> -- setup|revise|export|audit|preview.
Setup preserves an existing master. Export always reads the saved action.
"""
import argparse
import hashlib
import json
import math
import sys
from pathlib import Path
from types import SimpleNamespace
import bpy
from mathutils import Matrix, Vector

sys.path.insert(0, str(Path(__file__).parent))
import animate_rainbow_sprinter as idle

AUTHOR = idle.AUTHOR.with_name('RainbowSprinterJump.blend')
OUT = idle.ROOT / 'Builds/JumpNaturalQA'
FRAMES = 61


def ease(t):
    t = max(0, min(1, t))
    return t*t*(3-2*t)


def sample(keys, u):
    for (a, x), (b, y) in zip(keys, keys[1:]):
        if u <= b:
            t = ease((u-a)/(b-a))
            return x+(y-x)*t
    return keys[-1][1]


def aim(bone, direction):
    """Aim the FK control in armature space; avoid guessing imported Euler axes."""
    matrix = bone.matrix.copy()
    delta = (bone.tail-bone.head).normalized().rotation_difference(direction.normalized())
    result = delta.to_matrix().to_4x4() @ matrix
    result.translation = matrix.translation
    bone.matrix = result
    bpy.context.view_layer.update()


def setup(revise=False):
    if AUTHOR.exists() and not revise:
        print('JUMP_SOURCE_PRESERVED')
        return
    bpy.ops.wm.open_mainfile(filepath=str(idle.AUTHOR))
    scene = bpy.context.scene
    controls = bpy.data.objects['RainbowAnimationControls']
    controls.animation_data.action = bpy.data.actions['Idle_Playful']
    scene.frame_set(1)
    basis = {b.name: b.matrix_basis.copy() for b in controls.pose.bones}
    hip = controls.pose.bones['CTRL_Pelvis'].matrix.copy()
    feet = {s: controls.pose.bones['CTRL_Foot_'+s].matrix.copy() for s in ('L', 'R')}
    action = bpy.data.actions.new('Jump')
    action.use_fake_user = True
    controls.animation_data.action = action
    for frame in range(1, FRAMES+1):
        scene.frame_set(frame)
        u = (frame-1)/(FRAMES-1)
        # Deliberate poses: load / push / rise / apex / reach / absorb / recover.
        drop = sample([(0,0),(.14,.145),(.24,-.018),(.44,.018),(.69,.005),(.76,.02),(.83,.16),(1,0)],u)
        back = sample([(0,0),(.14,.035),(.24,-.008),(.45,0),(.76,0),(.83,.028),(1,0)],u)
        lean = sample([(0,0),(.14,18),(.24,-3),(.46,2),(.70,5),(.76,8),(.83,20),(1,0)],u)
        arm = sample([(0,6),(.10,-35),(.14,-24),(.27,125),(.45,80),(.67,15),(.76,25),(.84,50),(1,6)],u)
        elbow = sample([(0,18),(.14,15),(.27,24),(.45,38),(.67,27),(.76,32),(.84,38),(1,18)],u)
        lift = sample([(0,0),(.14,0),(.24,.022),(.43,.075),(.60,.04),(.74,0),(1,0)],u)
        trail = sample([(0,0),(.22,0),(.43,.055),(.60,.026),(.74,0),(1,0)],u)
        toe = sample([(0,0),(.14,0),(.25,19),(.44,12),(.65,5),(.74,0),(1,0)],u)
        for b in controls.pose.bones:
            b.matrix_basis = basis[b.name].copy()
        pelvis = hip.copy()
        pelvis.translation += Vector((0, back, -drop))
        controls.pose.bones['CTRL_Pelvis'].matrix = pelvis
        idle.pose_rotation(controls.pose.bones['CTRL_Spine'], (lean*.45, 0, 0))
        idle.pose_rotation(controls.pose.bones['CTRL_Chest'], (lean*.55, 0, 0))
        idle.pose_rotation(controls.pose.bones['CTRL_Head'], (-lean*.65, 0, 0))
        bpy.context.view_layer.update()
        for tag, sign in [('L', 1), ('R', -1)]:
            angle = math.radians(arm)
            fore = math.radians(arm+elbow)
            aim(controls.pose.bones['CTRL_Arm_'+tag], Vector((sign*(.16+.60*max(0,math.sin(angle))), -math.sin(angle), -math.cos(angle))))
            aim(controls.pose.bones['CTRL_Elbow_'+tag], Vector((sign*(.12+.30*max(0,math.sin(angle))), -math.sin(fore), -math.cos(fore))))
        for tag in ('L', 'R'):
            foot = Matrix.Rotation(math.radians(toe),4,'X') @ feet[tag]
            foot.translation = feet[tag].translation + Vector((0, trail, lift))
            controls.pose.bones['CTRL_Foot_'+tag].matrix = foot
            bpy.context.view_layer.update()
        for b in controls.pose.bones:
            if b.name.startswith('CTRL_'):
                idle.key(b, frame)
    for curve in idle.curves(action):
        for point in curve.keyframe_points:
            point.interpolation = 'BEZIER'
            point.handle_left_type = point.handle_right_type = 'AUTO_CLAMPED'
    for image in bpy.data.images:
        if image.source == 'FILE' and image.filepath:
            image.filepath = bpy.path.relpath(bpy.path.abspath(image.filepath), start=str(AUTHOR.parent))
    scene.frame_start, scene.frame_end = 1, FRAMES
    action['design'] = 'Grounded load, arm swing, extended push, loose flight, forward reach and leg absorption. Version 2.'
    scene.frame_set(1)
    bpy.ops.wm.save_as_mainfile(filepath=str(AUTHOR))


def export(write):
    bpy.ops.wm.open_mainfile(filepath=str(AUTHOR))
    rig = bpy.data.objects['RainbowSprinterRig']
    controls = bpy.data.objects['RainbowAnimationControls']
    controls.animation_data.action = bpy.data.actions['Jump']
    original = json.loads(rig['source_skeleton'])
    assert original == [[b.name, b.parent.name if b.parent else None, [list(r) for r in b.matrix_local]] for b in rig.data.bones]
    dependencies = []
    for image in bpy.data.images:
        if image.source == 'FILE' and image.filepath:
            assert image.filepath.startswith('//'), image.name
            assert Path(bpy.path.abspath(image.filepath)).exists(), image.name
            dependencies.append(image.filepath)
    assert not bpy.data.libraries
    samples, error = [], 0
    for frame in range(1, FRAMES+1):
        bpy.context.scene.frame_set(frame)
        samples.append({b.name: rig.matrix_world @ b.matrix for b in rig.pose.bones})
        for side, tag in [('Left', 'L'), ('Right', 'R')]:
            actual = (rig.matrix_world @ rig.pose.bones[idle.PREFIX+side+'Foot'].matrix).translation
            target = (controls.matrix_world @ controls.pose.bones['CTRL_Foot_'+tag].matrix).translation
            error = max(error, (target-actual).length)
    assert error < .008, error
    OUT.mkdir(parents=True, exist_ok=True)
    poses = [{name: [list(row) for row in matrix] for name, matrix in sample.items()} for sample in samples]
    digest = hashlib.sha256(json.dumps(poses, sort_keys=True).encode()).hexdigest()
    (OUT/'source-audit.json').write_text(json.dumps(dict(frames=FRAMES, ankle_target_error=error, pose_sha256=digest, skeleton_unchanged=True, image_dependencies=dependencies), indent=2))
    if write:
        target = idle.ROOT/'Game/Assets/_Game/Art/RainbowSprinterJump.fbx'
        idle.bake_samples(rig, samples, target, 'Jump_Baked')
        # Animation-only FBX must carry no active texture or library dependencies.
        from io_scene_fbx import parse_fbx
        tree, _ = parse_fbx.parse(str(target))
        def inspect(node):
            assert node.id not in (b'Texture', b'Video', b'RelativeFilename'), node.id
            for child in node.elems:
                inspect(child)
        inspect(tree)


def preview():
    bpy.ops.wm.open_mainfile(filepath=str(AUTHOR))
    rig = bpy.data.objects['RainbowSprinterRig']
    controls = bpy.data.objects['RainbowAnimationControls']
    controls.animation_data.action = bpy.data.actions['Jump']
    members = {rig, controls, *[o for o in bpy.data.objects if o.name.startswith('RainbowSprinter_LOD')]}
    root = bpy.data.objects.new('Preview physical jump height', None)
    bpy.context.scene.collection.objects.link(root)
    for obj in members:
        if obj.parent not in members:
            matrix = obj.matrix_world.copy()
            obj.parent = root
            obj.matrix_world = matrix
    for frame in range(1, FRAMES+1):
        u = (frame-1)/(FRAMES-1)
        t = max(0, min(1, (u-.24)/.52))
        root.location.z = 4*1.2*t*(1-t)
        root.keyframe_insert('location',frame=frame)
    idle.EVIDENCE = OUT
    idle.preview(rig, SimpleNamespace(lod='LOD0', engine='eevee', video=False,
                 size=480,ortho_scale=3.5,focus_height=1.45,label='poses',frames='1,9,17,28,42,47,51,61'))


if __name__ == '__main__':
    parser = argparse.ArgumentParser()
    parser.add_argument('command', choices=['setup', 'revise', 'export', 'audit', 'preview'])
    args = parser.parse_args(sys.argv[sys.argv.index('--')+1:])
    if args.command in ('setup', 'revise'):
        setup(args.command == 'revise')
    elif args.command == 'preview':
        preview()
    else:
        export(args.command == 'export')
