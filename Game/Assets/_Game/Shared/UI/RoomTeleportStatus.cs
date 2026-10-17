using UnityEngine;
using UnityEngine.UI;
namespace WhatTheFish {
 public sealed class RoomTeleportStatus:MonoBehaviour {
  RectTransform strip;Text label;Button cancel;string last="";float messageUntil;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Init(){new GameObject("Room travel status",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster)).AddComponent<RoomTeleportStatus>();}
  void Awake(){var canvas=GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=1100;var scale=GetComponent<CanvasScaler>();scale.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scale.referenceResolution=new Vector2(1600,900);scale.screenMatchMode=CanvasScaler.ScreenMatchMode.Expand;
   strip=CoveUI.Rect(transform,"Travel status",0,0,1060,80);strip.anchorMin=strip.anchorMax=new Vector2(.5f,0);strip.pivot=new Vector2(.5f,0);strip.anchoredPosition=new Vector2(0,24);CoveUI.Plate(strip,"Travel card",0,0,1060,80,CoveUI.Cream,24,0,true);label=CoveUI.Text(strip,"",24,12,820,56,26,true,null,TextAnchor.MiddleLeft);cancel=CoveUI.Button(strip,"Cancel travel","Cancel",856,12,180,56,CoveUI.Sky,()=>RoomTeleport.Instance.Cancel(),size:25);strip.gameObject.SetActive(false);
  }
  void Update(){var room=RoomTeleport.Instance;var app=AppRoot.Instance;if(!room||!app)return;string message=room.Message;if(message!=last){last=message;messageUntil=Time.unscaledTime+12;}bool busy=RoomTeleport.Busy;
   strip.gameObject.SetActive(app.Exploring&&(busy||message.Length>0&&Time.unscaledTime<messageUntil));if(!strip.gameObject.activeSelf)return;
   var phase=TeleportSequence.Instance?TeleportSequence.Instance.Phase:TeleportPhase.Preparing;
   label.text=busy?(phase==TeleportPhase.Preparing?"Preparing everyone's arrival…":phase==TeleportPhase.Settling?"Waiting for everyone to arrive…":"Travelling together to "+room.State.to+" · "+(room.State.destination==TeleportDestination.Station?"Skyrail station":"Game area")):message;
   cancel.gameObject.SetActive(busy&&app.rooms.Host);label.rectTransform.sizeDelta=new Vector2(busy&&app.rooms.Host?820:1012,56);
   var root=(RectTransform)transform;strip.localScale=Vector3.one*Mathf.Min(1,Screen.safeArea.width/Screen.width*root.rect.width/1100);strip.anchoredPosition=new Vector2((Screen.safeArea.center.x/Screen.width-.5f)*root.rect.width,24+Screen.safeArea.y/Screen.height*root.rect.height);
  }
 }
}
