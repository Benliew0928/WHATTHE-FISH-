using UnityEngine;
using UnityEngine.UI;

namespace WhatTheFish {
 public sealed partial class StartupWelcome {
  void OpenWelcomeInfo(){
   var card=UtilityPanel("Welcome info","Terms & privacy","Game information and useful links.",InfoButton,1040,760,WelcomeSymbol.Info);
   if(!card)return;
   InfoLink(card,"terms","Terms of use","Accounts, fair play and virtual items.",156);
   InfoLink(card,"privacy","Privacy policy","Your data, choices and account deletion.",290);
   var website=InfoLink(card,"website","Website","Game news, support and full documents.",424);
   website.interactable=PublicInfoDocuments.UrlFor("website").Length>0;
   Label(card,website.interactable?"Links open in your browser.":"Public website link is not available yet.",36,565,968,46,23,false,Ink,TextAnchor.MiddleCenter);
   if(!website.interactable)Label(card,"Terms and privacy open as offline drafts until publication.",36,611,968,46,21,false,Ink,TextAnchor.MiddleCenter);
   var version=Label(card,"Version "+Application.version,34,695,398,30,20,false,Ink);version.name="Game version";
   ActionButton(card,"Done welcome info","Done",810,684,198,52,"C9FFF0","82DDC4","57A996",CloseModal,WelcomeSymbol.None,26);
  }
  Button InfoLink(RectTransform card,string id,string title,string caption,float y){
   var button=ActionButton(card,"Welcome link "+id,title,32,y,976,80,"FFFFFF","E2F5DE","89BEB0",()=>OpenInfoDocument(id),WelcomeSymbol.None,31);
   Label(card,caption,52,y+84,936,34,23,false,Ink,TextAnchor.MiddleCenter);
   return button;
  }
  void OpenInfoDocument(string id){
   string url=PublicInfoDocuments.UrlFor(id);
   if(url.Length>0){Application.OpenURL(url);return;}
   if(id=="website")return;
   CloseModal();
   var document=PublicInfoDocuments.Get(id);
   var card=UtilityPanel("Welcome document",document?.title??"Terms & privacy","Preparation draft · not yet published",InfoButton,1140,836,WelcomeSymbol.Info);
   if(!card)return;
   var viewport=Rect(card,"Document viewport",32,154,1076,552);
   var hit=viewport.gameObject.AddComponent<Image>();hit.color=new Color(1,1,1,.35f);
   viewport.gameObject.AddComponent<RectMask2D>();
   var body=Rect(viewport,"Document content",0,0,1042,552);
   var scroll=viewport.gameObject.AddComponent<ScrollRect>();scroll.viewport=viewport;scroll.content=body;
   scroll.horizontal=false;scroll.vertical=true;scroll.movementType=ScrollRect.MovementType.Clamped;scroll.scrollSensitivity=45;
   float y=16;
   void Paragraph(string value,bool heading=false){
    var label=Label(body,value,18,y,1006,70,heading?29:25,heading,Ink);
    if(label is LocalizedText localized)localized.ManualFit=true;
    label.resizeTextForBestFit=false;label.supportRichText=false;
    label.alignment=TextAnchor.UpperLeft;
    label.horizontalOverflow=HorizontalWrapMode.Wrap;label.verticalOverflow=VerticalWrapMode.Overflow;
    float height=Mathf.Ceil(label is LocalizedText measured?measured.MeasureWrappedHeight():label.preferredHeight)+12;
    label.rectTransform.sizeDelta=new Vector2(1006,height);y+=height+(heading?7:24);
   }
   Paragraph("These pages are for review while the game is being developed. They are not effective terms or a final privacy notice. Unconfirmed details are identified below.");
   if(document==null)Paragraph("This document is not available in this build.");
   else{Paragraph(document.intro);foreach(var section in document.sections){Paragraph(section.heading,true);Paragraph(section.body);}}
   body.sizeDelta=new Vector2(1042,Mathf.Max(552,y));
   var track=Rect(viewport,"Document scrollbar",1050,8,18,536);var background=track.gameObject.AddComponent<Image>();background.color=LocalProfile.Hex("C5DFD4");
   var handle=Rect(track,"Handle",0,0,18,70);var handleImage=handle.gameObject.AddComponent<Image>();handleImage.color=LocalProfile.Hex("278B7C");
   var bar=track.gameObject.AddComponent<Scrollbar>();bar.handleRect=handle;bar.targetGraphic=handleImage;bar.direction=Scrollbar.Direction.BottomToTop;
   scroll.verticalScrollbar=bar;scroll.verticalScrollbarVisibility=ScrollRect.ScrollbarVisibility.Permanent;
   Canvas.ForceUpdateCanvases();scroll.verticalNormalizedPosition=1;
   Label(card,"Scroll to read the full draft.",36,724,1068,35,21,false,Ink,TextAnchor.MiddleCenter);
   ActionButton(card,"Back to welcome info","Back to info",32,773,340,48,"FFFFFF","E2F5DE","89BEB0",()=>{CloseModal();OpenWelcomeInfo();},WelcomeSymbol.None,25);
   ActionButton(card,"Done welcome document","Done",910,773,198,48,"C9FFF0","82DDC4","57A996",CloseModal,WelcomeSymbol.None,25);
  }
 }
}
