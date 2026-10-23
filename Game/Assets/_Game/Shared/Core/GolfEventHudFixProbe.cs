#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
namespace WhatTheFish {
 public sealed partial class GolfPolishProbe {
  IEnumerator EventHudFixPresentation(){
   yield return StartMatch();yield return new WaitForSeconds(.3f);
   var hud=GolfEventHUD.Instance;var atlas=Resources.Load<Texture2D>("GolfUI/Events/BadgeSheet");var warningAtlas=Resources.Load<Texture2D>("GolfUI/Events/WarningSheet");
   var art=hud.Card.GetComponentsInChildren<GolfEventBadgeGraphic>(true).First(a=>a.name=="Golf weather badge");var clock=art.GetComponentsInChildren<Text>(true).First(t=>t.name=="Event seconds");int materialChanges=0;UnityEngine.Events.UnityAction changed=()=>materialChanges++;art.RegisterDirtyMaterialCallback(changed);
   var state=match.Events.Snapshot;state.kind=GolfEventKind.StickyGreen;state.start=match.Now+10;state.end=state.start+GolfEventSchedule.Duration(state.kind);state.nextCycle=state.start+GolfEventSchedule.Cycle;state.rushAt=0;match.Events.Receive(state);
   yield return null;yield return new WaitForEndOfFrame();Check(art.Sheet==warningAtlas&&art.Badge==5&&materialChanges>0,"WARNING_ATLAS_DIRTIES_CANVAS_MATERIAL");
   var font=clock.font;int size=clock.fontSize;var style=clock.fontStyle;var outline=clock.GetComponent<Outline>();var color=outline.effectColor;var offset=outline.effectDistance;int before=materialChanges;
   for(int second=10;second>=1;second--){
    string expected=second.ToString("00")+"s";float until=Time.unscaledTime+1.5f;while(clock.text!=expected&&Time.unscaledTime<until)yield return null;
    Check(clock.text==expected&&clock.enabled&&clock.gameObject.activeInHierarchy,"LIVE_COUNTER_VISIBLE_"+second);
    Check(clock.font==font&&clock.fontSize==size&&clock.fontStyle==style&&outline.effectColor==color&&outline.effectDistance==offset&&clock.GetComponent<GolfWarningGradient>().enabled,"IDENTICAL_DIGIT_STYLE_"+second);
    Check(art.Badge==5&&art.Sheet==warningAtlas,"SAME_WARNING_ART_"+second);
    yield return new WaitForEndOfFrame();CaptureUI("warning-"+second.ToString("00"),null,0,0,0,false);
    if(second>1){until=Time.unscaledTime+1.5f;while(clock.text==expected&&Time.unscaledTime<until)yield return null;}
   }
   float end=Time.unscaledTime+2;while(art.Badge==5&&Time.unscaledTime<end)yield return null;yield return new WaitForEndOfFrame();
   Check(art.Badge==2&&art.Sheet==atlas&&materialChanges>before,"WARNING_END_REBINDS_ACTIVE_EVENT_ATLAS");Check(!clock.GetComponent<GolfWarningGradient>().enabled&&clock.fontStyle==FontStyle.Normal&&clock.enabled,"ACTIVE_EVENT_SECONDS_STYLE_RESTORED");
   CaptureUI("transition-initial",null,0,0,0,false);art.SetAllDirty();Canvas.ForceUpdateCanvases();CaptureUI("transition-forced-refresh",null,0,0,0,false);
   foreach(var kind in GolfEventSchedule.RandomKinds.Where(k=>!GolfHazardRules.IsHazard(k))){
    state=match.Events.Snapshot;state.kind=kind;state.start=match.Now+.4;state.end=state.start+GolfEventSchedule.Duration(kind);state.nextCycle=state.start+GolfEventSchedule.Cycle;match.Events.Receive(state);yield return new WaitForSeconds(.15f);yield return new WaitForEndOfFrame();Check(art.Badge==5&&art.Sheet==warningAtlas,"REPEATED_WARNING_"+kind);
    yield return new WaitForSeconds(.4f);yield return new WaitForEndOfFrame();Check(art.Badge==(int)kind-1&&art.Sheet==atlas,"CORRECT_EVENT_TYPE_"+kind);CaptureUI("active-"+kind,null,0,0,0,false);
   }
   match.ReviewEventHotkey(KeyCode.F6);yield return null;yield return new WaitForEndOfFrame();Check(hud.Card.GetComponentsInChildren<GolfEventBadgeGraphic>(true).Where(a=>a.gameObject.activeInHierarchy).All(a=>a.Sheet==atlas)&&hud.Card.GetComponentsInChildren<GolfEventBadgeGraphic>(true).Count(a=>a.gameObject.activeInHierarchy)==2,"RUSH_AND_WEATHER_USE_EVENT_ATLAS");
   CaptureUI("active-rush-and-weather",null,0,0,0,false);art.UnregisterDirtyMaterialCallback(changed);match.ClearMatch();
  }
 }
}
#endif
