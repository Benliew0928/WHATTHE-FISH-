using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

namespace WhatTheFish {
 // Reserve the local-player suffix and truncate names at Unicode text elements.
 internal static class IslandResultName {
  public static void Fit(Text label,string name,string prefix,string suffix,int maximum,int minimum){
   name??="";label.horizontalOverflow=HorizontalWrapMode.Overflow;label.verticalOverflow=VerticalWrapMode.Truncate;
   var settings=label.GetGenerationSettings(label.rectTransform.rect.size);settings.resizeTextForBestFit=false;settings.fontSize=maximum;
   float Width(string value)=>label.cachedTextGeneratorForLayout.GetPreferredWidth(value,settings)/Mathf.Max(.01f,label.pixelsPerUnit);
   float available=Mathf.Max(1,label.rectTransform.rect.width-4);string full=prefix+name+suffix;
   label.fontSize=Mathf.Clamp(Mathf.FloorToInt(maximum*available/Mathf.Max(available,Width(full))),minimum,maximum);settings.fontSize=label.fontSize;
   if(Width(full)>available){var elements=StringInfo.ParseCombiningCharacters(name);int count=elements.Length;
    while(count>0&&Width(prefix+name.Substring(0,count==elements.Length?name.Length:elements[count])+"…"+suffix)>available)count--;
    full=prefix+name.Substring(0,count==elements.Length?name.Length:elements[count])+"…"+suffix;
   }
   label.text=full;
  }
 }
}
