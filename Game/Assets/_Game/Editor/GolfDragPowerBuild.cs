#if UNITY_EDITOR
namespace WhatTheFish.Editor {
 public static class GolfDragPowerBuild {
  public static void Build()=>ProjectBuilder.BuildWindowsPlayer("../Builds/GolfDragPowerQA/Player/WhatTheFish.exe");
 }
}
#endif
