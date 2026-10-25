using UnityEngine;
using UnityEngine.UI;
namespace WhatTheFish {
 public sealed class FootballTrailPicker:MonoBehaviour {
  readonly Text[] labels=new Text[FootballTrailStyle.Count];
  readonly BasketballPlate[] plates=new BasketballPlate[FootballTrailStyle.Count];
  public static void Create(Transform parent,float width,float height,bool compact){
   var picker=parent.gameObject.AddComponent<FootballTrailPicker>();
   float gap=compact?8:18,tile=(width-2*gap)/3,row=(height-gap)/2;
   for(int i=0;i<FootballTrailStyle.Count;i++){
    int style=i;float x=(i%3)*(tile+gap),y=(i/3)*(row+gap);
    var b=FootballUIStyle.Button(parent,"Trail "+FootballTrailStyle.Names[i],"",x,y,tile,row,()=>{FootballTrailStyle.Select(style);picker.Refresh();},CoveUI.Cream);
    b.navigation=new Navigation{mode=Navigation.Mode.Automatic};
    picker.plates[i]=b.GetComponent<BasketballPlate>();
    float pad=compact?8:20,previewHeight=row*(compact?.40f:.45f);
    var swatch=BasketballUIStyle.Plate(b.transform,"Effect preview",pad,pad,tile-pad*2,previewHeight,CoveUI.Ink,compact?7:14,0);
    var preview=CoveUI.Rect(swatch.transform,"Animated trail",4,0,tile-pad*2-18,previewHeight).gameObject.AddComponent<FootballTrailPreview>();preview.Style=i;
    CoveUI.Image(swatch.transform,"Football",FootballUIStyle.Icon(FootballIcon.Ball),tile-pad*2-25,previewHeight/2-10,20,20);
    picker.labels[i]=BasketballUIStyle.Text(b.transform,"",pad,previewHeight+pad+3,tile-pad*2,compact?22:40,compact?13:26,true,TextAnchor.MiddleCenter);
    if(!compact)BasketballUIStyle.Text(b.transform,FootballTrailStyle.Descriptions[i],pad,previewHeight+pad+44,tile-pad*2,35,21,false,TextAnchor.MiddleCenter);
   }
   for(int i=0;i<FootballTrailStyle.Count;i++){int style=i;CosmeticLock.Bind(picker.plates[i].GetComponent<Button>(),"footballTrail",i,()=>{FootballTrailStyle.Select(style);picker.Refresh();});}
   picker.Refresh();
  }
  void OnEnable(){PlayerEconomy.Changed+=Refresh;Refresh();}
  void OnDisable()=>PlayerEconomy.Changed-=Refresh;
  void Refresh(){for(int i=0;i<labels.Length;i++)if(labels[i]){bool selected=FootballTrailStyle.Selected==i;labels[i].text=(selected?"✓ ":"")+FootballTrailStyle.Names[i];MenuDesigns.Selected(plates[i].GetComponent<Button>(),selected);plates[i].color=selected?CoveUI.Mint:CoveUI.Cream;plates[i].border=selected?2:1;plates[i].Refresh();}}
 }
}
