using UnityEngine;
using UnityEngine.Rendering;

namespace WhatTheFish {
 [DefaultExecutionOrder(70)]
 public sealed class BasketballPassVisual:MonoBehaviour {
  const int Steps=64;
  BasketballBall ball;MeshRenderer lane;Mesh mesh;Material material;float visibility;
  readonly Vector3[] points=new Vector3[Steps+1],vertices=new Vector3[(Steps+1)*2];
  readonly Vector2[] uv=new Vector2[(Steps+1)*2];readonly float[] distances=new float[Steps+1];
  public bool Visible=>lane&&lane.enabled;
  public Vector3 Direction=>Quaternion.Euler(0,ball.PassAim.heading,0)*Vector3.forward;
  public float Length {get;private set;}
  public BasketballPassPath Path {get;private set;}
  void Awake(){ball=GetComponent<BasketballBall>();}
  void Create(){
   var source=Resources.Load<Material>("BasketballPassAim");if(!source)return;
   material=new Material(source){name="Basketball pass trajectory"};mesh=new Mesh{name="Basketball pass ribbon"};mesh.MarkDynamic();
   var triangles=new int[Steps*6];for(int i=0;i<Steps;i++){int v=i*2,t=i*6;triangles[t]=v;triangles[t+1]=v+2;triangles[t+2]=v+1;triangles[t+3]=v+1;triangles[t+4]=v+2;triangles[t+5]=v+3;}
   mesh.vertices=vertices;mesh.uv=uv;mesh.triangles=triangles;
   var go=new GameObject("Shared basketball trajectory");go.layer=2;go.transform.SetParent(transform.parent,false);
   go.transform.SetPositionAndRotation(Vector3.zero,Quaternion.identity);go.transform.localScale=Vector3.one;
   go.AddComponent<MeshFilter>().sharedMesh=mesh;lane=go.AddComponent<MeshRenderer>();lane.sharedMaterial=material;
   lane.shadowCastingMode=ShadowCastingMode.Off;lane.receiveShadows=false;
  }
  void LateUpdate(){
   var aim=ball.PassAim;bool active=ball.Playing&&aim.phase>0&&(aim.phase<3?ball.Held:ball.PassClock-aim.released<BasketballPassRules.FlashSeconds);
   if(!active){visibility=0;if(lane)lane.enabled=false;return;}
   if(!lane)Create();if(!lane)return;
   float power=ball.PassPower;Length=BasketballPassRules.Range(power,aim.bend);Vector3 origin=aim.origin;
   if(aim.phase<3){var actor=ball.Holder;if(!actor){lane.enabled=false;return;}origin=actor.transform.position+Quaternion.Euler(0,aim.heading,0)*BasketballMotion.PassReleasePoint(aim.bend);}
   Path=BasketballPassPath.Create(origin,aim.heading,power,aim.bend,BasketballBall.PassFloor(origin));
   // A vertex on the real floor contact preserves the bounce corner.
   for(int i=0;i<=Steps;i++){
    float time=Path.Bounces?(i<=32?Path.bounceTime*i/32:Mathf.Lerp(Path.bounceTime,Path.duration,(i-32)/32f)):Path.duration*i/Steps;
    var next=Path.Point(time);points[i]=visibility>0&&aim.phase==1?Vector3.Lerp(points[i],next,1-Mathf.Exp(-20*Time.deltaTime)):next;distances[i]=i==0?0:distances[i-1]+Vector3.Distance(points[i-1],points[i]);
   }
   float length=distances[Steps];var camera=Camera.main;var previousSide=Quaternion.Euler(0,aim.heading,0)*Vector3.right;
   for(int i=0;i<=Steps;i++){
    var tangent=points[Mathf.Min(Steps,i+1)]-points[Mathf.Max(0,i-1)];
    var side=camera?Vector3.Cross(tangent,camera.transform.position-points[i]).normalized:previousSide;
    if(side.sqrMagnitude<.1f)side=previousSide;if(Vector3.Dot(side,previousSide)<0)side=-side;previousSide=side;
    vertices[i*2]=lane.transform.InverseTransformPoint(points[i]-side*.7f);vertices[i*2+1]=lane.transform.InverseTransformPoint(points[i]+side*.7f);
    uv[i*2]=new Vector2(-.7f,distances[i]/length);uv[i*2+1]=new Vector2(.7f,distances[i]/length);
   }
   mesh.vertices=vertices;mesh.uv=uv;mesh.RecalculateBounds();
   visibility=Mathf.MoveTowards(visibility,1,Time.deltaTime*9);
   float flash=aim.phase==3?Mathf.Clamp01((float)((ball.PassClock-aim.released)/BasketballPassRules.FlashSeconds)):0;
   material.SetFloat("_Bend",aim.bend);material.SetFloat("_Charge",power);material.SetFloat("_Length",length);material.SetFloat("_Opacity",visibility*(1-BasketballMotion.Ease(flash)));material.SetFloat("_Release",aim.phase==3?1-flash:0);lane.enabled=true;
  }
  void OnDisable(){visibility=0;if(lane)lane.enabled=false;}
  void OnDestroy(){if(lane)Destroy(lane.gameObject);if(mesh)Destroy(mesh);if(material)Destroy(material);}
 }
}
