#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;

namespace WhatTheFish {
 public sealed class LagoonPresentationReview:MonoBehaviour {
  string folder;bool failed;Camera camera;AppRoot app;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Init(){
   var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,"-lagoonReview");
   if(i<0||i+1>=args.Length)return;
   new GameObject("Lagoon art review").AddComponent<LagoonPresentationReview>().folder=args[i+1];
  }
  void Check(bool ok,string text){failed|=!ok;File.AppendAllText(Path.Combine(folder,"results.txt"),(ok?"PASS ":"FAIL ")+text+"\n");}
  Color32[] Capture(string name,Vector3 position,Vector3 target){
   camera.transform.position=position;camera.transform.LookAt(target);
   var rt=RenderTexture.GetTemporary(1600,900,24);var previous=RenderTexture.active;
   camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;
   var pixels=new Texture2D(1600,900,TextureFormat.RGB24,false);pixels.ReadPixels(new Rect(0,0,1600,900),0,0);pixels.Apply();
   File.WriteAllBytes(Path.Combine(folder,name+".png"),pixels.EncodeToPNG());var data=pixels.GetPixels32();Destroy(pixels);
   camera.targetTexture=null;RenderTexture.active=previous;RenderTexture.ReleaseTemporary(rt);return data;
  }
  static int Changed(Color32[] a,Color32[] b){int n=0;for(int i=0;i<a.Length;i++)if(Mathf.Abs(a[i].r-b[i].r)+Mathf.Abs(a[i].g-b[i].g)+Mathf.Abs(a[i].b-b[i].b)>12)n++;return n;}
  IEnumerator Start(){
   Directory.CreateDirectory(folder);File.WriteAllText(Path.Combine(folder,"results.txt"),"Lagoon water and one fish in three sizes\n");
   float deadline=Time.realtimeSinceStartup+90;
   while((!(app=AppRoot.Instance)||!app.Exploring||app.SelectedSport!=SportId.Fishing)&&Time.realtimeSinceStartup<deadline)yield return null;
   if(!app||!app.Exploring||app.SelectedSport!=SportId.Fishing){Check(false,"fishing exploration loaded");Finish();yield break;}
   var lagoon=app.stadium.GetComponentInChildren<LagoonPresentation>();Check(lagoon,"lagoon presentation loads with fishing island");
   if(!lagoon){Finish();yield break;}
   Check(((FishingLagoonView)app.stadium).playerStands.Length==5,"five player stands retained");
   Check(app.environments.roots.Count(r=>r&&r.activeSelf)==1,"one active sport island");
   var water=lagoon.transform.Find("Transparent moving lagoon").GetComponent<MeshRenderer>();
   Check(water.sharedMaterial.shader.isSupported&&water.sharedMaterial.renderQueue==2980,"transparent water shader supported and ordered before other transparent props");
   Check(lagoon.GetComponentsInChildren<Collider>().Length==0,"presentation adds no player-blocking colliders");
   Check(Shader.GetGlobalVector("_FishingLagoonCutout").w==1,"opaque ocean opening enabled with local lagoon");
   var fish=lagoon.transform.Find("Fish size comparison");Mesh first=null;Material fishMaterial=null;
   var lengths=new[]{.4f,.9f,1.8f};
   for(int i=0;i<3;i++){
    var instance=fish.GetChild(i);var group=instance.GetComponent<LODGroup>();
    Check(group&&group.GetLODs().Length==2,instance.name+" has two LODs");
    var r=group.GetLODs()[0].renderers[0];var mesh=r.GetComponent<MeshFilter>().sharedMesh;
    var rotation=instance.localRotation;instance.localRotation=Quaternion.identity;float actual=r.bounds.size.z;instance.localRotation=rotation;
    Check(Mathf.Abs(actual-lengths[i])<.012f,instance.name+" length "+actual.ToString("F3")+" metres");
    Check(mesh.HasVertexAttribute(UnityEngine.Rendering.VertexAttribute.Color),instance.name+" has authored vertex colour");
    Check(r.sharedMaterial.shader.isSupported,instance.name+" shader supported");
    if(i==0){first=mesh;fishMaterial=r.sharedMaterial;}else{
     Check(mesh==first,instance.name+" shares original fish mesh");Check(r.sharedMaterial==fishMaterial,instance.name+" shares material");
    }
   }
   app.enabled=false;app.view.enabled=false;app.LocalAthlete.gameObject.SetActive(false);
   foreach(var canvas in FindObjectsByType<Canvas>(FindObjectsSortMode.None))if(canvas.renderMode!=RenderMode.WorldSpace)canvas.enabled=false;
   camera=new GameObject("Lagoon capture camera").AddComponent<Camera>();camera.CopyFrom(Camera.main);camera.enabled=false;camera.fieldOfView=65;
   var actor=app.LocalAthlete;actor.gameObject.SetActive(true);actor.capsule.enabled=false;
   actor.transform.position=((FishingLagoonView)app.stadium).standingPositions[0];actor.transform.rotation=Quaternion.identity;
   actor.capsule.enabled=true;actor.ResetLocomotion();app.view.target=actor;app.view.active=true;app.view.enabled=true;app.view.yaw=0;app.view.pitch=32;
   for(int mode=0;mode<3;mode++){
    app.view.mode=mode;yield return new WaitForEndOfFrame();var gameplay=Camera.main;
    camera.fieldOfView=gameplay.fieldOfView;camera.nearClipPlane=gameplay.nearClipPlane;
    Check(Vector3.Distance(gameplay.transform.position,actor.transform.position)<25,"game camera "+mode+" follows the player at the pier");
    Capture("10-game-camera-"+mode,gameplay.transform.position,gameplay.transform.position+gameplay.transform.forward*10);
   }
   app.view.enabled=false;actor.gameObject.SetActive(false);camera.fieldOfView=65;
   yield return new WaitForSeconds(.5f);
   Capture("01-player-pier",new Vector3(0,2.95f,-22.5f),new Vector3(4,-.55f,-15));
   camera.fieldOfView=48;
   Capture("02-fish-close-underwater",new Vector3(6,1.7f,-18.2f),new Vector3(4,-1,-15.5f));
   camera.fieldOfView=65;
   Capture("03-lagoon-overview",new Vector3(28,23,-38),new Vector3(0,-.5f,0));
   Capture("04-shore-and-inlet",new Vector3(13,4,23),new Vector3(0,-.3f,32));
   var eye=new Vector3(0,3.5f,-22.5f);var focus=new Vector3(4,-1,-15.5f);camera.fieldOfView=48;
   var visible=Capture("05-fish-through-water",eye,focus);
   fish.gameObject.SetActive(false);var hidden=Capture("06-water-without-fish",eye,focus);
   Check(Changed(visible,hidden)>100,"fish change visible pixels through the transparent surface");fish.gameObject.SetActive(true);
   // Hold all opaque content fixed so this measures the moving surface alone.
   fishMaterial.SetFloat("_TailMotion",0);
   var before=Capture("07-water-motion-a",new Vector3(3,2.6f,-22),new Vector3(0,-.5f,-13));
   yield return new WaitForSeconds(.8f);
   var after=Capture("08-water-motion-b",new Vector3(3,2.6f,-22),new Vector3(0,-.5f,-13));
   Check(Changed(before,after)>2000,"water and caustics continuously animate at fixed camera");fishMaterial.SetFloat("_TailMotion",1);
   Directory.CreateDirectory(Path.Combine(folder,"Motion"));Time.captureFramerate=30;
   for(int frame=0;frame<90;frame++){
    yield return new WaitForEndOfFrame();Capture("Motion/frame-"+frame.ToString("D3"),new Vector3(0,2.95f,-22.5f),new Vector3(4,-.55f,-15));
   }
   Time.captureFramerate=0;
   // Asset inspection in a neutral scene, at exactly the authored relative sizes.
   int fishLayer=27;var stage=new GameObject("Fish comparison stage");
   for(int i=0;i<3;i++){
    var copy=Instantiate(fish.GetChild(i).gameObject,stage.transform);copy.SetActive(true);copy.transform.position=new Vector3((i-1)*1.7f,10,0);copy.transform.rotation=Quaternion.Euler(0,75,0);
    foreach(var t in copy.GetComponentsInChildren<Transform>(true))t.gameObject.layer=fishLayer;
    copy.GetComponent<LODGroup>().ForceLOD(0);
   }
   int mask=camera.cullingMask;var flags=camera.clearFlags;var background=camera.backgroundColor;
   camera.cullingMask=1<<fishLayer;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.035f,.075f,.095f);camera.fieldOfView=42;
   Capture("09-one-model-three-sizes",new Vector3(0,11.6f,-7),new Vector3(0,10,0));
   camera.cullingMask=mask;camera.clearFlags=flags;camera.backgroundColor=background;Destroy(stage);
   app.LocalAthlete.gameObject.SetActive(true);app.enabled=true;app.SelectSport(SportId.Football);var streaming=app.environments.GetComponent<SkySailStreaming>();
   yield return new WaitForSeconds(.5f);while(streaming.Busy)yield return null;
   Check(Shader.GetGlobalVector("_FishingLagoonCutout").w==0,"ocean becomes solid again when lagoon deactivates");
   yield return streaming.Prepare(SportId.Fishing);app.SelectSport(SportId.Fishing);yield return new WaitForSeconds(.5f);
   Check(FindObjectsByType<LagoonPresentation>(FindObjectsSortMode.None).Length==1,"one lagoon after island reload");
   Check(Shader.GetGlobalVector("_FishingLagoonCutout").w==1,"transparent lagoon returns after reload");Finish();
  }
  void Finish(){File.AppendAllText(Path.Combine(folder,"results.txt"),"LAGOON_REVIEW_COMPLETE success="+!failed+"\n");Application.Quit(failed?1:0);}
 }
}
#endif
