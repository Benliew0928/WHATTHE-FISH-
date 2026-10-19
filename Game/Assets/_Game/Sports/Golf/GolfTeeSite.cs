using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WhatTheFish {
 [Serializable] public struct GolfTeeSite {
  public int hole,count;public Vector3 cup,centre;public float radius,spacing,height;
  public float Angle=>Mathf.Atan2(centre.z-cup.z,centre.x-cup.x);
  public int Offset(int slot)=>slot==0?0:(slot%2==1?-(slot+1)/2:slot/2);
  public Vector3 Pad(int slot){float a=Angle+Offset(slot)*spacing/radius;return new Vector3(cup.x+radius*Mathf.Cos(a),height,cup.z+radius*Mathf.Sin(a));}
  public Vector3 Waiting(int index){float a=Angle+(index%5-2)*2.5f/(radius+12);return new Vector3(cup.x+(radius+12+index/5*3)*Mathf.Cos(a),height,cup.z+(radius+12+index/5*3)*Mathf.Sin(a));}
  public float AngleDelta(float x,float z)=>Mathf.DeltaAngle(Angle*Mathf.Rad2Deg,Mathf.Atan2(z-cup.z,x-cup.x)*Mathf.Rad2Deg)*Mathf.Deg2Rad;
  public float HalfWidth=>hole==1?.275f:6.5f/radius;
  public float Distance(float x,float z)=>Vector2.Distance(new Vector2(x,z),new Vector2(cup.x,cup.z));
  public float GroundWeight(float x,float z){float r=Distance(x,z);float outside=Mathf.Max(Mathf.Abs(r-(radius+.5f))-4.5f,(Mathf.Abs(AngleDelta(x,z))-HalfWidth)*radius);return 1-Mathf.SmoothStep(0,1,Mathf.Clamp01(outside/5));}
  public bool Clears(float x,float z)=>GroundWeight(x,z)>.01f||hole==1&&x>=-44&&x<=-10&&z>=-147&&z<=-137;
  // Local restoration footprint from the annotated T1 bunker reference.
  public static float RestoredBunker(float x,float z){float d=Mathf.Sqrt(Mathf.Pow((x+43)/23,2)+Mathf.Pow((z+112.5f)/18,2));return 1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(.94f,1.04f,d));}
  public bool Opening(float x,float z)=>hole==1&&RestoredBunker(x,z)<.5f&&Distance(x,z)>=6&&Distance(x,z)<=radius+5&&Mathf.Abs(AngleDelta(x,z))<HalfWidth;
  public int Surface(float x,float z){if(!Opening(x,z))return 0;float r=Distance(x,z);return r>=39&&r<=44?2:r>=23&&r<=27?3:1;}
 }
 // Authority-owned reservations. Randomness chooses players and equal-distance
 // sides; it never independently chooses an already occupied pad.
 public sealed class GolfTeeReservations {
  readonly Dictionary<(int,ulong),int> slots=new();readonly List<(int hole,ulong owner)> pending=new();
  readonly Dictionary<ulong,long> batches=new();
  readonly System.Random random;
  public GolfTeeReservations(int seed){random=new System.Random(seed);}
  public bool TryGet(int hole,ulong owner,out int slot)=>slots.TryGetValue((hole,owner),out slot);
  public bool Waiting(ulong owner)=>pending.Any(p=>p.owner==owner);
  public int QueueIndex(ulong owner)=>pending.FindIndex(p=>p.owner==owner);
  public void Restore(int hole,ulong owner,int slot){if(slots.Any(s=>s.Key.Item1==hole&&s.Value==slot))throw new InvalidOperationException("Duplicate restored golf tee");slots[(hole,owner)]=slot;}
  public void Request(int hole,ulong owner,long batch=0){if(slots.ContainsKey((hole,owner))||pending.Any(p=>p.owner==owner))return;var peers=pending.Select((p,i)=>(p,i)).Where(v=>v.p.hole==hole&&batches[v.p.owner]==batch).ToArray();int at=peers.Length==0?pending.Count:peers[0].i+random.Next(peers.Length+1);batches[owner]=batch;pending.Insert(at,(hole,owner));}
  public void RandomizeBatch(){for(int i=pending.Count-1;i>0;i--){int j=random.Next(i+1);(pending[i],pending[j])=(pending[j],pending[i]);}}
  public List<(int hole,ulong owner,int slot)> Assign(Func<int,int> capacity,Func<int,int,int> offset,Func<int,int,bool> clear){
   var result=new List<(int,ulong,int)>();
   foreach(var item in pending.ToArray()){
    var free=Enumerable.Range(0,capacity(item.hole)).Where(i=>!slots.Any(s=>s.Key.Item1==item.hole&&s.Value==i)&&clear(item.hole,i)).ToArray();
    if(free.Length==0)continue;int distance=free.Min(i=>Math.Abs(offset(item.hole,i)));var nearest=free.Where(i=>Math.Abs(offset(item.hole,i))==distance).ToArray();int slot=nearest[random.Next(nearest.Length)];
    slots[(item.hole,item.owner)]=slot;pending.Remove(item);batches.Remove(item.owner);result.Add((item.hole,item.owner,slot));
   }
   return result;
  }
  public (int hole,ulong owner,int slot)[] Occupied=>slots.Select(s=>(s.Key.Item1,s.Key.Item2,s.Value)).ToArray();
  public void Release(int hole,ulong owner)=>slots.Remove((hole,owner));
  public void Remove(ulong owner){foreach(var key in slots.Keys.Where(k=>k.Item2==owner).ToArray())slots.Remove(key);pending.RemoveAll(p=>p.owner==owner);batches.Remove(owner);}
 }
}
