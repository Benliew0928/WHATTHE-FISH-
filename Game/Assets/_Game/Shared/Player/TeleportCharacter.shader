Shader "WhatTheFish/TeleportCharacter" {
 Properties {
  _BaseMap("Color",2D)="white" {} _BumpMap("Normal",2D)="bump" {} _MetallicGlossMap("Metal",2D)="white" {} _Numbers("Digits",2D)="black" {}
  _BaseColor("Base",Color)=(1,1,1,1) _JerseyColor("Uniform",Color)=(.1,.4,.8,1) _SkinTint("Skin",Color)=(1,.745,.54,1) _AccentColor("Accent",Color)=(.2,.6,.7,1) _HairColor("Hair",Color)=(.1,.1,.1,1)
  _BumpScale("Normal strength",Float)=.6 _Metallic("Metallic",Float)=0 _Smoothness("Smoothness",Float)=.3 _IdentityNumber("Number",Float)=1 _ShowOriginalHair("Original hair",Float)=1 _Team("Team",Float)=0
  _Identity("Identity material",Float)=0 _Head("Head material",Float)=0 _DetailColor("Accessory trim",Color)=(.85,.63,.28,1) _Amount("Absorption",Float)=0 _BaseY("Feet",Float)=0 _Height("Height",Float)=2 _Glow("Glow",Float)=0
 }
 SubShader {
  Tags {"RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry"}
  HLSLINCLUDE
  #include "CharacterIdentityInput.hlsl"
  float _Identity,_Head,_Amount,_BaseY,_Height,_Glow;half4 _DetailColor;
  float TeleportEdge(IdentityVaryings i){
   if(_Identity>.5)IdentityClip(i.regions.g,i.uv,i.number.w);
   float h=(i.positionWS.y-_BaseY)/max(.1,_Height);
   float noise=sin(i.positionWS.x*37+i.positionWS.z*23+sin(i.positionWS.y*48))* .025;
   float edge=h+noise-lerp(-.1,1.15,_Amount);clip(edge);
   return 1-saturate(edge/.09);
  }
  ENDHLSL
  Pass {
   Name "ForwardLit" Tags {"LightMode"="UniversalForward"}
   HLSLPROGRAM
   #pragma target 3.0
   #pragma vertex IdentityVertex
   #pragma fragment TeleportFragment
   #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
   #pragma multi_compile_fragment _ _SHADOWS_SOFT
   #pragma multi_compile_fog
   half4 TeleportFragment(IdentityVaryings i):SV_Target {
    float edge=TeleportEdge(i);half4 color;
    if(_Identity>.5)color=IdentityFragment(i);
    else {half3 base=_Head>.5?lerp(_BaseColor.rgb,_DetailColor.rgb,i.regions.a)*i.regions.rgb:SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,i.uv).rgb*_BaseColor.rgb;Light sun=GetMainLight(TransformWorldToShadowCoord(i.positionWS));float diffuse=saturate(dot(normalize(i.normalWS),sun.direction));color=half4(base*(SampleSH(normalize(i.normalWS))+sun.color*diffuse*sun.shadowAttenuation),1);}
    float rim=pow(1-saturate(abs(dot(normalize(i.normalWS),GetWorldSpaceNormalizeViewDir(i.positionWS)))),2);
    color.rgb=lerp(color.rgb,half3(.08,.55,1),_Glow*.22)+half3(.12,.8,1.5)*(edge*1.6+rim*_Glow*.7);return color;
   }
   ENDHLSL
  }
  Pass {
   Name "DepthOnly" Tags {"LightMode"="DepthOnly"} ZWrite On ColorMask R
   HLSLPROGRAM
   #pragma vertex IdentityVertex
   #pragma fragment Depth
   half4 Depth(IdentityVaryings i):SV_Target {TeleportEdge(i);return i.positionCS.z;}
   ENDHLSL
  }
  Pass {
   Name "ShadowCaster" Tags {"LightMode"="ShadowCaster"} ZWrite On ColorMask 0
   HLSLPROGRAM
   #pragma vertex IdentityVertex
   #pragma fragment Shadow
   half4 Shadow(IdentityVaryings i):SV_Target {TeleportEdge(i);return 0;}
   ENDHLSL
  }
  Pass {
   Name "DepthNormals" Tags {"LightMode"="DepthNormals"} ZWrite On
   HLSLPROGRAM
   #pragma vertex IdentityVertex
   #pragma fragment Normals
   half4 Normals(IdentityVaryings i):SV_Target {TeleportEdge(i);return half4(normalize(i.normalWS),0);}
   ENDHLSL
  }
 }
}
