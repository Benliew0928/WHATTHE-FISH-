#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.EventSystems;

namespace WhatTheFish {
 // Opt-in checks of rules, real cup triggers, physical recovery and owner RPCs.
 public sealed class GolfMatchProbe:MonoBehaviour {
  string[] args;string report;int checks;AppRoot app;GolfMatchManager match;GameObject flatFixture;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)] static void Boot(){var args=Environment.GetCommandLineArgs();if(args.Contains("-golfMatchAudit")||args.Contains("-networkGolfMatchAudit"))new GameObject("Golf match checks").AddComponent<GolfMatchProbe>();}
  string Value(string key,string fallback){int i=Array.IndexOf(args,key);return i>=0&&i+1<args.Length?args[i+1]:fallback;}
  void Check(bool value,string name){File.AppendAllText(report,(value?"PASS ":"FAIL ")+name+"\n");if(!value)throw new Exception(name);checks++;}
  IEnumerator Start(){
   args=Environment.GetCommandLineArgs();report=Value("-report",Path.Combine(Application.persistentDataPath,"golf-match.txt"));DevelopmentProbe.TurnCommandActive=true;DevelopmentProbe.TurnCommand=default;
   float until=Time.realtimeSinceStartup+60;while((!AppRoot.Instance||!AppRoot.Instance.Exploring||AppRoot.Instance.SelectedSport!=SportId.Golf||!GolfMatchManager.Instance)&&Time.realtimeSinceStartup<until)yield return null;
   app=AppRoot.Instance;match=GolfMatchManager.Instance;Check(app&&match&&match.Context,"GOLF_MATCH_CONTEXT");yield return new WaitForSeconds(.3f);
   if(args.Contains("-networkGolfMatchAudit"))yield return Network();else {Rules();yield return Offline();}
   File.AppendAllText(report,"GOLF_MATCH_COMPLETE checks="+checks+"\n");
  }
  static GolfEntrant[] Roster(int count)=>Enumerable.Range(1,count).Select(i=>new GolfEntrant((ulong)i,"Player "+i)).ToArray();
  static IEnumerator PoseFrame(){if(SystemInfo.graphicsDeviceType==UnityEngine.Rendering.GraphicsDeviceType.Null)yield return null;else yield return new WaitForEndOfFrame();}
  void Finish(GolfMatchState state,ulong id,int strokes,double time){for(int i=0;i<strokes;i++)state.RecordSwing(id,time);for(int h=1;h<=5;h++)state.EnterHole(id,h,time);}
  void Rules(){
   var s=new GolfMatchState();Check(s.Start(Roster(3),100),"ODD_ROSTER_STARTS");Check(s.Players.All(p=>p.CurrentHole==1&&p.CompletedHoleCount==0&&p.TotalStroke==0&&!p.IsFinished&&!p.IsDNF)&&!s.CountdownStarted,"EVERY_PLAYER_STARTS_HOLE_ONE_NO_TIMER");
   Check(!s.RecordSwing(99,101)&&s.EnterHole(99,1,101)==GolfHoleResult.Ignored,"UNKNOWN_PLAYER_IGNORED");
   Check(s.RecordSwing(1,101)&&s.EnterHole(1,3,102)==GolfHoleResult.Invalid&&s.Player(1).TotalStroke==1&&s.Player(1).CurrentHoleStroke==1&&s.Player(1).CurrentHole==1,"WRONG_HOLE_PRESERVES_STROKE_AND_PROGRESS");
   s.EnterHole(1,1,103);s.EnterHole(1,2,104);s.EnterHole(1,3,105);s.EnterHole(2,1,106);
   Check(s.Player(1).CurrentHole==4&&s.Player(1).CompletedHoleCount==3&&s.Player(2).CurrentHole==2&&s.Player(3).CurrentHole==1,"THREE_INDEPENDENT_PROGRESS_SEQUENCES");
   Check(s.EnterHole(1,3,107)==GolfHoleResult.Invalid&&s.EnterHole(1,5,108)==GolfHoleResult.Invalid&&s.Player(1).CompletedHoleCount==3,"REPEAT_AND_SKIPPED_HOLES_INVALID");
   Check(s.Player(1).CurrentHoleStroke==0&&s.Player(1).TotalStroke==1,"NEXT_HOLE_RESETS_ONLY_HOLE_STROKES");
   s.Advance(1000000);Check(s.Phase==GolfMatchPhase.Playing&&!s.CountdownStarted,"NO_GLOBAL_MAXIMUM_OR_PER_HOLE_TIMER");
   s.Clear();s.Start(Roster(3),100);Finish(s,1,18,700);double deadline=s.CountdownDeadline;
   Check(s.Phase==GolfMatchPhase.FinalCountdown&&s.Remaining(700)==30&&s.FirstFinisher==1&&s.Player(1).FinishTime==600,"FIRST_FINISHER_STARTS_EXACT_THIRTY_AND_ELAPSED_TIME");
   Finish(s,3,18,710);Check(s.CountdownDeadline==deadline&&s.Remaining(710)==20,"LATER_FINISHER_NEVER_RESETS_COUNTDOWN");
   Finish(s,2,16,720);Check(s.Phase==GolfMatchPhase.Ended&&s.Remaining(720)==10,"ALL_FINISHED_ENDS_WITH_TIME_REMAINING");
   var r=s.Results();Check(r.Select(p=>p.player.PlayerId).SequenceEqual(new ulong[]{2,1,3}),"STROKES_FIRST_THEN_FINISH_TIME");Check(r.Select(p=>p.rank).SequenceEqual(new[]{1,2,3}),"FINAL_RANKS_FROM_GAMEPLAY");
   Check(!s.RecordSwing(2,721)&&s.EnterHole(2,5,721)==GolfHoleResult.Ignored&&s.Player(2).TotalStroke==16,"ENDED_SCORES_IMMUTABLE");
   s.Start(Roster(2),0);Finish(s,1,5,10);Finish(s,2,5,10);r=s.Results();Check(r.All(p=>p.rank==1&&p.tie)&&r[0].player.FinishTime==r[1].player.FinishTime,"EXACT_STROKE_TIME_TIE_SHARED_RANK");
   s.Start(Roster(3),0);Finish(s,1,20,10);s.RecordSwing(2,20);s.EnterHole(2,1,21);s.Advance(39.999);
   Check(s.Running&&!s.Player(2).IsDNF&&s.RecordSwing(2,39.999),"UNFINISHED_CAN_PLAY_UNTIL_DEADLINE");
   Check(s.EnterHole(2,2,40)==GolfHoleResult.Ignored&&s.Phase==GolfMatchPhase.Ended,"EXACT_ZERO_ENDS_BEFORE_LATE_HOLE");
   Check(s.Player(2).IsDNF&&s.Player(2).CompletedHoleCount==1&&s.Player(2).TotalStroke==2&&s.Player(3).IsDNF,"DNF_RETAINS_PROGRESS_AND_TOTAL");
   r=s.Results();Check(r[0].player.PlayerId==1&&r.Skip(1).All(p=>p.player.IsDNF&&p.rank==0),"FINISHED_ALWAYS_AHEAD_OF_LOW_STROKE_DNF");
   s.Start(Roster(2),0);for(int h=1;h<=5;h++){s.RecordSwing(1,h);s.EnterHole(2,h,h);}
   Check(s.Player(1).TotalStroke==5&&s.Player(1).CompletedHoleCount==0&&s.Player(2).IsFinished&&s.Player(2).TotalStroke==0,"HELPER_STROKES_AND_BALL_OWNER_COMPLETION_SEPARATE");
   Check(!s.RecordSwing(2,6)&&!s.RecordSwing(1,double.NaN),"FINISHED_PLAYER_AND_INVALID_CLOCK_CANNOT_SCORE");
   s.Advance(35);Check(s.Phase==GolfMatchPhase.Ended&&s.Player(1).IsDNF,"ONLY_FIRST_FINISH_COUNTDOWN_ENDS_UNFINISHED_MATCH");
   Check(s.Start(Roster(1),40)&&s.Player(1).TotalStroke==0&&!s.CountdownStarted&&s.Player(1).CurrentHole==1,"NEW_SINGLE_PLAYER_MATCH_CLEAN");Finish(s,1,7,42);Check(s.Phase==GolfMatchPhase.Ended&&!s.Player(1).IsDNF,"ONE_PLAYER_ALL_FINISHED_ENDS_IMMEDIATELY");
   Check(!s.Start(new[]{new GolfEntrant(1,"A"),new GolfEntrant(1,"B")},50)&&s.Player(1).TotalStroke==7,"BAD_ROSTER_DOES_NOT_DESTROY_RESULT");
   bool validClock=true;for(int tick=1;tick<=500;tick++){s.Clear();s.Start(Roster(2),0);double time=tick*.02;Finish(s,1,0,time);validClock&=Math.Ceiling(s.Remaining(time))==30&&s.Remaining(time)<=30;}
   Check(validClock,"FRACTIONAL_FIXED_CLOCK_INITIAL_COUNTDOWN_ALWAYS_THIRTY");
  }
  Vector3 Floor(Vector3 p){Check(Physics.Raycast(p+Vector3.up*15,Vector3.down,out var hit,40,1<<8,QueryTriggerInteraction.Ignore),"FIXTURE_SUPPORTED_FLOOR");return hit.point;}
  Vector3 FlatSite(){
   // Charge/contact assertions need a level lie. Real incline motion is
   // checked separately by GolfBallPhysicsProbe without freezing the ball.
   var site=Floor(new Vector3(-40,0,-100))+Vector3.up*3;
   flatFixture=new GameObject("Temporary level swing fixture");flatFixture.layer=8;flatFixture.transform.position=site-Vector3.up*.1f;flatFixture.AddComponent<BoxCollider>().size=new Vector3(30,.2f,30);Physics.SyncTransforms();return site;
  }
  void OnDestroy(){if(flatFixture)Destroy(flatFixture);}
  void Place(Athlete actor,Vector3 position,float yaw=0){actor.capsule.enabled=false;actor.transform.SetPositionAndRotation(position,Quaternion.Euler(0,yaw,0));actor.ResetLocomotion();var net=actor.GetComponent<NetworkTransform>();if(net&&net.IsSpawned&&net.IsServer)net.Teleport(position,actor.transform.rotation,Vector3.one);actor.capsule.enabled=match.Authority;Physics.SyncTransforms();}
  void CaptureClub(string name){
   if(!args.Contains("-golfMatchCapture"))return;var camera=Camera.main;var position=camera.transform.position;var rotation=camera.transform.rotation;float fov=camera.fieldOfView;
   camera.transform.position=app.LocalAthlete.transform.position+new Vector3(2.1f,1.6f,1.3f);camera.transform.LookAt(app.LocalAthlete.transform.position+Vector3.up*.92f);camera.fieldOfView=44;
   FindFirstObjectByType<DevelopmentProbe>()?.CaptureFrame(Path.ChangeExtension(report,"club-"+name+".png"));camera.transform.SetPositionAndRotation(position,rotation);camera.fieldOfView=fov;
  }
  void LogClub(Athlete actor,string label){var c=actor.GolfClubMotion;File.AppendAllText(report,$"CLUB_SAMPLE {label} action={c.State.action} elapsed={c.Elapsed:F3} gripError={c.GripError:F4} left={c.LeftPalm} right={c.RightPalm} head={c.Club.head.position} root={actor.transform.position}\n");}
  void CheckBallVisual(GolfBall ball){
   var lods=ball.GetComponent<LODGroup>().GetLODs();var near=lods[0].renderers.Single();var far=lods[1].renderers.Single();
   Check(near.GetComponent<MeshFilter>().sharedMesh.GetIndexCount(0)/3==15998&&far.GetComponent<MeshFilter>().sharedMesh.GetIndexCount(0)/3==720,"LIVE_BALL_USES_SUPPLIED_DIMPLED_MODEL_AND_ROUND_DISTANT_LOD");
   Check(Vector3.Distance(near.bounds.center,ball.transform.position)<.0001f&&Mathf.Abs(near.bounds.size.x-GolfBall.Radius*2)<.0002f,"NEW_BALL_VISUAL_CENTRED_ON_EXISTING_PHYSICS_RADIUS");
   Check(near.sharedMaterial==far.sharedMaterial&&near.sharedMaterial.GetTexture("_BumpMap")==null&&near.sharedMaterial.GetTexture("_BaseMap")==null,"GEOMETRIC_DIMPLES_SHARE_ONE_MATERIAL_WITHOUT_OLD_TEXTURES");
   Check(ball.GetComponentsInChildren<Rigidbody>().Length==1&&ball.GetComponentsInChildren<Collider>().Length==1&&Mathf.Abs(ball.GetComponent<SphereCollider>().radius-GolfBall.Radius)<.000001f,"MODEL_ADDS_NO_DUPLICATE_PHYSICS_OR_COLLISION_CHANGE");
   if(!args.Contains("-golfMatchCapture"))return;
   var camera=Camera.main;var position=camera.transform.position;var rotation=camera.transform.rotation;float fov=camera.fieldOfView,clip=camera.nearClipPlane;
   try{camera.nearClipPlane=.003f;camera.fieldOfView=42;camera.transform.position=ball.transform.position+new Vector3(.045f,.025f,.075f)*GolfBall.VisualScale;camera.transform.LookAt(ball.transform.position);ball.GetComponent<LODGroup>().ForceLOD(0);FindFirstObjectByType<DevelopmentProbe>()?.CaptureFrame(Path.ChangeExtension(report,"rounded-ball.png"));}
   finally{ball.GetComponent<LODGroup>().ForceLOD(-1);camera.transform.SetPositionAndRotation(position,rotation);camera.fieldOfView=fov;camera.nearClipPlane=clip;}
  }
  void CheckClosedGrip(Athlete actor,string label){
   var club=actor.GolfClubMotion;var skins=actor.visual.GetComponentsInChildren<SkinnedMeshRenderer>(true).Where(s=>s.sharedMesh.GetBlendShapeIndex("GolfRightGrip")>=0).ToArray();
   Check(skins.Length==2&&skins.All(s=>s.GetBlendShapeWeight(s.sharedMesh.GetBlendShapeIndex("GolfRightGrip"))>99),label+"_CLOSED_HAND_APPLIED_TO_BOTH_CHARACTER_LODS");
   var socket=actor.visual.GetComponentsInChildren<Transform>(true).First(t=>t.name=="Golf grip socket");
   Check(Vector3.Distance(socket.position,club.Club.rightGrip.position)<.001f&&Mathf.Abs(Vector3.Dot(socket.forward,club.Club.transform.up))>.999f,label+"_SHAFT_CENTRED_AND_ALIGNED_IN_ACTUAL_GRIP_CAVITY");
   if(SystemInfo.graphicsDeviceType==UnityEngine.Rendering.GraphicsDeviceType.Null)return;
   var skin=skins.OrderByDescending(s=>s.sharedMesh.vertexCount).First();var baked=new Mesh();skin.BakeMesh(baked,true);
   var vertices=baked.vertices;for(int i=0;i<vertices.Length;i++)vertices[i]=skin.transform.TransformPoint(vertices[i])-socket.position;
   var indices=baked.triangles;int enclosed=0;float nearest=float.PositiveInfinity,farthest=0;
   for(int ray=0;ray<8;ray++){
    var direction=Quaternion.AngleAxis(ray*45,club.Club.transform.up)*club.Club.transform.right;float hit=.08f;
    for(int i=0;i<indices.Length;i+=3){
     var a=vertices[indices[i]];var b=vertices[indices[i+1]];var c=vertices[indices[i+2]];if(a.sqrMagnitude>.04f&&b.sqrMagnitude>.04f&&c.sqrMagnitude>.04f)continue;
     var e1=b-a;var e2=c-a;var p=Vector3.Cross(direction,e2);float determinant=Vector3.Dot(e1,p);if(Mathf.Abs(determinant)<1e-10f)continue;
     float inverse=1/determinant;var t=-a;float u=Vector3.Dot(t,p)*inverse;if(u<0||u>1)continue;
     var q=Vector3.Cross(t,e1);float v=Vector3.Dot(direction,q)*inverse;if(v<0||u+v>1)continue;
     float distance=Vector3.Dot(e2,q)*inverse;if(distance>0&&distance<hit)hit=distance;
    }
    if(hit<.08f){enclosed++;nearest=Mathf.Min(nearest,hit);farthest=Mathf.Max(farthest,hit);}
   }
   Destroy(baked);File.AppendAllText(report,$"GRIP_SKIN {label} coveredRays={enclosed}/8 innerDistance={nearest:F4}..{farthest:F4}\n");
   var camera=Camera.main;var position=camera.transform.position;var rotation=camera.transform.rotation;float fov=camera.fieldOfView;
   camera.transform.position=socket.position+actor.transform.right*.38f+actor.transform.forward*.20f+Vector3.up*.10f;camera.transform.LookAt(socket.position);camera.fieldOfView=42;
   if(args.Contains("-golfMatchCapture"))FindFirstObjectByType<DevelopmentProbe>()?.CaptureFrame(Path.ChangeExtension(report,"grip-"+label.ToLowerInvariant()+".png"));
   camera.transform.SetPositionAndRotation(position,rotation);camera.fieldOfView=fov;
   Check(enclosed>=6&&nearest>.004f,label+"_REAL_SKIN_WRAPS_AROUND_HANDLE_WITH_OPEN_SHAFT_CHANNEL");
  }
  IEnumerator CarryGait(Athlete actor,string label,PlayerCommand command,float seconds){
   DevelopmentProbe.TurnCommand=command;yield return new WaitForSeconds(.45f);yield return PoseFrame();
   var club=actor.GolfClubMotion;float leftMin=float.PositiveInfinity,leftMax=float.NegativeInfinity,rightMin=float.PositiveInfinity,rightMax=float.NegativeInfinity,grip=0,headRange=0,minBack=1,minPitch=90,maxPitch=-90;
   var startHead=actor.transform.InverseTransformPoint(club.Club.head.position);var hand=actor.visual.GetComponentsInChildren<Transform>(true).First(t=>t.name=="mixamorig:RightHand");var startHand=Quaternion.Inverse(actor.transform.rotation)*hand.rotation;float wristDeviation=0,handSway=0;int samples=0;
   float highest=float.NegativeInfinity,lowest=float.PositiveInfinity,highPitch=0,lowPitch=0,clearance=float.PositiveInfinity;
   float until=Time.time+seconds;while(Time.time<until){
    yield return PoseFrame();var l=actor.transform.InverseTransformPoint(club.LeftPalm);var r=actor.transform.InverseTransformPoint(club.RightPalm);
    leftMin=Mathf.Min(leftMin,l.z);leftMax=Mathf.Max(leftMax,l.z);rightMin=Mathf.Min(rightMin,r.z);rightMax=Mathf.Max(rightMax,r.z);grip=Mathf.Max(grip,club.GripError);
    headRange=Mathf.Max(headRange,Vector3.Distance(startHead,actor.transform.InverseTransformPoint(club.Club.head.position)));
    clearance=Mathf.Min(clearance,club.Club.head.position.y-actor.transform.position.y);
    var axis=(club.Club.head.position-club.RightPalm).normalized;minBack=Mathf.Min(minBack,Vector3.Dot(Vector3.ProjectOnPlane(axis,Vector3.up).normalized,-actor.transform.forward));float pitch=Mathf.Asin(Mathf.Clamp(-axis.y,-1,1))*Mathf.Rad2Deg;minPitch=Mathf.Min(minPitch,pitch);maxPitch=Mathf.Max(maxPitch,pitch);
    if(r.y>highest){highest=r.y;highPitch=pitch;}
    if(r.y<lowest){lowest=r.y;lowPitch=pitch;}
    wristDeviation=Mathf.Max(wristDeviation,club.CarryWristDeviation);handSway=Mathf.Max(handSway,Quaternion.Angle(startHand,Quaternion.Inverse(actor.transform.rotation)*hand.rotation));samples++;
   }
   float leftRange=leftMax-leftMin,rightRange=rightMax-rightMin;
   File.AppendAllText(report,$"CARRY_GAIT {label} samples={samples} leftRange={leftRange:F4} rightRange={rightRange:F4} headRange={headRange:F4} gripError={grip:F4} wristDeviation={wristDeviation:F3} handSway={handSway:F3} minBack={minBack:F3} downPitch={minPitch:F2}..{maxPitch:F2}\n");
   File.AppendAllText(report,$"GRIP_EXTREMES {label} height={lowest:F3}..{highest:F3} pitchLow={lowPitch:F2} pitchHigh={highPitch:F2} headClearance={clearance:F3} shapeReady={club.GripShapeReady}\n");
   // Inspect live frames. Restoring two bone snapshots in one rendered frame
   // leaves Unity skinning matrices cached from the first pose.
   bool capturedHigh=false,capturedLow=false;float captureUntil=Time.time+6;
   while(Time.time<captureUntil&&(!capturedHigh||!capturedLow)){
    yield return PoseFrame();float height=actor.transform.InverseTransformPoint(club.RightPalm).y;
    if(!capturedHigh&&height>highest-.009f){CaptureClub(label.ToLowerInvariant()+"-high");CheckClosedGrip(actor,label+"_HIGH");capturedHigh=true;}
    else if(!capturedLow&&height<lowest+.009f){CaptureClub(label.ToLowerInvariant()+"-low");CheckClosedGrip(actor,label+"_LOW");capturedLow=true;}
   }
   Check(capturedHigh&&capturedLow,label+"_LIVE_HIGH_AND_LOW_CARRY_FRAMES_INSPECTED");
   Check(samples>8&&!club.TwoHanded&&grip<.001f,label+"_RIGHT_PALM_REMAINS_ATTACHED_TO_CLUB");
   Check(wristDeviation<65,label+"_RIGHT_WRIST_AVOIDS_EXTREME_FOLDING");
   if(command.move.sqrMagnitude>0)Check(handSway>15,label+"_RIGHT_PALM_ROTATES_NATURALLY_WITH_ARM_SWING");
   Check(club.GripShapeReady,label+"_REAL_MITTEN_GRIP_SHAPE_AVAILABLE");
   Check(minBack>.96f&&minPitch>5&&maxPitch<60,label+"_CLUB_STAYS_BACKWARD_WITH_POSE_DEPENDENT_DOWNWARD_PITCH");
   Check(clearance>.035f,label+"_CARRY_HEAD_CLEARS_PLAYER_FOOT_LEVEL_THROUGHOUT_GAIT");
   if(command.move.sqrMagnitude>0)Check(highPitch>30&&highPitch<60&&lowPitch<25,label+"_RAISED_HAND_TILTS_CLUB_DOWN_AND_LOW_HAND_CARRIES_NEAR_LEVEL");
   if(command.move.sqrMagnitude>0){Check(leftRange>.10f&&rightRange>.10f&&rightRange>leftRange*.55f&&rightRange<leftRange*1.65f,label+"_BOTH_ARMS_SWING_WITH_COMPARABLE_FORWARD_BACK_RANGE");Check(headRange>.12f,label+"_CLUB_SWAYS_WITH_RUNNING_OR_WALKING_HAND");}
   else {var direction=(club.Club.head.position-club.RightPalm).normalized;var horizontal=Vector3.ProjectOnPlane(direction,Vector3.up).normalized;File.AppendAllText(report,$"CARRY_IDLE_DIRECTION {direction} back={Vector3.Dot(horizontal,-actor.transform.forward):F3}\n");Check(Vector3.Dot(horizontal,-actor.transform.forward)>.8f&&direction.y<-.02f&&direction.y>-.45f,label+"_CLUB_POINTS_BACKWARD_NEAR_HORIZONTAL_AND_SLIGHTLY_DOWN");Check(headRange>.001f&&headRange<.15f,label+"_CLUB_RETAINS_SUBTLE_IDLE_HAND_SWAY");}
   CaptureClub(label.ToLowerInvariant());DevelopmentProbe.TurnCommand=default;
  }
  IEnumerator Drop(GolfBall ball,int number){
   var hole=match.Course.holes.First(h=>h.number==number);var reset=ball.ResetSequence;int completed=match.State.Player(ball.Owner).CompletedHoleCount;
   ball.Place(hole.cup.position+Vector3.up*.18f+Vector3.right*.035f,false);ball.Body.linearVelocity=Vector3.down*.7f;ball.Body.WakeUp();Physics.SyncTransforms();
   float until=Time.time+2;while(Time.time<until&&ball.Live&&match.State.Player(ball.Owner).CompletedHoleCount==completed&&ball.ResetSequence==reset+1)yield return new WaitForFixedUpdate();
   File.AppendAllText(report,$"CUP_SAMPLE owner={ball.Owner} entered={number} completed={match.State.Player(ball.Owner).CompletedHoleCount} reset={ball.ResetSequence} body={ball.Body.position} phase={match.State.Phase}\n");
  }
  IEnumerator Offline(){
   var a=app.LocalAthlete;var b=Instantiate(app.athletePrefab).GetComponent<Athlete>();b.name="Golf fixture B";b.Setup();
   Check(!a.GolfClubMotion.Equipped&&!b.GolfClubMotion.Equipped,"NO_CLUB_BEFORE_GOLF_MATCH");
   Check(match.StartTestMatch(new[]{(100UL,a,"Player A"),(200UL,b,"Player B")}),"OFFLINE_TWO_INDEPENDENT_BALLS");
   Check(match.HoleTriggers.Length==5&&match.HoleTriggers.All(h=>h.enabled&&h.GetComponent<BoxCollider>().enabled),"ALL_FIVE_CUP_TRIGGERS_OPEN_TOGETHER");
   var ball=match.Ball(200);var site=FlatSite();ball.Place(site+Vector3.up*(GolfBall.Radius+.002f),true);Place(a,site+Vector3.back*.9f+Vector3.up*.04f);Place(b,site+Vector3.right*5+Vector3.up*.04f);yield return new WaitForFixedUpdate();
   CheckBallVisual(ball);
   app.view.mode=1;app.view.yaw=0;app.view.pitch=14;yield return new WaitForSeconds(.15f);
   var club=a.GolfClubMotion;Check(club.Equipped&&b.GolfClubMotion.Equipped,"MATCH_GRANTS_EACH_PLAYER_MIDNIGHT_IRON");
   var lods=club.Club.GetComponent<LODGroup>().GetLODs();Check(lods.Length==3&&lods.All(l=>l.renderers.Length==1)&&lods.SelectMany(l=>l.renderers).Select(r=>r.sharedMaterial).Distinct().Count()==1,"CLUB_THREE_LODS_ONE_SHARED_MATERIAL");
   var material=lods[0].renderers[0].sharedMaterial;Check(material.GetTexture("_BaseMap") is Texture2D color&&color.width==512&&material.GetTexture("_BumpMap") is Texture2D normal&&normal.width==512&&material.GetTexture("_MetallicGlossMap") is Texture2D mask&&mask.width==256,"CLUB_RETAINS_COLOR_NORMAL_AND_METAL_SMOOTHNESS_2D_MAPS");
   Check(club.Club.GetComponentsInChildren<Collider>().Length==0&&club.Club.GetComponentsInChildren<Rigidbody>().Length==0,"VISUAL_CLUB_ADDS_NO_DUPLICATE_BALL_PHYSICS");
   yield return PoseFrame();LogClub(a,"carry");Check(!club.TwoHanded&&club.GripError<.025f&&Physics.Raycast(club.Club.head.position+Vector3.up*.5f,Vector3.down,out var clubGround,2,1<<8,QueryTriggerInteraction.Ignore)&&club.Club.head.position.y>clubGround.point.y+.04f,"SINGLE_RIGHT_HAND_CARRY_WITH_HEAD_CLEAR_OF_FLOOR");CaptureClub("carry");
   Check(app.view.BeginGolfSwing(42),"CLUB_CHARGE_BEGINS_IN_REACH");yield return null;yield return new WaitForSeconds(.30f);yield return PoseFrame();LogClub(a,"charge");
   Check(club.TwoHanded&&club.GripError<.025f&&club.State.action==GolfClubAction.Charge&&club.Elapsed>.20f,"BOTH_PALMS_STAY_ON_CLUB_GRIP_DURING_CHARGE");CaptureClub("charge");
   var head=a.visual.GetComponentsInChildren<Transform>(true).First(t=>t.name=="mixamorig:Head");var headScale=head.localScale;app.view.mode=0;app.view.pitch=42;yield return PoseFrame();LogClub(a,"first-person");
   Check(head.localScale.magnitude<.01f&&a.visual.GetComponentsInChildren<SkinnedMeshRenderer>(true).All(r=>r.enabled)&&club.TwoHanded&&club.GripError<.025f,"FIRST_PERSON_KEEPS_GRIPPING_ARMS_VISIBLE_AND_HIDES_LOCAL_HEAD");
   if(args.Contains("-golfMatchCapture"))FindFirstObjectByType<DevelopmentProbe>()?.CaptureFrame(Path.ChangeExtension(report,"club-first-person.png"));
   app.view.mode=1;app.view.pitch=14;yield return PoseFrame();Check(Vector3.Distance(head.localScale,headScale)<.0001f,"THIRD_PERSON_RESTORES_CHARACTER_HEAD");
   app.view.CancelGolfSwing(42);yield return new WaitForSeconds(.15f);yield return PoseFrame();
   Check(!club.TwoHanded&&club.State.action==GolfClubAction.Carry&&match.Player(a).TotalStroke==0,"CANCEL_RETURNS_TO_CARRY_WITHOUT_STROKE");
   Check(ball.GetComponentsInChildren<LODGroup>().Length==1&&ball.GetComponent<LODGroup>().GetLODs().Length==2,"LIVE_BALL_SHARES_TWO_AUTHORED_LODS_WITHOUT_NESTED_GROUPS");
   Check(!match.TrySwing(a,200,float.NaN,.5f,match.Round)&&!match.TrySwing(a,200,0,.5f,match.Round-1)&&match.Player(a).TotalStroke==0,"INVALID_AND_STALE_SWINGS_NOT_COUNTED");
   var velocityBefore=ball.Body.linearVelocity;
   Check(match.TrySwing(a,200,0,.08f,match.Round)&&ball.Owner==200&&match.Player(a).TotalStroke==1&&match.Player(b).TotalStroke==0,"HITTING_OTHER_BALL_COUNTS_ONLY_HITTER");
   Check(!match.TrySwing(a,200,0,.08f,match.Round)&&!a.CanRequestJump,"SWING_RECOVERY_REJECTS_DUPLICATE_AND_JUMP");
   var anchor=ball.LastShotPosition;Check(Vector3.Distance(ball.Body.linearVelocity,velocityBefore)<.00001f,"BALL_WAITS_FOR_CLUB_CONTACT");
   while(club.Elapsed<GolfClubMotion.ContactTime)yield return null;yield return PoseFrame();LogClub(a,"contact");CaptureClub("contact");
   Check(club.TwoHanded&&club.GripError<.025f,"BOTH_HANDS_HOLD_CLUB_AT_CONTACT");
   Check(Vector3.Dot(club.Club.transform.position-a.transform.position,a.transform.forward)>.24f,"CONTACT_HANDS_REMAIN_IN_FRONT_OF_TORSO");
   yield return new WaitForSeconds(.1f);Check(Vector3.Distance(ball.Body.position,anchor)>.05f,"REAL_RIGIDBODY_SWING_MOVES_BALL");anchor=ball.LastShotPosition;yield return PoseFrame();LogClub(a,"follow-through");Check(club.Club.head.position.y>=club.State.target.y-.015f&&club.GripError<.025f,"FOLLOW_THROUGH_LIFTS_CLUB_ABOVE_CONTACT_WITH_BOTH_GRIPS");CaptureClub("follow-through");yield return new WaitForSeconds(.7f);
   Check(!club.Busy&&!club.TwoHanded&&match.Player(a).TotalStroke==1,"FOLLOW_THROUGH_RETURNS_TO_CARRY_COUNTS_ONCE");
   for(int hole=2;hole<=5;hole++){yield return Drop(ball,hole);Check(match.Player(b).CurrentHole==1&&match.Player(b).CompletedHoleCount==0&&Vector3.Distance(ball.Body.position,anchor)<.04f&&match.Player(a).TotalStroke==1,"WRONG_HOLE_RECOVERS_WITHOUT_PROGRESS_OR_PENALTY_"+hole);}
   ball.Place(new Vector3(450,5,450),false);yield return new WaitForFixedUpdate();yield return new WaitForFixedUpdate();Check(Vector3.Distance(ball.Body.position,anchor)<.04f&&match.Player(a).TotalStroke==1,"OUT_OF_BOUNDS_RECOVERS_LAST_SHOT_WITHOUT_PENALTY");
   Check(ball.GetComponent<SphereCollider>().excludeLayers==((1<<9)|(1<<GolfCartMotor.Layer)),"BALL_CAN_CROSS_CHARACTER_SHORE_WALLS_AND_IGNORES_CART_PUSH");
   var h1=match.Course.holes.First(h=>h.number==1);Check(!match.HoleTriggers[0].Crossed(h1.cup.position+new Vector3(-1,.5f,0),h1.cup.position+new Vector3(1,.5f,0)),"FLY_OVER_NEVER_COUNTS_AS_CUP_ENTRY");
   yield return Drop(ball,1);Check(match.Player(b).CurrentHole==2&&match.Player(a).CurrentHole==1&&match.Player(b).TotalStroke==0&&Vector3.Distance(ball.Body.position,match.TeePosition(2,200))<.04f,"REAL_CUP_COMPLETES_OWNER_AND_RESPAWNS_NEXT_TEE");
   Check(match.Target(a).number==1&&match.Target(b).number==2,"TARGETS_READ_EACH_PLAYERS_OWN_PROGRESS");
   yield return null;var indicator=FindFirstObjectByType<GolfTargetIndicator>();Check(indicator.TargetHole==1&&indicator.BallOwner==100,"LOCAL_HOLE_AND_BALL_INDICATORS");
   app.view.enabled=false;app.view.transform.rotation=Quaternion.Euler(0,180,0);yield return null;
   var arrow=FindObjectsByType<GolfArrowGraphic>(FindObjectsSortMode.None).Single(g=>g.name=="Your target hole");
   Check(Mathf.Abs(Mathf.DeltaAngle(arrow.transform.localEulerAngles.z,GolfTargetIndicator.Bearing(a.transform.position,match.Target(a).cup.position,180)))<.1f,"INDICATOR_USES_PRESENTED_CAMERA_INCLUDING_REVERSE_VIEW");app.view.enabled=true;
   var own=match.Ball(a);var ownSite=Floor(new Vector3(-40,0,-100));own.Place(ownSite+Vector3.up*(GolfBall.Radius+.002f),true);Place(a,ownSite+Vector3.back*.9f+Vector3.up*.04f);app.view.yaw=0;yield return new WaitForSeconds(.1f);
   var swing=FindFirstObjectByType<GolfSwingButton>();var pointer=new PointerEventData(EventSystem.current){pointerId=17,button=PointerEventData.InputButton.Left};swing.OnPointerDown(pointer);yield return new WaitForSeconds(.08f);swing.OnPointerUp(pointer);yield return new WaitForSeconds(.15f);Check(match.Player(a).TotalStroke==2,"TOUCH_HOLD_RELEASE_COMMAND_COUNTS_ONCE");
   swing.OnPointerDown(pointer);yield return null;swing.OnPointerExit(pointer);swing.OnPointerUp(pointer);yield return new WaitForSeconds(.1f);Check(match.Player(a).TotalStroke==2&&!app.view.GolfCharging,"CANCELLED_TOUCH_NEVER_COUNTS");
   for(int hole=2;hole<=5;hole++)yield return Drop(ball,hole);
   Check(match.Player(b).IsFinished&&match.State.Phase==GolfMatchPhase.FinalCountdown&&match.Remaining>29&&match.Remaining<=30,"FIRST_ACTUAL_FINISH_CONTINUES_MATCH_FOR_THIRTY completed="+match.Player(b).CompletedHoleCount+" phase="+match.State.Phase+" remaining="+match.Remaining);
   Check(!match.TrySwing(a,200,0,.1f,match.Round)&&!match.TrySwing(b,100,0,.1f,match.Round),"FINISHED_BALL_AND_FINISHED_HITTER_REJECTED");
   yield return null;var ui=FindFirstObjectByType<GolfLeaderboardUI>();Check(ui.CountdownVisible&&ui.CountdownText=="00:30","COUNTDOWN_UI_STARTS_ONLY_AFTER_FIRST_FINISH");
   for(int hole=1;hole<=5;hole++)yield return Drop(own,hole);
   Check(match.State.Phase==GolfMatchPhase.Ended&&match.Remaining>20&&match.State.Results()[0].player.PlayerId==200,"ALL_ACTUAL_PLAYERS_FINISHED_END_EARLY_LOW_STROKES_FIRST");
   uint old=match.Round;Check(match.StartTestMatch(new[]{(100UL,a,"Player A"),(200UL,b,"Player B")})&&match.Round!=old&&match.Player(a).TotalStroke==0,"RESTART_RESETS_ROUND_PROGRESS_BALLS");yield return null;Check(!ui.CountdownVisible,"NEW_ROUND_HIDES_OLD_COUNTDOWN");
   Check(a.GetComponentsInChildren<GolfClubModel>(true).Length==1&&club.Equipped&&club.State.round==match.Round,"RESTART_REUSES_SINGLE_CLUB_AND_CLEARS_OLD_POSE");
   // Inspect each gait on the level fixture. Terrain slopes and accumulated
   // travel can change the sampled hand-height extremes between capture loops.
   Place(a,site+Vector3.up*.04f);yield return new WaitForFixedUpdate();
   yield return CarryGait(a,"IDLE",default,1.3f);
   Place(a,site+Vector3.up*.04f);yield return new WaitForFixedUpdate();
   yield return CarryGait(a,"WALK",new PlayerCommand{move=Vector2.up,heading=0},1.2f);
   Place(a,site+Vector3.up*.04f);yield return new WaitForFixedUpdate();
   yield return CarryGait(a,"RUN",new PlayerCommand{move=Vector2.up,heading=0,sprint=true},1.2f);
   Check(a.speed>4.5f&&club.GripError<.001f&&!club.TwoHanded,"RUNNING_CARRY_PRESERVES_SPEED_AND_FREE_ARM_GAIT");
   // Cart placement requires the actual island terrain. Do not depend on the
   // gait's frame timing carrying the actor off the temporary level fixture.
   Place(a,Floor(match.Course.holes[0].tee.position)+Vector3.up*.04f);yield return new WaitForSeconds(.2f);
   ulong cartId=GolfCartWorld.Key(a);Check(GolfCartWorld.Execute(a,GolfCartAction.Summon,cartId),"CLUB_FIXTURE_SUMMONS_CART_DURING_MATCH");var cart=GolfCartWorld.Owned(a);Place(a,Floor(cart.transform.position-cart.transform.right*1.8f)+Vector3.up*.04f);yield return new WaitForSeconds(.15f);
   Check(GolfCartWorld.Execute(a,GolfCartAction.Drive,cartId),"CLUB_FIXTURE_ENTERS_CART");yield return new WaitForSeconds(.2f);Check(!club.Equipped&&!club.TwoHanded&&!match.CanSwing(a),"DRIVING_STOWS_CLUB_AND_RELEASES_HANDS");
   Check(GolfCartWorld.Execute(a,GolfCartAction.Leave,cartId),"CLUB_FIXTURE_EXITS_CART");yield return new WaitForSeconds(.2f);yield return PoseFrame();Check(club.Equipped&&club.GripError<.025f&&a.GetComponentsInChildren<GolfClubModel>(true).Length==1,"LEAVING_CART_REEQUIPS_SAME_CLUB");GolfCartWorld.Execute(a,GolfCartAction.Recall,cartId);
   if(args.Contains("-golfMatchCapture")){app.view.mode=1;Place(a,match.Course.holes[0].tee.position+Vector3.back*2+Vector3.up*.04f);app.view.yaw=0;app.view.pitch=16;yield return new WaitForSeconds(.4f);FindFirstObjectByType<DevelopmentProbe>()?.CaptureFrame(Path.ChangeExtension(report,"playing.png"));yield return new WaitForSeconds(.4f);}
   match.ClearMatch();Destroy(b.gameObject);Destroy(flatFixture);yield return null;Check(!club.Equipped&&!club.Busy,"MATCH_CLEAR_HIDES_CLUB_AND_CANCELS_POSE");
   foreach(var sport in new[]{SportId.Football,SportId.Basketball,SportId.Golf}){yield return app.environments.GetComponent<SkySailStreaming>().Prepare(sport);app.SelectSport(sport);Check(app.environments.roots.Count(r=>r&&r.activeSelf)==1,"GOLF_MATCH_PRESERVES_SPORT_SWITCH_"+sport);}
   Check(GolfMatchManager.Instance.State.Phase==GolfMatchPhase.Idle,"STREAMED_REENTRY_HAS_NO_STALE_MATCH");
  }
  IEnumerator Network(){
   // Both peers need the same test ground for local swing eligibility.
   var site=FlatSite();
   if(match.Authority){
    Check(match.StartMatch()&&match.State.Players.Count==2,"HOST_STARTS_SHARED_TWO_PLAYER_MATCH");var guest=Athlete.Active.First(a=>a.GetComponent<NetworkAthlete>()&&a.GetComponent<NetworkAthlete>().OwnerClientId!=0);var guestId=guest.GetComponent<NetworkAthlete>().OwnerClientId;
    CheckBallVisual(match.Ball(0));
    Check(match.State.Players.All(p=>match.Tee(p.PlayerId))&&FindObjectsByType<GolfTee>(FindObjectsSortMode.None).Length==2,"HOST_CREATES_ONE_FIXED_OPENING_TEE_PER_BALL_OWNER");
    var hostBall=match.Ball(0);hostBall.Place(site+Vector3.up*(GolfBall.Radius+.002f),true);Place(guest,site+(args.Contains("-golfSideSwing")?Vector3.right*.5f:Vector3.back*.9f)+Vector3.up*.04f,args.Contains("-golfSideSwing")?135:0);Place(app.LocalAthlete,site+Vector3.right*5+Vector3.up*.04f);
    yield return new WaitForSeconds(.15f);Check(guest.GolfClubMotion.Equipped&&app.LocalAthlete.GolfClubMotion.Equipped,"HOST_EQUIPS_BOTH_NETWORK_PLAYERS");
    bool charged=false;float chargeUntil=Time.time+15;while(match.Player(guest).TotalStroke<1&&Time.time<chargeUntil){charged|=guest.GolfClubMotion.State.action==GolfClubAction.Charge&&guest.GolfClubMotion.TwoHanded&&guest.GolfClubMotion.GripError<.025f;yield return null;}Check(charged,"HOST_SEES_RELIABLE_CLIENT_TWO_HAND_CHARGE");
    float until=Time.time+15;while(match.Player(guest).TotalStroke<1&&Time.time<until)yield return null;
    Check(match.Player(guest).TotalStroke==1&&match.Player(app.LocalAthlete).TotalStroke==0&&hostBall.Owner==0,"CLIENT_RPC_HITS_HOST_BALL_WITHOUT_TRANSFERRING_OWNER");
    Check(guest.GolfClubMotion.State.action==GolfClubAction.Swing&&guest.GolfClubMotion.State.round==match.Round,"HOST_PUBLISHES_SWING_ROUND_AND_SHARED_START_TIME");yield return new WaitForSeconds(.35f);Check(Vector3.ProjectOnPlane(hostBall.Body.position-hostBall.LastShotPosition,Vector3.up).magnitude>.05f,"HOST_SIMULATES_ACTUAL_CLIENT_SHOT");
    yield return Drop(hostBall,1);Check(match.State.Player(0).CurrentHole==2&&match.Player(guest).CurrentHole==1,"NETWORK_HELPER_SHOT_COMPLETES_BALL_OWNER_ONLY");
    var guestBall=match.Ball(guestId);var anchor=guestBall.LastShotPosition;yield return Drop(guestBall,3);Check(match.Player(guest).CurrentHole==1&&Vector3.Distance(guestBall.Body.position,anchor)<.04f,"HOST_WRONG_CUP_RECOVERY_REPLICATED");
    yield return new WaitForSeconds(1);for(int h=2;h<=5;h++)yield return Drop(hostBall,h);double deadline=match.State.CountdownDeadline;
    Check(match.State.FirstFinisher==0&&match.Player(guest).CurrentHole==1,"HOST_FIRST_FINISHER_ONE_SHARED_COUNTDOWN");
    yield return new WaitForSeconds(1);Check(!match.TrySwing(guest,guestId,0,.1f,match.Round-1),"HOST_REJECTS_OLD_ROUND_RPC");
    yield return Drop(guestBall,1);Check(match.Player(guest).CurrentHole==2&&match.State.CountdownDeadline==deadline,"COUNTDOWN_PLAY_REMAINS_VALID_AND_DEADLINE_STABLE");
    until=Time.time+35;while(match.State.Running&&Time.time<until)yield return null;
    Check(match.State.Phase==GolfMatchPhase.Ended&&match.Player(guest).IsDNF&&match.Player(guest).CompletedHoleCount==1&&match.Player(guest).TotalStroke==1,"ACTUAL_THIRTY_SECONDS_ENDS_WITH_DNF_SCORES_SAVED");
    Check(match.State.Results()[0].player.PlayerId==0&&match.State.CountdownDeadline==deadline,"HOST_FINISHED_RESULT_BEFORE_DNF");yield return new WaitForSeconds(1);
   }else{
    Check(!match.StartMatch(),"CLIENT_CANNOT_AUTHOR_MATCH");float until=Time.time+15;while((!match.State.Running||match.State.Players.Count!=2||!match.Strikeable(app.LocalAthlete))&&Time.time<until)yield return null;
    Check(match.State.Players.Count==2&&match.Target(app.LocalAthlete).number==1,"CLIENT_RECEIVES_INDEPENDENT_START_STATE");Check(match.Strikeable(app.LocalAthlete).Owner==0,"CLIENT_CAN_SELECT_OTHER_OWNERS_BALL");var shotStart=match.Ball(0).Body.position;uint ownReset=match.Ball(app.LocalAthlete).ResetSequence;app.view.yaw=0;
    CheckBallVisual(match.Ball(0));
    Check(match.State.Players.All(p=>match.Tee(p.PlayerId))&&FindObjectsByType<GolfTee>(FindObjectsSortMode.None).Length==2,"CLIENT_DISPLAYS_BOTH_PLAYERS_FIXED_OPENING_TEES");
    yield return new WaitForSeconds(.2f);Check(Athlete.Active.Where(a=>match.Player(a)!=null).All(a=>a.GolfClubMotion.Equipped),"CLIENT_SEES_CLUB_ON_EVERY_MATCH_PLAYER");
    Check(app.view.BeginGolfSwing(42),"CLIENT_STARTS_RELIABLE_GOLF_CHARGE");yield return new WaitForSeconds(.35f);yield return PoseFrame();Check(app.LocalAthlete.GolfClubMotion.TwoHanded&&app.LocalAthlete.GolfClubMotion.GripError<.025f,"CLIENT_LOCAL_TWO_HAND_GRIP");
    if(args.Contains("-golfSideSwing")){
     var actor=app.LocalAthlete;var direction=Vector3.ProjectOnPlane(match.Ball(0).Body.position-actor.transform.position,Vector3.up);
     File.AppendAllText(report,$"SIDE_ADDRESS position={actor.transform.position} ball={match.Ball(0).Body.position} yaw={actor.transform.eulerAngles.y:F3} error={Vector3.Angle(actor.transform.forward,direction):F3}\n");
     Check(Vector3.Angle(actor.transform.forward,direction)<2,"CLIENT_SIDE_APPROACH_AUTO_ADDRESSES_BALL");
    }
    app.view.CancelGolfSwing(42);app.view.RequestGolfSwing(.08f);
    bool independent=false,countdown=false,moving=false,ownHole2=false,recovered=false;until=Time.time+50;
    while(Time.time<until&&match.State.Phase!=GolfMatchPhase.Ended){
     var local=match.Player(app.LocalAthlete);var host=match.State.Player(0);independent|=local!=null&&host!=null&&local.CurrentHole==1&&host.CurrentHole==2;
     var ui=FindFirstObjectByType<GolfLeaderboardUI>();countdown|=ui&&ui.CountdownVisible;moving|=match.Ball(0)&&Vector3.ProjectOnPlane(match.Ball(0).Body.position-shotStart,Vector3.up).magnitude>.05f;
     var own=match.Ball(app.LocalAthlete);recovered|=local!=null&&local.CurrentHole==1&&own&&own.ResetSequence>=ownReset+2&&Vector3.Distance(own.Body.position,match.TeePosition(1,own.Owner))<.05f;
     ownHole2|=local!=null&&local.CurrentHole==2;yield return null;
    }
    Check(independent&&ownHole2,"CLIENT_SEES_DIFFERENT_TARGETS_AND_CONTINUES_DURING_COUNTDOWN");Check(countdown&&moving,"CLIENT_BALL_POSES_AND_SHARED_COUNTDOWN_VISIBLE");
    Check(recovered,"CLIENT_RECEIVES_WRONG_CUP_RECOVERY_POSE_AND_RESET");
    var final=match.Player(app.LocalAthlete);Check(match.State.Phase==GolfMatchPhase.Ended&&final.IsDNF&&final.CompletedHoleCount==1&&final.TotalStroke==1,"CLIENT_RECEIVES_AUTHORITATIVE_DNF_AND_STROKES");Check(match.State.Results()[0].player.PlayerId==0,"CLIENT_RESULTS_FINISHED_BEFORE_DNF");
   }
  }
 }
}
#endif
