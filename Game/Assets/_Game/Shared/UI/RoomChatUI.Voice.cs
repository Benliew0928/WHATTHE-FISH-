using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace WhatTheFish {
 public sealed partial class RoomChatUI {
  RoomVoiceService voice;bool voiceOpen,voiceSpectators;string selectedVoiceMember="",renderedVoiceMember="";
  RectTransform voicePanel,voiceRoster,voiceViewport,voiceControls,voiceControlViewport;ScrollRect voiceScroll;
  Text voiceStatus,volumeLabel;Button voiceLauncher,voiceJoin,voiceMic,voiceSpeakers,voiceSettings,voiceSpectatorMute,voiceFallback;
  RoomCommsIcon launcherMicIcon,ownMicIcon,joinIcon,outputIcon,spectatorIcon,fallbackIcon;
  Slider voiceVolume;float voiceNextRefresh;readonly List<VoiceRow> voiceRows=new();
  sealed class VoiceRow {public string Id;public Text Name,State,VolumeText;public Button Mute;public RoomCommsIcon Icon;public Slider Volume;}
  public RectTransform VoicePanel=>voicePanel;
  public bool VoiceOpen=>voiceOpen;
  public bool SpectatorVoice=>voiceSpectators;
  public void OpenVoice(){voiceOpen=!voiceOpen;open=quick=options=false;ReleaseInput();Rebuild();app.view?.ClearMatchInput();}
  public void SetVoiceAudience(bool spectators){voiceSpectators=spectators;selectedVoiceMember="";Rebuild();}
  RoomCommsIcon Icon(Transform parent,RoomCommsSymbol symbol,float x,float y,float size,Color color){var icon=CoveUI.Rect(parent,symbol+" icon",x,y,size,size).gameObject.AddComponent<RoomCommsIcon>();icon.Symbol=symbol;icon.color=color;icon.raycastTarget=false;return icon;}
  Button IconButton(Transform parent,string id,RoomCommsSymbol symbol,float x,float y,float w,float h,Color tint,Action action,out RoomCommsIcon icon,string caption=""){
   var button=Button(parent,id,caption,x,y,w,h,tint,action,18);MenuDesigns.Apply(button);
   bool labelled=caption.Length>0,small=labelled&&w<150;float iconSize=small?24:32,left=small?42:57;icon=Icon(button.transform,symbol,labelled?(small?10:16):(w-iconSize)/2,(h-iconSize)/2,iconSize,tint==Mint?Ink:Cream);
   if(labelled){var text=button.GetComponentInChildren<Text>();text.rectTransform.anchoredPosition=new Vector2(left,-2);text.rectTransform.sizeDelta=new Vector2(w-left-8,h-4);text.alignment=TextAnchor.MiddleLeft;if(small)text.fontSize=16;}
   return button;
  }
  Button[] AudienceTabs(Transform parent,float width,float y,bool spectators,Action<bool> select,string prefix,float left=12,float right=12){
   var tabs=new Button[2];float cell=(width-left-right-8)/2;
   for(int i=0;i<2;i++){bool choice=i==1;tabs[i]=Button(parent,prefix+(choice?" spectators":" players"),choice?"Spectators":"Players",left+i*(cell+8),y,cell,44,choice==spectators?Mint:PlayerHubStyle.Raised,()=>select(choice),18);MenuDesigns.Apply(tabs[i]);MenuDesigns.Selected(tabs[i],choice==spectators);}
   return tabs;
  }
  void BuildVoicePanel(float width){
   voice=RoomVoiceService.Ensure(app.rooms);voiceRows.Clear();renderedVoiceMember="";
   if(!voiceFallback){voiceFallback=IconButton(safe,"Mute voice during travel",RoomCommsSymbol.Microphone,0,0,64,64,Mint,()=>voice.Voice.SetMicrophone(false),out fallbackIcon);voiceFallback.gameObject.SetActive(false);}
   voiceLauncher=IconButton(launchers,"Open room voice",RoomCommsSymbol.Microphone,76,0,64,64,CoveUI.Hex("553F50"),null,out launcherMicIcon);
   var gesture=voiceLauncher.gameObject.AddComponent<RoomMicPress>();gesture.Tap=()=>{voice.TapMicrophone();if(!voice.Available&&!voiceOpen)OpenVoice();};gesture.Hold=()=>{if(!voiceOpen)OpenVoice();};voiceLauncher.onClick.AddListener(gesture.Click);
   voiceControls=voiceControlViewport=null;
   voicePanel=MenuDesigns.Dialog(CoveUI.Plate(root,"Room voice panel",0,0,width,lobby?600:260,PanelColor,18,1)).rectTransform;voicePanel.GetComponent<CovePlate>().raycastTarget=true;
   Label(voicePanel,"VOICE",16,8,lobby?width-88:70,36,22,Mint);Button(voicePanel,"Close voice","×",width-60,7,48,48,PlayerHubStyle.Raised,Close,28);
   AudienceTabs(voicePanel,width,lobby?60:8,voiceSpectators,SetVoiceAudience,"Voice",lobby?12:88,lobby?12:64);
   Transform controls=voicePanel;float controlWidth=width,joinY=116,micY=180,volumeY=236,groupY=284,statusY=550;
   float rosterX=12,rosterTop=voiceSpectators?344:284,rosterWidth=width-24,rosterHeight=voiceSpectators?198:258;
   if(!lobby){
    CreateVoiceScroll(voicePanel,"Voice room controls",12,62,250,186,out voiceControlViewport,out voiceControls);
    controls=voiceControls;controlWidth=250;joinY=0;micY=50;volumeY=96;groupY=140;statusY=voiceSpectators?190:140;
    rosterX=274;rosterTop=62;rosterWidth=width-rosterX-12;rosterHeight=186;
   }
   float inset=lobby?12:0,inner=controlWidth-inset*2,cell=(inner-8)/2;
   voiceJoin=IconButton(controls,"Connect room voice",RoomCommsSymbol.Speaker,inset,joinY,inner,lobby?54:44,Mint,()=>{if(voice.Voice.Enabled&&!voice.Voice.Exhausted)voice.Leave();else _=voice.Join();},out joinIcon,"Join room voice");
   voiceMic=IconButton(controls,"Toggle microphone",RoomCommsSymbol.Microphone,inset,micY,cell,lobby?52:44,PlayerHubStyle.Raised,()=>voice.ToggleMicrophone(),out ownMicIcon,"Mic off");
   voiceSpeakers=IconButton(controls,"Toggle voice speakers",RoomCommsSymbol.Speaker,inset+cell+8,micY,cell,lobby?52:44,PlayerHubStyle.Raised,()=>voice.ToggleSpeakers(),out outputIcon,"Sound on");
   float sliderX=lobby?145:96;
   volumeLabel=Label(controls,"Volume",lobby?16:0,volumeY+6,lobby?134:94,30,lobby?16:14,Muted);
   voiceVolume=CreateVoiceSlider(controls,sliderX,volumeY,controlWidth-sliderX-(lobby?16:0),42);
   voiceVolume.SetValueWithoutNotify(voice.Voice.Volume);voiceVolume.onValueChanged.AddListener(v=>voice.SetVolume(v));
   voiceSpectatorMute=IconButton(controls,"Mute all spectators",RoomCommsSymbol.Speaker,inset,groupY,inner,lobby?48:44,PlayerHubStyle.Raised,()=>{voice.Voice.MuteSpectators(!voice.Voice.SpectatorsMuted);voiceNextRefresh=0;},out spectatorIcon,"Mute spectators");voiceSpectatorMute.gameObject.SetActive(voiceSpectators);
   voiceScroll=CreateVoiceScroll(voicePanel,"Voice roster",rosterX,rosterTop,rosterWidth,rosterHeight,out voiceViewport,out voiceRoster);
   voiceStatus=Label(controls,"",lobby?16:0,statusY,controlWidth-(lobby?32:0),40,14,CoveUI.Coral);
   voiceSettings=Button(controls,"Microphone device settings","Device settings",lobby?16:0,statusY,controlWidth-(lobby?32:0),40,PlayerHubStyle.Raised,RoomVoicePermissions.OpenSettings,17);
   MenuDesigns.Buttons(voicePanel);voiceSettings.gameObject.SetActive(false);voicePanel.gameObject.SetActive(voiceOpen);voiceNextRefresh=0;RefreshVoice();
  }
  static ScrollRect CreateVoiceScroll(Transform parent,string name,float x,float y,float w,float h,out RectTransform viewport,out RectTransform content){
   viewport=CoveUI.Rect(parent,name+" viewport",x,y,w,h);viewport.gameObject.AddComponent<RectMask2D>();viewport.gameObject.AddComponent<Image>().color=Color.clear;
   content=CoveUI.Rect(viewport,name+" content",0,0,w,h);
   var scroll=viewport.gameObject.AddComponent<ScrollRect>();scroll.viewport=viewport;scroll.content=content;scroll.horizontal=false;scroll.movementType=ScrollRect.MovementType.Clamped;scroll.scrollSensitivity=35;return scroll;
  }
  void RefreshVoiceFallback(bool controlsVisible){
   if(!voiceFallback||!voice)return;voiceFallback.gameObject.SetActive(!controlsVisible&&voice.Voice.MicEnabled&&app.rooms.Connected);
   voiceFallback.GetComponent<RectTransform>().anchoredPosition=new Vector2(Mathf.Max(8,safe.rect.width/2),-Mathf.Max(8,safe.rect.height-46));
  }
  static Slider CreateVoiceSlider(Transform parent,float x,float y,float w,float h,string id="Voice volume slider"){
   var r=CoveUI.Rect(parent,id,x,y,w,h);var slider=r.gameObject.AddComponent<Slider>();r.gameObject.AddComponent<Image>().color=Color.clear;
   var track=CoveUI.Plate(r,"Track",0,16,w,8,PlayerHubStyle.Raised,4,0);MenuDesigns.Surface(track,MenuSurface.Track);
   var fillArea=CoveUI.Rect(r,"Fill area",10,16,w-20,8);var fill=CoveUI.Plate(fillArea,"Fill",0,0,w-20,8,Mint,4,0);CoveUI.Stretch(fill.rectTransform);
   var handleArea=CoveUI.Rect(r,"Handle area",12,7,w-24,26);var handle=CoveUI.Plate(handleArea,"Handle",0,0,26,26,Cream,13,0);handle.rectTransform.sizeDelta=new Vector2(26,0);handle.rectTransform.pivot=new Vector2(.5f,.5f);handle.rectTransform.anchoredPosition=Vector2.zero;MenuDesigns.Surface(handle,MenuSurface.Thumb);
   slider.fillRect=fill.rectTransform;slider.handleRect=handle.rectTransform;slider.targetGraphic=handle;slider.minValue=0;slider.maxValue=1;slider.direction=Slider.Direction.LeftToRight;return slider;
  }
  void RefreshVoice(){
   if(!voice||!voiceLauncher||Time.unscaledTime<voiceNextRefresh)return;voiceNextRefresh=Time.unscaledTime+.1f;
   var state=voice.Voice;bool mic=state.MicEnabled;
   launcherMicIcon.Set(RoomCommsSymbol.Microphone,!mic,mic?Ink:CoveUI.Coral);
   voiceLauncher.targetGraphic.color=mic?Mint:CoveUI.Hex("553F50");MenuDesigns.Selected(voiceLauncher,mic||voice.PendingMicrophone);
   if(!voiceOpen)return;
   bool joined=state.Enabled&&!state.Exhausted;
   voiceJoin.interactable=voice.Available&&!voice.AskingPermission;
   voiceJoin.GetComponentInChildren<Text>().text=joined?"Leave room voice":state.Error.Length>0||voice.PermissionDenied?"Retry room voice":"Join room voice";
   joinIcon.Set(RoomCommsSymbol.Speaker,!joined,Ink);
   voiceMic.interactable=state.Ready;voiceMic.GetComponentInChildren<Text>().text=mic?"Mic on":"Mic off";ownMicIcon.Set(RoomCommsSymbol.Microphone,!mic,mic?Mint:CoveUI.Coral);
   voiceSpeakers.interactable=state.Ready;voiceSpeakers.GetComponentInChildren<Text>().text=state.SpeakersEnabled?"Sound on":"Sound off";outputIcon.Set(RoomCommsSymbol.Speaker,!state.SpeakersEnabled,state.SpeakersEnabled?Cream:CoveUI.Coral);
   voiceVolume.interactable=state.Ready&&state.SpeakersEnabled;volumeLabel.text="Volume "+Mathf.RoundToInt(state.Volume*100)+"%";
   voiceSpectatorMute.GetComponentInChildren<Text>().text=state.SpectatorsMuted?"Unmute spectators":"Mute spectators";
   spectatorIcon.Set(RoomCommsSymbol.Speaker,state.SpectatorsMuted,state.SpectatorsMuted?CoveUI.Coral:Cream);
   bool settings=voice.PermissionDenied||voice.NeedsDeviceSettings;voiceSettings.gameObject.SetActive(settings);voiceStatus.gameObject.SetActive(!settings);
   voiceStatus.text=voice.Notice.Length>0?voice.Notice:!voice.Available||state.Busy||state.Suspended||state.Error.Length>0||voice.AskingPermission?voice.Status:"";
   if(voiceControls){
    float needed=(voiceSpectators?190:140)+((settings||voiceStatus.text.Length>0)?40:0);
    voiceControls.sizeDelta=new Vector2(voiceControls.rect.width,Mathf.Max(voiceControlViewport.rect.height,needed));
   }
   var peers=voice.Roster.Where(p=>p.Spectator==voiceSpectators).OrderByDescending(p=>p.IsSelf).ThenBy(p=>voice.NameOf(p),StringComparer.Ordinal).ToArray();
   if(voiceRows.Count!=peers.Length||!voiceRows.Select(r=>r.Id).SequenceEqual(peers.Select(p=>p.Id))||renderedVoiceMember!=selectedVoiceMember)BuildVoiceRoster(peers);
   for(int i=0;i<peers.Length;i++){
    var peer=peers[i];var row=voiceRows[i];bool muted=peer.IsSelf?!mic:state.PlaybackMuted(peer.Id);
    GameLocalization.UserContent(row.Name);row.Name.text=voice.NameOf(peer)+(peer.IsSelf?" ("+GameLocalization.Translate("You")+")":"");
    row.State.text=!peer.Connected?"Not in voice":!muted&&peer.Speaking?"● Speaking":"";row.State.color=Mint;
    row.Icon.Set(peer.IsSelf?RoomCommsSymbol.Microphone:RoomCommsSymbol.Speaker,muted,muted?CoveUI.Coral:Cream);
    row.Mute.interactable=peer.IsSelf?state.Ready:!state.SpectatorsMuted||!peer.Spectator;
    if(row.Volume){row.Volume.interactable=!state.SpectatorsMuted||!peer.Spectator;row.Volume.SetValueWithoutNotify(state.VolumeOf(peer.Id));row.VolumeText.text=Mathf.RoundToInt(state.VolumeOf(peer.Id)*100)+"%";}
   }
  }
  void BuildVoiceRoster(RoomVoicePeer[] peers){
   bool selectionChanged=renderedVoiceMember!=selectedVoiceMember;float selectedTop=0;
   foreach(Transform child in voiceRoster){child.gameObject.SetActive(false);Destroy(child.gameObject);}voiceRows.Clear();renderedVoiceMember=selectedVoiceMember;
   float width=voiceRoster.rect.width,y=0;
   for(int i=0;i<peers.Length;i++){
    var peer=peers[i];string id=peer.Id;bool expanded=selectedVoiceMember==id&&!peer.IsSelf;
    var card=CoveUI.Plate(voiceRoster,"Voice member card "+i,0,y,width,expanded?128:64,PlayerHubStyle.Raised,12,1).rectTransform;
    var choose=Button(card,"Voice member "+i,"",0,0,width-56,64,PlayerHubStyle.Raised,()=>{if(!peer.IsSelf){selectedVoiceMember=selectedVoiceMember==id?"":id;voiceNextRefresh=0;}},18);
    var row=new VoiceRow{Id=id,Name=Label(choose.transform,voice.NameOf(peer),12,1,width-80,42,18),State=Label(choose.transform,"",12,43,width-80,17,13,Mint)};
    row.Mute=IconButton(card,"Mute voice member "+i,peer.IsSelf?RoomCommsSymbol.Microphone:RoomCommsSymbol.Speaker,width-52,8,44,44,PlayerHubStyle.Raised,()=>{if(peer.IsSelf)voice.ToggleMicrophone();else voice.Voice.ToggleMute(id);voiceNextRefresh=0;},out var icon);row.Icon=icon;
    if(expanded){selectedTop=y;row.VolumeText=Label(card,Mathf.RoundToInt(voice.Voice.VolumeOf(id)*100)+"%",12,80,54,32,15,Muted);row.Volume=CreateVoiceSlider(card,72,76,width-88,44,"Member volume slider");row.Volume.SetValueWithoutNotify(voice.Voice.VolumeOf(id));row.Volume.onValueChanged.AddListener(v=>{voice.Voice.SetPeerVolume(id,v);row.VolumeText.text=Mathf.RoundToInt(v*100)+"%";});}
    voiceRows.Add(row);y+=expanded?136:72;
   }
   if(peers.Length==0)Label(voiceRoster,voiceSpectators?"No spectators yet":"No players yet",6,14,width-12,48,18,Muted);
   voiceRoster.sizeDelta=new Vector2(width,Mathf.Max(voiceViewport.rect.height,y));
   if(selectionChanged&&selectedVoiceMember.Length>0){voiceScroll.StopMovement();voiceRoster.anchoredPosition=new Vector2(0,Mathf.Clamp(selectedTop,0,Mathf.Max(0,y-voiceViewport.rect.height)));}
  }
 }
}
