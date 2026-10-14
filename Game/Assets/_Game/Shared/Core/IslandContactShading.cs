using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace WhatTheFish {
 public static class IslandContactShading {
  public static ScreenSpaceAmbientOcclusion Resolve(Camera camera){
   var pipeline=GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
   if(!pipeline||!camera)return null;
   var cameraData=camera.GetComponent<UniversalAdditionalCameraData>();
   var renderer=cameraData?cameraData.scriptableRenderer:pipeline.scriptableRenderer;
   // Bundled scenes must control the player's renderer feature, not a bundled copy.
   var data=pipeline.rendererDataList;
   for(int i=0;i<data.Length;i++){
    if(!data[i]||pipeline.GetRenderer(i)!=renderer)continue;
    foreach(var feature in data[i].rendererFeatures)
     if(feature is ScreenSpaceAmbientOcclusion contact)return contact;
   }
   return null;
  }
 }
}
