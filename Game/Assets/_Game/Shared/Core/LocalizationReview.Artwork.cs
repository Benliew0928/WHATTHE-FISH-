#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
namespace WhatTheFish {
 public sealed partial class LocalizationReview {
  IEnumerator ArtworkGallery(){
   string[] paths={"StartButton/StartGame","CartButtons/Buggy","CartButtons/Recall","CartButtons/Drive","Badges/Exit","Badges/YourBall","Leaderboard/Title","Leaderboard/Columns","FinalCountdown/Plate","Leaderboard/Playing","Leaderboard/Finished","Events/BadgeSheet","Events/BadgeSheet","Events/BadgeSheet","Events/BadgeSheet","Events/TornadoBadge","Events/CrabBadge","Events/WarningSheet"};
   string[] words={"Start game","Buggy","Recall","Drive","Exit","Your ball","Leaderboard","","FINISH IN","Playing","Finished","TROPICAL WIND","STICKY GREEN","BOUNCE PARTY","ISLAND RUSH","TORNADO","CRAB CROSSING","EVENT INCOMING"};
   Rect[] areas={new(.115f,.14f,.82f,.65f),new(.47f,.31f,.42f,.43f),new(.465f,.27f,.43f,.42f),new(.455f,.22f,.435f,.49f),new(.50f,.25f,.36f,.45f),new(.345f,.30f,.54f,.36f),new(.135f,.12f,.77f,.47f),new(),new(.48f,.54f,.37f,.24f),new(.13f,.23f,.74f,.52f),new(.13f,.23f,.74f,.52f),new(.40f,.23f,.32f,.55f),new(.26f,.23f,.46f,.56f),new(.28f,.12f,.42f,.65f),new(.35f,.22f,.41f,.56f),new(.265f,.15f,.465f,.44f),new(.32f,.14f,.40f,.63f),new(.385f,.61f,.46f,.22f)};
   for(int batch=0;batch<2;batch++){
    var go=new GameObject("Artwork inspection",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler));var canvas=go.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=32000;
    var scaler=go.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1600,900);scaler.matchWidthOrHeight=1;
    var background=CoveUI.Plate(go.transform,"Inspection background",0,0,1600,900,CoveUI.Hex("18374B"),0,0);CoveUI.Stretch(background.rectTransform);
    for(int cell=0;cell<9;cell++){
     int i=batch*9+cell;var frame=CoveUI.Rect(go.transform,"Artwork "+paths[i],24+cell%3*525,20+cell/3*290,500,270);
     var heading=CoveUI.Text(frame,paths[i]+" / "+words[i],0,0,500,32,18,false,Color.white);GameLocalization.UserContent(heading);
     var rect=CoveUI.Rect(frame,"Original graphic",0,48,500,196);Graphic art;
     if(i<11){var img=rect.gameObject.AddComponent<Image>();img.sprite=Resources.Load<Sprite>("GolfUI/"+paths[i]);img.preserveAspect=true;art=img;}
     else {var badge=rect.gameObject.AddComponent<GolfEventBadgeGraphic>();badge.Sheet=Resources.Load<Texture2D>("GolfUI/"+paths[i]);badge.Badge=i==11?0:i==12?2:i==13?3:i==14?4:i==15?6:i==16?7:5;var crop=GolfEventBadgeGraphic.Crop(badge.Badge);rect.sizeDelta=new Vector2(500,500*crop.height/crop.width);art=badge;}
     if(i==7){float[] edges={.09f,.30f,.47f,.63f,.80f,.98f};string[] captions={"Player","Progress","Strokes","Status","Finish time"};for(int k=0;k<5;k++)LocalizedArtwork.Add(art,new Rect(edges[k],.2f,edges[k+1]-edges[k]-(k==4?.001f:.01f),.60f),captions[k],23,Color.clear,GolfHUDStyle.Ink);}
     else {Color color=i==0||i==9||i==10||i>=11&&i<=14?Color.white:i==6||i==15||i==16?GolfHUDStyle.Gold:GolfHUDStyle.Ink;LocalizedArtwork.Add(art,areas[i],words[i],30,Color.clear,color);}
     if(i==17)LocalizedArtwork.Add(art,new Rect(.46f,.08f,.30f,.17f),"Get ready!",18,Color.clear,Color.white);
    }
    foreach(GameLanguage language in System.Enum.GetValues(typeof(GameLanguage))){GameLocalization.Select(language);yield return new WaitForSecondsRealtime(.2f);yield return Capture(GameLocalization.Code+"-artwork-"+batch);}
    Destroy(go);yield return null;
   }
  }
 }
}
#endif
