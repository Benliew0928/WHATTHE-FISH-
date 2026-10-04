using UnityEngine;

namespace WhatTheFish {
 [DefaultExecutionOrder(100)]
 public sealed class GolfCart:MonoBehaviour {
  public Transform seat;public BoxCollider hull;public ulong Owner {get;set;}
  [Header("Wheel presentation")]
  public Transform[] wheelPivots;public float[] wheelRadii;
  [Range(.5f,1.5f)] public float wheelRollScale=.85f;
  public GolfCartState State {get;private set;}
  bool initialized;Vector3 smoothing,previousWheelPosition;Quaternion previousWheelRotation;
  readonly float[] wheelRoll=new float[4];float wheelSteering;bool wheelsReady;
  public void Apply(GolfCartState state,bool snap){
   bool entered=state.driver!=State.driver&&state.driver!=GolfCartState.NoDriver;
   State=state;
   if(!initialized||snap){bool first=!initialized;transform.SetPositionAndRotation(state.position,state.rotation);initialized=true;smoothing=Vector3.zero;if(first){previousWheelPosition=transform.position;previousWheelRotation=transform.rotation;wheelsReady=true;}}
   if(entered){
    var app=AppRoot.Instance;
    if(app&&app.LocalAthlete&&GolfCartWorld.Key(app.LocalAthlete)==state.driver){app.view.ClearMatchInput();app.view.yaw=state.rotation.eulerAngles.y;app.view.pitch=17;}
   }
  }
  public void PlaceDriver(Athlete athlete){
   if(!athlete||!seat)return;
   athlete.capsule.enabled=false;
   athlete.transform.SetPositionAndRotation(seat.position-transform.up*.70f,transform.rotation);
   athlete.speed=0;
   if(!athlete.GetComponent<GolfCartDriverPose>())athlete.gameObject.AddComponent<GolfCartDriverPose>();
  }
  void LateUpdate(){
   if(!GolfCartWorld.Allowed)return;
   if(!GolfCartWorld.Authority){
    transform.position=Vector3.SmoothDamp(transform.position,State.position,ref smoothing,.065f,float.PositiveInfinity,Time.deltaTime);
    transform.rotation=Quaternion.Slerp(transform.rotation,State.rotation,1-Mathf.Exp(-22*Time.deltaTime));
   }
   var actor=GolfCartWorld.ActorFor(State.driver);if(actor)PlaceDriver(actor);
   UpdateWheels();
   var view=PlayerView.Instance;if(actor&&view&&view.target==actor)view.UpdateCartCamera(this);
  }
  void UpdateWheels(){
   if(wheelPivots==null||wheelPivots.Length!=4||wheelRadii==null||wheelRadii.Length!=4)return;
   if(!wheelsReady){previousWheelPosition=transform.position;previousWheelRotation=transform.rotation;wheelsReady=true;}
   float angle=State.driver==GolfCartState.NoDriver?0:State.steering;
   wheelSteering=Mathf.MoveTowards(wheelSteering,angle,180*Time.deltaTime);
   // Use displayed ground travel: suspension tilt must not roll a stationary wheel.
   // Summons/rebases are discontinuities and must not create a rotation burst.
   if((transform.position-previousWheelPosition).sqrMagnitude<25){
    var beforeHeading=Quaternion.Euler(0,GolfCartMotor.Heading(previousWheelRotation),0);
    var afterHeading=Quaternion.Euler(0,GolfCartMotor.Heading(transform.rotation),0);
    var groundUp=(previousWheelRotation*Vector3.up+transform.up).normalized;
    for(int i=0;i<4;i++)if(wheelPivots[i]){
     var offset=Vector3.ProjectOnPlane(wheelPivots[i].localPosition,Vector3.up);
     var before=previousWheelPosition+beforeHeading*offset;
     var after=transform.position+afterHeading*offset;var path=after-before;
     var forward=afterHeading*Quaternion.Euler(0,i<2?wheelSteering:0,0)*Vector3.forward;
     var tangent=Vector3.ProjectOnPlane(forward,groundUp).normalized;
     float travel=Vector3.ProjectOnPlane(path,Vector3.up).sqrMagnitude<.00000001f?0:Vector3.Dot(path,tangent);
     wheelRoll[i]=Mathf.Repeat(wheelRoll[i]+travel/Mathf.Max(.01f,wheelRadii[i])*Mathf.Rad2Deg*wheelRollScale,360);
    }
   }
   for(int i=0;i<4;i++)if(wheelPivots[i])wheelPivots[i].localRotation=Quaternion.Euler(0,i<2?wheelSteering:0,0)*Quaternion.AngleAxis(wheelRoll[i],Vector3.right);
   previousWheelPosition=transform.position;previousWheelRotation=transform.rotation;
  }
  void OnEnable(){wheelsReady=false;}
  public void Camera(Camera camera,float yaw,float pitch,int mode){
   Vector3 focus=mode==0?seat.position+transform.up*.65f+transform.forward*.12f:transform.position+Vector3.up*1.30f;
   var rotation=Quaternion.Euler(mode==2?58:pitch,yaw,0);float distance=mode==0?0:mode==1?6.2f:10;
   var desired=focus-rotation*Vector3.forward*distance;
   if(distance>0&&Physics.SphereCast(focus,.2f,(desired-focus).normalized,out var hit,distance,1<<8,QueryTriggerInteraction.Ignore))desired=focus+(desired-focus).normalized*Mathf.Max(.45f,hit.distance-.15f);
   camera.nearClipPlane=mode==0?.045f:.15f;camera.transform.SetPositionAndRotation(desired,rotation);
  }
 }
}
