using UnityEngine;

namespace WhatTheFish {
 // Preserve the shared atlas and the stadium's authored seating zones.
 public sealed class SunvaleStadiumView:StadiumView {
  public GameObject[] flagModules;
  public Renderer[] accentRenderers;
  MaterialPropertyBlock accents;
  public override void Apply(StadiumAppearance appearance){
   appearance.Clamp();
   foreach(var module in flagModules)if(module)module.SetActive(appearance.flags);
   if(accents==null)accents=new MaterialPropertyBlock();
   accents.SetColor("_BaseColor",Color.Lerp(Color.white,LocalProfile.Teams[appearance.palette],.35f));
   foreach(var renderer in accentRenderers)if(renderer)renderer.SetPropertyBlock(accents);
   if(title)title.text=appearance.title.ToUpperInvariant();
   if(subtitle){
    subtitle.text=appearance.screen==0?new[]{"PLAY TOGETHER  •  SUNNY DAYS","ONE TEAM  /  GOOD ENERGY","MOVE • PLAY • REPEAT"}[appearance.design]:"WELCOME, TEAM "+new[]{"MINT","ROSE","LILAC","GOLD"}[appearance.palette];
    subtitle.color=LocalProfile.Teams[appearance.palette];
   }
   foreach(var sign in GetComponentsInChildren<StadiumSign>(true))sign.Apply(appearance);
  }
 }
}
