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

public static partial class ProjectBuilder {
 const string Root="Assets/_Game/";
 [MenuItem("Sports/Rebuild prototype scene")]
 public static void Setup(){
  foreach(string dir in new[]{"Resources","Materials","Scenes","Settings","Sports/Football","Sports/Basketball","Sports/Golf","Shared/Platform"})Directory.CreateDirectory(Root+dir);
  AssetDatabase.Refresh();
  foreach(string name in new[]{"GolfIsland"}){
   var importer=AssetImporter.GetAtPath(Root+"Art/"+name+".fbx") as ModelImporter;if(!importer)throw new Exception("Run the Blender asset generator first.");
   importer.globalScale=1;importer.useFileScale=true;importer.importCameras=false;importer.importLights=false;importer.materialImportMode=ModelImporterMaterialImportMode.ImportStandard;
   importer.animationType=ModelImporterAnimationType.None;importer.importAnimation=false;importer.isReadable=false;
   importer.SaveAndReimport();
  }
  const string athletePath=Root+"Art/RainbowSprinter.fbx";
  var athleteImporter=AssetImporter.GetAtPath(athletePath) as ModelImporter;
  if(!athleteImporter)throw new Exception("Run the Rainbow Sprinter Blender export first.");
  athleteImporter.globalScale=1;athleteImporter.useFileScale=true;athleteImporter.importCameras=false;athleteImporter.importLights=false;
  athleteImporter.materialImportMode=ModelImporterMaterialImportMode.None;
  athleteImporter.animationType=ModelImporterAnimationType.Generic;athleteImporter.importAnimation=false;athleteImporter.isReadable=false;
  athleteImporter.SaveAndReimport();
  const string runPath=Root+"Art/RainbowSprinterRun.fbx";
  var runImporter=AssetImporter.GetAtPath(runPath) as ModelImporter;
  if(!runImporter)throw new Exception("The Rainbow Sprinter running take is missing.");
  runImporter.globalScale=1;runImporter.useFileScale=true;runImporter.importCameras=false;runImporter.importLights=false;
  runImporter.materialImportMode=ModelImporterMaterialImportMode.None;
  runImporter.animationType=ModelImporterAnimationType.Generic;runImporter.importAnimation=true;runImporter.isReadable=false;
  var runningTakes=runImporter.defaultClipAnimations;
  if(runningTakes.Length!=1)throw new Exception("Rainbow Sprinter must import exactly one running take.");
  runningTakes[0].name="Running";runningTakes[0].loopTime=true;runningTakes[0].loopPose=true;
  runImporter.clipAnimations=runningTakes;runImporter.SaveAndReimport();
  const string idlePath=Root+"Art/RainbowSprinterIdle.fbx";
  var idleImporter=AssetImporter.GetAtPath(idlePath) as ModelImporter;
  if(!idleImporter)throw new Exception("Run animate_rainbow_sprinter.py export before rebuilding the scene.");
  idleImporter.globalScale=1;idleImporter.useFileScale=true;idleImporter.importCameras=false;idleImporter.importLights=false;
  idleImporter.materialImportMode=ModelImporterMaterialImportMode.None;
  idleImporter.animationType=ModelImporterAnimationType.Generic;idleImporter.importAnimation=true;
  // The animation-only FBX has one root. Retain it so curve paths match the
  // mesh FBX's RainbowSprinterRig/... hierarchy instead of starting at Hips.
  idleImporter.preserveHierarchy=true;
  idleImporter.animationCompression=ModelImporterAnimationCompression.Off;
  var idleTakes=idleImporter.defaultClipAnimations;
  if(idleTakes.Length!=1)throw new Exception("Rainbow Sprinter idle must contain exactly one take.");
  idleTakes[0].name="Idle_Playful";idleTakes[0].loopTime=true;idleTakes[0].loopPose=false;
  idleImporter.clipAnimations=idleTakes;idleImporter.SaveAndReimport();
  var turnClips=new System.Collections.Generic.Dictionary<string,AnimationClip>();
  foreach(var turnName in new[]{"Turn_Left_180","Turn_Left_90","Turn_Right_90","Turn_Right_180"}){
   var turnPath=Root+"Art/RainbowSprinter"+turnName+".fbx";
   var importer=AssetImporter.GetAtPath(turnPath) as ModelImporter;
   if(!importer)throw new Exception("Export turn_rainbow_sprinter.py before rebuilding: "+turnName);
   importer.globalScale=1;importer.useFileScale=true;importer.preserveHierarchy=true;importer.animationType=ModelImporterAnimationType.Generic;
   importer.importAnimation=true;importer.importCameras=false;importer.importLights=false;importer.materialImportMode=ModelImporterMaterialImportMode.None;
   importer.animationCompression=ModelImporterAnimationCompression.Off;
   var takes=importer.defaultClipAnimations;if(takes.Length!=1)throw new Exception("Turn must contain one take");
   takes[0].name=turnName;takes[0].loopTime=false;takes[0].loopPose=false;importer.clipAnimations=takes;importer.SaveAndReimport();
   turnClips.Add(turnName,AssetDatabase.LoadAllAssetsAtPath(turnPath).OfType<AnimationClip>().Single(c=>c.name==turnName));
  }
  var footballClips=new System.Collections.Generic.Dictionary<string,AnimationClip>();
  foreach(var name in new[]{"Slide_Tackle","Tackle_Hit"}){
   var path=Root+"Art/RainbowSprinter"+name+".fbx";var importer=AssetImporter.GetAtPath(path) as ModelImporter;
   if(!importer)throw new Exception("Export tackle_rainbow_sprinter.py before rebuilding: "+name);
   importer.globalScale=1;importer.useFileScale=true;importer.preserveHierarchy=true;importer.animationType=ModelImporterAnimationType.Generic;
   importer.importAnimation=true;importer.importCameras=false;importer.importLights=false;importer.materialImportMode=ModelImporterMaterialImportMode.None;importer.animationCompression=ModelImporterAnimationCompression.Off;
   var takes=importer.defaultClipAnimations;if(takes.Length!=1)throw new Exception("Football action must have one take");
   takes[0].name=name;takes[0].loopTime=false;takes[0].loopPose=false;importer.clipAnimations=takes;importer.SaveAndReimport();
   footballClips.Add(name,AssetDatabase.LoadAllAssetsAtPath(path).OfType<AnimationClip>().Single(c=>c.name==name));
  }
  var rainbowMaterial=RainbowMaterial();
  var renderer=AssetDatabase.LoadAssetAtPath<UniversalRendererData>(Root+"Settings/MobileRenderer.asset");if(!renderer){renderer=ScriptableObject.CreateInstance<UniversalRendererData>();AssetDatabase.CreateAsset(renderer,Root+"Settings/MobileRenderer.asset");}
  var pipeline=AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(Root+"Settings/MobileURP.asset");if(!pipeline){pipeline=UniversalRenderPipelineAsset.Create(renderer);AssetDatabase.CreateAsset(pipeline,Root+"Settings/MobileURP.asset");}
  pipeline.renderScale=.9f;pipeline.msaaSampleCount=2;pipeline.shadowDistance=65;pipeline.mainLightShadowmapResolution=2048;pipeline.supportsHDR=false;pipeline.supportsCameraDepthTexture=false;pipeline.supportsCameraOpaqueTexture=false;
  GraphicsSettings.defaultRenderPipeline=pipeline;QualitySettings.renderPipeline=pipeline;QualitySettings.vSyncCount=0;QualitySettings.antiAliasing=0;QualitySettings.shadows=UnityEngine.ShadowQuality.All;QualitySettings.shadowResolution=UnityEngine.ShadowResolution.Medium;
  PlayerSettings.colorSpace=ColorSpace.Linear;PlayerSettings.companyName="UMPSA Student Studio";PlayerSettings.productName="SportsPrototype";PlayerSettings.bundleVersion="0.3.0";PlayerSettings.SetApplicationIdentifier(UnityEditor.Build.NamedBuildTarget.Android,"com.umpsa.sportsprototype");
  PlayerSettings.defaultInterfaceOrientation=UIOrientation.LandscapeLeft;PlayerSettings.allowedAutorotateToPortrait=false;PlayerSettings.allowedAutorotateToPortraitUpsideDown=false;PlayerSettings.allowedAutorotateToLandscapeLeft=true;PlayerSettings.allowedAutorotateToLandscapeRight=true;
  PlayerSettings.Android.minSdkVersion=AndroidSdkVersions.AndroidApiLevel26;PlayerSettings.Android.targetSdkVersion=AndroidSdkVersions.AndroidApiLevelAuto;PlayerSettings.Android.targetArchitectures=AndroidArchitecture.ARM64;PlayerSettings.SetScriptingBackend(UnityEditor.Build.NamedBuildTarget.Android,ScriptingImplementation.IL2CPP);
  PlayerSettings.SetManagedStrippingLevel(UnityEditor.Build.NamedBuildTarget.Android,ManagedStrippingLevel.Low);PlayerSettings.Android.useCustomKeystore=false;PlayerSettings.runInBackground=true;PlayerSettings.defaultScreenWidth=1280;PlayerSettings.defaultScreenHeight=720;PlayerSettings.fullScreenMode=FullScreenMode.Windowed;
  EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
  var stadium=BuildFootball();var sv=stadium.GetComponent<StadiumView>();
  var athlete= new GameObject("Athlete");var model=PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(athletePath)) as GameObject;model.transform.SetParent(athlete.transform,false);
  var skinned=model.GetComponentsInChildren<SkinnedMeshRenderer>(true);var near=skinned.Single(r=>r.name=="RainbowSprinter_LOD0");var far=skinned.Single(r=>r.name=="RainbowSprinter_LOD1");
  near.sharedMaterial=rainbowMaterial;far.sharedMaterial=rainbowMaterial;
  var lodGroup=model.GetComponent<LODGroup>();if(!lodGroup)lodGroup=model.AddComponent<LODGroup>();lodGroup.SetLODs(new[]{new LOD(.24f,new Renderer[]{near}),new LOD(.045f,new Renderer[]{far})});lodGroup.RecalculateBounds();
  var capsule=athlete.AddComponent<CharacterController>();capsule.height=1.75f;capsule.radius=.30f;capsule.center=new Vector3(0,.875f,0);capsule.stepOffset=.3f;capsule.slopeLimit=48;
  var motor=athlete.AddComponent<Athlete>();motor.capsule=capsule;motor.visual=model.transform;athlete.AddComponent<FootballTackle>();
  var animator=model.GetComponent<Animator>();if(!animator)animator=model.AddComponent<Animator>();animator.applyRootMotion=false;
  string controllerPath=Root+"Settings/Athlete.controller";if(File.Exists(controllerPath))AssetDatabase.DeleteAsset(controllerPath);
  var controller=AnimatorController.CreateAnimatorControllerAtPath(controllerPath);controller.AddParameter("Speed",AnimatorControllerParameterType.Float);controller.AddParameter("RunPlayback",AnimatorControllerParameterType.Float);
  controller.AddParameter("Turning",AnimatorControllerParameterType.Bool);controller.AddParameter("TurnAngle",AnimatorControllerParameterType.Float);controller.AddParameter("TurnTime",AnimatorControllerParameterType.Float);controller.AddParameter("RunEntry",AnimatorControllerParameterType.Float);
  controller.AddParameter("Launching",AnimatorControllerParameterType.Bool);
  var allClips=AssetDatabase.LoadAllAssetsAtPath(runPath).OfType<AnimationClip>().Where(c=>!c.name.StartsWith("__preview__")).ToArray();
  var running=allClips.Single(c=>c.name=="Running");
  var states=controller.layers[0].stateMachine;
  var idle=states.AddState("Idle");idle.motion=AssetDatabase.LoadAllAssetsAtPath(idlePath).OfType<AnimationClip>().Single(c=>c.name=="Idle_Playful");
  var run=states.AddState("Run");run.motion=running;run.speedParameter="RunPlayback";run.speedParameterActive=true;states.defaultState=idle;
  run.cycleOffsetParameter="RunEntry";run.cycleOffsetParameterActive=true;
  Transition(idle,run,AnimatorConditionMode.Greater,.2f,.15f);Transition(run,idle,AnimatorConditionMode.Less,.1f,.25f);animator.runtimeAnimatorController=controller;
  var tree=new BlendTree{name="Directional turns",blendType=BlendTreeType.Simple1D,blendParameter="TurnAngle",useAutomaticThresholds=false};
  AssetDatabase.AddObjectToAsset(tree,controller);
  tree.AddChild(turnClips["Turn_Left_180"],-180);tree.AddChild(turnClips["Turn_Left_90"],-90);tree.AddChild(turnClips["Turn_Right_90"],90);tree.AddChild(turnClips["Turn_Right_180"],180);
  var turn=states.AddState("Turn");turn.motion=tree;turn.timeParameter="TurnTime";turn.timeParameterActive=true;
  // Keep the full-body Turn state for authoring audits; gameplay never enters it.
  // Reuse only the torso/head curves over the moving legs, with a runtime weight.
  var mask=new AvatarMask{name="Turn torso and head"};var maskedBones=model.GetComponentsInChildren<Transform>(true);mask.transformCount=maskedBones.Length;
  for(int i=0;i<mask.transformCount;i++){
   string path=AnimationUtility.CalculateTransformPath(maskedBones[i],model.transform);mask.SetTransformPath(i,path);
   mask.SetTransformActive(i,path.EndsWith("mixamorig:Spine")||path.EndsWith("mixamorig:Spine1")||path.EndsWith("mixamorig:Spine2")||path.EndsWith("mixamorig:Neck")||path.EndsWith("mixamorig:Head"));
  }
  for(int i=0;i<(int)AvatarMaskBodyPart.LastBodyPart;i++)mask.SetHumanoidBodyPartActive((AvatarMaskBodyPart)i,false);
  AssetDatabase.AddObjectToAsset(mask,controller);
  controller.AddLayer("Turn expression");var layers=controller.layers;
  layers[1].avatarMask=mask;layers[1].defaultWeight=0;layers[1].blendingMode=AnimatorLayerBlendingMode.Override;controller.layers=layers;
  var expression=layers[1].stateMachine.AddState("Turn expression");expression.motion=tree;expression.timeParameter="TurnTime";expression.timeParameterActive=true;layers[1].stateMachine.defaultState=expression;
  controller.AddParameter("FootballAction",AnimatorControllerParameterType.Int);controller.AddParameter("FootballTime",AnimatorControllerParameterType.Float);
  controller.AddLayer("Football action");var footballLayers=controller.layers;footballLayers[2].defaultWeight=0;controller.layers=footballLayers;
  var footballStates=footballLayers[2].stateMachine;
  var slide=footballStates.AddState("Slide tackle");slide.motion=footballClips["Slide_Tackle"];slide.timeParameter="FootballTime";slide.timeParameterActive=true;footballStates.defaultState=slide;
  var hit=footballStates.AddState("Tackle hit");hit.motion=footballClips["Tackle_Hit"];hit.timeParameter="FootballTime";hit.timeParameterActive=true;
  var toHit=slide.AddTransition(hit);var toSlide=hit.AddTransition(slide);
  foreach(var t in new[]{toHit,toSlide}){t.hasExitTime=false;t.hasFixedDuration=true;t.duration=.04f;t.interruptionSource=TransitionInterruptionSource.Destination;t.orderedInterruption=false;}
  toHit.AddCondition(AnimatorConditionMode.Equals,2,"FootballAction");toSlide.AddCondition(AnimatorConditionMode.Equals,1,"FootballAction");
  // Calibrate contact targets from the imported turn itself, including FBX axes.
  var placement=athlete.AddComponent<TurnFootPlacement>();
  var transforms=model.GetComponentsInChildren<Transform>(true);
  turnClips["Turn_Right_180"].SampleAnimation(model,0);
  placement.hips=transforms.Single(t=>t.name=="mixamorig:Hips");
  foreach(var side in new[]{"Left","Right"}){
   var leg=side=="Left"?placement.left:placement.right;
   leg.thigh=transforms.Single(t=>t.name=="mixamorig:"+side+"UpLeg");leg.knee=transforms.Single(t=>t.name=="mixamorig:"+side+"Leg");leg.ankle=transforms.Single(t=>t.name=="mixamorig:"+side+"Foot");
   leg.initialPosition=athlete.transform.InverseTransformPoint(leg.ankle.position);leg.initialRotation=Quaternion.Inverse(athlete.transform.rotation)*leg.ankle.rotation;
  }
  placement.hipOffset=athlete.transform.InverseTransformPoint(placement.hips.position)-(placement.left.initialPosition+placement.right.initialPosition)*.5f;placement.hipOffset.y=0;
  ((AnimationClip)idle.motion).SampleAnimation(model,0);
  var offlinePrefab=PrefabUtility.SaveAsPrefabAsset(athlete,Root+"Resources/OfflineAthlete.prefab");
  athlete.AddComponent<NetworkObject>();var nt=athlete.AddComponent<NetworkTransform>();nt.Interpolate=true;nt.SyncScaleX=nt.SyncScaleY=nt.SyncScaleZ=false;athlete.AddComponent<NetworkAthlete>();
  var playerPrefab=PrefabUtility.SaveAsPrefabAsset(athlete,Root+"Resources/NetworkAthlete.prefab");UnityEngine.Object.DestroyImmediate(athlete);
  var networkGO=new GameObject("Room network");var transport=networkGO.AddComponent<UnityTransport>();var manager=networkGO.AddComponent<NetworkManager>();manager.NetworkConfig=new NetworkConfig{ProtocolVersion=6,NetworkTransport=transport,PlayerPrefab=playerPrefab,TickRate=30,EnableSceneManagement=false,ConnectionApproval=true};manager.NetworkConfig.Prefabs.Add(new NetworkPrefab{Prefab=playerPrefab});
  var appGO=new GameObject("Sports Application");var rooms=appGO.AddComponent<RoomService>();var app=appGO.AddComponent<AppRoot>();app.rooms=rooms;app.athletePrefab=offlinePrefab;
  var cameraGO=new GameObject("Main Camera");cameraGO.tag="MainCamera";var camera=cameraGO.AddComponent<Camera>();camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=LocalProfile.Hex("BFE2E7");camera.farClipPlane=450;camera.fieldOfView=60;cameraGO.AddComponent<AudioListener>();cameraGO.AddComponent<UniversalAdditionalCameraData>();app.view=cameraGO.AddComponent<PlayerView>();cameraGO.transform.position=new Vector3(87,64,-103);cameraGO.transform.LookAt(Vector3.zero);
  var sunGO=new GameObject("Afternoon sun");var sun=sunGO.AddComponent<Light>();sun.type=LightType.Directional;sun.intensity=1.1f;sun.color=new Color(1,.94f,.85f);sun.shadows=LightShadows.Soft;sun.shadowBias=.08f;sun.shadowNormalBias=.3f;sunGO.transform.rotation=Quaternion.Euler(48,-35,0);RenderSettings.sun=sun;RenderSettings.ambientMode=AmbientMode.Trilight;RenderSettings.ambientSkyColor=new Color(.45f,.55f,.65f);RenderSettings.ambientEquatorColor=new Color(.28f,.35f,.39f);RenderSettings.ambientGroundColor=new Color(.18f,.22f,.18f);RenderSettings.fog=true;RenderSettings.fogColor=camera.backgroundColor;RenderSettings.fogMode=FogMode.Linear;RenderSettings.fogStartDistance=180;RenderSettings.fogEndDistance=430;
  RenderSettings.ambientMode=AmbientMode.Flat;RenderSettings.ambientLight=new Color(.58f,.64f,.7f);DynamicGI.UpdateEnvironment();
  var volumeGO=new GameObject("Gentle colour grading");var volume=volumeGO.AddComponent<Volume>();volume.isGlobal=true;var profile=AssetDatabase.LoadAssetAtPath<VolumeProfile>(Root+"Settings/Colour.asset");if(!profile){profile=ScriptableObject.CreateInstance<VolumeProfile>();AssetDatabase.CreateAsset(profile,Root+"Settings/Colour.asset");var tone=profile.Add<Tonemapping>();tone.mode.Override(TonemappingMode.ACES);AssetDatabase.AddObjectToAsset(tone,profile);}volume.sharedProfile=profile;camera.GetComponent<UniversalAdditionalCameraData>().renderPostProcessing=true;
  foreach(SportId id in Enum.GetValues(typeof(SportId))){string path=Root+"Sports/"+id+"/"+id+".asset";var def=AssetDatabase.LoadAssetAtPath<SportDefinition>(path);if(!def){def=ScriptableObject.CreateInstance<SportDefinition>();AssetDatabase.CreateAsset(def,path);}def.id=id;def.displayName=id.ToString();def.available=true;EditorUtility.SetDirty(def);}
  var basketball=BuildBasketball();var golf=BuildGolf();ConfigureEnvironments(app,stadium,basketball,golf,sun,camera);sv.Apply(LocalProfile.Stadium);EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(),Root+"Scenes/Bootstrap.unity");EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(Root+"Scenes/Bootstrap.unity",true)};AssetDatabase.SaveAssets();
  Debug.Log("PROTOTYPE_SETUP_COMPLETE clips="+string.Join(",",allClips.Select(c=>c.name)));
 }
 static void Transition(AnimatorState a,AnimatorState b,AnimatorConditionMode mode,float threshold,float seconds){var t=a.AddTransition(b);t.hasExitTime=false;t.hasFixedDuration=true;t.duration=seconds;t.interruptionSource=TransitionInterruptionSource.Destination;t.orderedInterruption=false;t.canTransitionToSelf=false;t.AddCondition(mode,threshold,"Speed");}
 static Material RainbowMaterial(){
  string textures=Root+"Art/RainbowSprinter/";
  foreach(var file in new[]{"albedo.png","normal.png","metallic_smoothness.png"}){
   var importer=AssetImporter.GetAtPath(textures+file) as TextureImporter;if(!importer)throw new Exception("Missing Rainbow Sprinter texture: "+file);
   importer.textureType=file=="normal.png"?TextureImporterType.NormalMap:TextureImporterType.Default;
   importer.sRGBTexture=file=="albedo.png";importer.maxTextureSize=2048;importer.textureCompression=TextureImporterCompression.CompressedHQ;importer.SaveAndReimport();
  }
  string path=Root+"Materials/RainbowSprinter.mat";
  var material=AssetDatabase.LoadAssetAtPath<Material>(path);
  if(!material){material=new Material(Shader.Find("Universal Render Pipeline/Lit"));material.name="RainbowSprinter";AssetDatabase.CreateAsset(material,path);}
  material.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(textures+"albedo.png"));
  material.SetTexture("_BumpMap",AssetDatabase.LoadAssetAtPath<Texture2D>(textures+"normal.png"));
  material.SetTexture("_MetallicGlossMap",AssetDatabase.LoadAssetAtPath<Texture2D>(textures+"metallic_smoothness.png"));
  material.SetFloat("_BumpScale",.6f);material.SetFloat("_Metallic",1);material.SetFloat("_Smoothness",1);
  material.EnableKeyword("_NORMALMAP");material.EnableKeyword("_METALLICSPECGLOSSMAP");EditorUtility.SetDirty(material);
  return material;
 }
 static void Remap(GameObject root){foreach(var renderer in root.GetComponentsInChildren<Renderer>()){
  var mats=renderer.sharedMaterials;for(int i=0;i<mats.Length;i++){if(!mats[i])continue;string name=mats[i].name;string path=Root+"Materials/"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);if(!m){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));m.name=name;m.SetColor("_BaseColor",mats[i].color);m.SetFloat("_Smoothness",.18f);m.enableInstancing=true;AssetDatabase.CreateAsset(m,path);}mats[i]=m;}renderer.sharedMaterials=mats;
 }}
 static RectTransform WorldCanvas(string name,Transform parent,Vector3 position,Vector2 size,Quaternion rotation){var o=new GameObject(name,typeof(RectTransform),typeof(Canvas));o.transform.SetParent(parent,false);o.transform.localPosition=position;o.transform.localRotation=rotation;o.transform.localScale=Vector3.one*(size.x/1700);var r=o.GetComponent<RectTransform>();r.sizeDelta=new Vector2(1700,1700*size.y/size.x);o.GetComponent<Canvas>().renderMode=RenderMode.WorldSpace;return r;}
 static Text WorldText(Transform parent,string value,Vector2 position,Vector2 size,int fontSize,Color color){var o=new GameObject("Graphic",typeof(RectTransform),typeof(Text));o.transform.SetParent(parent,false);var t=o.GetComponent<Text>();t.rectTransform.sizeDelta=size;t.rectTransform.anchoredPosition=position;t.text=value;t.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");t.fontSize=fontSize;t.alignment=TextAnchor.MiddleCenter;t.color=color;t.supportRichText=false;t.raycastTarget=false;return t;}
 [MenuItem("Sports/Build Windows testing player")]
 public static void BuildWindows(){
  Setup();Build(BuildTarget.StandaloneWindows64,"../Builds/WindowsFinal/SportsPrototype.exe");
  File.WriteAllText("../Builds/WindowsFinal/LATEST-BUILD.txt","Built UTC: "+DateTime.UtcNow.ToString("O")+"\nFootball: Sunvale modular stadium\nBasketball: Rally Court modular arena\nGolf: Tidebloom Island, sculpted terrain and tropical art\nScene: Assets/_Game/Scenes/Bootstrap.unity\nRebuild: Tools/Build/Build.ps1 -Target Windows\n");
 }
 public static void BuildWindowsReview(){Build(BuildTarget.StandaloneWindows64,"../Builds/BasketballReview/SportsPrototype.exe");}
 public static void SetupAndBuildWindows(){BuildWindows();}
 [MenuItem("Sports/Build Android development APK")]
 public static void BuildAndroid(){EditorUserBuildSettings.buildAppBundle=false;Build(BuildTarget.Android,"../Builds/Android/SportsPrototype.apk");}
 [MenuItem("Sports/Build Android release APK")]
 public static void BuildAndroidRelease(){EditorUserBuildSettings.buildAppBundle=false;Build(BuildTarget.Android,"../Builds/Android/SportsPrototype-release.apk",BuildOptions.CompressWithLz4HC);}
 static void Build(BuildTarget target,string path,BuildOptions options=BuildOptions.Development|BuildOptions.CompressWithLz4HC){Directory.CreateDirectory(Path.GetDirectoryName(path));var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{Root+"Scenes/Bootstrap.unity"},locationPathName=path,target=target,options=options});if(report.summary.result!=BuildResult.Succeeded)throw new Exception("Build failed: "+report.summary.result);Debug.Log("BUILD_OK "+target+" bytes="+report.summary.totalSize);}
}
