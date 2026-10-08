#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Unity.Netcode.Components;

namespace WhatTheFish {
 public sealed class SportsHudReview:MonoBehaviour {
  string folder,shared;bool failed,capture;AppRoot app;Athlete local;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Init(){var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,"-sportsHudReview");if(i<0||i+1>=args.Length)return;var r=new GameObject("Sports HUD review").AddComponent<SportsHudReview>();r.folder=args[i+1];r.capture=args.Contains("-hudCapture");}
  void Check(bool ok,string text){failed|=!ok;File.AppendAllText(Path.Combine(folder,"results.txt"),(ok?"PASS ":"FAIL ")+text+"\n");}
  void Place(Athlete actor,Vector3 point){actor.capsule.enabled=false;actor.transform.SetPositionAndRotation(point,Quaternion.identity);actor.ResetLocomotion();actor.capsule.enabled=true;var net=actor.GetComponent<NetworkTransform>();if(net&&net.IsSpawned)net.Teleport(point,Quaternion.identity,Vector3.one);Physics.SyncTransforms();}
  void Signal(string name){var p=Path.Combine(shared,name);File.WriteAllText(p+".tmp","ready");File.Move(p+".tmp",p);}
  IEnumerator Await(string name){float end=Time.realtimeSinceStartup+30;while(!File.Exists(Path.Combine(shared,name))&&Time.realtimeSinceStartup<end)yield return null;Check(File.Exists(Path.Combine(shared,name)),"checkpoint "+name);}
  IEnumerator Start(){
   Directory.CreateDirectory(folder);shared=Directory.GetParent(folder).FullName;File.WriteAllText(Path.Combine(folder,"results.txt"),"");DevelopmentProbe.TurnCommandActive=true;DevelopmentProbe.TurnCommand=default;
   float end=Time.realtimeSinceStartup+80;while((!(app=AppRoot.Instance)||!app.Exploring||!app.LocalAthlete)&&Time.realtimeSinceStartup<end)yield return null;
   local=app?app.LocalAthlete:null;if(!local){Check(false,"game starts");Finish();yield break;}yield return new WaitForSeconds(1);
   if(app.rooms.Connected){yield return Network();Finish();yield break;}
   yield return Football();yield return Basketball();
   foreach(var sport in new[]{SportId.Golf,SportId.Fishing}){yield return Enter(sport);StyleChecks(sport);yield return FeedbackChecks();yield return Capture(sport.ToString());}
   Finish();
  }
  IEnumerator Enter(SportId sport){app.view.ClearMatchInput();app.SelectSport(sport);while(app.PendingSelection.HasValue)yield return null;app.EnterOffline();yield return new WaitForSeconds(1);local=app.LocalAthlete;app.view.mode=sport==SportId.Football||sport==SportId.Basketball?2:1;yield return new WaitForSeconds(.6f);}
  Athlete Partner(Vector3 point,FootballTeam team=FootballTeam.None){var actor=Instantiate(app.athletePrefab).GetComponent<Athlete>();actor.Setup();actor.BasketballPracticeTeam=team;Place(actor,point);return actor;}
  void StyleChecks(SportId sport){
   var buttons=FindObjectsByType<Button>(FindObjectsInactive.Include,FindObjectsSortMode.None).Where(b=>b.GetComponent<GameButtonStyle>()).ToArray();
   Check(buttons.Length>=3,sport+" game controls have Cove skin");
   Check(buttons.All(b=>b.targetGraphic is CovePlate&&b.GetComponent<CoveFeedback>()&&b.transition==Selectable.Transition.None),sport+" plates use interactive feedback");
   Check(buttons.All(b=>b.GetComponent<Image>()==null||b.GetComponent<Image>().color.a==0&&b.GetComponent<Image>().raycastTarget),sport+" fixed transparent touch targets");
   Check(buttons.All(b=>!b.GetComponentInChildren<CoveIcon>(true)),sport+" buttons use text without icons");
   foreach(var b in buttons.Where(b=>((RectTransform)b.transform).rect.height>=120)){
    var r=(RectTransform)b.transform;var corners=new Vector3[4];r.GetWorldCorners(corners);Check(Mathf.Abs(r.rect.width-r.rect.height)<1&&r.rect.width>=132&&corners.All(c=>c.x>=0&&c.x<=Screen.width&&c.y>=0&&c.y<=Screen.height),sport+" round thumb target stays in bounds: "+b.name);
   }
   var visible=buttons.Where(b=>b.IsActive()&&b.GetComponentsInParent<CanvasGroup>().All(g=>g.alpha>.05f)).ToArray();
   Rect Bounds(Button b){var c=new Vector3[4];((RectTransform)b.transform).GetWorldCorners(c);return new Rect(c[0],c[2]-c[0]);}
   for(int i=0;i<visible.Length;i++)for(int j=i+1;j<visible.Length;j++)Check(!Bounds(visible[i]).Overlaps(Bounds(visible[j])),sport+" controls do not overlap: "+visible[i].name+" / "+visible[j].name);
   var button=buttons.First(b=>b.IsActive()&&b.IsInteractable());var feedback=button.GetComponent<CoveFeedback>();var pointer=new PointerEventData(EventSystem.current){pointerId=71,button=PointerEventData.InputButton.Left};
   feedback.OnPointerDown(pointer);feedback.OnPointerUp(new PointerEventData(EventSystem.current){pointerId=72});
   var face=(CovePlate)button.targetGraphic;Check(!face.raycastTarget&&face.shadow&&face.border>0,sport+" visual does not resize touch target");feedback.OnPointerUp(pointer);
  }
  void MapChecks(int players,SportId sport){
   Canvas.ForceUpdateCanvases();var map=FindFirstObjectByType<SportsMiniMap>();Check(map&&map.PlayerCount==players,sport+" map includes every active player");
   Check(map&&!map.raycastTarget&&map.rectTransform.anchorMin==new Vector2(.5f,0),sport+" map is bottom-centred and touch-through");
   Check(map.rectTransform.rect.width<=210&&map.rectTransform.rect.height<=120&&!map.GetComponentInChildren<Text>(),sport+" map is a small board without captions or legend");
   var p=map.MapPoint(local.transform.position);Check(Mathf.Abs(p.x)<=140&&Mathf.Abs(p.y)<=69,sport+" position maps inside field");
   var corners=new Vector3[4];map.rectTransform.GetWorldCorners(corners);Check(corners.All(c=>c.x>=0&&c.x<=Screen.width&&c.y>=0&&c.y<=Screen.height),sport+" map fits screen "+Screen.width+"x"+Screen.height);
  }
  IEnumerator FeedbackChecks(){
   var button=FindObjectsByType<Button>(FindObjectsSortMode.None).First(b=>b.IsActive()&&b.IsInteractable()&&b.GetComponent<GameButtonStyle>());
   var feedback=button.GetComponent<CoveFeedback>();var visual=button.transform.Find("Button visual");var rect=(RectTransform)button.transform;
   var corners=new Vector3[4];rect.GetWorldCorners(corners);var pointer=new PointerEventData(EventSystem.current){pointerId=81,button=PointerEventData.InputButton.Left};
   feedback.OnPointerDown(pointer);yield return new WaitForSeconds(.16f);
   Check(MenuPreferences.ReducedMotion||visual.localScale.x<.95f,"held button visibly compresses");
   feedback.OnPointerUp(new PointerEventData(EventSystem.current){pointerId=82});yield return new WaitForSeconds(.08f);
   Check(((CovePlate)button.targetGraphic).pressure==1,"another finger cannot release press feedback");
   var after=new Vector3[4];rect.GetWorldCorners(after);Check(corners.Zip(after,(a,b)=>Vector3.Distance(a,b)).All(d=>d<.01f),"press animation preserves thumb hit area");
   feedback.OnPointerUp(pointer);yield return new WaitForSeconds(.4f);
   Check(Mathf.Abs(visual.localScale.x-1)<.005f&&((CovePlate)button.targetGraphic).pressure==0,"release returns visual to rest");
  }
  void FootballPickup(Athlete actor){var ball=FootballBall.Instance;ball.ResetBall();ball.Body.position=ball.FootPosition(actor);ball.Body.linearVelocity=Vector3.zero;Physics.SyncTransforms();ball.RefreshControl(actor);Check(ball.CurrentController==actor,"football possession fixture");}
  IEnumerator Football(){
   yield return Enter(SportId.Football);var ball=FootballBall.Instance;var home=ball.KickoffPosition-Vector3.up*ball.WorldRadius+Vector3.back*5;Place(local,home);var rival=Partner(home+Vector3.forward*3);
   ball.ResetBall();ball.Body.position=home+Vector3.right*10;yield return new WaitForSeconds(.25f);
   var slot=FindFirstObjectByType<FootballActionSlot>();var kick=slot.GetComponent<KickButton>();var pressure=slot.GetComponent<FootballEffortButton>();var tackle=FindFirstObjectByType<TackleButton>();
   Check(!slot.Attacking&&!kick.enabled&&pressure.enabled&&tackle.enabled,"football loose ball defaults to defense");yield return Capture("Football-defense");
   FootballPickup(local);yield return new WaitForSeconds(.25f);
   Check(slot.Attacking&&kick.enabled&&!pressure.enabled&&!tackle.enabled,"football own possession swaps same slot to attack");MapChecks(2,SportId.Football);StyleChecks(SportId.Football);yield return FeedbackChecks();yield return Capture("Football-attack");
   Check(kick.label.text.StartsWith("Kick")&&kick.button.interactable,"new football role updates label and eligibility on the same frame");
   var pointer=new PointerEventData(EventSystem.current){pointerId=44,button=PointerEventData.InputButton.Left};kick.OnPointerDown(pointer);Check(app.view.Charging,"touch begins kick charge");
   FootballPickup(rival);yield return new WaitForSeconds(.2f);Check(!app.view.Charging&&!kick.enabled&&pressure.enabled,"turnover cancels captured kick before pressure slot activates");
   ExecuteEvents.Execute(slot.gameObject,pointer,ExecuteEvents.pointerUpHandler);Check(!app.view.ReadCommand().kick,"old finger release cannot kick after turnover");
   Check(!SportsPossession.Attacking(local,SportId.Football),"opponent possession is defense");Destroy(rival.gameObject);yield return null;Place(local,SkySailMap.Port(SportId.Football));yield return new WaitForSeconds(.5f);yield return Capture("Football-station");
  }
  IEnumerator BasketballPickup(Athlete actor){
   var ball=BasketballBall.Active;app.view.ClearMatchInput();ball.ResetHome();ball.autoPickup=true;ball.Place(ball.transform.parent.InverseTransformPoint(actor.transform.position+Vector3.up*.25f+Vector3.forward*.4f),Quaternion.identity,Vector3.zero,Vector3.zero);
   float end=Time.time+3;while(ball.Holder!=actor&&Time.time<end)yield return null;Check(ball.Holder==actor,"basketball possession fixture");yield return new WaitForSeconds(.25f);
  }
  void BasketballSlots(bool attacking,string name){
   var attacks=FindObjectsByType<BasketballShootButton>(FindObjectsSortMode.None);var defenses=FindObjectsByType<BasketballDefenseButton>(FindObjectsSortMode.None);var steal=FindFirstObjectByType<BasketballStealButton>();
   Check(attacks.Length==2&&attacks.All(b=>b.enabled==attacking)&&defenses.All(b=>b.enabled!=attacking),name+" switches shoot/pass and block/guard in place");
   Check(steal.GetComponent<CanvasGroup>().alpha==(attacking?0:1),name+" shows steal only on defense");
  }
  IEnumerator Basketball(){
   yield return Enter(SportId.Basketball);var ball=BasketballBall.Active;var arena=ball.transform.parent;Place(local,arena.TransformPoint(new Vector3(0,.07f,-3)));local.BasketballPracticeTeam=FootballTeam.A;
   var teammate=Partner(arena.TransformPoint(new Vector3(3,.07f,0)),FootballTeam.A);var rival=Partner(arena.TransformPoint(new Vector3(-3,.07f,0)),FootballTeam.B);
   ball.autoPickup=false;ball.ResetHome();yield return new WaitForSeconds(.3f);BasketballSlots(false,"loose ball");MapChecks(3,SportId.Basketball);StyleChecks(SportId.Basketball);yield return FeedbackChecks();yield return Capture("Basketball-defense");
   Check(SportsPossession.SameTeam(local,teammate,SportId.Basketball)&&!SportsPossession.SameTeam(local,rival,SportId.Basketball),"map colours share gameplay squad relation");
   yield return BasketballPickup(local);BasketballSlots(true,"own possession");yield return Capture("Basketball-attack");
   var shoot=FindObjectsByType<BasketballShootButton>(FindObjectsSortMode.None).First(b=>!b.pass);var pointer=new PointerEventData(EventSystem.current){pointerId=45,button=PointerEventData.InputButton.Left};shoot.OnPointerDown(pointer);Check(app.view.ShotCharging,"touch begins basketball shot");
   ball.autoPickup=false;ball.ResetHome();yield return new WaitForSeconds(.25f);BasketballSlots(false,"lost possession");Check(!app.view.ShotCharging&&!app.view.PassCharging,"turnover clears shot/pass gestures");
   ExecuteEvents.Execute(shoot.gameObject,pointer,ExecuteEvents.pointerUpHandler);Check(!app.view.ReadCommand().shoot,"old touch cannot shoot or reinterpret as block after turnover");
   Check(!ball.CanSteal(local),"loose ball does not allow stealing air");
   for(ulong id=0;id<10;id++)Check(SportsPossession.BasketballTeam(id)==(id%2==0?FootballTeam.A:FootballTeam.B),"stable alternating basketball squad for join "+id);
   Destroy(teammate.gameObject);Destroy(rival.gameObject);local.BasketballPracticeTeam=FootballTeam.None;yield return null;
  }
  IEnumerator Network(){
   var net=local.GetComponent<NetworkAthlete>();var id=net.OwnerClientId;
   if(app.rooms.Host){
    var players=Athlete.Active.Where(a=>a.GetComponent<NetworkAthlete>()&&a.GetComponent<NetworkAthlete>().IsSpawned).OrderBy(a=>a.GetComponent<NetworkAthlete>().OwnerClientId).ToArray();Check(players.Length==3,"three peers share basketball court");var ball=BasketballBall.Active;
    for(int i=0;i<players.Length;i++)Place(players[i],ball.transform.parent.TransformPoint(new Vector3((i-1)*3,.07f,-3)));
    for(int stage=0;stage<3;stage++){
     if(stage<2)yield return BasketballPickup(players[stage]);else{ball.autoPickup=false;ball.ResetHome();yield return new WaitForSeconds(.3f);}
     Signal("stage-"+stage);yield return new WaitForSeconds(.4f);BasketballSlots(stage==0,"host stage "+stage);MapChecks(3,SportId.Basketball);
     yield return Await("guest-1-"+stage);yield return Await("guest-2-"+stage);
    }
   }else {
    for(int stage=0;stage<3;stage++){yield return Await("stage-"+stage);yield return new WaitForSeconds(.5f);bool attacking=stage<2&&id%2==(ulong)stage;BasketballSlots(attacking,"guest "+id+" stage "+stage);if(stage<2&&id!=(ulong)stage)Check(!attacking||!BasketballBall.Active.CanShoot(local)&&!BasketballBall.Active.CanSteal(local)&&!BasketballBall.Active.CanBlock(local),"teammate cannot act on another player ball");MapChecks(3,SportId.Basketball);yield return Capture("Basketball-peer-"+id+"-stage-"+stage);Signal("guest-"+id+"-"+stage);}
   }
  }
  IEnumerator Capture(string name){if(!capture)yield break;yield return new WaitForEndOfFrame();SaveCapture(Path.Combine(folder,name+".png"));Check(File.Exists(Path.Combine(folder,name+".png")),"rendered "+name);}
  void SaveCapture(string path){
   var cam=Camera.main;var target=new RenderTexture(Screen.width,Screen.height,24);var previous=RenderTexture.active;var cameraTarget=cam.targetTexture;
   var overlays=FindObjectsByType<Canvas>(FindObjectsSortMode.None).Where(c=>c.renderMode==RenderMode.ScreenSpaceOverlay).ToArray();var cameras=overlays.Select(c=>c.worldCamera).ToArray();var distances=overlays.Select(c=>c.planeDistance).ToArray();
   try{cam.targetTexture=target;for(int i=0;i<overlays.Length;i++){overlays[i].renderMode=RenderMode.ScreenSpaceCamera;overlays[i].worldCamera=cam;overlays[i].planeDistance=1;}Canvas.ForceUpdateCanvases();cam.Render();RenderTexture.active=target;var image=new Texture2D(Screen.width,Screen.height,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,Screen.width,Screen.height),0,0);image.Apply();File.WriteAllBytes(path,image.EncodeToPNG());Destroy(image);}
   finally{for(int i=0;i<overlays.Length;i++){overlays[i].renderMode=RenderMode.ScreenSpaceOverlay;overlays[i].worldCamera=cameras[i];overlays[i].planeDistance=distances[i];}cam.targetTexture=cameraTarget;RenderTexture.active=previous;target.Release();Destroy(target);Canvas.ForceUpdateCanvases();}
  }
  void Finish(){DevelopmentProbe.TurnCommand=default;File.AppendAllText(Path.Combine(folder,"results.txt"),"SPORTS_HUD_COMPLETE success="+!failed+"\n");}
 }
}
#endif
