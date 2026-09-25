using UnityEngine;

namespace WhatTheFish {
 public sealed class FishingLagoonView:StadiumView {
  public FishingModule[] playerStands;
  public Vector3[] standingPositions;
  public Material[] waterMaterials;
  public override void Apply(StadiumAppearance appearance){}
  // Water shader animates only ripples; the coast-aligned color map remains fixed.
 }
}
