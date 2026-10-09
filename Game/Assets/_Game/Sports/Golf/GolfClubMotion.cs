using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace WhatTheFish {
 public enum GolfClubAction:byte { Carry,Charge,Swing }
 public struct GolfClubState:INetworkSerializable,IEquatable<GolfClubState> {
  public GolfClubAction action;public uint round,sequence;public double started;public float heading,charge;public Vector3 target;
  public void NetworkSerialize<T>(BufferSerializer<T> s) where T:IReaderWriter {s.SerializeValue(ref action);s.SerializeValue(ref round);s.SerializeValue(ref sequence);s.SerializeValue(ref started);s.SerializeValue(ref heading);s.SerializeValue(ref charge);s.SerializeValue(ref target);}
  public bool Equals(GolfClubState v)=>action==v.action&&round==v.round&&sequence==v.sequence&&started==v.started&&heading==v.heading&&charge==v.charge&&target==v.target;
 }

 // Bone presentation shares one clock with authoritative ball contact. Only
 // the arms/torso change; locomotion and the existing character rig remain.
 [DefaultExecutionOrder(120)]
 public sealed class GolfClubMotion:MonoBehaviour {
  public const float ContactTime=.10f,Duration=.74f,SwingReach=1.1f;
  public static double Clock=>NetworkManager.Singleton&&NetworkManager.Singleton.IsListening?NetworkManager.Singleton.ServerTime.Time:Time.timeAsDouble;
  public GolfClubState State {get;private set;}
  public GolfClubModel Club {get;private set;}
  public bool RigReady {get;private set;}
  public bool Equipped=>Club&&Club.gameObject.activeSelf;
  public bool Busy=>State.action==GolfClubAction.Swing&&Elapsed<Duration&&InRound;
  public float Elapsed=>Mathf.Max(0,(float)(Clock-State.started));
  public bool TwoHanded {get;private set;}
  public float GripError {get;private set;}
  public float CarryWristDeviation {get;private set;}
  public bool GripShapeReady=>gripSkins!=null&&gripSkins.Length>0;
  public Vector3 LeftPalm=>Palm(left);
  public Vector3 RightPalm=>Palm(right);
  public bool InRound=>GolfMatchManager.Instance&&State.round==GolfMatchManager.Instance.Round;
  public float AddressHeading=>HeadingTo(State.target);
  public bool ShowBodyInFirstPerson=>Available()&&Club;
  [Header("Club carry")]
  [Range(3,25)] public float carryDownAngle=12;
  [Range(30,70)] public float carryRaisedDownAngle=55;
  [Range(.03f,.15f)] public float carryGroundClearance=.06f;
  [Range(0,1)] public float carryWristSway=.18f;
  Athlete athlete;Transform hips,spine,chest,head;Limb left,right;Vector3 headScale;bool headHidden;
  Transform[] bones;Quaternion[] rotations;Vector3[] positions;bool applied;float twoHands;
  Vector3 headOffset;static GolfClubModel prefab;
  Transform gripSocket;SkinnedMeshRenderer[] gripSkins;int[] gripShapes;bool gripClosed;
  sealed class Limb {public Transform upper,lower,end;public Vector3 palmOffset,fingerAxis,palmAxis,shoulderOffset;public float reach;public readonly List<Transform> fingers=new();}
  public void Bind(Athlete owner){
   if(RigReady)return;athlete=owner;if(!owner.visual)return;bones=owner.visual.GetComponentsInChildren<Transform>(true);
   Transform Bone(string n)=>Array.Find(bones,t=>t.name=="mixamorig:"+n);
   hips=Bone("Hips");spine=Bone("Spine");chest=Bone("Spine2");head=Bone("Head");
   Limb Make(string side){
    var a=new Limb{upper=Bone(side+"Arm"),lower=Bone(side+"ForeArm"),end=Bone(side+"Hand")};if(!a.upper||!a.lower||!a.end)return null;
    a.shoulderOffset=transform.InverseTransformPoint(a.upper.position);a.reach=Vector3.Distance(a.upper.position,a.lower.position)+Vector3.Distance(a.lower.position,a.end.position);
    var middle=Bone(side+"HandMiddle1")??Bone(side+"HandMiddle4");var index=Bone(side+"HandIndex1");var pinky=Bone(side+"HandPinky1");
    var along=middle?middle.position-a.end.position:(a.end.position-a.lower.position).normalized*.08f;
    var normal=index&&pinky?Vector3.Cross(index.position-a.end.position,pinky.position-a.end.position).normalized:-transform.right;if(side=="Left")normal=-normal;
    a.palmOffset=a.end.InverseTransformVector(along*.52f);a.fingerAxis=a.end.InverseTransformDirection(along.normalized);a.palmAxis=a.end.InverseTransformDirection(normal);
    foreach(var b in bones)if(b.name.StartsWith("mixamorig:"+side+"Hand")&&b!=a.end&&b.childCount>0&&!b.name.EndsWith("_end"))a.fingers.Add(b);return a;
   }
   left=Make("Left");right=Make("Right");if(left==null||right==null||!hips||!spine||!chest)return;
   gripSocket=Bone("RightHand").Find("Golf grip socket");
   if(gripSocket){right.palmOffset=right.end.InverseTransformPoint(gripSocket.position);right.palmAxis=right.end.InverseTransformDirection(gripSocket.up);}
   gripSkins=Array.FindAll(owner.visual.GetComponentsInChildren<SkinnedMeshRenderer>(true),s=>s.sharedMesh&&s.sharedMesh.GetBlendShapeIndex("GolfRightGrip")>=0);
   gripShapes=Array.ConvertAll(gripSkins,s=>s.sharedMesh.GetBlendShapeIndex("GolfRightGrip"));
   rotations=new Quaternion[bones.Length];positions=new Vector3[bones.Length];RigReady=true;
  }
  public void Equip(uint round){
   if(!RigReady)return;
   if(!Club){if(!prefab)prefab=Resources.Load<GolfClubModel>("GolfClub");if(!prefab)return;Club=Instantiate(prefab,transform);Club.name="Midnight Iron";headOffset=Club.transform.InverseTransformPoint(Club.head.position);}
   if(State.round!=round)State=new GolfClubState{round=round,sequence=State.sequence+1};
  }
  public void Charge(bool value,float heading,Vector3 target,uint round){
   if(Busy)return;
   if(value){if(State.action!=GolfClubAction.Charge||State.round!=round)State=new GolfClubState{action=GolfClubAction.Charge,round=round,sequence=State.sequence+1,started=Clock,heading=heading,target=target};
    else {var s=State;s.heading=heading;s.target=target;State=s;}}
   else if(State.action==GolfClubAction.Charge){var s=State;s.action=GolfClubAction.Carry;s.sequence++;State=s;}
  }
  public void Swing(float heading,float charge,Vector3 target,uint round){State=new GolfClubState{action=GolfClubAction.Swing,round=round,sequence=State.sequence+1,started=Clock,heading=heading,charge=charge,target=target};}
  public void Receive(GolfClubState value){State=value;}
  public void ResetPose(){Restore();SetGrip(false);State=new GolfClubState{sequence=State.sequence+1};twoHands=0;TwoHanded=false;if(Club)Club.gameObject.SetActive(false);}
  void SetGrip(bool value){if(gripSkins==null||gripClosed==value)return;gripClosed=value;for(int i=0;i<gripSkins.Length;i++)gripSkins[i].SetBlendShapeWeight(gripShapes[i],value?100:0);}
  void Restore(){if(headHidden&&head){head.localScale=headScale;headHidden=false;}if(!applied)return;for(int i=0;i<bones.Length;i++)if(bones[i]){bones[i].localPosition=positions[i];bones[i].localRotation=rotations[i];}applied=false;}
  void Update(){Restore();}
  void OnDisable(){ResetPose();}
  bool Available(){var m=GolfMatchManager.Instance;return RigReady&&m&&m.Context&&m.State.Phase!=GolfMatchPhase.Idle&&m.Player(athlete)!=null&&!athlete.inTransit&&!GolfCartWorld.Driving(athlete);}
  static float Ease(float v){v=Mathf.Clamp01(v);return v*v*(3-2*v);}
  Vector3 Palm(Limb a)=>RigReady?a.end.TransformPoint(a.palmOffset):transform.position;
  Vector3 ContactGrip(Vector3 target){
   var shoulder=(left.upper.position+right.upper.position)*.5f;
   // The closest grip to both shoulders also reaches a ball beside the feet.
   // A fixed forward grip makes very close balls harder to reach than distant ones.
   return target+(shoulder-target).normalized*headOffset.magnitude;
  }
  float HeadingTo(Vector3 target){var delta=target-transform.position;return delta.x*delta.x+delta.z*delta.z>.0001f?Mathf.Atan2(delta.x,delta.z)*Mathf.Rad2Deg:transform.eulerAngles.y;}
  public void Address(Vector3 target){if(!athlete)return;float heading=HeadingTo(target);athlete.Motor.FaceGolf(heading);transform.rotation=Quaternion.Euler(0,heading,0);}
  public bool CanAddress(Vector3 target){
   if(!Club||!RigReady)return false;
   // Interaction depends on proximity, not the preceding animation pose or yaw.
   var delta=target-transform.position;
   return delta.x*delta.x+delta.z*delta.z<=SwingReach*SwingReach&&Mathf.Abs(delta.y)<.7f;
  }
  Quaternion ContactRotation(Vector3 target,Vector3 grip,Quaternion facing){
   var axis=(target-grip).normalized;var rotation=Quaternion.FromToRotation(headOffset,axis);
   var a=Vector3.ProjectOnPlane(rotation*Vector3.forward,axis);var b=Vector3.ProjectOnPlane(facing*Vector3.forward,axis);
   if(a.sqrMagnitude>.001f&&b.sqrMagnitude>.001f)rotation=Quaternion.AngleAxis(Vector3.SignedAngle(a,b,axis),axis)*rotation;return rotation;
  }
  void LateUpdate(){
   if(!Available()){SetGrip(false);if(Club)Club.gameObject.SetActive(false);twoHands=0;TwoHanded=false;return;}
   Equip(GolfMatchManager.Instance.Round);if(!Club)return;Club.gameObject.SetActive(true);
   SetGrip(true);
   var view=PlayerView.Instance;bool localCharge=view&&view.target==athlete&&view.GolfCharging;
   bool charge=localCharge||InRound&&State.action==GolfClubAction.Charge;bool swing=Busy;
   float amount=localCharge?view.GolfDisplayedCharge:charge?GolfShotRules.Power(Elapsed):State.charge;
   twoHands=Mathf.MoveTowards(twoHands,charge||swing?1:0,Time.deltaTime*14);TwoHanded=twoHands>.99f;
   Vector3 target=State.target;
   if(localCharge){var ball=GolfMatchManager.Instance.Ball(view.GolfBallOwner);if(ball)target=ball.Body.position;}
   var facing=Quaternion.Euler(0,charge||swing?HeadingTo(target):transform.eulerAngles.y,0);
   // Present the addressed stance after network transform interpolation, too.
   // The authoritative motor still controls movement and the shot heading.
   if(charge||swing)transform.rotation=facing;
   for(int i=0;i<bones.Length;i++){positions[i]=bones[i].localPosition;rotations[i]=bones[i].localRotation;}applied=true;
   var contact=ContactGrip(target);var contactRotation=ContactRotation(target,contact,facing);
   var high=(left.upper.position+right.upper.position)*.5f+facing*new Vector3(0,-.22f,.20f);
   float backswing=Mathf.Lerp(-28,-132,amount),angle=backswing;var grip=high;
   if(swing){
    if(Elapsed<ContactTime){float t=Ease(Elapsed/ContactTime);grip=Vector3.Lerp(high,contact,t);angle=Mathf.Lerp(backswing,0,t);}
    else {float t=Ease((Elapsed-ContactTime)/.34f);grip=Vector3.Lerp(contact,high+facing*new Vector3(-.16f,.12f,0),t);angle=Mathf.Lerp(0,156,t);}
   }
   // Sweep across the golfer's body. Rotating around the forward axis lifts
   // the head on both sides of contact instead of dipping it into the turf.
   spine.rotation=Quaternion.AngleAxis(10*twoHands,facing*Vector3.right)*spine.rotation;
   chest.rotation=Quaternion.AngleAxis((swing?Mathf.Lerp(-18,24,Ease(Elapsed/.45f)):-12*amount)*twoHands,Vector3.up)*chest.rotation;
   // Keep the arm's animated travel. A raised hand carries the shaft down and
   // back; distribute pronation through the forearm instead of folding the wrist.
   var animatedWrist=right.end.localRotation;
   var nativeCarry=right.end.rotation*Quaternion.LookRotation(right.fingerAxis,right.palmAxis)*Quaternion.Inverse(Quaternion.LookRotation(-Vector3.right,-Vector3.forward));
   var nativeFingers=right.end.TransformDirection(right.fingerAxis);
   var carryAxis=(nativeCarry*(headOffset-Club.rightGrip.localPosition)).normalized;
   float lift=Ease(Mathf.InverseLerp(.08f,.58f,(RightPalm.y-right.upper.position.y+right.reach)/right.reach));
   float down=Mathf.Lerp(carryDownAngle,carryRaisedDownAngle,lift)*Mathf.Deg2Rad;
   var flat=Vector3.ProjectOnPlane(carryAxis,Vector3.up);
   float yaw=flat.sqrMagnitude>.001f?Vector3.SignedAngle(-transform.forward,flat,Vector3.up):0;
   var back=Quaternion.AngleAxis(Mathf.Clamp(yaw*carryWristSway,-12,12),Vector3.up)*(-transform.forward);
   var carryRotation=nativeCarry;var carryHand=right.end.rotation;
   float shaftLength=Vector3.Scale(headOffset-Club.rightGrip.localPosition,Club.transform.lossyScale).magnitude;
   for(int step=0;step<3;step++){
    var direction=back*Mathf.Cos(down)-Vector3.up*Mathf.Sin(down);
    carryRotation=Quaternion.FromToRotation(carryAxis,direction)*nativeCarry;
    var from=Vector3.ProjectOnPlane(-(carryRotation*Vector3.right),direction);var to=Vector3.ProjectOnPlane(nativeFingers,direction);
    if(from.sqrMagnitude>.001f&&to.sqrMagnitude>.001f)carryRotation=Quaternion.AngleAxis(Vector3.SignedAngle(from,to,direction),direction)*carryRotation;
    carryHand=HandRotation(right,-(carryRotation*Vector3.forward),-(carryRotation*Vector3.right));
    var palm=right.end.position+carryHand*Vector3.Scale(right.palmOffset,right.end.lossyScale);
    float safe=Mathf.Asin(Mathf.Clamp01((palm.y-transform.position.y-carryGroundClearance)/shaftLength));
    if(down<=safe||step==2)break;down=safe;
   }
   var correction=carryHand*Quaternion.Inverse(right.end.rotation);
   var forearmAxis=(right.end.position-right.lower.position).normalized;
   var projected=Vector3.Project(new Vector3(correction.x,correction.y,correction.z),forearmAxis);
   var twist=new Quaternion(projected.x,projected.y,projected.z,correction.w);
   float norm=Mathf.Sqrt(Quaternion.Dot(twist,twist));if(norm>.0001f){twist=new Quaternion(twist.x/norm,twist.y/norm,twist.z/norm,twist.w/norm);right.lower.rotation=Quaternion.Slerp(Quaternion.identity,twist,1-twoHands)*right.lower.rotation;}
   right.end.rotation=Quaternion.Slerp(right.end.rotation,carryHand,1-twoHands);
   var carryGrip=RightPalm-carryRotation*Vector3.Scale(Club.rightGrip.localPosition,Club.transform.lossyScale);
   var rotation=Quaternion.AngleAxis(angle,-(facing*Vector3.forward))*contactRotation;
   var position=Vector3.Lerp(carryGrip,grip,twoHands);rotation=Quaternion.Slerp(carryRotation,rotation,twoHands);
   Club.transform.SetPositionAndRotation(position,rotation);
   if(twoHands>0)Arm(right,Club.rightGrip.position,-Club.transform.forward,-Club.transform.right,transform.right-Vector3.up*.7f,twoHands);
   if(!GripShapeReady)Curl(right,right.end.TransformDirection(right.palmAxis),1);
   CarryWristDeviation=Quaternion.Angle(animatedWrist,right.end.localRotation);
   Club.transform.position+=RightPalm-Club.rightGrip.position;
   if(twoHands>0)Arm(left,Club.leftGrip.position,Club.transform.forward,Club.transform.right,-transform.right-Vector3.up*.7f,twoHands);
   GripError=Vector3.Distance(RightPalm,Club.rightGrip.position);if(TwoHanded)GripError=Mathf.Max(GripError,Vector3.Distance(LeftPalm,Club.leftGrip.position));
   if(head&&view&&view.active&&view.target==athlete&&view.mode==0&&!view.GolfAiming){headScale=head.localScale;head.localScale=Vector3.one*.001f;headHidden=true;}
  }
  void Arm(Limb a,Vector3 palm,Vector3 normal,Vector3 fingers,Vector3 pole,float blend){
   var rotation=HandRotation(a,normal,fingers);
   var wrist=palm-rotation*Vector3.Scale(a.palmOffset,a.end.lossyScale);
   var start=a.upper.position;var elbow=a.lower.position;var end=a.end.position;float upper=Vector3.Distance(start,elbow),lower=Vector3.Distance(elbow,end);var delta=wrist-start;
   float d=Mathf.Clamp(delta.magnitude,Mathf.Abs(upper-lower)+.002f,upper+lower-.005f);var direction=delta.normalized;
   float along=(upper*upper-lower*lower+d*d)/(2*d);var bend=Vector3.ProjectOnPlane(pole,direction).normalized;
   var joint=start+direction*along+bend*Mathf.Sqrt(Mathf.Max(0,upper*upper-along*along));
   var before=a.upper.rotation;a.upper.rotation=Quaternion.Slerp(before,Quaternion.FromToRotation(elbow-start,joint-start)*before,blend);
   before=a.lower.rotation;a.lower.rotation=Quaternion.Slerp(before,Quaternion.FromToRotation(a.end.position-a.lower.position,start+direction*d-a.lower.position)*before,blend);
   a.end.rotation=Quaternion.Slerp(a.end.rotation,rotation,blend);
   if(a==left)Curl(a,normal,blend);
  }
  static Quaternion HandRotation(Limb a,Vector3 normal,Vector3 fingers)=>Quaternion.LookRotation(fingers,normal)*Quaternion.Inverse(Quaternion.LookRotation(a.fingerAxis,a.palmAxis));
  static void Curl(Limb a,Vector3 normal,float blend){foreach(var f in a.fingers){var axis=Vector3.Cross(f.GetChild(0).position-f.position,normal).normalized;f.rotation=Quaternion.AngleAxis((f.name.Contains("Thumb")?14:f.name.EndsWith("1")?28:48)*blend,axis)*f.rotation;}}
 }
}
