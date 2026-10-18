#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WhatTheFish {
 public sealed partial class SkySailReview {
  IEnumerator GeometryReview(SkySailStreaming streaming){
   // Cook the delivered LOD0 triangles, not the simplified gameplay colliders.
   var probes=new List<GameObject>();
   foreach(var root in world.stations.Select(s=>s.transform).Concat(world.scenery.Cast<Transform>().Where(t=>t.name.EndsWith(" · Tower")))){
    var lod=root.GetComponent<LODGroup>();
    foreach(var renderer in lod.GetLODs()[0].renderers){
     var mesh=renderer.GetComponent<MeshFilter>();if(!mesh)continue;
     var probe=new GameObject(root.name+" / "+renderer.name){layer=30};probe.transform.SetParent(renderer.transform,false);probe.AddComponent<MeshCollider>().sharedMesh=mesh.sharedMesh;probes.Add(probe);
    }
   }
   Physics.SyncTransforms();
   foreach(var source in SkySailMap.Circuit)foreach(int direction in new[]{-1,1}){
    var target=SkySailMap.Neighbor(source,direction);string collision=null;float symmetry=0,turn=0;var last=SkySailMap.CabinRotation(source,target,0);
    for(int i=0;i<=2000;i++){
     float t=i/2000f;var position=SkySailMap.Path(source,target,t)-world.Origin;var rotation=SkySailMap.CabinRotation(source,target,t);
     symmetry=Mathf.Max(symmetry,Vector3.Distance(position+world.Origin,SkySailMap.Path(target,source,1-t)));
     turn=Mathf.Max(turn,Quaternion.Angle(last,rotation));last=rotation;
     // Body and suspension below the intentional wheel/cable contact strip.
     var hits=Physics.OverlapBox(position+Vector3.up*1.65f,new Vector3(3.14f,2.08f,3.85f),rotation,1<<30);
     if(hits.Length==0)hits=Physics.OverlapBox(position+Vector3.up*5.05f,new Vector3(.36f,1.30f,1.40f),SkySailMap.HangerRotation(source,target,t),1<<30);
     if(hits.Length>0&&collision==null)collision="t="+t+" "+hits[0].name;
    }
    Check(collision==null,source+" → "+target+" 2,001 body/hanger swept clearances"+(collision==null?"":" COLLISION "+collision));
    Check(symmetry<.001f,source+" → "+target+" reverse route shares cable (max "+symmetry+")");
    Check(turn<3,source+" → "+target+" continuous heading (max step "+turn+")");
    Check(Quaternion.Angle(SkySailMap.CabinRotation(source,target,.02f),SkySailMap.Facing(source))<.01f&&Quaternion.Angle(SkySailMap.CabinRotation(source,target,.98f),SkySailMap.Facing(target))<.01f,source+" → "+target+" aligned throughout station throat");
   }
   foreach(var probe in probes)Destroy(probe);yield return null;
   bool passengerClearance=true;
   for(int count=1;count<=20;count++)for(int i=0;i<count;i++){
    var p=SkySailWorld.PassengerAnchor(i,count);passengerClearance&=Mathf.Abs(p.x)+.4f<1.72f;
    foreach(float z in new[]{-1.8f,1.8f})passengerClearance&=Vector2.Distance(new Vector2(p.x,p.z),new Vector2(.9f,z))>.45f;
    for(int j=0;j<i;j++)passengerClearance&=Vector3.Distance(p,SkySailWorld.PassengerAnchor(j,count))>.8f;
   }
   Check(passengerClearance,"All party sizes 1–20 clear seats, poles and adjacent passenger anchors");
   foreach(var source in SkySailMap.Circuit){
    yield return streaming.Prepare(source);app.SelectSport(source);yield return null;Physics.SyncTransforms();
    bool supported=true;
    for(int i=0;i<20;i++){var p=RoomSeats.StationLanding(source,i).position;supported&=Physics.Raycast(p+Vector3.up*.3f,Vector3.down,.6f,1<<8);}
    Check(supported,source+" all 20 player/spectator landings on boarding apron");
    foreach(int direction in new[]{-1,1}){
     var target=SkySailMap.Neighbor(source,direction);string blocked=null;
     for(int i=0;i<=400;i++){
      float t=i/1600f;var p=SkySailMap.Path(source,target,t)-world.Origin;var q=SkySailMap.CabinRotation(source,target,t);
      // Layer 9 contains invisible walking containment over open water. The
      // cabin intentionally crosses it with passenger controllers disabled.
      var hit=Physics.OverlapBox(p+Vector3.up*1.65f,new Vector3(3.14f,2.08f,3.85f),q,1<<8,QueryTriggerInteraction.Ignore).FirstOrDefault(c=>!c.transform.IsChildOf(world.cabin));
      if(hit){blocked="t="+t+" "+hit.name;break;}
     }
     Check(blocked==null,source+" → "+target+" departure clears island terrain and gameplay obstacles"+(blocked==null?"":" "+blocked));
    }
    Place(SkySailMap.Port(source)+SkySailMap.Facing(source)*new Vector3(0,.85f,-4.5f));
    yield return WalkTo(world.cabin.TransformPoint(new Vector3(0,.06f,-1.4f)),source+" walk through open doorway onto cabin floor");
    yield return WalkTo(SkySailMap.Port(source)+SkySailMap.Facing(source)*new Vector3(0,.85f,-4.5f),source+" walk out onto platform");
    var station=world.stations[(int)source].transform;
    camera.transform.position=station.TransformPoint(new Vector3(14,10,20));camera.transform.LookAt(station.TransformPoint(new Vector3(0,4,-2)));camera.fieldOfView=60;yield return null;Capture(source+"_Station");
    foreach(var view in new[]{("Interior",new Vector3(0,1.7f,-2.6f),new Vector3(0,1.6f,2)),("Doorway",new Vector3(0,1.8f,1.5f),new Vector3(0,1.6f,-4)),("Ceiling",new Vector3(0,1.5f,-2.6f),new Vector3(0,3.4f,0))}){
     camera.transform.position=world.cabin.TransformPoint(view.Item2);camera.transform.LookAt(world.cabin.TransformPoint(view.Item3));yield return null;Capture(source+"_"+view.Item1);
    }
   }
   // Stage exact production poses for close departure, tower and arrival views.
   enabled=false;world.enabled=false;
   foreach(var source in SkySailMap.Circuit){
    var target=SkySailMap.Neighbor(source,1);world.SetIsland(source);
    foreach(float t in new[]{.02f,.04f,.07f,.18f,.82f,.93f,.98f}){
     world.cabin.localPosition=SkySailMap.Path(source,target,t);world.cabin.localRotation=SkySailMap.CabinRotation(source,target,t);
     world.cabin.Find("CabinHanger").localRotation=Quaternion.Inverse(world.cabin.localRotation)*SkySailMap.HangerRotation(source,target,t);
     world.cabin.Find("CabinTrolley").localRotation=Quaternion.Inverse(world.cabin.localRotation)*Quaternion.LookRotation(SkySailMap.Tangent(source,target,t));
     camera.transform.position=world.cabin.position+world.cabin.rotation*new Vector3(13,9,17);camera.transform.LookAt(world.cabin.position+Vector3.up*3);yield return null;Capture(source+"_Route_"+t.ToString("F2",System.Globalization.CultureInfo.InvariantCulture));
    }
   }
  }
 }
}
#endif
