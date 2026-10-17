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
 // World-space energy ribbon shares the shooting material; a small halo marks the receiver.
 public sealed class FootballPassMarker:MaskableGraphic {
  const int Steps=48;
  MeshRenderer lane;Mesh mesh;Material laneMaterial;
  readonly Vector3[] vertices=new Vector3[(Steps+1)*2];readonly Vector2[] uv=new Vector2[(Steps+1)*2];
  Vector3 endpoint,velocity;float visibility,lockBlend;bool tracking;
  public Vector3 DisplayedEndpoint=>endpoint;
  public float LockBlend=>lockBlend;
  public bool PathVisible=>lane&&lane.enabled;
  Camera EventCamera {get{var canvas=GetComponentInParent<Canvas>();return canvas&&canvas.renderMode!=RenderMode.ScreenSpaceOverlay?canvas.worldCamera:null;}}
  bool Project(Camera camera,Vector3 world,out Vector2 point){var screen=camera.WorldToScreenPoint(world);RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform,screen,EventCamera,out point);return screen.z>camera.nearClipPlane;}
  void CreateLane(){
   var source=Resources.Load<Material>("FootballKickAim");if(!source)return;
   laneMaterial=new Material(source){name="Football pass energy path"};mesh=new Mesh{name="Football pass ribbon"};mesh.MarkDynamic();
   var indices=new int[Steps*6];for(int i=0;i<Steps;i++){int v=i*2,t=i*6;indices[t]=v;indices[t+1]=v+2;indices[t+2]=v+1;indices[t+3]=v+1;indices[t+4]=v+2;indices[t+5]=v+3;}
   mesh.vertices=vertices;mesh.uv=uv;mesh.triangles=indices;
   var go=new GameObject("Football pass trajectory");go.layer=2;go.AddComponent<MeshFilter>().sharedMesh=mesh;
   lane=go.AddComponent<MeshRenderer>();lane.sharedMaterial=laneMaterial;lane.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;lane.receiveShadows=false;
  }
  protected override void OnPopulateMesh(VertexHelper v){
   v.Clear();var camera=Camera.main;if(!tracking||!camera||visibility<.01f)return;
   // A restrained world-sized landing halo, with no dashed overlay or chunky outline.
   var tint=Color.Lerp(new Color(.25f,.88f,1),CoveUI.Mint,lockBlend);tint.a=visibility*(.35f+.65f*lockBlend);
   float radius=Mathf.Lerp(.48f,.65f,lockBlend);
   for(int i=0;i<64;i++){
    float a=i*Mathf.PI/32,b=(i+1)*Mathf.PI/32;
    if(Project(camera,endpoint+new Vector3(Mathf.Cos(a),0,Mathf.Sin(a))*radius,out var x)&&Project(camera,endpoint+new Vector3(Mathf.Cos(b),0,Mathf.Sin(b))*radius,out var y))BasketballUIMesh.Line(v,x,y,tint,2);
   }
  }
  void LateUpdate(){
   var view=PlayerView.Instance;var ball=FootballBall.Instance;var camera=Camera.main;
   bool active=view&&view.active&&!view.InputBlocked&&view.FootballPassAiming&&view.target&&ball&&ball.CanPass(view.target)&&camera&&!FootballReplay.Presenting;
   if(!active){Hide();return;}
   if(!lane)CreateLane();if(!lane)return;
   var target=view.FootballPassTarget;var start=ball.transform.position+Vector3.up*.06f;
   var desired=target?target.transform.position:view.target.transform.position+Quaternion.Euler(0,view.yaw,0)*Vector3.forward*12;
   desired.y=start.y;
   if(!tracking){endpoint=desired;velocity=Vector3.zero;tracking=true;}
   endpoint=Vector3.SmoothDamp(endpoint,desired,ref velocity,.12f,Mathf.Infinity,Time.unscaledDeltaTime);
   visibility=Mathf.MoveTowards(visibility,1,Time.unscaledDeltaTime*8);
   lockBlend=Mathf.Lerp(lockBlend,target?1:0,1-Mathf.Exp(-14*Time.unscaledDeltaTime));
   var direction=Vector3.ProjectOnPlane(endpoint-start,Vector3.up);float length=Mathf.Max(1,direction.magnitude);direction=direction.sqrMagnitude>.001f?direction.normalized:view.target.transform.forward;
   var side=Vector3.Cross(Vector3.up,direction);
   for(int i=0;i<=Steps;i++){
    float t=Mathf.Lerp(-.12f/length,1+.12f/length,i/(float)Steps);var point=start+direction*(t*length);
    vertices[i*2]=point-side*.67f;vertices[i*2+1]=point+side*.67f;uv[i*2]=new Vector2(-.67f,t);uv[i*2+1]=new Vector2(.67f,t);
   }
   mesh.vertices=vertices;mesh.uv=uv;mesh.RecalculateBounds();laneMaterial.SetFloat("_Length",length);laneMaterial.SetFloat("_Opacity",visibility*.86f);laneMaterial.SetFloat("_Lock",lockBlend);lane.enabled=true;
   SetVerticesDirty();
  }
  void Hide(){tracking=false;visibility=lockBlend=0;velocity=Vector3.zero;if(lane)lane.enabled=false;SetVerticesDirty();}
  protected override void OnDisable(){Hide();base.OnDisable();}
  protected override void OnDestroy(){if(lane)Destroy(lane.gameObject);if(mesh)Destroy(mesh);if(laneMaterial)Destroy(laneMaterial);base.OnDestroy();}
 }
}
