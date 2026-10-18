using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Netcode.Components;

namespace WhatTheFish {
 public enum TeleportPhase { Idle, Preparing, Charging, Absorbing, Travelling, Arriving, Settling }
 [DefaultExecutionOrder(700)]
 public sealed class TeleportSequence:MonoBehaviour {
  public const float ChargeSeconds=1,AbsorbSeconds=.65f,ArrivalSeconds=1.3f,SettleSeconds=.8f;
  public static TeleportSequence Instance {get;private set;}
  public static bool Busy=>Instance&&Instance.Phase!=TeleportPhase.Idle;
  public static bool OwnsCamera=>Busy&&Instance.Phase!=TeleportPhase.Preparing&&Instance.Phase!=TeleportPhase.Settling;
  public TeleportPhase Phase {get;private set;}
  public float PhaseProgress=>Mathf.Clamp01(phaseElapsed/Mathf.Max(.01f,Duration));
  public float Duration=>Phase switch{TeleportPhase.Charging=>ChargeSeconds,TeleportPhase.Absorbing=>AbsorbSeconds,TeleportPhase.Travelling=>travelSeconds,TeleportPhase.Arriving=>ArrivalSeconds,TeleportPhase.Settling=>SettleSeconds,_=>1};
  public string Error {get;private set;}="";
  public TeleportLanding[] Landings {get;private set;}=Array.Empty<TeleportLanding>();
  public float LargestCameraStep {get;private set;}
  public static TeleportSequence Ensure(){if(!Instance)new GameObject("Teleport animation service").AddComponent<TeleportSequence>();return Instance;}
  sealed class Passenger {public Athlete actor;public Vector3 position;public Quaternion rotation;public bool capsule,transit,transformEnabled;public NetworkTransform networkTransform;public TeleportEffect effect;}
  readonly List<Passenger> passengers=new();readonly Dictionary<Canvas,bool> canvases=new();
  AppRoot app;SkySailWorld world;SkySailStreaming streaming;Camera camera;GameObject arrivalCabin;SportId source,destination;TeleportDestination landingKind;
  float phaseElapsed,travelSeconds,oldFov,oldNear;bool oldBlocked,committed;int version,oldMode;float oldYaw,oldPitch;
  Vector3 cameraStart,depart,arrival,sourceFocus,targetFocus,lastCamera;Quaternion cameraRotation,arrivalRotation;float curveHeight;
  bool room,prepared,entry;uint entrySeed;
  public bool StartPreview(SportId island,TeleportDestination kind,IReadOnlyList<Athlete> roster)=>StartSequence(island,kind,roster,false);
  internal bool StartRoom(SportId island,TeleportDestination kind,IReadOnlyList<Athlete> roster,bool fromSky=false,uint seed=0){entry=fromSky;entrySeed=seed;return StartSequence(island,kind,roster,true);}
  bool StartSequence(SportId island,TeleportDestination kind,IReadOnlyList<Athlete> roster,bool shared){
   app=AppRoot.Instance;world=SkySailWorld.Instance;streaming=app?app.environments.GetComponent<SkySailStreaming>():null;Error="";
   if(Busy||!app||!app.Exploring||!world||world.Travelling||!streaming||streaming.Busy){Error="Enter an island and wait for the current journey to finish.";return false;}
   if(app.rooms.Connected&&!shared){Error="Use the host's room travel controls.";return false;}
   if(!Enum.IsDefined(typeof(SportId),island)||!Enum.IsDefined(typeof(TeleportDestination),kind)||roster==null||roster.Count<1||roster.Count(a=>a&&!a.IsSpectator)>TeleportDestinations.Capacity(island)||roster.Count(a=>a&&a.IsSpectator)>10||!roster.Contains(app.LocalAthlete)||roster.Any(a=>!a||!a.isActiveAndEnabled||a.inTransit)||roster.Distinct().Count()!=roster.Count){Error="Choose a valid group within the island capacity.";return false;}
   if(FootballMatch.Instance&&FootballMatch.Instance.Active||BasketballMatch.Instance&&BasketballMatch.Instance.Active||GolfMatchManager.Instance&&GolfMatchManager.Instance.State.Running||FishingGame.Instance&&(FishingGame.Instance.State.Match.phase==FishingRoundPhase.Playing||FishingGame.Instance.State.Match.phase==FishingRoundPhase.Countdown)||FootballMVP.Presenting||BasketballMVP.Presenting){Error="Finish the current match before previewing teleport.";return false;}
   room=shared;if(!shared)entry=false;prepared=false;camera=Camera.main;source=app.SelectedSport;destination=island;landingKind=kind;committed=false;Landings=Array.Empty<TeleportLanding>();LargestCameraStep=0;
   oldBlocked=app.view.InputBlocked;oldMode=app.view.mode;oldYaw=app.view.yaw;oldPitch=app.view.pitch;oldFov=camera.fieldOfView;oldNear=camera.nearClipPlane;
   cameraStart=camera.transform.position+world.Origin;cameraRotation=camera.transform.rotation;
   foreach(var actor in roster){var nt=actor.GetComponent<NetworkTransform>();passengers.Add(new Passenger{actor=actor,position=actor.transform.position,rotation=actor.transform.rotation,capsule=actor.capsule.enabled,transit=actor.inTransit,networkTransform=nt,transformEnabled=nt&&nt.enabled});actor.ResetLocomotion();actor.inTransit=true;actor.capsule.enabled=false;if(nt)nt.enabled=false;actor.GetComponent<NetworkAthlete>()?.ClearMatchInput();}
   app.view.InputBlocked=true;app.view.ClearMatchInput();SetPhase(TeleportPhase.Preparing);StartCoroutine(Prepare(++version));return true;
  }
  void Awake(){Instance=this;}
  IEnumerator Prepare(int ticket){
   yield return streaming.Prepare(destination);
   if(ticket!=version){if(!Busy&&SceneLifetime.CanPresent(gameObject)&&app&&SceneLifetime.CanPresent(app.gameObject))streaming.ReleaseOtherIslands(app.SelectedSport);yield break;}
   if(passengers.Any(p=>!p.actor)||streaming.Error!=null||!streaming.Loaded(destination)){Cancel("The destination could not load. Everyone stays at the start.");yield break;}
   var root=app.environments.roots[(int)destination];root.transform.position=SkySailMap.Center(destination)-world.Origin;root.SetActive(true);
   foreach(var c in world.stations[(int)destination].GetComponentsInChildren<Collider>(true))c.enabled=true;
   Physics.SyncTransforms();
   if(!TeleportDestinations.Resolve(destination,landingKind,passengers.Select(p=>p.actor).ToArray(),root.transform.position,out var plan,out var error)){Cancel(error);yield break;}
   Landings=plan;GolfCartWorld.ParkDrivers(true);if(entry){app.Show("stadium");app.view.InputBlocked=true;}
   // Show the destination's parked cabin during the approach. The existing cabin
   // takes over at the origin rebase, at the exact same pose. Meshes stay shared.
   if(destination!=source){arrivalCabin=Instantiate(world.cabin.gameObject,world.scenery);arrivalCabin.name="Teleport arrival cabin view";arrivalCabin.transform.SetLocalPositionAndRotation(SkySailMap.Berth(destination),SkySailMap.Facing(destination));foreach(var c in arrivalCabin.GetComponentsInChildren<Collider>(true))c.enabled=false;}
   foreach(var p in passengers){p.actor.ResetLocomotion();p.actor.inTransit=true;p.actor.capsule.enabled=false;p.actor.GetComponent<CharacterIdentity>()?.Refresh();p.effect=new TeleportEffect(p.actor);if(!p.effect.Supported){Cancel("Teleport materials are missing or unsupported.");yield break;}}
   world.proxies[(int)destination].SetActive(false);
   foreach(var c in FindObjectsByType<Canvas>(FindObjectsSortMode.None)){if(c.GetComponent<RoomTeleportStatus>())continue;canvases[c]=c.enabled;c.enabled=false;}
   sourceFocus=app.LocalAthlete.transform.position+world.Origin+Vector3.up*1.2f;
   int local=passengers.FindIndex(p=>p.actor==app.LocalAthlete);targetFocus=plan[local].position+SkySailMap.Center(destination)+Vector3.up*1.2f;
   Vector3 offset=Quaternion.Euler(0,oldYaw,0)*new Vector3(0,2.2f,-5.8f);Vector3 localFocus=sourceFocus-world.Origin;
   // Frame a station departure from the inland side, clear of the parked cabin.
   if(Vector3.Distance(app.LocalAthlete.transform.position,SkySailMap.Port(source))<18)offset=SkySailMap.Facing(source)*new Vector3(1.4f,2.4f,-7);
   if(Physics.SphereCast(localFocus,.22f,offset.normalized,out var wall,offset.magnitude,1<<8,QueryTriggerInteraction.Ignore))offset=offset.normalized*Mathf.Max(1.2f,wall.distance-.25f);
   depart=sourceFocus+offset;
   Vector3 arrivalOffset=landingKind==TeleportDestination.Station?SkySailMap.Facing(destination)*new Vector3(1.4f,2.4f,-7):plan[local].rotation*new Vector3(1.4f,2.1f,-6);
   Vector3 destinationFocus=targetFocus-world.Origin;
   if(Physics.SphereCast(destinationFocus,.22f,arrivalOffset.normalized,out var obstruction,arrivalOffset.magnitude,1<<8,QueryTriggerInteraction.Ignore))arrivalOffset=arrivalOffset.normalized*Mathf.Max(1.2f,obstruction.distance-.25f);
   arrival=targetFocus+arrivalOffset;
   cameraStart=camera.transform.position+world.Origin;cameraRotation=camera.transform.rotation;
   float distance=Vector3.Distance(depart,arrival);travelSeconds=Mathf.Clamp(2+distance/180,2.4f,6);curveHeight=source==destination?Mathf.Clamp(distance*.55f,12,110):150;
   if(entry){
    // One reproducible cloud position per room entry; each camera targets its own landing.
    float angle=(entrySeed*137.508f+(int)destination*53)*Mathf.Deg2Rad;
    depart=SkySailMap.Center(destination)+new Vector3(Mathf.Cos(angle)*150,165,Mathf.Sin(angle)*150);
    sourceFocus=targetFocus;cameraStart=depart;cameraRotation=Quaternion.LookRotation(targetFocus-depart);travelSeconds=5.5f;curveHeight=35;
    foreach(var p in passengers)p.effect.Present(1,0,Time.unscaledTime,false);
   }
   lastCamera=cameraStart;prepared=true;if(room)RoomTeleport.Instance.Report(1);else SetPhase(TeleportPhase.Charging);
  }
  void SetPhase(TeleportPhase value){Phase=value;phaseElapsed=0;}
  void Update(){
   if(!Busy)return;
   if(Input.GetKeyDown(KeyCode.Escape)){if(room)RoomTeleport.Instance.Cancel();else Cancel();return;}
   if(!app||!app.Exploring||app.rooms.Connected&&!room||passengers.Any(p=>!p.actor)){if(room)RoomTeleport.Instance.Report(1,true);else Cancel("Preview interrupted; the group was restored.");return;}
   if(Phase==TeleportPhase.Preparing){if(room&&prepared&&RoomTeleport.Instance.AllowsCharge)SetPhase(entry?TeleportPhase.Travelling:TeleportPhase.Charging);return;}
   // A loading/render hitch must lengthen the journey, never skip across the world.
   phaseElapsed+=Mathf.Min(Time.unscaledDeltaTime,.05f);
   if(PhaseProgress<1)return;
   switch(Phase){
    case TeleportPhase.Charging:SetPhase(TeleportPhase.Absorbing);break;
    case TeleportPhase.Absorbing:SetPhase(TeleportPhase.Travelling);break;
    case TeleportPhase.Travelling:
     if(room){RoomTeleport.Instance.Report(2);if(!RoomTeleport.Instance.AllowsArrival)return;}
     // Camera coordinates remain global while the destination becomes the local origin.
     app.SelectSport(destination,true);committed=true;ClearArrivalCabin();
     for(int i=0;i<passengers.Count;i++){var p=passengers[i];p.actor.capsule.enabled=false;p.actor.transform.SetPositionAndRotation(Landings[i].position,Landings[i].rotation);p.actor.ResetLocomotion();}
     Physics.SyncTransforms();world.SetIsland(destination);SetPhase(TeleportPhase.Arriving);break;
    case TeleportPhase.Arriving:
     foreach(var p in passengers){p.effect.Dispose();p.effect=null;if(!room){p.actor.inTransit=p.transit;p.actor.capsule.enabled=p.capsule;}p.actor.ResetLocomotion();}
     int local=passengers.FindIndex(p=>p.actor==app.LocalAthlete);app.view.yaw=Landings[local].rotation.eulerAngles.y;app.view.pitch=16;app.view.mode=oldMode;
     arrivalRotation=camera.transform.rotation;SetPhase(TeleportPhase.Settling);break;
    case TeleportPhase.Settling:if(room)RoomTeleport.Instance.Report(3);else Restore(false);break;
   }
  }
  public static Vector3 CameraPath(Vector3 from,Vector3 to,float height,float t){t=Mathf.SmoothStep(0,1,Mathf.Clamp01(t));float u=1-t;return u*u*u*from+3*u*u*t*(from+Vector3.up*height)+3*u*t*t*(to+Vector3.up*height)+t*t*t*to;}
  Vector3 FlightPosition(float progress){
   if(!entry||!app.LocalAthlete.IsSpectator||destination!=SportId.Football&&destination!=SportId.Basketball)return CameraPath(depart,arrival,curveHeight,progress);
   // Approach the covered seating from the open playing area, below the canopy,
   // instead of descending vertically through its roof.
   Vector3 center=SkySailMap.Center(destination),inward=Vector3.ProjectOnPlane(center-arrival,Vector3.up);
   Vector3 high=center+Vector3.up*110,approach=arrival+inward.normalized*Mathf.Min(36,inward.magnitude*.8f);
   float t=Mathf.SmoothStep(0,1,progress),u=1-t;return u*u*u*depart+3*u*u*t*high+3*u*t*t*approach+t*t*t*arrival;
  }
  void LateUpdate(){
   if(!Busy||Phase==TeleportPhase.Preparing||!camera)return;float t=PhaseProgress,smooth=Mathf.SmoothStep(0,1,t);Vector3 position=depart,focus=sourceFocus;Quaternion rotation;float amount=0,power=1;
   if(Phase==TeleportPhase.Charging){position=Vector3.Lerp(cameraStart,depart,Mathf.SmoothStep(0,1,Mathf.Min(1,t/.7f)));power=Mathf.SmoothStep(0,1,t);}
   else if(Phase==TeleportPhase.Absorbing)amount=smooth;
   else if(Phase==TeleportPhase.Travelling){amount=1;power=0;position=FlightPosition(t);focus=targetFocus;}
   else if(Phase==TeleportPhase.Arriving){position=arrival;focus=targetFocus;amount=1-Mathf.SmoothStep(0,1,Mathf.Clamp01(t/.72f));power=1-Mathf.SmoothStep(0,1,Mathf.Clamp01((t-.65f)/.35f));}
   foreach(var p in passengers)if(p.actor&&p.effect!=null)p.effect.Present(amount,power,Time.unscaledTime,Phase==TeleportPhase.Charging&&p.actor==app.LocalAthlete&&Vector3.Distance(position,sourceFocus)<1.1f);
   if(Phase==TeleportPhase.Settling){Vector3 native=camera.transform.position+world.Origin;position=Vector3.Lerp(arrival,native,smooth);rotation=Quaternion.Slerp(arrivalRotation,camera.transform.rotation,smooth);camera.fieldOfView=Mathf.Lerp(58,oldFov,smooth);}
   else {rotation=Quaternion.LookRotation((focus-position).normalized);if(Phase==TeleportPhase.Charging)rotation=Quaternion.Slerp(cameraRotation,rotation,Mathf.SmoothStep(0,1,Mathf.Min(1,t/.7f)));if(Phase==TeleportPhase.Travelling)rotation=Quaternion.Slerp(Quaternion.LookRotation(sourceFocus-depart),rotation,Mathf.SmoothStep(0,1,Mathf.Min(1,t/.25f)));camera.fieldOfView=Mathf.Lerp(oldFov,58,Phase==TeleportPhase.Charging?smooth:1);camera.nearClipPlane=.12f;}
   LargestCameraStep=Mathf.Max(LargestCameraStep,Vector3.Distance(lastCamera,position));lastCamera=position;camera.transform.SetPositionAndRotation(position-world.Origin,rotation);
  }
  public void Cancel(string reason=""){Error=reason;if(SceneLifetime.CanPresent(gameObject)&&room&&RoomTeleport.Busy)RoomTeleport.Instance.Report(1,true);Restore(true);}
  internal void FinishRoom(){if(room&&Busy)Restore(false);}
  void ClearArrivalCabin(){if(arrivalCabin){arrivalCabin.SetActive(false);Destroy(arrivalCabin);arrivalCabin=null;}}
  void Restore(bool rollback,bool resumePresentation=true){
   if(!Busy)return;version++;ClearArrivalCabin();
   resumePresentation&=SceneLifetime.CanPresent(gameObject)&&app&&SceneLifetime.CanPresent(app.gameObject);
   if(!resumePresentation){foreach(var p in passengers)if(p.effect!=null){p.effect.Dispose(false);p.effect=null;}passengers.Clear();canvases.Clear();SetPhase(TeleportPhase.Idle);return;}
   foreach(var p in passengers)if(p.effect!=null){p.effect.Dispose();p.effect=null;}
   if(app){
    if(rollback&&committed)app.SelectSport(source,true);
    foreach(var p in passengers)if(p.actor){
     p.actor.capsule.enabled=false;if(rollback)p.actor.transform.SetPositionAndRotation(p.position,p.rotation);
     if(p.networkTransform){
      p.networkTransform.enabled=p.transformEnabled;
      // A departing peer is despawned before Unity destroys its character.
      // Restore local presentation, but publish poses only for live authoritative objects.
      if(app.rooms.Host&&p.networkTransform.IsSpawned&&p.networkTransform.CanCommitToTransform)p.networkTransform.Teleport(p.actor.transform.position,p.actor.transform.rotation,Vector3.one);
     }
     p.actor.inTransit=p.transit;p.actor.ResetLocomotion();p.actor.capsule.enabled=p.capsule;p.actor.HideHead(false);p.actor.GetComponent<CharacterIdentity>()?.Refresh();
    }
    if(rollback){app.view.mode=oldMode;app.view.yaw=oldYaw;app.view.pitch=oldPitch;}
    app.view.InputBlocked=oldBlocked;app.view.ClearMatchInput();if(world)world.SetIsland(app.SelectedSport);
   }
   foreach(var pair in canvases)if(pair.Key)pair.Key.enabled=pair.Value;canvases.Clear();
   if(camera){camera.fieldOfView=oldFov;camera.nearClipPlane=oldNear;if(rollback)camera.transform.SetPositionAndRotation(cameraStart-(world?world.Origin:Vector3.zero),cameraRotation);}
   if(app&&streaming){var selected=app.SelectedSport;for(int i=0;i<app.environments.roots.Length;i++)if(app.environments.roots[i]){app.environments.roots[i].SetActive(i==(int)selected);app.environments.roots[i].transform.position=Vector3.zero;}streaming.ReleaseOtherIslands(selected);if(world)world.SetIsland(selected);}
   passengers.Clear();SetPhase(TeleportPhase.Idle);if(app&&app.Exploring)app.Show("stadium");
  }
  void OnDisable(){if(Busy&&Instance==this){if(SceneLifetime.CanPresent(gameObject)&&app&&SceneLifetime.CanPresent(app.gameObject))Cancel();else Restore(true,false);}}
  void OnDestroy(){if(Instance==this)Instance=null;}
 }
}
