using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
namespace WhatTheFish {
 public sealed class RoomRecoveryUI:MonoBehaviour {
  RoomService room;RectTransform card;Text detail,phase,waiting;Button leave;float began;bool exiting;
  public static RoomRecoveryUI Create(RoomService owner){var ui=new GameObject("Room reconnecting",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster)).AddComponent<RoomRecoveryUI>();ui.room=owner;return ui;}
  void Awake(){
   began=Time.realtimeSinceStartup;WorldUI.Canvas(gameObject,32000);
   var shade=CoveUI.Rect(transform,"Paused scene",0,0,1600,900);CoveUI.Stretch(shade);shade.gameObject.AddComponent<Image>().color=new Color(.025f,.04f,.035f,.42f);
   card=CoveUI.Rect(transform,"Recovery card",0,0,760,382);WorldUI.Card(card,"Recovery surface",0,0,760,382);
   WorldUI.Label(card,"CONNECTION INTERRUPTED",30,23,700,30,20,true,CoveUI.Gold);
   WorldUI.Label(card,"Reconnecting your crew",28,66,704,60,40,true);
   detail=WorldUI.Label(card,"",30,141,700,54,25);
   phase=WorldUI.Label(card,"",30,211,700,30,20,true,CoveUI.Mint);
   waiting=WorldUI.Label(card,"",30,252,438,99,20,false,WorldUI.Muted);
   leave=WorldUI.Button(card,"Leave recovering room","Leave room",496,287,234,60,async()=>{if(exiting)return;exiting=true;leave.interactable=false;await room.Leave();});
  }
  void Update(){if(!room)return;WorldUI.Fit(card,(RectTransform)transform,760,382);
   var net=NetworkManager.Singleton;bool connected=net&&net.IsConnectedClient;bool found=room.RecoveryMessage!="Finding the next host…";
   detail.text=exiting?"Leaving the room…":connected?"The crew is reconnecting. Restoring the last shared game state…":room.RecoveryMessage;
   phase.text=connected?"✓  Host found     ·     ✓  Connected     ·     RESUMING":found?"✓  Host found     ·     CONNECTING     ·     Resume":"FINDING HOST     ·     Connect     ·     Resume";
   waiting.text=Time.realtimeSinceStartup-began<15?"Gameplay is paused while the room reconnects.":"This is taking longer than usual. You can wait here or leave the room.";
  }
 }
}
