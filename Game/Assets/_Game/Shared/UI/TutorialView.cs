using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static WhatTheFish.CoveUI;

namespace WhatTheFish {
 // Presents the live HUD itself. Hidden controls retain their native layout,
 // illustrations and gesture handlers; revealing never creates a proxy button.
 [DefaultExecutionOrder(1200)]
 public sealed class TutorialView:MonoBehaviour {
  TutorialSession lesson;RectTransform safe,design,dock,card,chapters,hud;ControlLayoutEditor layout;
  Text title,copy,count,status,next;CoveButton retry,skip,chapter;ControlLayoutOutline focus;Text marker;
  CovePlate surface;RectTransform successMark;CovePlate[] progress;TutorialFeedbackAudio feedback;
  public bool SuccessVisible=>successMark&&successMark.gameObject.activeInHierarchy;
  public int SuccessSounds=>feedback?feedback.PlayCount:0;
  readonly Dictionary<Graphic,(bool enabled,bool raycast)> hidden=new();
  readonly List<Transform> allowed=new();readonly Vector3[] corners=new Vector3[4];
  Color accent;float scanAt;Graphic[] graphics=System.Array.Empty<Graphic>();
  public static TutorialView Create(TutorialSession session,RectTransform parent){
   var root=Rect(parent,"Guided practice",0,0,1600,900);Stretch(root);
   var ui=root.gameObject.AddComponent<TutorialView>();ui.lesson=session;ui.safe=parent;ui.Build();return ui;
  }
  void Build(){
   accent=lesson.Sport switch{SportId.Basketball=>Coral,SportId.Football=>Mint,SportId.Golf=>Gold,_=>Sky};
   design=Rect(transform,"Tutorial canvas",0,0,1600,900);
   dock=Rect(design,"Coach dock",458,24,684,294);
   surface=Plate(dock,"One lesson",0,0,684,190,Hex("142C41"),22,0,true);card=surface.rectTransform;
   Plate(card,"Sport accent",0,0,7,190,accent,3,0);
   count=Text(card,"",24,10,636,25,18,false,accent);
   progress=new CovePlate[lesson.Steps.Length];float segment=(636-(progress.Length-1)*5)/progress.Length;
   for(int i=0;i<progress.Length;i++)progress[i]=Plate(card,"Lesson progress "+(i+1),24+i*(segment+5),42,segment,6,Hex("3C5364"),3,0);
   title=Text(card,"Preparing your practice",24,61,636,42,30,true,Cream);
   copy=Text(card,"",24,106,636,72,23,false,Cream);
   successMark=Plate(card,"Lesson complete check",24,70,68,68,Ink,34,0).rectTransform;
   Icon(successMark,CoveSymbol.Check,16,16,36,Mint);
   next=Text(card,"",112,153,548,28,18,false,Ink);next.gameObject.SetActive(false);
   feedback=gameObject.AddComponent<TutorialFeedbackAudio>();
   Button(design,"Quit tutorial","Quit",1400,24,160,60,Cream,()=>lesson.Quit(),CoveSymbol.Close,24);
   retry=Button(dock,"Retry lesson","Retry",58,200,170,48,Cream,()=>lesson.Retry(),size:21);
   skip=Button(dock,"Skip lesson","Skip",256,200,170,48,Cream,()=>lesson.Skip(),size:21);
   chapter=Button(dock,"Tutorial chapters","Lessons",454,200,170,48,Cream,OpenChapters,size:21);
   status=Text(dock,"",0,260,684,34,22,true,Cream,TextAnchor.MiddleCenter);
   focus=Rect(transform,"Current control highlight",0,0,0,0).gameObject.AddComponent<ControlLayoutOutline>();focus.color=accent;focus.selected=true;focus.raycastTarget=false;
   marker=Text(transform,"↓",0,0,50,58,44,true,accent,TextAnchor.MiddleCenter);
   chapters=Rect(design,"Lesson list",0,0,1600,900);var shade=chapters.gameObject.AddComponent<Image>();shade.color=new Color(.025f,.06f,.1f,.88f);shade.raycastTarget=true;
   MenuDesigns.Dialog(Plate(chapters,"Lesson list card",170,20,1260,860,MenuDesigns.Navy,26,0,true));
   Text(chapters,lesson.Sport+" lessons",210,80,1180,60,40,true,Cream);
   Text(chapters,"Choose a lesson to practise. Completed steps are saved to your profile.",210,146,1180,44,24,false,Cream);
   for(int i=0;i<lesson.Steps.Length;i++){int ix=i;Button(chapters,"Lesson "+lesson.Steps[i].id,(i+1)+"  "+lesson.Steps[i].title,210+(i%2)*610,212+(i/2)*63,580,52,Cream,()=>lesson.Begin(ix),size:22);}
   Button(chapters,"Close lessons","Back",210,804,220,60,accent,()=>{CloseChapters();lesson.Pause(false);},CoveSymbol.Back,24);
   Button(chapters,"Quit from lessons","Quit tutorial",1130,804,260,60,Cream,()=>lesson.Quit(),size:24);
   foreach(var surface in new[]{card.GetComponent<CovePlate>(),dock.GetComponent<CovePlate>()})if(surface)MenuDesigns.Dialog(surface);chapters.gameObject.SetActive(false);Refresh();
  }
  static CoveButton Button(Transform parent,string id,string label,float x,float y,float w,float h,Color color,System.Action action,CoveSymbol icon=CoveSymbol.None,int size=28){var b=CoveUI.Button(parent,id,label,x,y,w,h,color,action,icon,size);MenuDesigns.Apply(b);return b;}
  public void Bind(RectTransform root,ControlLayoutEditor controls){hud=root;layout=controls;scanAt=0;}
  public void Refresh(){
   if(!lesson)return;bool ready=lesson.Ready,success=lesson.Succeeded&&!lesson.Finished;
   retry.gameObject.SetActive(ready&&!lesson.Finished&&!success);skip.gameObject.SetActive(ready&&!lesson.Finished&&!success);chapter.gameObject.SetActive(ready);
   bool complete=lesson.Finished&&lesson.CompletedCount==lesson.Steps.Length;
   surface.color=success||complete?Mint:Hex("142C41");surface.SetVerticesDirty();
   title.text=success?"Nice work!":lesson.Finished?complete?"Tutorial complete!":"Practice finished":ready?lesson.Step.title:"Preparing your practice";
   title.color=copy.color=count.color=success||complete?Ink:Cream;
   count.text="TUTORIAL  ·  "+lesson.Sport.ToString().ToUpperInvariant()+"  ·  "+(ready?(lesson.Index+1)+" / "+lesson.Steps.Length:"LOADING");
   title.rectTransform.anchoredPosition=new Vector2(success?112:24,success?-54:-61);title.rectTransform.sizeDelta=new Vector2(success?548:636,success?60:48);title.fontSize=success?38:30;
   copy.rectTransform.anchoredPosition=new Vector2(success?112:24,success?-116:-110);copy.rectTransform.sizeDelta=new Vector2(success?548:636,success?36:68);copy.fontSize=success?21:23;
   copy.text=lesson.Finished?lesson.CompletedCount+" / "+lesson.Steps.Length+" lessons learned. Use Lessons to "+(complete?"practise again.":"revisit skipped steps."):ready?lesson.Step.copy:"Loading the island and your controls…";
   if(success)copy.text=lesson.Step.title+" — complete";
   successMark.gameObject.SetActive(success);next.gameObject.SetActive(success);
   if(success)next.text=lesson.Index+1<lesson.Steps.Length?"Up next: "+lesson.Steps[lesson.Index+1].title:"All done. Your progress is saved.";
   for(int i=0;i<progress.Length;i++){progress[i].color=ready&&lesson.Completed(lesson.Steps[i].id)?(success||complete?Ink:Mint):i==lesson.Index?accent:success||complete?new Color(Ink.r,Ink.g,Ink.b,.18f):Hex("3C5364");progress[i].SetVerticesDirty();}
   status.text=lesson.Status;
  }
  public void Celebrate(){feedback.PlaySuccess();Refresh();}
  public void OpenChapters(){if(!lesson.Ready)return;lesson.Pause(true);chapters.gameObject.SetActive(true);}
  public void CloseChapters(){if(chapters)chapters.gameObject.SetActive(false);}
  bool Within(Transform t,Transform root)=>t==root||t.IsChildOf(root);
  void Visibility(){
   if(!hud||!layout)return;
   if(Time.unscaledTime>=scanAt){graphics=hud.GetComponentsInChildren<Graphic>(true);scanAt=Time.unscaledTime+1;}
   allowed.Clear();
   foreach(var c in layout.Controls){bool visible=lesson.Revealed.Contains(c.id);c.visibility.alpha=visible?1:0;c.visibility.blocksRaycasts=c.visibility.interactable=visible&&!lesson.Paused&&!lesson.Finished;if(visible)allowed.Add(c.wrapper);}
   foreach(var pad in hud.GetComponentsInChildren<TouchPad>(true))if(pad.look&&TutorialSession.Allows("look"))allowed.Add(pad.transform);
   if(lesson.Sport==SportId.Fishing){
    foreach(var t in hud.GetComponentsInChildren<RectTransform>(true)){
     if(lesson.Step.id=="skillChoice"&&t.name=="Fishing skill choice")allowed.Add(t);
     if(lesson.Revealed.Contains("left")&&(t.name=="Fishing catch and tension"||t.GetComponent<FishingFightGuide>()))allowed.Add(t);
     if(lesson.Revealed.Contains("cast")&&(t.name=="Fishing aim reticle"||t.name=="Selected fish brackets"))allowed.Add(t);
    }
    var ability=hud.GetComponentInChildren<FishingAbilityHUD>(true);
    if(ability&&lesson.Step.id=="skillChoice")foreach(var b in ability.ChoiceButtons)if(b)allowed.Add(b.transform.parent);
   }
   foreach(var g in graphics){
    if(!g)continue;bool show=false;for(int i=0;i<allowed.Count;i++)if(Within(g.transform,allowed[i])){show=true;break;}
    if(show){if(hidden.Remove(g,out var old)){g.enabled=old.enabled;g.raycastTarget=old.raycast;}}
    else {if(!hidden.ContainsKey(g))hidden[g]=(g.enabled,g.raycastTarget);g.enabled=false;g.raycastTarget=false;}
   }
  }
  void LateUpdate(){
   if(!lesson)return;transform.SetAsLastSibling();float scale=Mathf.Min(safe.rect.width/1600,safe.rect.height/900);design.localScale=Vector3.one*scale;
   design.anchoredPosition=new Vector2((safe.rect.width-1600*scale)/2,-(safe.rect.height-900*scale)/2);
   status.text=lesson.Status;Visibility();PositionDock();focus.enabled=false;marker.enabled=false;
   successMark.localScale=Vector3.one*(MenuPreferences.ReducedMotion?1:Mathf.Lerp(.82f,1,Mathf.SmoothStep(0,1,lesson.SuccessAge/.24f)));
   if(!lesson.Ready||lesson.Paused||lesson.Finished||lesson.Succeeded)return;
   var control=layout.Controls.FirstOrDefault(c=>c.id==lesson.Step.focus);
   if(control!=null&&control.target.gameObject.activeInHierarchy){
    control.target.GetWorldCorners(corners);var a=(Vector2)transform.InverseTransformPoint(corners[0]);var b=(Vector2)transform.InverseTransformPoint(corners[2]);
    float pulse=MenuPreferences.ReducedMotion?0:2*(1+Mathf.Sin(Time.unscaledTime*4));
    var r=(RectTransform)transform;focus.rectTransform.anchoredPosition=new Vector2(a.x-r.rect.xMin-7-pulse,b.y-r.rect.yMax+7+pulse);focus.rectTransform.sizeDelta=b-a+Vector2.one*(14+2*pulse);focus.enabled=true;focus.SetVerticesDirty();
   }
   if(lesson.TargetPoint.HasValue&&Camera.main){var p=Camera.main.WorldToScreenPoint(lesson.TargetPoint.Value+Vector3.up*1.25f);if(p.z>0&&RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)transform,p,null,out var point)){marker.rectTransform.anchorMin=marker.rectTransform.anchorMax=new Vector2(.5f,.5f);marker.rectTransform.pivot=new Vector2(.5f,0);marker.rectTransform.anchoredPosition=point;marker.enabled=true;}}
  }
  static readonly Vector2[] dockPositions={new(458,24),new(24,24),new(458,570),new(24,304)};
  void PositionDock(){
   if(!layout||!lesson.Ready)return;float best=float.PositiveInfinity;Vector2 chosen=dockPositions[0];
   foreach(var at in dockPositions){float overlap=0;var panel=new Rect(at,new Vector2(684,294));
    foreach(var c in layout.Controls){if(!lesson.Revealed.Contains(c.id)||!c.target.gameObject.activeInHierarchy)continue;c.target.GetWorldCorners(corners);Vector2 a=design.InverseTransformPoint(corners[0]),b=design.InverseTransformPoint(corners[2]);var r=new Rect(a.x-12,-b.y-12,b.x-a.x+24,b.y-a.y+24);
     if(panel.Overlaps(r))overlap+=Mathf.Max(0,Mathf.Min(panel.xMax,r.xMax)-Mathf.Max(panel.xMin,r.xMin))*Mathf.Max(0,Mathf.Min(panel.yMax,r.yMax)-Mathf.Max(panel.yMin,r.yMin));
    }
    if(overlap<best){best=overlap;chosen=at;}if(best==0)break;
   }
   dock.anchoredPosition=new Vector2(chosen.x,-chosen.y);
  }
  public void Restore(){foreach(var item in hidden)if(item.Key){item.Key.enabled=item.Value.enabled;item.Key.raycastTarget=item.Value.raycast;}hidden.Clear();if(layout)foreach(var c in layout.Controls)if(c.visibility){c.visibility.alpha=1;c.visibility.blocksRaycasts=c.visibility.interactable=true;}}
  void OnDestroy(){Restore();}
 }
}
