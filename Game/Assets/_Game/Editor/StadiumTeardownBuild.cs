#if UNITY_EDITOR
public static class StadiumTeardownBuild {
 public static void BuildReview()=>ProjectBuilder.BuildWindowsPlayer("../Builds/StadiumTeardownQA/Player/WhatTheFish.exe");
}
#endif
