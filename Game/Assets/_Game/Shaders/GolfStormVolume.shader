Shader "WhatTheFish/GolfStormVolume" {
 Properties { _Phase("Phase",Float)=0 }
 SubShader {
  Tags {"RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent+8"}
  Pass {
   Tags {"LightMode"="SRPDefaultUnlit"}
   Blend SrcAlpha OneMinusSrcAlpha
   ZWrite Off
   Cull Off
   HLSLPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
   CBUFFER_START(UnityPerMaterial)
   float _Phase;
   CBUFFER_END
   struct A {float4 positionOS:POSITION;float2 uv:TEXCOORD0;half4 color:COLOR;};
   struct V {float4 positionCS:SV_POSITION;float2 uv:TEXCOORD0;half4 color:COLOR;};
   V vert(A i){V o;o.positionCS=TransformObjectToHClip(i.positionOS.xyz);o.uv=i.uv;o.color=i.color;return o;}
   float hash(float2 p){return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453);}
   float noise(float2 p){float2 i=floor(p),f=frac(p);f=f*f*(3-2*f);return lerp(lerp(hash(i),hash(i+float2(1,0)),f.x),lerp(hash(i+float2(0,1)),hash(i+1),f.x),f.y);}
   half4 frag(V i):SV_Target {
    float2 p=float2(i.uv.x*9+i.uv.y*5-_Phase*.9,i.uv.y*11+_Phase*1.1);
    float n=noise(p)*.57+noise(p*2.03)*.28+noise(p*4.07)*.15;
    float folds=.5+.5*sin(i.uv.x*18.8496+i.uv.y*14-_Phase*2);
    float lighting=.68+.26*cos(i.uv.x*6.2832-.7)+.25*n;
    float edge=smoothstep(0,.035,i.uv.y)*(1-smoothstep(.86,1,i.uv.y));
    return half4(i.color.rgb*lighting,(.35+.55*n+.1*folds)*i.color.a*edge);
   }
   ENDHLSL
  }
 }
}
