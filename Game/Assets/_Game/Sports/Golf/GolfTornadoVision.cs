using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
namespace WhatTheFish {
 // Local presentation only. Warm the render path outside the visible 14 m zone,
 // then animate neutral-to-haze values without toggling modes at that boundary.
 [DefaultExecutionOrder(250)] public sealed class GolfTornadoVision:MonoBehaviour {
  GolfHazardWorld hazards;Volume volume;VolumeProfile profile;ColorAdjustments colour;DepthOfField blur;Tonemapping tone;
  Camera current;UniversalAdditionalCameraData data;bool applied,oldPost;CameraOverrideOption oldDepth;LayerMask oldMask;DepthTextureMode oldTexture;
  float strength,strengthVelocity;
  void Awake(){
   hazards=GetComponent<GolfHazardWorld>();var node=new GameObject("Local tornado proximity haze");node.transform.SetParent(transform,false);volume=node.AddComponent<Volume>();volume.isGlobal=true;volume.priority=650;volume.weight=0;profile=ScriptableObject.CreateInstance<VolumeProfile>();volume.sharedProfile=profile;
   colour=profile.Add<ColorAdjustments>(true);tone=profile.Add<Tonemapping>(false);
   blur=profile.Add<DepthOfField>(true);blur.mode.value=DepthOfFieldMode.Gaussian;blur.gaussianStart.value=2;blur.highQualitySampling.value=false;
   ApplyStrength();
  }
  void LateUpdate(){
   var camera=Camera.main;bool playing=hazards&&PlayerView.Instance&&PlayerView.Instance.active&&camera&&camera.GetComponent<PlayerView>();
   float target=playing?hazards.TornadoViewIntensity(camera.transform.position)*.8f:0;
   if(MenuPreferences.ReducedMotion)target*=.7f;
   // Critical damping keeps velocity continuous when the player or storm turns.
   // Limit a stalled frame's integration so a hitch cannot jump the tint forward.
   strength=Mathf.SmoothDamp(strength,target,ref strengthVelocity,.6f,float.PositiveInfinity,Mathf.Min(Time.unscaledDeltaTime,.05f));
   if(current!=camera){RestoreCamera();current=camera;}
   bool warm=playing&&hazards.TornadoVisualsActive&&(applied||hazards.NearestTornado(camera.transform.position)<28);
   if(playing&&(warm||strength>.0001f)){
    if(!applied){data=camera.GetUniversalAdditionalCameraData();oldPost=data.renderPostProcessing;oldDepth=data.requiresDepthOption;oldMask=data.volumeLayerMask;oldTexture=camera.depthTextureMode;applied=true;data.renderPostProcessing=true;data.requiresDepthOption=CameraOverrideOption.On;data.volumeLayerMask=oldMask.value|1;camera.depthTextureMode|=DepthTextureMode.Depth;
     // Enabling a previously disabled post path must not suddenly enable the
     // scene's unrelated tone mapping. Already-graded cameras keep their grade.
     tone.mode.overrideState=!oldPost;if(!oldPost)tone.mode.Override(TonemappingMode.None);
    }
    ApplyStrength();volume.weight=1;
   }else{RestoreCamera();strength=strengthVelocity=0;}
  }
  void ApplyStrength(){
   colour.saturation.value=-75*strength;colour.contrast.value=-8*strength;colour.postExposure.value=-.12f*strength;colour.colorFilter.value=Color.Lerp(Color.white,new Color(.83f,.85f,.88f),strength);
   float t=Mathf.Clamp01(strength/.8f);
   // A weighted Gaussian mode snaps on even at tiny volume weights. Keep the
   // volume fully weighted and fade both CoC and radius instead. Override allows
   // an intentional neutral radius below the inspector's 0.5 minimum.
   blur.gaussianMaxRadius.Override(1.12f*t);blur.gaussianEnd.value=2+7.2f/Mathf.Max(t*t,1e-8f);
  }
  void RestoreCamera(){volume.weight=0;if(!applied)return;if(data){data.renderPostProcessing=oldPost;data.requiresDepthOption=oldDepth;data.volumeLayerMask=oldMask;}if(current)current.depthTextureMode=oldTexture;applied=false;data=null;}
  void OnDisable(){RestoreCamera();strength=strengthVelocity=0;}
  void OnDestroy(){RestoreCamera();if(volume)Destroy(volume.gameObject);if(profile){foreach(var component in profile.components)if(component)Destroy(component);Destroy(profile);}}
 }
}
