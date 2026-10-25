using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace WhatTheFish {
 public sealed class SportTutorialUI:IslandTutorialUI {
  static readonly Dictionary<SportId,SportTutorialUI> popups=new();
  SportId sport;string[] titles,copy;float arrivedAt=-1;
  ControlDesigns.View stick,action,power;TutorialMeterGraphic meter;RectTransform touch;
  readonly List<RectTransform> floating=new();
  readonly List<(GolfEventBadgeGraphic badge,int index)> eventBadges=new();
  public override SportId Sport=>sport;
  protected override string[] PageTitles=>titles;
  protected override string[] PageCopy=>copy;
  protected override string SeenPreference=>SeenKey(sport);
  public static string SeenKey(SportId sport)=>"island.rules."+sport.ToString().ToLowerInvariant()+".v1";
#if UNITY_EDITOR || DEVELOPMENT_BUILD
  internal bool ReviewEntry;
#endif
  public static SportTutorialUI Find(SportId sport)=>popups.TryGetValue(sport,out var ui)&&ui?ui:null;
  public static SportTutorialUI Create(RectTransform parent,SportId sport,bool embedded=false,Action closed=null){
   if(sport==SportId.Fishing)throw new ArgumentException("Use the Fishing tutorial factory.",nameof(sport));
   var node=GolfHUDStyle.Rect(sport+" illustrated tutorial",parent,Vector2.one*.5f,Vector2.zero,Vector2.zero);CoveUI.Stretch(node);
   var ui=node.gameObject.AddComponent<SportTutorialUI>();ui.sport=sport;ui.SetCopy();ui.Initialize(parent,embedded,closed);if(!embedded)popups[sport]=ui;return ui;
  }
  void SetCopy(){
   if(sport==SportId.Football){
    titles=new[]{"MOVE & PASS","AIM & SHOOT","DEFEND & USE ABILITIES","PLAY TO WIN"};
    copy=new[]{
     "Move with the left stick.\nSwipe the open right side to look.\nWalk into a slow free ball to dribble.\nFace a teammate and tap Pass,\nor double-tap their character.",
     "Face the goal and hold Shoot.\nRelease to kick with that power.\nSlide to Cancel to stop the shot.\nHold Dash while moving for a burst.\nRelease Dash to recover stamina.",
     "Hold Pressure; Tackle up close.\nCollect halfway props. Tap to use.\nTurbo speeds up your whole team.\nPortal brings the ball (own half).\nWall: aim, then tap Place to block.",
     $"The host starts; choose your team.\nEach goal scores 1.\nMost goals after {MenuMatchRules.CurrentMinutes} minutes wins.\nA tie adds 60 seconds of golden goal.\nStill level? The match is a draw."
    };
   }else if(sport==SportId.Basketball){
    titles=new[]{"MOVE & PASS","HOLD, AIM & RELEASE","DEFEND & USE ABILITIES","PLAY TO WIN"};
    copy=new[]{
     "Move with the left stick.\nSwipe the open right side to look.\nWalk into the ball to pick it up.\nFace a teammate and tap Pass,\nor double-tap their character.",
     "Hold Shoot; aim at the hoop.\nRelease when the marker is green.\nSlide to Cancel to stop a shot.\nSlide up for a dunk, or diagonally\nfor a layup when near the hoop.",
     "Without the ball: hold Guard.\nSwipe up while holding to block.\nHold Steal close to the carrier.\nCollect and tap a courtside prop:\nLock-On, Turbo or Portal. One use.",
     $"The host starts; choose your team.\nBaskets score 2, or 3 from outside\nthe arc. Most points after\n{MenuMatchRules.CurrentMinutes} minutes wins.\nA tie goes to overtime."
    };
   }else{
    titles=new[]{"FIND YOUR BALL","AIM, PULL & RELEASE","READ THE COURSE","PLAY TO WIN"};
    copy=new[]{
     "The host starts the round.\nWalk near your ball and tap Aim.\nSwipe on the right to aim.\nFollow the numbered flag marker.\nTap Cancel to walk again.",
     "Pull the ball handle down.\nA longer pull adds more power.\nPush up to soften the shot.\nRelease your finger to swing.\nUse gentle power near the cup.\nHard shots can roll over it.",
     "Wind, low gravity, sticky greens,\ntornadoes and crabs affect the ball.\nWatch the badges and wind arrow.\nTap to skip the ball-follow replay.\nUse Buggy, then Drive to travel faster.",
     $"Finish all {GolfPlayerState.HoleCount} holes in fewer strokes.\nHole 1 is first; the rest are shuffled.\nEveryone follows the same route.\nThe first finisher starts a\n{GolfMatchState.FinalCountdownSeconds:0}-second final countdown."
    };
   }
  }
  protected override Sprite Illustration(int page)=>page==0||page==3?Resources.Load<Sprite>("IslandTutorial/"+sport):null;
  ControlDesigns.View Preview(int page,string id,Vector2 at,Vector2 box){var r=Rect("Actual "+sport+" "+id,examples[page],at,Vector2.zero);var v=ControlDesigns.Build(r,sport,id);float scale=Mathf.Min(box.x/r.rect.width,box.y/r.rect.height);r.localScale=Vector3.one*scale;foreach(var g in r.GetComponentsInChildren<Graphic>(true))g.raycastTarget=false;return v;}
  void Caption(Transform parent,string text,Vector2 at,Vector2 size,int points=18)=>Text(text,parent,text,at,size,points);
  void Prop(int index,Sprite sprite,string label){var r=Rect("Actual prop "+label,examples[2],new Vector2((index-1)*112,-66),new Vector2(98,92));Icon(r,label,sprite,new Vector2(0,12),Vector2.one*62);Caption(r,label,new Vector2(0,-35),new Vector2(108,28),17);floating.Add(r);}
  protected override void BuildExamples(){
   for(int i=0;i<4;i++)examples[i]=Rect(sport+" tutorial gameplay example "+i,content,new Vector2(-216,-71),new Vector2(376,132));
   if(sport==SportId.Golf){
    Preview(0,"aim",new Vector2(-87,0),new Vector2(170,84));Preview(0,"move",new Vector2(112,0),Vector2.one*92);
    examples[1].anchoredPosition=new Vector2(-216,8);examples[1].sizeDelta=new Vector2(376,260);
    power=Preview(1,"power",new Vector2(-77,5),new Vector2(135,238));
    touch=Icon(examples[1],"Animated touch",AppRoot.MakeCircle(),new Vector2(-77,80),Vector2.one*23).rectTransform;touch.GetComponent<Image>().color=CoveUI.Gold;
    Caption(examples[1],"PULL DOWN",new Vector2(84,61),new Vector2(148,32),22);Caption(examples[1],"RELEASE",new Vector2(84,-5),new Vector2(148,32),22);
    Icon(examples[1],"Release check",SportsUIStyle.Check,new Vector2(84,-59),Vector2.one*40);
    examples[2].anchoredPosition=new Vector2(-216,8);examples[2].sizeDelta=new Vector2(376,260);
    string[] assets={"BadgeSheet","BadgeSheet","BadgeSheet","TornadoBadge","CrabBadge"};int[] indices={0,1,2,6,7};
    for(int i=0;i<5;i++){var sheet=Resources.Load<Texture2D>("GolfUI/Events/"+assets[i]);var crop=GolfEventBadgeGraphic.Crop(indices[i]);float width=i==0?300:176;var at=i==0?new Vector2(0,72):new Vector2((i-1)%2==0?-92:92,i<3?-24:-99);var r=Rect("Actual event badge "+i,examples[2],at,new Vector2(width,width*crop.height/crop.width));var badge=r.gameObject.AddComponent<GolfEventBadgeGraphic>();badge.Sheet=sheet;badge.Badge=indices[i];badge.raycastTarget=false;eventBadges.Add((badge,i));}
   }else{
    stick=Preview(0,"move",new Vector2(-95,0),Vector2.one*100);Preview(0,"pass",new Vector2(100,0),Vector2.one*100);
    examples[1].anchoredPosition=new Vector2(-216,8);examples[1].sizeDelta=new Vector2(376,260);
    action=Preview(1,"shoot",new Vector2(0,50),Vector2.one*125);
    meter=Rect("Actual shot timing example",examples[1],new Vector2(0,-55),new Vector2(326,34)).gameObject.AddComponent<TutorialMeterGraphic>();meter.raycastTarget=false;meter.timing=sport==SportId.Basketball;
    Caption(examples[1],sport==SportId.Basketball?"RELEASE IN GREEN":"HOLD  →  RELEASE",new Vector2(0,-103),new Vector2(350,32),21);
    examples[2].anchoredPosition=new Vector2(-216,8);examples[2].sizeDelta=new Vector2(376,260);
    Preview(2,"shoot",new Vector2(-88,66),Vector2.one*100);Preview(2,sport==SportId.Football?"tackle":"pass",new Vector2(92,66),Vector2.one*100);
    foreach(var source in examples[2].GetComponentsInChildren<ControlDesignSource>())if(source.Id=="shoot"){source.View.Art.sprite=ControlArt.Get(sport==SportId.Football?"Pressure":"Guard");ControlDesigns.Caption(source.View.Label,sport==SportId.Football?"PRESSURE":"GUARD",.72f);if(source.View.Hint)source.View.Hint.text="HOLD";}else if(sport==SportId.Basketball){source.View.Art.sprite=ControlArt.Get("Steal");ControlDesigns.Caption(source.View.Label,"STEAL");}
    if(sport==SportId.Football){Prop(0,FootballPropHUD.Icon(SportsProp.Turbo),"TURBO");Prop(1,FootballPropHUD.Icon(SportsProp.Portal),"PORTAL");Prop(2,FootballPropHUD.Icon(SportsProp.Wall),"WALL");}
    else {Prop(0,ControlArt.Prop(SportsProp.LockOn),"LOCK-ON");Prop(1,ControlArt.Prop(SportsProp.Turbo),"TURBO");Prop(2,ControlArt.Prop(SportsProp.Portal),"PORTAL");}
   }
   var win=examples[3];Icon(win,"Winning score",sport==SportId.Football?FootballUIStyle.Icon(FootballIcon.Ball):BasketballUIStyle.Icon(sport==SportId.Golf?BasketballIcon.Points:BasketballIcon.Ball),new Vector2(-121,6),Vector2.one*62);
   Caption(win,sport==SportId.Golf?"FEWEST STROKES":sport==SportId.Football?"1 GOAL = 1":"2  OR  3 POINTS",new Vector2(46,14),new Vector2(240,42),23);
   Caption(win,sport==SportId.Golf?"5 HOLES  ·  ONE SHARED ROUTE":"TEAM SCORE WINS",new Vector2(28,-31),new Vector2(298,30),18);
  }
  protected override void AnimateExamples(){
   float t=MenuPreferences.ReducedMotion?.55f:Mathf.PingPong(Time.unscaledTime*.34f,1);
   if(stick!=null&&stick.Thumb)stick.Thumb.anchoredPosition=new Vector2(Mathf.Sin(t*Mathf.PI*2)*12,Mathf.Cos(t*Mathf.PI*2)*10);
   if(meter){meter.value=t;meter.SetVerticesDirty();}
   if(power!=null&&power.Thumb){power.Thumb.anchoredPosition=new Vector2(0,Mathf.Lerp(ControlDesigns.PowerTop(power.Root),ControlDesigns.PowerBottom(power.Root),t));if(touch){var at=power.Root.TransformPoint(power.Thumb.anchoredPosition);touch.position=at;touch.localScale=Vector3.one*(.9f+.12f*t);}}
   for(int i=0;i<floating.Count;i++){var r=floating[i];r.anchoredPosition=new Vector2((i-1)*112,-66+(MenuPreferences.ReducedMotion?0:Mathf.Sin(Time.unscaledTime*2+i)*3));}
   foreach(var (badge,index) in eventBadges)badge.color=new Color(1,1,1,MenuPreferences.ReducedMotion?1:.92f+.08f*Mathf.Sin(Time.unscaledTime*1.8f+index));
  }
  protected override void LateUpdate(){
   base.LateUpdate();if(embedded)return;var app=AppRoot.Instance;var view=PlayerView.Instance;
   bool context=app&&app.Exploring&&app.SelectedSport==sport&&view&&view.active&&view.target&&!view.target.inTransit&&!TutorialSession.Active;
   if(!context){HideUnread();arrivedAt=-1;return;}
#if UNITY_EDITOR || DEVELOPMENT_BUILD
   if(DevelopmentProbe.TurnCommandActive&&!ReviewEntry)return;
#endif
   bool busy=StartupWelcome.Blocking||TeleportSequence.Busy||RoomTeleport.Busy||RoomMatchStart.Travelling||FootballMVP.Presenting||BasketballMVP.Presenting||PostMatchFlow.Instance&&PostMatchFlow.Instance.Visible;
   if(busy){HideUnread();arrivedAt=-1;return;}
   bool menu=sport==SportId.Football?FootballHUD.Instance&&FootballHUD.Instance.MenuOpen:sport==SportId.Basketball?BasketballHUD.Instance&&BasketballHUD.Instance.MenuOpen:GolfHUD.Instance&&GolfHUD.Instance.IslandMenu&&GolfHUD.Instance.IslandMenu.Visible;
   if(menu||!Visible&&view.InputBlocked){arrivedAt=-1;return;}
   if(arrivedAt<0)arrivedAt=Time.unscaledTime;
   if(!Visible&&!HasBeenSeen&&Time.unscaledTime-arrivedAt>.35f)Open();
  }
  protected override void OnDestroy(){if(!embedded&&Find(sport)==this)popups.Remove(sport);base.OnDestroy();}
 }
 // A passive, touch-through example: it never drives the live shot meter.
 public sealed class TutorialMeterGraphic:MaskableGraphic {
  public float value;public bool timing;
  protected override void OnPopulateMesh(VertexHelper v){v.Clear();var r=rectTransform.rect;CovePlate.Round(v,r,r.height*.5f,CoveUI.Ink);var rail=new Rect(r.xMin+5,r.yMin+5,r.width-10,r.height-10);CovePlate.Round(v,rail,rail.height*.5f,CoveUI.Hex("4E737E"));
   if(timing)CovePlate.Round(v,new Rect(rail.x+rail.width*.6f,rail.y,rail.width*.22f,rail.height),rail.height*.3f,CoveUI.Mint);else CovePlate.Round(v,new Rect(rail.x,rail.y,rail.width*Mathf.Max(.01f,value),rail.height),rail.height*.5f,CoveUI.Gold);
   var p=new Vector2(Mathf.Lerp(rail.xMin+7,rail.xMax-7,value),rail.center.y);CovePlate.Round(v,new Rect(p-Vector2.one*13,Vector2.one*26),13,CoveUI.Ink);CovePlate.Round(v,new Rect(p-Vector2.one*9,Vector2.one*18),9,CoveUI.Cream);
  }
 }
}
