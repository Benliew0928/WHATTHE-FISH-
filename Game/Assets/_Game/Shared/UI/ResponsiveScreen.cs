using UnityEngine;

namespace WhatTheFish {
 // Window measurements, never a device/model whitelist. Insets can change while
 // the app is running (rotation, cutouts, system bars and desktop window resizing).
 public static class ResponsiveScreen {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
  public static Rect? ReviewSafeArea;
#endif
  public static Rect SafeArea {
   get {
    var area=Screen.safeArea;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
    if(ReviewSafeArea.HasValue)area=ReviewSafeArea.Value;
#endif
    return Validate(area,new Vector2(Screen.width,Screen.height));
   }
  }
  public static Rect Validate(Rect area,Vector2 size){
   if(float.IsNaN(area.x+area.y+area.width+area.height)||float.IsInfinity(area.x+area.y+area.width+area.height)||area.width<=0||area.height<=0)return new Rect(Vector2.zero,size);
   var clipped=Rect.MinMaxRect(Mathf.Clamp(area.xMin,0,size.x),Mathf.Clamp(area.yMin,0,size.y),Mathf.Clamp(area.xMax,0,size.x),Mathf.Clamp(area.yMax,0,size.y));
   return clipped.width>0&&clipped.height>0?clipped:new Rect(Vector2.zero,size);
  }
  public static Rect Centered(Rect area,Vector2 size){
   area=Validate(area,size);
   float x=Mathf.Max(area.xMin,size.x-area.xMax),y=Mathf.Max(area.yMin,size.y-area.yMax);
   // Some freeform windows have a safe region wholly on one side of centre.
   // Respect that region instead of manufacturing a zero/negative rectangle.
   return size.x>2*x&&size.y>2*y?new Rect(x,y,size.x-2*x,size.y-2*y):area;
  }
  public static Rect ContentArea=>Centered(SafeArea,new Vector2(Screen.width,Screen.height));
 }
}
