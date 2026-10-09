using UnityEngine;
using UnityEngine.UI;

namespace WhatTheFish {
 // One small procedural graphic; the map is informational and never consumes
 // movement/look touches. Arena-local coordinates also work on moved islands.
 public sealed class SportsMiniMap:MaskableGraphic {
  public static readonly Color OurTeam=CoveUI.Hex("2788ED"),Opponent=CoveUI.Hex("EC575D");
  SportId sport;float nextUpdate;Rect field=new Rect(-92,-47,184,94);
  public int PlayerCount {get;private set;}
  public bool IsAttack {get;private set;}
  public static SportsMiniMap Create(RectTransform parent,SportId sport){
   var root=new GameObject(sport+" pocket map",typeof(RectTransform)).GetComponent<RectTransform>();root.SetParent(parent,false);
   root.anchorMin=root.anchorMax=new Vector2(.5f,0);root.anchoredPosition=new Vector2(0, 70);root.sizeDelta=new Vector2(204,114);
   var map=root.gameObject.AddComponent<SportsMiniMap>();map.sport=sport;map.raycastTarget=false;
   return map;
  }
  void Update(){
   if(Time.unscaledTime<nextUpdate)return;nextUpdate=Time.unscaledTime+.08f;
   var app=AppRoot.Instance;IsAttack=app&&SportsPossession.Attacking(app.LocalAthlete,sport);
   SetVerticesDirty();
  }
  public Vector2 MapPoint(Vector3 world){
   Transform arena=null;Vector3 center=Vector3.zero;Vector2 half;
   if(sport==SportId.Football&&FootballBall.Instance){arena=FootballBall.Instance.Pitch;var bounds=FootballBall.Instance.PitchBounds;center=bounds.center;half=new Vector2(bounds.extents.x,bounds.extents.z);}
   else {arena=BasketballBall.Active?BasketballBall.Active.transform.parent:null;half=BasketballBall.PlayerCourtLimits;}
   var local=(arena?arena.InverseTransformPoint(world):world)-center;
   return new Vector2(Mathf.Clamp(local.z/Mathf.Max(1,half.y),-1,1)*field.width*.5f, -Mathf.Clamp(local.x/Mathf.Max(1,half.x),-1,1)*field.height*.5f)+field.center;
  }
  void Round(VertexHelper v,Rect r,float radius,Color c)=>CovePlate.Round(v,r,radius,c);
  void Dot(VertexHelper v,Vector2 p,float radius,Color c)=>Round(v,new Rect(p-Vector2.one*radius,Vector2.one*radius*2),radius,c);
  void Line(VertexHelper v,Vector2 a,Vector2 b,Color c,float width=1.5f){
   var n=new Vector2(-(b-a).y,(b-a).x).normalized*width*.5f;int i=v.currentVertCount;
   v.AddVert(a+n,c,Vector2.zero);v.AddVert(b+n,c,Vector2.zero);v.AddVert(b-n,c,Vector2.zero);v.AddVert(a-n,c,Vector2.zero);v.AddTriangle(i,i+1,i+2);v.AddTriangle(i,i+2,i+3);
  }
  void Ring(VertexHelper v,Vector2 p,float radius,Color c,float width=1.5f){for(int i=0;i<32;i++){float a=i*Mathf.PI/16,b=(i+1)*Mathf.PI/16;Line(v,p+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*radius,p+new Vector2(Mathf.Cos(b),Mathf.Sin(b))*radius,c,width);}}
  void Box(VertexHelper v,Rect r,Color c){Line(v,new(r.xMin,r.yMin),new(r.xMax,r.yMin),c);Line(v,new(r.xMax,r.yMin),new(r.xMax,r.yMax),c);Line(v,new(r.xMax,r.yMax),new(r.xMin,r.yMax),c);Line(v,new(r.xMin,r.yMax),new(r.xMin,r.yMin),c);}
  protected override void OnPopulateMesh(VertexHelper v){
   v.Clear();var r=rectTransform.rect;bool football=sport==SportId.Football;
   // A plain board with one border; the field markings identify the sport.
   Round(v,r,3,new Color(.93f,.98f,.96f,.9f));
   var inset=r;inset.xMin+=2;inset.xMax-=2;inset.yMin+=2;inset.yMax-=2;
   Round(v,inset,2,football?new Color(.10f,.25f,.21f,.91f):new Color(.25f,.21f,.17f,.91f));
   var paint=new Color(1,1,1,.38f);Box(v,field,paint);Line(v,new(0,-47),new(0,47),paint,1);Ring(v,Vector2.zero,football?13:11,paint,1);
   for(int sign=-1;sign<=1;sign+=2){
    Box(v,new Rect(sign<0?-92:69,-20,23,40),paint);
    if(football)Line(v,new(sign*95,-11),new(sign*95,11),Color.white,2);
    else {Ring(v,new(sign*69,0),11,paint,1);Ring(v,new(sign*85,0),3,Color.white,1);}
   }
   PlayerCount=0;var app=AppRoot.Instance;var own=app?app.LocalAthlete:null;
   if(app&&app.Exploring&&app.SelectedSport==sport&&!(SkySailWorld.Instance&&SkySailWorld.Instance.Travelling)){
    foreach(var actor in Athlete.Active){if(!actor||actor.inTransit||!actor.gameObject.activeInHierarchy)continue;PlayerCount++;
     var p=MapPoint(actor.transform.position);var c=SportsPossession.SameTeam(own,actor,sport)?OurTeam:Opponent;
     if(actor==own)Dot(v,p,5.6f,Color.white);
     Dot(v,p,actor==own?4:3.5f,c);
    }
    var ball=sport==SportId.Football?(FootballBall.Instance?FootballBall.Instance.transform:null):(BasketballBall.Active?BasketballBall.Active.transform:null);
    if(ball){var p=MapPoint(ball.position);Dot(v,p,3.4f,CoveUI.Ink);Dot(v,p,2.5f,Color.white);}
   }
  }
 }
}
