using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace WhatTheFish {
 // Uses the same plates, icon atlas, feedback and saved preferences as the courts.
 public sealed class FishingSettingsMenu:MonoBehaviour {
  RectTransform page,root,overlay,settings,leave;Button[] cameras;Text note,leaveText;bool blockedBefore;
  readonly Slider[] volumes=new Slider[4];readonly Text[] captions=new Text[4];
  static readonly string[] AudioNames={"Master","Game sounds","Buttons","Ambience"};
  public bool Visible=>overlay&&overlay.gameObject.activeSelf;
  public RectTransform GearRect {get;private set;}
  public float GearBottomInset=>GearRect?(-GearRect.anchoredPosition.y+GearRect.rect.height)*Mathf.Min(page.rect.width/800,page.rect.height/450):0;
  public bool FitsSafeFrame(){if(!page||!GearRect)return false;var corners=new Vector3[4];GearRect.GetWorldCorners(corners);foreach(var corner in corners){var p=page.InverseTransformPoint(corner);if(!page.rect.Contains(new Vector2(p.x,p.y)))return false;}return true;}
  public static FishingSettingsMenu Create(RectTransform page){
   var node=BasketballUIStyle.Center(page,"Fishing island menus",new Vector2(800,450),Vector2.zero,Vector2.one*.5f);
   var ui=node.gameObject.AddComponent<FishingSettingsMenu>();ui.page=page;ui.root=node;ui.Build();ui.Layout();return ui;
  }
  void Build(){
   var gear=BasketballUIStyle.Button(root,"Fishing settings","",0,0,48,48,Toggle);GearRect=(RectTransform)gear.transform;
   GearRect.anchorMin=GearRect.anchorMax=GearRect.pivot=Vector2.one;GearRect.anchoredPosition=new Vector2(-18,-15);
   gear.GetComponent<BasketballPlate>().radius=Vector2.one*17;BasketballUIStyle.Art(gear.transform,BasketballIcon.Gear,9.5f,9.5f,29,29);
   var shade=BasketballUIStyle.Plate(root,"Fishing menu overlay",0,0,800,450,new Color(.07f,.25f,.31f,.46f),0,0);overlay=shade.rectTransform;CoveUI.Stretch(overlay);shade.raycastTarget=true;
   settings=Panel("Island settings",650,422,BasketballIcon.Gear);
   BasketballUIStyle.Text(settings,"Camera angle",20,70,590,20,15,true);
   cameras=new Button[3];string[] names={"Eye level","Follow","Island view"};BasketballIcon[] icons={BasketballIcon.Eye,BasketballIcon.Follow,BasketballIcon.Court};
   for(int i=0;i<3;i++){int mode=i;cameras[i]=BasketballUIStyle.Button(settings,names[i],"",20+i*207,98,196,86,()=>SelectCamera(mode),BasketballUIStyle.Paper);BasketballUIStyle.Art(cameras[i].transform,icons[i],70,5,56,42);BasketballUIStyle.Text(cameras[i].transform,names[i],4,51,188,28,16,true,TextAnchor.MiddleCenter);CoveUI.Image(cameras[i].transform,"Selected camera",SportsUIStyle.Check,166,2,23,22).color=CoveUI.Ink;}
   note=BasketballUIStyle.Text(settings,"",20,191,610,26,12);
   for(int i=0;i<4;i++){
    int index=i;float x=20+i*155;captions[i]=BasketballUIStyle.Text(settings,AudioNames[i],x,226,146,22,12,true);
    var track=BasketballUIStyle.Plate(settings,AudioNames[i]+" volume",x,254,143,34,CoveUI.Sky,17,1);track.raycastTarget=true;
    var slider=volumes[i]=track.gameObject.AddComponent<Slider>();slider.direction=Slider.Direction.LeftToRight;
    var area=CoveUI.Rect(track.transform,"Handle area",16,17,111,0);var handle=BasketballUIStyle.Plate(area,"Handle",0,0,30,30,CoveUI.Gold,15,1.5f);handle.raycastTarget=true;handle.rectTransform.pivot=Vector2.one*.5f;slider.handleRect=handle.rectTransform;slider.targetGraphic=handle;
    slider.onValueChanged.AddListener(value=>{if(index==0)MenuPreferences.SetMaster(value);else if(index==1)MenuPreferences.SetSFX(value);else if(index==2)MenuPreferences.SetUI(value);else MenuPreferences.SetAmbience(value);captions[index].text=AudioNames[index]+" "+Mathf.RoundToInt(value*100)+"%";});
   }
   var guide=BasketballUIStyle.Button(settings,"Fishing guide","",20,304,298,48,()=>{Close();FishingHUD.Instance?.Tutorial?.Open();});BasketballUIStyle.Art(guide.transform,BasketballIcon.Book,12,9,30,30);BasketballUIStyle.Text(guide.transform,"How to fish",54,4,235,23,16,true);BasketballUIStyle.Text(guide.transform,"Controls, abilities & rules",54,26,235,16,11);
   BasketballUIStyle.Button(settings,"Back to lagoon","Back to lagoon",330,304,300,48,Close,CoveUI.Mint,16);
   BasketballUIStyle.Plate(settings,"Footer rule",18,366,614,1,CoveUI.Hex("CCD0BB"),0,0);
   BasketballUIStyle.Text(settings,"Room play continues while settings are open.",20,375,450,40,11,false,TextAnchor.MiddleCenter);
   BasketballUIStyle.Button(settings,"Leave island","Leave island",493,377,137,35,OpenLeave,CoveUI.Hex("FFDDD0"));
   leave=Panel("Leave island?",440,256,BasketballIcon.Exit);leaveText=BasketballUIStyle.Text(leave,"",24,82,392,78,14,false,TextAnchor.MiddleCenter);
   BasketballUIStyle.Button(leave,"Stay on island","Stay",58,185,128,42,Close,CoveUI.Gold);
   BasketballUIStyle.Button(leave,"Confirm leave island","Leave island",198,185,184,42,()=>{Close();AppRoot.Instance.LeaveCourt();},CoveUI.Hex("FFDDD0"));
   overlay.gameObject.SetActive(false);
  }
  RectTransform Panel(string title,float width,float height,BasketballIcon icon){
   var plate=BasketballUIStyle.Plate(overlay,title,0,0,width,height,CoveUI.Cream,25,3,true);var r=plate.rectTransform;r.anchorMin=r.anchorMax=r.pivot=Vector2.one*.5f;r.anchoredPosition=Vector2.zero;plate.raycastTarget=true;
   BasketballUIStyle.Art(r,icon,18,15,30,30);BasketballUIStyle.Text(r,title,59,8,width-125,44,24,true);var close=BasketballUIStyle.Button(r,"Close "+title,"",width-58,9,40,40,Close);BasketballUIStyle.Art(close.transform,BasketballIcon.Close,10,10,20,20);BasketballUIStyle.Plate(r,"Header rule",18,58,width-36,1,CoveUI.Hex("CCD0BB"),0,0);return r;
  }
  public void Toggle(){if(Visible)Close();else Open();}
  public void Open(){if(FishingHUD.Instance&&FishingHUD.Instance.BlocksGameplay)return;var view=PlayerView.Instance;blockedBefore=view&&view.InputBlocked;if(view){view.ClearMatchInput();view.InputBlocked=true;}foreach(var pad in page.GetComponentsInChildren<TouchPad>())pad.ResetInput();settings.gameObject.SetActive(true);leave.gameObject.SetActive(false);overlay.gameObject.SetActive(true);root.SetAsLastSibling();Refresh();}
  public void SelectCamera(int mode){var view=PlayerView.Instance;if(!view)return;view.mode=Mathf.Clamp(mode,0,2);PlayerPrefs.SetInt("camera",view.mode);PlayerPrefs.Save();Refresh();}
  void Refresh(){var view=PlayerView.Instance;for(int i=0;i<3;i++){bool chosen=view&&view.mode==i;var p=cameras[i].GetComponent<BasketballPlate>();p.color=chosen?CoveUI.Sky:BasketballUIStyle.Paper;p.border=chosen?3:2;p.Refresh();cameras[i].transform.Find("Selected camera").gameObject.SetActive(chosen);}note.text=view&&view.mode==0?"See from your player's eyes. Drag to look around.":view&&view.mode==1?"Stay behind your player. Drag to look around.":"A wider view of the lagoon. Drag to aim.";for(int i=0;i<4;i++){float value=i==0?MenuPreferences.MasterVolume:i==1?MenuPreferences.SFXVolume:i==2?MenuPreferences.UIVolume:MenuPreferences.AmbienceVolume;volumes[i].SetValueWithoutNotify(value);captions[i].text=AudioNames[i]+" "+Mathf.RoundToInt(value*100)+"%";}}
  void OpenLeave(){var rooms=AppRoot.Instance.rooms;leaveText.text=rooms.Connected?(rooms.Host?"Everyone returns to the room. The current match ends without a winner.":"Leaving voids the active match. Return to sport selection?"):"Return to sport selection? This practice score will reset.";settings.gameObject.SetActive(false);leave.gameObject.SetActive(true);}
  public void Close(){if(!Visible)return;MenuPreferences.Save();overlay.gameObject.SetActive(false);var view=PlayerView.Instance;if(view){view.ClearMatchInput();view.InputBlocked=blockedBefore;}EventSystem.current?.SetSelectedGameObject(null);}
  void Layout(){if(!page)return;float scale=Mathf.Min(page.rect.width/800,page.rect.height/450);root.localScale=Vector3.one*scale;root.sizeDelta=page.rect.size/Mathf.Max(.01f,scale);}
  void LateUpdate(){Layout();if(!FishingGame.Instance||!FishingGame.Instance.Context)Close();}
  void OnDisable()=>Close();
 }
}
