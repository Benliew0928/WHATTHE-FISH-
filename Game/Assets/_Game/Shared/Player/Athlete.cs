using UnityEngine;

namespace SportsPrototype {
 public sealed class Athlete : MonoBehaviour {
  public CharacterController capsule; public Transform visual; public bool controlled; public float speed; float gravity; Animator animator;
  public void Setup(){capsule=GetComponent<CharacterController>();animator=GetComponentInChildren<Animator>();}
  public void Simulate(PlayerCommand command,float dt){
   if(capsule==null)Setup(); if(!capsule.enabled)return;
   var move=Vector2.ClampMagnitude(command.move,1);Vector3 direction=Quaternion.Euler(0,command.heading,0)*new Vector3(move.x,0,move.y);
   speed=direction.magnitude*(command.sprint?7:4);
   if(capsule.isGrounded)gravity=-2;else gravity=Mathf.Max(-25,gravity-22*dt);
   capsule.Move((direction*(command.sprint?7:4)+Vector3.up*gravity)*dt);
   if(direction.sqrMagnitude>.01f)transform.rotation=Quaternion.Slerp(transform.rotation,Quaternion.LookRotation(direction),dt*12);
   if(transform.position.y<-10){capsule.enabled=false;transform.position=new Vector3(0,1,0);capsule.enabled=true;}
  }
  void Update(){if(animator)animator.SetFloat("Speed",speed);}
  public void Appearance(CharacterAppearance data){
   data.Clamp();Color[] skin={LocalProfile.Hex("F5C5A0"),LocalProfile.Hex("D69C73"),LocalProfile.Hex("A96E4B"),LocalProfile.Hex("6E4433")};
   Color[] hair={LocalProfile.Hex("33282D"),LocalProfile.Hex("805039"),LocalProfile.Hex("E4BC71"),LocalProfile.Hex("7968AD")};
   foreach(var r in GetComponentsInChildren<Renderer>(true)){
    r.gameObject.SetActive(!r.name.StartsWith("HairTuft")||data.hairstyle==1);
    foreach(var m in r.materials){var n=m.name;if(n.StartsWith("Skin"))m.color=skin[data.skin];if(n.StartsWith("Hair"))m.color=hair[data.hair];if(n.StartsWith("Jersey"))m.color=LocalProfile.Teams[data.outfit];}
   }
  }
  public void HideHead(bool hidden){foreach(var r in GetComponentsInChildren<Renderer>(true)){string n=r.name;bool head=n.StartsWith("Head")||n.StartsWith("Hair")||n.StartsWith("Eye")||n.StartsWith("Ear")||n.StartsWith("Cheek")||n.StartsWith("Nose")||n.StartsWith("Smile");if(head)r.enabled=!hidden;}}
 }
}
