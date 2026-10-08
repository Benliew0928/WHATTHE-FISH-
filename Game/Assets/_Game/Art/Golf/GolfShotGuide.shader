Shader "WhatTheFish/GolfShotGuide" {
 SubShader {
  Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent+10" }
  Pass {
   Tags { "LightMode"="SRPDefaultUnlit" }
   Blend SrcAlpha OneMinusSrcAlpha
   ZWrite Off
   Cull Off
   HLSLPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
   struct Attributes {float4 positionOS:POSITION;float2 uv:TEXCOORD0;float2 style:TEXCOORD1;};
   struct Varyings {float4 positionCS:SV_POSITION;float2 uv:TEXCOORD0;float2 style:TEXCOORD1;};
   Varyings vert(Attributes v){Varyings o;o.positionCS=TransformObjectToHClip(v.positionOS.xyz);o.uv=v.uv;o.style=v.style;return o;}
   half4 frag(Varyings i):SV_Target {
    half3 mint=half3(.17,.95,.72),ivory=half3(1,1,.87),gold=half3(1,.79,.22);
    if(i.style.x>.5){
     float r=length(i.uv),aa=max(fwidth(r),.009),pulse=.5+.5*sin(_Time.y*3.2);
     float ring=1-smoothstep(.027,.027+aa,abs(r-.49));
     float halo=exp2(-abs(r-.49)*14)*(.5+.17*pulse);
     float centre=exp2(-r*r*36)*.67;
     float ticks=(1-smoothstep(.025,.025+aa,min(abs(i.uv.x),abs(i.uv.y))))*(1-smoothstep(.12,.12+aa,abs(r-.68)));
     float alpha=saturate(ring+halo+centre+ticks*.8)*(1-smoothstep(.83,1,r));
     return half4(lerp(gold,ivory,saturate(ring+ticks*.7+centre)),alpha);
    }
    // Soft energy rails echo the ball-sport guides in a restrained golf palette.
    float x=abs(i.uv.x),aa=max(fwidth(x),.015);
    float core=1-smoothstep(.13,.13+aa,x),rail=exp2(-abs(x-.40)*36)*.45;
    float glow=exp2(-x*x*7)*.42;
    float flow=.86+.14*cos(i.uv.y*2.4-_Time.y*3);
    float alpha=saturate(core+rail+glow)*flow*(1-smoothstep(.80,1,x));
    return half4(lerp(mint,ivory,core*.9),alpha);
   }
   ENDHLSL
  }
 }
}
