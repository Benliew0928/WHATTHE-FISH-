using System;
using System.Collections.Generic;
using UnityEngine;
namespace WhatTheFish {
 // A render-only copy. No Animator, script, collider, Rigidbody or NetworkObject
 // is instantiated, and playback never writes to the live simulation.
 internal sealed class FootballReplayTrack {
  struct Pose {public Vector3 position,scale;public Quaternion rotation;}
  sealed class Frame {
   public Pose[] poses;public bool[] visible;public Vector4[] contacts,impulses;public float clock;
  }
  readonly Transform[] source;readonly Renderer[] renderers;readonly bool athlete;
  Frame[] frames=new Frame[FootballReplay.Capacity],playback=new Frame[FootballReplay.Capacity];readonly FootballNetSurface net;
  Transform[] copies;Renderer[] draws;bool[] priorHidden;GameObject root;
  readonly MaterialPropertyBlock block=new();readonly Vector4[] shifted=new Vector4[FootballNetSurface.Capacity];
  public Transform Source=>source[0];
  public Vector3 Position(int frame,bool replay=false)=>(replay?playback:frames)[frame].poses[0].position;
  public void Freeze(){(frames,playback)=(playback,frames);}
  public FootballReplayTrack(Transform transform,bool player=false){source=transform.GetComponentsInChildren<Transform>(true);renderers=transform.GetComponentsInChildren<Renderer>(true);athlete=player;net=transform.GetComponent<FootballNetSurface>();}
  public void Record(int index){
   var f=frames[index];if(f==null){f=new Frame{poses=new Pose[source.Length],visible=new bool[renderers.Length]};if(net){f.contacts=new Vector4[4];f.impulses=new Vector4[4];}frames[index]=f;}
   for(int i=0;i<source.Length;i++){var t=source[i];if(!t)continue;f.poses[i]=new Pose{position=i==0?t.position:t.localPosition,rotation=i==0?t.rotation:t.localRotation,scale=i==0?t.lossyScale:t.localScale};}
   for(int i=0;i<renderers.Length;i++){var r=renderers[i];f.visible[i]=r&&r.gameObject.activeInHierarchy&&(athlete||r.enabled);}
   f.clock=Time.time;if(net)net.CopyReplay(f.contacts,f.impulses);
  }
  public void Create(Transform parent){
   var map=new Dictionary<Transform,Transform>();copies=new Transform[source.Length];draws=new Renderer[renderers.Length];priorHidden=new bool[renderers.Length];
   for(int i=0;i<source.Length;i++){var t=source[i];var copy=new GameObject(t.name).transform;copies[i]=copy;map.Add(t,copy);copy.SetParent(i==0?parent:map[t.parent],false);copy.gameObject.layer=t.gameObject.layer;if(i==0)root=copy.gameObject;}
   for(int i=0;i<renderers.Length;i++){
    var r=renderers[i];if(!r)continue;var go=map[r.transform].gameObject;Renderer clone=null;
    if(r is SkinnedMeshRenderer skin){var s=go.AddComponent<SkinnedMeshRenderer>();s.sharedMesh=skin.sharedMesh;s.bones=Array.ConvertAll(skin.bones,b=>b&&map.TryGetValue(b,out var mapped)?mapped:null);s.rootBone=skin.rootBone&&map.TryGetValue(skin.rootBone,out var bone)?bone:copies[0];s.localBounds=skin.localBounds;s.updateWhenOffscreen=true;s.quality=skin.quality;for(int b=0;b<skin.sharedMesh.blendShapeCount;b++)s.SetBlendShapeWeight(b,skin.GetBlendShapeWeight(b));clone=s;}
    else if(r is MeshRenderer&&r.TryGetComponent<MeshFilter>(out var filter)){go.AddComponent<MeshFilter>().sharedMesh=filter.sharedMesh;clone=go.AddComponent<MeshRenderer>();}
    if(clone){clone.sharedMaterials=r.sharedMaterials;clone.shadowCastingMode=r.shadowCastingMode;clone.receiveShadows=r.receiveShadows;r.GetPropertyBlock(block);clone.SetPropertyBlock(block);}
    draws[i]=clone;priorHidden[i]=r.forceRenderingOff;r.forceRenderingOff=true;
   }
   foreach(var group in source[0].GetComponentsInChildren<LODGroup>(true)){
    var clone=map[group.transform].gameObject.AddComponent<LODGroup>();var levels=group.GetLODs();
    for(int i=0;i<levels.Length;i++)levels[i].renderers=Array.ConvertAll(levels[i].renderers,r=>{int index=Array.IndexOf(renderers,r);return index>=0?draws[index]:null;});
    clone.SetLODs(levels);clone.localReferencePoint=group.localReferencePoint;clone.size=group.size;clone.fadeMode=group.fadeMode;clone.animateCrossFading=group.animateCrossFading;
   }
  }
  public void Present(int left,int right,float blend){
   var a=playback[left];var b=playback[right];if(a==null||b==null||!root)return;
   for(int i=0;i<copies.Length;i++){var t=copies[i];var p=a.poses[i];var q=b.poses[i];t.localPosition=Vector3.LerpUnclamped(p.position,q.position,blend);t.localRotation=Quaternion.SlerpUnclamped(p.rotation,q.rotation,blend);t.localScale=Vector3.LerpUnclamped(p.scale,q.scale,blend);}
   for(int i=0;i<draws.Length;i++){if(draws[i])draws[i].enabled=a.visible[i];if(renderers[i])renderers[i].forceRenderingOff=true;}
   if(net&&draws.Length>0&&draws[0]){
    float recordedClock=Mathf.Lerp(a.clock,b.clock,blend);for(int i=0;i<4;i++){shifted[i]=a.contacts[i];shifted[i].w+=Time.time-recordedClock;}
    block.Clear();block.SetVector("_NetSize",net.Dimensions);block.SetVectorArray("_Contacts",shifted);block.SetVectorArray("_Impulses",a.impulses);draws[0].SetPropertyBlock(block);
   }
  }
  public void Restore(){if(root)UnityEngine.Object.Destroy(root);if(priorHidden!=null)for(int i=0;i<renderers.Length;i++)if(renderers[i])renderers[i].forceRenderingOff=priorHidden[i];root=null;copies=null;draws=null;priorHidden=null;}
 }
}
