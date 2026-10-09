using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace WhatTheFish {
 public sealed class FishingPresentation:MonoBehaviour {
  FishingGame game;Transform comparison;readonly GameObject[] fish=new GameObject[FishingState.FishCount];
  readonly LineRenderer[] halos=new LineRenderer[FishingState.FishCount];
  readonly Vector3[] targetOffsets=new Vector3[FishingState.FishCount];
  readonly Dictionary<ulong,FishingRodMotion> rods=new();Material lineMaterial;
  public int LiveFishCount=>fish.Count(f=>f&&f.activeSelf);
  public Vector3 TargetPosition(int id)=>id>=0&&id<fish.Length&&fish[id]?fish[id].transform.TransformPoint(targetOffsets[id]):DisplayPosition(id);
  public void Bind(FishingGame owner){game=owner;comparison=transform.Find("Lagoon presentation/Fish size comparison");lineMaterial=Resources.Load<Material>("FishingLine");}
  public static GameObject CloneVisual(Transform template,Transform parent,string name){
   var root=new GameObject(name);root.layer=9;root.transform.SetParent(parent,false);var map=new Dictionary<Renderer,Renderer>();
   foreach(var source in template.GetComponentsInChildren<MeshRenderer>(true)){
    var node=new GameObject(source.name);node.layer=9;node.transform.SetParent(root.transform,false);node.transform.localPosition=template.InverseTransformPoint(source.transform.position);node.transform.localRotation=Quaternion.Inverse(template.rotation)*source.transform.rotation;
    var a=source.transform.lossyScale;var b=template.lossyScale;node.transform.localScale=new Vector3(a.x/b.x,a.y/b.y,a.z/b.z);
    node.AddComponent<MeshFilter>().sharedMesh=source.GetComponent<MeshFilter>().sharedMesh;var r=node.AddComponent<MeshRenderer>();r.sharedMaterials=source.sharedMaterials;r.shadowCastingMode=source.shadowCastingMode;r.receiveShadows=source.receiveShadows;map[source]=r;
   }
   var lod=template.GetComponent<LODGroup>();if(lod){var group=root.AddComponent<LODGroup>();group.SetLODs(lod.GetLODs().Select(l=>new LOD(l.screenRelativeTransitionHeight,l.renderers.Where(map.ContainsKey).Select(r=>map[r]).ToArray())).ToArray());group.RecalculateBounds();}
   return root;
  }
  FishingPlayerRecord? Claim(int id){
   if(id<0||id>=game.State.Fish.Count)return null;var record=game.State.Fish[id];if(record.claimed)return game.State.Player(record.owner);
   foreach(var p in game.State.Players)if(p.fish==id&&p.phase==FishingPhase.Caught)return p;return null;
  }
  public Vector3 DisplayPosition(int id){
   var position=game.FishPosition(id,game.Now);var claimed=Claim(id);
   if(!claimed.HasValue)return position;var p=claimed.Value;var heading=Vector3.ProjectOnPlane(position-p.origin,Vector3.up).normalized;
   if(p.phase==FishingPhase.Reeling){
    var side=Vector3.Cross(Vector3.up,heading);position=p.origin+heading*p.distance+side*Mathf.Sin((float)game.Now*(5+p.surge*3)+id)*(.05f+p.surge*.14f);
    position.y=Mathf.Lerp(game.WaterY-.58f-FishingState.SizeForFish(id)*.14f,game.WaterY+.20f,p.progress*p.progress)+Mathf.Sin((float)game.Now*9+id)*(.035f+p.surge*.05f);
   }else if(p.phase==FishingPhase.Caught){
    float elapsed=(float)(game.Now-p.phaseAt);position=p.origin+heading*.85f+Vector3.up*(1.2f+Mathf.Sin(Mathf.Clamp01(elapsed/.8f)*Mathf.PI)*.28f);
   }
   return position;
  }
  void Update(){
   if(!game||!game.Context||game.State.Match.phase==FishingRoundPhase.Idle){Clear();return;}
   if(comparison)comparison.gameObject.SetActive(false);var view=PlayerView.Instance;var local=view?game.Player(view.target):null;int pier=view?game.NearestPier(view.target):-1;
   for(int i=0;i<fish.Length;i++){
    if(!fish[i]){var source=comparison?comparison.GetChild(FishingState.SizeForFish(i)):null;if(!source)continue;fish[i]=CloneVisual(source,transform,"Swimming fish "+i);var renderers=fish[i].GetComponentsInChildren<Renderer>();if(renderers.Length>0){var bounds=renderers[0].bounds;foreach(var r in renderers)bounds.Encapsulate(r.bounds);targetOffsets[i]=fish[i].transform.InverseTransformPoint(bounds.center);}}
    if(game.State.Fish.Count!=fish.Length)continue;
    var record=game.State.Fish[i];var p=Claim(i);bool landed=p.HasValue&&p.Value.phase==FishingPhase.Caught;bool visible=record.respawnAt<=game.Now||landed;fish[i].SetActive(visible);
    var position=DisplayPosition(i);var tangent=game.FishPosition(i,game.Now+.06)-game.FishPosition(i,game.Now);
    if(p.HasValue&&(p.Value.phase==FishingPhase.Reeling||landed)){tangent=p.Value.origin-position;tangent.y=0;float tilt=landed?Mathf.Sin((float)game.Now*12)*22:Mathf.Sin((float)game.Now*9)*p.Value.surge*12;fish[i].transform.SetPositionAndRotation(position,(tangent.sqrMagnitude>.000001f?Quaternion.LookRotation(tangent):Quaternion.identity)*Quaternion.Euler(0,0,tilt));}
    else fish[i].transform.SetPositionAndRotation(position,tangent.sqrMagnitude>.000001f?Quaternion.LookRotation(tangent):Quaternion.identity);
    bool selected=view&&view.SelectedFishingFish==i;bool nearby=FishingState.PierForFish(i)==pier&&local.HasValue&&local.Value.phase==FishingPhase.Ready&&game.State.Running;
    if(nearby&&visible){
     if(!halos[i]){halos[i]=Line("Fish target ring "+i,transform,.018f);halos[i].loop=true;halos[i].positionCount=24;}
     var ring=halos[i];ring.enabled=game.VisibleTarget(view.target,view.FishingCamera,i);float radius=selected?.45f+.035f*Mathf.Sin(Time.unscaledTime*6):.24f;
     ring.startWidth=ring.endWidth=selected?.05f:.018f;Color color=selected?new Color(.13f,.95f,1,1):new Color(.68f,1,1,.5f);if(!game.Available(view.target,i))color=new Color(.68f,.71f,.73f,.6f);ring.startColor=ring.endColor=color;
     var centre=position;centre.y=game.WaterHeight(centre)+.025f;
     for(int j=0;j<ring.positionCount;j++){float angle=j*Mathf.PI*2/ring.positionCount;ring.SetPosition(j,centre+new Vector3(Mathf.Cos(angle)*radius,0,Mathf.Sin(angle)*radius));}
    }else if(halos[i])halos[i].enabled=false;
   }
   foreach(var actor in Athlete.Active.ToArray()){
    if(!actor||!actor.isActiveAndEnabled)continue;var p=game.Player(actor);if(!p.HasValue||!p.Value.connected)continue;
    ulong id=FishingGame.Key(actor);if(!rods.TryGetValue(id,out var motion)||!motion){motion=actor.GetComponent<FishingRodMotion>()??actor.gameObject.AddComponent<FishingRodMotion>();motion.Bind(actor,this);rods[id]=motion;}
   }
  }
  public Transform RodTemplate(ulong owner){var rack=transform.Find("Fishing Rods");if(!rack)return null;return rack.Find(owner%2==0?"BlueLime":"TealOrange");}
  public Vector3 HookPoint(FishingPlayerRecord player){
   var p=player.phase==FishingPhase.Reeling||player.phase==FishingPhase.Caught?DisplayPosition(player.fish):game.FishPosition(player.fish,game.Now);
   if(player.phase!=FishingPhase.Caught)p.y=game.WaterHeight(p)+.03f;return p;
  }
  public LineRenderer Line(string name,Transform parent,float width){var go=new GameObject(name);go.layer=2;go.transform.SetParent(parent,false);var line=go.AddComponent<LineRenderer>();line.sharedMaterial=lineMaterial;line.startWidth=line.endWidth=width;line.startColor=line.endColor=new Color(.94f,.88f,.52f);line.shadowCastingMode=ShadowCastingMode.Off;line.receiveShadows=false;line.positionCount=12;return line;}
  public void Clear(){foreach(var f in fish)if(f)f.SetActive(false);foreach(var h in halos)if(h)h.enabled=false;foreach(var r in rods.Values)if(r)r.Stow();if(comparison)comparison.gameObject.SetActive(true);}
  void OnDisable(){Clear();}
 }
}
