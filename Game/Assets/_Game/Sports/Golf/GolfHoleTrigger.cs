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
   trigger.isTrigger=true;trigger.center=Vector3.down*(hole.cupDepth*.5f);
   trigger.size=new Vector3(hole.cupRadius*2,hole.cupDepth,hole.cupRadius*2);
  }
  bool Inside(Vector3 local){float radius=Mathf.Max(0,Hole.cupRadius-GolfBall.Radius);return new Vector2(local.x,local.z).sqrMagnitude<=radius*radius;}
  public bool Contains(Vector3 position){var p=transform.InverseTransformPoint(position);return p.y<=-GolfBall.Radius*.25f&&p.y>=-Hole.cupDepth-GolfBall.Radius&&Inside(p);}
  // Triggers are discrete. A downward sweep also catches a fast ball passing
  // through the cup between physics steps, without scoring a fly-over.
  public bool Crossed(Vector3 from,Vector3 to){
   var a=transform.InverseTransformPoint(from);var b=transform.InverseTransformPoint(to);float line=-GolfBall.Radius*.25f;
   return a.y>line&&b.y<=line&&Inside(Vector3.Lerp(a,b,(a.y-line)/(a.y-b.y)));
  }
  void OnTriggerEnter(Collider other){Check(other);}
  void OnTriggerStay(Collider other){Check(other);}
  void Check(Collider other){var ball=other.attachedRigidbody?other.attachedRigidbody.GetComponent<GolfBall>():null;if(ball&&Contains(ball.Body.position))match.EnterHole(ball,Hole.number);}
 }
}
