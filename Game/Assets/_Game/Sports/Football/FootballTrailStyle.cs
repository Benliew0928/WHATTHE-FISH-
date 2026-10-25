using UnityEngine;
namespace WhatTheFish {
 // A viewing preference, deliberately absent from player/ball network snapshots.
 public static class FootballTrailStyle {
  public const int Count=5;
  static string Preference=>PlayerEconomy.TrailKey("footballTrail");
  public static readonly string[] Names={"Electric Bolt","Ocean Blue","Rose Sparkle","Solar Flame","Aurora"};
  public static readonly string[] Descriptions={"Forked lightning","Flowing blue ribbons","Pink starlight","Golden flame wisps","Mint & violet waves"};
  static readonly Material[] materials=new Material[Count];
  public static int Selected {get;private set;}
  public static void Load()=>Selected=CosmeticStore.Allowed("footballTrail",Sanitize(PlayerPrefs.GetInt(Preference,PlayerIdentity.AccountId.Length==0?PlayerPrefs.GetInt("football.localTrail",0):0)));
  static int Sanitize(int style)=>style>=0&&style<Count?style:0;
  public static void Select(int style){style=Sanitize(style);if(!CosmeticStore.Owned("footballTrail",style))return;Selected=Sanitize(style);PlayerPrefs.SetInt(Preference,Selected);PlayerPrefs.Save();PlayerEconomy.SelectTrail("footballTrail",Selected);}
  public static Material Material(int style){
   style=Sanitize(style);
   if(!materials[style]){var source=Resources.Load<Material>("FootballTrail");if(!source)return null;materials[style]=new Material(source){name=Names[style]+" (shared local effect)"};materials[style].SetFloat("_Style",style);}
   materials[style].SetFloat("_Motion",MenuPreferences.ReducedMotion?0:1);
   return materials[style];
  }
 }
}
