using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace WhatTheFish {
 // Shared island menus use the courts' plates, icons, saved preferences and touch feedback.
 public class IslandSettingsMenu:MonoBehaviour {
  RectTransform page,root,overlay,settings,leave,guide;Button[] cameras;Text note,leaveText,confirmText,playNote;
  Button gear,leaveButton,confirm;bool blockedBefore;PlayerView blockedView;SportId sport;
  readonly Slider[] volumes=new Slider[4];readonly Text[] captions=new Text[4];
  static readonly string[] AudioNames={"Master","Game sounds","Buttons","Ambience"};
  public bool Visible=>overlay&&overlay.gameObject.activeSelf;
  public bool LeaveVisible=>Visible&&leave.gameObject.activeSelf;
  public RectTransform GearRect=>gear?(RectTransform)gear.transform:null;
  public float GearBottomInset=>GearRect?(-GearRect.anchoredPosition.y+GearRect.rect.height)*Scale:0;
  float Scale=>Mathf.Max(.01f,Mathf.Min(page.rect.width/800,page.rect.height/450));
  bool Fishing=>sport==SportId.Fishing;
  bool Context=>Fishing?FishingGame.Instance&&FishingGame.Instance.Context:GolfMatchManager.Instance&&GolfMatchManager.Instance.Context;
  bool CanLeave=>!RoomMatchStart.Travelling&&!TeleportSequence.Busy&&!RoomTeleport.Busy;
  public static IslandSettingsMenu Create(RectTransform page,SportId sport)=>Create<IslandSettingsMenu>(page,sport);
  protected static T Create<T>(RectTransform page,SportId sport) where T:IslandSettingsMenu {
   var node=BasketballUIStyle.Center(page,sport+" island menus",new Vector2(800,450),Vector2.zero,Vector2.one*.5f);
   var ui=node.gameObject.AddComponent<T>();ui.Initialize(page,node,sport);return ui;
  }
  void Initialize(RectTransform parent,RectTransform node,SportId id){page=parent;root=node;sport=id;Build();Layout();}
  void Build(){
   gear=BasketballUIStyle.Button(root,sport+" settings","",0,0,48,48,Toggle);
   var anchor=Vector2.one;GearRect.anchorMin=GearRect.anchorMax=GearRect.pivot=anchor;GearRect.anchoredPosition=new Vector2(-18,-16);
   gear.GetComponent<BasketballPlate>().radius=Vector2.one*17;BasketballUIStyle.Art(gear.transform,BasketballIcon.Gear,9.5f,9.5f,29,29);
   var shade=BasketballUIStyle.Plate(root,sport+" menu overlay",0,0,800,450,new Color(.07f,.25f,.31f,.46f),0,0);overlay=shade.rectTransform;CoveUI.Stretch(overlay);shade.raycastTarget=true;
   // Golf's finish clock has its own sorting canvas. The complete modal must
   // cover that clock and the ranking card, including their pointer targets.
   var modalCanvas=overlay.gameObject.AddComponent<Canvas>();modalCanvas.overrideSorting=true;modalCanvas.sortingOrder=200;overlay.gameObject.AddComponent<GraphicRaycaster>();
   settings=Panel("Island settings",650,422,BasketballIcon.Gear);
   BasketballUIStyle.Text(settings,"Camera angle",20,70,590,20,15,true);
   cameras=new Button[3];string[] names={"Eye level","Follow",Fishing?"Lagoon":"Course"};BasketballIcon[] icons={BasketballIcon.Eye,BasketballIcon.Follow,BasketballIcon.Court};
   for(int i=0;i<3;i++){int mode=i;cameras[i]=BasketballUIStyle.Button(settings,names[i],"",20+i*207,98,196,86,()=>SelectCamera(mode),BasketballUIStyle.Paper);BasketballUIStyle.Art(cameras[i].transform,icons[i],70,5,56,42);BasketballUIStyle.Text(cameras[i].transform,names[i],4,51,188,28,16,true,TextAnchor.MiddleCenter);CoveUI.Image(cameras[i].transform,"Selected camera",SportsUIStyle.Check,166,2,23,22).color=CoveUI.Ink;}
   note=BasketballUIStyle.Text(settings,"",20,191,610,26,12);
   for(int i=0;i<4;i++){
    int index=i;float x=20+i*155;captions[i]=BasketballUIStyle.Text(settings,AudioNames[i],x,226,146,22,12,true);
    var track=BasketballUIStyle.Plate(settings,AudioNames[i]+" volume",x,254,143,34,CoveUI.Sky,17,1);track.raycastTarget=true;
    var slider=volumes[i]=track.gameObject.AddComponent<Slider>();slider.direction=Slider.Direction.LeftToRight;
    var area=CoveUI.Rect(track.transform,"Handle area",16,17,111,0);var handle=BasketballUIStyle.Plate(area,"Handle",0,0,30,30,CoveUI.Gold,15,1.5f);handle.raycastTarget=true;handle.rectTransform.pivot=Vector2.one*.5f;slider.handleRect=handle.rectTransform;slider.targetGraphic=handle;slider.navigation=new Navigation{mode=Navigation.Mode.None};
    slider.onValueChanged.AddListener(value=>{if(index==0)MenuPreferences.SetMaster(value);else if(index==1)MenuPreferences.SetSFX(value);else if(index==2)MenuPreferences.SetUI(value);else MenuPreferences.SetAmbience(value);captions[index].text=AudioNames[index]+" "+Mathf.RoundToInt(value*100)+"%";});
   }
   var help=BasketballUIStyle.Button(settings,Fishing?"Fishing guide":"Golf guide","",20,304,194,48,OpenGuide);BasketballUIStyle.Art(help.transform,BasketballIcon.Book,12,9,30,30);BasketballUIStyle.Text(help.transform,Fishing?"How to fish":"How to golf",54,4,136,23,16,true);BasketballUIStyle.Text(help.transform,"Controls & rules",54,26,136,16,11);
   BasketballUIStyle.Button(settings,Fishing?"Back to lagoon":"Back to course",Fishing?"Back to lagoon":"Back to course",428,304,202,48,Close,CoveUI.Mint,16);
   BasketballUIStyle.Button(settings,"Layout","Layout",224,304,194,48,OpenLayout,CoveUI.Sky,16);
   BasketballUIStyle.Plate(settings,"Footer rule",18,366,614,1,CoveUI.Hex("CCD0BB"),0,0);
   playNote=BasketballUIStyle.Text(settings,"",20,375,450,40,11,false,TextAnchor.MiddleCenter);
   leaveButton=BasketballUIStyle.Button(settings,"Leave island","Leave island",493,377,137,35,OpenLeave,CoveUI.Hex("FFDDD0"));
   leave=Panel("Leave island?",440,256,BasketballIcon.Exit);leaveText=BasketballUIStyle.Text(leave,"",24,82,392,78,14,false,TextAnchor.MiddleCenter);
   BasketballUIStyle.Button(leave,"Stay on island","Stay",58,185,128,42,Back,CoveUI.Gold);
   confirm=BasketballUIStyle.Button(leave,"Confirm leave island","Leave island",198,185,184,42,()=>{if(!CanLeave)return;Close();AppRoot.Instance.LeaveCourt();},CoveUI.Hex("FFDDD0"));confirmText=confirm.GetComponentInChildren<Text>();
   if(!Fishing){
    guide=Panel("Golf guide",650,374,BasketballIcon.Book);
    BasketballUIStyle.Text(guide,"Aim & shoot",24,75,602,24,17,true);
    BasketballUIStyle.Text(guide,"Start a game, then walk near your ball and tap Aim. Drag the ball handle down to set power; release to hit. Tap Cancel to walk again.",24,103,602,53,14);
    BasketballUIStyle.Text(guide,"On the course",24,168,602,24,17,true);
    BasketballUIStyle.Text(guide, $"Shots switch to Putt near the cup. Reach all {GolfPlayerState.HoleCount} holes in fewer strokes. Tap the leaderboard for the full standings. Use Buggy to call your cart.",24,196,602,53,14);
    BasketballUIStyle.Text(guide,"Play together",24,261,602,24,17,true);
    BasketballUIStyle.Text(guide,"The host starts the room's game. Everyone travels together. Settings affect only you; the game and its timers keep running.",24,289,602,45,13);
    BasketballUIStyle.Button(guide,"Back to settings","Back to settings",20,333,220,31,Back,CoveUI.Mint,13);
   }
   overlay.gameObject.SetActive(false);
  }
  RectTransform Panel(string title,float width,float height,BasketballIcon icon){
   var plate=BasketballUIStyle.Plate(overlay,title,0,0,width,height,CoveUI.Cream,25,3,true);var r=plate.rectTransform;r.anchorMin=r.anchorMax=r.pivot=Vector2.one*.5f;r.anchoredPosition=Vector2.zero;plate.raycastTarget=true;
   BasketballUIStyle.Art(r,icon,18,15,30,30);BasketballUIStyle.Text(r,title,59,8,width-125,44,24,true);var close=BasketballUIStyle.Button(r,"Close "+title,"",width-58,9,40,40,Close);BasketballUIStyle.Art(close.transform,BasketballIcon.Close,10,10,20,20);BasketballUIStyle.Plate(r,"Header rule",18,58,width-36,1,CoveUI.Hex("CCD0BB"),0,0);return r;
  }
  public void OpenLayout(){if(!Visible)Open();if(Visible)page.GetComponent<ControlLayoutEditor>()?.Open(()=>overlay.gameObject);}
  public void Toggle(){if(Visible)Back();else Open();}
  public void Open(){
   if(Visible||!Context||TeleportSequence.Busy||RoomTeleport.Busy||PostMatchFlow.Instance&&PostMatchFlow.Instance.Visible||Fishing&&FishingHUD.Instance&&FishingHUD.Instance.BlocksGameplay)return;
   blockedView=PlayerView.Instance;blockedBefore=blockedView&&blockedView.InputBlocked;
   if(blockedView){blockedView.ClearMatchInput();blockedView.InputBlocked=true;}foreach(var swing in page.GetComponentsInChildren<GolfSwingButton>(true))swing.CancelGesture();ResetPads();ShowSettings();overlay.gameObject.SetActive(true);root.SetAsLastSibling();Refresh();
  }
  void ResetPads(){PlayerView.LookDelta=Vector2.zero;foreach(var pad in page.GetComponentsInChildren<TouchPad>(true))pad.ResetInput();}
  void ShowSettings(){settings.gameObject.SetActive(true);leave.gameObject.SetActive(false);if(guide)guide.gameObject.SetActive(false);}
  public void Back(){if(!Visible)return;if(!settings.gameObject.activeSelf){ShowSettings();Refresh();}else Close();}
  public void SelectCamera(int mode){var view=PlayerView.Instance;if(!view)return;view.mode=Mathf.Clamp(mode,0,2);if(!Fishing){PlayerPrefs.SetInt("camera",view.mode);PlayerPrefs.Save();}Refresh();}
  void OpenGuide(){if(Fishing){Close();FishingHUD.Instance?.Tutorial?.Open();}else {settings.gameObject.SetActive(false);guide.gameObject.SetActive(true);}}
  void Refresh(){
   var view=PlayerView.Instance;for(int i=0;i<3;i++){bool chosen=view&&view.mode==i;var p=cameras[i].GetComponent<BasketballPlate>();p.color=chosen?CoveUI.Sky:BasketballUIStyle.Paper;p.border=chosen?3:2;p.Refresh();cameras[i].transform.Find("Selected camera").gameObject.SetActive(chosen);}
   note.text=view&&view.mode==0?"See from your player's eyes. Drag to look around.":view&&view.mode==1?"Stay behind your player. Drag to look around.":Fishing?"Frame your cast and hooked fish. Drag to look before casting.":"See your shot and landing area, then follow the ball.";
   for(int i=0;i<4;i++){float value=i==0?MenuPreferences.MasterVolume:i==1?MenuPreferences.SFXVolume:i==2?MenuPreferences.UIVolume:MenuPreferences.AmbienceVolume;volumes[i].SetValueWithoutNotify(value);captions[i].text=AudioNames[i]+" "+Mathf.RoundToInt(value*100)+"%";}
   playNote.text=AppRoot.Instance.rooms.Connected?"Room play continues while settings are open.":"Play continues while settings are open.";RefreshLeave();
  }
  void RefreshLeave(){
   var rooms=AppRoot.Instance.rooms;leaveButton.interactable=confirm.interactable=CanLeave;
   leaveText.text=rooms.Connected?(rooms.Host?(PostMatchFlow.MatchActive?"Everyone returns to the waiting room. The current match ends.":"Everyone returns to the waiting room. You remain the host."):GuestLeaveCopy()):"Return to sport selection? This practice score will reset.";
   confirmText.text=rooms.Connected?(rooms.Host?"Return to room":"Leave room"):"Leave island";
  }
  string GuestLeaveCopy(){
   const string departure="You leave the room. ";
   if(AppRoot.Instance.LocalAthlete&&AppRoot.Instance.LocalAthlete.IsSpectator)return departure+"The remaining crew can continue playing.";
   if(Fishing&&FishingGame.Instance){var match=FishingGame.Instance.State.Match;
    if(match.phase==FishingRoundPhase.Drafting||match.phase==FishingRoundPhase.Countdown)return departure+"Leaving during skill choice or the countdown ends the match for everyone.";
    if(match.mode==FishingMode.Crew&&match.phase==FishingRoundPhase.Playing)return departure+"Leaving ends Crew Catch for everyone.";
    if(match.mode==FishingMode.Cup)return departure+"The remaining players can continue, but this Cup result becomes invalid.";
   }
   return departure+"Return to sport selection? The remaining players can continue.";
  }
  public void OpenLeave(){if(!Visible||!CanLeave)return;Refresh();settings.gameObject.SetActive(false);if(guide)guide.gameObject.SetActive(false);leave.gameObject.SetActive(true);}
  public void Close(){
   page.GetComponent<ControlLayoutEditor>()?.Dismiss();if(!Visible)return;MenuPreferences.Save();overlay.gameObject.SetActive(false);ResetPads();
   if(blockedView){blockedView.ClearMatchInput();blockedView.InputBlocked=blockedBefore;}blockedView=null;EventSystem.current?.SetSelectedGameObject(null);
  }
  void Layout(){if(!page)return;root.localScale=Vector3.one*Scale;root.sizeDelta=page.rect.size/Scale;}
  protected void LateUpdate(){Layout();if(!Context||PostMatchFlow.Instance&&PostMatchFlow.Instance.Visible){Close();return;}if(Visible){root.SetAsLastSibling();RefreshLeave();}}
  protected void OnDisable()=>Close();
  public bool FitsSafeFrame(){
   if(!page||!GearRect||!Fits(GearRect))return false;
   if(Visible)foreach(var panel in new[]{settings,leave,guide})if(panel&&panel.gameObject.activeSelf&&!Fits(panel))return false;return true;
  }
  bool Fits(RectTransform rect){var corners=new Vector3[4];rect.GetWorldCorners(corners);foreach(var corner in corners){var p=page.InverseTransformPoint(corner);if(!page.rect.Contains(new Vector2(p.x,p.y)))return false;}return true;}
 }
}
