Shader "WhatTheFish/FishingBuddy" {
 Properties { _BaseMap("Original character texture",2D)="white"{} _Opacity("Opacity",Range(0,1))=.82 }
 SubShader {
  Tags {"RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent"}
  Pass {
   Tags {"LightMode"="UniversalForward"}
   Blend SrcAlpha OneMinusSrcAlpha
   ZWrite On
   Cull Back
   HLSLPROGRAM
   #pragma vertex Vert
   #pragma fragment Frag
   #pragma multi_compile_fog
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
   TEXTURE2D(_BaseMap);SAMPLER(sampler_BaseMap);
   CBUFFER_START(UnityPerMaterial)
   float4 _BaseMap_ST;half _Opacity;
   CBUFFER_END
   struct A {float4 positionOS:POSITION;float3 normalOS:NORMAL;float2 uv:TEXCOORD0;};
   struct V {float4 positionCS:SV_POSITION;float3 normalWS:TEXCOORD0;float3 positionWS:TEXCOORD1;float2 uv:TEXCOORD2;half fog:TEXCOORD3;};
   V Vert(A a){V o;VertexPositionInputs p=GetVertexPositionInputs(a.positionOS.xyz);o.positionCS=p.positionCS;o.positionWS=p.positionWS;o.normalWS=TransformObjectToWorldNormal(a.normalOS);o.uv=TRANSFORM_TEX(a.uv,_BaseMap);o.fog=ComputeFogFactor(o.positionCS.z);return o;}
   half4 Frag(V i):SV_Target{
    half4 tex=SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,i.uv);clip(tex.a-.15h);
    half gray=dot(tex.rgb,half3(.2126h,.7152h,.0722h));half3 normal=normalize(i.normalWS);Light light=GetMainLight();
    half shade=.48h+.52h*saturate(dot(normal,light.direction));half rim=pow(1-saturate(dot(normal,normalize(GetWorldSpaceViewDir(i.positionWS)))),3);
    half value=saturate((.14h+gray*.82h)*shade+rim*.65h);half3 color=MixFog(value.xxx,i.fog);return half4(color,_Opacity);
   }
   ENDHLSL
  }
 }
}
