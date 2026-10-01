using System;
using System.Linq;
using System.Collections;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace WhatTheFish {
 [DefaultExecutionOrder(-50)]
 public sealed class SkySailWorld:MonoBehaviour {
  public static SkySailWorld Instance {get;private set;}
  public Transform scenery,cabin,leftDoor,rightDoor;
  public GameObject[] proxies,stations;public Material sky;public Light sun;
  public SkySailJourney Journey {get;private set;}
  public bool Travelling=>Journey.phase!=SkySailPhase.Idle;
  public Vector3 Origin {get;private set;}
  public float Progress {get;private set;}
  public string Message {get;private set;}="";
  public bool reviewCamera;
  AppRoot app;SkySailStreaming streaming;SkySailJourney offlineJourney;uint seenSequence;SkySailPhase seenPhase;
  float[] lengths;bool released,approach,prepared,onlineJourney;uint acknowledged;
  Athlete[] passengers=Array.Empty<Athlete>();Vector3 doorLeft,doorRight;
  Text heading,caption,wayfinding;Button previous,next;Image progressFill;Canvas ui;float uiTimer;
  AudioSource motor;
  bool Authority=>!app.rooms.Connected||app.rooms.Host;
  double Clock=>app.rooms.Connected?NetworkManager.Singleton.ServerTime.Time:Time.timeAsDouble;
  void Awake(){Instance=this;doorLeft=leftDoor.localPosition;doorRight=rightDoor.localPosition;foreach(var lod in cabin.GetComponentsInChildren<LODGroup>())lod.ForceLOD(0);}
  IEnumerator Start(){while(!AppRoot.Instance||!AppRoot.Instance.environments.View)yield return null;app=AppRoot.Instance;streaming=app.environments.GetComponent<SkySailStreaming>();SetIsland(app.SelectedSport);BuildUI();BuildAudio();if(Environment.GetCommandLineArgs().Contains("-skyStation")&&!app.rooms.Connected){app.EnterOffline();app.LocalAthlete.capsule.enabled=false;app.LocalAthlete.transform.position=SkySailMap.Exit(app.SelectedSport,0);app.LocalAthlete.capsule.enabled=true;app.view.yaw=SkySailMap.Facing(app.SelectedSport).eulerAngles.y;}}
  void OnDestroy(){if(Instance==this)Instance=null;}
  public void SetIsland(SportId id){
   Origin=SkySailMap.Center(id);scenery.localPosition=-Origin;
   for(int i=0;i<4;i++){
    bool detail=app&&app.environments.roots[i]&&app.environments.roots[i].activeSelf;
    proxies[i].SetActive(!detail||i!=(int)id);
    foreach(var c in stations[i].GetComponentsInChildren<Collider>(true))c.enabled=i==(int)id;
   }
   if(!Travelling){cabin.localPosition=SkySailMap.Berth(id);cabin.localRotation=SkySailMap.Facing(id);}
  }
  public bool CanTravel(SportId target,out string reason){
   reason="";if(!app||!app.Exploring){reason="Enter an island first.";return false;}
   if(!Authority){reason="Your host chooses the destination.";return false;}
   if(Travelling||streaming.Busy){reason="Preparing the journey…";return false;}
   if(!SkySailMap.Adjacent(app.SelectedSport,target)){reason="Choose a neighboring station.";return false;}
   var group=Group();if(group.Length>app.environments.Definition(target).maxPlayers){reason=target+" supports "+app.environments.Definition(target).maxPlayers+" players. Your group stays together.";return false;}
   var port=SkySailMap.Port(app.SelectedSport);if(group.Any(p=>Vector3.Distance(p.transform.position,port)>18)){reason="Bring everyone to the Sky-Sail station.";return false;}
   return true;
  }
  public void RequestTravel(SportId target){
   if(!CanTravel(target,out var why)){Message=why;return;}
   Message="";
   var s=new SkySailJourney{sequence=Journey.sequence+1,from=app.SelectedSport,to=target,phase=SkySailPhase.Preparing,started=Clock,duration=44};SetState(s);
  }
  void SetState(SkySailJourney s){if(app.rooms.Connected){if(app.rooms.Host&&NetworkAthlete.HostPlayer)NetworkAthlete.HostPlayer.WorldTravel.Value=s;}else offlineJourney=s;}
  Athlete[] Group()=>app.rooms.Connected?FindObjectsByType<NetworkAthlete>(FindObjectsSortMode.None).OrderBy(a=>a.OwnerClientId).Select(a=>a.GetComponent<Athlete>()).ToArray():new[]{app.LocalAthlete};
  void Update(){
   if(!app)return;
   Shader.SetGlobalVector("_SkySailOrigin",Origin);
   Journey=app.rooms.Connected&&NetworkAthlete.HostPlayer?NetworkAthlete.HostPlayer.WorldTravel.Value:offlineJourney;
   if(onlineJourney&&!app.rooms.Connected){AbortLocal();return;}
   if(Travelling&&!app.Exploring){if(Authority){var cancel=Journey;cancel.phase=SkySailPhase.Idle;SetState(cancel);}AbortLocal();return;}
   if(Travelling&&(seenSequence!=Journey.sequence||seenPhase==SkySailPhase.Idle)){
    seenSequence=Journey.sequence;onlineJourney=app.rooms.Connected;released=approach=prepared=false;acknowledged=0;lengths=SkySailMap.Distances(Journey.from,Journey.to);
    passengers=Group();foreach(var a in passengers){a.ResetLocomotion();a.inTransit=true;a.capsule.enabled=false;var nt=a.GetComponent<NetworkTransform>();if(nt)nt.enabled=false;}
    app.view.yaw=SkySailMap.Facing(Journey.from).eulerAngles.y;app.view.pitch=8;app.view.mode=0;StartCoroutine(Prepare(Journey.to,Journey.sequence));
   }
   if(Journey.phase!=seenPhase){
    if(Journey.phase==SkySailPhase.Docking)Arrive();
    if(Journey.phase==SkySailPhase.Idle&&seenPhase!=SkySailPhase.Idle)Disembark();
    seenPhase=Journey.phase;
   }
   if(Travelling){
    double elapsed=Clock-Journey.started;
    Progress=Journey.phase==SkySailPhase.Riding?Mathf.Clamp01((float)(elapsed/Journey.duration)):Journey.phase==SkySailPhase.Docking?1:0;
    float t=SkySailMap.Parameter(lengths,Progress);cabin.localPosition=SkySailMap.Path(Journey.from,Journey.to,t);
    var tangent=SkySailMap.Tangent(Journey.from,Journey.to,t);tangent.y=0;
    var rotation=Quaternion.LookRotation(tangent.sqrMagnitude>.01f?tangent:Vector3.forward);
    if(Progress<.025f)rotation=Quaternion.Slerp(SkySailMap.Facing(Journey.from),rotation,Progress/.025f);
    if(Progress>.975f)rotation=Quaternion.Slerp(rotation,SkySailMap.Facing(Journey.to),Mathf.InverseLerp(.975f,1,Progress));
    cabin.localRotation=rotation*Quaternion.Euler(0,0,Journey.phase==SkySailPhase.Riding?Mathf.Sin((float)Clock*1.5f)*.35f:0);
    if(Journey.phase==SkySailPhase.Riding&&Progress>.27f&&!released){released=true;proxies[(int)Journey.from].SetActive(true);if(app.environments.roots[(int)Journey.from])app.environments.roots[(int)Journey.from].SetActive(false);StartCoroutine(streaming.Release(Journey.from));}
    if(Journey.phase==SkySailPhase.Riding&&Progress>.83f&&!approach&&prepared){approach=true;var root=app.environments.roots[(int)Journey.to];root.transform.position=SkySailMap.Center(Journey.to)-Origin;root.SetActive(true);proxies[(int)Journey.to].SetActive(false);}
    PinPassengers();
    if(Authority){
     bool ready=prepared&&(!app.rooms.Connected||FindObjectsByType<NetworkAthlete>(FindObjectsSortMode.None).All(p=>p.TravelReady.Value==Journey.sequence));
     if(Journey.phase==SkySailPhase.Preparing&&ready)Advance(SkySailPhase.Boarding);
     else if(Journey.phase==SkySailPhase.Preparing&&elapsed>60){Message="A player could not prepare the island. Please try again.";var s=Journey;s.phase=SkySailPhase.Idle;SetState(s);}
     else if(Journey.phase==SkySailPhase.Boarding&&elapsed>3.5)Advance(SkySailPhase.Riding);
     else if(Journey.phase==SkySailPhase.Riding&&elapsed>=Journey.duration)Advance(SkySailPhase.Docking);
     else if(Journey.phase==SkySailPhase.Docking&&elapsed>3.5)Advance(SkySailPhase.Idle);
    }
   }else if(app.Exploring){cabin.localPosition=SkySailMap.Berth(app.SelectedSport);cabin.localRotation=SkySailMap.Facing(app.SelectedSport);if(Input.GetKeyDown(KeyCode.F))RequestTravel(SkySailMap.Neighbor(app.SelectedSport,1));}
   float open=Journey.phase==SkySailPhase.Idle||Journey.phase==SkySailPhase.Preparing?1:Journey.phase==SkySailPhase.Docking?Mathf.Clamp01((float)(Clock-Journey.started)/2):Journey.phase==SkySailPhase.Boarding?1-Mathf.Clamp01((float)(Clock-Journey.started)/2):0;
   leftDoor.localPosition=doorLeft+Vector3.left*(open*1.34f);rightDoor.localPosition=doorRight+Vector3.right*(open*1.34f);
   if(motor){motor.volume=Mathf.Lerp(motor.volume,Journey.phase==SkySailPhase.Riding?.10f:.016f,Time.deltaTime*2);motor.pitch=Mathf.Lerp(.55f,1,Mathf.Sin(Progress*Mathf.PI));}
   if((uiTimer-=Time.deltaTime)<0){uiTimer=.1f;UpdateUI();}
  }
  IEnumerator Prepare(SportId id,uint sequence){
   yield return streaming.Prepare(id);if(!Travelling||sequence!=seenSequence)yield break;
   if(!string.IsNullOrEmpty(streaming.Error)){Message=streaming.Error;yield break;}prepared=true;
   if(app.rooms.Connected){var local=FindObjectsByType<NetworkAthlete>(FindObjectsSortMode.None).FirstOrDefault(p=>p.IsOwner);if(local&&acknowledged!=sequence){acknowledged=sequence;local.TravelReadyRpc(sequence);}}
  }
  void Advance(SkySailPhase phase){var s=Journey;s.phase=phase;s.started=Clock;SetState(s);}
  void PinPassengers(){for(int i=0;i<passengers.Length;i++){var a=passengers[i];if(!a)continue;int row=i/5;var offset=new Vector3(row==0?-1.45f:1.45f,.06f,(i%5-2)*1.08f);a.transform.SetPositionAndRotation(cabin.TransformPoint(offset),cabin.rotation*Quaternion.Euler(0,row==0?90:-90,0));}}
  void Arrive(){
   float yaw=app.view.yaw,pitch=app.view.pitch;int mode=app.view.mode;
   app.SelectSport(Journey.to,true);SetIsland(Journey.to);app.view.yaw=yaw;app.view.pitch=pitch;app.view.mode=mode;
   if(app.rooms.Host&&NetworkAthlete.HostPlayer){NetworkAthlete.HostPlayer.WorldSport.Value=Journey.to;NetworkAthlete.HostPlayer.WorldAppearance.Value=JsonUtility.ToJson(LocalProfile.ForSport(Journey.to));app.rooms.SetTravelSport(Journey.to);}
   if(app.LocalAthlete)app.LocalAthlete.capsule.enabled=false;
  }
  void Disembark(){
   for(int i=0;i<passengers.Length;i++){var a=passengers[i];if(!a)continue;a.inTransit=false;var p=SkySailMap.Exit(app.SelectedSport,i);var q=SkySailMap.Facing(app.SelectedSport)*Quaternion.Euler(0,180,0);a.transform.SetPositionAndRotation(p,q);var nt=a.GetComponent<NetworkTransform>();if(nt){nt.enabled=true;if(Authority)nt.Teleport(p,q,Vector3.one);}a.capsule.enabled=Authority;a.ResetLocomotion();a.HideHead(false);}
   passengers=Array.Empty<Athlete>();onlineJourney=false;app.Show(app.Exploring?"stadium":"sport");streaming.ReleaseOtherIslands(app.SelectedSport);SetIsland(app.SelectedSport);
  }
  void AbortLocal(){
   onlineJourney=false;offlineJourney=default;Journey=default;seenPhase=SkySailPhase.Idle;
   foreach(var a in passengers)if(a){a.inTransit=false;var nt=a.GetComponent<NetworkTransform>();if(nt)nt.enabled=true;a.capsule.enabled=!app.rooms.Connected||app.rooms.Host;}
   passengers=Array.Empty<Athlete>();if(!streaming.Busy){app.SelectSport(app.SelectedSport,true);SetIsland(app.SelectedSport);}
  }
  void LateUpdate(){
   if(!app)return;
   // One sky and sun throughout every ride and every island; no per-scene flashes.
   RenderSettings.skybox=sky;RenderSettings.fog=true;RenderSettings.fogMode=FogMode.Linear;RenderSettings.fogStartDistance=1800;RenderSettings.fogEndDistance=4200;RenderSettings.fogColor=new Color(.60f,.78f,.86f);
   RenderSettings.ambientMode=AmbientMode.Flat;RenderSettings.ambientLight=new Color(.50f,.58f,.65f);
   Shader.SetGlobalVector("_SkySailOrigin",Origin);
   sun.transform.rotation=Quaternion.Euler(48,-35,0);sun.intensity=1.15f;sun.color=new Color(1,.95f,.86f);sun.shadowBias=.65f;sun.shadowNormalBias=.45f;
   if(Camera.main){Camera.main.clearFlags=CameraClearFlags.Skybox;Camera.main.farClipPlane=5000;}
  }
  public bool RideCamera(Camera camera,float yaw,float pitch,int mode){
   if(!Travelling||reviewCamera)return false;
   Quaternion look=Quaternion.Euler(pitch,yaw,0);Vector3 p;
   if(mode==0)p=cabin.TransformPoint(new Vector3(0,1.72f,-1.6f));
   else if(mode==1)p=cabin.position+Vector3.up*2.0f-look*Vector3.forward*12;
   else p=cabin.position+Vector3.up*10-look*Vector3.forward*22;
   camera.transform.SetPositionAndRotation(p,mode==2?Quaternion.LookRotation(cabin.position+Vector3.up*1.5f-p):look);camera.nearClipPlane=.06f;
   foreach(var a in passengers)if(a)a.HideHead(mode==0&&a==app.LocalAthlete);return true;
  }
  void BuildAudio(){
   var clip=AudioClip.Create("SkySail quiet electric drive",24000,1,24000,false);var samples=new float[24000];for(int i=0;i<samples.Length;i++){float t=i/24000f;samples[i]=.10f*Mathf.Sin(2*Mathf.PI*80*t)+.035f*Mathf.Sin(2*Mathf.PI*160*t)+.015f*Mathf.Sin(2*Mathf.PI*241*t);}clip.SetData(samples,0);motor=gameObject.AddComponent<AudioSource>();motor.clip=clip;motor.loop=true;motor.spatialBlend=0;motor.volume=0;motor.Play();
  }
  Text Text(Transform parent,string value,int size,Vector2 position,Vector2 dimensions){var o=new GameObject(value,typeof(RectTransform),typeof(Text));o.transform.SetParent(parent,false);var t=o.GetComponent<Text>();t.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");t.fontSize=size;t.alignment=TextAnchor.MiddleCenter;t.color=new Color(.96f,.94f,.86f);t.raycastTarget=false;t.text=value;var r=t.rectTransform;r.anchorMin=r.anchorMax=new Vector2(.5f,.5f);r.anchoredPosition=position;r.sizeDelta=dimensions;return t;}
  void BuildUI(){
   ui=new GameObject("Sky-Sail journey UI",typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster)).GetComponent<Canvas>();ui.renderMode=RenderMode.ScreenSpaceOverlay;ui.sortingOrder=20;
   var scaler=ui.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1600,900);scaler.screenMatchMode=CanvasScaler.ScreenMatchMode.Expand;
   var safe=new GameObject("Safe",typeof(RectTransform));safe.transform.SetParent(ui.transform,false);var sr=safe.GetComponent<RectTransform>();var sa=Screen.safeArea;sr.anchorMin=new Vector2(sa.x/Screen.width,sa.y/Screen.height);sr.anchorMax=new Vector2(sa.xMax/Screen.width,sa.yMax/Screen.height);sr.offsetMin=sr.offsetMax=Vector2.zero;
   var panel=new GameObject("Journey card",typeof(RectTransform),typeof(Image));panel.transform.SetParent(safe.transform,false);var r=panel.GetComponent<RectTransform>();r.anchorMin=r.anchorMax=new Vector2(.5f,0);r.anchoredPosition=new Vector2(0,175);r.sizeDelta=new Vector2(680,160);panel.GetComponent<Image>().color=new Color(.035f,.16f,.18f,.93f);
   heading=Text(panel.transform,"SKY-SAIL CIRCUIT",22,new Vector2(0,52),new Vector2(650,32));caption=Text(panel.transform,"",18,new Vector2(0,20),new Vector2(650,35));
   previous=TravelButton(panel.transform,-165,-37,-1);next=TravelButton(panel.transform,165,-37,1);
   var bar=new GameObject("Journey progress",typeof(RectTransform),typeof(Image));bar.transform.SetParent(panel.transform,false);progressFill=bar.GetComponent<Image>();progressFill.color=new Color(.89f,.68f,.35f);var br=bar.GetComponent<RectTransform>();br.anchorMin=new Vector2(0,0);br.anchorMax=new Vector2(1,0);br.pivot=Vector2.zero;br.anchoredPosition=Vector2.zero;br.sizeDelta=new Vector2(0,4);
   wayfinding=Text(safe.transform,"",20,new Vector2(0,350),new Vector2(650,35));
  }
  Button TravelButton(Transform parent,float x,float y,int direction){var g=new GameObject("Travel",typeof(RectTransform),typeof(Image),typeof(Button));g.transform.SetParent(parent,false);var r=g.GetComponent<RectTransform>();r.anchoredPosition=new Vector2(x,y);r.sizeDelta=new Vector2(302,54);g.GetComponent<Image>().color=new Color(.12f,.40f,.42f,1);var b=g.GetComponent<Button>();b.onClick.AddListener(()=>RequestTravel(SkySailMap.Neighbor(app.SelectedSport,direction)));Text(g.transform,"",21,Vector2.zero,new Vector2(290,50));return b;}
  void UpdateUI(){
   if(!ui)return;ui.enabled=app.Exploring;if(!ui.enabled)return;
   float distance=Vector3.Distance(app.LocalAthlete.transform.position,SkySailMap.Port(app.SelectedSport));heading.transform.parent.gameObject.SetActive(Travelling||distance<28);
   wayfinding.text=Travelling?"Look around • Camera: cabin / chase / panorama":"SKY-SAIL STATION  ·  "+Mathf.RoundToInt(distance)+" m";
   previous.gameObject.SetActive(!Travelling);next.gameObject.SetActive(!Travelling);
   if(Travelling){heading.text=Journey.from.ToString().ToUpperInvariant()+"  →  "+Journey.to.ToString().ToUpperInvariant();caption.text=Journey.phase switch{SkySailPhase.Preparing=>"Preparing your island • Everyone travels together",SkySailPhase.Boarding=>"Doors closing • Enjoy the journey",SkySailPhase.Docking=>"Welcome to "+Journey.to+" • Doors opening",_=>"Over the sea • "+Mathf.CeilToInt((1-Progress)*Journey.duration)+" seconds to arrival"};}
   else{heading.text="SKY-SAIL CIRCUIT";caption.text=Message.Length>0?Message:Authority?"Choose your next island • Everyone boards together":"Your host chooses the next island";var a=SkySailMap.Neighbor(app.SelectedSport,-1);var b=SkySailMap.Neighbor(app.SelectedSport,1);previous.GetComponentInChildren<Text>().text="← "+a;next.GetComponentInChildren<Text>().text=b+(Application.isMobilePlatform?" →":" → [F]");previous.interactable=Authority&&!streaming.Busy;next.interactable=Authority&&!streaming.Busy;}
   progressFill.rectTransform.localScale=new Vector3(Travelling?Mathf.Max(.015f,Progress):1,1,1);
  }
 }
}
