using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using UnityEditor.Android;
using UnityEditor.Build;
using UnityEngine;

public sealed class AccountSignInBuild:IPostGenerateGradleAndroidProject {
 public int callbackOrder=>110;
 public void OnPostGenerateGradleAndroidProject(string path) {
  string root=Path.GetFullPath(Path.Combine(Application.dataPath,"../.."));
  string target=Path.Combine(path,"src/main/java/com/umpsa/whatthefish/accounts");Directory.CreateDirectory(target);
  foreach(var file in Directory.GetFiles(Path.Combine(root,"Tools/Android/AccountSignIn"),"*.java"))
   File.Copy(file,Path.Combine(target,Path.GetFileName(file)),true);
  string gradlePath=Path.Combine(path,"build.gradle"),gradle=File.ReadAllText(gradlePath);
  // 6.15.0.300 advertises an AAR but its official POM returns 404 (20 Oct 2026).
  // Pin the latest verified complete publication instead of guessing transitive dependencies.
  void Dependency(string artifact,string version){
   string pattern=@"implementation\s+['""]"+Regex.Escape(artifact)+@":[^'""]+['""]";
   string declaration="implementation '"+artifact+":"+version+"'";
   // Unity reuses the generated Gradle tree during incremental builds.
   gradle=Regex.IsMatch(gradle,pattern)?Regex.Replace(gradle,pattern,declaration):gradle+"\ndependencies { "+declaration+" }\n";
  }
  Dependency("com.huawei.hms:hwid","6.14.0.300");
  Dependency("androidx.credentials:credentials","1.6.0");
  Dependency("androidx.credentials:credentials-play-services-auth","1.6.0");
  Dependency("com.google.android.libraries.identity.googleid:googleid","1.2.1");
  Dependency("com.google.android.gms:play-services-basement","18.12.0");
  File.WriteAllText(gradlePath,gradle);
  var manifestPath=Path.Combine(path,"src/main/AndroidManifest.xml");var manifest=XDocument.Load(manifestPath);
  XNamespace a="http://schemas.android.com/apk/res/android";var app=manifest.Root.Element("application");
  const string activity="com.umpsa.whatthefish.accounts.AccountActivity";
  if(!app.Elements("activity").Any(e=>(string)e.Attribute(a+"name")==activity))
   app.Add(new XElement("activity",new XAttribute(a+"name",activity),new XAttribute(a+"exported","false"),
    new XAttribute(a+"configChanges","orientation|screenSize|keyboardHidden|uiMode"),new XAttribute(a+"theme","@android:style/Theme.Material.Light.NoActionBar")));
  string google=Environment.GetEnvironmentVariable("WTF_GOOGLE_WEB_CLIENT_ID")??"";
  string endpoint=Environment.GetEnvironmentVariable("WTF_HUAWEI_AUTH_URL")??"";
  if(google.Length>0&&!System.Text.RegularExpressions.Regex.IsMatch(google,@"^[a-zA-Z0-9-]+\.apps\.googleusercontent\.com$"))
   throw new BuildFailedException("WTF_GOOGLE_WEB_CLIENT_ID must be the public OAuth Web client ID.");
  if(endpoint.Length>0&&(!Uri.TryCreate(endpoint,UriKind.Absolute,out var uri)||uri.Scheme!="https"||uri.UserInfo.Length>0||uri.Query.Length>0||uri.Fragment.Length>0))
   throw new BuildFailedException("WTF_HUAWEI_AUTH_URL must be a HTTPS endpoint with no credentials, query or fragment.");
  void Metadata(string key,string value){app.Elements("meta-data").Where(e=>(string)e.Attribute(a+"name")==key).Remove();app.Add(new XElement("meta-data",new XAttribute(a+"name",key),new XAttribute(a+"value",value)));}
  Metadata("wtf.google.client",google);Metadata("wtf.huawei.endpoint",endpoint);manifest.Save(manifestPath);
  string keep=Path.Combine(path,"proguard-unity.txt");
  string rules=File.ReadAllText(keep).Replace("-keep class com.huawei.hms.** { *; }","");
  // Provider AARs supply consumer rules; retain only the Unity JNI bridge ourselves.
  if(!rules.Contains("com.umpsa.whatthefish.accounts"))
   rules+="\n-keep class com.umpsa.whatthefish.accounts.** { *; }\n";
  // HMS base 6.13 guards this optional installer class with AvailableUtil.isInstallerLibExist.
  // This app does not bundle or launch the optional HMS installer.
  if(!rules.Contains("-dontwarn com.huawei.hms.availableupdate.UpdateAdapterMgr"))
   rules+="\n-dontwarn com.huawei.hms.availableupdate.UpdateAdapterMgr\n";
  File.WriteAllText(keep,rules);
  Debug.Log("ACCOUNT_BUILD googleConfigured="+(google.Length>0)+" huaweiEndpointConfigured="+(endpoint.Length>0));
 }
 public static void Windows()=>ProjectBuilder.BuildWindowsPlayer("../Builds/AccountSignInQA/Player/WhatTheFish.exe");
}
