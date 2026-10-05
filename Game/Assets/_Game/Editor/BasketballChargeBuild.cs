using UnityEditor;
using UnityEditor.SceneManagement;

namespace WhatTheFish.Editor {
 public static class BasketballChargeBuild {
  public static void Windows(){
   AssetDatabase.Refresh();EditorSceneManager.OpenScene("Assets/_Game/Scenes/Bootstrap.unity");
   ProjectBuilder.BuildWindowsPlayer("../Builds/BasketballChargeQA/Player/WhatTheFish.exe");
  }
 }
}
