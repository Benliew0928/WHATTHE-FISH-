using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

namespace WhatTheFish {
 [DefaultExecutionOrder(-100)]
 public sealed class PlayerView:MonoBehaviour,IPlayerCommandSource {
  public static Vector2 LookDelta; public static PlayerView Instance; public Athlete target; public TouchPad stick; public int mode; public float yaw=0,pitch=16; public bool active; Camera cam;bool pendingTackle,pendingJump,pendingShoot;
  void Awake(){Instance=this;cam=GetComponent<Camera>();mode=Mathf.Clamp(PlayerPrefs.GetInt("camera",1),0,2);}
  public void RequestTackle(){if(active&&target&&target.TackleReady)pendingTackle=true;}
  bool pendingKick;float pendingCharge,chargeStarted;int chargeSource;
  public bool Charging {get;private set;}
  MeshRenderer kickAim;Material kickAimMaterial;Mesh kickAimMesh;
  public float Charge=>Charging&&FootballBall.Instance?FootballBall.Instance.ChargeFraction(Time.time-chargeStarted):0;
  public void RequestKick(float charge=1){if(active&&target&&target.KickReady){pendingKick=true;pendingCharge=Mathf.Clamp01(charge);}}
  public bool BeginKick(int source=int.MinValue){if(Charging||!active||!target||!target.KickReady)return false;Charging=true;chargeSource=source;chargeStarted=Time.time;return true;}
  public void EndKick(int source=int.MinValue){if(!Charging||source!=chargeSource)return;float charge=Charge;Charging=false;HideKickAim();RequestKick(charge);}
  public void CancelKick(int source){if(Charging&&source==chargeSource){Charging=false;HideKickAim();}}
  public void ClearMatchInput(){CancelInput();}
  void CancelInput(){Charging=false;HideKickAim();pendingKick=false;pendingTackle=pendingJump=pendingShoot=false;}
  void HideKickAim(){if(kickAim)kickAim.enabled=false;}
  void OnDestroy(){if(kickAimMaterial)Destroy(kickAimMaterial);if(kickAimMesh)Destroy(kickAimMesh);}
  static Mesh CreateKickAimMesh(){
   const float rounding=.09f,headLengthMultiplier=.8f;
   var corners=new[]{new Vector3(-.22f,0,0),new Vector3(.22f,0,0),new Vector3(.22f,0,.56f),new Vector3(.5f,0,.56f),new Vector3(0,0,1),new Vector3(-.5f,0,.56f),new Vector3(-.22f,0,.56f)};
   var outline=new List<Vector3>();
   // Round every corner, including the tip and the inward shaft/head joins.
   for(int i=0;i<corners.Length;i++){
    var corner=corners[i];var previous=corners[(i+corners.Length-1)%corners.Length];var next=corners[(i+1)%corners.Length];
    var entry=corner+(previous-corner).normalized*rounding;var exit=corner+(next-corner).normalized*rounding;
    for(int step=0;step<=6;step++){float t=step/6f;outline.Add(entry*((1-t)*(1-t))+corner*(2*(1-t)*t)+exit*(t*t));}
   }
   // Normalize the rounded shape, then shorten only the head beyond its shoulder.
   var bounds=new Bounds(outline[0],Vector3.zero);foreach(var point in outline)bounds.Encapsulate(point);
   var vertices=new Vector3[outline.Count+1];var normals=new Vector3[vertices.Length];
   vertices[0]=new Vector3(0,0,(.56f-bounds.min.z)/bounds.size.z);
   for(int i=0;i<outline.Count;i++){
    var p=outline[i];float z=(p.z-bounds.min.z)/bounds.size.z;
    if(z>vertices[0].z)z=vertices[0].z+(z-vertices[0].z)*headLengthMultiplier;
    vertices[i+1]=new Vector3((p.x-bounds.center.x)/bounds.size.x,0,z);
   }
   for(int i=0;i<normals.Length;i++)normals[i]=Vector3.up;
   var triangles=new List<int>();
   // The shoulder centre sees the entire outline; one fan avoids alpha overlap.
   for(int i=0;i<outline.Count;i++){
    int current=i+1,next=(i+1)%outline.Count+1;
    if(Vector3.Cross(vertices[next]-vertices[0],vertices[current]-vertices[0]).y<=.0000001f)continue;
    triangles.Add(0);triangles.Add(next);triangles.Add(current);
   }
   var mesh=new Mesh{name="Rounded solid kick arrow"};mesh.vertices=vertices;mesh.triangles=triangles.ToArray();mesh.normals=normals;
   mesh.RecalculateBounds();return mesh;
  }
  void UpdateKickAim(){
   var ball=FootballBall.Instance;
   if(!Charging||!active||!target||!target.KickReady||!ball||!FootballBall.Allowed){HideKickAim();return;}
   if(!kickAim){
    var source=Resources.Load<Material>("FootballKickAim");if(!source)return;
    // An authored transparent material keeps its shader variant in player builds.
    kickAimMaterial=new Material(source){name="Kick aim"};
    var go=new GameObject("Kick aim");go.layer=2;go.transform.SetParent(transform,false);
    kickAimMesh=CreateKickAimMesh();go.AddComponent<MeshFilter>().sharedMesh=kickAimMesh;
    kickAim=go.AddComponent<MeshRenderer>();kickAim.sharedMaterial=kickAimMaterial;
    kickAim.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;kickAim.receiveShadows=false;
   }
   var direction=FootballBall.KickDirection(target);
   float charge=Charge;
   kickAimMaterial.SetColor("_BaseColor",charge>=1?new Color(1,0,0,.5f):new Color(1,1,1,.5f));
   kickAim.transform.SetPositionAndRotation(ball.transform.position+Vector3.up*.06f,Quaternion.LookRotation(direction));
   kickAim.transform.localScale=new Vector3(.85f*.8f,1,Mathf.Lerp(1.6f,1.6f*1.8f,charge));
   kickAim.enabled=true;
  }
  void OnDisable(){CancelInput();if(Instance==this)Instance=null;}
  void OnEnable(){Instance=this;}
  void OnApplicationFocus(bool focus){if(!focus)CancelInput();}
  void OnApplicationPause(bool paused){if(paused)CancelInput();}
  public void RequestJump(){if(active&&target&&target.CanRequestJump)pendingJump=true;}
  public void RequestShoot(){if(active&&target&&BasketballBall.Active&&BasketballBall.Active.CanShoot(target))pendingShoot=true;}
  void Update(){
   if(!active||!target||target.inTransit||FootballMatch.BlocksActions){CancelInput();return;}
   if(!FootballBall.Allowed){Charging=false;pendingKick=false;}
   if(Charging&&!target.KickReady)Charging=false;
   if(Input.GetKeyDown(KeyCode.Space)&&target.CanRequestJump){Charging=false;RequestJump();}
   if(Input.GetKeyDown(KeyCode.E)){Charging=false;RequestTackle();RequestShoot();}
   if(Input.GetKeyDown(KeyCode.F))BeginKick();if(Input.GetKeyUp(KeyCode.F))EndKick();
  }
  public void Switch(){mode=(mode+1)%3;PlayerPrefs.SetInt("camera",mode);PlayerPrefs.Save();}
  public PlayerCommand ReadCommand(){
   if(FootballMatch.BlocksMovement){CancelInput();return default;}
   if(FootballMatch.BlocksActions)CancelInput();
   bool tackle=pendingTackle,jump=pendingJump,shoot=pendingShoot,kick=pendingKick;float charge=pendingCharge;pendingTackle=pendingJump=pendingShoot=false;pendingKick=false;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
   if(DevelopmentProbe.TurnCommandActive){var command=DevelopmentProbe.TurnCommand;command.tackle|=tackle;command.jump|=jump;command.shoot|=shoot;command.charging|=Charging;if(kick){command.kick=true;command.kickCharge=charge;}return command;}
   if(DevelopmentProbe.IslandDriving)return new PlayerCommand{move=Vector2.up,heading=DevelopmentProbe.IslandHeading,sprint=true};
   if(DevelopmentProbe.Driving)return new PlayerCommand{move=Vector2.up,heading=DevelopmentProbe.Heading,sprint=false};
#endif
   var touch=stick?stick.value:Vector2.zero;var v=touch+new Vector2(Input.GetAxisRaw("Horizontal"),Input.GetAxisRaw("Vertical"));return new PlayerCommand{move=Vector2.ClampMagnitude(v,1),heading=yaw,tackle=tackle,jump=jump,shoot=shoot,kick=kick,kickCharge=charge,charging=Charging,sprint=Input.GetKey(KeyCode.LeftShift)||touch.magnitude>.8f};}
  void LateUpdate(){
   if(!active||!target){HideKickAim();return;}
   if(Input.GetKeyDown(KeyCode.C))Switch();
   if(Input.GetMouseButton(1)){LookDelta+=new Vector2(Input.GetAxis("Mouse X")*14,Input.GetAxis("Mouse Y")*14);}
   yaw+=LookDelta.x*.13f;pitch=Mathf.Clamp(pitch-LookDelta.y*.10f,-25,65);LookDelta=Vector2.zero;
   if(SkySailWorld.Instance&&SkySailWorld.Instance.RideCamera(cam,yaw,pitch,mode)){HideKickAim();return;}
   Vector3 focus=target.transform.position+Vector3.up*(mode==0?1.57f:1.35f);
   var rotation=Quaternion.Euler(mode==2?58:pitch,yaw,0);float distance=mode==0?0:mode==1?5:AppRoot.Instance.environments.Current.elevatedDistance;
   Vector3 desired=focus-rotation*Vector3.forward*distance;
   if(distance>0&&Physics.SphereCast(focus,.2f,(desired-focus).normalized,out var hit,distance,1<<8,QueryTriggerInteraction.Ignore))desired=focus+(desired-focus).normalized*Mathf.Max(.35f,hit.distance-.15f);
   target.HideHead(mode==0||Vector3.Distance(desired,focus)<1.15f);
   cam.nearClipPlane=mode==0?.06f:.15f;transform.SetPositionAndRotation(desired,rotation);
   UpdateKickAim();
  }
 }
}
