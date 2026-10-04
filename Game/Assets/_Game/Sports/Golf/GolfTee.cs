using UnityEngine;

namespace WhatTheFish {
 // A fixed wooden support at the first tee. The existing dynamic ball rests
 // on its shallow cup; the support never becomes part of the moving ball.
 [RequireComponent(typeof(MeshCollider))]
 public sealed class GolfTee:MonoBehaviour {
  public const int Layer=2;
  public const float SeatHeight=.078f;
  public const float CupRadius=.024f;
  MeshCollider support;
  void Awake(){support=GetComponent<MeshCollider>();support.contactOffset=.001f;support.excludeLayers=(1<<9)|(1<<GolfCartMotor.Layer);IgnorePlayers();}
  void FixedUpdate(){IgnorePlayers();}
  void IgnorePlayers(){foreach(var actor in Athlete.Active)if(actor&&actor.capsule&&actor.capsule.enabled)Physics.IgnoreCollision(support,actor.capsule);}
 }
}
