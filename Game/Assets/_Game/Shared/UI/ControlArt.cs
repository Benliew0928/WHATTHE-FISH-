using System;
using System.Collections.Generic;
using UnityEngine;

namespace WhatTheFish {
 // Normalized source rectangles survive platform texture resizing.
 public static class ControlArt {
  [Serializable] sealed class Atlas { public Entry[] entries; }
  [Serializable] sealed class Entry { public string name,sheet;public float x,y,width,height; }
  static readonly Dictionary<string,Sprite> sprites=new();
  static bool loaded;
  public static Sprite Get(string name){
   if(!loaded){
    loaded=true;
    var atlas=JsonUtility.FromJson<Atlas>(Resources.Load<TextAsset>("ControlArt/Atlas").text);
    var textures=new Dictionary<string,Texture2D>();
    foreach(var e in atlas.entries){
     if(!textures.TryGetValue(e.sheet,out var t))textures[e.sheet]=t=Resources.Load<Texture2D>("ControlArt/"+e.sheet);
     if(!t)throw new InvalidOperationException("Missing shared control atlas: "+e.sheet);
     var sprite=Sprite.Create(t,new Rect(e.x*t.width,e.y*t.height,e.width*t.width,e.height*t.height),Vector2.one*.5f,100,0,SpriteMeshType.FullRect);
     sprite.name="ControlArt/"+e.name;sprites[e.name]=sprite;
    }
   }
   return sprites.TryGetValue(name,out var result)?result:null;
  }
  public static Sprite Prop(SportsProp prop)=>Get(prop switch{SportsProp.LockOn=>"LockOn",SportsProp.Turbo=>"Turbo",SportsProp.Portal=>"Portal",SportsProp.Wall=>"Wall",_=>"Turbo"});
  public static Sprite Skill(FishingAbility skill)=>Get(skill switch{FishingAbility.SteelWire=>"SteelWire",FishingAbility.PowerReel=>"PowerReel",FishingAbility.FishingBuddy=>"FishingBuddy",FishingAbility.BubbleGuard=>"BubbleGuard",FishingAbility.SchoolCall=>"SchoolCall",_=>"Sonar"});
 }
}
