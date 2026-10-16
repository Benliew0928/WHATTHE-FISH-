using System;
using System.Linq;
using UnityEngine;

namespace WhatTheFish {
 [DefaultExecutionOrder(130)]
 public sealed class FishingRodMotion:MonoBehaviour {
  Athlete actor;FishingPresentation presentation;Transform upper,lower,hand,socket,leftUpper,leftLower,leftHand,spine,chest;Transform[] bones;Quaternion[] saved;bool applied;
  GameObject rod;LineRenderer line,floatRing;Vector3 tipOffset,gripOffset;Transform head;Vector3 headScale;bool hiddenHead,closedGrip;float shownSteer,shownPull;SkinnedMeshRenderer[] skins;
  public bool Equipped=>rod&&rod.activeSelf;
  public float GripError {get;private set;}
  public float FightLean {get;private set;}
  public Vector3 Tip=>rod?rod.transform.TransformPoint(tipOffset):transform.position;
  public void Bind(Athlete owner,FishingPresentation source){
   actor=owner;presentation=source;if(bones!=null)return;bones=owner.visual.GetComponentsInChildren<Transform>(true);
   Transform Bone(string n)=>Array.Find(bones,t=>t.name=="mixamorig:"+n);
   upper=Bone("RightArm");lower=Bone("RightForeArm");hand=Bone("RightHand");head=Bone("Head");socket=hand?hand.Find("Golf grip socket"):null;saved=new Quaternion[bones.Length];skins=owner.visual.GetComponentsInChildren<SkinnedMeshRenderer>(true).Where(s=>s.sharedMesh&&s.sharedMesh.GetBlendShapeIndex("GolfRightGrip")>=0).ToArray();
   spine=Bone("Spine");chest=Bone("Spine2");leftUpper=Bone("LeftArm");leftLower=Bone("LeftForeArm");leftHand=Bone("LeftHand");
  }
  void Restore(){if(hiddenHead&&head){head.localScale=headScale;hiddenHead=false;}if(!applied)return;for(int i=0;i<bones.Length;i++)if(bones[i])bones[i].localRotation=saved[i];applied=false;}
  void Update(){Restore();}
  public void Stow(){
   Restore();
   // Release our grip once; the golf presentation owns the same shape in its match.
   if(closedGrip){bool golfGrip=actor&&actor.GolfClubMotion&&actor.GolfClubMotion.ShowBodyInFirstPerson;if(!golfGrip&&skins!=null)foreach(var skin in skins)if(skin&&skin.sharedMesh){int grip=skin.sharedMesh.GetBlendShapeIndex("GolfRightGrip");if(grip>=0)skin.SetBlendShapeWeight(grip,0);}closedGrip=false;}
   if(rod)rod.SetActive(false);if(line)line.enabled=false;if(floatRing)floatRing.enabled=false;
  }
  void OnDisable(){Stow();}
  void OnDestroy(){if(line)Destroy(line.gameObject);if(floatRing)Destroy(floatRing.gameObject);}
  void LateUpdate(){
   var game=FishingGame.Instance;var state=game?game.Player(actor):null;
   if(!presentation||!game||!game.Context||!state.HasValue||!state.Value.connected||actor.inTransit||!hand){Stow();return;}
   var p=state.Value;
   if(!rod){var template=presentation.RodTemplate(p.owner);if(!template)return;rod=FishingPresentation.CloneVisual(template,transform,"Held fishing rod");tipOffset=template.Find("Tip").localPosition;gripOffset=template.Find("Grip").localPosition;line=presentation.Line("Fishing line",transform,.012f);floatRing=presentation.Line("Bite float ring",transform,.025f);floatRing.loop=true;floatRing.positionCount=10;}
   rod.SetActive(true);for(int i=0;i<bones.Length;i++)saved[i]=bones[i].localRotation;applied=true;
   var facing=Quaternion.Euler(0,p.Busy?game.Heading(actor):transform.eulerAngles.y,0);
   FightLean=0;
   if(p.phase==FishingPhase.Reeling){
    float pump=p.held?Mathf.Clamp(-p.velocity,0,2)*2:0;FightLean=7+p.tension*9+p.surge*5+pump;
    if(spine)spine.rotation=Quaternion.AngleAxis(-FightLean,facing*Vector3.right)*Quaternion.AngleAxis(-shownPull*7,facing*Vector3.forward)*spine.rotation;
    if(chest)chest.rotation=Quaternion.AngleAxis(shownSteer*15,facing*Vector3.up)*chest.rotation;
   }
   float elapsed=(float)(game.Now-p.phaseAt);var forward=facing*Vector3.forward;float lift=p.Busy?.4f:-.15f;
   if(p.phase==FishingPhase.Casting){float t=Mathf.Clamp01(elapsed/.65f);lift=Mathf.Lerp(1.45f,.4f,Mathf.SmoothStep(0,1,t));forward=facing*new Vector3(0,0,Mathf.Lerp(-.8f,1,t));}
   shownSteer=Mathf.Lerp(shownSteer,p.phase==FishingPhase.Reeling?p.steer:0,1-Mathf.Exp(-10*Time.deltaTime));shownPull=Mathf.Lerp(shownPull,p.pull,1-Mathf.Exp(-10*Time.deltaTime));
   if(p.phase==FishingPhase.Reeling){forward+=facing*Vector3.right*(shownSteer*.82f+shownPull*.16f);float stroke=(float)FishingState.StrokeSeconds(p.tapGrade);lift+=p.held?Mathf.Sin(Mathf.Clamp01((float)(game.Now-p.reelLease+stroke)/stroke)*Mathf.PI)*.32f:0;}
   if(p.phase==FishingPhase.Reeling)lift+=p.tension*.52f+p.surge*.16f+Mathf.Clamp(-p.fightMotion.y,-1,1)*.055f;
   if(p.phase==FishingPhase.Caught)lift=.75f;
   var direction=(forward+Vector3.up*lift).normalized;var rotation=Quaternion.FromToRotation(Vector3.up,direction)*facing;
   if(p.Busy){
    float cast=p.phase==FishingPhase.Casting?Mathf.Clamp01(elapsed/.65f):1;var wrist=transform.position+facing*new Vector3(.23f,1.12f+(p.phase==FishingPhase.Casting?Mathf.Sin(cast*Mathf.PI)*.28f:0),p.phase==FishingPhase.Casting?Mathf.Lerp(-.06f,.39f,cast):.35f);
    var start=upper.position;var elbow=lower.position;float a=Vector3.Distance(start,elbow),b=Vector3.Distance(elbow,hand.position);var delta=wrist-start;
    float d=Mathf.Clamp(delta.magnitude,Mathf.Abs(a-b)+.002f,a+b-.006f);var axis=delta.normalized;float along=(a*a-b*b+d*d)/(2*d);var bend=Vector3.ProjectOnPlane(transform.right-Vector3.up*.7f,axis).normalized;var joint=start+axis*along+bend*Mathf.Sqrt(Mathf.Max(0,a*a-along*along));
    upper.rotation=Quaternion.FromToRotation(elbow-start,joint-start)*upper.rotation;lower.rotation=Quaternion.FromToRotation(hand.position-lower.position,start+axis*d-lower.position)*lower.rotation;
   }
   // The existing authored closed-grip cavity supplies the rod socket.
   if(socket)hand.rotation=Quaternion.FromToRotation(socket.forward,direction)*hand.rotation;
   foreach(var skin in skins)skin.SetBlendShapeWeight(skin.sharedMesh.GetBlendShapeIndex("GolfRightGrip"),100);closedGrip=true;
   var palm=socket?socket.position:hand.position;rod.transform.SetPositionAndRotation(palm-rotation*gripOffset,rotation);GripError=Vector3.Distance(rod.transform.TransformPoint(gripOffset),palm);
   if(p.phase==FishingPhase.Reeling&&leftUpper&&leftLower&&leftHand){
    float cycle=p.held?(float)game.Now*16:0;var wrist=palm-facing*Vector3.right*.12f+facing*Vector3.forward*.03f+facing*Vector3.up*(-.10f+Mathf.Sin(cycle)*.045f);
    Arm(leftUpper,leftLower,leftHand,wrist,-transform.right-Vector3.up*.6f);leftHand.rotation=Quaternion.LookRotation(direction,-(facing*Vector3.right));
   }
   if(head&&PlayerView.Instance&&PlayerView.Instance.active&&PlayerView.Instance.target==actor&&PlayerView.Instance.mode==0&&!PlayerView.Instance.FishingFightCameraActive){headScale=head.localScale;head.localScale=Vector3.one*.001f;hiddenHead=true;}
   bool show=p.Busy||p.phase==FishingPhase.Caught&&elapsed<1.4f;line.enabled=floatRing.enabled=show;if(!show)return;
   var endpoint=presentation.HookPoint(p);
   if(p.phase==FishingPhase.Casting){float t=Mathf.Clamp01(elapsed/.65f);endpoint=Vector3.Lerp(Tip,endpoint,t)+Vector3.up*Mathf.Sin(t*Mathf.PI)*2;}
   if(p.phase==FishingPhase.Bite)endpoint.y-=.08f+.08f*Mathf.Sin((float)game.Now*18);
   for(int i=0;i<line.positionCount;i++){float t=i/(float)(line.positionCount-1);var point=Vector3.Lerp(Tip,endpoint,t);point.y-=Mathf.Sin(t*Mathf.PI)*(p.phase==FishingPhase.Reeling?Mathf.Lerp(.22f,.015f,p.tension):.16f);line.SetPosition(i,point);}
   floatRing.startColor=floatRing.endColor=p.phase==FishingPhase.Bite||p.phase==FishingPhase.Reeling&&p.held?new Color(1,.72f,.12f):p.phase==FishingPhase.Reeling&&p.tension>.75f?new Color(1,.32f,.18f):new Color(.2f,1,1);
   float radius=p.phase==FishingPhase.Bite?.20f+.035f*Mathf.Sin((float)game.Now*10):p.phase==FishingPhase.Reeling?.15f+p.surge*.2f:.11f;
   for(int i=0;i<floatRing.positionCount;i++){float angle=i*Mathf.PI*2/floatRing.positionCount;var centre=endpoint;if(p.phase==FishingPhase.Reeling)centre.y=game.WaterHeight(centre);floatRing.SetPosition(i,centre+new Vector3(Mathf.Cos(angle)*radius,.018f,Mathf.Sin(angle)*radius));}
  }
  static void Arm(Transform shoulder,Transform elbow,Transform wrist,Vector3 target,Vector3 pole){var start=shoulder.position;float a=Vector3.Distance(start,elbow.position),b=Vector3.Distance(elbow.position,wrist.position);var delta=target-start;float d=Mathf.Clamp(delta.magnitude,Mathf.Abs(a-b)+.002f,a+b-.005f);var axis=delta.normalized;float along=(a*a-b*b+d*d)/(2*d);var bend=Vector3.ProjectOnPlane(pole,axis).normalized;var joint=start+axis*along+bend*Mathf.Sqrt(Mathf.Max(0,a*a-along*along));shoulder.rotation=Quaternion.FromToRotation(elbow.position-start,joint-start)*shoulder.rotation;elbow.rotation=Quaternion.FromToRotation(wrist.position-elbow.position,start+axis*d-elbow.position)*elbow.rotation;}
 }
}
