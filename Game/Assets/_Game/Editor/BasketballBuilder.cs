using System;
using System.IO;
using System.Linq;
using WhatTheFish;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public static partial class ProjectBuilder {
 static GameObject BuildBasketball(){
  const string art=Root+"Art/Basketball/Rally/";
  var source=AssetDatabase.LoadAssetAtPath<GameObject>(Root+"Prefabs/Basketball/Rally/RallyArena.prefab");
  if(!source)throw new Exception("Build the Rally art library before rebuilding the player.");
  var arena=(GameObject)PrefabUtility.InstantiatePrefab(source);arena.name="Basketball Arena • Rally Court";
  if(arena.GetComponentsInChildren<LODGroup>(true).Length!=48||!arena.transform.Find("Court"))
   throw new Exception("Rally assembly is missing its court or seating LODs.");
  var floorRenderers=arena.GetComponentsInChildren<Renderer>(true);
  var foundation=floorRenderers.Single(r=>r.name=="Court__Foundation").bounds;
  var apron=floorRenderers.Single(r=>r.name=="Court__Apron").bounds;
  if(foundation.max.y>apron.min.y+.0001f||apron.max.y-foundation.max.y<.10f)
   throw new Exception("Rally floor surfaces overlap. Re-export the corrected Blender court before building.");
  var view=arena.AddComponent<BasketballArenaView>();view.modularRally=true;
  view.logoCatalog=new GameObject[4];view.logoCatalog[0]=arena.transform.Find("CenterEmblem").gameObject;
  BuildRallyLogoVariants(arena,view,art);
  // Indoor bounds match the refined geometry. These proxies also stop the camera.
  RallyCollision(arena,"Collision_Canopy",new Vector3(0,14.55f,0),new Vector3(43,.25f,57));
  RallyCollision(arena,"Collision_ScorerTable",new Vector3(10.1f,.70f,0),new Vector3(1.2f,1.4f,4.45f));
  foreach(float side in new[]{-1f,1f})foreach(float z in new[]{-7f,7f})
   RallyCollision(arena,"Collision_TeamSeats",new Vector3(side*10.1f,.55f,z),new Vector3(.7f,1.1f,3.35f));
  foreach(var renderer in arena.GetComponentsInChildren<Renderer>(true))
   if(renderer.name=="Scoreboard__Lettering")renderer.gameObject.SetActive(false);
  for(int i=0;i<4;i++){
   var angle=i*90f;var rotation=Quaternion.Euler(0,angle,0);var normal=rotation*Vector3.back;
   var panel=WorldCanvas("Rally scoreboard",arena.transform,new Vector3(0,10.2f,0)+normal*2.055f,new Vector2(3.42f,1.66f),rotation);
   WorldText(panel,"HOME       AWAY",new Vector2(0,285),new Vector2(1580,110),82,Color.white);
   WorldText(panel,"00 : 00",new Vector2(0,0),new Vector2(1580,330),240,LocalProfile.Hex("F5C356"));
   var sign=panel.gameObject.AddComponent<StadiumSign>();sign.nameSign=true;
   sign.label=WorldText(panel,"RALLY COURT",new Vector2(0,-290),new Vector2(1580,110),62,Color.white);
  }
  var bounce=new GameObject("Rally warm fill").AddComponent<Light>();bounce.transform.SetParent(arena.transform,false);
  bounce.type=LightType.Directional;bounce.color=new Color(1,.86f,.70f);bounce.intensity=.35f;
  bounce.shadows=LightShadows.None;bounce.transform.localRotation=Quaternion.Euler(32,155,0);
  view.Apply(LocalProfile.Basketball);return arena;
 }
 static void RallyCollision(GameObject parent,string name,Vector3 center,Vector3 size){
  var go=new GameObject(name);go.transform.SetParent(parent.transform,false);go.layer=8;
  var collider=go.AddComponent<BoxCollider>();collider.center=center;collider.size=size;
 }
 static void BuildRallyLogoVariants(GameObject arena,BasketballArenaView view,string art){
  // These prefabs retain the original saved logo indices without a placeholder FBX dependency.
  for(int i=1;i<4;i++){
   var path=art+"LogoVariants/Logo"+i+".prefab";
   var source=AssetDatabase.LoadAssetAtPath<GameObject>(path);
   if(!source)throw new Exception("Missing standalone Rally logo variant: "+path);
   var logo=(GameObject)PrefabUtility.InstantiatePrefab(source);
   logo.transform.SetParent(arena.transform,false);logo.name="Logo"+i;logo.layer=8;
   view.logoCatalog[i]=logo;
  }
 }
}
