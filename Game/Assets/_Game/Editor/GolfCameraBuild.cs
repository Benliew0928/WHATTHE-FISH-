#if UNITY_EDITOR
public static class GolfCameraBuild {
 public static void BuildReview()=>ProjectBuilder.BuildWindowsPlayer("../Builds/GolfCameraQA/Player/WhatTheFish.exe");
}
#endif
