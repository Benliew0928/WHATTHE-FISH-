using UnityEngine;
using UnityEngine.EventSystems;

namespace WhatTheFish {
 public sealed class PlayerView:MonoBehaviour,IPlayerCommandSource {
  public static Vector2 LookDelta; public static PlayerView Instance; public Athlete target; public TouchPad stick; public int mode; public float yaw=0,pitch=16; public bool active; Camera cam;bool pendingTackle;
  void Awake(){Instance=this;cam=GetComponent<Camera>();mode=Mathf.Clamp(PlayerPrefs.GetInt("camera",1),0,2);}
  public void RequestTackle(){if(active&&target&&target.TackleReady)pendingTackle=true;}
  void Update(){if(!active||!FootballTackle.Allowed){pendingTackle=false;return;}if(Input.GetKeyDown(KeyCode.Space))RequestTackle();}
  public void Switch(){mode=(mode+1)%3;PlayerPrefs.SetInt("camera",mode);PlayerPrefs.Save();}
  public PlayerCommand ReadCommand(){
   bool tackle=pendingTackle;pendingTackle=false;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
   if(DevelopmentProbe.TurnCommandActive){var command=DevelopmentProbe.TurnCommand;command.tackle|=tackle;return command;}
   if(DevelopmentProbe.IslandDriving)return new PlayerCommand{move=Vector2.up,heading=DevelopmentProbe.IslandHeading,sprint=true};
   if(DevelopmentProbe.Driving)return new PlayerCommand{move=Vector2.up,heading=DevelopmentProbe.Heading,sprint=false};
#endif
   var touch=stick?stick.value:Vector2.zero;var v=touch+new Vector2(Input.GetAxisRaw("Horizontal"),Input.GetAxisRaw("Vertical"));return new PlayerCommand{move=Vector2.ClampMagnitude(v,1),heading=yaw,tackle=tackle,sprint=Input.GetKey(KeyCode.LeftShift)||touch.magnitude>.8f};}
  void LateUpdate(){
   if(!active||!target)return;
   if(Input.GetKeyDown(KeyCode.C))Switch();
   if(Input.GetMouseButton(1)){LookDelta+=new Vector2(Input.GetAxis("Mouse X")*14,Input.GetAxis("Mouse Y")*14);}
   yaw+=LookDelta.x*.13f;pitch=Mathf.Clamp(pitch-LookDelta.y*.10f,-25,65);LookDelta=Vector2.zero;
   Vector3 focus=target.transform.position+Vector3.up*(mode==0?1.57f:1.35f);
   var rotation=Quaternion.Euler(mode==2?58:pitch,yaw,0);float distance=mode==0?0:mode==1?5:AppRoot.Instance.environments.Current.elevatedDistance;
   Vector3 desired=focus-rotation*Vector3.forward*distance;
   if(distance>0&&Physics.SphereCast(focus,.2f,(desired-focus).normalized,out var hit,distance,1<<8,QueryTriggerInteraction.Ignore))desired=focus+(desired-focus).normalized*Mathf.Max(.35f,hit.distance-.15f);
   target.HideHead(mode==0||Vector3.Distance(desired,focus)<1.15f);
   cam.nearClipPlane=mode==0?.06f:.15f;transform.SetPositionAndRotation(desired,rotation);
  }
 }
}
