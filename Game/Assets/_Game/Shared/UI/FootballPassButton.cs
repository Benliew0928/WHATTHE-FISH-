using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace WhatTheFish {
 public sealed class FootballPassButton:MonoBehaviour,IPointerDownHandler,IPointerUpHandler,ICancelHandler {
  public Button button;public Text label;
  public bool Tracking=>false;
  void Update()=>Refresh();
  public void Refresh(){var v=PlayerView.Instance;button.interactable=v&&v.active&&!v.InputBlocked&&!v.Charging&&FootballBall.Instance&&FootballBall.Instance.CanPass(v.target);}
  public void OnPointerDown(PointerEventData e){if(e.button==PointerEventData.InputButton.Left&&button.interactable)PlayerView.Instance.BeginFootballPass(e.pointerId);}
  public void OnPointerUp(PointerEventData e){}
  public void OnCancel(BaseEventData e){}
  public void Cancel(){}
 }
 // Only the selected teammate halo belongs to the local HUD. The flight ribbon
 // is attached to the ball and visible to every participant and spectator.
 public sealed class FootballPassMarker:MaskableGraphic {
  Vector3 endpoint;bool tracking;
  public Vector3 DisplayedEndpoint=>endpoint;
  public float LockBlend=>tracking?1:0;
  public bool PathVisible=>FootballBall.Instance&&FootballBall.Instance.GetComponent<PassFlightVisual>() is PassFlightVisual flight&&flight.Visible;
  Camera EventCamera {get{var canvas=GetComponentInParent<Canvas>();return canvas&&canvas.renderMode!=RenderMode.ScreenSpaceOverlay?canvas.worldCamera:null;}}
  bool Project(Camera camera,Vector3 world,out Vector2 point){var screen=camera.WorldToScreenPoint(world);RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform,screen,EventCamera,out point);return screen.z>camera.nearClipPlane;}
  protected override void OnPopulateMesh(VertexHelper v){
   v.Clear();var camera=Camera.main;if(!tracking||!camera)return;
   for(int i=0;i<64;i++){
    float a=i*Mathf.PI/32,b=(i+1)*Mathf.PI/32;
    if(Project(camera,endpoint+new Vector3(Mathf.Cos(a),0,Mathf.Sin(a))*.65f,out var x)&&Project(camera,endpoint+new Vector3(Mathf.Cos(b),0,Mathf.Sin(b))*.65f,out var y))BasketballUIMesh.Line(v,x,y,CoveUI.Mint,2);
   }
  }
  void LateUpdate(){
   var view=PlayerView.Instance;var target=view?view.FootballPassTarget:null;
   tracking=view&&view.active&&!view.InputBlocked&&target&&!FootballReplay.Presenting;
   if(tracking)endpoint=target.transform.position+Vector3.up*.06f;
   SetVerticesDirty();
  }
  protected override void OnDisable(){tracking=false;SetVerticesDirty();base.OnDisable();}
 }
}
