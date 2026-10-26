using System;
using System.Linq;
using UnityEngine;

namespace WhatTheFish {
 // Website/content.json is the authoring source; Prepare-PublicInfo.py refreshes
 // the bundled English keys and the shared three-language localization catalog.
 public static class PublicInfoDocuments {
  [Serializable] public sealed class Section { public string heading,body; }
  [Serializable] public sealed class Document { public string id,title,intro; public Section[] sections; }
  [Serializable] sealed class Bundle { public Document[] documents; }
  [Serializable] sealed class Links { public string baseUrl; }
  static Bundle bundle;
  public static Document Get(string id){
   if(bundle==null){var asset=Resources.Load<TextAsset>("AppGallery/Documents");bundle=asset?JsonUtility.FromJson<Bundle>(asset.text):new Bundle();}
   return bundle.documents?.FirstOrDefault(d=>d.id==id);
  }
  public static string UrlFor(string id){
   var asset=Resources.Load<TextAsset>("AppGallery/Links");
   string root=asset?JsonUtility.FromJson<Links>(asset.text)?.baseUrl:"";
   string value=BuildUrl(root,id,GameLocalization.Code);
   // Preserve the previously documented standalone policy configuration.
   if(value.Length==0&&id=="privacy"){
    var legacy=Resources.Load<TextAsset>("AppGallery/PrivacyPolicyUrl");
    if(legacy&&IsHttps(legacy.text.Trim()))return legacy.text.Trim();
   }
   return value;
  }
  public static bool IsHttps(string value)=>Uri.TryCreate(value,UriKind.Absolute,out var uri)&&
   uri.Scheme==Uri.UriSchemeHttps&&uri.UserInfo.Length==0&&!string.IsNullOrEmpty(uri.Host);
  public static string BuildUrl(string root,string id,string language){
   if(id!="website"&&id!="terms"&&id!="privacy"&&id!="support")return "";
   root=(root??"").Trim();
   if(!IsHttps(root)||!Uri.TryCreate(root,UriKind.Absolute,out var uri)||uri.Query.Length>0||uri.Fragment.Length>0)return "";
   string locale=language=="zh-Hans"?"zh/":language=="ms"?"ms/":"";
   return new Uri(new Uri(root.TrimEnd('/')+"/"),locale+(id=="website"?"index":id)+".html").AbsoluteUri;
  }
 }
}
