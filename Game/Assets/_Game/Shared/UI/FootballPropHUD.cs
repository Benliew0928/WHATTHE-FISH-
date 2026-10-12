using UnityEngine;
using UnityEngine.UI;

namespace WhatTheFish {
 [DefaultExecutionOrder(95)]
 public sealed class FootballPropHUD:MonoBehaviour {
  public static FootballPropHUD Instance {get;private set;}
  public bool Placing {get;private set;}public bool PlacementValid {get;private set;}public FootballWallState Preview {get;private set;}
  Button button,cancel;Image icon;Text caption,badge,hint,effects;CanvasGroup group;BasketballPropProgress progress;uint previewRevision;bool awaiting;float requestAt;string rejection;float rejectedUntil;static Sprite[] icons;
  public static Sprite Icon(SportsProp kind){if(!FootballPropRules.Kind(kind))return null;if(icons==null){icons=new Sprite[3];var t=Resources.Load<Texture2D>("FootballProps/Icons");if(t)for(int i=0;i<3;i++)icons[i]=Sprite.Create(t,new Rect(i*128+4,4,120,120),Vector2.one*.5f,100,0,SpriteMeshType.FullRect);}return icons[(int)kind-2];}
  public static void Create(Transform parent){var r=BasketballUIStyle.Center(parent,"Football ability HUD",new Vector2(80,80),new Vector2(-292,87),new Vector2(1,0));r.gameObject.AddComponent<FootballPropHUD>().Build();}
  void Build(){
   Instance=this;group=gameObject.AddComponent<CanvasGroup>();var frame=gameObject.AddComponent<BasketballPlate>();frame.radius=Vector2.one*27;frame.border=2;frame.edge=CoveUI.Cream;frame.inset=2;frame.insetColor=CoveUI.Gold;frame.shadow=4;frame.color=CoveUI.Ink;frame.raycastTarget=true;
   button=gameObject.AddComponent<Button>();button.targetGraphic=frame;button.transition=Selectable.Transition.None;button.navigation=new Navigation{mode=Navigation.Mode.None};button.onClick.AddListener(Activate);gameObject.AddComponent<BasketballButtonFeedback>();
   icon=CoveUI.Image(transform,"Football prop illustration",Icon(SportsProp.Wall),9,4,62,47);caption=BasketballUIStyle.Text(transform,"",2,54,76,20,12,true,TextAnchor.MiddleCenter,CoveUI.Cream);
   var plate=BasketballUIStyle.Plate(transform,"Football prop uses",59,-5,24,24,CoveUI.Gold,12,2);badge=BasketballUIStyle.Text(plate.transform,"1",0,0,24,24,12,true,TextAnchor.MiddleCenter);
   progress=CoveUI.Rect(transform,"Football ability duration",0,0,80,80).gameObject.AddComponent<BasketballPropProgress>();progress.raycastTarget=false;progress.color=CoveUI.Gold;
   cancel=FootballUIStyle.Button(transform,"Cancel wall placement","Cancel",5,-51,70,30,Cancel,FootballUIStyle.Cancel,11);cancel.gameObject.SetActive(false);
   hint=BasketballUIStyle.Text(transform,"",-58,-84,196,32,10,true,TextAnchor.MiddleCenter,CoveUI.Cream);hint.raycastTarget=false;
   var status=BasketballUIStyle.Center(transform.parent,"Football team ability timers",new Vector2(260,22),new Vector2(0,-89),new Vector2(.5f,1));effects=BasketballUIStyle.Text(status,"",0,0,260,22,11,true,TextAnchor.MiddleCenter,CoveUI.Cream);effects.raycastTarget=false;
  }
  public void Activate(){
   var ball=FootballBall.Instance;var view=PlayerView.Instance;var actor=AppRoot.Instance?AppRoot.Instance.LocalAthlete:null;
   if(!ball||!actor||!view||view.InputBlocked||view.Charging||awaiting)return;
   string reason=ball.PropUnavailable(actor);if(reason!=""){Feedback(reason);return;}
   var slot=ball.PropSlot(actor);
   if(slot.kind==SportsProp.Wall&&!Placing){FootballAudio.PlayMenu(FootballSound.WallPreview);Placing=true;previewRevision=slot.revision;return;}
   if(Placing&&(previewRevision!=slot.revision||!ball.TryWallPlacement(actor,view.yaw,out _,out reason))){Feedback(reason!=""?reason:"Pickup changed");return;}
   FootballAudio.PlayMenu();bool network=actor.GetComponent<NetworkAthlete>() is {IsSpawned:true};
   if(ball.RequestProp(actor,view.yaw)){Placing=false;awaiting=network;requestAt=Time.unscaledTime;previewRevision=slot.revision;view.ClearMatchInput();}else Feedback(ball.PropUnavailable(actor));
  }
  public void Cancel(){Placing=false;PlacementValid=false;awaiting=false;}
  public void Rejected(){awaiting=false;Feedback("Cannot use here. Prop kept.");}
  void Feedback(string message){FootballAudio.PlayMenu(FootballSound.UIError);rejection=string.IsNullOrEmpty(message)?"Cannot use here. Prop kept.":message;rejectedUntil=Time.unscaledTime+2;}
  void LateUpdate(){
   var ball=FootballBall.Instance;var view=PlayerView.Instance;var actor=AppRoot.Instance?AppRoot.Instance.LocalAthlete:null;
   if(!ball||!view||!actor){group.alpha=0;group.blocksRaycasts=false;Cancel();return;}
   var slot=ball.PropSlot(actor);float turbo=ball.TurboRemaining(actor);var wall=ball.TeamWall(ball.PropTeam(actor));float wallRemaining=ball.WallActive(wall)?Mathf.Max(0,(float)(wall.until-FootballBall.PropClock)):0;
   bool receiving=ball.PropLocked(actor);var kind=slot.kind!=SportsProp.None?slot.kind:receiving?SportsProp.Portal:SportsProp.None;
   if(awaiting&&(slot.revision!=previewRevision||Time.unscaledTime-requestAt>2))awaiting=false;
   if(Placing&&(!ball.PropsLive||view.InputBlocked||view.Charging||slot.kind!=SportsProp.Wall||slot.revision!=previewRevision||actor.Action!=FootballAction.None||actor.inTransit||actor.Airborne))Cancel();
   string reason=ball.PropUnavailable(actor);if(Placing){PlacementValid=ball.TryWallPlacement(actor,view.yaw,out var pose,out reason);if(!pose.active)pose=new FootballWallState{position=actor.transform.position+Quaternion.Euler(0,view.yaw,0)*Vector3.forward*2,heading=view.yaw};Preview=pose;}
   bool visible=ball.PropsLive&&kind!=SportsProp.None&&!view.Charging&&!view.InputBlocked;group.alpha=visible?1:0;group.blocksRaycasts=visible;
   button.interactable=visible&&!awaiting&&!receiving&&(Placing?PlacementValid:reason=="");icon.sprite=Icon(kind==SportsProp.None?SportsProp.Wall:kind);icon.color=button.interactable?Color.white:new Color(1,1,1,.65f);
   caption.text=Placing?"Place":receiving?"Receiving":kind==SportsProp.Turbo?"Turbo":kind==SportsProp.Portal?"Portal":"Wall";badge.text=awaiting?"...":"1";
   cancel.gameObject.SetActive(Placing&&visible);hint.text=Time.unscaledTime<rejectedUntil?rejection:Placing?(PlacementValid?"Aim, then tap Place":reason):reason!="Find a touchline prop"?reason:"";
   progress.Set(receiving?Mathf.Clamp01((float)(FootballBall.PropClock-ball.Props.portalAt)/SportsAbilityRules.PortalSeconds):0);
   effects.text=ball.PropsLive?(turbo>0?"TEAM TURBO  "+Mathf.CeilToInt(turbo)+"s":"")+(turbo>0&&wallRemaining>0?"     ":"")+(wallRemaining>0?"TEAM WALL  "+Mathf.CeilToInt(wallRemaining)+"s":""):"";
   if(Input.GetKeyDown(KeyCode.G))Activate();if(Placing&&Input.GetKeyDown(KeyCode.Escape)){FootballAudio.PlayMenu(FootballSound.UIBack);Cancel();}
  }
  void OnApplicationFocus(bool value){if(!value)Cancel();}void OnApplicationPause(bool value){if(value)Cancel();}
  void OnEnable(){Instance=this;}
  void OnDisable(){Cancel();if(effects)effects.text="";if(Instance==this)Instance=null;}
 }
}
