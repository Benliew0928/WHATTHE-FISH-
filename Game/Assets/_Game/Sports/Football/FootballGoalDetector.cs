using UnityEngine;
namespace WhatTheFish {
 // Coordinates in the authored pitch frame; outward sign points into the net.
 // This swept detector tracks a legal field-to-net passage, not a trigger overlap.
 public sealed class FootballGoalDetector {
  public Bounds Opening {get;} public float Line {get;} public int Sign {get;}
  bool armed,passage;Vector3 previous;double previousTime;bool sampled;
  public FootballGoalDetector(Bounds opening,float line,int sign){Opening=opening;Line=line;Sign=sign;}
  public void Reset(Vector3 point,double now){previous=point;previousTime=now;sampled=true;armed=false;passage=false;}
  bool Fits(Vector3 p,float rx,float ry)=>p.x-rx>=Opening.min.x&&p.x+rx<=Opening.max.x&&p.y-ry>=Opening.min.y-.04f&&p.y+ry<=Opening.max.y;
  public bool Sample(Vector3 point,Vector3 radius,double now,out double crossedAt){
   crossedAt=now;if(!sampled){Reset(point,now);return false;}
   float a=(previous.z-Line)*Sign,b=(point.z-Line)*Sign,r=radius.z;bool goal=false;
   if(a<=-r){armed=true;passage=false;}
   if(armed&&b>a){
    if(a<=-r&&b>-r){var entrance=Vector3.Lerp(previous,point,Mathf.Clamp01((-r-a)/(b-a)));passage=Fits(entrance,radius.x,radius.y);if(!passage)armed=false;}
    if(passage){
     float t0=Mathf.Clamp01((-r-a)/(b-a)),t1=Mathf.Clamp01((r-a)/(b-a));
     if(!Fits(Vector3.Lerp(previous,point,t0),radius.x,radius.y)||!Fits(Vector3.Lerp(previous,point,t1),radius.x,radius.y)){armed=false;passage=false;}
     else if(b>r&&a<=r){crossedAt=previousTime+(now-previousTime)*t1;goal=true;armed=false;passage=false;}
    }
   }else if(b<=-r){armed=true;passage=false;}
   else if(passage&&!Fits(point,radius.x,radius.y)){armed=false;passage=false;}
   previous=point;previousTime=now;return goal;
  }
 }
}
