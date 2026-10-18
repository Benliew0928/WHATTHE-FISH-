using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace WhatTheFish {
 // Unity objects can still compare non-null while their scene is being torn
 // down. Gameplay restoration must not reactivate that scene or create new UI.
 public static class SceneLifetime {
  static readonly HashSet<int> unloading=new();
  public static bool Quitting {get;private set;}
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]static void Initialize(){
   Quitting=false;unloading.Clear();Application.quitting-=Quit;Application.quitting+=Quit;
   SceneManager.sceneUnloaded-=Unloaded;SceneManager.sceneUnloaded+=Unloaded;
  }
  static void Quit()=>Quitting=true;
  static void Unloaded(Scene scene)=>unloading.Remove(scene.handle);
  internal static void BeginUnload(Scene scene)=>unloading.Add(scene.handle);
  internal static void EndUnload(Scene scene)=>unloading.Remove(scene.handle);
  public static bool CanPresent(GameObject target)=>target&&!Quitting&&target.scene.isLoaded&&!unloading.Contains(target.scene.handle);
 }
}
