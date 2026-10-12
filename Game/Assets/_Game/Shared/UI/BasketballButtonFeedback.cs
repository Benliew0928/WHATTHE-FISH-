using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace WhatTheFish {
 // Depress only the artwork. The original touch rectangle never moves.
 public sealed class BasketballButtonFeedback:MonoBehaviour,IPointerDownHandler,IPointerUpHandler,IPointerExitHandler,ICancelHandler {
  BasketballPlate plate;Button button;RectTransform[] children;Vector2[] positions;Vector3[] scales;int pointer=int.MinValue;float pressure;
  void Start(){plate=GetComponent<BasketballPlate>();button=GetComponent<Button>();int n=transform.childCount;children=new RectTransform[n];positions=new Vector2[n];scales=new Vector3[n];for(int i=0;i<n;i++){children[i]=transform.GetChild(i) as RectTransform;positions[i]=children[i].anchoredPosition;scales[i]=children[i].localScale;}}
  public static bool UsesPressSound(GameObject go)=>go.GetComponent<BasketballShootButton>()||go.GetComponent<BasketballDefenseButton>()||go.GetComponent<BasketballStealButton>()||go.GetComponent<JumpButton>();
  public void OnPointerDown(PointerEventData e){if(e.button!=PointerEventData.InputButton.Left||pointer!=int.MinValue||!GetComponent<Button>().IsInteractable())return;pointer=e.pointerId;if(UsesPressSound(gameObject))BasketballAudio.PlayMenu();}
  public void OnPointerUp(PointerEventData e){if(e.pointerId==pointer)pointer=int.MinValue;}
  public void OnPointerExit(PointerEventData e){if(e.pointerId==pointer)pointer=int.MinValue;}
  public void OnCancel(BaseEventData e)=>Reset();
  void LateUpdate(){if(!plate)return;float next=Mathf.MoveTowards(pressure,pointer!=int.MinValue&&button.IsInteractable()&&!MenuPreferences.ReducedMotion?1:0,Time.unscaledDeltaTime*12);if(Mathf.Approximately(next,pressure))return;pressure=next;Apply();}
  void Apply(){plate.pressure=pressure;plate.Refresh();for(int i=0;i<children.Length;i++){children[i].anchoredPosition=positions[i]+Vector2.down*(3*pressure);children[i].localScale=scales[i]*(1-.04f*pressure);}}
  void Reset(){pointer=int.MinValue;pressure=0;if(plate)Apply();}
  void OnDisable()=>Reset();void OnApplicationFocus(bool focus){if(!focus)Reset();}void OnApplicationPause(bool paused){if(paused)Reset();}
 }
}
