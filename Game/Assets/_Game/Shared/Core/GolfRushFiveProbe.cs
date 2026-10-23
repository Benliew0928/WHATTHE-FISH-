#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
namespace WhatTheFish {
 public sealed partial class GolfPolishProbe {
  IEnumerator FiveTeePresentation(){
   var local=AppRoot.Instance.LocalAthlete;var helpers=Enumerable.Range(0,4).Select(i=>{var a=Instantiate(AppRoot.Instance.athletePrefab).GetComponent<Athlete>();a.Setup();return a;}).ToArray();var actors=new[]{local}.Concat(helpers).ToArray();var ids=new[]{GolfCartWorld.Key(local),101UL,102UL,103UL,104UL};
   bool ready=false;float until=Time.time+18;while(!ready&&Time.time<until){ready=match.StartTestMatch(actors.Select((a,i)=>(ids[i],a,"Tee "+i)).ToArray());if(!ready)yield return null;}Check(ready,"FIVE_PLAYER_MATCH_READY");yield return new WaitForSeconds(.5f);
   for(int h=1;h<5;h++){
    foreach(var id in ids){var ball=match.Ball(id);ball.Place(match.Course.holes[h-1].cup.position+Vector3.up*GolfBall.Radius,true);Check(match.EnterHole(ball,h)==GolfHoleResult.Completed,"PROGRESS_H"+h+"_"+id);}yield return new WaitForSeconds(.25f);
    var site=match.Course.teeSites.Single(s=>s.hole==h+1);Check(site.count==5,"FIVE_PADS_T"+(h+1));Check(ids.Select(id=>match.AssignedTee(h+1,id)).OrderBy(i=>i).SequenceEqual(new[]{0,1,2,3,4}),"FIVE_DISTINCT_ASSIGNMENTS_T"+(h+1));
    Check(Enumerable.Range(0,5).All(i=>Mathf.Abs(Vector3.ProjectOnPlane(site.Pad(i)-site.cup,Vector3.up).magnitude-site.radius)<.001f),"EQUAL_CUP_DISTANCE_T"+(h+1));
    foreach(var id in ids)Check(Vector3.ProjectOnPlane(match.Ball(id).Body.position-match.Course.transform.TransformPoint(site.Pad(match.AssignedTee(h+1,id))),Vector3.up).magnitude<.1f,"BALL_AT_ASSIGNED_T"+(h+1)+"_"+id);
   }
   var reservations=new GolfTeeReservations(17);for(ulong i=1;i<=6;i++)reservations.Request(2,i);var assigned=reservations.Assign(h=>5,(h,s)=>s==0?0:(s%2==1?-(s+1)/2:s/2),(h,s)=>true);Check(assigned.Count==5&&assigned[0].slot==0&&assigned.Take(3).All(s=>s.slot<=2)&&reservations.Waiting(6),"CENTRE_THEN_SIDES_THEN_OUTER_QUEUE_SIXTH");
   match.ClearMatch();foreach(var a in helpers)Destroy(a.gameObject);
  }
  IEnumerator RushFivePresentation(){
   yield return FiveTeePresentation();yield return StartMatch();yield return new WaitForSeconds(.7f);var local=AppRoot.Instance.LocalAthlete;
   var settings=Resources.Load<GolfBallPhysicsSettings>("GolfBallPhysics");Check(Mathf.Abs(settings.LandingVelocity(new Vector3(10,-10,0),Vector3.up,true).x-7.2f)<.001f,"NORMAL_FIRST_HORIZONTAL_LOSS_28_PERCENT");
   var state=match.Events.Snapshot;state.kind=GolfEventKind.StickyGreen;state.start=match.Now+10;state.end=state.start+GolfEventSchedule.Duration(state.kind);state.nextCycle=state.start+GolfEventSchedule.Cycle;state.rushAt=0;match.Events.Receive(state);yield return new WaitForSeconds(.2f);yield return new WaitForEndOfFrame();
   var hud=GolfEventHUD.Instance;Check(!hud.WindVisible&&!hud.Card.GetComponentsInChildren<Transform>(true).Any(t=>t.name=="Golf wind direction"),"NO_WIND_DIRECTION_ARROW");
   var visible=hud.Card.GetComponentsInChildren<GolfEventBadgeGraphic>(true).Where(a=>a.gameObject.activeInHierarchy).ToArray();System.IO.File.AppendAllText(report,"BADGE_STATE "+string.Join(";",hud.Card.GetComponentsInChildren<GolfEventBadgeGraphic>(true).Select(a=>a.name+":"+a.Badge+":"+a.gameObject.activeInHierarchy))+"\n");Check(visible.Length==1,"ONLY_WARNING_ROW_VISIBLE");var warning=visible.Single();Check(warning.Badge==5&&warning.Sheet==Resources.Load<Texture2D>("GolfUI/Events/WarningSheet"),"SUPPLIED_WARNING_ARTWORK");yield return new WaitForEndOfFrame();CaptureUI("warning-10",null,0,0,0,false);string before=warning.GetComponentInChildren<Text>().text;yield return new WaitForSeconds(1.1f);Check(before=="10s"&&warning.GetComponentInChildren<Text>().text=="09s","LIVE_TEN_TO_NINE_SECONDS");
   yield return new WaitForSeconds(6.1f);yield return new WaitForEndOfFrame();CaptureUI("warning-03",null,0,0,0,false);yield return new WaitForSeconds(3);Check(hud.Card.GetComponentsInChildren<GolfEventBadgeGraphic>(true).Single(a=>a.gameObject.activeInHierarchy).Badge==2,"WARNING_REPLACED_BY_ACTUAL_EVENT");
   match.ReviewEventHotkey(KeyCode.F6);var at=local.transform.position;var floor=GameObject.CreatePrimitive(PrimitiveType.Cube);floor.layer=8;floor.GetComponent<MeshRenderer>().sharedMaterial=match.transform.parent.GetComponentsInChildren<MeshRenderer>().First(r=>r.name.StartsWith("Terrain__")&&!r.transform.IsChildOf(match.transform)).sharedMaterial;floor.transform.position=at+Vector3.up*30;floor.transform.localScale=new Vector3(100,.2f,100);TeePlace(local,floor.transform.position+Vector3.up*.1f);local.transform.rotation=Quaternion.identity;yield return new WaitForSeconds(.6f);
   var trail=local.GetComponent<GolfRushTrail>();Check(trail&&trail.VisibleVertices==0&&!trail.Emitting,"NO_RUSH_TRAIL_WHILE_IDLE");bool behind=true;float until=Time.time+1.3f;while(Time.time<until){local.Simulate(new PlayerCommand{move=Vector2.up},Time.deltaTime);yield return new WaitForEndOfFrame();behind&=trail.FrontVertexCount==0;}
   Check(trail.Emitting&&trail.VisibleVertices>20&&behind,"RUSH_RUNNING_RIBBONS_BEHIND_ONLY");var camera=Camera.main;var old=camera.transform.rotation;camera.transform.rotation=Quaternion.LookRotation(Vector3.right);CaptureWorld("rush-running-side",local.transform.position+Vector3.back*1.3f);camera.transform.rotation=old;
   local.transform.rotation=Quaternion.Euler(0,180,0);yield return new WaitForEndOfFrame();Check(trail.FrontVertexCount==0,"SHARP_TURN_NEVER_PUTS_TRAIL_IN_FRONT");yield return new WaitForSeconds(.9f);Check(trail.VisibleVertices==0,"RUSH_TRAIL_FADES_AWAY_ON_STOP");
   match.ReviewEventHotkey(KeyCode.F7);until=Time.time+.4f;while(Time.time<until){local.capsule.Move(Vector3.back*Time.deltaTime*9);yield return new WaitForEndOfFrame();}Check(!trail.Emitting&&trail.VisibleVertices==0,"NO_TRAIL_OUTSIDE_RUSH");Destroy(floor);TeePlace(local,at);match.ClearMatch();
  }
 }
}
#endif
