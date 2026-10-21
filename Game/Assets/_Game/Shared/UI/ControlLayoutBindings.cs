using UnityEngine;

namespace WhatTheFish {
 public sealed partial class BasketballHUD {
  public ControlLayoutEditor LayoutEditor {get;private set;}
  void CreateControlLayout(){
   LayoutEditor=ControlLayoutEditor.Create(root,SportId.Basketball);
   LayoutEditor.Register("move","Move",(RectTransform)AppRoot.Instance.view.stick.transform,BasketballUIStyle.Icon(BasketballIcon.Move));
   LayoutEditor.Register("shoot","Shoot / Guard",(RectTransform)shoot.transform,BasketballUIStyle.Icon(BasketballIcon.Shoot),(RectTransform)shoot.guide.transform);
   LayoutEditor.Register("pass","Pass / Steal",(RectTransform)pass.transform,BasketballUIStyle.Icon(BasketballIcon.Pass),(RectTransform)pass.guide.transform);
   LayoutEditor.Register("jump","Jump",(RectTransform)jump.transform,BasketballUIStyle.Icon(BasketballIcon.Jump));
   LayoutEditor.Register("ability","Ability",(RectTransform)props.transform,SportsUIStyle.PropIcon(SportsProp.Turbo));
  }
  public void OpenLayout(){if(!MenuOpen)ShowSettings();LayoutEditor.Open(()=>overlay.gameObject);}
 }
 public sealed partial class FootballHUD {
  public ControlLayoutEditor LayoutEditor {get;private set;}
  void CreateControlLayout(){
   LayoutEditor=ControlLayoutEditor.Create(root,SportId.Football);
   LayoutEditor.Register("move","Move",(RectTransform)AppRoot.Instance.view.stick.transform,BasketballUIStyle.Icon(BasketballIcon.Move));
   LayoutEditor.Register("shoot","Shoot / Pressure",(RectTransform)kick.transform,FootballUIStyle.Icon(FootballIcon.Ball),(RectTransform)kick.guide.transform);
   LayoutEditor.Register("dash","Dash",(RectTransform)dash.transform.parent,FootballUIStyle.Icon(FootballIcon.Dash));
   LayoutEditor.Register("pass","Pass",(RectTransform)jump.transform,FootballUIStyle.Icon(FootballIcon.Target));
   LayoutEditor.Register("tackle","Tackle",(RectTransform)tackle.transform,FootballUIStyle.Icon(FootballIcon.Tackle));
   LayoutEditor.Register("ability","Ability",(RectTransform)FootballPropHUD.Instance.transform,FootballPropHUD.Icon(SportsProp.Wall));
  }
  public void OpenLayout(){if(!MenuOpen)ShowSettings();LayoutEditor.Open(()=>overlay.gameObject);}
 }
 public sealed partial class GolfHUD {
  public ControlLayoutEditor LayoutEditor {get;private set;}
  void CreateControlLayout(){
   LayoutEditor=ControlLayoutEditor.Create(page,SportId.Golf);
   LayoutEditor.Register("move","Move",frames[moveControl],BasketballUIStyle.Icon(BasketballIcon.Move));
   LayoutEditor.Register("jump","Jump",frames[jumpControl],Resources.Load<Sprite>("GolfUI/Badges/Jump"));
   LayoutEditor.Register("power","Shot power",frames[ActionRect],Resources.Load<Sprite>("GolfUI/PowerSlider/Knob"));
   LayoutEditor.Register("aim","Aim / Cancel",frames[(RectTransform)swing.aimButton.transform],BasketballUIStyle.Icon(BasketballIcon.Eye));
   LayoutEditor.Register("start","Start game",frames[(RectTransform)swing.startButton.transform],Resources.Load<Sprite>("GolfUI/StartButton/StartGame"));
   foreach(var cart in carts)LayoutEditor.Register(cart.summon?"buggy":"drive",cart.summon?"Buggy / Recall":"Drive / Exit",frames[(RectTransform)cart.transform],Resources.Load<Sprite>("GolfUI/CartButtons/"+(cart.summon?"Buggy":"Drive")));
  }
 }
 public sealed partial class FishingHUD {
  public ControlLayoutEditor LayoutEditor {get;private set;}
  void CreateControlLayout(){
   LayoutEditor=ControlLayoutEditor.Create(page,SportId.Fishing);
   LayoutEditor.Register("move","Move",frames[moveControl],BasketballUIStyle.Icon(BasketballIcon.Move));
   LayoutEditor.Register("jump","Jump",frames[jumpControl],BasketballUIStyle.Icon(BasketballIcon.Jump));
   LayoutEditor.Register("cast","Cast / Reel right",frames[(RectTransform)transform],Resources.Load<Sprite>("FishingUI/ReelButton"));
   LayoutEditor.Register("left","Reel left",frames[leftControl],BasketballUIStyle.Icon(BasketballIcon.Move));
   LayoutEditor.Register("cancel","Cancel cast",frames[(RectTransform)cancel.transform],BasketballUIStyle.Icon(BasketballIcon.Close));
   GetComponent<FishingAbilityHUD>().RegisterLayout(LayoutEditor);
  }
 }
}
