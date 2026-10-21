using System.IO;
using System.Linq;
using System.Xml.Linq;
using UnityEditor.Android;
using UnityEngine;

public sealed class ProfilePhotoBuild:IPostGenerateGradleAndroidProject {
 public int callbackOrder=>120;
 public void OnPostGenerateGradleAndroidProject(string path) {
  string root=Path.GetFullPath(Path.Combine(Application.dataPath,"../.."));
  string target=Path.Combine(path,"src/main/java/com/umpsa/whatthefish/profiles");Directory.CreateDirectory(target);
  foreach(var file in Directory.GetFiles(Path.Combine(root,"Tools/Android/ProfilePhoto"),"*.java"))File.Copy(file,Path.Combine(target,Path.GetFileName(file)),true);
  string xml=Path.Combine(path,"src/main/res/xml");Directory.CreateDirectory(xml);
  File.WriteAllText(Path.Combine(xml,"profile_photo_paths.xml"),"<paths xmlns:android=\"http://schemas.android.com/apk/res/android\"><cache-path name=\"capture\" path=\"profile-capture/\" /></paths>");
  string manifestPath=Path.Combine(path,"src/main/AndroidManifest.xml");var doc=XDocument.Load(manifestPath);
  XNamespace a="http://schemas.android.com/apk/res/android";var app=doc.Root.Element("application");
  const string activity="com.umpsa.whatthefish.profiles.PhotoActivity",authority="${applicationId}.profilephotos";
  app.Elements("activity").Where(e=>(string)e.Attribute(a+"name")==activity).Remove();
  app.Add(new XElement("activity",new XAttribute(a+"name",activity),new XAttribute(a+"exported","false"),
   new XAttribute(a+"configChanges","orientation|screenSize|keyboardHidden|uiMode"),new XAttribute(a+"theme","@android:style/Theme.Material.Light.NoActionBar")));
  app.Elements("provider").Where(e=>(string)e.Attribute(a+"authorities")==authority).Remove();
  app.Add(new XElement("provider",new XAttribute(a+"name","androidx.core.content.FileProvider"),new XAttribute(a+"authorities",authority),
   new XAttribute(a+"exported","false"),new XAttribute(a+"grantUriPermissions","true"),
   new XElement("meta-data",new XAttribute(a+"name","android.support.FILE_PROVIDER_PATHS"),new XAttribute(a+"resource","@xml/profile_photo_paths"))));
  doc.Save(manifestPath);
  string keep=Path.Combine(path,"proguard-unity.txt"),rules=File.ReadAllText(keep);
  if(!rules.Contains("com.umpsa.whatthefish.profiles"))File.AppendAllText(keep,"\n-keep class com.umpsa.whatthefish.profiles.** { *; }\n");
 }
 public static void Windows()=>ProjectBuilder.BuildWindowsPlayer("../Builds/PlayerProfilesQA/Player/WhatTheFish.exe");
 // Only after a full profile build, with unchanged serialized assets/type layouts.
 public static void WindowsScripts(){
  var report=PackagedContentBuild.BuildScripts(UnityEditor.BuildTarget.StandaloneWindows64,
   ProjectBuilder.ResolveBuildOutputPath("../Builds/PlayerProfilesQA/Player/WhatTheFish.exe"),
   UnityEditor.BuildOptions.Development|UnityEditor.BuildOptions.CompressWithLz4HC);
  if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new UnityEditor.Build.BuildFailedException("Profile scripts build failed.");
 }
}
