#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.EventSystems;

namespace WhatTheFish {
 // Explicit opt-in integration checks exercise the real UI, command path,
 // collision and owner RPCs. No test fixtures run in normal play or release APKs.
 [DefaultExecutionOrder(120)]
 public sealed class GolfCartProbe:MonoBehaviour {
  string[] args;string output,captureName,poseCheck;AppRoot app;float presentedCameraYaw,presentedCartHeading;readonly Dictionary<Athlete,float> seatedGaps=new();
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)] static void Boot(){var args=Environment.GetCommandLineArgs();if(args.Contains("-cartAudit")||args.Contains("-networkCartAudit")||args.Contains("-cartTerrainAudit"))new GameObject("Golf cart probe").AddComponent<GolfCartProbe>();}
  string Value(string name,string fallback){int index=Array.IndexOf(args,name);return index>=0&&index+1<args.Length?args[index+1]:fallback;}
  void Record(string text){File.AppendAllText(output,text+Environment.NewLine);Debug.Log("CART_PROBE "+text);}
  void Check(bool condition,string name){Record((condition?"PASS ":"FAIL ")+name);}
  IEnumerator Start(){
   args=Environment.GetCommandLineArgs();output=Value("-report",Path.Combine(Application.persistentDataPath,"cart.txt"));
   DevelopmentProbe.TurnCommandActive=true;DevelopmentProbe.TurnCommand=default;
   float deadline=Time.realtimeSinceStartup+55;
   while((!AppRoot.Instance||!AppRoot.Instance.LocalAthlete||!AppRoot.Instance.Exploring||AppRoot.Instance.SelectedSport!=SportId.Golf)&&Time.realtimeSinceStartup<deadline)yield return null;
   app=AppRoot.Instance;Check(app&&app.Exploring&&app.SelectedSport==SportId.Golf,"CART_GOLF_READY");if(!app||!app.Exploring)yield break;
   yield return new WaitForSeconds(.6f);
   if(args.Contains("-cartTerrainAudit")){yield return TerrainAudit();Record("CART_TERRAIN_COMPLETE");yield break;}
   if(args.Contains("-networkCartAudit"))yield return NetworkAudit();else yield return OfflineAudit();
  }
  void Place(Athlete actor,Vector3 position,float yaw=0){actor.capsule.enabled=false;actor.transform.SetPositionAndRotation(position,Quaternion.Euler(0,yaw,0));actor.ResetLocomotion();var net=actor.GetComponent<NetworkTransform>();if(net&&net.IsServer)net.Teleport(position,actor.transform.rotation,Vector3.one);actor.capsule.enabled=GolfCartWorld.Authority;Physics.SyncTransforms();}
  Vector3 SafeSite(Athlete actor){
   foreach(var site in new[]{new Vector3(-40,0,-100),new Vector3(-45,0,-80),new Vector3(60,0,-80),new Vector3(45,0,70),new Vector3(-70,0,20),Vector3.zero})
    if(GolfCartMotor.Surface(site,0,out var supported,out var rotation)&&GolfCartMotor.Clear(supported,rotation,null,actor))return supported+Vector3.up*.08f;
   return app.environments.Current.Spawn(0);
  }
  Vector3 Beside(GolfCart cart){var at=cart.transform.position+cart.transform.right*2.35f;return GolfCartMotor.Floor(at,out var floor)?floor.point+Vector3.up*.08f:at;}
  void Capture(string name){captureName=name;}
  void CheckPose(string name){poseCheck=name;}
  void LateUpdate(){
   // Measure the cart and its camera together after both presentation updates.
   // Coroutine timers resume before LateUpdate and otherwise mix two frames.
   if(app){presentedCameraYaw=app.view.transform.eulerAngles.y;var localCart=GolfCartWorld.Driving(app.LocalAthlete);if(localCart)presentedCartHeading=GolfCartMotor.Heading(localCart.transform.rotation);}
   foreach(var actor in Athlete.Active){var cart=GolfCartWorld.Driving(actor);if(!cart||!actor.visual)continue;var hip=actor.visual.GetComponentsInChildren<Transform>().FirstOrDefault(t=>t.name=="mixamorig:Hips");if(hip)seatedGaps[actor]=Vector3.Distance(hip.position,cart.seat.position);}
   if(poseCheck!=null){
    foreach(var actor in Athlete.Active){
     var cart=GolfCartWorld.Driving(actor);if(!cart||!actor.visual)continue;
     var bones=actor.visual.GetComponentsInChildren<Transform>();Transform Bone(string name)=>bones.First(t=>t.name=="mixamorig:"+name);
     float Knee(string side){var knee=Bone(side+"Leg");return Vector3.Angle(Bone(side+"UpLeg").position-knee.position,Bone(side+"Foot").position-knee.position);}
     float left=Knee("Left"),right=Knee("Right");
     Check(left>178&&right>178,"CART_STRAIGHT_KNEES_"+poseCheck+" left="+left+" right="+right);
     foreach(var side in new[]{"Left","Right"}){
      var toe=Bone(side+"ToeBase");
      var direction=(toe.childCount>0?toe.GetChild(0).position-toe.position:toe.position-Bone(side+"Foot").position).normalized;
      float angle=Vector3.Angle(direction,cart.transform.forward);
      Check(angle<.1f,"CART_TOES_FORWARD_"+side+"_"+poseCheck+" angle="+angle);
     }
     var feet=new[]{Bone("LeftFoot"),Bone("RightFoot")}.Select(t=>cart.transform.InverseTransformPoint(t.position)-cart.transform.InverseTransformPoint(cart.seat.position)).ToArray();
     Check(feet.All(p=>Mathf.Abs(p.x)<.35f&&p.z>.25f&&p.z<.5f&&p.y>0&&p.y<.15f),"CART_FEET_AHEAD_AND_ABOVE_SEAT_"+poseCheck+" left="+feet[0]+" right="+feet[1]);
     var baked=new Mesh();float top=float.NegativeInfinity,footFront=float.PositiveInfinity;int footVertices=0;
     foreach(var renderer in actor.visual.GetComponentsInChildren<SkinnedMeshRenderer>(true)){
      // Include the imported FBX scale before transforming baked local vertices.
      renderer.BakeMesh(baked,true);
      foreach(var vertex in baked.vertices){
       var world=renderer.transform.TransformPoint(vertex);var local=cart.transform.InverseTransformPoint(world);top=Mathf.Max(top,local.y);
       if(Vector3.Distance(world,Bone("LeftFoot").position)<.075f||Vector3.Distance(world,Bone("RightFoot").position)<.075f){footFront=Mathf.Min(footFront,local.z-cart.transform.InverseTransformPoint(cart.seat.position).z);footVertices++;}
      }
     }Destroy(baked);
     Check(footVertices>0&&footFront>.18f,"CART_SKINNED_FEET_AHEAD_OF_SEAT_"+poseCheck+" minForward="+footFront+" vertices="+footVertices);
     var roof=cart.GetComponentsInChildren<BoxCollider>().Single(c=>c!=cart.hull);float underside=cart.transform.InverseTransformPoint(roof.transform.TransformPoint(roof.center)).y-roof.size.y*.5f;
     Check(top>cart.seat.localPosition.y+.75f&&top<underside-.025f,"CART_AVATAR_CLEARS_ROOF_"+poseCheck+" top="+top+" underside="+underside);
     CaptureSeatedPose(cart,poseCheck);
    }poseCheck=null;
   }
   if(captureName==null)return;FindFirstObjectByType<DevelopmentProbe>()?.CaptureFrame(Path.ChangeExtension(output,captureName+".png"));captureName=null;
  }
  void CaptureSeatedPose(GolfCart cart,string name){
   if(SystemInfo.graphicsDeviceType==UnityEngine.Rendering.GraphicsDeviceType.Null)return;
   var camera=Camera.main;var position=camera.transform.position;var rotation=camera.transform.rotation;float fov=camera.fieldOfView;
   try {
    camera.fieldOfView=38;camera.transform.position=cart.seat.position+cart.transform.right*2.8f+cart.transform.forward*.9f+cart.transform.up*.35f;
    camera.transform.LookAt(cart.seat.position+cart.transform.forward*.23f+cart.transform.up*.20f,cart.transform.up);
    FindFirstObjectByType<DevelopmentProbe>()?.CaptureFrame(Path.ChangeExtension(output,"straight-knees-"+name+".png"));
   } finally {camera.transform.SetPositionAndRotation(position,rotation);camera.fieldOfView=fov;}
  }
  float SeatGap(Athlete actor)=>seatedGaps.TryGetValue(actor,out var gap)?gap:float.PositiveInfinity;
  GolfCartButton Button(bool summon)=>FindObjectsByType<GolfCartButton>(FindObjectsSortMode.None).Single(b=>b.summon==summon);
  float Roll(GolfCart cart){var up=cart.wheelPivots[2].localRotation*Vector3.up;return Mathf.Atan2(up.z,up.y)*Mathf.Rad2Deg;}
  float FrontSteer(GolfCart cart)=>Vector3.SignedAngle(Vector3.right,cart.wheelPivots[0].localRotation*Vector3.right,Vector3.up);
  float CameraYaw=>presentedCameraYaw;
  IEnumerator CameraAudit(GolfCart cart){
   foreach(int mode in new[]{0,1,2}){
    app.view.mode=mode;DevelopmentProbe.TurnCommand=new PlayerCommand{move=Vector2.up};yield return new WaitForSeconds(.5f);
    app.view.yaw=presentedCartHeading+20;yield return null;
    float heading=presentedCartHeading,cameraYaw=CameraYaw;
    DevelopmentProbe.TurnCommand=new PlayerCommand{move=new Vector2(.2f,.98f)};yield return new WaitForSeconds(.4f);
    float turn=Mathf.DeltaAngle(heading,presentedCartHeading),viewTurn=Mathf.DeltaAngle(cameraYaw,CameraYaw);
    Record($"MEASURE cameraMode={mode} right20Percent cartTurn={turn:F2} cameraTurn={viewTurn:F2}");
    Check(turn>6&&Mathf.Abs(turn-viewTurn)<.6f,"CART_CAMERA_RIGHT_TURN_ONE_TO_ONE_MODE_"+mode);
    heading=presentedCartHeading;cameraYaw=CameraYaw;
    DevelopmentProbe.TurnCommand=new PlayerCommand{move=new Vector2(-.2f,.98f)};yield return new WaitForSeconds(.4f);
    turn=Mathf.DeltaAngle(heading,presentedCartHeading);viewTurn=Mathf.DeltaAngle(cameraYaw,CameraYaw);
    Record($"MEASURE cameraMode={mode} left20Percent cartTurn={turn:F2} cameraTurn={viewTurn:F2}");
    Check(turn< -6&&Mathf.Abs(turn-viewTurn)<.6f,"CART_CAMERA_LEFT_TURN_ONE_TO_ONE_MODE_"+mode);
    Check(Mathf.Abs(Mathf.DeltaAngle(presentedCartHeading+20,CameraYaw))<.6f,"CART_CAMERA_PRESERVES_MANUAL_LOOK_OFFSET_MODE_"+mode);
    PlayerView.LookDelta=new Vector2(100,0);yield return null;yield return null;
    Check(Mathf.Abs(Mathf.DeltaAngle(presentedCartHeading+33,CameraYaw))<.6f,"CART_CAMERA_LOOK_DRAG_STILL_WORKS_MODE_"+mode);
    Capture("following-camera"+mode);yield return null;
   }
   app.view.mode=0;app.view.yaw=presentedCartHeading;DevelopmentProbe.TurnCommand=new PlayerCommand{move=Vector2.down};yield return new WaitForSeconds(1.2f);
   float firstReverse=Mathf.Abs(Mathf.DeltaAngle(presentedCartHeading,CameraYaw));
   Record($"MEASURE firstPersonReverse yawOffset={firstReverse:F2} speed={cart.State.speed:F3}");
   Check(cart.State.speed< -1&&firstReverse<.6f,"CART_FIRST_PERSON_REVERSE_KEEPS_FORWARD_VIEW");Capture("first-person-reverse");yield return null;
   app.view.mode=1;app.view.yaw=presentedCartHeading;DevelopmentProbe.TurnCommand=new PlayerCommand{move=Vector2.up};yield return new WaitForSeconds(1.1f);
   DevelopmentProbe.TurnCommand=new PlayerCommand{move=Vector2.down};yield return new WaitForSeconds(.15f);
   float earlyReverse=Mathf.Abs(Mathf.DeltaAngle(app.view.yaw,CameraYaw));
   Check(earlyReverse>1&&earlyReverse<40,"CART_THIRD_PERSON_REVERSE_TURNS_GRADUALLY");
   yield return new WaitForSeconds(2.4f);float fullReverse=Mathf.Abs(Mathf.DeltaAngle(app.view.yaw,CameraYaw));
   Record($"MEASURE thirdPersonReverse early={earlyReverse:F2} settled={fullReverse:F2}");
   Check(fullReverse>174&&fullReverse<=180,"CART_THIRD_PERSON_REVERSE_LOOKS_BACK");Capture("third-person-reverse");yield return null;
   DevelopmentProbe.TurnCommand=new PlayerCommand{move=Vector2.up};yield return new WaitForSeconds(.15f);
   float earlyForward=Mathf.Abs(Mathf.DeltaAngle(app.view.yaw,CameraYaw));
   Check(earlyForward<fullReverse-1&&earlyForward>140,"CART_THIRD_PERSON_FORWARD_RETURNS_GRADUALLY");
   yield return new WaitForSeconds(2.4f);float fullForward=Mathf.Abs(Mathf.DeltaAngle(app.view.yaw,CameraYaw));
   Record($"MEASURE thirdPersonForward early={earlyForward:F2} settled={fullForward:F2}");
   Check(fullForward<6,"CART_THIRD_PERSON_FORWARD_RETURNS_TO_FRONT");Capture("third-person-forward");yield return null;
   app.view.mode=2;app.view.yaw=43;yield return null;float overheadHeading=presentedCartHeading,overheadYaw=CameraYaw;
   DevelopmentProbe.TurnCommand=new PlayerCommand{move=new Vector2(.5f,.866f)};yield return new WaitForSeconds(.5f);
   Check(Mathf.Abs(Mathf.DeltaAngle(overheadHeading,presentedCartHeading))>15&&Mathf.Abs(Mathf.DeltaAngle(overheadYaw,CameraYaw)-Mathf.DeltaAngle(overheadHeading,presentedCartHeading))<.6f,"CART_OVERHEAD_TURN_FOLLOWS_CART_WITH_LOOK_OFFSET");
   DevelopmentProbe.TurnCommand=new PlayerCommand{move=Vector2.down};yield return new WaitForSeconds(1.1f);
   Check(cart.State.speed< -1&&Mathf.Abs(Mathf.DeltaAngle(app.view.yaw,CameraYaw))<.1f,"CART_OVERHEAD_REVERSE_ADDS_NO_THIRD_PERSON_FLIP");Capture("overhead-reverse");yield return null;
   app.view.mode=0;app.view.yaw=presentedCartHeading;yield return new WaitForSeconds(.2f);
   Check(Mathf.Abs(Mathf.DeltaAngle(app.view.yaw,CameraYaw))<.1f,"CART_SWITCH_FROM_OVERHEAD_HAS_NO_OLD_TURN_OR_REVERSE_OFFSET");
   app.view.mode=1;yield return new WaitForSeconds(.2f);
   float switchReverse=Mathf.Abs(Mathf.DeltaAngle(app.view.yaw,CameraYaw));
   Check(switchReverse>1&&switchReverse<60,"CART_SWITCH_TO_THIRD_PERSON_REVERSE_STARTS_SMOOTHLY");
   DevelopmentProbe.TurnCommand=new PlayerCommand{move=Vector2.up};yield return new WaitForSeconds(.6f);DevelopmentProbe.TurnCommand=default;yield return new WaitForSeconds(.6f);
  }
  IEnumerator OfflineAudit(){
   var actor=app.LocalAthlete;var key=GolfCartWorld.Key(actor);Place(actor,SafeSite(actor));yield return new WaitForSeconds(.3f);
   Check(Button(true).button.interactable&&Button(true).label.text.StartsWith("召唤"),"CART_SUMMON_UI");
   var font=Button(true).label.font;Check(font&&"召唤收回驾驶离开".All(font.HasCharacter),"CART_CHINESE_GLYPHS_EMBEDDED");
   Check(!Button(false).button.interactable,"CART_DRIVE_HIDDEN_WHEN_NO_CART");
   Button(true).Press();yield return new WaitForSeconds(.4f);var cart=GolfCartWorld.Owned(actor);
   Check(cart&&GolfCartWorld.Count==1&&Button(true).label.text.StartsWith("收回"),"CART_SUMMON_CHANGES_SAME_BUTTON");if(!cart)yield break;
   Check(cart.wheelPivots.Length==4&&cart.wheelPivots.All(p=>p&&p.GetComponentsInChildren<MeshRenderer>(true).Length==3),"CART_FOUR_MOVABLE_WHEELS_ALL_THREE_LODS");
   Check(cart.GetComponent<LODGroup>().GetLODs().All(l=>l.renderers.Length==5),"CART_BODY_AND_FOUR_WHEELS_IN_EVERY_LOD");
   Check(!GolfCartWorld.Execute(actor,GolfCartAction.Summon,key)&&GolfCartWorld.Count==1,"CART_ONE_PER_PLAYER");
   Check(!GolfCartWorld.Execute(actor,GolfCartAction.Recall,12345)&&GolfCartWorld.HasCart(actor),"CART_RECALL_REJECTS_WRONG_OWNER");
   Check(!GolfCartWorld.Execute(actor,GolfCartAction.Drive,key),"CART_DRIVE_REJECTS_DISTANCE");
   Place(actor,Beside(cart));yield return new WaitForSeconds(.3f);
   Check(Button(false).button.interactable&&Button(false).label.text.StartsWith("驾驶"),"CART_DRIVE_VISIBLE_NEAR_EMPTY_CART");Capture("parked");
   Button(false).Press();yield return new WaitForSeconds(.35f);
   Check(GolfCartWorld.Driving(actor)==cart&&!actor.capsule.enabled&&Button(false).label.text.StartsWith("离开"),"CART_DRIVE_UI_ENTERS_SEAT");
   Check(!actor.CanRequestJump,"CART_DRIVER_CANNOT_JUMP");actor.RequestJump();Check(!actor.LoadingJump&&!actor.Airborne,"CART_DRIVER_JUMP_REJECTED");
   Check(SeatGap(actor)<.02f,"CART_SEATED_HIP gap="+SeatGap(actor));
   CheckPose("ENTRY");
   app.view.mode=1;app.view.yaw=145;app.view.pitch=12;yield return new WaitForSeconds(.3f);Capture("driver-close");yield return null;
   app.view.yaw=80;yield return new WaitForSeconds(.2f);Capture("driver-side");yield return null;
   app.view.yaw=0;yield return new WaitForSeconds(.2f);Capture("driver-rear");yield return null;
   var pad=app.view.stick;var touchOrigin=new Vector2(190,178);var touch=new PointerEventData(EventSystem.current){pointerId=31,position=touchOrigin};
   DevelopmentProbe.TurnCommandActive=false;pad.OnPointerDown(touch);var touchStart=cart.transform.position;touch.position=touchOrigin+Vector2.up*(70*pad.transform.lossyScale.x);pad.OnDrag(touch);yield return new WaitForSeconds(.5f);pad.OnPointerUp(touch);DevelopmentProbe.TurnCommandActive=true;
   Check(Vector3.Distance(cart.transform.position,touchStart)>.8f&&pad.value==Vector2.zero,"CART_TOUCH_STICK_DRIVES_AND_RELEASES");
   var before=cart.transform.position;DevelopmentProbe.TurnCommand=new PlayerCommand{move=Vector2.up};float roll=Roll(cart),forwardRoll=0,driveUntil=Time.time+1.5f;
   while(Time.time<driveUntil){yield return null;float next=Roll(cart);forwardRoll+=Mathf.DeltaAngle(roll,next);roll=next;}
   Record($"MEASURE forward speed={cart.State.speed:F3} travel={Vector3.Distance(before,cart.transform.position):F3} wheelRoll={forwardRoll:F1}");
   Check(Vector3.Distance(before,cart.transform.position)>10&&Mathf.Abs(cart.State.speed-17)<.05f,"CART_FORWARD_JOYSTICK_DOUBLED_17_MPS");
   Check(forwardRoll>180,"CART_ALL_WHEELS_ROLL_FORWARD");
   float straightSpeed=cart.State.speed;var heading=GolfCartMotor.Heading(cart.transform.rotation);DevelopmentProbe.TurnCommand=new PlayerCommand{move=new Vector2(.75f,.65f).normalized};yield return new WaitForSeconds(.8f);
   float turnAngle=Mathf.Abs(Mathf.DeltaAngle(heading,GolfCartMotor.Heading(cart.transform.rotation)));
   Record($"MEASURE straight={straightSpeed:F3} turn={cart.State.speed:F3} yawIn0.8s={turnAngle:F2} frontSteer={FrontSteer(cart):F2}");
   Check(turnAngle>55,"CART_STEERING_JOYSTICK_SHARPER");
   Check(Mathf.Abs(cart.State.speed-straightSpeed)<.05f,"CART_FULL_STICK_TURN_MATCHES_STRAIGHT_SPEED");
   Check(Mathf.Abs(FrontSteer(cart)-cart.State.steering)<1&&FrontSteer(cart)>10,"CART_FRONT_WHEELS_STEER_WITH_INPUT");
   Check(Mathf.Abs(Vector3.Dot(cart.wheelPivots[2].localRotation*Vector3.right,Vector3.forward))<.001f,"CART_REAR_WHEELS_KEEP_AXLES_STRAIGHT");CheckPose("DRIVING_TURN");Capture("wheels-steering");
   DevelopmentProbe.TurnCommand=new PlayerCommand{move=Vector2.up*.5f};yield return new WaitForSeconds(.7f);float halfStraight=cart.State.speed;
   DevelopmentProbe.TurnCommand=new PlayerCommand{move=new Vector2(.3f,.4f)};yield return new WaitForSeconds(.7f);
   Record($"MEASURE halfStraight={halfStraight:F3} halfTurn={cart.State.speed:F3}");
   Check(Mathf.Abs(halfStraight-8.5f)<.05f&&Mathf.Abs(cart.State.speed-halfStraight)<.05f,"CART_HALF_STICK_TURN_MATCHES_STRAIGHT_SPEED");
   DevelopmentProbe.TurnCommand=default;yield return new WaitForSeconds(.8f);Check(Mathf.Abs(cart.State.speed)<.01f,"CART_RELEASE_STICK_BRAKES");
   var stopped=cart.wheelPivots.Select(p=>p.localRotation).ToArray();yield return new WaitForSeconds(.2f);
   Check(cart.wheelPivots.Select((p,i)=>Quaternion.Angle(stopped[i],p.localRotation)).All(angle=>angle<.05f),"CART_STOPPED_WHEELS_DO_NOT_SPIN");
   Check(Mathf.Abs(FrontSteer(cart))<.05f,"CART_RELEASE_RETURNS_FRONT_WHEELS_TO_CENTRE");
   before=cart.transform.position;var forward=cart.transform.forward;DevelopmentProbe.TurnCommand=new PlayerCommand{move=Vector2.down};roll=Roll(cart);float reverseRoll=0;driveUntil=Time.time+1.2f;
   while(Time.time<driveUntil){yield return null;float next=Roll(cart);reverseRoll+=Mathf.DeltaAngle(roll,next);roll=next;}
   Record($"MEASURE reverse speed={cart.State.speed:F3} travel={Vector3.Dot(cart.transform.position-before,forward):F3} wheelRoll={reverseRoll:F1}");
   Check(Vector3.Dot(cart.transform.position-before,forward)<-4&&Mathf.Abs(cart.State.speed+8)<.05f,"CART_REVERSE_JOYSTICK_DOUBLED_8_MPS");
   Check(reverseRoll< -180,"CART_WHEELS_ROLL_BACKWARD_IN_REVERSE");
   DevelopmentProbe.TurnCommand=new PlayerCommand{move=new Vector2(.6f,-.8f)};yield return new WaitForSeconds(.7f);
   Check(Mathf.Abs(cart.State.speed+8)<.05f,"CART_FULL_STICK_REVERSE_TURN_REMAINS_8_MPS");DevelopmentProbe.TurnCommand=default;yield return new WaitForSeconds(.5f);
   yield return CameraAudit(cart);
   // A real collider directly ahead must stop the whole cart, including hitches.
   var wall=GameObject.CreatePrimitive(PrimitiveType.Cube);wall.name="Cart probe wall";wall.layer=8;wall.transform.SetPositionAndRotation(cart.transform.position+cart.transform.forward*4+Vector3.up*1.5f,cart.transform.rotation);wall.transform.localScale=new Vector3(10,4,.2f);Physics.SyncTransforms();
   var wallOrigin=cart.transform.position;forward=cart.transform.forward;
   for(int i=0;i<40;i++)actor.Simulate(new PlayerCommand{move=Vector2.up},.20f);
   Check(Vector3.Dot(cart.transform.position-wallOrigin,forward)<2.8f&&Mathf.Abs(cart.State.speed)<.01f,"CART_SWEPT_WALL_COLLISION_WITH_HITCHES");Destroy(wall);yield return null;
   foreach(int mode in new[]{0,1,2}){app.view.mode=mode;yield return new WaitForSeconds(.2f);Check(Vector3.Distance(app.view.transform.position,cart.seat.position)<13,"CART_CAMERA_MODE_"+mode);Capture("camera"+mode);}
   foreach(int lod in new[]{0,1,2}){cart.GetComponent<LODGroup>().ForceLOD(lod);app.view.mode=1;app.view.yaw=145;yield return new WaitForSeconds(.2f);Capture("lod"+lod);yield return null;}cart.GetComponent<LODGroup>().ForceLOD(-1);
   foreach(int lod in new[]{0,1}){actor.visual.GetComponentInChildren<LODGroup>().ForceLOD(lod);app.view.mode=1;app.view.yaw=GolfCartMotor.Heading(cart.transform.rotation)+80;yield return new WaitForSeconds(.2f);CheckPose("CHARACTER_LOD"+lod);Capture("seated-character-lod"+lod);yield return null;}actor.visual.GetComponentInChildren<LODGroup>().ForceLOD(-1);
   Button(false).Press();yield return new WaitForSeconds(.4f);Check(!GolfCartWorld.Driving(actor)&&GolfCartWorld.Owned(actor)==cart&&actor.capsule.enabled&&actor.Grounded,"CART_LEAVE_PERSISTS_AND_GROUNDED");
   before=actor.transform.position;DevelopmentProbe.TurnCommand=new PlayerCommand{move=Vector2.up};yield return new WaitForSeconds(.65f);DevelopmentProbe.TurnCommand=default;Check(Vector3.Distance(before,actor.transform.position)>1,"CART_EXIT_RESTORES_WALKING");
   app.view.mode=1;app.view.yaw=51;yield return new WaitForSeconds(.2f);Check(Mathf.Abs(Mathf.DeltaAngle(51,CameraYaw))<.1f,"CART_EXIT_RESTORES_NORMAL_CAMERA_WITHOUT_REVERSE_OFFSET");
   actor.RequestJump();yield return new WaitForSeconds(.25f);Check(actor.Airborne,"CART_EXIT_RESTORES_JUMP");yield return new WaitForSeconds(.6f);
   Place(actor,Beside(cart));yield return new WaitForSeconds(.2f);app.view.RequestCartUse();yield return new WaitForSeconds(.3f);app.view.RequestCartToggle();yield return new WaitForSeconds(.4f);
   Check(!GolfCartWorld.HasCart(actor)&&!GolfCartWorld.Driving(actor)&&actor.capsule.enabled&&GolfCartWorld.Count==0&&Button(true).label.text.StartsWith("召唤"),"CART_RECALL_OCCUPIED_EJECTS_AND_REMOVES");
   Place(actor,SafeSite(actor));yield return new WaitForSeconds(.2f);app.view.RequestCartToggle();yield return new WaitForSeconds(.3f);cart=GolfCartWorld.Owned(actor);var parked=cart.transform.position;
   Place(actor,Beside(cart));yield return new WaitForSeconds(.2f);app.view.RequestCartUse();yield return new WaitForSeconds(.3f);Check(GolfCartWorld.Driving(actor)==cart,"CART_DRIVER_BEFORE_ISLAND_SWITCH");
   var streaming=app.environments.GetComponent<SkySailStreaming>();
   foreach(var sport in new[]{SportId.Basketball,SportId.Football,SportId.Fishing}){
    yield return streaming.Prepare(sport);app.SelectSport(sport);app.Show("stadium");yield return new WaitForSeconds(.3f);
    Check(!GolfCartWorld.Allowed&&!GolfCartWorld.Execute(actor,GolfCartAction.Summon,key)&&FindObjectsByType<GolfCartButton>(FindObjectsSortMode.None).Length==0&&!cart.gameObject.activeSelf&&!GolfCartWorld.Driving(actor)&&actor.capsule.enabled,"CART_BLOCKED_OUTSIDE_GOLF_"+sport);
   }
   yield return streaming.Prepare(SportId.Golf);app.SelectSport(SportId.Golf);app.Show("stadium");yield return new WaitForSeconds(.3f);
   Check(cart.gameObject.activeSelf&&GolfCartWorld.HasCart(actor)&&Vector3.Distance(cart.transform.position,parked)<.01f,"CART_PARKED_PERSISTS_RETURN_TO_GOLF");
   // Containment is checked around the real curved shore, not a square bound.
   Check(GolfCartWorld.Execute(actor,GolfCartAction.Recall,key),"CART_RECALL_AFTER_RETURN");yield return null;
   var slope=GameObject.CreatePrimitive(PrimitiveType.Cube);slope.name="Terrain__CartProbeSlope";slope.layer=8;slope.transform.position=SafeSite(actor)+Vector3.up*6;slope.transform.rotation=Quaternion.Euler(12,0,10);slope.transform.localScale=new Vector3(15,.3f,15);Physics.SyncTransforms();
   if(GolfCartMotor.Surface(slope.transform.position,40,out var slopeFloor,out var slopeRotation)){
    var fixture=Instantiate(Resources.Load<GolfCart>("GolfCart"),slopeFloor,slopeRotation);var state=new GolfCartState{summoned=true,position=slopeFloor,rotation=slopeRotation,driver=GolfCartState.NoDriver};fixture.Apply(state,true);var initial=slopeRotation;
    for(int step=0;step<180;step++){GolfCartMotor.Step(fixture,ref state,Vector2.zero,1f/60);fixture.Apply(state,true);Physics.SyncTransforms();}
    Check(Quaternion.Angle(initial,state.rotation)<.10f&&Vector3.Distance(slopeFloor,state.position)<.01f,"CART_IDLE_ON_SLOPE_DOES_NOT_STEER_OR_DRIFT angle="+Quaternion.Angle(initial,state.rotation));Destroy(fixture.gameObject);
   }else Check(false,"CART_SLOPE_FIXTURE_SUPPORTED");Destroy(slope);yield return null;
   for(int n=0;n<12;n++){
    float yaw=n*30;var direction=Quaternion.Euler(0,yaw,0)*Vector3.forward;
    if(!Physics.Raycast(new Vector3(0,2,0),direction,out var boundary,400,1<<9)){Check(false,"CART_SHORE_RAY_"+n);continue;}
    var at=boundary.point-direction*8;
    if(!GolfCartMotor.Surface(at,yaw,out var floor,out var rotation)){Check(false,"CART_SHORE_SUPPORT_"+n);continue;}
    var fixture=Instantiate(Resources.Load<GolfCart>("GolfCart"),floor,rotation);fixture.Owner=12345;var state=new GolfCartState{summoned=true,position=floor,rotation=rotation,driver=GolfCartState.NoDriver};fixture.Apply(state,true);
    Physics.SyncTransforms();for(int step=0;step<140;step++){GolfCartMotor.Step(fixture,ref state,Vector2.up,.1f);fixture.Apply(state,true);Physics.SyncTransforms();}
    var radial=Vector3.ProjectOnPlane(state.position,Vector3.up);bool edge=Physics.Raycast(new Vector3(0,2,0),radial.normalized,out var shore,400,1<<9);
    Check(edge&&radial.magnitude<shore.distance-.3f&&GolfCartMotor.Surface(state.position,yaw,out _,out _)&&state.position.y>RefinedIslandEnvironment.Active.layout.sea_level+.12f,"CART_SHORE_CONTAINMENT_"+n);Destroy(fixture.gameObject);yield return null;
   }
   yield return TerrainAudit();
   yield return WheelContactAudit();
   Record("CART_OFFLINE_COMPLETE");
  }
  IEnumerator TerrainAudit(){
   var routes=RefinedIslandEnvironment.Active.layout.routes.Where(r=>r.name.StartsWith("Bunker ")).ToArray();
   Check(routes.Length==5,"CART_FIVE_REAL_BUNKER_ROUTES");
   foreach(var route in routes)foreach(bool across in new[]{false,true})foreach(bool opposite in new[]{false,true})foreach(bool reverse in new[]{false,true}){
    var start=route.points[opposite?2:0];var target=route.points[opposite?0:2];
    if(across){
     float reach=Vector3.ProjectOnPlane(route.points[2]-route.points[0],Vector3.up).magnitude*.35f;
     start=route.points[1]+Vector3.forward*(opposite?reach:-reach);target=route.points[1]+Vector3.forward*(opposite?-reach:reach);
    }
    var path=Vector3.ProjectOnPlane(target-start,Vector3.up);var direction=path.normalized;
    float yaw=Mathf.Atan2(direction.x,direction.z)*Mathf.Rad2Deg+(reverse?180:0);
    string name=route.name.Replace(' ','_')+(across?(opposite?"_SOUTH":"_NORTH"):(opposite?"_WEST":"_EAST"))+(reverse?"_REVERSE":"_FORWARD");
    if(!GolfCartMotor.Surface(start,yaw,out var floor,out var rotation)){Check(false,"CART_BUNKER_START_SUPPORTED_"+name);continue;}
    var fixture=Instantiate(Resources.Load<GolfCart>("GolfCart"),floor,rotation);fixture.Owner=12345;
    var state=new GolfCartState{summoned=true,position=floor,rotation=rotation,driver=GolfCartState.NoDriver};fixture.Apply(state,true);Physics.SyncTransforms();
    float progress=0;bool supported=true;float lowest=float.MaxValue;
    for(int step=0;step<720&&progress<path.magnitude-.25f;step++){
     GolfCartMotor.Step(fixture,ref state,reverse?Vector2.down:Vector2.up,1f/60);fixture.Apply(state,true);Physics.SyncTransforms();
     progress=Vector3.Dot(state.position-floor,direction);lowest=Mathf.Min(lowest,state.position.y);
     supported&=GolfCartMotor.Surface(state.position,yaw,out _,out _);
    }
    Check(progress>path.magnitude-.7f&&supported,"CART_BUNKER_ENTER_AND_EXIT_"+name+" progress="+progress+" required="+path.magnitude+" lowest="+lowest);
    if(progress<path.magnitude-.7f){
     var facing=Quaternion.Euler(0,yaw,0);
     foreach(var hit in Physics.BoxCastAll(state.position+facing*new Vector3(0,.66f,0),GolfCartMotor.HalfBody,direction,facing,.5f,GolfCartMotor.Obstacles,QueryTriggerInteraction.Ignore))
      if(!hit.collider.transform.IsChildOf(fixture.transform))Record("BLOCK "+name+" collider="+hit.collider.name+" distance="+hit.distance+" normal="+hit.normal);
    }
    Destroy(fixture.gameObject);yield return null;
   }
  }
  IEnumerator WheelContactAudit(){
   var state=new GolfCartState{summoned=true,position=new Vector3(1000,20,1000),rotation=Quaternion.identity,driver=GolfCartState.NoDriver};
   var fixture=Instantiate(Resources.Load<GolfCart>("GolfCart"),state.position,state.rotation);fixture.Apply(state,true);
   yield return null;yield return null;
   float Angle(int i){var up=fixture.wheelPivots[i].localRotation*Vector3.up;return Mathf.Atan2(up.z,up.y)*Mathf.Rad2Deg;}
   var initial=Enumerable.Range(0,4).Select(Angle).ToArray();
   state.rotation=Quaternion.Euler(15,0,0);state.position+=Vector3.up*.25f;fixture.Apply(state,true);yield return null;yield return null;
   for(int i=0;i<4;i++)Check(Mathf.Abs(Mathf.DeltaAngle(initial[i],Angle(i)))<.05f,"CART_SUSPENSION_SETTLING_DOES_NOT_ROLL_WHEEL_"+i);
   state.rotation=Quaternion.identity;fixture.Apply(state,true);yield return null;yield return null;
   state.speed=17;fixture.Apply(state,true);yield return null;yield return null;
   for(int i=0;i<4;i++)Check(Mathf.Abs(Mathf.DeltaAngle(initial[i],Angle(i)))<.05f,"CART_TARGET_SPEED_WITHOUT_MOVEMENT_DOES_NOT_SPIN_WHEEL_"+i);
   var radii=new float[4];
   for(int i=0;i<4;i++){
    var filter=fixture.wheelPivots[i].GetComponentsInChildren<MeshFilter>(true).Single(f=>f.name.EndsWith("LOD0"));
    var matrix=fixture.transform.worldToLocalMatrix*filter.transform.localToWorldMatrix;var e=filter.sharedMesh.bounds.extents;
    float y=Mathf.Abs(matrix.m10)*e.x+Mathf.Abs(matrix.m11)*e.y+Mathf.Abs(matrix.m12)*e.z;
    float z=Mathf.Abs(matrix.m20)*e.x+Mathf.Abs(matrix.m21)*e.y+Mathf.Abs(matrix.m22)*e.z;
    radii[i]=(y+z)*.5f;
    Check(Mathf.Abs(radii[i]-fixture.wheelRadii[i])<.002f,"CART_ROLLING_RADIUS_MATCHES_RENDERED_TYRE_"+i+" actual="+radii[i]);
   }
   var start=state.position;
   foreach(float distance in new[]{.10f,.25f,.50f}){
    state.position=start+Vector3.forward*distance;fixture.Apply(state,true);yield return null;yield return null;
    for(int i=0;i<4;i++){
     float expected=distance/(2*Mathf.PI*radii[i])*360*.85f;
     float actual=Mathf.DeltaAngle(initial[i],Angle(i));
     Check(Mathf.Abs(actual-expected)<.5f,"CART_STARTUP_ROLL_MATCHES_GROUND_DISTANCE_"+i+" metres="+distance+" expected="+expected+" actual="+actual);
    }
   }
   state.position-=Vector3.forward*.25f;fixture.Apply(state,true);yield return null;yield return null;
   for(int i=0;i<4;i++){
    float expected=.25f/(2*Mathf.PI*radii[i])*360*.85f;
    Check(Mathf.Abs(Mathf.DeltaAngle(initial[i],Angle(i))-expected)<.5f,"CART_REVERSE_ROLL_USES_SAME_DISTANCE_SCALE_"+i);
   }
   Destroy(fixture.gameObject);yield return null;
  }
  IEnumerator NetworkAudit(){
   var local=app.LocalAthlete;var own=local.GetComponent<NetworkAthlete>();float start=Time.realtimeSinceStartup;
   while(!NetworkAthlete.HostPlayer||FindObjectsByType<NetworkAthlete>(FindObjectsSortMode.None).Length!=2)yield return null;
   var host=NetworkAthlete.HostPlayer.GetComponent<Athlete>();var guest=FindObjectsByType<NetworkAthlete>(FindObjectsSortMode.None).Single(n=>n.OwnerClientId!=0).GetComponent<Athlete>();
   if(app.rooms.Host){Place(host,SafeSite(host));Place(guest,host.transform.position+Vector3.right*9);yield return new WaitForSeconds(.3f);app.view.RequestCartToggle();}
   while(!GolfCartWorld.HasCart(host)&&Time.realtimeSinceStartup<start+5)yield return null;yield return new WaitForSeconds(.3f);
   var cart=GolfCartWorld.Owned(host);Check(cart&&GolfCartWorld.Count==1,"CART_NETWORK_SUMMON_REPLICATES");if(!cart)yield break;
   if(!app.rooms.Host){own.GolfCartRpc(GolfCartAction.Recall,0);own.GolfCartRpc(GolfCartAction.Drive,0);}
   yield return new WaitForSeconds(.5f);Check(GolfCartWorld.HasCart(host)&&!GolfCartWorld.Driving(guest),"CART_NETWORK_WRONG_RECALL_AND_DISTANT_DRIVE_REJECTED");
   if(app.rooms.Host)Place(guest,Beside(cart));yield return new WaitForSeconds(.5f);
   if(!app.rooms.Host)app.view.RequestCartUse();yield return new WaitForSeconds(.6f);
   Check(GolfCartWorld.Driving(guest)==cart&&cart.State.driver==GolfCartWorld.Key(guest),"CART_NETWORK_GUEST_DRIVES_HOST_EMPTY_CART");
   if(app.rooms.Host){Place(host,Beside(cart));Check(!GolfCartWorld.Execute(host,GolfCartAction.Drive,0),"CART_NETWORK_OCCUPIED_REJECTS_SECOND_DRIVER");}
   var before=cart.transform.position;if(!app.rooms.Host)DevelopmentProbe.TurnCommand=new PlayerCommand{move=Vector2.up};yield return new WaitForSeconds(1.5f);DevelopmentProbe.TurnCommand=default;
   Check(Vector3.Distance(cart.transform.position,before)>4,"CART_NETWORK_REMOTE_INPUT_MOVES_HOST_CART");yield return new WaitForSeconds(.6f);
   app.view.mode=1;yield return null;float localCameraBefore=CameraYaw,localYawBefore=app.view.yaw,localCartHeading=presentedCartHeading;
   if(!app.rooms.Host)DevelopmentProbe.TurnCommand=new PlayerCommand{move=new Vector2(.6f,.8f)};
   float steerUntil=Time.realtimeSinceStartup+3;while(Mathf.Abs(cart.State.steering)<5&&Time.realtimeSinceStartup<steerUntil)yield return null;
   yield return new WaitForSeconds(.2f);
   Check(cart.State.steering>5&&Mathf.Abs(FrontSteer(cart)-cart.State.steering)<1,"CART_NETWORK_STEERING_AND_FRONT_WHEEL_POSE_REPLICATE");
   if(!app.rooms.Host){float cartTurn=Mathf.DeltaAngle(localCartHeading,presentedCartHeading),cameraTurn=Mathf.DeltaAngle(localCameraBefore,CameraYaw);Record($"MEASURE networkDriver cartTurn={cartTurn:F2} cameraTurn={cameraTurn:F2}");Check(Mathf.Abs(cartTurn-cameraTurn)<1,"CART_NETWORK_LOCAL_DRIVER_CAMERA_FOLLOWS_INTERPOLATED_TURN");}
   else Check(Mathf.Abs(Mathf.DeltaAngle(localYawBefore,app.view.yaw))<.1f,"CART_NETWORK_REMOTE_DRIVER_DOES_NOT_ROTATE_HOST_CAMERA");
   yield return new WaitForSeconds(.5f);DevelopmentProbe.TurnCommand=default;yield return new WaitForSeconds(.6f);
   app.view.mode=2;yield return null;localCameraBefore=CameraYaw;localYawBefore=app.view.yaw;localCartHeading=presentedCartHeading;
   if(!app.rooms.Host)DevelopmentProbe.TurnCommand=new PlayerCommand{move=new Vector2(-.6f,.8f)};yield return new WaitForSeconds(.4f);
   if(!app.rooms.Host){float cartTurn=Mathf.DeltaAngle(localCartHeading,presentedCartHeading),cameraTurn=Mathf.DeltaAngle(localCameraBefore,CameraYaw);Record($"MEASURE networkOverheadDriver cartTurn={cartTurn:F2} cameraTurn={cameraTurn:F2}");Check(cartTurn< -6&&Mathf.Abs(cartTurn-cameraTurn)<1,"CART_NETWORK_OVERHEAD_FOLLOWS_INTERPOLATED_LEFT_TURN");}
   else Check(Mathf.Abs(Mathf.DeltaAngle(localYawBefore,app.view.yaw))<.1f,"CART_NETWORK_REMOTE_DRIVER_DOES_NOT_ROTATE_HOST_OVERHEAD_CAMERA");
   DevelopmentProbe.TurnCommand=default;yield return new WaitForSeconds(.6f);
   Check(Vector3.Distance(cart.transform.position,NetworkAthlete.HostPlayer.Cart.Value.position)<.4f,"CART_NETWORK_HOST_POSE_REPLICATES");
   Check(SeatGap(guest)<.04f,"CART_NETWORK_GUEST_SEATED_ON_BOTH_PEERS gap="+SeatGap(guest));
   CheckPose("NETWORK_GUEST");yield return null;
   if(!app.rooms.Host)app.view.RequestCartUse();yield return new WaitForSeconds(.5f);
   Check(!GolfCartWorld.Driving(guest)&&GolfCartWorld.HasCart(host),"CART_NETWORK_GUEST_LEAVE_PRESERVES_OWNER_CART");
   if(!app.rooms.Host)app.view.RequestCartToggle();yield return new WaitForSeconds(.5f);
   Check(GolfCartWorld.Count==2&&GolfCartWorld.HasCart(host)&&GolfCartWorld.HasCart(guest),"CART_NETWORK_EACH_PLAYER_OWNS_ONE");
   if(!app.rooms.Host)own.GolfCartRpc(GolfCartAction.Summon,own.OwnerClientId);yield return new WaitForSeconds(.3f);Check(GolfCartWorld.Count==2,"CART_NETWORK_DUPLICATE_SUMMON_REJECTED");
   if(app.rooms.Host)Place(guest,Beside(cart));yield return new WaitForSeconds(.4f);if(!app.rooms.Host)own.GolfCartRpc(GolfCartAction.Drive,0);yield return new WaitForSeconds(.5f);
   Check(GolfCartWorld.Driving(guest)==cart,"CART_NETWORK_REENTER_OTHER_OWNER_CART");
   if(app.rooms.Host)app.view.RequestCartToggle();yield return new WaitForSeconds(.7f);
   Check(!GolfCartWorld.HasCart(host)&&GolfCartWorld.HasCart(guest)&&!GolfCartWorld.Driving(guest),"CART_NETWORK_OWNER_RECALL_EJECTS_GUEST_ONLY_REMOVES_OWN");
   if(app.rooms.Host)Check(guest.capsule.enabled&&guest.Grounded,"CART_NETWORK_RECALL_RESTORES_GUEST_CONTROLLER");
   if(app.rooms.Host){Place(host,SafeSite(host));yield return new WaitForSeconds(.2f);app.view.RequestCartToggle();}yield return new WaitForSeconds(.5f);
   Check(GolfCartWorld.Count==2,"CART_NETWORK_OWNER_CAN_SUMMON_AGAIN");
   if(!app.rooms.Host){Check(!Button(true).label.text.StartsWith("召唤"),"CART_NETWORK_UI_USES_LOCAL_OWNERSHIP");Record("CART_NETWORK_COMPLETE");yield return new WaitForSeconds(1.2f);var leave=app.rooms.Leave();while(!leave.IsCompleted)yield return null;}
   else {yield return new WaitForSeconds(1.2f);Check(GolfCartWorld.Count==1&&GolfCartWorld.HasCart(host)&&!GolfCartWorld.HasCart(guest),"CART_NETWORK_DISCONNECT_REMOVES_ONLY_LEAVING_OWNER");Record("CART_NETWORK_COMPLETE");}
  }
 }
}
#endif
