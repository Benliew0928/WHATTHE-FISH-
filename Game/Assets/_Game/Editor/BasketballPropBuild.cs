using System;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class BasketballPropBuild {
 const string Root="Assets/_Game/Resources/BasketballProps/";
 [MenuItem("WHATTHE FISH?/Basketball/Prepare props")]
 public static void Prepare(){
  Directory.CreateDirectory(Root);AssetDatabase.Refresh();
  string[] names={"Ivory","Gold","Orange","Ink","Mint","Blue","Black"};
  Color[] colors={new(.95f,.90f,.70f),new(1,.55f,.045f),new(.92f,.24f,.025f),new(.018f,.065f,.075f),new(.07f,.78f,.52f),new(.02f,.40f,1),new(.003f,.006f,.02f)};
  for(int i=0;i<names.Length;i++){
   string path=Root+"Prop"+names[i]+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);
   if(!m){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,path);}m.SetColor("_BaseColor",colors[i]);m.SetFloat("_Smoothness",.3f);m.SetFloat("_Metallic",i==1||i==5?.2f:0);m.enableInstancing=true;EditorUtility.SetDirty(m);
  }
  foreach(var name in new[]{"LockOn","Turbo","Portal"}){
   var model=AssetImporter.GetAtPath(Root+name+".fbx") as ModelImporter;
   model.importAnimation=false;model.importCameras=false;model.importLights=false;model.isReadable=false;model.meshCompression=ModelImporterMeshCompression.Medium;model.materialImportMode=ModelImporterMaterialImportMode.ImportStandard;
   model.SaveAndReimport();
   foreach(var material in AssetDatabase.LoadAllAssetsAtPath(Root+name+".fbx"))if(material is Material){var shared=AssetDatabase.LoadAssetAtPath<Material>(Root+material.name+".mat");if(shared)model.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material),material.name),shared);}
   model.SaveAndReimport();
   var texture=(TextureImporter)AssetImporter.GetAtPath(Root+name+".png");texture.textureType=TextureImporterType.Sprite;texture.spriteImportMode=SpriteImportMode.Single;texture.alphaIsTransparency=true;texture.mipmapEnabled=false;texture.isReadable=false;texture.maxTextureSize=256;texture.textureCompression=TextureImporterCompression.Compressed;
   texture.SetPlatformTextureSettings(new TextureImporterPlatformSettings{name="Android",overridden=true,maxTextureSize=256,format=TextureImporterFormat.ASTC_6x6,compressionQuality=60});texture.SaveAndReimport();
  }
  Make("PropEffect",Shader.Find("WhatTheFish/Basketball Prop Effect"));
  var trail=Make("PropTrail",Shader.Find("Universal Render Pipeline/Particles/Unlit"));trail.SetColor("_BaseColor",new Color(1,1,1,.7f));trail.SetFloat("_Surface",1);trail.SetFloat("_Blend",0);trail.SetFloat("_SrcBlend",5);trail.SetFloat("_DstBlend",10);trail.SetFloat("_ZWrite",0);trail.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");trail.renderQueue=3000;EditorUtility.SetDirty(trail);
  AssetDatabase.SaveAssets();Debug.Log("BASKETBALL_PROPS_IMPORT_OK");
 }
 static Material Make(string name,Shader shader){if(!shader)throw new Exception("Missing prop shader "+name);var m=AssetDatabase.LoadAssetAtPath<Material>(Root+name+".mat");if(!m){m=new Material(shader);AssetDatabase.CreateAsset(m,Root+name+".mat");}return m;}
 public static void BuildReview(){Prepare();ProjectBuilder.BuildWindowsPlayer("../Builds/BasketballPropsQA/Player/WhatTheFish.exe");}
}
