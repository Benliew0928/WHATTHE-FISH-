using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace WhatTheFish {
 // Local pooled presentation driven by the existing host-confirmed Cup/Finish
 // event. No flag, terrain, collider or scoring state is changed by this effect.
 public sealed class GolfCupCelebration:MonoBehaviour {
  const float Duration=2.6f;
  static readonly List<GolfCupCelebration> pool=new();
  readonly List<Vector3> vertices=new(3000);
  readonly List<Color> colors=new(3000);
  readonly List<int> triangles=new(6000);
  readonly List<Vector3> uv=new(3000);
  Mesh mesh;MeshRenderer draw;float born;bool playing;
  static readonly Color Gold=new(1,.84f,.18f),Aqua=new(.34f,.96f,1);
  public static int ActiveCount {get{pool.RemoveAll(p=>!p);int n=0;foreach(var p in pool)if(p.playing)n++;return n;}}
  public static void Play(Transform parent,Vector3 point){
   pool.RemoveAll(p=>!p);GolfCupCelebration effect=null;
   foreach(var p in pool)if(!p.playing){effect=p;break;}
   if(!effect&&pool.Count<4){var obj=new GameObject("Golf cup stars and spirals");obj.transform.SetParent(parent,false);effect=obj.AddComponent<GolfCupCelebration>();pool.Add(effect);}
   if(!effect){effect=pool[0];foreach(var p in pool)if(p.born<effect.born)effect=p;}
   effect.transform.position=point+Vector3.up*.025f;effect.born=Time.unscaledTime;effect.playing=true;effect.draw.enabled=true;
  }
  void Awake(){
   mesh=new Mesh{name="Pooled golf celebration"};mesh.MarkDynamic();gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;
   draw=gameObject.AddComponent<MeshRenderer>();draw.sharedMaterial=Resources.Load<Material>("GolfCelebration");draw.shadowCastingMode=ShadowCastingMode.Off;draw.receiveShadows=false;draw.enabled=false;
  }
  int Vertex(Vector3 p,Color c,Vector3 tex){int i=vertices.Count;vertices.Add(p);colors.Add(c);uv.Add(tex);return i;}
  void Tri(int a,int b,int c){triangles.Add(a);triangles.Add(b);triangles.Add(c);}
  static Color Alpha(Color c,float a){c.a*=a;return c;}
  void Quad(Vector3 a,Vector3 b,Vector3 c,Vector3 d,Color color,bool glow=false,bool strip=false){
   int i=vertices.Count;float mode=strip?2:glow?1:0;Vertex(a,color,new Vector3(0,0,mode));Vertex(b,color,new Vector3(1,0,mode));Vertex(c,color,new Vector3(1,1,mode));Vertex(d,color,new Vector3(0,1,mode));Tri(i,i+1,i+2);Tri(i,i+2,i+3);
  }
  void Disc(Vector3 p,Vector3 right,Vector3 up,float radius,Color color,int points=24,float inner=0){
   for(int j=0;j<points;j++){
    float a=j*Mathf.PI*2/points,b=(j+1)*Mathf.PI*2/points;
    var x=right*Mathf.Cos(a)+up*Mathf.Sin(a);var y=right*Mathf.Cos(b)+up*Mathf.Sin(b);
    Quad(p+x*inner,p+x*radius,p+y*radius,p+y*inner,color);
   }
  }
  void Star(Vector3 p,Vector3 right,Vector3 up,float radius,float spin,Color color){
   int centre=Vertex(p,color,Vector3.zero);
   for(int j=0;j<10;j++){
    float a=spin+j*Mathf.PI/5,b=spin+(j+1)*Mathf.PI/5;
    float ra=radius*(j%2==0?1:.48f),rb=radius*(j%2==0?.48f:1);
    Tri(centre,Vertex(p+(right*Mathf.Sin(a)+up*Mathf.Cos(a))*ra,color,Vector3.zero),Vertex(p+(right*Mathf.Sin(b)+up*Mathf.Cos(b))*rb,color,Vector3.zero));
   }
  }
  void LateUpdate(){
   if(!playing)return;float age=Time.unscaledTime-born,t=age/Duration;
   var match=GolfMatchManager.Instance;if(t>=1||!match||!match.Context){playing=false;draw.enabled=false;return;}
   var cam=Camera.main;if(!cam)return;
   float fade=Mathf.SmoothStep(0,1,Mathf.Clamp01(age/.13f))*Mathf.Clamp01((1-t)/.3f);
   float grow=MenuPreferences.ReducedMotion?1:Mathf.SmoothStep(.12f,1,Mathf.Clamp01(age/.35f));
   var right=transform.InverseTransformDirection(cam.transform.right);var up=transform.InverseTransformDirection(cam.transform.up);
   vertices.Clear();colors.Clear();triangles.Clear();uv.Clear();
   // Two continuous rising, gently expanding helical ribbons.
   for(int strand=0;strand<2;strand++){
    Vector3 last=Vector3.zero,lastSide=Vector3.zero;
    for(int j=0;j<=128;j++){
     float f=j/128f,angle=f*Mathf.PI*3.6f+age*2+strand*Mathf.PI;
     float radius=(.14f+.55f*Mathf.Sin(f*Mathf.PI*.7f))*grow;
     var p=new Vector3(Mathf.Cos(angle)*radius,(.1f+f*1.65f)*grow,Mathf.Sin(angle)*radius);
     float dr=.55f*Mathf.Cos(f*Mathf.PI*.7f)*Mathf.PI*.7f*grow;
     var tangent=new Vector3(Mathf.Cos(angle)*dr-Mathf.Sin(angle)*radius*Mathf.PI*3.6f,1.65f*grow,Mathf.Sin(angle)*dr+Mathf.Cos(angle)*radius*Mathf.PI*3.6f);
     var toward=transform.InverseTransformDirection(cam.transform.position-transform.TransformPoint(p));
     var side=Vector3.Cross(tangent,toward).normalized*.026f;
     if(j>0){var color=Alpha(strand==0?Gold:Aqua,fade*Mathf.Sin(f*Mathf.PI));Quad(last-lastSide*5,last+lastSide*5,p+side*5,p-side*5,Alpha(color,.24f),false,true);Quad(last-lastSide,last+lastSide,p+side,p-side,color,false,true);Quad(last-lastSide*.28f,last+lastSide*.28f,p+side*.28f,p-side*.28f,Alpha(Color.white,color.a));}
     last=p;lastSide=side;
    }
   }
   // Converging gold/white rays rising out of the existing hole.
   for(int j=0;j<9;j++){
    float angle=j*2.39996f;var top=new Vector3(Mathf.Cos(angle)*.5f,1.5f+(j%3)*.26f,Mathf.Sin(angle)*.5f)*grow;
    var width=right*(.009f+(j%2)*.007f);Quad(-width*5,width*5,top+width,top-width,Alpha(Gold,fade*.2f),false,true);Quad(-width,width,top+width*.2f,top-width*.2f,Alpha(j%2==0?Gold:Color.white,fade*.75f));
   }
   Disc(Vector3.zero,Vector3.right,Vector3.forward,.28f*grow,Alpha(Aqua,fade*.6f),48,.24f*grow);
   // Rounded-looking white bordered golden stars, aqua soap bubbles and glints.
   for(int j=0;j<7;j++){
    float a=j*2.39996f+age*.28f;var p=new Vector3(Mathf.Cos(a)*(.62f+j%2*.23f),.3f+j*.19f+Mathf.Sin(age*2+j)*.05f,Mathf.Sin(a)*.6f)*grow;
    float radius=.115f+(j%3)*.018f;float spin=Mathf.Sin(j)*.25f;
    Quad(p-right*radius*2-up*radius*2,p+right*radius*2-up*radius*2,p+right*radius*2+up*radius*2,p-right*radius*2+up*radius*2,Alpha(Gold,fade*.22f),true);
    Star(p,right,up,radius,spin,Alpha(Color.white,fade));Star(p,right,up,radius*.82f,spin,Alpha(Gold,fade));
   }
   for(int j=0;j<6;j++){
    float a=j*2.4f+1;var p=new Vector3(Mathf.Cos(a)*.82f,.25f+Mathf.Repeat(j*.26f+age*.28f,1.5f),Mathf.Sin(a)*.74f)*grow;float r=.055f+j%3*.009f;
    Disc(p,right,up,r,Alpha(Aqua,fade*.16f));Disc(p,right,up,r,Alpha(Color.white,fade*.9f),24,r*.8f);
    Disc(p+(-right+up)*r*.36f,right,up,r*.2f,Alpha(Color.white,fade));
   }
   for(int j=0;j<18;j++){
    float a=j*2.39996f;var p=new Vector3(Mathf.Cos(a)*(j%2==0?.45f:.95f),.12f+Mathf.Repeat(j*.19f+age*.2f,1.7f),Mathf.Sin(a)*.8f)*grow;
    float size=(.018f+Mathf.Sin(age*5+j)*.009f)*grow;
    if(j%3==0)Disc(p,right*.55f,up,size*2,Alpha(j%2==0?Gold:Aqua,fade),12);
    else{Star(p,right,up,size*1.7f,0,Alpha(Color.white,fade));Quad(p-right*size*4-up*size*4,p+right*size*4-up*size*4,p+right*size*4+up*size*4,p-right*size*4+up*size*4,Alpha(Aqua,fade*.3f),true);}
   }
   mesh.Clear();mesh.SetVertices(vertices);mesh.SetColors(colors);mesh.SetUVs(0,uv);mesh.SetTriangles(triangles,0);mesh.RecalculateBounds();
  }
  void OnDestroy(){pool.Remove(this);if(mesh)Destroy(mesh);}
 }
}
