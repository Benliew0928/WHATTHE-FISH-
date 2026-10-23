using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace WhatTheFish {
 // Explicit offline design preview; future host controls will use the same presentation.
 public sealed class TeleportPreview:MonoBehaviour {
  public static TeleportPreview Instance {get;private set;}
  readonly List<Athlete> extras=new();AppRoot app;bool panel=true,fullGroup;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Init(){if(Environment.GetCommandLineArgs().Contains("-teleportPreview")&&!Instance)new GameObject("Teleport preview controls").AddComponent<TeleportPreview>();}
  IEnumerator Start(){Instance=this;while(!AppRoot.Instance||!AppRoot.Instance.LocalAthlete||!SkySailWorld.Instance||!AppRoot.Instance.Menu)yield return null;app=AppRoot.Instance;if(!app.rooms.Connected&&!app.Exploring)app.EnterOffline();TeleportSequence.Ensure();}
  public Athlete[] Group(int count){
   if(TeleportSequence.Busy)throw new InvalidOperationException("Wait for the current preview.");
   if(extras.Count==count-1&&extras.All(a=>a))return new[]{app.LocalAthlete}.Concat(extras).ToArray();
   foreach(var actor in extras)if(actor){actor.gameObject.SetActive(false);Destroy(actor.gameObject);}extras.Clear();
   int localNumber=app.LocalAthlete.GetComponent<CharacterIdentity>().Number;
   for(int i=1;i<count;i++){var actor=Instantiate(app.athletePrefab).GetComponent<Athlete>();actor.name="Teleport preview character "+(i+1);actor.Setup();actor.GetComponent<CharacterIdentity>().SetNumber((byte)(1+(localNumber-1+i)%10));actor.capsule.enabled=false;actor.transform.position=app.LocalAthlete.transform.position+new Vector3((i%5)*1.8f,0,-(i/5+1)*2);actor.capsule.enabled=true;extras.Add(actor);}
   return new[]{app.LocalAthlete}.Concat(extras).ToArray();
  }
  public bool Play(SportId sport,TeleportDestination destination,bool maximum){
   if(!app||!app.Exploring||app.rooms.Connected||TeleportSequence.Busy||SkySailWorld.Instance&&SkySailWorld.Instance.Travelling)return false;
   // Reject before touching practice actors when a match or its result is active.
   if(FootballMatch.Instance&&FootballMatch.Instance.Active||BasketballMatch.Instance&&BasketballMatch.Instance.Active||GolfMatchManager.Instance&&GolfMatchManager.Instance.State.Running||FishingGame.Instance&&(FishingGame.Instance.State.Match.phase==FishingRoundPhase.Playing||FishingGame.Instance.State.Preparing)||FootballMVP.Presenting||BasketballMVP.Presenting)return false;
   FootballBots.Instance?.Clear();BasketballBots.Instance?.Clear();
   return TeleportSequence.Ensure().StartPreview(sport,destination,Group(maximum?TeleportDestinations.Capacity(sport):1));
  }
  void Update(){if(Input.GetKeyDown(KeyCode.T)&&!TeleportSequence.Busy)panel=!panel;}
  void OnGUI(){
   if(!app)return;float scale=Mathf.Min(Screen.width/1100f,Screen.height/680f);GUI.matrix=Matrix4x4.Scale(Vector3.one*scale);var service=TeleportSequence.Instance;
   if(TeleportSequence.Busy){GUI.Box(new Rect(20,20,350,74),"TELEPORT · "+service.Phase);if(GUI.Button(new Rect(32,54,326,28),"Cancel preview"))service.Cancel();return;}
   if(!panel){if(GUI.Button(new Rect(20,20,240,38),"Teleport preview"))panel=true;return;}
   GUI.Box(new Rect(20,20,390,350),"BLUE LIGHT TELEPORT · DESIGN PREVIEW");fullGroup=GUI.Toggle(new Rect(36,58,350,30),fullGroup,"Preview full island capacity (10 / fishing 5)");
   int row=0;foreach(var sport in SkySailMap.Circuit){float y=100+row++*48;GUI.Label(new Rect(36,y+6,110,30),sport.ToString());if(GUI.Button(new Rect(146,y,116,36),"Station"))Play(sport,TeleportDestination.Station,fullGroup);if(GUI.Button(new Rect(274,y,116,36),"Game"))Play(sport,TeleportDestination.Game,fullGroup);}
   GUI.Label(new Rect(36,296,355,40),service&&service.Error.Length>0?service.Error:"Walk anywhere, then choose an arrival.");if(GUI.Button(new Rect(36,335,355,25),"Hide controls"))panel=false;
  }
  void OnDestroy(){foreach(var actor in extras)if(actor)Destroy(actor.gameObject);if(Instance==this)Instance=null;}
 }
}
