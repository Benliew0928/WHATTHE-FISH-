using UnityEngine;
using UnityEngine.UI;
namespace WhatTheFish {
 // Vector art stays crisp at every HUD scale and adds no bitmap payload.
 public sealed class FishingSkillGraphic:MaskableGraphic {
  public FishingAbility ability;public bool ring;public float amount=1;public Color tint=CoveUI.Gold;
  public static void Icon(RectTransform parent,FishingAbility ability){
   Sprite sprite=ability switch {FishingAbility.Sonar=>SportsUIStyle.PropIcon(SportsProp.LockOn),FishingAbility.PowerReel=>Resources.Load<Sprite>("FishingUI/ReelButton"),FishingAbility.BubbleGuard=>FishingGearArt.Icon(4),_=>null};
   if(sprite){var image=parent.gameObject.AddComponent<Image>();image.sprite=sprite;image.preserveAspect=true;image.raycastTarget=false;}
   else {var image=parent.gameObject.AddComponent<FishingSkillGraphic>();image.ability=ability;image.raycastTarget=false;}
  }
  public void Tick(float value,Color color){amount=Mathf.Clamp01(value);tint=color;SetVerticesDirty();}
  protected override void OnPopulateMesh(VertexHelper v){
   v.Clear();float scale=Mathf.Min(rectTransform.rect.width,rectTransform.rect.height)/100;var centre=rectTransform.rect.center;
   void Line(Vector2 a,Vector2 b,Color c,float w)=>BasketballUIMesh.Line(v,centre+a*scale,centre+b*scale,c,w*scale);
   void Dot(Vector2 at,float radius,Color c)=>BasketballUIMesh.Dot(v,centre+at*scale,radius*scale,c);
   if(ring){float radius=46*scale;for(int i=0;i<90;i++){float a=i/90f*Mathf.PI*2,b=(i+1f)/90*Mathf.PI*2;var x=centre+new Vector2(Mathf.Sin(a),Mathf.Cos(a))*radius;var y=centre+new Vector2(Mathf.Sin(b),Mathf.Cos(b))*radius;BasketballUIMesh.Line(v,x,y,i/90f<amount?tint:new Color(.1f,.22f,.3f,.25f),5*scale);}return;}
   var ink=CoveUI.Ink;var cream=CoveUI.Cream;var blue=CoveUI.Sky;
   if(ability==FishingAbility.FishingBuddy){
    Dot(new Vector2(14,8),25,ink);Dot(new Vector2(14,8),21,new Color(.45f,.48f,.52f));
    Dot(new Vector2(-11,0),28,ink);Dot(new Vector2(-11,0),24,cream);Dot(new Vector2(-19,3),3,ink);Dot(new Vector2(-3,3),3,ink);
    Line(new Vector2(-19,-10),new Vector2(-11,-14),ink,3);Line(new Vector2(-11,-14),new Vector2(-3,-10),ink,3);
    Line(new Vector2(-33,28),new Vector2(-7,39),ink,8);Line(new Vector2(-33,28),new Vector2(-7,39),Color.white,4);
   }else if(ability==FishingAbility.SteelWire){
    Dot(Vector2.zero,30,ink);Dot(Vector2.zero,25,blue);Dot(Vector2.zero,15,ink);Dot(Vector2.zero,10,Color.white);
    for(int i=0;i<5;i++){float x=-28+i*12;Line(new Vector2(x,-30),new Vector2(x+12,-19),ink,6);Line(new Vector2(x,-30),new Vector2(x+12,-19),Color.white,3);}
    Line(new Vector2(-38,19),new Vector2(-24,35),ink,5);Line(new Vector2(24,35),new Vector2(38,19),ink,5);
   }else {
    for(int i=0;i<3;i++){var at=new Vector2((i-1)*25,(i%2==0?1:-1)*12);Dot(at,14,ink);Dot(at,10,i==1?CoveUI.Gold:blue);Dot(at+new Vector2(5,3),2,ink);Line(at+new Vector2(-12,0),at+new Vector2(-23,9),ink,7);Line(at+new Vector2(-23,9),at+new Vector2(-23,-9),ink,6);Line(at+new Vector2(-23,-9),at+new Vector2(-12,0),ink,7);}
    Line(new Vector2(29,28),new Vector2(38,37),CoveUI.Gold,5);Line(new Vector2(22,37),new Vector2(23,45),CoveUI.Gold,4);
   }
  }
 }
}
