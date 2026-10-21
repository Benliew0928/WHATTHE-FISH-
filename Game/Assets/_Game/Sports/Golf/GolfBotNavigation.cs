using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WhatTheFish {
 // A small runtime surface search avoids a baked navigation dependency on the
 // unfinished props. Terrain, water and solid obstacles remain authoritative.
 public sealed class GolfBotNavigation {
  const float Cell=2;
  readonly Dictionary<Vector2Int,Vector3> ground=new();
  readonly HashSet<Vector2Int> blocked=new();
  readonly List<Vector3> path=new();
  int step;bool searching;Vector3 target;float retry;
  public int Searches {get;private set;}
  public string Status=>$"search={searching} paths={Searches} step={step}/{path.Count}";
  public void Clear(){path.Clear();ground.Clear();blocked.Clear();step=0;searching=false;retry=0;}
  public Vector3 Direction(MonoBehaviour runner,Athlete actor,GolfCourse course,Vector3 destination){
   if((destination-target).sqrMagnitude>4){path.Clear();step=0;}
   target=destination;
   if(!searching&&path.Count==0&&Time.time>=retry){searching=true;runner.StartCoroutine(Search(actor.transform.position,destination,course));}
   while(step<path.Count&&Flat(path[step]-actor.transform.position).magnitude<.65f)step++;
   if(step>=path.Count){path.Clear();step=0;return Vector3.zero;}
   return Flat(path[step]-actor.transform.position);
  }
  static Vector3 Flat(Vector3 p)=>new(p.x,0,p.z);
  static Vector2Int Key(Vector3 p)=>new(Mathf.RoundToInt(p.x/Cell),Mathf.RoundToInt(p.z/Cell));
  bool Ground(Vector2Int key,GolfCourse course,out Vector3 point){
   if(ground.TryGetValue(key,out point))return true;if(blocked.Contains(key))return false;
   var p=new Vector3(key.x*Cell,course.transform.position.y+100,key.y*Cell);
   if(Physics.Raycast(p,Vector3.down,out var hit,180,1<<8,QueryTriggerInteraction.Ignore)&&hit.normal.y>.72f&&course.SafeDryPosition(hit.point,.65f)){
    point=hit.point+Vector3.up*.04f;
    if(!Physics.CheckCapsule(point+Vector3.up*.6f,point+Vector3.up*1.35f,.36f,(1<<8)|(1<<9),QueryTriggerInteraction.Ignore)){ground[key]=point;return true;}
   }
   blocked.Add(key);return false;
  }
  bool ClearLine(Vector3 a,Vector3 b,GolfCourse course){
   var travel=b-a;
   if(travel.sqrMagnitude>.001f&&Physics.SphereCast(a+Vector3.up*.9f,.32f,travel.normalized,out _,travel.magnitude,(1<<8)|(1<<9),QueryTriggerInteraction.Ignore))return false;
   int steps=Mathf.CeilToInt(Flat(b-a).magnitude);
   for(int i=1;i<=steps;i++){
    var p=Vector3.Lerp(a,b,(float)i/steps);
    if(!course.SafeDryPosition(p,.65f)||!Physics.Raycast(p+Vector3.up*1.5f,Vector3.down,out var hit,3,1<<8,QueryTriggerInteraction.Ignore)||hit.normal.y<.72f)return false;
   }
   return true;
  }
  IEnumerator Search(Vector3 start,Vector3 end,GolfCourse course){
   Searches++;path.Clear();step=0;var goal=Key(end);var first=Key(start);
   var costs=new Dictionary<Vector2Int,float>{{first,0}};var parents=new Dictionary<Vector2Int,Vector2Int>();
   var open=new SortedSet<(float score,int serial,Vector2Int key)>(Comparer<(float score,int serial,Vector2Int key)>.Create((a,b)=>{int c=a.score.CompareTo(b.score);return c!=0?c:a.serial.CompareTo(b.serial);}));
   int serial=0;open.Add((0,serial++,first));var closed=new HashSet<Vector2Int>();
   Vector2Int found=first;bool success=false;int count=0,batch=Application.isMobilePlatform?30:120;
   ground[first]=start;
   while(open.Count>0&&count++<18000){
    var entry=open.Min;open.Remove(entry);var current=entry.key;if(!closed.Add(current))continue;
    var p=ground[current];
    if(Flat(p-end).magnitude<2.1f&&ClearLine(p,end,course)){found=current;success=true;break;}
    for(int x=-1;x<=1;x++)for(int y=-1;y<=1;y++){
     if(x==0&&y==0)continue;var next=current+new Vector2Int(x,y);
     if(closed.Contains(next)||!Ground(next,course,out var q)||Mathf.Abs(p.y-q.y)>1.4f||!ClearLine(p,q,course))continue;
     float cost=costs[current]+Vector3.Distance(p,q);
     if(costs.TryGetValue(next,out float old)&&cost>=old)continue;
     costs[next]=cost;parents[next]=current;open.Add((cost+Vector2Int.Distance(next,goal)*Cell,serial++,next));
    }
    if(count%batch==0)yield return null;
   }
   if(success){
    var reverse=new List<Vector3>{end};while(found!=first){reverse.Add(ground[found]);found=parents[found];}reverse.Reverse();
    var previous=start;
    for(int i=0;i<reverse.Count;){int last=i;while(last+1<reverse.Count&&last-i<12&&ClearLine(previous,reverse[last+1],course))last++;path.Add(reverse[last]);previous=reverse[last];i=last+1;}
   }
   searching=false;retry=Time.time+1;
  }
 }
}
