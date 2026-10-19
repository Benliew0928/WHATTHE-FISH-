using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace WhatTheFish {
 // Single-pointer drag/release input. The existing match authority validates each shot.
 public sealed class GolfSwingButton:MonoBehaviour,IPointerDownHandler,IPointerUpHandler,IPointerExitHandler,IDragHandler,IInitializePotentialDragHandler {
  public Button button,startButton,aimButton;public Text label,startLabel,aimLabel;
  bool holding;int pointer;float grabOffset,power;RectTransform knob;Image rail;readonly Graphic[] hints=new Graphic[5];
  public bool Dragging=>holding;
  public float Power=>power;
  public RectTransform Knob=>knob;
  public const float ArrowLifetime=.5f;
  RectTransform Root=>(RectTransform)transform;
  float Top=>Root.rect.height*.385f;
  float Bottom=>-Root.rect.height*.34f;
  public void BuildSlider(){
   gameObject.name="Golf drag power slider";label.enabled=false;
   var hit=GetComponent<Image>();hit.color=Color.clear;hit.raycastTarget=true;button.transition=Selectable.Transition.None;
   rail=GolfHUDStyle.Art(transform,"Golf power rail","GolfUI/PowerSlider/Rail",new Vector2(210,350),Vector2.zero);
   rail.rectTransform.anchorMin=Vector2.zero;rail.rectTransform.anchorMax=Vector2.one;rail.rectTransform.offsetMin=rail.rectTransform.offsetMax=Vector2.zero;
   for(int i=0;i<hints.Length;i++){
    var r=GolfHUDStyle.Rect("Downward drag hint "+i,transform,new Vector2(.5f,.5f),Vector2.zero,new Vector2(22,13));
    hints[i]=r.gameObject.AddComponent<GolfPowerChevron>();hints[i].raycastTarget=false;
   }
   knob=GolfHUDStyle.Art(transform,"Golf ball power handle","GolfUI/PowerSlider/Knob",new Vector2(138,70),Vector2.zero).rectTransform;
   ResetSlider();
  }
  public static float HintAlpha(float time,int index){float age=Mathf.Repeat(time-index*.16f,1.3f);return age<ArrowLifetime?Mathf.Sin(Mathf.PI*age/ArrowLifetime):0;}
  void Update(){
   var view=PlayerView.Instance;var match=GolfMatchManager.Instance;
   button.interactable=view&&view.GolfAiming&&match&&!match.Starting&&match.CanStrike(view.target,match.Ball(view.AimedGolfOwner));
   if(holding&&(!button.interactable||!view.GolfCharging))Cancel();
   if(knob){knob.anchoredPosition=new Vector2(0,Mathf.Lerp(Top,Bottom,power));rail.color=button.interactable?Color.white:new Color(.82f,.86f,.88f,1);
    for(int i=0;i<hints.Length;i++){hints[i].rectTransform.anchoredPosition=new Vector2(0,Mathf.Lerp(Top-35,Bottom,i/(float)(hints.Length-1)));hints[i].color=new Color(1,1,1,holding?0:HintAlpha(Time.unscaledTime,i));}
   }
   if(aimButton){aimButton.gameObject.SetActive(match&&match.Context&&match.State.Running);aimButton.interactable=view&&match&&!match.Starting&&(view.GolfAimRequested||view.CanAimGolf);if(aimLabel)aimLabel.text=view&&view.GolfAimRequested?"Cancel":"Aim";}
   if(startButton){startButton.gameObject.SetActive(match&&match.Authority&&match.Context&&!match.State.Running);startButton.interactable=match&&match.CanStart;if(startLabel)startLabel.text="Start game";}
  }
  public void OnInitializePotentialDrag(PointerEventData e){e.useDragThreshold=false;}
  bool Local(PointerEventData e,out Vector2 at)=>RectTransformUtility.ScreenPointToLocalPointInRectangle(Root,e.position,e.pressEventCamera,out at);
  public void OnPointerDown(PointerEventData e){
   if(holding||e.button!=PointerEventData.InputButton.Left||!button.interactable||!PlayerView.Instance||!knob||!Local(e,out var at))return;
   // Grab the ball, with a generous touch radius; touching the rail cannot fire a shot.
   if(Mathf.Abs(at.y-knob.anchoredPosition.y)>45||Mathf.Abs(at.x)>65)return;
   pointer=e.pointerId;holding=PlayerView.Instance.BeginGolfSwing(pointer);if(holding){grabOffset=at.y-knob.anchoredPosition.y;power=0;}
  }
  public void OnDrag(PointerEventData e){
   if(!holding||e.pointerId!=pointer||!Local(e,out var at))return;
   power=Mathf.InverseLerp(Top,Bottom,at.y-grabOffset);PlayerView.Instance?.SetGolfPower(pointer,power);
   knob.anchoredPosition=new Vector2(0,Mathf.Lerp(Top,Bottom,power));
  }
  public void OnPointerUp(PointerEventData e){if(!holding||e.pointerId!=pointer)return;OnDrag(e);PlayerView.Instance?.EndGolfSwing(pointer);holding=false;ResetSlider();}
  // Pointer capture continues outside the rail, as with a native slider.
  public void OnPointerExit(PointerEventData e){}
  void ResetSlider(){power=0;if(knob)knob.anchoredPosition=new Vector2(0,Top);}
  void Cancel(){if(holding)PlayerView.Instance?.CancelGolfSwing(pointer);holding=false;ResetSlider();}
  void OnDisable(){Cancel();}
  void OnApplicationFocus(bool focused){if(!focused)Cancel();}
  void OnApplicationPause(bool paused){if(paused)Cancel();}
 }
 public sealed class GolfPowerChevron:Graphic {
  protected override void OnPopulateMesh(VertexHelper mesh){mesh.Clear();var r=rectTransform.rect;
   foreach(var v in Points)mesh.AddVert(new Vector3(r.xMin+v.x*r.width,r.yMin+v.y*r.height),color,Vector2.zero);
   mesh.AddTriangle(0,1,2);mesh.AddTriangle(0,2,5);mesh.AddTriangle(2,3,4);mesh.AddTriangle(2,4,5);
  }
  static readonly Vector2[] Points={new(0,.8f),new(.18f,1),new(.5f,.42f),new(.82f,1),new(1,.8f),new(.5f,0)};
 }
}
