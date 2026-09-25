#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SportsPrototype {
 public sealed partial class DevelopmentProbe {
  void PlaceAthlete(Athlete a,Vector3 position,float yaw){a.capsule.enabled=false;a.transform.SetPositionAndRotation(position,Quaternion.Euler(0,yaw,0));a.ResetLocomotion();a.capsule.enabled=true;Physics.SyncTransforms();}
  IEnumerator TackleAudit(){
   yield return new WaitForSeconds(2);var app=AppRoot.Instance;app.SelectSport(SportId.Football);app.EnterOffline();app.view.mode=1;app.view.yaw=-35;
   TurnCommandActive=true;TurnCommand=default;
   var actor=app.LocalAthlete;var rival=Instantiate(app.athletePrefab).GetComponent<Athlete>();rival.name="Tackle test opponent";rival.Setup();
   var source=actor.GetComponent<FootballTackle>();var victim=rival.GetComponent<FootballTackle>();
   yield return new WaitForSeconds(.6f);var origin=actor.transform.position;
   var button=FindFirstObjectByType<TackleButton>();Check(button,"FOOTBALL_TACKLE_BUTTON");
   if(args.Contains("-tackleVideo")){Time.captureFramerate=30;turnFrames=Path.Combine(Path.GetDirectoryName(output),"frames");Directory.CreateDirectory(turnFrames);turnFrame=0;recordingTurn=true;}
   foreach(int lod in new[]{0,1}){
    actor.GetComponentInChildren<LODGroup>().ForceLOD(lod);rival.GetComponentInChildren<LODGroup>().ForceLOD(lod);
    PlaceAthlete(actor,origin,0);PlaceAthlete(rival,origin+Vector3.forward*3.8f,180);TurnCommand=default;yield return null;
    int dealt=source.HitsDealt,received=victim.HitsReceived;var first=actor.transform.position;var rivalStart=rival.transform.position;
    button.OnPointerDown(new PointerEventData(EventSystem.current){button=PointerEventData.InputButton.Left});
    yield return null;
    Check(actor.Action==FootballAction.Slide&&Vector3.Dot(actor.transform.position-first,Vector3.forward)>.01f,"TACKLE_IMMEDIATE_LOD"+lod);
    // Repeated presses during cooldown must not queue another slide.
    app.view.RequestTackle();app.view.RequestTackle();
    bool hit=false,slideClip=false,hitClip=false,recoveredInput=false;float until=Time.time+1.3f;
    while(Time.time<until){
     rival.Simulate(default,Time.deltaTime);hit|=rival.Action==FootballAction.Hit;
     var animator=actor.GetComponentInChildren<Animator>();var rivalAnimator=rival.GetComponentInChildren<Animator>();
     slideClip|=animator.GetCurrentAnimatorClipInfo(2).Any(c=>c.clip.name=="Slide_Tackle")&&animator.GetLayerWeight(2)>.9f;
     hitClip|=rivalAnimator.GetCurrentAnimatorClipInfo(2).Any(c=>c.clip.name=="Tackle_Hit")&&rivalAnimator.GetLayerWeight(2)>.8f;
     if(source.Elapsed>=FootballTackle.SlideTravel&&actor.Action==FootballAction.Slide){
      TurnCommand=new PlayerCommand{move=Vector2.right,sprint=true};var before=actor.transform.position;yield return null;
      recoveredInput|=actor.transform.position.x>before.x+.01f;
     }else {if(actor.Action==FootballAction.None)TurnCommand=default;yield return null;}
    }
    Check(hit&&source.HitsDealt-dealt==1&&victim.HitsReceived-received==1,"TACKLE_SINGLE_HIT_LOD"+lod);
    Check(Vector3.Dot(rival.transform.position-rivalStart,Vector3.forward)>.3f,"TACKLE_OPPONENT_SLIDES_LOD"+lod);
    Check(slideClip&&hitClip&&recoveredInput,"TACKLE_ANIMATION_AND_EARLY_CONTROL_LOD"+lod);
    Check(actor.Action==FootballAction.None&&actor.TackleReady,"TACKLE_RECOVERED_NO_QUEUED_REPEAT_LOD"+lod);
   }
   // Miss, facing (rather than camera direction), wall obstruction and airborne rejection.
   PlaceAthlete(actor,origin,90);PlaceAthlete(rival,origin+Vector3.forward*4,180);TurnCommand=default;yield return null;
   int hits=source.HitsDealt;var start=actor.transform.position;app.view.RequestTackle();yield return null;
   Check(actor.transform.position.x>start.x+.01f&&Mathf.Abs(actor.transform.position.z-start.z)<.01f,"TACKLE_FACING_NOT_CAMERA");
   yield return new WaitForSeconds(1.2f);Check(source.HitsDealt==hits,"TACKLE_MISS_NO_HIT");
   Check(Vector3.Distance(actor.transform.position,start)>4.4f,"TACKLE_FULL_SLIDE_DISTANCE");recordingTurn=false;Time.captureFramerate=0;
   PlaceAthlete(actor,origin,0);PlaceAthlete(rival,origin+Vector3.forward*1.8f,180);
   var wall=GameObject.CreatePrimitive(PrimitiveType.Cube);wall.layer=8;wall.transform.position=origin+Vector3.forward+Vector3.up;wall.transform.localScale=new Vector3(4,2,.15f);Physics.SyncTransforms();
   app.view.RequestTackle();yield return null;float end=Time.time+1;
   while(Time.time<end){rival.Simulate(default,Time.deltaTime);yield return null;}
   Check(actor.transform.position.z<origin.z+.7f&&source.HitsDealt==hits,"TACKLE_WALL_BLOCKS_SLIDE_AND_HIT");Destroy(wall);
   PlaceAthlete(actor,origin+Vector3.up*3,0);Check(!actor.TryTackle(),"TACKLE_AIRBORNE_REJECTED");
   Destroy(rival.gameObject);
   foreach(var sport in new[]{SportId.Basketball,SportId.Golf}){
    app.SelectSport(sport);app.EnterOffline();yield return new WaitForSeconds(.25f);
    Check(!FindFirstObjectByType<TackleButton>()&&!actor.TryTackle(),"TACKLE_UNAVAILABLE_"+sport);
   }
   actor.GetComponentInChildren<LODGroup>().ForceLOD(-1);TurnCommandActive=false;Record("TACKLE_GAMEPLAY_COMPLETE");
  }
  IEnumerator NetworkTackleAudit(){
   TurnCommandActive=true;TurnCommand=default;
   while(!AppRoot.Instance||!AppRoot.Instance.Exploring)yield return null;
   float start=float.Parse(Value("-startAt","10"));bool positioned=false,repositioned=false,first=false,second=false;
   var slides=new HashSet<ulong>();var hits=new HashSet<ulong>();var travel=new Dictionary<ulong,float>();var positions=new Dictionary<ulong,Vector3>();
   while(elapsed<start+10){
    var players=FindObjectsByType<NetworkAthlete>(FindObjectsSortMode.None);var app=AppRoot.Instance;
    if(app.rooms.Host&&(!positioned||elapsed>start+5&&!repositioned)){
     foreach(var p in players){var a=p.GetComponent<Athlete>();var pos=new Vector3(-4,.1f,-8+(p.OwnerClientId==0?0:1.35f));float yaw=p.OwnerClientId==0?0:180;PlaceAthlete(a,pos,yaw);p.GetComponent<NetworkTransform>().Teleport(pos,Quaternion.Euler(0,yaw,0),Vector3.one);}
     if(positioned)repositioned=true;positioned=true;
    }
    if(app.rooms.Host&&elapsed>start+2&&!first){app.view.RequestTackle();first=true;}
    if(!app.rooms.Host&&elapsed>start+7&&!second){app.view.RequestTackle();second=true;}
    foreach(var p in players){
     var a=p.GetComponent<Athlete>();var id=p.OwnerClientId;
     if(a.Action==FootballAction.Slide)slides.Add(id);
     if(a.Action==FootballAction.Hit){hits.Add(id);if(positions.TryGetValue(id,out var before)){var delta=a.transform.position-before;delta.y=0;travel[id]=travel.GetValueOrDefault(id)+delta.magnitude;}}
     positions[id]=a.transform.position;
    }
    yield return null;
   }
   Check(slides.Count==expected&&hits.Count==expected,"NETWORK_BOTH_PLAYERS_TACKLE_AND_REACT");
   Check(travel.Count==expected&&travel.All(p=>p.Value>.2f),"NETWORK_TACKLE_KNOCKBACK "+string.Join(",",travel.Select(p=>p.Key+":"+p.Value)));
   TurnCommandActive=false;Record("NETWORK_TACKLE_COMPLETE");
  }
 }
}
#endif
