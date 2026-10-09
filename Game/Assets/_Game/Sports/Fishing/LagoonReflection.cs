using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace WhatTheFish {
 // Six explicit URP requests, once per island load. No background probe jobs.
 public sealed class LagoonReflection:MonoBehaviour {
  Material water;RenderTexture cube;Camera capture;
  public bool Ready=>water&&water.GetFloat("_ReflectionReady")>.5f;
  IEnumerator Start(){
   if(SystemInfo.graphicsDeviceType==GraphicsDeviceType.Null)yield break;
   var surface=transform.Find("Transparent moving lagoon");if(!surface)yield break;water=surface.GetComponent<Renderer>().material;
   if((SystemInfo.copyTextureSupport&CopyTextureSupport.DifferentTypes)==0){Debug.Log("LAGOON_REFLECTION sky fallback: cubemap copy unavailable");yield break;}
   cube=new RenderTexture(128,128,24,RenderTextureFormat.ARGB32){name="Lagoon reflection",dimension=TextureDimension.Cube,useMipMap=true,autoGenerateMips=false,filterMode=FilterMode.Trilinear};cube.Create();
   var node=new GameObject("Lagoon reflection capture");node.transform.SetParent(transform,false);node.transform.position=surface.position+Vector3.up*.7f;
   capture=node.AddComponent<Camera>();capture.enabled=false;capture.fieldOfView=90;capture.aspect=1;capture.allowHDR=false;capture.allowMSAA=false;capture.nearClipPlane=.2f;capture.farClipPlane=110;capture.clearFlags=CameraClearFlags.Skybox;capture.cullingMask=(1<<8)|1;
   var data=node.AddComponent<UniversalAdditionalCameraData>();data.renderPostProcessing=false;data.renderShadows=false;data.requiresColorOption=data.requiresDepthOption=CameraOverrideOption.Off;
   Vector3[] directions={Vector3.right,Vector3.left,Vector3.up,Vector3.down,Vector3.forward,Vector3.back};
   for(int face=0;face<6;face++){
    yield return null;capture.transform.rotation=Quaternion.LookRotation(directions[face],face==2?Vector3.back:face==3?Vector3.forward:Vector3.up);
    var request=new UniversalRenderPipeline.SingleCameraRequest{destination=cube,face=(CubemapFace)face};
    if(!RenderPipeline.SupportsRenderRequest(capture,request)){Debug.Log("LAGOON_REFLECTION sky fallback: request unavailable");yield break;}
    RenderPipeline.SubmitRenderRequest(capture,request);
   }
   cube.GenerateMips();water.SetTexture("_LagoonReflection",cube);water.SetFloat("_ReflectionReady",1);Debug.Log("LAGOON_REFLECTION ready=True faces=6 resolution=128");Destroy(capture.gameObject);
  }
  void OnDestroy(){if(water)Destroy(water);if(cube){cube.Release();Destroy(cube);}}
 }
}
