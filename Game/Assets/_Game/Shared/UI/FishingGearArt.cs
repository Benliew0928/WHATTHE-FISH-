using UnityEngine;
namespace WhatTheFish {
 // One small generated icon atlas and one tutorial atlas. Existing Sonar/Reel
 // illustrations are shared with sports/fishing; no duplicate delivery texture.
 public static class FishingGearArt {
  static Sprite[] icons,pages;
  static Sprite Cell(Texture2D texture,int column,int row,int columns,int rows){float w=texture.width/(float)columns,h=texture.height/(float)rows;return Sprite.Create(texture,new Rect(column*w,(rows-row-1)*h,w,h),Vector2.one*.5f,100,0,SpriteMeshType.FullRect);}
  public static Sprite Icon(int index){if(icons==null){var texture=Resources.Load<Texture2D>("FishingUI/GearIcons");icons=new Sprite[6];if(texture)for(int i=0;i<6;i++)icons[i]=Cell(texture,i%3,i/3,3,2);}return icons[Mathf.Clamp(index,0,5)];}
  public static Sprite Ability(FishingAbility ability)=>ability switch {FishingAbility.Sonar=>SportsUIStyle.PropIcon(SportsProp.LockOn),FishingAbility.SteelWire=>Icon(2),FishingAbility.FishingBuddy=>Icon(3),FishingAbility.PowerReel=>Resources.Load<Sprite>("FishingUI/ReelButton"),FishingAbility.BubbleGuard=>Icon(4),FishingAbility.SchoolCall=>Icon(5),_=>Icon(0)};
  public static Sprite Tutorial(int index){if(pages==null){var texture=Resources.Load<Texture2D>("FishingUI/TutorialPanels");pages=new Sprite[4];if(texture)for(int i=0;i<4;i++)pages[i]=Cell(texture,i%2,i/2,2,2);}return pages[Mathf.Clamp(index,0,3)];}
 }
}
