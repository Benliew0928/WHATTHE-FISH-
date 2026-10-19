#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using Unity.Netcode.Components;

namespace WhatTheFish {
 public sealed partial class GolfPolishProbe {
  IEnumerator PersonalRankingPresentation(){
   var ui=FindFirstObjectByType<GolfLeaderboardUI>();var badge=GolfFinalCountdown.Create(hudRoot());yield return null;
   Check(match.State.Phase==GolfMatchPhase.Idle&&!ui.BoardRect.gameObject.activeInHierarchy,"NO_RANKING_BEFORE_START_GAME");
   ui.ToggleButton.onClick.Invoke();yield return null;Check(!ui.Expanded&&!ui.BackdropButton.gameObject.activeSelf,"IDLE_CANNOT_OPEN_INVISIBLE_RANKING");CaptureUI("before-start-game",badge,30,match.Now+30,match.Now,false);
   yield return StartMatch();yield return new WaitForSeconds(.5f);var actor=AppRoot.Instance.LocalAthlete;var local=match.Player(actor);ulong id=local.PlayerId;string name=local.PlayerName;
   Check(ui.BoardRect.gameObject.activeInHierarchy&&ui.BoardRect.anchorMin==Vector2.one,"START_GAME_SHOWS_EXISTING_RIGHT_CORNER_STYLE");
   Check(ui.VisiblePlayerCount==1&&ui.DisplayedPlayerIds.Single()==id,"COLLAPSED_ONLY_SHOWS_LOCAL_PLAYER");
   var footer=ui.BoardRect.Find("Golf ranking toggle");Check(!footer.gameObject.activeSelf,"COLLAPSED_VIEW_ALL_FOOTER_REMOVED");
   Check(ui.DividersVisible()&&ui.RowsReadable()&&ui.FitsSafeFrame(),"EXISTING_SELF_ROW_ART_AND_COLUMNS_READABLE");float height=ui.RenderedHeight;CaptureUI("personal-ranking",badge,30,match.Now+30,match.Now,false);
   var roster=Enumerable.Range(101,8).Where(i=>(ulong)i!=id).Take(7).Select(i=>new GolfEntrant((ulong)i,"Player "+i)).Concat(new[]{new GolfEntrant(id,name)}).ToArray();
   match.State.Clear();Check(match.State.Start(roster,match.Now),"EIGHT_PLAYER_DISPLAY_FIXTURE_STARTED");match.State.RecordSwing(id,match.Now);yield return null;yield return new WaitForEndOfFrame();
   Check(ui.VisiblePlayerCount==1&&ui.DisplayedPlayerIds.Single()==id,"OTHER_PLAYERS_NEVER_APPEAR_WHILE_COLLAPSED");
   Check(Mathf.Abs(ui.RenderedHeight-height)<.1f,"MORE_PLAYERS_DO_NOT_GROW_COLLAPSED_BOARD");
   Check(ui.BoardRect.GetComponentsInChildren<UnityEngine.UI.Text>().Any(t=>t.name=="Player 0 Player"&&t.text.StartsWith("8.")),"LOCAL_ROW_KEEPS_ACTUAL_RANK");
   Check(ui.BoardRect.GetComponentsInChildren<UnityEngine.UI.Text>().Any(t=>t.name=="Player 0 Strokes"&&t.text=="1"),"LOCAL_SCORE_UPDATES");CaptureUI("personal-eighth-place",badge,30,match.Now+30,match.Now,false);
   ui.ToggleButton.onClick.Invoke();yield return null;yield return new WaitForEndOfFrame();
   Check(ui.Expanded&&ui.VisiblePlayerCount==8&&ui.RankingScroll.vertical,"TITLE_OPENS_UNCHANGED_FULL_SCROLLING_LIST");
   Check(footer.gameObject.activeSelf&&ui.DividersVisible()&&ui.FitsSafeFrame(),"EXPANDED_HEADER_COLUMNS_AND_CLOSE_FOOTER_RETAINED");CaptureUI("full-ranking",badge,30,match.Now+30,match.Now,false);
   ui.ToggleButton.onClick.Invoke();yield return null;Check(!ui.Expanded&&ui.VisiblePlayerCount==1&&ui.DisplayedPlayerIds.Single()==id,"FOOTER_CLOSE_RETURNS_TO_SELF_ONLY");
   ui.ToggleButton.onClick.Invoke();yield return null;ui.BackdropButton.onClick.Invoke();yield return null;Check(!ui.Expanded&&ui.VisiblePlayerCount==1,"BACKDROP_CLOSE_RETURNS_TO_SELF_ONLY");
   match.State.Clear();match.State.Start(new[]{new GolfEntrant(id,name)},match.Now);yield return null;
   var ball=match.Ball(actor);var origin=RefineOpenGround();ball.Place(origin+Vector3.up*(GolfBall.Radius+.003f),true);RefinePlace(actor,origin+Vector3.left*.8f);yield return new WaitForSeconds(.7f);
   Check(AppRoot.Instance.view.BeginGolfAim(),"AIM_ENTERED");yield return new WaitForSeconds(.3f);Check(!ui.BoardRect.gameObject.activeInHierarchy,"AIM_STILL_HIDES_RANKING");AppRoot.Instance.view.CancelGolfAim();yield return new WaitForSeconds(.4f);Check(ui.BoardRect.gameObject.activeInHierarchy&&ui.VisiblePlayerCount==1,"AIM_EXIT_RESTORES_ONLY_SELF");
   ui.ToggleButton.onClick.Invoke();yield return null;match.State.Clear();yield return null;Check(!ui.BoardRect.gameObject.activeInHierarchy&&!ui.Expanded&&!ui.BackdropButton.gameObject.activeSelf,"CLEAR_MATCH_HIDES_BOARD_AND_BACKDROP");
   Destroy(badge.gameObject);
  }
  IEnumerator RankingRollbackPresentation(){
   yield return StartMatch();yield return new WaitForSeconds(.5f);
   var ui=FindFirstObjectByType<GolfLeaderboardUI>();var badge=GolfFinalCountdown.Create(hudRoot());var actor=AppRoot.Instance.LocalAthlete;var local=match.Player(actor);ulong id=local.PlayerId;string playerName=local.PlayerName;
   Check(!ui.Expanded&&ui.BoardRect.anchorMin==Vector2.one,"ORIGINAL_RIGHT_CORNER_PREVIEW_RESTORED");
   Check(ui.VisiblePlayerCount==1&&ui.DividersVisible()&&ui.RowsReadable(),"ORIGINAL_FIVE_COLUMN_PREVIEW_VISIBLE");
   Check(!ui.BoardRect.Find("Open your full ranking")&&!ui.BoardRect.Find("Golf compact rank"),"SIMPLIFIED_CARDS_REMOVED");
   CaptureUI("original-ranking-preview",badge,30,match.Now+30,match.Now,false);
   var roster=Enumerable.Range(101,8).Where(i=>(ulong)i!=id).Take(7).Select(i=>new GolfEntrant((ulong)i,"Player "+i)).Concat(new[]{new GolfEntrant(id,playerName)}).ToArray();
   match.State.Clear();Check(match.State.Start(roster,match.Now),"DISPLAY_ROSTER_STARTED");yield return null;yield return new WaitForEndOfFrame();
   Check(ui.VisiblePlayerCount==4&&ui.DisplayedPlayerIds.SequenceEqual(roster.Take(3).Select(p=>p.id).Concat(new[]{id})),"PREVIEW_RETAINS_TOP_THREE_PLUS_LOCAL_PLAYER");
   Check(ui.FitsSafeFrame()&&ui.RowsReadable(),"ORIGINAL_PREVIEW_FITS_SAFE_FRAME");CaptureUI("original-ranking-multiplayer-preview",badge,30,match.Now+30,match.Now,false);
   ui.ToggleButton.onClick.Invoke();yield return null;yield return new WaitForEndOfFrame();
   Check(ui.Expanded&&ui.VisiblePlayerCount==8&&ui.RankingScroll.vertical,"ORIGINAL_FULL_SCROLLING_RANKING_RETAINED");
   var fullLabels=ui.BoardRect.GetComponentsInChildren<UnityEngine.UI.Text>().Where(t=>t.name.StartsWith("Player ")&&t.enabled).ToArray();
   // RectMask2D legitimately culls offscreen text in the scrolling list.
   Check(ui.FitsSafeFrame()&&fullLabels.All(t=>t.fontSize>=16)&&fullLabels.Any(t=>t.cachedTextGenerator.vertexCount>0),"FULL_RANKING_VISIBLE_TEXT_READABLE");CaptureUI("original-ranking-expanded",badge,30,match.Now+30,match.Now,false);
   ui.BackdropButton.onClick.Invoke();yield return null;Check(!ui.Expanded&&ui.VisiblePlayerCount==4,"CLOSE_RETURNS_TO_ORIGINAL_PREVIEW");
   match.State.Clear();match.State.Start(new[]{new GolfEntrant(id,playerName)},match.Now);yield return null;
   var ball=match.Ball(actor);var origin=RefineOpenGround();ball.Place(origin+Vector3.up*(GolfBall.Radius+.003f),true);RefinePlace(actor,origin+Vector3.left*.8f);yield return new WaitForSeconds(.7f);
   Check(AppRoot.Instance.view.BeginGolfAim(),"AIM_ACCEPTED");yield return new WaitForSeconds(.4f);
   var indicator=FindFirstObjectByType<GolfTargetIndicator>();var arrow=indicator.transform.Find("Your target hole") as RectTransform;
   float arrowBottom=arrow.anchoredPosition.y-arrow.rect.height*.5f,cardTop=indicator.TargetCard.anchoredPosition.y+indicator.TargetCard.rect.height*indicator.TargetCard.localScale.y*.5f;
   Check(Mathf.Abs(arrowBottom-cardTop+4)<.1f,"RECENT_AIM_ARROW_SPACING_RETAINED");Check(!ui.BoardRect.gameObject.activeInHierarchy,"AIM_STILL_HIDES_RANKING");AppRoot.Instance.view.CancelGolfAim();yield return new WaitForSeconds(.4f);
   Check(ui.BoardRect.gameObject.activeInHierarchy,"AIM_EXIT_RESTORES_ORIGINAL_RANKING");
   badge.Show(true,14,match.Now+14,match.Now);badge.ReviewAge(2);badge.Show(true,14,match.Now+14,match.Now);yield return null;
   Check(Vector2.Distance(badge.RenderedDigitSize,new Vector2(50,50))<.01f,"50_BY_50_COUNTDOWN_RETAINED");
   Check(Vector2.Distance(badge.RenderedSecondsSize,new Vector2(91,100)*(50f/142))<.01f,"PROPORTIONAL_S_RETAINED");CaptureUI("retained-countdown",badge,14,match.Now+14,match.Now);badge.Show(false,0,0,0);
   ball.Place(origin+Vector3.up*2,true);ball.Strike(Vector3.forward*8+Vector3.up*4);yield return new WaitForSeconds(.15f);var ribbon=ball.Trail.Ribbon;float last=1;
   Check(ribbon.widthCurve.Evaluate(0)==1&&ribbon.widthCurve.Evaluate(1)==0&&ribbon.numCapVertices==0,"CENTRED_THICK_TO_THIN_TRAIL_RETAINED");
   for(int i=1;i<=20;i++){float value=ribbon.widthCurve.Evaluate(i/20f);Check(value<=last,"TRAIL_MONOTONIC_"+i);last=value;}
   Destroy(badge.gameObject);
  }
  Vector3 RefineGround(Vector3 at){
   var island=match.GetComponentInParent<RefinedIslandEnvironment>();var ray=new Ray(at+Vector3.up*50,Vector3.down);
   foreach(var c in island.GetComponentsInChildren<MeshCollider>().Where(c=>c.name.StartsWith("Terrain__")))if(c.Raycast(ray,out var hit,100))return hit.point;
   throw new Exception("Refinement ground fixture missing");
  }
  Vector3 RefineOpenGround(){
   var tee=match.Course.holes[0].tee.position;
   for(int ring=1;ring<8;ring++)for(int k=0;k<16;k++){
    var p=RefineGround(tee+Quaternion.Euler(0,k*22.5f,0)*Vector3.forward*(ring*4));
    if(!match.Course.SafeDryPosition(p,3)||match.HoleTriggers.Any(h=>Vector3.Distance(h.transform.position,p)<5))continue;
    if(Physics.OverlapSphere(p+Vector3.up,2.3f,1<<8,QueryTriggerInteraction.Ignore).Any(c=>!GolfCartMotor.Terrain(c)))continue;
    if(!Physics.Raycast(p+Vector3.up,Vector3.down,out var hit,3,1<<8)||hit.normal.y<.99f)continue;
    return p;
   }
   throw new Exception("No clear circular orbit fixture");
  }
  void RefinePlace(Athlete actor,Vector3 at){
   actor.capsule.enabled=false;actor.transform.position=RefineGround(at)+Vector3.up*.035f;actor.ResetLocomotion();actor.capsule.enabled=match.Authority;
   var nt=actor.GetComponent<NetworkTransform>();if(nt&&nt.IsSpawned&&nt.IsServer)nt.Teleport(actor.transform.position,actor.transform.rotation,Vector3.one);Physics.SyncTransforms();
  }
  IEnumerator RefineOffline(Athlete actor,GolfBall ball){
   var app=AppRoot.Instance;var origin=RefineOpenGround();ball.Place(origin+Vector3.up*(GolfBall.Radius+.003f),true);RefinePlace(actor,origin+Vector3.left*.8f);
   yield return new WaitForSeconds(.7f);app.view.yaw=0;DevelopmentProbe.TurnCommand=new PlayerCommand{heading=0};
   Check(app.view.BeginGolfAim(),"OWN_BALL_CAN_ENTER_AIM");yield return new WaitForSeconds(.2f);Check(app.view.GolfAiming,"LOCAL_AIM_ACKNOWLEDGED");
   var anchor=ball.Body.position;float radius=Vector3.ProjectOnPlane(actor.transform.position-anchor,Vector3.up).magnitude;
   foreach(float heading in new[]{-90f,0,90,180,270,360}){
    app.view.yaw=heading;DevelopmentProbe.TurnCommand=new PlayerCommand{heading=heading};float until=Time.time+2.5f;float error=180;
    while(Time.time<until){yield return new WaitForFixedUpdate();var offset=actor.transform.position-anchor;error=Mathf.Abs(Mathf.DeltaAngle(Mathf.Atan2(offset.z,-offset.x)*Mathf.Rad2Deg,heading));if(error<1)break;}
    Check(error<1,"AIM_ORBITS_TO_CAMERA_HEADING_"+heading);
    Check(Mathf.Abs(Vector3.ProjectOnPlane(actor.transform.position-anchor,Vector3.up).magnitude-radius)<.025f,"AIM_ORBIT_RADIUS_RETAINED_"+heading);
    Check(Vector3.Distance(ball.Body.position,anchor)<.015f,"AIM_ORBIT_NEVER_MOVES_BALL_"+heading);
   }
   var obstacle=GameObject.CreatePrimitive(PrimitiveType.Cube);obstacle.name="Review orbit obstruction";obstacle.layer=8;obstacle.transform.position=RefineGround(anchor+Vector3.forward*radius)+Vector3.up*.9f;obstacle.transform.localScale=new Vector3(.35f,1.8f,.35f);Physics.SyncTransforms();
   app.view.yaw=90;DevelopmentProbe.TurnCommand=new PlayerCommand{heading=90};yield return new WaitForSeconds(.8f);
   var blockedOffset=actor.transform.position-anchor;Check(Mathf.Abs(Mathf.DeltaAngle(Mathf.Atan2(blockedOffset.z,-blockedOffset.x)*Mathf.Rad2Deg,90))>8,"ORBIT_STOPS_BEFORE_OBSTACLE_INSTEAD_OF_TELEPORTING");
   Check(Vector3.Distance(ball.Body.position,anchor)<.015f,"BLOCKED_ORBIT_LEAVES_BALL_FIXED");Destroy(obstacle);yield return null;yield return new WaitForSeconds(.5f);
   var indicator=UnityEngine.Object.FindFirstObjectByType<GolfTargetIndicator>();yield return new WaitForEndOfFrame();
   Check(indicator.LargeTargetArrowVisible,"AIM_RETAINS_SAME_HOLE_ARROW");var arrow=indicator.transform.Find("Your target hole") as RectTransform;
   Check(arrow&&Vector2.Distance(arrow.sizeDelta,app.GetComponentInChildren<GolfHUD>(true)?.LayoutConfig.targetArrowSize??GolfHUD.Instance.LayoutConfig.targetArrowSize)<100,"AIM_ARROW_EXISTS_WITH_RESPONSIVE_SIZE");
   File.AppendAllText(report,$"MEASURE aimWanted={app.view.GolfAimRequested} accepted={app.view.GolfAiming} serverAim={match.IsAiming(actor)} canSwing={match.CanSwing(actor)} canStrike={match.CanStrike(actor,ball)} grounded={actor.Grounded} arrow={arrow.sizeDelta} expected={GolfHUD.Instance.LayoutConfig.targetArrowSize*GolfHUD.Instance.LayoutConfig.aimTargetScale} actor={actor.transform.position} ball={ball.Body.position}\n");
   Check(Vector2.Distance(arrow.sizeDelta,GolfHUD.Instance.LayoutConfig.targetArrowSize*GolfHUD.Instance.LayoutConfig.aimTargetScale)<.01f,"AIM_ARROW_SMALLER_BY_EXISTING_AIM_SCALE");
   if(args.Contains("-polishPictures")){var badge=GolfFinalCountdown.Create(hudRoot());badge.Show(false,0,0,0);CaptureUI("aim-orbit-arrow",badge,30,match.Now+30,match.Now,false);Destroy(badge.gameObject);}
   // Charge remains anchored while look heading moves the golfer.
   Check(app.view.BeginGolfSwing(),"CHARGING_STARTS_FROM_ORBITED_ADDRESS");app.view.yaw=90;DevelopmentProbe.TurnCommand=new PlayerCommand{heading=90};yield return new WaitForSeconds(.8f);
   Check(app.view.GolfCharging&&Vector3.Distance(ball.Body.position,anchor)<.015f,"CHARGING_ORBIT_KEEPS_BALL_FIXED");app.view.CancelGolfSwing(int.MinValue);app.view.CancelGolfAim();yield return null;
   ball.Place(origin+Vector3.up*1.5f,true);ball.Strike(Vector3.forward*8+Vector3.up*4);yield return new WaitForSeconds(.1f);
   Check(ball.Trail.Active&&ball.Trail.Ribbon.numCapVertices==0,"TRAIL_HAS_NO_FORWARD_ROUND_CAP");Check(ball.Trail.Ribbon.widthCurve.Evaluate(0)==1,"TRAIL_THICKEST_AT_BALL_CENTRE");
   float previousWidth=1;for(int k=1;k<=20;k++){float width=ball.Trail.Ribbon.widthCurve.Evaluate(k/20f);Check(width<=previousWidth,"TRAIL_CONTINUOUSLY_NARROWS_"+k);previousWidth=width;}
   for(int i=0;i<8;i++){app.view.yaw=i*45;yield return new WaitForEndOfFrame();Check(Vector3.Distance(ball.Trail.Ribbon.transform.position,ball.transform.position)<.00001f,"TRAIL_EMITTER_CENTRED_FROM_CAMERA_"+i);}
   if(args.Contains("-polishPictures")){
    CaptureWorld("trail-from-ball-centre",ball.transform.position);
    var water=match.Course.waterPatches[1];var centre=match.transform.TransformPoint(water.centre);
    Check(GolfCartMotor.Surface(centre,0,out var position,out var rotation),"CART_WATER_PICTURE_SUPPORT");
    var cart=UnityEngine.Object.Instantiate(Resources.Load<GolfCart>("GolfCart"),match.transform);cart.Apply(new GolfCartState{summoned=true,position=position,rotation=rotation,driver=GolfCartState.NoDriver},true);yield return null;CaptureWorld("cart-on-pond",position);Destroy(cart.gameObject);yield return null;
    CaptureWorld("flag-wind",match.Course.holes[0].cup.position);
   }
   ball.Place(match.TeePosition(1,ball.Owner),true);RefinePlace(actor,match.Course.holes[0].tee.position-Vector3.forward*3);DevelopmentProbe.TurnCommand=default;yield return new WaitForSeconds(.3f);
  }
  IEnumerator RefineCountdown(GolfFinalCountdown badge,double end,double now){
   badge.ReviewAge(.6f);int refreshes=badge.DigitRefreshes;var positions=new System.Collections.Generic.HashSet<float>();
   // Freeze the supplied network timestamp: rendered motion must still advance.
   for(int i=0;i<22;i++){badge.Show(true,29,end,now+.6);positions.Add(badge.Card.anchoredPosition.x);yield return null;}
   Check(positions.Count>=18,"COUNTDOWN_MOVES_EVERY_RENDER_FRAME_WITH_STEPPED_SERVER_TIME");
   Check(badge.DigitRefreshes<=refreshes+1,"COUNTDOWN_DIGITS_REFRESH_ONLY_WHEN_SECOND_CHANGES");
   badge.ReviewAge(1.3f);badge.Show(true,29,end,now+1.3);
   Check(Vector2.Distance(badge.RenderedDigitSize,new Vector2(50,50))<.01f,"CORNER_NUMERALS_EXACTLY_50_BY_50");
   Check(Vector2.Distance(badge.RenderedSecondsSize,new Vector2(91,100)*(50f/142))<.01f,"SECONDS_S_SCALED_PROPORTIONALLY");
   Check(badge.Card.GetComponent<Canvas>().overrideSorting,"COUNTDOWN_USES_ISOLATED_CANVAS_BATCH");
   int before=badge.DigitRefreshes;long allocated=GC.GetAllocatedBytesForCurrentThread();var watch=System.Diagnostics.Stopwatch.StartNew();
   for(int i=0;i<1000;i++)badge.Show(true,29,end,now+1.3);
   watch.Stop();long bytes=GC.GetAllocatedBytesForCurrentThread()-allocated;
   Check(badge.DigitRefreshes==before,"STEADY_COUNTDOWN_DOES_NOT_REBUILD_DIGITS");File.AppendAllText(report,$"MEASURE 1000CountdownUpdatesMs={watch.Elapsed.TotalMilliseconds:F3} managedBytes={bytes}\n");
   yield return null;
  }
  string RefineSignal(string name)=>Path.Combine(Path.GetDirectoryName(report),name);
  IEnumerator RefineHost(){
   var local=match.Player(AppRoot.Instance.LocalAthlete);var guest=match.State.Players.First(p=>p.PlayerId!=local.PlayerId);var actor=Athlete.Active.First(a=>match.Player(a)?.PlayerId==guest.PlayerId);
   var own=match.Ball(local.PlayerId);var other=match.Ball(guest.PlayerId);var origin=RefineOpenGround();
   RefinePlace(AppRoot.Instance.LocalAthlete,origin+Vector3.right*12);other.Place(origin+Vector3.up*(GolfBall.Radius+.003f),true);RefinePlace(actor,origin+Vector3.left*.8f);yield return new WaitForSeconds(.8f);
   File.WriteAllText(RefineSignal("orbit-ready"),"ready");float until=Time.time+18;while(!File.Exists(RefineSignal("orbit-done"))&&Time.time<until)yield return null;Check(File.Exists(RefineSignal("orbit-done")),"REMOTE_ORBIT_STAGE_COMPLETED");
   match.ReleaseAim(actor);other.Place(origin+Vector3.forward*12+Vector3.up*GolfBall.Radius,true);own.Place(origin+Vector3.up*(GolfBall.Radius+.003f),true);RefinePlace(actor,origin+Vector3.left*.704f);yield return new WaitForSeconds(.5f);
   Check(match.CanSwing(actor)&&actor.GolfClubMotion.CanAddress(own.Body.position),"FOREIGN_ATTACK_FIXTURE_IN_VALID_SWING_RANGE");
   Check(match.AimCandidate(actor)==null&&!match.CanStrike(actor,own),"HOST_REJECTS_FOREIGN_BALL_CANDIDATE_AND_STRIKE");
   Check(!match.SetAim(actor,true,own.Owner,match.Round,0),"HOST_REJECTS_FOREIGN_AIM_REQUEST");uint sequence=own.StrikeSequence;int strokes=guest.TotalStroke;
   Check(!match.TrySwing(actor,own.Owner,0,.8f,match.Round),"HOST_REJECTS_DIRECT_FOREIGN_SWING");File.WriteAllText(RefineSignal("forge-ready"),own.Owner.ToString());
   until=Time.time+8;while(!File.Exists(RefineSignal("forge-done"))&&Time.time<until)yield return null;Check(File.Exists(RefineSignal("forge-done")),"REMOTE_FORGED_RPC_STAGE_COMPLETED");yield return new WaitForSeconds(.3f);
   Check(!match.IsAiming(actor)&&own.StrikeSequence==sequence&&guest.TotalStroke==strokes,"FOREIGN_RPCS_CANNOT_MOVE_BALL_OR_ADD_STROKES");
  }
  IEnumerator RefineClient(){
   var app=AppRoot.Instance;float until=Time.time+16;while(!File.Exists(RefineSignal("orbit-ready"))&&Time.time<until)yield return null;Check(File.Exists(RefineSignal("orbit-ready")),"REMOTE_ORBIT_FIXTURE_RECEIVED");yield return new WaitForSeconds(.8f);
   app.view.yaw=0;DevelopmentProbe.TurnCommand=new PlayerCommand{heading=0};Check(app.view.BeginGolfAim(),"REMOTE_CAN_AIM_OWN_BALL");until=Time.time+3;while(!app.view.GolfAiming&&Time.time<until)yield return null;Check(app.view.GolfAiming,"REMOTE_OWN_AIM_ACKNOWLEDGED");
   var ball=match.Ball(app.LocalAthlete);var origin=ball.Body.position;float radius=Vector3.ProjectOnPlane(app.LocalAthlete.transform.position-origin,Vector3.up).magnitude;
   foreach(float heading in new[]{-90f,90f}){
    app.view.yaw=heading;DevelopmentProbe.TurnCommand=new PlayerCommand{heading=heading};yield return new WaitForSeconds(2.3f);
    var offset=app.LocalAthlete.transform.position-origin;Check(Mathf.Abs(Mathf.DeltaAngle(Mathf.Atan2(offset.z,-offset.x)*Mathf.Rad2Deg,heading))<4,"REMOTE_SEES_AUTHORITY_ORBIT_"+heading);
    Check(Mathf.Abs(Vector3.ProjectOnPlane(offset,Vector3.up).magnitude-radius)<.05f&&Vector3.Distance(ball.Body.position,origin)<.03f,"REMOTE_ORBIT_PRESERVES_BALL_CENTRE_"+heading);
   }
   app.view.CancelGolfAim();DevelopmentProbe.TurnCommand=default;yield return new WaitForSeconds(.2f);File.WriteAllText(RefineSignal("orbit-done"),"done");
   until=Time.time+8;while(!File.Exists(RefineSignal("forge-ready"))&&Time.time<until)yield return null;Check(File.Exists(RefineSignal("forge-ready")),"REMOTE_FOREIGN_RPC_FIXTURE_RECEIVED");yield return new WaitForSeconds(.5f);
   ulong owner=ulong.Parse(File.ReadAllText(RefineSignal("forge-ready")));var net=app.LocalAthlete.GetComponent<NetworkAthlete>();
   net.GolfAimRpc(true,owner,match.Round,0);net.GolfChargeRpc(true,owner,0,match.Round);net.GolfSwingRpc(owner,0,.8f,match.Round);yield return new WaitForSeconds(.5f);Check(!match.IsAiming(app.LocalAthlete),"REMOTE_FOREIGN_AIM_NOT_ACKNOWLEDGED");File.WriteAllText(RefineSignal("forge-done"),"done");
  }
 }
}
#endif
