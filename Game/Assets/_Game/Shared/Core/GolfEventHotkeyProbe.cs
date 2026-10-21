#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Collections;
using System.Linq;
using UnityEngine;
namespace WhatTheFish {
 public sealed partial class GolfPolishProbe {
  IEnumerator HotkeyPresentation(){
   var app=AppRoot.Instance;var local=app.LocalAthlete;var helper=Instantiate(app.athletePrefab).GetComponent<Athlete>();helper.Setup();ulong id=GolfCartWorld.Key(local);bool ready=false;float until=Time.time+18;while(!ready&&Time.time<until){ready=match.StartTestMatch(new[]{(id,local,"You"),(101UL,helper,"Other")});if(!ready)yield return null;}Check(ready,"EVENT_V2_MATCH_READY");yield return new WaitForSeconds(.8f);
   Check(!match.ReviewEventHotkey(KeyCode.F5),"F5_SWAP_REMOVED");var ball=match.Ball(local);var home=ball.Body.position;
   var floor=GameObject.CreatePrimitive(PrimitiveType.Cube);floor.layer=8;floor.name="Event response fixture";floor.transform.position=home+Vector3.up*35;floor.transform.localScale=new Vector3(180,.2f,180);Physics.SyncTransforms();var origin=floor.transform.position+Vector3.up*(.102f+GolfBall.Radius);
   var normal=new GolfShotPlan();normal.Build(origin,Vector3.forward,.35f,GolfShotMode.Swing,match.Course,ball.PhysicsSettings);
   foreach(var kind in GolfEventSchedule.RandomKinds){
    match.ReviewEventHotkey((KeyCode)((int)KeyCode.F1+(int)kind-1));var conditions=match.ShotConditions(GolfShotMode.Swing);var plan=new GolfShotPlan();plan.Build(origin,Vector3.forward,.35f,GolfShotMode.Swing,match.Course,ball.PhysicsSettings,conditions);
    if(kind==GolfEventKind.TropicalWind)Check(plan.EndPoint.x>normal.EndPoint.x+2,"WIND_PREVIEW_DEFLECTS_BY_METRES");
    if(kind==GolfEventKind.StickyGreen)Check(plan.Resting&&plan.Velocities[plan.Count-1]==Vector3.zero,"STICKY_PREVIEW_ENDS_AT_STOP");
    if(kind==GolfEventKind.StickyGreen||kind==GolfEventKind.BounceParty){var a=new GolfShotPlan();var b=new GolfShotPlan();a.Build(origin,Vector3.forward,.15f,GolfShotMode.Putt,match.Course,ball.PhysicsSettings);b.Build(origin,Vector3.forward,.15f,GolfShotMode.Putt,match.Course,ball.PhysicsSettings,conditions);Check(a.EndPoint==b.EndPoint&&a.Duration==b.Duration,"PUTT_PLAN_IDENTICAL_"+kind);}
    uint contact=ball.GuideContactSequence;ball.Place(origin,true);ball.Strike(plan);until=Time.time+8;while(ball.GuideContactSequence==contact&&Time.time<until)yield return new WaitForFixedUpdate();Check(ball.GuideContactSequence>contact&&Vector3.Distance(ball.LastGuideContact,plan.EndPoint)<.02f,"ACTUAL_CONTACT_MATCHES_PREVIEW_"+kind);
    if(kind==GolfEventKind.StickyGreen)Check(ball.Motion==GolfBallMotion.Resting&&ball.Body.linearVelocity==Vector3.zero&&ball.Body.angularVelocity==Vector3.zero,"STICKY_AUTHORITY_STOPS_WITHOUT_ROLLING");
   }
   match.ReviewEvent(GolfEventKind.StickyGreen);var airborne=origin+Vector3.up*3;var interrupted=new GolfShotPlan();interrupted.Build(airborne,Vector3.forward,.1f,GolfShotMode.Swing,match.Course,ball.PhysicsSettings,match.ShotConditions(GolfShotMode.Swing));ball.Place(airborne,true);ball.Strike(interrupted);ball.ApplyGameplayImpulse(Vector3.zero);until=Time.time+6;while(ball.Motion!=GolfBallMotion.Resting&&Time.time<until)yield return new WaitForFixedUpdate();Check(ball.Motion==GolfBallMotion.Resting&&ball.Body.linearVelocity==Vector3.zero,"STICKY_ALSO_STOPS_INTERRUPTED_PHYSX_FLIGHT");
   Destroy(floor);ball.Place(home,true);yield return new WaitForFixedUpdate();match.ReviewEvent(GolfEventKind.TropicalWind);yield return new WaitForSeconds(.8f);
   var wind=match.GetComponent<GolfWindWorld>();Check(wind&&wind.Strength>.99f&&wind.FlagCount==5,"WIND_WORLD_AND_FIVE_CLOTHS_ACTIVE");var global=Shader.GetGlobalVector("_GolfWind");Check(Mathf.Abs(global.x-1)<.001f&&Mathf.Abs(global.y)<.001f&&global.z>.99f,"WORLD_WIND_FOLLOWS_REPLICATED_DIRECTION");
   var cloth=match.transform.parent.GetComponentsInChildren<Transform>().First(t=>t.name=="Aqua pennant");Check(Vector3.Dot(cloth.right,Vector3.right)>.99f,"FLAG_CLOTH_POINTS_DOWNWIND");
   CaptureWorld("wind-flag",cloth.position+Vector3.up*1.8f);yield return new WaitForSeconds(.4f);CaptureWorld("wind-flag-later",cloth.position+Vector3.up*1.8f);
   var sand=match.Course.sandPatches[0];var focus=match.transform.TransformPoint(sand.centre);if(Physics.Raycast(focus+Vector3.up*30,Vector3.down,out var hit,60,1<<8))focus=hit.point;TeePlace(local,focus);yield return new WaitForSeconds(1.5f);Check(wind.TotalSandParticles>0,"SAND_GRAINS_LIFT_AND_DRIFT_DOWNWIND");var ps=wind.transform.Find("Windblown bunker sand").GetComponent<ParticleSystem>();var grains=new ParticleSystem.Particle[384];int n=ps.GetParticles(grains);Check(n>0&&grains.Take(n).All(p=>Vector3.Dot(p.velocity,wind.Direction)>0),"SAND_PARTICLES_ACTUALLY_SIMULATE_DOWNWIND");var sample=grains.Take(n).OrderByDescending(p=>p.remainingLifetime).First();yield return new WaitForSeconds(.15f);n=ps.GetParticles(grains);var moved=grains.Take(n).FirstOrDefault(p=>p.randomSeed==sample.randomSeed);Check(moved.remainingLifetime>0&&Vector3.Dot(moved.position-sample.position,wind.Direction)>.1f,"SAND_GRAINS_MOVE_THROUGH_AIR");CaptureWorld("wind-sand",focus+Vector3.up*.35f);
   Check(!GolfHUD.Instance.GetComponentsInChildren<Transform>(true).First(t=>t.name=="Golf framed control hint").gameObject.activeSelf,"BOTTOM_CONTROL_HINT_REMOVED");
   var actorHome=local.transform.position;for(int h=1;h<=5;h++){match.EnterHole(match.Ball(helper),h);yield return new WaitForSeconds(.15f);}yield return new WaitForSeconds(1.1f);Check(match.Events.RushActive(match.Now)&&match.EventMoveMultiplier(local)==2.1f&&match.EventCartMultiplier==1.9f,"RUSH_110_PERCENT_PLAYER_90_PERCENT_CART");Check(match.Events.Active(match.Now)==GolfEventKind.TropicalWind&&match.Events.Snapshot.nextCycle==0,"ACTIVE_WIND_CONTINUES_WITH_RUSH_NO_FUTURE_CYCLE");Check(Vector3.Distance(actorHome,local.transform.position)<.2f&&match.Player(local).TotalStroke==0,"NO_SWAP_OR_SCORE_CHANGE");
   match.ClearMatch();Destroy(helper.gameObject);yield return EventBadgePresentation();
  }
 }
}
#endif
