#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using WhatTheFish;

// Exercises the actual Editor Play mode and HUD, where missing components use Unity null semantics.
[InitializeOnLoad]
public static class GolfEditorStartReview {
 const string Key="WhatTheFish.GolfEditorStartReview";
 static double deadline;static int frames;
 static string Folder=>Path.GetFullPath(Path.Combine(Application.dataPath,"../../Builds/GolfEditorStartQA"));
 static GolfEditorStartReview(){
  EditorApplication.playModeStateChanged+=Changed;
  if(SessionState.GetBool(Key,false))EditorApplication.update+=Poll;
 }
 static void Check(bool value,string message){if(!value)throw new Exception(message);File.AppendAllText(Path.Combine(Folder,"editor.txt"),"PASS "+message+"\n");}
 public static void Run(){
  Directory.CreateDirectory(Folder);File.WriteAllText(Path.Combine(Folder,"editor.txt"),"");
  var node=new GameObject("Missing collider regression fixture");
  try{
   var hole=new GolfHole{cup=node.transform,cupRadius=.1425f,cupDepth=.24f};
   var trigger=node.AddComponent<GolfHoleTrigger>();trigger.Bind(null,hole);
   var collider=node.GetComponent<BoxCollider>();Check(collider&&collider.isTrigger,"missing BoxCollider created in Editor");
   trigger.Bind(null,hole);Check(node.GetComponents<BoxCollider>().Length==1,"rebinding reuses collider");
   UnityEngine.Object.DestroyImmediate(collider);trigger.Bind(null,hole);
   Check(node.GetComponent<BoxCollider>(),"destroyed collider repaired in Editor");
  }finally{UnityEngine.Object.DestroyImmediate(node);}
  SessionState.SetBool(Key,true);SessionState.SetString(Key+"Result","");
  EditorSceneManager.OpenScene("Assets/_Game/Scenes/Bootstrap.unity");
  EditorApplication.update+=Poll;EditorApplication.EnterPlaymode();
 }
 static void Changed(PlayModeStateChange state){
  if(!SessionState.GetBool(Key,false))return;
  if(state==PlayModeStateChange.EnteredPlayMode){deadline=EditorApplication.timeSinceStartup+120;frames=0;}
  if(state!=PlayModeStateChange.EnteredEditMode)return;
  SessionState.SetBool(Key,false);EditorApplication.update-=Poll;
  var result=SessionState.GetString(Key+"Result","FAIL no Play mode result");File.AppendAllText(Path.Combine(Folder,"editor.txt"),result+"\n");
  if(!result.StartsWith("PASS")){Debug.LogError(result);EditorApplication.Exit(1);return;}
  try{ProjectBuilder.BuildWindowsPlayer("../Builds/GolfEditorStartQA/Player/WhatTheFish.exe");EditorApplication.Exit(0);}
  catch(Exception e){Debug.LogException(e);EditorApplication.Exit(1);}
 }
 static void Poll(){
  if(!SessionState.GetBool(Key,false)||!EditorApplication.isPlaying||EditorApplication.isPaused||deadline==0)return;
  try{
   if(EditorApplication.timeSinceStartup>deadline)throw new Exception("Golf HUD not ready in Editor Play mode");
   var app=AppRoot.Instance;if(!app||app.SelectedSport!=SportId.Golf||!app.Exploring)return;
   var ui=UnityEngine.Object.FindFirstObjectByType<GolfSwingButton>();if(!ui||!ui.startButton||++frames<10)return;
   var match=GolfMatchManager.Instance;
   Check(match&&match.HoleTriggers.Length==5,"Golf manager initializes all five hole triggers in Play mode");
   Check(match.HoleTriggers.All(t=>t&&t.GetComponent<BoxCollider>()&&t.GetComponent<BoxCollider>().isTrigger),"all five capture colliders exist");
   Check(ui.startButton.gameObject.activeInHierarchy&&ui.startButton.interactable,"Start golf match button visible and enabled in Editor");
   ui.startButton.onClick.Invoke();
   Check(match.State.Running&&match.State.Players.Count()==1,"HUD Start click begins offline Golf match");
   Check(match.Ball(app.LocalAthlete)&&match.Ball(app.LocalAthlete).Live,"live player ball spawned");
   SessionState.SetString(Key+"Result","PASS GOLF_EDITOR_START_COMPLETE");EditorApplication.ExitPlaymode();
  }catch(Exception e){SessionState.SetString(Key+"Result","FAIL "+e.Message);Debug.LogException(e);EditorApplication.ExitPlaymode();}
 }
}
#endif
