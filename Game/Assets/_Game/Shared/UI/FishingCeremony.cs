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
  Transform stage;RectTransform screen,content;Image curtain;Text title;FootballTake[] takes;Func<bool> valid;Action complete;
  readonly List<(CanvasGroup group,float alpha,bool raycasts,bool interactable)> hiddenUI=new();bool revealed;
  readonly List<RectTransform> rankBadges=new();readonly List<Transform> rankHeads=new();bool rankedGolf;
  public int CastCount=>cast.Count;
  public bool Begin(FishingGame source,FishingPlayerRecord[] records,Action onComplete){
   if(running||Presenting||!source||records.Length==0||source.State.Match.outcome==FishingOutcome.Abandoned||TeleportSequence.Busy||RoomTeleport.Busy)return false;
   bool crew=source.State.Match.mode==FishingMode.Crew,success=source.State.Match.outcome==FishingOutcome.Success;
   int highest=records.Max(p=>source.State.Match.cupFinished?p.cupPoints:p.score);var winners=records.Where(p=>(source.State.Match.cupFinished?p.cupPoints:p.score)==highest).ToArray();
   if(source.State.Match.cupFinished){int catchTotal=winners.Max(p=>p.cupScore);winners=winners.Where(p=>p.cupScore==catchTotal).ToArray();}
   string heading=crew?(success?"CREW CATCH COMPLETE!":"ONE CREW. NEXT ADVENTURE."):winners.Length>1?"SHARED HONOURS!":source.PlayerName(winners[0].owner).ToUpperInvariant()+" WINS!";
   uint round=source.State.Match.round;
   var ordered=records.OrderByDescending(p=>winners.Any(w=>w.owner==p.owner)).ThenBy(p=>p.owner).Select(p=>(p.owner,source.PlayerName(p.owner))).ToArray();
   var local=AppRoot.Instance.LocalAthlete;var radial=Vector3.ProjectOnPlane(local.transform.position-source.transform.position,Vector3.up).normalized;if(radial.sqrMagnitude<.5f)radial=Vector3.back;
   return BeginAwards(source.transform,source.transform.position+radial*29,Quaternion.LookRotation(-radial),ordered,heading,"LAGOON ROUND RESULTS",()=>source&&source.Context&&source.State.Match.round==round&&source.State.Match.phase==FishingRoundPhase.Ended,onComplete);
  }
  public bool BeginGolf(GolfMatchManager source,Action onComplete){
   if(!source||source.State.Phase!=GolfMatchPhase.Ended)return false;
   var results=source.State.Results().Where(r=>r.player.IsFinished&&!r.player.IsDNF).Take(3).ToArray();
   if(results.Length==0){onComplete?.Invoke();return true;}
   var winners=results.Where(r=>r.rank==1).ToArray();string heading=winners.Length>1?"SHARED HONOURS!":winners.Length==1?winners[0].player.PlayerName.ToUpperInvariant()+" WINS!":"ROUND COMPLETE!";
   var ordered=results.Select(r=>(r.player.PlayerId,r.player.PlayerName)).ToArray();uint round=source.Round;
   var tee=source.Course.holes.First(h=>h.number==1).tee;
   return BeginAwards(source.transform,tee.position+tee.right*4,Quaternion.Euler(0,tee.eulerAngles.y+180,0),ordered,heading,"ISLAND GOLF RESULTS",()=>source&&source.Context&&source.Round==round&&source.State.Phase==GolfMatchPhase.Ended,onComplete,results.ToDictionary(r=>r.player.PlayerId,r=>r.rank));
  }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
  // Render-only preview: no synthetic scores, match completion or network writes.
  public bool BeginGolfPreview(GolfMatchManager source){
   if(!source||!source.Context||!AppRoot.Instance.LocalAthlete)return false;
   var tee=source.Course.holes.First(h=>h.number==1).tee;
   var records=new (ulong id,string name)[]{(1,"Winner preview"),(2,"Runner-up preview"),(3,"Third-place preview")};
   return BeginAwards(source.transform,tee.position+tee.right*4,Quaternion.Euler(0,tee.eulerAngles.y+180,0),records,"GOLF PODIUM PREVIEW","TOP THREE FINISHERS",()=>source&&source.Context,null,new Dictionary<ulong,int>{{1,1},{2,2},{3,3}},AppRoot.Instance.LocalAthlete);
  }
#endif
  bool BeginAwards(Transform source,Vector3 spot,Quaternion facing,(ulong id,string name)[] records,string heading,string subtitle,Func<bool> stillValid,Action onComplete,IReadOnlyDictionary<ulong,int> golfRanks=null,Athlete previewActor=null){
   if(running||Presenting||records.Length==0||TeleportSequence.Busy||RoomTeleport.Busy)return false;
   camera=Camera.main;if(!camera||!AppRoot.Instance.LocalAthlete)return false;
   rankedGolf=golfRanks!=null;rankBadges.Clear();rankHeads.Clear();valid=stillValid;complete=onComplete;Instance=this;PostMatchFlow.Instance?.Close();
   savedPosition=camera.transform.position;savedRotation=camera.transform.rotation;savedFov=camera.fieldOfView;savedNear=camera.nearClipPlane;
   running=true;
   try {
   var view=PlayerView.Instance;blockedBefore=view&&view.ExplicitInputBlocked;if(view){view.ClearMatchInput();view.InputBlocked=true;}
   // Responsive widgets can live beside their HUD, and modal groups may ignore
   // parents. Cover the active UI roots and their groups, then restore exactly.
   foreach(var uiCanvas in FindObjectsByType<Canvas>(FindObjectsSortMode.None).Where(c=>c.isRootCanvas)){
    if(!uiCanvas.GetComponent<CanvasGroup>())uiCanvas.gameObject.AddComponent<CanvasGroup>();
    foreach(var group in uiCanvas.GetComponentsInChildren<CanvasGroup>(true)){hiddenUI.Add((group,group.alpha,group.blocksRaycasts,group.interactable));group.alpha=0;group.blocksRaycasts=group.interactable=false;}
   }
   screen=new GameObject("Island awards overlay",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster),typeof(CanvasGroup)).GetComponent<RectTransform>();
   var canvas=screen.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.overrideSorting=true;canvas.sortingOrder=950;screen.GetComponent<CanvasGroup>().ignoreParentGroups=true;
   var scaler=screen.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1600,900);scaler.matchWidthOrHeight=.5f;
   var hit=new GameObject("Ceremony input shield",typeof(RectTransform),typeof(Image)).GetComponent<Image>();hit.transform.SetParent(screen,false);CoveUI.Stretch(hit.rectTransform);hit.color=Color.clear;hit.raycastTarget=true;
   content=new GameObject("Awards safe area",typeof(RectTransform)).GetComponent<RectTransform>();content.SetParent(screen,false);LayoutAwards();
   var band=BasketballUIStyle.Plate(content,"Awards title",0,0,1100,124,new Color(.025f,.09f,.14f,.88f),28,0);var br=band.rectTransform;br.anchorMin=br.anchorMax=br.pivot=new Vector2(.5f,1);br.anchoredPosition=new Vector2(0,-58);
   title=BasketballUIStyle.Text(band.transform,heading,22,5,1056,62,43,true,TextAnchor.MiddleCenter,CoveUI.Cream);title.resizeTextForBestFit=true;title.resizeTextMinSize=40;title.resizeTextMaxSize=86;
   BasketballUIStyle.Text(band.transform,subtitle,22,76,1056,35,24,true,TextAnchor.MiddleCenter,CoveUI.Gold);
   var skip=BasketballUIStyle.Button(content,"Show island results","Show results",0,0,230,54,Skip,CoveUI.Cream,20);var sr=(RectTransform)skip.transform;sr.anchorMin=sr.anchorMax=sr.pivot=new Vector2(1,0);sr.anchoredPosition=new Vector2(-40,35);sr.localScale=Vector3.one*2;
   curtain=new GameObject("Awards dissolve",typeof(RectTransform),typeof(Image)).GetComponent<Image>();curtain.transform.SetParent(screen,false);CoveUI.Stretch(curtain.rectTransform);curtain.raycastTarget=false;curtain.color=new Color(.025f,.07f,.11f,0);
   stage=new GameObject("Island honours - render-only cast").transform;stage.SetParent(source,false);
   if(Physics.Raycast(spot+Vector3.up*15,Vector3.down,out var floor,30,1<<8,QueryTriggerInteraction.Ignore))spot.y=floor.point.y+.015f;else spot=AppRoot.Instance.LocalAthlete.transform.position;
   stage.SetPositionAndRotation(spot,facing);takes=BasketballCelebrationPoses.Defaults();
   foreach(var record in records){
    var actor=previewActor?previewActor:Athlete.Active.FirstOrDefault(a=>a&&a.visual&&(AppRoot.Instance.SelectedSport==SportId.Golf?GolfCartWorld.Key(a):FishingGame.Key(a))==record.id);if(!actor)continue;
    var net=actor.GetComponent<NetworkAthlete>();var visual=new FootballCelebrationActor(actor,new FootballPlayerRecord{id=record.id,name=record.name,number=(byte)(net&&net.IsSpawned?net.RoomSeat.Value+1:CharacterIdentity.Ensure(actor).Number),team=FootballTeam.None},stage);
    if(!visual.Ready){Destroy(visual.Root.gameObject);continue;}int i=cast.Count;float side=i%2==1?-1:1;
    // The awards camera looks back along stage Z: positive local X is screen-left.
    if(rankedGolf)side=-side;
    visual.Root.localPosition=new Vector3(i==0?0:((i+1)/2)*side*1.7f,0,i==0?0:.5f);visual.Root.gameObject.SetActive(false);cast.Add(visual);
    if(rankedGolf){rankBadges.Add(RankBadge(golfRanks[record.id]));rankHeads.Add(visual.Root.GetComponentsInChildren<Transform>().FirstOrDefault(t=>t.name=="mixamorig:Head"));}
   }
   if(cast.Count==0){End(false);return false;}
   revealed=false;opened=Time.unscaledTime;skipAt=-1;return true;
   }catch(Exception error){Debug.LogWarning("Island awards unavailable: "+error);End(false);return false;}
  }
  public void Skip(){if(running&&skipAt<0)skipAt=Time.unscaledTime;}
  RectTransform RankBadge(int rank){
   var badge=new GameObject("Golf place "+rank,typeof(RectTransform)).GetComponent<RectTransform>();badge.SetParent(content,false);
   badge.anchorMin=badge.anchorMax=new Vector2(.5f,.5f);badge.pivot=new Vector2(.5f,0);badge.sizeDelta=new Vector2(204,114);
   var medal=new GameObject("Original rank medal",typeof(RectTransform),typeof(Image)).GetComponent<Image>();medal.transform.SetParent(badge,false);
   medal.sprite=Resources.Load<Sprite>("GolfUI/Awards/Place"+rank);medal.color=Color.white;medal.preserveAspect=true;medal.raycastTarget=false;
   medal.rectTransform.anchorMin=medal.rectTransform.anchorMax=new Vector2(.5f,1);medal.rectTransform.pivot=new Vector2(.5f,1);medal.rectTransform.anchoredPosition=Vector2.zero;medal.rectTransform.sizeDelta=new Vector2(108,114);
   badge.gameObject.SetActive(false);return badge;
  }
  void PositionRanks(){
   float scale=Mathf.Min(1,Mathf.Max(.5f,(content.rect.width-80)/Mathf.Max(1,cast.Count+2)/205));
   for(int i=0;i<rankBadges.Count;i++){
    // Keep the medal just above the animated head, rather than a fixed tall
    // offset above the feet. No extra rank/name caption sits below the art.
    var anchor=rankHeads[i]?rankHeads[i].position+Vector3.up*.66f:cast[i].Root.position+Vector3.up*1.94f;
    var point=camera.WorldToScreenPoint(anchor);
    var badge=rankBadges[i];bool visible=point.z>0;badge.gameObject.SetActive(visible);if(!visible)continue;
    RectTransformUtility.ScreenPointToLocalPointInRectangle(content,point,null,out var at);
    badge.anchoredPosition=at;badge.localScale=Vector3.one*scale;
   }
  }
  void LayoutAwards(){if(!content)return;var safe=Screen.safeArea;content.anchorMin=new Vector2(safe.x/Mathf.Max(1,Screen.width),safe.y/Mathf.Max(1,Screen.height));content.anchorMax=new Vector2(safe.xMax/Mathf.Max(1,Screen.width),safe.yMax/Mathf.Max(1,Screen.height));content.offsetMin=content.offsetMax=Vector2.zero;}
  void LateUpdate(){
   if(!running)return;LayoutAwards();if(valid==null||!valid()||TeleportSequence.Busy||RoomTeleport.Busy){End(false);return;}
   float age=Time.unscaledTime-opened,duration=MenuPreferences.ReducedMotion?3.5f:12;
   if(skipAt<0&&age>=duration-.55f)skipAt=Time.unscaledTime;
   float fadeIn=Mathf.SmoothStep(0,1,age/.45f),fadeOut=skipAt<0?0:Mathf.SmoothStep(0,1,(Time.unscaledTime-skipAt)/.4f);
   foreach(var entry in hiddenUI)if(entry.group){entry.group.alpha=0;entry.group.blocksRaycasts=entry.group.interactable=false;}
   if(age>=.45f&&!revealed){revealed=true;foreach(var actor in Athlete.Active)if(actor)foreach(var renderer in actor.GetComponentsInChildren<Renderer>(true))if(!hidden.ContainsKey(renderer)){hidden[renderer]=renderer.forceRenderingOff;renderer.forceRenderingOff=true;}}
   foreach(var pair in hidden)if(pair.Key)pair.Key.forceRenderingOff=true;
   curtain.color=new Color(.025f,.07f,.11f,Mathf.Max(age<.45f?fadeIn:1-Mathf.SmoothStep(0,1,(age-.45f)/.45f),fadeOut));
   if(revealed){
    bool singleWinner=title.text.EndsWith(" WINS!");
    for(int i=0;i<cast.Count;i++){cast[i].Root.gameObject.SetActive(true);var take=takes[i%3];var pose=take.Sample(Mathf.Max(0,age-.45f+i*.45f)/take.duration);if(singleWinner&&i==0&&age>7){var solo=takes[3];pose=FootballPose.Lerp(pose,solo.Sample((age-7)/solo.duration),Mathf.SmoothStep(0,1,(age-7)/1.5f));}cast[i].Pose(pose);}
    float turn=MenuPreferences.ReducedMotion?0:Mathf.Sin(Mathf.Clamp01((age-.45f)/9)*Mathf.PI)*.16f;float spotlight=MenuPreferences.ReducedMotion||!singleWinner?0:Mathf.SmoothStep(0,1,(age-7)/3);float distance=Mathf.Lerp(cast.Count>3?10:8,4.8f,spotlight);
    if(rankedGolf){spotlight=cast.Count==1?spotlight:0;float extent=(cast.Count/2)*1.7f+1.7f;distance=Mathf.Max(8,extent/(Mathf.Tan(22.5f*Mathf.Deg2Rad)*Mathf.Max(.35f,camera.aspect)));}
    var position=stage.TransformPoint(new Vector3(turn*distance,2.9f,distance));var focus=stage.TransformPoint(new Vector3(0,1.1f,0));camera.transform.SetPositionAndRotation(position,Quaternion.LookRotation(focus-position));camera.fieldOfView=Mathf.Lerp(45,36,spotlight);camera.nearClipPlane=.12f;
    if(rankedGolf)PositionRanks();
   }
   if(skipAt>=0&&Time.unscaledTime-skipAt>=.45f)End(true);
  }
  public void Cancel()=>End(false);
  void End(bool showResults){
   if(!running)return;running=false;foreach(var pair in hidden)if(pair.Key)pair.Key.forceRenderingOff=pair.Value;hidden.Clear();cast.Clear();rankBadges.Clear();rankHeads.Clear();
   if(stage){stage.gameObject.SetActive(false);Destroy(stage.gameObject);}if(screen){screen.gameObject.SetActive(false);Destroy(screen.gameObject);}
   if(camera){camera.transform.SetPositionAndRotation(savedPosition,savedRotation);camera.fieldOfView=savedFov;camera.nearClipPlane=savedNear;}
   foreach(var entry in hiddenUI)if(entry.group){entry.group.alpha=entry.alpha;entry.group.blocksRaycasts=entry.raycasts;entry.group.interactable=entry.interactable;}hiddenUI.Clear();if(PlayerView.Instance){PlayerView.Instance.ClearMatchInput();PlayerView.Instance.InputBlocked=blockedBefore;}
   var done=complete;complete=null;valid=null;if(Instance==this)Instance=null;if(showResults)done?.Invoke();
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
