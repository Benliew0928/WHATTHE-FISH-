using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
namespace WhatTheFish {
 public sealed class GolfHazardVisual:MonoBehaviour {
  Transform crab;Transform[] legs,claws;Quaternion[] legHome,clawHome;LineRenderer ring;GolfTornadoFX cyclone;GolfTornadoAudio cycloneAudio;float bumpAt=-100;int lane;
  public static GolfHazardVisual Create(Transform parent,int lane){var node=new GameObject("Golf hazard lane "+lane);node.transform.SetParent(parent,false);var visual=node.AddComponent<GolfHazardVisual>();visual.lane=lane;return visual;}
  void BuildCrab(){
   var prefab=Resources.Load<GameObject>("GolfEvents/CrossingCrab");if(!prefab)return;crab=Instantiate(prefab,transform).transform;crab.name="Animated crossing crab";
   legs=crab.GetComponentsInChildren<Transform>(true).Where(t=>t.name.StartsWith("Leg_")).OrderBy(t=>t.name).ToArray();legHome=legs.Select(t=>t.localRotation).ToArray();
   claws=crab.GetComponentsInChildren<Transform>(true).Where(t=>t.name.StartsWith("Claw_")).OrderBy(t=>t.name).ToArray();clawHome=claws.Select(t=>t.localRotation).ToArray();
  }
  public void Tick(GolfEventKind kind,Vector3 point,Vector3 forward,double seconds,bool warning,ulong sandTrail=0,double sampledAt=0,double now=0,float opacity=1){
   gameObject.SetActive(true);transform.SetPositionAndRotation(point,Quaternion.LookRotation(forward));bool tornado=kind==GolfEventKind.Tornado;
   if(!ring){var node=new GameObject("Hazard ground warning");node.transform.SetParent(transform,false);ring=node.AddComponent<LineRenderer>();ring.sharedMaterial=Resources.Load<Material>("GolfBallTrail");ring.positionCount=48;ring.loop=true;ring.useWorldSpace=false;ring.widthMultiplier=.075f;ring.numCapVertices=3;ring.shadowCastingMode=ShadowCastingMode.Off;ring.receiveShadows=false;}
   ring.enabled=warning||!tornado;float pulse=MenuPreferences.ReducedMotion?1:.78f+.22f*Mathf.Sin((float)seconds*3);var color=warning?GolfHUDStyle.Gold:new Color(.14f,.24f,.27f,.4f);color.a*=pulse*opacity;ring.startColor=ring.endColor=color;
   float radius=tornado?GolfHazardRules.TornadoRadius:.72f;for(int i=0;i<48;i++){float a=i*Mathf.PI/24;ring.SetPosition(i,new Vector3(Mathf.Cos(a)*radius,.035f,Mathf.Sin(a)*radius));}
   if(tornado){if(crab)crab.gameObject.SetActive(false);if(!warning){if(!cyclone)cyclone=GolfTornadoFX.Create(transform,lane);cyclone.Tick(seconds,sandTrail,sampledAt,now,opacity);if(!cycloneAudio)cycloneAudio=gameObject.AddComponent<GolfTornadoAudio>();cycloneAudio.Tick(GolfHazardRules.TornadoGrowth(seconds),opacity,lane);}else{cyclone?.Hide();cycloneAudio?.Silence();}return;}
   cyclone?.Hide();cycloneAudio?.Silence();if(!crab)BuildCrab();if(!crab)return;crab.gameObject.SetActive(true);
   float walk=(float)seconds*9+lane;bool reduced=MenuPreferences.ReducedMotion;crab.localPosition=Vector3.up*(reduced?.015f:.025f+.02f*Mathf.Sin(walk*2));crab.localScale=Vector3.one*(warning?.75f:1);
   for(int i=0;i<legs.Length;i++)legs[i].localRotation=legHome[i]*(reduced?Quaternion.identity:Quaternion.Euler(Mathf.Sin(walk+i*Mathf.PI)*23,Mathf.Cos(walk+i*Mathf.PI)*5,Mathf.Cos(walk+i*Mathf.PI)*12));
   float bump=reduced?0:Mathf.Clamp01(1-(Time.unscaledTime-bumpAt)/.45f);
   for(int i=0;i<claws.Length;i++)claws[i].localRotation=clawHome[i]*(reduced?Quaternion.identity:Quaternion.Euler(Mathf.Sin(walk*.65f+i)*12+bump*44,(i==0?-1:1)*(10+12*Mathf.Sin(walk*.5f)),0));
  }
  public void Bump(){bumpAt=Time.unscaledTime;}
  public void Hide(){cyclone?.Hide();cycloneAudio?.Silence();gameObject.SetActive(false);}
 }
}
