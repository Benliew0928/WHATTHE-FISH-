Shader "WhatTheFish/FootballBoundary" {
 Properties {
  _BaseColor("Energy tint",Color)=(.08,.75,1,.35)
  [HideInInspector] _PlayerPosition("Player",Vector)=(0,0,0,0)
  [HideInInspector] _BallPosition("Ball",Vector)=(0,0,0,0)
  [HideInInspector] _Impact("Impact position and time",Vector)=(0,0,0,-10)
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
   #pragma multi_compile_fog
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
   CBUFFER_START(UnityPerMaterial)
    half4 _BaseColor;
    float4 _PlayerPosition,_BallPosition,_Impact;
   CBUFFER_END
   struct Attributes {float4 positionOS:POSITION;float2 uv:TEXCOORD0;};
   struct Varyings {float4 positionCS:SV_POSITION;float2 uv:TEXCOORD0;float3 world:TEXCOORD1;half fog:TEXCOORD2;};
   Varyings vert(Attributes v){Varyings o;o.world=TransformObjectToWorld(v.positionOS.xyz);o.positionCS=TransformWorldToHClip(o.world);o.uv=v.uv;o.fog=ComputeFogFactor(o.positionCS.z);return o;}
   half4 frag(Varyings i):SV_Target{
    float x=i.uv.x,y=i.uv.y,t=_Time.y;
    float2 player=i.world.xz-_PlayerPosition.xz,ball=i.world.xz-_BallPosition.xz;
    half nearby=max(exp2(-dot(player,player)*.085)*_PlayerPosition.w,exp2(-dot(ball,ball)*.18)*_BallPosition.w);
    float2 h=float2(x*1.4,y*2.42);
    h.x+=fmod(floor(h.y),2)*.5;
    float2 cell=abs(frac(h)-.5);
    float hex=max(cell.x*.866+cell.y*.5,cell.y);
    float aa=max(fwidth(hex),.012);
    half grid=1-smoothstep(.025,.025+aa,abs(hex-.43));
    half sweep=pow(saturate(.5+.5*sin(y*5.5-x*.24-t*2.6)),10);
    half fade=1-smoothstep(1.05,1.8,y);
    half foot=exp2(-y*13);
    half rail=1-smoothstep(.018,.043,abs(y-.075));
    half top=exp2(-abs(y-1.24)*28)*(.45+sweep*.5);
    half dash=(1-smoothstep(.18,.25,abs(frac(x*.8-t*.5)-.5)))*exp2(-abs(y-.22)*36);
    float age=max(0,t-_Impact.w),radius=age*4.5;
    float dist=distance(i.world,_Impact.xyz);
    half ripple=exp2(-abs(dist-radius)*14)*saturate(1-age/1.3);
    half flash=exp2(-dist*1.3)*saturate(1-age*3);
    half energy=nearby*.6+sweep*.18+ripple+flash;
    half3 tint=lerp(_BaseColor.rgb,half3(.55,.12,1),.5+.5*sin(x*.16-t*.6));
    tint=lerp(tint,half3(.1,1,.84),nearby*.75);
    half alpha=saturate((.065+grid*(.13+nearby*.34)+sweep*.12+nearby*.12)*fade+foot*.35+rail*.7+top*.28+dash*.5+ripple*.85+flash*.4);
    half3 color=lerp(tint,half3(.65,1,1),saturate(rail*.7+grid*energy*.7+dash*.8));
    color=lerp(color,half3(1,.35,.85),saturate(ripple+flash*.65));
    return half4(MixFog(color,i.fog),alpha);
   }
   ENDHLSL
  }
 }
}
