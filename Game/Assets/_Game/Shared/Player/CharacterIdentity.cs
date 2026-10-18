using System;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace WhatTheFish {
 [DefaultExecutionOrder(180)]
 public sealed class CharacterIdentity:MonoBehaviour {
  public static readonly string[] Names={"Swept quiff","Rounded crop","Rounded curls","Flat top","Crest","Ponytail","High bun","Twin puffs","Cap","Bucket hat"};
  public static readonly Color Blue=LocalProfile.Hex("176BCC"),Red=LocalProfile.Hex("D83B40");
  static readonly string[] Personal={"259F91","DA8B31","7858B9","508565","BD5480","398BA3","AF693B","646BBC","477873","A87B38"};
  static readonly string[] Hair={"10535C","362B29","513021","24232A","824A2D","332824","B27532","412A25","283542","463A29"};
  static readonly string[] Skin={"FFC28E","D69464","965F43","F4B27D","E2A172","BA7B53","FFD0A2","A76B49","EFB280","C88D61"};
  static readonly IdentitySlots offline=new();
  static CharacterIdentityLibrary library;
  static readonly Material[,] bodies=new Material[10,3];
  static readonly Material[] hairs=new Material[10];
  static readonly Material[] scalps=new Material[10];
  Athlete actor;SkinnedMeshRenderer[] body;MeshRenderer near,far,nearScalp,farScalp;Transform head;bool hidden,bound;
  int applied=-1,appliedTeam=-1;byte number;ulong offlineKey;
  public byte Number=>number;
  public int Preset=>Mathf.Max(0,number-1);
  public FootballTeam UniformTeam=>ResolveTeam(actor);
  public bool OriginalHair=>Preset==0;
  public Renderer[] AddedRenderers=>new Renderer[]{near,far,nearScalp,farScalp};
  public static CharacterIdentity Ensure(Athlete actor){var value=actor.GetComponent<CharacterIdentity>();if(!value)value=actor.gameObject.AddComponent<CharacterIdentity>();value.Bind(actor);return value;}
  void Bind(Athlete value){
   if(bound||!value.visual)return;actor=value;
   if(!library)library=actor.identityLibrary;
   if(!library){Debug.LogError("Character identity library missing");return;}
   body=actor.visual.GetComponentsInChildren<SkinnedMeshRenderer>(true);
   head=actor.visual.GetComponentsInChildren<Transform>(true).FirstOrDefault(t=>t.name=="mixamorig:Head");
   if(!head)return;
   MeshRenderer Make(string name,out MeshFilter filter){var g=new GameObject(name);g.transform.SetParent(head,false);filter=g.AddComponent<MeshFilter>();return g.AddComponent<MeshRenderer>();}
   near=Make("Identity hair near",out _);far=Make("Identity hair distant",out _);
   nearScalp=Make("Identity scalp near",out var scalpNear);farScalp=Make("Identity scalp distant",out var scalpFar);scalpNear.sharedMesh=library.nearScalp;scalpFar.sharedMesh=library.farScalp;
   var lod=actor.visual.GetComponent<LODGroup>();
   if(lod){var levels=lod.GetLODs();for(int i=0;i<levels.Length;i++)levels[i].renderers=levels[i].renderers.Concat(new Renderer[]{i==0?near:far,i==0?nearScalp:farScalp}).ToArray();lod.SetLODs(levels);lod.RecalculateBounds();}
   bound=true;offlineKey=unchecked((ulong)(long)actor.GetInstanceID());
   CharacterIdentityBadges.Ensure();
  }
  void OnDisable(){offline.Release(offlineKey);if(!GetComponent<NetworkAthlete>())number=0;}
  void OnDestroy()=>offline.Release(offlineKey);
  public void SetNumber(byte value){if(value<1||value>10)return;number=value;Refresh();}
  public void SetHidden(bool value){hidden=value;bool show=!hidden&&Preset!=0;if(near)near.enabled=show;if(far)far.enabled=show;if(nearScalp)nearScalp.enabled=show;if(farScalp)farScalp.enabled=show;}
  void LateUpdate(){
   if(!bound){var a=GetComponent<Athlete>();if(a)Bind(a);if(!bound)return;}
   var net=GetComponent<NetworkAthlete>();
   if(net&&net.IsSpawned){offline.Release(offlineKey);if(net.IdentityNumber.Value!=0)number=net.IdentityNumber.Value;}
   else if(!(NetworkManager.Singleton&&NetworkManager.Singleton.IsListening)&&number==0){number=offline.Reserve(offlineKey,actor==AppRoot.Instance?.LocalAthlete?LocalProfile.Character.preset:0);}
   if(number>0)Refresh();
   // Fishing scales the head out of the first-person view; attachment transforms follow it.
   SetHidden(hidden);
  }
  public static FootballTeam ResolveTeam(Athlete actor){
   var app=AppRoot.Instance;if(!actor||actor.IsSpectator||!app||!app.Exploring||actor.inTransit)return FootballTeam.None;
   if(app.SelectedSport==SportId.Football){var m=FootballMatch.Instance;if(m&&m.Context&&m.Snapshot.phase!=FootballMatchPhase.Idle&&m.Snapshot.phase!=FootballMatchPhase.Cancelled)return m.TeamOf(actor);}
   if(app.SelectedSport==SportId.Basketball){var m=BasketballMatch.Instance;if(m&&m.Context&&m.Snapshot.phase!=BasketballMatchPhase.Idle&&m.Snapshot.phase!=BasketballMatchPhase.Cancelled)return m.TeamOf(actor);}
   return FootballTeam.None;
  }
  public static Color TeamColor(FootballTeam team)=>team==FootballTeam.A?Blue:team==FootballTeam.B?Red:Color.white;
  public static string TeamName(FootballTeam team)=>team==FootballTeam.A?"Blue":team==FootballTeam.B?"Red":"Unassigned";
  // Retained result records may outlive their network actor. A render-only
  // copy can still use the recorded identity without changing a live player.
  internal static void ApplyHonoursIdentity(Transform copy,byte number,FootballTeam uniform){
   if(!library)return;int preset=Mathf.Clamp(number-1,0,9),team=(int)uniform;
   if(!bodies[preset,team]){var material=new Material(library.bodyMaterial){name="Identity "+number+" "+uniform};material.SetColor("_JerseyColor",team==0?LocalProfile.Hex(Personal[preset]):TeamColor(uniform));material.SetColor("_SkinTint",LocalProfile.Hex(Skin[preset]));material.SetColor("_AccentColor",LocalProfile.Hex(Personal[preset]));material.SetColor("_HairColor",LocalProfile.Hex(Hair[preset]));material.SetFloat("_IdentityNumber",number);material.SetFloat("_ShowOriginalHair",preset==0?1:0);material.SetFloat("_Team",team);bodies[preset,team]=material;}
   foreach(var skin in copy.GetComponentsInChildren<SkinnedMeshRenderer>(true))skin.sharedMaterial=bodies[preset,team];
   if(!hairs[preset]){hairs[preset]=new Material(library.hairMaterial){name="Identity hair "+number};hairs[preset].SetColor("_BaseColor",LocalProfile.Hex(Hair[preset]));}
   if(!scalps[preset]){scalps[preset]=new Material(library.hairMaterial){name="Identity scalp "+number};scalps[preset].SetColor("_BaseColor",LocalProfile.Hex(Skin[preset]));scalps[preset].SetFloat("_Smoothness",.2f);}
   foreach(var mesh in copy.GetComponentsInChildren<MeshFilter>(true)){
    bool scalp=mesh.name.StartsWith("Identity scalp"),hair=mesh.name.StartsWith("Identity hair");if(!scalp&&!hair)continue;
    mesh.sharedMesh=preset==0?null:scalp?library.nearScalp:mesh.name.EndsWith("near")?library.nearHair[preset]:library.farHair[preset];mesh.GetComponent<Renderer>().sharedMaterial=scalp?scalps[preset]:hairs[preset];
   }
  }
  public void Refresh(){
   if(!bound||number==0)return;int preset=Preset,team=(int)UniformTeam;
   if(applied==preset&&appliedTeam==team)return;applied=preset;appliedTeam=team;
   var material=bodies[preset,team];
   if(!material){material=new Material(library.bodyMaterial){name="Identity "+number+" "+UniformTeam};
    material.SetColor("_JerseyColor",team==0?LocalProfile.Hex(Personal[preset]):TeamColor(UniformTeam));
    material.SetColor("_SkinTint",LocalProfile.Hex(Skin[preset]));material.SetColor("_AccentColor",LocalProfile.Hex(Personal[preset]));
    material.SetColor("_HairColor",LocalProfile.Hex(Hair[preset]));
    material.SetFloat("_IdentityNumber",number);material.SetFloat("_ShowOriginalHair",preset==0?1:0);material.SetFloat("_Team",team);bodies[preset,team]=material;
   }
   foreach(var r in body)r.sharedMaterial=material;
   if(preset!=0){
    near.GetComponent<MeshFilter>().sharedMesh=library.nearHair[preset];far.GetComponent<MeshFilter>().sharedMesh=library.farHair[preset];
    nearScalp.GetComponent<MeshFilter>().sharedMesh=library.nearScalp;farScalp.GetComponent<MeshFilter>().sharedMesh=library.farScalp;
    if(!hairs[preset]){hairs[preset]=new Material(library.hairMaterial){name="Identity hair "+number};hairs[preset].SetColor("_BaseColor",LocalProfile.Hex(Hair[preset]));}
    near.sharedMaterial=far.sharedMaterial=hairs[preset];
    if(!scalps[preset]){scalps[preset]=new Material(library.hairMaterial){name="Identity scalp "+number};scalps[preset].SetColor("_BaseColor",LocalProfile.Hex(Skin[preset]));scalps[preset].SetFloat("_Smoothness",.2f);}
    nearScalp.sharedMaterial=farScalp.sharedMaterial=scalps[preset];
   }else foreach(var renderer in AddedRenderers)renderer.GetComponent<MeshFilter>().sharedMesh=null;
   actor.visual.GetComponent<LODGroup>()?.RecalculateBounds();SetHidden(hidden);
  }
 }
}
