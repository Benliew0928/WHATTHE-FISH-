#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace WhatTheFish {
 // Opt-in checks of the real packaged character, skinning, cameras and equipment.
 public sealed class FirstPersonBodyReview:MonoBehaviour {
  string folder;bool failed;int checks;AppRoot app;Athlete actor;PlayerView view;Camera camera;
  SkinnedMeshRenderer[] skins;Transform head;Vector3 headScale;CharacterIdentity identity;
  MaterialPropertyBlock block;static readonly int Mask=Shader.PropertyToID("_HideFirstPersonHead");
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Boot(){var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,"-firstPersonBodyReview");if(i>=0&&i+1<args.Length)new GameObject("First person body review").AddComponent<FirstPersonBodyReview>().folder=args[i+1];}
  void Check(bool ok,string label){checks++;failed|=!ok;File.AppendAllText(Path.Combine(folder,"results.txt"),(ok?"PASS ":"FAIL ")+label+"\n");}
  IEnumerator Until(Func<bool> done,string label,float seconds=90){float end=Time.realtimeSinceStartup+seconds;while(!done()&&Time.realtimeSinceStartup<end)yield return null;Check(done(),label);}
  void Place(Vector3 point,float yaw){actor.capsule.enabled=false;actor.transform.SetPositionAndRotation(point,Quaternion.Euler(0,yaw,0));actor.ResetLocomotion();actor.capsule.enabled=true;view.yaw=yaw;Physics.SyncTransforms();}
  void CheckPose(bool first,string label){
   Check(Vector3.Distance(head.localScale,headScale)<.0001f,label+" head bone keeps original scale");
   foreach(var skin in skins){skin.GetPropertyBlock(block);Check(Mathf.Abs(block.GetFloat(Mask)-(first?1:0))<.001f,label+" local mask "+skin.name);Check(skin.enabled,label+" body retained "+skin.name);Check(skin.sharedMaterial.GetFloat(Mask)==0,label+" shared remote material unchanged "+skin.name);Check(skin.sharedMaterial.shader.isSupported,label+" shader supported "+skin.name);}
   if(identity.Preset!=0)Check(identity.AddedRenderers.All(r=>r.enabled&&r.shadowCastingMode==(first?ShadowCastingMode.ShadowsOnly:ShadowCastingMode.On)),label+" hair/scalp visibility and shadows");
  }
  IEnumerator Start(){
   block=new MaterialPropertyBlock();Directory.CreateDirectory(folder);File.WriteAllText(Path.Combine(folder,"results.txt"),"");yield return Until(()=>DevelopmentProbe.ReadyToReviewSport&&PackagedContentBoot.Ready,"packaged sport ready");if(!DevelopmentProbe.ReadyToReviewSport){Finish();yield break;}
   app=AppRoot.Instance;actor=app.LocalAthlete;view=app.view;camera=Camera.main;identity=actor.GetComponent<CharacterIdentity>();skins=actor.visual.GetComponentsInChildren<SkinnedMeshRenderer>(true);head=actor.visual.GetComponentsInChildren<Transform>(true).First(t=>t.name=="mixamorig:Head");headScale=head.localScale;
   DevelopmentProbe.TurnCommandActive=true;DevelopmentProbe.TurnCommand=default;while(IslandTutorialUI.DismissTop()){}view.mode=1;QualitySettings.vSyncCount=0;Application.targetFrameRate=30;
   bool golf=app.SelectedSport==SportId.Golf;
   if(golf){var game=GolfMatchManager.Instance;bool started=false;yield return Until(()=>started||(started=game.StartTestMatch(new[]{(GolfCartWorld.Key(actor),actor,"You")})),"golf match starts",20);game.Events.Clear();var hole=game.Course.holes.OrderBy(h=>h.number).First();game.Ball(actor).Place(hole.tee.position+Vector3.up*.05f,true);Place(hole.tee.position+Vector3.back*.75f+Vector3.up*.1f,0);view.SkipGolfReplay();}
   else {var game=FishingGame.Instance;game.ResetOfflinePractice();var point=game.Pier(0);var direction=Vector3.ProjectOnPlane(game.transform.position-point,Vector3.up).normalized;Place(point,Quaternion.LookRotation(direction).eulerAngles.y);}
   yield return new WaitForSeconds(.7f);while(IslandTutorialUI.DismissTop()){}
   Check(skins.Length==2&&skins.All(s=>s.sharedMesh.GetVertexAttributeDimension(VertexAttribute.TexCoord2)==4),"both packaged LOD meshes contain head mask");
   Check(skins.All(s=>s.sharedMesh.GetBlendShapeIndex("GolfRightGrip")>=0),"both hand grip blendshapes retained");
   Check(golf?actor.GolfClubMotion.ShowBodyInFirstPerson:actor.GetComponent<FishingRodMotion>().Equipped,"equipment retains first-person body");
   var lod=actor.visual.GetComponent<LODGroup>();
   for(byte number=1;number<=10;number++){
    identity.SetNumber(number);
    for(int level=0;level<2;level++){
     lod.ForceLOD(level);view.mode=0;view.pitch=65;yield return new WaitForSeconds(.12f);yield return new WaitForEndOfFrame();string label="identity-"+number+"-lod-"+level;CheckPose(true,label+" first person");
     if(number==1){
      // Reproduce the former bone-collapse path in a single render, then restore.
      foreach(var skin in skins){skin.GetPropertyBlock(block);block.SetFloat(Mask,0);skin.SetPropertyBlock(block);}head.localScale=Vector3.one*.001f;Capture(label+"-old-collapse");head.localScale=headScale;actor.HideHead(true);yield return null;yield return new WaitForEndOfFrame();
     }
     Capture(label+"-down");view.mode=1;yield return new WaitForSeconds(.12f);yield return new WaitForEndOfFrame();CheckPose(false,label+" third person");if(level==0&&(number==1||number==6||number==10))Capture(label+"-third-person");
    }
   }
   identity.SetNumber(1);lod.ForceLOD(-1);view.mode=0;
   foreach(float pitch in new[]{0f,40f,65f}){view.pitch=pitch;yield return new WaitForSeconds(.15f);yield return new WaitForEndOfFrame();CheckPose(true,"pitch "+pitch);Capture("look-"+pitch);}
   if(golf){Check(view.BeginGolfAim(),"first-person golf aim starts");yield return new WaitForSeconds(.5f);yield return new WaitForEndOfFrame();CheckPose(false,"aim third-person body restored");Capture("aim");view.CancelGolfAim();yield return new WaitForSeconds(.2f);yield return new WaitForEndOfFrame();CheckPose(true,"cancel aim returns to first person");}
   Finish();
  }
  void Capture(string name){
   // Render world/body without screen overlays; this works in hidden test windows.
   var target=camera.targetTexture;var active=RenderTexture.active;var render=RenderTexture.GetTemporary(Screen.width,Screen.height,24);var pixels=new Texture2D(Screen.width,Screen.height,TextureFormat.RGB24,false);
   try{RenderPipeline.SubmitRenderRequest(camera,new UniversalRenderPipeline.SingleCameraRequest{destination=render});RenderTexture.active=render;pixels.ReadPixels(new Rect(0,0,Screen.width,Screen.height),0,0);pixels.Apply();File.WriteAllBytes(Path.Combine(folder,name+".png"),pixels.EncodeToPNG());var colors=pixels.GetPixels32();Check(colors.Where((_,i)=>i%100==0).Select(c=>(c.r/16,c.g/16,c.b/16)).Distinct().Count()>20,"nonblank "+name);Check(colors.Count(c=>c.r>230&&c.b>230&&c.g<30)<colors.Length/100,"no missing shader "+name);}
   finally{camera.targetTexture=target;RenderTexture.active=active;RenderTexture.ReleaseTemporary(render);Destroy(pixels);}
  }
  void Finish(){File.AppendAllText(Path.Combine(folder,"results.txt"),"FIRST_PERSON_BODY_COMPLETE checks="+checks+" success="+!failed+"\n");Application.Quit(failed?1:0);}
 }
}
#endif
