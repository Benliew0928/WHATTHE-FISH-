using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace WhatTheFish {
 // Wrappers keep animation, responsive layout, gesture coordinates and hitboxes together.
 // The original HUD remains responsible for contextual visibility and availability.
 [DefaultExecutionOrder(1000)]
 public sealed class ControlLayoutEditor:MonoBehaviour {
  public sealed class Control {
   public string id,label;public RectTransform target,wrapper,proxy;public Sprite icon;
   public ControlLayoutVisuals.Copy Artwork {get;internal set;}
   internal ControlLayoutOutline outline;internal GameObject handle;internal Rect bounds;internal CanvasGroup visibility;
  }
  public static ControlLayoutEditor Active {get;private set;}
  public SportId Sport {get;private set;}
  public bool Editing=>Active==this&&editor&&editor.gameObject.activeSelf;
  public IReadOnlyList<Control> Controls=>controls;
  public int Selected {get;private set;}
  public RectTransform Frame=>frame;
  public RectTransform Dock=>dock;
  public bool Preview {get;private set;}
  readonly List<Control> controls=new();
  readonly Vector3[] corners=new Vector3[4];
  Dictionary<string,ControlLayouts.Entry> saved,draft;
  RectTransform frame,editor,dock,choices;Text selection,percentage,dockToggle;Slider sizeSlider;
  Func<GameObject> menu;Action closed;CanvasGroup hiddenMenu;
  float oldAlpha;bool oldBlocks,oldInteract;string profile;
  Color accent,paper,ink;bool dark;
  public static ControlLayoutEditor Create(RectTransform frame,SportId sport,bool preview=false){
   var value=frame.gameObject.AddComponent<ControlLayoutEditor>();value.frame=frame;value.Sport=sport;value.Preview=preview;
   value.profile=PlayerIdentity.ProfileStorageKey;value.saved=ControlLayouts.Load(value.profile,sport);return value;
  }
  public void Register(string id,string label,RectTransform target,Sprite icon=null,params RectTransform[] companions){
   if(!target||controls.Any(c=>c.id==id))return;
   var parent=(RectTransform)target.parent;
   var wrap=new GameObject("Personal layout "+id,typeof(RectTransform)).GetComponent<RectTransform>();
   wrap.SetParent(parent,false);wrap.SetSiblingIndex(target.GetSiblingIndex());wrap.pivot=parent.pivot;CoveUI.Stretch(wrap);
   // Both parents now describe the same rectangle. Keep the child's anchors and offsets.
   target.SetParent(wrap,false);
   foreach(var related in companions)if(related)related.SetParent(wrap,false);
   controls.Add(new Control{id=id,label=label,target=target,wrapper=wrap,icon=icon,visibility=wrap.gameObject.AddComponent<CanvasGroup>()});
  }
  public void Open(Func<GameObject> sourceMenu=null,Action onClosed=null){
   if(Editing||controls.Count==0)return;
   if(Active)Active.Cancel();
   profile=PlayerIdentity.ProfileStorageKey;saved=ControlLayouts.Load(profile,Sport);
   draft=saved.ToDictionary(p=>p.Key,p=>p.Value.Copy());menu=sourceMenu;closed=onClosed;
   Active=this;BuildEditor();editor.gameObject.SetActive(true);HideMenu();Select(0);Apply();
   EventSystem.current?.SetSelectedGameObject(null);
  }
  void HideMenu(){
   var source=menu?.Invoke();if(!source)return;
   var group=source.GetComponent<CanvasGroup>()??source.AddComponent<CanvasGroup>();
   if(group==hiddenMenu)return;RestoreMenu();hiddenMenu=group;oldAlpha=group.alpha;oldBlocks=group.blocksRaycasts;oldInteract=group.interactable;
   group.alpha=0;group.blocksRaycasts=group.interactable=false;
  }
  void RestoreMenu(){if(hiddenMenu){hiddenMenu.alpha=oldAlpha;hiddenMenu.blocksRaycasts=oldBlocks;hiddenMenu.interactable=oldInteract;}hiddenMenu=null;}
  public void Save(){
   if(!Editing||profile!=PlayerIdentity.ProfileStorageKey){Cancel();return;}
   ControlLayouts.Save(profile,Sport,draft.Values);saved=draft.ToDictionary(p=>p.Key,p=>p.Value.Copy());Finish(true);
  }
  public void Cancel()=>Finish(true);
  public void Dismiss()=>Finish(false);
  void Finish(bool notify){
   if(!Editing)return;Active=null;RestoreMenu();editor.gameObject.SetActive(false);draft=null;
   foreach(var c in controls){c.visibility.alpha=1;c.visibility.blocksRaycasts=c.visibility.interactable=true;}Apply();
   EventSystem.current?.SetSelectedGameObject(null);
   var callback=closed;closed=null;menu=null;if(notify)callback?.Invoke();
   if(Preview)Destroy(gameObject);
  }
  public void ResetAll(){if(!Editing)return;draft.Clear();Apply();Select(Selected);}
  public void ResetSelected(){if(!Editing)return;draft.Remove(controls[Selected].id);Apply();Select(Selected);}
  public void Select(int index){
   if(controls.Count==0)return;Selected=(index+controls.Count)%controls.Count;
   if(!Editing)return;
   selection.text=controls[Selected].label;
   sizeSlider.SetValueWithoutNotify(Size(Selected));
   if(choices)choices.gameObject.SetActive(false);
   foreach(var c in controls){
    bool chosen=c==controls[Selected];c.outline.color=chosen?accent:new Color(1,1,1,.45f);c.outline.selected=chosen;c.outline.SetVerticesDirty();c.handle.SetActive(chosen);
    // Fishing's stick and countersteer replace each other during play. Show
    // one at a time here too, instead of piling two controls in the same place.
    bool reel=controls[Selected].id=="left";
    c.proxy.gameObject.SetActive(Sport!=SportId.Fishing||c.id!="left"&&c.id!="move"||(c.id=="left")==reel);
   }
   if(controls[Selected].bounds.Overlaps(Measure(dock))){dockBottom=controls[Selected].bounds.center.y>frame.rect.center.y;LayoutDock();}
   controls[Selected].proxy.SetAsLastSibling();dock.SetAsLastSibling();UpdateHint();
  }
  public float Size(int index){var source=Editing?draft:saved;return source.TryGetValue(controls[index].id,out var e)?e.size:1;}
  public Rect Bounds(int index)=>controls[index].bounds;
  public Vector2 NormalizedCenter(int index){
   var center=controls[index].bounds.center;return new Vector2((center.x-frame.rect.xMin)/frame.rect.width,(center.y-frame.rect.yMin)/frame.rect.height);
  }
  public void MoveSelected(Vector2 normalized){Change(normalized,Size(Selected));}
  public void ResizeSelected(float value){Change(NormalizedCenter(Selected),value);}
  void Change(Vector2 position,float scale){
   if(!Editing||!float.IsFinite(position.x)||!float.IsFinite(position.y)||!float.IsFinite(scale))return;
   var c=controls[Selected];draft[c.id]=new ControlLayouts.Entry{id=c.id,x=Mathf.Clamp01(position.x),y=Mathf.Clamp01(position.y),size=Mathf.Clamp(scale,ControlLayouts.MinSize,ControlLayouts.MaxSize)};
   Apply();sizeSlider.SetValueWithoutNotify(Size(Selected));UpdateHint();
  }
  void UpdateHint(){if(percentage)percentage.text=Mathf.RoundToInt(Size(Selected)*100)+"%";}
  Rect Measure(RectTransform target,RectTransform without=null){
   target.GetWorldCorners(corners);Vector2 min=Vector2.positiveInfinity,max=Vector2.negativeInfinity;
   foreach(var corner in corners){
    // Remove the previous personal transform mathematically. Resetting and
    // reapplying it every frame would unnecessarily dirty the entire HUD canvas.
    var world=without?without.parent.TransformPoint(without.InverseTransformPoint(corner)):corner;
    Vector2 p=frame.InverseTransformPoint(world);min=Vector2.Min(min,p);max=Vector2.Max(max,p);
   }
   return Rect.MinMaxRect(min.x,min.y,max.x,max.y);
  }
  public void Apply(){
   if(!frame||frame.rect.width<=0||frame.rect.height<=0||saved==null)return;
   var source=Editing?draft:saved;
   foreach(var c in controls){
    if(!c.target||!c.wrapper)continue;
    Rect original=Measure(c.target,c.wrapper);float factor=1;Vector2 offset=Vector2.zero;
    if(source.TryGetValue(c.id,out var e)){
     factor=Mathf.Clamp(e.size,ControlLayouts.MinSize,ControlLayouts.MaxSize);
     factor=Mathf.Min(factor,(frame.rect.width-16)/Mathf.Max(1,original.width),(frame.rect.height-16)/Mathf.Max(1,original.height));
     Vector2 half=original.size*factor*.5f;
     var center=new Vector2(Mathf.Lerp(frame.rect.xMin,frame.rect.xMax,e.x),Mathf.Lerp(frame.rect.yMin,frame.rect.yMax,e.y));
     center.x=Mathf.Clamp(center.x,frame.rect.xMin+half.x+8,frame.rect.xMax-half.x-8);
     center.y=Mathf.Clamp(center.y,frame.rect.yMin+half.y+8,frame.rect.yMax-half.y-8);
     // Keep the settings corner accessible, even after a control is enlarged.
     var cornerSize=new Vector2(frame.rect.height*.16f,frame.rect.height*.16f);
     bool left=Sport==SportId.Golf;
     var gear=new Rect(left?frame.rect.xMin:frame.rect.xMax-cornerSize.x,frame.rect.yMax-cornerSize.y,cornerSize.x,cornerSize.y);
     if(new Rect(center-half,half*2).Overlaps(gear))center.y=Mathf.Min(center.y,gear.yMin-half.y-8);
     Vector3 localCenter=c.wrapper.parent.InverseTransformPoint(frame.TransformPoint(original.center));
     Vector3 delta=c.wrapper.parent.InverseTransformVector(frame.TransformVector(center-original.center));
     offset=(Vector2)(localCenter*(1-factor)+delta);
    }
    if(c.wrapper.localScale!=Vector3.one*factor)c.wrapper.localScale=Vector3.one*factor;
    if((c.wrapper.anchoredPosition-offset).sqrMagnitude>.0001f)c.wrapper.anchoredPosition=offset;
    c.bounds=Measure(c.target);
    if(Editing&&c.proxy){
     c.proxy.anchoredPosition=c.bounds.center;c.proxy.sizeDelta=c.bounds.size;
     c.Artwork.Fit(c.target,c.bounds.size);
    }
   }
  }
  void LateUpdate(){
   if(profile!=PlayerIdentity.ProfileStorageKey){Dismiss();profile=PlayerIdentity.ProfileStorageKey;saved=ControlLayouts.Load(profile,Sport);}
   if(Editing){
    if(!Preview&&(PostMatchFlow.Instance&&PostMatchFlow.Instance.Visible||TeleportSequence.Busy||RoomTeleport.Busy||BasketballMVP.Presenting||FootballMVP.Presenting||FishingCeremony.Presenting)){Dismiss();return;}
    HideMenu();LayoutDock();dock.SetAsLastSibling();
   }
   Apply();
  }
  void OnDisable()=>Dismiss();
  void OnApplicationFocus(bool focused){if(!focused&&Editing)foreach(var drag in editor.GetComponentsInChildren<ControlLayoutDrag>(true))drag.Release();}
  void OnApplicationPause(bool paused){if(paused)OnApplicationFocus(false);}
  void BuildEditor(){
   if(editor){Destroy(editor.gameObject);foreach(var c in controls)c.proxy=null;}
   dark=Preview;accent=Sport switch{SportId.Basketball=>CoveUI.Hex("FFB17D"),SportId.Football=>CoveUI.Hex("BBEF6E"),SportId.Golf=>CoveUI.Hex(dark?"D1BAFF":"E8C36A"),_=>CoveUI.Hex("53DED7")};
   paper=dark?CoveUI.Hex("193D52"):Sport==SportId.Golf?CoveUI.Hex("FFF3D7"):CoveUI.Cream;ink=dark?CoveUI.Hex("F3F8FA"):CoveUI.Ink;
   editor=new GameObject(Sport+" layout editor",typeof(RectTransform),typeof(Canvas),typeof(GraphicRaycaster)).GetComponent<RectTransform>();
   editor.SetParent(frame,false);editor.pivot=frame.pivot;CoveUI.Stretch(editor);var canvas=editor.GetComponent<Canvas>();canvas.overrideSorting=true;canvas.sortingOrder=300;
   var veil=editor.gameObject.AddComponent<Image>();veil.color=new Color(.08f,.13f,.16f,.58f);veil.raycastTarget=true;
   foreach(var c in controls){
    var hit=new GameObject("Edit "+c.id,typeof(RectTransform),typeof(Image)).GetComponent<Image>();hit.transform.SetParent(editor,false);hit.color=Color.clear;hit.raycastTarget=true;
    c.proxy=hit.rectTransform;c.proxy.anchorMin=c.proxy.anchorMax=frame.pivot;c.proxy.pivot=Vector2.one*.5f;
    c.Artwork=ControlLayoutVisuals.Clone(c.target,c.proxy);ControlLayoutVisuals.CompleteContext(c.Artwork,Sport,c.id);
    c.visibility.alpha=0;c.visibility.blocksRaycasts=c.visibility.interactable=false;
    var outline=new GameObject("Selection outline",typeof(RectTransform),typeof(ControlLayoutOutline)).GetComponent<ControlLayoutOutline>();outline.transform.SetParent(c.proxy,false);CoveUI.Stretch(outline.rectTransform);outline.raycastTarget=false;c.outline=outline;
    var drag=hit.gameObject.AddComponent<ControlLayoutDrag>();drag.owner=this;drag.index=controls.IndexOf(c);
    float density=frame.rect.height/450;
    var handle=BasketballUIStyle.Plate(c.proxy,"Resize "+c.id,0,0,24,24,accent,8,1);var hr=handle.rectTransform;hr.localScale=Vector3.one*density;hr.anchorMin=hr.anchorMax=hr.pivot=new Vector2(1,0);hr.anchoredPosition=Vector2.zero;MenuDesigns.Surface(handle,MenuSurface.Thumb);handle.raycastTarget=true;c.handle=handle.gameObject;
    var hd=handle.gameObject.AddComponent<ControlLayoutDrag>();hd.owner=this;hd.index=drag.index;hd.resize=true;
    var mark=new GameObject("Resize mark",typeof(RectTransform),typeof(ControlLayoutResizeMark)).GetComponent<ControlLayoutResizeMark>();mark.transform.SetParent(hr,false);CoveUI.Stretch(mark.rectTransform);mark.color=CoveUI.Ink;mark.raycastTarget=false;
   }
   var toolbar=BasketballUIStyle.Plate(editor,"Layout tools",0,0,490,88,paper,14,1,true);MenuDesigns.Dialog(toolbar);dock=toolbar.rectTransform;dock.anchorMin=dock.anchorMax=dock.pivot=new Vector2(.5f,1);toolbar.raycastTarget=true;
   Button(dock,"Cancel layout","X",8,8,34,32,Cancel,false);
   var title=Label(dock,Sport+" layout",54,8,210,32,17);
   if(dark)title.font=Resources.Load<Font>("SportyMenu/BarlowDisplay")??title.font;
   Button(dock,"Reset layout","Reset",276,8,72,32,ResetAll,false);Button(dock,"Save layout","Save",356,8,84,32,Save,true);
   dockToggle=Button(dock,"Move tools","v",448,8,34,32,()=>{dockBottom=!dockBottom;LayoutDock();},false).GetComponentInChildren<Text>();
   var selector=Button(dock,"Choose control","",12,48,178,30,()=>choices.gameObject.SetActive(!choices.gameObject.activeSelf),false);
   selection=Label(selector.transform,"",10,0,145,30,13);Label(selector.transform,"v",157,0,12,30,11);
   var track=BasketballUIStyle.Plate(dock,"Control size",207,53,211,20,dark?CoveUI.Hex("29495D"):CoveUI.Sky,10,0);track.raycastTarget=true;
   sizeSlider=track.gameObject.AddComponent<Slider>();sizeSlider.minValue=ControlLayouts.MinSize;sizeSlider.maxValue=ControlLayouts.MaxSize;
   var area=CoveUI.Rect(track.transform,"Size handle area",12,10,187,0);
   var knob=BasketballUIStyle.Plate(area,"Size handle",0,0,24,24,accent,12,1);knob.raycastTarget=true;knob.rectTransform.pivot=Vector2.one*.5f;sizeSlider.handleRect=knob.rectTransform;sizeSlider.targetGraphic=knob;
   MenuDesigns.Slider(sizeSlider);sizeSlider.onValueChanged.AddListener(ResizeSelected);
   percentage=Label(dock,"",430,48,52,30,13);
   choices=BasketballUIStyle.Plate(dock,"Control list",12,94,218,controls.Count*34+8,paper,12,1,true).rectTransform;choices.GetComponent<Graphic>().raycastTarget=true;
   for(int i=0;i<controls.Count;i++){int index=i;Button(choices,"Choose "+controls[i].id,controls[i].label,4,4+i*34,210,32,()=>Select(index),false);}
   choices.gameObject.SetActive(false);
   LayoutDock();UpdateHint();
  }
  bool dockBottom;
  void LayoutDock(){if(!dock)return;float scale=Mathf.Min(frame.rect.width/720,frame.rect.height/450);dock.localScale=Vector3.one*scale;dock.anchorMin=dock.anchorMax=dock.pivot=new Vector2(.5f,dockBottom?0:1);dock.anchoredPosition=new Vector2(0,dockBottom?10:-10);if(dockToggle)dockToggle.text=dockBottom?"^":"v";if(choices)choices.anchoredPosition=new Vector2(12,dockBottom?choices.rect.height+6:-94);}
  Text Label(Transform parent,string value,float x,float y,float w,float h,int points)=>BasketballUIStyle.Text(parent,value,x,y,w,h,points,false,TextAnchor.MiddleLeft,ink);
  Button Button(Transform parent,string name,string caption,float x,float y,float w,float h,Action action,bool primary){
   var button=BasketballUIStyle.Button(parent,name,caption,x,y,w,h,()=>action(),primary?accent:paper,13);
   var text=button.GetComponentInChildren<Text>();text.color=primary?CoveUI.Ink:ink;
   text.font=SportsUIStyle.TextFont;
   var plate=button.GetComponent<BasketballPlate>();plate.edge=dark?CoveUI.Hex("37566A"):CoveUI.Line;plate.border=1;plate.shadow=0;plate.Refresh();return button;
  }
 }
 public sealed class ControlLayoutOutline:MaskableGraphic {
  public bool selected;
  protected override void OnPopulateMesh(VertexHelper mesh){
   mesh.Clear();var r=rectTransform.rect;float length=Mathf.Min(13,Mathf.Min(r.width,r.height)*.18f),width=selected?2:1;
   foreach(var corner in new[]{new Vector2(r.xMin,r.yMin),new Vector2(r.xMax,r.yMin),new Vector2(r.xMin,r.yMax),new Vector2(r.xMax,r.yMax)}){
    Vector2 d=new(corner.x==r.xMin?1:-1,corner.y==r.yMin?1:-1);
    BasketballUIMesh.Line(mesh,corner,corner+Vector2.right*d.x*length,color,width);BasketballUIMesh.Line(mesh,corner,corner+Vector2.up*d.y*length,color,width);
   }
  }
 }
 public sealed class ControlLayoutResizeMark:MaskableGraphic {
  protected override void OnPopulateMesh(VertexHelper mesh){
   mesh.Clear();var c=rectTransform.rect.center;BasketballUIMesh.Line(mesh,c+new Vector2(-5,5),c+new Vector2(5,-5),color,1.5f);
   foreach(float sign in new[]{-1f,1f}){var p=c+new Vector2(5,-5)*sign;BasketballUIMesh.Line(mesh,p,p+new Vector2(-4*sign,0),color,1.5f);BasketballUIMesh.Line(mesh,p,p+new Vector2(0,4*sign),color,1.5f);}
  }
 }
 public sealed class ControlLayoutDrag:MonoBehaviour,IPointerDownHandler,IBeginDragHandler,IDragHandler,IEndDragHandler,IPointerUpHandler,ICancelHandler {
  public ControlLayoutEditor owner;public int index;public bool resize;
  int pointer=int.MinValue;Vector2 start,center,extent;float scale;
  public void OnPointerDown(PointerEventData e){
   if(!owner.Editing||pointer!=int.MinValue||e.button!=PointerEventData.InputButton.Left)return;
   pointer=e.pointerId;owner.Select(index);RectTransformUtility.ScreenPointToLocalPointInRectangle(owner.Frame,e.position,e.pressEventCamera,out start);center=owner.NormalizedCenter(index);scale=owner.Size(index);extent=owner.Bounds(index).size;
  }
  public void OnBeginDrag(PointerEventData e)=>OnDrag(e);
  public void OnDrag(PointerEventData e){
   if(e.pointerId!=pointer||!owner.Editing||owner.Selected!=index)return;
   RectTransformUtility.ScreenPointToLocalPointInRectangle(owner.Frame,e.position,e.pressEventCamera,out var point);var delta=point-start;
   if(resize)owner.ResizeSelected(scale*(1+Vector2.Dot(new Vector2(delta.x,-delta.y),extent)/Mathf.Max(1,extent.sqrMagnitude)));
   else owner.MoveSelected(center+new Vector2(delta.x/owner.Frame.rect.width,delta.y/owner.Frame.rect.height));
  }
  public void OnEndDrag(PointerEventData e)=>OnPointerUp(e);
  public void OnPointerUp(PointerEventData e){if(e.pointerId==pointer)Release();}
  public void Release()=>pointer=int.MinValue;
  public void OnCancel(BaseEventData e)=>Release();
  void OnDisable()=>Release();
  void OnApplicationFocus(bool focused){if(!focused)Release();}
  void OnApplicationPause(bool paused){if(paused)Release();}
 }
}
