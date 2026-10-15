#if UNITY_EDITOR
public static class MatchExitBuild {
 public static void BuildReview(){BasketballHonoursBuild.CheckStats();FootballPlaymakerBuild.CheckStats();ProjectBuilder.BuildWindowsPlayer("../Builds/MatchExitQA/Player/WhatTheFish.exe");}
}
#endif
