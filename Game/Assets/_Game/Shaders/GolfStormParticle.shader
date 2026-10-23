Shader "WhatTheFish/GolfStormParticle" {
 Properties {_BaseMap("Vapor",2D)="white"{}}
 SubShader {
  Tags {"RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent+9"}
  Pass {
   Tags {"LightMode"="SRPDefaultUnlit"}
   Blend SrcAlpha OneMinusSrcAlpha
   ZWrite Off
   Cull Off
   HLSLPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
   TEXTURE2D(_BaseMap);SAMPLER(sampler_BaseMap);
   struct A {float4 positionOS:POSITION;float2 uv:TEXCOORD0;half4 color:COLOR;};
   struct V {float4 positionCS:SV_POSITION;float2 uv:TEXCOORD0;half4 color:COLOR;};
   V vert(A i){V o;o.positionCS=TransformObjectToHClip(i.positionOS.xyz);o.uv=i.uv;o.color=i.color;return o;}
   half4 frag(V i):SV_Target{return SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,i.uv)*i.color;}
   ENDHLSL
  }
 }
}
