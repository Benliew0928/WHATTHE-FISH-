using UnityEditor;
using UnityEditor.SceneManagement;

namespace WhatTheFish.Editor {
 public static class BasketballStealBuild {
  public static void Windows(){
   AssetDatabase.Refresh();EditorSceneManager.OpenScene("Assets/_Game/Scenes/Bootstrap.unity");
   ProjectBuilder.BuildWindowsPlayer("../Builds/BasketballStealQA/Player/WhatTheFish.exe");
  }
  public static void AnimationWindows(){
   AssetDatabase.Refresh();EditorSceneManager.OpenScene("Assets/_Game/Scenes/Bootstrap.unity");
   ProjectBuilder.BuildWindowsPlayer("../Builds/BasketballStealAnimationQA/Player/WhatTheFish.exe");
  }
 }
}
