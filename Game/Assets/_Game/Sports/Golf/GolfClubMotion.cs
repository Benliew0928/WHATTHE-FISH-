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
  public static double Clock=>NetworkManager.Singleton&&NetworkManager.Singleton.IsListening?RoomService.GameClock(NetworkManager.Singleton.ServerTime.Time):Time.timeAsDouble;
  public GolfClubState State {get;private set;}
  public GolfClubModel Club {get;private set;}
  public bool RigReady {get;private set;}
  public bool Equipped=>Club&&Club.gameObject.activeSelf;
  public bool Busy=>State.action==GolfClubAction.Swing&&Elapsed<Duration&&InRound;
  public float Elapsed=>Mathf.Max(0,(float)(Clock-State.started));
  public bool TwoHanded {get;private set;}
  public float GripError {get;private set;}
  public float LeftGripError {get;private set;}
  public float RightGripError {get;private set;}
  public float HeadTargetError {get;private set;}
  public bool LeftWristReachable {get;private set;}
  public bool RightWristReachable {get;private set;}
  public float LeftFingerShaftAngle {get;private set;}
  public float RightFingerShaftAngle {get;private set;}
  public float LeftForearmShaftAngle {get;private set;}
  public float RightForearmShaftAngle {get;private set;}
  public float LeftElbowAngle {get;private set;}
  public float RightElbowAngle {get;private set;}
  public float CarryWristDeviation {get;private set;}
  public float HeadPoseDeviation {get;private set;}
  public bool GripShapeReady=>gripSkins!=null&&gripSkins.Length>0;
  public Vector3 LeftPalm=>Palm(left);
  public Vector3 RightPalm=>Palm(right);
  public Vector3 LeftPalmNormal=>RigReady?left.end.TransformDirection(left.palmAxis):Vector3.zero;
  public Vector3 RightPalmNormal=>RigReady?right.end.TransformDirection(right.palmAxis):Vector3.zero;
  public Vector3 LeftShoulder=>RigReady?left.upper.position:transform.position;
  public Vector3 RightShoulder=>RigReady?right.upper.position:transform.position;
  public Vector3 LeftElbow=>RigReady?left.lower.position:transform.position;
  public Vector3 RightElbow=>RigReady?right.lower.position:transform.position;
  public float LeftHandHeightFromWaist=>RigReady?LeftPalm.y-hips.position.y:0;
  public float RightHandHeightFromWaist=>RigReady?RightPalm.y-hips.position.y:0;
  // Reach is measured to the wrist pivot. The palm center extends beyond the
  // forearm and can legitimately exceed the two-bone segment length.
  public float LeftArmReachFraction=>RigReady?Vector3.Distance(left.upper.position,left.end.position)/Mathf.Max(.001f,left.reach):0;
  public float RightArmReachFraction=>RigReady?Vector3.Distance(right.upper.position,right.end.position)/Mathf.Max(.001f,right.reach):0;
  public float LeftArmLength=>RigReady?left.reach:0;
  public float RightArmLength=>RigReady?right.reach:0;
  public float LeftRequestedReachFraction {get;private set;}
  public float RightRequestedReachFraction {get;private set;}
  public float HeadToRightGripLength=>Club?Vector3.Distance(Club.head.position,Club.rightGrip.position):0;
  public bool InRound=>GolfMatchManager.Instance&&State.round==GolfMatchManager.Instance.Round;
  public float AddressHeading=>HeadingTo(State.target);
  public bool ShowBodyInFirstPerson=>Available()&&Club;
  [Header("Club carry")]
  [Range(3,25)] public float carryDownAngle=12;
  [Range(30,70)] public float carryRaisedDownAngle=55;
  [Range(.03f,.15f)] public float carryGroundClearance=.06f;
  [Range(0,1)] public float carryWristSway=.18f;
  Athlete athlete;Transform hips,spine,chest,head;Limb left,right;
  Transform[] bones;Quaternion[] rotations;Vector3[] positions;bool applied;float twoHands;
  Vector3 headOffset,carryLeftGripLocal,carryRightGripLocal,carryClubScale;bool gripMarkersCaptured;static GolfClubModel prefab;
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
   if(!Club){if(!prefab)prefab=Resources.Load<GolfClubModel>("GolfClub");if(!prefab)return;Club=Instantiate(prefab,transform);Club.name="Midnight Iron";headOffset=Club.transform.InverseTransformPoint(Club.head.position);carryLeftGripLocal=Club.leftGrip.localPosition;carryRightGripLocal=Club.rightGrip.localPosition;carryClubScale=Club.transform.localScale;gripMarkersCaptured=true;}
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
  public void ResetPose(){Restore();SetGrip(false);SetAddressGripOffset(0);State=new GolfClubState{sequence=State.sequence+1};twoHands=0;TwoHanded=false;if(Club)Club.gameObject.SetActive(false);}
  void SetGrip(bool value){if(gripSkins==null||gripClosed==value)return;gripClosed=value;for(int i=0;i<gripSkins.Length;i++)gripSkins[i].SetBlendShapeWeight(gripShapes[i],value?100:0);}
  void Restore(){if(!applied)return;for(int i=0;i<bones.Length;i++)if(bones[i]){bones[i].localPosition=positions[i];bones[i].localRotation=rotations[i];}applied=false;}
  void Update(){Restore();}
  void OnDisable(){ResetPose();}
  bool Available(){var m=GolfMatchManager.Instance;return RigReady&&m&&m.Context&&m.State.Phase!=GolfMatchPhase.Idle&&m.Player(athlete)!=null&&!athlete.inTransit&&!GolfCartWorld.Driving(athlete);}
  static float Ease(float v){v=Mathf.Clamp01(v);return v*v*(3-2*v);}
  Vector3 Palm(Limb a)=>RigReady?a.end.TransformPoint(a.palmOffset):transform.position;
  void SetAddressGripOffset(float blend){
   if(!Club||!gripMarkersCaptured)return;
   // Restore the authored spacing: independent marker sliding had collapsed
   // both palms into the same point. Fit the whole visual club instead.
   Club.leftGrip.localPosition=carryLeftGripLocal;
   Club.rightGrip.localPosition=carryRightGripLocal;
   Club.transform.localScale=carryClubScale;
  }
  Vector3 ContactGrip(Vector3 target,Quaternion facing){
   var shoulder=(left.upper.position+right.upper.position)*.5f;
   float reach=(left.reach+right.reach)*.5f;
   var forward=facing*Vector3.forward;
   // Arms lead the fit. Put the hands clearly in front of the shoulders,
   // descending toward waist height, rather than choosing a point near the hips.
   var preferred=shoulder+forward*(reach*.70f)-Vector3.up*(reach*1.05f);
   var nativeScale=Club.transform.lossyScale;
   float nativeLength=Vector3.Scale(headOffset-carryRightGripLocal,nativeScale).magnitude;
   float Score(Vector3 candidate){
    float ratio=Vector3.Distance(target,candidate)/Mathf.Max(.001f,nativeLength);
    var scale=nativeScale*ratio;
    var rotation=ContactRotation(target,candidate,facing);
    var shaft=(target-candidate).normalized;
    var r=HandRotation(right,rotation*Vector3.forward,shaft);
    var l=HandRotation(left,-(rotation*Vector3.forward),shaft);
    var rw=candidate-r*Vector3.Scale(right.palmOffset,right.end.lossyScale);
    var lp=candidate+rotation*Vector3.Scale(carryLeftGripLocal-carryRightGripLocal,scale);
    var lw=lp-l*Vector3.Scale(left.palmOffset,left.end.lossyScale);
    float rd=Vector3.Distance(right.upper.position,rw),ld=Vector3.Distance(left.upper.position,lw);
    float re=rd/right.reach-.955f,le=ld/left.reach-.955f;
    float ro=Mathf.Max(0,rd-(right.reach-.007f)),lo=Mathf.Max(0,ld-(left.reach-.007f));
    float behind=Mathf.Max(0,reach*.50f-Vector3.Dot((rw+lw)*.5f-shoulder,forward));
    float scaleOverflow=Mathf.Max(0,.55f-ratio,ratio-1.05f);
    return (candidate-preferred).sqrMagnitude*25+(re*re+le*le)*12+
     (ro*ro+lo*lo+behind*behind)*2000+scaleOverflow*scaleOverflow*100;
   }
   // Continuous local refinement avoids the previous sphere sample changes
   // popping the hands between widely separated poses as the torso animates.
   var best=preferred;float bestScore=Score(best);
   for(float step=reach*.3f;step>reach*.001f;step*=.5f){
    for(int pass=0;pass<8;pass++){
     var next=best;float nextScore=bestScore;
     for(int axis=0;axis<3;axis++)for(int sign=-1;sign<=1;sign+=2){
      var delta=axis==0?forward:axis==1?Vector3.up:facing*Vector3.right;
      var candidate=best+delta*(step*sign);float score=Score(candidate);
      if(score<nextScore){next=candidate;nextScore=score;}
     }
     if(nextScore>=bestScore)break;best=next;bestScore=nextScore;
    }
   }
   float fittedScale=Vector3.Distance(target,best)/Mathf.Max(.001f,nativeLength);
   Club.transform.localScale=carryClubScale*Mathf.Lerp(1,fittedScale,twoHands);
   return best;
  }
  float HeadingTo(Vector3 target){var delta=target-transform.position;return delta.x*delta.x+delta.z*delta.z>.0001f?Mathf.Atan2(delta.x,delta.z)*Mathf.Rad2Deg:transform.eulerAngles.y;}
  Vector3 SwingArcGrip(Vector3 contact,Quaternion contactRotation,Quaternion facing,float phase,Vector3 addressLeft,Vector3 addressRight){
   if(Mathf.Abs(phase)<.00001f)return contact;
   var shoulder=(left.upper.position+right.upper.position)*.5f;
   var clubRotation=Quaternion.AngleAxis(phase<0?phase*118f:phase*150f,-(facing*Vector3.forward))*contactRotation;
   var arc=Quaternion.AngleAxis(-65f*phase,Vector3.up)*Quaternion.AngleAxis(-35f*Mathf.Abs(phase),facing*Vector3.right);
   var preferred=shoulder+arc*(contact-(addressLeft+addressRight)*.5f);
   var shaft=clubRotation*(headOffset-carryRightGripLocal).normalized;
   var r=HandRotation(right,clubRotation*Vector3.forward,shaft);
   var l=HandRotation(left,-(clubRotation*Vector3.forward),shaft);
   // A shared grip must lie on both wrist-reach spheres. Their intersection
   // retains the arm triangle as the chest turns, instead of pulling wrists
   // toward the chest and folding both elbows. Offsets include the real grips.
   var rc=right.upper.position+r*Vector3.Scale(right.palmOffset,right.end.lossyScale);
   var lc=left.upper.position+l*Vector3.Scale(left.palmOffset,left.end.lossyScale)-clubRotation*Vector3.Scale(carryLeftGripLocal-carryRightGripLocal,Club.transform.lossyScale);
   var cr=HandRotation(right,contactRotation*Vector3.forward,contactRotation*(headOffset-carryRightGripLocal).normalized);
   var cl=HandRotation(left,-(contactRotation*Vector3.forward),contactRotation*(headOffset-carryRightGripLocal).normalized);
   float initialR=Vector3.Distance(contact,addressRight+cr*Vector3.Scale(right.palmOffset,right.end.lossyScale));
   float initialL=Vector3.Distance(contact,addressLeft+cl*Vector3.Scale(left.palmOffset,left.end.lossyScale)-contactRotation*Vector3.Scale(carryLeftGripLocal-carryRightGripLocal,Club.transform.lossyScale));
   float blend=Ease(Mathf.Abs(phase));
   float rr=Mathf.Lerp(initialR,Mathf.Min(right.reach*.96f,right.reach-.007f),blend);
   float lr=Mathf.Lerp(initialL,Mathf.Min(left.reach*.96f,left.reach-.007f),blend);
   var delta=rc-lc;float distance=delta.magnitude;
   if(distance<.0001f)return lc+(preferred-lc).normalized*Mathf.Min(lr,rr);
   var axis=delta/distance;
   float along=(lr*lr-rr*rr+distance*distance)/(2*distance);
   var center=lc+axis*along;
   var radial=Vector3.ProjectOnPlane(preferred-center,axis);
   if(radial.sqrMagnitude<.000001f)radial=Vector3.ProjectOnPlane(facing*Vector3.forward,axis);
   // Authored arms have intersecting spheres. Keep a bounded fallback for
   // degenerate rigs rather than sending a non-finite target to the IK.
   if(distance>lr+rr||distance<Mathf.Abs(lr-rr))return Vector3.Lerp(lc,rc,lr/Mathf.Max(.001f,lr+rr));
   return center+radial.normalized*Mathf.Sqrt(Mathf.Max(0,lr*lr-along*along));
  }
  public void Address(Vector3 target){if(!athlete)return;float heading=HeadingTo(target);athlete.Motor.FaceGolf(heading);transform.rotation=Quaternion.Euler(0,heading,0);var state=State;state.target=target;state.heading=heading;State=state;}
  public bool CanAddress(Vector3 target){
   if(!Club||!RigReady)return false;
   // Interaction depends on proximity, not the preceding animation pose or yaw.
   var delta=target-transform.position;
   return delta.x*delta.x+delta.z*delta.z<=SwingReach*SwingReach&&Mathf.Abs(delta.y)<.7f;
  }
  Quaternion ContactRotation(Vector3 target,Vector3 grip,Quaternion facing){
   var shaft=headOffset-Club.rightGrip.localPosition;var axis=(target-grip).normalized;var rotation=Quaternion.FromToRotation(shaft,axis);
   // b must be the swing-travel direction (facing*left for a right-hander) so
    // the flat face (+Z local) squares to the target rather than the toe striking.
    var a=Vector3.ProjectOnPlane(rotation*Vector3.forward,axis);var b=Vector3.ProjectOnPlane(facing*Vector3.left,axis);
   if(a.sqrMagnitude>.001f&&b.sqrMagnitude>.001f)rotation=Quaternion.AngleAxis(Vector3.SignedAngle(a,b,axis),axis)*rotation;return rotation;
  }
  void LateUpdate(){
   if(!Available()){SetGrip(false);SetAddressGripOffset(0);if(Club)Club.gameObject.SetActive(false);twoHands=0;TwoHanded=false;return;}
   Equip(GolfMatchManager.Instance.Round);if(!Club)return;Club.gameObject.SetActive(true);
   SetGrip(true);
   var view=PlayerView.Instance;bool localCharge=view&&view.target==athlete&&view.GolfCharging;
   var match=GolfMatchManager.Instance;var aim=match?match.Aim(athlete):default;bool aiming=aim.active&&aim.round==match.Round;
   bool charge=localCharge||InRound&&State.action==GolfClubAction.Charge;bool swing=Busy;
   float amount=localCharge?view.GolfDisplayedCharge:charge?GolfShotRules.Power(Elapsed):swing?State.charge:0f;
   twoHands=Mathf.MoveTowards(twoHands,aiming||charge||swing?1:0,Time.deltaTime*14);TwoHanded=twoHands>.99f;
   SetAddressGripOffset(twoHands);
   Vector3 target=aiming?aim.ballPosition:State.target;
   if(localCharge){var ball=GolfMatchManager.Instance.Ball(view.GolfBallOwner);if(ball)target=ball.Body.position;}
   var facing=Quaternion.Euler(0,aiming||charge||swing?HeadingTo(target):transform.eulerAngles.y,0);
   // Present the addressed stance after network transform interpolation, too.
   // The authoritative motor still controls movement and the shot heading.
   if(aiming||charge||swing)transform.rotation=facing;
   for(int i=0;i<bones.Length;i++){positions[i]=bones[i].localPosition;rotations[i]=bones[i].localRotation;}applied=true;
   var animatedHead=head?head.rotation:Quaternion.identity;
   float spineTilt=18f*twoHands;
   float poseAmount=Ease(amount);
   float phase=-poseAmount;
   if(swing)phase=Elapsed<ContactTime?-poseAmount*(1-Ease(Elapsed/ContactTime)):Ease((Elapsed-ContactTime)/.34f)*(1-Ease((Elapsed-.44f)/(Duration-.44f)));
   float chestTurn=-65f*phase*twoHands;
   spine.rotation=Quaternion.AngleAxis(spineTilt,facing*Vector3.right)*spine.rotation;
   var contact=twoHands>0?ContactGrip(target,facing):target;var contactRotation=twoHands>0?ContactRotation(target,contact,facing):Quaternion.identity;
   var addressLeft=left.upper.position;var addressRight=right.upper.position;
   chest.rotation=Quaternion.AngleAxis(chestTurn,Vector3.up)*chest.rotation;
   // Counter the inherited torso turn in world space. Keep the animated
   // forward gaze, with only a little shoulder follow and address nod.
   if(head){head.rotation=Quaternion.AngleAxis(chestTurn*.08f,Vector3.up)*Quaternion.AngleAxis(spineTilt*.15f,facing*Vector3.right)*animatedHead;HeadPoseDeviation=Quaternion.Angle(animatedHead,head.rotation);}
   var grip=twoHands>0?SwingArcGrip(contact,contactRotation,facing,phase,addressLeft,addressRight):contact;
   float angle=phase<0?phase*118f:phase*150f;
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
   rotation=Quaternion.Slerp(carryRotation,rotation,twoHands);
   var rootForGrip=grip-rotation*Vector3.Scale(Club.rightGrip.localPosition,Club.transform.lossyScale);
   var position=Vector3.Lerp(carryGrip,rootForGrip,twoHands);
   Club.transform.SetPositionAndRotation(position,rotation);
   // In address, fold the small remaining bend behind the torso. This projects
   // the forearms toward the shaft in profile; opposing side poles visibly
   // flared them away from the handle. Keep the existing swing pole as the
   // club travels into the backswing.
   var addressPole=-transform.forward+Vector3.up*.08f;
   var rightPole=Vector3.Lerp(addressPole+transform.right*.07f,-Vector3.up+transform.right*.3f+transform.forward*.3f,Mathf.Abs(phase)*twoHands);
   var leftPole=addressPole-transform.right*.07f;
   var shaftDirection=(Club.head.position-Club.rightGrip.position).normalized;
   // The square club face (+Z) points to the golfer's left at address.
   // Use this axis for palm roll; marker +X points up/down along the stance.
   // Carry retains its existing orientation, and both palms follow the club during swings.
   if(twoHands>0)RightWristReachable=Arm(right,Club.rightGrip.position,Club.transform.forward,shaftDirection,rightPole,twoHands);
   if(!GripShapeReady)Curl(right,right.end.TransformDirection(right.palmAxis),1);
   CarryWristDeviation=Quaternion.Angle(animatedWrist,right.end.localRotation);
   if(twoHands<.999f)Club.transform.position+=(RightPalm-Club.rightGrip.position)*(1-twoHands);
   // Opposing palm normal keeps the left palm facing the golfer's right.
   if(twoHands>0)LeftWristReachable=Arm(left,Club.leftGrip.position,-Club.transform.forward,shaftDirection,leftPole,twoHands);
   RightGripError=Vector3.Distance(RightPalm,Club.rightGrip.position);LeftGripError=Vector3.Distance(LeftPalm,Club.leftGrip.position);
   GripError=TwoHanded?Mathf.Max(RightGripError,LeftGripError):RightGripError;
   HeadTargetError=Vector3.Distance(Club.head.position,target);
   var shaftAxis=shaftDirection;
   RightFingerShaftAngle=Vector3.Angle(right.end.TransformDirection(right.fingerAxis),shaftAxis);
   LeftFingerShaftAngle=Vector3.Angle(left.end.TransformDirection(left.fingerAxis),shaftAxis);
   RightForearmShaftAngle=Vector3.Angle(right.end.position-right.lower.position,shaftAxis);
   LeftForearmShaftAngle=Vector3.Angle(left.end.position-left.lower.position,shaftAxis);
   RightElbowAngle=Vector3.Angle(right.lower.position-right.upper.position,right.end.position-right.lower.position);
   LeftElbowAngle=Vector3.Angle(left.lower.position-left.upper.position,left.end.position-left.lower.position);
  }
  bool Arm(Limb a,Vector3 palm,Vector3 normal,Vector3 fingers,Vector3 pole,float blend){
   var rotation=HandRotation(a,normal,fingers);
   var wrist=palm-rotation*Vector3.Scale(a.palmOffset,a.end.lossyScale);
   var start=a.upper.position;var elbow=a.lower.position;var end=a.end.position;float upper=Vector3.Distance(start,elbow),lower=Vector3.Distance(elbow,end);var delta=wrist-start;
   float requested=delta.magnitude;if(a==left)LeftRequestedReachFraction=requested/Mathf.Max(.001f,a.reach);else RightRequestedReachFraction=requested/Mathf.Max(.001f,a.reach);bool reachable=requested>=Mathf.Abs(upper-lower)+.002f&&requested<=upper+lower-.005f;float d=Mathf.Clamp(requested,Mathf.Abs(upper-lower)+.002f,upper+lower-.005f);var direction=delta.normalized;
   float along=(upper*upper-lower*lower+d*d)/(2*d);var bend=Vector3.ProjectOnPlane(pole,direction).normalized;
   var joint=start+direction*along+bend*Mathf.Sqrt(Mathf.Max(0,upper*upper-along*along));
   var before=a.upper.rotation;a.upper.rotation=Quaternion.Slerp(before,Quaternion.FromToRotation(elbow-start,joint-start)*before,blend);
   before=a.lower.rotation;a.lower.rotation=Quaternion.Slerp(before,Quaternion.FromToRotation(a.end.position-a.lower.position,start+direction*d-a.lower.position)*before,blend);
   a.end.rotation=Quaternion.Slerp(a.end.rotation,rotation,blend);
   if(a==left)Curl(a,normal,blend);
   return reachable;
  }
  static Quaternion HandRotation(Limb a,Vector3 normal,Vector3 fingers)=>Quaternion.LookRotation(fingers,normal)*Quaternion.Inverse(Quaternion.LookRotation(a.fingerAxis,a.palmAxis));
  static void Curl(Limb a,Vector3 normal,float blend){foreach(var f in a.fingers){var axis=Vector3.Cross(f.GetChild(0).position-f.position,normal).normalized;f.rotation=Quaternion.AngleAxis((f.name.Contains("Thumb")?14:f.name.EndsWith("1")?28:48)*blend,axis)*f.rotation;}}
 }
}
