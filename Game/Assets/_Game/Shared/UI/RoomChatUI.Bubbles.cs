using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace WhatTheFish {
 public sealed partial class RoomChatUI {
  sealed class Bubble {public RoomChatMessage message;public RectTransform rect;public CanvasGroup group;public float until;}
  readonly Dictionary<string,Bubble> bubbles=new();
  readonly List<Rect> occupied=new();
  readonly List<string> expired=new();
  RectTransform bubbleRoot;
  Selectable[] protectedControls=System.Array.Empty<Selectable>();float nextControlScan;
  public int VisibleBubbleCount=>bubbles.Values.Count(b=>b.rect.gameObject.activeSelf);
  void BuildBubbles(){bubbleRoot=CoveUI.Rect(safe,"Speech above players",0,0,1600,900);CoveUI.Stretch(bubbleRoot);bubbleRoot.SetAsFirstSibling();}
  void ClearBubbles(){foreach(var b in bubbles.Values)Destroy(b.rect.gameObject);bubbles.Clear();}
  void ShowBubble(RoomChatMessage message){
   if(!app.Exploring||!Visible||chat.HideBubbles)return;
   if(bubbles.Remove(message.member,out var old))Destroy(old.rect.gameObject);
   bool sticker=!string.IsNullOrEmpty(message.sticker);RectTransform rect;
   if(sticker){rect=CoveUI.Image(bubbleRoot,"Reaction",Sticker(message.sticker),0,0,88,88).rectTransform;}
   else{
    var plate=CoveUI.Plate(bubbleRoot,"Speech",0,0,264,78,Cream,16,2,true);plate.borderColor=Ink;rect=plate.rectTransform;
    var label=Label(rect,message.text,12,8,240,60,22,Ink);label.alignment=TextAnchor.MiddleCenter;label.verticalOverflow=VerticalWrapMode.Truncate;
    var tail=CoveUI.Plate(rect,"Speech tail",125,69,14,14,Cream,2,0);tail.rectTransform.localRotation=Quaternion.Euler(0,0,45);
   }
   rect.pivot=new Vector2(.5f,0);rect.anchorMin=rect.anchorMax=Vector2.zero;
   var group=rect.gameObject.AddComponent<CanvasGroup>();group.blocksRaycasts=false;group.interactable=false;
   rect.gameObject.SetActive(false);bubbles[message.member]=new Bubble{message=message,rect=rect,group=group,until=Time.unscaledTime+(sticker?3:4)};
  }
  void LateUpdate(){
   if(!bubbleRoot||!app)return;
   bool show=Visible&&app.Exploring&&!chat.HideBubbles&&!(SkySailWorld.Instance&&SkySailWorld.Instance.Travelling);
   var camera=Camera.main;occupied.Clear();expired.Clear();int count=0;
   float w=safe.rect.width,h=safe.rect.height;
   // Keep scores, minimap and touch controls completely unobscured.
   occupied.Add(new Rect(0,h-170,w,170));occupied.Add(new Rect(0,0,w,215));occupied.Add(new Rect(0,h-285,345,285));
   occupied.Add(new Rect(0,0,300,300));occupied.Add(new Rect(w-490,0,490,600));
   if(Time.unscaledTime>=nextControlScan){nextControlScan=Time.unscaledTime+.5f;protectedControls=app.HudFrame?app.HudFrame.GetComponentsInChildren<Selectable>():System.Array.Empty<Selectable>();}
   foreach(var control in protectedControls){if(!control||!control.gameObject.activeInHierarchy||!control.targetGraphic||control.targetGraphic.canvasRenderer.GetInheritedAlpha()<.02f)continue;
    var area=Bounds((RectTransform)control.transform);if(area.width<w*.8f&&area.height<h*.8f)occupied.Add(area);
   }
   if(open&&panel)occupied.Add(Bounds(panel));if(quick&&quickPanel)occupied.Add(Bounds(quickPanel));if(options&&optionsPanel)occupied.Add(Bounds(optionsPanel));
   if(app.SelectedSport==SportId.Fishing&&FishingHUD.Instance){var aim=Local(FishingHUD.Instance.AimScreenPoint);occupied.Add(new Rect(aim.x-44,aim.y-44,88,88));if(fishingBoard)occupied.Add(Bounds(fishingBoard));}
   var people=RoomTeleport.Players();
   if(camera){
    foreach(var actor in Athlete.Active)if(actor&&CharacterIdentityBadges.TryGetScreenRect(actor,out var tag)){
     var min=Local(new Vector2(tag.xMin,Screen.height-tag.yMax));var max=Local(new Vector2(tag.xMax,Screen.height-tag.yMin));occupied.Add(Rect.MinMaxRect(min.x,min.y,max.x,max.y));
    }
    foreach(var p in people){var point=camera.WorldToScreenPoint(p.transform.position+Vector3.up*1.45f);if(point.z>0){var local=Local(point);occupied.Add(new Rect(local.x-22,local.y-45,44,70));}
     if(app.SelectedSport==SportId.Golf&&GolfMatchManager.Instance){var golf=GolfMatchManager.Instance.Ball(p.GetComponent<Athlete>());if(golf){var ballPoint=camera.WorldToScreenPoint(golf.transform.position);if(ballPoint.z>0){var local=Local(ballPoint);occupied.Add(new Rect(local.x-22,local.y-22,44,44));}}}
    }
    Transform ball=app.SelectedSport==SportId.Football&&FootballBall.Instance?FootballBall.Instance.transform:app.SelectedSport==SportId.Basketball&&BasketballBall.Active?BasketballBall.Active.transform:null;
    if(ball){var point=camera.WorldToScreenPoint(ball.position);if(point.z>0){var local=Local(point);occupied.Add(new Rect(local.x-26,local.y-26,52,52));}}
   }
   foreach(var b in bubbles.Values.OrderByDescending(b=>b.message.member==app.rooms.MemberId).ThenByDescending(b=>b.until)){
    float remaining=b.until-Time.unscaledTime;if(remaining<-.24f){expired.Add(b.message.member);continue;}
    b.rect.gameObject.SetActive(false);
    if(!show||!camera||count>=3||chat.IsMuted(b.message.member))continue;
    var actor=people.FirstOrDefault(p=>p.MemberId.Value.ToString()==b.message.member);if(!actor)continue;
    var head=actor.transform.position+Vector3.up*2.25f;var screen=camera.WorldToScreenPoint(head);
    if(screen.z<=0||!Screen.safeArea.Contains(screen))continue;
    if(Physics.Linecast(camera.transform.position,head,out var hit,Physics.DefaultRaycastLayers,QueryTriggerInteraction.Ignore)&&!hit.transform.IsChildOf(actor.transform)&&hit.distance<Vector3.Distance(camera.transform.position,head)-.65f)continue;
    Vector2 point=Local(screen)+Vector2.up*12;Vector2 size=b.rect.rect.size;bool placed=false;
    // Only move upward: a displaced bubble must still clearly belong to its player.
    for(int offset=0;offset<=40;offset+=20){var rect=new Rect(point.x-size.x/2,point.y+offset,size.x,size.y);
     if(rect.xMin<8||rect.xMax>w-8||rect.yMax>h-8||occupied.Any(r=>r.Overlaps(rect)))continue;
     b.rect.anchoredPosition=new Vector2(point.x,point.y+offset);occupied.Add(rect);placed=true;break;
    }
    if(!placed)continue;b.group.alpha=Mathf.Clamp01((remaining+.24f)/.24f);b.rect.gameObject.SetActive(true);count++;
   }
   foreach(string member in expired){Destroy(bubbles[member].rect.gameObject);bubbles.Remove(member);}
  }
  Vector2 Local(Vector2 screen){RectTransformUtility.ScreenPointToLocalPointInRectangle(safe,screen,null,out var point);return point-safe.rect.min;}
  Rect Bounds(RectTransform rect){var corners=new Vector3[4];rect.GetWorldCorners(corners);Vector2 min=Local(RectTransformUtility.WorldToScreenPoint(null,corners[0])),max=Local(RectTransformUtility.WorldToScreenPoint(null,corners[2]));return Rect.MinMaxRect(min.x,min.y,max.x,max.y);}
 }
}
