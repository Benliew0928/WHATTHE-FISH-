using System;
using UnityEngine;
using UnityEngine.UI;
namespace WhatTheFish {
 // The entry popup and How to Play instantiate this exact view and content.
 public sealed class FishingTutorialUI:IslandTutorialUI {
  public static readonly string[] Titles={"PICK 1 OF 3","CAST & CATCH","USE YOUR SKILL","PLAY TO WIN"};
  public static readonly string[] Copy={
   "The host starts the round. Everyone sees the same three skills.\nPick one for this round.\nYour skill can be reused after its cooldown.",
   "Aim at a free fish. Tap Cast when teal.\nTap Hook when ! appears.\nHold the glowing Left or Right button.\nSwitch when the glow changes sides.\nRelease on red rope; hold again when cool.",
   "Your skill button sits beside Cast / Right.\nIt appears when the skill can be used.\nBright: tap to use. Dim clock: recharging.\nA bright draining ring shows the active effect.",
   "Small 2  /  Medium 5  /  Large 10 points.\nWanted fish: +3 once per minute.\nMost points after 3 minutes wins.\nThe host can start another round."
  };
  public const string SeenKey="fishing.tutorial.round-skill.v1";
  bool entryObserved;
  FishingStickerGraphic demoLeft,demoRight,demoCatch,demoTension;FishingSkillGraphic demoCooldown;FishingFightGuide demoGuide;Text demoRightLabel;
  public override SportId Sport=>SportId.Fishing;
  protected override string[] PageTitles=>Titles;
  protected override string[] PageCopy=>Copy;
  protected override string SeenPreference=>SeenKey;
  protected override string FinishCaption=>"LET'S FISH!";
#if UNITY_EDITOR || DEVELOPMENT_BUILD
  internal bool ReviewEntry;
#endif
  public static FishingTutorialUI Create(RectTransform parent,bool embedded=false,Action closed=null){var node=GolfHUDStyle.Rect("Fishing illustrated tutorial",parent,new Vector2(.5f,.5f),Vector2.zero,Vector2.zero);CoveUI.Stretch(node);var ui=node.gameObject.AddComponent<FishingTutorialUI>();ui.Initialize(parent,embedded,closed);return ui;}
  protected override Sprite Illustration(int page)=>page==1||page==3?FishingGearArt.Tutorial(page):null;
  FishingStickerGraphic Graphic(Transform parent,string name,Vector2 position,Vector2 size,FishingStickerGraphic.Kind kind){var g=Rect(name,parent,position,size).gameObject.AddComponent<FishingStickerGraphic>();g.kind=kind;g.raycastTarget=false;return g;}
  protected override void BuildExamples(){
   // These are the same sprites and mesh widgets used in the live HUD. Read-only
   // examples never send gameplay inputs or create a separate set of controls.
   for(int i=0;i<examples.Length;i++)examples[i]=Rect("Tutorial gameplay example "+i,content,new Vector2(-216,-71),new Vector2(376,132));
   examples[0].anchoredPosition=new Vector2(-216,12);examples[0].sizeDelta=new Vector2(376,260);
   var sample=new[]{FishingAbility.SteelWire,FishingAbility.Sonar,FishingAbility.FishingBuddy};
   for(int i=0;i<3;i++){var a=sample[i];var r=Rect("Tutorial skill choice "+i,examples[0],new Vector2((i-1)*122,0),new Vector2(110,178));var plate=r.gameObject.AddComponent<BasketballPlate>();plate.color=i==0?CoveUI.Sky:CoveUI.Cream;plate.radius=Vector2.one*16;plate.border=i==0?3:2;plate.raycastTarget=false;var icon=Rect("Actual skill icon",r,new Vector2(0,36),Vector2.one*66);FishingSkillGraphic.Icon(icon,a);Text("Actual skill name",r,FishingAbilityRules.Name(a),new Vector2(0,-26),new Vector2(104,46),16);if(i==0)Icon(r,"Selected skill",SportsUIStyle.Check,new Vector2(33,68),Vector2.one*20);}
   demoLeft=Graphic(examples[1],"Actual left control",new Vector2(-143,9),Vector2.one*82,FishingStickerGraphic.Kind.Panel);demoLeft.UseIllustration("SteerLit");Text("Actual left label",demoLeft.transform,"LEFT",new Vector2(0,-20),new Vector2(68,25),18).color=Color.white;
   demoRight=Graphic(examples[1],"Actual right control",new Vector2(143,9),Vector2.one*82,FishingStickerGraphic.Kind.Panel);demoRight.UseIllustration("ReelButton");demoRightLabel=Text("Actual right label",demoRight.transform,"REEL",new Vector2(0,-20),new Vector2(68,25),18);demoRightLabel.color=Color.white;
   demoGuide=Rect("Actual fish pull meter",examples[1],new Vector2(0,43),new Vector2(168,26)).gameObject.AddComponent<FishingFightGuide>();demoGuide.raycastTarget=false;
   demoCatch=Graphic(examples[1],"Actual Catch meter",new Vector2(0,4),new Vector2(164,24),FishingStickerGraphic.Kind.Gauge);Text("Catch demo label",demoCatch.transform,"CATCH",Vector2.zero,new Vector2(150,22),16).color=Color.white;
   demoTension=Graphic(examples[1],"Actual Tension meter",new Vector2(0,-35),new Vector2(164,24),FishingStickerGraphic.Kind.Gauge);Text("Tension demo label",demoTension.transform,"TENSION",Vector2.zero,new Vector2(150,22),16).color=Color.white;
   examples[2].anchoredPosition=new Vector2(-216,12);
   for(int i=0;i<3;i++){var r=Rect("Tutorial round skill state "+i,examples[2],new Vector2((i-1)*122,0),new Vector2(98,100));var plate=r.gameObject.AddComponent<BasketballPlate>();plate.color=i==2?CoveUI.Sky:CoveUI.Cream;plate.radius=Vector2.one*32;plate.border=3;plate.raycastTarget=false;var icon=Rect("Actual skill icon",r,new Vector2(0,10),Vector2.one*60);FishingSkillGraphic.Icon(icon,FishingAbility.SteelWire);var ring=Rect("Actual cooldown border",r,Vector2.zero,Vector2.one*98).gameObject.AddComponent<FishingSkillGraphic>();ring.ring=true;ring.raycastTarget=false;ring.Tick(i==0?.35f:1,i==2?CoveUI.Sky:CoveUI.Gold);if(i==0){demoCooldown=ring;var alpha=r.gameObject.AddComponent<CanvasGroup>();alpha.alpha=.42f;}Text("Actual skill state",r,i==0?"6s":i==1?"USE":"ACTIVE",new Vector2(0,-30),new Vector2(86,23),15);Text("Skill state explanation",examples[2],i==0?"COOLDOWN":i==1?"READY":"IN USE",new Vector2((i-1)*122,-75),new Vector2(116,30),15);}
   for(int i=0;i<3;i++){var fish=Graphic(examples[3],"Actual fish points "+i,new Vector2((i-1)*112,23),new Vector2(46+i*8,42+i*7),FishingStickerGraphic.Kind.Fish);fish.Decorate();Text("Fish points "+i,examples[3],new[]{"2 POINTS","5 POINTS","10 POINTS"}[i],new Vector2((i-1)*112,-24),new Vector2(106,32),18);}
  }
  protected override void AnimateExamples(){if(!Visible)return;float phase=MenuPreferences.ReducedMotion?.7f:Mathf.Repeat(Time.unscaledTime*.18f,1);float tension=.28f+.42f*Mathf.Sin(phase*Mathf.PI);demoCatch.Refresh(.3f+phase*.65f,LocalProfile.Hex("4CDED6"));demoTension.Refresh(tension,Color.Lerp(GolfHUDStyle.Gold,LocalProfile.Hex("FF7659"),Mathf.InverseLerp(.4f,.8f,tension)));demoCooldown.Tick(phase,CoveUI.Gold);double now=MenuPreferences.ReducedMotion?0:Mathf.Repeat(Time.unscaledTime,(float)FishingState.TimingPeriod*6);var p=new FishingPlayerRecord{phase=FishingPhase.Reeling,owner=1,fish=0,phaseAt=0,held=true};bool left=FishingState.CounterSide(p,now)<0;p.steer=(sbyte)(left?-1:1);p.tension=tension;demoGuide.Tick(p,now);demoLeft.gameObject.SetActive(true);demoLeft.UseIllustration(left?"SteerLit":"SteerIdle");demoRight.UseIllustration(left?"SteerIdle":"SteerLit",false,true);demoRightLabel.text="RIGHT";}


  protected override void LateUpdate(){if(TutorialSession.Is(SportId.Fishing)){if(overlay)overlay.gameObject.SetActive(false);return;}base.LateUpdate();if(embedded)return;var game=FishingGame.Instance;var view=PlayerView.Instance;var p=game&&view?game.Player(view.target):null;bool context=game&&game.Context&&view&&view.active&&p.HasValue;
   if(!context){if(overlay)overlay.gameObject.SetActive(false);entryObserved=false;return;}
   if(StartupWelcome.Blocking||TeleportSequence.Busy||RoomTeleport.Busy||RoomMatchStart.Travelling||FishingCeremony.Presenting){HideUnread();entryObserved=false;return;}
   if(FishingHUD.Instance&&FishingHUD.Instance.IslandMenu&&FishingHUD.Instance.IslandMenu.Visible)return;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
   if(DevelopmentProbe.TurnCommandActive&&!ReviewEntry)return;
#endif
   if(!Visible&&view.InputBlocked)return;
   if(!entryObserved&&game.State.Match.phase==FishingRoundPhase.Practice&&!p.Value.Busy&&!p.Value.ready){entryObserved=true;if(PlayerPrefs.GetInt(SeenKey,0)==0)Open();}
   if(p.Value.Busy&&overlay)overlay.gameObject.SetActive(false);
  }
 }
}
