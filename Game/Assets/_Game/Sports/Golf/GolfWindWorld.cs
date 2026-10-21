using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace WhatTheFish {
 // Cosmetics follow the same replicated wind direction as shot physics.
 // Shader bending leaves trunks, colliders, roots and flagpoles stationary.
 public sealed class GolfWindWorld:MonoBehaviour {
  struct Flag {public Transform cloth;public Quaternion home;}
  readonly List<Flag> flags=new();readonly List<GolfSandPatch> sand=new(),nearSand=new();GolfBallPhysicsSettings settings;
  GolfMatchManager match;ParticleSystem dust;float strength,emitCredit;int patchCursor;
  static readonly int WindId=Shader.PropertyToID("_GolfWind"),AreaId=Shader.PropertyToID("_GolfWindArea");
  public float Strength=>strength;public int TotalSandParticles {get;private set;}
  public Vector3 Direction {get;private set;}
  public int FlagCount=>flags.Count;
  void Start(){
   settings=Resources.Load<GolfBallPhysicsSettings>("GolfBallPhysics");match=GetComponent<GolfMatchManager>();if(!match||!match.Course)return;
   foreach(var t in match.transform.parent.GetComponentsInChildren<Transform>(true))if(t.name=="Aqua pennant")flags.Add(new Flag{cloth=t,home=t.localRotation});
   sand.AddRange(match.Course.sandPatches);
   foreach(var p in new[]{new Vector3(-42,0,-112),new Vector3(43,0,-56),new Vector3(-38,0,16),new Vector3(38,0,74),new Vector3(-37,0,127)})sand.Add(new GolfSandPatch{centre=p,radii=new Vector2(10,6)});
  }
  void BuildDust(){
   var go=new GameObject("Windblown bunker sand");go.transform.SetParent(transform,false);dust=go.AddComponent<ParticleSystem>();dust.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
   var main=dust.main;main.loop=true;main.playOnAwake=false;main.maxParticles=384;main.simulationSpace=ParticleSystemSimulationSpace.World;main.startSpeed=0;main.duration=1;main.gravityModifier=.18f;
   var emission=dust.emission;emission.enabled=false;var shape=dust.shape;shape.enabled=false;
   var fade=dust.colorOverLifetime;fade.enabled=true;fade.color=new Gradient{alphaKeys=new[]{new GradientAlphaKey(0,0),new GradientAlphaKey(.55f,.12f),new GradientAlphaKey(0,1)},colorKeys=new[]{new GradientColorKey(Color.white,0),new GradientColorKey(Color.white,1)}};
   var renderer=go.GetComponent<ParticleSystemRenderer>();renderer.sharedMaterial=Resources.Load<Material>("GolfFeedback");renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;
  }
  void LateUpdate(){
   if(!match)return;bool active=match.Context&&match.Events.Active(match.Now)==GolfEventKind.TropicalWind;
   strength=Mathf.MoveTowards(strength,active?1:0,Time.unscaledDeltaTime*2);if(active)Direction=match.Events.Snapshot.wind.normalized;
   if(!match.Context)strength=0;
   Shader.SetGlobalVector(WindId,new Vector4(Direction.x,Direction.z,strength,(float)(match.Now+Time.timeAsDouble-Time.fixedTimeAsDouble)));
   var centre=match.transform.position;Shader.SetGlobalVector(AreaId,new Vector4(centre.x,centre.z,250,0));
   foreach(var f in flags)if(f.cloth){var home=f.cloth.parent.rotation*f.home;float yaw=Mathf.Atan2(-Direction.z,Direction.x)*Mathf.Rad2Deg;f.cloth.rotation=Quaternion.Slerp(home,Quaternion.Euler(0,yaw,0),strength);}
   if(strength<.01f){if(dust&&dust.particleCount==0)dust.Stop();return;}if(!dust)BuildDust();if(!dust.isPlaying)dust.Play();EmitSand();
  }
  void EmitSand(){
   var camera=Camera.main;if(!camera||sand.Count==0)return;
   nearSand.Clear();foreach(var patch in sand)if((camera.transform.position-transform.TransformPoint(patch.centre)).sqrMagnitude<45*45)nearSand.Add(patch);if(nearSand.Count==0)return;
   emitCredit=Mathf.Min(12,emitCredit+Time.deltaTime*85*strength);int tries=0;
   while(emitCredit>=1&&tries++<12){
    emitCredit--;var patch=nearSand[patchCursor++%nearSand.Count];var centre=transform.TransformPoint(patch.centre);if((camera.transform.position-centre).sqrMagnitude>50*50)continue;
    var disk=Random.insideUnitCircle*.85f;float a=patch.yaw*Mathf.Deg2Rad;var offset=new Vector3(disk.x*patch.radii.x,0,disk.y*patch.radii.y);offset=Quaternion.Euler(0,-a*Mathf.Rad2Deg,0)*offset;
    var point=transform.TransformPoint(patch.centre+offset);
    if(!Physics.Raycast(point+Vector3.up*25,Vector3.down,out var hit,60,1<<8,QueryTriggerInteraction.Ignore)||hit.normal.y<.6f||!match.Course.SafeDryPosition(hit.point,.05f))continue;
    if(match.Course.RollingResistance(hit.point,settings)<7)continue;
    float lift=Random.Range(.45f,1.3f);
    dust.Emit(new ParticleSystem.EmitParams{position=hit.point+hit.normal*.035f,velocity=Direction*Random.Range(3f,6f)+Vector3.up*lift,startLifetime=Mathf.Min(1.35f,2*lift/(Physics.gravity.magnitude*.18f)),startSize=Random.Range(.055f,.14f),startColor=new Color(.87f,.73f,.47f,.65f)},1);TotalSandParticles++;
   }
  }
  void OnDisable(){Shader.SetGlobalVector(WindId,Vector4.zero);foreach(var f in flags)if(f.cloth)f.cloth.localRotation=f.home;if(dust)dust.Clear();strength=0;}
 }
}
