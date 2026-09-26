using UnityEngine;

namespace WhatTheFish {
 // Shared static route anchors for validation, safe returns and future crowds.
 // They do not introduce autonomous actors or change network command ownership.
 public sealed class CoastalVenueRoutes:MonoBehaviour {
  public static CoastalVenueRoutes Active{get;private set;}
  public string sport;
  public Vector3[] entries,inside,stairBottom,stairTop,concourse;
  [System.Serializable] public class Stair{public Vector3[] points;}
  public Stair[] stairs;
  public Stair[] aisles;
  public Vector3 safeReturn;
  void OnEnable(){Active=this;}
  void OnDisable(){if(Active==this)Active=null;}
 }
}
