using UnityEngine;
using UnityEngine.UI;

namespace WhatTheFish {
 public sealed class GolfCartButton:MonoBehaviour {
  public bool summon;public Button button;public Text label;
  CanvasGroup visibility;float nextPress;
  void Awake(){visibility=gameObject.AddComponent<CanvasGroup>();}
  void Start(){button.onClick.AddListener(Press);}
  public void Press(){
   if(Time.unscaledTime<nextPress||!button.interactable)return;
   nextPress=Time.unscaledTime+.35f;
   if(summon)PlayerView.Instance.RequestCartToggle();else PlayerView.Instance.RequestCartUse();
  }
  void Update(){
   var view=PlayerView.Instance;var actor=view?view.target:null;var driving=GolfCartWorld.Driving(actor);
   bool available=view&&view.active&&!view.GolfAimRequested&&actor&&GolfCartWorld.Allowed&&!actor.inTransit;
   bool visible=available&&(summon||driving||GolfCartWorld.Nearest(actor));
   visibility.alpha=visible?1:0;visibility.blocksRaycasts=visible;button.interactable=visible&&Time.unscaledTime>=nextPress;
   label.text=summon?(GolfCartWorld.HasCart(actor)?"收回":"召唤"):(driving?"离开":"驾驶");
   if(!Application.isMobilePlatform)label.text+=summon?" [R]":" [E]";
  }
 }
}
