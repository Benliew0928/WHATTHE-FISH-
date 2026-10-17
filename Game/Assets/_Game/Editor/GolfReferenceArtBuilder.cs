using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// Deterministic delivery extraction from the approved screenshot, never a redraw.
// The editable source remains outside Assets; only independent UI sprites ship.
public static class GolfReferenceArtBuilder {
 const string Folder="Assets/_Game/Resources/GolfUI/Reference/";
 static Texture2D source;
 [Serializable] sealed class Record {public string path,sha256;public int width,height,pngBytes,androidAstc4x4Bytes;}
 [Serializable] sealed class Audit {public string basePath="repository root",source="ArtSource/Golf/UI/ApprovedHUD-reference.png",sourceSha256,method="Original pixel crops with traced alpha contours, clean live-caption interiors and small original default-caption overlays. No screenshot background ships.",sizeNote="PNG and raw ASTC bytes are not measured APK growth.";public Record[] assets;public int totalPngBytes,totalAndroidAstc4x4Bytes;}
 static readonly List<Record> records=new();
 static string Root=>Path.GetFullPath(Path.Combine(Application.dataPath,"../.."));
 static string Absolute(string asset)=>Path.Combine(Application.dataPath,asset.Substring("Assets/".Length));
 static string Hash(byte[] bytes){using(var sha=System.Security.Cryptography.SHA256.Create())return BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-","");}
 static Color Pixel(int x,int y)=>source.GetPixel(Mathf.Clamp(x,0,source.width-1),source.height-1-Mathf.Clamp(y,0,source.height-1));
 struct Clear {public RectInt rect;public int top,bottom;public Clear(int x,int y,int w,int h,int a,int b){rect=new RectInt(x,y,w,h);top=a;bottom=b;}}
 static Vector2[] P(params float[] p){var v=new Vector2[p.Length/2];for(int i=0;i<v.Length;i++)v[i]=new Vector2(p[i*2],p[i*2+1]);return v;}
 static float Polygon(Vector2 p,Vector2[] poly){bool inside=false;float distance=float.MaxValue;for(int i=0,j=poly.Length-1;i<poly.Length;j=i++){
  var a=poly[j];var b=poly[i];var d=b-a;float t=Mathf.Clamp01(Vector2.Dot(p-a,d)/Mathf.Max(.001f,d.sqrMagnitude));distance=Mathf.Min(distance,Vector2.Distance(p,a+d*t));
  if((a.y>p.y)!=(b.y>p.y)&&p.x<(b.x-a.x)*(p.y-a.y)/(b.y-a.y)+a.x)inside=!inside;
 }return Mathf.Clamp01(.5f+(inside?distance:-distance));}
 static float Round(Vector2 p,Rect r,float radius){var q=new Vector2(Mathf.Abs(p.x-r.center.x),Mathf.Abs(p.y-r.center.y))-(r.size*.5f-Vector2.one*radius);float d=new Vector2(Mathf.Max(q.x,0),Mathf.Max(q.y,0)).magnitude+Mathf.Min(Mathf.Max(q.x,q.y),0)-radius;return Mathf.Clamp01(.5f-d);}
 static float Ellipse(Vector2 p,Vector2 c,Vector2 r)=>Mathf.Clamp01(.5f+(1-new Vector2((p.x-c.x)/r.x,(p.y-c.y)/r.y).magnitude)*Mathf.Min(r.x,r.y));
 static void Extract(string name,RectInt crop,Func<Vector2,float> mask,Vector4 border,params Clear[] clears){
  var tex=new Texture2D(crop.width,crop.height,TextureFormat.RGBA32,false);var pixels=new Color[crop.width*crop.height];
  for(int y=0;y<crop.height;y++)for(int x=0;x<crop.width;x++){
   int sx=crop.x+x,sy=crop.y+y;Color c=Pixel(sx,sy);
   foreach(var clear in clears)if(clear.rect.Contains(new Vector2Int(sx,sy)))c=Color.Lerp(Pixel(sx,clear.top),Pixel(sx,clear.bottom),Mathf.InverseLerp(clear.top,clear.bottom,sy));
   // Blank caption interiors follow the original vertical colour gradient.
   // Sampling a single clean strip avoids stretching letter shadows into bars.
   if(name=="Aim"&&sx>=1078&&sx<1176&&sy>=864&&sy<946){float edge=Mathf.Clamp01(Mathf.Min(Mathf.Min(sx-1078,1175-sx),Mathf.Min(sy-864,945-sy))/8f);c=Color.Lerp(Pixel(sx,sy),Pixel(1076,sy),Mathf.SmoothStep(0,1,edge));}
   if(name=="Buggy"&&sx>=1107&&sx<1237&&sy>=772&&sy<811)c=Color.Lerp(Pixel(1148,767),Pixel(1148,816),Mathf.InverseLerp(767,816,sy));
   if(name=="Shot"&&sy>=934){float feather=Mathf.SmoothStep(0,1,Mathf.InverseLerp(934,950,sy))*Ellipse(new Vector2(sx+.5f,sy+.5f),new Vector2(1322,919),new Vector2(96,99));c=Color.Lerp(c,Color.Lerp(Pixel(1322,934),Pixel(1322,1007),Mathf.InverseLerp(934,1007,sy)),feather);}
   if(name=="Ball"&&sx>=846&&sx<1024&&sy>=580&&sy<611)c=Color.Lerp(Pixel(940,577),Pixel(940,614),Mathf.InverseLerp(577,614,sy));
   if(name=="Hint"&&sx>=620&&sx<902&&sy>=999&&sy<1034)c=Color.Lerp(Pixel(sx,sy),Color.Lerp(Pixel(760,992),Pixel(760,1037),Mathf.InverseLerp(992,1037,sy)),Round(new Vector2(sx+.5f,sy+.5f),new Rect(549,991,352,49),23));
   if(name=="Leave"&&sx>=1020&&sx<1188&&sy>=997&&sy<1034)c=Color.Lerp(Pixel(1100,992),Pixel(1100,1036),Mathf.InverseLerp(992,1036,sy));
   if(name=="Hole"&&sx>=664&&sx<845&&sy>=134&&sy<166)c=Color.Lerp(Pixel(760,129),Pixel(760,169),Mathf.InverseLerp(129,169,sy));
   c.a=mask(new Vector2(sx+.5f,sy+.5f));pixels[(crop.height-1-y)*crop.width+x]=c;
  }
  tex.SetPixels(pixels);tex.Apply();string path=Folder+name+".png",file=Absolute(path);var bytes=tex.EncodeToPNG();if(!File.Exists(file)||!System.Linq.Enumerable.SequenceEqual(File.ReadAllBytes(file),bytes))File.WriteAllBytes(file,bytes);UnityEngine.Object.DestroyImmediate(tex);
  records.Add(new Record{path="Game/"+path,sha256=Hash(bytes),width=crop.width,height=crop.height,pngBytes=bytes.Length,androidAstc4x4Bytes=((crop.width+3)/4)*((crop.height+3)/4)*16});
  AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceUpdate);var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.spriteBorder=border;importer.SaveAndReimport();
 }
 public static void Prepare(){
  records.Clear();Directory.CreateDirectory(Absolute(Folder));var reference=File.ReadAllBytes(Path.Combine(Root,"ArtSource/Golf/UI/ApprovedHUD-reference.png"));source=new Texture2D(2,2,TextureFormat.RGBA32,false);source.LoadImage(reference);
  try{
   Extract("Course",new RectInt(14,17,452,130),p=>Polygon(p,P(76,22,408,22,435,29,452,45,459,68,459,101,451,120,433,133,410,137,63,137,38,132,25,120,20,101,20,64,31,41,48,29)),Vector4.zero,new Clear(170,86,247,26,82,116));
   // Leaderboard art is derived separately from its supplied master.
   Extract("Hole",new RectInt(574,85,295,106),p=>Mathf.Max(Round(p,new Rect(579,119,283,64),32),Polygon(p,P(608,160,611,98,615,90,627,87,633,94,654,98,660,96,666,104,661,116,652,123,633,120,633,135,650,143,646,158,628,166))),Vector4.zero,new Clear(666,135,178,28,129,168));
   Extract("Arrow",new RectInt(692,52,67,70),p=>Polygon(p,P(697,88,715,65,724,56,731,57,751,80,754,90,749,96,738,98,738,110,735,116,715,116,710,111,711,98,699,98,695,94)),Vector4.zero);
   Extract("Ball",new RectInt(779,562,263,67),p=>Round(p,new Rect(784,566,254,59),30),new Vector4(62,28,34,28));
   Extract("BallArrow",new RectInt(795,626,47,41),p=>Polygon(p,P(800,640,800,634,805,629,811,629,817,633,821,629,830,628,838,633,838,640,823,659,819,663,814,660)),Vector4.zero);
   Extract("Aim",new RectInt(960,843,257,123),p=>Round(p,new Rect(966,851,243,107),53),new Vector4(79,48,50,48),new Clear(1085,871,73,68,866,942));
   Extract("Buggy",new RectInt(1009,745,246,94),p=>Mathf.Max(Round(p,new Rect(1015,752,232,81),40),Polygon(p,P(1013,801,1025,802,1037,811,1045,830,1031,829,1019,818))),new Vector4(98,35,30,35),new Clear(1108,776,125,36,767,815));
   Extract("Jump",new RectInt(1280,592,134,106),p=>Round(p,new Rect(1286,598,120,93),30),Vector4.zero);
   Extract("Camera",new RectInt(1280,691,132,92),p=>Round(p,new Rect(1286,698,120,79),27),Vector4.zero);
   Extract("Leave",new RectInt(932,974,274,81),p=>Round(p,new Rect(938,979,262,71),32),new Vector4(75,32,28,32),new Clear(1022,1000,165,32,992,1036));
   Extract("Hint",new RectInt(530,973,390,84),p=>Round(p,new Rect(536,980,378,70),32),new Vector4(85,32,30,32),new Clear(621,1000,281,33,992,1037));
   Extract("Utility",new RectInt(1280,691,132,92),p=>Round(p,new Rect(1286,698,120,79),27),new Vector4(30,32,30,32),new Clear(1307,715,75,49,711,765));
   Extract("Shot",new RectInt(1199,804,242,227),p=>Mathf.Max(Ellipse(p,new Vector2(1322,919),new Vector2(104,108)),Mathf.Max(Polygon(p,P(1207,945,1231,951,1235,979,1287,1007,1376,1005,1381,949,1407,943,1434,950,1424,976,1435,986,1426,1000,1398,1014,1345,1027,1281,1027,1236,1006,1210,977,1205,961)),Ellipse(p,new Vector2(1396,990),new Vector2(17,17)))),Vector4.zero);
   // Small original caption patches keep the exact reference lettering for
   // default fixed captions, while other states use the clean plate/live text.
   Extract("AimCaption",new RectInt(1072,864,104,83),p=>1,Vector4.zero);
   Extract("BuggyCaption",new RectInt(1107,772,130,39),p=>1,Vector4.zero);
   Extract("ShotCaption",new RectInt(1248,949,144,46),p=>1,Vector4.zero);
   Extract("HintCaption",new RectInt(620,999,282,35),p=>1,Vector4.zero);
   Extract("LeaveCaption",new RectInt(1020,997,168,37),p=>1,Vector4.zero);
   var audit=new Audit{sourceSha256=Hash(reference),assets=records.ToArray()};foreach(var record in records){audit.totalPngBytes+=record.pngBytes;audit.totalAndroidAstc4x4Bytes+=record.androidAstc4x4Bytes;}
   File.WriteAllText(Path.Combine(Root,"ArtSource/Golf/UI/reference-delivery-audit.json"),JsonUtility.ToJson(audit,true)+"\n");
   Debug.Log("GOLF_REFERENCE_ART_READY independent sprites; original UI pixels, dynamic captions cleared; no background scene ships");
  }finally{UnityEngine.Object.DestroyImmediate(source);source=null;}
 }
 public static void PrepareAndBuild(){Prepare();GolfHUDReviewBuild.BuildCandidate();}
}
