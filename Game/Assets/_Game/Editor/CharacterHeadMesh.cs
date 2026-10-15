using System;
using System.Collections.Generic;
using UnityEngine;

// Editable, texture-free head pieces. Coordinates match the shared model's bind
// pose; export into head-bone space so every existing animation moves the joins.
public sealed class CharacterHeadMesh {
 readonly Transform head;
 readonly int sectors,rings;
 readonly List<Vector3> positions=new(),normals=new();
 readonly List<Color32> colors=new();
 readonly List<int> triangles=new();
 readonly List<int> triangleOwners=new();
 readonly List<Func<Vector3,bool>> solids=new();
 int solid=-1;
 readonly bool distant;
 public CharacterHeadMesh(Transform head,bool distant){this.head=head;this.distant=distant;sectors=distant?24:48;rings=distant?10:20;}
 int Vertex(Vector3 p,Vector3 n,Color color){int index=positions.Count;positions.Add(head.InverseTransformPoint(p));normals.Add(head.InverseTransformDirection(n.normalized));colors.Add(color);return index;}
 static Color Tint(float light=1,float accent=0)=>new(light,light,light,accent);
 void Triangle(int a,int b,int c){if(Vector3.Dot(Vector3.Cross(positions[b]-positions[a],positions[c]-positions[a]),normals[a]+normals[b]+normals[c])<0)(b,c)=(c,b);if(Vector3.Cross(positions[b]-positions[a],positions[c]-positions[a]).sqrMagnitude<1e-28f)return;triangles.Add(a);triangles.Add(b);triangles.Add(c);triangleOwners.Add(solid);}
 void Surface(Func<float,float,Vector3> point,int columns,int rows,Func<float,float,Color> color=null,bool invert=false){
  int start=positions.Count;
  for(int y=0;y<=rows;y++)for(int x=0;x<=columns;x++){
   float u=(float)x/columns,v=(float)y/rows;var p=point(u,v);
   var tangent=point(u+.0002f,v)-point(u-.0002f,v);var vertical=point(u,Mathf.Min(.9999f,v+.0002f))-point(u,Mathf.Max(.0001f,v-.0002f));
   var n=Vector3.Cross(tangent,vertical);float length=n.magnitude;n=length>1e-12f?n/length:(v<.5f?Vector3.up:Vector3.down);if(invert)n=-n;
   Vertex(p,n,color?.Invoke(u,v)??Tint());
  }
  for(int y=0;y<rows;y++)for(int x=0;x<columns;x++){int a=start+y*(columns+1)+x,b=a+columns+1;Triangle(a,b,a+1);Triangle(a+1,b,b+1);}
 }
 void Ball(Vector3 center,Vector3 size,float grooves=0,int lobes=8,float accent=0,bool small=false){
  solid=solids.Count;var inner=size*(1-grooves)*.975f;
  solids.Add(p=>{var q=p-center;return q.x*q.x/(inner.x*inner.x)+q.y*q.y/(inner.y*inner.y)+q.z*q.z/(inner.z*inner.z)<1;});
  Surface((u,v)=>{float phi=u*Mathf.PI*2,t=v*Mathf.PI;var q=new Vector3(Mathf.Cos(phi)*Mathf.Sin(t),Mathf.Cos(t),Mathf.Sin(phi)*Mathf.Sin(t));float ripple=1+grooves*Mathf.Cos(phi*lobes+v*3)*Mathf.Sin(t);return center+Vector3.Scale(q,size)*ripple;},small?(distant?10:16):sectors,small?(distant?6:10):rings,(u,v)=>Tint(1-.07f*Mathf.Pow(Mathf.Max(0,Mathf.Sin(u*Mathf.PI*2*lobes+v*3)),4),accent));
  solid=-1;
 }
 Vector3 CapPoint(float u,float v,int preset){
  float phi=u*Mathf.PI*2,front=(Mathf.Sin(phi)+1)*.5f;
  float edge=Mathf.Lerp(1.075f,1.275f,Mathf.Pow(front,.85f));
  if(preset==1)edge+=.008f*Mathf.Cos(phi*8);
  float height=preset==3?.325f:preset==4||preset==9?.24f:.29f;
  float theta=v*Mathf.Acos(Mathf.Clamp((edge-1.325f)/height,-.99f,.99f));
  float wave=(preset==8||preset==9)?0:.004f*Mathf.Cos(phi*(preset==4?18:24)+theta*3)*Mathf.Sin(theta);
  float y=height*Mathf.Cos(theta);if(preset==3&&y>0)y=Mathf.Min(y*1.16f,.285f);
  return new Vector3((.29f+wave)*Mathf.Sin(theta)*Mathf.Cos(phi),1.325f+y,.06f+(.285f+wave)*Mathf.Sin(theta)*Mathf.Sin(phi));
 }
 void Cap(int preset){
  solid=solids.Count;float height=preset==3?.325f:preset==4||preset==9?.24f:.29f;
  // Only the convex upper interior is eligible. The inset protects the lowest
  // detail level's polygon edges and never removes exposed silhouette faces.
  solids.Add(p=>{var q=p-new Vector3(0,1.325f,.06f);return p.y>1.32f&&p.y<1.595f&&q.x*q.x/(.272f*.272f)+q.y*q.y/(height*height*.9f)+q.z*q.z/(.267f*.267f)<1;});
  Surface((u,v)=>CapPoint(u,v,preset),sectors,rings,(u,v)=>Tint(preset>=8?1:1-.06f*Mathf.Pow(Mathf.Max(0,Mathf.Cos(u*Mathf.PI*48+v*3)),6)));solid=-1;
 }
 static Vector3 Bezier(Vector3 a,Vector3 b,Vector3 c,Vector3 d,float t){float s=1-t;return s*s*s*a+3*s*s*t*b+3*s*t*t*c+t*t*t*d;}
 void Lock(Vector3 a,Vector3 b,Vector3 c,Vector3 d,float width,float depth,float accent=0){
  int along=distant?8:16,around=distant?6:10,start=positions.Count;
  for(int i=0;i<=along;i++){
   float t=(float)i/along;var p=Bezier(a,b,c,d,t);var tangent=(Bezier(a,b,c,d,Mathf.Min(1,t+.001f))-Bezier(a,b,c,d,Mathf.Max(0,t-.001f))).normalized;
   var right=Vector3.Cross(tangent,Vector3.up).normalized;if(right.sqrMagnitude<.2f)right=Vector3.right;var up=Vector3.Cross(right,tangent).normalized;
   float taper=Mathf.Lerp(1,.025f,t*t*t);float baseRound=Mathf.Lerp(.3f,1,Mathf.Min(t*8,1));
   for(int j=0;j<=around;j++){float angle=j*Mathf.PI*2/around;var n=right*Mathf.Cos(angle)+up*Mathf.Sin(angle);Vertex(p+(right*Mathf.Cos(angle)*width+up*Mathf.Sin(angle)*depth)*taper*baseRound,n,Tint(1-.05f*Mathf.Cos(angle*3),accent));}
  }
  for(int i=0;i<along;i++)for(int j=0;j<around;j++){int a0=start+i*(around+1)+j,b0=a0+around+1;Triangle(a0,b0,a0+1);Triangle(a0+1,b0,b0+1);}
  // Close tips even when the lock is used as a separate accessory.
  for(int j=1;j<around-1;j++){Triangle(start,start+j,start+j+1);int end=start+along*(around+1);Triangle(end,end+j+1,end+j);}
 }
 void Band(Vector3 center,Vector3 radius,float thickness,float accent=1){
  Surface((u,v)=>{float a=u*Mathf.PI*2,b=v*Mathf.PI*2;var radial=new Vector3(Mathf.Cos(a),Mathf.Sin(a),0);return center+new Vector3(radial.x*radius.x,radial.y*radius.y,0)+radial*(Mathf.Cos(b)*thickness)+Vector3.forward*(Mathf.Sin(b)*thickness);},sectors,distant?6:10,(u,v)=>Tint(1,accent));
 }
 void HatRing(float y,float radiusX,float radiusZ,float thickness,float accent){
  Surface((u,v)=>{float a=u*Mathf.PI*2,b=v*Mathf.PI*2;return new Vector3(Mathf.Cos(a)*(radiusX+Mathf.Cos(b)*thickness),y+Mathf.Sin(b)*thickness,.06f+Mathf.Sin(a)*(radiusZ+Mathf.Cos(b)*thickness));},sectors,distant?6:8,(u,v)=>Tint(1,accent),true);
 }
 void Visor(){
  // A closed curved bill, not a flattened sphere. Its front edge has a visible hem.
  int columns=distant?12:24,rows=distant?3:6;
  Vector3 Point(float u,float v,float offset){float x=(u*2-1)*.29f;float z=.20f+v*(.29f*Mathf.Sqrt(Mathf.Max(0,1-Mathf.Pow(u*2-1,2)))+.025f);return new Vector3(x,1.355f-(z-.2f)*.18f+.018f*(1-Mathf.Pow(u*2-1,2))+offset,z);}
  Surface((u,v)=>Point(u,v,0),columns,rows,(u,v)=>Tint(),true);
  Surface((u,v)=>Point(1-u,v,-.018f),columns,rows,(u,v)=>Tint(.72f),true);
  Surface((u,v)=>Point(u,1,-.018f*v),columns,1,(u,v)=>Tint(1,1),true);
  Surface((u,v)=>Point(1-u,0,-.018f*v),columns,1,invert:true);
  foreach(float side in new[]{0f,1f})Surface((u,v)=>Point(side,u,-.018f*v),rows,1,invert:side==0);
 }
 void Bucket(){
  // Cross-section travels from the underside, around the rolled brim, up the
  // tapered crown and across the top. Both LODs retain the same silhouette.
  Vector2[] profile={new(.25f,1.315f),new(.35f,1.285f),new(.365f,1.295f),new(.36f,1.315f),new(.285f,1.37f),new(.264f,1.53f),new(.245f,1.575f),new(.20f,1.595f),new(0,1.595f)};
  for(int i=0;i<profile.Length-1;i++){var a=profile[i];var b=profile[i+1];int segment=i;
   Surface((u,v)=>{var p=Vector2.Lerp(a,b,v);float angle=u*Mathf.PI*2;return new Vector3(p.x*Mathf.Cos(angle),p.y,.06f+p.x*.96f*Mathf.Sin(angle));},sectors,segment==4?4:1,(u,v)=>Tint(segment<2?.8f:1,segment==2?1:0),true);
  }
  HatRing(1.385f,.283f,.272f,.011f,1);HatRing(1.415f,.279f,.268f,.004f,1);
 }
 public void Make(int preset){
  Cap(preset);
  foreach(int side in new[]{-1,1})Lock(new Vector3(side*.245f,1.29f,.205f),new Vector3(side*.263f,1.25f,.211f),new Vector3(side*.267f,1.19f,.195f),new Vector3(side*.257f,1.16f,.18f),.021f,.018f);
  switch(preset){
   case 1:
    for(int i=0;i<7;i++){float x=(i-3)*.067f;Lock(new Vector3(x-.06f,1.55f,.16f),new Vector3(x-.025f,1.55f,.29f),new Vector3(x+.045f,1.40f,.35f),new Vector3(x+.025f,1.305f+Mathf.Abs(x)*.12f,.33f),.048f,.024f);}break;
   case 2:
    for(int ring=0;ring<5;ring++){float theta=.18f+ring*.29f;int count=ring==0?5:10+ring*2;for(int j=0;j<count;j++){float a=(j+.45f*(ring%2))*Mathf.PI*2/count;var c=new Vector3(Mathf.Cos(a)*.275f*Mathf.Sin(theta),1.325f+.29f*Mathf.Cos(theta),.06f+Mathf.Sin(a)*.27f*Mathf.Sin(theta));float size=.066f+.006f*Mathf.Sin(j*2+ring);Ball(c,new Vector3(size,size,size),.055f,5,small:true);}}break;
   case 3:
    // Broad level crown with a rounded perimeter and fine sculpted channels.
    for(int i=-3;i<=3;i++){float x=i*.06f;Lock(new Vector3(x,1.587f,-.09f),new Vector3(x,1.615f,.02f),new Vector3(x,1.614f,.18f),new Vector3(x,1.57f,.245f),.024f,.012f);}break;
   case 4:
    for(int i=0;i<5;i++){float z=.25f-i*.105f;Lock(new Vector3(0,1.48f,z),new Vector3(0,1.68f,z-.035f),new Vector3(0,1.74f,z-.12f),new Vector3(0,1.64f,z-.16f),.056f,.085f);}break;
   case 5:
    Lock(new Vector3(0,1.29f,-.19f),new Vector3(0,1.30f,-.43f),new Vector3(.04f,1.04f,-.43f),new Vector3(.085f,1.01f,-.36f),.105f,.09f);
    Band(new Vector3(0,1.27f,-.273f),new Vector3(.087f,.081f,0),.013f);break;
   case 6:
    Ball(new Vector3(0,1.58f,.025f),new Vector3(.11f,.085f,.11f));
    HatRing(1.60f,.108f,.105f,.014f,1);Ball(new Vector3(0,1.705f,.025f),new Vector3(.163f,.15f,.15f),.055f,11);break;
   case 7:
    foreach(int side in new[]{-1,1}){Ball(new Vector3(side*.245f,1.47f,.035f),new Vector3(.12f,.11f,.12f));Ball(new Vector3(side*.295f,1.505f,.03f),new Vector3(.17f,.165f,.16f),.07f,9);}break;
   case 8:
    Visor();HatRing(1.35f,.291f,.286f,.008f,1);
    for(int i=0;i<6;i++){float u=(float)i/6;Vector3 a=CapPoint(u,.03f,8),b=CapPoint(u,.35f,8),c=CapPoint(u,.65f,8),d=CapPoint(u,.87f,8);Lock(a+Vector3.up*.003f,b+Vector3.up*.005f,c+Vector3.up*.005f,d,.0035f,.0035f,.65f);}
    Ball(new Vector3(0,1.62f,.06f),new Vector3(.025f,.013f,.025f),accent:1);break;
   case 9:Bucket();break;
  }
 }
 public void Scalp(){
  // Wider and lower than the old sphere: the ear roots, jaw and neck overlap
  // the retained face rather than leaving a crescent of background visible.
  Ball(new Vector3(0,1.205f,.035f),new Vector3(.284f,.255f,.225f));
  var center=head.InverseTransformPoint(new Vector3(0,1.205f,.035f));
  for(int i=0;i<positions.Count;i++)if(Vector3.Dot((positions[i]-center).normalized,normals[i])<.5f)throw new InvalidOperationException("Scalp normal faces inward");
  foreach(int side in new[]{-1,1})Ball(new Vector3(side*.25f,1.11f,.075f),new Vector3(.06f,.09f,.09f));
 }
 public Mesh Mesh(string name){
  // All parts are rigid on the same bone. Fully buried triangles cannot become
  // visible during animation, so strip them instead of lowering visible detail.
  var world=positions.ConvertAll(p=>head.TransformPoint(p));var visible=new List<int>();
  for(int i=0;i<triangles.Count;i+=3){bool buried=false;int a=triangles[i],b=triangles[i+1],c=triangles[i+2];
   for(int j=0;j<solids.Count&&!buried;j++)if(j!=triangleOwners[i/3])buried=solids[j](world[a])&&solids[j](world[b])&&solids[j](world[c]);
   if(!buried){visible.Add(a);visible.Add(b);visible.Add(c);}
  }
  var remap=new Dictionary<int,int>();var p2=new List<Vector3>();var n2=new List<Vector3>();var c2=new List<Color32>();
  for(int i=0;i<visible.Count;i++){int old=visible[i];if(!remap.TryGetValue(old,out int index)){index=p2.Count;remap.Add(old,index);p2.Add(positions[old]);n2.Add(normals[old]);c2.Add(colors[old]);}visible[i]=index;}
  var m=new Mesh{name=name};if(p2.Count>65535)m.indexFormat=UnityEngine.Rendering.IndexFormat.UInt32;m.SetVertices(p2);m.SetNormals(n2);m.SetColors(c2);m.SetTriangles(visible,0);m.RecalculateBounds();
  Debug.Log($"HEAD_MESH {name} removed {triangles.Count/3-visible.Count/3} buried triangles; retained {visible.Count/3}");return m;
 }
}
