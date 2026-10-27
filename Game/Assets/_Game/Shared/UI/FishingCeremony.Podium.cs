using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WhatTheFish {
 public sealed partial class FishingCeremony {
  readonly List<int> castRanks=new();Transform podium;
  const float WalkSpeed=1.4f,QueueGap=1.85f,StrideLength=1.15f;
  const float EntryDelay=QueueGap/WalkSpeed;
  readonly Dictionary<Font,Material> numberMaterials=new();
  TextMesh[] podiumNumbers;Renderer[] numberRenderers;
  float WalkAt=>rankedGolf?2.5f:4.8f;
  float SettleAt=>MenuPreferences.ReducedMotion?.65f:WalkAt+Mathf.Max(7.2f,EntryDelay*(cast.Count-1)+4f);
  public bool PodiumSettled {get;private set;}
  public Vector3 PodiumCenter=>stage?stage.position:Vector3.zero;
  public int[] PresentedRanks=>castRanks.ToArray();
  void CreatePodium(){
   PodiumSettled=false;var prefab=Resources.Load<GameObject>("IslandAwards/"+(rankedGolf?"Golf":"Fishing"));
   if(!prefab)throw new System.InvalidOperationException("Baked IslandAwards prefab missing. Run IslandAwardsBuild.Prepare.");
   podium=Instantiate(prefab,stage,false).transform;podium.gameObject.SetActive(rankedGolf);
   podiumNumbers=podium.GetComponentsInChildren<TextMesh>(true);numberRenderers=podiumNumbers.Select(t=>t.GetComponent<Renderer>()).ToArray();
   // Presentation-only concealment; restore every renderer at End. Terrain,
   // water, colliders, players and network state remain untouched.
   foreach(var renderer in FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None)){
    if(renderer.transform.IsChildOf(stage)||renderer.GetComponentInParent<Athlete>())continue;
    var local=stage.InverseTransformPoint(renderer.bounds.center);var size=renderer.bounds.size;
    bool intrudes=rankedGolf&&new Vector2(local.x,local.z).magnitude<6.6f&&local.y<3.5f&&size.x<4&&size.z<4&&size.y<7;
    bool fish=!rankedGolf&&renderer.GetComponentsInParent<Transform>().Any(t=>t.name.StartsWith("Swimming fish "));
    if(intrudes||fish){hidden[renderer]=renderer.forceRenderingOff;renderer.forceRenderingOff=true;}
   }
  }

  void RefreshPodiumNumbers(){
   // Localization may replace a TextMesh font/material. Rebind its current atlas
   // after localization, but keep depth testing so nearby actors occlude digits.
   for(int i=0;i<podiumNumbers.Length;i++){
    var font=podiumNumbers[i].font;if(!font)continue;
    if(!numberMaterials.TryGetValue(font,out var material)){
     material=new Material(Resources.Load<Shader>("IslandAwards/DepthText")){name="Depth tested podium numerals"};numberMaterials.Add(font,material);
    }
    material.mainTexture=font.material.mainTexture;numberRenderers[i].sharedMaterial=material;
    numberRenderers[i].shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
   }
  }
  void ReleasePodiumMaterials(){foreach(var material in numberMaterials.Values)Destroy(material);numberMaterials.Clear();podiumNumbers=null;numberRenderers=null;}
  Vector3 ActorSeat(int index){
   int rank=castRanks[index],count=castRanks.Count(r=>r==rank),ordinal=castRanks.Take(index).Count(r=>r==rank);var seat=IslandAwardsStage.Seat(rank);
   // Shared ranks occupy the same level, with room between each pair of feet.
   seat.x+=(ordinal-(count-1)*.5f)*.78f;return seat;
  }
  void PresentPodium(float age){
   bool reduced=MenuPreferences.ReducedMotion;
   float walkAt=WalkAt,settleAt=SettleAt;RefreshPodiumNumbers();
   PodiumSettled=age>=settleAt;podium.gameObject.SetActive(rankedGolf||reduced||age>=1.7f);
   podium.localPosition=rankedGolf||reduced?Vector3.zero:Vector3.down*(2.7f*(1-Mathf.SmoothStep(0,1,(age-1.7f)/2.1f)));
   for(int i=0;i<cast.Count;i++){
    var actor=cast[i];
    // Prepare the complete spaced queue before revealing the entrance shot.
    // Enable everyone together outside the frustum, never at an in-frame spawn.
    actor.Root.gameObject.SetActive(true);
    foreach(var renderer in actor.Root.GetComponentsInChildren<Renderer>())renderer.forceRenderingOff=!PodiumSettled&&age<walkAt;
    var seat=ActorSeat(i);
    if(PodiumSettled){actor.Root.localPosition=seat;actor.Root.localRotation=Quaternion.identity;var take=takes[i%3];actor.Pose(take.Sample((age-settleAt+i*.4f)/take.duration));}
    else{
     float travelled=Mathf.Max(0,age-walkAt)*WalkSpeed;
     var direction=Vector3.back;
     var at=new Vector3(0,.38f,20)+direction*(travelled-i*QueueGap);
     if(rankedGolf){var world=stage.TransformPoint(at);if(Physics.Raycast(world+Vector3.up*10,Vector3.down,out var ground,20,1<<8,QueryTriggerInteraction.Ignore))at.y=stage.InverseTransformPoint(ground.point).y+.015f;else at.y=0;}
     actor.Root.localPosition=at;actor.Root.localRotation=Quaternion.LookRotation(direction);actor.Walk(WalkPose(travelled-i*QueueGap),(travelled-i*QueueGap)/StrideLength);
    }
   }
   Vector3 position,focus;
   if(PodiumSettled||reduced){
    float distance=Mathf.Max(15.5f,6.3f/(Mathf.Tan(21*Mathf.Deg2Rad)*Mathf.Max(.65f,camera.aspect)));
    // Screen-left is local +X. A quintic ease gives zero velocity/acceleration
    // at both ends: slow start, faster middle, gentle arrival. Then hold the shot.
    float sweepSeconds=rankedGolf?8:Mathf.Min(8,20-settleAt);
    float sweep=reduced?.5f:Ease5((age-settleAt)/sweepSeconds);float angle=Mathf.Lerp(18,-18,sweep)*Mathf.Deg2Rad;
    float radius=distance+(reduced?0:Mathf.Lerp(.65f,0,sweep));
    position=new Vector3(Mathf.Sin(angle)*radius,5.8f,Mathf.Cos(angle)*radius);focus=new Vector3(0,2.05f,.3f);
   }else if(age<walkAt){
    float approach=Ease5(age/walkAt);position=Vector3.Lerp(new Vector3(0,8.2f,21),new Vector3(1.4f,7.5f,18.5f),approach);focus=new Vector3(0,1.1f,1);
    // Establish the podium first, then dissolve into the offscreen queue shot.
    float cut=Mathf.Max(0,1-Mathf.Abs(age-walkAt)/.3f);if(cut>curtain.color.a)curtain.color=new Color(.025f,.07f,.11f,cut);
   }else{
    EntranceCamera(Ease5((age-walkAt)/(settleAt-walkAt)),out position,out focus);
    float cut=Mathf.Max(0,1-Mathf.Abs(age-walkAt)/.3f);if(cut>curtain.color.a)curtain.color=new Color(.025f,.07f,.11f,cut);
   }
   position=stage.TransformPoint(position);focus=stage.TransformPoint(focus);camera.transform.SetPositionAndRotation(position,Quaternion.LookRotation(focus-position));camera.fieldOfView=PodiumSettled||age<walkAt?42:2*Mathf.Atan(Mathf.Tan(21*Mathf.Deg2Rad)*Mathf.Max(1,1.777778f/Mathf.Max(.65f,camera.aspect)))*Mathf.Rad2Deg;camera.nearClipPlane=.12f;
   // A brief dissolve hides the time ellipsis; no full bridge traversal required.
   if(!reduced){float cut=Mathf.Max(0,1-Mathf.Abs(age-settleAt)/.3f);if(cut>curtain.color.a)curtain.color=new Color(.025f,.07f,.11f,cut);}
  }
  void EntranceCamera(float progress,out Vector3 position,out Vector3 focus){
   // The bridge / side path crosses the right edge. Do not fit unseen waiting
   // actors: doing so reveals the entrance point and causes visible spawning.
   focus=new Vector3(0,rankedGolf?.1f:1,12);position=new Vector3(7,5,18);
   var advance=(focus-position).normalized*(progress*.6f);position+=advance;focus+=advance;
  }
  static float Ease5(float value){float t=Mathf.Clamp01(value);return t*t*t*(t*(t*6-15)+10);}
  static Vector3 WalkingFoot(float phase){
   phase=Mathf.Repeat(phase,1);float z,y=0;
   // Stance occupies half a stride: backwards foot travel exactly cancels root
   // movement in world space. The other half is a lifted, eased return swing.
   if(phase<.5f)z=StrideLength*(.25f-phase);
   else{float u=(phase-.5f)*2;z=Mathf.Lerp(-StrideLength*.25f,StrideLength*.25f,Mathf.SmoothStep(0,1,u));y=Mathf.Sin(u*Mathf.PI)*.09f;}
   return new Vector3(0,y,z);
  }
  static FootballPose WalkPose(float distance){
   var pose=FootballMotionLibrary.Ready;float phase=distance/StrideLength,s=Mathf.Sin(phase*Mathf.PI*2),c=Mathf.Cos(phase*Mathf.PI*2);
   pose.body=new Vector3(3,0,s*1.3f);pose.pelvis=new Vector3(0,-.02f+Mathf.Abs(s)*.014f,0);pose.head=new Vector3(-2,-s*2,0);
   pose.leftFoot=WalkingFoot(phase);pose.rightFoot=WalkingFoot(phase+.5f);
   pose.leftHand=new Vector3(-.24f,.66f,.07f-c*.12f);pose.rightHand=new Vector3(.24f,.66f,.07f+c*.12f);return pose;
  }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
  public float ReviewSettleAt=>SettleAt;
  float? reviewAge;
  public void ReviewTime(float seconds){reviewAge=seconds;skipAt=-1;}
  public bool BeginFishingPreview(FishingGame source,bool crew=false,bool tied=false){
   if(!source||!source.Context||!AppRoot.Instance.LocalAthlete)return false;
   var count=crew?5:3;var records=Enumerable.Range(0,count).Select(i=>((ulong)i+1,"Preview "+(i+1))).ToArray();
   var ranks=crew?null:records.ToDictionary(r=>r.Item1,r=>tied?1:(int)r.Item1);var spot=source.transform.position;spot.y=source.WaterY+.02f;var toward=Vector3.ProjectOnPlane(source.Pier(0)-spot,Vector3.up).normalized;
   return BeginAwards(source.transform,spot,Quaternion.LookRotation(toward),records,crew?"CREW CATCH COMPLETE!":tied?"SHARED HONOURS!":"ROUND COMPLETE!","LAGOON ROUND RESULTS",()=>source&&source.Context,null,ranks,AppRoot.Instance.LocalAthlete);
  }
#endif
 }
}
