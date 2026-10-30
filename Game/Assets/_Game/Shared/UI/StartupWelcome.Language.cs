using UnityEngine;
using UnityEngine.UI;

namespace WhatTheFish {
 public sealed partial class StartupWelcome {
  Text languageCaption;GameLanguage artworkLanguage;bool languageArtworkInitialized,artworkReady,artworkError;
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
  void CreateLanguageArtwork(){
   languageCaption=LanguageButton.GetComponentInChildren<Text>();GameLocalization.UserContent(languageCaption);
  }
  void RefreshLanguageArtwork(){
   if(!readyTitle)return;var language=GameLocalization.Current;if(languageArtworkInitialized&&language==artworkLanguage&&IsReady==artworkReady&&HasError==artworkError)return;
   languageArtworkInitialized=true;artworkLanguage=language;artworkReady=IsReady;artworkError=HasError;
   readyTitle.gameObject.SetActive(IsReady&&!HasError);blankSign.SetActive(!IsReady||HasError);
   languageCaption.text=GameLocalization.Current==GameLanguage.SimplifiedChinese?"简体中文":GameLocalization.Current==GameLanguage.Malay?"Bahasa Melayu":"English";
  }
 }
}
