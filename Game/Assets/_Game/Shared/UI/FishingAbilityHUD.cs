using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
namespace WhatTheFish {
 public sealed class FishingAbilityHUD:MonoBehaviour {
  RectTransform frame,panel,sheet,skillRect,iconRect;FishingHUD owner;Button skill;CanvasGroup skillAlpha;Text skillLabel,title,footer;
  readonly Button[] cards=new Button[3];readonly RectTransform[] icons=new RectTransform[3];readonly Text[] names=new Text[3],descriptions=new Text[3];
  FishingSkillGraphic clockRing;FishingAbility shown=FishingAbility.None;FishingAbility[] shownOffers=new FishingAbility[3];uint seenActivation;bool observed,readyBefore;float readyPulse;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
  public bool ReviewPracticeChoice;
#endif
  public bool Modal=>panel&&panel.gameObject.activeSelf;
  public bool SkillVisible=>skillRect&&skillRect.gameObject.activeSelf;
  public bool SkillReady=>skill&&skill.interactable;
  public Button SkillButton=>skill;
  public Button[] ChoiceButtons=>cards;
  public float SkillOpacity=>skillAlpha?skillAlpha.alpha:0;
  public float CooldownFill=>clockRing?clockRing.amount:0;
  public void Build(RectTransform parent,FishingHUD hud,Font unused){
   frame=parent;owner=hud;
   skill=BasketballUIStyle.Button(frame,"Fishing round skill","",0,0,100,100,Use,CoveUI.Cream);skillRect=(RectTransform)skill.transform;skillRect.anchorMin=skillRect.anchorMax=skillRect.pivot=Vector2.one*.5f;
   skill.GetComponent<BasketballPlate>().radius=Vector2.one*35;skill.GetComponent<BasketballPlate>().border=3;skillAlpha=skill.gameObject.AddComponent<CanvasGroup>();
   iconRect=BasketballUIStyle.Center(skillRect,"Chosen skill icon",Vector2.one*62,new Vector2(0,8),Vector2.one*.5f);
   SetIcon(iconRect,FishingAbility.Sonar);
   skillLabel=BasketballUIStyle.Text(skillRect,"USE",7,72,86,21,12,true,TextAnchor.MiddleCenter);
   var ringRect=BasketballUIStyle.Center(skillRect,"Skill clock border",Vector2.one*98,Vector2.zero,Vector2.one*.5f);clockRing=ringRect.gameObject.AddComponent<FishingSkillGraphic>();clockRing.ring=true;clockRing.raycastTarget=false;
   var shade=BasketballUIStyle.Plate(frame,"Fishing skill draft overlay",0,0,100,100,new Color(.035f,.12f,.17f,.65f),0,0);panel=shade.rectTransform;CoveUI.Stretch(panel);shade.raycastTarget=true;
   var card=BasketballUIStyle.Plate(panel,"Shared three-card draft",0,0,692,366,CoveUI.Cream,26,3,true);sheet=card.rectTransform;sheet.anchorMin=sheet.anchorMax=sheet.pivot=Vector2.one*.5f;sheet.anchoredPosition=Vector2.zero;
   title=BasketballUIStyle.Text(sheet,"CHOOSE 1 SKILL",18,12,656,43,28,true,TextAnchor.MiddleCenter);
   for(int i=0;i<3;i++){int index=i;var b=cards[i]=BasketballUIStyle.Button(sheet,"Fishing skill choice "+i,"",18+i*220,92,204,218,()=>Choose(index),BasketballUIStyle.Paper);
    icons[i]=BasketballUIStyle.Center((RectTransform)b.transform,"Draft skill icon",Vector2.one*72,new Vector2(0,65),Vector2.one*.5f);
    names[i]=BasketballUIStyle.Text(b.transform,"",8,88,188,30,18,true,TextAnchor.MiddleCenter);
    descriptions[i]=BasketballUIStyle.Text(b.transform,"",13,123,178,55,14,false,TextAnchor.MiddleCenter);
   }
   footer=BasketballUIStyle.Text(sheet,"",18,323,656,28,15,true,TextAnchor.MiddleCenter);panel.gameObject.SetActive(false);skillRect.gameObject.SetActive(false);
  }
  public void RegisterLayout(ControlLayoutEditor editor){editor.Register("ability","Round skill",skillRect,FishingGearArt.Icon(0));}
  void SetIcon(RectTransform at,FishingAbility ability){foreach(Transform child in at)Destroy(child.gameObject);var r=BasketballUIStyle.Center(at,"Skill illustration",at.sizeDelta,Vector2.zero,Vector2.one*.5f);FishingSkillGraphic.Icon(r,ability);}
  void Choose(int i){var game=FishingGame.Instance;var view=PlayerView.Instance;if(game&&view)view.FishingAbilityInput(FishingAction.ChooseSkill,(int)game.State.Offer(i));}
  void Use(){PlayerView.Instance?.FishingAbilityInput(FishingAction.UseAbility);}
  public void CloseOverlay(){var game=FishingGame.Instance;var view=PlayerView.Instance;var p=game&&view?game.Player(view.target):null;if(p.HasValue&&!p.Value.skillPicked&&(game.State.Preparing||game.State.Match.phase==FishingRoundPhase.Practice))view.FishingAbilityInput(FishingAction.ChooseSkill,(int)game.State.Match.offer0);panel.gameObject.SetActive(false);}
  void LateUpdate(){
   var game=FishingGame.Instance;var view=PlayerView.Instance;var p=game&&view?game.Player(view.target):null;
   bool context=game&&game.Context&&view&&view.active&&p.HasValue;
   if(!context){panel.gameObject.SetActive(false);skillRect.gameObject.SetActive(false);observed=false;return;}
   var player=p.Value;bool practice=false;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
   practice=ReviewPracticeChoice&&game.State.Match.phase==FishingRoundPhase.Practice&&!player.skillPicked;
#endif
   bool choose=game.State.Match.phase==FishingRoundPhase.Drafting||practice;
   choose&=!(owner.Tutorial&&owner.Tutorial.Visible)&&!(owner.IslandMenu&&owner.IslandMenu.Visible)&&!(owner.Results&&owner.Results.Visible)&&!FishingCeremony.Presenting;
   panel.gameObject.SetActive(choose);if(choose){panel.SetAsLastSibling();var size=owner.Settings.skillCardSize;sheet.sizeDelta=new Vector2(size.x*3+80,size.y+148);float width=sheet.sizeDelta.x;
    title.text=player.skillPicked?"SKILL LOCKED":"CHOOSE 1 SKILL";title.rectTransform.sizeDelta=new Vector2((width-36)*2,86);
    footer.text=game.State.Preparing?(player.skillPicked?"Waiting for friends  ·  ":"Pick one  ·  ")+Math.Max(0,Math.Ceiling(game.State.Match.draftUntil-game.Now))+"s":"Choose one to try in practice";
    for(int i=0;i<3;i++){var ability=game.State.Offer(i);if(shownOffers[i]!=ability){shownOffers[i]=ability;SetIcon(icons[i],ability);}names[i].text=FishingAbilityRules.Name(ability);descriptions[i].text=FishingAbilityRules.Description(ability);
     var rect=(RectTransform)cards[i].transform;rect.sizeDelta=size;rect.anchoredPosition=new Vector2(18+i*(size.x+16),-92);cards[i].interactable=!player.skillPicked;var plate=cards[i].GetComponent<BasketballPlate>();plate.color=player.skillPicked&&player.item==ability?CoveUI.Sky:BasketballUIStyle.Paper;plate.border=player.skillPicked&&player.item==ability?4:2;plate.Refresh();
    }
    sheet.localScale=Vector3.one*Mathf.Min(1,(frame.rect.width-28)/sheet.rect.width,(frame.rect.height-24)/sheet.rect.height);
   }
   bool active=player.active!=FishingAbility.None&&game.Now<player.activeUntil;
   bool show=game.State.Running&&FishingAbilityRules.PhaseAllows(player.item,player.phase)&&!choose&&!owner.BlocksGameplay;
   LayoutSkill(0);skillRect.gameObject.SetActive(show);if(!show){readyBefore=false;return;}
   if(shown!=player.item){shown=player.item;SetIcon(iconRect,shown);}
   bool allowed=FishingAbilityRules.CanUse(player,game.Now)&&(player.item!=FishingAbility.SchoolCall||game.NearestPier(view.target)>=0);
   skill.interactable=allowed;if(allowed&&!readyBefore)readyPulse=1;readyBefore=allowed;readyPulse=Mathf.MoveTowards(readyPulse,0,Time.unscaledDeltaTime*1.7f);
   if(observed&&seenActivation!=player.itemSequence){readyPulse=1;}seenActivation=player.itemSequence;observed=true;
   skillAlpha.alpha=allowed||active?1:owner.Settings.skillCooldownOpacity;
   var tint=FishingAbilityRules.Color(player.item);var plateSkill=skill.GetComponent<BasketballPlate>();plateSkill.color=active?tint:CoveUI.Cream;plateSkill.border=allowed?4:3;plateSkill.Refresh();
   float fill=active?(float)((player.activeUntil-game.Now)/FishingAbilityRules.Duration(player.active)):allowed?1:1-(float)((player.cooldownUntil-game.Now)/FishingAbilityRules.Cooldown(player.item));clockRing.Tick(fill,active?tint:allowed?CoveUI.Gold:CoveUI.Ink);
   skillLabel.text=active?"ACTIVE":allowed?"USE":Math.Max(0,Math.Ceiling(player.cooldownUntil-game.Now))+"s";
   LayoutSkill(MenuPreferences.ReducedMotion?0:readyPulse);
  }
  void LayoutSkill(float pulse){
   float scale=owner.HudScale;var size=owner.Settings.skillButtonSize;skillRect.sizeDelta=size;var corners=new Vector3[4];owner.ActionRect.GetWorldCorners(corners);var top=frame.InverseTransformPoint(corners[1]);
   Vector2 at=new Vector2(top.x-(size.x*.5f+owner.Settings.skillButtonGap.x)*scale,top.y-size.y*.5f*scale);var half=size*scale*.5f;var b=frame.rect;
   at.x=Mathf.Clamp(at.x,b.xMin+half.x+8,b.xMax-half.x-8);at.y=Mathf.Clamp(at.y,b.yMin+half.y+8,b.yMax-half.y-8);skillRect.anchoredPosition=at-frame.rect.center;skillRect.localScale=Vector3.one*scale*(1+.05f*pulse);
  }
  public RectTransform[] VisibleRegions=>SkillVisible?new[]{skillRect}:Array.Empty<RectTransform>();
  public bool FitsSafeFrame(){var corners=new Vector3[4];foreach(var widget in VisibleRegions.Concat(Modal?new[]{sheet}:Array.Empty<RectTransform>())){widget.GetWorldCorners(corners);foreach(var c in corners){var at=frame.InverseTransformPoint(c);if(!frame.rect.Contains(at))return false;}}return true;}
 }
}
