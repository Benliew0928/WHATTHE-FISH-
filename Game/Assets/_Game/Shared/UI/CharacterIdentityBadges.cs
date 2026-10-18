using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace WhatTheFish {
 // Reuse the sports font and a small badge view per visible room member. No new badge sprites/font atlas.
 [DefaultExecutionOrder(10000)]
 public sealed class CharacterIdentityBadges:MonoBehaviour {
  sealed class Badge {public RectTransform root;public BasketballPlate background,numberChip,roleChip;public Text number,name,role;public FootballTeam team;}
  static CharacterIdentityBadges instance;Canvas canvas;readonly List<Rect> occupied=new();readonly List<Badge> views=new();
  static readonly List<RectTransform> hud=new();readonly List<Rect> hudBounds=new();readonly Vector3[] corners=new Vector3[4];
  public static void ReserveHUD(RectTransform bounds){if(bounds&&!hud.Contains(bounds))hud.Add(bounds);}
  readonly Dictionary<Athlete,Rect> locations=new();readonly Dictionary<Athlete,Badge> labels=new();
  public static bool TryGetScreenRect(Athlete actor,out Rect rect){rect=default;return instance&&instance.locations.TryGetValue(actor,out rect);}
  public static bool TryGetPresentation(Athlete actor,out string number,out string name,out FootballTeam team,out Color plate,out Color accent){
   number=name="";team=FootballTeam.None;plate=accent=default;
   if(!instance||!instance.labels.TryGetValue(actor,out var badge))return false;
   number=badge.number.text;name=badge.name.text;team=badge.team;plate=badge.background.color;accent=badge.numberChip.color;return true;
  }
  public static void Ensure(){if(!instance)instance=new GameObject("Player identity badges",typeof(RectTransform)).AddComponent<CharacterIdentityBadges>();}
  void Awake(){canvas=gameObject.AddComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=-10;}
  void OnDestroy(){if(instance==this)instance=null;}
  Badge Get(int index){
   if(index<views.Count)return views[index];
   var root=CoveUI.Rect(transform,"Player identity",0,0,130,29);
   var background=BasketballUIStyle.Plate(root,"Neutral name plate",0,0,130,29,new Color(1,.976f,.914f,.96f),12,1,true);background.edge=new Color(.07f,.25f,.31f,.82f);CoveUI.Stretch(background.rectTransform);
   var numberChip=BasketballUIStyle.Plate(root,"Team number tab",2,2,29,25,CoveUI.Ink,10,0);
   var number=CoveUI.Text(root,"",2,2,29,25,13,true,Color.white,TextAnchor.MiddleCenter);number.name="Player number";number.font=SportsUIStyle.DisplayFont;number.horizontalOverflow=HorizontalWrapMode.Overflow;
   var name=CoveUI.Text(root,"",37,1,90,27,12,true,CoveUI.Ink,TextAnchor.MiddleLeft);name.name="Player name";name.font=SportsUIStyle.DisplayFont;name.horizontalOverflow=HorizontalWrapMode.Overflow;
   var roleChip=BasketballUIStyle.Plate(root,"Player role",0,4,30,21,CoveUI.Gold,8,0);roleChip.gameObject.SetActive(false);
   var role=CoveUI.Text(roleChip.transform,"",0,0,30,21,8,true,CoveUI.Ink,TextAnchor.MiddleCenter);role.name="Player role";CoveUI.Stretch(role.rectTransform);
   var view=new Badge{root=root,background=background,numberChip=numberChip,roleChip=roleChip,number=number,name=name,role=role};views.Add(view);return view;
  }
  static void Place(RectTransform r,float x,float y,float width,float height){r.anchorMin=r.anchorMax=r.pivot=new Vector2(0,1);r.anchoredPosition=new Vector2(x,-y);r.sizeDelta=new Vector2(width,height);}
  static string PlayerName(Athlete actor){
   var net=actor.GetComponent<NetworkAthlete>();if(net&&net.Nickname.Value.Length>0)return net.Nickname.Value.ToString();
   var basketball=actor.GetComponent<BasketballBot>();if(basketball)return "BOT "+basketball.Slot;
   var football=actor.GetComponent<FootballBot>();if(football)return "BOT "+football.Slot;
   return actor==AppRoot.Instance?.LocalAthlete?PlayerIdentity.DisplayName:"PLAYER";
  }
  void LateUpdate(){
   var app=AppRoot.Instance;var camera=Camera.main;int count=0;occupied.Clear();locations.Clear();labels.Clear();hudBounds.Clear();
   for(int i=hud.Count-1;i>=0;i--){var bounds=hud[i];if(!bounds){hud.RemoveAt(i);continue;}if(!bounds.gameObject.activeInHierarchy)continue;bounds.GetWorldCorners(corners);hudBounds.Add(new Rect(corners[0].x,Screen.height-corners[2].y,corners[2].x-corners[0].x,corners[2].y-corners[0].y));}
   bool show=app&&app.Exploring&&camera&&!FootballReplay.Presenting&&!FootballMVP.Presenting&&!BasketballMVP.Presenting;canvas.enabled=show;
   if(show){
    float scale=Mathf.Clamp(Screen.height/720f,.8f,2);
    foreach(var actor in Athlete.Active){
     if(!actor||!actor.isActiveAndEnabled||actor.inTransit)continue;
     var id=actor.GetComponent<CharacterIdentity>();if(!id||id.Number==0)continue;
     bool local=actor==app.LocalAthlete;if(local&&app.view.mode==0)continue;
     Vector3 world=actor.transform.position+Vector3.up*1.86f;var screen=camera.WorldToScreenPoint(world);
     if(screen.z<=camera.nearClipPlane||screen.x<0||screen.x>Screen.width||screen.y<0||screen.y>Screen.height)continue;
     Vector3 delta=world-camera.transform.position;
     if(Physics.Raycast(camera.transform.position,delta.normalized,delta.magnitude-.15f,1<<8,QueryTriggerInteraction.Ignore))continue;
     string playerName=PlayerName(actor);string role=local?"YOU":actor.IsSpectator?"SPEC":"";
     var view=Get(count);view.name.fontSize=Mathf.RoundToInt(12*scale);view.name.text=playerName;
     float nameWidth=Mathf.Max(48*scale,Mathf.Ceil(view.name.preferredWidth)+12*scale),roleWidth=(role.Length==0?0:role=="SPEC"?39:31)*scale;
     var safe=Screen.safeArea;float width=41*scale+nameWidth+roleWidth,height=29*scale;
     var rect=new Rect(screen.x-width*.5f,Screen.height-screen.y-height,width,height);
     if(rect.x<safe.xMin||rect.xMax>safe.xMax||rect.y<Screen.height-safe.yMax+58*scale||rect.yMax>Screen.height-safe.yMin-70*scale)continue;
     foreach(var obstacle in hudBounds)if(obstacle.Overlaps(rect))rect.y=obstacle.yMax+5*scale;
     var anchor=rect;
     for(int step=0;step<10&&occupied.Exists(r=>r.Overlaps(rect));step++){rect=anchor;rect.x+=(step%2==0?1:-1)*(step/2+1)*(width+3*scale);}
     if(occupied.Exists(r=>r.Overlaps(rect))||hudBounds.Exists(r=>r.Overlaps(rect))||rect.x<safe.xMin||rect.xMax>safe.xMax||rect.yMax>Screen.height-safe.yMin-70*scale)continue;occupied.Add(rect);locations[actor]=rect;
     count++;view.root.gameObject.SetActive(true);view.root.anchoredPosition=new Vector2(rect.x,-rect.y);view.root.sizeDelta=rect.size;
     var team=id.UniformTeam;var color=team==FootballTeam.None?CoveUI.Muted:CharacterIdentity.TeamColor(team);color.a=.98f;view.team=team;view.numberChip.color=color;
     view.background.color=local?new Color(1,.94f,.76f,.98f):new Color(1,.976f,.914f,.96f);view.background.border=local?1.8f:1;view.background.Refresh();
     float unit=scale;Place(view.numberChip.rectTransform,2*unit,2*unit,29*unit,25*unit);Place(view.number.rectTransform,2*unit,2*unit,29*unit,25*unit);
     Place(view.name.rectTransform,37*unit,1*unit,nameWidth,27*unit);
     view.number.fontSize=Mathf.RoundToInt(13*scale);view.number.text=id.Number.ToString("00");
     view.roleChip.gameObject.SetActive(role.Length>0);if(role.Length>0){float x=37*unit+nameWidth;Place(view.roleChip.rectTransform,x,4*unit,roleWidth,21*unit);view.roleChip.color=local?CoveUI.Gold:CoveUI.Sky;view.role.text=role;view.role.fontSize=Mathf.RoundToInt(8*scale);}
     labels[actor]=view;
    }
   }
   for(int i=count;i<views.Count;i++)views[i].root.gameObject.SetActive(false);
  }
 }
}
