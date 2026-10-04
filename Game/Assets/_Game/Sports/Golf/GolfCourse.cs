using System;
using UnityEngine;

namespace WhatTheFish {
 // Serialized map data remains compatible with the existing five-hole assets.
 public sealed class GolfCourse : MonoBehaviour {
  public GolfHole[] holes;
  void OnEnable(){if(Application.isPlaying&&!GetComponent<GolfMatchManager>())gameObject.AddComponent<GolfMatchManager>();}
 }
 [Serializable] public sealed class GolfHole {
  public int number,par;
  public string title;
  public Transform cup,flag,tee;
  public float cupRadius,cupDepth,greenRadius;
 }
}
