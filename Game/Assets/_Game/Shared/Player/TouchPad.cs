using System;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
namespace SportsPrototype {
 public sealed class TouchPad:MonoBehaviour,IPointerDownHandler,IDragHandler,IPointerUpHandler {
  public bool look;public Vector2 value;public RectTransform knob;Vector2 origin;int pointer=int.MinValue;bool trace;
  void Awake(){trace=Debug.isDebugBuild&&Environment.GetCommandLineArgs().Contains("-inputTrace");}
  public void OnPointerDown(PointerEventData e){if(pointer!=int.MinValue)return;pointer=e.pointerId;origin=e.position;if(trace)Debug.Log("PAD_DOWN look="+look+" id="+pointer);OnDrag(e);}
  public void OnDrag(PointerEventData e){if(e.pointerId!=pointer)return;if(trace)Debug.Log("PAD_DRAG look="+look+" delta="+e.delta);if(look){PlayerView.LookDelta+=e.delta;return;}value=Vector2.ClampMagnitude((e.position-origin)/(70*transform.lossyScale.x),1);if(knob)knob.anchoredPosition=value*48;}
  public void OnPointerUp(PointerEventData e){if(e.pointerId!=pointer)return;pointer=int.MinValue;value=Vector2.zero;if(knob)knob.anchoredPosition=Vector2.zero;}
  void OnDisable(){pointer=int.MinValue;value=Vector2.zero;}
 }
}
