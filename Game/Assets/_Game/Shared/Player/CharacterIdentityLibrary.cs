using UnityEngine;

namespace WhatTheFish {
 public sealed class CharacterIdentityLibrary:ScriptableObject {
  public Material bodyMaterial,hairMaterial;
  public Mesh[] nearHair,farHair;
  public Mesh nearScalp,farScalp;
  public Vector3 headPosition;
  public Quaternion headRotation;
  public string sourceHash;
 }
}
