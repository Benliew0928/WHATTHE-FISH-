"""Author, audit, preview and export saved non-looping turnaround actions.

Runs in Blender. Makes a separate protected turn source; never regenerates idle.
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

AUTHOR = idle.AUTHOR.with_name("RainbowSprinterTurns.blend")
EVIDENCE = idle.ROOT / "Builds/TurnQA"
ACTIONS = {f"Turn_{side}_{angle}": sign*angle for side, sign in [("Left", -1), ("Right", 1)] for angle in [90,180]}


def ease(t):
    t = max(0, min(1, t))
    return t*t*(3-2*t)


def facing(t):
    return ease((t-.15)/.67)


def step(t, leading):
    """World footprint angle fraction and swing height; also used by Unity IK."""
    if leading:
        if t < .12:
            return 0, 0
        if t < .35:
            u=(t-.12)/.23
            return .60*ease(u), .065*math.sin(math.pi*u)**2
        if t < .70:
            return .60, 0
        if t < .96:
            u=(t-.70)/.26
            return .60+.40*ease(u), .045*math.sin(math.pi*u)**2
        return 1, 0
    if t < .35:
        return 0, 0
    if t < .70:
        u=(t-.35)/.35
        return ease(u), .075*math.sin(math.pi*u)**2
    return 1, 0


def setup():
    if AUTHOR.exists():
        print("TURN_SOURCE_PRESERVED", AUTHOR)
        return
    bpy.ops.wm.open_mainfile(filepath=str(idle.AUTHOR))
    scene=bpy.context.scene
    rig=bpy.data.objects["RainbowSprinterRig"]
    controls=bpy.data.objects["RainbowAnimationControls"]
    controls.animation_data.action=bpy.data.actions["Idle_Playful"]
    scene.frame_set(1)
    base={b.name:b.matrix_basis.copy() for b in controls.pose.bones}
    feet={tag:controls.pose.bones["CTRL_Foot_"+tag].matrix.copy() for tag in ("L","R")}
    # The supplied shoe weights include lower-leg influence. A 5 mm clearance
    # keeps the deforming sole within the contact tolerance during deep pivots.
    for matrix in feet.values():
        matrix.translation.z+=.005
    hip_offset=controls.pose.bones["CTRL_Pelvis"].matrix.translation-(feet['L'].translation+feet['R'].translation)*.5
    hip_offset.z=0
    facing_control=bpy.data.objects.new("TurnFacing",None)
    scene.collection.objects.link(facing_control)
    facing_control.empty_display_type="ARROWS"
    facing_control.empty_display_size=.4
    controls.parent=facing_control
    controls.matrix_parent_inverse=Matrix.Identity(4)
    rig["turn_base_feet"]=json.dumps({tag:[list(row) for row in matrix] for tag,matrix in feet.items()})
    for name,angle in ACTIONS.items():
        action=bpy.data.actions.new(name)
        action.use_fake_user=True
        action["turn_degrees"]=angle
        action["duration_seconds"]=.35 if abs(angle)==90 else .55
        action["loop"]=False
        controls.animation_data.action=action
        yaw_action=bpy.data.actions.new(name+"_Facing")
        yaw_action.use_fake_user=True
        facing_control.animation_data_create()
        facing_control.animation_data.action=yaw_action
        end=22 if abs(angle)==90 else 34
        for frame in range(1,end+1):
            scene.frame_set(frame)
            t=(frame-1)/(end-1)
            yaw=-math.radians(angle)*facing(t)
            facing_control.rotation_euler=(0,0,yaw)
            facing_control.keyframe_insert("rotation_euler",frame=frame)
            for b in controls.pose.bones:
                b.matrix_basis=base[b.name].copy()
            intensity=math.sin(math.pi*t)**2
            pelvis=controls.pose.bones["CTRL_Pelvis"]
            pelvis.location+=pelvis.bone.matrix_local.to_3x3().inverted()@Vector((0,0,-.020*intensity))
            idle.pose_rotation(controls.pose.bones["CTRL_Chest"],(2*intensity,0,-math.copysign(5,angle)*intensity))
            idle.pose_rotation(controls.pose.bones["CTRL_Head"],(0,0,-math.copysign(7,angle)*math.sin(math.pi*min(1,t/.72))**2))
            for tag,sign in [("L",1),("R",-1)]:
                idle.pose_rotation(controls.pose.bones["CTRL_Arm_"+tag],(-5-6*intensity,sign*(27-5*intensity),sign*4*intensity))
                idle.pose_rotation(controls.pose.bones["CTRL_Elbow_"+tag],(-14-10*intensity,0,0))
            bpy.context.view_layer.update()
            targets={}
            for tag in ("L","R"):
                leading=(tag=="R")==(angle>0)
                fraction,height=step(t,leading)
                world=Matrix.Rotation(-math.radians(angle)*fraction,4,"Z")@feet[tag]
                world.translation.z+=height
                targets[tag]=world
            # Carry the pelvis over the support polygon as the short legs step.
            center=(targets['L'].translation+targets['R'].translation)*.5+facing_control.matrix_world.to_3x3()@hip_offset
            pelvis_world=facing_control.matrix_world@pelvis.matrix
            pelvis_world.translation.x=center.x
            pelvis_world.translation.y=center.y
            pelvis.matrix=facing_control.matrix_world.inverted()@pelvis_world
            bpy.context.view_layer.update()
            for tag,world in targets.items():
                bone=controls.pose.bones["CTRL_Foot_"+tag]
                bone.matrix=facing_control.matrix_world.inverted()@world
                bpy.context.view_layer.update()
            for b in controls.pose.bones:
                if b.name.startswith("CTRL_"):
                    idle.key(b,frame)
        for a in (action,yaw_action):
            for curve in idle.curves(a):
                for point in curve.keyframe_points:
                    point.interpolation="BEZIER"
                    point.handle_left_type=point.handle_right_type="AUTO_CLAMPED"
    controls.animation_data.action=bpy.data.actions["Turn_Right_180"]
    facing_control.animation_data.action=bpy.data.actions["Turn_Right_180_Facing"]
    scene.frame_start,scene.frame_end=1,34
    scene.frame_set(1)
    bpy.ops.wm.save_as_mainfile(filepath=str(AUTHOR))
    print("TURN_SOURCE_READY", AUTHOR)


def load(name):
    bpy.ops.wm.open_mainfile(filepath=str(AUTHOR))
    controls=bpy.data.objects["RainbowAnimationControls"]
    controls.animation_data.action=bpy.data.actions[name]
    face=bpy.data.objects["TurnFacing"]
    face.animation_data.action=bpy.data.actions[name+"_Facing"]
    scene=bpy.context.scene
    scene.frame_start,scene.frame_end=1,22 if abs(ACTIONS[name])==90 else 34
    scene.frame_set(1)
    return bpy.data.objects["RainbowSprinterRig"],face


def audit(name,rig,face):
    scene=bpy.context.scene
    samples=[]
    worst=0
    base={tag:Matrix(rows) for tag,rows in json.loads(rig["turn_base_feet"]).items()}
    minimum=1
    for frame in range(scene.frame_start,scene.frame_end+1):
        scene.frame_set(frame)
        t=(frame-1)/(scene.frame_end-1)
        inverse=face.matrix_world.inverted()
        samples.append({b.name:inverse@rig.matrix_world@b.matrix for b in rig.pose.bones})
        for side,tag in [("Left","L"),("Right","R")]:
            fraction,height=step(t,(tag=="R")==(ACTIONS[name]>0))
            target=(Matrix.Rotation(-math.radians(ACTIONS[name])*fraction,4,"Z")@base[tag]).translation
            target.z+=height
            ankle=(rig.matrix_world@rig.pose.bones[idle.PREFIX+side+"Foot"].matrix).translation
            worst=max(worst,(ankle-target).length)
        mesh=bpy.data.objects['RainbowSprinter_LOD0'].evaluated_get(bpy.context.evaluated_depsgraph_get())
        data=mesh.to_mesh()
        minimum=min(minimum,min((mesh.matrix_world@v.co).z for v in data.vertices))
        mesh.to_mesh_clear()
    original=json.loads(rig["source_skeleton"])
    assert original==[[b.name,b.parent.name if b.parent else None,[list(row) for row in b.matrix_local]] for b in rig.data.bones]
    report={"action":name,"duration":(len(samples)-1)/60,"max_ankle_target_error_m":worst,"minimum_vertex_z":minimum,"skeleton_unchanged":True}
    EVIDENCE.mkdir(parents=True,exist_ok=True)
    (EVIDENCE/(name+"-audit.json")).write_text(json.dumps(report,indent=2))
    print("TURN_AUDIT",json.dumps(report))
    if worst>.005 or minimum<-.006:
        raise RuntimeError("Turn contact/deformation failed")
    return samples


def main():
    parser=argparse.ArgumentParser()
    parser.add_argument('command',choices=['setup','audit','export','preview'])
    parser.add_argument('--action',choices=list(ACTIONS),default='Turn_Right_180')
    parser.add_argument('--video',action='store_true')
    args=parser.parse_args(sys.argv[sys.argv.index('--')+1:])
    if args.command=='setup':
        setup();return
    names=list(ACTIONS) if args.command in ('audit','export') else [args.action]
    for name in names:
        rig,face=load(name)
        if args.command=='preview':
            idle.EVIDENCE=EVIDENCE
            end=bpy.context.scene.frame_end
            preview_args=SimpleNamespace(lod='LOD0',engine='eevee',video=args.video,size=512,label=name,
                frames=','.join(str(f) for f in [1,round(end*.25),round(end*.5),round(end*.75),end]))
            idle.preview(rig,preview_args)
        else:
            samples=audit(name,rig,face)
            if args.command=='export':
                idle.bake_samples(rig,samples,idle.ROOT/'Game/Assets/_Game/Art'/('RainbowSprinter'+name+'.fbx'),name+'_Baked')
                # Explicit yaw samples let Unity use the same authoring timing.
                (EVIDENCE/(name+'-yaw.json')).write_text(json.dumps([facing(i/(len(samples)-1)) for i in range(len(samples))]))


if __name__=='__main__':
    main()
