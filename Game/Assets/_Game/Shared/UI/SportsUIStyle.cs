using UnityEngine;

namespace WhatTheFish {
 // Both sports share one geometric font family. The menu/world fonts stay separate.
 public static class SportsUIStyle {
  static Font textFont,displayFont;static Sprite[] props;
  public static Font TextFont=>textFont?textFont:textFont=Resources.Load<Font>("SportsUI/Text");
  public static Font DisplayFont=>displayFont?displayFont:displayFont=Resources.Load<Font>("SportsUI/Display");
  // Full-colour prop illustrations share an atlas with the tintable selection check.
  public static Sprite PropIcon(SportsProp kind){
   if(kind==SportsProp.None)return null;
   return AtlasIcon((int)kind-1);
  }
  public static Sprite Check=>AtlasIcon(3);
  static Sprite AtlasIcon(int index){
   if(props==null){var t=Resources.Load<Texture2D>("SportsUI/Props");props=new Sprite[4];if(t)for(int i=0;i<4;i++)props[i]=Sprite.Create(t,new Rect(i*128+4,4,120,120),Vector2.one*.5f,100,0,SpriteMeshType.FullRect);}
   return props[index];
  }
 }
}
