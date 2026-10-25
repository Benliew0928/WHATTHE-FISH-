using UnityEngine;
namespace WhatTheFish {
 // Personal presentation: every viewer sees the ball, with their own saved style.
 public static class BasketballTrailStyle {
  public const int Count=6;
  static string Preference=>PlayerEconomy.TrailKey("basketballTrail");
  public static readonly string[] Names={"Solar Drive","Court Lightning","Glacier","Rose Comet","Aurora Rush","Championship"};
  public static readonly string[] Descriptions={"Amber heat & release rings","Electric blue speed lines","Ice blue flowing ribbons","Pink starlight & bounce rings","Mint and violet double wake","Gold streaks & bright sparks"};
  static readonly int[] designs={3,0,1,2,4,5};static readonly Material[] materials=new Material[Count];
  public static int Selected {get;private set;}
  static int Sanitize(int value)=>value>=0&&value<Count?value:0;
  public static void Load()=>Selected=CosmeticStore.Allowed("basketballTrail",Sanitize(PlayerPrefs.GetInt(Preference,PlayerIdentity.AccountId.Length==0?PlayerPrefs.GetInt("basketball.localTrail",0):0)));
  public static void Select(int value){value=Sanitize(value);if(!CosmeticStore.Owned("basketballTrail",value))return;Selected=Sanitize(value);PlayerPrefs.SetInt(Preference,Selected);PlayerPrefs.Save();PlayerEconomy.SelectTrail("basketballTrail",Selected);}
  public static Material Material(int value){
   value=Sanitize(value);if(!materials[value]){var source=Resources.Load<Material>("FootballTrail");if(!source)return null;materials[value]=new Material(source){name=Names[value]+" (basketball effect)"};materials[value].SetFloat("_Style",designs[value]);materials[value].SetFloat("_Sport",1);}
   materials[value].SetFloat("_Motion",MenuPreferences.ReducedMotion?0:1);return materials[value];
  }
 }
}
