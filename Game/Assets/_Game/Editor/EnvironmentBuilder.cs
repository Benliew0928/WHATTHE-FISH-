using System.Linq;
using WhatTheFish;
using UnityEditor;
using UnityEngine;

public static partial class ProjectBuilder {
 static void ConfigureEnvironments(AppRoot app,GameObject football,GameObject basketball,GameObject golf,GameObject fishing,Light light,Camera camera){
  var env=app.gameObject.AddComponent<SportEnvironmentController>();app.environments=env;env.mainLight=light;env.mainCamera=camera;
  var sports=new[]{SportId.Football,SportId.Basketball,SportId.Golf,SportId.Fishing};env.roots=new[]{football,basketball,golf,fishing};env.definitions=new SportDefinition[sports.Length];
  for(int i=0;i<sports.Length;i++){
   var sport=sports[i];var def=AssetDatabase.LoadAssetAtPath<SportDefinition>(Root+"Sports/"+sport+"/"+sport+".asset");
   def.environmentPrefab=PrefabUtility.SaveAsPrefabAsset(env.roots[i],Root+"Resources/"+sport+"Environment.prefab");
   def.maxPlayers=sport==SportId.Fishing?5:10;def.farClip=450;def.fogStart=180;def.fogEnd=430;def.elevatedDistance=19;def.indoor=false;def.lightIntensity=1.1f;def.lightRotation=new Vector3(48,-35,0);
   def.ambientColor=new Color(.58f,.64f,.7f);def.backgroundColor=LocalProfile.Hex("BFE2E7");
   switch(sport){
    case SportId.Football:
     def.lightIntensity=1.25f;def.lightRotation=new Vector3(56,-35,0);def.ambientColor=new Color(.48f,.58f,.70f);def.backgroundColor=LocalProfile.Hex("5CABEB");
     def.spawnPositions=Enumerable.Range(0,10).Select(n=>new Vector3((n%5-2)*2,1,-8-(n/5)*3)).ToArray();
     def.menuCamera=new Vector3(87,64,-103);def.menuFocus=new Vector3(0,0,4);break;
    case SportId.Basketball:
     def.spawnPositions=Enumerable.Range(0,10).Select(n=>new Vector3((n%5-2)*1.5f,1,-4-(n/5)*2)).ToArray();
     def.menuCamera=new Vector3(9,9.5f,15);def.menuFocus=new Vector3(0,1.2f,-1);def.elevatedDistance=12;def.indoor=true;
     def.lightIntensity=1.15f;def.lightRotation=new Vector3(72,-25,0);def.ambientColor=new Color(.66f,.69f,.74f);def.backgroundColor=LocalProfile.Hex("233950");break;
    case SportId.Golf:
     def.spawnPositions=ReadGolf().spawns;
     def.menuCamera=new Vector3(295,285,-350);def.menuFocus=new Vector3(-65,0,25);def.farClip=1400;def.fogStart=700;def.fogEnd=1300;
     def.lightIntensity=1.25f;def.lightRotation=new Vector3(48,-35,0);def.ambientColor=new Color(.64f,.72f,.78f);def.backgroundColor=LocalProfile.Hex("7FC6EF");def.elevatedDistance=25;break;
    case SportId.Fishing:
     def.spawnPositions=ReadFishing().spawns;def.menuCamera=new Vector3(94,126,-152);def.menuFocus=new Vector3(0,0,0);
     def.farClip=1200;def.fogStart=350;def.fogEnd=1000;def.elevatedDistance=18;
     def.lightIntensity=1.25f;def.lightRotation=new Vector3(48,-35,0);def.ambientColor=new Color(.64f,.72f,.78f);def.backgroundColor=LocalProfile.Hex("7FC6EF");break;
   }
   EditorUtility.SetDirty(def);env.definitions[i]=def;
  }
  env.Activate(SportId.Football);
 }
}
