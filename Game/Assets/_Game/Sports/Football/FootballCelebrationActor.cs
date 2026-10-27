using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
namespace WhatTheFish {
 // Render-only cast: no Athlete, network object, collider or gameplay script.
 // Running optionally uses a manually evaluated animation graph on cloned bones.
 internal sealed class FootballCelebrationActor {
  public readonly Transform Root;
  public readonly ulong Id;
  public readonly FootballTeam Team;
  public bool Ready {get;private set;}
  public float ReachError {get;private set;}
  public float BoneError {get;private set;}
  readonly Dictionary<Transform,Transform> map=new();
  AnimationClip runClip;GameObject runTarget;Avatar runAvatar;CelebrationRunSampler runSampler;
  Transform[] bones;Vector3[] positions;Quaternion[] rotations;
  Transform hips,spine,chest,head;Vector3 hip;Limb[] legs,arms;float soleOffset;
  sealed class Limb {
   public Transform upper,lower,end;public float a,b;public Quaternion upperBasis,lowerBasis,wrist,foot,palmBasis;public Vector3 ankle;
  }
  public FootballCelebrationActor(Athlete source,FootballPlayerRecord record,Transform parent){
   Id=record.id;Team=record.team;Root=new GameObject("Honours #"+record.number+" "+record.name).transform;Root.SetParent(parent,false);
   source.Setup();CharacterIdentity.Ensure(source).Refresh();
   foreach(var t in source.visual.GetComponentsInChildren<Transform>(true)){
    var copy=new GameObject(t.name).transform;map[t]=copy;copy.SetParent(t==source.visual?Root:map[t.parent],false);
    copy.gameObject.layer=t.gameObject.layer;copy.localPosition=t==source.visual?Vector3.zero:t.localPosition;copy.localRotation=t.localRotation;copy.localScale=t==source.visual?t.lossyScale:t.localScale;
   }
   var sourceAnimator=source.GetComponentInChildren<Animator>();
   if(sourceAnimator&&sourceAnimator.runtimeAnimatorController&&map.TryGetValue(sourceAnimator.transform,out var target)){
    runClip=sourceAnimator.runtimeAnimatorController.animationClips.FirstOrDefault(c=>c.name=="Running");runTarget=target.gameObject;runAvatar=sourceAnimator.avatar;
   }
   source.FootballMotion.CopyReferencePose(map);
   var block=new MaterialPropertyBlock();var renderers=new Dictionary<Renderer,Renderer>();
   foreach(var r in source.visual.GetComponentsInChildren<Renderer>(true)){
    Renderer clone=null;var go=map[r.transform].gameObject;
    if(r is SkinnedMeshRenderer skin&&skin.sharedMesh){
     var s=go.AddComponent<SkinnedMeshRenderer>();s.sharedMesh=skin.sharedMesh;s.bones=Array.ConvertAll(skin.bones,b=>b&&map.TryGetValue(b,out var v)?v:null);s.rootBone=skin.rootBone&&map.TryGetValue(skin.rootBone,out var root)?root:Root;s.localBounds=skin.localBounds;s.updateWhenOffscreen=true;s.quality=skin.quality;
     for(int i=0;i<skin.sharedMesh.blendShapeCount;i++)s.SetBlendShapeWeight(i,skin.GetBlendShapeWeight(i));clone=s;
    }else if(r is MeshRenderer&&r.TryGetComponent<MeshFilter>(out var filter)){go.AddComponent<MeshFilter>().sharedMesh=filter.sharedMesh;clone=go.AddComponent<MeshRenderer>();}
    if(!clone)continue;clone.sharedMaterials=r.sharedMaterials;clone.shadowCastingMode=r.shadowCastingMode;clone.receiveShadows=r.receiveShadows;r.GetPropertyBlock(block);clone.SetPropertyBlock(block);
    // LODGroup controls near/distant renderers; a first-person source may have
    // disabled every renderer, so visibility cannot be inherited from it.
    clone.enabled=true;renderers[r]=clone;
   }
   foreach(var group in source.visual.GetComponentsInChildren<LODGroup>(true)){
    var lod=map[group.transform].gameObject.AddComponent<LODGroup>();var levels=group.GetLODs();
    for(int i=0;i<levels.Length;i++)levels[i].renderers=levels[i].renderers.Where(r=>r&&renderers.ContainsKey(r)).Select(r=>renderers[r]).ToArray();
    lod.SetLODs(levels);lod.localReferencePoint=group.localReferencePoint;lod.size=group.size;lod.ForceLOD(0);
   }
   CharacterIdentity.ApplyHonoursIdentity(Root,record.number,record.team);
   bones=Root.GetComponentsInChildren<Transform>().Where(t=>t.name.StartsWith("mixamorig:")).ToArray();
   Transform Bone(string name)=>Array.Find(bones,t=>t.name=="mixamorig:"+name);
   hips=Bone("Hips");spine=Bone("Spine");chest=Bone("Spine2");head=Bone("Head");
   Limb Make(string side,bool arm){
    var l=new Limb{upper=Bone(side+(arm?"Arm":"UpLeg")),lower=Bone(side+(arm?"ForeArm":"Leg")),end=Bone(side+(arm?"Hand":"Foot"))};if(!l.upper||!l.lower||!l.end)return l;
    var u=l.lower.position-l.upper.position;var d=l.end.position-l.lower.position;var n=Vector3.Cross(u,d).normalized;if(n.sqrMagnitude<.01f)n=Root.right;
    l.a=u.magnitude;l.b=d.magnitude;l.upperBasis=Quaternion.Inverse(Quaternion.LookRotation(u,n))*l.upper.rotation;l.lowerBasis=Quaternion.Inverse(Quaternion.LookRotation(d,n))*l.lower.rotation;
    l.wrist=Quaternion.Inverse(l.lower.rotation)*l.end.rotation;l.foot=Quaternion.Inverse(Root.rotation)*l.end.rotation;l.ankle=Root.InverseTransformPoint(l.end.position);
    if(arm){var middle=Bone(side+"HandMiddle3");var index=Bone(side+"HandIndex1");var pinky=Bone(side+"HandPinky1");if(middle&&index&&pinky){var direction=(middle.position-l.end.position).normalized;var normal=Vector3.Cross(direction,index.position-pinky.position).normalized*(side=="Left"?1:-1);l.palmBasis=Quaternion.Inverse(Quaternion.LookRotation(direction,normal))*l.end.rotation;}else l.palmBasis=Quaternion.identity;
    }return l;
   }
   legs=new[]{Make("Left",false),Make("Right",false)};arms=new[]{Make("Left",true),Make("Right",true)};
   Ready=source.FootballMotion.RigReady&&hips&&spine&&chest&&head&&legs.All(l=>l.end)&&arms.All(l=>l.end);
   if(!Ready){Root.gameObject.SetActive(false);return;}
   hip=Root.InverseTransformPoint(hips.position);positions=bones.Select(t=>t.localPosition).ToArray();rotations=bones.Select(t=>t.localRotation).ToArray();
   // Calibrate to the actual rendered soles, not the controller clearance.
   var mesh=new Mesh();float floor=float.PositiveInfinity;
   foreach(var skin in Root.GetComponentsInChildren<SkinnedMeshRenderer>()){skin.BakeMesh(mesh);foreach(var vertex in mesh.vertices)floor=Mathf.Min(floor,Root.InverseTransformPoint(skin.transform.TransformPoint(vertex)).y);}
   UnityEngine.Object.Destroy(mesh);soleOffset=float.IsFinite(floor)?Mathf.Clamp(floor,0,.25f):0;
  }
  public bool HasRunningClip=>runClip&&runTarget;
  public void Run(float seconds){
   if(!Ready||!HasRunningClip)return;
   var t=runTarget.transform;var position=t.localPosition;var rotation=t.localRotation;var scale=t.localScale;
   if(!runSampler){runSampler=runTarget.AddComponent<CelebrationRunSampler>();runSampler.Bind(runAvatar,runClip);}
   runSampler.Sample(Mathf.Repeat(seconds,runClip.length));
   t.SetLocalPositionAndRotation(position,rotation);t.localScale=scale;
  }
  public void Pose(FootballPose pose){
   if(!Ready)return;ReachError=BoneError=0;
   for(int i=0;i<bones.Length;i++){bones[i].localPosition=positions[i];bones[i].localRotation=rotations[i];}
   hips.position=Root.TransformPoint(hip+pose.pelvis-Vector3.up*soleOffset);Rotate(hips,pose.body);Rotate(spine,pose.chest*.4f);Rotate(chest,pose.chest*.6f);Rotate(head,pose.head-pose.body*.22f);
   for(int i=0;i<2;i++){
    var leg=legs[i];var target=Root.TransformPoint(leg.ankle+(i==0?pose.leftFoot:pose.rightFoot)-Vector3.up*soleOffset);
    Solve(leg,target,Root.TransformDirection(new Vector3(i==0?-.12f:.12f,.2f,1)));leg.end.rotation=Root.rotation*leg.foot;
    var arm=arms[i];var hand=i==0?pose.leftHand:pose.rightHand;float sign=i==0?-1:1;
    // Elbows stay outside the ribs while high gestures open the shoulders.
    Solve(arm,Root.TransformPoint(hand-Vector3.up*soleOffset),Root.TransformDirection(new Vector3(sign*.85f,-.35f,-.35f)));
    float applause=Mathf.SmoothStep(0,1,Mathf.InverseLerp(.18f,.27f,Mathf.Min(pose.leftHand.z,pose.rightHand.z))*Mathf.InverseLerp(.30f,.20f,Mathf.Max(Mathf.Abs(pose.leftHand.x),Mathf.Abs(pose.rightHand.x))));
    float high=Mathf.SmoothStep(0,1,Mathf.InverseLerp(.98f,1.10f,hand.y));
    var palm=Vector3.Slerp(Root.forward,Root.right*-sign,applause);var fingers=(Root.up+Root.forward*.12f).normalized;
    arm.end.rotation=Quaternion.Slerp(arm.lower.rotation*arm.wrist,Quaternion.LookRotation(fingers,palm)*arm.palmBasis,Mathf.Max(high,applause));
   }
   for(int i=0;i<4;i++){var l=i<2?legs[i]:arms[i-2];BoneError=Mathf.Max(BoneError,Mathf.Abs(Vector3.Distance(l.upper.position,l.lower.position)-l.a),Mathf.Abs(Vector3.Distance(l.lower.position,l.end.position)-l.b));}
  }
  // Awards-only locomotion: hanging arms swing from the shoulder without the
  // bent-elbow celebration IK target. Keep wrists aligned with the forearms.
  public void Walk(FootballPose pose,float stridePhase){
   Pose(pose);if(!Ready)return;
   float swing=Mathf.Cos(stridePhase*Mathf.PI*2)*20*Mathf.Deg2Rad;
   for(int i=0;i<2;i++){
    var arm=arms[i];float sign=i==0?-1:1,angle=swing*sign;
    var axis=Root.TransformDirection(new Vector3(sign*.08f,-Mathf.Cos(angle),Mathf.Sin(angle))).normalized;
    var normal=Vector3.ProjectOnPlane(-Root.right,axis).normalized;
    arm.upper.rotation=Quaternion.LookRotation(axis,normal)*arm.upperBasis;
    arm.lower.rotation=Quaternion.LookRotation(axis,normal)*arm.lowerBasis;
    arm.end.rotation=arm.lower.rotation*arm.wrist;
    // Reverse the former outward roll at the shoulder and elbow together.
    // Preserve the reference wrist relation: this simplified rig has only
    // Hand/Middle4 bones, so finger-bone palm calibration is unavailable.
   }
  }
  void Rotate(Transform bone,Vector3 angle){bone.rotation=Root.rotation*Quaternion.Euler(angle)*Quaternion.Inverse(Root.rotation)*bone.rotation;}
  void Solve(Limb l,Vector3 target,Vector3 guide){
   var start=l.upper.position;var delta=target-start;float distance=delta.magnitude;if(distance<.0001f)return;
   float length=Mathf.Clamp(distance,Mathf.Abs(l.a-l.b)+.012f,l.a+l.b-.009f);var axis=delta/distance;var bend=Vector3.ProjectOnPlane(guide,axis).normalized;
   if(bend.sqrMagnitude<.01f)bend=Vector3.ProjectOnPlane(Root.forward,axis).normalized;
   float along=(l.a*l.a-l.b*l.b+length*length)/(2*length);var joint=start+axis*along+bend*Mathf.Sqrt(Mathf.Max(0,l.a*l.a-along*along));var normal=Vector3.Cross(bend,axis).normalized;
   l.upper.rotation=Quaternion.LookRotation(joint-start,normal)*l.upperBasis;l.lower.rotation=Quaternion.LookRotation(start+axis*length-joint,normal)*l.lowerBasis;ReachError=Mathf.Max(ReachError,Mathf.Abs(distance-length));
  }
 }
 // Imported Generic clips require an Animator binding in the player. Manual
 // evaluation keeps this local cast synchronized to the cinematic clock without
 // copying movement/network components or advancing the live player's Animator.
 internal sealed class CelebrationRunSampler:MonoBehaviour {
  PlayableGraph graph;AnimationClipPlayable clip;
  public void Bind(Avatar avatar,AnimationClip source){
   var animator=gameObject.AddComponent<Animator>();animator.avatar=avatar;animator.applyRootMotion=false;animator.cullingMode=AnimatorCullingMode.AlwaysAnimate;
   graph=PlayableGraph.Create("Golf showcase gameplay run");graph.SetTimeUpdateMode(DirectorUpdateMode.Manual);
   clip=AnimationClipPlayable.Create(graph,source);clip.SetApplyFootIK(false);var output=AnimationPlayableOutput.Create(graph,"Run",animator);output.SetSourcePlayable(clip);graph.Play();
  }
  public void Sample(float time){clip.SetTime(time);graph.Evaluate(0);}
  void OnDestroy(){if(graph.IsValid())graph.Destroy();}
 }
}
