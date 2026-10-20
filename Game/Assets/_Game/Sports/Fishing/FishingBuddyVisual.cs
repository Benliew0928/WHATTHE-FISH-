using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
namespace WhatTheFish {
 // A render-only live pose copy. It has no player, AI, network object or collider.
 internal sealed class FishingBuddyVisual {
  public readonly Athlete Source;public readonly Transform Root;
  readonly Transform[] originals,copies;readonly List<Renderer> renderers=new();readonly List<int> materialCounts=new();readonly MaterialPropertyBlock block=new();
  GameObject rod;Transform sourceRod;Vector3 offset;
  public FishingBuddyVisual(Athlete actor,Transform parent,Material material){
   Source=actor;Root=new GameObject("Monochrome fishing buddy "+FishingGame.Key(actor)).transform;Root.SetParent(parent,false);
   originals=actor.visual.GetComponentsInChildren<Transform>(true);copies=new Transform[originals.Length];var map=new Dictionary<Transform,Transform>();var rendererMap=new Dictionary<Renderer,Renderer>();
   for(int i=0;i<originals.Length;i++){var t=originals[i];var c=copies[i]=new GameObject(t.name).transform;c.SetParent(t==actor.visual?Root:map[t.parent],false);c.localPosition=t==actor.visual?Vector3.zero:t.localPosition;c.localRotation=t.localRotation;c.localScale=t==actor.visual?t.lossyScale:t.localScale;map[t]=c;}
   foreach(var original in actor.visual.GetComponentsInChildren<Renderer>(true)){
    Renderer copy=null;var go=map[original.transform].gameObject;
    if(original is SkinnedMeshRenderer skin&&skin.sharedMesh){var clone=go.AddComponent<SkinnedMeshRenderer>();clone.sharedMesh=skin.sharedMesh;clone.bones=Array.ConvertAll(skin.bones,b=>b&&map.TryGetValue(b,out var c)?c:null);clone.rootBone=skin.rootBone&&map.TryGetValue(skin.rootBone,out var rb)?rb:Root;clone.localBounds=skin.localBounds;clone.updateWhenOffscreen=true;for(int k=0;k<skin.sharedMesh.blendShapeCount;k++)clone.SetBlendShapeWeight(k,skin.GetBlendShapeWeight(k));copy=clone;}
    else if(original is MeshRenderer&&original.TryGetComponent<MeshFilter>(out var filter)){go.AddComponent<MeshFilter>().sharedMesh=filter.sharedMesh;copy=go.AddComponent<MeshRenderer>();}
    if(!copy)continue;ApplyMaterials(original,copy,material);rendererMap[original]=copy;
   }
   foreach(var original in actor.visual.GetComponentsInChildren<LODGroup>(true)){var group=map[original.transform].gameObject.AddComponent<LODGroup>();var lods=original.GetLODs();for(int i=0;i<lods.Length;i++)lods[i].renderers=Array.ConvertAll(lods[i].renderers,r=>r&&rendererMap.TryGetValue(r,out var c)?c:null);group.SetLODs(lods);group.localReferencePoint=original.localReferencePoint;group.size=original.size;}
   var motion=actor.GetComponent<FishingRodMotion>();sourceRod=motion?motion.HeldRod:null;
   if(sourceRod){rod=FishingPresentation.CloneVisual(sourceRod,Root,"Helper fishing rod");var from=sourceRod.GetComponentsInChildren<Renderer>();var to=rod.GetComponentsInChildren<Renderer>();for(int i=0;i<to.Length;i++)ApplyMaterials(from[Mathf.Min(i,from.Length-1)],to[i],material);}
   Root.gameObject.SetActive(false);
  }
  void ApplyMaterials(Renderer source,Renderer target,Material material){
   var originals=source.sharedMaterials;var materials=new Material[originals.Length];for(int i=0;i<materials.Length;i++)materials[i]=material;target.sharedMaterials=materials;for(int i=0;i<materials.Length;i++){var old=originals[i];block.Clear();if(old){var texture=old.HasProperty("_BaseMap")?old.GetTexture("_BaseMap"):old.HasProperty("_MainTex")?old.GetTexture("_MainTex"):null;if(texture)block.SetTexture("_BaseMap",texture);}block.SetFloat("_Opacity",.82f);target.SetPropertyBlock(block,i);}target.shadowCastingMode=ShadowCastingMode.Off;target.receiveShadows=false;target.enabled=true;renderers.Add(target);materialCounts.Add(materials.Length);
  }
  public void Tick(FishingPlayerRecord p,double now,float fade){
   Root.gameObject.SetActive(fade>0);if(fade<=0)return;offset=Source.transform.right*1.4f-Source.transform.forward*.25f;Root.SetPositionAndRotation(Source.transform.position+offset,Source.transform.rotation);
   for(int i=0;i<originals.Length;i++){if(!originals[i])continue;copies[i].gameObject.SetActive(originals[i].gameObject.activeSelf);copies[i].localPosition=originals[i].localPosition;copies[i].localRotation=originals[i].localRotation;copies[i].localScale=i==0?originals[i].lossyScale:originals[i].localScale;if(originals[i].name=="mixamorig:Head"&&copies[i].localScale.sqrMagnitude<.01f)copies[i].localScale=Vector3.one;}
   if(rod&&sourceRod)rod.transform.SetPositionAndRotation(sourceRod.position+offset,sourceRod.rotation);
   for(int r=0;r<renderers.Count;r++){var renderer=renderers[r];if(!renderer)continue;for(int i=0;i<materialCounts[r];i++){renderer.GetPropertyBlock(block,i);block.SetFloat("_Opacity",.82f*fade);renderer.SetPropertyBlock(block,i);}}
  }
  public Vector3 LineTip=>Source.GetComponent<FishingRodMotion>().Tip+offset;
  public void Hide(){if(Root)Root.gameObject.SetActive(false);}
  public void Destroy(){if(Root)UnityEngine.Object.Destroy(Root.gameObject);}
 }
}
