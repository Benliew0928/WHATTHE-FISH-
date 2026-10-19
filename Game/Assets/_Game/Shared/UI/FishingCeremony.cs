using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace WhatTheFish {
 // The courts' render-only actors and celebration takes; real players never move.
 [DefaultExecutionOrder(650)]
 public sealed class FishingCeremony:MonoBehaviour {
  public static FishingCeremony Instance {get;private set;}
  public static bool Presenting=>Instance&&Instance.running;
  readonly List<FootballCelebrationActor> cast=new();readonly Dictionary<Renderer,bool> hidden=new();
  Camera camera;Vector3 savedPosition;Quaternion savedRotation;float savedFov,savedNear,opened,skipAt=-1;bool running,blockedBefore;
  Transform stage;RectTransform screen,content;Image curtain;Text title;FootballTake[] takes;FishingGame game;uint round;Action complete;
  CanvasGroup hud;float hudAlpha;bool hudRaycasts;bool revealed;
  public int CastCount=>cast.Count;
  public bool Begin(FishingGame source,FishingPlayerRecord[] records,Action onComplete){
   if(running||Presenting||!source||records.Length==0||source.State.Match.outcome==FishingOutcome.Abandoned||TeleportSequence.Busy||RoomTeleport.Busy)return false;
   camera=Camera.main;if(!camera||!AppRoot.Instance.LocalAthlete)return false;
   game=source;round=source.State.Match.round;complete=onComplete;Instance=this;PostMatchFlow.Instance?.Close();
   savedPosition=camera.transform.position;savedRotation=camera.transform.rotation;savedFov=camera.fieldOfView;savedNear=camera.nearClipPlane;
   running=true;
   try {
   var view=PlayerView.Instance;blockedBefore=view&&view.InputBlocked;if(view){view.ClearMatchInput();view.InputBlocked=true;}
   var page=FishingHUD.Instance?FishingHUD.Instance.HudFrame:null;
   hud=page?(page.GetComponent<CanvasGroup>()??page.gameObject.AddComponent<CanvasGroup>()):null;if(hud){hudAlpha=hud.alpha;hudRaycasts=hud.blocksRaycasts;hud.alpha=0;hud.blocksRaycasts=false;}
   screen=new GameObject("Fishing awards overlay",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster),typeof(CanvasGroup)).GetComponent<RectTransform>();
   var canvas=screen.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.overrideSorting=true;canvas.sortingOrder=950;screen.GetComponent<CanvasGroup>().ignoreParentGroups=true;
   var scaler=screen.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1600,900);scaler.matchWidthOrHeight=.5f;
   var hit=new GameObject("Ceremony input shield",typeof(RectTransform),typeof(Image)).GetComponent<Image>();hit.transform.SetParent(screen,false);CoveUI.Stretch(hit.rectTransform);hit.color=Color.clear;hit.raycastTarget=true;
   content=new GameObject("Awards safe area",typeof(RectTransform)).GetComponent<RectTransform>();content.SetParent(screen,false);LayoutAwards();
   var band=BasketballUIStyle.Plate(content,"Awards title",0,0,1100,124,new Color(.025f,.09f,.14f,.88f),28,0);var br=band.rectTransform;br.anchorMin=br.anchorMax=br.pivot=new Vector2(.5f,1);br.anchoredPosition=new Vector2(0,-58);
   bool crew=source.State.Match.mode==FishingMode.Crew,success=source.State.Match.outcome==FishingOutcome.Success;
   int highest=records.Max(p=>source.State.Match.cupFinished?p.cupPoints:p.score);var winners=records.Where(p=>(source.State.Match.cupFinished?p.cupPoints:p.score)==highest).ToArray();
   if(source.State.Match.cupFinished){int catchTotal=winners.Max(p=>p.cupScore);winners=winners.Where(p=>p.cupScore==catchTotal).ToArray();}
   string heading=crew?(success?"CREW CATCH COMPLETE!":"ONE CREW. NEXT ADVENTURE."):winners.Length>1?"SHARED HONOURS!":source.PlayerName(winners[0].owner).ToUpperInvariant()+" WINS!";
   title=BasketballUIStyle.Text(band.transform,heading,22,5,1056,62,43,true,TextAnchor.MiddleCenter,CoveUI.Cream);title.resizeTextForBestFit=true;title.resizeTextMinSize=40;title.resizeTextMaxSize=86;
   BasketballUIStyle.Text(band.transform,crew?(success?"EVERYONE CONTRIBUTED":"THANKS FOR FISHING TOGETHER"):source.State.Match.cupFinished?"LAGOON CUP RESULTS":"LAGOON ROUND RESULTS",22,76,1056,35,24,true,TextAnchor.MiddleCenter,CoveUI.Gold);
   var skip=BasketballUIStyle.Button(content,"Show fishing results","Show results",0,0,230,54,Skip,CoveUI.Cream,20);var sr=(RectTransform)skip.transform;sr.anchorMin=sr.anchorMax=sr.pivot=new Vector2(1,0);sr.anchoredPosition=new Vector2(-40,35);sr.localScale=Vector3.one*2;
   curtain=new GameObject("Awards dissolve",typeof(RectTransform),typeof(Image)).GetComponent<Image>();curtain.transform.SetParent(screen,false);CoveUI.Stretch(curtain.rectTransform);curtain.raycastTarget=false;curtain.color=new Color(.025f,.07f,.11f,0);
   stage=new GameObject("Fishing honours - render-only cast").transform;stage.SetParent(source.transform,false);
   // The broad shore platform behind the local pier gives the full crew solid ground.
   var local=AppRoot.Instance.LocalAthlete;var radial=Vector3.ProjectOnPlane(local.transform.position-source.transform.position,Vector3.up).normalized;if(radial.sqrMagnitude<.5f)radial=Vector3.back;
   var spot=source.transform.position+radial*29;
   if(Physics.Raycast(spot+Vector3.up*15,Vector3.down,out var floor,30,1<<8,QueryTriggerInteraction.Ignore))spot.y=floor.point.y+.015f;else spot=local.transform.position;
   // Face the open water, keeping the awards camera away from shore cliffs/trees.
   stage.SetPositionAndRotation(spot,Quaternion.LookRotation(-radial));takes=BasketballCelebrationPoses.Defaults();
   var ordered=records.OrderByDescending(p=>winners.Any(w=>w.owner==p.owner)).ThenBy(p=>p.owner).ToArray();
   foreach(var record in ordered){var actor=Athlete.Active.FirstOrDefault(a=>a&&a.visual&&FishingGame.Key(a)==record.owner);if(!actor)continue;var net=actor.GetComponent<NetworkAthlete>();var visual=new FootballCelebrationActor(actor,new FootballPlayerRecord{id=record.owner,name=source.PlayerName(record.owner),number=(byte)(net&&net.IsSpawned?net.RoomSeat.Value+1:1),team=FootballTeam.None},stage);if(!visual.Ready){Destroy(visual.Root.gameObject);continue;}int i=cast.Count;visual.Root.localPosition=new Vector3(i==0?0:((i+1)/2)*(i%2==1?-1.7f:1.7f),0,i==0?0:.5f);visual.Root.gameObject.SetActive(false);cast.Add(visual);}
   if(cast.Count==0){End(false);return false;}
   revealed=false;opened=Time.unscaledTime;skipAt=-1;return true;
   }catch(Exception error){Debug.LogWarning("Fishing awards unavailable: "+error);End(false);return false;}
  }
  public void Skip(){if(running&&skipAt<0)skipAt=Time.unscaledTime;}
  void LayoutAwards(){if(!content)return;var safe=Screen.safeArea;content.anchorMin=new Vector2(safe.x/Mathf.Max(1,Screen.width),safe.y/Mathf.Max(1,Screen.height));content.anchorMax=new Vector2(safe.xMax/Mathf.Max(1,Screen.width),safe.yMax/Mathf.Max(1,Screen.height));content.offsetMin=content.offsetMax=Vector2.zero;}
  void LateUpdate(){
   if(!running)return;LayoutAwards();if(!game||!game.Context||game.State.Match.round!=round||game.State.Match.phase!=FishingRoundPhase.Ended||TeleportSequence.Busy||RoomTeleport.Busy){End(false);return;}
   float age=Time.unscaledTime-opened,duration=MenuPreferences.ReducedMotion?3.5f:9;
   if(skipAt<0&&age>=duration-.55f)skipAt=Time.unscaledTime;
   float fadeIn=Mathf.SmoothStep(0,1,age/.45f),fadeOut=skipAt<0?0:Mathf.SmoothStep(0,1,(Time.unscaledTime-skipAt)/.4f);
   if(age>=.45f&&!revealed){revealed=true;foreach(var actor in Athlete.Active)if(actor&&actor.visual)foreach(var renderer in actor.visual.GetComponentsInChildren<Renderer>(true))if(!hidden.ContainsKey(renderer)){hidden[renderer]=renderer.forceRenderingOff;renderer.forceRenderingOff=true;}}
   foreach(var pair in hidden)if(pair.Key)pair.Key.forceRenderingOff=true;
   curtain.color=new Color(.025f,.07f,.11f,Mathf.Max(age<.45f?fadeIn:1-Mathf.SmoothStep(0,1,(age-.45f)/.45f),fadeOut));
   if(revealed){
    for(int i=0;i<cast.Count;i++){cast[i].Root.gameObject.SetActive(true);var take=takes[i%3];cast[i].Pose(take.Sample(Mathf.Max(0,age-.45f+i*.45f)/take.duration));}
    float turn=MenuPreferences.ReducedMotion?0:Mathf.Sin(Mathf.Clamp01((age-.45f)/7)*Mathf.PI)*.11f;float distance=cast.Count>3?10:8;
    var position=stage.TransformPoint(new Vector3(turn*distance,2.9f,distance));var focus=stage.TransformPoint(new Vector3(0,1.1f,0));camera.transform.SetPositionAndRotation(position,Quaternion.LookRotation(focus-position));camera.fieldOfView=45;camera.nearClipPlane=.12f;
   }
   if(skipAt>=0&&Time.unscaledTime-skipAt>=.45f)End(true);
  }
  public void Cancel()=>End(false);
  void End(bool showResults){
   if(!running)return;running=false;foreach(var pair in hidden)if(pair.Key)pair.Key.forceRenderingOff=pair.Value;hidden.Clear();cast.Clear();
   if(stage){stage.gameObject.SetActive(false);Destroy(stage.gameObject);}if(screen){screen.gameObject.SetActive(false);Destroy(screen.gameObject);}
   if(camera){camera.transform.SetPositionAndRotation(savedPosition,savedRotation);camera.fieldOfView=savedFov;camera.nearClipPlane=savedNear;}
   if(hud){hud.alpha=hudAlpha;hud.blocksRaycasts=hudRaycasts;}if(PlayerView.Instance){PlayerView.Instance.ClearMatchInput();PlayerView.Instance.InputBlocked=blockedBefore;}
   var done=complete;complete=null;if(Instance==this)Instance=null;if(showResults)done?.Invoke();
  }
  void OnDisable()=>End(false);void OnDestroy()=>End(false);
 }
 // Spectators share the same awards camera without creating a player score record.
 public sealed class FishingSpectatorAwards:MonoBehaviour {
  uint shown;bool observed;FishingCeremony ceremony;
  void LateUpdate(){
   var game=FishingGame.Instance;var app=AppRoot.Instance;if(!game||!game.Context||!app||!app.LocalAthlete||!app.LocalAthlete.IsSpectator){ceremony?.Cancel();return;}
   var match=game.State.Match;if(match.phase!=FishingRoundPhase.Ended||match.outcome==FishingOutcome.Abandoned||game.State.Players.Count==0||observed&&shown==match.round)return;
   if(TeleportSequence.Busy||RoomTeleport.Busy)return;if(!ceremony)ceremony=gameObject.AddComponent<FishingCeremony>();
   if(ceremony.Begin(game,game.State.Players.ToArray(),null)){shown=match.round;observed=true;}
  }
  void OnDisable()=>ceremony?.Cancel();
 }
}
