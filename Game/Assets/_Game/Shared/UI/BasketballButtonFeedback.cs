using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace WhatTheFish {
 // Depress only the artwork. The original touch rectangle never moves.
 public sealed class BasketballButtonFeedback:MonoBehaviour,IPointerDownHandler,IPointerUpHandler,IPointerExitHandler,ICancelHandler {
  // Fishing changes its action caption between Cast and Right at runtime.
  // Keep those layout-owned captions out of the cached artwork press pose.
  public bool FixedFishingLabels;
  BasketballPlate plate;FishingStickerGraphic fishing;Button button;RectTransform presentation;RectTransform[] children;Vector2[] positions;Vector3[] scales;int pointer=int.MinValue;float pressure;
  void Start(){plate=GetComponent<BasketballPlate>();fishing=GetComponent<FishingStickerGraphic>();button=GetComponent<Button>();var source=GetComponentInParent<ControlDesignSource>();presentation=source?source.View.PressVisual:null;int n=presentation?1:transform.childCount;children=new RectTransform[n];positions=new Vector2[n];scales=new Vector3[n];for(int i=0;i<n;i++){children[i]=presentation?presentation:transform.GetChild(i) as RectTransform;positions[i]=children[i].anchoredPosition;scales[i]=children[i].localScale;}}
  public static bool UsesPressSound(GameObject go)=>go.GetComponent<BasketballShootButton>()||go.GetComponent<BasketballDefenseButton>()||go.GetComponent<BasketballStealButton>()||go.GetComponent<JumpButton>()||go.GetComponent<KickButton>()||go.GetComponent<FootballEffortButton>()||go.GetComponent<TackleButton>();
  public void OnPointerDown(PointerEventData e){if(e.button!=PointerEventData.InputButton.Left||pointer!=int.MinValue||!GetComponent<Button>().IsInteractable())return;pointer=e.pointerId;if(UsesPressSound(gameObject)){if(!GolfFishingAudio.PlayMenu()){if(FootballAudio.Context)FootballAudio.PlayMenu();else BasketballAudio.PlayMenu();}}}
  public void OnPointerUp(PointerEventData e){if(e.pointerId==pointer)pointer=int.MinValue;}
  public void OnPointerExit(PointerEventData e){if(e.pointerId==pointer)pointer=int.MinValue;}
  public void OnCancel(BaseEventData e)=>Reset();
  void LateUpdate(){if(!plate&&!fishing&&!presentation)return;float next=Mathf.MoveTowards(pressure,pointer!=int.MinValue&&button.IsInteractable()&&!MenuPreferences.ReducedMotion?1:0,Time.unscaledDeltaTime*12);if(Mathf.Approximately(next,pressure))return;pressure=next;Apply();}
  void Apply(){if(plate){plate.pressure=pressure;plate.Refresh();}if(fishing){fishing.pressure=pressure;fishing.SetVerticesDirty();}for(int i=0;children!=null&&i<children.Length;i++){if(FixedFishingLabels&&children[i].GetComponent<Text>())continue;children[i].anchoredPosition=positions[i]+Vector2.down*(3*pressure);children[i].localScale=scales[i]*(1-.04f*pressure);}}
  void Reset(){pointer=int.MinValue;pressure=0;if(plate||fishing||presentation)Apply();}
  void OnDisable()=>Reset();void OnApplicationFocus(bool focus){if(!focus)Reset();}void OnApplicationPause(bool paused){if(paused)Reset();}
 }
}
