#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace WhatTheFish {
 public sealed class BasketballAudioReview:MonoBehaviour {
  string folder,shared;bool failed;AppRoot app;BasketballBall ball;Athlete actor;BasketballAudio audio;Transform arena;
  readonly Dictionary<BasketballSound,int> counts=new();readonly HashSet<uint> networkIds=new();int duplicate;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Init(){var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,"-basketballAudioReview");if(i>=0&&i+1<args.Length)new GameObject("Basketball audio review").AddComponent<BasketballAudioReview>().folder=args[i+1];}
  void Check(bool ok,string message){failed|=!ok;File.AppendAllText(Path.Combine(folder,"results.txt"),(ok?"PASS ":"FAIL ")+message+"\n");}
  int Count(BasketballSound cue)=>counts.TryGetValue(cue,out int n)?n:0;
  void Heard(BasketballSound cue,Vector3 point,float gain,string source,uint id){counts[cue]=Count(cue)+1;if(source=="network"&&!networkIds.Add(id))duplicate++;File.AppendAllText(Path.Combine(folder,"events.tsv"),$"{BasketballMotion.Clock:F4}\t{cue}\t{source}\t{id}\t{gain:F3}\t{point}\n");}
  void PlaceActor(Vector3 local){actor.capsule.enabled=false;actor.transform.SetPositionAndRotation(arena.TransformPoint(local),Quaternion.identity);var net=actor.GetComponent<NetworkTransform>();if(net&&net.IsSpawned)net.Teleport(actor.transform.position,actor.transform.rotation,Vector3.one);actor.capsule.enabled=true;actor.ResetLocomotion();Physics.SyncTransforms();}
  IEnumerator Pickup(Vector3 local){app.view.ClearMatchInput();DevelopmentProbe.TurnCommand=default;PlaceActor(local);ball.ResetHome();ball.autoPickup=true;ball.Place(local+new Vector3(0,.2f,.6f),Quaternion.identity,Vector3.zero,Vector3.zero);float until=Time.time+2;while(ball.Holder!=actor&&Time.time<until)yield return null;yield return new WaitForSeconds(.2f);Check(ball.Holder==actor,"actual ball pickup");}
  bool Use(SportsProp kind){ball.ReviewGiveProp(actor,kind);return ball.TryUseProp(actor,ball.PropSlot(actor).revision);}
  IEnumerator Start(){
   Directory.CreateDirectory(folder);shared=Directory.GetParent(folder).FullName;File.WriteAllText(Path.Combine(folder,"results.txt"),"");BasketballAudio.Trace+=Heard;
   float end=Time.realtimeSinceStartup+70;while((!(app=AppRoot.Instance)||!app.Exploring||app.SelectedSport!=SportId.Basketball)&&Time.realtimeSinceStartup<end)yield return null;
   ball=BasketballBall.Active;actor=app?app.LocalAthlete:null;audio=BasketballAudio.Instance;
   if(!ball||!actor||!audio){Check(false,"court initialized");Finish();yield break;}
   arena=ball.transform.parent;DevelopmentProbe.Driving=false;DevelopmentProbe.TurnCommandActive=true;DevelopmentProbe.TurnCommand=default;app.view.mode=2;yield return new WaitForSeconds(.4f);
   Check(audio.LoadedClips==58,"58 approved clips loaded: "+audio.LoadedClips);Check(FindObjectsByType<AudioListener>(FindObjectsSortMode.None).Count(x=>x.enabled)==1,"one enabled listener");
   Check(Mathf.Abs(audio.ReviewClip(BasketballSound.LockAim).length-2)<.005f,"soft aim clip remains exactly two seconds");
   var aimClip=audio.ReviewClip(BasketballSound.LockAim);var samples=new float[aimClip.samples];aimClip.GetData(samples,0);float peak=samples.Max(x=>Mathf.Abs(x));Check(peak>.03f&&peak<.085f,"import preserves quiet aim level, peak="+peak);
   if(!ball.Authority){
    end=Time.realtimeSinceStartup+110;while(!File.Exists(Path.Combine(shared,"host-done"))&&Time.realtimeSinceStartup<end)yield return null;yield return new WaitForSeconds(.3f);
    Check(File.Exists(Path.Combine(shared,"host-done")),"host completed gameplay sequence");
    foreach(var cue in new[]{BasketballSound.Bounce,BasketballSound.Board,BasketballSound.Rim,BasketballSound.Catch,BasketballSound.Release,BasketballSound.Net,BasketballSound.Score,BasketballSound.Pickup,BasketballSound.LockFire,BasketballSound.Interrupt,BasketballSound.TurboStart,BasketballSound.PortalIn,BasketballSound.PortalOut})Check(Count(cue)>0,"guest heard "+cue);
    Check(duplicate==0&&networkIds.Count>12,"guest reliable event IDs unique: "+networkIds.Count);Check(BasketballAudio.NetworkDropped==0,"no stale/overflow live events dropped");
    Finish();yield break;
   }
   foreach(var other in Athlete.Active)if(other!=actor&&other.GetComponent<NetworkObject>()&&other.GetComponent<NetworkObject>().IsSpawned){other.capsule.enabled=false;other.transform.position=arena.TransformPoint(new Vector3(5,.08f,8));other.GetComponent<NetworkTransform>().Teleport(other.transform.position,other.transform.rotation,Vector3.one);other.capsule.enabled=true;}
   yield return Pickup(new Vector3(0,.08f,-6));int bounces=Count(BasketballSound.Bounce);yield return new WaitForSeconds(2.1f);Check(Count(BasketballSound.Bounce)-bounces>=3&&Count(BasketballSound.Bounce)-bounces<=5,"standing dribbles follow presented contacts without duplicates");
   int steps=Count(BasketballSound.Walk)+Count(BasketballSound.Run);DevelopmentProbe.TurnCommand=new PlayerCommand{move=Vector2.up};yield return new WaitForSeconds(1.3f);DevelopmentProbe.TurnCommand=new PlayerCommand{move=Vector2.up,sprint=true,heading=180};yield return new WaitForSeconds(1.3f);DevelopmentProbe.TurnCommand=default;yield return new WaitForSeconds(.3f);Check(Count(BasketballSound.Walk)+Count(BasketballSound.Run)>steps+2,"moving feet produce contact sounds");
   int landed=Count(BasketballSound.Land);actor.RequestJump();yield return new WaitForSeconds(1.8f);Check(Count(BasketballSound.Land)>landed,"jump landing produces one contact");
   uint passes=ball.PassCount;int released=Count(BasketballSound.Release);Check(ball.TryPass(actor,0),"pass accepted");end=Time.time+2;while(ball.PassCount==passes&&Time.time<end)yield return null;Check(Count(BasketballSound.Release)==released+1,"pass release sounds at actual release once");
   ball.autoPickup=false;PlaceActor(new Vector3(0,.08f,-6));ball.ResetHome();
   int floor=Count(BasketballSound.Bounce);ball.Place(new Vector3(2,1.5f,0),Quaternion.identity,Vector3.down,Vector3.zero);yield return new WaitForSeconds(1);Check(Count(BasketballSound.Bounce)>floor,"physical floor collision sounds");
   var board=arena.GetComponentsInChildren<Collider>().First(c=>c.name=="Hoop__BoardInset_Collision");var hoop=board.GetComponentInParent<BasketballHoop>().transform;
   int boards=Count(BasketballSound.Board);var start=board.bounds.center+hoop.forward*.8f;ball.Place(arena.InverseTransformPoint(start),Quaternion.identity,-hoop.forward*5,Vector3.zero);yield return new WaitForSeconds(.5f);Check(Count(BasketballSound.Board)>boards,"physical backboard collision sounds");
   int rims=Count(BasketballSound.Rim);ball.Place(arena.InverseTransformPoint(hoop.TransformPoint(new Vector3(.2476f,3.65f,0))),Quaternion.identity,Vector3.down*2,Vector3.zero);yield return new WaitForSeconds(.5f);Check(Count(BasketballSound.Rim)>rims,"physical rim collision sounds separately");
   ball.ReviewSpawnProps(SportsProp.Turbo,0);PlaceActor(BasketballPropRules.Anchor(0)+Vector3.up*.02f);int pickups=Count(BasketballSound.Pickup);yield return new WaitForSeconds(.3f);Check(Count(BasketballSound.Pickup)==pickups+1,"real prop pickup sounds once");
   yield return Pickup(new Vector3(0,.08f,-6));Check(Use(SportsProp.LockOn),"Lock-on accepted");yield return new WaitForSeconds(.3f);Check(audio.AimPlaying&&audio.ReviewAimVolume>0,"aim sound runs during accepted aim");
   float sfx=MenuPreferences.SFXVolume;MenuPreferences.SetSFX(0);yield return null;yield return null;Check(audio.ReviewAimVolume==0,"court-sound mute applies to ongoing aim");MenuPreferences.SetSFX(sfx);
   end=Time.time+8;while(ball.Props.aiming||ball.Score.result==BasketballResult.Flying){if(Time.time>end)break;yield return null;}
   Check(!audio.AimPlaying&&Count(BasketballSound.LockFire)>0,"aim stops at release and release accent plays");Check(Count(BasketballSound.Score)>0&&Count(BasketballSound.Net)>0,"actual scoring pairs net and confirmed score");
   yield return Pickup(new Vector3(0,.08f,-6));Check(Use(SportsProp.LockOn),"second aim accepted");yield return new WaitForSeconds(.3f);int fires=Count(BasketballSound.LockFire);ball.ResetHome();ball.autoPickup=false;yield return new WaitForSeconds(.12f);Check(!audio.AimPlaying&&Count(BasketballSound.Interrupt)>0,"interruption stops aim immediately");yield return new WaitForSeconds(2.1f);Check(Count(BasketballSound.LockFire)==fires,"cancelled aim never fires later");
   PlaceActor(new Vector3(0,.08f,-6));yield return new WaitForSeconds(.2f);ball.Place(new Vector3(2,1,5),Quaternion.identity,Vector3.zero,Vector3.zero);Check(Use(SportsProp.Portal),"portal accepted");yield return new WaitForSeconds(.9f);Check(ball.Holder==actor&&Count(BasketballSound.PortalIn)>0&&Count(BasketballSound.PortalOut)>0&&Count(BasketballSound.PortalClose)>0,"portal intake arrival catch and closure follow transfer");
   yield return new WaitForSeconds(.4f);Check(Use(SportsProp.Turbo),"turbo accepted");yield return new WaitForSeconds(5.2f);Check(Count(BasketballSound.TurboStart)>0&&Count(BasketballSound.TurboEnd)>0,"turbo start and expiry follow actual five-second effect");
   var button=FindObjectsByType<Button>(FindObjectsSortMode.None).First(x=>x.name=="Court settings");int clicks=Count(BasketballSound.UIOpen);button.onClick.Invoke();yield return null;Check(Count(BasketballSound.UIOpen)==clicks+1,"settings click exactly once");
   Check(BasketballHUD.Instance.GetComponentsInChildren<Slider>(true).Count()==4,"four independent volume controls");BasketballHUD.Instance.CloseMenu();
   yield return null;var shoot=FindObjectsByType<BasketballShootButton>(FindObjectsSortMode.None).Single(x=>!x.pass);int taps=Count(BasketballSound.UIClick);var touch=new PointerEventData(EventSystem.current){pointerId=809,button=PointerEventData.InputButton.Left};ExecuteEvents.Execute(shoot.gameObject,touch,ExecuteEvents.pointerDownHandler);ExecuteEvents.Execute(shoot.gameObject,touch,ExecuteEvents.pointerDownHandler);yield return null;shoot.CancelGesture();ExecuteEvents.Execute(shoot.gameObject,touch,ExecuteEvents.pointerUpHandler);Check(Count(BasketballSound.UIClick)==taps+1,"held action press clicks once despite duplicate pointer-down");
   Check(FindObjectsByType<BasketballShootButton>(FindObjectsSortMode.None).All(x=>x.GetComponent<BasketballButtonFeedback>())&&FindObjectsByType<BasketballStealButton>(FindObjectsSortMode.None).All(x=>x.GetComponent<BasketballButtonFeedback>()),"all action buttons have sound feedback");
   float master=MenuPreferences.MasterVolume;MenuPreferences.SetMaster(0);Check(AudioListener.volume==0,"master mute covers every source");MenuPreferences.SetMaster(master);
   Check(networkIds.Count==0,"host never replays its network events");
   audio.SendMessage("OnApplicationPause",true);Check(!audio.AimPlaying&&audio.PlayingVoices==0,"application suspend stops active voices");audio.SendMessage("OnApplicationPause",false);yield return null;
   File.WriteAllText(Path.Combine(shared,"host-done"),"ready");Finish();
  }
  void Finish(){BasketballAudio.Trace-=Heard;MenuPreferences.Save();DevelopmentProbe.TurnCommandActive=false;DevelopmentProbe.TurnCommand=default;File.AppendAllText(Path.Combine(folder,"results.txt"),"BASKETBALL_AUDIO_COMPLETE success="+!failed+"\n");Application.Quit(failed?1:0);}
 }
}
#endif
