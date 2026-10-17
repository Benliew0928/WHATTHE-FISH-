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
 public sealed class BasketballUIReview:MonoBehaviour {
  string folder;bool failed;AppRoot app;BasketballBall ball;Athlete actor;Transform hoop;BasketballHUD hud;BasketballShootButton shoot,pass;StandaloneInputModule module;BaseInput previous;BasketballUIReviewInput input;int originalCamera;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Init(){var a=Environment.GetCommandLineArgs();int i=Array.IndexOf(a,"-basketballUIReview");if(i>=0&&i+1<a.Length)new GameObject("Basketball UI review").AddComponent<BasketballUIReview>().folder=a[i+1];}
  void Check(bool ok,string name){failed|=!ok;File.AppendAllText(Path.Combine(folder,"results.txt"),(ok?"PASS ":"FAIL ")+name+"\n");}
  void Pump(){EventSystem.current.SendMessage("OnApplicationFocus",true,SendMessageOptions.DontRequireReceiver);module.UpdateModule();module.Process();input.Settle();}
  void Finger(int id,Vector2 point,TouchPhase phase){input.Set(id,point,phase);Pump();}
  Vector2 Point(BasketballShootButton b)=>RectTransformUtility.WorldToScreenPoint(null,b.transform.position);
  Vector2 Destination(BasketballShootButton b,BasketballDragChoice c)=>RectTransformUtility.WorldToScreenPoint(null,b.guide.transform.TransformPoint(b.guide.Target(c)));
  IEnumerator Pickup(float distance){
   input?.Clear();hud.CloseMenu();app.view.ClearMatchInput();DevelopmentProbe.TurnCommand=default;ball.ResetHome();ball.autoPickup=true;
   var p=hoop.TransformPoint(new Vector3(0,.07f,distance));float heading=Quaternion.LookRotation(hoop.position-p).eulerAngles.y;actor.capsule.enabled=false;actor.transform.SetPositionAndRotation(p,Quaternion.Euler(0,heading,0));actor.capsule.enabled=true;actor.ResetLocomotion();Physics.SyncTransforms();app.view.yaw=heading;DevelopmentProbe.TurnCommand=new PlayerCommand{heading=heading};
   ball.Place(ball.transform.parent.InverseTransformPoint(p+actor.transform.forward*.6f+Vector3.up*.18f),Quaternion.identity,Vector3.zero,Vector3.zero);float end=Time.time+3;while(ball.Holder!=actor&&Time.time<end)yield return null;yield return new WaitForSeconds(.2f);Check(ball.Holder==actor,"pickup at "+distance);Canvas.ForceUpdateCanvases();
  }
  IEnumerator Capture(string name){
   yield return new WaitForEndOfFrame();var camera=Camera.main;var canvas=hud.GetComponentInParent<Canvas>();var oldMode=canvas.renderMode;var oldCamera=canvas.worldCamera;float oldPlane=canvas.planeDistance;
   canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;Canvas.ForceUpdateCanvases();var rt=RenderTexture.GetTemporary(Screen.width,Screen.height,24);var old=RenderTexture.active;var target=camera.targetTexture;
   camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;var image=new Texture2D(Screen.width,Screen.height,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,Screen.width,Screen.height),0,0);image.Apply();File.WriteAllBytes(Path.Combine(folder,name+".png"),image.EncodeToPNG());Destroy(image);camera.targetTexture=target;RenderTexture.active=old;RenderTexture.ReleaseTemporary(rt);canvas.renderMode=oldMode;canvas.worldCamera=oldCamera;canvas.planeDistance=oldPlane;Canvas.ForceUpdateCanvases();yield return null;
  }
  IEnumerator Start(){
   Directory.CreateDirectory(folder);File.WriteAllText(Path.Combine(folder,"results.txt"),"");float end=Time.realtimeSinceStartup+90;
   while((!(app=AppRoot.Instance)||!app.Exploring||app.SelectedSport!=SportId.Basketball||!BasketballHUD.Instance)&&Time.realtimeSinceStartup<end)yield return null;
   ball=BasketballBall.Active;actor=app?app.LocalAthlete:null;hud=BasketballHUD.Instance;if(!ball||!actor||!hud){Check(false,"game initialized");Finish();yield break;}
   originalCamera=app.view.mode;app.view.mode=2;DevelopmentProbe.TurnCommandActive=true;DevelopmentProbe.TurnCommand=default;ball.ReviewShotRoll=()=>0;
   hoop=app.stadium.GetComponentsInChildren<Transform>().Single(t=>t.name=="Hoop_North");shoot=FindObjectsByType<BasketballShootButton>(FindObjectsSortMode.None).Single(b=>!b.pass);pass=FindObjectsByType<BasketballShootButton>(FindObjectsSortMode.None).Single(b=>b.pass);
   module=EventSystem.current.GetComponent<StandaloneInputModule>();previous=module.inputOverride;input=EventSystem.current.gameObject.AddComponent<BasketballUIReviewInput>();module.inputOverride=input;
   yield return new WaitForSeconds(1);yield return Pickup(8);ball.ReviewGiveProp(actor,SportsProp.Turbo);yield return null;yield return Capture("ready");Check(((RectTransform)shoot.transform).rect.size==Vector2.one*106,"approved 106-unit Shoot size");Check(shoot.label.font==SportsUIStyle.DisplayFont,"sports display font loaded");
   ball.autoPickup=false;ball.ResetHome();yield return null;CheckLabels("defense");yield return Capture("defense");Check(shoot.label.text=="Guard"&&pass.label.text=="Steal","defense captions match shared slot artwork immediately");
   foreach(var b in new[]{shoot,pass}){
    yield return Pickup(10);uint shots=ball.ShotCount,passes=ball.PassCount;Finger(81,Point(b),TouchPhase.Began);yield return null;Check(b.Tracking&&(b.pass?app.view.PassCharging:app.view.ShotCharging),"input module touch begins "+b.pass);
    var joystick=app.view.stick;var stickPoint=RectTransformUtility.WorldToScreenPoint(null,joystick.transform.position);Finger(82,stickPoint,TouchPhase.Began);Finger(82,stickPoint+Vector2.right*40,TouchPhase.Moved);Check(joystick.value.x>0&&b.Tracking,"movement thumb remains independent");Finger(82,stickPoint,TouchPhase.Ended);
    float before=app.view.ShotPower;Finger(81,Destination(b,BasketballDragChoice.Upper),TouchPhase.Moved);Check(b.guide.Choice==BasketballDragChoice.Upper,"upper path selects action");if(!b.pass)Check(Mathf.Abs(app.view.ShotPower-before)<.01f,"selection does not restart timing");yield return null;
    Check(b.pass?app.view.PassBend==1:app.view.ShotFinish==BasketballFinish.Dunk&&app.view.ShotNeedsTiming&&hud.ShotGuide.TimingVisible,"loft or far dunk uses correct meter");CheckLabels(b.pass?"loft":"far-dunk");yield return Capture(b.pass?"loft":"far-dunk");
    Finger(81,Destination(b,BasketballDragChoice.Diagonal),TouchPhase.Moved);yield return null;Check(b.pass?app.view.PassBend==-1:app.view.ShotFinish==BasketballFinish.Layup&&app.view.ShotNeedsTiming&&hud.ShotGuide.TimingVisible,"diagonal selects bounce or far layup");CheckLabels(b.pass?"bounce":"far-layup");yield return Capture(b.pass?"bounce":"far-layup");
    var foreign=new PointerEventData(EventSystem.current){pointerId=999,position=Destination(b,BasketballDragChoice.Cancel)};b.OnDrag(foreign);b.OnPointerUp(foreign);Check(b.Tracking,"foreign pointer cannot cancel or release");
    Finger(81,Point(b),TouchPhase.Moved);Check(b.pass?app.view.PassBend==0:app.view.ShotFinish==BasketballFinish.Shot,"return restores base action");
    Finger(81,Destination(b,BasketballDragChoice.Cancel),TouchPhase.Moved);Check(b.Tracking&&(b.pass?app.view.PassCharging:app.view.ShotCharging),"entering Cancel keeps the finger and charge active");yield return null;Check(b.guide.Cue=="Release to cancel","Cancel is visibly armed");yield return Capture(b.pass?"pass-cancel-armed":"shoot-cancel-armed");Finger(81,Point(b),TouchPhase.Moved);Check(b.Tracking&&(b.pass?app.view.PassCharging:app.view.ShotCharging)&&b.guide.Choice==BasketballDragChoice.Center,"sliding back resumes aim");Finger(81,Destination(b,BasketballDragChoice.Cancel),TouchPhase.Moved);Finger(81,Destination(b,BasketballDragChoice.Cancel),TouchPhase.Ended);yield return new WaitForSeconds(.4f);Check(ball.Holder==actor&&ball.ShotCount==shots&&ball.PassCount==passes,"cancel then lift never fires");
    yield return Pickup(8);Finger(81,Point(b),TouchPhase.Began);Finger(81,Point(b)+Vector2.right*220,TouchPhase.Moved);Finger(81,Point(b)+Vector2.right*220,TouchPhase.Ended);yield return new WaitForSeconds(.3f);Check(ball.Holder==actor&&!b.Tracking,"outside release keeps possession");
    Finger(81,Point(b),TouchPhase.Began);Finger(81,Point(b),TouchPhase.Canceled);yield return new WaitForSeconds(.3f);Check(ball.Holder==actor&&!b.Tracking,"OS touch cancellation never releases");
    Finger(81,Point(b),TouchPhase.Began);b.SendMessage("OnApplicationPause",true);app.view.SendMessage("OnApplicationPause",true);Finger(81,Point(b),TouchPhase.Ended);yield return new WaitForSeconds(.2f);Check(ball.Holder==actor&&!b.Tracking&&!(b.pass?app.view.PassCharging:app.view.ShotCharging),"suspension cancels owned gesture");
   }
   yield return TimedFallback(BasketballDragChoice.Upper);yield return TimedFallback(BasketballDragChoice.Diagonal);
   yield return Pickup(1.6f);Finger(81,Point(shoot),TouchPhase.Began);Finger(81,Destination(shoot,BasketballDragChoice.Diagonal),TouchPhase.Moved);yield return null;Check(!app.view.ShotNeedsTiming&&!hud.ShotGuide.TimingVisible,"eligible layup hides timing arc");yield return Capture("ready-layup");Finger(81,Destination(shoot,BasketballDragChoice.Cancel),TouchPhase.Moved);Finger(81,Destination(shoot,BasketballDragChoice.Cancel),TouchPhase.Ended);
   yield return Pickup(3.6f);DevelopmentProbe.TurnCommand=new PlayerCommand{move=Vector2.up,heading=actor.transform.eulerAngles.y};yield return new WaitForSeconds(.12f);Finger(81,Point(shoot),TouchPhase.Began);Finger(81,Destination(shoot,BasketballDragChoice.Upper),TouchPhase.Moved);yield return null;Check(app.view.FinishAvailability==BasketballFinishReason.Ready&&!hud.ShotGuide.TimingVisible,"running dunk hides timing arc only when eligible");yield return Capture("ready-dunk");Finger(81,Destination(shoot,BasketballDragChoice.Cancel),TouchPhase.Moved);Finger(81,Destination(shoot,BasketballDragChoice.Cancel),TouchPhase.Ended);DevelopmentProbe.TurnCommand=default;
   yield return Pickup(8);Finger(81,Point(shoot),TouchPhase.Began);hud.ShowSettings();Check(app.view.InputBlocked&&!app.view.ShotCharging&&!shoot.Tracking,"settings clears held input");Finger(81,Point(shoot),TouchPhase.Ended);yield return null;Check(app.view.ReadCommand().move==Vector2.zero,"settings blocks movement commands");CheckLabels("settings");yield return CheckVolumeControls();yield return Capture("settings");
   foreach(int camera in new[]{0,1,2}){hud.SelectCamera(camera);Check(app.view.mode==camera,"camera selection "+camera);}
   hud.OpenGuide();for(int i=0;i<3;i++){hud.SelectGuide(i);yield return null;CheckLabels("guide-"+i);yield return Capture("guide-"+i);}
   hud.OpenLeave();yield return null;CheckLabels("leave");yield return Capture("leave");hud.CloseMenu();Check(!app.view.InputBlocked&&!app.view.ShotCharging,"return does not resurrect hold");
   foreach(var size in new[]{new Vector2Int(1280,720),new Vector2Int(1560,720),new Vector2Int(1600,1200)}){
    Screen.SetResolution(size.x,size.y,FullScreenMode.Windowed);yield return new WaitForSeconds(.45f);Canvas.ForceUpdateCanvases();yield return null;CheckScreen(shoot.transform as RectTransform);CheckScreen(pass.transform as RectTransform);hud.ShowSettings();yield return null;CheckLabels("settings-"+size);yield return Capture("settings-"+size.x+"x"+size.y);hud.OpenGuide();hud.SelectGuide(1);yield return null;CheckLabels("controls-"+size);hud.CloseMenu();
   }
   ball.ResetHome();ball.autoPickup=false;actor.capsule.enabled=false;actor.transform.position+=Vector3.right*45;actor.capsule.enabled=true;yield return new WaitForSeconds(.5f);var marker=FindFirstObjectByType<StadiumPlayerIndicator>();Check(marker&&marker.Showing,"offscreen player marker visible during play");hud.ShowSettings();yield return null;Check(marker&&!marker.Showing,"player marker hides behind settings");hud.CloseMenu();
   Finish();
  }
  IEnumerator CheckVolumeControls(){
   foreach(var slider in hud.GetComponentsInChildren<Slider>()){
    float original=slider.value;var track=(RectTransform)slider.transform;var thumb=slider.handleRect;
    Check(Mathf.Abs(thumb.rect.width-thumb.rect.height)<.1f&&thumb.rect.height<=track.rect.height,slider.name+" round thumb stays inside track");
    Vector2 left=RectTransformUtility.WorldToScreenPoint(null,track.TransformPoint(new Vector3(track.rect.xMin+1,track.rect.center.y,0)));
    Vector2 right=RectTransformUtility.WorldToScreenPoint(null,track.TransformPoint(new Vector3(track.rect.xMax-1,track.rect.center.y,0)));
    Finger(95,left,TouchPhase.Began);yield return null;Check(slider.value<.01f,slider.name+" touch reaches zero");
    Finger(95,right,TouchPhase.Moved);yield return null;Check(slider.value>.99f,slider.name+" drag reaches full volume");
    Finger(95,right,TouchPhase.Ended);slider.value=original;yield return null;
   }
  }
  IEnumerator TimedFallback(BasketballDragChoice choice){
   yield return Pickup(10);Finger(81,Point(shoot),TouchPhase.Began);Finger(81,Destination(shoot,choice),TouchPhase.Moved);Finger(81,Destination(shoot,BasketballDragChoice.Cancel),TouchPhase.Moved);Finger(81,Destination(shoot,choice),TouchPhase.Moved);Check(shoot.Tracking,"cancel zone can be left before a real timed release");float end=Time.time+6;while(Mathf.Abs(app.view.ShotPower-BasketballBall.SweetSpot)>app.view.ShotWindow&&Time.time<end)yield return null;
   Check(app.view.ShotNeedsTiming,"far finish still requires timing");float power=app.view.ShotPower;uint shots=ball.ShotCount;Finger(81,Destination(shoot,choice),TouchPhase.Ended);yield return new WaitForSeconds(1);
   Check(ball.ShotCount==shots+1&&!actor.BasketballMotion.Finishing,"far "+choice+" releases exactly one normal shot");Check(Mathf.Abs(ball.LastReleasePower-power)<.02f,"far finish preserves displayed release phase");
  }
  void CheckLabels(string screen){
   Canvas.ForceUpdateCanvases();bool fonts=true,glyphs=true;
   foreach(var t in hud.GetComponentsInChildren<Text>()){
    if(!t.isActiveAndEnabled||t.text.Length==0||t.GetComponentsInParent<CanvasGroup>().Any(g=>g.alpha<.01f))continue;
    fonts&=t.font&&(t.font==SportsUIStyle.DisplayFont||t.font==SportsUIStyle.TextFont);
    glyphs&=t.font&&t.text.All(c=>char.IsWhiteSpace(c)||t.font.HasCharacter(c));
    Check(t.preferredHeight<=t.rectTransform.rect.height+.1f,screen+" text fits: "+t.text.Replace('\n',' ')+" ["+t.preferredHeight+"/"+t.rectTransform.rect.height+"]");
   }
   Check(fonts,screen+" uses sports fonts throughout");Check(glyphs,screen+" contains no missing font glyphs");
  }
  void CheckScreen(RectTransform r){var corners=new Vector3[4];r.GetWorldCorners(corners);Check(corners.All(p=>{var s=RectTransformUtility.WorldToScreenPoint(null,p);return Screen.safeArea.Contains(s);}),r.name+" within safe screen at "+Screen.width+"x"+Screen.height);}
  void Finish(){if(module)module.inputOverride=previous;if(input)Destroy(input);DevelopmentProbe.TurnCommandActive=false;DevelopmentProbe.TurnCommand=default;if(hud)hud.CloseMenu();if(app){app.view.ClearMatchInput();app.view.mode=originalCamera;PlayerPrefs.SetInt("camera",originalCamera);PlayerPrefs.Save();}File.AppendAllText(Path.Combine(folder,"results.txt"),"BASKETBALL_UI_COMPLETE success="+!failed+"\n");Application.Quit(failed?1:0);}
 }
 public sealed class BasketballUIReviewInput:BaseInput {
  readonly List<Touch> touches=new();
  public void Set(int id,Vector2 p,TouchPhase phase){int i=touches.FindIndex(t=>t.fingerId==id);Vector2 old=i>=0?touches[i].position:p;var touch=new Touch{fingerId=id,position=p,deltaPosition=p-old,phase=phase,type=TouchType.Direct};if(i<0)touches.Add(touch);else touches[i]=touch;}
  public void Settle(){for(int i=touches.Count-1;i>=0;i--){var t=touches[i];if(t.phase==TouchPhase.Ended||t.phase==TouchPhase.Canceled)touches.RemoveAt(i);else {t.phase=TouchPhase.Stationary;t.deltaPosition=Vector2.zero;touches[i]=t;}}}
  public void Clear()=>touches.Clear();public override bool mousePresent=>false;public override bool touchSupported=>true;public override int touchCount=>touches.Count;public override Touch GetTouch(int i)=>touches[i];public override float GetAxisRaw(string axis)=>0;public override bool GetButtonDown(string name)=>false;
 }
}
#endif
