using UnityEngine;

namespace WhatTheFish {
 // Separate from GolfHole's existing serialized map record.
 public sealed class GolfHoleTrigger:MonoBehaviour {
  public GolfHole Hole {get;private set;}
  GolfBallPhysicsSettings settings;
  public void Bind(GolfMatchManager owner,GolfHole hole){
   Hole=hole;settings=Resources.Load<GolfBallPhysicsSettings>("GolfBallPhysics");if(!settings)settings=ScriptableObject.CreateInstance<GolfBallPhysicsSettings>();var trigger=GetComponent<BoxCollider>();
   // Unity's missing-component placeholder in the Editor is not a C# null.
   if(!trigger)trigger=gameObject.AddComponent<BoxCollider>();
   trigger.isTrigger=true;
   float low=-hole.cupDepth-GolfBall.Radius,high=CaptureHeight;
   trigger.center=Vector3.up*((low+high)*.5f);
   trigger.size=new Vector3(CaptureRadius*2,high-low,CaptureRadius*2);
  }
  // The centre must cross the visible opening at turf height. The capture
  // speed gate on the ball keeps very powerful crossings in play.
  public float CaptureRadius=>GolfCupRules.Clearance(Hole.cupRadius,settings.cupRimForgiveness);
  public float CaptureHeight=>GolfCupRules.EntryHeight(settings.cupEntryHeightRatio);
  bool Inside(Vector3 local)=>new Vector2(local.x,local.z).sqrMagnitude<=CaptureRadius*CaptureRadius;
  public bool Contains(Vector3 position){var p=transform.InverseTransformPoint(position);return p.y<=CaptureHeight&&p.y>=-Hole.cupDepth-GolfBall.Radius&&Inside(p);}
  // A finite cylinder, also used for the wider guide-to-physics handoff.
  public bool Crossed(Vector3 from,Vector3 to){
   return Crossed(from,to,CaptureRadius,CaptureHeight);
  }
  public bool ApproachCrossed(Vector3 from,Vector3 to)=>Crossed(from,to,Hole.cupRadius+GolfBall.Radius+.02f,GolfBall.Radius+.04f);
  bool Crossed(Vector3 from,Vector3 to,float radius,float height){
   return CrossingPoint(from,to,radius,height,out _);
  }
  public bool CrossingPoint(Vector3 from,Vector3 to,out Vector3 point)=>CrossingPoint(from,to,CaptureRadius,CaptureHeight,out point);
  bool CrossingPoint(Vector3 from,Vector3 to,float radius,float height,out Vector3 point){
   point=to;
   var a=transform.InverseTransformPoint(from);var b=transform.InverseTransformPoint(to);var d=b-a;
   float lo=0,hi=1,low=-Hole.cupDepth-GolfBall.Radius;
   if(Mathf.Abs(d.y)<.000001f){if(a.y<low||a.y>height)return false;}
   else{float t0=(low-a.y)/d.y,t1=(height-a.y)/d.y;lo=Mathf.Max(lo,Mathf.Min(t0,t1));hi=Mathf.Min(hi,Mathf.Max(t0,t1));}
   float aa=d.x*d.x+d.z*d.z,bb=2*(a.x*d.x+a.z*d.z),cc=a.x*a.x+a.z*a.z-radius*radius;
   if(aa<.0000001f){if(cc>0||lo>hi)return false;point=Vector3.Lerp(from,to,lo);return true;}
   float discriminant=bb*bb-4*aa*cc;if(discriminant<0)return false;
   float root=Mathf.Sqrt(discriminant);lo=Mathf.Max(lo,(-bb-root)/(2*aa));hi=Mathf.Min(hi,(-bb+root)/(2*aa));
   if(lo>hi)return false;
   point=Vector3.Lerp(from,to,Mathf.Lerp(lo,hi,.05f));return true;
  }
  void OnTriggerEnter(Collider other){Check(other);}
  void OnTriggerStay(Collider other){Check(other);}
  void Check(Collider other){var ball=other.attachedRigidbody?other.attachedRigidbody.GetComponent<GolfBall>():null;if(ball)ball.TryCupCapture(this);}
 }
}
