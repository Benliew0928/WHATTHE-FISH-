using UnityEngine;

namespace WhatTheFish {
 public sealed partial class StorybookMenu {
  void LayoutHub(){
   Heading("LAYOUT","Your buttons, your way. Choose a sport to arrange its controls.");
   Text(content,"SAVED FOR "+PlayerIdentity.DisplayName.ToUpperInvariant(),64,179,1472,42,25,true,Mint);
   Text(content,"Separate layouts for each sport and account on this device.",64,231,1472,40,24,false,Muted);
   for(int i=0;i<HomeSports.Length;i++){
    var sport=HomeSports[i];
    SportCard(sport,64+i*374,308,350,350,()=>ControlLayoutPreview.Open(design,sport,SportArt(sport),()=>content.gameObject),"Layout "+sport,"Arrange buttons");
   }
   Plate(content,"Layout tip",64,695,1472,117,Cream);
   Text(content,"DRAG · RESIZE · SAVE",88,712,470,40,28,true,Gold);
   Text(content,"Drag a button to move it. Use the slider to resize.\nSave when it feels right.",578,710,930,80,24,false,Muted);
   ActionButton("Back from layouts","Back to settings",64,831,370,57,Raised,Back,CoveSymbol.Settings,23);
  }
 }
}
