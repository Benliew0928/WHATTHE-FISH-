using UnityEngine;
using UnityEngine.UI;
namespace WhatTheFish {
 public sealed partial class FootballHUD {
  RectTransform overlay,settingsPanel,leavePanel,cameraPage,guidePage;GameObject[] guidePages;Button[] cameraChoices,guideTabs;Button cameraNav,guideNav,leaveButton;Text cameraNote,leaveText,confirmText;int guideTab,menuPage;
  void BuildMenus(){
   bool wasOpen=MenuOpen,wasLeave=wasOpen&&leavePanel.gameObject.activeSelf;if(overlay){overlay.gameObject.SetActive(false);Destroy(overlay.gameObject);}
   float w=root.rect.width/root.rect.height>1.95f?Mathf.Min(960,root.rect.width-40):668;int body=w>700?15:12;float left=w>700?166:158,content=w-left-20;
   var bg=BasketballUIStyle.Plate(root,"Pitch menu overlay",0,0,800,450,new Color(.07f,.25f,.31f,.46f),0,0);bg.raycastTarget=true;overlay=bg.rectTransform;CoveUI.Stretch(overlay);
   settingsPanel=Panel("Pitch settings",w,390,BasketballIcon.Gear,CloseMenu);BasketballUIStyle.Text(settingsPanel,"The match keeps playing.",72,44,w-150,20,11);
   cameraNav=FootballUIStyle.Button(settingsPanel,"Camera navigation","",20,84,left-54,53,()=>SelectMenu(0),CoveUI.Mint);BasketballUIStyle.Art(cameraNav.transform,BasketballIcon.Eye,(left-54)/2-11,5,22,22);BasketballUIStyle.Text(cameraNav.transform,"Camera",3,29,left-60,23,13,true,TextAnchor.MiddleCenter);
   guideNav=FootballUIStyle.Button(settingsPanel,"Pitch guide navigation","",20,141,left-54,53,OpenGuide,BasketballUIStyle.Paper);BasketballUIStyle.Art(guideNav.transform,BasketballIcon.Book,(left-54)/2-11,5,22,22);BasketballUIStyle.Text(guideNav.transform,"Pitch guide",3,29,left-60,23,13,true,TextAnchor.MiddleCenter);
   BasketballUIStyle.Plate(settingsPanel,"Navigation divider",left-20,84,1,228,CoveUI.Hex("DADCCC"),0,0);
   cameraPage=CoveUI.Rect(settingsPanel,"Camera page",left,82,content,230);guidePage=CoveUI.Rect(settingsPanel,"Pitch guide page",left,82,content,230);
   BasketballUIStyle.Text(cameraPage,"Your view",0,0,content*.5f,26,18,true);BasketballUIStyle.Text(cameraPage,"Changes apply instantly",content*.5f,0,content*.5f,26,11,false,TextAnchor.MiddleRight);
   cameraChoices=new Button[3];string[] cameras={"Eye level","Follow","Stadium"},sub={"Up close to the action","Stay behind your player","See the wider play"};BasketballIcon[] art={BasketballIcon.Eye,BasketballIcon.Follow,BasketballIcon.Court};float tile=(content-20)/3;
   for(int i=0;i<3;i++){int mode=i;var b=FootballUIStyle.Button(cameraPage,cameras[i],"",i*(tile+10),34,tile,134,()=>SelectCamera(mode),BasketballUIStyle.Paper);b.GetComponent<BasketballPlate>().radius=Vector2.one*16;cameraChoices[i]=b;BasketballUIStyle.Art(b.transform,art[i],tile/2-29,15,58,46);BasketballUIStyle.Text(b.transform,cameras[i],4,67,tile-8,26,17,true,TextAnchor.MiddleCenter);BasketballUIStyle.Text(b.transform,sub[i],9,95,tile-18,34,body,false,TextAnchor.UpperCenter);var tick=BasketballUIStyle.Plate(b.transform,"Selected camera",tile-27,8,18,18,CoveUI.Ink,9,0);CoveUI.Image(tick.transform,"Check",SportsUIStyle.Check,3,3,12,12).color=CoveUI.Cream;}
   cameraNote=BasketballUIStyle.Text(cameraPage,"",0,180,content,49,body,false,TextAnchor.UpperLeft);
   guideTabs=new Button[4];guidePages=new GameObject[4];string[] tabs={"Controls","Rules","Stamina","Abilities"};
   for(int i=0;i<tabs.Length;i++){int tab=i;guideTabs[i]=FootballUIStyle.Button(guidePage,tabs[i]+" tab",tabs[i],i*(content+9)/4,0,(content-27)/4,32,()=>SelectGuide(tab),BasketballUIStyle.Paper,13);guidePages[i]=CoveUI.Rect(guidePage,tabs[i]+" content",0,45,content,192).gameObject;}
   BuildControls(guidePages[0].transform,content,body);BuildRules(guidePages[1].transform,content,body);BuildStamina(guidePages[2].transform,content,body);BuildAbilities(guidePages[3].transform,content,body);
   BasketballUIStyle.Plate(settingsPanel,"Footer rule",20,328,w-40,1,CoveUI.Hex("DADCCC"),0,0);
   leaveButton=FootballUIStyle.Button(settingsPanel,"Leave pitch","Leave pitch",20,340,146,39,OpenLeave,CoveUI.Cream,14);var leaveLabel=leaveButton.GetComponentInChildren<Text>();leaveLabel.color=CoveUI.Hex("A63438");leaveLabel.rectTransform.anchoredPosition=new Vector2(44,0);leaveLabel.rectTransform.sizeDelta=new Vector2(190,78);BasketballUIStyle.Art(leaveButton.transform,BasketballIcon.Exit,12,9,24,24);var leavePlate=leaveButton.GetComponent<BasketballPlate>();leavePlate.shadow=0;leavePlate.border=1;leavePlate.edge=CoveUI.Hex("C89F90");FootballUIStyle.Button(settingsPanel,"Back to play","Back to play",w-140,340,120,39,CloseMenu,CoveUI.Mint,14);
   leavePanel=Panel("Leave pitch?",440,256,BasketballIcon.Exit,ShowSettings);leaveText=BasketballUIStyle.Text(leavePanel,"",26,87,388,68,14,false,TextAnchor.MiddleCenter);
   FootballUIStyle.Button(leavePanel,"Keep playing","Keep playing",25,185,180,44,CloseMenu,CoveUI.Mint);var confirm=FootballUIStyle.Button(leavePanel,"Confirm leave pitch","Leave pitch",219,185,196,44,()=>{CloseMenu();AppRoot.Instance.LeaveCourt();},FootballUIStyle.Cancel);confirmText=confirm.GetComponentInChildren<Text>();
   BuildAudio(left,content);BuildCustomization(left,content);SelectMenu(menuPage);SelectGuide(guideTab);settingsPanel.gameObject.SetActive(!wasLeave);leavePanel.gameObject.SetActive(wasLeave);overlay.gameObject.SetActive(wasOpen);if(wasOpen){PlayerView.Instance.InputBlocked=true;controls.blocksRaycasts=false;if(wasLeave)OpenLeave();else RefreshMenu();}
  }
  RectTransform Panel(string title,float w,float h,BasketballIcon icon,System.Action close){
   var plate=BasketballUIStyle.Plate(overlay,title,0,0,w,h,CoveUI.Cream,26,3,true);plate.shadow=6;plate.raycastTarget=true;var r=plate.rectTransform;r.anchorMin=r.anchorMax=r.pivot=Vector2.one*.5f;r.anchoredPosition=Vector2.zero;
   BasketballUIStyle.Art(r,icon,25,18,33,33);BasketballUIStyle.Text(r,title,72,10,w-142,38,24,true);var b=FootballUIStyle.Button(r,"Close "+title,"",w-60,17,38,38,close,BasketballUIStyle.Paper);BasketballUIStyle.Art(b.transform,BasketballIcon.Close,11,11,16,16);BasketballUIStyle.Plate(r,"Header rule",20,68,w-40,1,CoveUI.Hex("DADCCC"),0,0);return r;
  }
  void Card(Transform p,float x,float y,float w,Sprite icon,string title,string body,int size){CoveUI.Image(p,title+" icon",icon,x,y+2,27,30);BasketballUIStyle.Text(p,title,x+35,y,w-35,24,14,true);BasketballUIStyle.Text(p,body,x+35,y+26,w-35,53,size,false,TextAnchor.UpperLeft);}
  void Note(Transform p,float w,string text,int size){BasketballUIStyle.Plate(p,"Guide note rule",0,154,w,1,CoveUI.Hex("DADCCC"),0,0);BasketballUIStyle.Text(p,text,0,163,w,34,size,false,TextAnchor.UpperLeft);}
  void BuildControls(Transform p,float w,int size){float col=(w-18)/2;
   Card(p,0,0,col,BasketballUIStyle.Icon(BasketballIcon.Move),"Move & look","Left stick to move; push fully to run. Drag right to look.",size);
   Card(p,col+18,0,col,FootballUIStyle.Icon(FootballIcon.Ball),"Kick with control","Tap Shoot for a light kick. Hold for power. Dash stays active.",size);
   Card(p,0,80,col,FootballUIStyle.Icon(FootballIcon.Pressure),"Defend","Hold Pressure to mark the carrier. Tap Tackle when close.",size);
   Card(p,col+18,80,col,FootballUIStyle.Icon(FootballIcon.Target),"Pass to a teammate","Aim and tap Pass (Space). Hold to see the receiver lock.",size);
   Note(p,w,"Changed your mind? Slide into Cancel and lift. Slide back to keep aiming.",size);
  }
  void BuildRules(Transform p,float w,int size){float col=(w-18)/2;
   Card(p,0,0,col,FootballUIStyle.Icon(FootballIcon.Team),"Small teams, quick games","1v1 to 5v5. Host needs an even roster. Pick a team before kickoff.",size);
   Card(p,col+18,0,col,FootballUIStyle.Icon(FootballIcon.Flag),"Play to the whistle",$"{MenuMatchRules.CurrentMinutes}-minute match. A tie adds 60 seconds of golden goal, then a draw.",size);
   Card(p,0,80,col,FootballUIStyle.Icon(FootballIcon.Ball),"Find the ball","Approach a slow free ball from the front to control it. Dribble, turn, kick.",size);
   Card(p,col+18,80,col,FootballUIStyle.Icon(FootballIcon.Target),"Keep play moving","Each goal scores one. The boundary keeps the ball in. Goals reset kickoff.",size);
   Note(p,w,"Props appear in matches and Explore Offline. Collect at halfway; tap the ability button.",size);
  }
  void BuildStamina(Transform p,float w,int size){
   FootballUIStyle.Art(p,FootballIcon.Dash,0,1,34,34);BasketballUIStyle.Text(p,"Save a burst for the break",44,0,w-44,25,17,true);BasketballUIStyle.Text(p,"Normal running does not spend stamina.",44,27,w-44,24,size);
   var bar=BasketballUIStyle.Plate(p,"Stamina example",0,61,w,9,CoveUI.Ink,4.5f,0);BasketballUIStyle.Plate(bar.transform,"Stamina example fill",2,2,w*.7f,5,BasketballUIStyle.Teal,2.5f,0);
   float col=(w-18)/2;BasketballUIStyle.Text(p,"Hold while moving",0,81,col,23,14,true);BasketballUIStyle.Text(p,"A full bar lasts about 3 seconds. Let go to recover; standing still costs nothing.",0,106,col,47,size,false,TextAnchor.UpperLeft);BasketballUIStyle.Text(p,"Empty? Release and rest",col+18,81,col,23,14,true);BasketballUIStyle.Text(p,"Lift your finger. Dash returns after stamina recovers to a quarter full.",col+18,106,col,47,size,false,TextAnchor.UpperLeft);
   Note(p,w,"Hold Dash, slide to Shoot, then slide back to release without ending Dash.",size);
  }
  void BuildAbilities(Transform p,float w,int size){float col=(w-18)/2;
   Card(p,0,0,col,FootballPropHUD.Icon(SportsProp.Turbo),"Team Turbo","Your whole team moves at 1.5× speed for 5 seconds. Cannot stack.",size);
   Card(p,col+18,0,col,FootballPropHUD.Icon(SportsProp.Portal),"Portal Steal","Use from your own half. Stay planted while the ball arrives at your feet.",size);
   Card(p,0,80,col,FootballPropHUD.Icon(SportsProp.Wall),"Place a wall","Aim, then tap Place. A solid wall blocks players and balls for 15 seconds.",size);
   Card(p,col+18,80,col,FootballUIStyle.Icon(FootballIcon.Target),"Watch halfway","Blue 5-second countdowns mark a touchline pickup. One wall per team.",size);
   Note(p,w,"Tap your prop (G on keyboard). One use; a new pickup replaces it. Cancel keeps your wall.",size);
  }
  public void ToggleSettings(){if(MenuOpen)CloseMenu();else ShowSettings();}
  void OpenMenu(bool leave){ClearInput();PlayerView.Instance.InputBlocked=true;controls.blocksRaycasts=false;settingsPanel.gameObject.SetActive(!leave);leavePanel.gameObject.SetActive(leave);overlay.gameObject.SetActive(true);RefreshMenu();}
  public void ShowSettings()=>OpenMenu(false);
  public void OpenGuide(){SelectMenu(1);ShowSettings();}
  public void SelectMenu(int page){menuPage=Mathf.Clamp(page,0,3);cameraPage.gameObject.SetActive(menuPage==0);guidePage.gameObject.SetActive(menuPage==1);audioPage.gameObject.SetActive(menuPage==2);customizePage.gameObject.SetActive(menuPage==3);RefreshAudio();for(int i=0;i<4;i++){var p=(i==0?cameraNav:i==1?guideNav:i==2?audioNav:customizeNav).GetComponent<BasketballPlate>();p.color=i==menuPage?CoveUI.Hex("D5EBDE"):CoveUI.Cream;p.border=i==menuPage?1:0;p.edge=CoveUI.Hex("96B9AE");p.shadow=0;p.Refresh();}}
  public void SelectGuide(int tab){guideTab=Mathf.Clamp(tab,0,guidePages.Length-1);for(int i=0;i<guidePages.Length;i++){guidePages[i].SetActive(i==guideTab);guideTabs[i].GetComponent<BasketballPlate>().color=i==guideTab?CoveUI.Ink:BasketballUIStyle.Paper;guideTabs[i].GetComponentInChildren<Text>().color=i==guideTab?CoveUI.Cream:CoveUI.Ink;}}
  public void SelectCamera(int mode){var view=PlayerView.Instance;if(!view)return;view.mode=Mathf.Clamp(mode,0,2);PlayerPrefs.SetInt("camera",view.mode);PlayerPrefs.Save();RefreshMenu();}
  void RefreshMenu(){var view=PlayerView.Instance;if(!view)return;for(int i=0;i<3;i++){var p=cameraChoices[i].GetComponent<BasketballPlate>();p.color=i==view.mode?CoveUI.Hex("DCF0D8"):BasketballUIStyle.Paper;p.border=i==view.mode?2.5f:1;p.edge=i==view.mode?CoveUI.Ink:CoveUI.Hex("CAD0BD");p.shadow=i==view.mode?2:0;p.Refresh();cameraChoices[i].transform.Find("Selected camera").gameObject.SetActive(i==view.mode);}cameraNote.text=view.mode==0?"Look through your player's eyes. Drag on the open right side to look around.":view.mode==1?"Drag on the open right side to look around. Your left thumb stays on movement.":"A wider, ball-following view. Drag to pan your view; your player's aim stays independent.";leaveButton.interactable=!(SkySailWorld.Instance&&SkySailWorld.Instance.Travelling);}
  public void OpenLeave(){var rooms=AppRoot.Instance.rooms;leaveText.text=rooms.Connected?(rooms.Host?"Everyone returns to the room. The current match ends without a winner.":"Leaving voids the active match without a winner. You return to sport selection."):"Return to sport selection? You can come back for another kickabout.";confirmText.text=rooms.Connected?(rooms.Host?"Return to room":"Leave room"):"Leave pitch";OpenMenu(true);}
  public void CloseMenu(){MenuPreferences.Save();if(!overlay)return;overlay.gameObject.SetActive(false);controls.blocksRaycasts=true;ClearInput();if(PlayerView.Instance)PlayerView.Instance.InputBlocked=false;UnityEngine.EventSystems.EventSystem.current?.SetSelectedGameObject(null);}
 }
}
