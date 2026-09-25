using System;
using System.Linq;
using WhatTheFish;
using UnityEditor;
using UnityEngine;

public static partial class ProjectBuilder {
 static GameObject BuildFootball(){
  const string path=Root+"Prefabs/Football/Sunvale/SunvaleStadium.prefab";
  var source=AssetDatabase.LoadAssetAtPath<GameObject>(path);
  if(!source)throw new Exception("Missing Sunvale assembly. Run WHATTHE FISH?/Football/Build Sunvale review assets first.");
  var stadium=(GameObject)PrefabUtility.InstantiatePrefab(source);stadium.name="Football Stadium • Sunvale";
  var meshes=stadium.GetComponentsInChildren<MeshFilter>(true);
  if(!meshes.Any(m=>m.name=="Lawn__Pitch")||stadium.GetComponentsInChildren<LODGroup>(true).Length!=32)
   throw new Exception("Sunvale assembly is missing its lawn or seating LODs.");
  var view=stadium.AddComponent<SunvaleStadiumView>();
  view.flagModules=new[]{stadium.transform.Find("Banners").gameObject,stadium.transform.Find("CornerFlags").gameObject};
  view.accentRenderers=view.flagModules.SelectMany(g=>g.GetComponentsInChildren<Renderer>(true)).ToArray();
  // Use the new sign's position and suppress its fixed Blender lettering.
  foreach(var renderer in stadium.GetComponentsInChildren<Renderer>(true))
   if(renderer.name=="Scoreboard__Lettering")renderer.gameObject.SetActive(false);
  var screen=WorldCanvas("Stadium screen",stadium.transform,new Vector3(0,20,-73.72f),new Vector2(13.7f,3.7f),Quaternion.Euler(0,180,0));
  view.title=WorldText(screen,"SUNVALE",new Vector2(0,75),new Vector2(1600,180),130,Color.white);
  view.subtitle=WorldText(screen,"MAKE YOURSELF AT HOME",new Vector2(0,-105),new Vector2(1600,90),46,LocalProfile.Teams[0]);
  return stadium;
 }
}
