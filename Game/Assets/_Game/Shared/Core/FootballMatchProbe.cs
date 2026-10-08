#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
namespace WhatTheFish {
 public sealed class FootballMatchProbe:MonoBehaviour {
  public string ReportPath;int count;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Init(){var args=Environment.GetCommandLineArgs();if(!args.Contains("-footballMatchAudit"))return;var probe=new GameObject("Football match checks").AddComponent<FootballMatchProbe>();int i=Array.IndexOf(args,"-report");probe.ReportPath=i>=0?args[i+1]:Path.Combine(Application.persistentDataPath,"football-match.txt");}
  void Check(bool value,string name){File.AppendAllText(ReportPath,(value?"PASS ":"FAIL ")+name+"\n");if(!value)throw new Exception(name);count++;}
  void Rules(){
   var state=new FootballMatchState();state.Start(0,180,60,3);
   Check(state.Phase==FootballMatchPhase.Kickoff&&!state.Goal(FootballTeam.A,1),"COUNTDOWN_NO_SCORE");state.Advance(2);Check(state.Phase==FootballMatchPhase.Kickoff,"FULL_COUNTDOWN");state.Advance(3);Check(state.Deadline==183,"REGULATION_180_AFTER_KICKOFF");
   Check(state.Goal(FootballTeam.B,13)&&state.ScoreB==1&&state.Phase==FootballMatchPhase.GoalReset,"ONE_POINT_REGULATION_GOAL");Check(!state.Goal(FootballTeam.B,14)&&state.ScoreB==1,"DUPLICATE_REJECTED");
   Check(state.Remaining(90)==170,"GOAL_PROCESSING_CLOCK_PAUSED");state.ResetCompleted(90);Check(state.Remaining(91)==2,"RESTART_COUNTDOWN_3");state.Advance(93);Check(state.Deadline==263,"REGULATION_REMAINING_PRESERVED");state.Advance(262.9);Check(state.Phase==FootballMatchPhase.Regulation,"LEAD_DOES_NOT_END_EARLY");state.Advance(263);Check(state.Result==FootballMatchResult.TeamB,"LEADER_WINS_AT_DEADLINE");Check(!state.Goal(FootballTeam.A,264)&&state.ScoreA==0,"FINISHED_SCORE_IMMUTABLE");
   state.Start(0,10,60,0);state.Advance(0);Check(!state.Goal(FootballTeam.A,10),"AT_REGULATION_DEADLINE_NOT_REGULATION_GOAL");state.Advance(10);Check(state.Phase==FootballMatchPhase.Overtime&&state.Deadline==70,"DIRECT_OVERTIME_60_SECONDS");Check(state.Goal(FootballTeam.A,10.1)&&state.Result==FootballMatchResult.TeamA,"GOLDEN_GOAL_IMMEDIATE_WIN");
   state.SelectTeams(0,10,180,60,3);Check(!state.Goal(FootballTeam.A,5)&&state.Remaining(5)==5,"SELECTION_NO_SCORE_CLOCK");state.Advance(10);Check(state.Phase==FootballMatchPhase.TeamSelection,"SELECTION_REQUIRES_AUTHORITY_CONFIRMATION");state.TeamsConfirmed(10);state.Advance(13);Check(state.Deadline==193,"SELECTION_DOES_NOT_CONSUME_REGULATION");
   state.Start(0,1,1,0);state.Advance(0);state.Advance(1);state.Advance(1);state.Advance(2);Check(state.Result==FootballMatchResult.Draw,"OVERTIME_TIMEOUT_DRAW");state.Start(0,10,60,0);state.Advance(0);Check(state.ScoreA==0&&state.ScoreB==0&&state.Result==FootballMatchResult.None,"NEW_MATCH_CLEARS_RESULT");Check(state.Goal(FootballTeam.A,9.999),"BEFORE_DEADLINE_COUNTS");state.Clear();Check(state.Phase==FootballMatchPhase.Idle&&state.ScoreA==0,"CLEAR_MATCH");
   state.Start(0,10,60,0);state.Advance(0);state.Goal(FootballTeam.A,1);state.ResetCompleted(1);state.Advance(1);state.Goal(FootballTeam.B,2);state.ResetCompleted(2);state.Advance(2);state.Advance(11);state.Advance(11);Check(state.ScoreA==1&&state.ScoreB==1&&state.Phase==FootballMatchPhase.Overtime,"EXTRA_PRESERVES_REGULATION_SCORE");
  }
  void Teams(){
   var selection=new FootballTeamSelection();
   Check(!selection.Begin(new ulong[]{1})&&!selection.Begin(new ulong[]{1,2,3}),"ODD_ROSTER_REJECTED");
   for(int size=2;size<=10;size+=2){
    var players=Enumerable.Range(1,size).Select(i=>(ulong)i).ToArray();selection.Begin(players);Check(selection.Capacity==size/2,"CAPACITY_"+size);
    for(int i=0;i<size/2;i++)Check(selection.Choose(players[i],FootballTeam.A),"FILL_A_"+size+"_"+i);
    Check(!selection.Choose(players[size/2],FootballTeam.A)&&selection.Count(FootballTeam.A)==size/2,"FULL_REJECTS_OVERFLOW_"+size);
    Check(selection.Choose(players[0],FootballTeam.A)&&selection.Count(FootballTeam.A)==size/2,"FULL_EXISTING_MEMBER_REPEAT_"+size);
    selection.Complete(new System.Random(size));Check(selection.Count(FootballTeam.A)==size/2&&selection.Count(FootballTeam.B)==size/2,"CAPACITY_BOUNDED_RANDOM_"+size);
    foreach(var team in new[]{FootballTeam.A,FootballTeam.B}){
     var slots=selection.AllocateKickoffSlots(team,new System.Random(size));
     Check(slots.Length==size/2&&slots.Distinct().Count()==size/2&&slots.All(id=>selection.TeamOf(id)==team),"FIXED_SLOTS_ONE_EACH_CORRECT_TEAM_"+size+"_"+team);
    }
    Check(selection.TeamOf(players[0])==FootballTeam.A&&!selection.Choose(players[0],FootballTeam.B),"PRESERVE_AND_LOCK_"+size);
   }
   selection.Begin(new ulong[]{1,2,3,4,5,6});selection.Choose(1,FootballTeam.A);selection.Choose(2,FootballTeam.A);selection.Choose(3,FootballTeam.A);selection.Choose(4,FootballTeam.B);selection.Complete(new System.Random(2));
   Check(selection.TeamOf(5)==FootballTeam.B&&selection.TeamOf(6)==FootballTeam.B,"THREE_V_THREE_ONLY_REMAINING_B_SEATS");
   selection.Begin(new ulong[]{1,2,3,4});selection.Choose(1,FootballTeam.A);selection.Choose(1,FootballTeam.B);Check(selection.Count(FootballTeam.A)==0&&selection.Count(FootballTeam.B)==1,"SWITCH_RELEASES_OLD_SEAT");
   Check(!selection.Choose(99,FootballTeam.A)&&!selection.Choose(1,FootballTeam.None),"UNKNOWN_AND_INVALID_TEAM_REJECTED");
   for(int seed=0;seed<100;seed++){selection.Begin(new ulong[]{1,2,3,4,5,6});selection.Choose(1,FootballTeam.A);selection.Choose(2,FootballTeam.B);selection.Complete(new System.Random(seed));if(selection.Count(FootballTeam.A)!=3||selection.Count(FootballTeam.B)!=3||selection.TeamOf(1)!=FootballTeam.A||selection.TeamOf(2)!=FootballTeam.B)throw new Exception("random balance");}
   Check(true,"ONE_HUNDRED_RANDOM_ALLOCATIONS_BALANCED_SELECTED_PRESERVED");
   Check(!selection.Begin(Enumerable.Range(0,12).Select(i=>(ulong)i)),"UNSUPPORTED_SIX_V_SIX_REJECTED");
  }
  void Geometry(){
   var bounds=new Bounds(new Vector3(0,1.22f,1),new Vector3(7.32f,2.44f,2));var r=Vector3.one*.22f;
   foreach(int sign in new[]{-1,1}){
    var detector=new FootballGoalDetector(bounds,0,sign);double at;
    detector.Reset(new Vector3(0,.5f,-sign),0);Check(!detector.Sample(new Vector3(0,.5f,-sign*.05f),r,1,out at),"HALF_BALL_NO_GOAL_"+sign);Check(!detector.Sample(new Vector3(0,.5f,sign*.1f),r,2,out at),"CENTRE_PAST_LINE_NO_GOAL_"+sign);Check(!detector.Sample(new Vector3(0,.5f,sign*.22f),r,3,out at),"TRAILING_EDGE_TOUCHING_NO_GOAL_"+sign);Check(detector.Sample(new Vector3(0,.5f,sign*.3f),r,4,out at),"WHOLE_BALL_COUNTS_"+sign);Check(!detector.Sample(new Vector3(0,.5f,-sign*.1f),r,5,out at),"BOUNCE_BACK_NO_SECOND_EVENT_"+sign);Check(!detector.Sample(new Vector3(0,.5f,sign*.5f),r,6,out at),"STAYS_NET_NO_REPEAT_"+sign);
    detector.Reset(new Vector3(0,.5f,sign),0);Check(!detector.Sample(new Vector3(0,.5f,-sign*.1f),r,1,out at),"BACK_ENTRY_REJECTED_"+sign);
    detector.Reset(new Vector3(4,.5f,-sign),0);Check(!detector.Sample(new Vector3(4,.5f,sign),r,1,out at),"OUTSIDE_POST_REJECTED_"+sign);Check(!detector.Sample(new Vector3(0,.5f,sign),r,2,out at),"SIDE_ENTRY_REJECTED_"+sign);
    detector.Reset(new Vector3(0,3,-sign),0);Check(!detector.Sample(new Vector3(0,3,sign),r,1,out at),"ABOVE_BAR_REJECTED_"+sign);Check(!detector.Sample(new Vector3(0,.5f,sign),r,2,out at),"DROPPING_IN_NET_REJECTED_"+sign);
    detector.Reset(new Vector3(3.5f,.5f,-sign),0);Check(!detector.Sample(new Vector3(3.5f,.5f,sign),r,1,out at),"SPHERE_POST_OVERLAP_REJECTED_"+sign);
    detector.Reset(new Vector3(0,2.3f,-sign),0);Check(!detector.Sample(new Vector3(0,2.3f,sign),r,1,out at),"SPHERE_BAR_OVERLAP_REJECTED_"+sign);
    detector.Reset(new Vector3(0,.5f,-sign),9);Check(detector.Sample(new Vector3(0,.5f,sign),r,10,out at)&&at<10&&at>9,"SWEPT_FAST_CROSSING_TIME_"+sign);
   }
  }
  void Formations(){
   var match=FootballMatch.Instance;var ball=FootballBall.Instance;var centre=ball.KickoffPosition;
   for(int size=1;size<=5;size++){
    var slots=match.rules.Formation(size);Check(slots!=null&&slots.Length==size,"INDEPENDENT_FORMATION_"+size);
    for(int slot=0;slot<size;slot++){
     bool a=match.TryKickoffPose(FootballTeam.A,size,slot,out var pa,out var ra),b=match.TryKickoffPose(FootballTeam.B,size,slot,out var pb,out var rb);
     Check(a&&b,"FORMATION_GROUNDED_IN_BOUNDS_"+size+"_"+slot);
     var da=Vector3.ProjectOnPlane(pa-centre,Vector3.up);var db=Vector3.ProjectOnPlane(pb-centre,Vector3.up);
     Check((da+db).magnitude<.01f&&Mathf.Abs(da.magnitude-db.magnitude)<.01f,"FORMATION_MIRRORED_EQUAL_RACE_"+size+"_"+slot);
     Check(Vector3.Dot(ra*Vector3.forward,-da.normalized)>.999f&&Vector3.Dot(rb*Vector3.forward,-db.normalized)>.999f,"FORMATION_FACES_EXISTING_BALL_"+size+"_"+slot);
     Check(da.magnitude>ball.controlDistance+1,"FORMATION_OUTSIDE_CONTROL_RANGE_"+size+"_"+slot);
     for(int other=0;other<slot;other++){match.TryKickoffPose(FootballTeam.A,size,other,out var p,out _);Check(Vector3.Distance(pa,p)>1,"FORMATION_DISTINCT_SLOTS_"+size+"_"+slot+"_"+other);}
    }
   }
   var frame=ball.Pitch;var oldPosition=frame.position;var oldRotation=frame.rotation;
   match.TryKickoffPose(FootballTeam.A,3,1,out var original,out _);var local=frame.InverseTransformPoint(original);
   try {
    frame.SetPositionAndRotation(oldPosition+new Vector3(12,0,-7),Quaternion.AngleAxis(73,Vector3.up)*oldRotation);Physics.SyncTransforms();
    Check(match.TryKickoffPose(FootballTeam.A,3,1,out var moved,out var facing)&&Vector3.Distance(moved,frame.TransformPoint(local))<.03f,"FORMATION_ROTATED_TRANSLATED_PITCH");
    Check(Vector3.Dot(facing*Vector3.forward,Vector3.ProjectOnPlane(ball.KickoffPosition-moved,Vector3.up).normalized)>.999f,"FORMATION_ROTATED_FACING");
   }finally{frame.SetPositionAndRotation(oldPosition,oldRotation);Physics.SyncTransforms();}
   Check(Vector3.Distance(ball.KickoffPosition,centre)<.001f,"FORMATION_NEVER_CHANGES_BALL_ANCHOR");
  }
  IEnumerator WaitPlaying(){while(!FootballMatch.Instance||!FootballMatch.Instance.Running)yield return new WaitForFixedUpdate();}
  IEnumerator SelectSportReady(AppRoot app,SportId sport){
   app.SelectSport(sport);float until=Time.realtimeSinceStartup+20;
   while(app.SelectedSport!=sport&&Time.realtimeSinceStartup<until)yield return null;
   Check(app.SelectedSport==sport,"STREAMED_SPORT_READY_"+sport);yield return new WaitForFixedUpdate();
  }
  IEnumerator ResetPresentation(string goal){
   var ball=FootballBall.Instance;
   var match=FootballMatch.Instance;var actor=AppRoot.Instance.LocalAthlete;
   Check(match.TryKickoffPose(match.TeamOf(actor),1,0,out var pose,out _)&&Vector3.Distance(actor.transform.position,pose)<.03f,"GOAL_RETURNS_TO_FIXED_SLOT_"+goal);
   Check(Vector3.Distance(ball.Body.position,ball.KickoffPosition)<.001f,"GOAL_USES_EXISTING_BALL_ANCHOR_"+goal);
   if(Application.isBatchMode)yield return null;else yield return new WaitForEndOfFrame();
   float gap=Vector3.Distance(ball.transform.position,ball.Body.position);
   File.AppendAllText(ReportPath,"RESET_RENDER "+goal+" gap="+gap.ToString("F4")+" body="+ball.Body.position+" visual="+ball.transform.position+" sleeping="+ball.Body.IsSleeping()+"\n");
   Check(gap<.01f,"RESET_RENDER_AT_CENTRE_"+goal);
   Check(ball.GetComponentsInChildren<Renderer>().Any(r=>r.enabled&&r.gameObject.activeInHierarchy&&Vector3.Distance(r.bounds.center,ball.Body.position)<ball.WorldRadius),"RESET_MESH_AT_CENTRE_"+goal);
   yield return WaitPlaying();yield return new WaitForSeconds(.1f);if(Application.isBatchMode)yield return null;else yield return new WaitForEndOfFrame();
   Check(Vector3.Distance(ball.transform.position,ball.Body.position)<.01f,"RESTART_VISIBLE_WITHOUT_CONTACT_"+goal);
   if(!Application.isBatchMode){
    // View centre from the side so the kickoff athlete does not occlude the ball.
    float yaw=PlayerView.Instance.yaw;PlayerView.Instance.yaw=30;yield return null;yield return new WaitForEndOfFrame();
    string dir=Path.GetDirectoryName(ReportPath);Directory.CreateDirectory(dir);var path=Path.Combine(dir,"restart-"+goal+".png");var probe=FindFirstObjectByType<DevelopmentProbe>();if(probe)probe.CaptureFrame(path);else ScreenCapture.CaptureScreenshot(path);
    yield return null;yield return new WaitForEndOfFrame();PlayerView.Instance.yaw=yaw;
   }
  }
  IEnumerator Shoot(int index){var ball=FootballBall.Instance;int sign=ball.GoalSign(index);float line=sign*Mathf.Max(sign*ball.GoalFront(index),sign*(sign>0?ball.PitchBounds.max.z+.06f:ball.PitchBounds.min.z-.06f));ball.Body.position=ball.Pitch.TransformPoint(new Vector3(0,ball.PitchBounds.max.y+.24f,line-sign));if(ball.Body.isKinematic){yield return new WaitForFixedUpdate();ball.Body.position=ball.Pitch.TransformPoint(new Vector3(0,ball.PitchBounds.max.y+.24f,line+sign));yield return new WaitForFixedUpdate();yield break;}ball.Body.linearVelocity=Vector3.zero;yield return new WaitForFixedUpdate();ball.Body.linearVelocity=ball.Pitch.TransformDirection(new Vector3(0,0,sign*12));float until=Time.time+1;while(Time.time<until&&FootballMatch.Instance.Running)yield return new WaitForFixedUpdate();
   if(FootballMatch.GoalFollowThrough){
    var match=FootballMatch.Instance;int score=match.State.ScoreA+match.State.ScoreB;double started=match.Now,remaining=match.Remaining;
    var surface=ball.Pitch.GetComponentsInChildren<FootballNetSurface>().First(n=>n.Sign==sign);uint impacts=surface.ImpactCount;
    Check(!ball.Body.isKinematic&&FootballMatch.BlocksActions&&!ball.TryKick(AppRoot.Instance.LocalAthlete),"GOAL_FOLLOW_THROUGH_NO_NEW_KICKS_"+sign);
    bool golden=match.State.Phase==FootballMatchPhase.Finished;
    if(golden)Check(match.State.Result!=FootballMatchResult.None,"GOLDEN_GOAL_RESULT_PRECEDES_NET_IMPACT");
    yield return new WaitForSeconds(.1f);
    Check(Math.Abs(match.Remaining-remaining)<.001,"CLOCK_PAUSED_DURING_NET_FOLLOW_THROUGH_"+sign);
    while(FootballMatch.GoalFollowThrough)yield return new WaitForFixedUpdate();
    File.AppendAllText(ReportPath,"FOLLOW_THROUGH_END phase="+match.State.Phase+" remaining="+match.Remaining.ToString("R")+" countdown="+match.rules.kickoffSeconds.ToString("R")+" elapsed="+(match.Now-started).ToString("R")+"\n");
    Check(surface.ImpactCount>impacts,"SCORED_BALL_REACHES_NET_"+sign);
    Check(match.Now-started<=.68&&match.State.ScoreA+match.State.ScoreB==score,"BOUNDED_FOLLOW_THROUGH_NO_DOUBLE_SCORE_"+sign);
    if(golden){yield return new WaitForFixedUpdate();Check(ball.Body.isKinematic&&Vector3.Distance(ball.Body.position,ball.KickoffPosition)>10,"GOLDEN_GOAL_FREEZES_AT_NET_WITHOUT_RESET");}
    else Check(match.State.Phase==FootballMatchPhase.Kickoff&&match.Remaining<=match.rules.kickoffSeconds+.001,"FOLLOW_THROUGH_THEN_NORMAL_KICKOFF_"+sign);
   }
  }
  IEnumerator Start(){
   Directory.CreateDirectory(Path.GetDirectoryName(ReportPath));File.WriteAllText(ReportPath,"Football match checks\n");Rules();Geometry();Teams();
   while(!AppRoot.Instance||!FootballBall.Instance||!AppRoot.Instance.LocalAthlete)yield return null;
   var app=AppRoot.Instance;app.EnterOffline();DevelopmentProbe.TurnCommandActive=true;DevelopmentProbe.TurnCommand=default;yield return new WaitForSeconds(.2f);
   var match=FootballMatch.Instance;var actor=app.LocalAthlete;var ball=FootballBall.Instance;
   Check(match.Snapshot.phase==FootballMatchPhase.Idle,"EXPLORATION_HAS_NO_MATCH");Check(match.rules.regulationSeconds==180&&match.rules.overtimeSeconds==60&&match.rules.kickoffSeconds==3,"CENTRAL_DEFAULTS");Formations();var kickoffAnchor=ball.KickoffPosition;
   var original=match.rules;match.rules=Instantiate(original);match.rules.regulationSeconds=3;match.rules.overtimeSeconds=1;match.rules.kickoffSeconds=.2f;match.rules.teamSelectionSeconds=.3f;
   actor.capsule.enabled=false;actor.transform.SetPositionAndRotation(ball.Pitch.TransformPoint(new Vector3(0,ball.PitchBounds.max.y+.07f,-.8f)),ball.Pitch.rotation);actor.ResetLocomotion();actor.capsule.enabled=true;Physics.SyncTransforms();PlayerView.Instance.RequestKick(1);PlayerView.Instance.BeginKick();
   Check(!match.StartMatch(),"NORMAL_START_REJECTS_SINGLE_PLAYER");Check(match.StartTestMatch(),"EXPLICIT_SINGLE_PLAYER_TEST_START");Check(!PlayerView.Instance.ReadCommand().kick&&!PlayerView.Instance.Charging,"START_CLEARS_QUEUED_KICK_AND_CHARGE");Check(match.State.Phase==FootballMatchPhase.TeamSelection&&match.TeamOf(actor)==FootballTeam.None,"NEW_MATCH_STARTS_UNSELECTED");
   uint selectionRevision=match.Snapshot.revision;Check(match.ChooseTeam(actor,FootballTeam.A,selectionRevision),"LOCAL_TEAM_REQUEST_ACCEPTED");Check(match.ChooseTeam(actor,FootballTeam.A,selectionRevision)&&match.Snapshot.teamA==1,"REPEATED_CHOICE_ONE_SEAT");
   yield return new WaitForSeconds(.1f);Check(match.State.Phase==FootballMatchPhase.TeamSelection,"ALL_CHOSEN_STILL_WAITS_FOR_DEADLINE");
   while(match.State.Phase==FootballMatchPhase.TeamSelection)yield return new WaitForFixedUpdate();
   Check(match.TryKickoffPose(FootballTeam.A,1,0,out var kickoffPose,out _)&&Vector3.Distance(actor.transform.position,kickoffPose)<.03f,"FIRST_KICKOFF_FIXED_ONE_PLAYER_SLOT");
   Check(Vector3.Distance(ball.Body.position,kickoffAnchor)<.001f&&ball.CurrentController==null,"FIRST_KICKOFF_EXISTING_BALL_POINT_NO_OWNER");
   Check(match.TeamOf(actor)==FootballTeam.A&&!match.ChooseTeam(actor,FootballTeam.B,selectionRevision),"SELECTED_TEAM_RETAINED_AND_LOCKED");var start=actor.transform.position;actor.Simulate(new PlayerCommand{move=Vector2.up,sprint=true,kick=true,tackle=true,jump=true},.1f);
   Check(Vector3.Distance(start,actor.transform.position)<.001f&&!actor.TryKick()&&!actor.TryTackle()&&!actor.CanRequestJump,"COUNTDOWN_BLOCKS_ALL_MATCH_ACTIONS");Check(ball.Body.isKinematic,"COUNTDOWN_BALL_FROZEN");yield return WaitPlaying();Check(!ball.Body.isKinematic,"KICKOFF_RESTORES_PHYSICS");
   int north=Enumerable.Range(0,ball.GoalCount).First(i=>ball.GoalSign(i)>0);int south=1-north;
   yield return Shoot(south);Check(match.State.ScoreB==1&&match.State.ScoreA==0,"OWN_GOAL_SCORES_OPPOSING_TEAM");Check(match.State.Phase==FootballMatchPhase.Kickoff,"GOAL_RESTART_COUNTDOWN");Check(ball.Body.linearVelocity==Vector3.zero&&ball.Body.angularVelocity==Vector3.zero,"RESET_CLEARS_MOMENTUM");Check(ball.CurrentController==null&&!PlayerView.Instance.Charging&&actor.Action==FootballAction.None,"RESET_CLEARS_TEMPORARY_STATE");Check(Mathf.Abs(ball.Pitch.InverseTransformPoint(ball.Body.position).z)<.01f,"RESET_TO_CENTRE");yield return ResetPresentation("SOUTH");yield return Shoot(north);Check(match.State.ScoreA==1&&match.State.ScoreB==1,"BOTH_GOALS_SCORE_CORRECT_TEAM");yield return ResetPresentation("NORTH");
   // Prepare a live charge and rolling/spinning ball immediately across the deadline.
   while(match.Now<match.State.Deadline-.06)yield return new WaitForFixedUpdate();
   actor.capsule.enabled=false;actor.transform.SetPositionAndRotation(ball.Pitch.TransformPoint(new Vector3(0,ball.PitchBounds.max.y+.07f,-1)),ball.Pitch.rotation);actor.ResetLocomotion();actor.capsule.enabled=true;
   ball.Body.position=actor.transform.position+actor.transform.forward*.9f+Vector3.up*.25f;ball.Body.linearVelocity=actor.transform.forward*2;ball.Body.angularVelocity=new Vector3(2,3,4);Physics.SyncTransforms();
   ball.RefreshControl(actor);Check(ball.CurrentController==actor,"POSSESSION_BEFORE_OVERTIME_CHARGE");
   Check(PlayerView.Instance.BeginKick(),"CHARGE_BEFORE_OVERTIME");uint liveRevision=match.Snapshot.revision;var actorBefore=actor.transform.position;
   while(match.State.Phase==FootballMatchPhase.Regulation)yield return new WaitForFixedUpdate();
   Check(match.State.Phase==FootballMatchPhase.Overtime&&!FootballMatch.BlocksMovement&&!FootballMatch.BlocksActions,"SEAMLESS_OVERTIME_NO_FREEZE");
   Check(PlayerView.Instance.Charging&&match.Snapshot.revision==liveRevision,"OVERTIME_PRESERVES_CHARGE_AND_REQUEST_REVISION");
   Check(ball.CurrentController==actor&&ball.Body.isKinematic&&Vector3.Distance(ball.Body.position,ball.FootPosition(actor))<.002f&&Vector3.Distance(actorBefore,actor.transform.position)<.01f,"OVERTIME_PRESERVES_ATTACHED_CONTROL_AND_PLAYER_POSITION");
   PlayerView.Instance.EndKick();Check(PlayerView.Instance.ReadCommand().kick,"CHARGE_RELEASE_AFTER_OVERTIME");Check(actor.TryKick(.5f),"ATTACHED_CHARGE_RELEASE_IN_OVERTIME");ball.ResetBall();yield return Shoot(north);
   Check(match.State.Result==FootballMatchResult.TeamA&&match.State.Phase==FootballMatchPhase.Finished,"ACTUAL_PITCH_GOLDEN_GOAL_WIN");
   start=actor.transform.position;actor.Simulate(new PlayerCommand{move=Vector2.up,sprint=true},.2f);Check(Vector3.Distance(start,actor.transform.position)>.05f&&!FootballMatch.BlocksMovement&&FootballMatch.BlocksActions,"FINISHED_CAN_RUN_ACTIONS_REMAIN_BLOCKED");
   int finalScore=match.State.ScoreA;yield return Shoot(north);Check(match.State.ScoreA==finalScore&&match.State.Phase==FootballMatchPhase.Finished,"FINISHED_GOAL_NO_SCORE_OR_RESET");
   Check(match.StartTestMatch()&&match.State.ScoreA==0&&match.State.ScoreB==0&&match.TeamOf(actor)==FootballTeam.None,"RESTART_FRESH_SCORE_AND_UNSELECTED_TEAM");
   Check(match.ChooseTeam(actor,FootballTeam.A,match.Snapshot.revision),"NEW_SELECTION_REQUEST");yield return WaitPlaying();
   while(match.Now<match.State.Deadline-.06)yield return new WaitForFixedUpdate();
   Check(actor.TryTackle(),"SLIDE_BEFORE_OVERTIME");liveRevision=match.Snapshot.revision;
   while(match.State.Phase==FootballMatchPhase.Regulation)yield return new WaitForFixedUpdate();
   Check(actor.Action==FootballAction.Slide&&actor.ActionProgress>0&&match.Snapshot.revision==liveRevision,"SLIDE_CONTINUES_IN_OVERTIME");
   uint priorMatchRevision=match.Snapshot.revision;match.ClearMatch();match.rules=original;yield return SelectSportReady(app,SportId.Basketball);Check(!FootballMatch.BlocksActions,"OTHER_SPORT_UNBLOCKED");yield return SelectSportReady(app,SportId.Football);Check(FootballMatch.Instance&&FootballMatch.Instance.State.Phase==FootballMatchPhase.Idle,"RETURN_NO_OLD_MATCH");
   Check(FootballMatch.Instance.Snapshot.revision!=priorMatchRevision,"CROSS_SCENE_ROUND_REVISION_UNIQUE");
   DevelopmentProbe.TurnCommandActive=false;File.AppendAllText(ReportPath,"MATCH_COMPLETE checks="+count+"\n");
  }
 }
}
#endif

