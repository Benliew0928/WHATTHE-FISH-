using System.Linq;
using UnityEngine;
using UnityEngine.UI;
namespace WhatTheFish {
 public sealed class RoomTeleportStatus:MonoBehaviour {
  public static bool Visible {get;private set;}
  RectTransform strip,crew;Text title,detail,stages,leftCrew,rightCrew;Button cancel,expand,retry,choose,dismiss;bool expanded;uint dismissed;string dismissedLocal="";
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Init(){new GameObject("Room travel status",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster)).AddComponent<RoomTeleportStatus>();}
  void Awake(){WorldUI.Canvas(gameObject,1100);
   strip=CoveUI.Rect(transform,"Travel status",0,0,1080,156);WorldUI.Card(strip,"Travel surface",0,0,1080,156);
   title=WorldUI.Label(strip,"",24,12,748,36,29,true);detail=WorldUI.Label(strip,"",24,55,748,50,21,false,WorldUI.Muted);
   stages=WorldUI.Label(strip,"",24,117,600,26,19,true,CoveUI.Mint);
   cancel=WorldUI.Button(strip,"Cancel travel","Cancel travel",856,82,200,56,()=>RoomTeleport.Instance.Cancel(),size:20);
   expand=WorldUI.Button(strip,"Travel crew","Crew details",856,20,200,56,()=>expanded=!expanded,size:21);
   retry=WorldUI.Button(strip,"Retry travel","Try again",856,20,200,56,()=>{var s=RoomTeleport.Instance.State;RoomTeleport.Instance.Request(s.to,s.destination);},CoveUI.Mint,21);
   choose=WorldUI.Button(strip,"Choose another island","Choose island",630,82,210,56,()=>PostMatchFlow.Instance.Open(),size:19);
   dismiss=WorldUI.Button(strip,"Dismiss travel notice","Dismiss",856,82,200,56,()=>{dismissed=RoomTeleport.Instance.State.sequence;dismissedLocal=TeleportSequence.Instance?TeleportSequence.Instance.Error:"";},size:20);
   crew=CoveUI.Rect(strip,"Travel crew details",0,-316,1080,300);WorldUI.Card(crew,"Crew surface",0,0,1080,300);WorldUI.Label(crew,"EVERYONE TRAVELS TOGETHER",24,14,1032,30,20,true,WorldUI.Muted);
   leftCrew=WorldUI.Label(crew,"",24,55,508,230,19);rightCrew=WorldUI.Label(crew,"",548,55,508,230,19);
   strip.gameObject.SetActive(false);
  }
  void OnDestroy(){Visible=false;}
  void Update(){
   var room=RoomTeleport.Instance;var app=AppRoot.Instance;if(!room||!app)return;var s=room.State;bool online=app.rooms.Connected;
   bool busy=RoomTeleport.Busy||TeleportSequence.Busy;string localError=TeleportSequence.Instance?TeleportSequence.Instance.Error:"";
   bool failed=online?s.phase==RoomTeleportPhase.Cancelled&&s.sequence!=dismissed:localError.Length>0&&localError!=dismissedLocal;
   strip.gameObject.SetActive(app.Exploring&&!RoomService.Paused&&(busy||failed)&&!(PostMatchFlow.Instance&&PostMatchFlow.Instance.Visible));Visible=strip.gameObject.activeSelf;if(!Visible)return;
   var phase=TeleportSequence.Instance?TeleportSequence.Instance.Phase:TeleportPhase.Preparing;
   SportId to=online?s.to:TeleportSequence.Instance.Destination;
   int step=online?(s.phase==RoomTeleportPhase.Preparing?1:s.phase==RoomTeleportPhase.Departing?2:3):phase==TeleportPhase.Preparing?1:phase is TeleportPhase.Arriving or TeleportPhase.Settling?3:2;
   var people=online?RoomTeleport.Players():System.Array.Empty<NetworkAthlete>();int ready=people.Count(n=>n.TeleportReady.Value.sequence==s.sequence&&!n.TeleportReady.Value.failed&&n.TeleportReady.Value.stage>=step);
   title.text=busy?(step==1?"Preparing ":step==2?"Travelling to ":"Arriving at ")+to:"Journey stopped";
   detail.text=busy?(online?ready+" / "+people.Length+" crew "+(step==1?"ready":step==2?"at destination":"settled")+" · ":"")+((online?s.destination:TeleportSequence.Instance.DestinationKind)==TeleportDestination.Station?"Skyrail station":"Game area"):online?s.reason.ToString():localError;
   stages.text=busy?step==1?"01  PREPARE     ·     02  TRAVEL     ·     03  ARRIVE":step==2?"✓  Prepared     ·     02  TRAVEL     ·     03  ARRIVE":"✓  Prepared     ·     ✓  Travelled     ·     03  ARRIVE":"Your group has been returned to the departure point.";
   bool host=online&&app.rooms.Host;detail.rectTransform.sizeDelta=new Vector2(!busy&&host?590:748,50);cancel.gameObject.SetActive(busy&&host);expand.gameObject.SetActive(busy&&online);
   retry.gameObject.SetActive(!busy&&host);choose.gameObject.SetActive(!busy&&host);dismiss.gameObject.SetActive(!busy);
   crew.gameObject.SetActive(busy&&online&&expanded);
   if(crew.gameObject.activeSelf){var rows=people.Select(n=>{var a=n.TeleportReady.Value;return n.Nickname.Value+" · "+(a.sequence==s.sequence&&a.failed?"Could not prepare":a.sequence==s.sequence&&a.stage>=step?"Ready":"Preparing…");}).ToArray();leftCrew.text=string.Join("\n",rows.Take(10));rightCrew.text=string.Join("\n",rows.Skip(10));}
   WorldUI.Fit(strip,(RectTransform)transform,1080,expanded?472:156,true);
  }
 }
}
