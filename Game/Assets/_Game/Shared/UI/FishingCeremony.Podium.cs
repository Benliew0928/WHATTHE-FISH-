using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WhatTheFish {
 public sealed partial class FishingCeremony {
  readonly List<int> castRanks=new();Transform podium;Mesh mistMesh;MeshRenderer mistRenderer;
  readonly List<Vector3> mistVertices=new();readonly List<Color> mistColors=new();readonly List<Vector3> mistUV=new();readonly List<int> mistIndices=new();
  static readonly Vector2[] MistCorners={new(0,0),new(1,0),new(1,1),new(0,1)};
  const float WalkSpeed=.82f,QueueGap=1.85f,StrideLength=.92f;
  const float EntryDelay=QueueGap/WalkSpeed;
  readonly Dictionary<Font,Material> numberMaterials=new();
  TextMesh[] podiumNumbers;Renderer[] numberRenderers;
  float WalkAt=>rankedGolf?1.5f:4.8f;
  float SettleAt=>MenuPreferences.ReducedMotion?.65f:WalkAt+Mathf.Max(7.2f,EntryDelay*(cast.Count-1)+1.2f);
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
   if(!rankedGolf){
    var smoke=new GameObject("Soft lagoon reveal mist");smoke.transform.SetParent(stage,false);mistMesh=new Mesh{name="Animated ceremony mist"};mistMesh.MarkDynamic();smoke.AddComponent<MeshFilter>().sharedMesh=mistMesh;mistRenderer=smoke.AddComponent<MeshRenderer>();mistRenderer.sharedMaterial=Resources.Load<Material>("GolfCelebration");mistRenderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;mistRenderer.receiveShadows=false;
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
   if(mistMesh)Mist(age,reduced);
   for(int i=0;i<cast.Count;i++){
    var actor=cast[i];bool show=PodiumSettled||age>=walkAt+i*EntryDelay;actor.Root.gameObject.SetActive(show);if(!show)continue;
    var seat=ActorSeat(i);
    if(PodiumSettled){actor.Root.localPosition=seat;actor.Root.localRotation=Quaternion.identity;var take=takes[i%3];actor.Pose(take.Sample((age-settleAt+i*.4f)/take.duration));}
    else{
     float travelled=Mathf.Max(0,age-walkAt-i*EntryDelay)*WalkSpeed;
     var at=new Vector3(0,.38f,(rankedGolf?11.8f:13.1f)-travelled);
     if(rankedGolf){var world=stage.TransformPoint(at);if(Physics.Raycast(world+Vector3.up*10,Vector3.down,out var ground,20,1<<8,QueryTriggerInteraction.Ignore))at.y=stage.InverseTransformPoint(ground.point).y+.015f;else at.y=0;}
     actor.Root.localPosition=at;actor.Root.localRotation=Quaternion.Euler(0,180,0);actor.Pose(WalkPose(travelled));
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
   }else{
    float progress=Ease5((age-walkAt)/(settleAt-walkAt));
    // Track alongside the single-file entrance before the ellipsis to the seats.
    position=Vector3.Lerp(new Vector3(8.5f,5.4f,19.5f),new Vector3(7.2f,4.9f,15.8f),progress);
    focus=Vector3.Lerp(new Vector3(0,1.2f,8.4f),new Vector3(0,1.35f,5.4f),progress);
    position=FrameEntrance(position,focus,age);
   }
   position=stage.TransformPoint(position);focus=stage.TransformPoint(focus);camera.transform.SetPositionAndRotation(position,Quaternion.LookRotation(focus-position));camera.fieldOfView=42;camera.nearClipPlane=.12f;
   // A brief dissolve hides the time ellipsis; no full bridge traversal required.
   if(!reduced){float cut=Mathf.Max(0,1-Mathf.Abs(age-settleAt)/.3f);if(cut>curtain.color.a)curtain.color=new Color(.025f,.07f,.11f,cut);}
  }
  Vector3 FrameEntrance(Vector3 position,Vector3 focus,float age){
   // Fit the whole queue above the result buttons and below the title. Include
   // waiting actors at their spawn so each entrance cannot pop the camera back.
   var rotation=Quaternion.LookRotation(focus-position);var inverse=Quaternion.Inverse(rotation);float back=0;
   float vertical=Mathf.Tan(21*Mathf.Deg2Rad),horizontal=vertical*Mathf.Max(.65f,camera.aspect);
   void Include(Vector3 point){var p=inverse*(point-position);back=Mathf.Max(back,Mathf.Abs(p.x)/(horizontal*.88f)-p.z,Mathf.Abs(p.y)/(vertical*.55f)-p.z);}
   foreach(int side in new[]{-1,1}){Include(new Vector3(side*5,.2f,0));Include(new Vector3(side*1.7f,2.5f,-2));}
   for(int i=0;i<cast.Count;i++){
    float z=(rankedGolf?11.8f:13.1f)-Mathf.Max(0,age-WalkAt-i*EntryDelay)*WalkSpeed;
    Include(new Vector3(-.4f,0,z));Include(new Vector3(.4f,2.1f,z));
   }
   return position-rotation*Vector3.forward*Mathf.Max(0,back);
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
  void Mist(float age,bool reduced){
   float opacity=reduced?0:Mathf.SmoothStep(0,1,(age-.6f)/1.65f)*(1-Mathf.SmoothStep(0,1,(age-3.3f)/2.1f));mistRenderer.enabled=opacity>.005f;if(!mistRenderer.enabled)return;
   mistVertices.Clear();mistColors.Clear();mistUV.Clear();mistIndices.Clear();var right=stage.InverseTransformDirection(camera.transform.right);var up=stage.InverseTransformDirection(camera.transform.up);
   for(int i=0;i<42;i++){
    float a=i*2.39996f+age*.1f,r=Mathf.Sqrt((i+.5f)/42)*4.8f;var p=new Vector3(Mathf.Cos(a)*r,.55f+(i%5)*.38f+age*.12f,Mathf.Sin(a)*r*.65f);float size=1.4f+Mathf.Sin(i*2.7f+age*.55f)*.35f;int start=mistVertices.Count;
    foreach(var uv in MistCorners){mistVertices.Add(p+right*(uv.x-.5f)*size*2+up*(uv.y-.5f)*size*1.6f);mistUV.Add(new Vector3(uv.x,uv.y,1));float shade=.67f+(i%3)*.035f;mistColors.Add(new Color(shade,shade+.015f,shade+.025f,opacity*.62f));}
    mistIndices.Add(start);mistIndices.Add(start+1);mistIndices.Add(start+2);mistIndices.Add(start);mistIndices.Add(start+2);mistIndices.Add(start+3);
   }
   mistMesh.Clear();mistMesh.SetVertices(mistVertices);mistMesh.SetColors(mistColors);mistMesh.SetUVs(0,mistUV);mistMesh.SetTriangles(mistIndices,0);mistMesh.RecalculateBounds();
  }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
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
