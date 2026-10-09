using UnityEngine;
using UnityEngine.UI;

namespace WhatTheFish {
 [DefaultExecutionOrder(-40)]
 public sealed class FootballActionSlot:MonoBehaviour {
  KickButton kick;FootballEffortButton pressure;TackleButton tackle;CanvasGroup tackleGroup;
  public bool Attacking {get;private set;}
  public static void Attach(Transform parent){
   var kick=parent.GetComponentInChildren<KickButton>(true);var slot=kick.gameObject.AddComponent<FootballActionSlot>();slot.kick=kick;
   slot.pressure=kick.gameObject.AddComponent<FootballEffortButton>();slot.pressure.pressure=true;slot.pressure.button=kick.button;slot.pressure.label=kick.label;slot.pressure.enabled=false;
   slot.tackle=parent.GetComponentInChildren<TackleButton>(true);slot.tackleGroup=slot.tackle.gameObject.AddComponent<CanvasGroup>();
  }
  void Update(){
   var view=PlayerView.Instance;Attacking=view&&SportsPossession.Attacking(view.target,SportId.Football);
   // OnDisable cancels the previous pointer before the replacement can accept
   // a new press. A held touch never fires a different action after a turnover.
   bool changed=kick.enabled!=Attacking||pressure.enabled==Attacking;
   kick.enabled=Attacking;pressure.enabled=!Attacking;tackle.enabled=!Attacking;
   if(changed){if(Attacking)kick.Refresh();else {pressure.Refresh();tackle.Refresh();}}
   tackleGroup.alpha=Attacking?0:1;tackleGroup.blocksRaycasts=tackleGroup.interactable=!Attacking;
  }
 }
}
