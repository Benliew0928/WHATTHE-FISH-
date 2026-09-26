Shader "WhatTheFish/RefinedFoliage" {
 Properties {
  _BaseMap("Albedo",2D)="white"{} _BaseColor("Tint",Color)=(1,1,1,1)
  _BumpMap("Normal",2D)="bump"{} _BumpScale("Normal strength",Float)=.4
  _MetallicGlossMap("Metallic and smoothness",2D)="white"{} _Metallic("Metallic",Float)=0 _Smoothness("Smoothness scale",Float)=1
  _OcclusionMap("Occlusion",2D)="white"{} _OcclusionStrength("Occlusion strength",Float)=1
  _EmissionMap("Emission",2D)="black"{} _EmissionColor("Emission tint",Color)=(0,0,0,0)
  _SpecColor("Specular",Color)=(.2,.2,.2,1) _Cutoff("Cutoff",Float)=.5
  _Cull("Cull",Float)=0 _Surface("Surface",Float)=0 _AlphaClip("Clip",Float)=0
 }
 SubShader {Tags {"RenderPipeline"="UniversalPipeline" "RenderType"="Opaque"} Cull Off
 HLSLINCLUDE
 #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
 float3 IslandWind(float3 p){float3 world=TransformObjectToWorld(p);float gust=sin(world.x*.23+world.z*.17+_Time.y*1.1)*.6+sin(world.x*.79-world.z*.5+_Time.y*1.8)*.4;world.x+=gust*.045;world.z+=gust*.023;return TransformWorldToObject(world);}
 ENDHLSL
 Pass {Name "ForwardLit" Tags {"LightMode"="UniversalForward"}
 HLSLPROGRAM
 #pragma target 3.0
 #pragma vertex WindVertex
 #pragma fragment LitPassFragment
 #pragma shader_feature_local _NORMALMAP
 #pragma shader_feature_local_fragment _METALLICSPECGLOSSMAP
 #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
 #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
 #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
 #pragma multi_compile_fog
 #pragma multi_compile_instancing
 #include "Packages/com.unity.render-pipelines.universal/Shaders/LitForwardPass.hlsl"
 Varyings WindVertex(Attributes a){UNITY_SETUP_INSTANCE_ID(a);a.positionOS.xyz=IslandWind(a.positionOS.xyz);return LitPassVertex(a);}
 ENDHLSL }
 Pass {Name "ShadowCaster" Tags {"LightMode"="ShadowCaster"} ZWrite On ZTest LEqual ColorMask 0
 HLSLPROGRAM
 #pragma target 3.0
 #pragma vertex WindShadow
 #pragma fragment ShadowPassFragment
 #pragma multi_compile_instancing
 #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW
 #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"
 Varyings WindShadow(Attributes a){UNITY_SETUP_INSTANCE_ID(a);a.positionOS.xyz=IslandWind(a.positionOS.xyz);return ShadowPassVertex(a);}
 ENDHLSL }
 Pass {Name "DepthOnly" Tags {"LightMode"="DepthOnly"} ZWrite On ColorMask R
 HLSLPROGRAM
 #pragma target 3.0
 #pragma vertex WindDepth
 #pragma fragment DepthOnlyFragment
 #pragma multi_compile_instancing
 #include "Packages/com.unity.render-pipelines.universal/Shaders/DepthOnlyPass.hlsl"
 Varyings WindDepth(Attributes a){UNITY_SETUP_INSTANCE_ID(a);a.position.xyz=IslandWind(a.position.xyz);return DepthOnlyVertex(a);}
 ENDHLSL }
 Pass {Name "DepthNormals" Tags {"LightMode"="DepthNormals"} ZWrite On
 HLSLPROGRAM
 #pragma target 3.0
 #pragma vertex WindNormals
 #pragma fragment DepthNormalsFragment
 #pragma shader_feature_local _NORMALMAP
 #pragma multi_compile_instancing
 #include "Packages/com.unity.render-pipelines.universal/Shaders/LitDepthNormalsPass.hlsl"
 Varyings WindNormals(Attributes a){UNITY_SETUP_INSTANCE_ID(a);a.positionOS.xyz=IslandWind(a.positionOS.xyz);return DepthNormalsVertex(a);}
 ENDHLSL }
 }
}
