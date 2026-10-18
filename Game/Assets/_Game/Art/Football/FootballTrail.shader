Shader "WhatTheFish/FootballTrail" {
 Properties { _Style("Design",Float)=0 _Motion("Motion",Float)=1 _Sport("Basketball accents",Float)=0 }
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
   float _Style, _Motion, _Sport;
   CBUFFER_END
   struct Attributes {float4 positionOS:POSITION;float2 uv:TEXCOORD0;half4 color:COLOR;};
   struct Varyings {float4 positionCS:SV_POSITION;float2 uv:TEXCOORD0;half4 color:COLOR;};
   Varyings vert(Attributes v){Varyings o;o.positionCS=TransformObjectToHClip(v.positionOS.xyz);o.uv=v.uv;o.color=v.color;return o;}
   float strand(float y,float centre,float width){return exp2(-pow((y-centre)/width,2)*3);}
   float hash(float x){return frac(sin(x*127.1+311.7)*43758.5453);}
   half4 frag(Varyings i):SV_Target {
    float x=i.uv.x,y=i.uv.y*2-1,t=_Time.y*_Motion;
    float core=0,glow=0;half3 tint=half3(.3,.65,1);
    if(_Style<.5){
     float cell=x*19+t*1.7,n=floor(cell);
     float zig=lerp(hash(n),hash(n+1),frac(cell))*.92-.46;
     core=strand(y,zig,.055)+strand(y,-zig*.75+.18*sin(x*34),.028)*.65;
     glow=strand(y,zig,.26)*.58;tint=half3(.38,.56,1);
    }else if(_Style<1.5){
     float wave=sin(x*16-t*3)*.23;
     core=strand(y,wave,.10)*.85+strand(y,-wave-.22,.065)*.6;
     glow=strand(y,0,.65)*.65;tint=lerp(half3(.06,.36,1),half3(.15,.95,1),x);
    }else if(_Style<2.5){
     float lane=x*9+t*.6,n=floor(lane),dx=(frac(lane)-.5)*2,dy=(y-(hash(n)*1.2-.6))*2.6;
     float star=pow(saturate(1-min(abs(dx)*8+abs(dy)*.65,abs(dx)*.65+abs(dy)*8)),2);
     core=star+strand(y,sin(x*12-t)*.15,.045)*.45;
     glow=strand(y,0,.67)*.5;tint=lerp(half3(1,.12,.52),half3(.86,.4,1),x);
    }else if(_Style<3.5){
     float flame=sin(x*22+t*4)*(.12+.23*x);
     core=strand(y,flame,.10)+strand(y,-flame*.7,.055)*.5;
     glow=strand(y,0,.7)*(.55+.15*sin(x*28+t*3));tint=lerp(half3(1,.72,.08),half3(1,.16,.035),x);
    }else if(_Style<4.5){
     float wave=sin(x*12-t*2)*.38;
     core=strand(y,wave,.085)*.65+strand(y,-wave,.065)*.65;
     glow=strand(y,wave,.4)*.55+strand(y,-wave,.32)*.3;
     tint=lerp(half3(.12,1,.64),half3(.67,.22,1),saturate(y*.6+.5));
    }else{
     float wave=sin(x*25-t*3)*.2;
     core=strand(y,wave,.075)+strand(y,-wave,.05)*.55;
     glow=strand(y,0,.65)*.7;tint=lerp(half3(1,.45,.04),half3(1,.92,.3),x);
    }
    // Continuous bright spine keeps the wake joined to the ball in every style.
    core+=strand(y,0,.09)*.8;glow+=strand(y,0,.43)*.22;
    if(_Sport>.5){
     float rails=.45+.09*sin(x*13-t*3);
     core+=(strand(y,rails,.04)+strand(y,-rails,.04))*(.4+.3*sin(x*18-t*5));
     float sparkX=frac(x*7-t*.7),sparkY=sin(floor(x*7-t*.7)*12.7)*.65;
     core+=pow(saturate(1-abs(sparkX-.5)*14),3)*strand(y,sparkY,.11)*.8*_Motion;
    }
    float edge=1-smoothstep(.8,1,abs(y));
    half3 color=lerp(tint,half3(1,.98,1),saturate(core)*.85);
    return half4(color,saturate(core+glow)*edge*i.color.a);
   }
   ENDHLSL
  }
 }
}
