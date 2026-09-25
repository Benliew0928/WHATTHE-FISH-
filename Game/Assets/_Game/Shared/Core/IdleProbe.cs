#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;

namespace SportsPrototype {
 public sealed partial class DevelopmentProbe {
  IEnumerator IdleAudit(){
   yield return new WaitForSeconds(2);
   var app=AppRoot.Instance;app.Show("character");yield return new WaitForSeconds(.5f);
   var athlete=app.LocalAthlete;var animator=athlete.GetComponentInChildren<Animator>();
   animator.cullingMode=AnimatorCullingMode.AlwaysAnimate;
   var transforms=athlete.GetComponentsInChildren<Transform>(true);
   var head=transforms.First(t=>t.name=="mixamorig:Head");var wrist=transforms.First(t=>t.name=="mixamorig:LeftHand");
   var pose=head.rotation;var hand=wrist.rotation;
   yield return new WaitForSeconds(2);
   Check(Quaternion.Angle(pose,head.rotation)>.1f&&Quaternion.Angle(hand,wrist.rotation)>.1f,"IDLE_PREVIEW_HEAD_AND_HAND_MOVE");
   Capture(Path.ChangeExtension(output,"preview.png"));
   foreach(SportId sport in Enum.GetValues(typeof(SportId))){
    app.SelectSport(sport);app.EnterOffline();app.view.mode=1;
    animator.Play("Idle",0,0);yield return new WaitForSeconds(.5f);
    Check(animator.GetCurrentAnimatorClipInfo(0).Any(c=>c.clip.name=="Idle_Playful"),"IDLE_CLIP_"+sport);
    var lod=athlete.GetComponentInChildren<LODGroup>();
    foreach(int level in new[]{0,1}){
     lod.ForceLOD(level);yield return new WaitForSeconds(.2f);Capture(Path.ChangeExtension(output,sport+"-lod"+level+".png"));
    }
    lod.ForceLOD(-1);
    app.view.mode=0;yield return new WaitForSeconds(.2f);
    Check(athlete.visual.GetComponentsInChildren<SkinnedMeshRenderer>(true).All(r=>!r.enabled),"IDLE_FIRST_PERSON_HIDDEN_"+sport);
    app.view.mode=1;yield return new WaitForSeconds(.2f);
    Check(athlete.visual.GetComponentsInChildren<SkinnedMeshRenderer>(true).All(r=>r.enabled),"IDLE_THIRD_PERSON_RESTORED_"+sport);
   }
   // Exercise real input/motor/Animator updates from distinct run phases.
   foreach(float phase in new[]{0f,.25f,.5f,.75f}){
    IslandDriving=true;IslandHeading=0;yield return new WaitForSeconds(.3f);animator.Play("Run",0,phase);yield return null;
    // Allow the 0.08 s stop easing plus the existing 0.25 s idle blend and frame scheduling.
    IslandDriving=false;yield return new WaitForSeconds(.5f);
    Check(animator.GetCurrentAnimatorStateInfo(0).IsName("Idle")&&!animator.IsInTransition(0),"IDLE_STOP_PHASE_"+phase);
   }
   animator.Play("Idle",0,5f/8f);yield return null;
   IslandDriving=true;yield return new WaitForSeconds(.25f);
   Check(animator.GetCurrentAnimatorStateInfo(0).IsName("Run"),"IDLE_HEEL_LIFT_INTERRUPT");
   for(int i=0;i<4;i++){
    IslandDriving=false;yield return new WaitForSeconds(.07f);IslandDriving=true;yield return new WaitForSeconds(.25f);
    Check(animator.GetCurrentAnimatorStateInfo(0).IsName("Run"),"IDLE_RAPID_RESTART_"+i);
   }
   IslandDriving=false;yield return new WaitForSeconds(.4f);
   animator.Play("Idle",0,0);yield return new WaitForSeconds(.1f);
   var start=athlete.transform.position;
   for(int i=0;i<24;i++){
    yield return new WaitForSeconds(1);
    Check(animator.GetCurrentAnimatorClipInfo(0).Any(c=>c.clip.name=="Idle_Playful"),"IDLE_THREE_LOOPS_SECOND_"+i);
   }
   Check(Vector2.Distance(new Vector2(start.x,start.z),new Vector2(athlete.transform.position.x,athlete.transform.position.z))<.001f,"IDLE_NO_ROOT_DRIFT");
   Record("IDLE_AUDIT_COMPLETE");
  }
  IEnumerator NetworkIdleAudit(){
   while(!AppRoot.Instance||!AppRoot.Instance.rooms.Connected)yield return null;
   yield return new WaitForSeconds(5);
   for(int pass=0;pass<2;pass++){
    if(pass==1){while(elapsed<float.Parse(Value("-returnAt","48"))+2)yield return null;}
    var athletes=FindObjectsByType<NetworkAthlete>(FindObjectsSortMode.None);
    var heads=athletes.Select(a=>a.GetComponentsInChildren<Transform>(true).First(t=>t.name=="mixamorig:Head")).ToArray();
    foreach(var a in athletes)a.GetComponentInChildren<Animator>().cullingMode=AnimatorCullingMode.AlwaysAnimate;
    var poses=heads.Select(h=>h.localRotation).ToArray();yield return new WaitForSeconds(2.5f);
    Check(athletes.Length==expected,"NETWORK_IDLE_COUNT_"+pass);
    for(int i=0;i<athletes.Length;i++){
     var anim=athletes[i].GetComponentInChildren<Animator>();
     Check(anim.GetCurrentAnimatorClipInfo(0).Any(c=>c.clip.name=="Idle_Playful")&&Quaternion.Angle(poses[i],heads[i].localRotation)>.05f,"NETWORK_IDLE_MOVES_"+pass+"_"+athletes[i].OwnerClientId);
    }
   }
   Record("NETWORK_IDLE_AUDIT_COMPLETE");
  }
 }
}
#endif
