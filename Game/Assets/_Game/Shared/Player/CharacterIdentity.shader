Shader "WhatTheFish/CharacterIdentity" {
 Properties {
  _BaseMap("Color",2D)="white" {} _BumpMap("Normal",2D)="bump" {} _MetallicGlossMap("Metal and smoothness",2D)="white" {} _Numbers("Shared digits",2D)="black" {}
  _BaseColor("Base",Color)=(1,1,1,1) _JerseyColor("Uniform",Color)=(.1,.4,.8,1) _SkinTint("Skin",Color)=(1,.745,.54,1) _AccentColor("Accent",Color)=(.2,.6,.7,1)
  _HairColor("Brow and fringe",Color)=(.1,.1,.1,1)
  _BumpScale("Normal strength",Float)=.6 _Metallic("Metallic",Float)=1 _Smoothness("Smoothness",Float)=1
  _IdentityNumber("Number",Float)=1 _ShowOriginalHair("Original hair",Float)=1 _Team("Team",Float)=0
 }
 SubShader {
  Tags {"RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry"}
  Pass {
   Name "ForwardLit" Tags {"LightMode"="UniversalForward"}
   HLSLPROGRAM
   #pragma multi_compile_instancing
   #pragma target 3.0
   #pragma vertex IdentityVertex
   #pragma fragment IdentityFragment
   #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
   #pragma multi_compile_fragment _ _SHADOWS_SOFT
   #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
   #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
   #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
   #pragma multi_compile_fog
   #include "CharacterIdentityInput.hlsl"
   ENDHLSL
  }
  Pass {
   Name "ShadowCaster" Tags {"LightMode"="ShadowCaster"} ZWrite On ColorMask 0
   HLSLPROGRAM
   #pragma multi_compile_instancing
   #pragma target 3.0
   #pragma vertex ShadowVertex
   #pragma fragment ShadowFragment
   #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW
   #include "CharacterIdentityInput.hlsl"
   float3 _LightDirection;float3 _LightPosition;
   IdentityVaryings ShadowVertex(IdentityAttributes v){IdentityVaryings o=IdentityVertex(v);float3 lightDirection=_LightDirection;
   #if defined(_CASTING_PUNCTUAL_LIGHT_SHADOW)
    lightDirection=normalize(_LightPosition-o.positionWS);
   #endif
    o.positionCS=TransformWorldToHClip(ApplyShadowBias(o.positionWS,o.normalWS,lightDirection));
   #if UNITY_REVERSED_Z
    o.positionCS.z=min(o.positionCS.z,UNITY_NEAR_CLIP_VALUE);
   #else
    o.positionCS.z=max(o.positionCS.z,UNITY_NEAR_CLIP_VALUE);
   #endif
    return o;}
   half4 ShadowFragment(IdentityVaryings i):SV_Target{IdentityClip(i.regions.g,i.uv,i.number.w);return 0;}
   ENDHLSL
  }
  Pass {
   Name "DepthOnly" Tags {"LightMode"="DepthOnly"} ZWrite On ColorMask R
   HLSLPROGRAM
   #pragma multi_compile_instancing
   #pragma vertex IdentityVertex
   #pragma fragment DepthFragment
   #include "CharacterIdentityInput.hlsl"
   half4 DepthFragment(IdentityVaryings i):SV_Target{IdentityClip(i.regions.g,i.uv,i.number.w);return i.positionCS.z;}
   ENDHLSL
  }
  Pass {
   Name "DepthNormals" Tags {"LightMode"="DepthNormals"} ZWrite On
   HLSLPROGRAM
   #pragma multi_compile_instancing
   #pragma vertex IdentityVertex
   #pragma fragment NormalFragment
   #include "CharacterIdentityInput.hlsl"
   half4 NormalFragment(IdentityVaryings i):SV_Target{IdentityClip(i.regions.g,i.uv,i.number.w);return half4(normalize(i.normalWS),0);}
   ENDHLSL
  }
 }
}
