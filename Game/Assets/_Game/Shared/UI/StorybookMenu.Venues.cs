using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static WhatTheFish.CoveUI;

namespace WhatTheFish {
 public sealed partial class StorybookMenu {
  int venueCamera;
  CoveButton[] venueViewButtons;
  bool venueEditAllowed;
  SportId draftSport;
  StadiumAppearance SavedVenue=>app.rooms.Connected&&NetworkAthlete.HostPlayer&&NetworkAthlete.HostPlayer.WorldAppearance.Value.Length>0
   ?JsonUtility.FromJson<StadiumAppearance>(NetworkAthlete.HostPlayer.WorldAppearance.Value.ToString()):app.CurrentAppearance;
  bool VenueDirty=>hasDraft&&JsonUtility.ToJson(draft)!=JsonUtility.ToJson(SavedVenue);
  void SelectVenue(SportId sport){
   if(sport==venueSport||app.rooms.Connected||app.PendingSelection.HasValue)return;
   ReadDraftName();
   void Change(){app.stadium?.Apply(SavedVenue);hasDraft=false;venueSport=sport;venueCamera=0;app.SelectSport(sport);app.Show("venues");}
   if(VenueDirty)Confirm("Switch sport?","Save this look first, or discard it to open "+sport+".","Discard changes",Change);
   else Change();
  }
  void VenueHub(){
   Heading("CUSTOMIZE VENUE",app.rooms.Connected?"Your room shares one island. The host chooses its look.":"Choose a sport. Shape its look. See your changes as you go.");
   SportTabs("Venue",venueSport,SelectVenue);
   if(app.rooms.Connected)foreach(var button in content.GetComponentsInChildren<CoveButton>().Where(b=>b.name.StartsWith("Venue - ")))button.interactable=false;
   if(app.PendingSelection.HasValue||app.SelectedSport!=venueSport){
    Text(content,"Loading your "+venueSport.ToString().ToLowerInvariant()+" venue...",180,390,1240,90,34,true,Ink,TextAnchor.MiddleCenter);return;
   }
   if(!hasDraft||draftSport!=venueSport){if(draftSport!=venueSport)venueCamera=0;draft=SavedVenue;draft.Clamp();draftSport=venueSport;hasDraft=true;}
   venueEditAllowed=CanEdit;
   bool golf=venueSport==SportId.Golf,fishing=venueSport==SportId.Fishing,island=golf||fishing;
   var p=Plate(content,"Venue controls",64,282,736,508,Cream,24,2,true);
   Text(p.transform,"VENUE NAME",24,15,688,31,21,true,Muted);
   venueName=Field(p.transform,"Venue name","Name your venue",draft.title,24,51,688,24);
   venueName.interactable=CanEdit;
   venueName.onEndEdit.AddListener(v=>{
    if(!CanEdit)return;draft.title=v;draft.Clamp();venueName.SetTextWithoutNotify(draft.title);venueCamera=venueSport==SportId.Football?1:3;
    // Ending an edit can be the pointer-down on Save or another choice. Keep
    // those controls alive so that the same click can finish normally.
    RefreshVenueViews();PreviewDraft();
   });
   Text(p.transform,venueSport switch{SportId.Football=>"FLAGS & SCREEN ACCENT",SportId.Basketball=>"SEATS & COURT TRIM",SportId.Golf=>"COURSE FLAG ACCENT",_=>"PIER ACCENTS"},24,131,688,31,21,true,Muted);
   string[] palettes={"Mint","Rose","Lilac","Gold"};
   for(int i=0;i<4;i++){int n=i;VenueButton(p.transform,"Palette "+palettes[i],palettes[i],24+i*174,170,166,58,LocalProfile.Teams[i],()=>{draft.palette=n;venueCamera=venueSport==SportId.Football?1:2;},draft.palette==i);}
   if(venueSport==SportId.Basketball){
    Text(p.transform,"CENTRE-COURT LOGO",24,256,688,31,21,true,Muted);
    string[] logos={"Rally crest","Star","Lightning","Shield"};
    for(int i=0;i<4;i++){int n=i;VenueButton(p.transform,"Logo "+logos[i],logos[i],24+i%2*352,303+i/2*90,336,72,draft.logo==i?Gold:Color.white,()=>{draft.logo=n;venueCamera=1;},draft.logo==i);}
   }else if(island){
    Text(p.transform,golf?"CLUB FINISH":"ROD FINISH",24,252,688,31,21,true,Muted);
    string[] finishes={"Original","Sea glass","Copper"};
    for(int i=0;i<3;i++){int n=i;VenueButton(p.transform,"Equipment "+finishes[i],finishes[i],24+i*234,292,220,64,draft.design==i?Gold:Color.white,()=>{draft.design=n;venueCamera=1;},draft.design==i);}
    Text(p.transform,"WELCOME SIGN",24,382,688,31,21,true,Muted);
    VenueButton(p.transform,"Sign title","Venue name",24,424,336,61,draft.screen==0?Mint:Color.white,()=>{draft.screen=0;venueCamera=3;},draft.screen==0);
    VenueButton(p.transform,"Sign welcome","Team welcome",376,424,336,61,draft.screen==1?Mint:Color.white,()=>{draft.screen=1;venueCamera=3;},draft.screen==1);
   }else{
    VenueButton(p.transform,"Screen design","Screen: "+(draft.screen==0?"Venue title":"Team welcome"),24,258,688,62,Color.white,()=>{draft.screen=1-draft.screen;venueCamera=1;});
    var board=VenueButton(p.transform,"Board design","Message: "+new[]{"Sunny days","Good energy","Move & play"}[draft.design],24,340,688,62,Color.white,()=>{draft.design=(draft.design+1)%3;venueCamera=1;});
    board.interactable=CanEdit&&draft.screen==0;
    VenueButton(p.transform,"Decorative flags","Decorative flags: "+(draft.flags?"On":"Off"),24,422,688,62,draft.flags?Mint:Color.white,()=>{draft.flags=!draft.flags;venueCamera=2;});
   }
   var side=Plate(content,"Venue preview",824,282,712,508,Cream,24,2,true);
   Text(side.transform,"LIVE PREVIEW",24,14,350,34,23,true);
   Badge(side.transform,venueSport.ToString().ToUpperInvariant(),466,12,222,Accent(venueSport));
   string[] views=island?new[]{"Island",golf?"Clubs":"Rods",golf?"Flags":"Piers","Sign"}:venueSport==SportId.Football?new[]{"Overview","Big screen","Flags"}:new[]{"Overview","Centre court","Seats & trim","Sign"};
   float width=(664-16*(views.Length-1))/views.Length;
   venueViewButtons=new CoveButton[views.Length];
   for(int i=0;i<views.Length;i++){int view=i;venueViewButtons[i]=Button(side.transform,i==0?"Preview overview":i==1?"Preview detail":i==2?"Preview accents":"Preview sign",views[i],24+i*(width+16),61,width,51,venueCamera==i?Gold:Color.white,()=>{ReadDraftName();venueCamera=view;RefreshVenueViews();PreviewDraft();},CoveSymbol.None,22);}
   var frame=Plate(side.transform,"Preview frame",24,126,664,294,Sky,10,1);
   venuePreview=Rect(frame.transform,"Live venue preview",3,3,658,288).gameObject.AddComponent<RawImage>();venuePreview.raycastTarget=false;
   previewTitle=Text(side.transform,draft.title,24,429,664,34,25,true,Ink,TextAnchor.MiddleCenter);GameLocalization.UserContent(previewTitle);
   Text(side.transform,island?"Cosmetic only. Your equipment keeps the same performance.":"Preview changes before saving your shared venue.",24,468,664,28,19,false,Muted,TextAnchor.MiddleCenter);
   var reset=ActionButton("Reset venue","Reset look",64,812,244,64,Cream,()=>{draft=LocalProfile.DefaultVenue(venueSport);Rebuild();},CoveSymbol.Paint,24);reset.interactable=CanEdit;
   Text(content,CanEdit?app.rooms.Connected?"Save to update the venue for everyone in this room.":"Saved per sport on this device. Used when you host.":"Your host chooses this room's venue look.",338,818,808,52,22,false,Ink,TextAnchor.MiddleCenter);
   var save=ActionButton("Save venue","Save look",1196,812,340,64,Mint,()=>{ReadDraftName();app.SaveStadium(draft);Rebuild();NoticeLater("Saved. Your "+venueSport.ToString().ToLowerInvariant()+" venue is ready.");},CoveSymbol.Check,26);save.interactable=CanEdit;
   PreviewDraft();
  }
  CoveButton VenueButton(Transform parent,string id,string label,float x,float y,float w,float h,Color color,Action change,bool selected=false){
   var b=Button(parent,id,label,x,y,w,h,color,()=>{if(!CanEdit)return;ReadDraftName();change();Rebuild();},selected?CoveSymbol.Check:CoveSymbol.None,23);b.interactable=CanEdit;return b;
  }
  void ReadDraftName(){if(!hasDraft||!CanEdit)return;if(venueName)draft.title=venueName.text;if(string.IsNullOrWhiteSpace(draft.title))draft.title=LocalProfile.DefaultVenue(venueSport).title;draft.Clamp();}
  void RefreshVenueViews(){
   if(venueViewButtons==null)return;
   for(int i=0;i<venueViewButtons.Length;i++)if(venueViewButtons[i]){
    var color=i==venueCamera?Gold:Raised;venueViewButtons[i].GetComponent<CoveFeedback>().SetTint(color);
    foreach(var label in venueViewButtons[i].GetComponentsInChildren<Text>())label.color=Foreground(color);
   }
  }
  void PreviewDraft(){if(!hasDraft||app.PendingSelection.HasValue)return;app.stadium?.Apply(draft);if(previewTitle)previewTitle.text=draft.title;StartCoroutine(CaptureVenue());}
  IEnumerator CaptureVenue(){
   yield return new WaitForEndOfFrame();if(Page!="venues"||!venuePreview||!Camera.main||app.PendingSelection.HasValue)yield break;
   if(!previewTexture){previewTexture=new RenderTexture(987,432,24){name="Venue preview"};previewTexture.Create();}
   if(!previewCamera){previewCamera=new GameObject("Venue preview camera").AddComponent<Camera>();previewCamera.enabled=false;}
   previewCamera.CopyFrom(Camera.main);previewCamera.enabled=false;previewCamera.orthographic=false;previewCamera.nearClipPlane=.1f;
   previewCamera.rect=new Rect(0,0,1,1);previewCamera.targetTexture=previewTexture;previewCamera.aspect=658f/288;previewCamera.ResetProjectionMatrix();
   // The overview must not inherit a gameplay camera or the previous sport's pose.
   previewCamera.transform.position=app.environments.Current.menuCamera;previewCamera.transform.LookAt(app.environments.Current.menuFocus);previewCamera.fieldOfView=60;
   FrameVenuePreview();Canvas.ForceUpdateCanvases();previewCamera.Render();venuePreview.texture=previewTexture;
  }
  void FrameVenuePreview(){
   if(venueCamera==0||!app.stadium)return;
   var view=app.stadium;
   if(view is SunvaleStadiumView football){
    if(venueCamera==1&&football.title)FrameVenueSign(football.title);
    else {
     // Use an individual banner, including while hidden, so pitch barriers
     // cannot obscure the decorative flags being toggled.
     var banner=football.accentRenderers.FirstOrDefault(r=>r&&r.name.Contains("Flags_"));
     if(banner){var focus=banner.bounds.center;var inward=Vector3.ProjectOnPlane(football.transform.position-focus,football.transform.up).normalized;
      float distance=Mathf.Max(5,banner.bounds.size.y*2);previewCamera.transform.position=focus+inward*distance;
      previewCamera.transform.LookAt(focus,football.transform.up);previewCamera.fieldOfView=36;}
    }
   }else if(view is BasketballArenaView arena){
    if(venueCamera==1&&arena.logoCatalog.Length>draft.logo&&arena.logoCatalog[draft.logo]){
     var renderers=arena.logoCatalog[draft.logo].GetComponentsInChildren<Renderer>();
     if(renderers.Length>0){var bounds=renderers[0].bounds;foreach(var r in renderers)bounds.Encapsulate(r.bounds);
      previewCamera.transform.position=bounds.center+arena.transform.TransformDirection(new Vector3(0,7,4));previewCamera.transform.LookAt(bounds.center);previewCamera.fieldOfView=36;}
    }else if(venueCamera==2){
     previewCamera.transform.position=arena.transform.TransformPoint(new Vector3(0,13,19));previewCamera.transform.LookAt(arena.transform.TransformPoint(new Vector3(0,0,3)));previewCamera.fieldOfView=62;
    }else if(venueCamera==3){var sign=arena.GetComponentsInChildren<StadiumSign>().FirstOrDefault(s=>s.nameSign&&s.label);if(sign)FrameVenueSign(sign.label);}
   }else IslandVenueStyle.Frame(previewCamera,view,venueCamera);
  }
  void FrameVenueSign(Text label){
   var canvas=label.GetComponentInParent<Canvas>();var rect=canvas?(RectTransform)canvas.transform:label.rectTransform;
   var size=Vector2.Scale(rect.rect.size,new Vector2(Mathf.Abs(rect.lossyScale.x),Mathf.Abs(rect.lossyScale.y)));
   float distance=Mathf.Max(size.y,size.x/previewCamera.aspect)*.6f/Mathf.Tan(18*Mathf.Deg2Rad);
   var focus=rect.TransformPoint(rect.rect.center);previewCamera.transform.position=focus-rect.forward*distance;
   previewCamera.transform.LookAt(focus,rect.up);previewCamera.fieldOfView=36;
  }
 }
}
