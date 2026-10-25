using UnityEngine;
using UnityEngine.UI;

namespace WhatTheFish {
 // Accepted local awards only. Repeated snapshots, late joins and refocus do
 // not replay a catch, and this presentation never awards or predicts points.
 public sealed class FishingCatchCue:MonoBehaviour {
  RectTransform root,badge;CanvasGroup group;Text points,detail;FishingStickerGraphic[] shines;
  bool observing;ulong owner;uint round,award;float age=3;
  public bool Visible=>root&&root.gameObject.activeSelf;
  public int Starts {get;private set;}
  public string PointsText=>points?points.text:"";
  public static FishingCatchCue Create(RectTransform parent,Font font){
   var root=GolfLeaderboardUI.Rect("Accepted fishing catch",parent,new Vector2(.5f,.5f),Vector2.zero,parent.sizeDelta);
   var cue=root.gameObject.AddComponent<FishingCatchCue>();cue.root=root;cue.group=root.gameObject.AddComponent<CanvasGroup>();cue.group.interactable=cue.group.blocksRaycasts=false;
   cue.badge=GolfLeaderboardUI.Rect("Landed fish badge",root,new Vector2(.5f,.5f),new Vector2(-188,0),new Vector2(98,98));
   var fish=cue.badge.gameObject.AddComponent<FishingStickerGraphic>();fish.kind=FishingStickerGraphic.Kind.Fish;fish.Decorate();fish.raycastTarget=false;
   Text Label(string name,string value,Vector2 position,Vector2 size,int fontSize,Color color){var node=GolfLeaderboardUI.Rect(name,root,new Vector2(.5f,.5f),position,size);var t=node.gameObject.AddComponent<LocalizedText>();t.text=value;t.font=font;t.fontSize=fontSize;t.fontStyle=FontStyle.Normal;t.alignment=TextAnchor.MiddleCenter;t.color=color;t.raycastTarget=false;t.supportRichText=false;return t;}
   Label("Catch confirmed","FISH LANDED!",new Vector2(46,49),new Vector2(348,44),25,LocalProfile.Hex("143B50"));
   cue.points=Label("Accepted catch points","+2 POINTS",new Vector2(46,3),new Vector2(348,60),43,LocalProfile.Hex("FFCB53"));
   var outline=cue.points.gameObject.AddComponent<Outline>();outline.effectColor=LocalProfile.Hex("143B50");outline.effectDistance=new Vector2(2,-2);
   cue.detail=Label("Caught fish detail","SMALL CATCH",new Vector2(46,-47),new Vector2(360,32),21,LocalProfile.Hex("143B50"));
   cue.shines=new FishingStickerGraphic[6];for(int i=0;i<cue.shines.Length;i++){var node=GolfLeaderboardUI.Rect("Landing glint "+i,root,new Vector2(.5f,.5f),Vector2.zero,Vector2.one*16);var s=node.gameObject.AddComponent<FishingStickerGraphic>();s.kind=FishingStickerGraphic.Kind.Shine;s.tint=LocalProfile.Hex("FFCB53");s.raycastTarget=false;cue.shines[i]=s;}
   root.gameObject.SetActive(false);return cue;
  }
  public void Tick(FishingPlayerRecord? player,uint currentRound,double now,bool eligible){
   if(!eligible||!player.HasValue||!player.Value.connected){observing=false;Hide();return;}
   var p=player.Value;
   if(!observing||round!=currentRound||owner!=p.owner){observing=true;round=currentRound;owner=p.owner;award=p.awardSequence;Hide();return;}
   if(award!=p.awardSequence){
    bool newer=unchecked((int)(p.awardSequence-award))>0;if(!newer)return;award=p.awardSequence;
    if(p.phase==FishingPhase.Caught&&p.result==FishingResult.Landed&&p.lastAward>0&&double.IsFinite(now)&&now>=p.phaseAt-.15&&now-p.phaseAt<=1.1){
     age=0;Starts++;points.text="+"+p.lastAward+" POINTS";detail.text=FishingState.SizeName(p.fish)+" CATCH"+(p.lastBonus>0?" · WANTED +"+p.lastBonus:"");
    }
   }
   age+=Time.unscaledDeltaTime;bool show=age<1.8f;root.gameObject.SetActive(show);if(!show)return;
   bool reduced=MenuPreferences.ReducedMotion;group.alpha=1-Mathf.InverseLerp(1.45f,1.8f,age);
   badge.localScale=Vector3.one*(reduced?1:1+Mathf.Sin(Mathf.Clamp01(age/.34f)*Mathf.PI)*.18f);
   for(int i=0;i<shines.Length;i++){
    float a=(i*60+20)*Mathf.Deg2Rad,travel=reduced?1:Mathf.Clamp01(age/.45f);
    shines[i].rectTransform.anchoredPosition=new Vector2(-188,0)+new Vector2(Mathf.Cos(a)*68,Mathf.Sin(a)*65)*(.72f+.28f*travel);
    shines[i].rectTransform.localScale=Vector3.one*(reduced?.65f:Mathf.Sin(Mathf.Clamp01(age/1.45f)*Mathf.PI));
   }
  }
  void Hide(){age=3;if(root)root.gameObject.SetActive(false);}
  public void Clear(){observing=false;Hide();}
 }
}
