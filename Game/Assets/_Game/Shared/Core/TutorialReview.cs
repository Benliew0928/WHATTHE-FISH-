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
 public sealed class TutorialReview:MonoBehaviour {
  string folder;AppRoot app;TutorialSession lesson;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Boot(){if(Environment.GetCommandLineArgs().Contains("-tutorialReview")&&Environment.GetCommandLineArgs().Contains("-guestProfile"))new GameObject("Tutorial review").AddComponent<TutorialReview>();}
  string Arg(string flag){var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,flag);return i<0?"":args[i+1];}
  void Check(bool ok,string label){File.AppendAllText(Path.Combine(folder,"results.txt"),(ok?"PASS ":"FAIL ")+label+"\n");if(!ok)throw new Exception(label);}
  IEnumerator Until(Func<bool> condition,string label,float timeout=20){float end=Time.realtimeSinceStartup+timeout;while(!condition()&&Time.realtimeSinceStartup<end)yield return null;Check(condition(),label);}
  void Click(string name){var b=FindObjectsByType<Button>(FindObjectsSortMode.None).First(x=>x.name==name&&x.gameObject.activeInHierarchy);Canvas.ForceUpdateCanvases();var r=(RectTransform)b.transform;var point=RectTransformUtility.WorldToScreenPoint(null,r.TransformPoint(r.rect.center));var e=new PointerEventData(EventSystem.current){pointerId=71,position=point,button=PointerEventData.InputButton.Left};var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(e,hits);Check(b.IsInteractable()&&hits.Count>0&&hits[0].gameObject.GetComponentInParent<Button>()==b,"RAYCAST "+name);ExecuteEvents.Execute(b.gameObject,e,ExecuteEvents.pointerDownHandler);ExecuteEvents.Execute(b.gameObject,e,ExecuteEvents.pointerUpHandler);ExecuteEvents.Execute(b.gameObject,e,ExecuteEvents.pointerClickHandler);}
  void Command(Vector2 move=default){DevelopmentProbe.TurnCommand=new PlayerCommand{move=move,heading=app.view.yaw};}
  IEnumerator Move(Vector2 value,float seconds){float end=Time.time+seconds;while(Time.time<end&&!lesson.Succeeded){Command(value);yield return null;}Command();}
  void MobileCopy(string tag){
   var pattern=new System.Text.RegularExpressions.Regex(@"(?i)\b(mouse|keyboard|wasd|spacebar|right.click|left.click)\b|\[(Space|Esc|Ctrl|Shift|[A-Z])\]|\b(press|hold) (Shift|Ctrl|Space|Esc)\b");
   var texts=FindObjectsByType<Text>(FindObjectsSortMode.None).Where(t=>t.enabled&&t.gameObject.activeInHierarchy&&t.canvasRenderer.GetInheritedAlpha()>.05f).ToArray();
   var bad=texts.Where(t=>pattern.IsMatch(t.text)).Select(t=>t.text).ToArray();Check(bad.Length==0,"MOBILE_COPY "+tag+" "+string.Join(" | ",bad));
  }
  void CoachFits(string tag){
   Canvas.ForceUpdateCanvases();var panel=lesson.UI.transform.Find("Tutorial canvas/Coach dock/One lesson");
   var cropped=panel.GetComponentsInChildren<Text>().Where(t=>t.enabled&&t.text.Length>0&&t.cachedTextGeneratorForLayout.GetPreferredHeight(t.text,t.GetGenerationSettings(t.rectTransform.rect.size))/t.pixelsPerUnit>t.rectTransform.rect.height+1).Select(t=>t.text).ToArray();
   Check(cropped.Length==0,"COACH_TEXT_FITS "+tag+" "+string.Join(" | ",cropped));
  }
  IEnumerator SportMenus(){
   var bb=FindFirstObjectByType<BasketballHUD>();var fb=FindFirstObjectByType<FootballHUD>();var island=FindFirstObjectByType<IslandSettingsMenu>();
   if(app.SelectedSport==SportId.Basketball&&bb){bb.ShowSettings();yield return null;MobileCopy("court settings");bb.OpenGuide();for(int i=0;i<3;i++){bb.SelectGuide(i);yield return null;MobileCopy("court guide "+i);}bb.CloseMenu();}
   else if(app.SelectedSport==SportId.Football&&fb){fb.ShowSettings();yield return null;MobileCopy("pitch settings");fb.OpenGuide();for(int i=0;i<4;i++){fb.SelectGuide(i);yield return null;MobileCopy("pitch guide "+i);}fb.CloseMenu();}
   else if(island){island.Open();yield return null;MobileCopy("island settings");island.OpenLayout();yield return null;MobileCopy("island layout");island.Close();}
  }
  IEnumerator Start(){
   folder=Arg("-tutorialReview");Directory.CreateDirectory(folder);File.WriteAllText(Path.Combine(folder,"results.txt"),"");DevelopmentProbe.TurnCommandActive=true;DevelopmentProbe.TurnCommand=default;
   yield return Until(()=>DevelopmentProbe.ReadyToReviewSport&&AppRoot.Instance.view.active,"SPORT_READY",110);app=AppRoot.Instance;yield return new WaitForSeconds(2);var sport=app.SelectedSport;
   MobileCopy("ordinary HUD");yield return SportMenus();app.ExitTutorial();yield return new WaitForSeconds(1);
   foreach(var page in new[]{"home","settings","layouts","help"}){app.Show(page);yield return new WaitForSeconds(.5f);MobileCopy(page);if(page=="help")Capture("mobile-guide");}
   app.Show("tutorials");yield return new WaitForSeconds(1);Capture("hub");Click("Learn "+sport);
   yield return Until(()=>TutorialSession.Active&&TutorialSession.Active.Ready,"TUTORIAL_READY",110);lesson=TutorialSession.Active;yield return new WaitForSeconds(.6f);
   Check(lesson.Revealed.SequenceEqual(new[]{"move"}),"ONLY_MOVEMENT_REVEALED");Check(!TutorialSession.Allows("shoot")&&!app.view.BeginShot()&&!app.view.BeginKick(),"HIDDEN_ACTIONS_BLOCKED");
   foreach(var c in lesson.Layout.Controls.Where(c=>c.id!="move"))Check(c.wrapper.GetComponentsInChildren<Graphic>(true).All(g=>!g.enabled),"HIDDEN_ART "+c.id);
   Capture("first-step");Click("Tutorial chapters");Check(lesson.Paused&&app.view.ReadCommand().move==Vector2.zero,"CHAPTERS_BLOCK_GAMEPLAY");yield return null;yield return null;Capture("chapters");Click("Close lessons");yield return new WaitForSeconds(.5f);
   int sounds=lesson.UI.SuccessSounds;Click("Skip lesson");yield return null;Check(!lesson.Completed("forward")&&lesson.Index==1,"SKIP_IS_NOT_COMPLETION");Check(!lesson.UI.SuccessVisible&&lesson.UI.SuccessSounds==sounds,"SKIP_HAS_NO_SUCCESS_FEEDBACK");Click("Quit tutorial");yield return null;
   TutorialSession.Launch(sport);yield return Until(()=>TutorialSession.Active&&TutorialSession.Active.Ready,"RESUME_READY",110);lesson=TutorialSession.Active;Check(lesson.Index==1,"RESUME_BOOKMARK");yield return new WaitForSeconds(.5f);Click("Retry lesson");Check(!lesson.Completed("backward"),"RETRY_NOT_COMPLETION");
   for(int i=0;i<lesson.Steps.Length;i++){
    lesson.Begin(i);Command();string id=lesson.Step.id;sounds=lesson.UI.SuccessSounds;
    Check(!lesson.UI.SuccessVisible,"SUCCESS_CLEARED "+id);yield return new WaitForSeconds(.55f);MobileCopy(id);CoachFits(id);
    File.AppendAllText(Path.Combine(folder,"results.txt"),"EXERCISE "+id+"\n");
    yield return Exercise(id);yield return Until(()=>lesson.Completed(id),"COMPLETED "+id,20);Command();
    Check(lesson.UI.SuccessVisible&&lesson.UI.GetComponentsInChildren<Text>().Any(t=>t.text=="Nice work!"),"SUCCESS_VISIBLE "+id);
    Check(lesson.UI.SuccessSounds==sounds+1,"ONE_SUCCESS_CHIME "+id);
    CoachFits("success "+id);
    if(i==0){yield return new WaitForSeconds(1);Check(lesson.Index==i&&lesson.UI.SuccessVisible,"SUCCESS_READABLE_BEFORE_ADVANCE");Capture("success");}
    if(id is "shoot" or "pass" or "reel" or "power" or "block" or "fishSkill")Capture(id);
   }
   yield return Until(()=>lesson.Finished,"COURSE_FINISHED",5);Check(lesson.CompletedCount==lesson.Steps.Length,"ALL_EXERCISES_VERIFIED");
   var audio=lesson.UI.GetComponent<TutorialFeedbackAudio>();float ui=MenuPreferences.UIVolume,master=MenuPreferences.MasterVolume;sounds=audio.PlayCount;
   MenuPreferences.SetUI(0);audio.PlaySuccess();Check(audio.PlayCount==sounds&&audio.Gain==0,"SUCCESS_RESPECTS_UI_MUTE");MenuPreferences.SetUI(ui);
   MenuPreferences.SetMaster(0);audio.PlaySuccess();Check(audio.PlayCount==sounds,"SUCCESS_RESPECTS_MASTER_MUTE");MenuPreferences.SetMaster(master);
   bool reduced=MenuPreferences.ReducedMotion;MenuPreferences.SetMotion(true);yield return null;yield return null;Check(lesson.UI.transform.Find("Tutorial canvas/Coach dock/One lesson/Lesson complete check").localScale==Vector3.one,"SUCCESS_RESPECTS_REDUCED_MOTION");MenuPreferences.SetMotion(reduced);
   string profile=PlayerIdentity.ProfileStorageKey;Check(TutorialProgress.Load(profile,sport).complete.Count==lesson.Steps.Length,"SAVED_TO_PROFILE");Check(TutorialProgress.Load(profile+"other",sport).complete.Count==0,"PROFILE_ISOLATION");Capture("finished");
   Click("Quit tutorial");yield return null;Check(!TutorialSession.Active&&!app.Exploring&&app.Menu.Page=="tutorials","QUIT_RETURNS_TO_HUB");
   TutorialSession.Launch(sport,true,Array.FindIndex(TutorialCourse.For(sport),s=>s.id=="shoot"||s.id=="power"||s.id=="reel"));yield return Until(()=>TutorialSession.Active&&TutorialSession.Active.Ready,"REENTER_PRACTICE",110);lesson=TutorialSession.Active;yield return new WaitForSeconds(.6f);
   if(sport==SportId.Golf)app.view.BeginGolfAim();else if(sport==SportId.Basketball)app.view.BeginShot();else if(sport==SportId.Football)app.view.BeginKick();else app.view.BeginCountersteer(-1,71);
   Click("Quit tutorial");yield return null;Check(!TutorialSession.Active&&!app.view.ShotCharging&&!app.view.Charging&&!app.view.GolfAimRequested&&!app.view.FishingHeld&&!GolfCartWorld.Driving(app.LocalAthlete),"QUIT_CLEARS_ACTIVE_GESTURES");
   app.EnterOffline();yield return new WaitForSeconds(1);Check(TutorialSession.Allows("shoot")&&TutorialSession.Allows("ability"),"ORDINARY_GAME_UNRESTRICTED");
   File.AppendAllText(Path.Combine(folder,"results.txt"),"TUTORIAL_COMPLETE\n");Application.Quit();
  }
  IEnumerator Exercise(string id){
   var a=app.LocalAthlete;var v=app.view;var sport=app.SelectedSport;
   switch(id){
    case "forward":yield return Move(Vector2.up,.8f);yield break;
    case "backward":yield return Move(Vector2.down,.8f);yield break;
    case "sideways":yield return Move(Vector2.left,.7f);yield return Move(Vector2.right,.7f);yield break;
    case "look":PlayerView.LookDelta=new Vector2(250,0);yield break;
    case "jump":v.RequestJump();yield break;
    case "collect":case "prop":case "pier":yield return Move(Vector2.up,1.2f);yield break;
   }
   if(sport==SportId.Basketball||sport==SportId.Football){
    bool bb=sport==SportId.Basketball;
    switch(id){
     case "shoot":case "cancelShot":
      Check(bb?v.BeginShot(71):v.BeginKick(71),"BEGIN_SHOT");yield return new WaitForSeconds(.55f);
      if(id=="cancelShot"){if(bb)v.CancelShot(71);else v.CancelKick(71);}else if(bb)v.EndShot(71);else v.EndKick(71);break;
     case "pass":Check(bb?v.BeginPass():v.BeginFootballPass(),"TAP_PASS");break;
     case "directPass":Check(v.RequestTargetPass(FindObjectsByType<Athlete>(FindObjectsSortMode.None).First(x=>x.name=="Practice teammate")),"DIRECT_RECEIVER_INPUT");break;
     case "ability":if(bb)BasketballBall.Active.RequestProp(a);else FootballBall.Instance.RequestProp(a,v.yaw);break;
     case "guard":Check(v.BeginGuard(71),"BEGIN_GUARD");break;
     case "steal":Check(v.BeginSteal(71),"BEGIN_STEAL");yield return new WaitForSeconds(1);yield return Move(Vector2.up,.5f);break;
     case "block":Check(v.BeginGuard(71)&&v.SetGuardBlock(true,71),"GUARD_BLOCK_GESTURE");break;
     case "dash":DevelopmentProbe.TurnCommand=new PlayerCommand{dash=true,move=Vector2.up,heading=v.yaw};yield return new WaitForSeconds(1);Command();break;
     case "pressure":DevelopmentProbe.TurnCommand=new PlayerCommand{pressure=true,heading=v.yaw};yield return new WaitForSeconds(2);Command();break;
     case "tackle":File.AppendAllText(Path.Combine(folder,"results.txt"),"TACKLE_READY "+a.TackleReady+" holder="+FootballBall.Instance.CurrentController+" actor="+a.transform.position+" yaw="+v.yaw+"\n");v.RequestTackle();yield return new WaitForSeconds(.7f);File.AppendAllText(Path.Combine(folder,"results.txt"),"TACKLE_RESULT holder="+FootballBall.Instance.CurrentController+" hits="+a.GetComponent<FootballTackle>().HitsDealt+" actor="+a.transform.position+"\n");Capture("tackle");break;
    }
   }
   if(sport==SportId.Golf){
    switch(id){
     case "start":Check(GolfMatchManager.Instance.StartMatch(),"START_GOLF");break;
     case "aim":Check(v.BeginGolfAim(),"AIM_INPUT");break;
     case "cancelAim":Check(v.BeginGolfAim(),"AIM_CANCEL_BEGIN");yield return new WaitForSeconds(.5f);v.CancelGolfAim();break;
     case "power":case "putt":Check(v.BeginGolfAim(),"AIM_SWING_BEGIN");yield return new WaitForSeconds(.3f);Check(v.BeginGolfSwing(71),"POWER_DRAG_BEGIN");v.SetGolfPower(71,id=="putt"?.12f:.4f);yield return new WaitForSeconds(.3f);v.EndGolfSwing(71);break;
     case "buggy":case "recall":v.RequestCartToggle();break;
     case "drive":case "exitCart":v.RequestCartUse();break;
     case "steer":yield return Move(new Vector2(.5f,1),3);break;
    }
   }
   if(sport==SportId.Fishing){var g=FishingGame.Instance;
    switch(id){
     case "skillChoice":Click("Fishing skill choice 0");break;
     case "cast":case "cancelCast":
      var p=g.Player(a).Value;int fish=g.State.Fish.First(f=>g.CanCast(a,f.id)).id;DevelopmentProbe.TurnCommand=new PlayerCommand{fishingAction=FishingAction.Cast,fishingRound=g.State.Match.round,fishingSequence=p.sequence,fishingFish=fish,heading=v.yaw};yield return null;yield return null;Command();
      if(id=="cancelCast"){yield return new WaitForSeconds(.4f);v.CancelFishing();}break;
     case "hook":Check(v.BeginFishing(71),"HOOK_INPUT");break;
     case "reel":Check(v.BeginCountersteer(-1,71),"LEFT_REEL_INPUT");yield return new WaitForSeconds(.65f);v.EndFishing(71);yield return null;Check(v.BeginCountersteer(1,72),"RIGHT_REEL_INPUT");yield return new WaitForSeconds(.65f);v.EndFishing(72);break;
     case "cool":yield return new WaitForSeconds(1.5f);break;
     case "fishSkill":Check(v.FishingAbilityInput(FishingAction.UseAbility),"SKILL_INPUT");break;
     case "catch":
      float end=Time.time+55;while(!lesson.Completed(id)&&Time.time<end){var record=g.Player(a).Value;int side=FishingState.CounterSide(record,g.Now);if(record.tension>.65f)v.EndFishing(71);else {v.EndFishing(71);v.BeginCountersteer(side,71);}yield return new WaitForSeconds(.15f);}v.EndFishing(71);break;
    }
   }
  }
  void Capture(string name){
   var canvases=FindObjectsByType<Canvas>(FindObjectsSortMode.None).Where(c=>c.renderMode==RenderMode.ScreenSpaceOverlay).ToArray();var cameras=canvases.Select(c=>c.worldCamera).ToArray();var distances=canvases.Select(c=>c.planeDistance).ToArray();
   var camera=Camera.main;var previousTarget=camera.targetTexture;var previousActive=RenderTexture.active;
   var target=new RenderTexture(Screen.width,Screen.height,24);var image=new Texture2D(Screen.width,Screen.height,TextureFormat.RGB24,false);
   try{
    foreach(var c in canvases){c.renderMode=RenderMode.ScreenSpaceCamera;c.worldCamera=camera;c.planeDistance=.5f;}camera.targetTexture=target;
    // Changing canvas render mode can rebuild the shared dynamic font atlas.
    // Warm it and rebuild labels before reading the final frame.
    for(int pass=0;pass<3;pass++){foreach(var text in FindObjectsByType<Text>(FindObjectsSortMode.None))text.SetAllDirty();Canvas.ForceUpdateCanvases();camera.Render();}
    RenderTexture.active=target;image.ReadPixels(new Rect(0,0,Screen.width,Screen.height),0,0);image.Apply();File.WriteAllBytes(Path.Combine(folder,name+".png"),image.EncodeToPNG());
   }
   finally{camera.targetTexture=previousTarget;RenderTexture.active=previousActive;Destroy(target);Destroy(image);for(int i=0;i<canvases.Length;i++){canvases[i].renderMode=RenderMode.ScreenSpaceOverlay;canvases[i].worldCamera=cameras[i];canvases[i].planeDistance=distances[i];}}
  }
 }
}
#endif
