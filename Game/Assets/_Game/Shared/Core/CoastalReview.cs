#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;

namespace WhatTheFish {
 // Explicit development capture mode only. No player traversal or new game controls.
 public sealed class CoastalReview:MonoBehaviour {
  string folder;bool failed;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Init(){
   var args=Environment.GetCommandLineArgs();int index=Array.IndexOf(args,"-coastalReview");
   if(index<0)return;
   var r=new GameObject("Coastal exterior review").AddComponent<CoastalReview>();
   r.folder=index+1<args.Length?args[index+1]:Application.persistentDataPath+"/CoastalReview";
  }
  void Check(bool ok,string label){failed|=!ok;File.AppendAllText(Path.Combine(folder,"review.txt"),(ok?"PASS ":"FAIL ")+label+"\n");}
  IEnumerator Start(){
   Directory.CreateDirectory(folder);File.WriteAllText(Path.Combine(folder,"review.txt"),"");yield return new WaitForSeconds(3);
   var app=AppRoot.Instance;var camera=Camera.main;
   foreach(var sport in new[]{SportId.Football,SportId.Basketball}){
    app.SelectSport(sport);app.EnterOffline();app.view.enabled=false;
    foreach(var canvas in FindObjectsByType<Canvas>(FindObjectsSortMode.None))if(canvas.renderMode!=RenderMode.WorldSpace)canvas.gameObject.SetActive(false);
    app.LocalAthlete.capsule.enabled=false;app.LocalAthlete.gameObject.SetActive(false);
    var coast=app.environments.roots[(int)sport].GetComponentInChildren<CoastalEnvironment>();
    Check(coast!=null,sport+" exterior attached");
    Check(app.environments.roots.Count(r=>r.activeSelf)==1,sport+" one active world");
    Check(coast.GetComponentsInChildren<Collider>().Length>16,sport+" walkable coast collision");
    Check(app.stadium.GetComponentsInChildren<LODGroup>().Length==(sport==SportId.Football?32:48),sport+" original stadium seating retained");
    if(sport==SportId.Basketball)Check(!app.stadium.transform.Find("Ceiling"),"Basketball open sky");
    Check(RenderSettings.skybox==coast.sky&&camera.farClipPlane==3000,sport+" sky and distant scenery configured");
    foreach(var p in app.environments.Current.spawnPositions)Check(Physics.Raycast(p,Vector3.down,2,1<<8),sport+" original spawn floor "+p);
    bool football=sport==SportId.Football;
    var positions=football?new[]{new Vector3(210,112,-285),new Vector3(-230,90,-230),new Vector3(-6,1.25f,-166),new Vector3(96,1.7f,-87),new Vector3(78,1.7f,-61),new Vector3(61,2.0f,-111)}:
     new[]{new Vector3(115,61,-154),new Vector3(-123,48,-125),new Vector3(-5,1.15f,-94),new Vector3(51,1.6f,-45),new Vector3(43,1.65f,-30),new Vector3(31,1.8f,-61)};
    var targets=football?new[]{new Vector3(0,16,0),new Vector3(0,8,0),new Vector3(1,-.4f,-146),new Vector3(107,1,-75),new Vector3(65,2,-40),new Vector3(68,1,-100)}:
     new[]{new Vector3(0,9,0),new Vector3(0,5,0),new Vector3(1,-.3f,-78),new Vector3(57,.5f,-31),new Vector3(33,2,-13),new Vector3(37,1,-51)};
    string[] names={"01_Island","02_Reverse","03_Dock_Close","04_Shore_Close","05_Promenade","06_Garden_Bridge"};
    for(int i=0;i<names.Length;i++){
     camera.transform.position=positions[i];camera.transform.LookAt(targets[i]);camera.fieldOfView=i==0?42:i==1?35:62;
     yield return new WaitForSeconds(.8f);yield return new WaitForEndOfFrame();
     Capture(camera,Path.Combine(folder,sport+"_"+names[i]+".png"));
    }
    // Document unchanged player views as well as the off-limits exterior.
    app.LocalAthlete.gameObject.SetActive(true);app.LocalAthlete.capsule.enabled=true;app.view.enabled=true;app.view.mode=0;app.view.pitch=8;
    yield return new WaitForSeconds(.7f);yield return new WaitForEndOfFrame();Capture(camera,Path.Combine(folder,sport+"_07_Existing_Player_View.png"));
    Check(!coast.exteriorContactShading.isActive,sport+" exterior contact shading disabled inside original venue");
   }
   foreach(var sport in new[]{SportId.Golf,SportId.Fishing,SportId.Football,SportId.Basketball}){
    app.SelectSport(sport);Check(app.environments.roots.Count(r=>r.activeSelf)==1,"Switch "+sport);
    Check(camera.clearFlags==((sport==SportId.Football||sport==SportId.Basketball)?CameraClearFlags.Skybox:CameraClearFlags.SolidColor),"Sky scope "+sport);
   }
   File.AppendAllText(Path.Combine(folder,"review.txt"),"COASTAL_REVIEW_COMPLETE\n");Application.Quit(failed?1:0);
  }
  void Capture(Camera camera,string path){
   var target=new RenderTexture(1920,1080,24){antiAliasing=4};var old=RenderTexture.active;var prior=camera.targetTexture;
   camera.targetTexture=target;camera.Render();RenderTexture.active=target;
   var image=new Texture2D(1920,1080,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,1920,1080),0,0);image.Apply();
   File.WriteAllBytes(path,image.EncodeToPNG());camera.targetTexture=prior;RenderTexture.active=old;Destroy(target);Destroy(image);
  }
 }
}
#endif
