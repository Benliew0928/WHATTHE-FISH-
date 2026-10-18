#if UNITY_EDITOR
public static class SportsTrailBuild {
 public static void BuildReview()=>ProjectBuilder.BuildWindowsPlayer("../Builds/SportsTrailQA/Player/WhatTheFish.exe");
 public static void BuildFinal()=>ProjectBuilder.BuildWindowsPlayer("../Builds/SportsTrailQA/FinalPlayer/WhatTheFish.exe");
}
#endif
