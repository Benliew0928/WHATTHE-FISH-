using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using WhatTheFish;

// Authoring preview only: renders the actual runtime geometry and shaders.
public static class GolfTornadoArtPreview {
 [MenuItem("WHATTHE FISH?/Golf/Render tornado art preview")]
 public static void Render(){
  EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
  var directory=Path.GetFullPath(Path.Combine(Application.dataPath,"../../Builds/GolfTornadoArt/Preview"));Directory.CreateDirectory(directory);
  var camera=new GameObject("Art preview camera").AddComponent<Camera>();camera.gameObject.AddComponent<UniversalAdditionalCameraData>();camera.orthographic=true;camera.orthographicSize=8;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.31f,.38f,.43f);camera.nearClipPlane=.1f;camera.farClipPlane=150;
  camera.transform.position=new Vector3(0,13,-32);camera.transform.LookAt(new Vector3(0,4,0));
  var ground=GameObject.CreatePrimitive(PrimitiveType.Plane);ground.transform.localScale=new Vector3(6,1,3);var material=new Material(Shader.Find("Universal Render Pipeline/Unlit"));material.SetColor("_BaseColor",new Color(.43f,.59f,.28f));ground.GetComponent<Renderer>().sharedMaterial=material;
  var root=new GameObject("Six growth stages");float[] ages={0,1.6f,3.2f,4.8f,6.4f,8};
  for(int i=0;i<ages.Length;i++){var anchor=new GameObject("Growth stage "+(i+1)).transform;anchor.SetParent(root.transform);anchor.position=new Vector3((i-2.5f)*6.7f,0,0);var fx=GolfTornadoFX.Create(anchor);fx.Tick(ages[i]);}
  Save(camera,Path.Combine(directory,"growth.png"),2400,900);
  Object.DestroyImmediate(root);root=new GameObject("Four interpolated silhouettes");
  for(int i=0;i<4;i++){var anchor=new GameObject("Silhouette "+(i+1)).transform;anchor.SetParent(root.transform);anchor.position=new Vector3((i-1.5f)*8,0,0);var fx=GolfTornadoFX.Create(anchor);fx.Tick(24+i*6);}
  camera.orthographicSize=7.5f;Save(camera,Path.Combine(directory,"shapes.png"),2200,900);
  Object.DestroyImmediate(root);root=new GameObject("Close funnel view");var close=GolfTornadoFX.Create(root.transform);close.Tick(12);camera.orthographicSize=5.6f;camera.transform.position=new Vector3(10,11,-19);camera.transform.LookAt(new Vector3(0,4.4f,0));Save(camera,Path.Combine(directory,"close.png"),1200,1200);
  Object.DestroyImmediate(root);Object.DestroyImmediate(material);Debug.Log("TORNADO_ART_PREVIEW_RENDERED");
 }
 [MenuItem("WHATTHE FISH?/Golf/Render tornado atmosphere preview")]
 public static void RenderAtmosphere(){
  EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
  var directory=Path.GetFullPath(Path.Combine(Application.dataPath,"../../Builds/GolfTornadoAtmosphere/Preview"));Directory.CreateDirectory(directory);
  var camera=new GameObject("Atmosphere camera").AddComponent<Camera>();var data=camera.gameObject.AddComponent<UniversalAdditionalCameraData>();camera.orthographic=true;camera.orthographicSize=7.3f;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.43f,.64f,.80f);camera.nearClipPlane=.1f;camera.farClipPlane=150;camera.transform.position=new Vector3(11,10,-20);camera.transform.LookAt(new Vector3(0,4.1f,0));
  var ground=GameObject.CreatePrimitive(PrimitiveType.Plane);ground.transform.localScale=new Vector3(5,1,5);var material=new Material(Shader.Find("Universal Render Pipeline/Unlit"));material.SetColor("_BaseColor",new Color(.35f,.55f,.23f));ground.GetComponent<Renderer>().sharedMaterial=material;
  var sand=GameObject.CreatePrimitive(PrimitiveType.Plane);sand.transform.position=new Vector3(-2,.015f,-1);sand.transform.localScale=new Vector3(.65f,1,.65f);var sandMaterial=new Material(material);sandMaterial.SetColor("_BaseColor",new Color(.75f,.63f,.39f));sand.GetComponent<Renderer>().sharedMaterial=sandMaterial;
  for(int i=0;i<5;i++){var prop=GameObject.CreatePrimitive(PrimitiveType.Cylinder);prop.transform.position=new Vector3(-5+i*2.5f,2,4);prop.transform.localScale=new Vector3(.6f,2,.6f);prop.GetComponent<Renderer>().sharedMaterial=material;}
  var root=new GameObject("Reference tornado atmosphere");var fx=GolfTornadoFX.Create(root.transform);fx.Tick(12,127,12,12);Save(camera,Path.Combine(directory,"sand-enter.png"),1400,1000);
  fx.Tick(12,ulong.MaxValue,12,12);Save(camera,Path.Combine(directory,"sand-steady.png"),1400,1000);
  fx.Tick(12,ulong.MaxValue<<13,12,12);Save(camera,Path.Combine(directory,"sand-leave.png"),1400,1000);
  fx.Tick(12,0,12,12);Save(camera,Path.Combine(directory,"clear-grass.png"),1400,1000);
  var volume=new GameObject("Proximity preview volume").AddComponent<UnityEngine.Rendering.Volume>();volume.isGlobal=true;volume.priority=650;volume.weight=.8f;var profile=ScriptableObject.CreateInstance<UnityEngine.Rendering.VolumeProfile>();volume.sharedProfile=profile;var color=profile.Add<ColorAdjustments>(true);color.saturation.value=-75;color.contrast.value=-8;color.postExposure.value=-.12f;color.colorFilter.value=new Color(.83f,.85f,.88f);var blur=profile.Add<DepthOfField>(true);blur.mode.value=DepthOfFieldMode.Gaussian;blur.gaussianStart.value=0;blur.gaussianEnd.value=4;blur.gaussianMaxRadius.value=1.15f;data.renderPostProcessing=true;data.requiresDepthOption=CameraOverrideOption.On;data.volumeLayerMask=1;camera.depthTextureMode|=DepthTextureMode.Depth;data.volumeTrigger=camera.transform;UnityEngine.Rendering.VolumeManager.instance.Update(camera.transform,1);Save(camera,Path.Combine(directory,"near-grey-blur.png"),1400,1000);
  Object.DestroyImmediate(root);Object.DestroyImmediate(material);Object.DestroyImmediate(sandMaterial);Object.DestroyImmediate(profile);Debug.Log("TORNADO_ATMOSPHERE_PREVIEW_RENDERED");
 }
 static void Save(Camera camera,string path,int width,int height){
  var rt=new RenderTexture(width,height,24,RenderTextureFormat.ARGB32);rt.Create();camera.targetTexture=rt;camera.aspect=width/(float)height;camera.Render();var previous=RenderTexture.active;RenderTexture.active=rt;var image=new Texture2D(width,height,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,width,height),0,0);image.Apply();File.WriteAllBytes(path,image.EncodeToPNG());RenderTexture.active=previous;camera.targetTexture=null;Object.DestroyImmediate(image);rt.Release();Object.DestroyImmediate(rt);
 }
}
