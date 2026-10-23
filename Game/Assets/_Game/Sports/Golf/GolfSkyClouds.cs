using System.Linq;
using UnityEngine;
namespace WhatTheFish {
 // Golf sits near one side of the shared cloud ring. Keep its sky distant
 // while on the course, and restore the shared travel scenery on exit.
 public sealed class GolfSkyClouds:MonoBehaviour {
  Transform[] clouds;Vector3[] positions,scales;
  void LateUpdate(){
   var world=SkySailWorld.Instance;if(!world||!AppRoot.Instance)return;
   if(clouds==null){clouds=world.scenery.GetComponentsInChildren<Transform>(true).Where(t=>t.name.StartsWith("Shared cloud ")).ToArray();positions=clouds.Select(t=>t.localPosition).ToArray();scales=clouds.Select(t=>t.localScale).ToArray();}
   bool golf=AppRoot.Instance.SelectedSport==SportId.Golf&&!world.Travelling;
   for(int i=0;i<clouds.Length;i++)if(clouds[i]){clouds[i].localPosition=golf?positions[i]+SkySailMap.Center(SportId.Golf):positions[i];clouds[i].localScale=golf?Vector3.Scale(scales[i],new Vector3(.48f,.52f,.48f)):scales[i];}
  }
  void OnDisable(){if(clouds==null)return;for(int i=0;i<clouds.Length;i++)if(clouds[i]){clouds[i].localPosition=positions[i];clouds[i].localScale=scales[i];}}
 }
}
