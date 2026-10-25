using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace WhatTheFish {
 // A local film cast, never a real cart, driver, opponent or gameplay force.
 internal sealed class GolfRushShowcase:IDisposable {
  Transform root,cart;readonly List<Transform> wheels=new();readonly List<float> radii=new();
  readonly List<FootballCelebrationActor> runners=new();FootballCelebrationActor driver;
  readonly List<LineRenderer> ribbons=new();Vector3 origin,forward,right,seat;
  readonly float[] groundHeights=new float[14];bool passed;float age;
  public bool Visible=>root;
  public int RunnerCount=>runners.Count;
  public static float CartTravel(float t)=>8*t+1.4f*t*t;
  public static float RunnerTravel(float t)=>8*Mathf.Max(0,t-.18f)+1.4f*Mathf.Pow(Mathf.Max(0,t-.18f),2);
  bool FindLane(Athlete owner){
   var heading=Vector3.ProjectOnPlane(owner.transform.forward,Vector3.up).normalized;
   for(int attempt=0;attempt<18;attempt++){
    forward=Quaternion.Euler(0,(attempt%6)*60,0)*heading;right=Vector3.Cross(Vector3.up,forward);
    origin=owner.transform.position+right*(9+(attempt/6)*7);
    bool clear=true;
    for(int i=0;i<groundHeights.Length;i++){
     var point=origin+forward*(-32+i*4);
     if(!Physics.Raycast(point+Vector3.up*18,Vector3.down,out var hit,40,1<<8,QueryTriggerInteraction.Ignore)||hit.normal.y<.88f||Physics.CheckBox(hit.point+Vector3.up*1.9f,new Vector3(2.5f,1.35f,1.8f),Quaternion.LookRotation(forward),1<<8,QueryTriggerInteraction.Ignore)){clear=false;break;}
     var course=GolfMatchManager.Instance.Course;var local=course.transform.InverseTransformPoint(hit.point);
     if(course.waterPatches.Any(w=>Mathf.Pow((local.x-w.centre.x)/w.radii.x,2)+Mathf.Pow((local.z-w.centre.z)/w.radii.y,2)<1.3f)){clear=false;break;}
     groundHeights[i]=hit.point.y;
    }
    if(clear)return true;
   }
   return false;
  }
  Vector3 Point(float x,float z,float lift=0){float sample=Mathf.Clamp((z+32)/4,0,13);int i=Mathf.Min(12,Mathf.FloorToInt(sample));var p=origin+right*x+forward*z;p.y=Mathf.Lerp(groundHeights[i],groundHeights[i+1],sample-i);if(GolfCartMotor.Floor(p,out var floor))p.y=floor.point.y;p.y+=lift;return p;}
  bool Create(Athlete owner){
   var prefab=Resources.Load<GolfCart>("GolfCart");if(!prefab||!FindLane(owner))return false;
   root=new GameObject("Island Rush demonstration (visual only)").transform;
   cart=new GameObject("Rush demonstration cart").transform;cart.SetParent(root,false);
   var map=new Dictionary<Transform,Transform>{{prefab.transform,cart}};
   foreach(var t in prefab.GetComponentsInChildren<Transform>(true)){
    if(t==prefab.transform)continue;var copy=new GameObject(t.name).transform;copy.gameObject.layer=2;copy.SetParent(map[t.parent],false);copy.localPosition=t.localPosition;copy.localRotation=t.localRotation;copy.localScale=t.localScale;map.Add(t,copy);
   }
   var renderers=new Dictionary<Renderer,Renderer>();
   foreach(var source in prefab.GetComponentsInChildren<MeshRenderer>(true))if(source.TryGetComponent<MeshFilter>(out var filter)){
    var go=map[source.transform].gameObject;go.AddComponent<MeshFilter>().sharedMesh=filter.sharedMesh;var clone=go.AddComponent<MeshRenderer>();clone.sharedMaterials=source.sharedMaterials;renderers[source]=clone;
   }
   foreach(var source in prefab.GetComponentsInChildren<LODGroup>(true)){
    var group=map[source.transform].gameObject.AddComponent<LODGroup>();var levels=source.GetLODs();
    for(int i=0;i<levels.Length;i++)levels[i].renderers=levels[i].renderers.Where(r=>r&&renderers.ContainsKey(r)).Select(r=>renderers[r]).ToArray();
    group.SetLODs(levels);group.localReferencePoint=source.localReferencePoint;group.size=source.size;group.ForceLOD(0);
   }
   for(int i=0;i<prefab.wheelPivots.Length;i++){wheels.Add(map[prefab.wheelPivots[i]]);radii.Add(prefab.wheelRadii[i]);}
   seat=prefab.transform.InverseTransformPoint(prefab.seat.position);
   var sources=Athlete.Active.Where(a=>a&&a.visual&&!a.IsSpectator).ToArray();
   driver=Actor(owner,1,cart);driver.Root.localPosition=seat-Vector3.up*.75f;
   for(int i=0;i<3;i++){var runner=Actor(sources.Length>i?sources[i]:owner,(byte)(i+2),root);runner.Root.name="Rush chasing runner "+(i+1);runners.Add(runner);}
   for(int i=0;i<12;i++){
    var go=new GameObject("Rush acceleration ribbon "+i);go.transform.SetParent(root,false);go.layer=2;
    var line=go.AddComponent<LineRenderer>();line.sharedMaterial=Resources.Load<Material>("GolfBallTrail");line.positionCount=20;line.widthMultiplier=i<3?.11f:.055f;line.useWorldSpace=true;line.shadowCastingMode=ShadowCastingMode.Off;line.receiveShadows=false;line.numCornerVertices=3;ribbons.Add(line);
   }
   passed=false;return true;
  }
  static FootballCelebrationActor Actor(Athlete source,byte number,Transform parent)=>new(source,new FootballPlayerRecord{id=number,name="Rush preview",number=number,team=FootballTeam.None},parent);
  static FootballPose Sprint(float distance,int lane){
   var pose=FootballMotionLibrary.Ready;float p=distance/1.65f+lane*.23f,s=Mathf.Sin(p*Mathf.PI*2),c=Mathf.Cos(p*Mathf.PI*2);
   pose.body=new Vector3(18,0,s*3);pose.head=new Vector3(-12,0,-s*2);pose.pelvis=new Vector3(0,.025f+Mathf.Abs(s)*.055f,0);
   pose.leftFoot=new Vector3(0,Mathf.Max(0,s)*.23f,c*.28f);pose.rightFoot=new Vector3(0,Mathf.Max(0,-s)*.23f,-c*.28f);
   pose.leftHand=new Vector3(-.24f,.76f,.12f-c*.26f);pose.rightHand=new Vector3(.24f,.76f,.12f+c*.26f);pose.chest.y=s*7;return pose;
  }
  public bool Present(float time,Athlete owner){
   age=time;if(!Visible&&!Create(owner))return false;
   float travel=CartTravel(age),z=14-travel;var rotation=Quaternion.LookRotation(-forward);
   var cartPoint=Point(0,z);var cartRotation=rotation;
   if(GolfCartMotor.Surface(cartPoint,rotation.eulerAngles.y,out var supported,out var slope)){cartPoint=supported;cartRotation=slope;}
   cart.SetPositionAndRotation(cartPoint+Vector3.up*.025f,cartRotation);
   for(int i=0;i<wheels.Count;i++)wheels[i].localRotation=Quaternion.Euler(travel/radii[i]*Mathf.Rad2Deg,0,0);
   var sitting=FootballMotionLibrary.Ready;sitting.body=Vector3.zero;sitting.leftFoot=new Vector3(0,.4f,.4f);sitting.rightFoot=sitting.leftFoot;sitting.leftHand=new Vector3(-.15f,.9f,.50f);sitting.rightHand=new Vector3(.15f,.9f,.50f);driver.Pose(sitting);
   for(int i=0;i<runners.Count;i++){
    float run=RunnerTravel(age-i*.06f);runners[i].Root.SetPositionAndRotation(Point((i-1)*1.15f,18+i*.5f-run),rotation);runners[i].Pose(Sprint(run,i));
   }
   float alpha=Mathf.SmoothStep(0,1,age/.4f)*(1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(3,3.5f,age)));
   for(int lane=0;lane<ribbons.Count;lane++){
    var line=ribbons[lane];bool vehicle=lane<3;int runner=Mathf.Min(2,(lane-3)/3);int side=vehicle?lane-1:(lane-3)%3-1;
    var at=vehicle?cart.position:runners[runner].Root.position;float width=vehicle?.7f:.2f;
    line.startColor=new Color(.5f,1,.75f,alpha*.85f);line.endColor=new Color(1,1,.75f,0);
    for(int j=0;j<20;j++){float u=j/19f;line.SetPosition(j,at+right*(side*width+Mathf.Sin(age*13+u*7+lane)*u*.13f)+Vector3.up*(vehicle?.3f+lane*.28f:.15f+(side+1)*.3f)+forward*(.65f+u*(vehicle?4:2.8f)));}
   }
   GolfFishingAudio.Current?.Loop("rush-demo-motor","cart-motor",true,cart.position,.65f,Mathf.Lerp(1.1f,1.6f,Mathf.Clamp01(age/1.7f)),true);
   if(age>=1.5f&&!passed){passed=true;GolfFishingAudio.Current?.Play("golf-swish",cart.position,.9f,true);}
   return true;
  }
  public void Camera(out Vector3 focus,out Vector3 position){
   // A continuous track across the near pass turns the view almost 180 degrees.
   float after=Mathf.SmoothStep(0,1,Mathf.InverseLerp(1.4f,2.1f,age));
   float cameraZ=-.5f-age*.7f;
   position=Point(2.5f,cameraZ,1.35f);
   var pursuit=Vector3.Lerp(cart.position,runners[1].Root.position,.45f);
   focus=Vector3.Lerp(cart.position,pursuit,after)+Vector3.up*.95f;
  }
  public void Dispose(){
   if(root){root.gameObject.SetActive(false);UnityEngine.Object.Destroy(root.gameObject);}root=cart=null;runners.Clear();wheels.Clear();radii.Clear();ribbons.Clear();driver=null;
   GolfFishingAudio.Current?.StopLoop("rush-demo-motor");
  }
 }
}
