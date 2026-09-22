using System;
using System.IO;
using System.Linq;
using SportsPrototype;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using Unity.Netcode;
using Unity.Netcode.Components;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public static class ProjectBuilder {
 const string Root="Assets/_Game/";
 [MenuItem("Sports/Rebuild prototype scene")]
 public static void Setup(){
  foreach(string dir in new[]{"Resources","Materials","Scenes","Settings","Sports/Football","Sports/Basketball","Sports/Golf","Shared/Platform"})Directory.CreateDirectory(Root+dir);
  AssetDatabase.Refresh();
  foreach(string name in new[]{"Stadium","Athlete"}){
   var importer=AssetImporter.GetAtPath(Root+"Art/"+name+".fbx") as ModelImporter;if(!importer)throw new Exception("Run the Blender asset generator first.");
   importer.globalScale=1;importer.useFileScale=true;importer.importCameras=false;importer.importLights=false;importer.materialImportMode=ModelImporterMaterialImportMode.ImportStandard;
   importer.animationType=name=="Athlete"?ModelImporterAnimationType.Generic:ModelImporterAnimationType.None;importer.importAnimation=name=="Athlete";importer.isReadable=false;
   if(name=="Athlete"){var clips=importer.defaultClipAnimations;foreach(var c in clips)c.loopTime=true;importer.clipAnimations=clips;}
   importer.SaveAndReimport();
  }
  var renderer=AssetDatabase.LoadAssetAtPath<UniversalRendererData>(Root+"Settings/MobileRenderer.asset");if(!renderer){renderer=ScriptableObject.CreateInstance<UniversalRendererData>();AssetDatabase.CreateAsset(renderer,Root+"Settings/MobileRenderer.asset");}
  var pipeline=AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(Root+"Settings/MobileURP.asset");if(!pipeline){pipeline=UniversalRenderPipelineAsset.Create(renderer);AssetDatabase.CreateAsset(pipeline,Root+"Settings/MobileURP.asset");}
  pipeline.renderScale=.9f;pipeline.msaaSampleCount=2;pipeline.shadowDistance=65;pipeline.mainLightShadowmapResolution=2048;pipeline.supportsHDR=false;pipeline.supportsCameraDepthTexture=false;pipeline.supportsCameraOpaqueTexture=false;
  GraphicsSettings.defaultRenderPipeline=pipeline;QualitySettings.renderPipeline=pipeline;QualitySettings.vSyncCount=0;QualitySettings.antiAliasing=0;QualitySettings.shadows=UnityEngine.ShadowQuality.All;QualitySettings.shadowResolution=UnityEngine.ShadowResolution.Medium;
  PlayerSettings.colorSpace=ColorSpace.Linear;PlayerSettings.companyName="UMPSA Student Studio";PlayerSettings.productName="SportsPrototype";PlayerSettings.bundleVersion="0.1.0";PlayerSettings.SetApplicationIdentifier(UnityEditor.Build.NamedBuildTarget.Android,"com.umpsa.sportsprototype");
  PlayerSettings.defaultInterfaceOrientation=UIOrientation.LandscapeLeft;PlayerSettings.allowedAutorotateToPortrait=false;PlayerSettings.allowedAutorotateToPortraitUpsideDown=false;PlayerSettings.allowedAutorotateToLandscapeLeft=true;PlayerSettings.allowedAutorotateToLandscapeRight=true;
  PlayerSettings.Android.minSdkVersion=AndroidSdkVersions.AndroidApiLevel26;PlayerSettings.Android.targetSdkVersion=AndroidSdkVersions.AndroidApiLevelAuto;PlayerSettings.Android.targetArchitectures=AndroidArchitecture.ARM64;PlayerSettings.SetScriptingBackend(UnityEditor.Build.NamedBuildTarget.Android,ScriptingImplementation.IL2CPP);
  PlayerSettings.SetManagedStrippingLevel(UnityEditor.Build.NamedBuildTarget.Android,ManagedStrippingLevel.Low);PlayerSettings.Android.useCustomKeystore=false;PlayerSettings.runInBackground=true;PlayerSettings.defaultScreenWidth=1280;PlayerSettings.defaultScreenHeight=720;PlayerSettings.fullScreenMode=FullScreenMode.Windowed;
  EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
  var stadium=PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Root+"Art/Stadium.fbx")) as GameObject;stadium.name="Football Stadium • Blender source";Remap(stadium);
  foreach(var mf in stadium.GetComponentsInChildren<MeshFilter>()){
   mf.gameObject.isStatic=true;mf.gameObject.layer=8;
   if(mf.name.StartsWith("Ground")||mf.name.StartsWith("Concrete")||mf.name.StartsWith("Grass")){var col=mf.gameObject.AddComponent<MeshCollider>();col.sharedMesh=mf.sharedMesh;}
  }
  var sv=stadium.AddComponent<StadiumView>();
  // Text and graphic content is interactive UI, mounted over Blender-authored signs.
  var screen=WorldCanvas("Stadium screen",stadium.transform,new Vector3(0,11,-71.1f),new Vector2(17,7),Quaternion.Euler(0,180,0));
  sv.title=WorldText(screen,"SUNNY PARK",new Vector2(0,70),new Vector2(1600,180),116,Color.white);
  sv.subtitle=WorldText(screen,"MAKE YOURSELF AT HOME",new Vector2(0,-100),new Vector2(1600,110),44,LocalProfile.Teams[0]);
  for(int i=0;i<7;i++){var sign=WorldCanvas("Sports board",stadium.transform,new Vector3(-33+i*11,.72f,58.60f),new Vector2(10.2f,.8f),Quaternion.identity);var label=WorldText(sign,"PLAY TOGETHER",Vector2.zero,new Vector2(1600,105),66,Color.white);var component=sign.gameObject.AddComponent<StadiumSign>();component.label=label;}
  var entry=WorldCanvas("Tunnel welcome",stadium.transform,new Vector3(0,3,57.4f),new Vector2(4.2f,.5f),Quaternion.identity);var titleSign=entry.gameObject.AddComponent<StadiumSign>();titleSign.nameSign=true;titleSign.label=WorldText(entry,"SUNNY PARK",Vector2.zero,new Vector2(1600,160),120,Color.white);
  var athlete= new GameObject("Athlete");var model=PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Root+"Art/Athlete.fbx")) as GameObject;model.transform.SetParent(athlete.transform,false);Remap(model);
  var capsule=athlete.AddComponent<CharacterController>();capsule.height=1.85f;capsule.radius=.30f;capsule.center=new Vector3(0,.94f,0);capsule.stepOffset=.3f;capsule.slopeLimit=48;
  var motor=athlete.AddComponent<Athlete>();motor.capsule=capsule;motor.visual=model.transform;
  var animator=model.GetComponent<Animator>();if(!animator)animator=model.AddComponent<Animator>();animator.applyRootMotion=false;
  string controllerPath=Root+"Settings/Athlete.controller";if(File.Exists(controllerPath))AssetDatabase.DeleteAsset(controllerPath);
  var controller=AnimatorController.CreateAnimatorControllerAtPath(controllerPath);controller.AddParameter("Speed",AnimatorControllerParameterType.Float);
  var allClips=AssetDatabase.LoadAllAssetsAtPath(Root+"Art/Athlete.fbx").OfType<AnimationClip>().Where(c=>!c.name.StartsWith("__preview__")).ToArray();
  var states=controller.layers[0].stateMachine;
  var idle=states.AddState("Idle");idle.motion=allClips.FirstOrDefault(c=>c.name.Contains("Idle"));var walk=states.AddState("Walk");walk.motion=allClips.FirstOrDefault(c=>c.name.Contains("Walk"));var run=states.AddState("Run");run.motion=allClips.FirstOrDefault(c=>c.name.Contains("Run"));states.defaultState=idle;
  Transition(idle,walk,AnimatorConditionMode.Greater,.2f);Transition(walk,idle,AnimatorConditionMode.Less,.2f);Transition(walk,run,AnimatorConditionMode.Greater,5);Transition(run,walk,AnimatorConditionMode.Less,5);animator.runtimeAnimatorController=controller;
  var offlinePrefab=PrefabUtility.SaveAsPrefabAsset(athlete,Root+"Resources/OfflineAthlete.prefab");
  athlete.AddComponent<NetworkObject>();var nt=athlete.AddComponent<NetworkTransform>();nt.Interpolate=true;nt.SyncScaleX=nt.SyncScaleY=nt.SyncScaleZ=false;athlete.AddComponent<NetworkAthlete>();
  var playerPrefab=PrefabUtility.SaveAsPrefabAsset(athlete,Root+"Resources/NetworkAthlete.prefab");UnityEngine.Object.DestroyImmediate(athlete);
  var networkGO=new GameObject("Room network");var transport=networkGO.AddComponent<UnityTransport>();var manager=networkGO.AddComponent<NetworkManager>();manager.NetworkConfig=new NetworkConfig{NetworkTransport=transport,PlayerPrefab=playerPrefab,TickRate=30,EnableSceneManagement=false,ConnectionApproval=true};manager.NetworkConfig.Prefabs.Add(new NetworkPrefab{Prefab=playerPrefab});
  var appGO=new GameObject("Sports Application");var rooms=appGO.AddComponent<RoomService>();var app=appGO.AddComponent<AppRoot>();app.rooms=rooms;app.stadium=sv;app.athletePrefab=offlinePrefab;
  var cameraGO=new GameObject("Main Camera");cameraGO.tag="MainCamera";var camera=cameraGO.AddComponent<Camera>();camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=LocalProfile.Hex("BFE2E7");camera.farClipPlane=450;camera.fieldOfView=60;cameraGO.AddComponent<AudioListener>();cameraGO.AddComponent<UniversalAdditionalCameraData>();app.view=cameraGO.AddComponent<PlayerView>();cameraGO.transform.position=new Vector3(87,64,-103);cameraGO.transform.LookAt(Vector3.zero);
  var sunGO=new GameObject("Afternoon sun");var sun=sunGO.AddComponent<Light>();sun.type=LightType.Directional;sun.intensity=1.1f;sun.color=new Color(1,.94f,.85f);sun.shadows=LightShadows.Soft;sun.shadowBias=.08f;sun.shadowNormalBias=.3f;sunGO.transform.rotation=Quaternion.Euler(48,-35,0);RenderSettings.sun=sun;RenderSettings.ambientMode=AmbientMode.Trilight;RenderSettings.ambientSkyColor=new Color(.45f,.55f,.65f);RenderSettings.ambientEquatorColor=new Color(.28f,.35f,.39f);RenderSettings.ambientGroundColor=new Color(.18f,.22f,.18f);RenderSettings.fog=true;RenderSettings.fogColor=camera.backgroundColor;RenderSettings.fogMode=FogMode.Linear;RenderSettings.fogStartDistance=180;RenderSettings.fogEndDistance=430;
  var volumeGO=new GameObject("Gentle colour grading");var volume=volumeGO.AddComponent<Volume>();volume.isGlobal=true;var profile=AssetDatabase.LoadAssetAtPath<VolumeProfile>(Root+"Settings/Colour.asset");if(!profile){profile=ScriptableObject.CreateInstance<VolumeProfile>();AssetDatabase.CreateAsset(profile,Root+"Settings/Colour.asset");var tone=profile.Add<Tonemapping>();tone.mode.Override(TonemappingMode.ACES);AssetDatabase.AddObjectToAsset(tone,profile);}volume.sharedProfile=profile;camera.GetComponent<UniversalAdditionalCameraData>().renderPostProcessing=true;
  foreach(SportId id in Enum.GetValues(typeof(SportId))){string path=Root+"Sports/"+id+"/"+id+".asset";var def=AssetDatabase.LoadAssetAtPath<SportDefinition>(path);if(!def){def=ScriptableObject.CreateInstance<SportDefinition>();AssetDatabase.CreateAsset(def,path);}def.id=id;def.displayName=id.ToString();def.available=id==SportId.Football;EditorUtility.SetDirty(def);}
  sv.Apply(LocalProfile.Stadium);EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(),Root+"Scenes/Bootstrap.unity");EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(Root+"Scenes/Bootstrap.unity",true)};AssetDatabase.SaveAssets();
  Debug.Log("PROTOTYPE_SETUP_COMPLETE clips="+string.Join(",",allClips.Select(c=>c.name)));
 }
 static void Transition(AnimatorState a,AnimatorState b,AnimatorConditionMode mode,float threshold){var t=a.AddTransition(b);t.hasExitTime=false;t.duration=.15f;t.AddCondition(mode,threshold,"Speed");}
 static void Remap(GameObject root){foreach(var renderer in root.GetComponentsInChildren<Renderer>()){
  var mats=renderer.sharedMaterials;for(int i=0;i<mats.Length;i++){if(!mats[i])continue;string name=mats[i].name;string path=Root+"Materials/"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);if(!m){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));m.name=name;m.SetColor("_BaseColor",mats[i].color);m.SetFloat("_Smoothness",.18f);m.enableInstancing=true;AssetDatabase.CreateAsset(m,path);}mats[i]=m;}renderer.sharedMaterials=mats;
 }}
 static RectTransform WorldCanvas(string name,Transform parent,Vector3 position,Vector2 size,Quaternion rotation){var o=new GameObject(name,typeof(RectTransform),typeof(Canvas));o.transform.SetParent(parent,false);o.transform.localPosition=position;o.transform.localRotation=rotation;o.transform.localScale=Vector3.one*(size.x/1700);var r=o.GetComponent<RectTransform>();r.sizeDelta=new Vector2(1700,1700*size.y/size.x);o.GetComponent<Canvas>().renderMode=RenderMode.WorldSpace;return r;}
 static Text WorldText(Transform parent,string value,Vector2 position,Vector2 size,int fontSize,Color color){var o=new GameObject("Graphic",typeof(RectTransform),typeof(Text));o.transform.SetParent(parent,false);var t=o.GetComponent<Text>();t.rectTransform.sizeDelta=size;t.rectTransform.anchoredPosition=position;t.text=value;t.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");t.fontSize=fontSize;t.alignment=TextAnchor.MiddleCenter;t.color=color;t.supportRichText=false;t.raycastTarget=false;return t;}
 [MenuItem("Sports/Build Windows testing player")]
 public static void BuildWindows(){Build(BuildTarget.StandaloneWindows64,"../Builds/WindowsFinal/SportsPrototype.exe");}
 public static void SetupAndBuildWindows(){Setup();BuildWindows();}
 [MenuItem("Sports/Build Android development APK")]
 public static void BuildAndroid(){EditorUserBuildSettings.buildAppBundle=false;Build(BuildTarget.Android,"../Builds/Android/SportsPrototype.apk");}
 static void Build(BuildTarget target,string path){Directory.CreateDirectory(Path.GetDirectoryName(path));var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{Root+"Scenes/Bootstrap.unity"},locationPathName=path,target=target,options=BuildOptions.Development|BuildOptions.CompressWithLz4HC});if(report.summary.result!=BuildResult.Succeeded)throw new Exception("Build failed: "+report.summary.result);Debug.Log("BUILD_OK "+target+" bytes="+report.summary.totalSize);}
}
