using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.Scripting;

namespace WhatTheFish {
 // Local device preferences, partitioned by authenticated identity (or guest) and sport.
 public static class ControlLayouts {
  [Serializable,Preserve] public sealed class Entry {
   public string id;public float x,y,size=1;
   public Entry Copy()=>new(){id=id,x=x,y=y,size=size};
  }
  [Serializable,Preserve] sealed class Document {public int version=1;public List<Entry> controls=new();}
  public const float MinSize=.7f,MaxSize=1.6f;
  public static string Key(string profile,SportId sport){
   using var sha=SHA256.Create();
   return "controls.v1."+BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(profile))).Replace("-","").ToLowerInvariant()+"."+sport;
  }
  static bool Finite(float value)=>!float.IsNaN(value)&&!float.IsInfinity(value);
  public static Dictionary<string,Entry> Load(string profile,SportId sport){
   var result=new Dictionary<string,Entry>();string json=PlayerPrefs.GetString(Key(profile,sport),"");
   if(json.Length==0||json.Length>32768)return result;
   try{
    var doc=JsonUtility.FromJson<Document>(json);if(doc==null||doc.version!=1||doc.controls==null)return result;
    foreach(var e in doc.controls.Take(64)){
     if(e==null||string.IsNullOrEmpty(e.id)||e.id.Length>48||!Finite(e.x)||!Finite(e.y)||!Finite(e.size))continue;
     result[e.id]=new Entry{id=e.id,x=Mathf.Clamp01(e.x),y=Mathf.Clamp01(e.y),size=Mathf.Clamp(e.size,MinSize,MaxSize)};
    }
   }catch(ArgumentException){ }
   return result;
  }
  public static void Save(string profile,SportId sport,IEnumerable<Entry> values){
   var doc=new Document{controls=values.Select(e=>e.Copy()).OrderBy(e=>e.id).ToList()};
   PlayerPrefs.SetString(Key(profile,sport),JsonUtility.ToJson(doc));PlayerPrefs.Save();
  }
  public static void IdentityChanging(string before,string after,bool link){
   if(!link||before==after)return;
   foreach(SportId sport in Enum.GetValues(typeof(SportId))){
    string key=Key(after,sport),old=Key(before,sport);
    if(!PlayerPrefs.HasKey(key)&&PlayerPrefs.HasKey(old))PlayerPrefs.SetString(key,PlayerPrefs.GetString(old));
   }
   PlayerPrefs.Save();
  }
 }
}
