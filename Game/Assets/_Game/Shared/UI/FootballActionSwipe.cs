using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
namespace WhatTheFish {
 // One captured pointer can visit every action. Continuous Dash/Pressure holds
 // survive the visit; Pass/Shoot release when returning to another action.
 public sealed class FootballActionSwipe:MonoBehaviour,IPointerDownHandler,IInitializePotentialDragHandler,IBeginDragHandler,IDragHandler,IPointerUpHandler,IEndDragHandler,ICancelHandler {
  FootballActionSwipe[] targets;readonly List<FootballActionSwipe> holds=new();
  FootballActionSwipe current,transient;int pointer;bool tracking;static bool dispatching;
  public static void Attach(params GameObject[] buttons){var all=new FootballActionSwipe[buttons.Length];for(int i=0;i<all.Length;i++)all[i]=buttons[i].AddComponent<FootballActionSwipe>();foreach(var item in all)item.targets=all;}
  public void Capture(PointerEventData e){if(dispatching||tracking&&pointer!=e.pointerId)return;tracking=true;pointer=e.pointerId;if(!current)current=this;Remember(this);}
  public void OnPointerDown(PointerEventData e){var view=PlayerView.Instance;if(e.button==PointerEventData.InputButton.Left&&view&&view.active&&!view.InputBlocked&&FootballBall.Allowed)Capture(e);}
  void Remember(FootballActionSwipe item){var effort=item.GetComponent<FootballEffortButton>();var kick=item.GetComponent<KickButton>();var pass=item.GetComponent<FootballPassButton>();if(effort&&effort.enabled&&effort.Tracking){if(!holds.Contains(item))holds.Add(item);}else if(kick&&kick.enabled&&kick.Tracking||pass&&pass.Tracking)transient=item;}
  public void OnInitializePotentialDrag(PointerEventData e)=>e.useDragThreshold=false;
  public void OnBeginDrag(PointerEventData e)=>Track(e);
  public void OnDrag(PointerEventData e)=>Track(e);
  public void Track(PointerEventData e){
   if(!tracking||e.pointerId!=pointer)return;
   FootballActionSwipe next=null;
   foreach(var item in targets){var group=item.GetComponent<CanvasGroup>();if(group&&!group.interactable)continue;if(RectTransformUtility.RectangleContainsScreenPoint((RectTransform)item.transform,e.position,e.pressEventCamera)){next=item;break;}}
   if(!next||next==current)return;
   if(transient){var nextKick=next.GetComponent<KickButton>();var nextPass=next.GetComponent<FootballPassButton>();if(nextPass||nextKick&&nextKick.enabled)CancelTransient();else Release(transient,e);transient=null;}
   current=next;if(holds.Contains(next))return;
   dispatching=true;
   try{
    var kick=next.GetComponent<KickButton>();var effort=next.GetComponent<FootballEffortButton>();var pass=next.GetComponent<FootballPassButton>();var tackle=next.GetComponent<TackleButton>();
    if(kick&&kick.enabled){if(kick.Tracking)return;kick.Refresh();kick.OnPointerDown(e);if(!kick.Tracking)return;}
    else if(effort&&effort.enabled){if(effort.Tracking)return;effort.Refresh();effort.OnPointerDown(e);if(!effort.Tracking)return;}
    else if(pass&&pass.enabled){if(pass.Tracking)return;pass.Refresh();pass.OnPointerDown(e);if(!pass.Tracking)return;}
    else if(tackle&&tackle.enabled){tackle.Refresh();tackle.OnPointerDown(e);}
    Remember(next);
   }finally{dispatching=false;}
  }
  static void Release(FootballActionSwipe item,PointerEventData e){var kick=item.GetComponent<KickButton>();if(kick&&kick.enabled)kick.OnPointerUp(e);var pass=item.GetComponent<FootballPassButton>();if(pass)pass.OnPointerUp(e);}
  public void OnPointerUp(PointerEventData e){if(!tracking||e.pointerId!=pointer)return;if(!KickButton.CanceledTouch(pointer)&&transient)Release(transient,e);Clear();}
  public void OnEndDrag(PointerEventData e){if(tracking&&e.pointerId==pointer)Clear();}
  public void OnCancel(BaseEventData e)=>Clear();
  void Update(){var view=PlayerView.Instance;if(tracking&&(!view||view.InputBlocked||!view.active||!FootballBall.Allowed))Clear();}
  void CancelTransient(){transient.GetComponent<KickButton>()?.CancelGesture();transient.GetComponent<FootballPassButton>()?.Cancel();}
  void Clear(){foreach(var item in holds)if(item)item.GetComponent<FootballEffortButton>()?.Clear();holds.Clear();if(transient)CancelTransient();transient=current=null;tracking=false;}
  void OnDisable()=>Clear();void OnApplicationFocus(bool focused){if(!focused)Clear();}void OnApplicationPause(bool paused){if(paused)Clear();}
 }
}
