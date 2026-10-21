#if UNITY_EDITOR
namespace WhatTheFish.Editor {
 public static class IslandSettingsBuild {
  public static void Build()=>ProjectBuilder.BuildWindowsPlayer("../Builds/IslandSettingsQA/Player/WhatTheFish.exe");
 }
}
#endif
