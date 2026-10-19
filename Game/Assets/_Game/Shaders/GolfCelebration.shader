Shader "WhatTheFish/GolfCelebration" {
 SubShader {
  Tags {"RenderPipeline"="UniversalPipeline" "Queue"="Transparent+20" "RenderType"="Transparent"}
  Pass {
   Tags {"LightMode"="SRPDefaultUnlit"}
   Blend SrcAlpha OneMinusSrcAlpha
   ZWrite Off
   Cull Off
   HLSLPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
   struct A {float4 positionOS:POSITION;half4 color:COLOR;float3 uv:TEXCOORD0;};
   struct V {float4 positionCS:SV_POSITION;half4 color:COLOR;float3 uv:TEXCOORD0;};
   V vert(A a){V o;o.positionCS=TransformObjectToHClip(a.positionOS.xyz);o.color=a.color;o.uv=a.uv;return o;}
   half4 frag(V i):SV_Target {half alpha=i.uv.z>1.5?pow(saturate(1-abs(i.uv.x*2-1)),2):i.uv.z>.5?pow(saturate(1-length(i.uv.xy*2-1)),2):1;return half4(i.color.rgb,i.color.a*alpha);}
   ENDHLSL
  }
 }
}
