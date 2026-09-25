using UnityEngine;
using UnityEngine.UI;
namespace WhatTheFish {
 public sealed class StadiumSign:MonoBehaviour { public Text label;public bool nameSign; public void Apply(StadiumAppearance a){label.text=nameSign?a.title.ToUpperInvariant():new[]{"PLAY TOGETHER  •  SUNNY DAYS","ONE TEAM  /  GOOD ENERGY","MOVE • PLAY • REPEAT"}[a.design];} }
}
