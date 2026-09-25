using UnityEngine;

namespace WhatTheFish {
 // Stable sockets and independent prefabs let future customization replace one
 // module without rebuilding terrain or changing a player's station identity.
 public sealed class FishingModule:MonoBehaviour {
  public string moduleType,slotId;
  public Color accent=Color.white;
  public void SetAccent(Color color){
   accent=color;var block=new MaterialPropertyBlock();block.SetColor("_BaseColor",color);
   foreach(var renderer in GetComponentsInChildren<MeshRenderer>(true)){
    var mats=renderer.sharedMaterials;
    for(int i=0;i<mats.Length;i++)if(mats[i]&&mats[i].name=="LG_Accent")renderer.SetPropertyBlock(block,i);
   }
  }
  void OnEnable(){if(moduleType=="PlayerStand")SetAccent(accent);}
 }
}
