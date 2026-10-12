#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace WhatTheFish {
 public sealed class FootballUIReview:MonoBehaviour {
  string folder;bool failed;AppRoot app;Athlete actor,rival;FootballBall ball;FootballHUD hud;KickButton kick;StandaloneInputModule module;BaseInput previous;BasketballUIReviewInput input;int originalCamera;Vector3 home;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Init(){var a=Environment.GetCommandLineArgs();int i=Array.IndexOf(a,"-footballUIReview");if(i>=0&&i+1<a.Length)new GameObject("Football UI review").AddComponent<FootballUIReview>().folder=a[i+1];}
  void Check(bool ok,string name){failed|=!ok;File.AppendAllText(Path.Combine(folder,"results.txt"),(ok?"PASS ":"FAIL ")+name+"\n");}
  void Finger(int id,Vector2 p,TouchPhase phase){input.Set(id,p,phase);EventSystem.current.SendMessage("OnApplicationFocus",true,SendMessageOptions.DontRequireReceiver);module.UpdateModule();module.Process();input.Settle();}
  Vector2 Point(Component c)=>RectTransformUtility.WorldToScreenPoint(null,c.transform.position);
  void Place(Athlete a,Vector3 p){a.capsule.enabled=false;a.transform.SetPositionAndRotation(p,Quaternion.identity);a.capsule.enabled=true;a.ResetLocomotion();Physics.SyncTransforms();}
  IEnumerator Pickup(){input.Clear();hud.CloseMenu();Place(actor,home);if(rival)Place(rival,home+Vector3.right*8);ball.ResetBall();ball.Body.position=ball.FootPosition(actor);ball.Body.linearVelocity=Vector3.zero;Physics.SyncTransforms();ball.RefreshControl(actor);yield return new WaitForSeconds(.2f);Check(ball.CurrentController==actor,"fixture acquires ball");Canvas.ForceUpdateCanvases();}
  IEnumerator Capture(string name){
   yield return new WaitForEndOfFrame();var camera=Camera.main;var canvas=hud.GetComponentInParent<Canvas>();var oldMode=canvas.renderMode;var oldCamera=canvas.worldCamera;float oldPlane=canvas.planeDistance;
   canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;Canvas.ForceUpdateCanvases();var rt=RenderTexture.GetTemporary(Screen.width,Screen.height,24);var old=RenderTexture.active;var target=camera.targetTexture;
   camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;var image=new Texture2D(Screen.width,Screen.height,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,Screen.width,Screen.height),0,0);image.Apply();File.WriteAllBytes(Path.Combine(folder,name+".png"),image.EncodeToPNG());Destroy(image);camera.targetTexture=target;RenderTexture.active=old;RenderTexture.ReleaseTemporary(rt);canvas.renderMode=oldMode;canvas.worldCamera=oldCamera;canvas.planeDistance=oldPlane;Canvas.ForceUpdateCanvases();yield return null;
  }

  IEnumerator Start(){
   Directory.CreateDirectory(folder);File.WriteAllText(Path.Combine(folder,"results.txt"),"");float until=Time.realtimeSinceStartup+90;
   while((!(app=AppRoot.Instance)||!app.Exploring||!FootballHUD.Instance)&&Time.realtimeSinceStartup<until)yield return null;
   hud=FootballHUD.Instance;ball=FootballBall.Instance;actor=app?app.LocalAthlete:null;if(!hud||!ball||!actor){Check(false,"football initialized");Finish();yield break;}
   originalCamera=app.view.mode;app.view.mode=1;app.view.yaw=0;DevelopmentProbe.Driving=false;DevelopmentProbe.TurnCommandActive=true;DevelopmentProbe.TurnCommand=default;home=ball.KickoffPosition-Vector3.up*ball.WorldRadius+Vector3.back*5;
   kick=hud.Kick;module=EventSystem.current.GetComponent<StandaloneInputModule>();previous=module.inputOverride;input=EventSystem.current.gameObject.AddComponent<BasketballUIReviewInput>();module.inputOverride=input;
   yield return new WaitForSeconds(1);yield return Pickup();Check(((RectTransform)kick.transform).rect.size==Vector2.one*108,"approved primary size 108");Check(kick.label.font==SportsUIStyle.DisplayFont,"sports display font loaded");Check(hud.GetComponentsInChildren<Image>(true).All(i=>i.sprite),"all illustrated assets loaded");CheckLabels("ready");yield return Capture("ready");
   Finger(81,Point(kick),TouchPhase.Began);yield return new WaitForSeconds(.35f);Check(kick.Tracking&&app.view.Charging&&app.view.Charge>.25f,"real touch module begins charged kick");yield return Capture("charge");
   var stick=app.view.stick;Finger(82,Point(stick),TouchPhase.Began);Finger(82,Point(stick)+Vector2.right*50,TouchPhase.Moved);Check(stick.value.x>0&&kick.Tracking,"second thumb moves independently");Finger(82,Point(stick),TouchPhase.Ended);
   var foreign=new PointerEventData(EventSystem.current){pointerId=99,position=Point(kick.cancelArea)};kick.OnDrag(foreign);kick.OnPointerUp(foreign);Check(kick.Tracking&&!kick.CancelArmed,"foreign finger cannot cancel or release");
   float power=app.view.Charge;Finger(81,Point(kick.cancelArea),TouchPhase.Moved);Check(kick.CancelArmed&&kick.Tracking&&app.view.Charging&&ball.CurrentController==actor,"cancel only arms on entry");yield return null;Check(kick.guide.Cue=="Release to cancel","cancel release instruction");yield return Capture("cancel-armed");
   Finger(81,Point(kick),TouchPhase.Moved);Check(!kick.CancelArmed&&app.view.Charging&&app.view.Charge>=power,"sliding back preserves charge");yield return new WaitForSeconds(1.1f);Check(app.view.Charge==1&&ball.CurrentController==actor,"full power holds without auto kick");CheckLabels("full-power");yield return Capture("full-power");
   Finger(81,Point(kick.cancelArea),TouchPhase.Moved);Finger(81,Point(kick.cancelArea),TouchPhase.Ended);yield return new WaitForSeconds(.3f);Check(!kick.Tracking&&!app.view.Charging&&ball.CurrentController==actor&&!app.view.ReadCommand().kick,"lift in cancel keeps ball with no delayed kick");
   Finger(81,Point(kick),TouchPhase.Began);Finger(81,Point(kick),TouchPhase.Canceled);yield return null;Check(!kick.Tracking&&ball.CurrentController==actor,"OS canceled touch never releases ball");
   Finger(81,Point(kick),TouchPhase.Began);kick.SendMessage("OnApplicationPause",true);app.view.SendMessage("OnApplicationPause",true);Finger(81,Point(kick),TouchPhase.Ended);yield return null;Check(!app.view.Charging&&ball.CurrentController==actor,"suspension clears owned gesture");
   Finger(81,Point(kick),TouchPhase.Began);yield return new WaitForSeconds(1.1f);Finger(81,Point(kick.cancelArea),TouchPhase.Moved);Finger(81,Point(kick),TouchPhase.Moved);Finger(81,Point(kick),TouchPhase.Ended);var command=app.view.ReadCommand();Check(command.kick&&command.kickCharge==1,"slide back then release queues one full-power kick");actor.Simulate(command,.02f);Check(!ball.CurrentController&&ball.Body.linearVelocity.magnitude>20,"real full-power ball launch");Check(!app.view.ReadCommand().kick,"release cannot queue twice");
   yield return Pickup();Finger(81,Point(kick),TouchPhase.Began);hud.ShowSettings();Finger(81,Point(kick),TouchPhase.Ended);yield return null;Check(app.view.InputBlocked&&!app.view.Charging&&!kick.Tracking&&ball.CurrentController==actor,"opening settings cancels gesture and blocks commands");Check(!app.view.BeginKick(8)&&!app.view.BeginDash(8)&&!app.view.BeginPressure(8)&&app.view.ReadCommand().move==Vector2.zero,"all football holds blocked by settings");CheckLabels("camera");yield return Capture("settings-camera");
   foreach(int mode in new[]{0,1,2}){hud.SelectCamera(mode);Check(app.view.mode==mode,"camera selection "+mode);}
   hud.OpenGuide();for(int i=0;i<3;i++){hud.SelectGuide(i);yield return null;CheckLabels("guide-"+i);yield return Capture("guide-"+i);}
   hud.OpenLeave();yield return null;CheckLabels("leave");yield return Capture("leave");hud.CloseMenu();Check(!app.view.InputBlocked&&!app.view.Charging,"back to play cannot resurrect old press");
   rival=Instantiate(app.athletePrefab).GetComponent<Athlete>();rival.Setup();Place(actor,home);Place(rival,home+Vector3.forward*3);ball.ResetBall();ball.Body.position=ball.FootPosition(rival);Physics.SyncTransforms();ball.RefreshControl(rival);yield return new WaitForSeconds(.2f);
   var pressure=kick.GetComponent<FootballEffortButton>();var tackle=hud.GetComponentInChildren<TackleButton>();Check(!kick.enabled&&pressure.enabled&&tackle.enabled,"opponent possession swaps primary to pressure");CheckLabels("defense");yield return Capture("defense");Finger(81,Point(pressure),TouchPhase.Began);Check(app.view.PressureHeld,"pressure pointer begins contain");Finger(81,Point(pressure),TouchPhase.Ended);Check(!app.view.PressureHeld,"pressure pointer release clears hold");
   yield return Pickup();Finger(81,Point(kick),TouchPhase.Began);ball.ResetBall();ball.Body.position=ball.FootPosition(rival);Physics.SyncTransforms();ball.RefreshControl(rival);yield return null;yield return null;Finger(81,Point(kick),TouchPhase.Ended);Check(!app.view.Charging&&!app.view.PressureHeld&&!app.view.ReadCommand().kick,"role turnover cancels finger without another action");
   ball.ResetBall();ball.Body.position=home+Vector3.right*15+Vector3.up*ball.WorldRadius;yield return new WaitForSeconds(.2f);Check(pressure.enabled&&!pressure.button.interactable&&tackle.enabled,"loose ball keeps tackle but disables pressure");yield return Capture("loose-ball");
   var dash=hud.GetComponentsInChildren<FootballEffortButton>().Single(b=>!b.pressure);actor.ResetLocomotion();Finger(81,Point(dash),TouchPhase.Began);Check(app.view.DashHeld,"dash touch begins");DevelopmentProbe.TurnCommand=new PlayerCommand{move=Vector2.up};yield return new WaitForSeconds(3.5f);Check(actor.FootballEffort.Exhausted&&!actor.FootballEffort.Dashing,"held dash exhausts with rest feedback");yield return Capture("exhausted");Finger(81,Point(dash),TouchPhase.Ended);DevelopmentProbe.TurnCommand=default;Check(!app.view.DashHeld,"exhausted dash remains releasable");
   var score=hud.GetComponentInChildren<FootballMatchHUD>();score.enabled=false;var present=typeof(FootballMatchHUD).GetMethod("Present",BindingFlags.Instance|BindingFlags.NonPublic);
   foreach(var phase in new[]{FootballMatchPhase.TeamSelection,FootballMatchPhase.Kickoff,FootballMatchPhase.Regulation,FootballMatchPhase.GoalReset,FootballMatchPhase.Overtime,FootballMatchPhase.Finished}){var s=new FootballMatchSnapshot{phase=phase,scoreA=2,scoreB=2,teamA=1,teamB=1,teamCapacity=1,result=FootballMatchResult.Draw};present.Invoke(score,new object[]{s,38d,FootballTeam.A,true,0f});yield return null;CheckLabels("match-"+phase);yield return Capture("match-"+phase);}score.enabled=true;
   foreach(var size in new[]{new Vector2Int(1280,720),new Vector2Int(1560,720),new Vector2Int(1600,1200)}){
    hud.CloseMenu();yield return Pickup();Finger(81,Point(kick),TouchPhase.Began);Screen.SetResolution(size.x,size.y,FullScreenMode.Windowed);yield return new WaitForSeconds(.5f);Canvas.ForceUpdateCanvases();yield return null;Finger(81,Point(kick),TouchPhase.Ended);Check(!kick.Tracking&&!app.view.Charging&&ball.CurrentController==actor,"resize cancels old gesture "+size);CheckScreen((RectTransform)kick.transform);CheckScreen((RectTransform)dash.transform);yield return Capture("ready-"+size.x+"x"+size.y);hud.SelectMenu(0);hud.ShowSettings();yield return null;CheckLabels("settings-"+size);yield return Capture("settings-"+size.x+"x"+size.y);hud.OpenGuide();for(int i=0;i<3;i++){hud.SelectGuide(i);yield return null;CheckLabels("guide-"+i+"-"+size);}hud.SelectGuide(0);yield return Capture("controls-"+size.x+"x"+size.y);hud.CloseMenu();
   }
   Finish();
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
  void CheckScreen(RectTransform r){var c=new Vector3[4];r.GetWorldCorners(c);Check(c.All(p=>Screen.safeArea.Contains(RectTransformUtility.WorldToScreenPoint(null,p))),r.name+" inside safe area "+Screen.width+"x"+Screen.height);}
  void Finish(){if(module)module.inputOverride=previous;if(input)Destroy(input);DevelopmentProbe.TurnCommandActive=false;DevelopmentProbe.TurnCommand=default;if(hud)hud.CloseMenu();if(app){app.view.mode=originalCamera;PlayerPrefs.SetInt("camera",originalCamera);PlayerPrefs.Save();}if(rival)Destroy(rival.gameObject);File.AppendAllText(Path.Combine(folder,"results.txt"),"FOOTBALL_UI_COMPLETE success="+!failed+"\n");Application.Quit(failed?1:0);}
 }
}
#endif
