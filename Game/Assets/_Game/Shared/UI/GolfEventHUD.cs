using System;
using UnityEngine;
using UnityEngine.UI;

namespace WhatTheFish {
 [DefaultExecutionOrder(-50)] public sealed class GolfEventHUD:MonoBehaviour {
  sealed class Row {public RectTransform root;public GolfEventBadgeGraphic art;public Text clock;public int previous=-1,index=-1;}
  RectTransform page,card;Row weather,rushRow;Texture2D sheet,warningSheet;
  readonly Vector3[] gearCorners=new Vector3[4];
  RectTransform windArrow,windCompass;GolfWindEdgeGraphic edge;GolfEventKind observed;uint observedRound;bool observedRush;float windAt=-100,weatherAt=-100,rushAt=-100;
  float windEdgeLevel;
  public RectTransform Card=>card;
  public bool WindVisible=>windArrow&&windArrow.gameObject.activeInHierarchy;
  public float StackHeight {get;private set;}
  public static GolfEventHUD Instance {get;private set;}
  public static GolfEventHUD Create(RectTransform page){var ui=page.gameObject.AddComponent<GolfEventHUD>();ui.Build(page);return ui;}
  void Build(RectTransform parent){
   Instance=this;page=parent;sheet=Resources.Load<Texture2D>("GolfUI/Events/BadgeSheet");warningSheet=Resources.Load<Texture2D>("GolfUI/Events/WarningSheet");
   var border=GolfHUDStyle.Rect("Wind arrival screen edges",page,new Vector2(.5f,.5f),Vector2.zero,Vector2.zero);border.anchorMin=Vector2.zero;border.anchorMax=Vector2.one;border.offsetMin=border.offsetMax=Vector2.zero;border.SetAsFirstSibling();edge=border.gameObject.AddComponent<GolfWindEdgeGraphic>();edge.raycastTarget=false;edge.Strength(0);
   card=GolfHUDStyle.Rect("Golf event notices",page,new Vector2(0,1),new Vector2(20,-20),new Vector2(410,230));card.pivot=new Vector2(0,1);
   var group=card.gameObject.AddComponent<CanvasGroup>();group.blocksRaycasts=false;group.interactable=false;
   weather=BuildRow("Golf weather badge");rushRow=BuildRow("Golf rush badge");
   windCompass=GolfHUDStyle.Rect("Wind compass in badge",weather.root,new Vector2(.5f,.5f),Vector2.zero,new Vector2(32,32));var face=GolfHUDStyle.Panel(windCompass,GolfHUDStyle.Cream,true);face.cornerRadius=100;
   windArrow=GolfHUDStyle.Rect("Wind pushes ball this way",windCompass,new Vector2(.5f,.5f),Vector2.zero,new Vector2(18,22));var arrow=windArrow.gameObject.AddComponent<GolfArrowGraphic>();arrow.color=GolfHUDStyle.Ink;arrow.raycastTarget=false;windCompass.gameObject.SetActive(false);
   card.gameObject.SetActive(false);
  }
  Row BuildRow(string name){
   var row=new Row();row.root=GolfHUDStyle.Rect(name,card,new Vector2(0,1),Vector2.zero,new Vector2(410,135));row.root.pivot=new Vector2(0,1);
   row.art=row.root.gameObject.AddComponent<GolfEventBadgeGraphic>();row.art.Sheet=sheet;row.art.raycastTarget=false;
   row.clock=GolfHUDStyle.Text("Event seconds",row.root,"",new Vector2(55,34),27,TextAnchor.MiddleCenter,Color.white);row.clock.font=CoveUI.DisplayFont??CoveUI.TextFont;row.clock.horizontalOverflow=HorizontalWrapMode.Overflow;row.clock.verticalOverflow=VerticalWrapMode.Overflow;
   var outline=row.clock.gameObject.AddComponent<Outline>();outline.effectColor=LocalProfile.Hex("123A91");outline.effectDistance=new Vector2(1.6f,-1.6f);
   return row;
  }
  public static string Name(GolfEventKind kind)=>kind switch {GolfEventKind.TropicalWind=>"TROPICAL WIND",GolfEventKind.StickyGreen=>"STICKY GREEN",GolfEventKind.BounceParty=>"BOUNCE PARTY",_=>""};
  float ShowRow(Row row,int index,int seconds,float y,float width){
   row.root.gameObject.SetActive(true);row.root.anchoredPosition=new Vector2(0,-y);
   float height=width*GolfEventBadgeGraphic.Crop(index).height/GolfEventBadgeGraphic.Crop(index).width;row.root.sizeDelta=new Vector2(width,height);
   {
    var crop=GolfEventBadgeGraphic.Crop(index);var box=GolfEventBadgeGraphic.TimeBox(index);float scale=width/crop.width;
    if(row.index!=index){row.index=index;row.previous=-1;row.art.Badge=index;}
    var r=row.clock.rectTransform;r.anchorMin=r.anchorMax=new Vector2(0,1);r.pivot=new Vector2(.5f,.5f);r.anchoredPosition=new Vector2(box.center.x-crop.x,-(box.center.y-crop.y))*scale;r.sizeDelta=box.size*scale;row.clock.fontSize=Mathf.RoundToInt((index==5?310:45)*scale);
    row.art.Sheet=index==5?warningSheet:sheet;row.clock.enabled=true;row.clock.fontStyle=index==5?FontStyle.Bold:FontStyle.Normal;
    var outline=row.clock.GetComponent<Outline>();outline.effectColor=index==5?Color.white:LocalProfile.Hex("123A91");outline.effectDistance=Vector2.one*(index==5?2.3f:1.6f);
    var gradient=row.clock.GetComponent<GolfWarningGradient>();if(index==5&&!gradient)gradient=row.clock.gameObject.AddComponent<GolfWarningGradient>();if(gradient)gradient.enabled=index==5;
    if(row.previous!=seconds){row.previous=seconds;row.clock.text=(index==5?Mathf.Max(0,seconds).ToString("00"):Mathf.Max(0,seconds).ToString())+"s";}
   }
   return height;
  }
  void Update(){
   var match=GolfMatchManager.Instance;StackHeight=0;
   if(!match||!match.Context){card.gameObject.SetActive(false);edge.Strength(0);windEdgeLevel=0;observed=GolfEventKind.None;observedRush=false;return;}
   double now=match.Now;var state=match.Events.Snapshot;bool rush=match.Events.RushActive(now),warn=match.Events.WarningActive(now);var active=match.Events.Active(now);
   if(observedRound!=match.Round){observedRound=match.Round;observed=GolfEventKind.None;observedRush=false;}
   if(active!=observed){if(active!=GolfEventKind.None)weatherAt=Time.unscaledTime;if(active==GolfEventKind.TropicalWind)windAt=Time.unscaledTime;observed=active;}
   if(rush&&!observedRush)rushAt=Time.unscaledTime;observedRush=rush;
   var config=GolfPresentationSettings.Current;float age=(Time.unscaledTime-windAt)/config.windEdgeSeconds;
   float breath=.85f+.15f*Mathf.Sin(Time.unscaledTime*2);
   float windTarget=active==GolfEventKind.TropicalWind?Mathf.Max(config.windSustainOpacity*breath,Mathf.Sin(Mathf.Clamp01(age)*Mathf.PI)*config.windEdgeOpacity):active is GolfEventKind.StickyGreen or GolfEventKind.BounceParty?config.weatherEdgeOpacity*breath:0;
   windEdgeLevel=MenuPreferences.ReducedMotion?0:Mathf.MoveTowards(windEdgeLevel,windTarget,Time.unscaledDeltaTime*.6f);
   edge.Strength(windEdgeLevel,active==GolfEventKind.StickyGreen?config.stickyColor:active==GolfEventKind.BounceParty?config.bounceColor:new Color(.58f,.68f,.77f));
   bool visible=rush||warn||active!=GolfEventKind.None;card.gameObject.SetActive(visible);if(!visible)return;
   // The island menu scales independently of this HUD. Convert its actual
   // button edge into our page space so all notice types clear it on resize.
   float left=20;var menu=GolfHUD.Instance?GolfHUD.Instance.IslandMenu:null;
   if(menu&&menu.GearRect&&menu.GearRect.gameObject.activeInHierarchy){
    menu.GearRect.GetWorldCorners(gearCorners);
    for(int i=0;i<gearCorners.Length;i++)left=Mathf.Max(left,page.InverseTransformPoint(gearCorners[i]).x-page.rect.xMin+12);
   }
   card.anchoredPosition=new Vector2(left,-20);
   float width=Mathf.Min(410,Mathf.Min(page.rect.width*.43f-24,page.rect.width-left-20)),y=0;weather.root.gameObject.SetActive(warn||active!=GolfEventKind.None);rushRow.root.gameObject.SetActive(rush);
   if(warn||active!=GolfEventKind.None){int index=warn?5:Mathf.Clamp((int)state.kind-1,0,3);int seconds=(int)Math.Ceiling(warn?state.start-now:state.end-now);y+=ShowRow(weather,index,seconds,y,width)+8;}
   if(active==GolfEventKind.TropicalWind&&!warn)y+=config.windCompassSize+16;
   if(rush)y+=ShowRow(rushRow,4,(int)Math.Ceiling(state.rushAt+GolfEventSchedule.RushDuration-now),y,width)+8;
   Pulse(weather,weatherAt);Pulse(rushRow,rushAt);
   windCompass.gameObject.SetActive(active==GolfEventKind.TropicalWind&&!warn);
   if(WindVisible){float size=config.windCompassSize;windCompass.anchorMin=windCompass.anchorMax=new Vector2(0,0);windCompass.anchoredPosition=new Vector2(size*.5f+4,-size*.5f-8);windCompass.sizeDelta=Vector2.one*size;windCompass.localScale=Vector3.one*(MenuPreferences.ReducedMotion?1:1+.04f*Mathf.Sin(Time.unscaledTime*2));windArrow.sizeDelta=new Vector2(size*.58f,size*.7f);var camera=Camera.main;if(camera){var drift=state.wind;float x=Vector3.Dot(camera.transform.right,drift),yDirection=Vector3.Dot(Vector3.ProjectOnPlane(camera.transform.forward,Vector3.up).normalized,drift);windArrow.localRotation=Quaternion.Euler(0,0,-Mathf.Atan2(x,yDirection)*Mathf.Rad2Deg);}}
   StackHeight=y;card.sizeDelta=new Vector2(width,y);
  }
  static void Pulse(Row row,float at){float age=Time.unscaledTime-at;float scale=MenuPreferences.ReducedMotion||age>=.45f?1:1+.035f*Mathf.Sin(Mathf.Clamp01(age/.45f)*Mathf.PI);row.root.localScale=Vector3.one*scale;}
  void OnDestroy(){if(Instance==this)Instance=null;}
 }
}
