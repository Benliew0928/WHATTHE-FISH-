Shader "WhatTheFish/Basketball Prop Effect" {
 Properties { _Tint("Tint",Color)=(0,.6,1,1) _Mode("Mode",Float)=0 _Progress("Progress",Float)=1 }
 SubShader { Tags { "Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" }
 Pass { Blend SrcAlpha OneMinusSrcAlpha ZWrite Off Cull Off
 HLSLPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 struct A { float4 p:POSITION;float2 uv:TEXCOORD0;float3 normal:NORMAL;};struct V { float4 p:SV_POSITION;float2 uv:TEXCOORD0;float3 normal:TEXCOORD1;float3 world:TEXCOORD2;};
 CBUFFER_START(UnityPerMaterial)
 float4 _Tint;float _Mode,_Progress;
 CBUFFER_END
 V vert(A v){V o;o.p=TransformObjectToHClip(v.p.xyz);o.uv=v.uv;o.normal=TransformObjectToWorldNormal(v.normal);o.world=TransformObjectToWorld(v.p.xyz);return o;}
 half4 frag(V i):SV_Target {
  float2 p=(i.uv-.5)*2;float r=length(p),a=atan2(p.y,p.x);float edge=1-smoothstep(.88,1,r);
  if(_Mode>6.5){
   float hex=max(abs(p.x)*.866025+abs(p.y)*.5,abs(p.y));float rim=exp(-pow((hex-.76)*24,2));
   float swirl=pow(saturate(sin(a*3+r*22-_Time.y*6)),12)*smoothstep(.15,.80,r);
   return half4(lerp(float3(.002,.004,.015),_Tint.rgb,saturate(rim+swirl*.35)),(1-smoothstep(.89,.97,hex))*.98);
  }
  if(_Mode>5.5){
   float border=1-smoothstep(.008,.030,min(min(i.uv.x,1-i.uv.x),min(i.uv.y,1-i.uv.y)));
   float scan=pow(.5+.5*sin(i.world.y*11-_Time.y*2),18)*.12;
   float glint=pow(saturate(1-abs(i.uv.x+i.uv.y*1.7-frac(_Time.y*.08)*3)),24)*.16;
   return half4(lerp(_Tint.rgb,float3(.72,.97,1),border*.8),min(.85,_Tint.a+border*.32+scan+glint));
  }
  if(_Mode>3.5&&_Mode<4.5){
   float scan=.5+.5*sin(i.world.y*110-_Time.y*7);float rim=pow(1-abs(dot(normalize(i.normal),GetWorldSpaceNormalizeViewDir(i.world))),2);
   return half4(lerp(_Tint.rgb,float3(.6,.9,1),rim*.65),_Tint.a*(.56+.15*scan+.15*rim));
  }
  if(_Mode>4.5){float rings=exp(-pow((r-.80)*32,2))+exp(-pow((r-.57)*34,2))*.5;float ticks=step(.80,cos(a*16+_Time.y))*(smoothstep(.84,.88,r)-smoothstep(.94,.98,r));return half4(_Tint.rgb,(rings+ticks)*_Tint.a+max(0,1-r)*.04);}
  if(_Mode>1.5&&_Mode<2.5){float halo=exp(-pow((r-.72)*14,2))*.55;return half4(_Tint.rgb,edge*(halo+.17*(1-r)));}
  float rim=exp(-pow((r-.77)*18,2));
  if(_Mode>2.5){float angle=frac(a/6.283185+1);return half4(_Tint.rgb,rim*(angle<_Progress?.95:.16));}
  float swirl=pow(saturate(sin(a*3+r*22-_Time.y*6)),12)*smoothstep(.25,.85,r);
  float light=rim+swirl*.28;return half4(lerp(float3(.001,.003,.009),_Tint.rgb,light),edge*.98);
 }
 ENDHLSL
 } }
}
