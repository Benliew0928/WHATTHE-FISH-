using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WhatTheFish {
 [DefaultExecutionOrder(70)]
 public sealed class GolfCartWorld:MonoBehaviour {
  public const ulong OfflineOwner=ulong.MaxValue-1;
  public const float DriveDistance=3.35f;
  public static GolfCartWorld Instance {get;private set;}
  sealed class Record {public Athlete owner;public GolfCartState state;public GolfCart view;public Vector3 recoil;public float impactAt=-10;}
  readonly Dictionary<ulong,Record> records=new();GolfCart prefab;
  static AppRoot App=>AppRoot.Instance;
  public static bool Authority=>App&&(!App.rooms.Connected||App.rooms.Host);
  public static bool Allowed=>App&&App.Exploring&&App.SelectedSport==SportId.Golf&&!(SkySailWorld.Instance&&SkySailWorld.Instance.Travelling);
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)] static void Boot(){Ensure();}
  public static GolfCartWorld Ensure(){if(!Instance)new GameObject("Golf carts").AddComponent<GolfCartWorld>();return Instance;}
  void Awake(){Instance=this;}
  void OnDestroy(){foreach(var record in records.Values)if(record.view)Destroy(record.view.gameObject);if(Instance==this)Instance=null;}
  public static ulong Key(Athlete athlete){var network=athlete?athlete.GetComponent<NetworkAthlete>():null;return network&&network.StableId!=ulong.MaxValue?network.StableId:IslandBots.IsBot(athlete)?IslandBots.Id(athlete):OfflineOwner;}
  public static int Count=>Instance?Instance.records.Values.Count(r=>r.state.summoned):0;
  public static GolfCart Owned(Athlete athlete)=>athlete&&Instance&&Instance.records.TryGetValue(Key(athlete),out var record)&&record.state.summoned?record.view:null;
  public static bool HasCart(Athlete athlete)=>athlete&&Instance&&Instance.records.TryGetValue(Key(athlete),out var record)&&record.state.summoned;
  public static GolfCartState State(Athlete athlete)=>athlete&&Instance&&Instance.records.TryGetValue(Key(athlete),out var record)?record.state:default;
  public static GolfCart Driving(Athlete athlete){
   if(!athlete||!Instance)return null;ulong key=Key(athlete);
   foreach(var record in Instance.records.Values)if(record.state.summoned&&record.state.driver==key)return record.view;
   return null;
  }
  public static GolfCart Nearest(Athlete athlete){
   if(!athlete||!Allowed||!Instance)return null;GolfCart found=null;float best=DriveDistance;
   foreach(var record in Instance.records.Values){
    if(!record.state.summoned||record.state.driver!=GolfCartState.NoDriver||!record.view)continue;
    var delta=record.view.transform.position-athlete.transform.position;delta.y=0;
    if(delta.magnitude<best&&Mathf.Abs(record.view.transform.position.y-athlete.transform.position.y)<2){best=delta.magnitude;found=record.view;}
   }
   return found;
  }
  public static bool Execute(Athlete actor,GolfCartAction action,ulong target) {
   if(!actor||actor.IsSpectator||!Authority||!Allowed||actor.inTransit)return false;
   var world=Ensure();ulong caller=Key(actor);
   if(action==GolfCartAction.Summon)return caller==target&&world.Summon(actor,caller);
   if(action==GolfCartAction.Recall)return caller==target&&world.Recall(actor,caller);
   if(action==GolfCartAction.Drive)return world.Enter(actor,target);
   if(action==GolfCartAction.Leave)return world.Exit(actor,target,true);
   return false;
  }
  bool Summon(Athlete owner,ulong key){
   if(GolfMatchManager.Instance&&GolfMatchManager.Instance.IsAiming(owner))return false;
   if(records.TryGetValue(key,out var existing)&&existing.state.summoned)return false;
   if(!prefab)prefab=Resources.Load<GolfCart>("GolfCart");if(!prefab)return false;
   var forward=owner.transform.forward;forward.y=0;forward.Normalize();
   var right=Vector3.Cross(Vector3.up,forward);bool placed=false;Vector3 position=default;Quaternion rotation=default;
   foreach(var offset in new[]{forward*3.8f,right*3.8f,-right*3.8f,-forward*3.8f,forward*5.3f,right*5.3f,-right*5.3f}){
    var proposed=owner.transform.position+offset;
    if(GolfCartMotor.Surface(proposed,owner.transform.eulerAngles.y,out position,out rotation)&&Mathf.Abs(position.y-owner.transform.position.y)<1.8f&&GolfCartMotor.Clear(position,rotation,null,owner)){placed=true;break;}
   }
   if(!placed)return false;
   var record=existing??new Record();record.owner=owner;record.recoil=Vector3.zero;record.impactAt=-10;
   record.state=new GolfCartState{summoned=true,position=position,rotation=rotation,driver=GolfCartState.NoDriver,revision=record.state.revision+1};
   records[key]=record;CreateView(key,record);record.view.Apply(record.state,true);return true;
  }
  bool Recall(Athlete owner,ulong key){
   if(!records.TryGetValue(key,out var record)||record.owner!=owner||!record.state.summoned)return false;
   var driver=Actor(record.state.driver);if(driver)Exit(driver,key,true);
   record.state.summoned=false;record.state.driver=GolfCartState.NoDriver;record.state.speed=0;record.recoil=Vector3.zero;record.state.revision++;
   if(record.view){record.view.gameObject.SetActive(false);Destroy(record.view.gameObject);}record.view=null;return true;
  }
  bool Enter(Athlete actor,ulong key){
   if(Driving(actor)||actor.Airborne||actor.LoadingJump||!actor.Grounded||!records.TryGetValue(key,out var record)||!record.state.summoned||record.state.driver!=GolfCartState.NoDriver||!record.view)return false;
   var delta=record.state.position-actor.transform.position;delta.y=0;
   if(delta.magnitude>DriveDistance||Mathf.Abs(record.state.position.y-actor.transform.position.y)>2)return false;
   // A short visibility check prevents entering through pavilion walls.
   var from=actor.transform.position+Vector3.up*.9f;var to=record.view.seat.position;
   if(Physics.Linecast(from,to,out var hit,1<<8,QueryTriggerInteraction.Ignore)&&!GolfCartMotor.Terrain(hit.collider))return false;
   actor.ResetLocomotion();actor.GetComponent<NetworkAthlete>()?.ClearMatchInput();actor.capsule.enabled=false;
   record.state.driver=Key(actor);record.state.speed=0;record.state.revision++;
   record.view.Apply(record.state,true);record.view.PlaceDriver(actor);
   if(App.LocalAthlete==actor)App.view.ClearMatchInput();
   return true;
  }
  bool Exit(Athlete actor,ulong key,bool relocate){
   if(!records.TryGetValue(key,out var record)||!record.state.summoned||record.state.driver!=Key(actor))return false;
   Vector3 position=actor.transform.position;bool found=false;
   if(relocate&&record.view){
    foreach(float radius in new[]{2.15f,3.0f,4.0f}){
     foreach(var direction in new[]{Vector3.right,Vector3.left,Vector3.back,Vector3.forward,new Vector3(1,0,1).normalized,new Vector3(-1,0,-1).normalized}){
      var candidate=record.state.position+record.state.rotation*direction*radius;
      if(GolfCartMotor.Floor(candidate,out var floor)&&GolfCartMotor.ExitClear(floor.point,actor,record.view)){position=floor.point+Vector3.up*.08f;found=true;break;}
     }
     if(found)break;
    }
    if(!found)position=App.environments.Definition(SportId.Golf).Spawn(0);
   }
   record.state.driver=GolfCartState.NoDriver;record.state.speed=record.state.steering=0;record.state.revision++;
   actor.capsule.enabled=false;if(relocate)actor.transform.position=position;
   actor.ResetLocomotion();actor.capsule.enabled=!actor.inTransit&&Authority;
   actor.GetComponent<NetworkAthlete>()?.ClearMatchInput();if(App.LocalAthlete==actor)App.view.ClearMatchInput();
   return true;
  }
  public static bool Simulate(Athlete actor,PlayerCommand command,float dt){
   var cart=Driving(actor);if(!cart)return false;
   if(!Authority||!Allowed||actor.inTransit)return true;
   var record=Instance.records[cart.Owner];
   // Do not let held throttle immediately cancel the collision separation.
   // Release driving once nearly all of the bounded recoil has been travelled.
   if(record.recoil.sqrMagnitude>.04f){record.state.speed=0;record.state.steering=command.move.x*GolfCartMotor.WheelSteeringAngle;}
   else GolfCartMotor.Step(cart,ref record.state,command.move,dt);
   cart.Apply(record.state,true);cart.PlaceDriver(actor);actor.speed=0;return true;
  }
  public static void Impact(GolfCart cart,GolfCart other,Vector3 normal){
   if(!Authority||!Allowed||!cart||!other||cart==other||!Instance||!Instance.records.TryGetValue(cart.Owner,out var a)||!Instance.records.TryGetValue(other.Owner,out var b))return;
   if(!a.state.summoned||!b.state.summoned||Time.fixedTime-a.impactAt<.35f||Time.fixedTime-b.impactAt<.35f)return;
   normal=Vector3.ProjectOnPlane(normal,Vector3.up);
   if(normal.sqrMagnitude<.01f)normal=Vector3.ProjectOnPlane(a.state.position-b.state.position,Vector3.up);
   if(normal.sqrMagnitude<.01f)return;normal.Normalize();
   var av=a.state.rotation*Vector3.forward*a.state.speed+a.recoil;var bv=b.state.rotation*Vector3.forward*b.state.speed+b.recoil;
   float closing=-Vector3.Dot(av-bv,normal);if(closing<.8f)return;
   // Exact exponential integration gives 1-3 m per cart on unobstructed ground.
   float impulse=5*Mathf.Lerp(1,3,Mathf.InverseLerp(.8f,20,closing));
   a.recoil=normal*impulse;b.recoil=-normal*impulse;
   a.state.speed*=.2f;b.state.speed*=.2f;a.impactAt=b.impactAt=Time.fixedTime;
   GolfFeedback.Emit(GolfFeedbackCue.CartBump,(a.state.position+b.state.position)*.5f+Vector3.up*.65f,normal,Mathf.Clamp01(closing/15),cart.Owner,other.Owner);
  }
  void FixedUpdate(){
   if(!Authority)return;
   foreach(var record in records.Values){
    if(!Allowed||!record.state.summoned||!record.view){record.recoil=Vector3.zero;continue;}
    if(record.recoil.sqrMagnitude<.0025f){record.recoil=Vector3.zero;continue;}
    if(!GolfCartMotor.Recoil(record.view,ref record.state,record.recoil*((1-Mathf.Exp(-5*Time.fixedDeltaTime))/5)))record.recoil=Vector3.zero;
    else record.recoil*=Mathf.Exp(-5*Time.fixedDeltaTime);
    record.view.Apply(record.state,true);var driver=Actor(record.state.driver);if(driver)record.view.PlaceDriver(driver);
   }
  }
  static Athlete Actor(ulong key)=>key==GolfCartState.NoDriver?null:Athlete.Active.FirstOrDefault(a=>Key(a)==key);
  public static Athlete ActorFor(ulong key)=>Actor(key);
  void CreateView(ulong key,Record record){
   if(record.view)return;if(!prefab)prefab=Resources.Load<GolfCart>("GolfCart");if(!prefab)return;
   record.view=Instantiate(prefab,record.state.position,record.state.rotation);record.view.name="Golf cart · "+key;record.view.Owner=key;
  }
  public static void Receive(Athlete owner,GolfCartState state){
   if(Authority||!owner)return;var world=Ensure();ulong key=Key(owner);
   if(!world.records.TryGetValue(key,out var record)){record=new Record{owner=owner};world.records[key]=record;}
   record.state=state;
  }
  public static void ParkDrivers(bool relocate){
   if(!Instance||!Authority)return;
   foreach(var item in Instance.records.ToArray()){var driver=Actor(item.Value.state.driver);if(driver&&item.Value.state.summoned)Instance.Exit(driver,item.Key,relocate);}
  }
  public static void Forget(Athlete actor){
   if(!Instance||!actor)return;ulong key=Key(actor);
   foreach(var item in Instance.records.ToArray()){
    if(item.Value.state.summoned&&item.Value.state.driver==key&&Authority)Instance.Exit(actor,item.Key,false);
    if(item.Value.owner!=actor)continue;
    if(Authority){var driver=Actor(item.Value.state.driver);if(driver)Instance.Exit(driver,item.Key,true);}
    if(item.Value.view)Destroy(item.Value.view.gameObject);Instance.records.Remove(item.Key);
   }
  }
  void Update(){
   if(!App)return;
   if(Authority&&!Allowed)ParkDrivers(App.SelectedSport==SportId.Golf&&!(SkySailWorld.Instance&&SkySailWorld.Instance.Travelling));
   foreach(var item in records.ToArray()){
    var record=item.Value;if(!record.owner){if(record.view)Destroy(record.view.gameObject);records.Remove(item.Key);continue;}
    if(!record.state.summoned){if(record.view)Destroy(record.view.gameObject);record.view=null;continue;}
    if(Allowed){CreateView(item.Key,record);if(record.view){record.view.gameObject.SetActive(true);record.view.Apply(record.state,Authority);}}
    else if(record.view)record.view.gameObject.SetActive(false);
   }
  }
 }
}
