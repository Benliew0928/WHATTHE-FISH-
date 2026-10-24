using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
namespace WhatTheFish {
 // Editable 3D interpretation of the supplied tornado reference sheet. The
 // synchronized event age drives growth/morphs; no collision or force lives here.
 public sealed class GolfTornadoFX:MonoBehaviour {
  public const float GrowthSeconds=GolfHazardRules.TornadoGrowthSeconds;
  const int Sides=40,Rows=40;
  Batch funnel,details,ribbons,fog,sediment,groundWind;int lane;
  ulong sandTrail;double sandTime,clock;float growth;readonly float[] groundHeights=new float[8];float nextGround;
  float width,bend,phase;bool reduced;
  static readonly float[] Profile=BuildProfile();
  static readonly Vector3[] PuffNormals=BuildPuffNormals();
  static readonly Color WindColor=new Color(.79f,.88f,1,.62f).linear;
  static readonly float[] Width={1,.92f,1.05f,.90f},Bend={.15f,.65f,-.4f,.82f},Tilt={-3,7,-9,11};
  static readonly Vector3[] Ico={new(-1,1.618034f,0),new(1,1.618034f,0),new(-1,-1.618034f,0),new(1,-1.618034f,0),new(0,-1,1.618034f),new(0,1,1.618034f),new(0,-1,-1.618034f),new(0,1,-1.618034f),new(1.618034f,0,-1),new(1.618034f,0,1),new(-1.618034f,0,-1),new(-1.618034f,0,1)};
  static readonly int[] Faces={0,11,5,0,5,1,0,1,7,0,7,10,0,10,11,1,5,9,5,11,4,11,10,2,10,7,6,7,1,8,3,9,4,3,4,2,3,2,6,3,6,8,3,8,9,4,9,5,2,4,11,6,2,10,8,6,7,9,8,1};
  public static GolfTornadoFX Create(Transform parent,int lane=0){var node=new GameObject("Reference sculpted tornado");node.transform.SetParent(parent,false);var fx=node.AddComponent<GolfTornadoFX>();fx.lane=lane;fx.Build();return fx;}
  void Build(){var material=Resources.Load<Material>("GolfEvents/StormVolume");funnel=new Batch(transform,"Rolled hollow funnel",material,3000);details=new Batch(transform,"Orbiting leaves rocks and dust",material,5000);ribbons=new Batch(transform,"White helical wind ribbons",Resources.Load<Material>("GolfBallTrail"),600);fog=new Batch(transform,"Soft evolving ground haze",material,1600);sediment=new Batch(transform,"Rising entrained sand",material,5000);groundWind=new Batch(transform,"Inward ground wind circles",Resources.Load<Material>("GolfBallTrail"),1600);}
  Vector3 Centre(float t)=>new Vector3(bend*t*t+.12f*Mathf.Sin(t*3+phase*.19f)*t,0,.16f*Mathf.Sin(t*2.2f+phase*.13f)*t);
  static float[] BuildProfile(){var values=new float[257];for(int i=0;i<values.Length;i++)values[i]=Mathf.Pow(i/256f,1.12f);return values;}
  static float ProfileRadius(float t){float f=Mathf.Clamp01(t)*256;int i=Mathf.Min(255,(int)f);return Mathf.Lerp(Profile[i],Profile[i+1],f-i);}
  static Vector3[] BuildPuffNormals(){var values=new Vector3[8*13];for(int y=0;y<=7;y++)for(int x=0;x<=12;x++){float a=x*Mathf.PI*2/12,t=y*Mathf.PI/7;values[y*13+x]=new Vector3(Mathf.Sin(t)*Mathf.Cos(a),Mathf.Cos(t),Mathf.Sin(t)*Mathf.Sin(a));}return values;}
  float Radius(float t,float a)=>width*(.10f+2.02f*ProfileRadius(t))*(1+.13f*Mathf.Cos(a+t*31-phase*2.3f)+.025f*Mathf.Sin(a*3+t*8+phase));
  Vector3 Surface(float t,float a){float r=Radius(t,a),t2=t*t,t4=t2*t2;return Centre(t)+new Vector3(Mathf.Cos(a)*r,t*8+t4*t4*.12f*Mathf.Cos(a-phase*.4f),Mathf.Sin(a)*r);}
  public void Tick(double seconds,ulong history=0,double sampledAt=0,double now=0,float opacity=1){
   if(funnel==null)Build();gameObject.SetActive(true);reduced=MenuPreferences.ReducedMotion;
   float age=Mathf.Max(0,(float)seconds);growth=GolfHazardRules.TornadoGrowth(age);sandTrail=history;sandTime=sampledAt;clock=now;
   float style=reduced?lane%4:age/6+lane*.37f;int first=Mathf.FloorToInt(style)%4,next=(first+1)%4;float blend=Mathf.SmoothStep(0,1,Mathf.Repeat(style,1));
   width=Mathf.Lerp(Width[first],Width[next],blend);bend=Mathf.Lerp(Bend[first],Bend[next],blend);phase=reduced?lane*.7f:age+lane*1.73f;
   transform.localScale=new Vector3(1.55f,1.15f,1.55f)*growth;transform.localRotation=Quaternion.Euler(0,0,Mathf.Lerp(Tilt[first],Tilt[next],blend));
   funnel.Opacity(.5f*opacity,0,phase);details.Opacity(.4f*opacity,0,phase);ribbons.Opacity(.4f*opacity,0,phase);fog.Opacity(.4f*opacity,1,phase);sediment.Opacity(.4f*opacity,.55f,phase);groundWind.Opacity(.4f*opacity,0,phase);
   SculptFunnel();SculptDetails();SculptRibbons();SculptFog();SculptSand();SculptGroundWind();
  }
  void SculptFunnel(){
   funnel.Clear();
   for(int y=0;y<=Rows;y++)for(int x=0;x<=Sides;x++){
    float t=y/(float)Rows,a=x*Mathf.PI*2/Sides,ridge=.5f+.5f*Mathf.Cos(a+t*31-phase*2.3f);
    var color=Color.Lerp(new Color(.37f,.40f,.48f),new Color(.69f,.72f,.79f),Mathf.SmoothStep(.1f,.95f,ridge));
    color=Color.Lerp(color,new Color(.72f,.75f,.81f),t*.18f);
    float lower=Mathf.Max(0,t-.003f),upper=Mathf.Min(1,t+.003f),slope=(Radius(upper,a)-Radius(lower,a))/((upper-lower)*8);
    funnel.Vertex(Surface(t,a),new Vector3(Mathf.Cos(a),-slope,Mathf.Sin(a)).normalized,color,new Vector2(x/(float)Sides,t));
   }
   funnel.Grid(0,Rows,Sides,true);
   // A faint recessed wisp suggests depth without an opaque inner shell.
   int start=funnel.Count;const int inner=10;
   for(int y=0;y<=inner;y++)for(int x=0;x<=Sides;x++){
    float f=y/(float)inner,t=1-f*.32f,a=x*Mathf.PI*2/Sides;float r=Radius(1,a)*Mathf.Lerp(.90f,.20f,f);
    var p=Centre(t)+new Vector3(Mathf.Cos(a)*r,8-f*2.8f+Mathf.Pow(1-f,4)*.12f*Mathf.Cos(a-phase*.4f),Mathf.Sin(a)*r);
    funnel.Vertex(p,new Vector3(-Mathf.Cos(a),.25f,-Mathf.Sin(a)).normalized,Color.Lerp(new Color(.47f,.50f,.58f,.16f),new Color(.37f,.40f,.49f,0),f),Vector2.zero);
   }
   funnel.Grid(start,inner,Sides,true);
   // Thick rolled rim, matching the pale rounded lip in the supplied sheet.
   start=funnel.Count;const int tube=10;
   for(int y=0;y<=tube;y++)for(int x=0;x<=Sides;x++){
    float a=x*Mathf.PI*2/Sides,b=y*Mathf.PI*2/tube;var radial=new Vector3(Mathf.Cos(a),0,Mathf.Sin(a));float r=Radius(1,a)*.96f;
    var normal=(radial*Mathf.Cos(b)+Vector3.up*Mathf.Sin(b)).normalized;
    funnel.Vertex(Centre(1)+radial*r+normal*.16f+Vector3.up*(8+.12f*Mathf.Cos(a-phase*.4f)),normal,new Color(.72f,.75f,.82f),Vector2.zero);
   }
   funnel.Grid(start,tube,Sides,true);funnel.Upload();
  }
  void SculptRibbons(){
   ribbons.Clear();const int segments=120;
   for(int band=0;band<2;band++){
    int start=ribbons.Count;
    for(int i=0;i<=segments;i++){
     float f=i/(float)segments,t=Mathf.Lerp(.06f,1.02f,f),a=f*Mathf.PI*(band==0?6.8f:4.8f)-phase*1.8f+band*2.6f;
     float r=Radius(Mathf.Min(1,t),a)+.16f+band*.14f;var radial=new Vector3(Mathf.Cos(a),0,Mathf.Sin(a));
     var p=Centre(t)+radial*r+Vector3.up*(t*8+.09f*Mathf.Sin(a));
     float taper=Mathf.Pow(Mathf.Sin(f*Mathf.PI),.5f),size=(.14f+.20f*t)*taper;
     var color=WindColor;color.a*=taper;
     for(int side=0;side<2;side++)ribbons.Vertex(p+Vector3.up*((side==0?-1:1)*size),radial,color,new Vector2(f,side));
    }
    for(int i=0;i<segments;i++)ribbons.Quad(start+i*2,start+i*2+1,start+i*2+2,start+i*2+3);
   }
   ribbons.Upload();
  }
  void SculptDetails(){
   details.Clear();
   for(int i=0;i<12;i++){
    float life=Mathf.Repeat(i*.618034f+phase*.14f,1),t=.02f+life*.88f,a=i*2.399963f+phase*(1.4f+i%3*.16f)+life*7,r=Mathf.Lerp(5.8f,Radius(t,a)+.25f,Mathf.SmoothStep(0,1,life));
    var p=Centre(t)+new Vector3(Mathf.Cos(a)*r,t*7.7f,Mathf.Sin(a)*r);float size=(.10f+(i%4)*.055f)*Mathf.Pow(Mathf.Max(0,Mathf.Sin(life*Mathf.PI)),.35f);
    Rock(details,p,Quaternion.Euler(a*37,a*55,i*31+phase*45),new Vector3(size,size*(.8f+i%2*.25f),size*.8f),new Color(.48f,.47f,.44f));
   }
   for(int i=0;i<16;i++){
    float life=Mathf.Repeat(i*.381966f+phase*.18f,1),t=.02f+life*.88f,a=i*2.399963f+phase*1.9f+life*8,r=Mathf.Lerp(6.5f,Radius(t,a)+.3f,Mathf.SmoothStep(0,1,life));
    var p=Centre(t)+new Vector3(Mathf.Cos(a)*r,t*7.9f,Mathf.Sin(a)*r);
    Leaf(details,p,Quaternion.Euler(20+Mathf.Sin(a)*35,-a*Mathf.Rad2Deg+70,Mathf.Sin(a*2)*45),(.55f+(i%4)*.11f)*Mathf.Pow(Mathf.Max(0,Mathf.Sin(life*Mathf.PI)),.35f));
   }
   details.Upload();
  }
  static void Leaf(Batch b,Vector3 p,Quaternion q,float size){
   Vector3[] points=LeafPoints;
   for(int i=0;i<4;i++){
    var a=p+q*(points[i]*size);var c=p+q*(points[(i+1)%4]*size);var centre=p+q*(new Vector3(0,.075f,0)*size);
    var tint=i<2?new Color(.39f,.72f,.035f):new Color(.24f,.55f,.025f);b.Triangle(a,centre,c,tint);b.Triangle(c,centre,a,tint);
   }
   var tip=p+q*(Vector3.right*.34f*size);var basePoint=p-q*(Vector3.right*.34f*size);var mid=p+q*(Vector3.up*.08f*size);
   b.Triangle(basePoint,mid+q*(Vector3.forward*.012f*size),tip,new Color(.65f,.83f,.11f));
  }
  static readonly Vector3[] LeafPoints={new(-.42f,0,0),new(0,0,.18f),new(.42f,0,0),new(0,0,-.18f)};
  static void Rock(Batch b,Vector3 p,Quaternion q,Vector3 scale,Color color){for(int i=0;i<Faces.Length;i+=3)b.Triangle(p+q*Vector3.Scale((Ico[Faces[i]]*.525731f),scale),p+q*Vector3.Scale((Ico[Faces[i+1]]*.525731f),scale),p+q*Vector3.Scale((Ico[Faces[i+2]]*.525731f),scale),color);}
  static void Sphere(Batch b,Vector3 p,Vector3 scale,Color color){
   const int sides=12,rows=7;int start=b.Count;
   for(int y=0;y<=rows;y++)for(int x=0;x<=sides;x++){
    var n=PuffNormals[y*(sides+1)+x];b.Vertex(p+Vector3.Scale(n,scale),n,color,Vector2.zero);
   }b.Grid(start,rows,sides,false);
  }
  // A grain at height t samples the sand source from t*2.8 seconds ago.
  // Entering and leaving sand therefore propagates upward, rather than toggling
  // the whole column or fading it from the top down.
  float SandAt(float t){float index=(float)((sandTime-clock+t*2.8)/.1);if(index>=63)return 0;int i=Mathf.Max(0,Mathf.FloorToInt(index));float a=(sandTrail&(1UL<<i))!=0?1:0,b=(sandTrail&(1UL<<Mathf.Min(63,i+1)))!=0?1:0;return Mathf.Lerp(a,b,Mathf.Clamp01(index-i));}
  void SculptFog(){
   fog.Clear();
   for(int i=0;i<13;i++){float a=i*2.399963f+phase*.15f,r=.35f+(i%4)*.34f+.12f*Mathf.Sin(phase*.8f+i);float size=.38f+.24f*(.5f+.5f*Mathf.Sin(i*5.7f+phase*.9f));var p=FloorPoint(new Vector3(Mathf.Cos(a)*r,.2f+.15f*Mathf.Sin(i+phase*.7f),Mathf.Sin(a)*r));Sphere(fog,p,new Vector3(size*1.3f,size*.72f,size)*(1+.16f*Mathf.Sin(phase+i)),new Color(.73f,.74f,.73f));}fog.Upload();
  }
  void SculptSand(){
   sediment.Clear();
   for(int i=0;i<64;i++){float t=Mathf.Repeat(i*.618034f+phase*.21f,1),level=SandAt(t);if(level<.01f)continue;float a=i*2.399963f+phase*3+t*9,r=Mathf.Lerp(2.6f,Radius(t,a)+.08f,t);var p=Centre(t)+new Vector3(Mathf.Cos(a)*r,t*7.8f,Mathf.Sin(a)*r);float size=(.024f+(i%3)*.012f)*level;var color=new Color(.76f,.63f,.40f,level*.8f);Rock(sediment,p,Quaternion.Euler(phase*70,i*31,phase*40),Vector3.one*size,color);}
   for(int i=0;i<9;i++){float t=(i+.25f)/9,level=SandAt(t);if(level<.01f)continue;float a=phase*2.1f+i*2.399963f,r=Radius(t,a)*.88f;Sphere(sediment,Centre(t)+new Vector3(Mathf.Cos(a)*r,t*7.7f,Mathf.Sin(a)*r),Vector3.one*(.23f+.22f*t)*level,new Color(.73f,.61f,.42f,level*.34f));}sediment.Upload();
  }
  Vector3 FloorPoint(Vector3 p){
   if(Application.isPlaying&&Time.unscaledTime>=nextGround){nextGround=Time.unscaledTime+.2f;for(int i=0;i<8;i++){float a=i*Mathf.PI/4;var at=transform.parent.TransformPoint(new Vector3(Mathf.Cos(a)*7*growth*1.55f,0,Mathf.Sin(a)*7*growth*1.55f));groundHeights[i]=GolfCartMotor.Floor(at,out var hit)?hit.point.y-transform.parent.position.y:0;}}
   var world=transform.parent.TransformPoint(Vector3.Scale(p,new Vector3(1.55f,1.15f,1.55f)*growth));float f=Mathf.Repeat(Mathf.Atan2(p.z,p.x)/(Mathf.PI*2)*8,8);int index=(int)f;world.y+=Mathf.Lerp(groundHeights[index],groundHeights[(index+1)%8],f-index)*Mathf.Clamp01(new Vector2(p.x,p.z).magnitude/7);return transform.InverseTransformPoint(world);
  }
  void SculptGroundWind(){
   groundWind.Clear();const int segments=72;
   for(int ring=0;ring<5;ring++){int start=groundWind.Count;float life=Mathf.Repeat(phase*.24f+ring*.2f,1),radius=Mathf.Lerp(7,.6f,life);for(int i=0;i<=segments;i++){float f=i/(float)segments,a=f*Mathf.PI*(1.45f+ring*.08f)+phase*1.8f+ring*2.4f;float r=radius+.15f*Mathf.Sin(a*3+phase),w=(.12f+.12f*life)*Mathf.Sin(f*Mathf.PI);var radial=new Vector3(Mathf.Cos(a),0,Mathf.Sin(a));var color=WindColor;color.a=.7f*Mathf.Sin(life*Mathf.PI)*Mathf.Sin(f*Mathf.PI);for(int side=0;side<2;side++)groundWind.Vertex(FloorPoint(radial*(r+(side==0?-w:w))+Vector3.up*.055f),Vector3.up,color,new Vector2(f,side));}for(int i=0;i<segments;i++)groundWind.Quad(start+i*2,start+i*2+1,start+i*2+2,start+i*2+3);}groundWind.Upload();
  }
  public void Hide(){gameObject.SetActive(false);}
  void OnDestroy(){funnel?.Dispose();details?.Dispose();ribbons?.Dispose();fog?.Dispose();sediment?.Dispose();groundWind?.Dispose();}
  // Lists retain their capacity. One shared material and one mesh per layer,
  // with no runtime colliders, per-leaf objects or per-frame managed allocation.
  sealed class Batch {
   readonly Mesh mesh;readonly MeshRenderer renderer;readonly MaterialPropertyBlock block=new();readonly List<Vector3> v,n;readonly List<Color> c;readonly List<Vector2> uv;readonly List<int> tris;int uploaded=-1;
   public int Count=>v.Count;
   public Batch(Transform parent,string name,Material material,int capacity){mesh=new Mesh{name=name};mesh.MarkDynamic();v=new(capacity);n=new(capacity);c=new(capacity);uv=new(capacity);tris=new(capacity*6);var go=new GameObject(name);go.transform.SetParent(parent,false);go.AddComponent<MeshFilter>().sharedMesh=mesh;renderer=go.AddComponent<MeshRenderer>();renderer.sharedMaterial=material;renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;}
   public void Opacity(float opacity,float softness,float phase){block.SetFloat("_Opacity",opacity);block.SetFloat("_Softness",softness);block.SetFloat("_Phase",phase);block.SetFloat("_Fade",opacity);renderer.SetPropertyBlock(block);}
   public void Clear(){v.Clear();n.Clear();c.Clear();uv.Clear();tris.Clear();}
   public void Vertex(Vector3 p,Vector3 normal,Color color,Vector2 tex){v.Add(p);n.Add(normal);c.Add(color);uv.Add(tex);}
   public void Triangle(Vector3 a,Vector3 b,Vector3 d,Color color){int i=Count;var normal=Vector3.Cross(b-a,d-a).normalized;Vertex(a,normal,color,Vector2.zero);Vertex(b,normal,color,Vector2.zero);Vertex(d,normal,color,Vector2.zero);tris.Add(i);tris.Add(i+1);tris.Add(i+2);}
   public void Quad(int a,int b,int d,int e){tris.Add(a);tris.Add(b);tris.Add(d);tris.Add(d);tris.Add(b);tris.Add(e);}
   public void Grid(int start,int rows,int sides,bool reverse){for(int y=0;y<rows;y++)for(int x=0;x<sides;x++){int i=start+y*(sides+1)+x;if(reverse)Quad(i+1,i,i+sides+2,i+sides+1);else Quad(i,i+1,i+sides+1,i+sides+2);}}
   public void Upload(){if(mesh.vertexCount!=v.Count){mesh.Clear();uploaded=-1;}mesh.SetVertices(v);mesh.SetNormals(n);mesh.SetColors(c);mesh.SetUVs(0,uv);if(uploaded!=tris.Count){mesh.SetTriangles(tris,0,false);uploaded=tris.Count;}mesh.bounds=new Bounds(Vector3.up*4,new Vector3(18,16,18));}
   public void Dispose(){if(!mesh)return;if(Application.isPlaying)Destroy(mesh);else DestroyImmediate(mesh);}
  }
 }
}
