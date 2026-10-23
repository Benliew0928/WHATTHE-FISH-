using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

namespace WhatTheFish {
 // Open type: a small team number and outlined name, attached to the animated head.
 [DefaultExecutionOrder(10000)]
 public sealed class CharacterIdentityBadges:MonoBehaviour {
  sealed class Badge {
   public Athlete actor;public CharacterIdentity identity;public RectTransform root;
   public Image hit,leader;public BasketballPlate chip;public Text number,name;public Button button;
   public string profileId;public FootballTeam team;public Vector2 anchor,offset;public float compactUntil;
  }
  static CharacterIdentityBadges instance;Canvas canvas;
  readonly Dictionary<Athlete,Badge> views=new();readonly Dictionary<Athlete,Rect> locations=new();
  readonly List<Athlete> removed=new();readonly List<Badge> candidates=new();
  readonly List<Rect> occupied=new(),obstacles=new();readonly Vector3[] corners=new Vector3[4];
  static readonly List<RectTransform> hud=new();
  Selectable[] controls=System.Array.Empty<Selectable>();float nextScan;
  static readonly Vector2[] slots={Vector2.zero,new Vector2(0,-18),new Vector2(-12,-9),new Vector2(12,-9)};
  public static void ReserveHUD(RectTransform bounds){if(bounds&&!hud.Contains(bounds))hud.Add(bounds);}
  public static bool TryGetScreenRect(Athlete actor,out Rect rect){rect=default;return instance&&instance.locations.TryGetValue(actor,out rect);}
  public static bool TryGetPresentation(Athlete actor,out string number,out string name,out FootballTeam team,out Color plate,out Color accent){
   number=name="";team=FootballTeam.None;plate=accent=default;
   if(!instance||!instance.locations.ContainsKey(actor)||!instance.views.TryGetValue(actor,out var badge))return false;
   number=badge.number.gameObject.activeSelf?badge.number.text:"";name=badge.name.gameObject.activeSelf?badge.name.text:"";team=badge.team;accent=badge.chip.color;return true;
  }
  public static void Ensure(){if(!instance)instance=new GameObject("Player identity badges",typeof(RectTransform)).AddComponent<CharacterIdentityBadges>();}
  void Awake(){canvas=gameObject.AddComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=-10;gameObject.AddComponent<GraphicRaycaster>();}
  void OnDestroy(){if(instance==this)instance=null;}
  Badge Get(Athlete actor,CharacterIdentity identity){
   if(views.TryGetValue(actor,out var found))return found;
   var root=CoveUI.Rect(transform,"Player identity",0,0,130,25);
   var hit=root.gameObject.AddComponent<Image>();hit.color=Color.clear;hit.raycastTarget=false;
   var leader=CoveUI.Rect(root,"Head anchor",0,0,1,1).gameObject.AddComponent<Image>();leader.color=new Color(1,1,1,.65f);leader.raycastTarget=false;leader.transform.SetAsFirstSibling();
   var chip=BasketballUIStyle.Plate(root,"Team number tab",0,1,27,23,CoveUI.Mint,7,1);chip.edge=new Color(.03f,.12f,.17f,.8f);chip.raycastTarget=false;
   var number=CoveUI.Text(root,"",0,1,27,23,12,true,Color.white,TextAnchor.MiddleCenter);number.name="Player number";number.font=SportsUIStyle.DisplayFont;
   var name=CoveUI.Text(root,"",33,0,90,25,13,true,Color.white,TextAnchor.MiddleLeft);name.name="Player name";name.font=SportsUIStyle.DisplayFont;name.supportRichText=false;
   number.raycastTarget=name.raycastTarget=false;number.horizontalOverflow=name.horizontalOverflow=HorizontalWrapMode.Overflow;
   var outline=name.gameObject.AddComponent<Outline>();outline.effectColor=new Color(.015f,.06f,.09f,.95f);outline.effectDistance=new Vector2(1,-1);outline.useGraphicAlpha=false;
   var shadow=name.gameObject.AddComponent<Shadow>();shadow.effectColor=new Color(.015f,.06f,.09f,.65f);shadow.effectDistance=new Vector2(0,-2);
   var view=new Badge{actor=actor,identity=identity,root=root,hit=hit,leader=leader,chip=chip,number=number,name=name};
   view.button=root.gameObject.AddComponent<Button>();view.button.targetGraphic=hit;view.button.transition=Selectable.Transition.None;view.button.navigation=new Navigation{mode=Navigation.Mode.None};
   view.button.onClick.AddListener(()=>{if(!string.IsNullOrEmpty(view.profileId))SocialPanel.OpenPlayer(view.profileId);});
   views.Add(actor,view);return view;
  }
  static void Place(RectTransform r,float x,float y,float width,float height){r.anchorMin=r.anchorMax=r.pivot=new Vector2(0,1);r.anchoredPosition=new Vector2(x,-y);r.sizeDelta=new Vector2(width,height);}
  internal static string PlayerName(Athlete actor){
   var net=actor.GetComponent<NetworkAthlete>();if(net&&net.Nickname.Value.Length>0)return net.Nickname.Value.ToString();
   var basketball=actor.GetComponent<BasketballBot>();if(basketball)return "BOT "+basketball.Slot;
   var football=actor.GetComponent<FootballBot>();if(football)return "BOT "+football.Slot;
   if(IslandBots.IsBot(actor))return IslandBots.Name(actor);
   return actor==AppRoot.Instance?.LocalAthlete?PlayerIdentity.DisplayName:"PLAYER";
  }
  void AddObstacle(RectTransform bounds){
   if(!bounds||!bounds.gameObject.activeInHierarchy)return;var parent=bounds.GetComponentInParent<Canvas>();if(parent&&!parent.enabled)return;
   bounds.GetWorldCorners(corners);var rect=new Rect(corners[0].x,Screen.height-corners[2].y,corners[2].x-corners[0].x,corners[2].y-corners[0].y);
   if(rect.width<Screen.width*.8f&&rect.height<Screen.height*.8f)obstacles.Add(rect);
  }
  bool Clear(Rect rect,Rect safe){
   if(rect.xMin<safe.xMin||rect.xMax>safe.xMax||rect.yMin<safe.yMin||rect.yMax>safe.yMax)return false;
   foreach(var r in occupied)if(r.Overlaps(rect))return false;
   foreach(var r in obstacles)if(r.Overlaps(rect))return false;return true;
  }
  bool Position(Badge view,float width,float height,float scale,Rect safe,out Rect rect,bool displace=true){
   rect=new Rect(view.anchor.x-width*.5f,view.anchor.y-height-5*scale,width,height);
   var prior=rect;prior.position+=view.offset*scale;
   if(view.root.gameObject.activeSelf&&Clear(prior,safe)){rect=prior;return true;}
   foreach(var offset in slots){if(!displace&&offset!=Vector2.zero)continue;var test=rect;test.position+=offset*scale;if(!Clear(test,safe))continue;rect=test;view.offset=offset;return true;}return false;
  }
  void LateUpdate(){
   var app=AppRoot.Instance;var camera=Camera.main;locations.Clear();occupied.Clear();obstacles.Clear();candidates.Clear();removed.Clear();
   foreach(var entry in views)if(!entry.Key)removed.Add(entry.Key);
   foreach(var actor in removed){Destroy(views[actor].root.gameObject);views.Remove(actor);}
   bool show=app&&app.Exploring&&camera&&!SocialPanel.Visible&&!AccountPanel.Visible&&!TeleportSequence.OwnsCamera&&!FootballReplay.Presenting&&!FootballMVP.Presenting&&!BasketballMVP.Presenting&&!FishingCeremony.Presenting;canvas.enabled=show;
   if(!show){foreach(var v in views.Values)v.root.gameObject.SetActive(false);return;}
   float scale=Mathf.Clamp(Screen.height/720f,.8f,2);var area=Screen.safeArea;
   var safe=Rect.MinMaxRect(area.xMin+6*scale,Screen.height-area.yMax+58*scale,area.xMax-6*scale,Screen.height-area.yMin-70*scale);
   for(int i=hud.Count-1;i>=0;i--){if(!hud[i])hud.RemoveAt(i);else AddObstacle(hud[i]);}
   if(Time.unscaledTime>=nextScan){nextScan=Time.unscaledTime+.5f;controls=app.HudFrame?app.HudFrame.GetComponentsInChildren<Selectable>(true):System.Array.Empty<Selectable>();}
   foreach(var control in controls)if(control&&control.targetGraphic&&control.targetGraphic.canvasRenderer.GetInheritedAlpha()>.02f)AddObstacle((RectTransform)control.transform);
   if(FishingHUD.Instance&&app.SelectedSport==SportId.Fishing){var p=FishingHUD.Instance.AimScreenPoint;obstacles.Add(new Rect(p.x-28*scale,Screen.height-p.y-28*scale,56*scale,56*scale));}
   foreach(var actor in Athlete.Active){
    if(!actor||!actor.isActiveAndEnabled||actor.inTransit)continue;
    var identity=actor.GetComponent<CharacterIdentity>();if(!identity||identity.Number==0)continue;
    if(actor==app.LocalAthlete&&app.view.mode==0&&!app.view.GolfShotCameraActive)continue;
    var world=identity.LabelAnchor;var point=camera.WorldToScreenPoint(world);
    if(point.z<=camera.nearClipPlane||!area.Contains(point))continue;
    var delta=world-camera.transform.position;if(Physics.Raycast(camera.transform.position,delta.normalized,delta.magnitude-.15f,1<<8,QueryTriggerInteraction.Ignore))continue;
    var view=Get(actor,identity);view.anchor=new Vector2(point.x,Screen.height-point.y);candidates.Add(view);
   }
   // Stable identity order prevents labels swapping owners when the roster changes.
   candidates.Sort((a,b)=>{int local=(b.actor==app.LocalAthlete).CompareTo(a.actor==app.LocalAthlete);return local!=0?local:a.identity.Number.CompareTo(b.identity.Number);});
   foreach(var view in candidates){
    bool local=view.actor==app.LocalAthlete;view.team=view.identity.UniformTeam;
    string fullName=local?"You":PlayerName(view.actor);view.name.fontSize=Mathf.RoundToInt(13*scale);view.name.text=fullName;
    // Truncate on grapheme boundaries, including emoji/surrogate pairs.
    float limit=112*scale;if(view.name.preferredWidth>limit){var starts=StringInfo.ParseCombiningCharacters(fullName);int count=starts.Length;do{count--;view.name.text=fullName.Substring(0,starts[count])+"…";}while(count>0&&view.name.preferredWidth>limit);}
    float nameWidth=Mathf.Ceil(view.name.preferredWidth)+4*scale;
    float width=local?nameWidth:33*scale+nameWidth,height=25*scale;bool compact=!local&&Time.unscaledTime<view.compactUntil;
    if(compact)width=27*scale;
    bool placed=Position(view,width,height,scale,safe,out var rect,local||compact);
    if(!placed&&!local&&!compact){compact=true;width=27*scale;placed=Position(view,width,height,scale,safe,out rect);view.compactUntil=Time.unscaledTime+.8f;}
    if(!placed){view.root.gameObject.SetActive(false);continue;}
    view.root.gameObject.SetActive(true);Place(view.root,rect.x,rect.y,rect.width,rect.height);
    view.number.text=view.identity.Number.ToString("00");view.number.fontSize=Mathf.RoundToInt(12*scale);
    view.chip.color=view.team==FootballTeam.None?CoveUI.Mint:CharacterIdentity.TeamColor(view.team);view.chip.radius=Vector2.one*(view.team==FootballTeam.B?3:7)*scale;view.chip.Refresh();
    view.chip.gameObject.SetActive(!local);view.number.gameObject.SetActive(!local);view.name.gameObject.SetActive(!compact);
    view.number.color=view.team==FootballTeam.None?CoveUI.Ink:Color.white;view.name.color=local?new Color(1,.91f,.55f):Color.white;
    Place(view.chip.rectTransform,0,scale,27*scale,23*scale);Place(view.number.rectTransform,0,scale,27*scale,23*scale);Place(view.name.rectTransform,local?0:33*scale,0,nameWidth,height);
    var net=view.actor.GetComponent<NetworkAthlete>();view.profileId=net?net.ProfileId.Value.ToString():"";
    bool tappable=!local&&!string.IsNullOrEmpty(view.profileId);view.root.name=tappable?"View character profile "+view.profileId:"Player identity";view.hit.raycastTarget=view.button.interactable=tappable;
    var start=new Vector2(rect.width*.5f,-rect.height);var end=new Vector2(view.anchor.x-rect.x,-(view.anchor.y-rect.y));var line=end-start;
    view.leader.gameObject.SetActive(view.offset.sqrMagnitude>1);var lr=view.leader.rectTransform;lr.anchorMin=lr.anchorMax=new Vector2(0,1);lr.pivot=new Vector2(.5f,0);lr.anchoredPosition=start;lr.sizeDelta=new Vector2(scale,line.magnitude);lr.localRotation=Quaternion.Euler(0,0,Mathf.Atan2(line.y,line.x)*Mathf.Rad2Deg-90);
    locations[view.actor]=rect;var padded=rect;padded.xMin-=3*scale;padded.xMax+=3*scale;padded.yMin-=3*scale;padded.yMax+=3*scale;occupied.Add(padded);
   }
   foreach(var pair in views)if(!locations.ContainsKey(pair.Key))pair.Value.root.gameObject.SetActive(false);
  }
 }
}
