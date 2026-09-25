using System;
using UnityEngine;
using UnityEngine.Rendering;
namespace SportsPrototype {
 public sealed class SportEnvironmentController:MonoBehaviour {
  public SportDefinition[] definitions;public GameObject[] roots;public Light mainLight;public Camera mainCamera;
  public SportId Selected {get;private set;}=SportId.Football;
  public SportDefinition Current=>Definition(Selected);
  public StadiumView View=>roots[Array.FindIndex(definitions,d=>d.id==Selected)].GetComponent<StadiumView>();
  public SportDefinition Definition(SportId sport)=>Array.Find(definitions,d=>d.id==sport);
  public void Activate(SportId sport){
   var def=Definition(sport);if(!def||!def.available)throw new ArgumentException("Sport unavailable: "+sport);
   Selected=sport;for(int i=0;i<roots.Length;i++)roots[i].SetActive(definitions[i].id==sport);
   mainLight.transform.rotation=Quaternion.Euler(def.lightRotation);mainLight.intensity=def.lightIntensity;mainLight.color=def.indoor?Color.white:new Color(1,.94f,.85f);mainLight.shadows=def.indoor?LightShadows.None:LightShadows.Soft;
   RenderSettings.ambientMode=AmbientMode.Flat;RenderSettings.ambientLight=def.ambientColor;RenderSettings.fog=!def.indoor;mainCamera.backgroundColor=def.backgroundColor;
   mainCamera.farClipPlane=def.farClip;RenderSettings.fogMode=FogMode.Linear;RenderSettings.fogStartDistance=def.fogStart;RenderSettings.fogEndDistance=def.fogEnd;RenderSettings.fogColor=def.backgroundColor;
  }
 }
}
