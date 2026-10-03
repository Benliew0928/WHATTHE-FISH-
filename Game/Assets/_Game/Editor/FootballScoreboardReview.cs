using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using WhatTheFish;

// Explicit editor-only presentation review; never changes authoritative match state.
[InitializeOnLoad]
public static class FootballScoreboardReview {
 // Deterministic render of the real Canvas widgets, without entering or changing a match.
 [MenuItem("WHATTHE FISH?/Football/Capture Team A lettering")]
 public static void CaptureTeamAWordmark(){
  var scene=EditorSceneManager.NewPreviewScene();
  var folder=Path.GetFullPath(Path.Combine(Application.dataPath,"../../Builds/FootballTeamAWordmarkQA/Visuals"));
  Directory.CreateDirectory(folder);
  try{
   var canvas=new GameObject("Scoreboard review",typeof(RectTransform),typeof(Canvas)).GetComponent<Canvas>();
   SceneManager.MoveGameObjectToScene(canvas.gameObject,scene);canvas.renderMode=RenderMode.WorldSpace;
   var rect=canvas.GetComponent<RectTransform>();rect.sizeDelta=new Vector2(1600,900);
   var camera=new GameObject("Review camera",typeof(Camera)).GetComponent<Camera>();
   SceneManager.MoveGameObjectToScene(camera.gameObject,scene);camera.scene=scene;camera.orthographic=true;
   camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.19f,.31f,.24f);camera.nearClipPlane=.1f;camera.farClipPlane=100;
   canvas.worldCamera=camera;
   var hud=FootballMatchHUD.Create(canvas.transform,Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"));hud.enabled=false;
   hud.Preview(new FootballMatchSnapshot{phase=FootballMatchPhase.Regulation,scoreA=2,scoreB=1},137,FootballTeam.A,1);
   Canvas.ForceUpdateCanvases();
   RenderWordmark(camera,folder,"scoreboard-1600x900",1600,900,Vector2.zero,450);
   RenderWordmark(camera,folder,"scoreboard-detail",1140,300,new Vector2(0,382),75);
   RenderWordmark(camera,folder,"team-a-detail",864,288,new Vector2(-187,396),24);
   rect.sizeDelta=new Vector2(900f*844/390,900);Canvas.ForceUpdateCanvases();
   RenderWordmark(camera,folder,"scoreboard-844x390",844,390,Vector2.zero,450);
   hud.Preview(new FootballMatchSnapshot{phase=FootballMatchPhase.TeamSelection,teamA=3,teamB=1,teamCapacity=3},7,FootballTeam.None,1);
   Canvas.ForceUpdateCanvases();RenderWordmark(camera,folder,"selection-844x390",844,390,Vector2.zero,450);
   Debug.Log("TEAM_A_WORDMARK_CAPTURE_PASS: full-size, compact, detail and team-selection Canvas renders captured.");
  }finally{EditorSceneManager.ClosePreviewScene(scene);}
 }
 static void RenderWordmark(Camera camera,string folder,string name,int width,int height,Vector2 centre,float halfHeight){
  var target=new RenderTexture(width,height,24,RenderTextureFormat.ARGB32){antiAliasing=4};
  var texture=new Texture2D(width,height,TextureFormat.RGB24,false);
  var previous=RenderTexture.active;
  try{
   camera.transform.position=new Vector3(centre.x,centre.y,-10);camera.orthographicSize=halfHeight;camera.targetTexture=target;
   camera.Render();RenderTexture.active=target;texture.ReadPixels(new Rect(0,0,width,height),0,0);texture.Apply();
   File.WriteAllBytes(Path.Combine(folder,name+".png"),texture.EncodeToPNG());
  }finally{camera.targetTexture=null;RenderTexture.active=previous;Object.DestroyImmediate(texture);target.Release();Object.DestroyImmediate(target);}
 }
 static int step;static double next;static FootballMatchHUD hud;
 static FootballScoreboardReview(){EditorApplication.playModeStateChanged+=Changed;}
 [MenuItem("WHATTHE FISH?/Football/Review scoreboard presentation")]
 public static void Run(){if(EditorApplication.isPlaying)return;SessionState.SetBool("FootballScoreboardReview",true);EditorApplication.isPlaying=true;}
 static void Changed(PlayModeStateChange state){
  if(state==PlayModeStateChange.ExitingPlayMode){EditorApplication.update-=Tick;return;}
  if(state!=PlayModeStateChange.EnteredPlayMode||!SessionState.GetBool("FootballScoreboardReview",false))return;
  SessionState.SetBool("FootballScoreboardReview",false);step=0;next=EditorApplication.timeSinceStartup+2;EditorApplication.update+=Tick;
 }
 static void Tick(){
  if(EditorApplication.timeSinceStartup<next||!AppRoot.Instance||!FootballBall.Instance)return;
  next=EditorApplication.timeSinceStartup+2;
  if(step==0){AppRoot.Instance.EnterOffline();step++;return;}
  if(step==1){hud=Object.FindFirstObjectByType<FootballMatchHUD>();if(!hud)return;hud.enabled=false;Show(FootballMatchPhase.Regulation,2,1,137);step++;return;}
  switch(step++){
   case 2:Capture("regulation");break;
   case 3:Show(FootballMatchPhase.Kickoff,3,1,3);break;
   case 4:Capture("goal-flash");break;
   case 5:Show(FootballMatchPhase.Regulation,0,0,180);Show(FootballMatchPhase.Overtime,0,0,43);break;
   case 6:Capture("overtime");break;
   case 7:Show(FootballMatchPhase.Finished,0,0,0,FootballMatchResult.Draw);break;
   case 8:Capture("finished");break;
   case 9:hud.Preview(new FootballMatchSnapshot{phase=FootballMatchPhase.TeamSelection,teamA=3,teamB=1,teamCapacity=3},7,FootballTeam.None,1);Canvas.ForceUpdateCanvases();break;
   case 10:Capture("team-selection");break;
   case 11:hud.enabled=true;EditorApplication.update-=Tick;Debug.Log("Scoreboard presentation review captured; live HUD restored.");break;
  }
 }
 static void Show(FootballMatchPhase phase,int a,int b,double seconds,FootballMatchResult result=FootballMatchResult.None){hud.Preview(new FootballMatchSnapshot{phase=phase,scoreA=a,scoreB=b,result=result},seconds,FootballTeam.A,phase==FootballMatchPhase.Kickoff?0:1);Canvas.ForceUpdateCanvases();}
 static void Capture(string name){var path=Path.GetFullPath(Path.Combine(Application.dataPath,"../../Builds/ScoreboardUI"));Directory.CreateDirectory(path);ScreenCapture.CaptureScreenshot(Path.Combine(path,name+".png"));}
}



