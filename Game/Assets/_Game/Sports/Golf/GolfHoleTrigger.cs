using UnityEngine;

namespace WhatTheFish {
 // Separate from GolfHole's existing serialized map record.
 public sealed class GolfHoleTrigger:MonoBehaviour {
  public GolfHole Hole {get;private set;}
  GolfMatchManager match;
  public void Bind(GolfMatchManager owner,GolfHole hole){
   match=owner;Hole=hole;var trigger=GetComponent<BoxCollider>();
   // Unity's missing-component placeholder in the Editor is not a C# null.
   if(!trigger)trigger=gameObject.AddComponent<BoxCollider>();
   trigger.isTrigger=true;
   float low=-hole.cupDepth-GolfBall.Radius,high=CaptureHeight;
   trigger.center=Vector3.up*((low+high)*.5f);
   trigger.size=new Vector3(CaptureRadius*2,high-low,CaptureRadius*2);
  }
  // The ball's surface touching the cup rim is sufficient. A small vertical
  // tolerance accommodates terrain support, but airborne fly-overs do not score.
  public float CaptureRadius=>Hole.cupRadius+GolfBall.Radius;
  public float CaptureHeight=>GolfBall.Radius+.025f;
  bool Inside(Vector3 local)=>new Vector2(local.x,local.z).sqrMagnitude<=CaptureRadius*CaptureRadius;
  public bool Contains(Vector3 position){var p=transform.InverseTransformPoint(position);return p.y<=CaptureHeight&&p.y>=-Hole.cupDepth-GolfBall.Radius&&Inside(p);}
  // Intersect the whole swept segment with the finite capture cylinder. This
  // catches horizontal rim contacts as well as steep drops between fixed steps.
  public bool Crossed(Vector3 from,Vector3 to){
   var a=transform.InverseTransformPoint(from);var b=transform.InverseTransformPoint(to);var d=b-a;
   float lo=0,hi=1,low=-Hole.cupDepth-GolfBall.Radius;
   if(Mathf.Abs(d.y)<.000001f){if(a.y<low||a.y>CaptureHeight)return false;}
   else{float t0=(low-a.y)/d.y,t1=(CaptureHeight-a.y)/d.y;lo=Mathf.Max(lo,Mathf.Min(t0,t1));hi=Mathf.Min(hi,Mathf.Max(t0,t1));}
   float aa=d.x*d.x+d.z*d.z,bb=2*(a.x*d.x+a.z*d.z),cc=a.x*a.x+a.z*a.z-CaptureRadius*CaptureRadius;
   if(aa<.0000001f)return cc<=0&&lo<=hi;
   float discriminant=bb*bb-4*aa*cc;if(discriminant<0)return false;
   float root=Mathf.Sqrt(discriminant);lo=Mathf.Max(lo,(-bb-root)/(2*aa));hi=Mathf.Min(hi,(-bb+root)/(2*aa));
   return lo<=hi;
  }
  void OnTriggerEnter(Collider other){Check(other);}
  void OnTriggerStay(Collider other){Check(other);}
  void Check(Collider other){var ball=other.attachedRigidbody?other.attachedRigidbody.GetComponent<GolfBall>():null;if(ball&&Contains(ball.Body.position))match.EnterHole(ball,Hole.number);}
 }
}
