#if UNITY_EDITOR
using System.IO;
using System.Linq;
using System.Xml.Linq;
using UnityEditor.Android;
using UnityEditor.Build.Reporting;
using UnityEditor;
namespace WhatTheFish.Editor {
 public sealed class RoomVoiceBuild:IPostGenerateGradleAndroidProject {
  public int callbackOrder=>150;
  public void OnPostGenerateGradleAndroidProject(string path){
   string file=Path.Combine(path,"src/main/AndroidManifest.xml");var doc=XDocument.Load(file);XNamespace a="http://schemas.android.com/apk/res/android";
   foreach(string permission in new[]{"android.permission.RECORD_AUDIO","android.permission.MODIFY_AUDIO_SETTINGS","android.permission.BLUETOOTH_CONNECT"}){
    if(!doc.Root.Elements("uses-permission").Any(e=>(string)e.Attribute(a+"name")==permission))doc.Root.Add(new XElement("uses-permission",new XAttribute(a+"name",permission)));
   }
   OptionalMicrophone(doc);
   // Unity must not request the mic on app launch. The Join voice action owns the prompt.
   var app=doc.Root.Element("application");const string key="unityplayer.SkipPermissionsDialog";
   app.Elements("meta-data").Where(e=>(string)e.Attribute(a+"name")==key).Remove();app.Add(new XElement("meta-data",new XAttribute(a+"name",key),new XAttribute(a+"value","true")));
   doc.Save(file);
   // Android merges required uses-feature flags with OR. The launcher's explicit
   // replacement must win over the SDK AAR's required microphone declaration.
   string launcher=Path.GetFullPath(Path.Combine(path,"../launcher/src/main/AndroidManifest.xml"));
   var launcherDoc=XDocument.Load(launcher);OptionalMicrophone(launcherDoc);launcherDoc.Save(launcher);
  }
  static void OptionalMicrophone(XDocument doc){
   XNamespace a="http://schemas.android.com/apk/res/android",tools="http://schemas.android.com/tools";
   doc.Root.SetAttributeValue(XNamespace.Xmlns+"tools",tools.NamespaceName);
   doc.Root.Elements("uses-feature").Where(e=>(string)e.Attribute(a+"name")=="android.hardware.microphone").Remove();
   doc.Root.Add(new XElement("uses-feature",new XAttribute(a+"name","android.hardware.microphone"),new XAttribute(a+"required","false"),new XAttribute(tools+"node","replace")));
  }
  public static void Windows()=>ProjectBuilder.BuildWindowsPlayer("../Builds/VoiceQA/Player/WhatTheFish.exe");
  public static void Scripts(){var r=PackagedContentBuild.BuildScripts(BuildTarget.StandaloneWindows64,ProjectBuilder.ResolveBuildOutputPath("../Builds/VoiceQA/Player/WhatTheFish.exe"),BuildOptions.Development|BuildOptions.CompressWithLz4HC);if(r.summary.result!=BuildResult.Succeeded)throw new System.Exception("Voice scripts build failed.");}
 }
}
#endif
