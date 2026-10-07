Shader "WhatTheFish/BasketballPassCharge" {
 Properties {
  _Charge("Charge",Range(0,1))=0
  _Length("Receiving range",Float)=4
  _Opacity("Opacity",Range(0,1))=1
  _Release("Release pulse",Range(0,1))=0
  _Bend("Loft or bounce",Range(-1,1))=0
  [HideInInspector] _Surface("Surface",Float)=1
  [HideInInspector] _ZWrite("Depth write",Float)=0
 }
 SubShader {
  Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent" }
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
    float _Charge,_Length,_Opacity,_Release,_Surface,_ZWrite,_Bend;
   CBUFFER_END
   struct Attributes {float4 positionOS:POSITION;float2 uv:TEXCOORD0;};
   struct Varyings {float4 positionCS:SV_POSITION;float2 uv:TEXCOORD0;};
   Varyings vert(Attributes v){Varyings o;o.positionCS=TransformObjectToHClip(v.positionOS.xyz);o.uv=v.uv;return o;}
   float Segment(float2 p,float2 a,float2 b){float2 d=b-a;return length(p-a-d*saturate(dot(p-a,d)/dot(d,d)));}
   half4 frag(Varyings i):SV_Target {
    float2 p=float2(i.uv.x,i.uv.y*_Length);
    float aa=max(fwidth(p.x),fwidth(p.y))*.8+.003;
    float tip=Segment(float2(abs(p.x),p.y),float2(.48,_Length-.62),float2(0,_Length));
    float rail=Segment(float2(abs(p.x),p.y),float2(.24,.30),float2(.24,_Length-.60));
    float edge=min(tip,rail);
    float stroke=1-smoothstep(.016,.016+aa,edge);
    float glow=exp2(-edge*38);
    float border=1-smoothstep(.053,.053+aa,edge);
    float inside=(1-smoothstep(.22,.22+aa,abs(p.x)))*smoothstep(.28,.40,p.y)*(1-smoothstep(_Length-.7,_Length-.6,p.y));
    // Constant metre spacing, with chevrons moving toward the actual release heading.
    float v=abs(frac((p.y+abs(p.x)*1.4-_Time.y*(1.25+_Charge*.7))/1.25)-.5)*1.25;
    float chevron=(1-smoothstep(.035,.035+aa,v))*inside;
    float ring=abs(length(p)-.22);
    float ringLine=(1-smoothstep(.018,.018+aa,ring))*(1-smoothstep(.28,.30,abs(p.y)));
    float angle=atan2(p.x,-p.y)/6.283185+.5;
    float charged=ringLine*step(angle,_Charge);
    float pulse=(.5+.5*sin(_Time.y*4))*smoothstep(.94,1,_Charge);
    half3 teal=lerp(half3(.06,.89,.83),_Bend>0?half3(.28,.63,1):half3(1,.30,.11),abs(_Bend)),amber=half3(1,.78,.25);
    half3 color=lerp(half3(.015,.09,.12),teal,saturate(stroke+glow*.55+chevron));
    color=lerp(color,amber,saturate(charged+stroke*_Charge*smoothstep(_Length-.8,_Length,p.y)));
    color=lerp(color,half3(.86,1,.96),saturate(chevron*.6+_Release*.65));
    float alpha=max(border*.70,max(stroke*.92+glow*.20,inside*.10+chevron*.68));
    alpha=max(alpha,ringLine*.85);
    return half4(color,saturate(alpha+glow*pulse*.12)*_Opacity);
   }
   ENDHLSL
  }
 }
}
