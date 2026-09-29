using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace WhatTheFish {
 // Explicit instancing avoids thousands of individual plant render submissions
 // under the SRP batcher. Meshes, materials and authored placements are shared.
 public sealed class MobileVegetationInstances:MonoBehaviour {
  [Serializable] public sealed class Level {public Mesh mesh;public Material[] materials;public float threshold;}
  [Serializable] public sealed class Prototype {public Level[] levels;public Vector3 center;public float size;}
  [Serializable] public struct Placement {public int prototype;public Matrix4x4 matrix;public float scale;}
  public Prototype[] prototypes;
  public Placement[] placements;
  public int DrawSubmissions {get;private set;}
  sealed class Batch {public Matrix4x4[] matrices;public int count;}
  Batch[][] batches;Matrix4x4[] world;Vector3[] centers;Matrix4x4 previous;Camera view;
  void OnEnable(){
   view=Camera.main;world=new Matrix4x4[placements.Length];centers=new Vector3[placements.Length];batches=new Batch[prototypes.Length][];
   var counts=new int[prototypes.Length];foreach(var p in placements)counts[p.prototype]++;
   for(int i=0;i<prototypes.Length;i++){batches[i]=new Batch[prototypes[i].levels.Length];for(int j=0;j<batches[i].Length;j++)batches[i][j]=new Batch{matrices=new Matrix4x4[counts[i]]};}
   RefreshMatrices();
  }
  void RefreshMatrices(){previous=transform.localToWorldMatrix;for(int i=0;i<placements.Length;i++){world[i]=previous*placements[i].matrix;centers[i]=world[i].MultiplyPoint3x4(prototypes[placements[i].prototype].center);}}
  void LateUpdate(){
   if(!view){view=Camera.main;if(!view)return;}
   RenderForCamera(view);
  }
  public void RenderForCamera(Camera camera){
   if(batches==null||!camera)return;
   if(previous!=transform.localToWorldMatrix)RefreshMatrices();
   foreach(var levels in batches)foreach(var batch in levels)batch.count=0;
   float projection=2*Mathf.Tan(camera.fieldOfView*.5f*Mathf.Deg2Rad);var eye=camera.transform.position;
   for(int i=0;i<placements.Length;i++){
    var p=placements[i];var prototype=prototypes[p.prototype];float distance=Mathf.Max(.01f,Vector3.Distance(eye,centers[i]));
    float height=prototype.size*p.scale*QualitySettings.lodBias/(distance*projection);
    // Distance LODs keep off-screen shadow casters; Unity culls each draw batch.
    for(int level=0;level<prototype.levels.Length;level++)if(height>=prototype.levels[level].threshold){var batch=batches[p.prototype][level];batch.matrices[batch.count++]=world[i];break;}
   }
   DrawSubmissions=0;
   for(int i=0;i<prototypes.Length;i++)for(int level=0;level<batches[i].Length;level++){
    var batch=batches[i][level];if(batch.count==0)continue;var source=prototypes[i].levels[level];
    for(int sub=0;sub<source.materials.Length;sub++){
     var parameters=new RenderParams(source.materials[sub]){camera=camera,layer=10,shadowCastingMode=ShadowCastingMode.On,receiveShadows=true,lightProbeUsage=LightProbeUsage.Off};
     if(SystemInfo.supportsInstancing){
      // 256 remains below URP's per-draw matrix limit on GLES and Vulkan.
      for(int start=0;start<batch.count;start+=256){Graphics.RenderMeshInstanced(parameters,source.mesh,sub,batch.matrices,Math.Min(256,batch.count-start),start);DrawSubmissions++;}
     }else for(int instance=0;instance<batch.count;instance++){Graphics.RenderMesh(parameters,source.mesh,sub,batch.matrices[instance]);DrawSubmissions++;}
    }
   }
  }
 }
}
