using System;
using UnityEngine;
using UnityEngine.UI;
using static WhatTheFish.CoveUI;

namespace WhatTheFish {
 public sealed partial class StorybookMenu {
  // Stored on this device only. An interrupted visit is shown again at next login.
  const string FirstVisitKey="menu.firstVisitGuide.v1";
  GameObject firstVisitRoot;RectTransform firstVisitDesign;int firstVisitStep;FirstVisitNarration firstVisitNarration;
  struct IntroductionStep {
   public string speech;public Rect target;public bool top;
   public IntroductionStep(string value,float x,float y,float w,float h,bool dialogAtTop=false){speech=value;target=new Rect(x,y,w,h);top=dialogAtTop;}
  }
  static readonly IntroductionStep[] Introduction={
   new IntroductionStep("Welcome aboard! I'll show you around. Tap anywhere to continue.",0,0,0,0),
   new IntroductionStep("Create a room to host friends. Choose a starting sport and share your room code.",883,205,300,70),
   new IntroductionStep("Join a room with your host's code or scan their QR invitation.",1203,205,305,70),
   new IntroductionStep("Fishing takes you to the lagoon to catch fish. Choose offline exploration or play against bots.",64,390,350,310),
   new IntroductionStep("Basketball takes you to the court. Explore offline or challenge bots.",438,390,350,310),
   new IntroductionStep("Football takes you to the pitch. Explore offline or challenge bots.",812,390,350,310),
   new IntroductionStep("Golf takes you to the course. Explore offline or challenge bots.",1186,390,350,310),
   new IntroductionStep("In one room, your crew can travel together between all four islands.",64,176,780,128),
   new IntroductionStep("Rewards opens your daily calendar. Tap the available day to claim coins.",584,48,276,68),
   new IntroductionStep("This balance shows your coins. You can use them for available customization items.",340,48,224,68),
   new IntroductionStep("Friends lets you find players, manage requests and see your friends.",880,48,185,68),
   new IntroductionStep("Your profile is where you can edit your player details and picture.",1085,48,248,68),
   new IntroductionStep("Settings lets you adjust language, sound and other preferences.",1353,48,183,68),
   new IntroductionStep("Customize venue lets you choose a sport and make its venue your own.",64,730,724,124,true),
   new IntroductionStep("Tutorials teaches the controls and rules for each sport. Have fun!",812,730,724,124,true)
  };
  public void BeginFirstVisitGuide(){
   if(Page!="home"||firstVisitRoot||PlayerPrefs.GetInt(FirstVisitKey,0)!=0)return;
   firstVisitStep=0;firstVisitRoot=Rect(shell,"First visit guide",0,0,1600,900).gameObject;Stretch((RectTransform)firstVisitRoot.transform);
   firstVisitNarration=firstVisitRoot.AddComponent<FirstVisitNarration>();
   var catchAll=firstVisitRoot.AddComponent<Image>();catchAll.color=new Color(0,0,0,.001f);catchAll.raycastTarget=true;
   var advance=firstVisitRoot.AddComponent<UnityEngine.UI.Button>();advance.transition=Selectable.Transition.None;advance.targetGraphic=catchAll;advance.onClick.AddListener(()=>{CoveSound.Play();AdvanceFirstVisit();});
   firstVisitDesign=Rect(firstVisitRoot.transform,"First visit design",0,0,1600,900);firstVisitDesign.anchorMin=firstVisitDesign.anchorMax=firstVisitDesign.pivot=new Vector2(.5f,.5f);firstVisitDesign.anchoredPosition=Vector2.zero;
   LayoutScreen();DrawFirstVisit();firstVisitNarration.PlayStep(firstVisitStep);
  }
  void AdvanceFirstVisit(){if(++firstVisitStep>=Introduction.Length){FinishFirstVisit();return;}DrawFirstVisit();firstVisitNarration.PlayStep(firstVisitStep);}
  void FinishFirstVisit(){firstVisitNarration?.Stop();PlayerPrefs.SetInt(FirstVisitKey,1);PlayerPrefs.Save();if(firstVisitRoot)Destroy(firstVisitRoot);firstVisitRoot=null;firstVisitDesign=null;}
  void DrawFirstVisit(){
   foreach(Transform child in firstVisitDesign){child.gameObject.SetActive(false);Destroy(child.gameObject);}
   var step=Introduction[firstVisitStep];var t=IntroductionTarget();var shade=new Color(0.01f,.06f,.09f,.8f);
   float width=layoutWidth,height=layoutHeight;
   if(t.width<=0){Plate(firstVisitDesign,"Guide shade",0,0,width,height,shade,0,0);}
   else {
    float x=Mathf.Max(0,t.x-6),y=Mathf.Max(0,t.y-6),right=Mathf.Min(width,t.xMax+6),bottom=Mathf.Min(height,t.yMax+6);
    Plate(firstVisitDesign,"Guide shade top",0,0,width,y,shade,0,0);Plate(firstVisitDesign,"Guide shade bottom",0,bottom,width,height-bottom,shade,0,0);
    Plate(firstVisitDesign,"Guide shade left",0,y,x,bottom-y,shade,0,0);Plate(firstVisitDesign,"Guide shade right",right,y,width-right,bottom-y,shade,0,0);
    Plate(firstVisitDesign,"Guide highlight top",x,y,right-x,4,Gold,0,0);Plate(firstVisitDesign,"Guide highlight bottom",x,bottom-4,right-x,4,Gold,0,0);
    Plate(firstVisitDesign,"Guide highlight left",x,y,4,bottom-y,Gold,0,0);Plate(firstVisitDesign,"Guide highlight right",right-4,y,4,bottom-y,Gold,0,0);
   }
   float panelW=Mathf.Min(1260,width-96),panelX=(width-panelW)/2,dialogY=t.width>0&&t.yMax>height-260?80:height-222;
   var panel=Plate(firstVisitDesign,"Guide dialogue",panelX,dialogY,panelW,206,Background,16,2);panel.borderColor=Gold;panel.border=2;
   Text(panel.transform,step.speech,165,12,panelW-207,134,30,false,Ink,TextAnchor.MiddleLeft);
   Text(panel.transform,"Tap anywhere to continue",165,159,panelW-360,35,21,true,Gold);
   Text(panel.transform,(firstVisitStep+1)+" / "+Introduction.Length,panelW-190,159,150,35,21,true,Muted,TextAnchor.MiddleRight);
   var portrait=Rect(firstVisitDesign,"Guide character",panelX+16,dialogY+8,107,160).gameObject.AddComponent<RawImage>();portrait.texture=Resources.Load<Texture2D>("PlayerPortraits/Character-01");portrait.raycastTarget=false;
   Text(panel.transform,"AI-generated voice",8,175,145,24,14,false,Muted,TextAnchor.MiddleCenter);
   var skip=Button(firstVisitDesign,"Skip introduction","Skip",width-228,t.xMax>width-240&&t.y<120?230:10,180,58,Raised,FinishFirstVisit,CoveSymbol.None,24);
   skip.transform.SetAsLastSibling();
  }
  Rect IntroductionTarget(){
   if(firstVisitStep==0)return new Rect();
   string[] names={"","Create room","Join room","Island - Fishing","Island - Basketball","Island - Football","Island - Golf","Play together","Rewards","Coin balance","Friends icon","Player profile","Settings","Customize venue","Tutorials"};
   var target=content.Find(names[firstVisitStep]) as RectTransform;if(!target)return new Rect();
   var corners=new Vector3[4];target.GetWorldCorners(corners);var a=firstVisitDesign.InverseTransformPoint(corners[0]);var b=firstVisitDesign.InverseTransformPoint(corners[2]);
   return new Rect(a.x+layoutWidth/2,layoutHeight/2-b.y,b.x-a.x,b.y-a.y);
  }
 }
}
