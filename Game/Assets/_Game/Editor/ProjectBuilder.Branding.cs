using System;
using System.IO;
using UnityEditor;
using UnityEditor.Android;
using UnityEditor.Build;
using UnityEngine;

public static partial class ProjectBuilder {
 // Derive launcher sizes from the existing logo; keep its editable master intact.
 public static void PrepareAndroidBranding(){
  const string folder="Assets/_Game/Art/AppBranding";
  string project=Path.GetFullPath(Path.Combine(Application.dataPath,".."));
  string DiskPath(string assetPath)=>Path.Combine(project,assetPath);
  Directory.CreateDirectory(DiskPath(folder));
  var logo=new Texture2D(2,2,TextureFormat.RGBA32,false);
  if(!logo.LoadImage(File.ReadAllBytes(DiskPath("Assets/_Game/Resources/Menu/WhatTheFishLogo.png"))))
   throw new InvalidOperationException("Game logo could not be read for Android branding.");
  var background=new Color32(8,31,44,255);
  try{
   WriteIcon("Background",1,false);
   WriteIcon("Foreground",.72f,true);
   WriteIcon("StoreIcon",.94f,false);
  }finally{UnityEngine.Object.DestroyImmediate(logo);}
  AssetDatabase.Refresh();
  Texture2D Load(string name){
   string path=folder+"/"+name+".png";
   var importer=(TextureImporter)AssetImporter.GetAtPath(path);
   importer.textureType=TextureImporterType.Default;importer.mipmapEnabled=false;
   importer.alphaIsTransparency=true;importer.textureCompression=TextureImporterCompression.Uncompressed;
   importer.maxTextureSize=512;importer.SaveAndReimport();
   return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
  }
  var front=Load("Foreground");var back=Load("Background");var flat=Load("StoreIcon");
  foreach(var kind in new[]{AndroidPlatformIconKind.Adaptive,AndroidPlatformIconKind.Round,AndroidPlatformIconKind.Legacy}){
   var icons=PlayerSettings.GetPlatformIcons(NamedBuildTarget.Android,kind);
   foreach(var icon in icons){
    if(kind==AndroidPlatformIconKind.Adaptive)icon.SetTextures(new[]{back,front});
    else icon.SetTexture(flat);
   }
   PlayerSettings.SetPlatformIcons(NamedBuildTarget.Android,kind,icons);
  }
  AssetDatabase.SaveAssets();
  Debug.Log("ANDROID_BRANDING_READY: existing logo assigned to adaptive, round and legacy launcher slots; 512px store icon generated.");

  void WriteIcon(string name,float scale,bool transparent){
   const int size=512;
   var texture=new Texture2D(size,size,TextureFormat.RGBA32,false);
   try{
    var pixels=new Color32[size*size];
    for(int y=0;y<size;y++)for(int x=0;x<size;x++){
     Color color=transparent?Color.clear:background;
     if(name!="Background"){
      float u=((x+.5f)/size-.5f)/scale+.5f,v=((y+.5f)/size-.5f)/scale+.5f;
      if(u>=0&&u<=1&&v>=0&&v<=1){
       var sample=logo.GetPixelBilinear(u,v);
       color=transparent?sample:new Color(sample.r*sample.a+color.r*(1-sample.a),sample.g*sample.a+color.g*(1-sample.a),sample.b*sample.a+color.b*(1-sample.a),1);
      }
     }
     pixels[y*size+x]=color;
    }
    texture.SetPixels32(pixels);texture.Apply();
    string path=folder+"/"+name+".png";var bytes=texture.EncodeToPNG();
    string diskPath=DiskPath(path);
    if(!File.Exists(diskPath)||!System.Linq.Enumerable.SequenceEqual(File.ReadAllBytes(diskPath),bytes))File.WriteAllBytes(diskPath,bytes);
   }finally{UnityEngine.Object.DestroyImmediate(texture);}
  }
 }
}
