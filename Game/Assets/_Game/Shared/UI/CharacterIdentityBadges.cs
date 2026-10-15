using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace WhatTheFish {
 // Reuse the sports font and a small badge view per visible room member. No new badge sprites/font atlas.
 [DefaultExecutionOrder(10000)]
 public sealed class CharacterIdentityBadges:MonoBehaviour {
  sealed class Badge {public RectTransform root;public Image background;public Text text;public Outline outline;}
  static CharacterIdentityBadges instance;Canvas canvas;readonly List<Rect> occupied=new();readonly List<Badge> views=new();
  static readonly List<RectTransform> hud=new();readonly List<Rect> hudBounds=new();readonly Vector3[] corners=new Vector3[4];
  public static void ReserveHUD(RectTransform bounds){if(bounds&&!hud.Contains(bounds))hud.Add(bounds);}
  readonly Dictionary<Athlete,Rect> locations=new();
  public static bool TryGetScreenRect(Athlete actor,out Rect rect){rect=default;return instance&&instance.locations.TryGetValue(actor,out rect);}
  public static void Ensure(){if(!instance)instance=new GameObject("Player identity badges",typeof(RectTransform)).AddComponent<CharacterIdentityBadges>();}
  void Awake(){canvas=gameObject.AddComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=-10;}
  void OnDestroy(){if(instance==this)instance=null;}
  Badge Get(int index){
   if(index<views.Count)return views[index];
   var root=CoveUI.Rect(transform,"Player number",0,0,38,25);var background=root.gameObject.AddComponent<Image>();background.raycastTarget=false;
   var outline=root.gameObject.AddComponent<Outline>();outline.effectColor=Color.white;outline.effectDistance=new Vector2(2,-2);
   var text=CoveUI.Text(root,"",0,0,38,25,15,true,Color.white,TextAnchor.MiddleCenter);text.font=SportsUIStyle.DisplayFont;text.horizontalOverflow=HorizontalWrapMode.Overflow;CoveUI.Stretch(text.rectTransform);
   var view=new Badge{root=root,background=background,text=text,outline=outline};views.Add(view);return view;
  }
  void LateUpdate(){
   var app=AppRoot.Instance;var camera=Camera.main;int count=0;occupied.Clear();locations.Clear();hudBounds.Clear();
   for(int i=hud.Count-1;i>=0;i--){var bounds=hud[i];if(!bounds){hud.RemoveAt(i);continue;}if(!bounds.gameObject.activeInHierarchy)continue;bounds.GetWorldCorners(corners);hudBounds.Add(new Rect(corners[0].x,Screen.height-corners[2].y,corners[2].x-corners[0].x,corners[2].y-corners[0].y));}
   bool show=app&&app.Exploring&&camera&&!FootballReplay.Presenting&&!BasketballMVP.Presenting;canvas.enabled=show;
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
     var safe=Screen.safeArea;float width=(actor.IsSpectator?(local?102:65):local?73:38)*scale,height=25*scale;
     var rect=new Rect(screen.x-width*.5f,Screen.height-screen.y-height,width,height);
     if(rect.x<safe.xMin||rect.xMax>safe.xMax||rect.y<Screen.height-safe.yMax+58*scale||rect.yMax>Screen.height-safe.yMin-70*scale)continue;
     foreach(var obstacle in hudBounds)if(obstacle.Overlaps(rect))rect.y=obstacle.yMax+5*scale;
     var anchor=rect;
     for(int step=0;step<10&&occupied.Exists(r=>r.Overlaps(rect));step++){rect=anchor;rect.x+=(step%2==0?1:-1)*(step/2+1)*(width+3*scale);}
     if(occupied.Exists(r=>r.Overlaps(rect))||hudBounds.Exists(r=>r.Overlaps(rect))||rect.x<safe.xMin||rect.xMax>safe.xMax||rect.yMax>Screen.height-safe.yMin-70*scale)continue;occupied.Add(rect);locations[actor]=rect;
     var view=Get(count++);view.root.gameObject.SetActive(true);view.root.anchoredPosition=new Vector2(rect.x,-rect.y);view.root.sizeDelta=rect.size;
     var team=id.UniformTeam;var color=team==FootballTeam.None?new Color(.08f,.13f,.19f,.94f):CharacterIdentity.TeamColor(team);color.a=.94f;view.background.color=color;
     view.outline.enabled=local;view.text.fontSize=Mathf.RoundToInt(15*scale);view.text.text=(local?"YOU ":"")+(actor.IsSpectator?"S ":"")+id.Number.ToString("00");
    }
   }
   for(int i=count;i<views.Count;i++)views[i].root.gameObject.SetActive(false);
  }
 }
}
