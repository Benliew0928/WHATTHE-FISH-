using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using UnityEditor.Android;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

// Applies to the generated Gradle project, keeping Unity's current toolchain templates intact.
public sealed class HuaweiScanBuild:IPostGenerateGradleAndroidProject,IPostprocessBuildWithReport {
 public int callbackOrder=>100;
 public void OnPostprocessBuild(BuildReport report) {
  if(report.summary.platform!=UnityEditor.BuildTarget.StandaloneWindows64)return;
  string notices=Path.Combine(Path.GetDirectoryName(report.summary.outputPath),"ThirdPartyNotices");Directory.CreateDirectory(notices);
  File.Copy(Path.Combine(Application.dataPath,"ThirdParty/CodecreteQr/LICENSE.txt"),Path.Combine(notices,"CodecreteQr.txt"),true);
 }
 public void OnPostGenerateGradleAndroidProject(string path) {
  string root=Path.GetFullPath(Path.Combine(Application.dataPath,"../.."));
  string source=Path.Combine(root,"Tools/Android/RoomScan");
  string target=Path.Combine(path,"src/main/java/com/umpsa/whatthefish/scan");Directory.CreateDirectory(target);
  foreach(string file in Directory.GetFiles(source,"*.java"))File.Copy(file,Path.Combine(target,Path.GetFileName(file)),true);
  string notices=Path.Combine(path,"src/main/assets/ThirdPartyNotices");Directory.CreateDirectory(notices);
  File.Copy(Path.Combine(Application.dataPath,"ThirdParty/CodecreteQr/LICENSE.txt"),Path.Combine(notices,"CodecreteQr.txt"),true);
  string gradlePath=Path.Combine(path,"build.gradle"),gradle=File.ReadAllText(gradlePath);
  if(!gradle.Contains("com.huawei.hms:scanplus:"))gradle+="\ndependencies { implementation 'com.huawei.hms:scanplus:2.15.0.301' }\n";
  File.WriteAllText(gradlePath,gradle);
  string settingsPath=Path.Combine(path,"../settings.gradle"),settings=File.ReadAllText(settingsPath);
  if(!settings.Contains("https://developer.huawei.com/repo/")) {
   const string marker="dependencyResolutionManagement {";int index=settings.IndexOf(marker,StringComparison.Ordinal);
   if(index<0)throw new BuildFailedException("Huawei Scan Kit: generated dependencyResolutionManagement was not found.");
   settings=settings.Insert(index+marker.Length,"\n    repositories { maven { url 'https://developer.huawei.com/repo/' } }\n");
   File.WriteAllText(settingsPath,settings);
  }
  var manifestPath=Path.Combine(path,"src/main/AndroidManifest.xml");var manifest=XDocument.Load(manifestPath);XNamespace a="http://schemas.android.com/apk/res/android";
  var document=manifest.Root;var app=document.Element("application");
  if(!document.Elements("uses-permission").Any(e=>(string)e.Attribute(a+"name")=="android.permission.CAMERA"))document.Add(new XElement("uses-permission",new XAttribute(a+"name","android.permission.CAMERA")));
  foreach(string name in new[]{"android.hardware.camera","android.hardware.camera.autofocus"})if(!document.Elements("uses-feature").Any(e=>(string)e.Attribute(a+"name")==name))document.Add(new XElement("uses-feature",new XAttribute(a+"name",name),new XAttribute(a+"required","false")));
  const string activity="com.umpsa.whatthefish.scan.RoomScanActivity";
  if(!app.Elements("activity").Any(e=>(string)e.Attribute(a+"name")==activity))app.Add(new XElement("activity",new XAttribute(a+"name",activity),new XAttribute(a+"exported","false"),new XAttribute(a+"screenOrientation","sensorLandscape"),new XAttribute(a+"configChanges","orientation|screenSize|keyboardHidden|uiMode"),new XAttribute(a+"hardwareAccelerated","true"),new XAttribute(a+"theme","@android:style/Theme.Material.NoActionBar.Fullscreen")));
  // A local environment setting can associate this SDK with the team's registered AGC app.
  string appId=Environment.GetEnvironmentVariable("WTF_HUAWEI_APP_ID");
  if(!string.IsNullOrEmpty(appId)) {
   if(!appId.All(char.IsDigit))throw new BuildFailedException("WTF_HUAWEI_APP_ID must contain only the numeric AppGallery Connect app ID.");
   const string key="com.huawei.hms.client.appid";app.Elements("meta-data").Where(e=>(string)e.Attribute(a+"name")==key).Remove();
   app.Add(new XElement("meta-data",new XAttribute(a+"name",key),new XAttribute(a+"value","appid="+appId)));
  }
  manifest.Save(manifestPath);
  string keep=Path.Combine(path,"proguard-unity.txt"),rules=File.ReadAllText(keep);
  rules=System.Text.RegularExpressions.Regex.Replace(rules,"(?s)\\n# BEGIN ROOM SCAN.*?# END ROOM SCAN\\n","");
  File.WriteAllText(keep,rules+"\n# BEGIN ROOM SCAN\n"+File.ReadAllText(Path.Combine(source,"proguard-rules.pro"))+"\n# END ROOM SCAN\n");
  Debug.Log("HUAWEI_SCAN_BUILD sdk=scanplus:2.15.0.301 appIdConfigured="+!string.IsNullOrEmpty(appId));
 }
 public static void Windows()=>ProjectBuilder.BuildWindowsPlayer("../Builds/ScanKitQA/Player/WhatTheFish.exe");
}
