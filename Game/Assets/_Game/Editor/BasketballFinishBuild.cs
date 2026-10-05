using UnityEditor;
using UnityEditor.SceneManagement;

namespace WhatTheFish.Editor {
 public static class BasketballFinishBuild {
  public static void Windows(){
   AssetDatabase.Refresh();EditorSceneManager.OpenScene("Assets/_Game/Scenes/Bootstrap.unity");
   ProjectBuilder.BuildWindowsPlayer("../Builds/BasketballFinishQA/Player/WhatTheFish.exe");
  }
 }
}
