using UnityEngine;
using UnityEngine.UI;

namespace WhatTheFish {
 public sealed partial class BasketballHUD {
  RectTransform overlay,settingsPanel,guidePanel,leavePanel;GameObject[] guidePages;Button[] cameraChoices,guideTabs;Text cameraNote,roamText,leaveText,confirmText;Button leaveButton,roamButton;int guideTab;
  void BuildMenus(){
   var backdrop=BasketballUIStyle.Plate(root,"Court menu overlay",0,0,800,450,new Color(.07f,.25f,.31f,.46f),0,0);backdrop.raycastTarget=true;overlay=backdrop.rectTransform;CoveUI.Stretch(overlay);
   settingsPanel=MenuPanel("Court settings",650,422,BasketballIcon.Gear,CloseMenu);
   BasketballUIStyle.Text(settingsPanel,"Camera angle",20,70,590,20,15,true);
   cameraChoices=new Button[3];string[] cameras={"Eye level","Follow","Court view"};BasketballIcon[] art={BasketballIcon.Eye,BasketballIcon.Follow,BasketballIcon.Court};
   for(int i=0;i<3;i++){int mode=i;var b=BasketballUIStyle.Button(settingsPanel,cameras[i],"",20+i*207,98,196,86,()=>SelectCamera(mode),BasketballUIStyle.Paper);cameraChoices[i]=b;BasketballUIStyle.Art(b.transform,art[i],70,5,56,42);BasketballUIStyle.Text(b.transform,cameras[i],4,51,188,28,16,true,TextAnchor.MiddleCenter);CoveUI.Image(b.transform,"Selected camera",SportsUIStyle.Check,166,2,23,22).color=CoveUI.Ink;}
   cameraNote=BasketballUIStyle.Text(settingsPanel,"",20,191,610,26,12);
   BuildAudioSliders();
   var guide=BasketballUIStyle.Button(settingsPanel,"Court guide","",20,304,194,48,OpenGuide);BasketballUIStyle.Art(guide.transform,BasketballIcon.Book,12,9,30,30);BasketballUIStyle.Text(guide.transform,"Court guide",54,4,136,23,16,true);BasketballUIStyle.Text(guide.transform,"Controls & rules",54,26,136,16,11);
   BasketballUIStyle.Button(settingsPanel,"Customize basketball","Ball effects",224,304,194,48,ShowCustomization,CoveUI.Mint,16);BuildCustomization();
   BasketballUIStyle.Button(settingsPanel,"Layout","Layout",428,304,202,48,OpenLayout,CoveUI.Sky,16);
   Footer(settingsPanel,366,650,58);
   roamButton=BasketballUIStyle.Button(settingsPanel,"Free roam","Free roam",18,377,120,35,ToggleCourt,null,13);roamText=roamButton.GetComponentInChildren<Text>();
   BasketballUIStyle.Text(settingsPanel,"Room play continues while this menu is open.",160,375,300,40,11,false,TextAnchor.MiddleCenter);
   leaveButton=BasketballUIStyle.Button(settingsPanel,"Leave court","Leave court",493,377,137,35,OpenLeave,CoveUI.Hex("FFDDD0"));
   guidePanel=MenuPanel("Court guide",650,368,BasketballIcon.Book,CloseMenu);guideTabs=new Button[3];guidePages=new GameObject[3];string[] tabs={"Rules","Controls","Props"};
   for(int i=0;i<3;i++){int tab=i;guideTabs[i]=BasketballUIStyle.Button(guidePanel,tabs[i]+" tab",tabs[i],20+i*207,70,196,34,()=>SelectGuide(tab),BasketballUIStyle.Paper);var r=CoveUI.Rect(guidePanel,tabs[i]+" content",20,118,610,187);guidePages[i]=r.gameObject;}
   BuildRules(guidePages[0].transform);BuildControls(guidePages[1].transform);BuildPropsGuide(guidePages[2].transform);
   Footer(guidePanel,316,650,52);BasketballUIStyle.Button(guidePanel,"Back to settings","‹ Settings",18,326,115,32,ShowSettings,null,13);BasketballUIStyle.Button(guidePanel,"Back to court","Back to court",485,324,145,36,CloseMenu,CoveUI.Gold);
   leavePanel=MenuPanel("Leave court?",440,256,BasketballIcon.Exit,CloseMenu);
   leaveText=BasketballUIStyle.Text(leavePanel,"",24,82,392,78,14,false,TextAnchor.MiddleCenter);
   BasketballUIStyle.Button(leavePanel,"Stay on court","Stay",58,185,128,42,CloseMenu,CoveUI.Gold);
   var confirm=BasketballUIStyle.Button(leavePanel,"Confirm leave court","Leave court",198,185,184,42,()=>{CloseMenu();AppRoot.Instance.LeaveCourt();},CoveUI.Hex("FFDDD0"));confirmText=confirm.GetComponentInChildren<Text>();
   MenuDesigns.Sliders(overlay);overlay.gameObject.SetActive(false);LayoutMenus();
  }
  RectTransform MenuPanel(string title,float w,float h,BasketballIcon icon,UnityEngine.Events.UnityAction close){
   var plate=BasketballUIStyle.Plate(overlay,title,0,0,w,h,CoveUI.Cream,25,3,true);MenuDesigns.Dialog(plate);var r=plate.rectTransform;r.anchorMin=r.anchorMax=r.pivot=Vector2.one*.5f;r.anchoredPosition=Vector2.zero;plate.raycastTarget=true;
   BasketballUIStyle.Art(r,icon,18,15,30,30);BasketballUIStyle.Text(r,title,59,8,w-125,44,24,true);var b=BasketballUIStyle.Button(r,"Close "+title,"",w-58,9,40,40,()=>close());BasketballUIStyle.Art(b.transform,BasketballIcon.Close,10,10,20,20);BasketballUIStyle.Plate(r,"Header rule",18,58,w-36,1,CoveUI.Hex("CCD0BB"),0,0);return r;
  }
  void Footer(Transform p,float y,float w,float h){BasketballUIStyle.Plate(p,"Footer rule",18,y,w-36,1,CoveUI.Hex("CCD0BB"),0,0);}
  void BuildRules(Transform p){
   GuideCard(p,0,BasketballIcon.Move,"Pick up & play","Move close to a reachable ball to collect it. Dribbling happens automatically.");
   GuideCard(p,207,BasketballIcon.Points,"Make a basket","Shots inside the arc score 2. Shots beyond it score 3. Chase the rebound after a miss.");
   GuideCard(p,414,BasketballIcon.Guard,"Defend together","Hold Guard, swipe up and keep holding to jump and contest nearby shots. Reach the rising ball to block it. Return to centre, then slide left to steal. Blocking and stealing are separate choices.");
   BasketballUIStyle.Text(p,AppRoot.Instance.rooms.Connected?"20-second shot clock. Fixed hoops. Quick restarts. No free throws.":"Solo restart: 2 seconds out → stand at the line and pass to re-enter.",0,163,610,23,12,false,TextAnchor.MiddleCenter);
  }
  void GuideCard(Transform p,float x,BasketballIcon icon,string title,string body){BasketballUIStyle.Art(p,icon,x+69,0,54,54);BasketballUIStyle.Text(p,title,x,61,196,24,17,true,TextAnchor.MiddleCenter);BasketballUIStyle.Text(p,body,x+3,91,190,68,12,false,TextAnchor.UpperCenter);}
  void BuildControls(Transform p){
   ControlCard(p,0,0,BasketballIcon.Move,"Move & look","Left stick to move; push fully to run. Drag open space on the right to look or aim.");
   ControlCard(p,319,0,BasketballIcon.Shoot,"Shoot & finish","Hold Shoot; release in green. Keep holding and drag up for Dunk, upper-left for Layup.");
   ControlCard(p,0,80,BasketballIcon.Pass,"Pass your way","Tap Pass to find the teammate closest to your view. Passes lead their run. Anyone in the path can intercept.");
   ControlCard(p,319,80,BasketballIcon.Block,"Defend","Hold Guard to face the carrier. Slide left to Steal; return to repeat. Slide up and keep holding to contest shots.");
   BasketballUIStyle.Text(p,"Double-tap a teammate to choose them directly. Sharp turns leave the pass on its original route.",0,162,610,25,11,false,TextAnchor.MiddleCenter);
  }
  void ControlCard(Transform p,float x,float y,BasketballIcon icon,string title,string body){BasketballUIStyle.Art(p,icon,x,y,35,35);BasketballUIStyle.Text(p,title,x+43,y,244,22,14,true);BasketballUIStyle.Text(p,body,x+43,y+24,244,51,12,false,TextAnchor.UpperLeft);}
  void BuildPropsGuide(Transform p){
   string[] names={"Lock-on","Team Turbo","Portal"},body={"Stand still and aim for 1 second, then fire an assisted shot. A block can interrupt it.","Your team moves 1.5× faster for 5 seconds. An active boost cannot stack.","Use from your home half to bring the ball to you. The centre line is outside your half."};
   for(int i=0;i<3;i++){float x=i*207;CoveUI.Image(p,names[i],SportsUIStyle.PropIcon((SportsProp)(i+1)),x+67,0,62,53).color=Color.white;BasketballUIStyle.Text(p,names[i],x,58,196,23,16,true,TextAnchor.MiddleCenter);BasketballUIStyle.Text(p,body[i],x+3,88,190,70,12,false,TextAnchor.UpperCenter);}
   BasketballUIStyle.Text(p,"One slot · one use · new pickups replace the old prop · use on the painted court",0,163,610,24,11,false,TextAnchor.MiddleCenter);
  }
  void LayoutMenus(){foreach(var panel in new[]{settingsPanel,guidePanel,leavePanel,customizePanel})if(panel)panel.localScale=Vector3.one*Mathf.Min(1,(root.rect.width-28)/panel.rect.width,(root.rect.height-28)/panel.rect.height);}
  public void ToggleSettings(){if(MenuOpen)CloseMenu();else ShowSettings();}
  void OpenMenu(RectTransform panel){
   var view=PlayerView.Instance;shoot.CancelGesture();pass.CancelGesture();if(view){view.ClearMatchInput();view.InputBlocked=true;PlayerView.LookDelta=Vector2.zero;foreach(var pad in controls.GetComponentsInChildren<TouchPad>())pad.ResetInput();}
   controls.blocksRaycasts=false;settingsPanel.gameObject.SetActive(panel==settingsPanel);guidePanel.gameObject.SetActive(panel==guidePanel);leavePanel.gameObject.SetActive(panel==leavePanel);customizePanel.gameObject.SetActive(panel==customizePanel);overlay.gameObject.SetActive(true);RefreshMenu();
  }
  public void ShowSettings(){roamButton.interactable=!(BasketballMatch.Instance&&BasketballMatch.Instance.Active);OpenMenu(settingsPanel);}
  public void OpenGuide(){CloseMenu();SportTutorialUI.Find(SportId.Basketball)?.Open();}
  public void SelectGuide(int tab){guideTab=Mathf.Clamp(tab,0,2);for(int i=0;i<3;i++){guidePages[i].SetActive(i==guideTab);MenuDesigns.Selected(guideTabs[i],i==guideTab);guideTabs[i].GetComponent<BasketballPlate>().color=i==guideTab?CoveUI.Ink:BasketballUIStyle.Paper;guideTabs[i].GetComponentInChildren<Text>().color=i==guideTab?CoveUI.Cream:CoveUI.Ink;}}
  public void SelectCamera(int mode){var view=PlayerView.Instance;if(!view)return;view.mode=Mathf.Clamp(mode,0,2);PlayerPrefs.SetInt("camera",view.mode);PlayerPrefs.Save();RefreshMenu();}
  void RefreshMenu(){RefreshAudioSliders();var view=PlayerView.Instance;if(!view)return;for(int i=0;i<3;i++){MenuDesigns.Selected(cameraChoices[i],i==view.mode);var p=cameraChoices[i].GetComponent<BasketballPlate>();p.color=i==view.mode?CoveUI.Sky:BasketballUIStyle.Paper;p.border=i==view.mode?3:2;p.Refresh();cameraChoices[i].transform.Find("Selected camera").gameObject.SetActive(i==view.mode);}cameraNote.text=view.mode==0?"See from your player's eyes. Drag to look around.":view.mode==1?"Stay behind your player. Drag to look around.":"A wider view of the court. Drag to aim.";roamText.text=view.target&&view.target.BasketballFreeRoam?"Play basketball":"Free roam";leaveButton.interactable=!(SkySailWorld.Instance&&SkySailWorld.Instance.Travelling);}
  public void OpenLeave(){var rooms=AppRoot.Instance.rooms;leaveText.text=rooms.Connected?(rooms.Host?(PostMatchFlow.MatchActive?"Everyone returns to the waiting room. This match ends without a winner.":"Everyone returns to the waiting room. You remain the host."):(AppRoot.Instance.LocalAthlete&&AppRoot.Instance.LocalAthlete.IsSpectator||!PostMatchFlow.MatchActive?"You leave the room. The remaining crew can continue playing.":"You leave the room. This active match becomes void, with no winner.")):"Return to sport selection? This practice score will reset.";confirmText.text=rooms.Connected?(rooms.Host?"Return to room":"Leave room"):"Leave court";OpenMenu(leavePanel);}
  public void CloseMenu(){LayoutEditor?.Dismiss();MenuPreferences.Save();if(!overlay)return;overlay.gameObject.SetActive(false);controls.blocksRaycasts=true;if(PlayerView.Instance){PlayerView.Instance.ClearMatchInput();PlayerView.Instance.InputBlocked=false;}UnityEngine.EventSystems.EventSystem.current?.SetSelectedGameObject(null);}
 }
}
