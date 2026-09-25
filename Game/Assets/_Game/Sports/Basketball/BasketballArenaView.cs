using UnityEngine;
namespace WhatTheFish {
 public sealed class BasketballArenaView:StadiumView {
  public GameObject[] logoCatalog;
  public bool modularRally;
  MaterialPropertyBlock accents;
  public static readonly string[] LogoNames={"Rally crest","Star","Lightning bolt","Shield"};
  public override void Apply(StadiumAppearance appearance){
   appearance.Clamp();
   if(modularRally)ApplyRallyPalette(appearance.palette);
   else foreach(var renderer in GetComponentsInChildren<Renderer>(true))foreach(var material in renderer.materials){
    if(material.name.StartsWith("BBTrim")||material.name.StartsWith("BBLogo"))material.color=LocalProfile.Teams[appearance.palette];
    if(material.name.StartsWith("BBSeat"))material.color=Color.Lerp(LocalProfile.Teams[appearance.palette],new Color(.025f,.045f,.06f),.65f);
   }
   for(int i=0;i<logoCatalog.Length;i++)logoCatalog[i].SetActive(i==appearance.logo);
   foreach(var sign in GetComponentsInChildren<StadiumSign>(true))sign.Apply(appearance);
   if(title)title.text=appearance.title.ToUpperInvariant();
  }
  void ApplyRallyPalette(int palette){
   if(accents==null)accents=new MaterialPropertyBlock();
   foreach(var renderer in GetComponentsInChildren<Renderer>(true)){
    var materials=renderer.sharedMaterials;
    for(int i=0;i<materials.Length;i++){
     var m=materials[i];if(!m)continue;var n=m.name;
     bool seat=n.StartsWith("Rally_LowerSeating_Seats_")||n.StartsWith("Rally_UpperSeating_Seats_");
     bool trim=n=="Rally_Court_Apron"||n=="Rally_Court_KeyPaint"||n=="Rally_Hoop_Padding"||n=="Rally_CenterEmblem_Disk"||n=="Rally_PerimeterPads_Cushion_Teal";
     if(!seat&&!trim)continue;
     Color original=m.GetColor("_BaseColor");
     Color color=palette==0?original:seat?Color.Lerp(original,LocalProfile.Teams[palette],.55f):LocalProfile.Teams[palette];
     renderer.GetPropertyBlock(accents,i);accents.SetColor("_BaseColor",color);renderer.SetPropertyBlock(accents,i);accents.Clear();
    }
   }
  }
 }
}
