using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace WhatTheFish {
 public sealed class FootballPassButton:MonoBehaviour,IPointerDownHandler,IPointerUpHandler,ICancelHandler {
  public Button button;public Text label;bool held;int pointer;
  public bool Tracking=>held;
  void Update()=>Refresh();
  public void Refresh(){var v=PlayerView.Instance;button.interactable=v&&v.active&&!v.InputBlocked&&!v.Charging&&FootballBall.Instance&&FootballBall.Instance.CanPass(v.target);if(held&&(!button.interactable||!v.FootballPassAiming))Cancel();}
  public void OnPointerDown(PointerEventData e){if(e.button==PointerEventData.InputButton.Left&&!held&&button.interactable&&PlayerView.Instance.BeginFootballPass(e.pointerId)){held=true;pointer=e.pointerId;GetComponent<FootballActionSwipe>()?.Capture(e);}}
  public void OnPointerUp(PointerEventData e){if(!held||e.pointerId!=pointer)return;if(KickButton.CanceledTouch(pointer)){Cancel();return;}held=false;PlayerView.Instance?.EndFootballPass(pointer);}
  public void OnCancel(BaseEventData e)=>Cancel();
  public void Cancel(){if(held)PlayerView.Instance?.CancelFootballPass();held=false;}
  void OnDisable()=>Cancel();void OnApplicationFocus(bool focus){if(!focus)Cancel();}void OnApplicationPause(bool pause){if(pause)Cancel();}
 }
 // Screen-space receiver marker follows all three football cameras.
 public sealed class FootballPassMarker:MaskableGraphic {
  Text receiver;BasketballPlate receiverPlate;
  Camera EventCamera {get{var canvas=GetComponentInParent<Canvas>();return canvas&&canvas.renderMode!=RenderMode.ScreenSpaceOverlay?canvas.worldCamera:null;}}
  bool Project(Camera camera,Vector3 world,out Vector2 point){var screen=camera.WorldToScreenPoint(world);RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform,screen,EventCamera,out point);return screen.z>camera.nearClipPlane;}
  protected override void OnPopulateMesh(VertexHelper v){
   v.Clear();var view=PlayerView.Instance;var target=view?view.FootballPassTarget:null;var camera=Camera.main;
   if(!view||!view.FootballPassAiming||!view.target||!camera||FootballReplay.Presenting)return;
   var start=FootballBall.Instance?FootballBall.Instance.transform.position:view.target.transform.position+Vector3.up*.12f;
   var end=target?target.transform.position+Vector3.up*.12f:view.target.transform.position+Quaternion.Euler(0,view.yaw,0)*Vector3.forward*12+Vector3.up*.12f;
   var color=target?CoveUI.Mint:CoveUI.Cream;
   // Sample in world space so the ground-pass guide follows perspective in all cameras.
   for(int i=0;i<40;i++){if(i%5==4)continue;if(Project(camera,Vector3.Lerp(start,end,i/40f),out var a)&&Project(camera,Vector3.Lerp(start,end,(i+1)/40f),out var b)){BasketballUIMesh.Line(v,a,b,CoveUI.Ink,7);BasketballUIMesh.Line(v,a,b,color,4);}}
   if(!Project(camera,end,out var p))return;
   if(Project(camera,Vector3.Lerp(start,end,.88f),out var before)){var direction=(p-before).normalized;var side=new Vector2(-direction.y,direction.x);BasketballUIMesh.Line(v,p-direction*13+side*7,p,color,4);BasketballUIMesh.Line(v,p-direction*13-side*7,p,color,4);}
   for(int i=0;i<48;i++){float a=i*Mathf.PI/24,b=(i+1)*Mathf.PI/24;var x=p+new Vector2(Mathf.Cos(a)*24,Mathf.Sin(a)*9);var y=p+new Vector2(Mathf.Cos(b)*24,Mathf.Sin(b)*9);BasketballUIMesh.Line(v,x,y,CoveUI.Ink,6);BasketballUIMesh.Line(v,x,y,color,3);}
   if(!target||!Project(camera,target.transform.position+Vector3.up*2.5f,out var q))return;
   BasketballUIMesh.Line(v,q+new Vector2(-7,7),q,CoveUI.Mint,3);BasketballUIMesh.Line(v,q+new Vector2(7,7),q,CoveUI.Mint,3);
  }
  void LateUpdate(){
   if(!receiver){receiverPlate=BasketballUIStyle.Plate(transform,"Locked pass receiver",0,0,114,24,CoveUI.Mint,8,1.5f,true);receiverPlate.rectTransform.anchorMin=receiverPlate.rectTransform.anchorMax=rectTransform.pivot;receiverPlate.rectTransform.pivot=Vector2.one*.5f;receiver=BasketballUIStyle.Text(receiverPlate.transform,"",0,0,114,24,12,true,TextAnchor.MiddleCenter,CoveUI.Ink);receiver.raycastTarget=false;}
   var view=PlayerView.Instance;var target=view?view.FootballPassTarget:null;var camera=Camera.main;
   bool visible=target&&camera&&!FootballReplay.Presenting;Vector2 p=default;if(visible)visible=Project(camera,target.transform.position+Vector3.up*.12f,out p);
   receiver.enabled=visible;receiverPlate.gameObject.SetActive(visible);if(visible){var identity=target.GetComponent<CharacterIdentity>();receiver.text=identity?"LOCKED  #"+identity.Number.ToString("00"):"TEAMMATE LOCKED";receiverPlate.rectTransform.anchoredPosition=p+Vector2.down*26;}
   SetVerticesDirty();
  }
 }
}
