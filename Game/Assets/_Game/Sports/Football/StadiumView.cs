using UnityEngine;
using UnityEngine.UI;

namespace WhatTheFish {
 public class StadiumView:MonoBehaviour {
  public Text title,subtitle; public virtual void Apply(StadiumAppearance a){
   a.Clamp();foreach(var r in GetComponentsInChildren<Renderer>(true)){
    if(r.name.StartsWith("Flags")){r.gameObject.SetActive(a.flags);continue;}
    foreach(var m in r.materials){if(m.name.StartsWith("Seat")||m.name.StartsWith("Trim"))m.color=LocalProfile.Teams[a.palette];}
   }
   if(title)title.text=a.title.ToUpperInvariant();if(subtitle)subtitle.text=a.screen==0?"MAKE YOURSELF AT HOME":"WELCOME, TEAM "+new[]{"MINT","ROSE","LILAC","GOLD"}[a.palette];
   foreach(var sign in GetComponentsInChildren<StadiumSign>())sign.Apply(a);
  }
 }
}
