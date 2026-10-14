using System;
using System.IO;
using WhatTheFish;
public static class RoomMatchBuild {
 public static void CheckRules(){
  string folder="../Builds/RoomMatchQA";Directory.CreateDirectory(folder);int count=0;
  using var writer=new StreamWriter(Path.Combine(folder,"pure-rules.txt"));
  RoomMatchRuleChecks.Run((ok,name)=>{writer.WriteLine((ok?"PASS ":"FAIL ")+name);writer.Flush();if(!ok)throw new Exception(name);count++;});
  writer.WriteLine("RULES_COMPLETE checks="+count);
 }
 public static void BuildReview(){CheckRules();ProjectBuilder.BuildWindowsPlayer("../Builds/RoomMatchQA/Player/WhatTheFish.exe");}
}
