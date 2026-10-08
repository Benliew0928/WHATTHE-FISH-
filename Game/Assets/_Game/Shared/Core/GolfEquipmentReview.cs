#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;

namespace WhatTheFish {
 // Opt-in review camera; ordinary gameplay and the Android release use the scene prefabs.
 public sealed class GolfEquipmentReview:MonoBehaviour {
  AppRoot app;Camera camera;Transform equipment;string folder;bool preview,walking,failed;int view;
  readonly string[] labels={"All equipment","Ball dimples","Driver face","Driver back","Iron face","Putter face","Island context"};
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Init(){var a=Environment.GetCommandLineArgs();int i=Array.IndexOf(a,"-golfEquipmentReview");bool preview=a.Contains("-golfEquipmentPreview");if(i<0&&!preview)return;var r=new GameObject("Golf equipment review").AddComponent<GolfEquipmentReview>();r.preview=preview;r.folder=i>=0&&i+1<a.Length?a[i+1]:null;}
  void Check(bool ok,string text){failed|=!ok;if(folder!=null)File.AppendAllText(Path.Combine(folder,"results.txt"),(ok?"PASS ":"FAIL ")+text+"\n");Debug.Log("GOLF_EQUIPMENT "+(ok?"PASS ":"FAIL ")+text);}
  IEnumerator Start(){
   if(folder!=null){Directory.CreateDirectory(folder);File.WriteAllText(Path.Combine(folder,"results.txt"),"");}
   float deadline=Time.realtimeSinceStartup+90;while((!(app=AppRoot.Instance)||!app.Exploring||app.SelectedSport!=SportId.Golf)&&Time.realtimeSinceStartup<deadline)yield return null;
   if(!app||!app.Exploring||app.SelectedSport!=SportId.Golf){Check(false,"golf exploration initialized");Finish();yield break;}
   equipment=app.stadium.transform.Find("Golf Equipment");if(!equipment){Check(false,"equipment present in streamed golf scene");Finish();yield break;}
   Physics.SyncTransforms();Check(app.environments.roots.Count(r=>r&&r.activeSelf)==1,"one active island");
   foreach(var name in new[]{"Ball","Driver","Iron","Putter"}){
    var t=equipment.Find(name);Check(t,"one "+name+" in practice set");if(!t)continue;
    var lod=t.GetComponent<LODGroup>();Check(lod&&lod.GetLODs().Length==2,name+" has two delivery LODs");
    Check(t.GetComponentsInChildren<Renderer>(true).All(r=>r.sharedMaterial&&r.sharedMaterial.shader&&r.sharedMaterial.shader.isSupported),name+" has supported material");
    var bounds=BoundsOf(name);var ray=new Ray(new Vector3(bounds.center.x,bounds.min.y+1,bounds.center.z),Vector3.down);
    var hits=Physics.RaycastAll(ray,3,1<<8).Where(h=>h.collider.name.StartsWith("Terrain__")).OrderBy(h=>h.distance).ToArray();Check(hits.Length>0&&Mathf.Abs(bounds.min.y-hits[0].point.y)<.065f,name+" supported by practice terrain");
   }
   var ball=equipment.Find("Ball");Check(ball&&ball.GetComponent<SphereCollider>()&&Mathf.Abs(ball.GetComponent<SphereCollider>().radius-GolfBall.Radius)<.00001f,"ball collider matches current gameplay radius");
   app.enabled=false;app.view.enabled=false;camera=Camera.main;camera.nearClipPlane=.004f;
   foreach(var canvas in FindObjectsByType<Canvas>(FindObjectsSortMode.None))if(canvas.renderMode!=RenderMode.WorldSpace)canvas.enabled=false;
   app.LocalAthlete.gameObject.SetActive(false);yield return new WaitForSeconds(.5f);
   if(folder!=null){
    for(view=0;view<labels.Length;view++){SetView();yield return new WaitForSeconds(.3f);yield return new WaitForEndOfFrame();Capture(view.ToString("D2")+"-"+labels[view].Replace(' ','-'));}
    // All equipment must release with its island, and return once on reload.
    app.enabled=true;app.SelectSport(SportId.Football);var streaming=app.environments.GetComponent<SkySailStreaming>();yield return new WaitForSeconds(.5f);while(streaming.Busy)yield return null;
    Check(!FindObjectsByType<Transform>(FindObjectsSortMode.None).Any(t=>t.name=="Golf Equipment"),"golf equipment releases with its streamed island");
    yield return streaming.Prepare(SportId.Golf);app.SelectSport(SportId.Golf);yield return new WaitForSeconds(.3f);Check(app.stadium.transform.Find("Golf Equipment"),"golf equipment returns with island");Finish();yield break;
   }
   view=0;SetView();
  }
  Bounds BoundsOf(string name){var renderers=equipment.Find(name).GetComponentsInChildren<Renderer>(true).Where(r=>r.name.EndsWith("_LOD0")).ToArray();var b=renderers[0].bounds;foreach(var r in renderers.Skip(1))b.Encapsulate(r.bounds);return b;}
  void SetView(){
   if(!equipment)return;walking=false;app.enabled=false;app.view.enabled=false;app.LocalAthlete.gameObject.SetActive(false);camera.nearClipPlane=.003f;
   var driver=BoundsOf("Driver");var centre=driver.center+Vector3.right*.34f;Vector3 target=centre,offset=new Vector3(.7f,.28f,2.4f);camera.fieldOfView=37;
   switch(view){
    case 1:target=BoundsOf("Ball").center;offset=new Vector3(.045f,.025f,.075f);camera.fieldOfView=42;break;
    case 2:target=driver.center-Vector3.up*(driver.size.y*.43f);offset=new Vector3(.12f,.075f,.35f);camera.fieldOfView=40;break;
    case 3:target=driver.center-Vector3.up*(driver.size.y*.43f);offset=new Vector3(-.15f,.075f,-.36f);camera.fieldOfView=40;break;
    case 4:var iron=BoundsOf("Iron");target=iron.center-Vector3.up*(iron.size.y*.43f);offset=new Vector3(.13f,.05f,.30f);camera.fieldOfView=40;break;
    case 5:var putter=BoundsOf("Putter");target=putter.center-Vector3.up*(putter.size.y*.44f);offset=new Vector3(.09f,.055f,.28f);camera.fieldOfView=40;break;
    case 6:offset=new Vector3(4,2.5f,6);camera.fieldOfView=55;break;
   }
   camera.transform.position=target+offset;camera.transform.LookAt(target);
  }
  void OnGUI(){if(!preview||!equipment)return;GUI.Box(new Rect(18,18,610,88),"Golf equipment · actual game scene");for(int i=0;i<labels.Length;i++)if(GUI.Button(new Rect(28+i*84,48,82,26),(i+1)+" "+labels[i].Split(' ')[0])){view=i;SetView();}if(GUI.Button(new Rect(28,80,165,23),walking?"Return to close views":"Walk around the green")){if(walking){SetView();}else{walking=true;app.LocalAthlete.gameObject.SetActive(true);app.LocalAthlete.capsule.enabled=false;var ball=BoundsOf("Ball").center;var p=ball+new Vector3(0,1.5f,2.5f);if(Physics.Raycast(p+Vector3.up*5,Vector3.down,out var hit,15,1<<8))p=hit.point+Vector3.up*.12f;app.LocalAthlete.transform.position=p;app.LocalAthlete.capsule.enabled=true;app.LocalAthlete.ResetLocomotion();app.view.mode=0;app.view.yaw=180;app.view.pitch=25;app.view.enabled=true;app.enabled=true;camera.fieldOfView=65;}}}
  void Capture(string name){var rt=RenderTexture.GetTemporary(1600,1100,24);var previous=camera.targetTexture;var active=RenderTexture.active;camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;var pixels=new Texture2D(1600,1100,TextureFormat.RGB24,false);pixels.ReadPixels(new Rect(0,0,1600,1100),0,0);pixels.Apply();File.WriteAllBytes(Path.Combine(folder,name+".png"),pixels.EncodeToPNG());Destroy(pixels);camera.targetTexture=previous;RenderTexture.active=active;RenderTexture.ReleaseTemporary(rt);}
  void Finish(){if(folder!=null)File.AppendAllText(Path.Combine(folder,"results.txt"),"GOLF_EQUIPMENT_REVIEW_COMPLETE success="+(!failed)+"\n");if(!preview)Application.Quit(failed?1:0);}
 }
}
#endif
