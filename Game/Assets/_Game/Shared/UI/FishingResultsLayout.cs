using UnityEngine;

namespace WhatTheFish {
 [CreateAssetMenu(menuName="WHATTHE FISH?/Fishing results layout")]
 public sealed class FishingResultsLayout:ScriptableObject {
  [Header("Landscape safe frame")]
  [Min(0)]public float margin=24,padding=26,gap=12;
  [Min(400)]public float cardWidth=940;
  [Min(24)]public float headerHeight=82,columnHeight=30,rowHeight=52,summaryHeight=154,notesHeight=66,footerHeight=62;
  public Vector2 closeSize=new(48,48),readySize=new(230,54),reopenSize=new(170,52),reopenPosition=new(0,-230);
  [Header("Native lettering")]
  [Range(16,48)]public int titleFontSize=34,rowFontSize=24,labelFontSize=21,statFontSize=23;
  [Header("Crew checklist inside clock")]
  [Min(60)]public float crewExtraHeight=88;
  public Vector2 crewSize=new(360,82);
  [Range(14,32)]public int crewFontSize=19,crewScoreFontSize=21;
 }
}
