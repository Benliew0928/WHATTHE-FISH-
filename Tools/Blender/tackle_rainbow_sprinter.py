"""Saved football slide/reaction actions; setup never overwrites authored work."""
import argparse
import json
import math
import sys
from pathlib import Path
from types import SimpleNamespace
import bpy
from mathutils import Matrix, Vector

sys.path.insert(0, str(Path(__file__).parent))
import animate_rainbow_sprinter as idle

AUTHOR = idle.AUTHOR.with_name('RainbowSprinterFootball.blend')
OUT = idle.ROOT / 'Builds/TackleQA'
ACTIONS = {'Slide_Tackle': 52, 'Tackle_Hit': 28}


def ease(t):
    t = max(0, min(1, t))
    return t*t*(3-2*t)


def setup():
    if AUTHOR.exists():
        print('FOOTBALL_SOURCE_PRESERVED', AUTHOR)
        return
    bpy.ops.wm.open_mainfile(filepath=str(idle.AUTHOR))
    scene = bpy.context.scene
    controls = bpy.data.objects['RainbowAnimationControls']
    controls.animation_data.action = bpy.data.actions['Idle_Playful']
    scene.frame_set(1)
    basis = {b.name: b.matrix_basis.copy() for b in controls.pose.bones}
    hip = controls.pose.bones['CTRL_Pelvis'].matrix.copy()
    feet = {s: controls.pose.bones['CTRL_Foot_'+s].matrix.copy() for s in ('L', 'R')}
    for name, end in ACTIONS.items():
        action = bpy.data.actions.new(name)
        action.use_fake_user = True
        controls.animation_data.action = action
        for frame in range(1, end+1):
            scene.frame_set(frame)
            time = (frame-1)/60
            if name == 'Slide_Tackle':
                weight = ease(time/.13)*(1-ease((time-.43)/(.85-.43)))
                drop, lean = .29*weight, -58*weight
            else:
                weight = ease(time/.07)*(1-ease((time-.12)/.33))
                drop, lean = .07*weight, -16*weight
            for b in controls.pose.bones:
                b.matrix_basis = basis[b.name].copy()
            pelvis = controls.pose.bones['CTRL_Pelvis']
            target = hip.copy()
            target.translation += Vector((.015*weight, (.025 if name == 'Slide_Tackle' else .015)*weight, -drop))
            rotation = Matrix.Rotation(math.radians(8*weight if name == 'Slide_Tackle' else 0), 4, 'Y') @ Matrix.Rotation(math.radians(lean), 4, 'X') @ hip
            rotation.translation = target.translation
            pelvis.matrix = rotation
            idle.pose_rotation(controls.pose.bones['CTRL_Chest'], (5*weight, 0, -5*weight))
            idle.pose_rotation(controls.pose.bones['CTRL_Head'], ((25 if name == 'Slide_Tackle' else 8)*weight, 0, 3*weight))
            for tag, sign in [('L', 1), ('R', -1)]:
                idle.pose_rotation(controls.pose.bones['CTRL_Arm_'+tag],
                                   (-5+(-10 if tag=='L' else 25)*weight, sign*(27+(18 if tag=='L' else 3)*weight), sign*8*weight))
                idle.pose_rotation(controls.pose.bones['CTRL_Elbow_'+tag], (-14+(-22 if tag=='L' else 0)*weight, 0, 0))
            bpy.context.view_layer.update()
            for tag in ('L', 'R'):
                foot = feet[tag].copy()
                if name == 'Slide_Tackle':
                    foot.translation += Vector(((.025 if tag=='L' else -.015)*weight,
                                                (-.25 if tag=='L' else .12)*weight, .012*weight))
                    if tag == 'L':
                        foot.translation.z += .02*(math.sin(math.pi*ease((time-.43)/.42))**2 + math.sin(math.pi*ease(time/.13))**2)
                else:
                    foot.translation += Vector((0, (.06 if tag=='L' else -.025)*weight, .025*weight if tag=='L' else 0))
                controls.pose.bones['CTRL_Foot_'+tag].matrix = foot
                bpy.context.view_layer.update()
            for b in controls.pose.bones:
                if b.name.startswith('CTRL_'):
                    idle.key(b, frame)
        for curve in idle.curves(action):
            for point in curve.keyframe_points:
                point.interpolation = 'BEZIER'
                point.handle_left_type = point.handle_right_type = 'AUTO_CLAMPED'
    controls.animation_data.action = bpy.data.actions['Slide_Tackle']
    scene.frame_start, scene.frame_end = 1, ACTIONS['Slide_Tackle']
    scene.frame_set(1)
    bpy.ops.wm.save_as_mainfile(filepath=str(AUTHOR))
    print('FOOTBALL_SOURCE_READY', AUTHOR)


def load(name):
    bpy.ops.wm.open_mainfile(filepath=str(AUTHOR))
    bpy.data.objects['RainbowAnimationControls'].animation_data.action = bpy.data.actions[name]
    bpy.context.scene.frame_start, bpy.context.scene.frame_end = 1, ACTIONS[name]
    return bpy.data.objects['RainbowSprinterRig']


def audit(name, rig):
    original = json.loads(rig['source_skeleton'])
    assert original == [[b.name, b.parent.name if b.parent else None, [list(r) for r in b.matrix_local]] for b in rig.data.bones]
    samples, minimum, reach = [], 1, 0
    for frame in range(1, ACTIONS[name]+1):
        bpy.context.scene.frame_set(frame)
        samples.append({b.name: rig.matrix_world @ b.matrix for b in rig.pose.bones})
        controls = bpy.data.objects['RainbowAnimationControls']
        for side, tag in [('Left','L'), ('Right','R')]:
            actual = (rig.matrix_world @ rig.pose.bones[idle.PREFIX+side+'Foot'].matrix).translation
            target = (controls.matrix_world @ controls.pose.bones['CTRL_Foot_'+tag].matrix).translation
            reach = max(reach, (target-actual).length)
        obj = bpy.data.objects['RainbowSprinter_LOD0'].evaluated_get(bpy.context.evaluated_depsgraph_get())
        mesh = obj.to_mesh()
        minimum = min(minimum, min((obj.matrix_world @ v.co).z for v in mesh.vertices))
        obj.to_mesh_clear()
    report = dict(action=name, duration=(ACTIONS[name]-1)/60, minimum_vertex_z=minimum, ankle_target_error=reach, bones=len(rig.data.bones))
    OUT.mkdir(parents=True, exist_ok=True)
    (OUT/(name+'-audit.json')).write_text(json.dumps(report, indent=2))
    print('FOOTBALL_AUDIT', json.dumps(report))
    if minimum < -.006 or reach > .008:
        raise RuntimeError('Football pose contact/reach failed')
    return samples


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('command', choices=['setup','audit','export','preview'])
    parser.add_argument('--action', choices=list(ACTIONS), default='Slide_Tackle')
    parser.add_argument('--video', action='store_true')
    args = parser.parse_args(sys.argv[sys.argv.index('--')+1:])
    if args.command == 'setup':
        setup(); return
    for name in (list(ACTIONS) if args.command in ('audit','export') else [args.action]):
        rig = load(name)
        if args.command == 'preview':
            idle.EVIDENCE = OUT
            idle.preview(rig, SimpleNamespace(lod='LOD0', engine='eevee', video=args.video, size=512, label=name,
                         frames=','.join(str(f) for f in [1,6,13,22,ACTIONS[name]])))
        else:
            samples = audit(name, rig)
            if args.command == 'export':
                idle.bake_samples(rig, samples, idle.ROOT/'Game/Assets/_Game/Art'/('RainbowSprinter'+name+'.fbx'), name+'_Baked')


if __name__ == '__main__':
    main()
