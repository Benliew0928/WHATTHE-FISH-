using System;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace WhatTheFish {
 public enum GolfFeedbackCue:byte {Whoosh,Swing,Putt,Turf,Sand,Cup,Finish,Ready,Stone,Wood,Ball,Water,Recovery,CartBump}
 public struct GolfFeedbackEvent:INetworkSerializable {
  public uint sequence,round;public double time;public ulong actor,owner;
  public GolfFeedbackCue cue;public Vector3 point,normal;public float strength;
  public void NetworkSerialize<T>(BufferSerializer<T> s) where T:IReaderWriter {
   s.SerializeValue(ref sequence);s.SerializeValue(ref round);s.SerializeValue(ref time);s.SerializeValue(ref actor);s.SerializeValue(ref owner);
   s.SerializeValue(ref cue);s.SerializeValue(ref point);s.SerializeValue(ref normal);s.SerializeValue(ref strength);
  }
 }
 // Presentation only. Host-confirmed contact/landing/cup events use their own
 // cosmetic channel; no physics, roster or existing network schema changes.
 [DefaultExecutionOrder(220)]
 public sealed class GolfFeedback:MonoBehaviour {
  const string Channel="Golf.Feedback.v1";
  public static GolfFeedback Instance {get;private set;}
  public GolfFeedbackSettings Settings {get;private set;}
  GolfMatchManager match;GolfFeedbackSounds sounds;CustomMessagingManager bus;
  AudioSource charging;readonly AudioSource[] voices=new AudioSource[12];readonly float[] voiceGains=new float[12];int voiceIndex;
  readonly ParticleSystem[] bursts=new ParticleSystem[8];int burstIndex;System.Random random=new(28593);
  GolfChargeGraphic ring,edge;RectTransform uiRoot,powerMeter;Text success;CanvasGroup successOpacity;
  float chargeGain,shakeUntil,shakeStrength,successAt=-100;
  bool focused=true,paused,isQuiet=true;uint serial,received,receivedRound,observedRound;
  readonly int[] counts=new int[14];
  public float Charge {get;private set;}
  public float EdgeOpacity {get;private set;}
  public float LastCameraKick {get;private set;}
  public bool ChargeLoopPlaying=>charging&&charging.isPlaying;
  public bool ChargeRingVisible=>ring&&ring.enabled;
  public int CueCount(GolfFeedbackCue cue)=>counts[(int)cue];
  public AudioClip CueClip(GolfFeedbackCue cue)=>sounds?.cues[(int)cue];
  public int ParticleCount {get{int n=0;foreach(var burst in bursts)if(burst)n+=burst.particleCount;return n;}}
  bool Context=>match&&match.Context&&isActiveAndEnabled;
  bool Audible=>Context&&focused&&!paused&&!AudioListener.pause&&Time.timeScale>0;
  ulong LocalOwner=>AppRoot.Instance&&AppRoot.Instance.LocalAthlete?GolfCartWorld.Key(AppRoot.Instance.LocalAthlete):ulong.MaxValue;
  void Awake(){
   match=GetComponent<GolfMatchManager>();Settings=Resources.Load<GolfFeedbackSettings>("GolfFeedback");
   if(!Settings)Settings=ScriptableObject.CreateInstance<GolfFeedbackSettings>();
   GolfFishingAudio.Ensure(this);sounds=new GolfFeedbackSounds();charging=Source("Golf charge",true);charging.clip=sounds.charge;
   for(int i=0;i<voices.Length;i++)voices[i]=Source("Golf contact voice "+i,false);
   var material=Resources.Load<Material>("GolfFeedback");
   for(int i=0;i<bursts.Length;i++){
    var go=new GameObject("Golf contact particles "+i);go.transform.SetParent(transform,false);
    var ps=go.AddComponent<ParticleSystem>();ps.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
    var main=ps.main;main.loop=false;main.playOnAwake=false;main.maxParticles=64;main.simulationSpace=ParticleSystemSimulationSpace.World;main.startSpeed=0;main.duration=1;main.gravityModifier=.35f;
    var emission=ps.emission;emission.enabled=false;var shape=ps.shape;shape.enabled=false;
    var size=ps.sizeOverLifetime;size.enabled=true;size.size=new ParticleSystem.MinMaxCurve(1,AnimationCurve.Linear(0,1,1,0));
    var fade=ps.colorOverLifetime;fade.enabled=true;fade.color=new Gradient{colorKeys=new[]{new GradientColorKey(Color.white,0),new GradientColorKey(Color.white,1)},alphaKeys=new[]{new GradientAlphaKey(1,0),new GradientAlphaKey(.7f,.4f),new GradientAlphaKey(0,1)}};
    var renderer=go.GetComponent<ParticleSystemRenderer>();renderer.sharedMaterial=material;renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;
    bursts[i]=ps;
   }
  }
  AudioSource Source(string name,bool loop){var go=new GameObject(name);go.transform.SetParent(transform,false);var source=go.AddComponent<AudioSource>();source.playOnAwake=false;source.loop=loop;source.dopplerLevel=0;source.spatialBlend=loop?0:.65f;source.rolloffMode=AudioRolloffMode.Linear;source.minDistance=3;source.maxDistance=65;source.volume=0;return source;}
  void OnEnable(){Instance=this;focused=Application.isFocused;}
  void EnsureBus(){var network=NetworkManager.Singleton;var current=network&&network.IsListening?network.CustomMessagingManager:null;if(ReferenceEquals(current,bus))return;if(bus!=null)bus.UnregisterNamedMessageHandler(Channel);bus=current;received=receivedRound=0;if(bus!=null)bus.RegisterNamedMessageHandler(Channel,Receive);}
  static void Stretch(RectTransform rect){rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=rect.offsetMax=Vector2.zero;}
  void EnsureUI(){
   if(uiRoot||!GolfHUD.Instance||!GolfHUD.Instance.ActionRect)return;
   var parent=GolfHUD.Instance.FrameRect;uiRoot=GolfHUDStyle.Rect("Golf feedback HUD",parent,new Vector2(.5f,.5f),Vector2.zero,Vector2.zero);Stretch(uiRoot);
   edge=GolfHUDStyle.Rect("Golf warm edge",uiRoot,new Vector2(.5f,.5f),Vector2.zero,Vector2.zero).gameObject.AddComponent<GolfChargeGraphic>();Stretch(edge.rectTransform);edge.vignette=true;edge.raycastTarget=false;edge.enabled=false;
   powerMeter=GolfHUDStyle.Rect("Golf character charge meter",uiRoot,new Vector2(.5f,.5f),Vector2.zero,new Vector2(26,120));var background=powerMeter.gameObject.AddComponent<GolfChargeGraphic>();background.meterFrame=true;background.Show(1,1);background.raycastTarget=false;
   ring=GolfHUDStyle.Rect("Golf character power fill",powerMeter,new Vector2(.5f,.5f),Vector2.zero,Vector2.zero).gameObject.AddComponent<GolfChargeGraphic>();Stretch(ring.rectTransform);ring.rectTransform.offsetMin=new Vector2(4,4);ring.rectTransform.offsetMax=new Vector2(-4,-4);ring.raycastTarget=false;ring.enabled=false;powerMeter.gameObject.SetActive(false);
   success=GolfHUDStyle.Text("Golf hole success",uiRoot,"",new Vector2(540,70),30,TextAnchor.MiddleCenter,Color.white);success.rectTransform.anchorMin=success.rectTransform.anchorMax=new Vector2(.5f,.65f);success.font=CoveUI.DisplayFont??success.font;success.raycastTarget=false;
   var outline=success.gameObject.AddComponent<Outline>();outline.effectColor=GolfHUDStyle.Ink;outline.effectDistance=new Vector2(1.2f,-1.2f);successOpacity=success.gameObject.AddComponent<CanvasGroup>();successOpacity.interactable=successOpacity.blocksRaycasts=false;successOpacity.alpha=0;
  }
  void Update(){
   EnsureBus();EnsureUI();
   if(observedRound!=match.Round){Quiet();observedRound=match.Round;}
   if(!Audible){Quiet();return;}
   for(int i=0;i<voices.Length;i++)voices[i].volume=voiceGains[i]*MenuPreferences.SFXVolume;
   var view=PlayerView.Instance;bool held=view&&view.GolfAiming&&view.GolfCharging;
   isQuiet=false;Charge=held?view.GolfDisplayedCharge:0;
   bool putt=held&&view.GolfMode==GolfShotMode.Putt;
   float target=held?(putt?Settings.puttChargeVolume:Settings.chargeVolume)*MenuPreferences.SFXVolume:0;
   chargeGain=Mathf.MoveTowards(chargeGain,target,Time.unscaledDeltaTime*3);
   charging.volume=chargeGain;charging.pitch=.8f+Charge*.65f;
   if(chargeGain>.001f&&!charging.isPlaying)charging.Play();else if(chargeGain<=.001f&&charging.isPlaying)charging.Stop();
   // Full power is communicated by the red meter and screen edge only.
   // The draggable power rail replaces the old second meter beside the character.
   if(ring){powerMeter.gameObject.SetActive(false);ring.enabled=false;}
   EdgeOpacity=held&&!putt?Settings.edgeOpacity*Mathf.InverseLerp(.8f,1,Charge):0;
   if(edge){edge.enabled=EdgeOpacity>0;edge.Show(1,EdgeOpacity);}
   if(successOpacity){float age=Time.unscaledTime-successAt;successOpacity.alpha=age<1.1f?Mathf.Clamp01((1.1f-age)/.3f):0;}
  }
  void LateUpdate(){
   LastCameraKick=0;
   if(!Audible||!PlayerView.Instance||!PlayerView.Instance.active)return;
   var camera=PlayerView.Instance.transform;
   if(!MenuPreferences.ReducedMotion&&Charge>0)camera.position+=camera.forward*(Settings.chargeCameraPush*Charge);
   if(!Settings.cameraShake||MenuPreferences.ReducedMotion)return;
   float remaining=shakeUntil-Time.unscaledTime;if(remaining<=0)return;
   float envelope=Mathf.Clamp01(remaining/Mathf.Max(.02f,Settings.shakeSeconds));LastCameraKick=Settings.shakeDistance*shakeStrength*envelope;
   float t=Time.unscaledTime*145;camera.position+=camera.right*(Mathf.Sin(t)*LastCameraKick)+camera.up*(Mathf.Cos(t*1.37f)*LastCameraKick*.65f);
  }
  public static void Emit(GolfFeedbackCue cue,Vector3 point,Vector3 normal,float strength,ulong actor,ulong owner){
   var a=Instance;if(!a||!a.Context||!a.match.Authority)return;
   var e=new GolfFeedbackEvent{sequence=++a.serial,round=a.match.Round,time=a.match.Now,actor=actor,owner=owner,cue=cue,point=point,normal=normal,strength=Mathf.Clamp01(strength)};
   a.Present(e);a.EnsureBus();var network=NetworkManager.Singleton;
   if(a.bus!=null&&network.IsServer){using var writer=new FastBufferWriter(96,Allocator.Temp);writer.WriteNetworkSerializable(e);a.bus.SendNamedMessageToAll(Channel,writer,NetworkDelivery.ReliableSequenced);}
  }
  void Receive(ulong sender,FastBufferReader reader){var network=NetworkManager.Singleton;if(!network||network.IsServer||sender!=NetworkManager.ServerClientId||!reader.TryBeginRead(61))return;reader.ReadNetworkSerializable(out GolfFeedbackEvent e);Accept(e);}
  internal bool Accept(GolfFeedbackEvent e){
   if(!Context||e.round!=match.Round||!double.IsFinite(e.time)||!float.IsFinite(e.point.sqrMagnitude)||!float.IsFinite(e.normal.sqrMagnitude)||!float.IsFinite(e.strength)||e.strength<0||e.strength>1||((int)e.cue>=14||e.cue==GolfFeedbackCue.Ready))return false;
   double age=match.Now-e.time;if(age<-.25||age>1.2)return false;
   if(receivedRound!=e.round){receivedRound=e.round;received=0;}if(e.sequence<=received)return false;received=e.sequence;Present(e);return true;
  }
  void Present(GolfFeedbackEvent e){
   if(observedRound!=match.Round){Quiet();observedRound=match.Round;}
   if(!Audible)return;isQuiet=false;counts[(int)e.cue]++;
   match.Ball(e.owner)?.GetComponent<GolfBallEventFX>()?.Contact(e);
   bool local=e.actor==LocalOwner;float gain;
   switch(e.cue){
    case GolfFeedbackCue.Whoosh:gain=.35f*e.strength;break;
    case GolfFeedbackCue.Swing:case GolfFeedbackCue.Putt:gain=Settings.hitVolume*Mathf.Lerp(.35f,1,e.strength)*(e.cue==GolfFeedbackCue.Putt?.65f:1);break;
    case GolfFeedbackCue.Turf:case GolfFeedbackCue.Sand:case GolfFeedbackCue.Stone:case GolfFeedbackCue.Wood:case GolfFeedbackCue.Ball:gain=Settings.landingVolume*Mathf.Lerp(.15f,1,e.strength);break;
    default:gain=Settings.cupVolume;break;
   }
   Play(e.cue,e.point,gain,(e.cue==GolfFeedbackCue.Cup||e.cue==GolfFeedbackCue.Finish)&&e.owner==LocalOwner);
   if(e.cue==GolfFeedbackCue.Swing&&local&&e.strength>=.65f){shakeUntil=Time.unscaledTime+Settings.shakeSeconds;shakeStrength=e.strength;}
   if(e.cue!=GolfFeedbackCue.Whoosh&&e.cue< GolfFeedbackCue.Ready)Burst(e);
   if(e.cue==GolfFeedbackCue.Finish)GolfFishingAudio.Golf?.Play("golf-cup",e.point,Settings.cupVolume);
   if(e.cue==GolfFeedbackCue.Cup||e.cue==GolfFeedbackCue.Finish)GolfCupCelebration.Play(transform,e.point);
   if(e.cue==GolfFeedbackCue.Cup&&e.owner==LocalOwner)GolfFishingAudio.Golf?.Play("golf-hole",e.point,.7f,true);
   if((e.cue==GolfFeedbackCue.Cup||e.cue==GolfFeedbackCue.Finish)&&e.owner==LocalOwner){EnsureUI();successAt=Time.unscaledTime;if(success)success.text=e.cue==GolfFeedbackCue.Finish?"ALL 5 HOLES COMPLETE!":"HOLE COMPLETE!";}
  }
  void Play(GolfFeedbackCue cue,Vector3 point,float gain,bool flat=false){
   int index=voiceIndex++%voices.Length;voiceGains[index]=gain;var source=voices[index];source.Stop();source.transform.position=point;source.spatialBlend=flat?0:.65f;source.clip=sounds.Choose(cue);source.volume=gain*MenuPreferences.SFXVolume;source.pitch=1;source.Play();
  }
  float Range(float low,float high)=>Mathf.Lerp(low,high,(float)random.NextDouble());
  void Burst(GolfFeedbackEvent e){
   bool cup=e.cue==GolfFeedbackCue.Cup||e.cue==GolfFeedbackCue.Finish;
   bool landing=e.cue==GolfFeedbackCue.Turf||e.cue==GolfFeedbackCue.Sand;
   int count=cup?Settings.cupParticles*(e.cue==GolfFeedbackCue.Finish?2:1):landing?Settings.landingParticles:e.cue==GolfFeedbackCue.Putt?Settings.puttSparks:Settings.swingSparks;
   if(landing)count=Mathf.RoundToInt(count*Mathf.Lerp(.3f,1,e.strength));
   var ps=bursts[burstIndex++%bursts.Length];ps.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);ps.Play();
   var normal=e.normal.sqrMagnitude>.01f?e.normal.normalized:Vector3.up;
   for(int i=0;i<count;i++){
    var velocity=new Vector3(Range(-1,1),Range(.2f,1),Range(-1,1));velocity=Vector3.ProjectOnPlane(velocity,normal)+normal*Range(.15f,cup?1.6f:.65f);
    float scale=cup?1:landing?.45f:.35f;
    var color=cup?new Color(1,.83f,.18f):landing?e.cue==GolfFeedbackCue.Sand?new Color(.94f,.8f,.5f,.65f):new Color(.63f,.73f,.38f,.7f):new Color(1,.96f,.7f);
    var p=new ParticleSystem.EmitParams{position=e.point+normal*.025f,velocity=velocity*scale,startColor=color,startSize=cup?Range(.04f,.09f):Range(.025f,.06f),startLifetime=cup?Range(.45f,.85f):landing?Range(.2f,.4f):Range(.08f,.18f)};ps.Emit(p,1);
   }
  }
  void Quiet(){if(isQuiet)return;isQuiet=true;Charge=EdgeOpacity=chargeGain=0;shakeUntil=0;charging.Stop();foreach(var source in voices)if(source)source.Stop();foreach(var burst in bursts)if(burst)burst.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);if(ring)ring.enabled=false;if(powerMeter)powerMeter.gameObject.SetActive(false);if(edge)edge.enabled=false;if(successOpacity)successOpacity.alpha=0;successAt=-100;}
#if UNITY_EDITOR || DEVELOPMENT_BUILD
  public void ReviewFocus(bool value){focused=value;if(!value)Quiet();}
#endif
  void OnApplicationFocus(bool value){focused=value;if(!value)Quiet();}
  void OnApplicationPause(bool value){paused=value;if(value)Quiet();}
  void OnDisable(){Quiet();if(bus!=null){bus.UnregisterNamedMessageHandler(Channel);bus=null;}received=receivedRound=0;if(Instance==this)Instance=null;}
  void OnDestroy(){if(bus!=null)bus.UnregisterNamedMessageHandler(Channel);sounds?.Dispose();if(uiRoot)Destroy(uiRoot.gameObject);if(ring)Destroy(ring.gameObject);}
 }
}
