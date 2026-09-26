using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace WhatTheFish {
 [Serializable] public sealed class IslandLayout {
  public string sport,concept,name,terrain_file,structures_file,cascade_file,source;
  public float extent,water_extent,sea_level; public int capacity;
  public Vector3[] spawns,stands,bunkers; public Vector3 safe_return;
  public IslandPlacement[] instances; public IslandBoundary[] boundaries;
  public IslandRoute[] routes; public IslandReviewView[] views;
  public IslandSign[] signs;
 }
 [Serializable] public sealed class IslandPlacement {public string module,id,accent;public Vector3 position;public float yaw,scale;}
 [Serializable] public sealed class IslandBoundary {public Vector3 a,b;}
 [Serializable] public sealed class IslandRoute {public string name;public Vector3[] points;}
 [Serializable] public sealed class IslandReviewView {public string name;public Vector3 position,target;public float fov;}
 [Serializable] public sealed class IslandSign {public string title,caption;public Vector3 position;public float yaw,width;}
 [Serializable] public sealed class IslandLightingProfile {
  public Material sky; public Color ambient=new Color(.46f,.55f,.64f),sunColor=new Color(1,.95f,.87f),fogColor=new Color(.59f,.78f,.88f);
  public Vector3 sunRotation=new Vector3(53,-35,0);
  public float intensity=1.18f,farClip=3000,fogStart=1000,fogEnd=2700;
  public bool contactShading=true;
  public float contactIntensity=.5f,contactRadius=.22f,shadowBias=.65f,shadowNormalBias=.45f;
 }
 public sealed class RefinedIslandEnvironment:MonoBehaviour {
  public IslandLayout layout;public IslandLightingProfile lighting;
  public ScreenSpaceAmbientOcclusion contactShading;
  public static RefinedIslandEnvironment Active {get;private set;}
  string previousContact;Light appliedSun;float previousBias,previousNormalBias;
  void OnEnable(){Active=this;}
  void OnDisable(){
   if(Active==this)Active=null;
   if(contactShading){if(previousContact!=null)JsonUtility.FromJsonOverwrite(previousContact,contactShading);contactShading.SetActive(false);previousContact=null;}
   if(appliedSun){appliedSun.shadowBias=previousBias;appliedSun.shadowNormalBias=previousNormalBias;appliedSun=null;}
  }
  public void Apply(Camera camera,Light sun){
   Active=this;var p=lighting;RenderSettings.skybox=p.sky;camera.clearFlags=CameraClearFlags.Skybox;
   RenderSettings.ambientMode=AmbientMode.Flat;RenderSettings.ambientLight=p.ambient;
   RenderSettings.fog=true;RenderSettings.fogMode=FogMode.Linear;RenderSettings.fogStartDistance=p.fogStart;RenderSettings.fogEndDistance=p.fogEnd;RenderSettings.fogColor=p.fogColor;
   sun.transform.rotation=Quaternion.Euler(p.sunRotation);sun.color=p.sunColor;sun.intensity=p.intensity;sun.shadows=LightShadows.Soft;
   if(!appliedSun){appliedSun=sun;previousBias=sun.shadowBias;previousNormalBias=sun.shadowNormalBias;}
   sun.shadowBias=p.shadowBias;sun.shadowNormalBias=p.shadowNormalBias;
   camera.farClipPlane=p.farClip;var data=camera.GetComponent<UniversalAdditionalCameraData>();if(data)data.renderPostProcessing=false;
   if(contactShading){
    if(previousContact==null)previousContact=JsonUtility.ToJson(contactShading);
    // URP's SSAO settings are serialized private fields, so use its serialization
    // contract without reflection. Restore the stadium settings on world exit.
    JsonUtility.FromJsonOverwrite("{\"m_Settings\":{\"Intensity\":"+p.contactIntensity.ToString(System.Globalization.CultureInfo.InvariantCulture)+",\"Radius\":"+p.contactRadius.ToString(System.Globalization.CultureInfo.InvariantCulture)+"}}",contactShading);
    contactShading.SetActive(p.contactShading);
   }
  }
 }
}
