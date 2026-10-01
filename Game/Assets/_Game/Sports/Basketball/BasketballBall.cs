using System;
using Unity.Netcode;
using UnityEngine;

namespace WhatTheFish {
 public struct BasketballSnapshot:INetworkSerializable,IEquatable<BasketballSnapshot> {
  public bool valid;public Vector3 position;public Quaternion rotation;public uint reset;public double time;
  public void NetworkSerialize<T>(BufferSerializer<T> s) where T:IReaderWriter {
   s.SerializeValue(ref valid);s.SerializeValue(ref position);s.SerializeValue(ref rotation);s.SerializeValue(ref reset);s.SerializeValue(ref time);
  }
  public bool Equals(BasketballSnapshot b)=>valid==b.valid&&position==b.position&&rotation==b.rotation&&reset==b.reset&&time==b.time;
 }

 // One arena-local ball. Only the host (or offline player) owns physical simulation.
 // The visual and optional effect anchors inherit this body's rigid rotation.
 [RequireComponent(typeof(Rigidbody),typeof(SphereCollider))]
 public sealed class BasketballBall:MonoBehaviour {
  public const float Radius=.12f;
  public Rigidbody Body {get;private set;}
  public bool Authority=>!NetworkManager.Singleton||!NetworkManager.Singleton.IsListening||NetworkManager.Singleton.IsServer;
  public bool Simulating {get;private set;}
  Vector3 home;Quaternion homeRotation;float sendAt,receivedAt;bool initialized,wasConnected,wasAuthority;
  uint reset;BasketballSnapshot previous,target;bool haveTarget;
  void Awake(){Body=GetComponent<Rigidbody>();home=transform.localPosition;homeRotation=transform.localRotation;Body.maxAngularVelocity=100;Body.centerOfMass=Vector3.zero;Body.inertiaTensor=Vector3.one*(2f/3*Body.mass*Radius*Radius);initialized=true;}
  void OnEnable(){if(initialized)ResetHome();}
  void OnDisable(){if(Body){Body.isKinematic=true;Simulating=false;}haveTarget=false;}
  void SetPose(Vector3 position,Quaternion rotation){
   Body.position=transform.parent?transform.parent.TransformPoint(position):position;
   Body.rotation=transform.parent?transform.parent.rotation*rotation:rotation;
  }
  public bool Place(Vector3 localPosition,Quaternion rotation,Vector3 velocity,Vector3 spin){
   if(!Authority||!Finite(localPosition)||!Finite(velocity)||!Finite(spin)||!Finite(rotation))return false;
   // Reset interpolation history at a deliberate teleport. Subsequent frame
   // interpolation must not briefly display the old location inside the floor.
   var interpolation=Body.interpolation;Body.interpolation=RigidbodyInterpolation.None;
   SetPose(localPosition,rotation.normalized);transform.SetPositionAndRotation(Body.position,Body.rotation);Body.interpolation=interpolation;reset++;
   bool kinematic=Body.isKinematic;Body.isKinematic=false;Body.linearVelocity=Vector3.ClampMagnitude(velocity,45);Body.angularVelocity=Vector3.ClampMagnitude(spin,100);Body.isKinematic=kinematic;
   return true;
  }
  public void ResetHome(){if(!initialized)return;haveTarget=false;if(Authority)Place(home,homeRotation,Vector3.zero,Vector3.zero);}
  static bool Finite(Vector3 v)=>float.IsFinite(v.x)&&float.IsFinite(v.y)&&float.IsFinite(v.z);
  static bool Finite(Quaternion q)=>float.IsFinite(q.x)&&float.IsFinite(q.y)&&float.IsFinite(q.z)&&float.IsFinite(q.w)&&q.x*q.x+q.y*q.y+q.z*q.z+q.w*q.w>.01f;
  void FixedUpdate(){
   var app=AppRoot.Instance;bool connected=NetworkManager.Singleton&&NetworkManager.Singleton.IsListening;
   bool authority=Authority;var host=NetworkAthlete.HostPlayer;
   bool playing=!app||(app.Exploring&&app.SelectedSport==SportId.Basketball&&!(SkySailWorld.Instance&&SkySailWorld.Instance.Travelling));
   // Freeze clients even while their player/network variables are still arriving.
   bool simulate=authority&&playing&&(!connected||(host&&host.IsSpawned));
   if(connected!=wasConnected||authority!=wasAuthority){ResetHome();wasConnected=connected;wasAuthority=authority;}
   if(Body.isKinematic==simulate)Body.isKinematic=!simulate;
   Simulating=simulate;Body.detectCollisions=authority;
   Body.interpolation=authority?RigidbodyInterpolation.Interpolate:RigidbodyInterpolation.None;
   if(simulate&&(transform.localPosition.y < -3||transform.localPosition.y>25||Mathf.Abs(transform.localPosition.x)>35||Mathf.Abs(transform.localPosition.z)>45))ResetHome();
   if(connected&&authority&&host&&host.IsSpawned&&Time.unscaledTime>=sendAt){
    sendAt=Time.unscaledTime+.05f;
    host.Basketball.Value=new BasketballSnapshot{valid=true,position=transform.localPosition,rotation=transform.localRotation,reset=reset,time=NetworkManager.Singleton.ServerTime.Time};
   }
  }
  void Update(){
   if(Authority)return;var host=NetworkAthlete.HostPlayer;if(!host||!host.IsSpawned||host.WorldSport.Value!=SportId.Basketball)return;
   var next=host.Basketball.Value;if(!next.valid||!Finite(next.position)||!Finite(next.rotation))return;
   if(!haveTarget||next.time!=target.time){
    bool snap=!haveTarget||next.reset!=target.reset;previous=target;target=next;receivedAt=Time.unscaledTime;
    if(snap){previous=target;SetPose(target.position,target.rotation);}haveTarget=true;
   }
   float duration=Mathf.Clamp((float)(target.time-previous.time),.02f,.2f);
   float alpha=Mathf.Clamp01((Time.unscaledTime-receivedAt)/duration);
   SetPose(Vector3.Lerp(previous.position,target.position,alpha),Quaternion.Slerp(previous.rotation,target.rotation,alpha));
  }
 }
}
