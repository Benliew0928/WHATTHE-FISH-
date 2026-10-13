Shader "WhatTheFish/GolfBallTrail" {
 Properties { _Fade("Fade",Range(0,1))=1 }
 SubShader {
  Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent+12" }
  Pass {
   Tags { "LightMode"="SRPDefaultUnlit" }
   Blend SrcAlpha OneMinusSrcAlpha
   ZWrite Off
   Cull Off
   HLSLPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
   CBUFFER_START(UnityPerMaterial)
   float _Fade;
   CBUFFER_END
   struct Attributes {float4 positionOS:POSITION;float2 uv:TEXCOORD0;half4 color:COLOR;};
   struct Varyings {float4 positionCS:SV_POSITION;float2 uv:TEXCOORD0;half4 color:COLOR;};
   Varyings vert(Attributes v){Varyings o;o.positionCS=TransformObjectToHClip(v.positionOS.xyz);o.uv=v.uv;o.color=v.color;return o;}
   half4 frag(Varyings i):SV_Target {
    float distance=abs(i.uv.y-.5)*2;
    float core=1-smoothstep(.10,.22,distance);
    float glow=exp2(-distance*distance*6)*(1-smoothstep(.8,1,distance));
    half3 color=lerp(i.color.rgb,half3(1,1,.90),core*.92);
    return half4(color,saturate(core+glow*.42)*i.color.a*_Fade);
   }
   ENDHLSL
  }
 }
}
