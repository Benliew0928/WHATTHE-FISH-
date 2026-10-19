using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Object=UnityEngine.Object;

public static class GolfPolishBuild {
 const string Output="Assets/_Game/Resources/GolfUI/FinalCountdown/";
 static string Masters=>Path.GetFullPath(Path.Combine(Application.dataPath,"../../ArtSource/Golf/UI/FinalCountdown/"));
 static string Delivery=>Path.Combine(Application.dataPath,"_Game/Resources/GolfUI/FinalCountdown/");
 static Texture2D Read(string name){var t=new Texture2D(2,2,TextureFormat.RGBA32,false);if(!t.LoadImage(File.ReadAllBytes(Masters+name+"-master.png")))throw new Exception(name);return t;}
 static void Save(Texture2D tex,string name){
  var output=new Texture2D(1024,Mathf.RoundToInt(tex.height*1024f/tex.width),TextureFormat.RGBA32,false);
  for(int y=0;y<output.height;y++)for(int x=0;x<output.width;x++)output.SetPixel(x,y,tex.GetPixelBilinear((x+.5f)/output.width,(y+.5f)/output.height));
  output.Apply();File.WriteAllBytes(Delivery+name+".png",output.EncodeToPNG());Object.DestroyImmediate(output);
 }
 static void Import(string name,int max=1024){
  string path=Output+name+".png";AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);var i=(TextureImporter)AssetImporter.GetAtPath(path);
  i.textureType=TextureImporterType.Sprite;i.spriteImportMode=SpriteImportMode.Single;var textureSettings=new TextureImporterSettings();i.ReadTextureSettings(textureSettings);textureSettings.spriteMeshType=SpriteMeshType.FullRect;i.SetTextureSettings(textureSettings);i.mipmapEnabled=false;i.alphaIsTransparency=true;i.npotScale=TextureImporterNPOTScale.None;i.maxTextureSize=max;i.textureCompression=TextureImporterCompression.CompressedHQ;
  var android=i.GetPlatformTextureSettings("Android");android.overridden=true;android.maxTextureSize=max;android.format=TextureImporterFormat.ASTC_4x4;i.SetPlatformTextureSettings(android);i.SaveAndReimport();
 }
 public static void Prepare(){
  Directory.CreateDirectory(Delivery);var plate=Read("Plate");Save(plate,"Plate");Object.DestroyImmediate(plate);Import("Plate");
  var original=Read("Reference");Save(original,"Reference");Object.DestroyImmediate(original);Import("Reference");
  var atlas=Read("Digits");
  // AI atlas cells are recorded explicitly, rather than runtime slicing guesses.
  // Keep only the largest connected alpha component per glyph; generated loose
  // antialias specks outside the outlined letter are not delivery artwork.
  int[] xs={24,409,748,1101},xe={369,705,1066,1434};int[] ys={54,413,739},ye={390,733,1042};
  for(int n=0;n<11;n++){
   int col=n%4,row=n/4,x0=xs[col],y0=atlas.height-ye[row],w=xe[col]-x0,h=ye[row]-ys[row];var data=atlas.GetPixels(x0,y0,w,h);var visited=new bool[data.Length];var best=new List<int>();
   for(int at=0;at<data.Length;at++)if(!visited[at]&&data[at].a>.2f){var group=new List<int>();var queue=new Queue<int>();queue.Enqueue(at);visited[at]=true;while(queue.Count>0){int k=queue.Dequeue();group.Add(k);int x=k%w,y=k/w;foreach(int next in new[]{x>0?k-1:-1,x+1<w?k+1:-1,y>0?k-w:-1,y+1<h?k+w:-1})if(next>=0&&!visited[next]&&data[next].a>.2f){visited[next]=true;queue.Enqueue(next);}}if(group.Count>best.Count)best=group;}
   var mask=new bool[data.Length];foreach(int k in best)mask[k]=true;
   int minx=w,miny=h,maxx=0,maxy=0;foreach(int k in best){minx=Math.Min(minx,k%w);maxx=Math.Max(maxx,k%w);miny=Math.Min(miny,k/w);maxy=Math.Max(maxy,k/w);}
   var glyph=new Texture2D(maxx-minx+5,maxy-miny+5,TextureFormat.RGBA32,false);var clean=new Color[glyph.width*glyph.height];
   for(int y=miny;y<=maxy;y++)for(int x=minx;x<=maxx;x++){int k=y*w+x;if(mask[k])clean[(y-miny+2)*glyph.width+x-minx+2]=data[k];}
   glyph.SetPixels(clean);glyph.Apply();File.WriteAllBytes(Delivery+"Glyph"+n+".png",glyph.EncodeToPNG());Object.DestroyImmediate(glyph);Import("Glyph"+n,256);
  }
  Object.DestroyImmediate(atlas);
  string material="Assets/_Game/Resources/GolfCelebration.mat";if(!AssetDatabase.LoadAssetAtPath<Material>(material)){var m=new Material(Shader.Find("WhatTheFish/GolfCelebration")){name="GolfCelebration"};AssetDatabase.CreateAsset(m,material);}
  AssetDatabase.SaveAssets();Debug.Log("GOLF_POLISH_ART_READY");
 }
 public static void Build(){Prepare();ProjectBuilder.BuildWindowsPlayer("../Builds/GolfPolishQA/Player/WhatTheFish.exe");}
 public static void RefreshScripts(){var report=PackagedContentBuild.BuildScripts(BuildTarget.StandaloneWindows64,ProjectBuilder.ResolveBuildOutputPath("../Builds/GolfPolishQA/Player/WhatTheFish.exe"),BuildOptions.Development|BuildOptions.CompressWithLz4HC);if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new Exception("Script refresh failed");}
}
