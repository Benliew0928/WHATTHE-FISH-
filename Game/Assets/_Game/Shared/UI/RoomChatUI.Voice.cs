using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace WhatTheFish {
 public sealed partial class RoomChatUI {
  RoomVoiceService voice;
  bool voiceOpen;
  RectTransform voicePanel,voiceRoster;
  Text voiceLauncher,voiceStatus,voiceNotice,voiceCount,volumeLabel;
  Button voiceJoin,voiceMic,voiceSpeakers,voiceSettings;
  Button voiceFallback;
  Slider voiceVolume;
  float voiceNextRefresh;
  readonly List<VoiceRow> voiceRows=new();
  sealed class VoiceRow {public string Id;public Button Button;public Text Name,State;}
  public RectTransform VoicePanel=>voicePanel;
  public bool VoiceOpen=>voiceOpen;
  public void OpenVoice(){voiceOpen=!voiceOpen;open=quick=options=false;ReleaseInput();Rebuild();app.view?.ClearMatchInput();}
  void BuildVoicePanel(float width){
   voice=RoomVoiceService.Ensure(app.rooms);voiceRows.Clear();
   if(!voiceFallback){voiceFallback=Button(safe,"Mute voice during travel","Mic on · tap to mute",0,0,280,54,Mint,()=>voice.Voice.SetMicrophone(false),19);MenuDesigns.Apply(voiceFallback);voiceFallback.gameObject.SetActive(false);}
   var launcher=Button(launchers,"Open room voice","Voice",210,0,130,58,Cream,OpenVoice,22);voiceLauncher=launcher.GetComponentInChildren<Text>();
   voicePanel=MenuDesigns.Dialog(CoveUI.Plate(root,"Room voice panel",0,0,width,610,PanelColor,18,1)).rectTransform;
   voicePanel.GetComponent<CovePlate>().raycastTarget=true;
   Label(voicePanel,"ROOM VOICE",16,10,width-88,30,22,Mint);
   Label(voicePanel,"One room · every island",16,42,width-34,24,16,Muted);
   Button(voicePanel,"Close voice","×",width-60,7,48,48,PlayerHubStyle.Raised,Close,28);
   voiceStatus=Label(voicePanel,"",16,76,width-32,49,18);
   voiceJoin=Button(voicePanel,"Connect room voice","Join voice",16,133,width-32,52,Mint,()=>{if(voice.Voice.Enabled&&!voice.Voice.Exhausted)voice.Leave();else _=voice.Join();},22);
   float cell=(width-44)/2;
   voiceMic=Button(voicePanel,"Toggle microphone","Unmute mic",16,196,cell,54,PlayerHubStyle.Raised,()=>voice.ToggleMicrophone(),20);
   voiceSpeakers=Button(voicePanel,"Toggle voice speakers","Sound on",28+cell,196,cell,54,PlayerHubStyle.Raised,()=>voice.ToggleSpeakers(),20);
   volumeLabel=Label(voicePanel,"Voice volume",16,260,140,28,17,Muted);
   voiceVolume=CreateVoiceSlider(voicePanel,156,255,width-172,40);
   voiceVolume.SetValueWithoutNotify(voice.Voice.Volume);voiceVolume.onValueChanged.AddListener(v=>voice.SetVolume(v));
   voiceCount=Label(voicePanel,"CREW IN VOICE",16,301,width-32,27,16,Mint);
   var viewport=CoveUI.Rect(voicePanel,"Voice roster viewport",12,335,width-24,212);viewport.gameObject.AddComponent<RectMask2D>();
   viewport.gameObject.AddComponent<Image>().color=Color.clear;
   voiceRoster=CoveUI.Rect(viewport,"Voice participants",0,0,width-24,212);
   var scroll=viewport.gameObject.AddComponent<ScrollRect>();scroll.viewport=viewport;scroll.content=voiceRoster;scroll.horizontal=false;scroll.movementType=ScrollRect.MovementType.Clamped;scroll.scrollSensitivity=35;
   Label(voiceRoster,"Your crew will appear here when connected.",4,8,width-32,72,17,Muted);
   voiceNotice=Label(voicePanel,"",16,553,width-32,45,14,Muted);
   voiceSettings=Button(voicePanel,"Microphone device settings","Device settings",16,553,width-32,43,PlayerHubStyle.Raised,RoomVoicePermissions.OpenSettings,17);
   MenuDesigns.Buttons(voicePanel);MenuDesigns.Apply(launcher);
   voiceSettings.gameObject.SetActive(false);voicePanel.gameObject.SetActive(voiceOpen);voiceNextRefresh=0;RefreshVoice();
  }
  void RefreshVoiceFallback(bool controlsVisible){
   if(!voiceFallback||!voice)return;
   voiceFallback.gameObject.SetActive(!controlsVisible&&voice.Voice.MicEnabled&&app.rooms.Connected);
   voiceFallback.GetComponent<RectTransform>().anchoredPosition=new Vector2(Mathf.Max(8,(safe.rect.width-280)/2),-Mathf.Max(8,safe.rect.height-72));
  }
  static Slider CreateVoiceSlider(Transform parent,float x,float y,float w,float h){
   var r=CoveUI.Rect(parent,"Voice volume slider",x,y,w,h);var slider=r.gameObject.AddComponent<Slider>();
   var hit=r.gameObject.AddComponent<Image>();hit.color=Color.clear;
   var track=CoveUI.Plate(r,"Track",0,16,w,8,PlayerHubStyle.Raised,4,0);MenuDesigns.Surface(track,MenuSurface.Track);
   var fillArea=CoveUI.Rect(r,"Fill area",10,16,w-20,8);var fill=CoveUI.Plate(fillArea,"Fill",0,0,w-20,8,Mint,4,0);CoveUI.Stretch(fill.rectTransform);
   var handleArea=CoveUI.Rect(r,"Handle area",12,7,w-24,26);var handle=CoveUI.Plate(handleArea,"Handle",0,0,26,26,Cream,13,0);handle.rectTransform.sizeDelta=new Vector2(26,0);handle.rectTransform.pivot=new Vector2(.5f,.5f);handle.rectTransform.anchoredPosition=Vector2.zero;MenuDesigns.Surface(handle,MenuSurface.Thumb);
   slider.fillRect=fill.rectTransform;slider.handleRect=handle.rectTransform;slider.targetGraphic=handle;slider.minValue=0;slider.maxValue=1;slider.direction=Slider.Direction.LeftToRight;
   return slider;
  }
  void RefreshVoice(){
   if(!voice||!voiceLauncher||Time.unscaledTime<voiceNextRefresh)return;voiceNextRefresh=Time.unscaledTime+.1f;
   var state=voice.Voice;voiceLauncher.text=state.MicEnabled?"Mic on":state.Ready?"Voice on":"Voice";
   voiceLauncher.color=state.MicEnabled?CoveUI.Hex("137250"):Ink;
   if(!voiceOpen)return;
   voiceStatus.text=voice.Status;
   voiceJoin.interactable=voice.Available&&!voice.AskingPermission;
   voiceJoin.GetComponentInChildren<Text>().text=state.Enabled&&!state.Exhausted?"Leave voice":state.Error.Length>0||voice.PermissionDenied?"Retry voice":"Join voice";
   voiceMic.interactable=state.Ready;voiceMic.GetComponentInChildren<Text>().text=state.MicEnabled?"Mute mic":"Unmute mic";
   voiceSpeakers.interactable=state.Ready;voiceSpeakers.GetComponentInChildren<Text>().text=state.SpeakersEnabled?"Sound on":"Sound off";
   voiceVolume.interactable=state.Ready&&state.SpeakersEnabled;volumeLabel.text="Volume  "+Mathf.RoundToInt(state.Volume*100)+"%";
   bool settings=voice.PermissionDenied||voice.NeedsDeviceSettings;voiceSettings.gameObject.SetActive(settings);voiceNotice.gameObject.SetActive(!settings);
   if(voice.NeedsDeviceSettings)voiceStatus.text=voice.Notice;
   voiceNotice.text=voice.Notice.Length>0?voice.Notice:state.Ready?"Tap a crew member to mute their voice.":"Your mic starts off. You choose when to speak.";
   var peers=state.Ready?state.Peers.Where(p=>p.IsSelf||state.IsRoomMember(p.Id)).OrderByDescending(p=>p.IsSelf).ThenBy(p=>voice.NameOf(p)).ToArray():Array.Empty<RoomVoicePeer>();
   voiceCount.text=peers.Length==0?"CREW IN VOICE":peers.Length+" IN VOICE · SPEAKING LIGHTS UP";
   if(voiceRows.Count!=peers.Length||!voiceRows.Select(r=>r.Id).SequenceEqual(peers.Select(p=>p.Id))){
    foreach(Transform child in voiceRoster){child.gameObject.SetActive(false);Destroy(child.gameObject);}voiceRows.Clear();float width=voiceRoster.rect.width;
    for(int i=0;i<peers.Length;i++){
     var peer=peers[i];string id=peer.Id;var b=Button(voiceRoster,"Voice member "+i,"",0,i*56,width,50,PlayerHubStyle.Raised,()=>{if(!peer.IsSelf)state.ToggleMute(id);},18);
     var name=Label(b.transform,voice.NameOf(peer),12,3,width-130,44,18);name.horizontalOverflow=HorizontalWrapMode.Wrap;
     var status=Label(b.transform,"",width-118,3,106,44,15,Muted);status.alignment=TextAnchor.MiddleRight;
     voiceRows.Add(new VoiceRow{Id=id,Button=b,Name=name,State=status});
    }
    voiceRoster.sizeDelta=new Vector2(width,Mathf.Max(212,peers.Length*56));
    if(peers.Length==0)Label(voiceRoster,"Your crew will appear here when connected.",4,8,width-8,72,17,Muted);
   }
   for(int i=0;i<peers.Length;i++){
    var peer=peers[i];bool muted=peer.IsSelf?!state.MicEnabled:state.IsMuted(peer.Id);var row=voiceRows[i];row.Name.text=voice.NameOf(peer);
    row.State.text=muted?"Muted":peer.Speaking?"● Speaking":"Connected";
    row.State.color=!muted&&peer.Speaking?Mint:Muted;row.Button.interactable=!peer.IsSelf;
   }
  }
 }
}
