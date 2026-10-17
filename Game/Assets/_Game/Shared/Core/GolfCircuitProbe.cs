#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;

namespace WhatTheFish {
 // Opt-in real Rigidbody route traversal. It uses ordinary guided shots and
 // rolling contacts, with a bounded charge search, not teleporting between shots.
 public sealed class GolfCircuitProbe:MonoBehaviour {
  GolfMatchManager match;GolfBall ball;string report;int checks;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)] static void Boot(){if(Environment.GetCommandLineArgs().Contains("-golfCircuitAudit"))new GameObject("Golf circuit audit").AddComponent<GolfCircuitProbe>();}
  void Check(bool pass,string label){File.AppendAllText(report,(pass?"PASS ":"FAIL ")+label+"\n");if(!pass)throw new Exception(label);checks++;}
  Vector3 Floor(Vector3 local){var terrain=match.GetComponentInParent<RefinedIslandEnvironment>().GetComponentsInChildren<MeshCollider>().Where(c=>c.name.StartsWith("Terrain__"));var ray=new Ray(match.transform.TransformPoint(local+Vector3.up*100),Vector3.down);foreach(var c in terrain)if(c.Raycast(ray,out var hit,200))return hit.point+Vector3.up*(GolfBall.Radius+.002f);foreach(var h in match.Course.holes)if(Vector2.Distance(new Vector2(local.x,local.z),new Vector2(h.cup.localPosition.x,h.cup.localPosition.z))<h.cupRadius)return h.cup.position+Vector3.up*(GolfBall.Radius+.002f);throw new Exception("Missing route ground "+local);}
  IEnumerator Start(){
   var args=Environment.GetCommandLineArgs();report=args[Array.IndexOf(args,"-report")+1];Directory.CreateDirectory(Path.GetDirectoryName(report));
   float deadline=Time.realtimeSinceStartup+60;while((!AppRoot.Instance||!AppRoot.Instance.Exploring||!GolfMatchManager.Instance)&&Time.realtimeSinceStartup<deadline)yield return null;
   match=GolfMatchManager.Instance;Check(match&&match.StartMatch(),"CIRCUIT_AUTHORITY_STARTS");DevelopmentProbe.TurnCommandActive=true;DevelopmentProbe.TurnCommand=default;
   // This is a course-clearance fixture, not a ball-to-ball collision test.
   // Opening match balls occupy the same tee and can transfer the first shot's
   // momentum; isolate them only during this opt-in independent route probe.
   var openingBalls=match.State.Players.Select(p=>match.Ball(p.PlayerId)).Where(b=>b).ToArray();
   foreach(var opening in openingBalls){opening.SetLive(false);opening.GetComponent<Collider>().enabled=false;}
   var node=new GameObject("Route ball fixture");node.transform.SetParent(match.transform,false);ball=node.AddComponent<GolfBall>();ball.Bind(match,999999);ball.SetLive(true);Time.timeScale=5;
   Check(match.Course.fairways.Length==6&&match.Course.fairways.Count(f=>f.alternative)==1&&match.Course.sandPatches.Length==20&&match.Course.roughPatches.Length==20&&match.Course.waterPatches.Length==6,"RUNTIME_RECEIVES_FIVE_ROUTES_GATE_BYPASS_AND_SAND");
   var obstacle=match.Course.transform.Find("Circuit obstacles");Check(obstacle&&obstacle.childCount==225,"RUNTIME_SHARED_KIT_OBSTACLES_LOADED");
   foreach(var route in match.Course.fairways){
    string routeLabel=route.hole+(route.alternative?"_BYPASS":"");
    ball.Place(Floor(route.points[0]),true);yield return new WaitForSeconds(.3f);uint reset=ball.ResetSequence;int strokes=0;
    for(int waypoint=1;waypoint<route.points.Length;waypoint++){
     // This independent fixture has no scoring owner: finish at the green
     // approach rather than entering a cup that only normal match balls score.
     var target=Floor(route.points[waypoint]);if(waypoint==route.points.Length-1)target=Floor(route.points[waypoint]-Vector3.ProjectOnPlane(route.points[waypoint]-route.points[waypoint-1],Vector3.up).normalized*6);
     int attempts=0;
     while(Vector3.ProjectOnPlane(target-ball.Body.position,Vector3.up).magnitude>4&&attempts++<10){
      var origin=ball.Body.position;var direction=Vector3.ProjectOnPlane(target-origin,Vector3.up).normalized;float remaining=Vector3.ProjectOnPlane(target-origin,Vector3.up).magnitude;
      var mode=remaining<12?GolfShotMode.Putt:GolfShotMode.Swing;GolfShotPlan best=null;float error=float.PositiveInfinity,bestCharge=0;
      // Preserve each normal shot model. Approximate subsequent run-out only
      // for choosing a useful test charge, then measure the actual simulation.
      for(int candidate=0;candidate<=40;candidate++){
       float charge=candidate/40f;var shot=new GolfShotPlan();shot.Build(origin,direction,charge,mode,match.Course,ball.PhysicsSettings);
       var predicted=shot.EndPoint;
       if(mode==GolfShotMode.Swing){var v=shot.Velocities[shot.Count-1];var tangent=Vector3.ProjectOnPlane(v,shot.Normal);float speed=tangent.magnitude;float airborne=2*Mathf.Max(0,Vector3.Dot(v,shot.Normal))/9.81f/(1-ball.PhysicsSettings.turfRestitution);float resistance=match.Course.RollingResistance(predicted,ball.PhysicsSettings);predicted+=tangent.normalized*(speed*airborne+speed*speed/(2*(resistance+1)));}
       float miss=Vector3.ProjectOnPlane(predicted-target,Vector3.up).magnitude;if(shot.Normal.y<.35f)miss+=50;
       if(miss<error){error=miss;best=shot;bestCharge=charge;}
      }
      ball.Strike(best);strokes++;float until=Time.time+20;do{yield return new WaitForFixedUpdate();}while(ball.Motion!=GolfBallMotion.Resting&&Time.time<until&&ball.ResetSequence==reset);
      File.AppendAllText(report,$"ROUTE hole={route.hole} waypoint={waypoint} shot={strokes} charge={bestCharge:F3} remaining={remaining:F2} moved={Vector3.Distance(origin,ball.Body.position):F2} error={Vector3.ProjectOnPlane(target-ball.Body.position,Vector3.up).magnitude:F2} state={ball.Motion} position={ball.Body.position}\n");
      Check(ball.ResetSequence==reset&&ball.Motion==GolfBallMotion.Resting,"ROUTE_"+routeLabel+"_SHOT_"+strokes+"_RESTS_WITHOUT_RECOVERY");
      if(Vector3.Distance(origin,ball.Body.position)<.15f)break;
     }
     Check(Vector3.ProjectOnPlane(target-ball.Body.position,Vector3.up).magnitude<=4,"ROUTE_"+routeLabel+"_REACHES_SAFE_WAYPOINT_"+waypoint);
    }
    File.AppendAllText(report,$"ROUTE_COMPLETE hole={routeLabel} strokes={strokes}\n");Check(strokes>=2&&strokes<=20,"ROUTE_"+routeLabel+"_MULTISHOT_PLAYABLE");
   }
   Time.timeScale=1;Destroy(node);foreach(var opening in openingBalls){opening.GetComponent<Collider>().enabled=true;opening.SetLive(true);}File.AppendAllText(report,"GOLF_CIRCUIT_COMPLETE checks="+checks+"\n");
  }
 }
}
#endif
