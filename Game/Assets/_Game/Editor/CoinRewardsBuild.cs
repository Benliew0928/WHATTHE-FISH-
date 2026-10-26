using UnityEditor;
public static class CoinRewardsBuild {
 public static void Windows(){
  AssetDatabase.Refresh();AssetDatabase.ImportAsset("Assets/_Game/Resources/Menu/GoldFishCoin.png",ImportAssetOptions.ForceUpdate);
  var coin=UnityEngine.Resources.Load<UnityEngine.Sprite>("Menu/GoldFishCoin");
  if(!coin||coin.texture.width!=256||coin.texture.height!=256)throw new UnityEditor.Build.BuildFailedException("Gold fish coin must import as a 256px sprite.");
  ProjectBuilder.BuildWindowsPlayer("../Builds/CoinRewardsQA/Player/WhatTheFish.exe");
 }
 public static void Scripts(){var report=PackagedContentBuild.BuildScripts(BuildTarget.StandaloneWindows64,ProjectBuilder.ResolveBuildOutputPath("../Builds/CoinRewardsQA/Player/WhatTheFish.exe"),BuildOptions.Development|BuildOptions.CompressWithLz4HC);if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new UnityEditor.Build.BuildFailedException("Coin calendar compilation failed.");}
}
