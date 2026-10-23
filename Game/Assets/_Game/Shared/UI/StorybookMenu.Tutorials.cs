using UnityEngine;
using static WhatTheFish.CoveUI;
namespace WhatTheFish {
 public sealed partial class StorybookMenu {
  void TutorialHub(){
   Heading("TUTORIALS","Learn one control at a time. Leave whenever you like.");
   for(int i=0;i<HomeSports.Length;i++){
    var sport=HomeSports[i];float x=64+i*374;
    SportCard(sport,x,260,350,360,()=>TutorialSession.Launch(sport),"Learn "+sport,TutorialProgress.Caption(sport),!app.rooms.Connected);
    Button(content,"Replay tutorial "+sport,"Start over",x,646,350,56,Raised,()=>TutorialSession.Launch(sport,true),CoveSymbol.Back,22).interactable=!app.rooms.Connected;
   }
   Button(content,"Tutorial reference","Controls & rules",64,779,350,65,Raised,()=>Go("guides"),CoveSymbol.Book,24);
   if(app.rooms.Connected)Footer("Leave your room to begin a private tutorial.");
  }
 }
}
