using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace WhatTheFish {
 // Coastal daylight and quality settings for the two walkable island venues.
 public sealed class CoastalEnvironment : MonoBehaviour {
  public Material sky;
  public bool basketball;
  public Vector2 islandRadii;
  public ScreenSpaceAmbientOcclusion exteriorContactShading;
  Light sun; Camera view;
  public void Apply(Camera camera,Light light) {
   view=camera;sun=light;
   RenderSettings.skybox=sky;view.clearFlags=CameraClearFlags.Skybox;
   // Keep the game's original bright display palette; its old renderer had no post-process resources.
   var data=view.GetComponent<UniversalAdditionalCameraData>();if(data)data.renderPostProcessing=false;
   view.farClipPlane=3000;
   {RenderSettings.fog=true;RenderSettings.fogStartDistance=1100;RenderSettings.fogEndDistance=2700;RenderSettings.fogColor=new Color(.59f,.78f,.88f);}
  }
  void LateUpdate(){
   if(!view||!sun)return;
   bool outside=basketball?(Mathf.Abs(view.transform.position.x)>27||Mathf.Abs(view.transform.position.z)>35):
    (Mathf.Abs(view.transform.position.x)>66||Mathf.Abs(view.transform.position.z)>86);
   if(exteriorContactShading)exteriorContactShading.SetActive(outside);
   if(!basketball)return;
   // The open court and exterior share daylight shadows.
   sun.shadows=LightShadows.Soft;
  }
  void OnDisable(){if(exteriorContactShading)exteriorContactShading.SetActive(false);}
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
  static void DesktopQuality(){
   if(Application.isMobilePlatform)return;
   // Windows builds register this as the default pipeline. Keeping it outside
   // Resources prevents shipping the desktop renderer in every Android APK.
   var pipeline=UnityEngine.Rendering.GraphicsSettings.defaultRenderPipeline as UniversalRenderPipelineAsset;
#if UNITY_EDITOR
   pipeline=UnityEditor.AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>("Assets/_Game/Settings/DesktopCoastURP.asset");
#endif
   if(!pipeline)return;
   // Runtime copy preserves the project's Android pipeline asset.
   pipeline=Object.Instantiate(pipeline);pipeline.name="Desktop coastal quality";
   pipeline.renderScale=1;pipeline.msaaSampleCount=4;pipeline.shadowDistance=230;
   pipeline.mainLightShadowmapResolution=4096;pipeline.shadowCascadeCount=4;
   QualitySettings.renderPipeline=pipeline;QualitySettings.lodBias=1.5f;
  }
 }
}
