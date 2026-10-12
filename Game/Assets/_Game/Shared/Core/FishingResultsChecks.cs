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
 // UI fixtures use five synthetic records, not five network clients or phones.
 public static class FishingResultsChecks {
  static IEnumerator Frame(){yield return null;yield return new WaitForEndOfFrame();Canvas.ForceUpdateCanvases();}
  static bool CrewTextInsideClock(FishingResultsUI ui){
   if(!ui.CrewVisible)return true;var clock=(RectTransform)ui.CrewRect.parent;var interior=clock.rect;
   // The illustrated sliced frame has a twenty-unit pale border. White labels
   // must remain on the coloured interior, not merely inside their own rects.
   interior.xMin+=20;interior.xMax-=20;interior.yMin+=20;interior.yMax-=20;
   foreach(var label in ui.CrewRect.GetComponentsInChildren<Text>()){
    if(!label.isActiveAndEnabled||string.IsNullOrEmpty(label.text))continue;var mesh=label.canvasRenderer.GetMesh();if(!mesh||mesh.vertexCount==0)return false;
    foreach(var vertex in mesh.vertices){var point=clock.InverseTransformPoint(label.transform.TransformPoint(vertex));if(point.x<interior.xMin-1||point.x>interior.xMax+1||point.y<interior.yMin-1||point.y>interior.yMax+1)return false;}
   }return true;
  }
  static void Click(Button button,Action<bool,string> check,string tag){
   var rect=(RectTransform)button.transform;var pointer=new PointerEventData(EventSystem.current){pointerId=89,button=PointerEventData.InputButton.Left,position=RectTransformUtility.WorldToScreenPoint(null,rect.TransformPoint(rect.rect.center))};
   var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(pointer,hits);var hit=hits.Count>0?hits[0].gameObject:null;
   check(hit&&hit.GetComponentInParent<Button>()==button,"RESULT_POINTER_ROUTES_TO_"+tag);ExecuteEvents.Execute(button.gameObject,pointer,ExecuteEvents.pointerClickHandler);
  }
  static void Capture(string report,string tag){
   if(SystemInfo.graphicsDeviceType==UnityEngine.Rendering.GraphicsDeviceType.Null)return;var camera=Camera.main;if(!camera)return;
   string path=Path.ChangeExtension(report,tag+".png");var canvases=UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None).Where(c=>c.renderMode==RenderMode.ScreenSpaceOverlay).ToArray();var cameras=canvases.Select(c=>c.worldCamera).ToArray();var planes=canvases.Select(c=>c.planeDistance).ToArray();
   var target=RenderTexture.GetTemporary(Screen.width,Screen.height,24);var previous=camera.targetTexture;var active=RenderTexture.active;var texture=new Texture2D(Screen.width,Screen.height,TextureFormat.RGB24,false);
   try{foreach(var canvas in canvases){canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=.5f;}camera.targetTexture=target;Canvas.ForceUpdateCanvases();camera.Render();RenderTexture.active=target;texture.ReadPixels(new Rect(0,0,Screen.width,Screen.height),0,0);texture.Apply();File.WriteAllBytes(path,texture.EncodeToPNG());}
   finally{camera.targetTexture=previous;RenderTexture.active=active;UnityEngine.Object.Destroy(texture);RenderTexture.ReleaseTemporary(target);for(int i=0;i<canvases.Length;i++){canvases[i].renderMode=RenderMode.ScreenSpaceOverlay;canvases[i].worldCamera=cameras[i];canvases[i].planeDistance=planes[i];}Canvas.ForceUpdateCanvases();}
   File.AppendAllText(report,"CAPTURE "+Path.GetFileName(path)+" dimensions="+Screen.width+"x"+Screen.height+" synthetic_result_players=5\n");
  }
  static FishingPlayerRecord[] Players(ulong local){
   var ids=new[]{local}.Concat(Enumerable.Range(1,5).Select(i=>(ulong)i).Where(i=>i!=local)).Take(5).ToArray();
   return ids.Select((id,index)=>new FishingPlayerRecord{owner=id,connected=true,phase=FishingPhase.Ready,fish=-1,pier=-1,score=index<=1?45:30-index,smallCatches=index==0?2:1,mediumCatches=index==0?3:0,largeCatches=index==0?2:0,catches=index==0?7:1,bonusTotal=index==0?6:0,missedBites=index==0?2:0,snappedLines=index==0?1:0,cancels=index==0?3:0,timeouts=index==0?1:0,cupPoints=index<=1?12:7-index,cupScore=index<=1?112:70-index,catchMask=7}).ToArray();
  }
  public static IEnumerator Run(AppRoot app,FishingGame game,Action<bool,string> check,string report){
   var ui=UnityEngine.Object.FindFirstObjectByType<FishingResultsUI>();check(ui&&app&&game&&app.LocalAthlete,"FISHING_RESULTS_COMPONENT_READY");
   var savedMatch=game.State.Match;var savedPlayers=game.State.Players.ToArray();var savedFish=game.State.Fish.ToArray();bool savedReview=game.ReviewSnapshot,savedApp=app.enabled,savedCommandActive=DevelopmentProbe.TurnCommandActive;var savedTurn=DevelopmentProbe.TurnCommand;game.ReviewSnapshot=true;DevelopmentProbe.TurnCommandActive=true;
   var safe=ui.GetComponentInParent<Canvas>().GetComponentsInChildren<RectTransform>(true).First(r=>r.name=="Safe area");Vector2 savedMin=safe.anchorMin,savedMax=safe.anchorMax,savedOffsetMin=safe.offsetMin,savedOffsetMax=safe.offsetMax;
   int savedWidth=Screen.width,savedHeight=Screen.height;ulong local=FishingGame.Key(app.LocalAthlete);var players=Players(local);uint round=savedMatch.round+1000;
   var match=new FishingMatchSnapshot{round=round,phase=FishingRoundPhase.Ended,mode=FishingMode.Round,outcome=FishingOutcome.Finished,started=game.Now-180,deadline=game.Now,ended=game.Now};
   File.AppendAllText(report,"FIXTURE completed_standings_only=5 actual_network_clients=0 actual_phones=0\n");
   try{
    ui.ClearContext();int beforeDelayed=ui.ShownRounds;game.State.Receive(match,Array.Empty<FishingPlayerRecord>(),savedFish);ui.Tick(game,app.LocalAthlete,true);yield return Frame();
    check(!ui.Visible&&ui.ShownRounds==beforeDelayed,"ENDED_SNAPSHOT_WAITS_FOR_DELAYED_LOCAL_PLAYER_RECORD");
    game.State.Receive(match,players,savedFish);ui.Tick(game,app.LocalAthlete,true);yield return Frame();
    check(ui.Visible&&ui.ModalBlocksRaycasts&&ui.ResultRound==round&&ui.Title=="ROUND RESULTS","ENDED_ROUND_OPENS_ONE_MODAL");
    check(ui.ShownRounds==beforeDelayed+1,"DELAYED_LOCAL_RECORD_RETRIES_AND_OPENS_ROUND_EXACTLY_ONCE");
    check(EventSystem.current.currentSelectedGameObject&&EventSystem.current.currentSelectedGameObject.transform.IsChildOf(ui.CardRect),"RESULT_MODAL_OWNS_KEYBOARD_AND_CONTROLLER_SELECTION");
    check(ui.CardRect.GetComponentsInChildren<Button>().All(b=>b.navigation.mode==Navigation.Mode.Explicit&&new[]{b.navigation.selectOnLeft,b.navigation.selectOnRight,b.navigation.selectOnUp,b.navigation.selectOnDown}.All(s=>s&&s.transform.IsChildOf(ui.CardRect))),"RESULT_CONTROLLER_NAVIGATION_CANNOT_REACH_UNDERLYING_HUD");
    check(ui.StatTexts.Contains("SMALL × 2")&&ui.StatTexts.Contains("MEDIUM × 3")&&ui.StatTexts.Contains("LARGE × 2")&&ui.StatTexts.Contains("WANTED BONUS +6")&&ui.StatTexts.Contains("MISSED 2")&&ui.StatTexts.Contains("SNAPPED 1")&&ui.StatTexts.Contains("CANCELLED 3")&&ui.StatTexts.Contains("TIMED OUT 1"),"RESULTS_SHOW_HOST_RECORD_STATS_WITHOUT_BONUS_DOUBLE_COUNT");
    check(ui.CardRect.Find("Fishing final standing 0/Fishing final rank 0").GetComponent<Text>().text=="1"&&ui.CardRect.Find("Fishing final standing 1/Fishing final rank 1").GetComponent<Text>().text=="1","ROUND_TIE_RETAINS_SHARED_FIRST_PLACE");
    int shown=ui.ShownRounds;string frozenScore=ui.CardRect.Find("Fishing final standing 0/Fishing final score 0").GetComponent<Text>().text;var changed=players.ToArray();changed[0].ready=true;changed[0].score=999;game.State.Receive(match,changed,savedFish);ui.Tick(game,app.LocalAthlete,true);yield return Frame();
    check(ui.ShownRounds==shown&&ui.CardRect.Find("Fishing final standing 0/Fishing final score 0").GetComponent<Text>().text==frozenScore,"ENDED_READY_UPDATES_DO_NOT_CHANGE_FROZEN_RESULTS");
    game.State.Receive(match,players,savedFish);ui.Tick(game,app.LocalAthlete,true);
    DevelopmentProbe.TurnCommand=new PlayerCommand{move=Vector2.right};check(app.view.ReadCommand().move==Vector2.zero,"OPEN_RESULT_MODAL_BLOCKS_WALKING_INPUT");
    Click(ui.CloseButton,check,"CLOSE");check(app.view.ReadCommand().move==Vector2.right,"CLOSE_RESTORES_WALKING_INPUT_IMMEDIATELY");DevelopmentProbe.TurnCommand=savedTurn;yield return Frame();check(!ui.Visible&&ui.ReopenButton.gameObject.activeInHierarchy,"CLOSE_RETURNS_TO_ISLAND_WITH_RESULTS_BUTTON");check(!EventSystem.current.currentSelectedGameObject,"CLOSE_CLEARS_MODAL_SUBMIT_TARGET");
    for(int i=0;i<4;i++)ui.Tick(game,app.LocalAthlete,true);check(!ui.Visible&&ui.ShownRounds==shown,"CLOSED_CARD_NEVER_AUTO_REOPENS_SAME_ROUND");
    Click(ui.ReopenButton,check,"REOPEN");yield return Frame();check(ui.Visible&&!ui.ReopenButton.gameObject.activeInHierarchy&&ui.ShownRounds==shown,"REOPEN_REUSES_SAME_COMPLETED_ROUND");
    app.enabled=false;
    foreach(var size in new[]{new[]{1600,900},new[]{1280,960},new[]{1920,900},new[]{1280,720},new[]{844,390}}){
     Screen.SetResolution(size[0],size[1],false);yield return new WaitForSeconds(.4f);
     safe.anchorMin=new Vector2(48f/Screen.width,24f/Screen.height);safe.anchorMax=Vector2.one-new Vector2(24f/Screen.width,16f/Screen.height);safe.offsetMin=safe.offsetMax=Vector2.zero;yield return Frame();
     string tag=size[0]+"x"+size[1];check(Screen.width==size[0]&&Screen.height==size[1],"RESULT_LANDSCAPE_APPLIED_"+tag);check(ui.FitsSafeFrame(),"RESULT_CARD_INSIDE_ASYMMETRIC_SAFE_FRAME_"+tag);bool readable=ui.Readable();if(!readable){File.AppendAllText(report,ui.ReadabilityProblems()+"\n");Capture(report,"unreadable-results-"+tag);}check(readable,"RESULT_NATIVE_FONTS_RENDER_WITHOUT_EXTRA_WEIGHT_"+tag);Capture(report,"results-"+tag);
     Click(ui.CloseButton,check,"CLOSE_"+tag);yield return Frame();check(ui.FitsSafeFrame(),"RESULT_REOPEN_BUTTON_INSIDE_SAFE_FRAME_"+tag);Click(ui.ReopenButton,check,"REOPEN_"+tag);yield return Frame();
    }
    Screen.SetResolution(1600,900,false);yield return new WaitForSeconds(.4f);safe.anchorMin=Vector2.zero;safe.anchorMax=Vector2.one;safe.offsetMin=safe.offsetMax=Vector2.zero;yield return Frame();
    ui.ReadyButton.onClick.Invoke();var command=app.view.ReadCommand();check(!ui.Visible&&command.fishingAction==FishingAction.Ready&&command.fishingRound==round,"RESULT_READY_CLOSE_QUEUES_EXISTING_READY_ACTION_ONCE");
    check(game.Execute(app.LocalAthlete,command.fishingAction,command.fishingRound,command.fishingSequence)&&game.Player(app.LocalAthlete).Value.ready,"RESULT_READY_COMMAND_ACCEPTED_BY_HOST_RULES");
    ui.Open();ui.Tick(game,app.LocalAthlete,true);ui.ReadyButton.onClick.Invoke();command=app.view.ReadCommand();check(!ui.Visible&&command.fishingAction==FishingAction.None&&game.Player(app.LocalAthlete).Value.ready,"ALREADY_READY_CLOSE_NEVER_TOGGLES_UNREADY");
    match.round=++round;match.mode=FishingMode.Cup;match.cupRound=3;match.cupValid=match.cupFinished=true;game.State.Receive(match,players,savedFish);ui.Tick(game,app.LocalAthlete,true);yield return Frame();
    check(ui.Visible&&ui.Title=="CUP RESULTS"&&ui.CardRect.Find("Fishing final standing 0/Fishing final cup points 0").GetComponent<Text>().text=="12"&&ui.CardRect.Find("Fishing final standing 0/Fishing final cup catch 0").GetComponent<Text>().text=="112","CUP_RESULTS_SEPARATE_ROUND_AND_TOTAL_POINTS");
    check(ui.CardRect.Find("Fishing final standing 0/Fishing final rank 0").GetComponent<Text>().text=="1"&&ui.CardRect.Find("Fishing final standing 1/Fishing final rank 1").GetComponent<Text>().text=="1","CUP_TIE_RETAINS_SHARED_RANK");Capture(report,"cup-results");
    match.round=++round;match.cupFinished=match.cupValid=false;players[4].connected=false;game.State.Receive(match,players,savedFish);ui.Tick(game,app.LocalAthlete,true);yield return Frame();
    check(ui.CardRect.GetComponentsInChildren<Text>().Any(t=>t.text=="LEFT")&&ui.CardRect.GetComponentsInChildren<Text>().Any(t=>t.text.Contains("adds no Cup points")),"DEPARTED_PLAYER_AND_INVALID_CUP_HAVE_CLEAR_RESULT_LABELS");
    match.round=++round;match.mode=FishingMode.Crew;match.phase=FishingRoundPhase.Playing;match.outcome=FishingOutcome.None;match.crewTarget=100;match.crewScore=45;match.crewMask=3;players[4].connected=true;players[4].catches=0;game.State.Receive(match,players,savedFish);ui.Tick(game,app.LocalAthlete,true);yield return Frame();
    check(!ui.Visible&&ui.CrewVisible&&ui.CrewNeed.Contains("NEEDS A CATCH")&&ui.CrewRect.GetComponentsInChildren<CoveIcon>().Count(i=>i.symbol==CoveSymbol.Check)==2,"CREW_CHECKLIST_SHOWS_COLLECTED_SIZES_AND_MISSING_CONTRIBUTOR");
    foreach(var size in new[]{new[]{1600,900},new[]{1280,960},new[]{1920,900},new[]{844,390}}){Screen.SetResolution(size[0],size[1],false);yield return new WaitForSeconds(.4f);safe.anchorMin=new Vector2(48f/Screen.width,24f/Screen.height);safe.anchorMax=Vector2.one-new Vector2(24f/Screen.width,16f/Screen.height);safe.offsetMin=safe.offsetMax=Vector2.zero;yield return Frame();check(ui.FitsSafeFrame()&&FishingHUD.Instance.FitsSafeFrame()&&FishingHUD.Instance.RegionsSeparate(),"CREW_CLOCK_EXTENSION_FITS_ALL_HUD_FRAMES_"+size[0]+"x"+size[1]);bool readable=ui.Readable()&&CrewTextInsideClock(ui);if(!readable){File.AppendAllText(report,ui.ReadabilityProblems()+"\n");Capture(report,"unreadable-crew-"+size[0]+"x"+size[1]);}check(readable,"CREW_NATIVE_CHECKLIST_TEXT_RENDERS_INSIDE_COLOURED_CLOCK_"+size[0]+"x"+size[1]);Capture(report,"crew-checklist-"+size[0]+"x"+size[1]);}
    match.phase=FishingRoundPhase.Ended;match.outcome=FishingOutcome.Failed;game.State.Receive(match,players,savedFish);ui.Tick(game,app.LocalAthlete,true);yield return Frame();check(ui.Visible&&!ui.CrewVisible&&ui.Conditions.Contains("MISSING LARGE")&&ui.CardRect.GetComponentsInChildren<Text>().Any(t=>t.text.StartsWith("NEEDS A CATCH:")),"CREW_FAILED_CARD_STATES_UNMET_CONDITIONS");Capture(report,"crew-incomplete");
    match.round=++round;match.crewScore=match.crewTarget;players[4].catches=1;game.State.Receive(match,players,savedFish);ui.Tick(game,app.LocalAthlete,true);yield return Frame();
    check(ui.Conditions.Contains("MISSING LARGE")&&ui.CardRect.GetComponentsInChildren<Text>().Any(t=>t.text=="EVERYONE CAUGHT · CREW GOALS INCOMPLETE")&&!ui.CardRect.GetComponentsInChildren<Text>().Any(t=>t.text.Contains("MORE POINTS NEEDED")),"CREW_MET_BASKET_WITH_MISSING_SIZE_NEVER_REQUESTS_MORE_POINTS");
    match.round=++round;match.outcome=FishingOutcome.Abandoned;game.State.Receive(match,players,savedFish);ui.Tick(game,app.LocalAthlete,true);yield return Frame();check(ui.Title=="ROUND STOPPED"&&ui.CardRect.GetComponentsInChildren<Text>().Any(t=>t.text.Contains("does not update your personal best")),"ABANDONED_ROUND_NEVER_PRESENTED_AS_COMPLETED_SUCCESS");
    check(ui.Conditions=="ROSTER CHANGED · READY TO TRY AGAIN","ABANDONED_RESULT_ROSTER_LABEL_COVERS_JOIN_AND_LEAVE");
    ui.Close();EventSystem.current.SetSelectedGameObject(ui.ReopenButton.gameObject);match.round=++round;match.phase=FishingRoundPhase.Countdown;game.State.Receive(match,players,savedFish);ui.Tick(game,app.LocalAthlete,true);yield return Frame();
    check(!ui.Visible&&!ui.ReopenButton.gameObject.activeInHierarchy&&!EventSystem.current.currentSelectedGameObject,"NEXT_ROUND_CLEARS_SELECTED_REOPEN_BUTTON");
    ui.ClearContext();check(!ui.Visible&&!ui.CrewVisible&&!ui.ReopenButton.gameObject.activeInHierarchy,"CONTEXT_RESET_SYNCHRONOUSLY_CLEARS_MODAL_CHECKLIST_AND_REOPEN");
   }finally{
    game.State.Receive(savedMatch,savedPlayers,savedFish);game.ReviewSnapshot=savedReview;DevelopmentProbe.TurnCommand=savedTurn;DevelopmentProbe.TurnCommandActive=savedCommandActive;ui.ClearContext();safe.anchorMin=savedMin;safe.anchorMax=savedMax;safe.offsetMin=savedOffsetMin;safe.offsetMax=savedOffsetMax;app.enabled=savedApp;Screen.SetResolution(savedWidth,savedHeight,false);
   }
  }
 }
}
#endif
