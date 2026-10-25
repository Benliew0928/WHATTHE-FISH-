using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace WhatTheFish {
 // Shared shell for the entry popup, island settings and menu reference.
 // The Fishing examples remain the reference implementation for all sports.
 public abstract class IslandTutorialUI:MonoBehaviour {
  static readonly List<IslandTutorialUI> instances=new();
  protected RectTransform frame,overlay,card,content;
  protected readonly RectTransform[] examples=new RectTransform[4];
  protected bool embedded;
  Text title,body,count,nextLabel;Image picture;Button back;CanvasGroup pageFade;
  Action closed;int step;float openedAt,changedAt;
  public abstract SportId Sport {get;}
  protected abstract string[] PageTitles {get;}
  protected abstract string[] PageCopy {get;}
  protected abstract string SeenPreference {get;}
  protected virtual string FinishCaption=>"LET'S PLAY!";
  public bool Visible=>overlay&&overlay.gameObject.activeInHierarchy;
  public int Step=>step;
  public string Body=>body?body.text:"";
  public bool HasBeenSeen=>PlayerPrefs.GetInt(SeenPreference,0)!=0;
  public static bool ModalOpen {get{foreach(var ui in instances)if(ui&&!ui.embedded&&ui.Visible)return true;return false;}}
  public static bool DismissTop(){for(int i=instances.Count-1;i>=0;i--)if(instances[i]&&!instances[i].embedded&&instances[i].Visible){instances[i].Close();return true;}return false;}
  public static IslandTutorialUI CreateFor(RectTransform parent,SportId sport,bool embedded=false,Action closed=null)=>sport==SportId.Fishing?FishingTutorialUI.Create(parent,embedded,closed):SportTutorialUI.Create(parent,sport,embedded,closed);
  protected void Initialize(RectTransform parent,bool embedded,Action closed){this.frame=parent;this.embedded=embedded;this.closed=closed;instances.Add(this);Build();if(embedded)Open();}
  protected RectTransform Rect(string name,Transform parent,Vector2 pos,Vector2 size)=>GolfHUDStyle.Rect(name,parent,new Vector2(.5f,.5f),pos,size);
  protected Text Text(string name,Transform parent,string value,Vector2 pos,Vector2 size,int points){var t=Rect(name,parent,pos,size).gameObject.AddComponent<LocalizedText>();t.font=points<=23?SportsUIStyle.TextFont:SportsUIStyle.DisplayFont;t.text=value;t.fontSize=points;t.color=GolfHUDStyle.Ink;t.alignment=TextAnchor.MiddleCenter;t.raycastTarget=false;t.resizeTextForBestFit=true;t.resizeTextMinSize=Mathf.Max(10,Mathf.RoundToInt(points*.55f));t.resizeTextMaxSize=points;return t;}
  Button Button(string name,Vector2 pos,Vector2 size,string caption,Action action,out Text label){var r=Rect(name,card,pos,size);var graphic=r.gameObject.AddComponent<FishingStickerGraphic>();graphic.kind=FishingStickerGraphic.Kind.Panel;graphic.tint=GolfHUDStyle.Cream;graphic.UseSolidPanel();graphic.raycastTarget=true;var b=r.gameObject.AddComponent<Button>();b.targetGraphic=graphic;b.transition=Selectable.Transition.None;b.navigation=new Navigation{mode=Navigation.Mode.None};label=Text(name+" caption",r,caption,Vector2.zero,size-new Vector2(12,8),24);b.onClick.AddListener(()=>{GolfFishingAudio.PlayMenu("ui-select");action();});r.gameObject.AddComponent<BasketballButtonFeedback>();r.gameObject.AddComponent<GolfFishingButtonSound>();MenuDesigns.Apply(b);return b;}
  void Build(){
   string prefix=Sport+" tutorial ";
   overlay=Rect(prefix+"overlay",transform,Vector2.zero,Vector2.zero);CoveUI.Stretch(overlay);var shade=overlay.gameObject.AddComponent<Image>();shade.color=embedded?Color.clear:new Color(.025f,.11f,.15f,.6f);shade.raycastTarget=!embedded;
   if(!embedded){var modal=overlay.gameObject.AddComponent<Canvas>();modal.overrideSorting=true;modal.sortingOrder=300;overlay.gameObject.AddComponent<GraphicRaycaster>();}
   card=Rect(prefix+"card",overlay,new Vector2(0,embedded?-75:0),new Vector2(860,452));var panel=card.gameObject.AddComponent<FishingStickerGraphic>();panel.kind=FishingStickerGraphic.Kind.Panel;panel.tint=GolfHUDStyle.Cream;panel.cornerRadius=28;panel.UseSolidPanel();panel.raycastTarget=true;MenuDesigns.Dialog(panel);
   content=Rect(prefix+"page",card,Vector2.zero,card.sizeDelta);pageFade=content.gameObject.AddComponent<CanvasGroup>();pageFade.blocksRaycasts=false;
   title=Text(prefix+"title",content,PageTitles[0],new Vector2(0,175),new Vector2(780,46),32);
   picture=Rect(prefix+"illustration",content,new Vector2(-216,69),new Vector2(350,142)).gameObject.AddComponent<Image>();picture.preserveAspect=true;picture.raycastTarget=false;
   BuildExamples();
   body=Text(prefix+"instructions",content,PageCopy[0],new Vector2(198,7),new Vector2(380,246),23);body.alignment=TextAnchor.MiddleLeft;
   count=Text(prefix+"step",card,"1 / 4",new Vector2(-74,-175),new Vector2(90,42),24);
   Button(prefix+"skip",new Vector2(-312,-175),new Vector2(180,48),embedded?"DONE":"SKIP",Close,out _);
   back=Button(prefix+"back",new Vector2(76,-175),new Vector2(136,48),"BACK",()=>Show(step-1),out _);
   var next=Button(prefix+"next",new Vector2(272,-175),new Vector2(212,48),"NEXT",()=>{if(step<3)Show(step+1);else Close();},out nextLabel);next.GetComponent<FishingStickerGraphic>().Refresh(1,GolfHUDStyle.Gold);
   overlay.gameObject.SetActive(false);
  }
  public virtual void Open(){if(!overlay)return;if(!embedded)ClearInput();openedAt=Time.unscaledTime;Show(0);overlay.gameObject.SetActive(true);transform.SetAsLastSibling();Layout();}
  public void Show(int value){step=Mathf.Clamp(value,0,3);changedAt=Time.unscaledTime;title.text=PageTitles[step];body.text=PageCopy[step];count.text=(step+1)+" / 4";picture.sprite=Illustration(step);picture.enabled=picture.sprite;for(int i=0;i<examples.Length;i++)examples[i].gameObject.SetActive(i==step);back.interactable=step>0;nextLabel.text=step<3?"NEXT":FinishCaption;}
  protected abstract void BuildExamples();
  protected abstract void AnimateExamples();
  protected virtual Sprite Illustration(int page)=>null;
  protected Image Icon(Transform parent,string name,Sprite sprite,Vector2 position,Vector2 size){var image=Rect(name,parent,position,size).gameObject.AddComponent<Image>();image.sprite=sprite;image.preserveAspect=true;image.raycastTarget=false;return image;}
  protected void HideUnread(){bool wasVisible=Visible;if(overlay)overlay.gameObject.SetActive(false);if(wasVisible&&!embedded)ClearInput();}
  public void Close(){bool wasVisible=Visible;HideUnread();if(wasVisible&&!embedded){PlayerPrefs.SetInt(SeenPreference,1);PlayerPrefs.Save();}closed?.Invoke();}
  static void ClearInput(){var view=PlayerView.Instance;if(view){view.ClearMatchInput();view.stick?.ResetInput();PlayerView.LookDelta=Vector2.zero;}}
  void Layout(){if(!frame||!card)return;float scale=Mathf.Min(embedded?1.15f:1.35f,frame.rect.width*.88f/860,frame.rect.height*(embedded?.68f:.88f)/452);float reveal=MenuPreferences.ReducedMotion?1:Mathf.SmoothStep(0,1,(Time.unscaledTime-openedAt)/.22f);card.localScale=Vector3.one*Mathf.Max(.1f,scale)*Mathf.Lerp(.96f,1,reveal);if(pageFade)pageFade.alpha=MenuPreferences.ReducedMotion?1:Mathf.Lerp(.45f,1,Mathf.SmoothStep(0,1,(Time.unscaledTime-changedAt)/.16f));}
  protected virtual void LateUpdate(){Layout();if(Visible)AnimateExamples();}
  protected virtual void OnDestroy(){instances.Remove(this);}
  public bool FitsSafeFrame(){if(!Visible)return true;var corners=new Vector3[4];card.GetWorldCorners(corners);var bounds=frame.rect;foreach(var c in corners){var p=frame.InverseTransformPoint(c);if(p.x<bounds.xMin-1||p.x>bounds.xMax+1||p.y<bounds.yMin-1||p.y>bounds.yMax+1)return false;}return true;}
 }
}
