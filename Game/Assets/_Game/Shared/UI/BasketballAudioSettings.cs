using System;
using UnityEngine;
using UnityEngine.UI;

namespace WhatTheFish {
 public sealed partial class BasketballHUD {
  readonly Slider[] audioSliders=new Slider[4];readonly Text[] audioLabels=new Text[4];
  static readonly string[] AudioNames={"Master","Court sounds","Buttons","Ambience"};
  float AudioValue(int i)=>i==0?MenuPreferences.MasterVolume:i==1?MenuPreferences.SFXVolume:i==2?MenuPreferences.UIVolume:MenuPreferences.AmbienceVolume;
  void BuildAudioSliders(){
   for(int i=0;i<4;i++){
    int index=i;float x=20+i*155;
    audioLabels[i]=BasketballUIStyle.Text(settingsPanel,AudioNames[i],x,226,146,22,12,true);
    var track=BasketballUIStyle.Plate(settingsPanel,AudioNames[i]+" volume",x,254,143,34,CoveUI.Sky,17,1);track.raycastTarget=true;
    var slider=track.gameObject.AddComponent<Slider>();audioSliders[i]=slider;slider.direction=Slider.Direction.LeftToRight;
    var area=CoveUI.Rect(track.transform,"Handle area",16,0,111,34);var handle=BasketballUIStyle.Plate(area,"Handle",0,0,30,30,CoveUI.Gold,15,1.5f);handle.raycastTarget=true;handle.rectTransform.pivot=Vector2.one*.5f;slider.handleRect=handle.rectTransform;slider.targetGraphic=handle;
    slider.navigation=new Navigation{mode=Navigation.Mode.Automatic};slider.onValueChanged.AddListener(value=>{
     if(index==0)MenuPreferences.SetMaster(value);else if(index==1)MenuPreferences.SetSFX(value);else if(index==2)MenuPreferences.SetUI(value);else MenuPreferences.SetAmbience(value);
     audioLabels[index].text=AudioNames[index]+" "+Mathf.RoundToInt(value*100)+"%";
    });
   }RefreshAudioSliders();
  }
  void RefreshAudioSliders(){for(int i=0;i<4;i++)if(audioSliders[i]){float value=AudioValue(i);audioSliders[i].SetValueWithoutNotify(value);audioLabels[i].text=AudioNames[i]+" "+Mathf.RoundToInt(value*100)+"%";}}
 }
}
