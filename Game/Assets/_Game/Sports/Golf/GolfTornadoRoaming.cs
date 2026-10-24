using System.Collections.Generic;
using UnityEngine;
namespace WhatTheFish {
 public sealed partial class GolfHazardWorld {
  readonly List<Lane> tornadoes=new();readonly HashSet<ulong> stormOwners=new();double nextStormPublish;
  static readonly int Foliage0=Shader.PropertyToID("_GolfTornado0"),Foliage1=Shader.PropertyToID("_GolfTornado1");
  Lane Storm(ulong owner){foreach(var lane in tornadoes)if(lane.storm.owner==owner)return lane;return null;}
  static float Random(ref GolfTornadoSnapshot s){uint x=s.random;if(x==0)x=0x6d2b79f5;x^=x<<13;x^=x>>17;x^=x<<5;s.random=x;return (x&0xffffff)/16777216f;}
  bool StormPoint(Vector3 centre,ref GolfTornadoSnapshot s,bool spawn,out Vector3 point){
   for(int i=0;i<24;i++){
    float angle=Random(ref s)*Mathf.PI*2,r=spawn?GolfHazardRules.TornadoRoamRadius*(.6f+Random(ref s)*.3f):4+Mathf.Sqrt(Random(ref s))*(GolfHazardRules.TornadoRoamRadius-5);
    var p=centre+match.Course.transform.right*(Mathf.Cos(angle)*r)+match.Course.transform.forward*(Mathf.Sin(angle)*r);
    if(ClearCup(p)&&Ground(p,out point)&&Vector3.ProjectOnPlane(point-centre,Vector3.up).sqrMagnitude<=GolfHazardRules.TornadoRoamRadius*GolfHazardRules.TornadoRoamRadius)return true;
   }point=s.position;return false;
  }
  void UpdateTornadoes(GolfEventSnapshot e){
   bool visible=e.kind==GolfEventKind.Tornado&&(match.Events.WarningActive(match.Now)||match.Events.Active(match.Now)==GolfEventKind.Tornado);
   var host=NetworkAthlete.HostPlayer;
   if(!visible){ClearTornadoes();if(host&&host.IsServer&&host.GolfTornadoes.Count>0)host.GolfTornadoes.Clear();return;}
   stormOwners.Clear();double now=match.Now;float dt=Time.fixedDeltaTime;
   foreach(var player in match.State.Players){
    var ball=match.Ball(player.PlayerId);if(player.IsFinished||player.IsDNF||!ball||!ball.Live||ball.Sinking)continue;
    ulong owner=player.PlayerId;stormOwners.Add(owner);var lane=Storm(owner);
    if(lane==null){
     lane=new Lane{id=1000+(int)(owner%100000),hole=player.CurrentHole,forward=match.Course.transform.forward};
     lane.storm=new GolfTornadoSnapshot{owner=owner,round=match.Round,sequence=e.sequence,random=unchecked(e.seed+e.sequence*104729+(uint)owner*7919+(uint)(owner>>32)*3989),hole=player.CurrentHole};
     // Existing host-migration field preserves the random generator, waypoint,
     // velocity and sand history instead of re-drawing a path on a new host.
     if(host)foreach(var saved in host.GolfTornadoes)if(saved.owner==owner&&saved.round==match.Round&&saved.sequence==e.sequence){lane.storm=saved;break;}
     tornadoes.Add(lane);
    }
    var s=lane.storm;lane.hole=player.CurrentHole;
    var centre=ball.Body.position;centre.y=match.Course.transform.position.y;
    bool relocate=s.revision==0||s.sequence!=e.sequence||s.ballReset!=ball.ResetSequence||Vector3.ProjectOnPlane(s.position-centre,Vector3.up).sqrMagnitude>GolfHazardRules.TornadoRoamRadius*GolfHazardRules.TornadoRoamRadius;
    if(relocate){
     s.sequence=e.sequence;s.round=match.Round;s.hole=player.CurrentHole;s.ballReset=ball.ResetSequence;
     if(!StormPoint(centre,ref s,true,out var spawn)){if(lane.visual)lane.visual.Hide();continue;}
     s.position=spawn;s.goal=spawn;s.velocity=Vector3.zero;s.revision++;s.appearAt=System.Math.Max(now,e.start);s.steerAt=now;s.sandTrail=0;s.time=now;
    }
    if(now>=e.start){
     if(now>=s.steerAt||Vector3.ProjectOnPlane(s.goal-s.position,Vector3.up).sqrMagnitude<2.25f){StormPoint(centre,ref s,false,out s.goal);s.steerAt=now+1.8+Random(ref s)*2.2;}
     float growth=GolfHazardRules.TornadoGrowth(now-e.start);
     var delta=Vector3.ProjectOnPlane(s.goal-s.position,Vector3.up).normalized;
     var local=match.Course.transform.InverseTransformDirection(delta);
     var desired=match.Course.transform.TransformDirection(new Vector3(local.x*6*growth,0,local.z*5*growth));
     s.velocity=Vector3.MoveTowards(s.velocity,desired,4*growth*dt);
     // Per-axis acceleration limits, not instantaneous position interpolation.
     var velocity=match.Course.transform.InverseTransformDirection(s.velocity);velocity.x=Mathf.Clamp(velocity.x,-6*growth,6*growth);velocity.z=Mathf.Clamp(velocity.z,-5*growth,5*growth);velocity.y=0;s.velocity=match.Course.transform.TransformDirection(velocity);
     var next=s.position+s.velocity*dt;
     if(Vector3.ProjectOnPlane(next-centre,Vector3.up).sqrMagnitude<=GolfHazardRules.TornadoRoamRadius*GolfHazardRules.TornadoRoamRadius&&ClearCup(next)&&Ground(next,out var ground))s.position=ground;
     else{s.velocity=Vector3.zero;s.steerAt=now;}
    }
    int samples=Mathf.Clamp((int)(System.Math.Floor(now*10)-System.Math.Floor(s.time*10)),0,64);
    if(samples>0){bool sand=now>=e.start&&match.Course.SandAmount(s.position)>.45f;for(int i=0;i<samples;i++)s.sandTrail=(s.sandTrail<<1)|(sand?1UL:0UL);s.time=now;}
    s.hole=player.CurrentHole;lane.storm=s;lane.centre=centre;
   }
   for(int i=tornadoes.Count-1;i>=0;i--)if(!stormOwners.Contains(tornadoes[i].storm.owner)){ReleaseLane(tornadoes[i]);if(tornadoes[i].visual)Destroy(tornadoes[i].visual.gameObject);tornadoes.RemoveAt(i);}
   if(host&&host.IsServer&&now>=nextStormPublish){nextStormPublish=now+.1;while(host.GolfTornadoes.Count>tornadoes.Count)host.GolfTornadoes.RemoveAt(host.GolfTornadoes.Count-1);for(int i=0;i<tornadoes.Count;i++){if(i>=host.GolfTornadoes.Count)host.GolfTornadoes.Add(tornadoes[i].storm);else host.GolfTornadoes[i]=tornadoes[i].storm;}}
  }
  void ReceiveTornadoes(GolfEventSnapshot e){
   var host=NetworkAthlete.HostPlayer;if(!host)return;stormOwners.Clear();
   foreach(var s in host.GolfTornadoes){if(s.round!=match.Round||s.sequence!=e.sequence||s.revision==0)continue;stormOwners.Add(s.owner);var lane=Storm(s.owner);if(lane==null){lane=new Lane{id=1000+(int)(s.owner%100000),forward=match.Course.transform.forward};tornadoes.Add(lane);}lane.storm=s;lane.hole=s.hole;}
   for(int i=tornadoes.Count-1;i>=0;i--)if(!stormOwners.Contains(tornadoes[i].storm.owner)){ReleaseLane(tornadoes[i]);if(tornadoes[i].visual)Destroy(tornadoes[i].visual.gameObject);tornadoes.RemoveAt(i);}
  }
  Vector3 RenderTornado(Lane lane,out float opacity){
   var s=lane.storm;
   if(lane.renderRevision!=s.revision){bool previous=lane.renderRevision!=0;lane.relocateFrom=lane.renderPosition;lane.renderRevision=s.revision;lane.relocateAt=Time.unscaledTime-(previous?0:Mathf.Clamp((float)(match.Now-s.appearAt)+.2f,.2f,1));lane.renderVelocity=Vector3.zero;}
   float age=Time.unscaledTime-lane.relocateAt;
   if(age<.2f){opacity=1-age/.2f;return lane.relocateFrom;}
   if(age<.2f+Time.unscaledDeltaTime*1.5f)lane.renderPosition=s.position;
   var target=s.position+s.velocity*Mathf.Clamp((float)(match.Now-s.time),0,.10f);
   if(lane.renderPosition==Vector3.zero)lane.renderPosition=s.position;
   lane.renderPosition=Vector3.SmoothDamp(lane.renderPosition,target,ref lane.renderVelocity,.10f,float.PositiveInfinity,Time.unscaledDeltaTime);
   opacity=Mathf.SmoothStep(0,1,Mathf.Clamp01((age-.2f)/.5f));
   if(match.Now<match.Events.Snapshot.start)opacity=1;
   return lane.renderPosition;
  }
  public bool TornadoVisualsActive=>match&&match.Context&&match.State.Running&&match.Events.Snapshot.kind==GolfEventKind.Tornado&&(match.Events.WarningActive(match.Now)||match.Events.Active(match.Now)==GolfEventKind.Tornado);
  public float NearestTornado(Vector3 point){float best=float.PositiveInfinity;if(!TornadoVisualsActive)return best;foreach(var lane in tornadoes)if(lane.storm.revision!=0&&lane.renderRevision!=0)best=Mathf.Min(best,Vector3.ProjectOnPlane(point-lane.renderPosition,Vector3.up).magnitude);return best;}
  public float TornadoViewIntensity(Vector3 point){
   if(!TornadoVisualsActive||match.Events.WarningActive(match.Now))return 0;
   float strongest=0;
   foreach(var lane in tornadoes){if(lane.storm.revision==0||lane.renderRevision==0)continue;float distance=Vector3.ProjectOnPlane(point-lane.renderPosition,Vector3.up).magnitude;strongest=Mathf.Max(strongest,Mathf.SmoothStep(0,1,Mathf.InverseLerp(14,2,distance))*lane.renderOpacity);}
   return strongest;
  }
  void UpdateTornadoFoliage(GolfEventSnapshot e,Camera camera){
   ClearFoliage();if(!camera||e.kind!=GolfEventKind.Tornado||match.Now<e.start||MenuPreferences.ReducedMotion)return;
   Lane first=null,second=null;float a=float.PositiveInfinity,b=float.PositiveInfinity;
   foreach(var lane in tornadoes){if(lane.storm.revision==0)continue;float d=(lane.renderPosition-camera.transform.position).sqrMagnitude;if(d<a){second=first;b=a;first=lane;a=d;}else if(d<b){second=lane;b=d;}}
   float growth=GolfHazardRules.TornadoGrowth(match.Now-e.start);
   if(first!=null)Shader.SetGlobalVector(Foliage0,new Vector4(first.renderPosition.x,first.renderPosition.z,10*growth,1));
   if(second!=null)Shader.SetGlobalVector(Foliage1,new Vector4(second.renderPosition.x,second.renderPosition.z,10*growth,1));
  }
  static void ClearFoliage(){Shader.SetGlobalVector(Foliage0,Vector4.zero);Shader.SetGlobalVector(Foliage1,Vector4.zero);}
  void ReleaseLane(Lane lane){if(!match||!match.Authority)return;for(int i=carries.Count-1;i>=0;i--)if(carries[i].lane==lane){if(carries[i].ball)carries[i].ball.ReleaseHazard(Vector3.zero);carries.RemoveAt(i);}}
  void ClearTornadoes(){if(match&&match.Authority)ReleaseAll();foreach(var lane in tornadoes)if(lane.visual)Destroy(lane.visual.gameObject);tornadoes.Clear();nextStormPublish=0;ClearFoliage();}
 }
}
