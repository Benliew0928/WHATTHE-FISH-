#if UNITY_EDITOR
public static class FootballReplayBuild {
 public static void BuildReview()=>ProjectBuilder.BuildWindowsPlayer("../Builds/FootballReplayQA/Player/WhatTheFish.exe");
 public static void BuildTimingReview()=>ProjectBuilder.BuildWindowsPlayer("../Builds/FootballReplayTimingQA/Player/WhatTheFish.exe");
}
#endif
