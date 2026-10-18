#if UNITY_EDITOR
public static class BasketballCelebrationBuild {
 public static void BuildReview(){
  BasketballHonoursBuild.CheckStats();
  ProjectBuilder.BuildWindowsPlayer("../Builds/BasketballCelebrationQA/Player/WhatTheFish.exe");
 }
}
#endif
