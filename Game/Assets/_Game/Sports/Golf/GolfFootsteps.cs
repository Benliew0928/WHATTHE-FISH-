using System.Collections.Generic;
using UnityEngine;

namespace WhatTheFish {
 public enum GolfFootSurface {Grass,Sand,Water,Wood,Stone}
 // Detect each displayed foot lowering after a lift, gated by real displacement.
 [DefaultExecutionOrder(210)]
 public sealed class GolfFootsteps:MonoBehaviour {
  sealed class Step {public Vector3 last;public float travelled,lastTime=-10,lastMoved,leftHeight,rightHeight;public bool leftLift,rightLift;public TurnFootPlacement feet;public Animator animator;public int beat=-1,state;}
  readonly Dictionary<Athlete,Step> steps=new();readonly List<Athlete> dead=new();
  readonly AudioSource[] voices=new AudioSource[8];readonly float[] gains=new float[8];readonly int[] counts=new int[5];
  AudioClip[] clips,walkway;GolfBallPhysicsSettings physics;int cursor,walkwayTake;bool focused=true;GolfMatchManager match;GolfFeedbackSettings settings;
  public AudioClip[] WalkwayClips=>walkway;
  public int RecordedContacts {get;private set;}
  public int Count(GolfFootSurface surface)=>counts[(int)surface];
  public AudioClip Clip(GolfFootSurface surface)=>clips[(int)surface];
  void Awake(){match=GetComponent<GolfMatchManager>();settings=Resources.Load<GolfFeedbackSettings>("GolfFeedback");clips=GolfFeedbackSounds.Footsteps();walkway=Resources.LoadAll<AudioClip>("GolfAudio/Walkway");System.Array.Sort(walkway,(a,b)=>string.CompareOrdinal(a.name,b.name));physics=Resources.Load<GolfBallPhysicsSettings>("GolfBallPhysics");for(int i=0;i<voices.Length;i++){var go=new GameObject("Golf footsteps "+i);go.transform.SetParent(transform,false);var source=go.AddComponent<AudioSource>();source.playOnAwake=false;source.spatialBlend=.8f;source.dopplerLevel=0;source.rolloffMode=AudioRolloffMode.Linear;source.minDistance=2;source.maxDistance=22;voices[i]=source;}}
  public static bool IsWalkway(Collider support){if(!support)return false;string name=support.name.ToLowerInvariant();return name.Contains("path")||name.Contains("approach")||name.Contains("paving");}
  public GolfFootSurface Surface(Vector3 point,Collider support=null){
   var course=match.Course;var local=course.transform.InverseTransformPoint(point);
   foreach(var water in course.waterPatches)if(water.Contains(local.x,local.z)&&local.y<=water.centre.y+.15f)return GolfFootSurface.Water;
   var island=course.GetComponentInParent<RefinedIslandEnvironment>();if(island&&island.layout!=null){var p=island.transform.InverseTransformPoint(point);if(p.y<=island.layout.sea_level+.04f)return GolfFootSurface.Water;}
   string name=support?support.name.ToLowerInvariant():"";
   if(IsWalkway(support))return GolfFootSurface.Stone;
   if(name.Contains("wood")||name.Contains("deck")||name.Contains("bridge")||name.Contains("pier")||name.Contains("plank"))return GolfFootSurface.Wood;
   if(name.Contains("rock")||name.Contains("stone")||name.Contains("paving")||name.Contains("station"))return GolfFootSurface.Stone;
   if(course.RollingResistance(point,physics)>5.7f)return GolfFootSurface.Sand;
   return GolfFootSurface.Grass;
  }
  void LateUpdate(){
   if(!match.Context||!focused||AudioListener.pause||Time.timeScale<=0){steps.Clear();foreach(var voice in voices)voice.Stop();return;}
   for(int i=0;i<voices.Length;i++)voices[i].volume=gains[i]*MenuPreferences.SFXVolume;
   foreach(var actor in Athlete.Active){
    if(!actor||!actor.isActiveAndEnabled)continue;var point=actor.transform.position;
    if(!steps.TryGetValue(actor,out var step)){steps[actor]=new Step{last=point,feet=actor.GetComponent<TurnFootPlacement>(),animator=actor.GetComponentInChildren<Animator>()};continue;}
    float distance=Vector3.ProjectOnPlane(point-step.last,Vector3.up).magnitude;step.last=point;
    bool allowed=!actor.inTransit&&!actor.Airborne&&!actor.LoadingJump&&!match.IsAiming(actor)&&!GolfCartWorld.Driving(actor)&&actor.Grounded;
    if(!allowed||distance>1){step.travelled=0;step.beat=-1;step.leftLift=step.rightLift=false;continue;}
    if(distance<.0001f){if(Time.time-step.lastMoved>.15f){step.travelled=0;step.beat=-1;step.leftLift=step.rightLift=false;}continue;}step.lastMoved=Time.time;
    float speed=distance/Mathf.Max(.001f,Time.deltaTime);bool contact=false;
    if(step.feet&&step.feet.left.ankle&&step.feet.right.ankle){
     bool Plant(TurnFootPlacement.Leg leg,ref float previous,ref bool lifted){float height=actor.transform.InverseTransformPoint(leg.ankle.position).y;bool hit=FootContact(height,leg.initialPosition.y,ref previous,ref lifted,settings.footLiftHeight,settings.footPlantHeight);return hit;}
     bool left=Plant(step.feet.left,ref step.leftHeight,ref step.leftLift),right=Plant(step.feet.right,ref step.rightHeight,ref step.rightLift);contact=left||right;
    }else if(step.animator&&step.animator.runtimeAnimatorController){
     var state=step.animator.GetCurrentAnimatorStateInfo(0);int beat=Mathf.FloorToInt((state.normalizedTime-settings.footContactPhase)*2);
     if(!state.IsName("Run")){step.beat=-1;continue;}
     if(step.state!=state.fullPathHash||step.beat<0){step.state=state.fullPathHash;step.beat=beat;continue;}
     contact=beat>step.beat&&beat-step.beat<=2;step.beat=beat;
    }else {step.travelled+=distance;float stride=speed>4?settings.runningStepDistance:settings.walkingStepDistance;contact=step.travelled>=stride;if(contact)step.travelled%=stride;}
    if(!contact||Time.time-step.lastTime<settings.minimumFootstepInterval)continue;step.lastTime=Time.time;
    Physics.Raycast(point+Vector3.up*.15f,Vector3.down,out var ground,.6f,1<<8,QueryTriggerInteraction.Ignore);
    var surface=Surface(ground.collider?ground.point:point,ground.collider);Emit(surface,point,speed,IsWalkway(ground.collider));
   }
   dead.Clear();foreach(var pair in steps)if(!pair.Key||!Athlete.Active.Contains(pair.Key))dead.Add(pair.Key);foreach(var actor in dead)steps.Remove(actor);
  }
  public static bool FootContact(float height,float rest,ref float previous,ref bool lifted,float lift,float plant){if(height>rest+lift)lifted=true;bool hit=lifted&&height<=rest+plant&&height<=previous;if(hit)lifted=false;previous=height;return hit;}
  void Emit(GolfFootSurface surface,Vector3 point,float speed,bool path){counts[(int)surface]++;int i=cursor++%voices.Length;var source=voices[i];source.Stop();source.transform.position=point;bool recorded=path&&walkway.Length>0;source.clip=recorded?walkway[walkwayTake++%walkway.Length]:clips[(int)surface];if(recorded)RecordedContacts++;source.pitch=recorded?1:Random.Range(.94f,1.06f);gains[i]=settings.footstepVolume*Mathf.Lerp(.65f,1,Mathf.InverseLerp(1,6,speed));source.volume=gains[i]*MenuPreferences.SFXVolume;source.Play();}
#if UNITY_EDITOR || DEVELOPMENT_BUILD
  public void ReviewFocus(bool value){focused=value;}
#endif
  void OnApplicationFocus(bool value){focused=value;}
  void OnDisable(){steps.Clear();foreach(var voice in voices)if(voice)voice.Stop();}
  void OnDestroy(){if(clips!=null)foreach(var clip in clips)if(clip)Destroy(clip);}
 }
}
