using System;
using System.Collections.Generic;
using UnityEngine;
namespace WhatTheFish {
 [DefaultExecutionOrder(210)]
 public sealed class FishingAbilityPresentation:MonoBehaviour {
  sealed class Effect {
   public Athlete actor;public Transform root;public readonly List<LineRenderer> lines=new();public FishingBuddyVisual buddy;public FishingPlayerRecord previous;public bool observed;public float buddyFade,popAt=-10;
  }
  FishingGame game;Material buddyMaterial;readonly Dictionary<ulong,Effect> effects=new();
  public Material BuddyMaterial=>buddyMaterial;
  public int VisibleBuddies {get{int n=0;foreach(var e in effects.Values)if(e.buddy!=null&&e.buddy.Root&&e.buddy.Root.gameObject.activeSelf)n++;return n;}}
  void Start(){game=GetComponent<FishingGame>();buddyMaterial=Resources.Load<Material>("FishingUI/Buddy");}
  LineRenderer Line(Effect e,int index){while(e.lines.Count<=index)e.lines.Add(game.Presentation.Line("Fishing skill trace "+e.lines.Count,e.root,.04f));return e.lines[index];}
  static void Tint(LineRenderer l,Color c,float width,bool loop,int points){l.enabled=true;l.loop=loop;l.positionCount=points;l.startColor=l.endColor=c;l.startWidth=l.endWidth=width;}
  void Circle(Effect e,int index,Vector3 centre,float radius,Color color,float width=0.045f,int axis=0){var l=Line(e,index);Tint(l,color,width,true,48);for(int i=0;i<48;i++){float a=i*Mathf.PI*2/48;var p=axis==0?new Vector3(Mathf.Cos(a),0,Mathf.Sin(a)):axis==1?new Vector3(Mathf.Cos(a),Mathf.Sin(a),0):new Vector3(0,Mathf.Sin(a),Mathf.Cos(a));l.SetPosition(i,centre+p*radius);}}
  void LateUpdate(){
   if(!game)return;if(!game.Context){Clear();return;}
   foreach(var e in effects.Values){foreach(var line in e.lines)if(line)line.enabled=false;}
   foreach(var actor in Athlete.Active){
    if(!actor)continue;var record=game.Player(actor);if(!record.HasValue||!record.Value.connected)continue;var p=record.Value;ulong id=FishingGame.Key(actor);
    if(effects.TryGetValue(id,out var e)&&e.actor!=actor){Destroy(e.root.gameObject);effects.Remove(id);e=null;}
    bool active=game.State.Running&&p.active!=FishingAbility.None&&game.Now<p.activeUntil;bool tail=game.State.Running&&p.schoolFishMask!=0&&game.Now>=p.schoolAt&&game.Now<p.schoolAt+FishingAbilityRules.Duration(FishingAbility.SchoolCall)+FishingAbilityRules.SchoolReleaseSeconds;
    if(e==null){if(!active&&!tail)continue;e=new Effect{actor=actor,root=new GameObject("Fishing skill effects "+id).transform};e.root.SetParent(transform,false);effects[id]=e;}
    if(e.observed&&e.previous.bubbleGuard&&!p.bubbleGuard&&game.Now<e.previous.activeUntil)e.popAt=Time.unscaledTime;
    float age=(float)(game.Now-p.activeAt);var rod=actor.GetComponent<FishingRodMotion>();var hook=p.fish>=0?game.Presentation.HookPoint(p):actor.transform.position;
    if(active&&p.active==FishingAbility.SteelWire&&rod&&rod.LineVisible){
     Vector3 axis=(hook-rod.Tip).normalized,side=Vector3.Cross(axis,Vector3.up).normalized;if(side.sqrMagnitude<.01f)side=Vector3.right;var up=Vector3.Cross(axis,side);
     for(int strand=0;strand<2;strand++){var l=Line(e,strand);Tint(l,strand==0?Color.white:new Color(.35f,.77f,1),.033f,false,48);for(int i=0;i<48;i++){float t=i/47f,a=t*48+strand*Mathf.PI;var point=Vector3.Lerp(rod.Tip,hook,t);point.y-=Mathf.Sin(t*Mathf.PI)*Mathf.Lerp(.22f,.015f,p.tension);point+=(side*Mathf.Cos(a)+up*Mathf.Sin(a))*.027f;l.SetPosition(i,point);}}
    }
    if(active&&p.active==FishingAbility.PowerReel&&rod&&rod.LineVisible){for(int i=0;i<3;i++){float t=Mathf.Repeat(age*1.3f+i/3f,1);var l=Line(e,i);Tint(l,new Color(1,.76f,.15f),.08f,false,4);for(int k=0;k<4;k++){float at=Mathf.Clamp01(t+k*.018f);var point=Vector3.Lerp(rod.Tip,hook,at);point.y-=Mathf.Sin(at*Mathf.PI)*Mathf.Lerp(.22f,.015f,p.tension);l.SetPosition(k,point);}}}
    float desired=active&&p.active==FishingAbility.FishingBuddy&&p.phase==FishingPhase.Reeling?1:0;e.buddyFade=Mathf.MoveTowards(e.buddyFade,desired,Time.unscaledDeltaTime*4);
    if(e.buddyFade>0&&rod&&rod.Equipped&&buddyMaterial){if(e.buddy==null)e.buddy=new FishingBuddyVisual(actor,e.root,buddyMaterial);e.buddy.Tick(p,game.Now,e.buddyFade);var l=Line(e,4);Tint(l,new Color(.95f,.98f,1,e.buddyFade),.027f,false,12);for(int i=0;i<12;i++){float t=i/11f;var point=Vector3.Lerp(e.buddy.LineTip,hook,t);point.y-=Mathf.Sin(t*Mathf.PI)*.1f;l.SetPosition(i,point);}}else e.buddy?.Hide();
    float pop=Time.unscaledTime-e.popAt;bool bubble=active&&p.active==FishingAbility.BubbleGuard&&p.bubbleGuard;
    if(bubble||pop<.45f){var at=hook;at.y=game.WaterHeight(at)+.34f;float radius=bubble?.52f:.52f+pop*1.8f;var color=new Color(.5f,.9f,1,bubble?.82f:Mathf.Clamp01(1-pop/.45f));for(int i=0;i<3;i++)Circle(e,i,at,radius,color,.035f,i);if(pop<.45f)Circle(e,3,new Vector3(at.x,game.WaterHeight(at)+.03f,at.z),radius*1.4f,color,.05f);}
    if(active&&p.active==FishingAbility.Sonar){for(int i=0;i<3;i++){float phase=Mathf.Repeat(age*.42f+i/3f,1);var at=game.transform.position;at.y=game.WaterY+.025f;Circle(e,i,at,1+phase*23,new Color(.25f,1,.85f,(1-phase)*.85f),.055f);}}
    if(tail){float schoolAge=(float)(game.Now-p.schoolAt);for(int i=0;i<3;i++){float phase=MenuPreferences.ReducedMotion?.5f:Mathf.Repeat(schoolAge*.7f+i/3f,1);var at=p.schoolPoint;at.y=game.WaterHeight(at)+.025f;float fade=1-Mathf.SmoothStep(0,1,(schoolAge-(float)FishingAbilityRules.Duration(FishingAbility.SchoolCall))/FishingAbilityRules.SchoolReleaseSeconds);Circle(e,i,at,.45f+phase*2.6f,new Color(1,.73f,.22f,(1-phase)*fade),.05f);}}
    e.previous=p;e.observed=true;
   }
   foreach(var pair in effects){if(!pair.Value.actor||!game.State.Player(pair.Key).HasValue||!game.State.Player(pair.Key).Value.connected){foreach(var l in pair.Value.lines)if(l)l.enabled=false;pair.Value.buddy?.Hide();}}
  }
  void Clear(){foreach(var e in effects.Values)if(e.root)Destroy(e.root.gameObject);effects.Clear();}
  void OnDisable()=>Clear();void OnDestroy()=>Clear();
 }
}
