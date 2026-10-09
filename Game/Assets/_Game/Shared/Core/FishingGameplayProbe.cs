#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.EventSystems;

namespace WhatTheFish {
 public sealed class FishingGameplayProbe:MonoBehaviour {
  string[] args;string report;int checks;AppRoot app;FishingGame game;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Boot(){var a=Environment.GetCommandLineArgs();if(a.Contains("-fishingGameplayAudit")||a.Contains("-networkFishingAudit"))new GameObject("Fishing gameplay review").AddComponent<FishingGameplayProbe>();}
  string Value(string key,string fallback){int i=Array.IndexOf(args,key);return i>=0&&i+1<args.Length?args[i+1]:fallback;}
  void Check(bool pass,string name){File.AppendAllText(report,(pass?"PASS ":"FAIL ")+name+"\n");if(!pass)throw new Exception(name);checks++;}
  IEnumerator Start(){
   args=Environment.GetCommandLineArgs();report=Value("-report",Path.Combine(Application.persistentDataPath,"fishing-review.txt"));DevelopmentProbe.TurnCommandActive=true;DevelopmentProbe.TurnCommand=default;
   float end=Time.realtimeSinceStartup+65;while((!AppRoot.Instance||!AppRoot.Instance.Exploring||!FishingGame.Instance||!FishingGame.Instance.Context)&&Time.realtimeSinceStartup<end)yield return null;
   app=AppRoot.Instance;game=FishingGame.Instance;Check(app&&game&&game.Context,"FISHING_CONTEXT");
   end=Time.realtimeSinceStartup+15;while(!app.view.active&&Time.realtimeSinceStartup<end)yield return null;
   Check(app.view.active,"ARRIVAL_HANDS_CONTROL_TO_PLAYER");
   if(args.Contains("-networkFishingAudit"))yield return Network();else{Rules();yield return Offline();}
   File.AppendAllText(report,"FISHING_GAMEPLAY_COMPLETE checks="+checks+"\n");
  }
  void Rules()=>FishingRuleChecks.Run(Check);
  void Place(Athlete actor,int pier){var pos=game.Pier(pier);Check(Physics.Raycast(pos+Vector3.up*12,Vector3.down,out var hit,30,1<<8,QueryTriggerInteraction.Ignore),"PIER_FLOOR_"+pier);pos=hit.point+Vector3.up*.04f;actor.capsule.enabled=false;actor.transform.SetPositionAndRotation(pos,Quaternion.LookRotation(Vector3.ProjectOnPlane(game.transform.position-pos,Vector3.up)));actor.ResetLocomotion();var net=actor.GetComponent<NetworkTransform>();if(net&&net.IsSpawned&&net.IsServer)net.Teleport(pos,actor.transform.rotation,Vector3.one);actor.capsule.enabled=game.Authority;Physics.SyncTransforms();app.view.yaw=actor.transform.eulerAngles.y;}
  IEnumerator AimFish(int fish,int mode){
   var view=app.view;view.mode=mode;view.pitch=30;view.yaw=app.LocalAthlete.transform.eulerAngles.y;yield return null;
   float limit=Time.time+2;
   while(Time.time<limit){
    var point=game.Presentation.TargetPosition(fish);
    if(mode==2){var delta=Vector3.ProjectOnPlane(point-app.LocalAthlete.transform.position,Vector3.up);view.yaw=Mathf.Atan2(delta.x,delta.z)*Mathf.Rad2Deg;view.pitch=20+(6.5f-delta.magnitude)/.08f;}
    else{var ray=view.FishingCamera.ScreenPointToRay(view.FishingAimScreenPoint);var local=view.FishingCamera.transform.InverseTransformDirection((point-ray.origin).normalized);var aim=view.FishingCamera.transform.InverseTransformDirection(ray.direction);view.yaw+=(Mathf.Atan2(local.x,local.z)-Mathf.Atan2(aim.x,aim.z))*Mathf.Rad2Deg*.8f;view.pitch-=(Mathf.Atan2(local.y,local.z)-Mathf.Atan2(aim.y,aim.z))*Mathf.Rad2Deg*.8f;}
    yield return null;
    if(view.SelectedFishingFish==fish&&game.VisibleTarget(app.LocalAthlete,view.FishingCamera,fish))yield break;
   }
   bool blocked=Physics.Linecast(view.FishingCamera.transform.position,game.Presentation.TargetPosition(fish),out var blocker,1<<8,QueryTriggerInteraction.Ignore);
   File.AppendAllText(report,"AIM_CONTEXT fish="+fish+" mode="+mode+" selected="+view.SelectedFishingFish+" available="+game.Available(app.LocalAthlete,fish)+" pitch="+view.pitch+" camera="+view.transform.position+" target="+game.Presentation.TargetPosition(fish)+" blocker="+(blocked?blocker.collider.name:"none")+"\n");Capture("aim-blocked");
  }
  IEnumerator CatchWithInput(bool touch,int size=0){
   var actor=app.LocalAthlete;var view=app.view;var ui=FindFirstObjectByType<FishingHUD>();var e=new PointerEventData(EventSystem.current){pointerId=71,button=PointerEventData.InputButton.Left};
   float end=Time.time+40;while(!game.Player(actor).HasValue||game.Player(actor).Value.phase!=FishingPhase.Ready){if(Time.time>end){Check(false,"READY_TIMEOUT");yield break;}yield return null;}
   int fish=game.NearestPier(actor)*FishingState.FishPerPier+size;yield return AimFish(fish,1);
   Check(view.SelectedFishingFish==fish&&ui.AimLocked,"RETICLE_HIGHLIGHTS_VISIBLE_FISH_"+size);yield return null;Capture("selected-"+size);
   int cueStarts=ui.HookCue.CueStarts;var hitArea=new Vector3[4];ui.ActionRect.GetWorldCorners(hitArea);
   if(touch){ui.OnPointerDown(e);ui.OnPointerUp(e);}else Check(view.BeginFishing(),"CAST_COMMAND_QUEUED");
   float until=Time.time+8;while(game.Player(actor).Value.phase!=FishingPhase.Bite&&Time.time<until)yield return null;
   Check(game.Player(actor).Value.phase==FishingPhase.Bite,"HOST_BITE_CUE_REPLICATED");
   yield return PoseFrame();var bite=game.Player(actor).Value;
   Check(ui.HookCue.Visible&&ui.ActionRect.GetComponent<UnityEngine.UI.Button>().interactable&&ui.StatusText=="HOOK!","AUTHORITATIVE_BITE_SHOWS_HOOK_AND_ALERT_"+size);
   Check(Mathf.Abs(ui.HookCue.Remaining01-(float)Math.Max(0,(bite.hookUntil-game.Now)/FishingState.HookSeconds))<.08f,"BITE_CUE_READS_REAL_HOOK_DEADLINE_"+size);
   Check(ui.HookCue.CueStarts==cueStarts+1&&ui.HookCue.DecorationsIgnoreRaycast,"ONE_TOUCH_THROUGH_CUE_PER_ACCEPTED_CAST_"+size);Capture("bite-hook-alert-"+size);
   yield return PoseFrame();var afterArea=new Vector3[4];ui.ActionRect.GetWorldCorners(afterArea);
   Check(ui.HookCue.CueStarts==cueStarts+1&&hitArea.Zip(afterArea,(a,b)=>Vector3.Distance(a,b)).All(d=>d<.05f),"REPEATED_BITE_TICKS_KEEP_INTRO_AND_HIT_AREA_STABLE_"+size);
   if(touch){ui.OnPointerDown(e);ui.OnPointerUp(e);}else Check(view.BeginFishing(),"HOOK_COMMAND_QUEUED");
   until=Time.time+2;while(game.Player(actor).Value.phase!=FishingPhase.Reeling&&Time.time<until)yield return null;
   Check(game.Player(actor).Value.phase==FishingPhase.Reeling,"HOOK_ENTERS_REELING");
   yield return PoseFrame();Check(!ui.HookCue.Visible,"ACCEPTED_HOOK_CLEARS_ALERT_"+size);
   yield return new WaitForSeconds(.15f);yield return PoseFrame();Check(ui.FitsSafeFrame()&&ui.RegionsSeparate(),"LIVE_GAUGES_FIT_WITHOUT_CONTROL_OVERLAP");Capture("hooked");CaptureGrip();Check(!view.SelectFishingFish(game.NearestPier(actor)*FishingState.FishPerPier+1),"CANNOT_RETARGET_ACTIVE_LINE");bool holding=false,danger=false,pointerChecked=false;int pulse=ui.ScorePulseCount;
   while(game.Player(actor).Value.phase==FishingPhase.Reeling&&Time.time<end){var p=game.Player(actor).Value;if(p.tension<.58f&&!holding){if(touch)ui.OnPointerDown(e);else view.BeginFishing();holding=true;if(touch&&!pointerChecked){pointerChecked=true;view.EndFishing(72);Check(view.FishingHeld,"OTHER_FINGER_CANNOT_RELEASE");}}else if(p.tension>.84f&&holding){if(touch)ui.OnPointerExit(e);else view.EndFishing();holding=false;}if(!danger&&p.tension>.74f){danger=true;Capture("danger-"+size);}yield return null;}
    if(touch)ui.OnPointerUp(e);else view.EndFishing();Check(game.Player(actor).Value.phase==FishingPhase.Caught,"INPUT_LANDS_CATCH_"+size);yield return PoseFrame();Check(ui.ScorePulseCount>pulse,"LIVE_SCORE_AWARD_ANIMATES_"+size);HudRefinement(ui,"award-visible-"+size,true);Capture("landed-"+size);yield return new WaitForSeconds(.2f);
  }
  void Capture(string name){
   if(!args.Contains("-fishingCapture"))return;
   // Batch players do not present a back buffer. Render the real overlays with the scene.
   var camera=Camera.main;var target=new RenderTexture(Screen.width,Screen.height,24);var active=RenderTexture.active;var previous=camera.targetTexture;
   var overlays=FindObjectsByType<Canvas>(FindObjectsSortMode.None).Where(c=>c.renderMode==RenderMode.ScreenSpaceOverlay).ToArray();var cameras=overlays.Select(c=>c.worldCamera).ToArray();var distances=overlays.Select(c=>c.planeDistance).ToArray();
   try{camera.targetTexture=target;foreach(var c in overlays){c.renderMode=RenderMode.ScreenSpaceCamera;c.worldCamera=camera;c.planeDistance=1;}Canvas.ForceUpdateCanvases();camera.Render();RenderTexture.active=target;
    var image=new Texture2D(Screen.width,Screen.height,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,Screen.width,Screen.height),0,0);image.Apply();File.WriteAllBytes(Path.ChangeExtension(report,name+".png"),image.EncodeToPNG());Destroy(image);
   }finally{for(int i=0;i<overlays.Length;i++){overlays[i].renderMode=RenderMode.ScreenSpaceOverlay;overlays[i].worldCamera=cameras[i];overlays[i].planeDistance=distances[i];}camera.targetTexture=previous;RenderTexture.active=active;target.Release();Destroy(target);Canvas.ForceUpdateCanvases();}
  }
  // Always cross a frame boundary before observing Update/LateUpdate output.
  // Another EndOfFrame wait inside an EndOfFrame fixture can resume too soon.
  static IEnumerator PoseFrame(){yield return null;if(SystemInfo.graphicsDeviceType!=UnityEngine.Rendering.GraphicsDeviceType.Null)yield return new WaitForEndOfFrame();}
   static bool RenderedTextInside(UnityEngine.UI.Text label,RectTransform container){
    if(!label||!label.isActiveAndEnabled||!container)return false;var vertices=label.cachedTextGenerator.verts;if(vertices.Count<=4)return false;
    var bounds=container.rect;float units=1/label.pixelsPerUnit;
    // TextGenerator includes a final empty quad; inspect the actual glyph mesh.
    for(int i=0;i<vertices.Count-4;i++){var point=container.InverseTransformPoint(label.rectTransform.TransformPoint(vertices[i].position*units));if(point.x<bounds.xMin-2||point.x>bounds.xMax+2||point.y<bounds.yMin-2||point.y>bounds.yMax+2)return false;}return true;
   }
   static bool NativeOutline(UnityEngine.UI.Text label,float width){
    if(!label)return false;var shadows=label.GetComponents<UnityEngine.UI.Shadow>().Where(s=>s.enabled).ToArray();return shadows.Length==1&&shadows[0] is UnityEngine.UI.Outline outline&&outline.effectColor==LocalProfile.Hex("143B50")&&Vector2.Distance(outline.effectDistance,new Vector2(width,-width))<.01f;
   }
   void HudRefinement(FishingHUD ui,string view,bool requireAward=false){
   if(!args.Contains("-fishingCapture"))return;
   var frame=ui.GetComponentInParent<Canvas>().GetComponentsInChildren<RectTransform>(true).First(t=>t.name=="Fishing HUD frame"&&t.gameObject.activeInHierarchy);
   var rows=frame.GetComponentsInChildren<RectTransform>().Where(t=>t.name.StartsWith("Catch row ")).ToArray();
   var scores=rows.Select(r=>r.GetComponentsInChildren<UnityEngine.UI.Text>().FirstOrDefault(t=>t.transform.parent==r&&t.name=="0")).ToArray();
   Check(scores.Length==game.State.Players.Count&&scores.All(t=>t&&t.color==LocalProfile.Hex("143B50")&&t.fontStyle==FontStyle.Normal&&t.GetComponents<UnityEngine.UI.Shadow>().All(s=>!s.enabled)&&t.cachedTextGenerator.vertexCount>0),"NAVY_CRISP_NATIVE_SCORE_DIGITS_"+view);
   string localName="Catch row "+FishingGame.Key(app.LocalAthlete);var local=rows.FirstOrDefault(r=>r.name==localName);var panel=local?local.GetComponent<FishingStickerGraphic>():null;
   Check(panel&&panel.SolidPanel&&panel.tint==LocalProfile.Hex("69C8F4")&&rows.Where(r=>r!=local).All(r=>r.GetComponent<FishingStickerGraphic>().SolidPanel&&new[]{LocalProfile.Hex("C0EFF1"),LocalProfile.Hex("FFCB53")}.Contains(r.GetComponent<FishingStickerGraphic>().tint)),"FULL_BLUE_LOCAL_ROW_AND_SOLID_FRIEND_SURFACES_"+view);
    var labels=frame.GetComponentsInChildren<UnityEngine.UI.Text>(true);var display=Resources.Load<Font>("Menu/CoveDisplay");var wanted=labels.FirstOrDefault(t=>t.name=="Wanted target");var bonus=labels.FirstOrDefault(t=>t.name=="Wanted bonus");
    Check(display&&wanted&&wanted.font==display&&wanted.fontStyle==FontStyle.Normal&&wanted.horizontalOverflow==HorizontalWrapMode.Overflow&&!wanted.text.Contains("\n")&&wanted.cachedTextGenerator.lineCount==1&&RenderedTextInside(wanted,wanted.rectTransform)&&RenderedTextInside(wanted,(RectTransform)wanted.transform.parent),"WANTED_NATIVE_WEIGHT_SINGLE_LINE_GLYPHS_FIT_"+view);
    Check(bonus&&bonus.font==display&&bonus.fontStyle==FontStyle.Normal&&bonus.fontSize>=32&&bonus.color==(bonus.text=="OK"?LocalProfile.Hex("4CDED6"):LocalProfile.Hex("FFCB53"))&&NativeOutline(bonus,2)&&RenderedTextInside(bonus,(RectTransform)bonus.transform.parent)&&!bonus.transform.parent.GetComponent<FishingStickerGraphic>()&&!bonus.transform.parent.GetComponent<UnityEngine.UI.Image>(),"LARGE_NAKED_GOLD_OR_CLAIMED_TEAL_BONUS_OUTLINE_TWO_"+view);
    var names=rows.Select(r=>r.GetComponentsInChildren<UnityEngine.UI.Text>().FirstOrDefault(t=>t.name=="Player name")).ToArray();
    Check(names.Length==rows.Length&&names.All(t=>t&&t.font==display&&t.fontStyle==FontStyle.Normal&&t.GetComponents<UnityEngine.UI.Shadow>().All(s=>!s.enabled)&&RenderedTextInside(t,t.rectTransform)),"PLAYER_NAMES_USE_EXISTING_NATIVE_DISPLAY_WEIGHT_"+view);
    var title=labels.FirstOrDefault(t=>t.text=="LAGOON CUP");Check(title&&title.font==display&&title.color==LocalProfile.Hex("143B50")&&title.fontStyle==FontStyle.Normal&&title.GetComponents<UnityEngine.UI.Shadow>().All(s=>!s.enabled)&&RenderedTextInside(title,title.rectTransform)&&!frame.GetComponentsInChildren<RectTransform>(true).Any(t=>t.name=="Lagoon header sticker"),"PLAIN_NAVY_LAGOON_TITLE_WITHOUT_HEADER_PANEL_"+view);
    var awards=rows.Select(r=>r.GetComponentsInChildren<UnityEngine.UI.Text>(true).FirstOrDefault(t=>t.name=="Award points")).ToArray();
    Check(awards.Length==rows.Length&&awards.All(t=>t&&t.color==LocalProfile.Hex("F17C27")&&t.font==display&&t.fontStyle==FontStyle.Normal&&t.fontSize>=28&&NativeOutline(t,2)&&!t.transform.parent.GetComponent<FishingStickerGraphic>()&&!t.transform.parent.GetComponent<UnityEngine.UI.Image>()),"NAKED_ORANGE_AWARD_POINTS_OUTLINE_TWO_HAVE_NO_PANEL_"+view);
    if(requireAward){var award=local?local.GetComponentsInChildren<UnityEngine.UI.Text>(true).FirstOrDefault(t=>t.name=="Award points"):null;var p=game.Player(app.LocalAthlete);Check(award&&p.HasValue&&p.Value.lastAward>0&&RenderedTextInside(award,award.rectTransform)&&(award.text=="+"+(p.Value.lastAward-p.Value.lastBonus)||p.Value.lastBonus>0&&award.text=="+"+p.Value.lastBonus),"VISIBLE_POP_DISPLAYS_ACTUAL_CATCH_AWARD_"+view);}
    var gaugeLabels=frame.GetComponentsInChildren<FishingStickerGraphic>(true).Where(g=>g.kind==FishingStickerGraphic.Kind.Gauge).SelectMany(g=>g.GetComponentsInChildren<UnityEngine.UI.Text>(true)).ToArray();
    Check(gaugeLabels.Length==2&&gaugeLabels.All(t=>t.fontStyle==FontStyle.Normal&&NativeOutline(t,2))&&NativeOutline(ui.ActionRect.GetComponentsInChildren<UnityEngine.UI.Text>().FirstOrDefault(t=>t.transform.parent==ui.ActionRect),1),"GAUGE_OUTLINE_TWO_AND_ACTION_OUTLINE_ONE_"+view);
    var setup=frame.GetComponentsInChildren<RectTransform>(true).First(t=>t.name=="Fishing match setup");var setupButtons=setup.GetComponentsInChildren<UnityEngine.UI.Button>(true);var modePanels=setupButtons.Where(b=>b.name.StartsWith("Mode ")).Select(b=>b.targetGraphic as FishingStickerGraphic).ToArray();
    Check(setupButtons.Length==6&&setupButtons.All(b=>{var skin=b.targetGraphic as FishingStickerGraphic;var caption=b.GetComponentsInChildren<UnityEngine.UI.Text>(true).FirstOrDefault();return b.enabled&&skin&&skin.SolidPanel&&skin.raycastTarget&&caption&&caption.enabled&&!caption.raycastTarget&&caption.fontStyle==FontStyle.Normal&&!string.IsNullOrWhiteSpace(caption.text)&&(!b.gameObject.activeInHierarchy||RenderedTextInside(caption,caption.rectTransform));})&&modePanels.Length==3&&modePanels.Count(g=>g&&g.tint==LocalProfile.Hex("4CDED6"))==1&&modePanels.Count(g=>g&&g.tint==LocalProfile.Hex("FFF3D7"))==2,"SETUP_NATIVE_CAPTIONS_AND_SELECTED_MODE_USE_SOLID_TINTED_BUTTONS_"+view);
   var glyphs=frame.GetComponentsInChildren<FishingStickerGraphic>();var camera=glyphs.FirstOrDefault(g=>g.kind==FishingStickerGraphic.Kind.Camera);var jump=glyphs.FirstOrDefault(g=>g.kind==FishingStickerGraphic.Kind.Jump);var close=glyphs.FirstOrDefault(g=>g.kind==FishingStickerGraphic.Kind.Close);
   Check(camera&&jump&&close&&Mathf.Abs(camera.rectTransform.rect.width/camera.rectTransform.rect.height-1.1f/.95f)<.01f&&new[]{camera,jump,close}.All(g=>g.isActiveAndEnabled&&!g.raycastTarget&&g.rectTransform.rect.width>10&&g.rectTransform.rect.height>10),"VISIBLE_SLIM_CAMERA_JUMP_CANCEL_GLYPHS_IGNORE_RAYS_"+view);
   var cameraButton=camera.GetComponentInParent<UnityEngine.UI.Button>();var jumpHandler=jump.GetComponentInParent<JumpButton>();var cancelButton=close.GetComponentInParent<UnityEngine.UI.Button>();var cancelPanel=cancelButton?cancelButton.GetComponent<FishingStickerGraphic>():null;
   Check(cameraButton&&cameraButton.isActiveAndEnabled&&cameraButton.targetGraphic&&cameraButton.GetComponentsInChildren<UnityEngine.UI.Text>(true).All(t=>!t.enabled)&&jumpHandler&&jumpHandler.isActiveAndEnabled&&jumpHandler.button&&jumpHandler.button.isActiveAndEnabled&&jumpHandler.label&&!jumpHandler.label.enabled&&jumpHandler.button.targetGraphic,"NATIVE_ICON_CONTROLS_PRESERVE_BUTTONS_AND_JUMP_HANDLER_"+view);
   Check(cancelButton&&cancelButton.isActiveAndEnabled&&cancelPanel&&cancelPanel.SolidPanel&&cancelPanel.tint==LocalProfile.Hex("F7656A")&&cancelButton.targetGraphic==cancelPanel,"SOLID_RED_CANCEL_BUTTON_HAS_NATIVE_X_"+view);
    foreach(var button in new[]{cameraButton,jumpHandler.button}){
     var rect=(RectTransform)button.transform;var skin=button.targetGraphic as FishingStickerGraphic;var outer=new Vector3[4];var inner=new Vector3[4];rect.GetWorldCorners(outer);if(skin)skin.rectTransform.GetWorldCorners(inner);
     float expectedWidth=button==cameraButton?104:120;Check(Mathf.Abs(rect.rect.width-expectedWidth)<.1f&&skin&&skin.SolidPanel&&Mathf.Abs(skin.cornerRadius-36)<.1f&&skin.rectTransform.anchorMin==Vector2.zero&&skin.rectTransform.anchorMax==Vector2.one&&skin.rectTransform.offsetMin==Vector2.zero&&skin.rectTransform.offsetMax==Vector2.zero&&outer.Zip(inner,(a,b)=>Vector3.Distance(a,b)).All(d=>d<.1f),"NARROW_ROUNDED_UTILITY_SKIN_MATCHES_NATIVE_HIT_RECT_"+button.name+"_"+view);
    }
   foreach(var button in new[]{cameraButton,jumpHandler.button,cancelButton}){
    var rect=(RectTransform)button.transform;var pointer=new PointerEventData(EventSystem.current){position=RectTransformUtility.WorldToScreenPoint(null,rect.TransformPoint(rect.rect.center))};var hits=new System.Collections.Generic.List<RaycastResult>();EventSystem.current.RaycastAll(pointer,hits);
    Check(hits.Count>0&&hits[0].gameObject.GetComponentInParent<UnityEngine.UI.Button>()==button,"DECORATIVE_ICON_RETAINS_NATIVE_POINTER_TARGET_"+button.name+"_"+view);
   }
  }
  void CaptureGrip(){
   if(!args.Contains("-fishingCapture"))return;var socket=app.LocalAthlete.visual.GetComponentsInChildren<Transform>(true).First(t=>t.name=="Golf grip socket");
   var camera=Camera.main;var oldPosition=camera.transform.position;var oldRotation=camera.transform.rotation;float fov=camera.fieldOfView;
   camera.transform.position=socket.position+app.LocalAthlete.transform.right*.45f-app.LocalAthlete.transform.forward*.3f+Vector3.up*.15f;camera.transform.LookAt(socket.position);camera.fieldOfView=42;FindFirstObjectByType<DevelopmentProbe>()?.CaptureFrame(Path.ChangeExtension(report,"hand-grip.png"));camera.transform.SetPositionAndRotation(oldPosition,oldRotation);camera.fieldOfView=fov;
  }
  IEnumerator Offline(){
   if(args.Contains("-fishingCapture")){
    foreach(var page in new[]{"home","rules","help"}){
     app.Show(page);while(app.Menu.IsTransitioning)yield return null;yield return PoseFrame();
     Check(app.Menu.Content.GetComponentsInChildren<UnityEngine.UI.Text>().All(t=>!t.text.Contains("mechanics will come later")&&!t.text.Contains("not available yet")&&!t.text.Contains("exploration island for now")),"CURRENT_FISHING_MENU_"+page.ToUpperInvariant());Capture("menu-"+page);
     if(page=="rules")Check(app.Menu.Content.GetComponentsInChildren<UnityEngine.UI.Text>().Where(t=>new[]{"Round & Cup","Fish & bonuses","Crew Catch","Ready together"}.Contains(t.text)).Count(t=>t.cachedTextGenerator.vertexCount>0)==4,"FISHING_RULE_HEADINGS_RENDER");
    }
    app.Show("stadium");yield return null;while(app.Menu.IsTransitioning||!app.view.active)yield return null;
   }
   var actor=app.LocalAthlete;Check(game.State.Match.phase==FishingRoundPhase.Practice,"OFFLINE_ENTERS_FREE_FISHING");
   Check(!game.Execute(actor,FishingAction.Cast,game.State.Match.round,game.Player(actor).Value.sequence),"SPAWN_AWAY_FROM_PIER_CANNOT_CAST");
   for(int pier=0;pier<5;pier++){Place(actor,pier);yield return new WaitForSeconds(.15f);Check(game.NearestPier(actor)==pier,"FIVE_SUPPORTED_CASTING_PIERS_"+pier);}
   Place(actor,0);yield return new WaitForSeconds(.2f);Check(game.Presentation.LiveFishCount==FishingState.FishCount,"THIRTY_SWIMMING_FISH_USE_EXISTING_LODS");
   bool safe=true,moving=false;for(int i=0;i<FishingState.FishCount;i++)for(int t=0;t<20;t++){var p=game.FishPosition(i,t);safe&=Vector3.ProjectOnPlane(p-game.transform.position,Vector3.up).magnitude<22&&p.y<game.WaterY-.25f;moving|=Vector3.Distance(p,game.FishPosition(i,t+1))>.1f;}
   Check(safe&&moving,"SWIMMING_STAYS_INSIDE_LAGOON_BELOW_SURFACE");
   Check(Mathf.Abs(game.WaterHeight(game.Pier(0))-game.WaterY)<.037f,"FLOAT_FOLLOWS_BOUNDED_SURFACE_RIPPLES");
   var hud=FindFirstObjectByType<FishingHUD>();Check(hud.TypographyClean(),"HUD_NATIVE_FONT_NO_SYNTHETIC_BOLD_OR_STACKED_SHADOWS");yield return PoseFrame();HudRefinement(hud,"practice");Capture("crisp-practice");
   for(int pier=0;pier<FishingState.PierCount;pier++){
    Place(actor,pier);yield return new WaitForSeconds(.12f);
    for(int mode=0;mode<3;mode++)for(int slot=0;slot<FishingState.FishPerPier;slot++){int fish=pier*FishingState.FishPerPier+slot;yield return AimFish(fish,mode);Check(app.view.SelectedFishingFish==fish,"AIM_SIX_TARGETS_PIER_"+pier+"_CAMERA_"+mode+"_SLOT_"+slot);}
   }
   Place(actor,0);yield return new WaitForSeconds(.15f);hud.ReviewFrameInsets(new Vector2(96,40),new Vector2(24,16));yield return null;yield return null;
   for(int mode=0;mode<3;mode++){yield return AimFish(4,mode);yield return PoseFrame();Check(hud.AimLocked&&app.view.SelectedFishingFish==4&&Vector2.Distance(hud.AimScreenPoint,app.view.FishingCamera.pixelRect.center)>20,"RETICLE_TARGETS_WITH_ASYMMETRIC_SAFE_AREA_"+mode);Capture("aim-safe-insets-"+mode);}
   hud.ReviewFrameInsets(Vector2.zero,Vector2.zero);yield return null;yield return AimFish(1,1);uint castSequence=game.Player(actor).Value.sequence;
   PlayerView.LookDelta=new Vector2(90,15);yield return null;yield return null;Check(game.Player(actor).Value.sequence==castSequence&&game.Player(actor).Value.phase==FishingPhase.Ready,"AIM_DRAG_NEVER_CASTS");
   var claimed=game.State.Fish[1];game.ReviewSnapshot=true;game.State.Fish[1]=new FishingFishRecord{id=1,claimed=true,owner=0};yield return null;yield return null;Check(app.view.SelectedFishingFish!=1,"RETICLE_REJECTS_CLAIMED_FISH");game.State.Fish[1]=new FishingFishRecord{id=1,respawnAt=game.Now+7};yield return null;yield return null;Check(app.view.SelectedFishingFish!=1,"RETICLE_REJECTS_RESPAWNING_FISH");game.State.Fish[1]=claimed;game.ReviewSnapshot=false;
   yield return AimFish(1,1);var blocker=new GameObject("Temporary aim occlusion fixture");blocker.layer=8;var point=game.Presentation.TargetPosition(1);blocker.transform.SetPositionAndRotation(Vector3.Lerp(app.view.transform.position,point,.6f),Quaternion.LookRotation(point-app.view.transform.position));blocker.AddComponent<BoxCollider>().size=new Vector3(.6f,.6f,.2f);Physics.SyncTransforms();yield return null;yield return null;
   Check(!game.VisibleTarget(actor,app.view.FishingCamera,1)&&app.view.SelectedFishingFish!=1,"RETICLE_REJECTS_TERRAIN_OCCLUDED_TARGET");Destroy(blocker);yield return null;Physics.SyncTransforms();
   app.view.yaw+=180;yield return null;yield return null;Check(app.view.SelectedFishingFish<0&&!app.view.BeginFishing(),"OFFSCREEN_AIM_CANNOT_CAST_OR_REPLACE_TARGET");
   yield return AimFish(1,1);Check(app.view.BeginFishing(),"RETICLE_CAST_CONFIRMS_EXPLICIT_ID");Check(!app.view.BeginFishing(),"PENDING_CAST_CANNOT_REPEAT_OR_RETARGET");app.view.yaw+=90;yield return new WaitForSeconds(.2f);Check(game.Player(actor).Value.fish==1&&app.view.SelectedFishingFish==1,"QUEUED_CAST_FREEZES_TARGET_WHILE_CAMERA_MOVES");app.view.CancelFishing();yield return new WaitForSeconds(2.2f);
   yield return CatchWithInput(true);Check(game.Player(actor).Value.score==2&&game.Player(actor).Value.catches==1,"TOUCH_CATCH_SCORES_TWO");
   var rod=actor.GetComponent<FishingRodMotion>();Check(rod&&rod.Equipped&&rod.GripError<.002f,"AUTHORED_ROD_ALIGNED_WITH_HAND_SOCKET");
    Check(!game.StartMatch(),"START_REQUIRES_READY");bool nativeSetup=args.Contains("-fishingCapture");var setupButtons=hud.GetComponentInParent<Canvas>().GetComponentsInChildren<UnityEngine.UI.Button>(true);if(nativeSetup)setupButtons.First(b=>b.name=="Fishing ready").onClick.Invoke();else app.view.ReadyFishing();yield return new WaitForSeconds(.2f);uint old=game.State.Match.round;bool started;if(nativeSetup){setupButtons.First(b=>b.name=="Start fishing round").onClick.Invoke();started=game.State.Match.phase==FishingRoundPhase.Countdown;}else started=game.StartMatch();Check(started&&game.State.Match.round!=old&&game.Player(actor).Value.score==0,"HOST_START_RESETS_PRACTICE_AND_ROUND_TOKEN");
   Check(!game.Execute(actor,FishingAction.Cast,old,0),"LATE_PRACTICE_CAST_REJECTED_BY_NEW_MATCH");
   Check(game.State.Match.phase==FishingRoundPhase.Countdown&&!app.view.BeginFishing(),"LIVE_COUNTDOWN_BLOCKS_INPUT");yield return new WaitForSeconds(3.1f);
   yield return CatchWithInput(false);Check(game.Player(actor).Value.score==5,"KEYBOARD_CATCH_WANTED_BONUS");Check(FindFirstObjectByType<FishingHUD>().WantedText.Contains("SMALL")&&FindFirstObjectByType<FishingHUD>().WantedBonusText=="OK","WANTED_BADGE_CONFIRMS_LOCAL_BONUS_CLAIM");
   yield return new WaitForSeconds(2.1f);yield return AimFish(1,1);Check(app.view.BeginFishing(),"EXPLICIT_SECOND_CAST_QUEUED");yield return new WaitForSeconds(.2f);app.view.CancelFishing();yield return new WaitForSeconds(.2f);Check(!game.Busy(actor)&&game.Player(actor).Value.score==5,"CANCEL_INPUT_KEEPS_SCORE_AND_RELEASES_LINE");
   yield return new WaitForSeconds(2.1f);
   for(int mode=0;mode<3;mode++){yield return AimFish(1,mode);yield return PoseFrame();Check(float.IsFinite(app.view.transform.position.x)&&float.IsFinite(app.view.transform.position.y),"CAMERA_FINITE_"+mode);Check(app.view.SelectedFishingFish==1,"FISH_SELECTABLE_CAMERA_"+mode);Capture("camera-"+mode);}
   if(args.Contains("-fishingCapture")){
    yield return new WaitForSeconds(.5f);Check(FindFirstObjectByType<LagoonReflection>().Ready,"ENVIRONMENT_REFLECTION_CAPTURE_READY");Check(FindObjectsByType<Light>(FindObjectsSortMode.None).Any(l=>l.enabled&&l.type==LightType.Directional),"DIRECTIONAL_LIGHT_ACTIVE");
    string[] assets={"Flame","FishBadge","FisherBadge","ClockBadge","LagoonFrame","ReelButton"};int[] limits={64,128,128,128,256,256};
    for(int i=0;i<assets.Length;i++){var sprite=Resources.Load<Sprite>("FishingUI/"+assets[i]);Check(sprite&&sprite.texture.width<=limits[i]&&sprite.texture.height<=limits[i],"COMPRESSED_UI_SIZE_"+assets[i]);}
   }
   yield return MissedHookCue();yield return StickerFixtures();yield return HookCueFixtures();yield return GripSportTransition();
  }
  IEnumerator MissedHookCue(){
   var actor=app.LocalAthlete;var ui=FindFirstObjectByType<FishingHUD>();int score=game.Player(actor).Value.score;
   yield return AimFish(3,1);Check(app.view.BeginFishing(),"MISS_CUE_CAST_CONFIRMS_TARGET");float end=Time.time+6;bool quiet=true;
   while(game.Player(actor).Value.phase!=FishingPhase.Bite&&Time.time<end){quiet&=!ui.HookCue.Visible;yield return null;}
   Check(quiet&&game.Player(actor).Value.phase==FishingPhase.Bite,"CAST_AND_WAIT_QUIET_BEFORE_AUTHORITATIVE_BITE");yield return PoseFrame();Check(ui.HookCue.Visible,"MISSED_BITE_FIRST_SHOWS_ALERT");
   end=Time.time+3;while(game.Player(actor).Value.phase==FishingPhase.Bite&&Time.time<end)yield return null;
   yield return PoseFrame();var p=game.Player(actor).Value;Check(p.phase==FishingPhase.Escaped&&p.result==FishingResult.Missed&&p.score==score&&!ui.HookCue.Visible,"EXPIRED_BITE_CLEARS_CUE_WITHOUT_SCORE");Capture("bite-missed");yield return new WaitForSeconds(2.1f);
  }
  IEnumerator GripSportTransition(){
   var actor=app.LocalAthlete;var departing=game.State;var motion=actor.GetComponent<FishingRodMotion>();var streaming=app.environments.GetComponent<SkySailStreaming>();var fishingHUD=FindFirstObjectByType<FishingHUD>();
   yield return streaming.Prepare(SportId.Golf);yield return AimFish(1,1);Check(app.view.BeginFishing(),"CONTEXT_EXIT_CAST_CONFIRMS_TARGET");float biteEnd=Time.time+6;
   while(game.Player(actor).Value.phase!=FishingPhase.Bite&&Time.time<biteEnd)yield return null;
   yield return PoseFrame();Check(game.Player(actor).Value.phase==FishingPhase.Bite&&fishingHUD.HookCue.Visible,"CONTEXT_EXIT_STARTS_FROM_VISIBLE_BITE_CUE");app.SelectSport(SportId.Golf);yield return new WaitForSeconds(.2f);
   Check(!motion.Equipped&&departing.Match.phase==FishingRoundPhase.Idle&&departing.Fish.Count==0,"FISHING_EXIT_RELEASES_ROD_AND_SHARED_ROUND");
   yield return PoseFrame();Check(!fishingHUD.HookCue.Visible,"ISLAND_CONTEXT_EXIT_CLEARS_HOOK_CUE");Capture("bite-context-exit");
   var golf=GolfMatchManager.Instance;Check(golf&&golf.StartMatch(),"GOLF_AFTER_FISHING_CAN_START");app.view.mode=0;yield return new WaitForSeconds(.3f);yield return PoseFrame();
   var skins=actor.visual.GetComponentsInChildren<SkinnedMeshRenderer>(true).Where(s=>s.sharedMesh&&s.sharedMesh.GetBlendShapeIndex("GolfRightGrip")>=0).ToArray();
   Check(actor.GolfClubMotion.Equipped&&skins.Length>0&&skins.All(s=>s.GetBlendShapeWeight(s.sharedMesh.GetBlendShapeIndex("GolfRightGrip"))>99),"FISHING_STOW_PRESERVES_GOLF_HAND_GRIP");
   if(SystemInfo.graphicsDeviceType!=UnityEngine.Rendering.GraphicsDeviceType.Null){var head=actor.visual.GetComponentsInChildren<Transform>(true).First(t=>t.name=="mixamorig:Head");Check(head.localScale.magnitude<.01f&&skins.All(s=>s.enabled),"GOLF_FIRST_PERSON_HANDOFF_KEEPS_BODY_AND_HIDES_HEAD");}
   yield return streaming.Prepare(SportId.Fishing);app.SelectSport(SportId.Fishing);app.view.mode=1;yield return new WaitForSeconds(.3f);yield return PoseFrame();game=FishingGame.Instance;
   Check(game&&game.State.Match.phase==FishingRoundPhase.Practice&&motion.Equipped&&skins.All(s=>s.GetBlendShapeWeight(s.sharedMesh.GetBlendShapeIndex("GolfRightGrip"))>99),"RETURN_TO_FISHING_REEQUIPS_ROD");
  }
  IEnumerator StickerFixtures(){
   if(!args.Contains("-fishingCapture"))yield break;
   var savedMatch=game.State.Match;var savedPlayers=game.State.Players.ToArray();var savedFish=game.State.Fish.ToArray();
   var fixture=new FishingState();fixture.Begin(new ulong[]{0,1,2,3,4},100,game.Now,true);
   int[] scores={17,19,12,5,2};for(int i=0;i<5;i++){var p=fixture.Players[i];p.score=scores[i];fixture.Players[i]=p;}
   game.ReviewSnapshot=true;game.State.Receive(fixture.Match,fixture.Players,fixture.Fish);app.view.mode=1;yield return new WaitForSeconds(.35f);Canvas.ForceUpdateCanvases();var ui=FindFirstObjectByType<FishingHUD>();Check(ui.VisibleScoreCount==5,"FIVE_PLAYER_SCORE_DIGITS_RENDER");HudRefinement(ui,"five-player");Capture("five-player-rank");
   int pulses=ui.ScorePulseCount;var awardedRow=ui.GetComponentInParent<Canvas>().GetComponentsInChildren<RectTransform>(true).First(t=>t.name=="Catch row 0");float beforeY=awardedRow.anchoredPosition.y;var leader=game.State.Players[0];leader.score=25;leader.catches++;leader.lastAward=8;leader.lastBonus=3;leader.awardSequence++;game.State.Players[0]=leader;
   // Observe a completed UI frame. A slow frame may finish the reorder before
   // RowsMoving is sampled, but the actual row must move and award exactly once.
   yield return PoseFrame();File.AppendAllText(report,"RANK_CONTEXT before="+beforeY+" after="+awardedRow.anchoredPosition.y+" pulses="+pulses+"->"+ui.ScorePulseCount+" moving="+ui.RowsMoving+" frameSeconds="+Time.unscaledDeltaTime+"\n");Check(awardedRow.anchoredPosition.y>beforeY+.5f&&ui.ScorePulseCount==pulses+1&&game.State.Rank(leader.owner)==1,"RANK_REORDER_AND_SCORE_POP");HudRefinement(ui,"local-crosses-twenty");Capture("rank-overtake");yield return new WaitForSeconds(.75f);Capture("wanted-pop-three");
   leader=game.State.Players[0];leader.phase=FishingPhase.Reeling;leader.fish=0;leader.origin=app.LocalAthlete.transform.position;leader.castDistance=8;leader.distance=4;leader.lineLength=4;leader.tension=.85f;leader.progress=.65f;game.State.Players[0]=leader;
    // The delivered game supports landscape only; exercise four supported aspect ratios.
    int[][] resolutions={new[]{844,390},new[]{1024,768},new[]{1920,810},new[]{1280,720}};
   foreach(var size in resolutions){Screen.SetResolution(size[0],size[1],false);yield return new WaitForSeconds(.6f);Canvas.ForceUpdateCanvases();Check(ui.FitsSafeFrame()&&ui.RegionsSeparate(),"SAFE_FRAME_NO_OVERLAP_"+size[0]+"x"+size[1]);Check(ui.VisibleScoreCount==5,"SCORE_DIGITS_"+size[0]+"x"+size[1]);HudRefinement(ui,size[0]+"x"+size[1]);Capture("layout-"+size[0]+"x"+size[1]);}
   Screen.SetResolution(844,390,false);ui.ReviewFrameInsets(new Vector2(96,40),new Vector2(24,16));yield return new WaitForSeconds(.6f);Check(ui.FitsSafeFrame()&&ui.RegionsSeparate(),"ASYMMETRIC_SAFE_FRAME_INSETS");Capture("safe-insets");ui.ReviewFrameInsets(Vector2.zero,Vector2.zero);
    var nextMatch=game.State.Match;nextMatch.started=game.Now-55;nextMatch.deadline=nextMatch.started+180;game.State.Receive(nextMatch,game.State.Players.ToArray(),game.State.Fish.ToArray());yield return new WaitForSeconds(.3f);yield return PoseFrame();Check(ui.WantedText.Contains("NEXT")&&ui.WantedText.Contains("MEDIUM")&&ui.WantedBonusText=="+3","WANTED_NEXT_TARGET_PREVIEW");HudRefinement(ui,"wanted-next");Capture("wanted-next");
   nextMatch.started=game.Now-174;nextMatch.deadline=nextMatch.started+180;game.State.Receive(nextMatch,game.State.Players.ToArray(),game.State.Fish.ToArray());yield return new WaitForSeconds(.3f);Capture("timer-urgent");
   nextMatch.mode=FishingMode.Crew;nextMatch.crewTarget=100;game.State.Receive(nextMatch,game.State.Players.ToArray(),game.State.Fish.ToArray());yield return new WaitForSeconds(.3f);Check(ui.FitsSafeFrame()&&ui.RegionsSeparate(),"CREW_CHECKLIST_FITS_EXTENDED_CLOCK_FRAME");Capture("crew-card");
   Screen.SetResolution(1600,900,false);yield return new WaitForSeconds(.5f);game.State.Receive(savedMatch,savedPlayers,savedFish);game.ReviewSnapshot=false;
  }
  FishingPlayerRecord ReviewBite(int fish,double remaining=FishingState.HookSeconds){
   var p=game.State.Players[0];p.sequence++;p.phase=FishingPhase.Bite;p.fish=fish;p.pier=FishingState.PierForFish(fish);p.connected=true;p.held=p.ready=false;p.result=FishingResult.None;p.origin=app.LocalAthlete.transform.position;p.castDistance=p.distance=p.lineLength=7;p.phaseAt=game.Now-3;p.biteAt=game.Now-(FishingState.HookSeconds-remaining);p.hookUntil=game.Now+remaining;
   game.State.Players[0]=p;game.State.Fish[fish]=new FishingFishRecord{id=fish,claimed=true,owner=p.owner};return p;
  }
  IEnumerator HookCueFixtures(){
   // Presentation fixtures isolate late snapshots and lifecycle transitions from
   // the real Cast/Hook/miss paths above; they never establish network latency.
   var savedMatch=game.State.Match;var savedPlayers=game.State.Players.ToArray();var savedFish=game.State.Fish.ToArray();var actor=app.LocalAthlete;var ui=FindFirstObjectByType<FishingHUD>();bool reduced=MenuPreferences.ReducedMotion;
   Place(actor,0);var fixture=new FishingState();fixture.Begin(new ulong[]{0,1,2,3,4},savedMatch.round+1000,game.Now,true);
   for(int i=0;i<5;i++){var p=fixture.Players[i];p.score=20-i*3;fixture.Players[i]=p;}
   game.ReviewSnapshot=true;game.State.Receive(fixture.Match,fixture.Players,fixture.Fish);MenuPreferences.SetMotion(false);yield return null;yield return null;
   yield return AimFish(0,1);ReviewBite(0);yield return null;yield return PoseFrame();
   var artwork=ui.ActionRect.GetComponentsInChildren<UnityEngine.UI.Image>().FirstOrDefault(i=>i.sprite==Resources.Load<Sprite>("FishingUI/ReelButton"));Check(artwork,"HOOK_ALERT_USES_EXISTING_NATIVE_BUTTON_ART");
   int repeatedStarts=ui.HookCue.CueStarts;float originalRemaining=ui.HookCue.Remaining01;var originalArea=new Vector3[4];ui.ActionRect.GetWorldCorners(originalArea);Capture("bite-alert-onset");
   float minScale=float.MaxValue,maxScale=float.MinValue,sampleEnd=Time.time+.35f;
   while(Time.time<sampleEnd){float scale=artwork.rectTransform.localScale.x;minScale=Mathf.Min(minScale,scale);maxScale=Mathf.Max(maxScale,scale);yield return null;}
   game.State.Receive(game.State.Match,game.State.Players.ToArray(),game.State.Fish.ToArray());yield return PoseFrame();var repeatedArea=new Vector3[4];ui.ActionRect.GetWorldCorners(repeatedArea);
   Check(ui.HookCue.Visible&&maxScale-minScale>.005f&&originalArea.Zip(repeatedArea,(a,b)=>Vector3.Distance(a,b)).All(d=>d<.05f),"HOOK_ART_PULSES_WITHOUT_MOVING_PRIMARY_HIT_AREA");
   Check(ui.HookCue.CueStarts==repeatedStarts&&ui.HookCue.Remaining01<originalRemaining,"REPEATED_BITE_SNAPSHOT_CANNOT_REPLAY_OR_EXTEND_CUE");Capture("bite-alert-pulse");
   sampleEnd=Time.time+1.4f;while(ui.HookCue.Remaining01>.2f&&ui.HookCue.Visible&&Time.time<sampleEnd)yield return null;yield return PoseFrame();Check(ui.HookCue.Visible&&ui.HookCue.Remaining01<=.2f,"HOOK_ALERT_COUNTDOWN_REACHES_REAL_DEADLINE");Capture("bite-alert-deadline");
   for(int mode=0;mode<3;mode++){
    var ready=game.State.Players[0];ready.phase=FishingPhase.Ready;ready.fish=-1;game.State.Players[0]=ready;game.State.Fish[0]=new FishingFishRecord{id=0};yield return AimFish(0,mode);ReviewBite(0);yield return PoseFrame();
    Check(ui.HookCue.Visible&&ui.HookCue.WorldCueVisible&&ui.HookCue.FitsSafeFrame()&&ui.HookCue.DecorationsIgnoreRaycast,"BITE_ALERT_FITS_AND_MARKS_FISH_CAMERA_"+mode);Capture("bite-camera-"+mode);
   }
   app.view.mode=1;app.view.yaw+=180;ReviewBite(0);yield return null;yield return PoseFrame();
   Check(ui.HookCue.Visible&&!ui.HookCue.WorldCueVisible&&ui.ActionRect.GetComponent<UnityEngine.UI.Button>().interactable,"OFFSCREEN_BITE_RETAINS_HOOK_BUTTON_FALLBACK");Capture("bite-offscreen-fallback");
   app.view.yaw=actor.transform.eulerAngles.y;app.view.pitch=30;yield return null;yield return null;
   ReviewBite(0,.25);yield return PoseFrame();var late=game.State.Players[0];
   Check(ui.HookCue.Visible&&ui.HookCue.Remaining01<.25f&&Mathf.Abs(ui.HookCue.Remaining01-(float)Math.Max(0,(late.hookUntil-game.Now)/FishingState.HookSeconds))<.08f,"LATE_BITE_SNAPSHOT_USES_REMAINING_DEADLINE");
   late.hookUntil=game.Now-.01;game.State.Players[0]=late;yield return null;yield return PoseFrame();
   Check(!ui.HookCue.Visible&&!ui.ActionRect.GetComponent<UnityEngine.UI.Button>().interactable&&ui.StatusText=="MISSED BITE"&&!app.view.BeginFishing(),"EXPIRED_REPLICATED_BITE_HAS_NO_FALSE_HOOK_CUE");
   ReviewBite(0);yield return PoseFrame();var cancelButton=ui.GetComponentInParent<Canvas>().GetComponentsInChildren<UnityEngine.UI.Button>().First(b=>b.name=="Fishing cancel");Check(cancelButton.interactable,"NATIVE_CANCEL_BUTTON_ENABLED_FOR_ACTIVE_BITE");cancelButton.onClick.Invoke();float cancelledAt=Time.time+1;while(game.State.Players[0].phase==FishingPhase.Bite&&Time.time<cancelledAt)yield return null;yield return PoseFrame();
   Check(game.State.Players[0].result==FishingResult.Cancelled&&!game.State.Fish[0].claimed&&!ui.HookCue.Visible,"CANCEL_AT_BITE_CLEARS_CUE_AND_HOST_RESERVATION");
   ReviewBite(0);yield return PoseFrame();int starts=ui.HookCue.CueStarts;ui.SendMessage("OnApplicationFocus",false);app.view.SendMessage("OnApplicationFocus",false);float focusEnd=Time.time+1;while(game.State.Players[0].phase==FishingPhase.Bite&&Time.time<focusEnd)yield return null;yield return PoseFrame();
   Check(!ui.HookCue.Visible&&!ui.ActionRect.GetComponent<UnityEngine.UI.Button>().interactable&&game.State.Players[0].result==FishingResult.Cancelled,"FOCUS_LOSS_CLEARS_BITE_CUE_AND_HOOK_ELIGIBILITY");ui.SendMessage("OnApplicationFocus",true);app.view.SendMessage("OnApplicationFocus",true);yield return PoseFrame();
   Check(!ui.HookCue.Visible&&ui.HookCue.CueStarts==starts,"FOCUS_RESUME_DOES_NOT_REPLAY_STALE_BITE");
   ReviewBite(0);yield return PoseFrame();starts=ui.HookCue.CueStarts;ui.SendMessage("OnApplicationPause",true);yield return PoseFrame();Check(!ui.HookCue.Visible,"PAUSE_CLEARS_BITE_CUE");ui.SendMessage("OnApplicationPause",false);yield return PoseFrame();Check(!ui.HookCue.Visible&&ui.HookCue.CueStarts==starts,"PAUSE_RESUME_SUPPRESSES_SAME_CAST_CUE");
   ReviewBite(0);yield return PoseFrame();Check(ui.HookCue.Visible,"ROUND_END_STARTS_FROM_VISIBLE_BITE_CUE");var match=game.State.Match;match.phase=FishingRoundPhase.Ended;game.State.Receive(match,game.State.Players.ToArray(),game.State.Fish.ToArray());yield return PoseFrame();Check(!ui.HookCue.Visible,"ROUND_END_CLEARS_HOOK_CUE");Capture("bite-round-ended");
   match.phase=FishingRoundPhase.Playing;match.round++;game.State.Receive(match,game.State.Players.ToArray(),game.State.Fish.ToArray());ReviewBite(0);yield return PoseFrame();Check(ui.HookCue.Visible,"NEW_ROUND_CAN_SHOW_NEW_CAST_CUE");
   foreach(var size in new[]{new[]{844,390},new[]{1024,768},new[]{1600,900}}){
    Screen.SetResolution(size[0],size[1],false);yield return new WaitForSeconds(.6f);ui.ReviewFrameInsets(new Vector2(48,24),new Vector2(24,16));ReviewBite(0);yield return PoseFrame();
    Check(ui.HookCue.Visible&&ui.HookCue.FitsSafeFrame()&&ui.FitsSafeFrame()&&ui.RegionsSeparate()&&game.State.Players.Count==5&&(SystemInfo.graphicsDeviceType==UnityEngine.Rendering.GraphicsDeviceType.Null||ui.VisibleScoreCount==5),"HOOK_ALERT_RESPECTS_FIVE_ROWS_AND_SAFE_AREA_"+size[0]+"x"+size[1]);Capture("bite-layout-"+size[0]+"x"+size[1]);ui.ReviewFrameInsets(Vector2.zero,Vector2.zero);
   }
   MenuPreferences.SetMotion(true);ReviewBite(0);yield return PoseFrame();Check(ui.HookCue.Visible&&ui.HookCue.FitsSafeFrame()&&ui.ActionRect.GetComponent<UnityEngine.UI.Button>().interactable,"REDUCED_MOTION_RETAINS_BITE_CUE_AND_HOOK");Capture("bite-reduced-motion");MenuPreferences.SetMotion(reduced);
   ReviewBite(0);yield return PoseFrame();var disconnected=game.State.Players[0];disconnected.connected=false;game.State.Players[0]=disconnected;yield return PoseFrame();Check(!ui.HookCue.Visible,"DISCONNECTED_LOCAL_BITE_CANNOT_KEEP_CUE_VISIBLE");
   ReviewBite(0);yield return PoseFrame();var missing=game.State.Players[0];game.State.Players.RemoveAt(0);yield return null;yield return PoseFrame();Check(!ui.HookCue.Visible&&!ui.ActionRect.GetComponent<UnityEngine.UI.Button>().interactable,"MISSING_LOCAL_PLAYER_CLEARS_CUE_AND_HOOK");game.State.Players.Insert(0,missing);
   var local=game.State.Players[0];local.phase=FishingPhase.Ready;local.fish=-1;game.State.Players[0]=local;var remote=game.State.Players[1];remote.phase=FishingPhase.Bite;remote.sequence++;remote.hookUntil=game.Now+FishingState.HookSeconds;game.State.Players[1]=remote;yield return null;yield return PoseFrame();Check(!ui.HookCue.Visible,"REMOTE_PLAYER_BITE_CANNOT_FLASH_LOCAL_CUE");
   game.State.Receive(savedMatch,savedPlayers,savedFish);game.ReviewSnapshot=false;yield return null;yield return PoseFrame();
  }
  IEnumerator Network(){
   var actor=app.LocalAthlete;float end=Time.time+15;
   while((game.State.Players.Count!=2||!game.Player(actor).HasValue)&&Time.time<end)yield return null;
   Check(game.State.Players.Count==2,"BOTH_PEERS_JOIN_PRACTICE");app.view.ReadyFishing();yield return new WaitForSeconds(.25f);
   if(game.Authority){
    end=Time.time+10;while(!game.State.AllReady&&Time.time<end)yield return null;Check(game.State.AllReady,"BOTH_READY_REPLICATED");
    var guest=Athlete.Active.First(a=>a.GetComponent<NetworkAthlete>()&&a.GetComponent<NetworkAthlete>().OwnerClientId!=0);Place(actor,0);Place(guest,1);
    Check(game.StartMatch()&&game.State.Players.Count==2,"HOST_STARTS_TWO_PLAYER_CONTEST");yield return new WaitForSeconds(3.2f);
    end=Time.time+40;while(game.Player(guest).Value.catches==0&&Time.time<end)yield return null;
    Check(game.Player(guest).Value.catches==1&&game.Player(guest).Value.score==5&&game.Player(actor).Value.score==0,"GUEST_TARGET_RPC_HOST_ONLY_AWARDS");
    Check(game.State.Fish.All(f=>!f.claimed),"GUEST_LANDING_RELEASES_RESERVATION");
    yield return CatchWithInput(false,2);Check(game.Player(actor).Value.score==10&&game.State.Rank(0)==1&&game.State.Rank(FishingGame.Key(guest))==2,"HOST_LARGE_CATCH_OVERTAKES_GUEST");
    yield return new WaitForSeconds(1);uint old=game.State.Match.round;game.State.Advance(game.State.Match.deadline,0);yield return new WaitForSeconds(.6f);Check(game.State.Match.phase==FishingRoundPhase.Ended,"HOST_SHARED_RESULTS_DEADLINE");
    app.view.ReadyFishing();end=Time.time+10;while(!game.State.AllReady&&Time.time<end)yield return null;Check(game.StartMatch()&&game.State.Match.round!=old&&game.State.Players.All(p=>p.score==0),"READY_REMATCH_RESETS_BOTH_PEERS");
   }else{
    end=Time.time+15;while((game.State.Match.phase!=FishingRoundPhase.Playing||game.NearestPier(actor)<0)&&Time.time<end)yield return null;
    Check(game.State.Match.phase==FishingRoundPhase.Playing,"GUEST_RECEIVES_COUNTDOWN_AND_PLAYING");Check(!game.StartMatch(),"GUEST_CANNOT_START");
    var p=game.Player(actor).Value;actor.GetComponent<NetworkAthlete>().FishingRpc(FishingAction.Cast,game.State.Match.round-1,p.sequence,FishingState.FishPerPier);yield return new WaitForSeconds(.2f);Check(game.Player(actor).Value.phase==FishingPhase.Ready,"STALE_GUEST_ROUND_REJECTED");
    actor.GetComponent<NetworkAthlete>().FishingRpc(FishingAction.Cast,game.State.Match.round,p.sequence,0);yield return new WaitForSeconds(.2f);Check(game.Player(actor).Value.phase==FishingPhase.Ready,"GUEST_CANNOT_SELECT_OTHER_PIER_FISH");
    yield return CatchWithInput(true);Check(game.Player(actor).Value.score==5&&game.State.Player(0).Value.score==0,"GUEST_OBSERVES_AUTHORITATIVE_BONUS");
    end=Time.time+40;while(game.State.Player(0).Value.catches==0&&Time.time<end)yield return null;yield return new WaitForSeconds(.15f);
    Check(game.State.Player(0).Value.score==10&&game.State.Rank(FishingGame.Key(actor))==2,"GUEST_OBSERVES_OTHER_PLAYER_OVERTAKE");
    end=Time.time+8;while(game.State.Match.phase!=FishingRoundPhase.Ended&&Time.time<end)yield return null;Check(game.State.Match.phase==FishingRoundPhase.Ended,"GUEST_RECEIVES_FINAL_RESULTS");
    uint old=game.State.Match.round;app.view.ReadyFishing();end=Time.time+8;while(game.State.Match.round==old&&Time.time<end)yield return null;Check(game.State.Match.round!=old&&game.Player(actor).Value.score==0,"GUEST_RECEIVES_CLEAN_REMATCH");
   }
  }
 }
}
#endif
