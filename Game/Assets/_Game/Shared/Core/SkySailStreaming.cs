using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace WhatTheFish {
 public sealed class SkySailStreaming:MonoBehaviour {
  public bool enabledForWorld;
  public bool Busy {get;private set;}
  public string Error {get;private set;}
  SportEnvironmentController environment;
  void Awake(){environment=GetComponent<SportEnvironmentController>();}
  public static string SceneName(SportId id)=>"SkySail_"+id;
  public bool Loaded(SportId id)=>environment.roots[(int)id];
  void Bind(SportId id){
   var scene=SceneManager.GetSceneByName(SceneName(id));
   foreach(var root in scene.GetRootGameObjects())if(root.GetComponent<SkySailIslandScene>()){environment.roots[(int)id]=root;return;}
   throw new InvalidOperationException("Island scene has no root: "+id);
  }
  public void Ensure(SportId id){
   if(!enabledForWorld||Loaded(id))return;
   throw new InvalidOperationException("Prepare the island asynchronously before activation: "+id);
  }
  public IEnumerator Prepare(SportId id){
   Error=null;while(Busy)yield return null;if(Loaded(id))yield break;Busy=true;
   AsyncOperation operation=null;
   try{operation=SceneManager.LoadSceneAsync(SceneName(id),LoadSceneMode.Additive);}catch(Exception e){Error=e.Message;}
   if(operation!=null){yield return operation;try{Bind(id);}catch(Exception e){Error=e.Message;}}
   Busy=false;
  }
  public IEnumerator Release(SportId id){
   while(Busy)yield return null;var scene=SceneManager.GetSceneByName(SceneName(id));if(!scene.isLoaded)yield break;
   Busy=true;environment.roots[(int)id]=null;yield return SceneManager.UnloadSceneAsync(scene);yield return Resources.UnloadUnusedAssets();Busy=false;
  }
  public void ReleaseOtherIslands(SportId keep){if(!enabledForWorld)return;StartCoroutine(ReleaseOthers(keep));}
  IEnumerator ReleaseOthers(SportId keep){foreach(SportId id in Enum.GetValues(typeof(SportId)))if(id!=keep&&Loaded(id))yield return Release(id);}
 }
}
