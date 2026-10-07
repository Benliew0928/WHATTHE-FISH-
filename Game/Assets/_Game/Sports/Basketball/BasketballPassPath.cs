using UnityEngine;

namespace WhatTheFish {
 // One trajectory plan supplies both the visible ribbon and the host's launch.
 // A bounce requires a real floor contact; only that first clean rebound uses
 // the planned 15% tangential loss and the ball material's 0.78 restitution.
 public struct BasketballPassPath {
  public Vector3 start,velocity,bounce,rebound;public float bounceTime,duration;
  public bool Bounces=>bounceTime>0;
  public Vector3 Point(float time)=>Bounces&&time>bounceTime?Travel(bounce,rebound,time-bounceTime):Travel(start,velocity,time);
  public static Vector3 Travel(Vector3 origin,Vector3 velocity,float time)=>origin+velocity*time+Physics.gravity*(time*time*.5f+time*Time.fixedDeltaTime*.5f);
  public static BasketballPassPath Create(Vector3 start,float heading,float power,float bend,float floor){
   bend=Mathf.Clamp(bend,-1,1);power=Mathf.Clamp01(power);float gravity=-Physics.gravity.y;
   var forward=Quaternion.Euler(0,heading,0)*Vector3.forward;float range=BasketballPassRules.Range(power,bend);
   var path=new BasketballPassPath{start=start};
   if(bend<-.01f){
    path.bounceTime=Mathf.Lerp(.34f,.30f,-bend);float after=.28f;
    float speed=range/(path.bounceTime+after*.85f);
    float height=Mathf.Min(start.y-.15f,floor+BasketballBall.Radius+.003f);
    float vertical=(height-start.y)/path.bounceTime+gravity*(path.bounceTime+Time.fixedDeltaTime)*.5f;
    path.velocity=forward*speed+Vector3.up*vertical;
    path.bounce=Travel(start,path.velocity,path.bounceTime);
    path.rebound=forward*(speed*.85f)+Vector3.up*((gravity*path.bounceTime-vertical)*.78f+gravity*Time.fixedDeltaTime*.5f);
    path.duration=path.bounceTime+after;
   }else{
    float direct=range/Mathf.Lerp(8,13,power);
    path.duration=Mathf.Lerp(direct,2*Mathf.Sqrt(2*Mathf.Lerp(1.25f,2.8f,power)/gravity),bend);
    path.velocity=forward*(range/path.duration)+Vector3.up*(gravity*(path.duration+Time.fixedDeltaTime)*.5f);
   }
   return path;
  }
 }
 public sealed partial class BasketballBall {
  bool passBounceArmed;BasketballPassPath launchedPass;double passLaunchedAt;
  public uint PassBounces {get;private set;}
  public static float PassFloor(Vector3 point){
   return Physics.Raycast(point+Vector3.up*.2f,Vector3.down,out var hit,8,1<<8,QueryTriggerInteraction.Ignore)?hit.point.y:point.y-BasketballMotion.PassPoint.y;
  }
  bool BouncePassContact(Collision collision){
   if(!passBounceArmed)return false;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
   Debug.Log($"PASS_BOUNCE_CONTACT time={BasketballMotion.Clock-passLaunchedAt:F4} distance={Vector3.Distance(Body.position,launchedPass.bounce):F4} ball={Body.position:F4} expected={launchedPass.bounce:F4} contacts={collision.contactCount} collider={collision.collider.name}");
   for(int i=0;i<collision.contactCount;i++)Debug.Log($"PASS_BOUNCE_SURFACE {collision.GetContact(i).point:F4} normal={collision.GetContact(i).normal:F4}");
#endif
   passBounceArmed=false;
   if(Held||BasketballMotion.Clock-passLaunchedAt>launchedPass.bounceTime+.18||Vector3.Distance(Body.position,launchedPass.bounce)>.65f)return false;
   for(int i=0;i<collision.contactCount;i++){
    var contact=collision.GetContact(i);
    if(contact.normal.y<.94f||Mathf.Abs(contact.point.y-(launchedPass.bounce.y-Radius))>.08f)continue;
    Body.linearVelocity=launchedPass.rebound;Body.linearDamping=0;passing=true;RestoreFloorContacts();PassBounces++;sendAt=0;return true;
   }
   return false;
  }
 }
}
