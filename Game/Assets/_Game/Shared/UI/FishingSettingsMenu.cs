using UnityEngine;

namespace WhatTheFish {
 public sealed class FishingSettingsMenu:IslandSettingsMenu {
  public static FishingSettingsMenu Create(RectTransform page)=>Create<FishingSettingsMenu>(page,SportId.Fishing);
 }
}
