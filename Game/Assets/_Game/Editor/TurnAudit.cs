using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using SportsPrototype;
using UnityEditor;
using UnityEngine;

public static class TurnAudit {
 static readonly List<string> lines=new();
 static void Require(bool value,string message){if(!value)throw new Exception(message);}
 public static void Run(){
  lines.Clear();MotorChecks();AssetChecks();
  Directory.CreateDirectory("../Builds/TurnQA");File.WriteAllLines("../Builds/TurnQA/editor-audit.txt",lines);
  Debug.Log("TURN_AUDIT_COMPLETE "+string.Join("; ",lines));
 }
 static void MotorChecks(){
  foreach(int fps in new[]{30,60,120})foreach(int framesPerSwitch in new[]{1,3,6,12})foreach(float magnitude in new[]{.2f,1f}){
   var motor=new LocomotionMotor();motor.Reset(0);float dt=1f/fps,distance=0,maxYaw=0;
   for(int i=0;i<fps*3;i++){
    var desired=(i/framesPerSwitch%2==0?Vector3.forward:Vector3.back)*magnitude;float before=motor.Yaw;
    var delta=motor.Step(desired,7,true,dt);distance+=delta.magnitude;
    Require(Vector3.Dot(delta,desired.normalized)>0,"Fresh input failed to move in its requested direction");
    Require(Vector3.Cross(delta,desired).magnitude<.00001f,"Translation follows body instead of stick");
    maxYaw=Mathf.Max(maxYaw,Mathf.Abs(Mathf.DeltaAngle(before,motor.Yaw)));
    Require(motor.Velocity.magnitude<=7*magnitude+.001f,"Speed exceeded input strength");
   }
   Require(distance>7*magnitude*2.85f,"Rapid reversals lost movement");
   Require(maxYaw<1620f/fps+.01f,"Facing snapped");
   for(int i=0;i<fps;i++)motor.Step(Vector3.right,4,true,dt);
   Require(Mathf.Abs(Mathf.DeltaAngle(motor.Yaw,90))<1&&Mathf.Abs(motor.Velocity.magnitude-4)<.01f,"Held input/sprint release did not settle");
   float yaw=motor.Yaw;for(int i=0;i<fps;i++)motor.Step(Vector3.zero,4,true,dt);
   Require(motor.Phase==LocomotionPhase.Idle&&motor.Velocity.sqrMagnitude==0&&Mathf.Abs(Mathf.DeltaAngle(yaw,motor.Yaw))<.001f,"Release did not cancel turning");
   lines.Add($"PASS responsive reversals fps={fps} switchEvery={framesPerSwitch}frames magnitude={magnitude} path={distance:F3}m maxYawStep={maxYaw:F2}");
  }
  foreach(int fps in new[]{30,60,120})foreach(float angle in new[]{-180f,-150f,-90f,90f,150f,180f}){
   var m=new LocomotionMotor();m.Reset(0);var desired=Quaternion.Euler(0,angle,0)*Vector3.forward;
   Require(Vector3.Dot(m.Step(desired,7,true,1f/fps),desired)>0,"Standing start waited for turn");
   float yaw=m.Yaw;m.Step(Vector3.zero,7,true,1f/fps);Require(Mathf.Abs(Mathf.DeltaAngle(yaw,m.Yaw))<.001f,"Release finished old turn");
   Require(Vector3.Dot(m.Step(-desired,7,true,1f/fps),-desired)>0,"New input blocked by braking");
  }
  var idle=new LocomotionMotor();idle.Reset(0);Require(idle.Step(Vector3.right*.1f,7,true,.1f)==Vector3.zero,"Dead zone drift");
  Require(idle.Step(Vector3.back,7,false,.02f).z<0&&idle.Phase!=LocomotionPhase.Turning,"Airborne input blocked");
  lines.Add("PASS first-tick movement, release cancels facing, brake interruption, dead zone and airborne movement");
 }
 static void AssetChecks(){
  const string art="Assets/_Game/Art/RainbowSprinter";
  foreach(var name in new[]{"Turn_Left_90","Turn_Left_180","Turn_Right_90","Turn_Right_180"}){
   var clips=AssetDatabase.LoadAllAssetsAtPath(art+name+".fbx").OfType<AnimationClip>().Where(c=>!c.name.StartsWith("__preview__")).ToArray();
   Require(clips.Length==1&&clips[0].name==name&&!clips[0].isLooping,"Turn clip import mismatch "+name);
   Require(Mathf.Abs(clips[0].length-(name.EndsWith("180")?.55f:.35f))<.002f,"Turn duration mismatch");
  }
  var prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Game/Resources/OfflineAthlete.prefab");
  var instance=UnityEngine.Object.Instantiate(prefab);
  try {
   var athlete=instance.GetComponent<Athlete>();athlete.Motor.Reset(0);
   Require(athlete.Snapshot(10).Equals(athlete.Snapshot(11)),"Idle snapshot resends without a state change");
   athlete.Motor.Step(Vector3.back,7,true,.02f);var first=athlete.Snapshot(12);
   Require(first.phase==LocomotionPhase.Turning&&Math.Abs(first.started-athlete.Snapshot(12.1).started)<.000001,"Turn start timestamp changes within a phase");
   lines.Add("PASS stable network phase timestamp and unchanged idle snapshot");
   var animator=instance.GetComponentInChildren<Animator>();animator.cullingMode=AnimatorCullingMode.AlwaysAnimate;animator.Rebind();
   var feet=instance.GetComponent<TurnFootPlacement>();float maximum=0,minY=0;
   foreach(float angle in new[]{-180f,-150f,-120f,-90f,90f,120f,150f,180f}){
    animator.SetBool("Turning",true);animator.SetFloat("TurnAngle",angle);animator.Play("Turn",0,0);
    for(int i=0;i<=60;i++){
     float p=i/60f;instance.transform.rotation=Quaternion.Euler(0,angle*LocomotionMotor.FacingProgress(p),0);
     animator.SetFloat("TurnTime",p);animator.Update(0);feet.Apply(p,angle,0);
     if(p>=.15f)maximum=Mathf.Max(maximum,feet.MaximumTargetError);
     if(i%15==0)foreach(var renderer in instance.GetComponentsInChildren<SkinnedMeshRenderer>()){
      var mesh=new Mesh();renderer.BakeMesh(mesh,true);
      foreach(var v in mesh.vertices)minY=Mathf.Min(minY,renderer.transform.TransformPoint(v).y);
      UnityEngine.Object.DestroyImmediate(mesh);
     }
    }
   }
   Require(maximum<.005f,"Runtime foot targets unreachable: "+maximum);
   Require(minY>-.008f,"Turn sole penetration: "+minY);
   animator.SetFloat("RunPlayback",1);animator.SetBool("Turning",true);animator.SetFloat("Speed",7);animator.SetFloat("TurnTime",.5f);
   animator.SetLayerWeight(1,0);animator.Play("Run",0,.2f);animator.Update(0);
   var head=instance.GetComponentsInChildren<Transform>().Single(t=>t.name=="mixamorig:Head");
   var headBefore=head.localRotation;var leftBefore=feet.left.ankle.localRotation;var rightBefore=feet.right.ankle.localRotation;var hipsBefore=feet.hips.localPosition;
   animator.SetLayerWeight(1,.65f);animator.Play("Run",0,.2f);animator.Update(0);
   Require(Quaternion.Angle(headBefore,head.localRotation)>.1f,"Turn expression mask has no head motion");
   Require(Quaternion.Angle(leftBefore,feet.left.ankle.localRotation)<.01f&&Quaternion.Angle(rightBefore,feet.right.ankle.localRotation)<.01f&&Vector3.Distance(hipsBefore,feet.hips.localPosition)<.00001f,"Turn layer overrides running legs");
   Require(animator.GetCurrentAnimatorStateInfo(0).IsName("Run"),"Turning interrupted base running state");
   lines.Add("PASS torso/head expression keeps running legs and base Run state active");
   lines.Add($"PASS imported turn clips and 8 angles: maxFootError={maximum:F7}m lowestVertex={minY:F7}m");
  } finally {UnityEngine.Object.DestroyImmediate(instance);}
 }
}
