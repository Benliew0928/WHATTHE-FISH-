#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace WhatTheFish {
 public sealed class CharacterLightingReview:MonoBehaviour {
  [Serializable] sealed class Settings {public float Intensity,Radius;}
  [Serializable] sealed class FeatureState {public Settings m_Settings;}
  [Serializable] sealed class Sample {public string island,view;public bool active;public float intensity,radius,meanLuminance;}
  [Serializable] sealed class Report {public bool success,packaged;public Sample[] captures;}
  readonly List<Sample> samples=new();
  string folder;bool failed;Camera camera;AppRoot app;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Init(){
   var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,"-characterLightingReview");
   if(i>=0&&i+1<args.Length)new GameObject("Character lighting review").AddComponent<CharacterLightingReview>().folder=args[i+1];
  }
  void Check(bool pass,string label){failed|=!pass;File.AppendAllText(Path.Combine(folder,"results.txt"),(pass?"PASS ":"FAIL ")+label+"\n");}
  static Settings Read(ScreenSpaceAmbientOcclusion feature)=>JsonUtility.FromJson<FeatureState>(JsonUtility.ToJson(feature)).m_Settings;
  ScreenSpaceAmbientOcclusion ActiveFeature(){
   var pipeline=(UniversalRenderPipelineAsset)GraphicsSettings.currentRenderPipeline;
   var renderer=camera.GetComponent<UniversalAdditionalCameraData>().scriptableRenderer;
   for(int i=0;i<pipeline.rendererDataList.Length;i++){
    var data=pipeline.rendererDataList[i];
    if(data&&pipeline.GetRenderer(i)==renderer)return data.rendererFeatures.OfType<ScreenSpaceAmbientOcclusion>().SingleOrDefault();
   }
   return null;
  }
  IEnumerator Start(){
   Directory.CreateDirectory(folder);File.WriteAllText(Path.Combine(folder,"results.txt"),"");
   double deadline=Time.realtimeSinceStartupAsDouble+120;
   while((!AppRoot.Instance||!AppRoot.Instance.Menu||!AppRoot.Instance.LocalAthlete||!PackagedContentBoot.Ready)&&Time.realtimeSinceStartupAsDouble<deadline)yield return null;
   app=AppRoot.Instance;Check(app&&app.LocalAthlete&&PackagedContentBoot.Ready,"Packaged world ready");
   if(failed){Application.Quit(1);yield break;}
   app.EnterOffline();app.enabled=false;app.view.enabled=false;SkySailWorld.Instance.reviewCamera=true;camera=Camera.main;
   var streaming=app.environments.GetComponent<SkySailStreaming>();
   var athlete=app.LocalAthlete;athlete.HideHead(false);
   var animator=athlete.GetComponentInChildren<Animator>();
   foreach(var canvas in FindObjectsByType<Canvas>(FindObjectsSortMode.None))if(canvas.renderMode!=RenderMode.WorldSpace)canvas.enabled=false;
   Check(IslandContactShading.Resolve(null)==null,"Missing camera is safe");
   for(int cycle=0;cycle<2;cycle++)foreach(var sport in SkySailMap.Circuit){
    while(streaming.Busy)yield return null;
    yield return streaming.Prepare(sport);app.SelectSport(sport);yield return null;while(streaming.Busy)yield return null;
    Check(streaming.Error==null&&app.SelectedSport==sport,sport+" loaded cycle "+cycle);
    var root=app.environments.roots[(int)sport];
    var coast=root.GetComponentInChildren<CoastalEnvironment>();var refined=root.GetComponent<RefinedIslandEnvironment>();
    var effect=ActiveFeature();Check(effect,sport+" active camera has contact shading");
    if(!effect){Application.Quit(1);yield break;}
    Check((coast?coast.exteriorContactShading:refined.contactShading)==effect,sport+" environment controls actual renderer feature cycle "+cycle);
    camera.transform.position=athlete.transform.position+new Vector3(-2.4f,1.8f,-3.2f);
    camera.transform.LookAt(athlete.transform.position+Vector3.up*.95f);camera.fieldOfView=38;
    yield return null;
    if(coast){
     camera.transform.position=new Vector3(0,3,0);yield return null;
     Check(!effect.isActive,sport+" contact shading off inside playing area");
     camera.transform.position=new Vector3(coast.basketball?40:80,3,0);yield return null;
     Check(effect.isActive,sport+" exterior contact shading on");
     camera.transform.position=new Vector3(0,3,0);yield return null;
     Check(!effect.isActive,sport+" contact shading off on return");
     var restored=Read(effect);Check(Mathf.Approximately(restored.Intensity,1.05f)&&Mathf.Approximately(restored.Radius,.55f),sport+" exterior defaults restored after island switch");
    }else{
     var settings=Read(effect);
     Check(effect.isActive==refined.lighting.contactShading,sport+" authored contact shading activation");
     Check(Mathf.Approximately(settings.Intensity,refined.lighting.contactIntensity)&&Mathf.Approximately(settings.Radius,refined.lighting.contactRadius),sport+" authored intensity and radius on actual renderer");
    }
    if(cycle!=0)continue;
    athlete.HideHead(false);athlete.enabled=false;if(animator){animator.Update(0);animator.enabled=false;}
    var renderers=athlete.GetComponentsInChildren<SkinnedMeshRenderer>();
    Check(renderers.Any(r=>r.enabled&&r.sharedMesh)&&renderers.All(r=>r.sharedMaterials.All(m=>m&&m.shader&&m.shader.isSupported)),sport+" character mesh and materials supported");
    var material=renderers.First(r=>r.sharedMaterial).sharedMaterial;
    var textureNames=new[]{"_BaseMap","_BumpMap","_MetallicGlossMap"};var textures=textureNames.Select(material.GetTexture).ToArray();
    var valueNames=new[]{"_BumpScale","_Metallic","_Smoothness"};var values=valueNames.Select(material.GetFloat).ToArray();var tint=material.GetColor("_BaseColor");
    Vector3 ground=athlete.transform.position;
    if(coast){athlete.capsule.enabled=false;athlete.transform.position=new Vector3(0,ground.y,0);athlete.capsule.enabled=true;ground=athlete.transform.position;}
    athlete.transform.rotation=Quaternion.identity;Physics.SyncTransforms();
    foreach(string view in new[]{"front","back"}){
     camera.transform.position=ground+new Vector3(view=="front"?-2.4f:2.4f,1.8f,view=="front"?3.2f:-3.2f);
     camera.transform.LookAt(ground+Vector3.up*.95f);camera.fieldOfView=38;yield return null;
     Capture(sport,view+"-fixed",effect);
     // Reproduce the former stuck renderer settings without altering shipped assets.
     string saved=JsonUtility.ToJson(effect);bool active=effect.isActive;
     JsonUtility.FromJsonOverwrite("{\"m_Settings\":{\"Intensity\":1.05,\"Radius\":0.55}}",effect);effect.SetActive(true);
     Capture(sport,view+"-old-settings",effect);
     JsonUtility.FromJsonOverwrite(saved,effect);effect.SetActive(active);
    }
    Check(textureNames.Select(material.GetTexture).SequenceEqual(textures)&&valueNames.Select(material.GetFloat).SequenceEqual(values)&&material.GetColor("_BaseColor")==tint,sport+" lighting comparison leaves character material unchanged");
    camera.transform.position=app.environments.Current.menuCamera;camera.transform.LookAt(app.environments.Current.menuFocus);camera.fieldOfView=60;yield return null;Capture(sport,"island-fixed",effect);
    if(animator)animator.enabled=true;athlete.enabled=true;
    var start=athlete.transform.position;
    for(int step=0;step<30;step++)athlete.Simulate(new PlayerCommand{move=Vector2.up,heading=90},1f/60);
    Check(Vector3.Distance(start,athlete.transform.position)>.15f,sport+" character movement retained");
   }
   File.WriteAllText(Path.Combine(folder,"report.json"),JsonUtility.ToJson(new Report{success=!failed,packaged=PackagedContentBoot.Ready,captures=samples.ToArray()},true));
   File.AppendAllText(Path.Combine(folder,"results.txt"),"CHARACTER_LIGHTING_REVIEW_COMPLETE success="+!failed+"\n");Application.Quit(failed?1:0);
  }
  void Capture(SportId sport,string view,ScreenSpaceAmbientOcclusion effect){
   const int width=1280,height=720;var target=RenderTexture.GetTemporary(width,height,24);var previous=RenderTexture.active;var original=camera.targetTexture;
   var pixels=new Texture2D(width,height,TextureFormat.RGB24,false);
   try{
    camera.targetTexture=target;foreach(var instances in FindObjectsByType<MobileVegetationInstances>(FindObjectsSortMode.None))instances.RenderForCamera(camera);
    camera.Render();RenderTexture.active=target;pixels.ReadPixels(new Rect(0,0,width,height),0,0);pixels.Apply();
    File.WriteAllBytes(Path.Combine(folder,sport+"-"+view+".png"),pixels.EncodeToPNG());
    var colors=pixels.GetPixels32();Check(colors.Where((_,i)=>i%100==0).Select(c=>(c.r/16,c.g/16,c.b/16)).Distinct().Count()>20,sport+" "+view+" nonblank");
    Check(colors.Count(c=>c.r>230&&c.b>230&&c.g<30)<colors.Length/100,sport+" "+view+" no missing-shader magenta");
    var settings=Read(effect);samples.Add(new Sample{island=sport.ToString(),view=view,active=effect.isActive,intensity=settings.Intensity,radius=settings.Radius,meanLuminance=(float)colors.Average(c=>(.2126*c.r+.7152*c.g+.0722*c.b)/255)});
   }finally{camera.targetTexture=original;RenderTexture.active=previous;RenderTexture.ReleaseTemporary(target);Destroy(pixels);}
  }
 }
}
#endif
