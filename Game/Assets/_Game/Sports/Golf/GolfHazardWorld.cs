using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace WhatTheFish {
 // Visual lanes are reconstructed from the host clock. Only the authority
 // applies forces; existing ball snapshots carry movement and the hold lock.
 [DefaultExecutionOrder(80)]public sealed class GolfHazardWorld:MonoBehaviour {
  public sealed class Lane {public int id,hole;public Vector3 centre,forward;public GolfHazardVisual visual;public Vector3 chase;public double chaseAt;public uint chaseSequence;}
  sealed class Carry {public GolfBall ball;public uint reset;public double at;public Vector3 from,to;public Lane lane;}
  readonly List<Lane> lanes=new();readonly List<Carry> carries=new();readonly Dictionary<ulong,double> bumped=new();
  GolfMatchManager match;uint round;bool built;public int TornadoTosses {get;private set;}public int CrabBumps {get;private set;}
  public IReadOnlyList<Lane> Lanes=>lanes;
  void Awake(){match=GetComponent<GolfMatchManager>();}
  bool Ground(Vector3 p,out Vector3 ground){
   ground=p;if(!match.Course.SafeDryPosition(p,1)||!Physics.Raycast(p+Vector3.up*25,Vector3.down,out var hit,60,1<<8,QueryTriggerInteraction.Ignore)||hit.normal.y<.55f)return false;
   ground=hit.point;return !match.OutsideCourse(ground);
  }
  void Build(){
   Clear();round=match.Round;built=true;
   foreach(var hole in match.Course.holes.OrderBy(h=>h.number)){
    var path=match.Course.fairways.FirstOrDefault(f=>f.hole==hole.number).points;
    for(int site=0;site<3;site++){
     float t=.18f+site*.31f;Vector3 centre,forward;
     if(path!=null&&path.Length>1){float value=t*(path.Length-1);int i=Mathf.Min(path.Length-2,(int)value);centre=match.Course.transform.TransformPoint(Vector3.Lerp(path[i],path[i+1],value-i));forward=match.Course.transform.TransformDirection(path[i+1]-path[i]);}
     else{centre=Vector3.Lerp(hole.tee.position,hole.cup.position,t);forward=hole.cup.position-hole.tee.position;}
     forward=Vector3.ProjectOnPlane(forward,Vector3.up).normalized;if(forward.sqrMagnitude<.5f)forward=Vector3.forward;
     if(Ground(centre,out centre))lanes.Add(new Lane{id=hole.number*3+site,hole=hole.number,centre=centre,forward=forward});
    }
   }
  }
  public Vector3 Position(Lane lane,GolfEventSnapshot e,double now){
   var patrol=lane.centre+GolfHazardRules.Offset(e.kind,e,lane.id,now,lane.forward);
   if(e.kind!=GolfEventKind.CrabCrossing)return patrol;
   if(lane.chaseSequence!=e.sequence){lane.chase=patrol;lane.chaseAt=now;lane.chaseSequence=e.sequence;}
   var target=match.State.Players.Where(p=>!p.IsFinished&&!p.IsDNF&&p.CurrentHole==lane.hole)
    .OrderBy(p=>p.PlayerId).Select(p=>match.Ball(p.PlayerId)).Where(b=>b&&b.Live&&!b.Sinking&&!b.HazardHeld&&ClearCup(b.transform.position)&&Vector3.ProjectOnPlane(b.transform.position-lane.centre,Vector3.up).sqrMagnitude<16*16)
    .OrderBy(b=>(b.transform.position-lane.chase).sqrMagnitude).FirstOrDefault();
   var desired=target?target.transform.position+new Vector3(Mathf.Cos(lane.id*2.399963f),0,Mathf.Sin(lane.id*2.399963f))*.6f:patrol;desired.y=lane.centre.y;
   var next=Vector3.MoveTowards(lane.chase,desired,3.8f*Mathf.Clamp((float)(now-lane.chaseAt),0,.1f));
   if(ClearCup(next)&&Ground(next,out var dry))lane.chase=dry;lane.chaseAt=now;return lane.chase;
  }
  bool ClearCup(Vector3 p)=>match.Course.holes.All(h=>Vector3.ProjectOnPlane(p-h.cup.position,Vector3.up).sqrMagnitude>GolfHazardRules.CupClearance*GolfHazardRules.CupClearance);
  bool Landing(GolfBall ball,Lane lane,Vector3 from,GolfEventSnapshot e,out Vector3 to){
   float distance=GolfHazardRules.TossDistance(e,ball.Owner);float phase=GolfHazardRules.Phase(e,lane.id);
   // Prefer a long throw, but a narrow fairway must not make a visibly
   // touching tornado ignore the ball. Shorten only if no safe long arc fits.
   foreach(float reach in new[]{distance,distance*.85f,10f,8f})for(int i=0;i<18;i++){
    var direction=Quaternion.AngleAxis(phase*Mathf.Rad2Deg+i*20,Vector3.up)*lane.forward;
    if(Ground(from+direction*reach,out to)&&ClearCup(to))return true;
   }return Ground(from,out to)&&ClearCup(to);
  }
  void FixedUpdate(){
   if(!match||!match.Context||!match.State.Running){ReleaseAll();return;}
   if(round!=match.Round||!built)Build();var e=match.Events.Snapshot;
   if(!match.Authority)return;
   bool active=GolfHazardRules.IsHazard(e.kind)&&match.Events.Active(match.Now)==e.kind&&!match.Starting;
   if(!active){ReleaseAll();return;}
   for(int i=carries.Count-1;i>=0;i--){var carry=carries[i];var ball=carry.ball;
    if(!ball||!ball.Live||ball.ResetSequence!=carry.reset||!ball.HazardHeld){if(ball)ball.ReleaseHazard(Vector3.zero);carries.RemoveAt(i);continue;}
    float t=Mathf.Clamp01((float)((match.Now-carry.at)/GolfHazardRules.CarrySeconds));var centre=Position(carry.lane,e,match.Now);Ground(centre,out centre);float angle=t*Mathf.PI*3;
    var orbit=centre+new Vector3(Mathf.Cos(angle),0,Mathf.Sin(angle))*(.7f*(1-t))+Vector3.up*(GolfBall.Radius+.2f+3*t);
    ball.HoldForHazard(Vector3.Lerp(carry.from,orbit,Mathf.SmoothStep(0,1,t*3)));
    if(t>=1){ball.ReleaseHazard(GolfHazardRules.TossVelocity(ball.Body.position,carry.to+Vector3.up*GolfBall.Radius));TornadoTosses++;carries.RemoveAt(i);}
   }
   ulong stamp=GolfHazardRules.Stamp(match.Round,e.sequence);
   foreach(var player in match.State.Players){
    var ball=match.Ball(player.PlayerId);if(player.IsFinished||player.IsDNF||!ball||!ball.Live||ball.Sinking||ball.HazardHeld||!ClearCup(ball.Body.position))continue;
    foreach(var lane in lanes){
     if(lane.hole!=player.CurrentHole)continue;var p=Position(lane,e,match.Now);float radius=e.kind==GolfEventKind.Tornado?GolfHazardRules.TornadoRadius:GolfHazardRules.CrabRadius+GolfBall.Radius;
     var delta=Vector3.ProjectOnPlane(ball.Body.position-p,Vector3.up);if(delta.sqrMagnitude>radius*radius||!Ground(p,out p)||ball.Body.position.y-p.y>1.2f||ball.Body.position.y<p.y-.25f)continue;
     if(e.kind==GolfEventKind.Tornado){
      if(ball.TornadoStamp==stamp||!Landing(ball,lane,ball.Body.position,e,out var to)||!ball.BeginHazardHold(stamp))continue;
      carries.Add(new Carry{ball=ball,reset=ball.ResetSequence,at=match.Now,from=ball.Body.position,to=to,lane=lane});GolfFeedback.Emit(GolfFeedbackCue.Whoosh,p,Vector3.up,.6f,ball.Owner,ball.Owner);break;
     }
     if(ball.Body.position.y-p.y>.35f||bumped.TryGetValue(ball.Owner,out double at)&&match.Now-at<3.5)continue;
     var direction=delta.sqrMagnitude>.001f?delta.normalized:Vector3.Cross(Vector3.up,lane.forward);var impulse=direction*4.8f+Vector3.up*2.6f;
     if(ball.NudgeByHazard(impulse)){bumped[ball.Owner]=match.Now;CrabBumps++;GolfFeedback.Emit(GolfFeedbackCue.Ball,ball.Body.position,Vector3.up,.6f,ball.Owner,ball.Owner);lane.visual?.Bump();}break;
    }
   }
  }
  void LateUpdate(){
   if(!match||!match.Context||!match.State.Running){Hide();return;}
   if(round!=match.Round||!built)Build();var e=match.Events.Snapshot;bool warning=match.Events.WarningActive(match.Now);bool visible=GolfHazardRules.IsHazard(e.kind)&&(warning||match.Events.Active(match.Now)==e.kind);
   if(!visible){Hide();return;}var camera=Camera.main;
   foreach(var lane in lanes){var p=Position(lane,e,match.Now);bool near=camera&&Vector3.ProjectOnPlane(p-camera.transform.position,Vector3.up).sqrMagnitude<70*70&&ClearCup(p)&&Ground(p,out p);
    if(!near){lane.visual?.Hide();continue;}if(!lane.visual)lane.visual=GolfHazardVisual.Create(transform,lane.id);var heading=e.kind==GolfEventKind.CrabCrossing?Vector3.ProjectOnPlane(p-lane.centre,Vector3.up):lane.forward;if(heading.sqrMagnitude<.01f)heading=lane.forward;lane.visual.Tick(e.kind,p,heading,match.Now-e.start,warning);
   }
  }
  void ReleaseAll(){foreach(var c in carries)if(c.ball)c.ball.ReleaseHazard(Vector3.zero);carries.Clear();}
  void Hide(){foreach(var lane in lanes)lane.visual?.Hide();}
  void Clear(){ReleaseAll();foreach(var lane in lanes)if(lane.visual)Destroy(lane.visual.gameObject);lanes.Clear();bumped.Clear();}
  void OnDisable(){ReleaseAll();Hide();}
  void OnDestroy(){Clear();}
 }
}
