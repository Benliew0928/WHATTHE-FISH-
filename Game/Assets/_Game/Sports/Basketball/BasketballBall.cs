using System;
using Unity.Netcode;
using UnityEngine;

namespace WhatTheFish {
 public struct BasketballSnapshot:INetworkSerializable,IEquatable<BasketballSnapshot> {
  public bool valid,held,queued;public ulong holder;public Vector3 position;public Quaternion rotation;public uint reset,shots,passes,steals;public double time,stealTime;public float dribble,cadence;
  public BasketballPassAim passAim;public BasketballDefenseSnapshot defense;public BasketballFinishNotice finish;public BasketballCharge charge;public BasketballScore score;public BasketballNetHit northNet,southNet;
  public void NetworkSerialize<T>(BufferSerializer<T> s) where T:IReaderWriter {
   s.SerializeValue(ref valid);s.SerializeValue(ref held);s.SerializeValue(ref queued);s.SerializeValue(ref holder);s.SerializeValue(ref position);s.SerializeValue(ref rotation);s.SerializeValue(ref reset);s.SerializeValue(ref shots);s.SerializeValue(ref passes);s.SerializeValue(ref dribble);s.SerializeValue(ref cadence);s.SerializeValue(ref time);
   s.SerializeValue(ref passAim);s.SerializeValue(ref defense);s.SerializeValue(ref finish);s.SerializeValue(ref charge);s.SerializeValue(ref score);s.SerializeValue(ref northNet);s.SerializeValue(ref southNet);
   s.SerializeValue(ref steals);s.SerializeValue(ref stealTime);
  }
  public bool Equals(BasketballSnapshot b)=>passAim.Equals(b.passAim)&&defense.Equals(b.defense)&&valid==b.valid&&held==b.held&&queued==b.queued&&holder==b.holder&&position==b.position&&rotation==b.rotation&&reset==b.reset&&shots==b.shots&&passes==b.passes&&dribble==b.dribble&&cadence==b.cadence&&time==b.time&&finish.Equals(b.finish)&&charge.Equals(b.charge)&&score.Equals(b.score)&&northNet.Equals(b.northNet)&&southNet.Equals(b.southNet)&&steals==b.steals&&stealTime==b.stealTime;
 }

 // One arena-local ball. Only the host (or offline player) owns physical simulation.
 // The visual and optional effect anchors inherit this body's rigid rotation.
 [RequireComponent(typeof(Rigidbody),typeof(SphereCollider))]
 [DefaultExecutionOrder(50)]
 public sealed partial class BasketballBall:MonoBehaviour {
  public const float Radius=.15f;
  public Rigidbody Body {get;private set;}
  public bool Authority=>!NetworkManager.Singleton||!NetworkManager.Singleton.IsListening||NetworkManager.Singleton.IsServer;
  public bool Simulating {get;private set;}
  public uint PresentationReset=>Authority?reset:haveTarget?target.reset:0;
  Vector3 home;Quaternion homeRotation;float sendAt,receivedAt;bool initialized,wasConnected,wasAuthority,wasPlaying;
  uint reset;BasketballSnapshot previous,target;bool haveTarget;
  void Awake(){Body=GetComponent<Rigidbody>();home=transform.localPosition;home.y=Mathf.Max(home.y,Radius+.002f);homeRotation=transform.localRotation;Body.maxAngularVelocity=100;Body.centerOfMass=Vector3.zero;Body.inertiaTensor=Vector3.one*(2f/3*Body.mass*Radius*Radius);restDamping=Body.linearDamping;PrepareHoops();BasketballBoundaryVisual.Attach(this);gameObject.AddComponent<BasketballPassVisual>();initialized=true;}
  void OnEnable(){Active=this;if(initialized)ResetHome();}
  void OnDisable(){ClearPossession();if(Active==this)Active=null;if(Body){Body.isKinematic=true;Simulating=false;}haveTarget=false;wasPlaying=false;}
  void SetPose(Vector3 position,Quaternion rotation){
   Body.position=transform.parent?transform.parent.TransformPoint(position):position;
   Body.rotation=transform.parent?transform.parent.rotation*rotation:rotation;
  }
  public bool Place(Vector3 localPosition,Quaternion rotation,Vector3 velocity,Vector3 spin){
   if(!Authority||!Finite(localPosition)||!Finite(velocity)||!Finite(spin)||!Finite(rotation))return false;
   ClearPossession();
   // Reset interpolation history at a deliberate teleport. Subsequent frame
   // interpolation must not briefly display the old location inside the floor.
   var interpolation=Body.interpolation;Body.interpolation=RigidbodyInterpolation.None;
   SetPose(localPosition,rotation.normalized);transform.SetPositionAndRotation(Body.position,Body.rotation);Body.interpolation=interpolation;reset++;
   bool kinematic=Body.isKinematic;Body.isKinematic=false;Body.linearVelocity=Vector3.ClampMagnitude(velocity,45);Body.angularVelocity=Vector3.ClampMagnitude(spin,100);Body.isKinematic=kinematic;
   return true;
  }
  public void ResetHome(){if(!initialized)return;haveTarget=false;if(Authority){Place(home,homeRotation,Vector3.zero,Vector3.zero);pickupAt=Time.time+.35f;outsideFor=looseFor=0;}}
  static bool Finite(Vector3 v)=>float.IsFinite(v.x)&&float.IsFinite(v.y)&&float.IsFinite(v.z);
  static bool Finite(Quaternion q)=>float.IsFinite(q.x)&&float.IsFinite(q.y)&&float.IsFinite(q.z)&&float.IsFinite(q.w)&&q.x*q.x+q.y*q.y+q.z*q.z+q.w*q.w>.01f;
  void FixedUpdate(){
   var app=AppRoot.Instance;bool connected=NetworkManager.Singleton&&NetworkManager.Singleton.IsListening;
   bool authority=Authority;var host=NetworkAthlete.HostPlayer;
   bool playing=Playing;
   // Freeze clients even while their player/network variables are still arriving.
   bool simulate=authority&&playing&&(!connected||(host&&host.IsSpawned));
   if(connected!=wasConnected||authority!=wasAuthority||playing!=wasPlaying){ResetCourtPlayers();if(authority)ResetSessionScore();ResetHome();wasConnected=connected;wasAuthority=authority;wasPlaying=playing;}
   if(simulate){StepBlocks();StepShotPhysics();StepSteals();StepInteraction();}
   bool dynamic=simulate&&!Held;
   if(Body.isKinematic==dynamic)Body.isKinematic=!dynamic;
   Simulating=simulate;Body.detectCollisions=authority&&!Held;
   Body.interpolation=dynamic?RigidbodyInterpolation.Interpolate:RigidbodyInterpolation.None;
   if(simulate&&(transform.localPosition.y < -3||transform.localPosition.y>25||Mathf.Abs(transform.localPosition.x)>35||Mathf.Abs(transform.localPosition.z)>45))ResetHome();
   if(connected&&authority&&host&&host.IsSpawned&&Time.unscaledTime>=sendAt){
    sendAt=Time.unscaledTime+.05f;
    host.Basketball.Value=new BasketballSnapshot{valid=true,held=Held,queued=ActionQueued,holder=HolderId,shots=shotCount,passes=passCount,steals=steals,stealTime=stealTime,dribble=dribblePhase,cadence=dribbleCadence,position=transform.localPosition,rotation=transform.localRotation,reset=reset,time=NetworkManager.Singleton.ServerTime.Time,passAim=passAim,defense=Defense,finish=finishNotice,charge=Charge,score=score,northNet=baskets[0]?baskets[0].Hit:default,southNet=baskets[1]?baskets[1].Hit:default};
   }
  }
  void Update(){
   if(Authority)return;var host=NetworkAthlete.HostPlayer;if(!host||!host.IsSpawned||host.WorldSport.Value!=SportId.Basketball)return;
   var next=host.Basketball.Value;if(!next.valid||!Finite(next.position)||!Finite(next.rotation))return;
   if(!haveTarget||next.time!=target.time){
    bool snap=!haveTarget||next.reset!=target.reset;previous=target;target=next;receivedAt=Time.unscaledTime;
    if(snap){previous=target;SetPose(target.position,target.rotation);}haveTarget=true;
    if(baskets[0])baskets[0].Receive(next.northNet);if(baskets[1])baskets[1].Receive(next.southNet);
   }
   float duration=Mathf.Clamp((float)(target.time-previous.time),.02f,.2f);
   float alpha=Mathf.Clamp01((Time.unscaledTime-receivedAt)/duration);
   SetPose(Vector3.Lerp(previous.position,target.position,alpha),Quaternion.Slerp(previous.rotation,target.rotation,alpha));
  }
 }
}
