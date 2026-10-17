using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace WhatTheFish {
 // Contact shading produces coarse dark speckles on the small cabin surfaces.
 // Cover inside and nearby third-person views, retaining normal sun shadows.
 public sealed class SkySailCabinLighting:MonoBehaviour {
  ScreenSpaceAmbientOcclusion contact;Camera suppressed;bool restore;
  public bool Suppressed {get;private set;}
  public bool InteriorView(Camera camera){if(!camera)return false;var p=transform.InverseTransformPoint(camera.transform.position);return Mathf.Abs(p.x)<15.5f&&Mathf.Abs(p.z)<16.5f&&p.y>-3&&p.y<14;}
  void Awake(){foreach(var r in GetComponentsInChildren<Renderer>(true))foreach(var m in r.sharedMaterials)if(m&&m.name=="SS_Lamp"){r.shadowCastingMode=ShadowCastingMode.Off;r.receiveShadows=false;}}
  void OnEnable(){RenderPipelineManager.beginCameraRendering+=Before;RenderPipelineManager.endCameraRendering+=After;}
  void Before(ScriptableRenderContext context,Camera camera){if(!InteriorView(camera))return;contact=IslandContactShading.Resolve(camera);if(!contact)return;restore=contact.isActive;suppressed=camera;contact.SetActive(false);Suppressed=true;}
  void After(ScriptableRenderContext context,Camera camera){if(camera==suppressed)Restore();}
  void Restore(){if(contact)contact.SetActive(restore);contact=null;suppressed=null;Suppressed=false;}
  void OnDisable(){RenderPipelineManager.beginCameraRendering-=Before;RenderPipelineManager.endCameraRendering-=After;Restore();}
 }
}
