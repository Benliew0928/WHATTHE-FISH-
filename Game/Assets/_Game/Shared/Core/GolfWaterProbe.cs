#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;

namespace WhatTheFish {
 // Opt-in water/anchor/network checks only. No effect in ordinary gameplay.
 public sealed class GolfWaterProbe:MonoBehaviour {
  GolfMatchManager match;string report;int checks;string[] args;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)] static void Boot(){if(Environment.GetCommandLineArgs().Contains("-golfWaterAudit"))new GameObject("Golf water audit").AddComponent<GolfWaterProbe>();}
  void Check(bool value,string name){File.AppendAllText(report,(value?"PASS ":"FAIL ")+name+"\n");if(!value)throw new Exception(name);checks++;}
  Vector3 Floor(Vector3 local){
   var ray=new Ray(match.transform.TransformPoint(local+Vector3.up*100),Vector3.down);
   foreach(var c in match.GetComponentInParent<RefinedIslandEnvironment>().GetComponentsInChildren<MeshCollider>().Where(c=>c.name.StartsWith("Terrain__")))if(c.Raycast(ray,out var hit,200))return hit.point+hit.normal*(GolfBall.Radius+.002f);
   throw new Exception("Water audit ground missing "+local);
  }
  Vector3 Dry(GolfWaterPatch water){
   for(int i=0;i<16;i++){
    float a=i*Mathf.PI/8;var p=Floor(water.centre+new Vector3(Mathf.Cos(a)*water.radii.x*2.1f,0,Mathf.Sin(a)*water.radii.y*2.1f));
    if(!match.Course.SafeDryPosition(p)||match.OutsideCourse(p))continue;
    if(Physics.OverlapSphere(p,GolfBall.Radius*2,1<<8,QueryTriggerInteraction.Ignore).Any(c=>!c.name.StartsWith("Terrain__")))continue;
    return p;
   }
   throw new Exception("No dry fixture beside "+water.name);
  }
  Vector3 NextDry(Vector3 dry){
   for(int i=0;i<8;i++){float a=i*Mathf.PI/4;var p=Floor(match.transform.InverseTransformPoint(dry)+new Vector3(Mathf.Cos(a)*1.5f,0,Mathf.Sin(a)*1.5f));if(match.Course.SafeDryPosition(p)&&!Physics.OverlapSphere(p,GolfBall.Radius*2,1<<8,QueryTriggerInteraction.Ignore).Any(c=>!c.name.StartsWith("Terrain__")))return p;}
   throw new Exception("No second dry fixture");
  }
  IEnumerator Start(){
   args=Environment.GetCommandLineArgs();report=args[Array.IndexOf(args,"-report")+1];Directory.CreateDirectory(Path.GetDirectoryName(report));DevelopmentProbe.TurnCommandActive=true;DevelopmentProbe.TurnCommand=default;
   float until=Time.realtimeSinceStartup+70;while((!AppRoot.Instance||!AppRoot.Instance.Exploring||!GolfMatchManager.Instance||!GolfMatchManager.Instance.Context)&&Time.realtimeSinceStartup<until)yield return null;
   match=GolfMatchManager.Instance;Check(match&&match.Context,"WATER_MATCH_CONTEXT");Check(match.Course.waterPatches.Length==6,"SIX_PONDS_LOADED");
   if(args.Contains("-networkWater"))yield return Network();else yield return Offline();
   File.AppendAllText(report,"GOLF_WATER_COMPLETE checks="+checks+"\n");
  }
  IEnumerator WaitRecovery(GolfBall ball,uint before){float until=Time.time+3;while(ball.WaterRecoverySequence==before&&Time.time<until)yield return new WaitForFixedUpdate();Check(ball.WaterRecoverySequence==before+1,"ONE_WATER_RECOVERY");}
  IEnumerator Offline(){
   Check(match.Authority&&match.StartMatch(),"OFFLINE_AUTHORITY_STARTS");
   var obj=new GameObject("Water fixture ball");obj.transform.SetParent(match.transform,false);var ball=obj.AddComponent<GolfBall>();ball.Bind(match,999999);ball.SetLive(true);
   foreach(var water in match.Course.waterPatches){
    var dry=Dry(water);ball.Place(dry,true);yield return new WaitForFixedUpdate();
    var second=NextDry(dry);
    ball.Body.position=second;ball.Body.linearVelocity=Vector3.zero;yield return new WaitForFixedUpdate();yield return new WaitForFixedUpdate();
    var anchor=ball.LastDryPosition;Check(Vector3.Distance(anchor,second)<.15f&&Vector3.Distance(anchor,ball.LastShotPosition)>1,"LATEST_DRY_CONTACT_UPDATES_"+water.name);
    uint recovery=ball.WaterRecoverySequence,reset=ball.ResetSequence;int strokes=match.State.Players.Sum(p=>p.TotalStroke);
    var centre=match.transform.TransformPoint(water.centre);
    ball.Place(centre+Vector3.up*2,false);ball.Body.linearVelocity=Vector3.down*6;
    yield return WaitRecovery(ball,recovery);
    Check(Vector3.Distance(ball.Body.position,anchor)<.15f,"FALL_RETURNS_TO_LATEST_DRY_"+water.name);
    Check(ball.ResetSequence>=reset+2&&!ball.FollowingShot,"RESET_CLEARS_SHOT_AND_SYNCS_"+water.name);
    Check(ball.Body.linearVelocity.magnitude<.4f&&ball.Body.angularVelocity.magnitude<1,"RECOVERY_CLEARS_MOTION_"+water.name);
    yield return new WaitForSeconds(.2f);Check(ball.WaterRecoverySequence==recovery+1,"NO_REPEATED_RECOVERY_"+water.name);
    Check(match.State.Players.Sum(p=>p.TotalStroke)==strokes,"NO_ADDITIONAL_STROKE_OR_PROGRESS_CHANGE_"+water.name);
    recovery=ball.WaterRecoverySequence;anchor=ball.LastDryPosition;
    var origin=centre+Vector3.up*2;ball.Place(origin,false);
    var plan=new GolfShotPlan();plan.Build(origin,Vector3.zero,0,GolfShotMode.Putt,match.Course,ball.PhysicsSettings);ball.Strike(plan);
    yield return WaitRecovery(ball,recovery);
    Check(!ball.FollowingShot&&Vector3.Distance(ball.Body.position,anchor)<.15f,"GUIDED_DROP_RECOVERS_BEFORE_TERRAIN_"+water.name);
    Check(water.Touches(water.centre+Vector3.left*(water.radii.x+2),water.centre+Vector3.right*(water.radii.x+2),GolfBall.Radius),"SWEPT_FAST_SIDE_CONTACT_"+water.name);
    Check(water.Touches(water.centre+Vector3.up*3,water.centre+Vector3.down*3,GolfBall.Radius),"SWEPT_FAST_FALL_CONTACT_"+water.name);
    Check(!water.Touches(water.centre+Vector3.up*2+Vector3.left*15,water.centre+Vector3.up*2+Vector3.right*15,GolfBall.Radius),"FLYOVER_DOES_NOT_TOUCH_"+water.name);
    Check(!match.Course.SafeDryPosition(centre),"WATER_NEVER_BECOMES_DRY_ANCHOR_"+water.name);
   }
   Destroy(obj);
  }
  IEnumerator Network(){
   var water=match.Course.waterPatches[0];var dry=Dry(water);
   if(match.Authority){
    float until=Time.time+25;while(Athlete.Active.Count(a=>a&&a.GetComponent<NetworkAthlete>()&&a.GetComponent<NetworkAthlete>().IsSpawned)<2&&Time.time<until)yield return null;
    Check(match.StartMatch()&&match.State.Players.Count==2,"HOST_STARTS_TWO_PLAYER_WATER_FIXTURE");
    var guest=match.State.Players.First(p=>p.PlayerId!=0);var ball=match.Ball(guest.PlayerId);var hostBall=match.Ball(0);var hostPosition=hostBall.Body.position;
    ball.Place(dry,true);yield return new WaitForSeconds(1);
    uint recovery=ball.WaterRecoverySequence;var centre=match.transform.TransformPoint(water.centre);
    ball.Place(centre+Vector3.up*2,false);ball.Body.linearVelocity=Vector3.down*6;yield return WaitRecovery(ball,recovery);
    yield return new WaitForSeconds(2);
    Check(Vector3.Distance(ball.Body.position,dry)<.15f,"HOST_RECOVERS_GUEST_OWNED_BALL");
    Check(guest.TotalStroke==0&&guest.CurrentHole==1,"HOST_WATER_PRESERVES_GUEST_SCORING");
    Check(Vector3.Distance(hostBall.Body.position,hostPosition)<.15f,"OTHER_OWNER_BALL_UNCHANGED");
   }else{
    Check(!match.StartMatch(),"CLIENT_CANNOT_AUTHOR_MATCH");float until=Time.time+25;while((!match.State.Running||match.State.Players.Count!=2||!match.Ball(AppRoot.Instance.LocalAthlete))&&Time.time<until)yield return null;
    var ball=match.Ball(AppRoot.Instance.LocalAthlete);Check(ball,"CLIENT_RECEIVES_OWN_BALL");
    bool sawAir=false,sawDry=false;uint airReset=0;until=Time.time+15;
    while(Time.time<until){
     var local=match.transform.InverseTransformPoint(ball.Body.position);
     if(water.Contains(local.x,local.z)&&local.y>water.centre.y+.15f){sawAir=true;airReset=ball.ResetSequence;}
     if(sawAir&&ball.ResetSequence!=airReset&&Vector3.Distance(ball.Body.position,dry)<.2f){sawDry=true;break;}
     yield return null;
    }
    Check(sawAir&&sawDry,"CLIENT_SEES_AUTHORITATIVE_FALL_AND_DRY_RESET");
    Check(ball.Body.isKinematic&&ball.WaterRecoverySequence==0,"CLIENT_PRESENTS_WITHOUT_LOCAL_WATER_SIMULATION");
    Check(match.Player(AppRoot.Instance.LocalAthlete).TotalStroke==0&&match.Player(AppRoot.Instance.LocalAthlete).CurrentHole==1,"CLIENT_SCORING_REMAINS_UNCHANGED");
   }
  }
 }
}
#endif
