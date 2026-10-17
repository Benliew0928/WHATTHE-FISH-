using System.Collections.Generic;

namespace WhatTheFish {
 // A lease belongs to an actor, never its team, roster position or bot owner.
 public sealed class IdentitySlots {
  readonly Dictionary<ulong,byte> leases=new();
  public byte Reserve(ulong actor,int preferred=0){
   if(leases.TryGetValue(actor,out var existing))return existing;
   for(int offset=0;offset<10;offset++){
    byte number=(byte)((System.Math.Clamp(preferred,0,9)+offset)%10+1);
    if(!leases.ContainsValue(number)){leases.Add(actor,number);return number;}
   }
   return 0;
  }
  public void Release(ulong actor)=>leases.Remove(actor);
  // The first owner profile may arrive after spawn. Honor it only if free;
  // never evict or renumber another participant to satisfy a preference.
  public byte Prefer(ulong actor,int preferred){
   byte desired=(byte)(System.Math.Clamp(preferred,0,9)+1);
   if(!leases.ContainsValue(desired))leases[actor]=desired;
   return Reserve(actor,preferred);
  }
  public void Clear()=>leases.Clear();
  public int Count=>leases.Count;
 }
}
