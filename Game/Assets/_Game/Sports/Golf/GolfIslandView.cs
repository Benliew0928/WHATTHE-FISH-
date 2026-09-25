using UnityEngine;
namespace SportsPrototype {
 public sealed class GolfIslandView:StadiumView {
  public Material[] waterMaterials;
  public override void Apply(StadiumAppearance appearance){}
  void Update(){
   if(waterMaterials==null)return;
   for(int i=0;i<waterMaterials.Length;i++)if(waterMaterials[i])
    waterMaterials[i].SetTextureOffset("_BaseMap",i==3?new Vector2(0,-Time.time*.18f):new Vector2(Time.time*.012f,i==0?Time.time*.007f:0));
  }
  void OnDisable(){if(waterMaterials!=null)foreach(var m in waterMaterials)if(m)m.SetTextureOffset("_BaseMap",Vector2.zero);}
 }
}
