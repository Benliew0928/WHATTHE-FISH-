using System;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace WhatTheFish {
 public struct GolfMatchSnapshot:INetworkSerializable,IEquatable<GolfMatchSnapshot> {
  public GolfMatchPhase phase;public uint round;public double started,deadline;public ulong firstFinisher;
  public void NetworkSerialize<T>(BufferSerializer<T> s) where T:IReaderWriter {s.SerializeValue(ref phase);s.SerializeValue(ref round);s.SerializeValue(ref started);s.SerializeValue(ref deadline);s.SerializeValue(ref firstFinisher);}
  public bool Equals(GolfMatchSnapshot v)=>phase==v.phase&&round==v.round&&started==v.started&&deadline==v.deadline&&firstFinisher==v.firstFinisher;
 }
 public struct GolfPlayerSnapshot:INetworkSerializable,IEquatable<GolfPlayerSnapshot> {
  public bool valid,finished,dnf;public uint round;public int completed,holeStroke,totalStroke;public double finishTime;public FixedString64Bytes name;
  public void NetworkSerialize<T>(BufferSerializer<T> s) where T:IReaderWriter {s.SerializeValue(ref valid);s.SerializeValue(ref round);s.SerializeValue(ref name);s.SerializeValue(ref completed);s.SerializeValue(ref holeStroke);s.SerializeValue(ref totalStroke);s.SerializeValue(ref finishTime);s.SerializeValue(ref finished);s.SerializeValue(ref dnf);}
  public bool Equals(GolfPlayerSnapshot v)=>valid==v.valid&&round==v.round&&name==v.name&&completed==v.completed&&holeStroke==v.holeStroke&&totalStroke==v.totalStroke&&finishTime==v.finishTime&&finished==v.finished&&dnf==v.dnf;
  public static GolfPlayerSnapshot Capture(GolfPlayerState p,uint round)=>p==null?default:new GolfPlayerSnapshot{valid=true,round=round,name=p.PlayerName,completed=p.CompletedHoleCount,holeStroke=p.CurrentHoleStroke,totalStroke=p.TotalStroke,finishTime=p.FinishTime,finished=p.IsFinished,dnf=p.IsDNF};
 }
 public struct GolfBallSnapshot:INetworkSerializable,IEquatable<GolfBallSnapshot> {
  public bool valid,active;public uint round,reset;public Vector3 position;public Quaternion rotation;public double time;
  public void NetworkSerialize<T>(BufferSerializer<T> s) where T:IReaderWriter {s.SerializeValue(ref valid);s.SerializeValue(ref active);s.SerializeValue(ref round);s.SerializeValue(ref reset);s.SerializeValue(ref position);s.SerializeValue(ref rotation);s.SerializeValue(ref time);}
  public bool Equals(GolfBallSnapshot v)=>valid==v.valid&&active==v.active&&round==v.round&&reset==v.reset&&position==v.position&&rotation==v.rotation&&time==v.time;
 }
 public struct GolfPlayerRecord:INetworkSerializable,IEquatable<GolfPlayerRecord> {
  public ulong owner;public GolfPlayerSnapshot progress;
  public void NetworkSerialize<T>(BufferSerializer<T> s) where T:IReaderWriter {s.SerializeValue(ref owner);s.SerializeValue(ref progress);}
  public bool Equals(GolfPlayerRecord v)=>owner==v.owner&&progress.Equals(v.progress);
 }
}
