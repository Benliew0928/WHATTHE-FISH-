Shader "WhatTheFish/StadiumFireworks" {
 Properties { }
 SubShader {
  Tags { "Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" }
  Pass {
   Blend SrcAlpha OneMinusSrcAlpha
   ZWrite Off
   Cull Off
   HLSLPROGRAM
   #pragma vertex Vert
   #pragma fragment Frag
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
   struct Attributes {float4 positionOS:POSITION;float2 uv:TEXCOORD0;half4 color:COLOR;};
   struct Varyings {float4 positionCS:SV_POSITION;float2 uv:TEXCOORD0;half4 color:COLOR;};
   Varyings Vert(Attributes input){Varyings o;o.positionCS=TransformObjectToHClip(input.positionOS.xyz);o.uv=input.uv;o.color=input.color;return o;}
   // A broad saturated centre survives the bright sky. Only the edge feathers;
   // daylight visibility does not depend on white bloom or night lighting.
   half4 Frag(Varyings i):SV_Target {half edge=1-smoothstep(.58,1,abs(i.uv.x*2-1));half tip=saturate(i.uv.y*3);return half4(i.color.rgb,i.color.a*edge*tip);}
   ENDHLSL
  }
 }
}
