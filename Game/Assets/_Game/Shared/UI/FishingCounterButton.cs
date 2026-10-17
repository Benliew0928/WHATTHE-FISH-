using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace WhatTheFish {
 // Each thumb owns its hold; releasing an old finger cannot cancel a new hold.
 public sealed class FishingCounterButton:MonoBehaviour,IPointerDownHandler,IPointerUpHandler,IPointerExitHandler,ICancelHandler {
  public int side=-1;int pointer=int.MinValue;
  public void OnPointerDown(PointerEventData e){
   if(e.button!=PointerEventData.InputButton.Left||pointer!=int.MinValue||!GetComponent<Button>().IsInteractable())return;
   if(PlayerView.Instance&&PlayerView.Instance.BeginCountersteer(side,e.pointerId))pointer=e.pointerId;
  }
  public void OnPointerUp(PointerEventData e){if(e.pointerId==pointer)Release();}
  public void OnPointerExit(PointerEventData e){if(e.pointerId==pointer)Release();}
  public void OnCancel(BaseEventData e)=>Release();
  void Release(){if(pointer!=int.MinValue)PlayerView.Instance?.EndFishing(pointer);pointer=int.MinValue;}
  void OnDisable()=>Release();void OnApplicationFocus(bool focus){if(!focus)Release();}void OnApplicationPause(bool pause){if(pause)Release();}
 }
 // One shared-material meter, with no texture, animator or effects package.
 // Fish position previews a turn; the arrow shows the player's actual input.
 public sealed class FishingFightGuide:MaskableGraphic {
  Image fishIcon;float pull,steer;bool resting,wrong,danger;float beat;int requiredSide;
  static readonly Color Ink=LocalProfile.Hex("143B50"),Gold=LocalProfile.Hex("FFCB53"),Teal=LocalProfile.Hex("4CDED6"),Coral=LocalProfile.Hex("FF7659");
  public void Tick(FishingPlayerRecord p,double now){
   if(!fishIcon){var node=new GameObject("Fish pull marker",typeof(RectTransform),typeof(Image));node.transform.SetParent(transform,false);fishIcon=node.GetComponent<Image>();fishIcon.sprite=Resources.Load<Sprite>("FishingUI/FishBadge");fishIcon.raycastTarget=false;fishIcon.preserveAspect=true;fishIcon.rectTransform.sizeDelta=new Vector2(36,29);}
   pull=Mathf.Lerp(pull,FishingState.Pull(p,now),1-Mathf.Exp(-12*Time.unscaledDeltaTime));
   fishIcon.rectTransform.anchoredPosition=new Vector2(pull*rectTransform.rect.width*.4f,0);
   steer=Mathf.Lerp(steer,p.held?p.steer:0,1-Mathf.Exp(-14*Time.unscaledDeltaTime));
   requiredSide=FishingState.CounterSide(p,now);resting=!p.held;wrong=p.held&&p.steer!=requiredSide;danger=p.tension>.82f;
   beat=MenuPreferences.ReducedMotion?0:Mathf.Sin(Time.unscaledTime*7);SetVerticesDirty();
  }
  public float MarkerPosition=>pull;
  protected override void OnPopulateMesh(VertexHelper v){
   v.Clear();var r=rectTransform.rect;BasketballPlate.Draw(v,r,Ink,Vector2.one*18,2,0,0,0,Ink);float reach=r.width*.40f,y=r.center.y;
   BasketballUIMesh.Line(v,new Vector2(-reach,y),new Vector2(reach,y),new Color(.65f,.85f,.9f,.35f),9);
   BasketballUIMesh.Line(v,new Vector2(-reach,y-5),new Vector2(-reach,y+5),Ink,3);
   BasketballUIMesh.Line(v,new Vector2(reach,y-5),new Vector2(reach,y+5),Ink,3);
   var zone=new Vector2(requiredSide*reach*.7f,y);var cue=danger?Coral:Gold;
   BasketballUIMesh.Line(v,zone-Vector2.right*reach*.22f,zone+Vector2.right*reach*.22f,cue,13+beat);
   float x=pull*reach;BasketballUIMesh.Dot(v,new Vector2(x,y),10,Ink);BasketballUIMesh.Dot(v,new Vector2(x,y),7,Teal);
   var mark=new Vector2(steer*reach*.7f,y-10);var tint=wrong||danger?Coral:resting?Ink:Gold;
   BasketballUIMesh.Line(v,mark-Vector2.right*5,mark+Vector2.up*6,tint,3);
   BasketballUIMesh.Line(v,mark+Vector2.up*6,mark+Vector2.right*5,tint,3);
  }
 }
 // A touch-through, texture-free arrow points at the one relevant control.
 public sealed class FishingTapCue:MaskableGraphic {
  public bool ready;
  void Update(){SetVerticesDirty();}
  protected override void OnPopulateMesh(VertexHelper v){
   v.Clear();var r=rectTransform.rect;float size=Mathf.Min(r.width,r.height),bounce=MenuPreferences.ReducedMotion||!ready?0:Mathf.Sin(Time.unscaledTime*6)*size*.045f;
   var tip=r.center+Vector2.up*(bounce-size*.22f);Color ink=LocalProfile.Hex("143B50"),tint=ready?LocalProfile.Hex("FFCB53"):Color.white;
   if(!ready){ink.a=.3f;tint.a=.3f;}
   for(int pass=0;pass<2;pass++){float width=pass==0?10:5;var c=pass==0?ink:tint;BasketballUIMesh.Line(v,tip+Vector2.up*size*.5f,tip,c,width);BasketballUIMesh.Line(v,tip+new Vector2(-size*.2f,size*.2f),tip,c,width);BasketballUIMesh.Line(v,tip,tip+new Vector2(size*.2f,size*.2f),c,width);}
  }
 }
}
