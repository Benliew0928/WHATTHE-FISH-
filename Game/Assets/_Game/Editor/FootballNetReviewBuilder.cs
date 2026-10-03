using UnityEditor;
using UnityEditor.SceneManagement;

public static partial class ProjectBuilder {
 // Uses the configured delivery pipeline for a representative low-cost preview.
 public static void BuildFootballNetReview(){
  EditorSceneManager.OpenScene("Assets/_Game/Scenes/Bootstrap.unity");
  Build(BuildTarget.StandaloneWindows64,"../Builds/FootballNetQA/Player/WhatTheFish.exe",BuildOptions.Development|BuildOptions.CompressWithLz4);
 }
}
