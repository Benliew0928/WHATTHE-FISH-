using UnityEngine;
using UnityEngine.UI;
namespace WhatTheFish {
 public sealed partial class FootballHUD {
  RectTransform audioPage;Button audioNav;readonly Slider[] audioSliders=new Slider[4];readonly Text[] audioLabels=new Text[4];
  static readonly string[] AudioNames={"Master","Pitch sounds","Buttons","Ambience"};
  void BuildAudio(float left,float width){
   audioNav=FootballUIStyle.Button(settingsPanel,"Audio navigation","Sound",20,198,left-54,53,()=>SelectMenu(2),BasketballUIStyle.Paper,16);
   audioPage=CoveUI.Rect(settingsPanel,"Sound page",left,82,width,230);
   BasketballUIStyle.Text(audioPage,"Sound",0,0,width,28,18,true);
   for(int i=0;i<4;i++){
    int index=i;float y=40+i*45;
    audioLabels[i]=BasketballUIStyle.Text(audioPage,AudioNames[i],0,y,150,32,12,true);
    var track=BasketballUIStyle.Plate(audioPage,AudioNames[i]+" volume",158,y,width-160,32,CoveUI.Sky,16,1);track.raycastTarget=true;
    var slider=track.gameObject.AddComponent<Slider>();audioSliders[i]=slider;slider.direction=Slider.Direction.LeftToRight;
    var area=CoveUI.Rect(track.transform,"Handle area",16,16,width-192,0);var handle=BasketballUIStyle.Plate(area,"Handle",0,0,30,30,CoveUI.Gold,15,1.5f);handle.raycastTarget=true;handle.rectTransform.pivot=Vector2.one*.5f;slider.handleRect=handle.rectTransform;slider.targetGraphic=handle;
    slider.navigation=new Navigation{mode=Navigation.Mode.Automatic};slider.onValueChanged.AddListener(value=>{if(index==0)MenuPreferences.SetMaster(value);else if(index==1)MenuPreferences.SetSFX(value);else if(index==2)MenuPreferences.SetUI(value);else MenuPreferences.SetAmbience(value);audioLabels[index].text=AudioNames[index]+" "+Mathf.RoundToInt(value*100)+"%";});
   }
   RefreshAudio();
  }
  void RefreshAudio(){for(int i=0;i<4;i++)if(audioSliders[i]){float v=i==0?MenuPreferences.MasterVolume:i==1?MenuPreferences.SFXVolume:i==2?MenuPreferences.UIVolume:MenuPreferences.AmbienceVolume;audioSliders[i].SetValueWithoutNotify(v);audioLabels[i].text=AudioNames[i]+" "+Mathf.RoundToInt(v*100)+"%";}}
 }
}
