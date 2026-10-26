using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace WhatTheFish {
 // A deliberately conservative, versioned vocabulary, shared with the service.
 // This detects listed terms, not intent. Never apply it to private report evidence.
 public static class TextSafety {
  [Serializable] sealed class Policy {public int version;public string[] latin,phrases;}
  static Regex[] patterns;
  public const string NameError="Choose a name without offensive or sexual wording.";
  public const string ChatError="Please remove offensive or sexual wording before sending.";
  public static bool Blocked(string value){
   if(string.IsNullOrWhiteSpace(value))return false;
   if(value.Length>4096)return true;
   if(patterns==null){var p=JsonUtility.FromJson<Policy>(Resources.Load<TextAsset>("TextSafety").text);patterns=p.latin.Select(t=>Pattern(t,true)).Concat(p.phrases.Select(t=>Pattern(t,false))).ToArray();}
   string plain=Normalize(value,false),substituted=Normalize(value,true);
   try{foreach(var pattern in patterns)if(pattern.IsMatch(plain)||pattern.IsMatch(substituted))return true;}catch(RegexMatchTimeoutException){return true;}
   return false;
  }
  static Regex Pattern(string term,bool boundary){
   // Separators inside a listed term catch spaced/dotted spellings. Letter
   // boundaries avoid matching innocent longer words (class, Scunthorpe, etc.).
   string body=string.Join("[^\\p{L}\\p{N}\\p{M}]*",term.Where(c=>c!=' ').Select(c=>Regex.Escape(c.ToString())+"+"));
   return new Regex((boundary?"(?<![\\p{L}\\p{M}])":"")+body+(boundary?"(?![\\p{L}\\p{M}])":""),RegexOptions.CultureInvariant,TimeSpan.FromMilliseconds(50));
  }
  static string Normalize(string value,bool leet){
   var result=new StringBuilder();
   foreach(char original in value.Normalize(NormalizationForm.FormKC).ToLowerInvariant()){
    if(char.GetUnicodeCategory(original)==UnicodeCategory.Format)continue;
    char c=original;if(leet)c=c switch{'0'=>'o','1'=>'i','3'=>'e','4'=>'a','5'=>'s','7'=>'t','@'=>'a','$'=>'s',_=>c};result.Append(c);
   }
   return result.ToString();
  }
 }
}
