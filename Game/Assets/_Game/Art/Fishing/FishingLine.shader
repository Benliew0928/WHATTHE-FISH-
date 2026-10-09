Shader "WhatTheFish/FishingLine" {
 Properties { _BaseColor ("Line tint", Color) = (1,1,1,1) }
 SubShader {
  Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent+10" }
  Pass {
   Tags { "LightMode"="SRPDefaultUnlit" }
   Blend SrcAlpha OneMinusSrcAlpha
   ZWrite Off
   Cull Off
   HLSLPROGRAM
   #pragma vertex Vert
   #pragma fragment Frag
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
   CBUFFER_START(UnityPerMaterial)
    half4 _BaseColor;
   CBUFFER_END
   struct Attributes { float4 positionOS : POSITION; half4 color : COLOR; };
   struct Varyings { float4 positionCS : SV_POSITION; half4 color : COLOR; };
   Varyings Vert(Attributes v) { Varyings o; o.positionCS=TransformObjectToHClip(v.positionOS.xyz); o.color=v.color*_BaseColor; return o; }
   half4 Frag(Varyings v) : SV_Target { return v.color; }
   ENDHLSL
  }
 }
}
