Shader "WhatTheFish/SkySailSign" {
 Properties {[PerRendererData] _MainTex("Font atlas",2D)="white"{} }
 SubShader {Tags {"Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline"}
 Pass {Blend SrcAlpha OneMinusSrcAlpha ZWrite Off ZTest LEqual Cull Back
 HLSLPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 TEXTURE2D(_MainTex);SAMPLER(sampler_MainTex);float4 _TextureSampleAdd;
 struct A {float4 p:POSITION;float2 uv:TEXCOORD0;half4 color:COLOR;};
 struct V {float4 p:SV_POSITION;float2 uv:TEXCOORD0;half4 color:COLOR;};
 V vert(A a){V o;o.p=TransformObjectToHClip(a.p.xyz);o.uv=a.uv;o.color=a.color;return o;}
 half4 frag(V i):SV_Target{return (SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv)+_TextureSampleAdd)*i.color;}
 ENDHLSL
 }}
}
