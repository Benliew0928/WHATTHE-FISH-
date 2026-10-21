using UnityEngine;

namespace WhatTheFish {
 // Property blocks retain authored textures and shared materials. Cache the
 // renderer list; only write it again when the room's appearance changes.
 public sealed class VenueEquipmentFinish:MonoBehaviour {
  public SportId Sport {get;private set;}
  public int Finish {get;private set;}=-1;
  Renderer[] renderers;MaterialPropertyBlock block;
  static readonly Color[] Tints={Color.white,new Color(.58f,1f,.91f),new Color(1f,.68f,.45f)};
  public static VenueEquipmentFinish Attach(GameObject target,SportId sport){
   var finish=target.GetComponent<VenueEquipmentFinish>()??target.AddComponent<VenueEquipmentFinish>();finish.Sport=sport;return finish;
  }
  void LateUpdate(){
   var app=AppRoot.Instance;if(!app||app.SelectedSport!=Sport||!app.stadium)return;
   var style=app.stadium.GetComponent<IslandVenueStyle>();if(style)ApplyFinish(style.Appearance.design);
  }
  public void ApplyFinish(int finish){
   block??=new MaterialPropertyBlock();
   finish=Mathf.Clamp(finish,0,2);if(Finish==finish)return;Finish=finish;
   if(renderers==null)renderers=GetComponentsInChildren<Renderer>(true);
   foreach(var renderer in renderers)if(renderer){
    var materials=renderer.sharedMaterials;
    for(int i=0;i<materials.Length;i++){
     var material=materials[i];if(!material)continue;renderer.GetPropertyBlock(block,i);
     foreach(var property in new[]{"_BaseColor","_Color"})if(material.HasProperty(property))block.SetColor(property,material.GetColor(property)*Tints[finish]);
     renderer.SetPropertyBlock(block,i);block.Clear();
    }
   }
  }
 }
}
