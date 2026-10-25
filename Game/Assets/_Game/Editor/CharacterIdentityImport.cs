using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// Derive compact region/number coordinates without changing topology, UV0,
// skin weights, normals, bone paths or the GolfRightGrip blend shape.
public sealed class CharacterIdentityImport:AssetPostprocessor {
 public override uint GetVersion()=>15;
 public override int GetPostprocessOrder()=>100;
 void OnPostprocessModel(GameObject root){
  if(assetPath!="Assets/_Game/Art/RainbowSprinter.fbx")return;
  var texture=new Texture2D(2,2,TextureFormat.RGB24,false);texture.LoadImage(File.ReadAllBytes("Assets/_Game/Art/RainbowSprinter/albedo.png"));
  foreach(var renderer in root.GetComponentsInChildren<SkinnedMeshRenderer>(true)){
   var mesh=renderer.sharedMesh;var vertices=mesh.vertices;var uv=mesh.uv;var normals=mesh.normals;
   var colors=new Color32[vertices.Length];var numbers=new List<Vector4>(vertices.Length);var headRegion=new bool[vertices.Length];int hairCount=0,clothCount=0;
   for(int i=0;i<vertices.Length;i++){
    Vector3 p=renderer.transform.TransformPoint(vertices[i]),n=renderer.transform.TransformDirection(normals[i]);var c=texture.GetPixelBilinear(uv[i].x,uv[i].y);
    bool skin=c.r>c.g*1.10f&&c.g>c.b*1.15f&&c.g>.12f&&c.r-c.b>.15f;
    bool protectFace=p.z>.20f&&Mathf.Abs(p.x)<.235f&&p.y<1.305f;
    // The texture has dark ear creases and baked hair shadows. They are still
    // skin: classifying those vertices by color alone punches holes in the ears.
    bool ear=Mathf.Abs(p.x)>.225f&&p.y>1.025f&&p.y<1.20f&&p.z>.005f&&p.z<.20f;
    bool lowHair=p.y>.95f&&p.z<.07f&&c.g>c.r*1.15f&&c.b>c.r*1.15f;
    bool hair=p.y>1.305f||p.y>1.20f&&!protectFace||p.y>1.015f&&!skin&&!protectFace&&!ear||lowHair;
    bool cloth=p.y>.445f&&p.y<.96f&&(p.y>.77f||Mathf.Abs(p.x)<.195f)&&!skin;
    bool accent=p.y<.355f&&!skin&&Mathf.Max(c.r,Mathf.Max(c.g,c.b))-Mathf.Min(c.r,Mathf.Min(c.g,c.b))>.18f;
    bool fringe=protectFace&&p.y>1.235f;
    colors[i]=new Color32((byte)(cloth?255:0),(byte)(hair?255:fringe?64:0),(byte)((skin||ear)&&!hair?255:0),(byte)(accent?255:0));
    bool front=p.z>.105f;float width=front?.19f:.25f,height=front?.15f:.19f;
    // Sub-millimetre number coordinates compress well without changing the body mesh.
    float numberU=(front?-p.x:p.x)/width+.5f,numberV=(p.y-.665f)/height;
    bool sideHead=Mathf.Abs(p.x)>.21f&&p.y>1.015f&&p.y<1.235f&&p.z<.23f;
    // Heads cannot carry a jersey number, so the unused negative facing value
    // carries the ear mask; the fourth coordinate is the local first-person mask.
    // The head's horizontal number coordinate and facing are never sampled.
    // Canonical zeros compress much better than thousands of unused values;
    // retain its vertical coordinate because the fringe shader uses height.
    headRegion[i]=p.y>1.015f;if(headRegion[i])numberU=0;
    // A dedicated rest-pose mask excludes the fused head/fringe/hair in the
    // local first-person color/depth passes, without changing skinning or shadows.
    // Include the grey underside of the oversized hair below the old face region.
    numbers.Add(new Vector4(Mathf.Round(numberU*1024)/1024,Mathf.Round(numberV*1024)/1024,sideHead?-1:p.y>1.015f?0:Mathf.Abs(n.z)>.55f?1:0,p.y>.94f?1:0));
    if(hair)hairCount++;if(cloth)clothCount++;
   }
   // Preserve height on every triangle that can interpolate into the fringe
   // mask. Elsewhere on the head, any value above the jersey rectangle is
   // equivalent; constant coordinates remove entropy from the packed mesh.
   var keepHeight=new bool[vertices.Length];var triangles=mesh.triangles;
   for(int t=0;t<triangles.Length;t+=3){int a=triangles[t],b=triangles[t+1],c=triangles[t+2];
    int minimum=Mathf.Min(colors[a].g,Mathf.Min(colors[b].g,colors[c].g)),maximum=Mathf.Max(colors[a].g,Mathf.Max(colors[b].g,colors[c].g));
    if(minimum<125&&maximum>25)keepHeight[a]=keepHeight[b]=keepHeight[c]=true;
   }
   for(int i=0;i<numbers.Count;i++)if(headRegion[i]&&!keepHeight[i]){var value=numbers[i];value.y=2;numbers[i]=value;}
   mesh.colors32=colors;mesh.SetUVs(2,numbers);
   if(hairCount<100||clothCount<100||mesh.GetBlendShapeIndex("GolfRightGrip")<0)throw new InvalidOperationException("Identity import regions/grip missing: "+mesh.name);
   Debug.Log($"IDENTITY_IMPORT {mesh.name} hair={hairCount} cloth={clothCount} vertices={mesh.vertexCount} grip preserved");
  }
  UnityEngine.Object.DestroyImmediate(texture);
 }
}
