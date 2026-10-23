using UnityEngine;
using UnityEngine.UI;
namespace WhatTheFish {
 public static partial class ControlDesigns {
  static void Court(View v,SportId sport,string id){
   bool football=sport==SportId.Football;
   if(id=="move"){SharedStick(v,sport);return;}
   if(id=="ability"){
    Shell(v,"Ability");
    ActionArt(v,football?"Wall":"Turbo",football?"WALL":"TURBO",new(54,47),new(0,10),14,new(0,-24),23);
    var badge=Picture(v.Visual,"One use","RoundGold",Vector2.one*23,new(29,29));
    v.Badge=GlossLabel(badge.transform,"1",new(19,23),Vector2.zero,15);
    v.Progress=Center(v.Visual,"Ability progress",Vector2.one*80).gameObject.AddComponent<BasketballPropProgress>();v.Progress.raycastTarget=false;v.Progress.color=CoveUI.Gold;return;
   }
   if(!football){
    Shell(v,id=="shoot"?"RoundGold":id=="pass"?"TileCyan":"TileGold");
    if(id=="shoot"){ActionArt(v,"Basketball","SHOOT",new(69,61),new(0,17),22,new(0,-25),30);v.Hint=Hint(v,"HOLD",new(0,-41),80,9);}
    else if(id=="pass")ActionArt(v,"BasketPass","PASS",new(57,46),new(0,11),18,new(0,-24),24);
    else ActionArt(v,"Jump","JUMP",new(44,37),new(0,9),14,new(0,-19),21);
    return;
   }
   Shell(v,id=="shoot"?"TileGold":id=="dash"?"RoundLime":"TileCyan");
   if(id=="shoot"){ActionArt(v,"Football","SHOOT",new(46,43),new(0,13),17,new(0,-17),23);v.Hint=Hint(v,"HOLD",new(0,-30),68,8);}
   if(id=="pass"){ActionArt(v,"FootballPass","PASS",new(43,37),new(0,11),15,new(0,-21),21);v.Hint=Hint(v,"",new(0,-28),57,7);v.Hint.enabled=false;}
   if(id=="tackle"){ActionArt(v,"Tackle","TACKLE",new(57,46),new(0,10),16,new(0,-23),23);v.Hint=Hint(v,"",new(0,-31),64,8);v.Hint.enabled=false;}
   if(id=="dash"){
    ActionArt(v,"Dash","DASH",new(69,62),new(0,17),22,new(0,-22),28);
    var bar=Plate(Center(v.Visual,"Your football stamina",new(72,7),new(0,-40)),ButtonInk,3.5f,0);bar.inset=0;bar.shadow=0;
    v.Meter=BasketballUIStyle.Plate(bar.transform,"Stamina fill",1.5f,1.5f,69,4,CoveUI.Hex("B6FF35"),2,0);
    v.Hint=Hint(v,"HOLD / SWIPE ACTIONS",new(0,-64),146,8);
   }
  }
 }
}
