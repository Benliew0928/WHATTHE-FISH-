using System;
using UnityEngine;

namespace WhatTheFish {
 // Three authored patches, shared by both islands instead of baking each copy.
 public sealed class MobileGrassLibrary : ScriptableObject {
  [Serializable] public struct Blade {public Vector3 a,b,c,d,e;public bool light;}
  [Serializable] public sealed class Patch {public Blade[] blades;}
  [Serializable] public sealed class CoastalPatch {public string sport,cell;public Vector3 position;public float[] heights;}
  public Patch[] patches;
  public CoastalPatch[] coastal;
 }
}
