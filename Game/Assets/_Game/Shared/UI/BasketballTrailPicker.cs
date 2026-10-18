using UnityEngine;
using UnityEngine.UI;
namespace WhatTheFish {
 public sealed class BasketballTrailPicker:MonoBehaviour {
  readonly Text[] labels=new Text[BasketballTrailStyle.Count];readonly BasketballPlate[] plates=new BasketballPlate[BasketballTrailStyle.Count];
  public static void Create(Transform parent,float width,float height,bool compact){
   var picker=parent.gameObject.AddComponent<BasketballTrailPicker>();float gap=compact?10:18,tile=(width-2*gap)/3,row=(height-gap)/2;
   for(int i=0;i<BasketballTrailStyle.Count;i++){
    int style=i;float pad=compact?8:20,previewHeight=row*.42f;
    var b=FootballUIStyle.Button(parent,"Trail "+BasketballTrailStyle.Names[i],"",i%3*(tile+gap),i/3*(row+gap),tile,row,()=>{BasketballTrailStyle.Select(style);picker.Refresh();},CoveUI.Cream);
    b.navigation=new Navigation{mode=Navigation.Mode.Automatic};picker.plates[i]=b.GetComponent<BasketballPlate>();
    var swatch=BasketballUIStyle.Plate(b.transform,"Effect preview",pad,pad,tile-2*pad,previewHeight,CoveUI.Ink,compact?7:14,0);
    var preview=CoveUI.Rect(swatch.transform,"Animated trail",4,0,tile-pad*2-22,previewHeight).gameObject.AddComponent<FootballTrailPreview>();preview.Style=i;preview.Basketball=true;
    BasketballUIStyle.Art(swatch.transform,BasketballIcon.Shoot,tile-pad*2-28,previewHeight/2-11,22,22);
    picker.labels[i]=BasketballUIStyle.Text(b.transform,"",pad,previewHeight+pad+4,tile-pad*2,compact?25:40,compact?14:26,true,TextAnchor.MiddleCenter);
    if(!compact)BasketballUIStyle.Text(b.transform,BasketballTrailStyle.Descriptions[i],pad,previewHeight+pad+47,tile-pad*2,35,21,false,TextAnchor.MiddleCenter);
   }
   picker.Refresh();
  }
  void OnEnable()=>Refresh();
  void Refresh(){for(int i=0;i<labels.Length;i++)if(labels[i]){bool selected=BasketballTrailStyle.Selected==i;labels[i].text=(selected?"✓ ":"")+BasketballTrailStyle.Names[i];plates[i].color=selected?CoveUI.Mint:CoveUI.Cream;plates[i].border=selected?2:1;plates[i].Refresh();}}
 }
}
