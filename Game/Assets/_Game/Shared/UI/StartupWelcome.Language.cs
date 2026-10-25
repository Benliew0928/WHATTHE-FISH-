using UnityEngine;
using UnityEngine.UI;

namespace WhatTheFish {
 public sealed partial class StartupWelcome {
  Text languageCaption;string languageArtworkState;
  void OpenLanguagePicker(){
   if(ModalOpen)return;returnFocus=LanguageButton;CreateModal();
   var card=Plate(modal,"Language picker",466,170,740,600,"FFFAEB","F5E9CF","76AA9C",32,true);
   Label(card.transform,"Language",32,20,676,64,35,true,Ink,TextAnchor.MiddleCenter);
   Label(card.transform,"Choose your language. Changes apply throughout the game.",40,88,660,66,24,false,Ink,TextAnchor.MiddleCenter);
   var names=new[]{"English","简体中文","Bahasa Melayu"};
   for(int i=0;i<names.Length;i++){
    var language=(GameLanguage)i;
    var button=ActionButton(card.transform,"Language option "+i,names[i],60,174+i*92,620,74,"C9FFF0","82DDC4","57A996",()=>{GameLocalization.Select(language);CloseModal();},WelcomeSymbol.None,29);
    GameLocalization.UserContent(button.GetComponentInChildren<Text>());
   }
   var back=ActionButton(card.transform,"Close language picker","Back",225,484,290,70,"FFFAEB","F5E9CF","76AA9C",CloseModal,WelcomeSymbol.None,26);
   SetBaseInteractable(false);back.Select();
  }
  LocalizedArtwork welcomeHeading;
  void CreateLanguageArtwork(){
   var art=Design.Find("Cable-car welcome artwork").GetComponent<RawImage>();
   LocalizedArtwork Caption(string text,float x,float y,float w,float h,int size,Color ink){
    var caption=LocalizedArtwork.Add(art,new UnityEngine.Rect(x/Width,1-(y+h)/Height,w/Width,h/Height),text,size,Color.clear,ink);caption.Plain();return caption;
   }
   welcomeHeading=Caption("Ready to play",1238,345,292,62,38,Ink);
   welcomeHeading.SetCaptionArea(new UnityEngine.Rect(1205/Width,1-404/Height,358/Width,58/Height));
   if(!(AndroidAccountSignIn.Supported&&AndroidAccountSignIn.Capabilities.Preferred==AccountProvider.Google))
    Caption("Sign in with Huawei",1260,467,330,48,29,LocalProfile.Hex("170F00"));
   Caption("Continue as guest",1285,578,280,50,29,Ink).SetCaptionArea(new UnityEngine.Rect(1258/Width,1-628/Height,334/Width,50/Height));
   var languageArt=Caption("English",1267,125,97,31,24,Ink);languageArt.SetCaptionArea(new UnityEngine.Rect(1243/Width,1-156/Height,145/Width,31/Height));languageCaption=languageArt.Caption;GameLocalization.UserContent(languageCaption);
   Caption("Settings",1404,125,104,31,24,Ink);
   Caption("Info",1561,125,59,31,24,Ink).SetCaptionArea(new UnityEngine.Rect(1522/Width,1-156/Height,137/Width,31/Height));
  }
  void RefreshLanguageArtwork(){
   if(!welcomeHeading)return;string state=GameLocalization.Code+IsReady+HasError;if(state==languageArtworkState)return;languageArtworkState=state;
   welcomeHeading.enabled=IsReady&&!HasError;
   blankSign.SetActive(!IsReady||HasError);
   languageCaption.text=GameLocalization.Current==GameLanguage.SimplifiedChinese?"简体中文":GameLocalization.Current==GameLanguage.Malay?"Bahasa Melayu":"English";
  }
 }
}
