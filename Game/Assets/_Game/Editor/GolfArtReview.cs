using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using WhatTheFish;
public static class GolfArtReview {
 public static void Capture(){
  EditorSceneManager.OpenScene("Assets/_Game/Scenes/Bootstrap.unity");
  var app=Object.FindFirstObjectByType<AppRoot>();app.environments.Activate(SportId.Golf);Physics.SyncTransforms();
  var root=app.environments.View;var lines=root.GetComponentsInChildren<MeshRenderer>().Where(r=>r.name.Contains("Flags")||r.name.Contains("TeeMarkers")||r.name.Contains("Cascade")).Select(r=>r.name+" "+r.bounds.ToString()).ToList();
  foreach(var p in new[]{new Vector3(-13,100,-146),new Vector3(13,100,-146),new Vector3(-13,100,146),new Vector3(13,100,146)}){
   if(Physics.Raycast(p,Vector3.down,out var hit,150,1<<8))lines.Add(p+" -> "+hit.point+" "+hit.collider.name);
  }
  File.WriteAllLines("../Builds/golf-coordinate-audit.txt",lines);
  var cam=Camera.main;cam.transform.position=new Vector3(290,260,-350);cam.transform.LookAt(new Vector3(0,3,5));
  ShaderUtil.allowAsyncCompilation=false;var rt=new RenderTexture(1600,1000,24);cam.targetTexture=rt;
  for(int i=0;i<3;i++)cam.Render();RenderTexture.active=rt;var tex=new Texture2D(1600,1000,TextureFormat.RGB24,false);
  tex.ReadPixels(new Rect(0,0,1600,1000),0,0);tex.Apply();File.WriteAllBytes("../Docs/VisualDirection/Golf/05_Unity_Overview.png",tex.EncodeToPNG());
  cam.targetTexture=null;RenderTexture.active=null;Object.DestroyImmediate(rt);Object.DestroyImmediate(tex);
 }
}
