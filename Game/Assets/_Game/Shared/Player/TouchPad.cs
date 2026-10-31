using System;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
namespace WhatTheFish {
 public sealed class TouchPad:MonoBehaviour,IPointerDownHandler,IDragHandler,IPointerUpHandler,ICancelHandler {
  public bool look;public Vector2 value;public RectTransform knob;public float designRadius;Vector2 origin,lastPosition;int pointer=int.MinValue;bool trace;
  void Awake(){trace=Debug.isDebugBuild&&Environment.GetCommandLineArgs().Contains("-inputTrace");}
  public void OnPointerDown(PointerEventData e){if(e.button!=PointerEventData.InputButton.Left||pointer!=int.MinValue)return;pointer=e.pointerId;origin=lastPosition=e.position;if(trace)Debug.Log("PAD_DOWN look="+look+" id="+pointer);if(!look)OnDrag(e);}
  public void OnDrag(PointerEventData e){if(e.pointerId!=pointer)return;if(trace)Debug.Log("PAD_DRAG look="+look+" delta="+e.delta);var delta=e.position-lastPosition;lastPosition=e.position;if(look){if(!PlayerView.Instance||!PlayerView.Instance.InputBlocked)PlayerView.LookDelta+=delta;return;}value=Vector2.ClampMagnitude((e.position-origin)/((designRadius>0?designRadius:70)*transform.lossyScale.x),1);if(knob)knob.anchoredPosition=value*(designRadius>0?25:48);}
  public void OnPointerUp(PointerEventData e){if(e.pointerId!=pointer)return;pointer=int.MinValue;value=Vector2.zero;if(knob)knob.anchoredPosition=Vector2.zero;}
  public void ResetInput(){pointer=int.MinValue;value=Vector2.zero;if(knob)knob.anchoredPosition=Vector2.zero;}
  public void OnCancel(BaseEventData e)=>ResetInput();
  void OnDisable()=>ResetInput();
  void OnApplicationFocus(bool focused){if(!focused)ResetInput();}
  void OnApplicationPause(bool paused){if(paused)ResetInput();}
 }
}
