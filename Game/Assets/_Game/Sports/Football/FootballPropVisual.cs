using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace WhatTheFish {
 public sealed class FootballPropVisual:MonoBehaviour {
  const float SpawnDisplayScale=4;
  FootballBall ball;Transform root;GameObject[] models=new GameObject[3],walls=new GameObject[2];GameObject ghost;
  Mesh quad;Mesh[] digits;MeshRenderer pad,hologram;readonly MeshRenderer[] portals=new MeshRenderer[2];Material effect,trailMaterial;MaterialPropertyBlock block;
  Transform[] ballParts;Vector3[] ballScales;int countdown;readonly Dictionary<Athlete,TrailRenderer> trails=new();readonly List<Athlete> retired=new();
  public Transform PickupModel {get;private set;}
  public int CountdownNumber=>countdown;
  public MeshRenderer Hologram=>hologram;
  void Start(){
   ball=GetComponent<FootballBall>();if(!ball.Pitch){enabled=false;return;}
   var pieces=GetComponentsInChildren<MeshRenderer>();ballParts=new Transform[pieces.Length];ballScales=new Vector3[pieces.Length];for(int i=0;i<pieces.Length;i++){ballParts[i]=pieces[i].transform;ballScales[i]=pieces[i].transform.localScale;}
   root=new GameObject("Football ability presentation").transform;root.SetParent(ball.Pitch,true);block=new MaterialPropertyBlock();effect=Resources.Load<Material>("BasketballProps/PropEffect");trailMaterial=Resources.Load<Material>("BasketballProps/PropTrail");
   quad=new Mesh{name="Football ability effect quad"};quad.vertices=new[]{new Vector3(-.5f,-.5f,0),new Vector3(.5f,-.5f,0),new Vector3(.5f,.5f,0),new Vector3(-.5f,.5f,0)};quad.uv=new[]{Vector2.zero,Vector2.right,Vector2.one,Vector2.up};quad.triangles=new[]{0,2,1,0,3,2};quad.RecalculateNormals();quad.RecalculateBounds();
   string[] names={"Turbo","Portal","Wall"};for(int i=0;i<3;i++){var prefab=Resources.Load<GameObject>("FootballProps/"+names[i]);if(!prefab){Debug.LogError("Missing football prop "+names[i]);continue;}models[i]=Instantiate(prefab,root);models[i].SetActive(false);}
   var wallPrefab=Resources.Load<GameObject>("FootballProps/DeployedWall");if(wallPrefab){for(int i=0;i<2;i++){walls[i]=Instantiate(wallPrefab,root);walls[i].SetActive(false);}ghost=Instantiate(wallPrefab,root);var r=ghost.GetComponent<MeshRenderer>();var materials=r.sharedMaterials;for(int i=0;i<materials.Length;i++)materials[i]=effect;r.sharedMaterials=materials;ghost.SetActive(false);}
   pad=Plane("Football pickup projection");hologram=Plane("Football spawn hologram");hologram.sharedMaterial=Resources.Load<Material>("FootballProps/Countdown");digits=new Mesh[5];for(int i=0;i<5;i++)digits[i]=BasketballPropVisual.Digit(i+1);for(int i=0;i<2;i++)portals[i]=Plane("Football hex portal "+i);
  }
  MeshRenderer Plane(string name){var go=new GameObject(name);go.transform.SetParent(root,false);go.AddComponent<MeshFilter>().sharedMesh=quad;var r=go.AddComponent<MeshRenderer>();r.sharedMaterial=effect;r.shadowCastingMode=ShadowCastingMode.Off;r.receiveShadows=false;r.enabled=false;return r;}
  void Style(Renderer r,Color color,int mode){block.Clear();block.SetColor("_Tint",color);block.SetFloat("_Mode",mode);r.SetPropertyBlock(block);}
  static Color ColorOf(SportsProp k)=>k==SportsProp.Turbo?new Color(.1f,.9f,.65f):k==SportsProp.Wall?new Color(1,.52f,.15f):new Color(.12f,.65f,1);
  void LateUpdate(){
   if(!root||!ball)return;bool live=ball.PropsVisible;var state=ball.Props;root.gameObject.SetActive(live);PickupModel=null;
   if(!live){ScaleBall(1);ClearTrails();return;}
   var spawn=state.pickup;bool warning=spawn.countingDown&&spawn.kind==SportsProp.None;var anchor=ball.PropAnchor(spawn.anchor);
   for(int i=0;i<models.Length;i++)if(models[i]){bool visible=(int)spawn.kind==i+2;models[i].SetActive(visible);if(visible){PickupModel=models[i].transform;PickupModel.SetPositionAndRotation(anchor+Vector3.up*((.95f+Mathf.Sin(Time.time*2)*.075f)*SpawnDisplayScale),Quaternion.Euler(0,Time.time*30,0));PickupModel.localScale=Vector3.one*(1.12f*SpawnDisplayScale);}}
   pad.enabled=warning||spawn.kind!=SportsProp.None;pad.transform.SetPositionAndRotation(anchor+Vector3.up*.014f,Quaternion.Euler(90,0,0));pad.transform.localScale=Vector3.one*(1.55f*SpawnDisplayScale);Style(pad,warning?new Color(.025f,.12f,.72f,1):ColorOf(spawn.kind),warning?5:2);
   countdown=warning?spawn.CountdownNumber(FootballBall.PropClock):0;hologram.enabled=warning;
   if(warning){hologram.GetComponent<MeshFilter>().sharedMesh=digits[countdown-1];hologram.transform.position=anchor+Vector3.up*(1.2f*SpawnDisplayScale);var forward=Camera.main?Vector3.ProjectOnPlane(hologram.transform.position-Camera.main.transform.position,Vector3.up):Vector3.forward;if(forward.sqrMagnitude>.001f)hologram.transform.rotation=Quaternion.LookRotation(forward);hologram.transform.localScale=Vector3.one*(1.15f*SpawnDisplayScale);}
   for(int i=0;i<2;i++){var wall=i==0?state.wallA:state.wallB;if(!walls[i])continue;walls[i].SetActive(ball.WallActive(wall));walls[i].transform.SetPositionAndRotation(wall.position,wall.Rotation);}
   if(ghost){var hud=FootballPropHUD.Instance;bool preview=hud&&hud.Placing;ghost.SetActive(preview);if(preview){ghost.transform.SetPositionAndRotation(hud.Preview.position,hud.Preview.Rotation);Style(ghost.GetComponent<MeshRenderer>(),hud.PlacementValid?new Color(.13f,.67f,1,.45f):new Color(1,.29f,.22f,.45f),4);}}
   float phase=(float)(FootballBall.PropClock-state.portalAt)/SportsAbilityRules.PortalSeconds;bool show=state.portalOutcome!=2&&state.portalSequence>0&&phase>=0&&phase<1.26f&&(state.transferring||state.portalOutcome==1);
   float size=Mathf.SmoothStep(0,1,phase/.15f)*(1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(1,1.26f,phase)));
   for(int i=0;i<2;i++){var r=portals[i];r.enabled=show;r.transform.position=i==0?state.source:state.destination;var view=Camera.main?r.transform.position-Camera.main.transform.position:Vector3.forward;if(view.sqrMagnitude>.001f)r.transform.rotation=Quaternion.LookRotation(view);r.transform.localScale=Vector3.one*(.95f*Mathf.Max(.001f,size));Style(r,new Color(.07f,.61f,1),7);}
   float scale=state.transferring?(phase<.5f?1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(.14f,.46f,phase)):Mathf.SmoothStep(0,1,Mathf.InverseLerp(.54f,.9f,phase))):1;ScaleBall(Mathf.Max(.001f,scale));
   foreach(var a in Athlete.Active){bool active=ball.TurboRemaining(a)>0;if(active&&!trails.ContainsKey(a)){var go=new GameObject("Football team Turbo trail");go.transform.SetParent(a.transform,false);go.transform.localPosition=new Vector3(0,.1f,-.15f);var t=go.AddComponent<TrailRenderer>();t.sharedMaterial=trailMaterial;t.time=.3f;t.startWidth=.2f;t.endWidth=0;t.startColor=new Color(.15f,1,.76f,.8f);t.endColor=new Color(.15f,1,.76f,0);t.shadowCastingMode=ShadowCastingMode.Off;trails[a]=t;}if(trails.TryGetValue(a,out var trail))trail.emitting=active&&a.speed>.5f;}
   retired.Clear();foreach(var pair in trails)if(!pair.Key||!Athlete.Active.Contains(pair.Key))retired.Add(pair.Key);foreach(var a in retired){if(trails[a])Destroy(trails[a].gameObject);trails.Remove(a);}
  }
  void ScaleBall(float scale){if(ballParts==null)return;for(int i=0;i<ballParts.Length;i++)if(ballParts[i]&&ballParts[i]!=transform)ballParts[i].localScale=ballScales[i]*scale;}
  void ClearTrails(){foreach(var t in trails.Values)if(t){t.emitting=false;t.Clear();}}
  void OnDisable(){if(root)root.gameObject.SetActive(false);ScaleBall(1);ClearTrails();}
  void OnDestroy(){if(root)Destroy(root.gameObject);if(quad)Destroy(quad);if(digits!=null)foreach(var d in digits)Destroy(d);foreach(var t in trails.Values)if(t)Destroy(t.gameObject);}
 }
}
