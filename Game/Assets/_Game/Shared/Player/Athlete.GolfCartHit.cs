using UnityEngine;

namespace WhatTheFish {
 public sealed partial class Athlete {
  Vector3 cartHitVelocity;float cartHitUntil,cartHitAt=-10;
  public bool CartKnocked=>cartHitUntil>Time.time;
  // Authority only: impact input cannot be submitted by a client.
  public bool HitByGolfCart(Vector3 direction,float impactSpeed){
   if(!GolfCartWorld.Authority||!GolfCartWorld.Allowed||IsSpectator||inTransit||GolfCartWorld.Driving(this)||Time.time-cartHitAt<1.2f||impactSpeed<1.5f)return false;
   if(!initialized)Setup();if(!capsule.enabled)return false;
   direction=Vector3.ProjectOnPlane(direction,Vector3.up);if(direction.sqrMagnitude<.001f)return false;
   GolfMatchManager.Instance?.ReleaseAim(this);GolfClubMotion?.ResetPose();
   var net=GetComponent<NetworkAthlete>();if(net&&net.IsSpawned){net.ClearMatchInput();net.GolfCartHitRpc();}
   else if(PlayerView.Instance&&PlayerView.Instance.target==this)PlayerView.Instance.ClearMatchInput();
   Motor.Reset(transform.eulerAngles.y);Charging=false;
   float strength=Mathf.InverseLerp(1.5f,20,impactSpeed);
   cartHitVelocity=direction.normalized*Mathf.Lerp(3,8,strength);
   Jump.Launch(Mathf.Lerp(4.5f,7,strength));cartHitUntil=Time.time+3;cartHitAt=Time.time;
   return true;
  }
  bool SimulateCartHit(float dt){
   if(!CartKnocked)return false;
   if(!GolfCartWorld.Allowed||!capsule.enabled){cartHitVelocity=Vector3.zero;cartHitUntil=0;return false;}
   remote=false;float step=Mathf.Min(dt,.25f)/Mathf.Max(1,Mathf.CeilToInt(Mathf.Min(dt,.25f)*60));
   int steps=Mathf.Max(1,Mathf.CeilToInt(Mathf.Min(dt,.25f)*60));
   for(int i=0;i<steps;i++){
    capsule.stepOffset=0;var before=transform.position;
    float vertical=Jump.Step(Grounded,false,step);
    var flags=capsule.Move(cartHitVelocity*step+Vector3.up*vertical);Jump.Collide(flags);
    cartHitVelocity*=Mathf.Exp(-1.4f*step);
    if((flags&CollisionFlags.Sides)!=0)cartHitVelocity*=.35f;
    speed=Vector3.ProjectOnPlane(transform.position-before,Vector3.up).magnitude/step;
    var island=RefinedIslandEnvironment.Active;
    if(island&&transform.position.y<island.layout.sea_level-2){capsule.enabled=false;transform.position=island.layout.safe_return;capsule.enabled=true;ResetLocomotion();break;}
    if(!Jump.Airborne){cartHitUntil=0;cartHitVelocity=Vector3.zero;Motor.Reset(transform.eulerAngles.y);break;}
   }
   lastPosition=transform.position;return true;
  }
 }
}
