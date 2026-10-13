using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
namespace WhatTheFish {
 // Shared meshes/materials and short line effects. All inventory lives in host state.
 public sealed class FishingAbilityPresentation:MonoBehaviour {
  FishingGame game;Transform[] supplies=new Transform[5],treasures=new Transform[5];TextMesh[] labels=new TextMesh[5];int[] shownSeconds=new int[5];LineRenderer[] supplyRings=new LineRenderer[5];readonly Dictionary<ulong,LineRenderer> effects=new();
  void Start(){game=GetComponent<FishingGame>();var template=Resources.Load<GameObject>("BasketballProps/Turbo");
   for(int i=0;i<5;i++){
    if(template){supplies[i]=Instantiate(template,transform).transform;supplies[i].name="Shore supply "+i;foreach(var collider in supplies[i].GetComponentsInChildren<Collider>())collider.enabled=false;}
    var text=new GameObject("Supply countdown "+i);text.transform.SetParent(transform,false);labels[i]=text.AddComponent<TextMesh>();labels[i].font=SportsUIStyle.DisplayFont;labels[i].fontSize=42;labels[i].characterSize=.025f;labels[i].anchor=TextAnchor.MiddleCenter;labels[i].color=LocalProfile.Hex("FFF3D7");text.GetComponent<MeshRenderer>().sharedMaterial=labels[i].font.material;
    supplyRings[i]=game.Presentation.Line("Supply halo "+i,transform,.04f);supplyRings[i].loop=true;supplyRings[i].positionCount=32;
    var crate=new GameObject("Shared treasure "+i).transform;crate.SetParent(transform,false);treasures[i]=crate;
    Part(crate,"Treasure body",Vector3.zero,new Vector3(.62f,.34f,.4f),"Orange");Part(crate,"Gold lid",Vector3.up*.2f,new Vector3(.68f,.1f,.44f),"Gold");
    foreach(float x in new[]{-.2f,.2f})Part(crate,"Treasure band",new Vector3(x,.02f,0),new Vector3(.07f,.46f,.46f),"Gold");Part(crate,"Treasure latch",new Vector3(0,.08f,-.215f),new Vector3(.12f,.15f,.035f),"Ink");
   }
  }
  void LateUpdate(){if(!game)return;bool visible=game.Context&&game.State.Running;double now=game.Now;
   for(int i=0;i<5;i++){
    bool ready=now>=game.State.SupplyReady(i);var pos=game.SupplyPosition(i);var floor=game.Pier(i).y;pos.y=floor+.55f;
    if(supplies[i]){supplies[i].gameObject.SetActive(visible&&ready);supplies[i].position=pos+Vector3.up*(MenuPreferences.ReducedMotion?0:Mathf.Sin((float)now*2+i)*.08f);if(!MenuPreferences.ReducedMotion)supplies[i].Rotate(Vector3.up,30*Time.deltaTime,Space.World);}
    labels[i].gameObject.SetActive(visible&&!ready);int seconds=ready?-1:(int)Math.Ceiling(game.State.SupplyReady(i)-now);if(seconds!=shownSeconds[i]){shownSeconds[i]=seconds;labels[i].text=ready?"":seconds+"s";}labels[i].transform.position=pos+Vector3.up*.9f;if(Camera.main)labels[i].transform.rotation=Quaternion.LookRotation(labels[i].transform.position-Camera.main.transform.position);
    Ring(supplyRings[i],new Vector3(pos.x,floor+.03f,pos.z),.7f,ready?LocalProfile.Hex("4CDED6"):LocalProfile.Hex("69C8F4"),visible);
    var salvager=game.State.Players.FirstOrDefault(p=>p.connected&&p.phase==FishingPhase.Salvaging&&p.pier==i);bool busy=salvager.phase==FishingPhase.Salvaging;bool treasureReady=now>=game.State.Match.salvageReadyAt;
    var inward=Vector3.ProjectOnPlane(game.transform.position-game.Pier(i),Vector3.up).normalized;var treasure=game.Pier(i)+inward*3;float progress=busy?Mathf.Clamp01((float)(now-salvager.phaseAt)/6):0;treasure.y=game.WaterHeight(treasure)+.08f+progress*.75f;
    treasures[i].gameObject.SetActive(visible&&(treasureReady||busy));treasures[i].position=treasure;treasures[i].rotation=Quaternion.Euler(0,(float)now*12,Mathf.Sin((float)now*1.4f)*5);
   }
   foreach(var actor in Athlete.Active){if(!actor)continue;var player=game.Player(actor);bool effect=visible&&player.HasValue&&(player.Value.activeUntil>now||player.Value.speedUntil>now||player.Value.sonarUntil>now);ulong id=FishingGame.Key(actor);
    if(!effects.TryGetValue(id,out var ring)){if(!effect)continue;ring=game.Presentation.Line("Fishing gear effect "+id,transform,.045f);ring.loop=true;ring.positionCount=32;effects[id]=ring;}
    float age=player.HasValue?(float)(now-player.Value.activeAt):0;float wave=MenuPreferences.ReducedMotion?.4f:Mathf.Repeat(age*.65f,1);var colour=FishingAbilityRules.Color(player.HasValue?player.Value.active:FishingAbility.None);colour.a=1-wave*.65f;var point=actor.transform.position+Vector3.up*.08f;float radius=.6f+wave*1.4f;
    if(player.HasValue&&player.Value.bubbleGuard&&player.Value.phase==FishingPhase.Reeling){point=game.Presentation.HookPoint(player.Value);point.y=game.WaterHeight(point)+.05f;radius=.6f;}
    Ring(ring,point,radius,colour,effect);
   }
   foreach(var pair in effects)if(!visible||!game.State.Player(pair.Key).HasValue||!game.State.Player(pair.Key).Value.connected)pair.Value.enabled=false;
  }
  static void Ring(LineRenderer line,Vector3 centre,float radius,Color colour,bool visible){line.enabled=visible;if(!visible)return;line.startColor=line.endColor=colour;for(int i=0;i<line.positionCount;i++){float a=i*Mathf.PI*2/line.positionCount;line.SetPosition(i,centre+new Vector3(Mathf.Cos(a),0,Mathf.Sin(a))*radius);}}
  static void Part(Transform root,string name,Vector3 position,Vector3 scale,string material){var part=GameObject.CreatePrimitive(PrimitiveType.Cube);part.name=name;part.transform.SetParent(root,false);part.transform.localPosition=position;part.transform.localScale=scale;part.GetComponent<Collider>().enabled=false;Destroy(part.GetComponent<Collider>());var renderer=part.GetComponent<MeshRenderer>();renderer.sharedMaterial=Resources.Load<Material>("FootballProps/"+material);renderer.shadowCastingMode=ShadowCastingMode.Off;}
 }
}
