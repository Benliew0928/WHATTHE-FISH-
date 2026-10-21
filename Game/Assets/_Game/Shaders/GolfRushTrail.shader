Shader "WhatTheFish/GolfRushTrail" {
 SubShader { Tags {"RenderPipeline"="UniversalPipeline" "Queue"="Transparent+5" "RenderType"="Transparent"} Pass {
  Blend SrcAlpha OneMinusSrcAlpha ZWrite Off Cull Off
  HLSLPROGRAM
  #pragma vertex vert
  #pragma fragment frag
  #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
  float4 _RunnerPlane;
  struct A {float4 positionOS:POSITION;float3 uv:TEXCOORD0;half4 color:COLOR;};
  struct V {float4 positionCS:SV_POSITION;float3 world:TEXCOORD1;float3 uv:TEXCOORD0;half4 color:COLOR;};
  V vert(A i){V o;o.world=TransformObjectToWorld(i.positionOS.xyz);o.positionCS=TransformWorldToHClip(o.world);o.uv=i.uv;o.color=i.color;return o;}
  half4 frag(V i):SV_Target {
   clip(-dot(float4(i.world,1),_RunnerPlane));float2 p=i.uv.xy*2-1;half4 c=i.color;
   if(i.uv.z<.5){float d=abs(p.y);c.rgb=lerp(c.rgb,half3(1,1,.97),pow(saturate(1-d),3));c.a*=smoothstep(1,.25,d);}
   else if(i.uv.z<1.5){float shape=p.y*p.y+pow(abs(p.x),.7);float inside=1-smoothstep(.85,1,shape);float vein=1-smoothstep(.015,.065,abs(p.x+p.y*.25));c.rgb=lerp(c.rgb,half3(1,1,.7),saturate(vein+smoothstep(.83,.94,shape)));c.a*=inside;}
   else {float d=min(abs(p.x)*3+abs(p.y),abs(p.x)+abs(p.y)*3);c.rgb=lerp(c.rgb,half3(1,1,1),1-saturate(d));c.a*=1-smoothstep(.8,1,d);}
   return c;
  }
  ENDHLSL
 } }
}
