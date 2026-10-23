#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace WhatTheFish {
 public sealed partial class ControlLayoutReview {
  IEnumerator ReviewButtonStates(){
   var board=new GameObject("Control art state review",typeof(RectTransform),typeof(Canvas),typeof(GraphicRaycaster));
   var canvas=board.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=32000;
   var shade=board.AddComponent<Image>();shade.color=new Color(.025f,.09f,.16f,.96f);shade.raycastTarget=false;
   var samples=new List<ControlDesigns.View>();int index=0;var sport=app.SelectedSport;
   Text Note(string value,Vector2 position,int size,Vector2 dimensions){
    var rect=GolfHUDStyle.Rect(value,board.transform,Vector2.one*.5f,position,dimensions);
    var text=rect.gameObject.AddComponent<Text>();text.font=CoveUI.DisplayFont;text.fontSize=size;text.text=value;text.color=Color.white;text.alignment=TextAnchor.MiddleCenter;text.raycastTarget=false;return text;
   }
   Note(sport+" · shared button states",new Vector2(0,Screen.height*.5f-48),32,new Vector2(1400,50));
   ControlDesigns.View Sample(string id,string caption=null,string icon=null,string frame=null,bool disabled=false,string note=null){
    int i=index++;var position=new Vector2((i%6-2.5f)*248,Screen.height*.5f-215-i/6*245);
    var root=GolfHUDStyle.Rect("Review "+id,board.transform,Vector2.one*.5f,position,Vector2.one);
    var v=ControlDesigns.Build(root,sport,id);root.localScale=Vector3.one*Mathf.Min(1.7f,200/root.rect.width,178/root.rect.height);
    if(!string.IsNullOrEmpty(icon)){if(v.IconRoot)ControlDesigns.SkillIcon(v.IconRoot,(FishingAbility)System.Enum.Parse(typeof(FishingAbility),icon));else v.Art.sprite=ControlArt.Get(icon);}
    if(!string.IsNullOrEmpty(frame)&&v.Face is Image face)face.sprite=ControlArt.Get(frame);
    if(caption!=null&&v.Label)ControlDesigns.Caption(v.Label,caption,caption=="RECEIVING"?.78f:1);
    if(id!="move"){var button=ControlDesigns.BindButton(v);button.interactable=!disabled;ControlDesigns.Availability(button,!disabled);}
    if(disabled&&v.Ring)v.Ring.Tick(.42f,CoveUI.Gold);
    Note(note??caption??id,new Vector2(position.x,position.y-142),18,new Vector2(230,34));samples.Add(v);return v;
   }
   if(sport==SportId.Basketball){
    Sample("shoot","SHOOT","Basketball");Sample("shoot","GUARD","Guard","RoundCyan");Sample("pass","PASS","BasketPass");Sample("pass","STEAL","Steal");Sample("jump","JUMP");Sample("move",note:"Translucent movement");
    foreach(string prop in new[]{"Turbo","LockOn","Portal"})Sample("ability",prop=="LockOn"?"LOCK-ON":prop.ToUpperInvariant(),prop);
    Sample("ability","RECEIVING","Portal");Sample("shoot","100%","Basketball");Sample("pass","STEAL","Steal",disabled:true,note:"Unavailable action");
   }else if(sport==SportId.Football){
    Sample("shoot","SHOOT","Football");Sample("shoot","PRESSURE","Pressure","TileCyan");Sample("dash","DASH");Sample("pass","PASS");Sample("tackle","TACKLE");Sample("move",note:"Translucent movement");
    foreach(string prop in new[]{"Wall","Turbo","LockOn","Portal"})Sample("ability",prop=="LockOn"?"LOCK-ON":prop.ToUpperInvariant(),prop);
    Sample("tackle","1.5s",disabled:true,note:"Tackle cooldown");Sample("dash","REST",disabled:true,note:"Recovering stamina");
   }else if(sport==SportId.Fishing){
    foreach(var ability in FishingAbilityRules.Roster)Sample("ability","USE",ability.ToString(),note:FishingAbilityRules.Name(ability));
    Sample("ability","12s","Sonar",disabled:true,note:"Skill cooldown");Sample("ability","ACTIVE","BubbleGuard");Sample("ability","CHOOSE","Sonar");Sample("jump","JUMP");Sample("cancel","CANCEL");Sample("move",note:"Translucent movement");
   }else Sample("move",note:"Translucent movement");
   yield return null;Canvas.ForceUpdateCanvases();Capture("shared-state-gallery");
   foreach(var v in samples){
    Check(v.Root.GetComponentsInChildren<Image>().Where(i=>i.name!="Shared design probe").All(i=>i.sprite),"STATE_SPRITES_"+sport+"_"+v.Root.name);
    if(v.Label&&v.Label.isActiveAndEnabled){v.Label.SetAllDirty();Canvas.ForceUpdateCanvases();Check(v.Label.cachedTextGenerator.vertexCount>0,"STATE_CAPTION_VISIBLE_"+sport+"_"+v.Label.text);}
   }
   var press=samples.FirstOrDefault(v=>v.PressVisual);
   if(press!=null){
    var feedback=press.Root.GetComponent<BasketballButtonFeedback>();var hit=press.Root;var before=hit.position;var size=hit.rect.size;var visual=press.PressVisual;var scale=visual.localScale;
    var pointer=new PointerEventData(EventSystem.current){pointerId=609,button=PointerEventData.InputButton.Left,position=ScreenPoint(hit)};
    Check(Top(pointer.position)?.GetComponentInParent<Button>()==hit.GetComponent<Button>(),"GLOSSY_FIXED_TARGET_RAYCAST_"+sport);
    feedback.OnPointerDown(pointer);yield return new WaitForSecondsRealtime(.2f);
    Check(hit.position==before&&hit.rect.size==size,"PRESS_KEEPS_TOUCH_TARGET_"+sport);
    Check(MenuPreferences.ReducedMotion||visual.localScale.x<scale.x,"PRESS_DEPRESSES_SHARED_ART_"+sport);
    feedback.OnPointerUp(new PointerEventData(EventSystem.current){pointerId=610});yield return null;
    Check(MenuPreferences.ReducedMotion||visual.localScale.x<scale.x,"FOREIGN_RELEASE_IGNORED_"+sport);
    feedback.OnPointerUp(pointer);yield return new WaitForSecondsRealtime(.2f);
    Check((visual.localScale-scale).sqrMagnitude<.0001f,"PRESS_RELEASE_RESTORES_ART_"+sport);
   }
   Destroy(board);yield return null;
  }
 }
}
#endif
