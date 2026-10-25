using System.IO;
using UnityEditor;
using UnityEngine;

namespace WhatTheFish.Editor {
 public static class IslandAwardsBuild {
  public static void Prepare(){
   Directory.CreateDirectory("Assets/_Game/Art/IslandAwards");Directory.CreateDirectory("Assets/_Game/Resources/IslandAwards");AssetDatabase.Refresh();
   foreach(bool fishing in new[]{true,false}){
    string name=fishing?"Fishing":"Golf",folder="Assets/_Game/Art/IslandAwards/"+name;Directory.CreateDirectory(folder);AssetDatabase.Refresh();
    var root=IslandAwardsStage.Create(fishing);
    foreach(var filter in root.GetComponentsInChildren<MeshFilter>()){
     string path=folder+"/"+filter.name.Replace(" ","-")+".asset";var old=AssetDatabase.LoadAssetAtPath<Mesh>(path);
     if(old){EditorUtility.CopySerialized(filter.sharedMesh,old);Object.DestroyImmediate(filter.sharedMesh);filter.sharedMesh=old;}else AssetDatabase.CreateAsset(filter.sharedMesh,path);
     var renderer=filter.GetComponent<MeshRenderer>();var mat=renderer.sharedMaterial;string matPath=folder+"/"+mat.name.Replace(" ","-")+".mat";var existing=AssetDatabase.LoadAssetAtPath<Material>(matPath);
     if(existing){EditorUtility.CopySerialized(mat,existing);Object.DestroyImmediate(mat);renderer.sharedMaterial=existing;}else AssetDatabase.CreateAsset(mat,matPath);
    }
    PrefabUtility.SaveAsPrefabAsset(root,"Assets/_Game/Resources/IslandAwards/"+name+".prefab");Object.DestroyImmediate(root);
   }
   AssetDatabase.SaveAssets();AssetDatabase.Refresh();Debug.Log("ISLAND_AWARDS_ASSETS_READY");
  }
  public static void Windows(){Prepare();ProjectBuilder.BuildWindowsPlayer("../Builds/AwardsEventPolishQA/PlayerV2/WhatTheFish.exe");}
 }
}
