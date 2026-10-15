Shader "WhatTheFish/TeleportLight" {
 Properties {_Color("Blue light",Color)=(.08,.55,1,1) _Power("Power",Float)=1 _Kind("Floor ring",Float)=0 _Phase("Phase",Float)=0}
 SubShader {
  Tags {"RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent"}
  Pass {
   Blend SrcAlpha One ZWrite Off Cull Off
   HLSLPROGRAM
   #pragma vertex Vert
   #pragma fragment Frag
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
   struct A {float4 vertex:POSITION;float2 uv:TEXCOORD0;};struct V {float4 vertex:SV_POSITION;float2 uv:TEXCOORD0;};
   CBUFFER_START(UnityPerMaterial)
   half4 _Color;float _Power,_Kind,_Phase;
   CBUFFER_END
   V Vert(A v){V o;o.vertex=TransformObjectToHClip(v.vertex.xyz);o.uv=v.uv;return o;}
   half4 Frag(V i):SV_Target {
    float a;
    if(_Kind>.5){float r=length(i.uv*2-1);float outer=exp(-pow((r-.79)*75,2));float inner=exp(-pow((r-.58)*55,2));float sweep=exp(-pow((r-frac(_Phase*.65)) *40,2))*.35;float spokes=pow(saturate(cos(atan2(i.uv.y-.5,i.uv.x-.5)*12+_Phase*1.8)),24)*exp(-pow((r-.69)*18,2));a=(outer+inner*.5+spokes*.6+sweep)*smoothstep(1,.91,r);}
    else {float y=i.uv.y;float helix=pow(saturate(cos(i.uv.x*18.84956-y*9+_Phase*3)),30);float scan=pow(saturate(cos(y*54-_Phase*7)),38)*.25;float veil=.075+helix*.7+scan;float ends=smoothstep(0,.08,y)*(1-smoothstep(.65,1,y));a=veil*ends;}
    return half4(lerp(_Color.rgb,half3(.65,.94,1),saturate(a)*.45),saturate(a*_Power));
   }
   ENDHLSL
  }
 }
}
