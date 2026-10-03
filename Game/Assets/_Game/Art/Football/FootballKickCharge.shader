Shader "WhatTheFish/FootballKickCharge" {
 Properties {
  _BaseColor("Charge tint",Color)=(.08,.85,1,.9)
  _Charge("Charge",Range(0,1))=0
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
    half4 _BaseColor;
    float _Charge,_Surface,_ZWrite;
   CBUFFER_END
   struct Attributes {float4 positionOS:POSITION;float2 uv:TEXCOORD0;};
   struct Varyings {float4 positionCS:SV_POSITION;float2 uv:TEXCOORD0;};
   Varyings vert(Attributes v){Varyings o;o.positionCS=TransformObjectToHClip(v.positionOS.xyz);o.uv=v.uv;return o;}
   float Line(float2 p,float2 a,float2 b){float2 d=b-a;return length(p-a-d*saturate(dot(p-a,d)/dot(d,d)));}
   half3 Spectrum(float t){return .5+.5*cos(6.28318*(t+float3(0,.67,.33)));}
   half4 frag(Varyings i):SV_Target{
    float2 p=i.uv;float t=_Time.y;
    float aa=max(fwidth(p.x),fwidth(p.y))*.8+.002;
    // Two uninterrupted, straight aiming rails. Nothing moves their direction.
    float rail=Line(float2(abs(p.x),p.y),float2(.26,.02),float2(.26,.75));
    float head=Line(float2(abs(p.x),p.y),float2(.54,.69),float2(0,1.02));
    float shoulder=Line(float2(abs(p.x),p.y),float2(.26,.75),float2(.54,.69));
    float edge=min(rail,min(head,shoulder));
    half hot=1-smoothstep(.008,.008+aa,edge);
    half neon=exp2(-edge*70);
    half halo=exp2(-edge*24);
    half shaft=(1-smoothstep(.21,.21+aa,abs(p.x)))*smoothstep(.015,.04,p.y)*(1-smoothstep(.72,.75,p.y));
    half headFill=smoothstep(.675,.70,p.y)*(1-smoothstep(.99,1.02,p.y))*(1-smoothstep((1.02-p.y)*1.63,(1.02-p.y)*1.63+aa,abs(p.x)));
    half fill=max(shaft,headFill);
    float chevron=abs(frac(p.y*5+abs(p.x)*2.3-t*(1.1+_Charge*1.8))-.5);
    half flow=1-smoothstep(.045,.045+aa*4,chevron);
    half bead=pow(saturate(.5+.5*cos((p.y-t*(.7+_Charge))*24)),12)*neon;
    half pulse=(.5+.5*sin(t*7))*smoothstep(.9,1,_Charge);
    half3 spectrum=Spectrum(p.y*.65-t*.16+_Charge*.16);
    half3 railColor=p.x<0?half3(.04,.9,1):half3(1,.08,.65);
    railColor=lerp(railColor,half3(1,.8,.08),saturate(p.y-.65)*2*_Charge);
    half3 color=lerp(spectrum,railColor,saturate(neon*1.5));
    color=lerp(color,half3(.88,1,1),saturate(hot*.75+bead*.45));
    color=lerp(color,half3(1,.93,.38),flow*fill*.5);
    color=lerp(color,_BaseColor.rgb,saturate(p.y-.75)*_Charge*.5);
    half alpha=saturate(hot*.88+neon*.7+halo*.2+fill*(.42+flow*.43+pulse*.12));
    half spine=(1-smoothstep(.009,.009+aa,abs(p.x)))*fill*(.4+flow*.6);
    return half4(lerp(color,half3(.95,1,1),spine*.6),max(alpha,spine*.8));
   }
   ENDHLSL
  }
 }
}
