#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace WhatTheFish {
 // Explicit opt-in presentation review. Synthetic score rows never run in normal play.
 public sealed class GolfHUDReview:MonoBehaviour {
  string report;int checks;AppRoot app;GolfMatchManager match;GolfHUD hud;GolfLeaderboardUI table;GolfSwingButton swing;Athlete actor;GolfBall ball;RectTransform safe;
  bool insetActive,savedAppEnabled;Vector2 savedMin,savedMax,savedOffsetMin,savedOffsetMax;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)] static void Boot(){if(Environment.GetCommandLineArgs().Contains("-golfHudAudit"))new GameObject("Golf HUD presentation review").AddComponent<GolfHUDReview>();}
  void Record(string value){File.AppendAllText(report,value+Environment.NewLine);Debug.Log("GOLF_HUD "+value);}
  void Check(bool value,string name){Record((value?"PASS ":"FAIL ")+name);if(!value)throw new Exception(name);checks++;}
  static IEnumerator Frame(){if(SystemInfo.graphicsDeviceType==UnityEngine.Rendering.GraphicsDeviceType.Null)yield return null;else yield return new WaitForEndOfFrame();}
  Text FindText(string name)=>table.GetComponentInParent<Canvas>().GetComponentsInChildren<Text>(true).FirstOrDefault(t=>t.name==name);
  static bool Overlaps(RectTransform a,RectTransform b){var ac=new Vector3[4];var bc=new Vector3[4];a.GetWorldCorners(ac);b.GetWorldCorners(bc);var ar=new Rect(ac[0].x,ac[0].y,ac[2].x-ac[0].x,ac[2].y-ac[0].y);var br=new Rect(bc[0].x,bc[0].y,bc[2].x-bc[0].x,bc[2].y-bc[0].y);return ar.Overlaps(br);}
  void RoutedPress(Button button,PointerEventData pointer,string tag){
   pointer.position=RectTransformUtility.WorldToScreenPoint(null,button.transform.position);var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(pointer,hits);
   Check(hits.Count>0&&hits[0].gameObject==button.gameObject,"ACTUAL_RAYCAST_REACHES_FIXED_BUTTON_"+tag);
   var image=button.GetComponent<Image>();Check(image&&image.enabled&&image.raycastTarget&&image.color.a==0,"TRANSPARENT_FIXED_TOUCH_TARGET_"+tag);
   ExecuteEvents.Execute(button.gameObject,pointer,ExecuteEvents.pointerDownHandler);
  }
  void CheckLayout(string tag){
   Canvas.ForceUpdateCanvases();Check(hud.FitsSafeFrame()&&table.FitsSafeFrame(),"ALL_GOLF_FRAMES_INSIDE_SAFE_AREA_"+tag);
   var config=Resources.Load<GolfHUDLayout>("GolfUILayout");Check(table.AimSummary?table.RenderedHeight==config.boardAimSummaryHeight:table.Expanded||table.RenderedHeight<=safe.rect.height*config.boardCompactHeightFraction+1,"BOUNDED_RANKING_HEIGHT_AT_EVERY_REVIEWED_LAYOUT_"+tag);
   Check(hud.RegionsSeparate()&&!Overlaps(table.BoardRect,(RectTransform)hud.JumpButton.transform),"SCORECARD_AND_TOUCH_CONTROLS_SEPARATE_"+tag);
   Check(hud.TypographyClean()&&table.RowsReadable(),"LIVE_NATIVE_TEXT_HAS_READABLE_GEOMETRY_"+tag);Check(table.AimSummary||table.DividersVisible(),"REFERENCE_COLUMN_HEADER_VISIBLE_OUTSIDE_AIM_"+tag);Check(hud.GetComponentInParent<Canvas>().GetComponentsInChildren<Image>(true).Any(i=>i.name=="Reference Hint"&&i.sprite),"REFERENCE_HINT_ATTENTION_ART_AVAILABLE_"+tag);
   var cells=table.BoardRect.GetComponentsInChildren<Text>().Where(t=>t.name.StartsWith("Player ")).ToArray();
   Check(cells.Length==table.VisiblePlayerCount*5&&cells.Where(t=>t.enabled).All(t=>t.fontStyle==FontStyle.Normal&&(t.font==CoveUI.TextFont||t.font==CoveUI.DisplayFont)&&t.cachedTextGenerator.vertexCount>0),"VISIBLE_PLAYER_LIVE_CELLS_RENDER_SHARP_"+tag);
  }
  Vector3 Floor(Vector3 at){if(Physics.Raycast(at+Vector3.up*4,Vector3.down,out var hit,8,1<<8,QueryTriggerInteraction.Ignore))return hit.point;throw new Exception("Golf presentation fixture has no floor");}
  void Place(Vector3 at){actor.capsule.enabled=false;actor.transform.position=Floor(at)+Vector3.up*.035f;actor.ResetLocomotion();actor.capsule.enabled=true;Physics.SyncTransforms();}
  void Insets(Vector2 lower,Vector2 upper){
   if(insetActive)RestoreInsets();savedAppEnabled=app.enabled;app.enabled=false;savedMin=safe.anchorMin;savedMax=safe.anchorMax;savedOffsetMin=safe.offsetMin;savedOffsetMax=safe.offsetMax;
   safe.anchorMin=savedMin+new Vector2(lower.x/Screen.width,lower.y/Screen.height);safe.anchorMax=savedMax-new Vector2(upper.x/Screen.width,upper.y/Screen.height);safe.offsetMin=safe.offsetMax=Vector2.zero;insetActive=true;
  }
  void RestoreInsets(){if(!insetActive)return;if(safe){safe.anchorMin=savedMin;safe.anchorMax=savedMax;safe.offsetMin=savedOffsetMin;safe.offsetMax=savedOffsetMax;}if(app)app.enabled=savedAppEnabled;insetActive=false;}
  void Capture(string label){
   if(SystemInfo.graphicsDeviceType==UnityEngine.Rendering.GraphicsDeviceType.Null)return;
   string path=Path.ChangeExtension(report,label+".png");var canvases=FindObjectsByType<Canvas>(FindObjectsSortMode.None).Where(c=>c.renderMode==RenderMode.ScreenSpaceOverlay).ToArray();var cameras=canvases.Select(c=>c.worldCamera).ToArray();var distances=canvases.Select(c=>c.planeDistance).ToArray();var camera=Camera.main;
   try{
    foreach(var canvas in canvases){canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=.5f;}
    // The shared helper fixes 1600x900. Other aspect ratios need their actual tested dimensions.
    if(Screen.width==1600&&Screen.height==900)FindFirstObjectByType<DevelopmentProbe>()?.CaptureFrame(path);
    else {
     var target=RenderTexture.GetTemporary(Screen.width,Screen.height,24);var previous=camera.targetTexture;var active=RenderTexture.active;var texture=new Texture2D(Screen.width,Screen.height,TextureFormat.RGB24,false);
     try{camera.targetTexture=target;Canvas.ForceUpdateCanvases();camera.Render();RenderTexture.active=target;texture.ReadPixels(new Rect(0,0,Screen.width,Screen.height),0,0);texture.Apply();File.WriteAllBytes(path,texture.EncodeToPNG());}
     finally{camera.targetTexture=previous;RenderTexture.active=active;Destroy(texture);RenderTexture.ReleaseTemporary(target);}
    }
   }finally{for(int i=0;i<canvases.Length;i++){canvases[i].renderMode=RenderMode.ScreenSpaceOverlay;canvases[i].worldCamera=cameras[i];canvases[i].planeDistance=distances[i];}Canvas.ForceUpdateCanvases();}
   Record("CAPTURE "+Path.GetFileName(path)+" dimensions="+Screen.width+"x"+Screen.height);
  }
  IEnumerator Start(){
   var args=Environment.GetCommandLineArgs();int index=Array.IndexOf(args,"-report");report=index>=0&&index+1<args.Length?args[index+1]:Path.Combine(Application.persistentDataPath,"golf-hud.txt");Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(report)));File.WriteAllText(report,"");
   DevelopmentProbe.TurnCommandActive=true;DevelopmentProbe.TurnCommand=default;
   float until=Time.realtimeSinceStartup+65;while((!AppRoot.Instance||!AppRoot.Instance.Exploring||AppRoot.Instance.SelectedSport!=SportId.Golf||!GolfMatchManager.Instance||!GolfHUD.Instance||!FindFirstObjectByType<GolfLeaderboardUI>())&&Time.realtimeSinceStartup<until)yield return null;
   app=AppRoot.Instance;match=GolfMatchManager.Instance;hud=GolfHUD.Instance;table=FindFirstObjectByType<GolfLeaderboardUI>();swing=FindFirstObjectByType<GolfSwingButton>();actor=app?app.LocalAthlete:null;
   Check(app&&match&&hud&&table&&swing&&actor&&match.Context&&match.Authority&&!app.rooms.Connected,"OFFLINE_GOLF_HUD_READY");Check(SystemInfo.graphicsDeviceType!=UnityEngine.Rendering.GraphicsDeviceType.Null,"REVIEW_USES_RENDERED_PLAYER");
   GolfFeedback.Instance.ReviewFocus(true);
   safe=table.GetComponentInParent<Canvas>().GetComponentsInChildren<RectTransform>(true).First(r=>r.name=="Safe area");yield return new WaitForSeconds(.6f);yield return Frame();
   Check(match.State.Phase==GolfMatchPhase.Idle&&swing.startButton.gameObject.activeInHierarchy&&swing.startButton.interactable&&!table.CountdownVisible,"IDLE_SHOWS_START_WITHOUT_ORDINARY_TIMER");
   Check(!swing.startLabel.enabled&&(swing.startButton.targetGraphic as Image).sprite==Resources.Load<Sprite>("GolfUI/StartButton/StartGame"),"START_GAME_ORIGINAL_IMAGE_NO_DUPLICATE_TEXT");
   Check(!hud.CameraButton.gameObject.activeInHierarchy&&hud.JumpButton.transform.position.y>hud.ActionRect.position.y,"CAMERA_REMOVED_JUMP_ABOVE_SWING");
   var carts=FindObjectsByType<GolfCartButton>(FindObjectsSortMode.None);Check(carts.Length==2&&carts.First(c=>c.summon).button.interactable,"ROAMING_SUMMON_CONTROL_AVAILABLE");CheckShotArt("AimFirst");Capture("idle-course");
   Check(carts.All(c=>!c.label.text.Contains("[")&&!c.label.text.Contains("]")),"CART_LABELS_WITHOUT_KEYBOARD_SUFFIX");
   var previewHeader=table.BoardRect.GetComponentsInChildren<Image>().First(i=>i.sprite==Resources.Load<Sprite>("GolfUI/Leaderboard/Columns"));
   var previewTitle=table.BoardRect.GetComponentsInChildren<Image>().First(i=>i.sprite==Resources.Load<Sprite>("GolfUI/Leaderboard/Title"));
   Check(Mathf.Abs(previewHeader.rectTransform.rect.width/previewHeader.rectTransform.rect.height-previewHeader.sprite.rect.width/previewHeader.sprite.rect.height)<.01f,"COMPACT_COLUMN_ART_KEEPS_ORIGINAL_ASPECT");
   Check(previewTitle.rectTransform.parent.GetComponent<RectTransform>().rect.width>=320,"COMPACT_TITLE_HAS_LARGER_NATURAL_PROPORTIONS");
   var use=carts.First(c=>!c.summon);CheckCartArt(carts.First(c=>c.summon),"Buggy");
   carts.First(c=>c.summon).Press();yield return new WaitForSeconds(.6f);var cart=GolfCartWorld.Owned(actor);Place(cart.transform.position+cart.transform.right*1.6f);yield return new WaitForSeconds(.2f);yield return Frame();Check(use.button.interactable,"DRIVE_CONTROL_VISIBLE_NEAR_CART");Check(carts.First(c=>c.summon).label.text=="Recall"&&use.label.text=="Drive","DRIVE_RECALL_NATIVE_WORDS_ONLY");CheckCartArt(use,"Drive");CheckCartArt(carts.First(c=>c.summon),"Recall");Capture("drive-recall");use.Press();yield return new WaitForSeconds(.5f);yield return Frame();CheckCartArt(use,"Exit");Capture("exit-cart");use.Press();yield return new WaitForSeconds(.5f);
   swing.startButton.onClick.Invoke();yield return new WaitForSeconds(.2f);Check(match.State.Running&&match.Player(actor)!=null&&match.Ball(actor),"EXISTING_START_BUTTON_STARTS_REAL_MATCH");ball=match.Ball(actor);ulong localId=match.Player(actor).PlayerId;GolfFeedbackChecks.Run(GolfFeedback.Instance,match,Check);
   // Audit startup skips the normal wait. Restore only its real state deadline
   // to exercise both countdown presentations without changing release rules.
   match.State.Clear();Check(match.State.Start(new[]{new GolfEntrant(localId,"You")},match.Now,5),"COUNTDOWN_PRESENTATION_FIXTURE_USES_REAL_START_DEADLINE");
   yield return new WaitForSeconds(.2f);yield return Frame();
   var countdown=hud.GetComponentsInChildren<Image>(true).Single(i=>i.name=="Golf countdown badge");
   Check(match.Starting&&countdown.gameObject.activeInHierarchy&&countdown.sprite&&countdown.sprite.name.StartsWith("Countdown_"),"CENTRAL_LARGE_START_COUNTDOWN_ART_RETAINED");
   var targetText=FindText("Your target");Check(targetText&&targetText.text.Contains("HOLE 1")&&FindText("Target distance").text.EndsWith(" m")&&!targetText.text.Contains("STARTS"),"UPPER_HOLE_CARD_KEEPS_DISTANCE_WITHOUT_START_COUNTDOWN");Capture("start-countdown-distance-only");
   while(match.Starting)yield return null;yield return Frame();Check(countdown.gameObject.activeInHierarchy&&countdown.sprite==Resources.Load<Sprite>("GolfUI/Countdown_GO"),"CENTRAL_GO_ANIMATION_RETAINED");
   var tee=match.Course.holes[0].tee;ball.Place(tee.position+Vector3.up*(GolfBall.Radius+.01f),true);Place(tee.position-tee.forward*2.6f);app.view.mode=1;app.view.yaw=tee.eulerAngles.y;app.view.pitch=24;yield return new WaitForSeconds(.7f);
   yield return Frame();Capture("reference-single-player");
   var previewRow=table.BoardRect.GetComponentsInChildren<RectTransform>().First(r=>r.name=="Golf score row 0");
   var compactStatus=FindText("Player 0 Status").rectTransform.parent.GetComponentsInChildren<Image>().First(i=>i.sprite==Resources.Load<Sprite>("GolfUI/Leaderboard/Playing"));
   Check(compactStatus.type==Image.Type.Simple&&compactStatus.preserveAspect&&!FindText("Player 0 Status").enabled,"COMPACT_PLAYING_USES_SAME_ROUNDED_RECTANGLE_AS_EXPANDED");
   Check(table.BoardRect.GetComponentsInChildren<CoveFeedback>(true).All(f=>!f.enabled),"RANKING_CHILD_BUTTONS_HAVE_NO_INDEPENDENT_SCALE_ANIMATION");
   Check(!table.Expanded&&previewRow.rect.height>=60&&table.RenderedHeight>=260&&table.BoardRect.localScale==Vector3.one,"SINGLE_PLAYER_PREVIEW_HAS_TALL_READABLE_ROW_WITHOUT_SQUEEZING");
   var indicator=FindFirstObjectByType<GolfTargetIndicator>();
   Check((hud.JumpButton.targetGraphic as Image).sprite==Resources.Load<Sprite>("GolfUI/Badges/Jump")&&(hud.JumpButton.targetGraphic as Image).preserveAspect,"JUMP_USES_SUPPLIED_IMAGE_WITH_ORIGINAL_ASPECT");
   indicator.enabled=false;
   foreach(float metres in new[]{5f,132f,1234f}){
    indicator.SetCaption(2,metres);Canvas.ForceUpdateCanvases();yield return Frame();
    var texts=new[]{FindText("Your target"),FindText("Target separator"),FindText("Target distance")};var edges=new Vector2[3];
    for(int i=0;i<3;i++){var verts=texts[i].cachedTextGenerator.verts;float lo=float.PositiveInfinity,hi=float.NegativeInfinity;for(int v=0;v+3<verts.Count;v+=4){float a=verts[v].position.x,b=verts[v+2].position.x;if(Mathf.Abs(b-a)<.001f)continue;lo=Mathf.Min(lo,Mathf.Min(a,b));hi=Mathf.Max(hi,Mathf.Max(a,b));}edges[i]=new Vector2(lo,hi)/texts[i].pixelsPerUnit+Vector2.one*texts[i].rectTransform.anchoredPosition.x;}
    float first=edges[1].x-edges[0].y,second=edges[2].x-edges[1].y;
    Check(first>0&&Mathf.Abs(first-second)<1.1f&&edges[0].x>=-65&&edges[2].y<=147,"HOLE_TWO_CLEAR_OF_FLAG_AND_EQUAL_DOT_GAPS_"+metres);Capture("hole-two-"+metres+"m");
   }
   indicator.enabled=true;yield return Frame();
   Check(indicator.BallCaptionVisible&&indicator.BallArrowVisible,"DISTANT_BALL_CAPTION_AND_ARROW_VISIBLE");
   var card=FindText("Your ball").rectTransform.parent.GetComponent<RectTransform>();var art=card.GetComponentsInChildren<Image>().First(i=>i.name=="Reference Ball");
   Check(art.type==Image.Type.Simple&&Mathf.Abs(card.rect.width/card.rect.height-art.sprite.rect.width/art.sprite.rect.height)<.02f,"BALL_ART_PRESERVES_ROUND_ASPECT");
   CheckBallText();
   Place(ball.Body.position-tee.forward*.9f);yield return new WaitForSeconds(.2f);yield return Frame();Check(!indicator.BallCaptionVisible&&indicator.BallArrowVisible,"NEAR_BALL_HIDES_CAPTION_ONLY");Capture("near-ball-arrow-only");
   Place(ball.Body.position-tee.forward*1.65f);yield return new WaitForSeconds(.2f);Check(!indicator.BallCaptionVisible&&indicator.BallArrowVisible,"NEAR_DISTANCE_HYSTERESIS_AVOIDS_FLASHING");
   Place(tee.position-tee.forward*2.6f);yield return new WaitForSeconds(.2f);yield return Frame();Check(indicator.BallCaptionVisible&&indicator.BallArrowVisible,"MOVING_AWAY_RESTORES_CAPTION");
   var puttAt=Floor(match.Course.holes[0].cup.position+tee.forward*5);ball.Place(puttAt+Vector3.up*(GolfBall.Radius+.01f),true);Place(puttAt-tee.forward*.9f);yield return new WaitForSeconds(.3f);
   swing.aimButton.onClick.Invoke();yield return new WaitForSeconds(.5f);yield return Frame();Check(app.view.GolfAiming&&app.view.GolfMode==GolfShotMode.Putt,"PUTT_UI_STATE_FROM_EXISTING_RULES");CheckShotArt("Putt");Check(hud.AimFocused&&hud.AuxiliaryOpacity==0,"PUTT_USES_SAME_CLEAN_FOCUS_HUD");Capture("putt-badge");
   Check(app.view.BeginGolfSwing(314),"PUTT_BEGINS_LOCAL_CHARGE_FEEDBACK");yield return new WaitForSeconds(.25f);yield return Frame();
   Check(GolfFeedback.Instance.ChargeLoopPlaying&&GolfFeedback.Instance.ChargeRingVisible&&GolfFeedback.Instance.EdgeOpacity==0,"PUTT_HAS_QUIET_CHARGE_RING_WITHOUT_EDGE_GLOW");Capture("putt-charging-feedback");app.view.CancelGolfSwing(314);yield return new WaitForSeconds(.15f);
   Check(!GolfFeedback.Instance.ChargeLoopPlaying&&!GolfFeedback.Instance.ChargeRingVisible,"CANCEL_STOPS_CHARGE_SOUND_AND_RING");swing.aimButton.onClick.Invoke();yield return new WaitForSeconds(.3f);Check(!hud.AimFocused&&hud.AuxiliaryOpacity==1,"CANCEL_RESTORES_ROAMING_HUD");
   ball.Place(tee.position+Vector3.up*(GolfBall.Radius+.01f),true);Place(tee.position-tee.forward*2.6f);yield return new WaitForSeconds(.3f);
   // The supplied 1448x1086 reference is 4:3. Use a window that fits a 1080p
   // desktop; requesting 1086 client pixels is otherwise clamped by Windows.
   Screen.SetResolution(1280,960,false);yield return new WaitForSeconds(.6f);yield return Frame();Check(Screen.width==1280&&Screen.height==960,"APPROVED_REFERENCE_ASPECT_APPLIED");Capture("reference-4x3");Screen.SetResolution(1600,900,false);yield return new WaitForSeconds(.6f);yield return Frame();
   Check(new[]{"Course","Hole","Arrow","Ball","BallArrow","Aim","Buggy","Jump","Camera","Leave","Hint","Shot"}.All(n=>Resources.Load<Sprite>("GolfUI/Reference/"+n)),"ALL_APPROVED_REFERENCE_SPRITES_IMPORTED");
   // Only score presentation is synthetic. The local player keeps the real actor/ball mapping.
   var smallRoster=Enumerable.Range(1,3).Select(i=>new GolfEntrant((ulong)i,"Friend "+i)).Append(new GolfEntrant(localId,"You")).ToArray();match.State.Clear();Check(match.State.Start(smallRoster,match.Now),"FOUR_PLAYER_PRESENTATION_ROSTER_STARTED");
   Screen.SetResolution(1920,900,false);yield return new WaitForSeconds(.6f);yield return null;yield return Frame();CheckLayout("four-player-wide-preview");Check(table.VisiblePlayerCount==4,"FOUR_PLAYER_PREVIEW_RETAINS_ALL_FIXED_ROWS");Capture("four-player-taller-preview");
   Screen.SetResolution(1600,900,false);yield return new WaitForSeconds(.6f);yield return Frame();

   var roster=Enumerable.Range(1,9).Select(i=>new GolfEntrant((ulong)i,i==9?"Long Teammate Display Name":"Friend "+i)).Append(new GolfEntrant(localId,"YOU")).ToArray();match.State.Clear();Check(match.State.Start(roster,match.Now),"SYNTHETIC_TEN_PLAYER_SCORECARD_STARTED");
   for(int i=1;i<10;i++){ulong id=(ulong)i;for(int shot=0;shot<i*2;shot++)match.State.RecordSwing(id,match.Now);for(int hole=1;hole<=i%4;hole++)match.State.EnterHole(id,hole,match.Now);}
   Record("FIXTURE scorecard_only=10 local_actor_and_ball=real additional_world_players=0");yield return null;yield return Frame();Check(match.State.Players.Count==10&&!table.CountdownVisible,"TEN_INDEPENDENT_PROGRESS_ROWS_HAVE_NO_TIMER_BEFORE_FINISH");CheckLayout("1600x900-roaming");Capture("ten-player-scorecard");
   var settings=Resources.Load<GolfHUDLayout>("GolfUILayout");
   Check(!table.Expanded&&table.VisiblePlayerCount==4&&table.DisplayedPlayerIds.SequenceEqual(new ulong[]{1,2,3,localId})&&table.DisplayedPlayerIds.Distinct().Count()==4,"COMPACT_SHOWS_FIRST_THREE_AND_LOCAL_OUTSIDE_TOP_THREE_ONCE");
   Check(table.RenderedHeight<=safe.rect.height*settings.boardCompactHeightFraction+1,"COMPACT_HEIGHT_STAYS_WITHIN_CONFIGURED_SCREEN_FRACTION");
   float previewHeight=table.RenderedHeight;
   Check(table.BoardRect.localScale.x==table.BoardRect.localScale.y&&table.BoardRect.GetComponentsInChildren<RectTransform>().First(r=>r.name=="Golf score row 0").rect.height>=60,"CROWDED_PREVIEW_SCALES_UNIFORMLY_WITHOUT_FLATTENING_ROWS");
   var rankPointer=new PointerEventData(EventSystem.current){pointerId=91,button=PointerEventData.InputButton.Left};RoutedPress(table.ToggleButton,rankPointer,"RANKING_EXPAND");ExecuteEvents.Execute(table.ToggleButton.gameObject,rankPointer,ExecuteEvents.pointerUpHandler);ExecuteEvents.Execute(table.ToggleButton.gameObject,rankPointer,ExecuteEvents.pointerClickHandler);yield return null;yield return Frame();
   Check(table.Expanded&&table.BackdropButton.gameObject.activeInHierarchy&&table.VisiblePlayerCount==10&&table.RankingScroll.vertical&&table.RankingScroll.verticalScrollbar.gameObject.activeInHierarchy&&table.FitsSafeFrame()&&table.RenderedHeight<=safe.rect.height*settings.boardExpandedHeightFraction+1,"EXPANDED_ALL_TEN_IN_FIXED_HEIGHT_NATIVE_SCROLL_VIEW");Capture("expanded-ten-player-ranking");
   Check(!FindText("Player 0 Player").text.Contains("…"),"COMMON_PLAYER_NAME_REMAINS_COMPLETE_IN_FULL_RANKING");
   var hover=table.BoardRect.GetComponent<GolfRankingHover>();var hoverPointer=new PointerEventData(EventSystem.current){pointerId=-93};
   bool motionBefore=MenuPreferences.ReducedMotion,hadMotion=PlayerPrefs.HasKey("cove.motion");int motionSetting=PlayerPrefs.GetInt("cove.motion");
   try{
    MenuPreferences.SetMotion(false);hover.OnPointerExit(hoverPointer);yield return new WaitForSeconds(.3f);Canvas.ForceUpdateCanvases();
    var parts=new[]{table.BoardRect.GetComponentsInChildren<RectTransform>().First(r=>r.name=="Golf leaderboard title"),table.BoardRect.GetComponentsInChildren<RectTransform>().First(r=>r.name=="Golf leaderboard frame"),table.BoardRect.GetComponentsInChildren<RectTransform>().First(r=>r.name=="Golf score row 0")};
    var beforeWidths=new float[parts.Length];for(int i=0;i<parts.Length;i++){var corners=new Vector3[4];parts[i].GetWorldCorners(corners);beforeWidths[i]=corners[2].x-corners[0].x;}
    hover.OnPointerEnter(hoverPointer);yield return new WaitForSeconds(.3f);yield return Frame();
    bool together=table.HoverScaleFactor>1.01f&&table.HoverScaleFactor<1.03f;
    for(int i=0;i<parts.Length;i++){var corners=new Vector3[4];parts[i].GetWorldCorners(corners);float ratio=(corners[2].x-corners[0].x)/beforeWidths[i];together&=Mathf.Abs(ratio-table.HoverScaleFactor)<.002f&&parts[i].localScale==Vector3.one;}
    Check(together&&table.FitsSafeFrame(),"HOVER_GROWS_TITLE_FRAME_AND_ROWS_TOGETHER_IN_SAFE_AREA");Capture("expanded-whole-panel-hover");
    hover.OnPointerExit(hoverPointer);yield return new WaitForSeconds(.35f);Check(Mathf.Abs(table.HoverScaleFactor-1)<.001f,"HOVER_EXIT_RESTORES_ENTIRE_PANEL_SCALE");
    MenuPreferences.SetMotion(true);hover.OnPointerEnter(hoverPointer);yield return new WaitForSeconds(.2f);Check(Mathf.Abs(table.HoverScaleFactor-1)<.001f,"WHOLE_PANEL_HOVER_RESPECTS_REDUCED_MOTION");hover.OnPointerExit(hoverPointer);
   }finally{MenuPreferences.SetMotion(motionBefore);if(hadMotion)PlayerPrefs.SetInt("cove.motion",motionSetting);else PlayerPrefs.DeleteKey("cove.motion");}

   var wheel=new PointerEventData(EventSystem.current){position=RectTransformUtility.WorldToScreenPoint(null,table.RowViewport.position),scrollDelta=new Vector2(0,-6)};ExecuteEvents.Execute(table.RowViewport.gameObject,wheel,ExecuteEvents.scrollHandler);yield return null;yield return Frame();
   Check(table.RankingScroll.verticalNormalizedPosition<.95f,"NATIVE_SCROLL_WHEEL_MOVES_COMPLETE_RANKING");
   var dragStart=RectTransformUtility.WorldToScreenPoint(null,table.RowViewport.position);var drag=new PointerEventData(EventSystem.current){pointerId=92,position=dragStart,pressPosition=dragStart,button=PointerEventData.InputButton.Left};ExecuteEvents.Execute(table.RowViewport.gameObject,drag,ExecuteEvents.beginDragHandler);drag.position+=new Vector2(0,120);drag.delta=new Vector2(0,120);ExecuteEvents.Execute(table.RowViewport.gameObject,drag,ExecuteEvents.dragHandler);ExecuteEvents.Execute(table.RowViewport.gameObject,drag,ExecuteEvents.endDragHandler);yield return null;yield return Frame();
   Check(table.RankingScroll.content.anchoredPosition.y>0,"NATIVE_TOUCH_DRAG_SCROLLS_COMPLETE_RANKING");table.RankingScroll.StopMovement();table.RankingScroll.verticalNormalizedPosition=0;yield return null;yield return Frame();
   Check(table.DisplayedPlayerIds.Last()==localId&&Overlaps(table.RowViewport,table.BoardRect.GetComponentsInChildren<RectTransform>().First(r=>r.name=="Golf score row 9")),"SCROLL_REACHES_LAST_PLAYER_WITHOUT_CHANGING_RANKING");Capture("expanded-ranking-last-player");
   var backdropPointer=new PointerEventData(EventSystem.current){position=new Vector2(80,Screen.height*.5f)};var backdropHits=new List<RaycastResult>();EventSystem.current.RaycastAll(backdropPointer,backdropHits);Check(backdropHits.Count>0&&backdropHits[0].gameObject==table.BackdropButton.gameObject,"OPEN_RANKING_BACKDROP_BLOCKS_UNDERLYING_GAMEPLAY_UI");ExecuteEvents.Execute(table.BackdropButton.gameObject,backdropPointer,ExecuteEvents.pointerClickHandler);yield return null;yield return Frame();Check(!table.Expanded&&!table.BackdropButton.gameObject.activeInHierarchy&&Mathf.Abs(previewHeight-table.RenderedHeight)<1,"BACKDROP_CLOSE_RESTORES_BOUNDED_PREVIEW");
   Check(swing.aimButton.interactable&&!swing.button.interactable,"EXISTING_AIM_ENTRY_AND_SHOT_ELIGIBILITY_PRESERVED");table.ToggleButton.onClick.Invoke();yield return null;Check(table.Expanded,"OPEN_RANKING_BEFORE_EXTERNAL_AIM_REQUEST");swing.aimButton.onClick.Invoke();until=Time.realtimeSinceStartup+3;while(!app.view.GolfAiming&&Time.realtimeSinceStartup<until)yield return null;yield return null;yield return Frame();
   Check(app.view.GolfAiming&&swing.button.interactable&&swing.aimButton.interactable&&swing.aimLabel.text.StartsWith("Cancel",StringComparison.OrdinalIgnoreCase),"AIM_SWITCHES_TO_EXISTING_CANCEL_AND_ENABLES_SWING");
   Check(swing.aimLabel.text=="Cancel"&&swing.label.text=="Swing","AIM_CANCEL_SWING_HAVE_NO_KEYBOARD_LETTERS");
   CheckShotArt("Swing");
   Check(table.AimSummary&&!table.Expanded&&!table.BackdropButton.gameObject.activeInHierarchy&&table.VisiblePlayerCount==0&&table.DisplayedPlayerIds.Count==0&&table.RenderedHeight==settings.boardAimSummaryHeight,"AIM_REMOVES_RANKING_PROGRESS_AND_STROKE_SUMMARY");yield return new WaitForSeconds(.3f);yield return Frame();
   Check(hud.AimFocused&&hud.AuxiliaryOpacity==0&&!table.BoardRect.gameObject.activeInHierarchy&&table.BoardRect.GetComponent<CanvasGroup>().alpha==0&&!table.BoardRect.GetComponent<CanvasGroup>().blocksRaycasts,"AIM_FOCUS_HIDES_RANKING_AND_BLOCKS_ITS_TOUCH_TARGETS");
   Check(hud.CourseRect.parent.GetComponent<CanvasGroup>().alpha==0&&!hud.JumpButton.transform.parent.GetComponent<CanvasGroup>().blocksRaycasts,"AIM_FOCUS_HIDES_COURSE_AND_BLOCKS_AUXILIARY_CONTROLS");
   Check(indicator.transform.GetComponentsInChildren<CanvasGroup>(true).Any(g=>g.name=="Your ball indicator"&&g.alpha==0)&&FindText("Your target").gameObject.activeInHierarchy&&app.view.GolfPreview.Visible,"AIM_FOCUS_RETAINS_HOLE_AND_TRAJECTORY_WITHOUT_BALL_ARROW");Capture("aim-focus-clean-screen");
   Check(!indicator.LargeTargetArrowVisible&&Mathf.Abs(indicator.TargetCard.localScale.x-settings.aimTargetScale)<.001f&&indicator.TargetCard.anchoredPosition.y>-90,"AIM_HOLE_CARD_IS_SMALL_TOP_CENTER_AND_LARGE_ARROW_HIDDEN");
   float savedAimYaw=app.view.yaw,savedAimPitch=app.view.pitch;var toHole=match.Target(actor).cup.position-ball.Body.position;
   app.view.yaw=Mathf.Atan2(toHole.x,toHole.z)*Mathf.Rad2Deg;app.view.pitch=20;yield return null;yield return null;yield return Frame();
   Check(!indicator.EdgeArrowVisible&&!indicator.LargeTargetArrowVisible,"VISIBLE_HOLE_HAS_NO_DIRECTION_ARROW_DURING_AIM");Capture("aim-hole-visible-no-arrow");
   app.view.yaw+=180;yield return null;yield return null;yield return Frame();
   Check(indicator.EdgeArrowVisible&&!indicator.LargeTargetArrowVisible&&indicator.EdgeArrow.rect.width<=48,"OFFSCREEN_HOLE_USES_SMALL_EDGE_ARROW_ONLY");
   var edgePoint=indicator.EdgeArrow.anchoredPosition;Check(Mathf.Abs(edgePoint.x)>safe.rect.width*.4f||Mathf.Abs(edgePoint.y)>safe.rect.height*.4f,"OFFSCREEN_ARROW_STAYS_OUT_OF_CENTRAL_LANDING_VIEW");
   Check(!Overlaps(indicator.EdgeArrow,hud.ActionRect)&&!Overlaps(indicator.EdgeArrow,(RectTransform)swing.aimButton.transform)&&!Overlaps(indicator.EdgeArrow,(RectTransform)FindText("Golf hold and release hint").transform.parent)&&!Overlaps(indicator.EdgeArrow,indicator.TargetCard),"EDGE_ARROW_AVOIDS_HUD_CONTROLS_HINT_AND_HOLE_CARD");Capture("aim-hole-offscreen-edge-arrow");
   app.view.yaw=savedAimYaw;app.view.pitch=savedAimPitch;yield return null;yield return null;yield return Frame();
   Check(!hud.CameraButton.gameObject.activeInHierarchy&&!hud.JumpButton.interactable&&carts.All(c=>c.transform.parent.GetComponent<CanvasGroup>().alpha==0&&!c.GetComponent<CanvasGroup>().blocksRaycasts),"AIM_DISABLES_CAMERA_JUMP_AND_HIDES_CART_INPUT");
   var pointer=new PointerEventData(EventSystem.current){pointerId=63,button=PointerEventData.InputButton.Left};int before=match.Player(actor).TotalStroke;int readyBefore=GolfFeedback.Instance.CueCount(GolfFeedbackCue.Ready);int impactBefore=GolfFeedback.Instance.CueCount(GolfFeedbackCue.Swing);RoutedPress(swing.button,pointer,"SWING");yield return new WaitForSeconds(.9f);yield return Frame();
   Check(app.view.GolfCharging&&app.view.GolfDisplayedCharge>0&&FindText("Golf hold and release hint").text.Contains("%")&&!swing.label.enabled,"REAL_POINTER_HOLD_UPDATES_HINT_POWER_WITHOUT_COVERING_BADGE_LETTERING");Check(GolfFeedback.Instance.ChargeLoopPlaying&&GolfFeedback.Instance.ChargeRingVisible,"SWING_CHARGING_PLAYS_LOOP_AND_DRAWS_POWER_RING");Capture("hold-and-release");
   float readyDeadline=Time.realtimeSinceStartup+6;while(GolfFeedback.Instance.Charge<.99f&&Time.realtimeSinceStartup<readyDeadline){GolfFeedback.Instance.ReviewFocus(true);yield return null;}yield return new WaitForEndOfFrame();
   Check(GolfFeedback.Instance.CueCount(GolfFeedbackCue.Ready)==readyBefore&&GolfFeedback.Instance.EdgeOpacity>.28f&&GolfFeedback.Instance.ChargeRingVisible,"FULL_SWING_HAS_THIRTY_PERCENT_RED_EDGE_WITHOUT_READY_SOUND");
   var warmEdge=hud.FrameRect.GetComponentsInChildren<GolfChargeGraphic>().First(g=>g.vignette);Check(warmEdge.rectTransform.rect.width>1500&&warmEdge.rectTransform.rect.height>800,"WARM_EDGE_COVERS_SCREEN_"+warmEdge.rectTransform.rect.size);Capture("near-full-charge-feedback");warmEdge.enabled=false;Capture("charge-without-edge");warmEdge.enabled=true;
   ExecuteEvents.Execute(swing.gameObject,pointer,ExecuteEvents.pointerExitHandler);yield return null;Check(!app.view.GolfCharging&&app.view.GolfAiming&&match.Player(actor).TotalStroke==before,"POINTER_EXIT_CANCELS_CHARGE_WITHOUT_STROKE");yield return new WaitForSeconds(.15f);Check(!GolfFeedback.Instance.ChargeLoopPlaying&&GolfFeedback.Instance.CueCount(GolfFeedbackCue.Swing)==impactBefore,"CANCELLED_CHARGE_NEVER_MAKES_HIT_SOUND");
   foreach(var size in new[]{new[]{1600,900},new[]{1280,960},new[]{1920,900}}){
    Screen.SetResolution(size[0],size[1],false);yield return new WaitForSeconds(.6f);yield return Frame();Check(Screen.width==size[0]&&Screen.height==size[1],"REQUESTED_LANDSCAPE_RESOLUTION_APPLIED_"+size[0]+"x"+size[1]);
    Insets(new Vector2(48,24),new Vector2(24,16));yield return null;yield return null;yield return Frame();CheckLayout(size[0]+"x"+size[1]+"-insets");Capture("layout-"+size[0]+"x"+size[1]+"-insets");RestoreInsets();yield return null;
   }
   Screen.SetResolution(1600,900,false);yield return new WaitForSeconds(.6f);yield return Frame();Check(app.view.GolfAiming&&swing.button.interactable,"RESPONSIVE_LAYOUT_PRESERVES_AIM_INPUT");RoutedPress(swing.button,pointer,"RELEASE");yield return new WaitForSeconds(.6f);ExecuteEvents.Execute(swing.gameObject,pointer,ExecuteEvents.pointerUpHandler);Check(GolfFeedback.Instance.CueCount(GolfFeedbackCue.Swing)==impactBefore,"POINTER_RELEASE_DOES_NOT_PLAY_IMPACT_EARLY");yield return new WaitForSeconds(.4f);Check(GolfFeedback.Instance.CueCount(GolfFeedbackCue.Swing)==impactBefore+1,"ACCEPTED_REAL_CLUB_CONTACT_PLAYS_EXACTLY_ONE_SWING_IMPACT");Check(match.Player(actor).TotalStroke==before+1&&!app.view.GolfAimRequested,"REAL_RELEASE_COUNTS_EXACTLY_ONE_STROKE");
   yield return new WaitForSeconds(.3f);yield return Frame();Check(!hud.AimFocused&&hud.AuxiliaryOpacity==1&&table.BoardRect.GetComponent<CanvasGroup>().alpha==1&&table.BoardRect.GetComponent<CanvasGroup>().blocksRaycasts,"SHOT_EXIT_RESTORES_AUXILIARY_HUD_AND_TOUCH_TARGETS");Check(indicator.LargeTargetArrowVisible&&!indicator.EdgeArrowVisible&&indicator.TargetCard.localScale==Vector3.one,"SHOT_EXIT_RESTORES_NORMAL_HOLE_CARD_AND_ARROW");Capture("focus-restored-after-shot");yield return GolfFeedbackChecks.Motion(GolfFeedback.Instance,actor,Check);yield return GolfFollowChecks.Run(match,actor,Check,Capture);
   double finishAt=match.Now;Finish(1,finishAt,5);Finish(2,finishAt,5);yield return null;yield return Frame();
   Check(match.State.Phase==GolfMatchPhase.FinalCountdown&&table.CountdownVisible&&FindText("First finisher notice").text=="A player has finished!"&&match.State.CountdownDeadline==finishAt+GolfMatchState.FinalCountdownSeconds,"FIRST_FINISH_SHOWS_EXISTING_NOTICE_AND_SHARED_THIRTY_SECONDS");Capture("final-countdown");
   match.State.Advance(match.State.CountdownDeadline);yield return null;yield return Frame();var results=match.State.Results();
   Check(results[0].rank==1&&results[1].rank==1&&results[0].tie&&results[1].tie&&results.Skip(2).All(r=>r.player.IsDNF&&r.rank==0),"RESULTS_RETAIN_SHARED_RANK_AND_UNRANKED_DNF");
   Check(FindText("Player 0 Player").text.StartsWith("1= ")&&FindText("Player 1 Player").text.StartsWith("1= ")&&FindText("Player 2 Status").text=="DNF"&&table.CountdownText=="00:00"&&FindText("First finisher notice").text=="Match finished","RESULT_ROWS_AND_EXPIRED_TIMER_MATCH_GAMEPLAY_STATE");Check(!FindText("Player 2 Player").text.StartsWith("3.")&&FindText("Player 2 Status").enabled,"DNF_REMAINS_UNRANKED_WITH_LIVE_STATUS_LABEL");Check(FindText("Player 0 Status").rectTransform.parent.GetComponentsInChildren<Image>().Any(i=>i.sprite==Resources.Load<Sprite>("GolfUI/Leaderboard/Finished")),"FINISHED_PLAYERS_USE_ORIGINAL_GREEN_STATUS_ART");CheckLayout("results-ten-players");Capture("results-ties-dnf");
   match.State.Clear();Check(match.State.Start(Enumerable.Range(1,24).Select(i=>new GolfEntrant((ulong)i,"Player "+i)).Append(new GolfEntrant(localId,"You")),match.Now),"LARGE_PRESENTATION_ROSTER_STARTED");yield return null;yield return Frame();
   Check(!table.Expanded&&table.VisiblePlayerCount==4&&Mathf.Abs(table.RenderedHeight-previewHeight)<1&&table.DisplayedPlayerIds.Last()==localId,"TWENTY_FIVE_PLAYERS_DO_NOT_GROW_COMPACT_BOARD");
   table.ToggleButton.onClick.Invoke();yield return null;yield return Frame();Check(table.Expanded&&table.VisiblePlayerCount==25&&table.RankingScroll.vertical&&table.FitsSafeFrame(),"EXPANDED_LIST_HAS_NO_TEN_PLAYER_PRESENTATION_CAP");table.RankingScroll.verticalNormalizedPosition=0;yield return null;yield return Frame();Capture("expanded-twenty-five-last-player");
   table.ToggleButton.onClick.Invoke();yield return null;
   match.ClearMatch();yield return null;Check(match.StartMatch(),"REVIEW_RESTORES_REAL_SINGLE_PLAYER_MATCH");yield return null;yield return Frame();Check(match.State.Players.Count==1&&!table.CountdownVisible&&!table.Expanded&&table.VisiblePlayerCount==1&&table.DisplayedPlayerIds.Distinct().Count()==1,"RESTART_REMOVES_SYNTHETIC_ROWS_AND_OLD_COUNTDOWN");
   yield return GolfPolishChecks.Run(match,actor,Check,Capture);
   yield return GolfRefineChecks.Run(match,actor,Check,Capture);
   Record("GOLF_HUD_COMPLETE checks="+checks);Application.Quit();
  }
  void Finish(ulong id,double now,int strokes){var player=match.State.Player(id);for(int i=player.TotalStroke;i<strokes;i++)match.State.RecordSwing(id,now);for(int hole=player.CurrentHole;hole<=5;hole++)match.State.EnterHole(id,hole,now);}
  void CheckShotArt(string name){var art=swing.button.targetGraphic as Image;Check(art&&art.sprite==Resources.Load<Sprite>("GolfUI/Badges/"+name)&&art.preserveAspect&&!swing.label.enabled&&!swing.GetComponentsInChildren<Image>(true).Any(i=>i.name=="Reference ShotCaption"),"ORIGINAL_ACTION_IMAGE_WITHOUT_TEXT_OVERLAY_"+name);}
  void CheckCartArt(GolfCartButton cart,string name){
   var art=cart.button.targetGraphic as Image;var expected=Resources.Load<Sprite>((name=="Exit"?"GolfUI/Badges/":"GolfUI/CartButtons/")+name);
   Check(expected&&art&&art.sprite==expected&&art.type==Image.Type.Simple&&art.preserveAspect&&!cart.label.enabled,"ORIGINAL_CART_ART_AND_LETTERING_"+name);
   Check(!cart.GetComponentsInChildren<Graphic>(true).Any(g=>g.name=="Drive steering wheel"||g.name=="Drive icon gold backing"||g.name=="Reference BuggyCaption"),"NO_OLD_CART_ICON_OR_CAPTION_PATCH_"+name);
  }
  void CheckBallText(){var label=FindText("Your ball");Check(!label.enabled&&label.text=="","YOUR_BALL_NO_DISTANCE_TEXT");}
  void OnDestroy(){RestoreInsets();}
 }
}
#endif
