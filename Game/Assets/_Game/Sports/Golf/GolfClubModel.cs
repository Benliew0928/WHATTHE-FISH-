using UnityEngine;

namespace WhatTheFish {
 public sealed class GolfClubModel:MonoBehaviour {
  public Transform leftGrip,rightGrip,head;
  void Awake(){VenueEquipmentFinish.Attach(gameObject,SportId.Golf);}
 }
}
