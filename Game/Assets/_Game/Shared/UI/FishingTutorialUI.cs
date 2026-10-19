using System;
using UnityEngine;
using UnityEngine.UI;
namespace WhatTheFish {
 // The entry popup and How to Play instantiate this exact view and content.
 public sealed class FishingTutorialUI:MonoBehaviour {
  public static readonly string[] Titles={"CHOOSE 2 SKILLS","CAST & CATCH","USE YOUR GEAR","PLAY TO WIN"};
  public static readonly string[] Copy={
   "After this guide, tap CHOOSE 2 SKILLS.\nPick two favourites, then Ready.\nCarry one item. The host starts when all are ready.",
   "Aim at a free fish. Tap Cast when teal.\nTap Hook when ! appears.\nHold the glowing Left or Right button.\nSwitch when the glow changes sides.\nRelease on red rope; hold again when cool.",
   "Tap the shore bag to take one favourite.\nOr recover treasure at a pier: 6 seconds.\nTap your gear icon to use it. The meter shows time left.",
   "Small 2  /  Medium 5  /  Large 10 points.\nWanted fish: +3 once per minute.\nRound: most points after 3 minutes.\nCrew: shared target = 20 x players. All sizes; everyone catches."
  };
  public const string SeenKey="fishing.tutorial.hold-glow.v1";
  RectTransform frame,overlay,card;Text title,body,count;Image picture;Button next,back;Text nextLabel;Action closed;int step;bool embedded,entryObserved;
  readonly RectTransform[] examples=new RectTransform[4];FishingStickerGraphic demoLeft,demoRight,demoCatch,demoTension,demoDuration;FishingFightGuide demoGuide;Text demoRightLabel;
  public bool Visible=>overlay&&overlay.gameObject.activeInHierarchy;
  public int Step=>step;
  public string Body=>body?body.text:"";
#if UNITY_EDITOR || DEVELOPMENT_BUILD
  internal bool ReviewEntry;
#endif
  public static FishingTutorialUI Create(RectTransform parent,bool embedded=false,Action closed=null){var node=GolfHUDStyle.Rect("Fishing illustrated tutorial",parent,new Vector2(.5f,.5f),Vector2.zero,Vector2.zero);node.anchorMin=Vector2.zero;node.anchorMax=Vector2.one;node.offsetMin=node.offsetMax=Vector2.zero;var ui=node.gameObject.AddComponent<FishingTutorialUI>();ui.frame=parent;ui.embedded=embedded;ui.closed=closed;ui.Build();if(embedded)ui.Open();return ui;}
  RectTransform Rect(string name,Transform parent,Vector2 pos,Vector2 size)=>GolfHUDStyle.Rect(name,parent,new Vector2(.5f,.5f),pos,size);
  Text Text(string name,Transform parent,string value,Vector2 pos,Vector2 size,int points){var t=Rect(name,parent,pos,size).gameObject.AddComponent<Text>();t.font=points<=23?SportsUIStyle.TextFont:SportsUIStyle.DisplayFont;t.text=value;t.fontSize=points;t.color=GolfHUDStyle.Ink;t.alignment=TextAnchor.MiddleCenter;t.raycastTarget=false;t.resizeTextForBestFit=true;t.resizeTextMinSize=Mathf.Min(points,points<=23?20:24);t.resizeTextMaxSize=points;return t;}
  Button Button(string name,Vector2 pos,Vector2 size,string caption,Action action,out Text label){var r=Rect(name,card,pos,size);var graphic=r.gameObject.AddComponent<FishingStickerGraphic>();graphic.kind=FishingStickerGraphic.Kind.Panel;graphic.tint=GolfHUDStyle.Cream;graphic.UseSolidPanel();graphic.raycastTarget=true;var b=r.gameObject.AddComponent<Button>();b.targetGraphic=graphic;b.transition=Selectable.Transition.None;label=Text(name+" caption",r,caption,Vector2.zero,size-new Vector2(12,8),24);b.onClick.AddListener(()=>{GolfFishingAudio.PlayMenu("ui-select");action();});r.gameObject.AddComponent<BasketballButtonFeedback>();r.gameObject.AddComponent<GolfFishingButtonSound>();return b;}
  void Build(){
   overlay=Rect("Fishing tutorial overlay",transform,Vector2.zero,Vector2.zero);overlay.anchorMin=Vector2.zero;overlay.anchorMax=Vector2.one;overlay.offsetMin=overlay.offsetMax=Vector2.zero;var shade=overlay.gameObject.AddComponent<Image>();shade.color=embedded?Color.clear:new Color(.025f,.11f,.15f,.6f);shade.raycastTarget=!embedded;
   card=Rect("Fishing tutorial card",overlay,new Vector2(0,embedded?-75:0),new Vector2(860,452));var panel=card.gameObject.AddComponent<FishingStickerGraphic>();panel.kind=FishingStickerGraphic.Kind.Panel;panel.tint=GolfHUDStyle.Cream;panel.cornerRadius=28;panel.UseSolidPanel();panel.raycastTarget=true;
   title=Text("Fishing tutorial title",card,Titles[0],new Vector2(0,175),new Vector2(780,46),32);
   picture=Rect("Fishing tutorial illustration",card,new Vector2(-216,69),new Vector2(350,142)).gameObject.AddComponent<Image>();picture.preserveAspect=true;picture.raycastTarget=false;
   BuildExamples();
   body=Text("Fishing tutorial instructions",card,Copy[0],new Vector2(198,7),new Vector2(380,246),23);body.alignment=TextAnchor.MiddleLeft;
   count=Text("Fishing tutorial step",card,"1 / 4",new Vector2(-74,-175),new Vector2(90,42),24);
   Button("Fishing tutorial skip",new Vector2(-312,-175),new Vector2(180,48),embedded?"DONE":"SKIP",Close,out _);
   back=Button("Fishing tutorial back",new Vector2(76,-175),new Vector2(136,48),"BACK",()=>Show(step-1),out _);
   next=Button("Fishing tutorial next",new Vector2(272,-175),new Vector2(212,48),"NEXT",()=>{if(step<3)Show(step+1);else Close();},out nextLabel);next.GetComponent<FishingStickerGraphic>().Refresh(1,GolfHUDStyle.Gold);
   overlay.gameObject.SetActive(false);
  }
  public void Open(){if(!overlay)return;Show(0);overlay.gameObject.SetActive(true);transform.SetAsLastSibling();Layout();}
  void Show(int value){step=Mathf.Clamp(value,0,3);title.text=Titles[step];body.text=Copy[step];count.text=(step+1)+" / 4";picture.sprite=FishingGearArt.Tutorial(step);for(int i=0;i<examples.Length;i++)examples[i].gameObject.SetActive(i==step);back.interactable=step>0;nextLabel.text=step<3?"NEXT":"LET'S FISH!";}
  Image Icon(Transform parent,string name,Sprite sprite,Vector2 position,Vector2 size){var image=Rect(name,parent,position,size).gameObject.AddComponent<Image>();image.sprite=sprite;image.preserveAspect=true;image.raycastTarget=false;return image;}
  FishingStickerGraphic Graphic(Transform parent,string name,Vector2 position,Vector2 size,FishingStickerGraphic.Kind kind){var g=Rect(name,parent,position,size).gameObject.AddComponent<FishingStickerGraphic>();g.kind=kind;g.raycastTarget=false;return g;}
  void BuildExamples(){
   // These are the same sprites and mesh widgets used in the live HUD. Read-only
   // examples never send gameplay inputs or create a separate set of controls.
   for(int i=0;i<examples.Length;i++)examples[i]=Rect("Tutorial gameplay example "+i,card,new Vector2(-216,-71),new Vector2(376,132));
   for(int i=0;i<FishingAbilityRules.Roster.Length;i++){var a=FishingAbilityRules.Roster[i];var r=Rect("Tutorial skill "+a,examples[0],new Vector2((i%3-1)*122,32-i/3*64),new Vector2(118,62));Icon(r,"Actual skill icon",FishingGearArt.Ability(a),new Vector2(0,15),Vector2.one*36);Text("Actual skill name",r,FishingAbilityRules.Name(a),new Vector2(0,-18),new Vector2(116,32),16);if(a==FishingAbility.Sonar||a==FishingAbility.QuickLure)Icon(r,"Selected skill",SportsUIStyle.Check,new Vector2(-20,29),Vector2.one*15).color=GolfHUDStyle.Ink;}
   demoLeft=Graphic(examples[1],"Actual left control",new Vector2(-143,9),Vector2.one*82,FishingStickerGraphic.Kind.Panel);demoLeft.UseIllustration("SteerLit");Text("Actual left label",demoLeft.transform,"LEFT",new Vector2(0,-20),new Vector2(68,25),18).color=Color.white;
   demoRight=Graphic(examples[1],"Actual right control",new Vector2(143,9),Vector2.one*82,FishingStickerGraphic.Kind.Panel);demoRight.UseIllustration("ReelButton");demoRightLabel=Text("Actual right label",demoRight.transform,"REEL",new Vector2(0,-20),new Vector2(68,25),18);demoRightLabel.color=Color.white;
   demoGuide=Rect("Actual fish pull meter",examples[1],new Vector2(0,43),new Vector2(168,26)).gameObject.AddComponent<FishingFightGuide>();demoGuide.raycastTarget=false;
   demoCatch=Graphic(examples[1],"Actual Catch meter",new Vector2(0,4),new Vector2(164,24),FishingStickerGraphic.Kind.Gauge);Text("Catch demo label",demoCatch.transform,"CATCH",Vector2.zero,new Vector2(150,22),16).color=Color.white;
   demoTension=Graphic(examples[1],"Actual Tension meter",new Vector2(0,-35),new Vector2(164,24),FishingStickerGraphic.Kind.Gauge);Text("Tension demo label",demoTension.transform,"TENSION",Vector2.zero,new Vector2(150,22),16).color=Color.white;
   for(int i=0;i<3;i++){var r=Rect("Tutorial gear button "+i,examples[2],new Vector2((i-1)*122,6),new Vector2(100,110));var plate=r.gameObject.AddComponent<FishingStickerGraphic>();plate.kind=FishingStickerGraphic.Kind.Panel;plate.cornerRadius=40;plate.UseSolidPanel();plate.raycastTarget=false;Icon(r,"Actual gear icon",i==0?FishingGearArt.Icon(0):i==1?FishingGearArt.Ability(FishingAbility.Sonar):FishingGearArt.Icon(1),new Vector2(0,14),Vector2.one*64);Text("Actual gear caption",r,i==0?"GET":i==1?"USE":"6s",new Vector2(0,-28),new Vector2(88,25),18);if(i==1)demoDuration=Graphic(r,"Actual ability duration",new Vector2(0,-44),new Vector2(74,6),FishingStickerGraphic.Kind.Gauge);}
   for(int i=0;i<3;i++){var fish=Graphic(examples[3],"Actual fish points "+i,new Vector2((i-1)*112,23),new Vector2(46+i*8,42+i*7),FishingStickerGraphic.Kind.Fish);fish.Decorate();Text("Fish points "+i,examples[3],new[]{"2 POINTS","5 POINTS","10 POINTS"}[i],new Vector2((i-1)*112,-24),new Vector2(106,32),18);}
  }
  void AnimateExamples(){if(!Visible)return;float phase=MenuPreferences.ReducedMotion?.7f:Mathf.Repeat(Time.unscaledTime*.18f,1);float tension=.28f+.42f*Mathf.Sin(phase*Mathf.PI);demoCatch.Refresh(.3f+phase*.65f,LocalProfile.Hex("4CDED6"));demoTension.Refresh(tension,Color.Lerp(GolfHUDStyle.Gold,LocalProfile.Hex("FF7659"),Mathf.InverseLerp(.4f,.8f,tension)));demoDuration.Refresh(1-phase,LocalProfile.Hex("4CDED6"));double now=Mathf.Repeat(Time.unscaledTime,(float)FishingState.TimingPeriod*6);var p=new FishingPlayerRecord{phase=FishingPhase.Reeling,owner=1,fish=0,phaseAt=0,held=true};bool left=FishingState.CounterSide(p,now)<0;p.steer=(sbyte)(left?-1:1);p.tension=tension;demoGuide.Tick(p,now);demoLeft.gameObject.SetActive(true);demoLeft.UseIllustration(left?"SteerLit":"SteerIdle");demoRight.UseIllustration(left?"SteerIdle":"SteerLit",false,true);demoRightLabel.text="RIGHT";}


  public void Close(){if(overlay)overlay.gameObject.SetActive(false);if(!embedded){PlayerPrefs.SetInt(SeenKey,1);PlayerPrefs.Save();}closed?.Invoke();}
  void Layout(){if(!frame||!card)return;float scale=Mathf.Min(embedded?1.15f:1.35f,frame.rect.width*.88f/860,frame.rect.height*(embedded?.68f:.88f)/452);card.localScale=Vector3.one*Mathf.Max(.1f,scale);}
  void LateUpdate(){Layout();AnimateExamples();if(embedded)return;var game=FishingGame.Instance;var view=PlayerView.Instance;var p=game&&view?game.Player(view.target):null;bool context=game&&game.Context&&view&&view.active&&p.HasValue;
   if(!context){if(overlay)overlay.gameObject.SetActive(false);entryObserved=false;return;}
#if UNITY_EDITOR || DEVELOPMENT_BUILD
   if(DevelopmentProbe.TurnCommandActive&&!ReviewEntry)return;
#endif
   if(!entryObserved&&game.State.Match.phase==FishingRoundPhase.Practice&&!p.Value.Busy&&!p.Value.ready){entryObserved=true;if(PlayerPrefs.GetInt(SeenKey,0)==0)Open();}
   if(p.Value.Busy&&overlay)overlay.gameObject.SetActive(false);
  }
  public bool FitsSafeFrame(){if(!Visible)return true;var corners=new Vector3[4];card.GetWorldCorners(corners);var bounds=frame.rect;foreach(var c in corners){var p=frame.InverseTransformPoint(c);if(p.x<bounds.xMin-1||p.x>bounds.xMax+1||p.y<bounds.yMin-1||p.y>bounds.yMax+1)return false;}return true;}
 }
}
