using UnityEngine;
using UnityEngine.UI;

namespace WhatTheFish {
 [DefaultExecutionOrder(90)]
 public sealed class BasketballPropHUD:MonoBehaviour {
  Button button;Image icon;Text badge,caption;Sprite[] icons;BasketballPropProgress progress;CanvasGroup group;
  public bool GestureHidden;
  public static BasketballPropHUD Create(Transform parent){var r=BasketballUIStyle.Center(parent,"Basketball ability HUD",Vector2.one*80,new Vector2(-177,213),new Vector2(1,0));var hud=r.gameObject.AddComponent<BasketballPropHUD>();hud.Build();return hud;}
  void Build(){
   group=gameObject.AddComponent<CanvasGroup>();var frame=gameObject.AddComponent<BasketballPlate>();frame.radius=Vector2.one*27;frame.border=2;frame.edge=CoveUI.Cream;frame.inset=2;frame.insetColor=CoveUI.Gold;frame.shadow=4;frame.color=CoveUI.Ink;frame.raycastTarget=true;
   button=gameObject.AddComponent<Button>();button.targetGraphic=frame;button.transition=Selectable.Transition.None;button.navigation=new Navigation{mode=Navigation.Mode.None};button.onClick.AddListener(()=>{BasketballAudio.PlayMenu();Activate();});gameObject.AddComponent<BasketballButtonFeedback>();
   icon=CoveUI.Image(transform,"Prop illustration",null,10,6,60,45);caption=BasketballUIStyle.Text(transform,"",4,56,72,20,12,true,TextAnchor.MiddleCenter,CoveUI.Cream);
   var count=BasketballUIStyle.Plate(transform,"One use",58,-5,25,25,CoveUI.Gold,12.5f,2);badge=BasketballUIStyle.Text(count.transform,"1",0,0,25,25,13,true,TextAnchor.MiddleCenter);
   progress=CoveUI.Rect(transform,"Ability progress",0,0,80,80).gameObject.AddComponent<BasketballPropProgress>();progress.raycastTarget=false;progress.color=CoveUI.Gold;
   icons=new[]{null,Resources.Load<Sprite>("BasketballProps/LockOn"),Resources.Load<Sprite>("BasketballProps/Turbo"),Resources.Load<Sprite>("BasketballProps/Portal")};
  }
  public void Activate(){var ball=BasketballBall.Active;var actor=AppRoot.Instance?AppRoot.Instance.LocalAthlete:null;var view=PlayerView.Instance;if(view&&!view.InputBlocked&&ball&&ball.PropUnavailable(actor)=="")ball.RequestProp(actor);}
  void LateUpdate(){
   var ball=BasketballBall.Active;var actor=AppRoot.Instance?AppRoot.Instance.LocalAthlete:null;if(!ball||!actor)return;var view=PlayerView.Instance;
   var slot=ball.PropSlot(actor);var state=ball.Props;bool aiming=state.aiming&&ball.PropActor(state.aimer)==actor,receiving=state.transferring&&ball.PropActor(state.receiver)==actor;float turbo=ball.TurboRemaining(actor);
   var kind=aiming?SportsProp.LockOn:receiving?SportsProp.Portal:slot.kind!=SportsProp.None?slot.kind:turbo>0?SportsProp.Turbo:SportsProp.None;
   bool visible=kind!=SportsProp.None&&!GestureHidden;group.alpha=visible?1:0;group.blocksRaycasts=visible;button.interactable=view&&!view.InputBlocked&&!view.ShotCharging&&!view.PassCharging&&ball.PropUnavailable(actor)=="";
   icon.sprite=icons[(int)kind];icon.color=button.interactable||aiming||receiving||turbo>0?Color.white:new Color(.7f,.7f,.7f,.8f);
   caption.text=aiming?"Aiming":receiving?"Receiving":kind==SportsProp.LockOn?"Lock-on":kind==SportsProp.Turbo?"Turbo":"Portal";
   badge.text=aiming?Mathf.CeilToInt((float)(state.aimUntil-BasketballMotion.Clock)).ToString():slot.kind==SportsProp.None&&turbo>0?Mathf.CeilToInt(turbo).ToString():"1";
   progress.Set(aiming?Mathf.Clamp01(1-(float)(state.aimUntil-BasketballMotion.Clock)/BasketballPropRules.AimSeconds):receiving?Mathf.Clamp01((float)(BasketballMotion.Clock-state.portalAt)/BasketballPropRules.PortalSeconds):slot.kind==SportsProp.None&&turbo>0?turbo/5:0);
   if(Input.GetKeyDown(KeyCode.G))Activate();
  }
 }
 public sealed class BasketballPropProgress:MaskableGraphic {
  float value;public void Set(float v){if(Mathf.Abs(value-v)<.003f)return;value=v;SetVerticesDirty();}
  protected override void OnPopulateMesh(VertexHelper v){v.Clear();float r=Mathf.Min(rectTransform.rect.width,rectTransform.rect.height)*.47f;var c=rectTransform.rect.center;for(int i=0;i<64&&i/64f<value;i++){float a=i/64f*Mathf.PI*2,b=Mathf.Min((i+1)/64f,value)*Mathf.PI*2;var x=c+new Vector2(Mathf.Sin(a),Mathf.Cos(a))*r;var y=c+new Vector2(Mathf.Sin(b),Mathf.Cos(b))*r;BasketballUIMesh.Line(v,x,y,color,2.5f);}}
 }
}
