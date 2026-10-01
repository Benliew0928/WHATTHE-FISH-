#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;

namespace WhatTheFish {
 public sealed class FishingRodReview:MonoBehaviour {
  AppRoot app;Camera camera;Transform equipment;string folder;bool preview,walking,failed;int view;
  readonly string[] labels={"Both rods","Blue reel","Orange reel","Reverse","Guide rings","Distance LOD","Pier context"};
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Init(){var a=Environment.GetCommandLineArgs();int i=Array.IndexOf(a,"-fishingRodReview");bool p=a.Contains("-fishingRodPreview");if(i<0&&!p)return;var r=new GameObject("Fishing rod review").AddComponent<FishingRodReview>();r.preview=p;r.folder=i>=0&&i+1<a.Length?a[i+1]:null;}
  void Check(bool ok,string text){failed|=!ok;if(folder!=null)File.AppendAllText(Path.Combine(folder,"results.txt"),(ok?"PASS ":"FAIL ")+text+"\n");Debug.Log("FISHING_RODS "+(ok?"PASS ":"FAIL ")+text);}
  IEnumerator Start(){
   if(folder!=null){Directory.CreateDirectory(folder);File.WriteAllText(Path.Combine(folder,"results.txt"),"");}
   float deadline=Time.realtimeSinceStartup+90;while((!(app=AppRoot.Instance)||!app.Exploring||app.SelectedSport!=SportId.Fishing)&&Time.realtimeSinceStartup<deadline)yield return null;
   if(!app||!app.Exploring||app.SelectedSport!=SportId.Fishing){Check(false,"fishing exploration initialized");Finish();yield break;}
   equipment=app.stadium.transform.Find("Fishing Rods");if(!equipment){Check(false,"rods present in streamed fishing scene");Finish();yield break;}
   Check(app.environments.roots.Count(r=>r&&r.activeSelf)==1,"one active island");
   Check(((FishingLagoonView)app.stadium).playerStands.Length==5,"five independent player stands retained");
   Physics.SyncTransforms();
   foreach(var name in new[]{"BlueLime","TealOrange"}){
    var rod=equipment.Find(name);Check(rod,"one "+name+" rod");if(!rod)continue;
    var lod=rod.GetComponent<LODGroup>();Check(lod&&lod.GetLODs().Length==2&&lod.GetLODs().All(l=>l.renderers.Length==1),name+" LODs assigned");
    var renderers=rod.GetComponentsInChildren<MeshRenderer>(true);Check(renderers.All(r=>r.sharedMaterial&&r.sharedMaterial.shader&&r.sharedMaterial.shader.isSupported),name+" supported material");
    Check(renderers.All(r=>r.sharedMaterial.GetTexture("_BaseMap")&&r.sharedMaterial.GetTexture("_BumpMap")),name+" colour and normal maps");
    Check(rod.Find("Grip")&&rod.Find("Tip"),name+" reusable attachment sockets");
    Check(rod.GetComponentsInChildren<Collider>().Length==0,name+" thin visual geometry does not obstruct players");
    var bounds=BoundsOf(name);Check(bounds.size.y>1.7f&&bounds.size.y<1.85f,name+" 1.8 metre rod scale");
    var point=rod.position;var hits=Physics.RaycastAll(point+Vector3.up*.2f,Vector3.down,.4f,1<<8);Check(hits.Any(h=>h.collider.name=="Lower rest"),name+" supported by rack");
   }
   // Exercise the same centre lane that players use to reach the front of the pier.
   var athlete=app.LocalAthlete;var stand=((FishingLagoonView)app.stadium).playerStands.Single(s=>s.slotId=="Stand_01").transform;
   athlete.capsule.enabled=false;athlete.transform.position=stand.TransformPoint(new Vector3(0,.15f,.5f));athlete.capsule.enabled=true;athlete.ResetLocomotion();
   var target=stand.TransformPoint(new Vector3(0,.15f,5.4f));
   for(int i=0;i<180;i++){var delta=target-athlete.transform.position;delta.y=0;if(delta.magnitude<.25f)break;athlete.Simulate(new PlayerCommand{move=Vector2.up,heading=Mathf.Atan2(delta.x,delta.z)*Mathf.Rad2Deg,sprint=true},.02f);}
   Check(Vector2.Distance(new Vector2(athlete.transform.position.x,athlete.transform.position.z),new Vector2(target.x,target.z))<.3f,"pier centre lane remains walkable");
   app.enabled=false;app.view.enabled=false;camera=Camera.main;camera.nearClipPlane=.004f;
   foreach(var canvas in FindObjectsByType<Canvas>(FindObjectsSortMode.None))if(canvas.renderMode!=RenderMode.WorldSpace)canvas.enabled=false;
   athlete.gameObject.SetActive(false);yield return new WaitForSeconds(.5f);
   if(folder!=null){
    for(view=0;view<labels.Length;view++){SetView();yield return new WaitForSeconds(.3f);yield return new WaitForEndOfFrame();Capture(view.ToString("D2")+"-"+labels[view].Replace(' ','-'));}
    app.enabled=true;app.SelectSport(SportId.Football);var streaming=app.environments.GetComponent<SkySailStreaming>();yield return new WaitForSeconds(.5f);while(streaming.Busy)yield return null;
    Check(!FindObjectsByType<Transform>(FindObjectsSortMode.None).Any(t=>t.name=="Fishing Rods"),"rods release with their island");
    yield return streaming.Prepare(SportId.Fishing);app.SelectSport(SportId.Fishing);yield return new WaitForSeconds(.3f);
    Check(app.stadium.transform.Find("Fishing Rods"),"rods return with island");Check(FindObjectsByType<Transform>(FindObjectsSortMode.None).Count(t=>t.name=="Fishing Rods")==1,"no duplicate rod rack after reload");Finish();yield break;
   }
   view=0;SetView();
  }
  Bounds BoundsOf(string name){return equipment.Find(name).GetComponentsInChildren<Renderer>(true).Single(r=>r.name.EndsWith("_LOD0")).bounds;}
  void SetView(){
   if(!equipment)return;walking=false;app.enabled=false;app.view.enabled=false;app.LocalAthlete.gameObject.SetActive(false);camera.nearClipPlane=.004f;
   foreach(var lod in equipment.GetComponentsInChildren<LODGroup>())lod.ForceLOD(view==5?1:-1);
   var target=equipment.TransformPoint(new Vector3(0,.95f,0));var offset=new Vector3(1.7f,.65f,-3.7f);camera.fieldOfView=36;
   switch(view){
    case 1:target=equipment.Find("BlueLime").TransformPoint(new Vector3(0,.52f,.08f));offset=new Vector3(.3f,.18f,-1.15f);camera.fieldOfView=40;break;
    case 2:target=equipment.Find("TealOrange").TransformPoint(new Vector3(0,.53f,.08f));offset=new Vector3(.3f,.18f,-1.15f);camera.fieldOfView=40;break;
    case 3:offset=new Vector3(-1.7f,.65f,3.7f);break;
    case 4:target=equipment.TransformPoint(new Vector3(0,1.38f,.1f));offset=new Vector3(.7f,.35f,-1.3f);camera.fieldOfView=48;break;
    case 5:offset=new Vector3(2,.8f,-5);camera.fieldOfView=48;break;
    case 6:offset=new Vector3(5,3,-6);camera.fieldOfView=55;break;
   }
   camera.transform.position=target+equipment.TransformDirection(offset);camera.transform.LookAt(target);
  }
  void OnGUI(){if(!preview||!equipment)return;GUI.Box(new Rect(18,18,690,88),"Fishing rods - actual game scene");for(int i=0;i<labels.Length;i++)if(GUI.Button(new Rect(28+i*94,48,92,26),(i+1)+" "+labels[i].Split(' ')[0])){view=i;SetView();}
   if(GUI.Button(new Rect(28,80,165,23),walking?"Return to close views":"Walk around the pier")){
    if(walking)SetView();else{walking=true;foreach(var lod in equipment.GetComponentsInChildren<LODGroup>())lod.ForceLOD(-1);var athlete=app.LocalAthlete;athlete.gameObject.SetActive(true);athlete.capsule.enabled=false;athlete.transform.position=equipment.TransformPoint(new Vector3(1.7f,.18f,-1.5f));athlete.capsule.enabled=true;athlete.ResetLocomotion();app.view.mode=0;app.view.yaw=0;app.view.pitch=18;app.view.enabled=true;app.enabled=true;camera.fieldOfView=65;}
   }
  }
  void Capture(string name){var rt=RenderTexture.GetTemporary(1600,1100,24);var previous=camera.targetTexture;var active=RenderTexture.active;camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;var pixels=new Texture2D(1600,1100,TextureFormat.RGB24,false);pixels.ReadPixels(new Rect(0,0,1600,1100),0,0);pixels.Apply();File.WriteAllBytes(Path.Combine(folder,name+".png"),pixels.EncodeToPNG());Destroy(pixels);camera.targetTexture=previous;RenderTexture.active=active;RenderTexture.ReleaseTemporary(rt);}
  void Finish(){if(folder!=null)File.AppendAllText(Path.Combine(folder,"results.txt"),"FISHING_RODS_REVIEW_COMPLETE success="+(!failed)+"\n");if(!preview)Application.Quit(failed?1:0);}
 }
}
#endif
