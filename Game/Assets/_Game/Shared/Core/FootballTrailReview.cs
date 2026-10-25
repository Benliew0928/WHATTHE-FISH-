#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
namespace WhatTheFish {
 public sealed class FootballTrailReview:MonoBehaviour {
  string folder;bool failed;int checks;AppRoot app;FootballBall ball;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Init(){var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,"-footballTrailReview");if(i>=0&&i+1<args.Length)new GameObject("Football trail checks").AddComponent<FootballTrailReview>().folder=args[i+1];}
  void Check(bool value,string name){checks++;failed|=!value;File.AppendAllText(Path.Combine(folder,"results.txt"),(value?"PASS ":"FAIL ")+name+"\n");}
  void Place(Vector3 position){var actor=app.LocalAthlete;actor.capsule.enabled=false;actor.transform.SetPositionAndRotation(position,Quaternion.identity);actor.ResetLocomotion();actor.capsule.enabled=true;Physics.SyncTransforms();}
  IEnumerator Start(){
   EconomyRenderReview.UnlockAll();
   Directory.CreateDirectory(folder);File.WriteAllText(Path.Combine(folder,"results.txt"),"");
   float until=Time.realtimeSinceStartup+80;while((!AppRoot.Instance||!AppRoot.Instance.Exploring||!FootballBall.Instance||!AppRoot.Instance.LocalAthlete)&&Time.realtimeSinceStartup<until)yield return null;
   app=AppRoot.Instance;ball=FootballBall.Instance;Check(app&&ball&&app.LocalAthlete,"football ready");if(!app||!ball){Finish();yield break;}
   DevelopmentProbe.Driving=false;DevelopmentProbe.TurnCommandActive=true;DevelopmentProbe.TurnCommand=default;
   yield return new WaitForSeconds(.6f);
   int original=FootballTrailStyle.Selected;var trail=ball.GetComponent<FootballTrail>();
   Check(Mathf.Abs(ball.WorldRadius-.264f)<.0001f,"collider is exactly twenty percent larger");
   var meshes=ball.GetComponentsInChildren<MeshRenderer>();var bounds=meshes[0].bounds;foreach(var r in meshes)bounds.Encapsulate(r.bounds);
   Check(Mathf.Abs(bounds.size.x-.528f)<.001f&&Mathf.Abs(bounds.size.y-.528f)<.001f,"render mesh matches enlarged collider");
   Check(Mathf.Abs(ball.KickoffPosition.y-ball.Pitch.TransformPoint(new Vector3(0,ball.PitchBounds.max.y,0)).y-ball.WorldRadius)<.003f,"centre origin rests on pitch");
   Check(trail&&trail.Ribbon.sharedMaterial&&trail.Ribbon.sharedMaterial.shader.isSupported,"trail shader supported");
   var hud=FootballHUD.Instance;hud.ShowSettings();hud.SelectMenu(3);yield return new WaitForSeconds(.3f);
   Check(GameObject.Find("Customize page")&&GameObject.Find("Trail choices"),"in-game customize reachable");
   yield return Capture("customize-in-game");
   for(int style=0;style<5;style++){
    var button=hud.GetComponentsInChildren<Button>(true).First(b=>b.name=="Trail "+FootballTrailStyle.Names[style]);button.onClick.Invoke();yield return new WaitForEndOfFrame();
    Check(FootballTrailStyle.Selected==style&&trail.Style==style,"button applies local style "+style);
   }
   hud.CloseMenu();app.view.enabled=false;
   for(int style=0;style<5;style++){
    FootballTrailStyle.Select(style);ball.ResetBall();Place(ball.KickoffPosition+new Vector3(0,-ball.WorldRadius,-.9f));yield return new WaitForSeconds(.4f);
    Check(ball.TryKick(app.LocalAthlete,1),"real kick "+style);
    float flightEnd=Time.time+.28f,headError=0;int visibleFrames=0;
    while(Time.time<flightEnd){yield return new WaitForEndOfFrame();if(trail.Ribbon.enabled){visibleFrames++;headError=Mathf.Max(headError,Vector3.Distance(trail.Wake.Head,ball.transform.position));}}
    Check(visibleFrames>2&&headError<.0001f,"trail head stays pinned throughout kick "+style+" frames="+visibleFrames+" error="+headError);
    Check(trail.Wake.Length<=3.201f,"football wake length capped "+style);
    Check(trail.Style==style&&trail.Wake.Emitting&&trail.Ribbon.positionCount>3,"moving ball emits chosen trail "+style);
    Check(trail.Ribbon.sharedMaterial==FootballTrailStyle.Material(style),"shared material "+style);
    var camera=Camera.main;camera.transform.position=ball.transform.position+new Vector3(2.2f,1.5f,1);camera.transform.LookAt(ball.transform.position+Vector3.back*1.25f);camera.fieldOfView=55;
    yield return Capture("style-"+style+"-close");
    camera.transform.position=ball.transform.position+new Vector3(6,8,-7);camera.transform.LookAt(ball.transform.position);yield return Capture("style-"+style+"-wide");
    ball.ResetBall();Place(ball.KickoffPosition+new Vector3(4,-ball.WorldRadius,-4));yield return null;yield return null;
    var points=new Vector3[trail.Ribbon.positionCount];trail.Ribbon.GetPositions(points);
    Check(points.All(p=>Vector3.Distance(p,ball.transform.position)<.1f),"reset clears teleport streak "+style);
    yield return new WaitForSeconds(.8f);Check(!trail.Wake.Emitting&&!trail.Ribbon.enabled,"stationary effect fades "+style);
   }
   FootballTrailStyle.Select(2);FootballTrailStyle.Load();Check(FootballTrailStyle.Selected==2,"saved selection reloads");
   FootballTrailStyle.Select(99);Check(FootballTrailStyle.Selected==0,"invalid preference falls back safely");
   trail.enabled=false;Check(trail.Ribbon.positionCount==0&&!trail.Wake.Emitting,"disable releases trail history");trail.enabled=true;
   app.view.enabled=true;app.LeaveCourt();yield return new WaitForSeconds(.3f);
   app.Show("customize");yield return new WaitForSeconds(.4f);Check(app.Menu.Page=="customize","main customization page reachable");yield return Capture("customize-main");
   FootballTrailStyle.Select(original);Finish();
  }
  IEnumerator Capture(string name){
   yield return new WaitForEndOfFrame();var camera=Camera.main;var canvases=FindObjectsByType<Canvas>(FindObjectsSortMode.None).Where(c=>c.isRootCanvas&&c.renderMode==RenderMode.ScreenSpaceOverlay).ToArray();
   foreach(var c in canvases){c.renderMode=RenderMode.ScreenSpaceCamera;c.worldCamera=camera;c.planeDistance=1;}Canvas.ForceUpdateCanvases();
   var rt=RenderTexture.GetTemporary(Screen.width,Screen.height,24);var prior=RenderTexture.active;var target=camera.targetTexture;camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;
   var pixels=new Texture2D(Screen.width,Screen.height,TextureFormat.RGB24,false);pixels.ReadPixels(new Rect(0,0,Screen.width,Screen.height),0,0);pixels.Apply();File.WriteAllBytes(Path.Combine(folder,name+".png"),pixels.EncodeToPNG());Destroy(pixels);camera.targetTexture=target;RenderTexture.active=prior;RenderTexture.ReleaseTemporary(rt);
   foreach(var c in canvases){c.renderMode=RenderMode.ScreenSpaceOverlay;c.worldCamera=null;}Canvas.ForceUpdateCanvases();
  }
  void Finish(){File.AppendAllText(Path.Combine(folder,"results.txt"),"FOOTBALL_TRAIL_COMPLETE success="+!failed+" checks="+checks+"\n");Application.Quit(failed?1:0);}
 }
}
#endif
