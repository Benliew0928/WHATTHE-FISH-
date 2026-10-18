#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
namespace WhatTheFish {
 public sealed class BasketballTrailReview:MonoBehaviour {
  string folder,shared;AppRoot app;BasketballBall ball;BasketballTrail trail;Athlete actor;bool failed;int checks,original;bool reduced;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Init(){var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,"-basketballTrailReview");if(i>=0&&i+1<args.Length)new GameObject("Basketball effects checks").AddComponent<BasketballTrailReview>().folder=args[i+1];}
  void Check(bool ok,string text){checks++;failed|=!ok;File.AppendAllText(Path.Combine(folder,"results.txt"),(ok?"PASS ":"FAIL ")+text+"\n");}
  void Signal(string name)=>File.WriteAllText(Path.Combine(shared,name),"ready");
  bool Has(string name)=>File.Exists(Path.Combine(shared,name));
  IEnumerator Await(string name){float end=Time.realtimeSinceStartup+15;while(!Has(name)&&Time.realtimeSinceStartup<end)yield return null;Check(Has(name),"peer "+name);}
  IEnumerator Start(){
   Directory.CreateDirectory(folder);shared=Directory.GetParent(folder).FullName;File.WriteAllText(Path.Combine(folder,"results.txt"),"");
   float end=Time.realtimeSinceStartup+80;while((!DevelopmentProbe.ReadyToReviewSport||!AppRoot.Instance||!AppRoot.Instance.LocalAthlete||!BasketballBall.Active)&&Time.realtimeSinceStartup<end)yield return null;
   app=AppRoot.Instance;ball=BasketballBall.Active;actor=app?app.LocalAthlete:null;Check(app&&ball&&actor,"basketball initialized");if(!app||!ball||!actor){Finish();yield break;}
   original=BasketballTrailStyle.Selected;reduced=MenuPreferences.ReducedMotion;MenuPreferences.SetMotion(false);trail=ball.GetComponent<BasketballTrail>();
   DevelopmentProbe.Driving=false;DevelopmentProbe.TurnCommandActive=true;DevelopmentProbe.TurnCommand=default;yield return new WaitForSeconds(.6f);
   Check(trail&&BasketballTrailStyle.Material(0)&&BasketballTrailStyle.Material(0).shader.isSupported,"effects attached and shader supported");
   if(app.rooms.Connected&&!ball.Authority){yield return Guest();Finish();yield break;}
   if(app.rooms.Connected)yield return Await("guest-ready");
   var hud=BasketballHUD.Instance;hud.ShowSettings();yield return null;
   var customize=hud.GetComponentsInChildren<Button>(true).First(b=>b.name=="Customize basketball");customize.onClick.Invoke();yield return null;
   Check(GameObject.Find("Basketball trail choices"),"court settings opens effects picker");yield return Capture("customize-court",0);
   for(int i=0;i<BasketballTrailStyle.Count;i++){
    hud.GetComponentsInChildren<Button>(true).First(b=>b.name=="Trail "+BasketballTrailStyle.Names[i]).onClick.Invoke();yield return null;
    Check(BasketballTrailStyle.Selected==i&&trail.Style==i,"picker applies style "+i);
   }
   hud.CloseMenu();
   for(int i=0;i<BasketballTrailStyle.Count;i++){
    BasketballTrailStyle.Select(i);yield return Possess();Check(!trail.Wake.Line.enabled,"held ball has no flight smear "+i);
    uint before=ball.PassCount+ball.ShotCount;int pulses=trail.ReleasePulses;
    bool shot=i%2==0;
    if(shot)Check(ball.TryShoot(actor,0,BasketballBall.SweetSpot),"real shot accepted "+i);
    else {Check(ball.BeginPassCharge(actor,0,ball.Defense.play),"real pass charge "+i);yield return new WaitForSeconds(.45f);Check(ball.ReleasePassCharge(actor,0,ball.Defense.play,ball.PassClock,i==3?-1:i==5?1:0),"real pass release "+i);}
    end=Time.time+3;while(ball.PassCount+ball.ShotCount==before&&Time.time<end)yield return null;ball.autoPickup=false;
    Check(ball.PassCount+ball.ShotCount==before+1,"one ball release "+i);yield return new WaitForEndOfFrame();Check(trail.ReleasePulses==pulses+1,"release ring "+i);
    if(app.rooms.Connected)Signal("flight-"+i);
    yield return Observe(.36f,"style "+i);Check(trail.Wake.Line.enabled&&trail.Wake.Line.positionCount>3,"visible moving ribbon "+i);
    yield return Capture("style-"+i+"-close",1);yield return Capture("style-"+i+"-court",2);
    if(app.rooms.Connected)yield return Await("seen-"+i);
   }
   ball.autoPickup=false;ball.Place(new Vector3(0,1.8f,-4),Quaternion.identity,new Vector3(3,-4,1),Vector3.zero);yield return null;int bounces=trail.BouncePulses;
   end=Time.time+2;while(trail.BouncePulses==bounces&&Time.time<end)yield return null;Check(trail.BouncePulses>bounces,"floor rebound produces an impact ring");yield return Capture("floor-impact",1);
   ball.Place(new Vector3(3,2,-2),Quaternion.identity,Vector3.zero,Vector3.zero);yield return new WaitForEndOfFrame();Check(trail.Wake.Line.positionCount<2&&trail.ActivePulses==0,"teleport clears wake and pulses");
   yield return Possess();Check(!trail.Wake.Line.enabled&&trail.ActivePulses==0,"catch clears old flight");
   MenuPreferences.SetMotion(true);int quiet=trail.ReleasePulses;Check(ball.TryShoot(actor,0,BasketballBall.SweetSpot),"reduced motion shot accepted");uint quietShots=ball.ShotCount;end=Time.time+3;while(ball.ShotCount==quietShots&&Time.time<end)yield return null;ball.autoPickup=false;
   yield return Observe(.4f,"reduced motion");Check(trail.Wake.Length<=1.501f&&trail.ReleasePulses==quiet&&trail.ActivePulses==0,"reduced motion short wake without pulses");MenuPreferences.SetMotion(false);
   trail.enabled=false;Check(trail.Wake.Line.positionCount==0&&trail.ActivePulses==0,"disable clears all effects");trail.enabled=true;
   BasketballTrailStyle.Select(4);BasketballTrailStyle.Load();Check(BasketballTrailStyle.Selected==4,"saved preference reloads");BasketballTrailStyle.Select(99);Check(BasketballTrailStyle.Selected==0,"invalid preference falls back");
   if(app.rooms.Connected){Signal("host-done");yield return Await("guest-done");}
   else {app.LeaveCourt();yield return new WaitForSeconds(.3f);app.Show("customize");yield return null;Check(GameObject.Find("Personal basketball style"),"main menu opens basketball customization");yield return Capture("customize-main",0);}
   Finish();
  }
  IEnumerator Possess(){
   app.view.ClearMatchInput();ball.ResetHome();actor.capsule.enabled=false;actor.transform.SetPositionAndRotation(new Vector3(0,.07f,-5),Quaternion.identity);actor.ResetLocomotion();actor.capsule.enabled=true;Physics.SyncTransforms();ball.GiveMatchPossession(actor);ball.autoPickup=true;app.view.yaw=0;yield return new WaitForSeconds(.35f);
  }
  IEnumerator Observe(float seconds,string name){
   float end=Time.time+seconds,maxError=0;int visible=0;
   while(Time.time<end){yield return new WaitForEndOfFrame();if(trail.Wake.Line.enabled){visible++;maxError=Mathf.Max(maxError,Vector3.Distance(trail.Wake.Head,ball.transform.position));}}
   Check(visible>2&&maxError<.0001f,"head pinned to rendered ball "+name+" frames="+visible+" error="+maxError.ToString("F6"));
   Check(trail.Wake.Length<=4.401f,"bounded trail length "+name);
  }
  IEnumerator Guest(){
   BasketballTrailStyle.Select(2);Signal("guest-ready");
   for(int i=0;i<BasketballTrailStyle.Count;i++){
    yield return Await("flight-"+i);yield return Observe(.32f,"guest flight "+i);
    Check(trail.Style==2&&trail.Wake.Line.sharedMaterial==BasketballTrailStyle.Material(2),"guest choice independent of host "+i);
    if(i==0)yield return Capture("guest-shot",2);Signal("seen-"+i);
   }
   Check(trail.ReleasePulses>=6,"guest sees replicated release accents");yield return Await("host-done");Signal("guest-done");
  }
  IEnumerator Capture(string name,int view){
   yield return new WaitForEndOfFrame();var camera=Camera.main;var p=camera.transform.position;var q=camera.transform.rotation;float fov=camera.fieldOfView;
   if(view>0){var centre=ball.transform.position;camera.transform.position=centre+(view==1?new Vector3(2.8f,1.6f,1.8f):new Vector3(6,7,-8));camera.transform.LookAt(centre+Vector3.back*.6f);camera.fieldOfView=view==1?58:55;}
   var canvases=FindObjectsByType<Canvas>(FindObjectsSortMode.None).Where(c=>c.isRootCanvas&&c.renderMode==RenderMode.ScreenSpaceOverlay).ToArray();
   foreach(var c in canvases){c.renderMode=RenderMode.ScreenSpaceCamera;c.worldCamera=camera;c.planeDistance=1;}Canvas.ForceUpdateCanvases();
   var rt=RenderTexture.GetTemporary(Screen.width,Screen.height,24);var prior=RenderTexture.active;var target=camera.targetTexture;camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;
   var pixels=new Texture2D(Screen.width,Screen.height,TextureFormat.RGB24,false);pixels.ReadPixels(new Rect(0,0,Screen.width,Screen.height),0,0);pixels.Apply();File.WriteAllBytes(Path.Combine(folder,name+".png"),pixels.EncodeToPNG());Destroy(pixels);camera.targetTexture=target;RenderTexture.active=prior;RenderTexture.ReleaseTemporary(rt);
   foreach(var c in canvases){c.renderMode=RenderMode.ScreenSpaceOverlay;c.worldCamera=null;}camera.transform.SetPositionAndRotation(p,q);camera.fieldOfView=fov;Canvas.ForceUpdateCanvases();
  }
  void Finish(){BasketballTrailStyle.Select(original);MenuPreferences.SetMotion(reduced);MenuPreferences.Save();File.AppendAllText(Path.Combine(folder,"results.txt"),"BASKETBALL_TRAIL_COMPLETE success="+!failed+" checks="+checks+"\n");Application.Quit(failed?1:0);}
 }
}
#endif
