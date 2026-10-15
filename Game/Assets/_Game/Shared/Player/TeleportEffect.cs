using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace WhatTheFish {
 // One effect works with every skinned character, identity and hair mesh.
 // Source meshes/textures are shared; only short-lived material instances are needed.
 public sealed class TeleportEffect {
  sealed class Surface {public Renderer renderer;public Material[] original,effect;public MaterialPropertyBlock saved=new();public bool hidden;}
  readonly List<Surface> surfaces=new();readonly Dictionary<Material,Material> copies=new();readonly MaterialPropertyBlock block=new();
  readonly Athlete actor;readonly GameObject light;readonly Renderer column,ring;readonly Vector3 visualPosition;readonly float height;
  static Mesh columnMesh,ringMesh;
  public bool Supported {get;private set;}
  public float Absorption {get;private set;}
  public TeleportEffect(Athlete actor){
   this.actor=actor;visualPosition=actor.visual.localPosition;actor.HideHead(false);var template=Resources.Load<Material>("TeleportCharacter");var lightMaterial=Resources.Load<Material>("TeleportLight");Supported=template&&lightMaterial&&template.shader.isSupported&&lightMaterial.shader.isSupported;
   if(!Supported)return;
   var bounds=new Bounds(actor.transform.position+Vector3.up,Vector3.one);
   foreach(var renderer in actor.visual.GetComponentsInChildren<Renderer>(true)){
    if(!(renderer is SkinnedMeshRenderer)&&!(renderer is MeshRenderer))continue;
    var s=new Surface{renderer=renderer,original=renderer.sharedMaterials,hidden=renderer.forceRenderingOff};renderer.GetPropertyBlock(s.saved);s.effect=new Material[s.original.Length];
    for(int i=0;i<s.effect.Length;i++){var source=s.original[i];if(!source)continue;if(!copies.TryGetValue(source,out var copy)){copy=new Material(template);copy.CopyPropertiesFromMaterial(source);copy.SetFloat("_Identity",source.shader.name=="WhatTheFish/CharacterIdentity"?1:0);copy.SetFloat("_Head",source.shader.name=="WhatTheFish/CharacterHead"?1:0);copy.renderQueue=2000;copies.Add(source,copy);}s.effect[i]=copy;}
    surfaces.Add(s);if(renderer.enabled)bounds.Encapsulate(renderer.bounds);
   }
   height=Mathf.Clamp(bounds.max.y-actor.transform.position.y,1.6f,3.5f);
   light=new GameObject("Teleport blue light");column=Make("Spiral light",ColumnMesh(),lightMaterial);ring=Make("Landing rings",RingMesh(),lightMaterial);
  }
  Renderer Make(string name,Mesh mesh,Material material){var g=new GameObject(name);g.transform.SetParent(light.transform,false);g.AddComponent<MeshFilter>().sharedMesh=mesh;var renderer=g.AddComponent<MeshRenderer>();renderer.sharedMaterial=material;renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;return renderer;}
  public void Present(float absorption,float power,float seconds,bool hideCharacter=false){
   Absorption=Mathf.Clamp01(absorption);if(!Supported||!actor)return;light.SetActive(power>.001f);light.transform.position=actor.transform.position;
   light.transform.localScale=new Vector3(1,Mathf.Max(1,height/2),1);actor.visual.localPosition=visualPosition+Vector3.up*(Mathf.Sin(Absorption*Mathf.PI)*.20f);
   foreach(var s in surfaces)if(s.renderer){s.renderer.sharedMaterials=s.effect;s.renderer.forceRenderingOff=s.hidden||hideCharacter||Absorption>=.999f;s.renderer.GetPropertyBlock(block);block.SetFloat("_Amount",Absorption);block.SetFloat("_BaseY",actor.transform.position.y);block.SetFloat("_Height",height);block.SetFloat("_Glow",power);s.renderer.SetPropertyBlock(block);}
   block.Clear();block.SetFloat("_Power",power);block.SetFloat("_Phase",seconds);column.SetPropertyBlock(block);block.SetFloat("_Kind",1);ring.SetPropertyBlock(block);
  }
  public void Dispose(){foreach(var s in surfaces)if(s.renderer){s.renderer.sharedMaterials=s.original;s.renderer.forceRenderingOff=s.hidden;s.renderer.SetPropertyBlock(s.saved);}if(actor&&actor.visual)actor.visual.localPosition=visualPosition;foreach(var mat in copies.Values)Object.Destroy(mat);if(light)Object.Destroy(light);}
  static Mesh ColumnMesh(){
   if(columnMesh)return columnMesh;const int segments=48;var vertices=new Vector3[(segments+1)*2];var uv=new Vector2[vertices.Length];var triangles=new int[segments*6];
   for(int i=0;i<=segments;i++){float a=i*Mathf.PI*2/segments;for(int j=0;j<2;j++){vertices[i*2+j]=new Vector3(Mathf.Cos(a)*(j==0?.84f:.48f),j*3.2f,Mathf.Sin(a)*(j==0?.84f:.48f));uv[i*2+j]=new Vector2((float)i/segments,j);}if(i<segments){int t=i*6,v=i*2;triangles[t]=v;triangles[t+1]=v+1;triangles[t+2]=v+2;triangles[t+3]=v+1;triangles[t+4]=v+3;triangles[t+5]=v+2;}}
   columnMesh=new Mesh{name="Shared teleport light shell",vertices=vertices,uv=uv,triangles=triangles};columnMesh.RecalculateBounds();return columnMesh;
  }
  static Mesh RingMesh(){if(ringMesh)return ringMesh;ringMesh=new Mesh{name="Shared teleport ring",vertices=new[]{new Vector3(-1.12f,.025f,-1.12f),new Vector3(-1.12f,.025f,1.12f),new Vector3(1.12f,.025f,1.12f),new Vector3(1.12f,.025f,-1.12f)},uv=new[]{Vector2.zero,Vector2.up,Vector2.one,Vector2.right},triangles=new[]{0,1,2,0,2,3}};ringMesh.RecalculateBounds();return ringMesh;}
 }
}
