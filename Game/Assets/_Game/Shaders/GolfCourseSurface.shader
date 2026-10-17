Shader "WhatTheFish/GolfCourseSurface" {
 Properties { _BaseMap("Shared island texture",2D)="white" {} _Tint("Course tint",Color)=(1,1,1,1) }
 SubShader {
  Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent-5" "RenderType"="Transparent" }
  Blend SrcAlpha OneMinusSrcAlpha ZWrite Off Offset -1,-1
  Pass {
   Tags { "LightMode"="UniversalForward" }
   HLSLPROGRAM
   #pragma vertex Vert
   #pragma fragment Frag
   #pragma multi_compile_instancing
   #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
   #pragma multi_compile_fragment _ _SHADOWS_SOFT
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
   TEXTURE2D(_BaseMap);SAMPLER(sampler_BaseMap);
   CBUFFER_START(UnityPerMaterial)
    half4 _Tint;
   CBUFFER_END
   struct A {float4 p:POSITION;float3 n:NORMAL;float2 uv:TEXCOORD0;half4 c:COLOR;UNITY_VERTEX_INPUT_INSTANCE_ID};
   struct V {float4 p:SV_POSITION;half3 n:TEXCOORD0;float2 uv:TEXCOORD1;half a:TEXCOORD2;float3 world:TEXCOORD3;};
   V Vert(A input){UNITY_SETUP_INSTANCE_ID(input);V o;o.world=TransformObjectToWorld(input.p.xyz);o.p=TransformWorldToHClip(o.world);o.n=TransformObjectToWorldNormal(input.n);o.uv=input.uv;o.a=input.c.a;return o;}
   half4 Frag(V input):SV_Target{Light sun=GetMainLight(TransformWorldToShadowCoord(input.world));half3 lit=SampleSH(input.n)+sun.color*(.2+.8*saturate(dot(normalize(input.n),sun.direction)))*sun.shadowAttenuation;return half4(SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,input.uv).rgb*_Tint.rgb*lit,input.a*_Tint.a);}
   ENDHLSL
  }
 }
}
