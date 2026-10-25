using System.Collections.Generic;
using UnityEngine;

namespace WhatTheFish {
 // Editable geometric master for the two reference podiums. Editor baking merges
 // each colour into one mesh; the player loads only the resulting prefab.
 public static class IslandAwardsStage {
  public static Vector3 Seat(int rank)=>rank==1?new Vector3(0,2.42f,-.65f):new Vector3(rank==2?2.75f:-2.75f,rank==2?1.47f:1.27f,.1f);
  public static GameObject Create(bool fishing){return new Builder(fishing).Build();}
  sealed class Builder {
   readonly bool fishing;readonly Transform root;readonly Dictionary<string,Material> materials=new();
   readonly Dictionary<Material,List<CombineInstance>> batches=new();readonly List<Mesh> parts=new();
   const string Cream="FFF0CC",Stone="EAC99B",Gold="FFC329",Silver="C2D5EA",Bronze="EF8C45",Blue="159EE9",Teal="08A7BC",Wood="B57035",Rope="ECCC92";
   public Builder(bool water){fishing=water;root=new GameObject(water?"Lagoon lantern podium":"Island garden podium").transform;}
   Material Mat(string hex){if(materials.TryGetValue(hex,out var m))return m;m=new Material(Shader.Find("Universal Render Pipeline/Lit")){name="Awards "+hex};ColorUtility.TryParseHtmlString("#"+hex,out var c);m.color=c;m.SetFloat("_Smoothness",hex==Gold||hex==Silver||hex==Bronze||hex==Blue||hex==Teal?.65f:.28f);if(hex=="68B827"||hex=="2F9A37")m.SetFloat("_Cull",0);m.SetFloat("_Metallic",hex==Gold||hex==Silver||hex==Bronze?.25f:0);materials[hex]=m;return m;}
   void Add(Mesh mesh,string color,Matrix4x4 matrix){var m=Mat(color);if(!batches.TryGetValue(m,out var b))batches[m]=b=new();b.Add(new CombineInstance{mesh=mesh,transform=matrix});parts.Add(mesh);}
   void Mesh(string name,List<Vector3> v,List<int> t,string c,Vector3 p,Quaternion q){var m=new Mesh{name=name};m.SetVertices(v);m.SetTriangles(t,0);m.RecalculateNormals();Add(m,c,Matrix4x4.TRS(p,q,Vector3.one));}
   static void Quad(List<int> t,int a,int b,int c,int d){t.AddRange(new[]{a,b,c,a,c,d});}
   void Lathe(Vector3 at,float radius,float height,string color,int sides=64){
    var v=new List<Vector3>();var t=new List<int>();float bevel=Mathf.Min(.065f,height*.23f);
    var rings=new[]{new Vector2(0,0),new Vector2(radius-bevel,0),new Vector2(radius,bevel),new Vector2(radius,height-bevel),new Vector2(radius-bevel,height),new Vector2(0,height)};
    foreach(var ring in rings)for(int i=0;i<=sides;i++){float a=i*Mathf.PI*2/sides;v.Add(new Vector3(Mathf.Sin(a)*ring.x,ring.y,Mathf.Cos(a)*ring.x));}
    for(int y=0;y<rings.Length-1;y++)for(int i=0;i<sides;i++){int a=y*(sides+1)+i;Quad(t,a,a+1,a+sides+2,a+sides+1);}Mesh("Bevelled circular masonry",v,t,color,at,Quaternion.identity);
   }
   void Box(Vector3 p,Vector3 size,string color,float bevel=.035f,Quaternion? rot=null){
    var v=new List<Vector3>();var t=new List<int>();var half=size*.5f;float r=Mathf.Min(bevel,Mathf.Min(half.x,Mathf.Min(half.y,half.z))*.9f);var inner=half-Vector3.one*r;
    for(int side=0;side<6;side++){
     Vector3 n=side==0?Vector3.right:side==1?Vector3.left:side==2?Vector3.up:side==3?Vector3.down:side==4?Vector3.forward:Vector3.back;
     Vector3 u=side<2?Vector3.forward:Vector3.right, w=Vector3.Cross(n,u);int start=v.Count;
     for(int y=0;y<4;y++)for(int x=0;x<4;x++){
      float ax=x==0?-1:x==3?1:x==1?-.8f:.8f,ay=y==0?-1:y==3?1:y==1?-.8f:.8f;
      var point=Vector3.Scale(n+u*ax+w*ay,half);var nearest=new Vector3(Mathf.Clamp(point.x,-inner.x,inner.x),Mathf.Clamp(point.y,-inner.y,inner.y),Mathf.Clamp(point.z,-inner.z,inner.z));v.Add(nearest+(point-nearest).normalized*r);
     }
     for(int y=0;y<3;y++)for(int x=0;x<3;x++){int a=start+y*4+x;Quad(t,a,a+1,a+5,a+4);}
    }Mesh("Rounded carved block",v,t,color,p,rot??Quaternion.identity);
   }
   void Tube(List<Vector3> path,float radius,string color,int sides=8){
    var v=new List<Vector3>();var t=new List<int>();
    for(int k=0;k<path.Count;k++){var axis=(path[Mathf.Min(path.Count-1,k+1)]-path[Mathf.Max(0,k-1)]).normalized;var u=Vector3.Cross(axis,Mathf.Abs(axis.y)>.9f?Vector3.right:Vector3.up).normalized;var w=Vector3.Cross(axis,u);for(int i=0;i<=sides;i++){float a=i*Mathf.PI*2/sides;v.Add(path[k]+radius*(u*Mathf.Cos(a)+w*Mathf.Sin(a)));}}
    for(int k=0;k<path.Count-1;k++)for(int i=0;i<sides;i++){int a=k*(sides+1)+i;Quad(t,a,a+1,a+sides+2,a+sides+1);}Mesh("Woven curved trim",v,t,color,Vector3.zero,Quaternion.identity);
   }
   void Ring(Vector3 p,float radius,float tube,string c,bool vertical=false){var path=new List<Vector3>();for(int i=0;i<=48;i++){float a=i*Mathf.PI*2/48;path.Add(p+(vertical?new Vector3(Mathf.Sin(a),Mathf.Cos(a),0):new Vector3(Mathf.Sin(a),0,Mathf.Cos(a)))*radius);}Tube(path,tube,c);}
   void Blob(Vector3 p,Vector3 scale,string color){
    var v=new List<Vector3>();var t=new List<int>();for(int y=0;y<=8;y++)for(int x=0;x<=12;x++){float a=x*Mathf.PI*2/12,b=y*Mathf.PI/8;v.Add(Vector3.Scale(new Vector3(Mathf.Sin(b)*Mathf.Cos(a),Mathf.Cos(b),Mathf.Sin(b)*Mathf.Sin(a)),scale));}for(int y=0;y<8;y++)for(int x=0;x<12;x++){int a=y*13+x;Quad(t,a,a+1,a+14,a+13);}Mesh("Sculpted foliage",v,t,color,p,Quaternion.identity);
   }
   void Star(Vector3 p,float radius,float depth,string color){
    var pts=new List<Vector2>();for(int i=0;i<10;i++){float a=Mathf.PI/2+i*Mathf.PI/5;pts.Add(new Vector2(Mathf.Cos(a),Mathf.Sin(a))*radius*(i%2==0?1:.56f));}
    var curve=new List<Vector2>();for(int i=0;i<10;i++)for(int j=0;j<5;j++){float u=j/5f;var a=Vector2.Lerp(pts[(i+9)%10],pts[i],.72f);var b=pts[i];var c=Vector2.Lerp(pts[i],pts[(i+1)%10],.28f);curve.Add((1-u)*(1-u)*a+2*(1-u)*u*b+u*u*c);}
    var v=new List<Vector3>();var t=new List<int>();for(int k=0;k<3;k++)foreach(var a in curve)v.Add(new Vector3(a.x*(k==1?1:.93f),a.y*(k==1?1:.93f),k*depth/2));
    int count=curve.Count;for(int k=0;k<2;k++)for(int i=0;i<count;i++)Quad(t,k*count+i,k*count+(i+1)%count,(k+1)*count+(i+1)%count,(k+1)*count+i);
    v.Add(new Vector3(0,0,depth));for(int i=0;i<count;i++)t.AddRange(new[]{v.Count-1,2*count+i,2*count+(i+1)%count});Mesh("Rounded star medallion",v,t,color,p,Quaternion.identity);
   }
   void Number(Vector3 p,int number,string color){
    var go=new GameObject("Sculpted place "+number);go.transform.SetParent(root,false);go.transform.localPosition=p;go.transform.localRotation=Quaternion.Euler(0,180,0);var text=go.AddComponent<TextMesh>();text.text=number.ToString();text.font=Resources.Load<Font>("Menu/CoveDisplay");text.fontSize=100;text.characterSize=.12f;text.anchor=TextAnchor.MiddleCenter;text.alignment=TextAlignment.Center;text.color=Mat(color).color;go.GetComponent<MeshRenderer>().sharedMaterial=text.font.material;
   }
   void Leaves(Vector3 p,float size,int seed){
    for(int i=0;i<5;i++){float a=(seed*1.7f+i*1.3f);var dir=new Vector3(Mathf.Cos(a),0,Mathf.Sin(a));var path=new List<Vector3>();var v=new List<Vector3>();var t=new List<int>();for(int k=0;k<=8;k++){float u=k/8f;var center=p+dir*(size*u*.72f)+Vector3.up*(size*Mathf.Sin(u*1.8f));var across=Vector3.Cross(dir,Vector3.up)*Mathf.Sin(u*Mathf.PI)*size*.22f;v.Add(center-across);v.Add(center+Vector3.up*.055f*size);v.Add(center+across);path.Add(center+Vector3.up*.058f*size);}for(int k=0;k<8;k++){int a0=k*3;Quad(t,a0,a0+1,a0+4,a0+3);Quad(t,a0+1,a0+2,a0+5,a0+4);}Mesh("Broad tropical leaf",v,t,i%2==0?"68B827":"2F9A37",Vector3.zero,Quaternion.identity);Tube(path,.012f,"B3DF48",5);}
   }
   void Post(Vector3 p,bool lantern){
    Lathe(p,.16f,1.05f,Wood,12);Ring(p+Vector3.up*.68f,.18f,.045f,Rope);Ring(p+Vector3.up*.77f,.18f,.045f,Rope);Ring(p+Vector3.up*.86f,.18f,.045f,Rope);Lathe(p+Vector3.up*1.03f,.22f,.13f,fishing?Teal:Gold,12);
    if(!lantern){if(!fishing)Blob(p+Vector3.up*1.25f,Vector3.one*.14f,Gold);return;}
    Box(p+Vector3.up*1.38f,new Vector3(.35f,.5f,.35f),"FFDE79");for(int x=-1;x<=1;x+=2)for(int z=-1;z<=1;z+=2){var a=p+new Vector3(x*.2f,1.12f,z*.2f);Tube(new List<Vector3>{a,a+Vector3.up*.53f},.032f,Gold);}
    Lathe(p+Vector3.up*1.66f,.34f,.1f,Teal,8);Lathe(p+Vector3.up*1.76f,.23f,.09f,Teal,8);Blob(p+Vector3.up*1.9f,Vector3.one*.075f,Teal);
   }
   void RopeSpan(Vector3 a,Vector3 b){var points=new List<Vector3>();for(int i=0;i<=18;i++){float u=i/18f;points.Add(Vector3.Lerp(a,b,u)-Vector3.up*Mathf.Sin(u*Mathf.PI)*.22f);}Tube(points,.05f,Rope);}
   void Top(Vector3 at,float radius,string color){
    // Clipped tiles/planks preserve the circular silhouette and visible joints.
    float step=fishing?.36f:.48f;for(float z=-radius;z<radius;z+=step)for(float x=-radius;x<radius;x+=step){
     var polygon=new List<Vector3>{new(x+.008f,0,z+.008f),new(x+.008f,0,z+step-.008f),new(x+step-.008f,0,z+step-.008f),new(x+step-.008f,0,z+.008f)};
     for(int i=0;i<48&&polygon.Count>0;i++){float angle=i*Mathf.PI*2/48;var normal=new Vector3(Mathf.Cos(angle),0,Mathf.Sin(angle));var clipped=new List<Vector3>();for(int k=0;k<polygon.Count;k++){var a=polygon[k];var b=polygon[(k+1)%polygon.Count];float da=Vector3.Dot(a,normal)-(radius-.055f),db=Vector3.Dot(b,normal)-(radius-.055f);if(da<=0)clipped.Add(a);if((da<=0)!=(db<=0))clipped.Add(Vector3.Lerp(a,b,da/(da-db)));}polygon=clipped;}
     if(polygon.Count<3)continue;var triangles=new List<int>();for(int i=1;i<polygon.Count-1;i++)triangles.AddRange(new[]{0,i,i+1});bool alternate=(Mathf.RoundToInt(x/step)+Mathf.RoundToInt(z/step))%2==0;Mesh("Inset top tile",polygon,triangles,fishing&&color!=Silver?(alternate?"D28A48":"C27B3D"):alternate?color:color==Gold?"FFE775":color==Silver?"EBF1FF":"FFBF7F",at,Quaternion.identity);
    }
   }
   void Drum(int rank){
    var seat=Seat(rank);float radius=rank==1?1.66f:1.32f;float h=seat.y-.34f;var p=new Vector3(seat.x,.34f,seat.z);string metal=rank==1?Gold:rank==2?Silver:Bronze;
    Lathe(p,radius,h-.22f,Cream);Lathe(p+Vector3.up*(h-.36f),radius+.03f,.15f,Blue);Lathe(p+Vector3.up*(h-.22f),radius+.02f,.22f,metal);Top(seat+Vector3.up*.006f,radius,metal);Ring(seat-Vector3.up*.015f,radius-.06f,.018f,"FFF7CF");
    var badge=p+new Vector3(0,h*.52f,radius-.04f);for(int side=-1;side<=1;side+=2)for(int i=0;i<3;i++)Blob(badge+new Vector3(side*(.51f+i*.035f),.24f-i*.22f,.03f),new Vector3(.3f,.17f,.08f),"FFFCF0");
    Star(badge,.72f,.14f,rank==1?"E8A313":rank==2?"7E95B2":"D0672C");Star(badge+Vector3.forward*.14f,.64f,.07f,metal);Number(badge+Vector3.forward*.24f+new Vector3(.027f,-.025f,0),rank,rank==1?"713900":rank==2?"253D57":"792C13");Number(badge+Vector3.forward*.255f,rank,rank==1?"FFE68C":rank==2?"DAE7FF":"FFBC6B");
    if(fishing){for(int band=0;band<2;band++){var v=new List<Vector3>();var t=new List<int>();for(int i=0;i<=96;i++){float a=i*Mathf.PI*2/96;float y=.25f+Mathf.Sin(a*4+band*.6f)*.14f+band*.08f;var d=new Vector3(Mathf.Sin(a),0,Mathf.Cos(a))*(radius+.015f+band*.005f);v.Add(p+d+Vector3.up*.025f);v.Add(p+d+Vector3.up*y);}for(int i=0;i<96;i++)Quad(t,i*2,i*2+2,i*2+3,i*2+1);Mesh("Blue scalloped wave relief",v,t,band==0?"007FC6":"12BDF4",Vector3.zero,Quaternion.identity);}}
   }
   public GameObject Build(){
    Lathe(new Vector3(0,0,0),5,.34f,Stone,96);Lathe(new Vector3(0,.32f,0),4.87f,.06f,Cream,96);
    for(int i=0;i<50;i++){float a=i*Mathf.PI*2/50;Box(new Vector3(Mathf.Sin(a)*4.88f,.19f,Mathf.Cos(a)*4.88f),new Vector3(.595f,.32f,.28f),i%3==0?"F3DCB1":Stone,.035f,Quaternion.Euler(0,a*Mathf.Rad2Deg,0));}
    Drum(1);Drum(2);Drum(3);
    for(int s=0;s<3;s++)Box(new Vector3(0,.43f+s*.13f,2.05f-s*.28f),new Vector3(2.02f,.17f,1.02f),fishing?Wood:Stone);
    if(fishing){
     for(int i=0;i<100;i++){float z=4.1f+i*.33f;Box(new Vector3(0,.30f,z),new Vector3(1.9f,.16f,.315f),i%3==0?"D08A49":"BD783E");for(int side=-1;side<=1;side+=2)Lathe(new Vector3(side*.82f,.386f,z),.025f,.015f,"687575",8);}
     for(int i=0;i<17;i++){float z=4.35f+i*2;Post(new Vector3(-1.1f,-.35f,z),false);Post(new Vector3(1.1f,-.35f,z),false);}
    }else{
     for(int i=0;i<4;i++){float z=4.9f-i*.43f;Box(new Vector3(0,.14f+i*.16f,z),new Vector3(2.8f,.22f,.70f),i%2==0?"EFD1A3":"E5BC89");}
     foreach(int s in new[]{-1,1}){Tube(new List<Vector3>{new(s*1.5f,.32f,5.1f),new(s*1.5f,.9f,3.15f)},.13f,Blue);for(int i=0;i<6;i++)Blob(new Vector3(s*(2.25f+i*.38f),.5f,3.2f-i*.45f),new Vector3(.46f,.42f,.4f),i%2==0?"6EAE32":"3C9634");}
    }
    for(int side=-1;side<=1;side+=2){
     Post(new Vector3(side*4.35f,.1f,.5f),fishing);Post(new Vector3(side*2.05f,.25f,-2.35f),fishing);Post(new Vector3(side*4.3f,.1f,2.4f),false);RopeSpan(new Vector3(side*4.35f,1,.5f),new Vector3(side*4.3f,1,2.4f));
     if(fishing){Ring(new Vector3(side*4.72f,.55f,2.1f),.34f,.11f,"F17632",true);for(int k=0;k<4;k++){float a=k*Mathf.PI/2;Blob(new Vector3(side*4.72f+Mathf.Sin(a)*.34f,.55f+Mathf.Cos(a)*.34f,2.1f),new Vector3(.12f,.12f,.12f),"FFF9E5");}}
     for(int k=0;k<4;k++){var p=new Vector3(side*(1.65f+k*.8f),.36f,1.7f-k*.65f);Leaves(p,fishing?.62f:.48f,k+side);Blob(p+new Vector3(.28f,-.01f,.15f),new Vector3(.26f,.24f,.24f),k%2==0?"A9A9A5":"B9B8AD");if(!fishing){for(int j=0;j<5;j++){float a=j*Mathf.PI*2/5;Blob(p+new Vector3(Mathf.Cos(a)*.08f,.22f,Mathf.Sin(a)*.08f),new Vector3(.065f,.025f,.04f),"FFFCE7");}Blob(p+Vector3.up*.24f,Vector3.one*.035f,Gold);}}
     if(!fishing)for(int k=0;k<4;k++)Blob(new Vector3(side*(1.65f+k*.55f),1.1f-k*.1f,-2),new Vector3(.6f,.65f,.58f),k%2==0?"65A938":"488D32");
    }
    foreach(var pair in batches){var go=new GameObject(pair.Key.name);go.transform.SetParent(root,false);var mesh=new Mesh{name=pair.Key.name};mesh.indexFormat=UnityEngine.Rendering.IndexFormat.UInt32;mesh.CombineMeshes(pair.Value.ToArray(),true,true);go.AddComponent<MeshFilter>().sharedMesh=mesh;go.AddComponent<MeshRenderer>().sharedMaterial=pair.Key;}
    foreach(var part in parts)Object.DestroyImmediate(part);return root.gameObject;
   }
  }
 }
}
