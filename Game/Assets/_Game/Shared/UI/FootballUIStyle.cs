using System;
using UnityEngine;
using UnityEngine.UI;
namespace WhatTheFish {
 public enum FootballIcon { Ball,Dash,Pressure,Tackle,Swipe,Target,Flag,Team }
 public static class FootballUIStyle {
  static Sprite[] sprites;
  public static readonly Color Cancel=CoveUI.Hex("FFE2D5"),CancelArmed=CoveUI.Hex("FFC5AE"),Disabled=CoveUI.Hex("D5DBCE");
  public static Sprite Icon(FootballIcon icon){
   if(sprites==null){
    var t=Resources.Load<Texture2D>("FootballUI/Icons");sprites=new Sprite[8];
    if(t)for(int i=0;i<8;i++){
     var rect=new Rect(i%4*128+4,256-(i/4+1)*128+4,120,120);
     // The horizontal tackle pose needs a wide sprite: remove only transparent
     // rows from its cell so the player stays readable above the button label.
     if(i==(int)FootballIcon.Tackle){rect.y+=24;rect.height=76;}
     sprites[i]=Sprite.Create(t,rect,Vector2.one*.5f,100,0,SpriteMeshType.FullRect);
    }
   }
   return sprites[(int)icon];
  }
  public static Image Art(Transform p,FootballIcon icon,float x,float y,float w,float h)=>CoveUI.Image(p,icon.ToString(),Icon(icon),x,y,w,h);
  public static Button Button(Transform p,string name,string caption,float x,float y,float w,float h,Action action,Color? fill=null,int size=14){
   var g=BasketballUIStyle.Plate(p,name,x,y,w,h,fill??CoveUI.Cream,13,1.5f,true);g.raycastTarget=true;
   var b=g.gameObject.AddComponent<TouchButton>();b.targetGraphic=g;b.transition=Selectable.Transition.None;b.navigation=new Navigation{mode=Navigation.Mode.None};b.onClick.AddListener(()=>{if(!BasketballButtonFeedback.UsesPressSound(b.gameObject))FootballAudio.PlayMenu(FootballAudio.ButtonCue(name));action?.Invoke();});
   BasketballUIStyle.Text(g.transform,caption,8,0,w-16,h,size,true,TextAnchor.MiddleCenter);b.gameObject.AddComponent<BasketballButtonFeedback>();MenuDesigns.Apply(b,h>=75?MenuSurface.Card:MenuSurface.Button);return b;
  }
 }
 public sealed class FootballJoystickFace:MaskableGraphic {protected override void OnPopulateMesh(VertexHelper v){v.Clear();BasketballUIMesh.Dot(v,Vector2.zero,52.5f,new Color(1,.976f,.914f,.16f));BasketballUIMesh.Arc(v,52.5f,0,360,new Color(1,.976f,.914f,.6f),3);}}
}
