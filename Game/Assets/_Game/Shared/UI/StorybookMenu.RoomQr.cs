using System;
using UnityEngine;
using UnityEngine.UI;
using static WhatTheFish.CoveUI;

namespace WhatTheFish {
 public sealed partial class StorybookMenu {
  RawImage roomQrImage;CoveButton roomQrButton;Text roomQrHint;Texture2D roomQrTexture;
  string renderedRoomCode;bool roomQrModal;
  void ResetRoomQr(){HuaweiRoomScan.Cancel();if(roomQrTexture)Destroy(roomQrTexture);roomQrTexture=null;roomQrImage=null;roomQrButton=null;roomQrHint=null;renderedRoomCode=null;roomQrModal=false;}
  void BuildRoomQr(Transform parent) {
   roomQrButton=Button(parent,"Show room QR","",328,70,168,168,Color.white,ShowRoomQr,size:20);
   roomQrImage=Rect(roomQrButton.transform,"Room QR image",4,4,160,160).gameObject.AddComponent<RawImage>();roomQrImage.raycastTarget=false;roomQrImage.enabled=false;
   roomQrHint=Text(roomQrButton.transform,"Online\nroom only",8,43,152,75,21,false,Muted,TextAnchor.MiddleCenter);
   RefreshRoomQr();
  }
  void RefreshRoomQr() {
   if(!roomQrButton)return;
   string code=app.rooms.Connected&&!app.rooms.Recovering?app.rooms.Code:"";
   if(code==renderedRoomCode)return;renderedRoomCode=code;
   if(roomQrModal&&modalRoot){Destroy(modalRoot);modalRoot=null;roomQrModal=false;}
   if(roomQrTexture)Destroy(roomQrTexture);roomQrTexture=null;roomQrImage.texture=null;roomQrImage.enabled=false;roomQrHint.gameObject.SetActive(true);roomQrButton.interactable=false;
   roomQrHint.text="Online\nroom only";if(!ValidCode(code))return;
   try {roomQrTexture=HuaweiRoomScan.Generate(code);roomQrImage.texture=roomQrTexture;roomQrImage.enabled=true;roomQrHint.gameObject.SetActive(false);roomQrButton.interactable=true;}
   catch(Exception e){Debug.LogWarning("Room QR generation failed: "+e.GetType().Name);roomQrHint.text="Share the\nroom code";}
  }
  void ShowRoomQr() {
   if(!roomQrTexture||modalRoot||!app.rooms.Connected)return;
   var overlay=Plate(content,"Room QR invitation",0,0,1600,900,new Color(.04f,.10f,.17f,.88f),0,0);overlay.raycastTarget=true;modalRoot=overlay.gameObject;roomQrModal=true;group.interactable=false;
   var card=Plate(overlay.transform,"Room QR card",430,40,740,820,Cream,24,2,true);card.gameObject.AddComponent<CanvasGroup>().ignoreParentGroups=true;
   Text(card.transform,"SCAN TO JOIN",30,22,680,60,36,true,null,TextAnchor.MiddleCenter);
   var qr=Rect(card.transform,"Large room QR",130,100,480,480).gameObject.AddComponent<RawImage>();qr.texture=roomQrTexture;qr.raycastTarget=false;
   Text(card.transform,renderedRoomCode,30,594,680,55,38,true,null,TextAnchor.MiddleCenter);
   Text(card.transform,"Open Join a room > Scan QR on another phone.\nYou can also type the code. Internet required.",30,657,680,69,23,false,Muted,TextAnchor.MiddleCenter);
   Button(card.transform,"Close room QR","Done",190,741,360,58,Gold,()=>{Destroy(overlay.gameObject);modalRoot=null;roomQrModal=false;group.interactable=true;},size:25).Select();
  }
  void ScanRoomInvitation()=>ScanRoomInvitation(false);
  void ScanRoomInvitation(bool fromImage) {
   if(app.rooms.busy||HuaweiRoomScan.Busy||modalRoot)return;
   var page=content;
   if(message)message.text=fromImage?"Opening image picker…":"Opening camera…";
   HuaweiRoomScan.Scan((status,value)=>{
    if(!this||Page!="join"||content!=page||!roomCode)return;
    if(status=="success") {
     if(RoomInvitation.TryDecode(value,out string code,out string error)){roomCode.text=code;if(message)message.text="Room code scanned. Tap Join room to connect.";}
     else if(message)message.text=error;
    }else if(message)message.text=status=="cancel"?"Scan cancelled. You can still enter the room code.":value;
    Refresh();
   },fromImage);
  }
 }
}
