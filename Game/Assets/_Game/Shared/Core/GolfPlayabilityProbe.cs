#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;

namespace WhatTheFish {
 // Measures actual Rigidbody outcomes on the delivered course, including slopes.
 public sealed class GolfPlayabilityProbe:MonoBehaviour {
  GolfMatchManager match;GolfBall ball;string report;int checks;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)] static void Boot(){if(Environment.GetCommandLineArgs().Contains("-golfPlayabilityAudit"))new GameObject("Golf playability measurements").AddComponent<GolfPlayabilityProbe>();}
  void Check(bool pass,string label){File.AppendAllText(report,(pass?"PASS ":"FAIL ")+label+"\n");if(!pass)throw new Exception(label);checks++;}
  Vector3 Ground(Vector3 p){Check(Physics.Raycast(p+Vector3.up*30,Vector3.down,out var hit,80,1<<8,QueryTriggerInteraction.Ignore),"SHOT_SUPPORTED");return hit.point+Vector3.up*(GolfBall.Radius+.002f);}
  float distance,duration;
  IEnumerator Roll(Vector3 start,Vector3 velocity,string label,float timeout=20,GolfShotPlan shot=null){
   ball.Place(start,true);if(shot==null)ball.Strike(velocity);else ball.Strike(shot);uint reset=ball.ResetSequence;var before=ball.Body.position;float began=Time.time;
   do{yield return new WaitForFixedUpdate();}while(ball.Motion!=GolfBallMotion.Resting&&ball.ResetSequence==reset&&Time.time-began<timeout);
   distance=Vector3.ProjectOnPlane(ball.Body.position-before,Vector3.up).magnitude;duration=Time.time-began;
   File.AppendAllText(report,$"MEASURE {label} distance={distance:F3} seconds={duration:F3} state={ball.Motion} reset={ball.ResetSequence!=reset}\n");
   Check(ball.ResetSequence==reset&&ball.Motion==GolfBallMotion.Resting,label+"_SETTLES_WITHOUT_RECOVERY");
  }
  IEnumerator Start(){
   var args=Environment.GetCommandLineArgs();report=args[Array.IndexOf(args,"-report")+1];Directory.CreateDirectory(Path.GetDirectoryName(report));
   float until=Time.realtimeSinceStartup+60;while((!AppRoot.Instance||!AppRoot.Instance.Exploring||!GolfMatchManager.Instance)&&Time.realtimeSinceStartup<until)yield return null;
   match=GolfMatchManager.Instance;Check(match&&match.StartMatch(),"MATCH_START");ball=match.Ball(AppRoot.Instance.LocalAthlete);
   DevelopmentProbe.TurnCommandActive=true;DevelopmentProbe.TurnCommand=default;Time.timeScale=6;
   yield return new WaitForSeconds(.4f);
   // Identical flat support isolates material resistance from terrain slope.
   var plate=new GameObject("Temporary surface comparison");plate.layer=8;plate.AddComponent<BoxCollider>().size=new Vector3(50,.2f,50);
   var positions=new[]{new Vector3(8,30,-106),new Vector3(0,30,-65),new Vector3(-103,30,-60),new Vector3(43,30,-56)};
   var names=new[]{"GREEN","FAIRWAY","ROUGH","SAND"};var distances=new float[4];
   for(int i=0;i<positions.Length;i++){
    plate.transform.position=positions[i];Physics.SyncTransforms();
    yield return Roll(positions[i]+Vector3.up*(.1f+GolfBall.Radius+.002f),Vector3.forward*4,names[i]);distances[i]=distance;
   }
   Check(distances[0]>distances[1]&&distances[1]>distances[2]&&distances[2]>distances[3],"GREEN_FAIRWAY_ROUGH_SAND_DISTANCE_ORDER");Destroy(plate);yield return null;
   foreach(var hole in match.Course.holes){
    // Four directions exercise uphill, downhill and cross-slope putting.
    foreach(var direction in new[]{Vector3.right,Vector3.left,Vector3.forward,Vector3.back}){
     yield return Roll(Ground(hole.cup.position+direction*1.5f),direction*3,$"HOLE_{hole.number}_PUTT_{direction}",8);
     Check(distance>.8f&&distance<4.8f&&duration<5,$"HOLE_{hole.number}_CONTROLLED_PUTT");
     var rest=ball.Body.position;yield return new WaitForSeconds(1);Check(Vector3.Distance(rest,ball.Body.position)<.003f,$"HOLE_{hole.number}_REST_STAYS_PUT");
    }
    var aim=Vector3.ProjectOnPlane(hole.cup.position-hole.tee.position,Vector3.up).normalized;float previousDistance=0;
    foreach(float charge in new[]{.25f,.65f,1f}){
     var start=match.TeePosition(hole.number,ball.Owner);var shot=new GolfShotPlan();shot.Build(start,aim,charge,GolfShotMode.Swing,match.Course,ball.PhysicsSettings);
     yield return Roll(start,shot.LaunchVelocity,$"HOLE_{hole.number}_CHARGE_{charge:F2}",20,shot);
     Check(distance>previousDistance+.25f,$"HOLE_{hole.number}_MORE_CHARGE_MORE_DISTANCE");previousDistance=distance;
     if(charge==1)Check(distance>25&&distance<65&&duration<10,$"HOLE_{hole.number}_USEFUL_BOUNDED_FULL_SHOT");
    }
   }
   Time.timeScale=1;File.AppendAllText(report,"GOLF_PLAYABILITY_COMPLETE checks="+checks+"\n");
  }
 }
}
#endif
