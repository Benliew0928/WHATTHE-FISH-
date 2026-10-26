using System;
using UnityEngine;
using UnityEngine.UI;
using static WhatTheFish.PlayerHubStyle;

namespace WhatTheFish {
 public sealed class PlayerReportUI:MonoBehaviour {
  static PlayerReportUI instance;static int closedFrame=-1;
  public static bool Visible=>instance;
  public static bool BlocksBack=>Visible||closedFrame==Time.frameCount;
  RectTransform design,card;Text feedback,counter;InputField details;CoveButton submit;CoveButton[] reasons;
  PublicProfile target;int selected=-1;bool sending,complete,small;string context,evidence,lastPayload="",requestId="",identityScope;
  public static void Open(PublicProfile player,bool playerCard=false){
   if(!PlayerReports.CanReport(player)||instance)return;
   instance=new GameObject("Report player dialog").AddComponent<PlayerReportUI>();instance.target=PlayerReports.Copy(player);instance.small=playerCard;
   instance.context=PlayerReports.Context;instance.evidence=PlayerReports.Evidence(player.id);instance.identityScope=PlayerIdentity.ProfileStorageKey;instance.Build();
   PlayerView.Instance?.ClearMatchInput();
  }
  void Build(){
   var canvas=gameObject.AddComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=31000;gameObject.AddComponent<GraphicRaycaster>();
   var dim=gameObject.AddComponent<Image>();dim.color=new Color(.015f,.04f,.06f,.83f);dim.raycastTarget=true;
   design=CoveUI.Rect(transform,"Report safe area",0,0,1100,810);BuildCard();Layout();
  }
  void BuildCard(){
   if(card){card.gameObject.SetActive(false);Destroy(card.gameObject);}
   card=(RectTransform)Card(design,"Report content",0,small?180:0,1100,small?420:810,Background).transform;
   Label(card,small?"PLAYER CARD":"REPORT PLAYER",32,20,860,55,39,true);
   Action(card,"Close report","Close",922,22,145,55,Raised,Close,CoveSymbol.Close,23);
   ProfileAvatar.Add(card,target,32,94,80);
   var name=Label(card,target.name,136,88,900,48,34,true);GameLocalization.UserContent(name);name.resizeTextForBestFit=true;name.resizeTextMinSize=22;name.resizeTextMaxSize=34;
   GameLocalization.UserContent(Label(card,"#"+target.code,138,137,900,30,20,false,Muted));
   if(small){
    Label(card,"Match continues while this card is open.",32,202,1030,65,24,false,Muted);
    Action(card,"Report player","Report player",32,305,1036,75,Lime,()=>{small=false;BuildCard();},CoveSymbol.Warning,30);return;
   }
   Label(card,"Choose one reason",32,184,1036,38,26,true,Cyan);
   reasons=new CoveButton[PlayerReports.Reasons.Length];
   for(int i=0;i<reasons.Length;i++){int index=i;reasons[i]=Action(card,"Report reason "+PlayerReports.Reasons[i],PlayerReports.Labels[i],32+(i%2)*526,231+(i/2)*62,510,52,Raised,()=>Select(index),size:24);}
   Label(card,"Details (optional; required for Other)",32,486,960,34,23,true);
   details=Entry(card,"Report explanation","Describe what happened…","",32,529,1036,500);details.lineType=InputField.LineType.MultiLineNewline;details.characterValidation=InputField.CharacterValidation.None;
   ((RectTransform)details.transform).sizeDelta=new Vector2(1036,112);details.textComponent.alignment=TextAnchor.UpperLeft;details.textComponent.supportRichText=false;details.textComponent.rectTransform.sizeDelta=new Vector2(996,104);((Text)details.placeholder).rectTransform.sizeDelta=new Vector2(996,104);
   ((Text)details.placeholder).alignment=TextAnchor.UpperLeft;details.onValueChanged.AddListener(_=>Refresh());
   counter=Label(card,"0 / 500",820,646,248,26,18,false,Muted,TextAnchor.MiddleRight);GameLocalization.UserContent(counter);
   feedback=Label(card,"Reports go to the game team for review. The match continues.",32,676,1036,51,21,false,Muted);
   submit=Action(card,"Submit player report","Submit report",570,741,498,49,Lime,Send,CoveSymbol.Warning,26);
   Action(card,"Cancel player report","Cancel",32,741,498,49,Raised,Close,size:26);Refresh();
  }
  void Select(int index){if(sending||complete)return;selected=index;for(int i=0;i<reasons.Length;i++){var plate=reasons[i].GetComponent<CovePlate>();plate.border=i==selected?3:1.5f;plate.borderColor=i==selected?Lime:Line;plate.SetVerticesDirty();reasons[i].GetComponent<CoveFeedback>().activeBorder=i==selected?Lime:Line;}Refresh();}
  void Refresh(){if(counter)counter.text=details.text.Length+" / 500";if(submit)submit.interactable=!sending&&!complete&&selected>=0&&(PlayerReports.Reasons[selected]!="other"||!string.IsNullOrWhiteSpace(details.text));}
  async void Send(){
   if(sending||complete||selected<0||!submit.interactable)return;
   var request=new PlayerReportRequest{target=target.id,reason=PlayerReports.Reasons[selected],details=details.text.Trim(),context=context,evidence=PlayerReports.Reasons[selected]=="abuse"||PlayerReports.Reasons[selected]=="sexual"||PlayerReports.Reasons[selected]=="spam"?evidence:""};
   string payload=JsonUtility.ToJson(request);if(payload!=lastPayload){lastPayload=payload;requestId=Guid.NewGuid().ToString("N");}request.requestId=requestId;
   sending=true;details.interactable=false;foreach(var reason in reasons)reason.interactable=false;feedback.text="Sending your report…";Refresh();
   try{await PlayerReports.Submit(request);if(!this)return;complete=true;feedback.text="Report saved for the game team to review. You can also block this player.";}
   catch(Exception e){if(this)feedback.text=PlayerProfiles.Explain(e);}
   finally{if(this){sending=false;details.interactable=!complete;foreach(var reason in reasons)reason.interactable=!complete;Refresh();}}
  }
  void Layout(){var safe=Screen.safeArea;float scale=Mathf.Min(safe.width/1100,safe.height/810)*.94f;design.localScale=Vector3.one*scale;design.anchoredPosition=new Vector2(safe.center.x-550*scale,-(Screen.height-safe.center.y-405*scale));}
  void Update(){Layout();if(identityScope!=PlayerIdentity.ProfileStorageKey){Destroy(gameObject);return;}if(Input.GetKeyDown(KeyCode.Escape))Close();}
  void Close(){if(sending)return;closedFrame=Time.frameCount;Destroy(gameObject);}
  void OnDestroy(){if(instance==this)instance=null;closedFrame=Time.frameCount;}
 }
 // Reusable row action. The cached account snapshot follows the actual row ID,
 // not its rank or display name, and survives a player leaving this room.
 public sealed class PlayerReportAction:MonoBehaviour {
  PublicProfile target;ulong stableId=ulong.MaxValue;bool cardMode;CoveButton button;
  public bool Available=>PlayerReports.CanReport(target);
  public string TargetId=>target?.id??"";
  public static PlayerReportAction Create(Transform parent,bool cardMode=false){
   var b=Action(parent,"Report player","",0,0,44,44,Raised,()=>{},CoveSymbol.Warning,20);var a=b.gameObject.AddComponent<PlayerReportAction>();a.button=b;a.cardMode=cardMode;
   b.onClick.AddListener(()=>{if(a.Available)PlayerReportUI.Open(a.target,a.cardMode);});return a;
  }
  public static PlayerReportAction CreateCardTarget(Transform parent){
   var r=CoveUI.Rect(parent,"View player card",0,0,341,52);var image=r.gameObject.AddComponent<Image>();image.color=Color.clear;
   var b=r.gameObject.AddComponent<CoveButton>();b.targetGraphic=image;b.transition=Selectable.Transition.None;
   var a=r.gameObject.AddComponent<PlayerReportAction>();a.button=b;a.cardMode=true;b.onClick.AddListener(()=>{if(a.Available)PlayerReportUI.Open(a.target,true);});return a;
  }
  public void Bind(ulong id,bool visible=true){
   if(id!=stableId){stableId=id;target=null;}target=PlayerReports.Resolve(id);
   button.interactable=Available;gameObject.SetActive(visible&&Available);
  }
  public void Bind(PublicProfile p){target=PlayerReports.Copy(p);button.interactable=Available;gameObject.SetActive(Available);}
  public void Place(float x,float y,float width,float height){var r=(RectTransform)transform;r.anchorMin=r.anchorMax=r.pivot=new Vector2(0,1);r.anchoredPosition=new Vector2(x,-y);r.sizeDelta=new Vector2(width,height);var icon=GetComponentInChildren<CoveIcon>();if(icon){var ir=icon.rectTransform;ir.anchorMin=ir.anchorMax=ir.pivot=new Vector2(.5f,.5f);ir.anchoredPosition=Vector2.zero;ir.sizeDelta=Vector2.one*Mathf.Min(22,Mathf.Min(width,height)*.6f);}}
 }
}
